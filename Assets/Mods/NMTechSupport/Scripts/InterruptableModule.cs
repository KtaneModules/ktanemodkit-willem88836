using UnityEngine;

public class InterruptableModule
{
	public KMBombModule BombModule;
	public KMSelectable Selectable;
	public GameObject PassLight;
	public GameObject StrikeLight;
	public GameObject ErrorLight;

	public bool IsFocussed { get; private set; }

	public InterruptableModule(
		KMBombModule bombModule, 
		KMSelectable selectable, 
		GameObject passLight, 
		GameObject strikeLight, 
		GameObject errorLight)
	{
		BombModule = bombModule;
		Selectable = selectable;
		PassLight = passLight;
		StrikeLight = strikeLight;
		ErrorLight = errorLight;

		Selectable.OnFocus += delegate { IsFocussed = true; };
		Selectable.OnDefocus += delegate { IsFocussed = false; };
	}
}

