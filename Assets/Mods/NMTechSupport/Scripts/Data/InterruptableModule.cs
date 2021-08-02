using UnityEngine;

namespace NMTechSupport
{
	/// <summary>
	///		Data container for one KTANE Module that is marked as interruptable.
	/// </summary>
	public class InterruptableModule
	{
		public KMBombModule BombModule;
		public KMSelectable Selectable;
		public GameObject PassLight;
		public GameObject StrikeLight;
		public GameObject ErrorLight;
		public GameObject OffLight;

		public bool IsFocussed { get; private set; }

		public InterruptableModule(
			KMBombModule bombModule,
			KMSelectable selectable,
			GameObject passLight,
			GameObject strikeLight,
			GameObject errorLight,
			GameObject offLight)
		{
			BombModule = bombModule;
			Selectable = selectable;
			PassLight = passLight;
			StrikeLight = strikeLight;
			ErrorLight = errorLight;
			OffLight = offLight;

			Selectable.OnFocus += delegate
			{ IsFocussed = true; };
			Selectable.OnDefocus += delegate
			{ IsFocussed = false; };
		}

		/// <summary>
		///		A module is only interrupted when the off light is on, 
		///		and it isn't currently used. 
		/// </summary>
		public bool CanBeInterrupted()
		{
			return !(PassLight.activeSelf || StrikeLight.activeSelf || ErrorLight.activeSelf || IsFocussed);
		}
	}
}
