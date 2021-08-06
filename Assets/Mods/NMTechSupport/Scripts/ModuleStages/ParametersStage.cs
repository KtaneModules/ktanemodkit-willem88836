using System.Collections.Generic;
using UnityEngine;

namespace NMTechSupport
{
	class ParametersStage : IModuleStage
	{
		public string[] GetData()
		{
			return TechSupportData.Parameters;
		}

		public string GetDisplayName()
		{
			return "launch parameters";
		}

		public IModuleStage GetNextStage()
		{
			return null;
		}

		public string GetIncorrectSelectionMessage()
		{
			return "Wrong parameters";
		}

		public int GetCorrectIndex(ErrorData currentError, KMNeedyModule needyModule, List<ErrorData> allErrors)
		{
			string error = currentError.GetError();

			// sum of the first three icons.
			int a = 0;
			for (int i = 2; i < 5; i++)
			{
				char l = error[i];
				int k = "1234567890".IndexOf(l) >= 0
					? int.Parse(l.ToString())
					: StringManipulation.AlphabetToIntPlusOne(l);
				a += k;
			}

			// sum of the last three icons.
			int b = 0;
			for (int i = 5; i < 8; i++)
			{
				char l = error[i];
				int k = "1234567890".IndexOf(l) >= 0
					? int.Parse(l.ToString())
					: StringManipulation.AlphabetToIntPlusOne(l);
				b += k;
			}


			// multiplied by line/column, calculated delta.
			int x = Mathf.Abs((a * currentError.LineIndex) - (b * currentError.ColumnIndex));
			// xor operation.
			x ^= a * b;
			// subtracting parameter count until it is below that. 
			x %= TechSupportData.Parameters.Length;

			return x;
		}
	}
}
