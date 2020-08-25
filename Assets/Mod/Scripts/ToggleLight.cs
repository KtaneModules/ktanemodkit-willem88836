using UnityEngine;

public class ToggleLight : MonoBehaviour
{
	public MeshRenderer meshRenderer;
	public Light myLight;
	
	[Space] public Color32 colorOff;
	public Color32 colorOn;

	public bool IsOn;


	private void Awake()
	{
		Toggle(IsOn);
	}

	public void Toggle(bool on)
	{
		IsOn = on;

		myLight.gameObject.SetActive(on);

		if (on)
		{
			meshRenderer.material.color = colorOn;
		}
		else
		{
			meshRenderer.material.color = colorOff;
		}
	}
}
