using System;
using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMAirTrafficControl
{
	public class SelectionOption : MonoBehaviour
	{
		[NonSerialized] public int Index = -1;
		[SerializeField] private string selectEncapsulation;

		private Text label;
		private string text;

		public void SetOption(string option)
		{
			if(label == null)
			{
				label = transform.GetChild(0).GetComponent<Text>();
			}

			text = option;
			label.text = option;
		}


		public void Select()
		{
			label.text = string.Format(selectEncapsulation, text);
		}

		public void Deselect()
		{
			label.text = text;
		}
	}
}
