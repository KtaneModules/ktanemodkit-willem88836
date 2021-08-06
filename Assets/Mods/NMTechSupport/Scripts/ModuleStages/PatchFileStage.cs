using System.Collections.Generic;

namespace NMTechSupport
{
	public class PatchFileStage : IModuleStage
	{
		public string[] GetData()
		{
			return TechSupportData.PatchFiles;
		}

		public string GetDisplayName()
		{
			return "patch file";
		}

		public IModuleStage GetNextStage()
		{
			return new ParametersStage();
		}

		public string GetIncorrectSelectionMessage()
		{
			return "Wrong patch file";
		}

		public int GetCorrectIndex(
			ErrorData currentError,
			KMNeedyModule needyModule,
			List<ErrorData> allErrors)
		{
			string errorString = currentError.GetError();
			string sourceString = currentError.GetSourceFile();

			//However, if the error's source file is either satcle.bb, plor.pom, or equely.ctl, ignore all rules above and select exed.asc.
			if (currentError.SourceFileIndex == 2 || currentError.SourceFileIndex == 9)
				return 5;

			//Otherwise, if the source file's number of vowels is equal to or greater than the number of consonants, or the column index is higher than the line index, select faee.sup.
			int v = 0;
			int c = 0;
			foreach (char l in sourceString)
			{
				if (l == '.')
					break;

				bool isVowel = "aeiou".IndexOf(l) >= 0;

				if (isVowel)
					v++;
				else
					c++;
			}

			if (v >= c)
				return 4;

			//Otherwise, if the source file's first letter is in the last third of the alphabet, select prle.cba.
			if (StringManipulation.AlphabetToIntPlusOne(sourceString[0]) >= 26f / 3f * 2f)
				return 0;

			//Otherwise, if the less than 99 seconds is still available and the column is higher than 75, select linion.dart.
			if (needyModule.GetNeedyTimeRemaining() < 99f && currentError.ColumnIndex > 75)
				return 7;

			//Otherwise, if the error's line and column are both even, select razcle.pxi.
			if (currentError.LineIndex % 2 == 0 && currentError.ColumnIndex % 2 == 0)
				return 2;

			//If any of the error code's letters are contained in the crashed source file's name, select wane.drf.
			for (int i = 2; i < errorString.Length; i++)
			{
				string l1 = errorString[i].ToString().ToLower();
				foreach (char l in sourceString)
				{
					string l2 = l.ToString().ToLower();
					if (l1 == l2)
						return 3;
				}
			}

			//Otherwise, if this is the fourth or later crash and the cumulative line number of all previous errors is over 450, select gilick.pxd.
			if (allErrors.Count >= 4)
			{
				int cumulativeLines = 0;
				foreach (ErrorData error in allErrors)
					cumulativeLines += error.LineIndex;

				if (cumulativeLines >= 450)
					return 6;
			}

			//Otherwise, select shuttle lonist.ftl.
			return 4;
		}
	}
}
