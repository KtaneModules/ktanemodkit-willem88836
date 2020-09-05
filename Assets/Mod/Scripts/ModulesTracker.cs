using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KMBombInfo))]
public class ModulesTracker : MonoBehaviour
{
	private KMBombInfo bombInfo;

	private float testQuantum = 0.2f;
	private List<string> previousSolvedModuleIDs = new List<string>();

	private Action<string> OnModuleSolved;

	public void AddOnModuleSolvedListener(Action<string> listener)
	{
		OnModuleSolved += listener;
	}

	public void RemoveModuleSolvedListener(Action<string> listener)
	{
		OnModuleSolved -= listener;
	}

	private void Awake()
	{
		bombInfo = GetComponent<KMBombInfo>();
		StartCoroutine(TestModules());
	}

	private IEnumerator TestModules()
	{
		while (true)
		{
			if (OnModuleSolved != null)
			{
				List<string> solvedModuleIDs = bombInfo.GetSolvedModuleNames();

				if (solvedModuleIDs.Count != previousSolvedModuleIDs.Count)
				{
					string solved = Difference(solvedModuleIDs, previousSolvedModuleIDs);
					OnModuleSolved.Invoke(solved);
				}

				previousSolvedModuleIDs = solvedModuleIDs;
			}

			yield return new WaitForSeconds(testQuantum);
		}
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

}
