using System;
using UnityEngine;

[RequireComponent(typeof(KMBombModule))]
public class TamagotchiLog : MonoBehaviour
{
	private static TamagotchiLog singleton;
	private KMBombModule moduleInfo;

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			moduleInfo = GetComponent<KMBombModule>();
		}
		else
		{
			throw new Exception("Duplicate singleton");
		}
	}

	public static void Log(string message)
	{
		Debug.LogFormat(@"[{0}] {1}", singleton.moduleInfo.ModuleDisplayName, message);
	}

	public static void LogFormat(string message, params object[] obj)
	{
		Log(string.Format(message, obj));
	}
}
