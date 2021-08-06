using System;

namespace NMTechSupport
{
	/// <summary>
	///		Data container for one single error.
	/// </summary>
	[Serializable]
	public class ErrorData
	{
		public int ErrorIndex { get; private set; }
		public int SourceFileIndex { get; private set; }
		public int LineIndex { get; private set; }
		public int ColumnIndex { get; private set; }
		public string ModuleName { get; private set; }

		public ErrorData(
			int errorIndex,
			int sourceFileIndex,
			int lineIndex,
			int columnIndex,
			string moduleName)
		{
			this.ErrorIndex = errorIndex;
			this.SourceFileIndex = sourceFileIndex;
			this.LineIndex = lineIndex;
			this.ColumnIndex = columnIndex;
			this.ModuleName = moduleName;

			TechSupportLog.LogFormat("Created New Error for module {4} - Error: {0}, Source: {1}, Line: {2}, Column: {3}",
				GetError(),
				GetSourceFile(),
				lineIndex,
				columnIndex,
				moduleName ?? "[no module]");
		}

		public string GetError()
		{
			return TechSupportData.ErrorCodes[ErrorIndex];
		}

		public string GetSourceFile()
		{
			return TechSupportData.SourceFileNames[SourceFileIndex];
		}
	}
}
