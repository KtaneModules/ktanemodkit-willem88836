using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMAirTrafficControl
{
	
	public class SelectionOption : MonoBehaviour
	{
		public int Index = -1;

		private Text label;

		public void SetOption(string option)
		{
			if(label == null)
			{
				label = transform.GetChild(0).GetComponent<Text>();
			}

			label.text = option;
		}


		public void Select()
		{

		}

		public void Deselect()
		{

		}
	}
}
