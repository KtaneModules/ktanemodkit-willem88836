using System.Collections.Generic;

namespace NMTechSupport
{
	public interface IModuleStage
	{
		string[] GetData();
		string GetDisplayName();
		IModuleStage GetNextStage();
		string GetIncorrectSelectionMessage();
		int GetCorrectIndex(
			ErrorData currentError,
			KMNeedyModule needyModule,
			List<ErrorData> allErrors);
	}
}
