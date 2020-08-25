using System.Collections;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(KMBombInfo), typeof(KMAudio), typeof(KMBombModule))]
[RequireComponent(typeof(KMBossModule))]
public class TamagotchiModule : MonoBehaviour 
{
	[SerializeField, Range(0f, 1f)] private float completionThreshold;
	[SerializeField] private ToggleLight completedLight;
	[SerializeField] private BombButton[] buttons;


	private KMBombInfo bombInfo;
	private KMAudio bombAudio;
	private KMBombModule bombModule;
	private KMBossModule bossModule;

	private float completionAlpha = 0f;
	private float alphaIncrement;
	private TamagotchiState state = TamagotchiState.Neutral;


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

		// Finds all solvable modules, filters out ignored ones. 
		// and attaches an alpha increment to those modules' OnPass handler.
		KMBombModule[] solvableModules = GameObject.FindObjectsOfType<KMBombModule>();
		string[] ignoredModules = bossModule.GetIgnoredModules(bombModule);
		int moduleCount = 0;
		for(int i = 0; i < solvableModules.Length; i++)
		{
			KMBombModule current = solvableModules[i];
			if (!ignoredModules.Contains(current.ModuleDisplayName))
			{
				moduleCount++;

				current.OnPass += delegate
				{
					completionAlpha += alphaIncrement;
					if(completionAlpha >= completionThreshold)
					{
						completedLight.Toggle(true);
					}
					return true;
				};
			}
		}
		alphaIncrement = 1f / moduleCount;

		// Starts the game.
		StartCoroutine(PlayTamagotchi());
	}


	private IEnumerator PlayTamagotchi()
	{
		while (true)
		{


			yield return new WaitForSeconds(1);
		}
	}


	#region ModuleInteraction 

	private void OnFoodButtonClicked()
	{
		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[0].transform);
	}

	private void OnMedicineButtonClicked()
	{
		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[1].transform);
	}

	private void OnDisciplineButtonClicked()
	{
		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[2].transform);
	}

	private void OnPlayButtonClicked()
	{
		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[3].transform);
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


public class Tamagotchi
{
	public int Type;

	public int Age;
}


public enum TamagotchiState
{
	Neutral, 
	Hungry, 
	Sick, 
	Bored,
	Messy, 
	Dead
}
