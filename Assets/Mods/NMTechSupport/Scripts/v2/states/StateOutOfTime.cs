using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

public sealed class StateOutOfTime : MonoBehaviour, IState
{
    [SerializeField] private KMNeedyModule needyModule;
    [SerializeField] private VirtualConsole console; 

    [Space]
    [SerializeField] private string messageStart;
    [SerializeField] private string messageComplete;
    [SerializeField] private int rebootTime;

    private TechSupportController controller;
    private Coroutine rebootRoutine;

    public void Initialize(TechSupportController controller, GlobalState globalState)
    {
        this.controller = controller;
        TechSupportLog.Log("STRIKE: Out of time.");
        needyModule.HandleStrike();
        console.WriteMessage(messageStart);
        rebootRoutine = StartCoroutine(Reboot());
    }

    private IEnumerator Reboot() {
        console.WriteMessage(messageStart);
        yield return new WaitForSeconds(rebootTime);
        console.WriteMessage(messageComplete);
        controller.SetState(typeof(StateCompleteModule));
    }

    public void Terminate()
    {
        if (rebootRoutine != null) {
            StopCoroutine(rebootRoutine);
            rebootRoutine = null;
        }
    }
}
