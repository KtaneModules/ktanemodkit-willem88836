using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMAirTrafficControl
{
	public class MessageField : MonoBehaviour
	{
		[SerializeField] private Text messageField;
		[SerializeField] private float flickerSpeed;
		private InteractableButton okButton;
		private GameObject firstChild;

		public bool IsDisplaying { get; private set; }
		private float flicker;


		private void Start()
		{
			firstChild = transform.GetChild(0).gameObject;
		}

		private void Update()
		{
			if (IsDisplaying && this.okButton != null)
			{
				flicker += Time.deltaTime;
				if (flicker >= flickerSpeed)
				{
					flicker = 0;
					bool isActive = firstChild.activeSelf;
					firstChild.SetActive(!isActive);
				}
			}
		}


		public void Initialize(InteractableButton okButton)
		{
			this.okButton = okButton;
			okButton.AddListener(OnClick);
		}


		public void ShowMessage(string message)
		{
			IsDisplaying = true;
			messageField.text = message.Replace("\\n", "\n");
		}

		private void OnClick()
		{
			if (!IsDisplaying)
			{
				return;
			}

			IsDisplaying = false;
			flicker = 0;
			firstChild.SetActive(false);
		}
	}
}
