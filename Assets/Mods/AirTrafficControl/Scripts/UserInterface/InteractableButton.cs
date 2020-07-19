using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
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
			if(onClick != null)
			{
				onClick.Invoke();
			}

			return true;
		}
	}
}
