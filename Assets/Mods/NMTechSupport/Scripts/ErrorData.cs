using System;

[Serializable]
public class ErrorData
{
	public int ErrorIndex;
	public int SourceFileIndex;
	public int LineIndex;
	public int ColumnIndex;


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
		int columnIndex)
	{
		this.ErrorIndex = errorIndex;
		this.SourceFileIndex = sourceFileIndex;
		this.LineIndex = lineIndex;
		this.ColumnIndex = columnIndex;

		TechSupportLog.LogFormat(
			"Created New Error - Error: {0}, Source: {1}, Line: {2}, Column: {3}",
			Error,
			SourceFile,
			lineIndex,
			columnIndex);
	}
}
