using System.Collections.Generic;
using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
	public class SelectionMenu : MonoBehaviour
	{
		private AirTrafficControl airTrafficControl;
		private List<SelectionOption> options = new List<SelectionOption>();
		private int current;


		public bool IsShowing { get; private set; }


		private void Start()
		{
			int c = transform.childCount;
			for(int i = 0; i < c; i++)
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
		}


		public void Initialize(
			AirTrafficControl airTrafficControl,
			InteractableButton okButton,
			InteractableButton upButton,
			InteractableButton downButton)
		{
			this.airTrafficControl = airTrafficControl;
			okButton.AddListener(OnOkClicked);
			upButton.AddListener(OnUpClicked);
			downButton.AddListener(OnDownClicked);
		}


		public void Show(string[] options)
		{
			for(int i = 0; i < this.options.Count; i++)
			{
				SelectionOption option = this.options[i];
				if(i > options.Length)
				{
					option.SetOption(null);
				}
				else
				{
					string label = options[i];
					option.SetOption(label);
				}
			}

			gameObject.SetActive(false);
			IsShowing = true;
			SetSelection(0);
		}

		private void SetSelection(int next)
		{
			options[current].Deselect();
			options[next].Select();
			current = next;
		}


		private void OnOkClicked()
		{
			if (!IsShowing)
			{
				return;
			}

			if(current < options.Count - 1)
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

			int next = (current - 1) % options.Count;
			SetSelection(next);
		}

		private void OnDownClicked()
		{
			if (!IsShowing)
			{
				return;
			}

			int next = (current + 1) % options.Count;
			SetSelection(next);
		}
	}
}
