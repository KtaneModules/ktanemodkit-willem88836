using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMTechSupport
{
	public class MessageField : MonoBehaviour
	{
		[SerializeField] private Text messageField;
		[SerializeField] private float flickerSpeed;

		private Lock interactionLock;
		private Lock screenLock;
		private object previousScreenOwner = null;
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


		public void Initialize(
			Lock interactionLock, 
			Lock screenLock, 
			InteractableButton okButton)
		{
			this.interactionLock = interactionLock;
			this.screenLock = screenLock;
			this.okButton = okButton;
			okButton.AddListener(OnClick);
		}


		public void ShowMessage(string message)
		{
			IsDisplaying = true;
			messageField.text = message.Replace("\\n", "\n");
			previousScreenOwner = screenLock.Owner;
			screenLock.Claim(this);
		}

		public void Close()
		{

			IsDisplaying = false;
			flicker = 0;
			firstChild.SetActive(false);

			if (previousScreenOwner == (object)this)
			{
				screenLock.Unclaim(this);
			}
			else
			{
				screenLock.Claim(previousScreenOwner);
			}
		}

		private void OnClick()
		{
			if (!screenLock.IsOwnedBy(this) 
				|| !interactionLock.Available)
			{
				return;
			}

			Close();
		}
	}
}
