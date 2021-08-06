using System.Collections.Generic;

namespace NMTechSupport
{
	public class VersionStage : IModuleStage
	{
		public string[] GetData()
		{
			return TechSupportData.VersionNumbers;
		}

		public string GetDisplayName()
		{
			return "software version";
		}

		public IModuleStage GetNextStage()
		{
			return new PatchFileStage();
		}

		public string GetIncorrectSelectionMessage()
		{
			return "Wrong software version";
		}

		public int GetCorrectIndex(
			ErrorData currentError,
			KMNeedyModule needyModule,
			List<ErrorData> allErrors)
		{
			return TechSupportData.OriginSerialCrossTable[currentError.ErrorIndex, currentError.SourceFileIndex];
		}
	}
}
