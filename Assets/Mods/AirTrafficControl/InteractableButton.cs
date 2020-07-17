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

		public void AddListener(Action action)
		{
			onClick += action;
		}
	}
}
