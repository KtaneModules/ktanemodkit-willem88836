
namespace wmeijer.techsupport.v2.states {
    public sealed class StatePatchFileSelection : StateSelection {
        public override void Initialize(TechSupportController controller, GlobalState globalState)
        {
            base.Initialize(controller, globalState);
            console.WriteOptions(message, TechSupportData.PatchFiles);
        }
        
        public override void OnOkButtonClicked()
        {
            string selectedPatchFile = TechSupportData.PatchFiles[console.GetCurrentOption()];
            TechSupportLog.LogFormat("Selected patch file: \"{0}\"", selectedPatchFile);
            console.Write(selectedPatchFile);
            int correctPatchFileIndex = CorrectPatchFile(globalState.GetErrorData(), globalState.GetErrorMemento());
            string correctPatchFile = TechSupportData.PatchFiles[correctPatchFileIndex];
            TechSupportLog.LogFormat("Correct patch file: \"{0}\"", correctPatchFile);
            if (selectedPatchFile.Equals(correctPatchFile)) {
                console.Write(correctAnswerMessage);
                controller.SetState(typeof(StateParameterSelection));
            } 
            else {
                needyModule.HandleStrike();
                TechSupportLog.Log("STRIKE: Incorrect patch file");
                console.Write(incorrectAnswerMessage);
                console.WriteOptions(message, TechSupportData.PatchFiles);
            }
        }

        private int CorrectPatchFile(ErrorData errorData, ErrorData[] allErrors) {
            // Data where seed = 0;
            // 0 "prle.cba",
            // 1 "resble.bbc",
            // 2 "razcle.pxi",
            // 3 "wane.drf",
            // 4 "faee.sup",
            // 5 "exed.asc",
            // 6 "gilick.pxd",
            // 7 "linion.dart"

            //However, if the error's source file is either satcle.bb, plor.pom, or equely.ctl, ignore all rules above and select exed.asc.
            if (errorData.SourceFileIndex == 2
                || errorData.SourceFileIndex == 9)
            {
                return 5;
            }

            //Otherwise, if the source file's number of vowels is equal to or greater than the number of consonants, or the column index is higher than the line index, select faee.sup.
            int v = 0;
            int c = 0;
            foreach(char l in errorData.SourceFile)
            {
                if (l == '.')
                {
                    break;
                }

                bool isVowel = "aeiou".IndexOf(l) >= 0;

                if (isVowel)
                {
                    v++;
                }
                else
                {
                    c++;
                }
            }

            if (v >= c || errorData.ColumnIndex > errorData.LineIndex) 
            {
                return 4;
            }

            //Otherwise, if the source file's first letter is in the last third of the alphabet, select prle.cba.
            if(StringManipulation.AlphabetToIntPlusOne(errorData.SourceFile[0]) 
                >= 26f / 3f * 2f)
            {
                return 0;
            }

            //Otherwise, if the less than 99 seconds is still available and the column is higher than 75, select linion.dart.
            if (needyModule.GetNeedyTimeRemaining() < 99f 
                && errorData.ColumnIndex > 75)
            {
                return 7;
            }

            //Otherwise, if the error's line and column are both even, select razcle.pxi.
            if (errorData.LineIndex % 2 == 0
                && errorData.ColumnIndex % 2 == 0)
            {
                return 2;
            }

            //If any of the error code's letters are contained in the crashed source file's name, select wane.drf.
            for (int i = 2; i < errorData.Error.Length; i++)
            {
                string l1 = errorData.Error[i].ToString().ToLower();
                foreach (char l in errorData.SourceFile)
                {
                    string l2 = l.ToString().ToLower();
                    if (l1 == l2)
                    {
                        return 3;
                    }
                }
            }

            //Otherwise, if this is the fourth or later crash and the cumulative line number of all previous errors is over 450, select gilick.pxd.
            if (allErrors.Length >= 4)
            {
                int cumulativeLines = 0;
                foreach (ErrorData error in allErrors)
                {
                    cumulativeLines += error.LineIndex;
                }

                if (cumulativeLines >= 450)
                {
                    return 6;
                }
            }

            //Otherwise, select shuttle lonist.ftl.
            return 4;
        }
    }
}

