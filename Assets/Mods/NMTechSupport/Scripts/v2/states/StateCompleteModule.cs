using UnityEngine;

/// <summary>
/// Tech Support state that undoes the module interruption. 
/// </summary>
public sealed class StateCompleteModule : MonoBehaviour, IState
{
    [SerializeField] private KMNeedyModule needyModule;
    [SerializeField] private VirtualConsole console;

    [SerializeField] private string message;

    public void Initialize(TechSupportController controller, GlobalState globalState)
    {
        InterruptableModule interrupted = globalState.GetInterruptedModule();
        globalState.SetInterruptedModuleIndex(-1);
        globalState.SetErrorData(null);
        if (interrupted != null) 
        {
            RemoveModListener(interrupted);
            ResetModuleLights(interrupted);
            interrupted.Selectable.OnInteract = null;
        }
        console.WriteMessage(message);
        needyModule.HandlePass();
        controller.SetState(typeof(StateIdle));
    }

    private void RemoveModListener(InterruptableModule interrupted)
    {
        foreach (var listener in interrupted.Selectable.OnInteract.GetInvocationList())
        {
            if (listener.Target.ToString().Contains("StateErrorInitialize"))
            {
                interrupted.Selectable.OnInteract -= (KMSelectable.OnInteractHandler)listener;
                TechSupportLog.Log("Removed OnInterract Listener.");
                return;
            }
            TechSupportLog.Log("Did not remove OnInterract Listener.");
        }
    }

    private void ResetModuleLights(InterruptableModule interrupted)
    {
        interrupted.ErrorLight.SetActive(false);
        if (!interrupted.PassLight.activeSelf)
        {
            interrupted.OffLight.SetActive(true);
            interrupted.StrikeLight.SetActive(false);
        }
    }

    public void Terminate()
    {
        // unused
    }
}
