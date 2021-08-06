using System;
using UnityEngine;

namespace NMTechSupport
{
	public class InteractableButton : MonoBehaviour
	{
		private Action onClick;

		private void Start()
		{
			KMSelectable selectable = GetComponent<KMSelectable>();
			selectable.OnInteract += OnClick;
		}

		public void AddListener(Action action)
		{
			onClick += action;
		}

		private bool OnClick()
		{
			if (onClick != null)
				onClick.Invoke();

			return false;
		}
	}
}
