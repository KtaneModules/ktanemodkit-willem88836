using UnityEngine;

/// <summary> 
/// Tech Support state that is active when 
/// the needy module is inactive.
/// </summary>
public sealed class StateIdle : MonoBehaviour, IState
{
    public void Initialize(TechSupportController controller, GlobalState globalState)
    {
        // unused
    }

    public void Terminate()
    {
        // unused
    }
}
