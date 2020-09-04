using UnityEngine;

public class LightToggle : MonoBehaviour
{
	public MeshRenderer meshRenderer;
	public Color32 OnColor;
	public Light OnLight;

	private bool isOn;

	internal bool IsOn()
	{
		return isOn;
	}

	internal void ToggleOn()
	{
		isOn = true;
		meshRenderer.material.color = OnColor;
		OnLight.gameObject.SetActive(true);
	}
}
