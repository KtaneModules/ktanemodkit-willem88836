using UnityEngine; 

namespace wmeijer.techsupport.v2.states {
    public sealed class StateErrorInitialize : MonoBehaviour, IState
    {
        [SerializeField] private KMBombInfo bombInfo;
        [SerializeField] private KMNeedyModule needyModule;
        [SerializeField] private KMAudio bombAudio;
        [SerializeField] private TechSupportData techSupportData;
        [SerializeField] private Console console;
        
        [Space]
        [SerializeField] private float minimumTimeFactor;
        [SerializeField] private string errorFormat; 
        [SerializeField] private string[] errorFormatsNoModule;
        [SerializeField] private string startMessage;

        public void Initialize(TechSupportController controller, GlobalState globalState)
        {
            int moduleIndex = bombInfo.GetTime() >= minimumTimeFactor * needyModule.CountdownTime
                ? SelectModule(globalState.GetModules()) : -1;
            globalState.SetInterruptedModuleIndex(moduleIndex);
            ErrorData errorData = null;
            if (moduleIndex >= 0) {
                InterruptableModule module = globalState.GetInterruptedModule();
                errorData = techSupportData.GenerateError(module.BombModule.ModuleDisplayName);
                TechSupportLog.LogFormat("Throwing error with module \"{0}\".", module.BombModule.ModuleDisplayName);
                CreateErrorWithModule(module);
                errorData.Message = string.Format(
                    errorFormat, module.BombModule.ModuleDisplayName, 
                    errorData.Error, errorData.SourceFile, 
                    errorData.LineIndex, errorData.ColumnIndex);
            }
            else {
                TechSupportLog.Log("Throwing error without module.");
                errorData = techSupportData.GenerateError(null);
                errorData.Message = string.Format(
                    errorFormatsNoModule.PickRandom(), errorData.Error, 
                    errorData.SourceFile, errorData.LineIndex, 
                    errorData.ColumnIndex
                );
            }
            globalState.SetErrorData(errorData);
            console.WriteMessage(string.Format("{0} {1}", errorData.Message, startMessage));
            TechSupportLog.Log(errorData.Message);
            controller.SetState(typeof(StateVersionSelection));
        }

        private int SelectModule(InterruptableModule[] modules) {
            foreach(InterruptableModule module in modules.Shuffle()) {
                if (!module.PassLight.activeSelf 
                    && !module.StrikeLight.activeSelf 
                    && !module.ErrorLight.activeSelf 
                    && !module.IsFocussed)
                {
                    return System.Array.IndexOf(modules, module);
                }
            }
            return -1;
        }

        private void CreateErrorWithModule(InterruptableModule module) {
            module.OffLight.SetActive(false);
            module.ErrorLight.SetActive(true);
            module.Selectable.OnInteract += new KMSelectable.OnInteractHandler(delegate {
                bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.NeedyActivated, module.BombModule.transform);
                return false;
            });
        }

        public void Terminate()
        {
            // unused.
        }
    }
}
