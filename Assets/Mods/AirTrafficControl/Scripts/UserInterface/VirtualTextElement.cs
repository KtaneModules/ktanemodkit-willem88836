using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMTechSupport
{
	public class VirtualTextElement : MonoBehaviour
	{
		private Text actualText;
		private Text virtualText;

		private void Awake()
		{
			actualText = GetComponent<Text>();
			virtualText = transform.GetChild(0).GetComponent<Text>();
		}


		public void SetText(string text)
		{
			actualText.text = text;
			virtualText.text = text;

			float a = 1f / virtualText.rectTransform.localScale.x;

			virtualText.rectTransform.sizeDelta = new Vector2(
				a * actualText.rectTransform.sizeDelta.x,
				a * actualText.rectTransform.sizeDelta.y);
		}
	}
}
