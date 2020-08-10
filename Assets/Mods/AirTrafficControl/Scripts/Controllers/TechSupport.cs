using KModkit;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WillemMeijer.NMTechSupport
{
	[RequireComponent(typeof(KMBombInfo))]
	[RequireComponent(typeof(KMNeedyModule))]
	public class TechSupport : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private TechSupportData data;
		[SerializeField] private VirtualConsole console;
		[SerializeField] private GameObject errorLightPrefab;
		[SerializeField] private InteractableButton okButton;
		[SerializeField] private InteractableButton upButton;
		[SerializeField] private InteractableButton downButton;

		[Header("Settings")]
		[SerializeField] private Int2 interruptInterval;

		[Header("Text")]
		[SerializeField] private string errorFormat;
		[SerializeField] private string selectedOptionFormat;
		[SerializeField] private string unselectedOptionFormat;
		[SerializeField] private string optionConfirmedFormat;
		[SerializeField] private string moduleReleasedFormat;
		[SerializeField] private string startMessage;
		[SerializeField] private string selectVersionMessage;
		[SerializeField] private string selectPatchFileMessage;
		[SerializeField] private string selectParametersMessage;
		[SerializeField] private string incorrectSelectionMessage;
		[SerializeField] private string correctSelectionMessage;


		private KMBombInfo bombInfo;
		private KMNeedyModule needyModule;
		private SevenSegDisplay segDisplay;
		private MonoRandom monoRandom;

		// Respectively: module, selectable, passed light, error light.
		private List<Quatruple<KMBombModule, KMSelectable, GameObject, GameObject>> interruptableModules;
		private int interrupted;
		private KMSelectable.OnInteractHandler interruptedInteractHandler;

		private int currentOption;
		private List<Tuple<string, int>> options;
		private Action OnSelected;


		private void Start()
		{
			bombInfo = GetComponent<KMBombInfo>();
			needyModule = GetComponent<KMNeedyModule>();
			interruptableModules = new List<Quatruple<KMBombModule, KMSelectable, GameObject, GameObject>>();
			options = new List<Tuple<string, int>>();

			// TODO: do something with KMSeedable here.
			monoRandom = new MonoRandom(0);
			data.Generate(monoRandom, 16, 12, 9, 9, 9);

			// Adds methods to buttons.
			okButton.AddListener(OnOKClicked);
			upButton.AddListener(OnUpClicked);
			downButton.AddListener(OnDownClicked);

			// Starts interrupting.
			StartCoroutine(DelayedStart());
		}


		private IEnumerator<YieldInstruction> DelayedStart()
		{
			yield return new WaitForEndOfFrame();

			FindAllModules();
			NeedyTimer timer = GetComponentInChildren<NeedyTimer>();
			segDisplay = timer.Display;
			// Disables the original timer, to assure TechSupport has full control.
			timer.StopTimer(NeedyTimer.NeedyState.Terminated);
			segDisplay.On = true;

			string message = string.Format(startMessage, bombInfo.GetSerialNumber());
			console.Show(message);

			StartCoroutine(Interrupt());
		}

		private void FindAllModules()
		{
			KMBombModule[] bombModules = FindObjectsOfType<KMBombModule>();

			foreach (KMBombModule bombModule in bombModules)
			{
				// Collects the module's KMSelectable.
				KMSelectable selectable = bombModule.GetComponent<KMSelectable>();

				// Spawns the module's error light.
				// Selects the module's pass light.
				StatusLight light = bombModule.gameObject.GetComponentInChildren<StatusLight>();
				GameObject passLight = light.PassLight;
				GameObject errorLight = Instantiate(errorLightPrefab, light.transform);

				// Stores the acquired data.
				Quatruple<KMBombModule, KMSelectable, GameObject, GameObject> interruptableModule
					= new Quatruple<KMBombModule, KMSelectable, GameObject, GameObject>(bombModule, selectable, passLight, errorLight);
				interruptableModules.Add(interruptableModule);
			}
		}


		private IEnumerator<YieldInstruction> Interrupt()
		{
			// After X seconds, a module is interrupted.
			int d = Random.Range(interruptInterval.X, interruptInterval.Y);

			while (d >= 0)
			{
				segDisplay.DisplayValue = Mathf.Min(99, d);
				d--;
				yield return new WaitForSeconds(1);
			}


			// Selects module to interrupt.
			Quatruple<KMBombModule, KMSelectable, GameObject, GameObject> selected = null;
			do
			{
				// Small safety measure to prevent bricking the bomb
				// if for whatever reason there are no more modules.
				if (interruptableModules.Count == 0)
				{
					StopCoroutine(Interrupt());
					break;
				}

				interrupted = Random.Range(0, interruptableModules.Count);
				var current = interruptableModules[interrupted];

				if (!current.C.activeSelf)
				{
					selected = current;
				}
				else
				{
					// If the module is passed, it can no longer be interrupted.
					interruptableModules.RemoveAt(interrupted);
				}
			} while (selected == null);

			// All other lights are disabled, and the error light is enabled.
			Transform parent = selected.D.transform.parent;
			int c = parent.childCount;
			for (int i = 0; i < c; i++)
			{
				parent.GetChild(i).gameObject.SetActive(false);
			}

			selected.D.SetActive(true);

			// Disabling all interaction with the module.
			interruptedInteractHandler = selected.B.OnInteract;
			selected.B.OnInteract = new KMSelectable.OnInteractHandler(delegate { return false; });

			// Updating the console. 
			ErrorData error = data.GenerateError();
			string message = string.Format(errorFormat, selected.A.ModuleDisplayName, error.Error, error.SourceFile, error.LineIndex, error.ColumnIndex);
			console.Show(message);

			StartVersionSelection();
		}


		private void StartVersionSelection()
		{
			ShowOptions(TechSupportData.VersionNumbers, selectVersionMessage);

			OnSelected = delegate
			{
				ConfirmSelection();
				Debug.Log("hit!");

				bool isCorrect = true;
				if (isCorrect)
				{
					console.Show(correctSelectionMessage);
					StartPatchFileSelection();
				}
				else
				{
					needyModule.HandleStrike();
					console.Show(incorrectSelectionMessage);
					StartVersionSelection();
				}
			};
		}

		private void StartPatchFileSelection()
		{
			ShowOptions(TechSupportData.PatchFiles, selectPatchFileMessage);

			OnSelected = delegate
			{
				ConfirmSelection();

				bool isCorrect = true;
				if (isCorrect)
				{
					console.Show(correctSelectionMessage);
					StartParametersSelection();
				}
				else
				{
					needyModule.HandleStrike();
					console.Show(incorrectSelectionMessage);
					StartPatchFileSelection();
				}
			};
		}

		private void StartParametersSelection()
		{
			ShowOptions(TechSupportData.Parameters, selectParametersMessage);

			OnSelected = delegate
			{
				ConfirmSelection();

				bool isCorrect = true;
				if (isCorrect)
				{
					Quatruple<KMBombModule, KMSelectable, GameObject, GameObject> module = interruptableModules[interrupted];

					// Console updates and removes interaction.
					console.Show(correctSelectionMessage);
					string message = string.Format(moduleReleasedFormat, module.A.ModuleDisplayName);
					console.Show(message);
					OnSelected = null;

					// Enables interrupted module.
					module.B.OnInteract = interruptedInteractHandler;
					Transform parent = module.C.transform.parent;
					int c = parent.childCount;
					for (int i = 0; i < c; i++)
					{
						Transform child = parent.GetChild(i);
						if (child.name == "Component_LED_OFF")
						{
							child.gameObject.SetActive(true);
						}
						else
						{
							child.gameObject.SetActive(false);
						}
					}

					StartCoroutine(Interrupt());
				}
				else
				{
					needyModule.HandleStrike();
					console.Show(incorrectSelectionMessage);
					StartParametersSelection();
				}
			};
		}


		private void ShowOptions(string[] list, string header)
		{
			console.Show(header);

			currentOption = 0;
			options.Clear();
			for (int i = 0; i < list.Length; i++)
			{
				string message = string.Format(unselectedOptionFormat, list[i]);
				int h = console.Show(message);
				Tuple<string, int> option = new Tuple<string, int>(list[i], h);
				options.Add(option);
			}

			UpdateSelected(0);
		}

		private void ConfirmSelection()
		{
			for (int i = 0; i < options.Count; i++)
			{
				if (i != currentOption)
				{
					console.Remove(options[i].B);
				}
			}

			Tuple<string, int> selected = options[currentOption];
			string message = string.Format(optionConfirmedFormat, selected.A);
			console.Replace(selected.B, message);
		}

		private void UpdateSelected(int previous)
		{
			Tuple<string, int> ph = options[previous];
			string message = string.Format(unselectedOptionFormat, ph.A);
			ph.B = console.Replace(ph.B, message);
			options[previous] = ph;

			Tuple<string, int> ch = options[currentOption];
			message = string.Format(selectedOptionFormat, ch.A);
			ch.B = console.Replace(ch.B, message);
			options[currentOption] = ch;
		}


		private void OnOKClicked()
		{
			if (OnSelected != null)
			{
				OnSelected.Invoke();
			}
		}

		private void OnUpClicked()
		{
			int previous = currentOption;
			currentOption--;
			if (currentOption <= 0)
			{
				currentOption = 0;
			}
			UpdateSelected(previous);
		}

		private void OnDownClicked()
		{
			int previous = currentOption;
			currentOption++;
			if (currentOption >= options.Count - 1)
			{
				currentOption = options.Count - 1;
			}
			UpdateSelected(previous);
		}
	}
}
