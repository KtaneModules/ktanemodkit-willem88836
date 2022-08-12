
namespace wmeijer.techsupport.v2.states {
    public sealed class StateVersionSelection : StateSelection
    {
        public override void Initialize(TechSupportController controller, GlobalState globalState)
        {
            base.Initialize(controller, globalState);
            console.WriteOptions(message, TechSupportData.VersionNumbers);
        }
        
        public override void OnOkButtonClicked()
        {
            string selectedVersion = TechSupportData.VersionNumbers[console.GetCurrentOption()];
            TechSupportLog.LogFormat("Selected version: \"{0}\"", selectedVersion);
            console.Write(selectedVersion);
            ErrorData errorData = globalState.GetErrorData();
            int correctVersionIndex = TechSupportData.OriginSerialCrossTable[errorData.ErrorIndex, errorData.SourceFileIndex];
            string correctVersion = TechSupportData.VersionNumbers[correctVersionIndex];
            TechSupportLog.LogFormat("Correct version: \"{0}\"", correctVersion);
            if (selectedVersion.Equals(correctVersion)) {
                console.Write(correctAnswerMessage);
                controller.SetState(typeof(StatePatchFileSelection));
            }
            else {
                needyModule.HandleStrike();
                TechSupportLog.Log("STRIKE: Incorrect software version");
                console.Write(incorrectAnswerMessage);
                console.WriteOptions(message, TechSupportData.VersionNumbers);
            }
        }
    }
}
