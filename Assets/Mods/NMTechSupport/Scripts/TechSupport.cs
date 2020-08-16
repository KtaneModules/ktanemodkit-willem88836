using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


[RequireComponent(typeof(KMBombInfo))]
[RequireComponent(typeof(KMNeedyModule))]
[RequireComponent(typeof(KMAudio))]
public class TechSupport : MonoBehaviour
{
	[Header("Debug")]
	[SerializeField] private bool forceVersionCorrect = false;
	[SerializeField] private bool forcePatchFileCorrect = false;
	[SerializeField] private bool forceParametersCorrect = false;

	[Header("References")]
	[SerializeField] private TechSupportData data;
	[SerializeField] private VirtualConsole console;
	[SerializeField] private GameObject errorLightPrefab;
	[SerializeField] private InteractableButton okButton;
	[SerializeField] private InteractableButton upButton;
	[SerializeField] private InteractableButton downButton;
	[SerializeField] private ObjectToggle screenOverlay;

	[Header("Settings")]
	[SerializeField] private int rebootDuration;

	[Header("Text")]
	[SerializeField] private string errorFormat;
	[SerializeField] private string selectedOptionFormat;
	[SerializeField] private string unselectedOptionFormat;
	[SerializeField] private string optionConfirmedFormat;
	[SerializeField] private string moduleReleasedFormat;
	[SerializeField] private string startMessage;
	[SerializeField] private string startMessageExtended;
	[SerializeField] private string selectVersionMessage;
	[SerializeField] private string selectPatchFileMessage;
	[SerializeField] private string selectParametersMessage;
	[SerializeField] private string incorrectSelectionMessage;
	[SerializeField] private string correctSelectionMessage;
	[SerializeField] private string timerExpiredMessage;
	[SerializeField] private string[] rebootMessages;
	[SerializeField] private string rebootCompletedMessage;


	private KMBombInfo bombInfo;
	private KMNeedyModule needyModule;
	private KMAudio bombAudio;
	private MonoRandom monoRandom;

	// Respectively: module, selectable, passed light, error light.
	private List<Quatruple<KMBombModule, KMSelectable, GameObject, GameObject>> interruptableModules;
	private int interrupted;
	private KMSelectable.OnInteractHandler interruptedInteractHandler;
	private ErrorData errorData;

	private int selectedOption;
	private List<Tuple<string, int>> options;
	private Action OnSelected;
	private List<ErrorData> allErrors;
	private bool moduleResolved;


	public void Start()
	{
		bombInfo = GetComponent<KMBombInfo>();
		needyModule = GetComponent<KMNeedyModule>();
		bombAudio = GetComponent<KMAudio>();

		interruptableModules = new List<Quatruple<KMBombModule, KMSelectable, GameObject, GameObject>>();
		options = new List<Tuple<string, int>>();
		allErrors = new List<ErrorData>();

		needyModule.OnActivate += OnActivate;
		needyModule.OnNeedyActivation += Interrupt;
		needyModule.OnTimerExpired += OnTimerExpired; 

		screenOverlay.Toggle(false);

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

	private IEnumerator DelayedStart()
	{
		yield return new WaitForEndOfFrame();

		KMBombModule[] bombModules = FindObjectsOfType<KMBombModule>();

		foreach (KMBombModule bombModule in bombModules)
		{
			try
			{
				// Collects the module's KMSelectable.
				KMSelectable selectable = bombModule.GetComponent<KMSelectable>();

				GameObject passLight = TransformUtilities.FindChildIn(bombModule.transform, "Component_LED_PASS").gameObject;
				Transform statusLight = passLight.transform.parent;
				GameObject errorLight = Instantiate(
					errorLightPrefab,
					statusLight.position,
					statusLight.rotation,
					statusLight.transform);
				errorLight.SetActive(false);

				// Stores the acquired data.
				Quatruple<KMBombModule, KMSelectable, GameObject, GameObject> interruptableModule
					= new Quatruple<KMBombModule, KMSelectable, GameObject, GameObject>(bombModule, selectable, passLight, errorLight);
				interruptableModules.Add(interruptableModule);
			}
			catch (Exception exception)
			{
				TechSupportLog.LogFormat
					("Set-Up Interruptable ({0}) failed with message ({1}), at ({2}).",
					bombModule.ModuleDisplayName,
					exception.Message,
					exception.StackTrace);
			}
		}

		TechSupportLog.LogFormat("Loaded total of {0} interruptable modules", interruptableModules.Count);
	}


	#region NeedyModuleHooks

	public void OnActivate()
	{
		screenOverlay.Toggle(true);
		string message = string.Format(startMessage, bombInfo.GetSerialNumber());
		console.Show(message);
		message = string.Format(startMessageExtended, interruptableModules.Count);
		console.Show(message);
	}

	private void Interrupt()
	{
		moduleResolved = false;

		// Selects module to interrupt.
		Quatruple<KMBombModule, KMSelectable, GameObject, GameObject> selected = null;
		do
		{
			// Small safety measure to prevent bricking the bomb
			// if for whatever reason there are no more modules.
			if (interruptableModules.Count == 0)
			{
				TechSupportLog.Log("Out of Interruptable modules.");
				return;
			}

			interrupted = Random.Range(0, interruptableModules.Count);
			var current = interruptableModules[interrupted];

			// a module can't be interrupted when it's either finished 
			// or already being interrupted.
			if (!current.C.activeSelf && !current.D.activeSelf)
			{
				selected = current;
			}
			else if (current.C.activeSelf)
			{
				// If the module is passed, it can no longer be interrupted.
				interruptableModules.RemoveAt(interrupted);
			}
		} while (selected == null);

		TechSupportLog.LogFormat("Interrupting: {0}", selected.A.ModuleDisplayName);

		// All other lights are disabled, and the error light is enabled.
		Transform parent = selected.D.transform.parent;
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			parent.GetChild(i).gameObject.SetActive(false);
		}

		selected.D.SetActive(true);

		// Disabling all interaction with the module.
		interruptedInteractHandler = selected.B.OnInteract;
		selected.B.OnInteract = new KMSelectable.OnInteractHandler(delegate 
		{
			bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.NeedyActivated, selected.A.transform);
			return false; 
		});

		// Updating the console. 
		errorData = data.GenerateError();
		allErrors.Add(errorData);
		string message = string.Format(errorFormat, selected.A.ModuleDisplayName, errorData.Error, errorData.SourceFile, errorData.LineIndex, errorData.ColumnIndex);
		console.Show(message);

		StartVersionSelection();
	}

	private void OnTimerExpired()
	{
		if (moduleResolved)
		{
			return;
		}

		TechSupportLog.Log("STRIKE: Timer Expired");
		needyModule.HandleStrike();

		for (int i = 0; i < options.Count; i++)
		{
			console.Remove(options[i].B);
		}

		console.Show(timerExpiredMessage);
		StartCoroutine(RebootModule());
	}

	#endregion


	#region ModuleStages

	private void StartVersionSelection()
	{
		ShowOptions(TechSupportData.VersionNumbers, selectVersionMessage);

		OnSelected = delegate
		{
			ConfirmSelection();

			int correctVersion = CorrectVersion();

			TechSupportLog.LogFormat("Software Version: Selected option: {0}; Correct option {1}.", 
				TechSupportData.VersionNumbers[selectedOption], 
				TechSupportData.VersionNumbers[correctVersion]);

			if (selectedOption == correctVersion || forceVersionCorrect)
			{
				console.Show(correctSelectionMessage);
				StartPatchFileSelection();
			}
			else
			{
				TechSupportLog.Log("STRIKE: Wrong software version.");
				needyModule.HandleStrike();
				console.Show(incorrectSelectionMessage);
				StartVersionSelection();
			}
		};
	}
	private int CorrectVersion()
	{
		int correctVersion = TechSupportData.OriginSerialCrossTable[errorData.ErrorIndex, errorData.SourceFileIndex];
		return correctVersion;
	}


	private void StartPatchFileSelection()
	{
		ShowOptions(TechSupportData.PatchFiles, selectPatchFileMessage);

		OnSelected = delegate
		{
			ConfirmSelection();

			int correctPatchFile = CorrectPatchFile();

			TechSupportLog.LogFormat("Patch File: Selected option: {0}; Correct option {1}.",
				TechSupportData.PatchFiles[selectedOption],
				TechSupportData.PatchFiles[correctPatchFile]);

			if (selectedOption == correctPatchFile || forcePatchFileCorrect)
			{
				console.Show(correctSelectionMessage);
				StartParametersSelection();
			}
			else
			{
				TechSupportLog.Log("STRIKE: Wrong patch file.");
				needyModule.HandleStrike();
				console.Show(incorrectSelectionMessage);
				StartPatchFileSelection();
			}
		};
	}
	private int CorrectPatchFile()
	{
		// Data where seed = 0;
		// 0 "prle.cba",
		// 1 "resble.bbc",
		// 2 "razcle.pxi",
		// 3 "wane.drf",
		// 4 "faee.sup",
		// 5 "exed.asc",
		// 6 "gilick.pxd",
		// 7 "linion.dart",
		// 8 "lonist.ftl"

		//However, if the error's source file is either satcle.bb, plor.pom, or equely.ctl, ignore all rules above and select exed.asc.
		if (errorData.SourceFileIndex == 2
			|| errorData.SourceFileIndex == 5
			|| errorData.SourceFileIndex == 9)
		{
			return 5;
		}

		//If any of the error code's letters are contained in the crashed source file's name, select wane.drf.
		for (int i = 2; i < errorData.Error.Length; i++)
		{
			string l1 = errorData.Error[i].ToString().ToLower();
			foreach(char l in errorData.SourceFile)
			{
				string l2 = l.ToString().ToLower();
				if(l1 == l2)
				{
					return 3;
				}
			}
		}

		//Otherwise, if the error's line and column are both even, select razcle.pxi.
		if (errorData.LineIndex % 2 == 0
			&& errorData.ColumnIndex % 2 == 0)
		{
			return 2;
		}

		//Otherwise, if the source file's number of vowels is equal to or greater than the number of consonants, or the column index is higher than the line index, select faee.sup.
		int v = 0;
		int c = 0;
		foreach(char l in errorData.SourceFile)
		{
			if (l == '.')
			{
				break;
			}

			bool isVowel = "aeiou".IndexOf(l) >= 0;

			if (isVowel)
			{
				v++;
			}
			else
			{
				c++;
			}
		}

		if(v >= c)
		{
			return 4;
		}

		//Otherwise, if the source file's first letter is in the last fourth of the alphabet, select prle.cba.
		if(StringManipulation.AlphabetToIntPlusOne(errorData.SourceFile[0]) 
			>= 26f / 4f * 3f)
		{
			return 0;
		}

		//Otherwise, if the less than 99 seconds is still available and the column is higher than 75, select linion.dart.
		if (needyModule.GetNeedyTimeRemaining() < 99f 
			&& errorData.ColumnIndex > 75)
		{
			return 7;
		}

		//Otherwise, if this is the fourth or later crash and the cumulative line number of all previous errors is over 450, select gilick.pxd.
		if (allErrors.Count >= 4)
		{
			int cumulativeLines = 0;
			foreach (ErrorData error in allErrors)
			{
				cumulativeLines += error.LineIndex;
			}

			if (cumulativeLines >= 450)
			{
				return 6;
			}
		}

		//Otherwise, select shuttle lonist.ftl.
		return 8;
	}


	private void StartParametersSelection()
	{
		ShowOptions(TechSupportData.Parameters, selectParametersMessage);

		OnSelected = delegate
		{
			ConfirmSelection();

			int correctParameter = CorrectParameter();

			TechSupportLog.LogFormat("Parameter: Selected option: {0}; Correct option {1}.",
				TechSupportData.Parameters[selectedOption],
				TechSupportData.Parameters[correctParameter]);

			if (selectedOption == correctParameter || forceParametersCorrect)
			{
				ReactivateInterruptedModule();

				Quatruple<KMBombModule, KMSelectable, GameObject, GameObject> module = interruptableModules[interrupted];

				console.Show(correctSelectionMessage);
				string message = string.Format(moduleReleasedFormat, module.A.ModuleDisplayName);
				console.Show(message);

				moduleResolved = true;
			}
			else
			{
				TechSupportLog.Log("STRIKE: Wrong parameters.");
				needyModule.HandleStrike();
				console.Show(incorrectSelectionMessage);
				StartParametersSelection();
			}
		};
	}
	private int CorrectParameter()
	{
		// sum of the first three icons.
		int a = 0;
		for (int i = 2; i< 5; i++)
		{
			char l = errorData.Error[i];
			int k = "1234567890".IndexOf(l) >= 0
				? int.Parse(l.ToString())
				: StringManipulation.AlphabetToIntPlusOne(l);
			a += k;
		}

		// sum of the last three icons.
		int b = 0;
		for (int i = 5; i < 8; i++)
		{
			char l = errorData.Error[i];
			int k = "1234567890".IndexOf(l) >= 0
				? int.Parse(l.ToString())
				: StringManipulation.AlphabetToIntPlusOne(l);
			b += k;
		}


		// multiplied by line/column, calculated delta.
		int x = Mathf.Abs((a * errorData.LineIndex) - (b * errorData.ColumnIndex));
		// xor operation.
		x ^= a * b;
		// subtracting parameter count until it is below that. 
		x %= TechSupportData.Parameters.Length;

		return x;
	}

	#endregion


	#region Resetting

	private void ReactivateInterruptedModule()
	{
		Quatruple<KMBombModule, KMSelectable, GameObject, GameObject> module = interruptableModules[interrupted];

		OnSelected = null;

		// Enables interrupted module.
		module.B.OnInteract = interruptedInteractHandler;
		Transform parent = module.C.transform.parent;
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
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
	}

	private IEnumerator RebootModule()
	{
		string moduleName = interruptableModules[interrupted].A.ModuleDisplayName;
		string message = string.Format(rebootMessages[0], moduleName);
		int messageHash = console.Show(message);
		for (int i = 1; i < rebootDuration + 1; i++)
		{
			yield return new WaitForSeconds(1);
			message = string.Format(rebootMessages[i % rebootMessages.Length], moduleName);
			messageHash = console.Replace(messageHash, message);
		}

		message = string.Format(rebootCompletedMessage, moduleName);
		console.Replace(messageHash, message);
		
		options.Clear();
		selectedOption = 0;

		ReactivateInterruptedModule();
	}

	#endregion


	#region Selection

	private void ShowOptions(string[] list, string caption)
	{
		console.Show(caption);

		selectedOption = 0;
		options.Clear();
		for (int i = 0; i < list.Length; i++)
		{
			string message = string.Format(unselectedOptionFormat, list[i]);
			int hash = console.Show(message);
			Tuple<string, int> option = new Tuple<string, int>(list[i], hash);
			options.Add(option);
		}

		UpdateSelected(0);
	}

	private void ConfirmSelection()
	{
		for (int i = 0; i < options.Count; i++)
		{
			if (i != selectedOption)
			{
				console.Remove(options[i].B);
			}
		}

		Tuple<string, int> selected = options[selectedOption];
		string message = string.Format(optionConfirmedFormat, selected.A);
		console.Replace(selected.B, message);
	}

	private void UpdateSelected(int previous)
	{
		if(options.Count == 0)
		{
			return;
		}

		Tuple<string, int> hashPrevious = options[previous];
		string message = string.Format(unselectedOptionFormat, hashPrevious.A);
		hashPrevious.B = console.Replace(hashPrevious.B, message);
		options[previous] = hashPrevious;

		Tuple<string, int> hashCurrent = options[selectedOption];
		message = string.Format(selectedOptionFormat, hashCurrent.A);
		hashCurrent.B = console.Replace(hashCurrent.B, message);
		options[selectedOption] = hashCurrent;
	}

	#endregion


	#region ModuleInteraction


	private void OnOKClicked()
	{
		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, okButton.transform);

		if (OnSelected != null)
		{
			OnSelected.Invoke();
		}
	}

	private void OnUpClicked()
	{
		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, upButton.transform);

		int previous = selectedOption;
		selectedOption--;
		if (selectedOption <= 0)
		{
			selectedOption = 0;
		}
		UpdateSelected(previous);
	}

	private void OnDownClicked()
	{
		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, downButton.transform);

		int previous = selectedOption;
		selectedOption++;
		if (selectedOption >= options.Count - 1)
		{
			selectedOption = options.Count - 1;
		}
		UpdateSelected(previous);
	}

	#endregion


	#region TwitchPlays

	public readonly string TwitchHelpMessage = "Press the Confirm button with \"!confirm\". " +
		"Press the up button with \"!up\" followed by the number times you want to go up." +
		"Press the down button with \"!down\" followed by the number of timers you want to go down.";

	public IEnumerator ProcessTwitchCommand(string command)
	{
		command = command.ToLowerInvariant().Trim();
		string[] split = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

		if (split[0] == "confirm")
		{
			OnOKClicked();
			yield return new WaitForEndOfFrame();
		}
		else if (split[0] == "up")
		{
			int i;
			if(int.TryParse(split[1], out i) && i > 0 && i < 10)
			{
				for (; i > 0; i--)
				{
					OnUpClicked();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
		else if (split[0] == "down")
		{
			int i;
			if (int.TryParse(split[1], out i) && i > 0 && i < 10)
			{
				for (; i > 0; i--)
				{
					OnDownClicked();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
	}

	#endregion
}
