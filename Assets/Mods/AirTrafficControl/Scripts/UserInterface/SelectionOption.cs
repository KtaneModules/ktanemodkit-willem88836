using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMAirTrafficControl
{
	[RequireComponent(typeof(Image))]
	public class SelectionOption : MonoBehaviour
	{
		public int Index = -1;
		[SerializeField] private string selectEncapsulation;

		private Text label;
		private Image image;
		private string text;


		public void SetOption(string option)
		{
			if(label == null || image == null)
			{
				label = transform.GetChild(0).GetComponent<Text>();
				image = GetComponent<Image>();
			}

			text = option;

			if(option == null)
			{
				label.text = "";
				image.enabled = false;
			}
			else
			{
				label.text = option;
				image.enabled = true;
			}
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
