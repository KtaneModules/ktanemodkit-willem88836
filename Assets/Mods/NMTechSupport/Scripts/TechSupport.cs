using KModkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NMTechSupport
{
	[RequireComponent(typeof(KMBombInfo), typeof(KMNeedyModule), typeof(KMAudio)), 
		RequireComponent(typeof(KMBossModule), typeof(TechSupportService), typeof(TechSupportData))]
	public class TechSupport : MonoBehaviour
	{
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
		[SerializeField] private RuleParameters parameters;

		[Header("Text")]
		[SerializeField] private string errorFormat;
		[SerializeField] private string selectedOptionFormat;
		[SerializeField] private string unselectedOptionFormat;
		[SerializeField] private string optionConfirmedFormat;
		[SerializeField] private string moduleReleasedFormat;
		[SerializeField] private string makeSelectionMessageFormat;
		[SerializeField] private string startMessage;
		[SerializeField] private string startMessageExtended;
		[SerializeField] private string incorrectSelectionMessage;
		[SerializeField] private string correctSelectionMessage;
		[SerializeField] private string timerExpiredMessage;
		[SerializeField] private string[] rebootMessages;
		[SerializeField] private string rebootCompletedMessage;
		[SerializeField] private string[] exceptionWithoutModuleMessages;
		[SerializeField] private string exceptionWithoutModuleResolvedMessage;
		[SerializeField] private string showParametersAgainMessage;


		private KMBombInfo bombInfo;
		private KMNeedyModule needyModule;
		private KMAudio bombAudio;
		private TechSupportData data;

		// Respectively: module, selectable, passed light, error light.
		private List<InterruptableModule> interruptableModules = new List<InterruptableModule>();
		private int interrupted = -1;
		private KMSelectable.OnInteractHandler interruptedInteractHandler;
		private ErrorData currentError;

		private List<Tuple<string, int>> options = new List<Tuple<string, int>>();
		private List<ErrorData> allErrors = new List<ErrorData>();
		private IModuleStage currentStage;
		private bool moduleResolved;
		private int selectedOption;


		#region Unity Hooks

		public void Start()
		{
			bombInfo = GetComponent<KMBombInfo>();
			needyModule = GetComponent<KMNeedyModule>();
			bombAudio = GetComponent<KMAudio>();
			data = GetComponent<TechSupportData>();
		
			needyModule.OnActivate += OnActivate;
			needyModule.OnNeedyActivation += Interrupt;
			needyModule.OnTimerExpired += OnTimerExpired; 

			screenOverlay.Toggle(false);

			// TODO: do something with KMSeedable here.
			parameters.Random = new MonoRandom(0);
			data.Generate(parameters);

			// Adds methods to buttons.
			okButton.AddListener(OnOKClicked);
			upButton.AddListener(OnUpClicked);
			downButton.AddListener(OnDownClicked);

			StartCoroutine(DelayedStart());
		}

		private void Update()
		{
			if (interrupted > -1)
			{
				// HACK: ensures that when the passlight does turn on for some reason, 
				// the error light is turned off.
				InterruptableModule module = interruptableModules[interrupted];
				if (module.PassLight.activeSelf)
					module.ErrorLight.SetActive(false);
			}
		}

		#endregion


		#region Needy Module Hooks

		public void OnActivate()
		{
			screenOverlay.Toggle(true);
			console.Show(string.Format(startMessage, bombInfo.GetSerialNumber()));
			console.Show(string.Format(startMessageExtended, interruptableModules.Count));
		}

		private void Interrupt()
		{
			moduleResolved = false;

			// Selects module to interrupt.
			InterruptableModule selected = FindInterruptableModule();
			interrupted = interruptableModules.IndexOf(selected);

			// Interrupts that module (if there is one).
			if (selected == null)
				GenerateEmptyError();
			else
				GenerateModuleError(selected);

			currentStage = new VersionStage();
			ShowOptions(currentStage.GetData(), 
				string.Format(makeSelectionMessageFormat, currentStage.GetDisplayName()));
		}

		private void OnTimerExpired()
		{
			if (moduleResolved)
				return;

			TechSupportLog.Log("STRIKE: Timer Expired");
			needyModule.HandleStrike();

			for (int i = 0; i < options.Count; i++)
				console.Remove(options[i].B);

			console.Show(timerExpiredMessage);
			StartCoroutine(RebootModule());
		}

		#endregion


		#region Initialization

		private IEnumerator DelayedStart()
		{
			// A coroutine is used as mystery service and the Boss Module need time to initialize. 
			yield return new WaitForEndOfFrame();

			// Waits for the mysteryservice to be loaded.
			TechSupportService mysteryKeyService = GetComponent<TechSupportService>();
			while (!mysteryKeyService.SettingsLoaded)
				yield return null;

			// Loads the boss module.
			KMBossModule bossModule = GetComponent<KMBossModule>();
			string[] ignoredModules = bossModule.GetIgnoredModules(needyModule.ModuleDisplayName);

			// If there are no ignored modules, something I can't fix will probably 
			// have gone wrong, and therefore, the default (incomplete) list will be loaded.
			if (ignoredModules == null || ignoredModules.Length == 0)
			{
				TechSupportLog.Log("Using backup ignore-list as the hosted list cannot be loaded.");
				ignoredModules = backUpIgnoreList.text.Split('\n');
			}

			LoadInterruptableModules(mysteryKeyService, ignoredModules);
		}

		private void LoadInterruptableModules(TechSupportService mysteryKeyService, string[] ignoredBossModules)
		{
			// An iteration is made through all the active modules in the scene.
			foreach (KMBombModule bombModule in FindObjectsOfType<KMBombModule>())
			{
				// Modules marked by either Mystery Module or Boss Module are ignored.
				if (mysteryKeyService.MustNotBeHidden(bombModule.ModuleType) || ignoredBossModules.Contains(bombModule.ModuleDisplayName))
					continue;

				// Collects the module's KMSelectable, and relevant objects/components and stores them in a container object.
				KMSelectable selectable = bombModule.GetComponent<KMSelectable>();
				GameObject passLight = TransformUtilities.FindChildIn(bombModule.transform, "Component_LED_PASS").gameObject;
				Transform statusLight = passLight.transform.parent;
				InterruptableModule interruptableModule = new InterruptableModule(
					bombModule,
					selectable,
					passLight,
					strikeLight: TransformUtilities.FindChildIn(statusLight, "Component_LED_STRIKE").gameObject,
					errorLight: Instantiate(errorLightPrefab, statusLight.position, statusLight.rotation, statusLight.transform),
					offLight: TransformUtilities.FindChildIn(statusLight, "Component_LED_OFF").gameObject
				);
				interruptableModule.ErrorLight.SetActive(false);
				interruptableModules.Add(interruptableModule);

				TechSupportLog.LogFormat("Loaded module {0}.", bombModule.ModuleDisplayName);
			}

			TechSupportLog.LogFormat("Loaded total of {0} interruptable modules", interruptableModules.Count);
		}

		#endregion


		#region Error Generation

		private InterruptableModule FindInterruptableModule()
		{
			// The module can no longer reset when too little time is left.
			float bombTime = bombInfo.GetTime();
			if (bombTime < needyModule.CountdownTime)
				return null;

			InterruptableModule selected = null;

			InterruptableModule[] potentials = new InterruptableModule[interruptableModules.Count];
			interruptableModules.CopyTo(potentials);
			potentials.Shuffle();

			foreach (InterruptableModule current in potentials)
			{
				if (current.CanBeInterrupted())
				{
					selected = current;
					break;
				}
			}

			return selected;
		}

		private void GenerateEmptyError()
		{
			TechSupportLog.Log("Could not find/select interruptable module. Creating exception without one.");

			currentError = data.GenerateError(null);
			allErrors.Add(currentError);

			string message = exceptionWithoutModuleMessages[Random.Range(0, exceptionWithoutModuleMessages.Length)];
			message = string.Format(message, currentError.GetError(), currentError.GetSourceFile(), currentError.LineIndex, currentError.ColumnIndex);
			console.Show(message);
		}

		private void GenerateModuleError(InterruptableModule selected)
		{
			TechSupportLog.LogFormat("Interrupting: {0}", selected.BombModule.ModuleDisplayName);

			// All other lights are disabled, and the error light is enabled.
			Transform parent = selected.PassLight.transform.parent;
			int childCount = parent.childCount;
			for (int i = 0; i < childCount; i++)
				parent.GetChild(i).gameObject.SetActive(false);

			selected.ErrorLight.SetActive(true);

			// Disabling all interaction with the module.
			interruptedInteractHandler = selected.Selectable.OnInteract;
			selected.Selectable.OnInteract = new KMSelectable.OnInteractHandler(delegate
			{
				bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.NeedyActivated, selected.BombModule.transform);
				return false;
			});

			// Generating error and Updating the console. 
			currentError = data.GenerateError(selected.BombModule.ModuleDisplayName);
			allErrors.Add(currentError);

			string message = string.Format(errorFormat, selected.BombModule.ModuleDisplayName, currentError.GetError(), currentError.GetSourceFile(), currentError.LineIndex, currentError.ColumnIndex);
			console.Show(message);
		}

		#endregion


		#region Module Resetting

		private void ReactivateInterruptedModule()
		{
			currentStage = null;

			if (interrupted == -1 ||
				currentError.ModuleName == null)
			{
				return;
			}

			InterruptableModule module = interruptableModules[interrupted];
			interrupted = -1;

			// Enables interrupted module.
			module.Selectable.OnInteract = interruptedInteractHandler;
			Transform parent = module.PassLight.transform.parent;
			options.Clear();

			module.ErrorLight.SetActive(false);

			if (!module.PassLight.activeSelf)
			{
				module.OffLight.SetActive(true);
				module.StrikeLight.SetActive(false);
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


		#region Answer Selection

		private void SelectAnswer()
		{
			int correctOption = currentStage.GetCorrectIndex(currentError, needyModule, allErrors);

			// current stage is error sage.
			if (correctOption == -1)
				SelectErrorAnswer();
			else
				SelectRegularAnswer(correctOption);
		}

		private void SelectRegularAnswer(int correctOption)
		{
			TechSupportLog.LogFormat("({0}): Selected option ({1}); correct option ({2}).",
				currentStage.GetDisplayName(),
				selectedOption,
				correctOption);

			// Visual effect.
			ConfirmSelection(); 

			if (selectedOption == correctOption)
			{
				// Correct option selected.
				console.Show(correctSelectionMessage);
				currentStage = currentStage.GetNextStage();

				// If there is no next stage, the current stage was the final stage.
				// Else, the next options can be shown.
				if (currentStage == null)
					CompleteModule();
				else
					ShowOptions(currentStage.GetData(), 
						string.Format(makeSelectionMessageFormat, currentStage.GetDisplayName()));
			}
			else
			{
				// Incorrect option selected.
				TechSupportLog.Log("STRIKE: " + currentStage.GetIncorrectSelectionMessage());
				needyModule.HandleStrike();
				console.Show(incorrectSelectionMessage);
				currentStage = new ErrorStage(currentStage);
				ShowOptions(currentStage.GetData(), showParametersAgainMessage);
			}
		}

		private void SelectErrorAnswer()
		{
			currentStage = currentStage.GetNextStage();

			if (selectedOption == 1)
			{
				// Prompt exception message again.
				string exceptionMessage = string.Format(
					errorFormat, 
					currentError.ModuleName, 
					currentError.GetError(), 
					currentError.GetSourceFile(), 
					currentError.LineIndex, 
					currentError.ColumnIndex);
				console.Show(exceptionMessage);
			}

			ShowOptions(currentStage.GetData(), 
				string.Format(makeSelectionMessageFormat, currentStage.GetDisplayName()));
		}

		private void CompleteModule()
		{
			if (currentError.ModuleName == null)
			{
				console.Show(exceptionWithoutModuleResolvedMessage);
			}
			else
			{
				InterruptableModule module = interruptableModules[interrupted];
				string message = string.Format(moduleReleasedFormat, module.BombModule.ModuleDisplayName);
				console.Show(message);
			}

			ReactivateInterruptedModule();
			moduleResolved = true;
		}

		#endregion


		#region Visual Updates

		private void ShowOptions(string[] data, string caption)
		{
			console.Show(caption);

			selectedOption = 0;
			options.Clear();
			foreach(string entry in data)
			{
				int pointer = console.Show(string.Format(unselectedOptionFormat, entry));
				options.Add(new Tuple<string, int>(entry, pointer));
			}

			UpdateSelected(0);
		}

		private void ConfirmSelection()
		{
			for (int i = 0; i < options.Count; i++)
				if (i != selectedOption)
					console.Remove(options[i].B);

			Tuple<string, int> selected = options[selectedOption];
			string message = string.Format(optionConfirmedFormat, selected.A);
			console.Replace(selected.B, message);
		}

		private void UpdateSelected(int previous)
		{
			if(options.Count == 0)
				return;

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


		#region Module Interaction

		private void OnOKClicked()
		{
			bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, okButton.transform);
			if (currentStage != null)
				SelectAnswer();
		}

		private void OnUpClicked()
		{
			bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, upButton.transform);
			int previous = selectedOption;
			selectedOption = options.Count == 0 ? 0 : (selectedOption + options.Count - 1) % options.Count;
			UpdateSelected(previous);
		}

		private void OnDownClicked()
		{
			bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, downButton.transform);
			int previous = selectedOption;
			selectedOption = options.Count == 0 ? 0 : (selectedOption + 1) % options.Count;
			UpdateSelected(previous);
		}

		#endregion
	}
}
