using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ModulesTracker : MonoBehaviour
{
	[Serializable]
	private class Module
	{
		public Transform Transform;
		public string ModuleName;
		public GameObject Led;
		public bool Completed;
	}

	private const string MODULE_PARENT =
#if UNITY_EDITOR
		"Modules";
#else
		"VisualTransform";
#endif
	private const string SEARCH_TERM = "Component_LED_PASS";
	private const string VANILLA_COMPONENT = "BombComponent";
	private const string VANILLA_NAME_METHOD = "GetModuleDisplayName";
	private readonly string[] IGNORED_VANILLA_MODULES = new string[]
	{
		"Timer",
		"None"
	};

	private static ModulesTracker singleton; 

	private List<IModuleTracker> listeners = new List<IModuleTracker>();
	private List<Module> modules = new List<Module>();
	private float testQuantum = 0.2f;

	public static bool Initialized { get; private set; }
	public static int Count { get; private set; }
	public static string[] SolvableDisplayNames { get; private set; }

	#region Initialization

	private void Awake()
	{
		if (singleton)
		{
			this.enabled = false;
			return;
		}

		singleton = this;
		StartCoroutine(FindVanillaLeds());
		StartCoroutine(TestModules());
	}

	private IEnumerator FindVanillaLeds()
	{
		yield return new WaitForSeconds(0.1f);

		Transform moduleParent = GameObject.Find(MODULE_PARENT).transform;
		int childCount = moduleParent.childCount;
		
		for(int i = 0; i < childCount; i++)
		{
			Transform child = moduleParent.GetChild(i);
			string moduleName = GetModuleDisplayName(child); 

			if(moduleName == null)
			{
				continue;
			}

			// Adds found module to the module list. 
			GameObject led = FindLEDIn(child);

			Module module = new Module()
			{
				Transform = child,
				ModuleName = moduleName,
				Led = led,
				Completed = false
			};

			Debug.Log("[Modules Tracker] found module: " + module.ModuleName);

			modules.Add(module);
		}

		SetAttributes();
	}

	private string GetModuleDisplayName(Transform moduleObject)
	{
		// tests if it is a modded module.
		KMBombModule moddedModule = moduleObject.GetComponent<KMBombModule>();
		if (moddedModule != null)
		{
			return moddedModule.ModuleDisplayName;
		}

		// tests if it is a vanilla module.
		Component vanillaComponent = moduleObject.GetComponent(VANILLA_COMPONENT);
		if (vanillaComponent != null)
		{
			Type type = vanillaComponent.GetType();
			MethodInfo method = type.GetMethod(VANILLA_NAME_METHOD);
			string moduleName = (string)method.Invoke(vanillaComponent, null);
			if (IGNORED_VANILLA_MODULES.Contains(moduleName))
			{
				return null;
			}

			return moduleName;
		}

		// it was no module.
		return null;
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

	private void SetAttributes()
	{
		Count = modules.Count;
		SolvableDisplayNames = new string[modules.Count];
		for(int i = 0; i < modules.Count; i++)
		{
			SolvableDisplayNames[i] = modules[i].ModuleName;
		}
		Initialized = true;
	}

#endregion


#region Listeners

	public static void AddOnModuleSolvedListener(IModuleTracker listener)
	{
		if (singleton.listeners.IndexOf(listener) == -1)
		{
			singleton.listeners.Add(listener);
		}
	}

	public static void RemoveModuleSolvedListener(IModuleTracker listener)
	{
		int index = singleton.listeners.IndexOf(listener);
		if (index == -1)
		{
			singleton.listeners.RemoveAt(index);
		}
	}

#endregion


#region Invocation

	private IEnumerator TestModules()
	{
		while (true)
		{
			if (listeners.Count > 0 && modules.Count > 0)
			{
				for(int i = 0; i < modules.Count; i++)
				{
					Module module = modules[i];
					if (module.Completed != module.Led.activeSelf)
					{
						Debug.Log("[Module Tracker] Module Solved: " + module.ModuleName);
						module.Completed = module.Led.activeSelf;
						foreach(IModuleTracker listener in listeners)
						{
							listener.OnModuleSolved(module.ModuleName, module.Transform);
						}
					}
				}
			}
			yield return new WaitForSeconds(testQuantum);
		}
	}

#endregion
}
