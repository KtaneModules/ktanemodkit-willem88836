using UnityEngine;

namespace wmeijer.techsupport.v2 {
	[RequireComponent(typeof(KMNeedyModule))]
	public class TechSupportLog : MonoBehaviour 
	{
		private static TechSupportLog instance;
		private KMNeedyModule needyModule;

		// Use this for initialization
		private void Awake() 
		{
			instance = this;
			needyModule = GetComponent<KMNeedyModule>();
		}

		public static void Log(string message)
		{
			Debug.LogFormat("[{0}] {1}", instance.needyModule.ModuleDisplayName, message);
		}

		public static void LogFormat(string format, params object[] elements)
		{
			Log(string.Format(format, elements));
		}
	}
}
