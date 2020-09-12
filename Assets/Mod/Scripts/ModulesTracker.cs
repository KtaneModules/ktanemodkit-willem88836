using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(KMBombInfo))]
public class ModulesTracker : MonoBehaviour
{
	private const string MODULE_PARENT = "VisualTransform";
	private const string SEARCH_TERM = "Component_LED_PASS";
	private readonly string[] VanillaModuleNames = new string[]
	{
		"PasswordComponent(Clone)",
		"ButtonComponent(Clone)",
		"WireSequenceComponent(Clone)",
		"InvisibleWallsComponent(Clone)",
		"WireSetComponent(Clone)",
		"MemoryComponent(Clone)",
		"WhosOnFirstComponent(Clone)",
		"KeypadComponent(Clone)",
		"MorseComponent(Clone)",
		"VennWiresComponent(Clone)",
		"SimonComponent(Clone)"
	};


	private KMBombInfo bombInfo;
	private List<IModuleTracker> listeners = new List<IModuleTracker>();

	private float testQuantum = 0.2f;

	private List<string> previousSolvedModdedModuleIDs = new List<string>();

	private List<Transform> vanillaPassLeds = new List<Transform>();
	private bool[] completedVanillaModules;


	#region Initialization

	private void Awake()
	{
		bombInfo = GetComponent<KMBombInfo>();
		FindVanillaLeds();
		StartCoroutine(TestModules());
	}

	private void FindVanillaLeds()
	{
		Transform moduleParent = GameObject.Find(MODULE_PARENT).transform;
		int childCount = moduleParent.childCount;
		
		for(int i = 0; i < childCount; i++)
		{
			Transform child = moduleParent.GetChild(i);

			if (VanillaModuleNames.Contains(child.name))
			{
				GameObject led = FindLEDIn(child);
			}
			else
			{
				KMBombModule moddedModule = child.GetComponent<KMBombModule>();
				if (moddedModule != null)
				{
					// TODO: continue here. 
					// it's a modded module. 
					GameObject led = FindLEDIn(child);
					string name = moddedModule.ModuleDisplayName;
				}
				else
				{
					// it's something else. 
				}
			}
		}
	}


	private GameObject FindLEDIn(Transform parent)
	{
		int childCount = parent.childCount;

		for (int i = 0; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if(child.name == SEARCH_TERM)
			{
				return child.gameObject;
			}
			else
			{
				GameObject result = FindLEDIn(child);
				if (result != null)
				{
					return result;
				}
			}
		}

		return null;
	}


	private void FindVanillaLedsIn(Transform parent)
	{
		if(parent.name == SEARCH_TERM
			&& IsVanilla(parent))
		{
			vanillaPassLeds.Add(parent);
		}

		int childCount = parent.childCount;
		for(int i = 0; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			FindVanillaLedsIn(child);
		}
	}

	private bool IsVanilla(Transform subject)
	{
		bool isVanilla = true;
		Transform parent = subject.parent;

		do
		{
			// Vanilla modules do not have a KMBombModule component. 
			KMBombModule moddedModule = parent.GetComponent<KMBombModule>();
			if(moddedModule != null)
			{
				isVanilla = false;
				break;
			}
			parent = subject.parent;
		} while (parent != null);

		return isVanilla;
	}

	#endregion


	#region Listeners

	public void AddOnModuleSolvedListener(IModuleTracker listener)
	{
		if (listeners.IndexOf(listener) == -1)
		{
			listeners.Add(listener);
		}
	}

	public void RemoveModuleSolvedListener(IModuleTracker listener)
	{
		int index = listeners.IndexOf(listener);
		if (index == -1)
		{
			listeners.RemoveAt(index);
		}
	}

	#endregion


	#region Invocation

	private IEnumerator TestModules()
	{
		while (true)
		{
			if (listeners.Count > 0)
			{
				TestVanillaModules();
				TestModdedModules();
			}

			yield return new WaitForSeconds(testQuantum);
		}
	}

	private void TestVanillaModules()
	{
		for (int i = 0; i < vanillaPassLeds.Count; i++)
		{
			bool isCompleted = vanillaPassLeds[i].gameObject.activeSelf;
			if (isCompleted != completedVanillaModules[i])
			{
				completedVanillaModules[i] = isCompleted;

				foreach (IModuleTracker listener in listeners)
				{
					listener.OnVanillaModuleSolved();
				}
			}
		}
	}

	private void TestModdedModules()
	{
		List<string> solvedModuleIDs = bombInfo.GetSolvedModuleNames();

		if (solvedModuleIDs.Count != previousSolvedModdedModuleIDs.Count)
		{
			string solved = Difference(solvedModuleIDs, previousSolvedModdedModuleIDs);

			foreach (IModuleTracker listener in listeners)
			{
				listener.OnModdedModuleSolved(solved);
			}
		}

		previousSolvedModdedModuleIDs = solvedModuleIDs;
	}

	private string Difference(List<string> arrayA, List<string> arrayB)
	{
		foreach (string e in arrayA)
		{
			if (!arrayB.Contains(e))
			{
				return e;
			}
		}

		return null;
	}

	#endregion
}
