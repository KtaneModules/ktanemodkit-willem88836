using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(KMBombModule), typeof(KMBombInfo), typeof(KMAudio))]
[RequireComponent(typeof(KMBossModule))]
public class ThreeSwitchesModule : MonoBehaviour, IModuleTracker
{
	public Switch[] Switches;
	public LightToggle CompletionLight;
	public ModulesTracker Tracker;

	private KMBombInfo bombInfo;
	private KMBombModule module;
	private KMAudio bombAudio;
	private KMBossModule bossModule;

	public float CompletionAlpha;
	public int MinimumModuleCount;

	private string[] solvableModules;

	private bool[] switchStates;
	private float completionAlpha;
	private float completionAlphaIncrement;

	private int solvedModuleCount = -1;
	private string lastSolvedModuleName;
	private Transform lastSolvedModuleTransform;
	private bool[] targetStates;


	private void Start () 
	{
		InitializeModule();
		StartCoroutine(InitializeSolvability());
	}

	private void InitializeModule()
	{
		module = GetComponent<KMBombModule>();
		bombInfo = GetComponent<KMBombInfo>();
		bombAudio = GetComponent<KMAudio>();
		bossModule = GetComponent<KMBossModule>();

		switchStates = new bool[Switches.Length];
		targetStates = new bool[Switches.Length];

		for (int i = 0; i < Switches.Length; i++)
		{
			Switches[i].Initialize(this, bombAudio, i);
		}
	}

	private IEnumerator InitializeSolvability()
	{
		while(!ModulesTracker.Initialized)
		{
			yield return new WaitForSeconds(0.15f);
		}

		// sets the alpha increment for this module.
		List<string> solvableModules = new List<string>(ModulesTracker.SolvableDisplayNames);
		string[] ignoredModules = bossModule.GetIgnoredModules(module);
		for (int i = solvableModules.Count - 1; i >= 0; i--)
		{
			foreach (string ignored in ignoredModules)
			{
				if (solvableModules[i] == ignored)
				{
					solvableModules.RemoveAt(i);
					break;
				}
			}
		}

		if (solvableModules.Count <= MinimumModuleCount)
		{
			CompleteModule();
		}
		else
		{
			completionAlphaIncrement = 1f / solvableModules.Count;
			this.solvableModules = solvableModules.ToArray();
			ModulesTracker.AddOnModuleSolvedListener(this);
		}

		Debug.LogFormat(@"[{0}] Initialized with {1} modules, alpha increment is {2}", module.ModuleDisplayName, this.solvableModules.Length, completionAlphaIncrement);
	}


	public void OnModuleSolved(string module, Transform transform)
	{
		if (CompletionLight.IsOn() 
			|| !solvableModules.Contains(module))
		{
			return;
		}

		lastSolvedModuleName = module;
		lastSolvedModuleTransform = transform;
		solvedModuleCount++;

		completionAlpha += completionAlphaIncrement;

		if (completionAlpha >= CompletionAlpha)
		{
			CompleteModule();
		}
		else
		{
			TestStates();
			CalculateNewTargets();
		}
	}


	private void CompleteModule()
	{
		CompletionLight.ToggleOn();
		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, CompletionLight.transform);
		Debug.LogFormat(@"[{0}] Module can now be completed!", module.ModuleDisplayName);
	}

	private void TestStates()
	{
		for (int i = 0; i < switchStates.Length; i++)
		{
			if (switchStates[i] != targetStates[i])
			{
				module.HandleStrike();
				Debug.LogFormat(@"[{0}] STRIKE: Incorrect Configuration. Switch {1} is {2}, should be {3}", 
					module.ModuleDisplayName, i, (switchStates[i] ? "ON" : "OFF"), (targetStates[i] ? "ON" : "OFF"));
				break;
			}
		}
	}

	private void CalculateNewTargets()
	{
		if(solvedModuleCount % 3 == 0)
		{
			//If the number of solved modules is a multiplication of 7, flip switch one.
			if (solvedModuleCount % 7 == 0 && solvedModuleCount != 0)
			{
				Flip(0);
			}

			// If the first letter of the last resolved module is a vowel, flip switch three.
			if (new char[] { 'a', 'e', 'i', 'o', 'u' }.Contains(lastSolvedModuleName.ToLower()[0]))
			{
				Flip(2);
			}

			//If all switches are on the same side, flip switch two.
			bool sameSide = true;
			bool active = switchStates[0];
			for(int i = 1; i < switchStates.Length; i++)
			{
				if (active != switchStates[i])
				{
					sameSide = false;
					break;
				}
			}
			if (sameSide)
			{
				Flip(1);
			}

			// If the solved module was on the same side as this module, flip switch three.
			Vector3 rotA = lastSolvedModuleTransform.rotation.eulerAngles;
			Vector3 rotB = transform.rotation.eulerAngles;
			if (Vector3.Magnitude(rotA - rotB) <= Mathf.Epsilon)
			{
				Flip(2);
			}

			// If the third switch is flipped, flip it.
			if(switchStates[2])
			{
				Flip(2);
			}

			//If you made no mistakes, flip switch one.
			if(bombInfo.GetStrikes() == 0)
			{
				Flip(0);
			}
		}
		else if (solvedModuleCount % 3 == 1)
		{
			//If you have made a mistake, flip switch two.
			if(bombInfo.GetStrikes() > 0)
			{
				Flip(1);
			}

			//If all the switches are not on one side, flip switch three.
			bool sameSide = true;
			bool active = switchStates[0];
			for (int i = 1; i < switchStates.Length; i++)
			{
				if (active != switchStates[i])
				{
					sameSide = false;
					break;
				}
			}
			if (!sameSide)
			{
				Flip(1);
			}

			//If the last letter of the last resolved module is a vowel, flip switch one.
			if (new char[] { 'a', 'e', 'i', 'o', 'u' }.Contains(lastSolvedModuleName.ToLower()[lastSolvedModuleName.Length - 1]))
			{
				Flip(0);
			}

			//If the solved module is not on the same side as this module, flip switch one.
			Vector3 rotA = lastSolvedModuleTransform.rotation.eulerAngles;
			Vector3 rotB = transform.rotation.eulerAngles;
			if (Vector3.Magnitude(rotA - rotB) > Mathf.Epsilon)
			{
				Flip(0);
			}

			//If the number of solved modules is a multiplication of 9, flip switch three.
			if(solvedModuleCount % 9 == 0 && solvedModuleCount != 0)
			{
				Flip(2);
			}

			//If the first switch is flipped, flip switch two.
			if(switchStates[0])
			{
				Flip(1);
			}
		}
		else
		{
			//If only the middle switch is ON, flip switch one.
			if (switchStates[0] == switchStates[2] == false && switchStates[1])
			{
				Flip(0);
			}

			//If you the second switch is ON, flip switch two.
			if (switchStates[1])
			{
				Flip(0);
			}

			//If the solved module is not on the same side as this module, flip switch two.
			Vector3 rotA = lastSolvedModuleTransform.rotation.eulerAngles;
			Vector3 rotB = transform.rotation.eulerAngles;
			if (Vector3.Magnitude(rotA - rotB) > Mathf.Epsilon)
			{
				Flip(1);
			}

			//If you have made a two or more mistakes, flip switch three.
			if(bombInfo.GetStrikes() >= 2)
			{
				Flip(2);
			}

			//If the last letter of the last resolved module is a number, flip switch three.
			int a;
			if(int.TryParse(lastSolvedModuleName[lastSolvedModuleName.Length - 1].ToString(), out a))
			{
				Flip(2);
			}

			//If the number of solved modules is a multiplication of 6, flip switch one.
			if (solvedModuleCount % 6 == 0 && solvedModuleCount != 0)
			{
				Flip(0);
			}
		}

		string o = "Target States Updated to: ";
		foreach(bool b in targetStates)
		{
			o += b.ToString();
			o += ", ";
		}
		Debug.LogFormat(@"[{0}] {1}", module.ModuleDisplayName, o);
	}

	private void Flip(int i)
	{
		targetStates[i] = !targetStates[i];
	}

	internal void OnClick(int index, bool clicked)
	{
		switchStates[index] = clicked;

		if (CompletionLight.IsOn())
		{
			bool ended = true;
			bool active = switchStates[0];

			for(int i = 1; i < switchStates.Length; i++)
			{
				if(switchStates[i] != active)
				{
					ended = false;
				}
			}

			if (ended)
			{
				module.HandlePass();
				Debug.LogFormat(@"[{0}] Completed!", module.ModuleDisplayName);
			}
		}
	}
}
