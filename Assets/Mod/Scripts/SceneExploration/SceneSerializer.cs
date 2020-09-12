using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SceneSerializer : MonoBehaviour
{
	public KMBombModule ParentModule;
	public bool LogComponents;

	StringBuilder output = new StringBuilder();

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

		UnityEngine.Debug.Log(output.ToString());
		UnityEngine.Debug.LogFormat(@"[{0}] Scene succesfullly serialized", ParentModule.ModuleDisplayName);
	}


	private void Serialize(GameObject obj, int i)
	{
		output.Append(string.Format(@"[{0}] ", ParentModule.ModuleDisplayName));

		for (int j = 0; j < i; j++)
		{ 
			output.Append("\t"); 
		}

		output.Append(obj.name);
		output.Append("\n");

		if (LogComponents)
		{
			Component[] components = obj.GetComponents<Component>();
			foreach(Component c in components)
			{
				for (int j = 0; j < i; j++)
				{
					output.Append("\t");
				}
				output.Append(" [c] ");
				output.Append(c.GetType().ToString());
				output.Append("\n");
			}
		}

		int childCount = obj.transform.childCount;
		for(int j = 0; j < childCount; j++)
		{
			GameObject child = obj.transform.GetChild(j).gameObject;
			Serialize(child, i + 1);
		}
	}
}
