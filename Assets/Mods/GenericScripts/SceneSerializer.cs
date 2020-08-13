using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class SceneSerializer : MonoBehaviour
{
	public void Start()
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

		UnityEngine.Debug.LogFormat("[NMTechSupport] {0}", "Scene succesfullly serialized");
	}


	private void Serialize(GameObject obj, int i)
	{
		string output = "";
		for (int j = 0; j < i; j++)
		{ 
			output += "\t"; 
		}

		output += obj.name;

		TechSupportLog.Log(output);

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
			TechSupportLog.Log(output);
		}


		int childCount = obj.transform.childCount;
		for(int j = 0; j < childCount; j++)
		{
			GameObject child = obj.transform.GetChild(j).gameObject;
			Serialize(child, i + 1);
		}
	}
}
