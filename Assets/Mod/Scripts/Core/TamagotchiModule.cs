using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(KMBombInfo), typeof(KMAudio), typeof(KMBombModule))]
[RequireComponent(typeof(KMBossModule))]
public class TamagotchiModule : MonoBehaviour
{
	private enum MenuState { Menu, FoodMenu };


	[SerializeField, Range(0f, 1f)] private float completionThreshold;
	[SerializeField] private float textureScaleFactor;
	[SerializeField] private int AttentionCallInterval;

	[SerializeField] private Transform tamagotchiSprite;
	[SerializeField] private ToggleLight completedLight;
	[SerializeField] private FoodMenu foodMenu;
	[SerializeField] private BombButton[] buttons;
	[SerializeField] private GameObject[] menuIcons;
	[SerializeField] private GameObject attentionIcon;
	[SerializeField] private Tamagotchi[] tamagotchiTypes;


	private KMBombInfo bombInfo;
	private KMAudio bombAudio;
	private KMBombModule bombModule;
	private KMBossModule bossModule;

	private static int tamagotchiInstanceIndex = 0;
	private int myInstanceIndex;

	private Vector3 creatureBasePosition;
	private float completionAlpha = 0f;
	private float alphaIncrement;
	private TamagotchiStats tamagotchi;

	private int lastTimeSinceAttentionCall = 0;

	private MenuState menuState = MenuState.Menu; 
	private int currentMenuIcon = 0;

	private string StoragePath 
	{ 
		get 
		{ 
			return Path.Combine(Application.persistentDataPath, 
				string.Format("tamagotchi_{0}.json", myInstanceIndex)); 
		} 
	}


	#region Initialization

	private void Start () 
	{
		myInstanceIndex = tamagotchiInstanceIndex;
		tamagotchiInstanceIndex++;

		// Sets up basic references.
		bombInfo = GetComponent<KMBombInfo>();
		bombAudio = GetComponent<KMAudio>();
		bombModule = GetComponent<KMBombModule>();
		bossModule = GetComponent<KMBossModule>();

		// Sets up button interaction.
		foreach (BombButton button in buttons)
		{
			button.SetAudio(bombAudio);
		}

		buttons[0].AddListener(OnLeftButtonClicked);
		buttons[1].AddListener(OnMiddleButtonClicked);
		buttons[2].AddListener(OnRightButtonClicked);
		buttons[3].AddListener(OnCompleteButtonClicked);

		creatureBasePosition = tamagotchiSprite.localPosition;

		// Sets up storage of the module data on bomb end. 
		bombInfo.OnBombExploded += OnBombExploded;

		// gives the scriptablje objects indices.
		for(int i = 0; i < tamagotchiTypes.Length; i++)
		{
			tamagotchiTypes[i].Index = i;
		}

		// Loads tamagotchi json data if existing.
		if (!TryLoadTamagotchi(out tamagotchi))
		{
			tamagotchi = new TamagotchiStats();
			tamagotchi.SetTamagotchi(tamagotchiTypes[0]);
		}
		else
		{
			tamagotchi.SetTamagotchi(tamagotchiTypes[tamagotchi.TamagotchiIndex]);
		}

		currentMenuIcon = menuIcons.Length;
		UpdateMenuButtons();

		UpdateTamagotchiSprite();

		// Sets up the alpha increment.
		SetUpAlphaIncrement();

		// Starts the game.
		StartCoroutine(PlayTamagotchi());
	}

	private bool TryLoadTamagotchi(out TamagotchiStats tamagotchi)
	{
		try
		{
			string path = StoragePath;
			TamagotchiLog.LogFormat("Looking for tamagotchi file at: {0}", path);
			if (File.Exists(path))
			{
				string json = File.ReadAllText(path);
				tamagotchi = (TamagotchiStats)JsonConvert.DeserializeObject(json, typeof(TamagotchiStats));
				TamagotchiLog.Log("Tamagotchi File Found!");
				return true;
			}

			tamagotchi = null;
			TamagotchiLog.Log("No Tamagotchi File Found");
			return false;
		} 
		catch(Exception ex)
		{
			TamagotchiLog.LogFormat("ERROR: {0}", ex.ToString());
			tamagotchi = null;
			return false;
		}
	}

	private void SetUpAlphaIncrement()
	{
		// Finds all solvable modules, filters out ignored ones. 
		// and attaches an alpha increment to those modules' OnPass handler.
		KMBombModule[] solvableModules = GameObject.FindObjectsOfType<KMBombModule>();
		string[] ignoredModules = bossModule.GetIgnoredModules(bombModule);
		int moduleCount = 0;

		foreach(KMBombModule module in solvableModules)
		{
			if (!ignoredModules.Contains(module.ModuleDisplayName))
			{
				moduleCount++;
				module.OnPass += OnTrackedModuleSolved;
			}
		}

		alphaIncrement = 1f / moduleCount;
	}

	#endregion


	#region Delegates

	public bool OnTrackedModuleSolved()
	{
		completionAlpha += alphaIncrement;

		if (completionAlpha >= completionThreshold)
		{
			bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CapacitorPop, completedLight.transform);
			completedLight.Toggle(true);
		}

		return true;
	}

	private void OnDestroy()
	{
		if (tamagotchi.IsDead)
		{
			File.Delete(StoragePath);
			TamagotchiLog.Log("Creature is currently dead, deleted data.");
		}
		else if (tamagotchi != null)
		{
			string json = JsonConvert.SerializeObject(tamagotchi);
			File.WriteAllText(StoragePath, json);
			TamagotchiLog.LogFormat("Saved creature data at: {0}.", StoragePath);
		}
	}

	private void OnBombExploded()
	{
		tamagotchi.IsDead = true;
		TamagotchiLog.Log("Bomb Exploded, your pet did not survive...");
	}

	#endregion


	#region GameState

	private IEnumerator PlayTamagotchi()
	{
		while (true)
		{
			yield return new WaitForSeconds(1);
			if (tamagotchi.IsDead)
			{
				continue;
			}

			tamagotchi.Tick();

			tamagotchiSprite.localPosition = creatureBasePosition
				+ (tamagotchi.Age % 2 == 0 ? Vector3.zero : new Vector3(0, 0.1f, 0));


			if (tamagotchi.IsEvolving)
			{
				// TODO: evolving requirements.
				Evolve();
			}

			bool callsForAttention = tamagotchi.Hunger <= 0
				|| tamagotchi.Happiness <= 0
				|| tamagotchi.Sleepiness <= 0
				|| tamagotchi.Misbehaving <= 0;

			if(callsForAttention 
				&& lastTimeSinceAttentionCall >= AttentionCallInterval)
			{
				CallAttention();
			}

			if(tamagotchi.Age >= tamagotchi.Base.LifeSpan)
			{
				tamagotchi.IsEvolving = true;
				tamagotchi.IsSick = true;
			}
		}
	}

	private void Evolve()
	{
		// TODO: Evolution Sequence.
	}

	private void CallAttention()
	{
		attentionIcon.SetActive(true);
	}

	#endregion


	#region Misc

	private void UpdateTamagotchiSprite()
	{
		Texture texture = tamagotchi.Base.Texture;
		tamagotchiSprite.GetComponent<MeshRenderer>().material.mainTexture = texture;

		float scale = texture.width / textureScaleFactor;
		tamagotchiSprite.localScale = Vector3.one * scale;
	}

	private void PetDiedOldAge()
	{
		tamagotchi.IsDead = true;

		completionAlpha = 1f;
		OnTrackedModuleSolved();
		TamagotchiLog.Log("Your pet died of old age... farewell buddy...");
	}

	private void PetDiedPoorCare()
	{
		tamagotchi.IsDead = true;

		bombModule.HandleStrike();
		TamagotchiLog.Log("Strike: Your pet died due to poor caretaking... tisk tisk...");
	}

	#endregion


	#region ModuleInteraction 


	private void UpdateMenuButtons()
	{
		for(int i = 0; i < menuIcons.Length; i++)
		{
			if (i == currentMenuIcon)
			{
				menuIcons[i].SetActive(true);
			}
			else
			{
				menuIcons[i].SetActive(false);
			}
		}
	}

	private void OnCompleteButtonClicked()
	{
		if (completionAlpha >= completionThreshold)
		{
			bombModule.HandlePass();
			TamagotchiLog.LogFormat("Solved with a = {0} > b = {1}", completionAlpha, completionThreshold);
		}
		else
		{
			bombModule.HandleStrike();
			TamagotchiLog.LogFormat("STRIKE: Not enough modules solved yet! a = {0} < b = {1}", completionAlpha, completionThreshold);
		}
	}


	private void OnLeftButtonClicked()
	{
		switch (menuState)
		{
			case MenuState.Menu:
				currentMenuIcon = (currentMenuIcon - 1);
				if (currentMenuIcon < 0)
				{
					currentMenuIcon = menuIcons.Length;
				}
				UpdateMenuButtons();
				break;
			case MenuState.FoodMenu:
				foodMenu.Toggle();
				break;
		}
	}

	private void OnRightButtonClicked()
	{
		switch (menuState)
		{
			case MenuState.Menu:
				currentMenuIcon = (currentMenuIcon + 1) % (menuIcons.Length + 1);
				UpdateMenuButtons();
				break;
			case MenuState.FoodMenu:
				menuState = MenuState.Menu;
				foodMenu.Disable();
				tamagotchiSprite.gameObject.SetActive(true);
				break;
		}
	}

	private void OnMiddleButtonClicked()
	{
		switch (menuState)
		{
			case MenuState.Menu:
				ConfirmMenuSelect();
				break;
			case MenuState.FoodMenu:
				bool isSnack = foodMenu.Confirm();
				if(isSnack)
				{
					tamagotchi.Happiness++;
					tamagotchi.Weight += 2;
				}
				else 
				{
					tamagotchi.Hunger = int.MaxValue;
				}
				break;
		}
	}

	private void ConfirmMenuSelect()
	{
		/* 0 = food
		 * 1 = light
		 * 2 = sports
		 * 3 = medicin
		 * 4 = cleaning
		 * 5 = status 
		 * 6 = scold
		 */
		if (currentMenuIcon == 0)
		{
			tamagotchiSprite.gameObject.SetActive(false);
			foodMenu.Enable();
			menuState = MenuState.FoodMenu;
		}
	}

	#endregion
}
