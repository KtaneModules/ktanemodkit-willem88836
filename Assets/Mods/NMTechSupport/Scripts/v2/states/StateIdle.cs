
using UnityEngine; 

namespace wmeijer.techsupport.v2.states
{
    public sealed class StateIdle : MonoBehaviour, IState
    {
        public void Initialize(TechSupportController controller, GlobalState globalState)
        {
            TechSupportLog.Log("Started idling...");
        }

        public void Terminate() { 
            // unused.
        }
    }
}
