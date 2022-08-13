using System;
using UnityEngine;

public sealed class StateParameterSelection : StateSelection {

    public override void Initialize(TechSupportController controller, GlobalState globalState)
    {
        base.Initialize(controller, globalState);
        console.Write(message, TechSupportData.Parameters, true);
    }

    protected override int GetCorrectOption(GlobalState globalState)
    {
        ErrorData errorData = globalState.GetErrorData();

        // sum of the first three icons.
        int a = 0;
        for (int i = 2; i < 5; i++)
        {
            char l = errorData.Error[i];
            int k = "1234567890".IndexOf(l) >= 0
                ? int.Parse(l.ToString())
                : StringManipulation.AlphabetToIntPlusOne(l);
            a += k;
        }

        // sum of the last three icons.
        int b = 0;
        for (int i = 5; i < 8; i++)
        {
            char l = errorData.Error[i];
            int k = "1234567890".IndexOf(l) >= 0
                ? int.Parse(l.ToString())
                : StringManipulation.AlphabetToIntPlusOne(l);
            b += k;
        }


        // multiplied by line/column, calculated delta.
        int x = Mathf.Abs((a * errorData.LineIndex) - (b * errorData.ColumnIndex));
        // xor operation.
        x ^= a * b;
        // subtracting parameter count until it is below that. 
        x %= TechSupportData.Parameters.Length;

        return x;
    }

    protected override Type GetNextState()
    {
        return typeof(StateCompleteModule);
    }

    protected override string[] GetOptionStrings()
    {
        return TechSupportData.Parameters;
    }
}
