using UnityEngine;

namespace NMTechSupport
{
	public class ObjectToggle : MonoBehaviour
	{
		private GameObject ObjectOn;
		private GameObject ObjectOff;

		private void Awake()
		{
			ObjectOn = transform.GetChild(0).gameObject;
			ObjectOff = transform.GetChild(1).gameObject;
		}

		public void Toggle(bool on)
		{
			ObjectOn.SetActive(on);
			ObjectOff.SetActive(!on);
		}
	}
}
