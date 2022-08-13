using System;
using System.Collections.Generic;

/// <summary>
/// Data container holding all data that is 
/// shared across <c>IState</c> objects.
/// </summary>
public sealed class GlobalState
{
    private InterruptableModule[] modules;
    private int currentModuleIndex = -1;
    private ErrorData currentError = null;
    private List<ErrorData> errorMemento = new List<ErrorData>();

    public void SetModules(InterruptableModule[] modules)
    {
        if (this.modules != null)
            throw new InvalidOperationException("Modules cannot be set twice.");
        this.modules = modules;
    }

    public InterruptableModule[] GetModules()
    {
        return this.modules;
    }

    public void SetInterruptedModuleIndex(int newIndex)
    {
        this.currentModuleIndex = newIndex;
    }

    public InterruptableModule GetInterruptedModule()
    {
        if (this.currentModuleIndex < 0 
            || this.currentModuleIndex >= this.modules.Length)
            return null;
        return this.modules[this.currentModuleIndex];
    }

    public ErrorData GetErrorData()
    {
        return this.currentError;
    }

    public void SetErrorData(ErrorData newError)
    {
        this.currentError = newError;
        if (newError != null)
            this.errorMemento.Add(newError);
    }

    /// <summary> 
    /// Stores all errors that have occurred in one session.
    /// </summary>
    public ErrorData[] GetErrorMemento()
    {
        return this.errorMemento.ToArray();
    }
}
