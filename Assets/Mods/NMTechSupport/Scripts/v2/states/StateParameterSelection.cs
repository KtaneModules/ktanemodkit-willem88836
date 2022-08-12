using UnityEngine;

namespace wmeijer.techsupport.v2.states {
    public sealed class StateParameterSelection : StateSelection {

        public override void Initialize(TechSupportController controller, GlobalState globalState)
        {
            base.Initialize(controller, globalState);
            console.WriteOptions(message, TechSupportData.Parameters);
        }

        public override void OnOkButtonClicked()
        {
            string selectedParameters = TechSupportData.Parameters[console.GetCurrentOption()];
            TechSupportLog.LogFormat("Selected parameters: \"{0}\"", selectedParameters);
            console.Write(selectedParameters);
            int correctPatchFileIndex = CorrectParameters(globalState.GetErrorData());
            string correctPatchFile = TechSupportData.Parameters[correctPatchFileIndex];
            TechSupportLog.LogFormat("Correct parameters: \"{0}\"", correctPatchFile);
            if (selectedParameters.Equals(correctPatchFile)) {
                console.Write(correctAnswerMessage);
                controller.SetState(typeof(StateCompleteModule));
            } 
            else {
                needyModule.HandleStrike();
                TechSupportLog.Log("STRIKE: Incorrect parameters");
                console.Write(incorrectAnswerMessage);
                console.WriteOptions(message, TechSupportData.Parameters);
            }
        }

        private int CorrectParameters(ErrorData errorData)
        {
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
    }
}
