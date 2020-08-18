using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


[RequireComponent(typeof(KMBombInfo), typeof(KMNeedyModule), typeof(KMAudio)), 
	RequireComponent(typeof(KMBossModule), typeof(TechSupportService), typeof(TechSupportData))]
public class TechSupport : MonoBehaviour
{
	[Header("Debug")]
	[SerializeField] private bool forceVersionCorrect = false;
	[SerializeField] private bool forcePatchFileCorrect = false;
	[SerializeField] private bool forceParametersCorrect = false;

	[Header("References")]
	[SerializeField] private TextAsset backUpIgnoreList;
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
	[SerializeField] private string[] exceptionWithoutModuleMessages;


	private KMBombInfo bombInfo;
	private KMNeedyModule needyModule;
	private KMAudio bombAudio;
	private TechSupportData data;
	private MonoRandom monoRandom;

	// Respectively: module, selectable, passed light, error light.
	private List<InterruptableModule> interruptableModules;
	private int interrupted;
	private KMSelectable.OnInteractHandler interruptedInteractHandler;
	private ErrorData errorData;

	private int selectedOption;
	private List<Tuple<string, int>> options;
	private Action onSelected;
	private List<ErrorData> allErrors;
	private bool moduleResolved;


	public void Start()
	{
		bombInfo = GetComponent<KMBombInfo>();
		needyModule = GetComponent<KMNeedyModule>();
		bombAudio = GetComponent<KMAudio>();
		data = GetComponent<TechSupportData>();

		interruptableModules = new List<InterruptableModule>();
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

		StartCoroutine(DelayedStart());
	}

	private IEnumerator DelayedStart()
	{
		TechSupportService mysteryKeyService = GetComponent<TechSupportService>();

		while (!mysteryKeyService.SettingsLoaded)
		{
			yield return null;
		}

		KMBossModule bossModule = GetComponent<KMBossModule>();
		string[] ignoredModules = bossModule.GetIgnoredModules(needyModule.ModuleDisplayName);

		if (ignoredModules == null || ignoredModules.Length == 0)
		{
			TechSupportLog.Log("Using backup ignorelist.");
			ignoredModules = backUpIgnoreList.text.Split('\n');
		}

		KMBombModule[] bombModules = FindObjectsOfType<KMBombModule>();

		foreach (KMBombModule bombModule in bombModules)
		{
			try
			{
				bool mustNotBeHidden = mysteryKeyService.MustNotBeHidden(bombModule.ModuleType);
				bool isIgnored = ignoredModules.Contains(bombModule.ModuleDisplayName);

				// Ignored modules are ignored.
				if (mustNotBeHidden || isIgnored)
				{
					TechSupportLog.LogFormat("Ignored module {0} - Must Not Be Hidden: {1}; Is Ignored {2}", 
						bombModule.ModuleDisplayName, 
						mustNotBeHidden, 
						isIgnored);
					continue;
				}

				// Collects the module's KMSelectable.
				KMSelectable selectable = bombModule.GetComponent<KMSelectable>();

				GameObject passLight = TransformUtilities.FindChildIn(bombModule.transform, "Component_LED_PASS").gameObject;
				Transform statusLight = passLight.transform.parent;
				GameObject strikeLight = TransformUtilities.FindChildIn(statusLight, "Component_LED_STRIKE").gameObject;
				GameObject errorLight = Instantiate(
					errorLightPrefab,
					statusLight.position,
					statusLight.rotation,
					statusLight.transform);
				errorLight.SetActive(false);

				// Stores the acquired data.
				InterruptableModule interruptableModule = new InterruptableModule(bombModule, selectable, passLight, strikeLight, errorLight);
				
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
		InterruptableModule selected = null;

		// The module can no longer reset when too little time is left.
		float bombTime = bombInfo.GetTime();
		if (bombTime >= needyModule.CountdownTime)
		{
			InterruptableModule[] potentials = new InterruptableModule[interruptableModules.Count];
			interruptableModules.CopyTo(potentials);
			potentials.Shuffle();

			foreach(InterruptableModule current in potentials)
			{
				// A module is only interrupted when the off light is on, 
				// and it isn't currently used. 
				TechSupportLog.Log(!current.PassLight.activeSelf
					+ " " + !current.StrikeLight.activeSelf
					+ " " + !current.ErrorLight.activeSelf
					+ " " + !current.IsFocussed);

				if (!current.PassLight.activeSelf
					&& !current.StrikeLight.activeSelf
					&& !current.ErrorLight.activeSelf
					&& !current.IsFocussed)
				{
					selected = current;
					break;
				}
				else if (current.PassLight.activeSelf)
				{
					// If the module is passed, it can no longer be interrupted.
					interruptableModules.RemoveAt(interrupted);
				}
			}
		}
		else
		{
			TechSupportLog.Log("Not enough time left, forcing to interrupt without module.");
		}

		// Interrupts that module (if there is one).
		if (selected == null)
		{
			TechSupportLog.Log("Could not find interruptable module. Creating exception without one.");

			errorData = data.GenerateError(null);
			allErrors.Add(errorData);

			string message = exceptionWithoutModuleMessages[Random.Range(0, exceptionWithoutModuleMessages.Length)];
			message = string.Format(message, errorData.Error, errorData.SourceFile, errorData.LineIndex, errorData.ColumnIndex);
			console.Show(message);
		}
		else
		{
			TechSupportLog.LogFormat("Interrupting: {0}", selected.BombModule.ModuleDisplayName);

			// All other lights are disabled, and the error light is enabled.
			Transform parent = selected.PassLight.transform.parent;
			int childCount = parent.childCount;
			for (int i = 0; i < childCount; i++)
			{
				parent.GetChild(i).gameObject.SetActive(false);
			}

			selected.ErrorLight.SetActive(true);

			// Disabling all interaction with the module.
			interruptedInteractHandler = selected.Selectable.OnInteract;
			selected.Selectable.OnInteract = new KMSelectable.OnInteractHandler(delegate 
			{
				bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.NeedyActivated, selected.BombModule.transform);
				return false; 
			});

			// Generating error and Updating the console. 
			errorData = data.GenerateError(selected.BombModule.ModuleDisplayName);
			allErrors.Add(errorData);

			string message = string.Format(errorFormat, selected.BombModule.ModuleDisplayName, errorData.Error, errorData.SourceFile, errorData.LineIndex, errorData.ColumnIndex);
			console.Show(message);
		}

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

		onSelected = delegate
		{
			ConfirmSelection();

			int correctVersion = CorrectVersion(errorData);

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
	private int CorrectVersion(ErrorData errorData)
	{
		int correctVersion = TechSupportData.OriginSerialCrossTable[errorData.ErrorIndex, errorData.SourceFileIndex];
		return correctVersion;
	}


	private void StartPatchFileSelection()
	{
		ShowOptions(TechSupportData.PatchFiles, selectPatchFileMessage);

		onSelected = delegate
		{
			ConfirmSelection();

			int correctPatchFile = CorrectPatchFile(errorData);

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
	private int CorrectPatchFile(ErrorData errorData)
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
			|| errorData.SourceFileIndex == 9)
		{
			return 5;
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

		//Otherwise, if the source file's first letter is in the last third of the alphabet, select prle.cba.
		if(StringManipulation.AlphabetToIntPlusOne(errorData.SourceFile[0]) 
			>= 26f / 3f * 2f)
		{
			return 0;
		}

		//Otherwise, if the less than 99 seconds is still available and the column is higher than 75, select linion.dart.
		if (needyModule.GetNeedyTimeRemaining() < 99f 
			&& errorData.ColumnIndex > 75)
		{
			return 7;
		}

		//Otherwise, if the error's line and column are both even, select razcle.pxi.
		if (errorData.LineIndex % 2 == 0
			&& errorData.ColumnIndex % 2 == 0)
		{
			return 2;
		}

		//If any of the error code's letters are contained in the crashed source file's name, select wane.drf.
		for (int i = 2; i < errorData.Error.Length; i++)
		{
			string l1 = errorData.Error[i].ToString().ToLower();
			foreach (char l in errorData.SourceFile)
			{
				string l2 = l.ToString().ToLower();
				if (l1 == l2)
				{
					return 3;
				}
			}
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

		onSelected = delegate
		{
			ConfirmSelection();

			int correctParameter = CorrectParameter(errorData);

			TechSupportLog.LogFormat("Parameter: Selected option: {0}; Correct option {1}.",
				TechSupportData.Parameters[selectedOption],
				TechSupportData.Parameters[correctParameter]);

			if (selectedOption == correctParameter || forceParametersCorrect)
			{
				ReactivateInterruptedModule();

				InterruptableModule module = interruptableModules[interrupted];

				console.Show(correctSelectionMessage);
				string message = string.Format(moduleReleasedFormat, module.BombModule.ModuleDisplayName);
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
	private int CorrectParameter(ErrorData errorData)
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
		if (errorData.ModuleName == null)
		{
			return;
		}

		InterruptableModule module = interruptableModules[interrupted];

		onSelected = null;

		// Enables interrupted module.
		module.Selectable.OnInteract = interruptedInteractHandler;
		Transform parent = module.PassLight.transform.parent;
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
		string moduleName = interruptableModules[interrupted].BombModule.ModuleDisplayName;
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

		if (onSelected != null)
		{
			onSelected.Invoke();
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
