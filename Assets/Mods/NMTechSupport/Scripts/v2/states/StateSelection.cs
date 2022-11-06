using System;
using UnityEngine;

/// <summary> 
/// Base class for all Tech Support states in 
/// which some selection is made. 
/// </summary>
public abstract class StateSelection : MonoBehaviour, IState, IButtonSubscriber
{
    [SerializeField] protected TechSupportData techSupportData;
    [SerializeField] protected VirtualConsole console;
    [SerializeField] protected ButtonPublisher buttonPublisher;
    [SerializeField] protected KMNeedyModule needyModule;

    [Space]
    [SerializeField] protected string message;
    [SerializeField] protected string correctAnswerMessage;
    [SerializeField] protected string incorrectAnswerMessage;
    
    public bool DebugMode = false;

    protected TechSupportController controller;
    protected GlobalState globalState;

    public virtual void Initialize(TechSupportController controller, GlobalState globalState)
    {
        this.controller = controller;
        this.globalState = globalState;
        buttonPublisher.Subscribe(this);
    }

    public virtual void Terminate()
    {
        buttonPublisher.Unsubscribe(this);
    }

    public virtual void OnOkButtonClicked()
    {
        string[] optionStrings = GetOptionStrings();
        int selectedIndex = console.GetCurrentOption();
        string selectedOption = optionStrings[selectedIndex];
        console.WriteMessage(selectedOption);
        int correctIndex = GetCorrectOption(this.globalState);
        string correctOption = optionStrings[correctIndex];
        TechSupportLog.LogFormat("Selected answer: \"{0}\", Correct answer: \"{1}\"", selectedOption, correctOption);
        if (selectedIndex == correctIndex || DebugMode)
        {
            console.WriteMessage(correctAnswerMessage);
            controller.SetState(GetNextState());
        }
        else
        {
            needyModule.HandleStrike();
            TechSupportLog.Log("STRIKE: Incorrect answer");
            console.WriteMessage(incorrectAnswerMessage);
            console.WriteMessage(globalState.GetErrorData().Message);
            console.WriteMessage(message);
            console.Refresh();
        }
    }

    public virtual void OnUpButtonClicked()
    {
        // unused.
    }

    public virtual void OnDownButtonClicked()
    {
        // unused.
    }

    protected abstract string[] GetOptionStrings();

    /// <summary>Calculates the correct answer</summary>
    protected abstract int GetCorrectOption(GlobalState globalState);

    protected abstract Type GetNextState();
}
