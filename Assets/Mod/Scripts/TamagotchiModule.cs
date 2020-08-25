using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(KMBombInfo), typeof(KMAudio), typeof(KMBombModule))]
[RequireComponent(typeof(KMBossModule))]
public class TamagotchiModule : MonoBehaviour 
{
	[SerializeField, Range(0f, 1f)] private float completionThreshold;
	[SerializeField, Range(0f, 1f)] private float diseaseChance;
	[SerializeField, Range(0f, 1f)] private float replenishChance;
	[SerializeField] private int timeUntilImpairment;
	[SerializeField] private int timeUntilSickness;
	[SerializeField] private int timeUntilDeath;
	[SerializeField] private int timeUntilReset;
	[SerializeField] private int[] timesUntilGrowth;
	[SerializeField] private int[] wellbeingChart;
	[SerializeField] private int wellbeingDeviation;
	[SerializeField] private RectTransform tamagotchiSprite;
	[SerializeField] private ToggleLight completedLight;
	[SerializeField] private BombButton[] buttons;
	[SerializeField] private TamagotchiTypes[] tamagotchiTypes;


	private KMBombInfo bombInfo;
	private KMAudio bombAudio;
	private KMBombModule bombModule;
	private KMBossModule bossModule;

	private float completionAlpha = 0f;
	private float alphaIncrement;
	private Tamagotchi tamagotchi;
	private int timeSinceStateChange;


	private string StoragePath { get { return Path.Combine(Application.persistentDataPath, "tamagotchi.json"); } }


	#region Initialization

	private void Start () 
	{
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

		buttons[0].AddListener(OnFoodButtonClicked);
		buttons[1].AddListener(OnMedicineButtonClicked);
		buttons[2].AddListener(OnDisciplineButtonClicked);
		buttons[3].AddListener(OnPlayButtonClicked);
		buttons[4].AddListener(OnCompleteButtonClicked);

		// Sets up storage of the module data on bomb end. 
		bombInfo.OnBombSolved += OnBombExploded;
		bombInfo.OnBombExploded += OnBombExploded;

		// Loads tamagotchi json data if existing.
		if (!TryLoadTamagotchi(out tamagotchi))
		{
			tamagotchi = new Tamagotchi(tamagotchiTypes[0]);
		}
		UpdateTamagotchiSprite();

		// Sets up the alpha increment.
		SetUpAlphaIncrement();

		// Starts the game.
		StartCoroutine(PlayTamagotchi());
	}

	private bool TryLoadTamagotchi(out Tamagotchi tamagotchi)
	{
		try
		{
			string path = StoragePath;
			if (File.Exists(path))
			{
				string json = File.ReadAllText(path);
				tamagotchi = (Tamagotchi)JsonConvert.DeserializeObject(json, typeof(Tamagotchi));
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

	private void OnBombEnded()
	{
		TamagotchiLog.Log("Bomb ended!");

		string json = JsonConvert.SerializeObject(tamagotchi);
		File.WriteAllText(StoragePath, json);
	}

	private void OnBombExploded()
	{
		TamagotchiLog.Log("Bomb exploded!");

		tamagotchi.State = Tamagotchi.TamagotchiState.Dead;
		OnBombEnded();
	}

	#endregion


	#region GameState

	private IEnumerator PlayTamagotchi()
	{
		while (true)
		{
			switch (tamagotchi.State)
			{
				case Tamagotchi.TamagotchiState.Neutral:
					HandleStateNeutral();
					break;
				case Tamagotchi.TamagotchiState.Sick:
					HandleStateSick();
					break;
				case Tamagotchi.TamagotchiState.Dead:
					HandleStateDead();
					break;
				default:
					HandleStateImpaired();
					break;
			}

			tamagotchi.Tick();
			timeSinceStateChange++;
			yield return new WaitForSeconds(1);
		}
	}

	private void HandleStateNeutral()
	{
		if(timeSinceStateChange >= timeUntilImpairment)
		{
			timeSinceStateChange = 0;

			if (Random.Range(0f, 1f) <= diseaseChance)
			{
				tamagotchi.State = Tamagotchi.TamagotchiState.Sick;
			}
			else if(tamagotchi.Boredness >= tamagotchi.Cleanliness
				&& tamagotchi.Boredness >= tamagotchi.Hunger)
			{
				tamagotchi.State = Tamagotchi.TamagotchiState.Bored;
			}
			else if (tamagotchi.Cleanliness >= tamagotchi.Hunger)
			{
				tamagotchi.State = Tamagotchi.TamagotchiState.Messy;
			}
			else
			{
				tamagotchi.State = Tamagotchi.TamagotchiState.Hungry;
			}
		}
		else if(tamagotchi.Age >= timesUntilGrowth[tamagotchi.AgeState])
		{
			tamagotchi.Age++;

			if(tamagotchi.Age >= timesUntilGrowth[timesUntilGrowth.Length - 1])
			{
				PetDiedOldAge();
			}
			else
			{
				int target = wellbeingChart[tamagotchi.AgeState];
				int subTypeCount = 0;

				if(tamagotchi.TimeImpaired <= target)
				{
					subTypeCount = tamagotchiTypes[tamagotchi.AgeState].Length(0);
				}
				else if (tamagotchi.TimeImpaired <= target + wellbeingDeviation)
				{
					subTypeCount = tamagotchiTypes[tamagotchi.AgeState].Length(1);
				}
				else
				{
					subTypeCount = tamagotchiTypes[tamagotchi.AgeState].Length(3);
				}

				if (subTypeCount > 0)
				{
					tamagotchi.Subtype = Random.Range(0, subTypeCount);
					UpdateTamagotchiSprite();
				}
				else
				{
					PetDiedOldAge();
				}
			}
		}

		tamagotchiSprite.localPosition = timeSinceStateChange % 2 == 0
			? new Vector3(0, 0.1f, 0)
			: Vector3.zero;
	}

	private void HandleStateDead()
	{
		if (timeSinceStateChange >= timeUntilReset)
		{
			tamagotchi = new Tamagotchi(tamagotchiTypes[0]);
			timeSinceStateChange = 0;
		}
	}

	private void HandleStateImpaired()
	{
		if (timeSinceStateChange >= timeUntilSickness)
		{
			tamagotchi.State = Tamagotchi.TamagotchiState.Sick;
			timeSinceStateChange = 0;
		}
	}

	private void HandleStateSick()
	{
		if (timeSinceStateChange >= timeUntilDeath)
		{
			tamagotchi.State = Tamagotchi.TamagotchiState.Dead;
			timeSinceStateChange = 0;

			bombModule.HandleStrike();
			TamagotchiLog.Log("Strike: Your pet died due to poor caretaking... tisk tisk...");
		}
	}


	private void UpdateTamagotchiSprite()
	{
		tamagotchiSprite.GetComponent<Image>().sprite = tamagotchiTypes[tamagotchi.AgeState].Get(tamagotchi.Type, tamagotchi.Subtype);
	}

	private void PetDiedOldAge()
	{
		tamagotchi.State = Tamagotchi.TamagotchiState.Dead;
		completionAlpha = 1;
		OnTrackedModuleSolved();

		TamagotchiLog.Log("Your pet died of old age... farewell buddy...");
	}



	#endregion


	#region ModuleInteraction 

	private void OnFoodButtonClicked()
	{
		if(Random.Range(0f, 1f) <= replenishChance)
		{
			tamagotchi.Hunger = 0;
			tamagotchi.State = Tamagotchi.TamagotchiState.Neutral;
		}
	}

	private void OnMedicineButtonClicked()
	{
		if (Random.Range(0f, 1f) <= replenishChance)
		{
			tamagotchi.Sickness = 0;
			tamagotchi.State = Tamagotchi.TamagotchiState.Neutral;
		}
	}

	private void OnDisciplineButtonClicked()
	{
		if (Random.Range(0f, 1f) <= replenishChance)
		{
			tamagotchi.Cleanliness = 0;
			tamagotchi.State = Tamagotchi.TamagotchiState.Neutral;
		}
	}

	private void OnPlayButtonClicked()
	{
		if (Random.Range(0f, 1f) <= replenishChance)
		{
			tamagotchi.Boredness = 0;
			tamagotchi.State = Tamagotchi.TamagotchiState.Neutral;
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

	#endregion
}
