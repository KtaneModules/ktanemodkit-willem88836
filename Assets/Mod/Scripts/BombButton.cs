using System;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class BombButton : MonoBehaviour
{
	private Action onClick;

	private void Awake()
	{
		KMSelectable selectable = GetComponent<KMSelectable>();
		selectable.OnInteract += OnClick;
	}

	private bool OnClick()
	{
		if (onClick != null)
		{
			onClick.Invoke();
			TamagotchiLog.LogFormat("Button {0} clicked", gameObject.name);
		}
		else
		{
			TamagotchiLog.LogFormat("Button {0} clicked without listener", gameObject.name);
		}
		return true;
	}


	public void AddListener(Action onClick)
	{
		this.onClick += onClick;
	}

	public void RemoveListener(Action onClick)
	{
		this.onClick -= onClick;
	}
}
