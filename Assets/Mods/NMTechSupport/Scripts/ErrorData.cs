using System;
using System.Reflection;

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

		TechSupportLog.LogFormat(
			"Created New Error for module {4} - Error: {0}, Source: {1}, Line: {2}, Column: {3}",
			Error,
			SourceFile,
			lineIndex,
			columnIndex,
			moduleName ?? "[no module]");
	}
}
