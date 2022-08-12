using System;

namespace wmeijer.techsupport.v2 {
	[Serializable]
	public class ErrorData
	{
		public int ErrorIndex;
		public int SourceFileIndex;
		public int LineIndex;
		public int ColumnIndex;
		public string ModuleName;

		public string Error 
		{ 
			get 
			{ 
				return TechSupportData.ErrorCodes[ErrorIndex]; 
			} 
		}

		public string SourceFile
		{
			get
			{
				return TechSupportData.SourceFileNames[SourceFileIndex];
			}
		}

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
		}
	}
}
