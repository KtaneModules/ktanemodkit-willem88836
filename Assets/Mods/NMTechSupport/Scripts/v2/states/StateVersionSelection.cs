using System;

public sealed class StateVersionSelection : StateSelection
{
    public override void Initialize(TechSupportController controller, GlobalState globalState)
    {
        base.Initialize(controller, globalState);
        console.Write(message, TechSupportData.VersionNumbers, false);
    }

    protected override int GetCorrectOption(GlobalState globalState)
    {
        ErrorData errorData = globalState.GetErrorData();
        return TechSupportData.OriginSerialCrossTable[errorData.ErrorIndex, errorData.SourceFileIndex];
    }

    protected override Type GetNextState()
    {
        return typeof(StatePatchFileSelection);
    }

    protected override string[] GetOptionStrings()
    {
        return TechSupportData.VersionNumbers;
    }
}
