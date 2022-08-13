using UnityEngine; 

public sealed class StateIdle : MonoBehaviour, IState
{
    public void Initialize(TechSupportController controller, GlobalState globalState)
    {
        // unused
    }

    public void Terminate() { 
        // unused
    }
}
