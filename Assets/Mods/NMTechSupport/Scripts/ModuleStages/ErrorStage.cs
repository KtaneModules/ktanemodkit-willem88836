using System.Collections.Generic;

namespace NMTechSupport
{
	public class ErrorStage : IModuleStage
	{
		private IModuleStage faultedStage;


		public ErrorStage(IModuleStage faultedStage)
		{
			this.faultedStage = faultedStage;
		}

		public int GetCorrectIndex(ErrorData currentError, KMNeedyModule needyModule, List<ErrorData> allErrors)
		{
			return -1;
		}

		public string[] GetData()
		{
			return new string[] { "no", "yes" };
		}

		public string GetDisplayName()
		{
			return "error";
		}

		public string GetIncorrectSelectionMessage()
		{
			return null;
		}

		public IModuleStage GetNextStage()
		{
			return this.faultedStage;
		}
	}
}
