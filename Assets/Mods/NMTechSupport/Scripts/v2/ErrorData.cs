using System;

/// <summary>
/// Data container object for all data
/// related to a single Error. 
/// </summary>
[Serializable]
public sealed class ErrorData
{
    public int ErrorIndex { get; private set; }
    public int SourceFileIndex { get; private set; }
    public int LineIndex { get; private set; }
    public int ColumnIndex { get; private set; }
    public string ModuleName { get; private set; }
    private string message = null;
    public string Message
    {
        get
        {
            return this.message;
        }

        set
        {
            if (Message != null)
                throw new InvalidOperationException("Cannot set message twice");
            this.message = value;
        }
    }

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
