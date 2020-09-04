using System.Collections.Generic;
using UnityEngine;

public class SceneSerializer : MonoBehaviour
{
	public KMBombModule ParentModule;

	private void Start()
	{
		StartCoroutine(DelayedStart());
	}

	private IEnumerator<YieldInstruction> DelayedStart()
	{
		yield return new WaitForSeconds(1);

		GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

		foreach (GameObject root in rootObjects)
		{
			Serialize(root, 0);
		}

		UnityEngine.Debug.LogFormat(@"[{0}] Scene succesfullly serialized", ParentModule.ModuleDisplayName);
	}


	private void Serialize(GameObject obj, int i)
	{
		string output = "";
		for (int j = 0; j < i; j++)
		{ 
			output += "\t"; 
		}

		output += obj.name;

		UnityEngine.Debug.LogFormat(@"[{0}] {1}", ParentModule.ModuleDisplayName, output);

		output = "";

		Component[] components = obj.GetComponents<Component>();
		foreach(Component c in components)
		{
			output = "";
			for (int j = 0; j < i; j++)
			{
				output += "\t";
			}
			output += " [c] " + c.GetType().ToString();
			UnityEngine.Debug.LogFormat(@"[{0}] {1}", ParentModule.ModuleDisplayName, output);
		}


		int childCount = obj.transform.childCount;
		for(int j = 0; j < childCount; j++)
		{
			GameObject child = obj.transform.GetChild(j).gameObject;
			Serialize(child, i + 1);
		}
	}
}
