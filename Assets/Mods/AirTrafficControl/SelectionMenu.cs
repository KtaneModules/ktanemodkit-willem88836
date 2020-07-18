using System.Collections.Generic;
using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
	public class SelectionMenu : MonoBehaviour
	{
		[SerializeField] private string returnLabel;
		private AirTrafficControl airTrafficControl;
		private List<SelectionOption> options = new List<SelectionOption>();
		private int current;
		private bool startedThisframe = false;
		private int activeOptionCount;


		public bool IsShowing { get; private set; }

		
		private void LateUpdate()
		{
			startedThisframe = false;
		}


		public void Initialize(
			AirTrafficControl airTrafficControl,
			InteractableButton okButton,
			InteractableButton upButton,
			InteractableButton downButton)
		{
			this.airTrafficControl = airTrafficControl;

			int c = transform.childCount;
			for (int i = 0; i < c; i++)
			{
				Transform container = transform.GetChild(i);
				int d = container.childCount;

				for (int j = 0; j < d; j++)
				{
					Transform child = container.GetChild(j);
					SelectionOption option = child.GetComponent<SelectionOption>();
					option.Index = j;
					options.Add(option);
				}
			}

			okButton.AddListener(OnOkClicked);
			upButton.AddListener(OnUpClicked);
			downButton.AddListener(OnDownClicked);
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

			activeOptionCount = options.Length;
			startedThisframe = true;
		}

		private void SetSelection(int next)
		{
			options[current].Deselect();
			options[next].Select();
			current = next;
		}


		private void OnOkClicked()
		{
			if (!IsShowing || startedThisframe)
			{
				return;
			}

			if (current < activeOptionCount)
			{
				SelectionOption option = options[current];
				int index = option.Index;
				airTrafficControl.OnSelect(index);
			}

			IsShowing = false;
			gameObject.SetActive(false);
		}

		private void OnUpClicked()
		{
			if (!IsShowing)
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
			if (!IsShowing)
			{
				return;
			}

			int next = (current + 1) % (activeOptionCount + 1);
			SetSelection(next);
		}
	}
}
