using UnityEngine;
using KModkit;
using System.Collections.Generic;
using System.Linq;
using IEnumerator = System.Collections.IEnumerator;

/// <summary>
/// Tech Support state in which the module is set up.
/// i.e. it loads all interruptable modules and 
/// prepares them for interruption.
/// </summary>
public sealed class StateSetup : MonoBehaviour, IState
{
    [SerializeField] private TechSupportService mysteryKeyService;
    [SerializeField] private KMBossModule bossModule;
    [SerializeField] private KMNeedyModule needyModule;
    [SerializeField] private KMBombInfo bombInfo;
    [SerializeField] private VirtualConsole console;

    [Space]
    [SerializeField] private GameObject errorLightPrefab;
    [SerializeField] private TextAsset backUpIgnoreList;

    [Space]
    [SerializeField] private string bootMessage;

    private Coroutine delayedStartRoutine = null;

    public void Initialize(TechSupportController controller, GlobalState globalState)
    {
        if (!this.mysteryKeyService.SettingsLoaded || this.delayedStartRoutine == null)
        {
            this.delayedStartRoutine = StartCoroutine(InitializeLater(controller, globalState));
            return;
        }
        InterruptableModule[] interruptableModules = BuildInterruptableModules();
        GlobalState newGlobalState = new GlobalState();
        newGlobalState.SetModules(interruptableModules);
        controller.SetGlobalState(newGlobalState);
        controller.SetState(typeof(StateIdle));
        this.console.WriteMessage(string.Format(this.bootMessage,
                            this.bombInfo.GetSerialNumber(),
                            interruptableModules.Length));
        this.console.ToggleDim(false);
    }

    public void Terminate()
    {
        StopCoroutine(this.delayedStartRoutine);
    }

    // Delays the initialize function by 0.25 seconds.
    private IEnumerator InitializeLater(TechSupportController controller, GlobalState globalState)
    {
        TechSupportLog.Log("Mystery service not started yet, delaying initialization...");
        yield return new WaitForSeconds(0.25f);
        Initialize(controller, globalState);
    }

    // Builds all interruptable modules, that:
    // 1) are not ignored by the mystery module
    // 2) are not ignored by the boss module
    // 3) don't throw an error when building the module.
    private InterruptableModule[] BuildInterruptableModules()
    {
        List<InterruptableModule> interruptableModules = new List<InterruptableModule>();
        string[] ignoredModules = GetIgnoredBossModules();
        KMBombModule[] bombModules = FindObjectsOfType<KMBombModule>();
        TechSupportLog.LogFormat("Found {0} modules in scene", bombModules.Length);
        foreach (KMBombModule bombModule in bombModules)
        {
            if (IsIgnoredModule(bombModule, ignoredModules))
                continue;
            InterruptableModule interruptable = null;
            try
            {
                interruptable = BuildInterruptableModule(bombModule);
            }
            finally
            {
                if (interruptable != null)
                    interruptableModules.Add(interruptable);
            }
        }
        TechSupportLog.LogFormat("Loaded {0} interruptable modules: {1}",
            interruptableModules.Count(),
            string.Join(", ", interruptableModules.Select(m => m.ToString()).ToArray()));
        return interruptableModules.ToArray();
    }

    private bool IsIgnoredModule(KMBombModule bombModule, string[] ignoredModules) {
        return mysteryKeyService.MustNotBeHidden(bombModule.ModuleType)
            || ignoredModules.Contains(bombModule.ModuleDisplayName)
            || !IsOnSameBomb(bombModule);
    }
    
    // Uses the bomb serial of the module to determine 
    // whether it's on the same bomb. Returns false if 
    // no bombinfo was found.
    private bool IsOnSameBomb(KMBombModule bombModule) {
        Transform current = bombModule.transform;
        KMBombInfo otherBombInfo = null;
        while (otherBombInfo == null && current != null) {
            otherBombInfo = current.GetComponent<KMBombInfo>();
            current = current.transform.parent;
        }
        // By default, modules without bomb info are ignored.
        if (otherBombInfo == null)
            return false;
        return otherBombInfo.GetSerialNumber()
            .Equals(this.bombInfo.GetSerialNumber());
    }

    // Loads all modules that are ignored through the boss module.
    private string[] GetIgnoredBossModules()
    {
        string[] ignoredModules = bossModule.GetIgnoredModules(needyModule.ModuleDisplayName);
        if (ignoredModules == null || ignoredModules.Length == 0)
        {
            TechSupportLog.Log("Using back-up boss ignore list.");
            ignoredModules = backUpIgnoreList.text.Split('\n');
        }
        return ignoredModules;
    }

    // Factory method to build an interruptable module object with. 
    // Instantiates the relevant objects as well. 
    private InterruptableModule BuildInterruptableModule(KMBombModule bombModule)
    {
        GameObject passLight = TransformUtilities.FindChildIn(bombModule.transform, "Component_LED_PASS").gameObject;
        Transform statusLight = passLight.transform.parent;
        GameObject errorLight = Instantiate(
            errorLightPrefab,
            statusLight.position,
            statusLight.rotation,
            statusLight.transform
        );
        errorLight.SetActive(false);
        return new InterruptableModule(
            bombModule,
            bombModule.GetComponent<KMSelectable>(),
            passLight,
            TransformUtilities.FindChildIn(statusLight, "Component_LED_STRIKE").gameObject,
            errorLight,
            TransformUtilities.FindChildIn(statusLight, "Component_LED_OFF").gameObject);
    }
}
