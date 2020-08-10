using System.Collections.Generic;
using UnityEngine;

namespace WillemMeijer.NMTechSupport
{
	public class SelectionMenu : MonoBehaviour
	{
		[SerializeField] private string returnLabel;

		private AirTrafficControl airTrafficControl;
		private List<SelectionOption> options = new List<SelectionOption>();

		private Lock interactionLock;
		private Lock screenLock;

		// Currently selected option.
		private int current;
		// startedThisFrame resolves the issue where an option is selected
		// immediately when the menu is opened (caused by the event order of
		// the interaction buttons). 
		private bool startedThisframe = false;
		// Number of options for the active selection.
		private int activeOptionCount;


		public bool IsShowing { get; private set; }

		
		private void LateUpdate()
		{
			startedThisframe = false;
		}


		public void Initialize(
			AirTrafficControl airTrafficControl,
			Lock interactionLock,
			Lock screenLock,
			InteractableButton okButton,
			InteractableButton upButton,
			InteractableButton downButton)
		{
			this.airTrafficControl = airTrafficControl;
			this.interactionLock = interactionLock;
			this.screenLock = screenLock;

			okButton.AddListener(OnOkClicked);
			upButton.AddListener(OnUpClicked);
			downButton.AddListener(OnDownClicked);


			// Finds all SelectionOptions in children. 
			int c = transform.childCount;
			int k = 0;
			for (int i = 0; i < c; i++)
			{
				Transform container = transform.GetChild(i);
				int d = container.childCount;

				for (int j = 0; j < d; j++)
				{
					Transform child = container.GetChild(j);
					SelectionOption option = child.GetComponent<SelectionOption>();
					option.Index = k;
					k++;
					options.Add(option);
				}
			}
		}


		public void Show(string[] options)
		{
			for(int i = 0; i < this.options.Count; i++)
			{
				SelectionOption option = this.options[i];
				if(i > options.Length || i > this.options.Count)
				{
					option.SetOption(null);
				}
				else if(i == options.Length || i == this.options.Count)
				{
					option.SetOption(returnLabel);
				}
				else
				{
					string label = options[i];
					option.SetOption(label);
				}
			}

			IsShowing = true;
			SetSelection(0);
			gameObject.SetActive(true);
			screenLock.Claim(this);

			activeOptionCount = options.Length;
			startedThisframe = true;
		}

		public void Disable()
		{
			screenLock.Unclaim(this);
			IsShowing = false;
			gameObject.SetActive(false);
		}


		private void SetSelection(int next)
		{
			options[current].Deselect();
			options[next].Select();
			current = next;
		}

		private void OnOkClicked()
		{
			if (startedThisframe 
				|| !interactionLock.Available 
				|| !screenLock.IsOwnedBy(this))
			{
				return;
			}

			if (current < activeOptionCount)
			{
				SelectionOption option = options[current];
				int index = option.Index;
				airTrafficControl.OnSelect(index);
			}

			Disable();
		}

		private void OnUpClicked()
		{
			if (!interactionLock.Available 
				|| !screenLock.IsOwnedBy(this))
			{
				return;
			}

			int next = (current - 1);
			if(next < 0)
			{
				next = activeOptionCount + 1 + next;
			}
			SetSelection(next);
		}

		private void OnDownClicked()
		{
			if (!interactionLock.Available
				|| !screenLock.IsOwnedBy(this))
			{
				return;
			}

			int next = (current + 1) % (activeOptionCount + 1);
			SetSelection(next);
		}
	}
}
