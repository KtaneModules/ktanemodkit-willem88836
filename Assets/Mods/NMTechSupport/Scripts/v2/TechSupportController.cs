using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class TechSupportController : MonoBehaviour, INeedyModule
{
    [SerializeField] private KMBombInfo bombInfo;
    [SerializeField] private KMNeedyModule needyModule;
    [SerializeField] private KMAudio bombAudio;

    [Space]
    [SerializeField] private MonoBehaviour[] availableStates;
    private Dictionary<Type, IState> states;

    private IState state = null;
    private GlobalState globalState = null;

    public void SetGlobalState(GlobalState newGlobalState)
    {
        if (this.globalState != null)
            throw new InvalidOperationException("Global state can only be set once.");
        this.globalState = newGlobalState;
    }

    public GlobalState GetGlobalState()
    {
        return this.globalState;
    }

    private void Start()
    {
        NeedyUtils.Subscribe(this, this.needyModule);
        InitializeStates();
        TechSupportLog.Log("Started module!");
        needyModule.OnActivate += () => SetState(typeof(StateSetup));
    }

    private void InitializeStates()
    {
        states = new Dictionary<Type, IState>(availableStates.Length);
        foreach (IState state in availableStates)
            states[state.GetType()] = state;
        availableStates = null;
    }

    public void SetState(Type newState)
    {
        TechSupportLog.LogFormat(
            "Changing module state from \"{0}\" to \"{1}\"",
            this.state == null ? "no state" : this.state.GetType().Name,
            newState.Name);
        if (this.state != null)
            this.state.Terminate();
        this.state = this.states[newState];
        this.state.Initialize(this, this.globalState);
    }

    // TODO: add check for modules that complete when interrupted, disabling the error.

    public void OnNeedyActivation()
    {
        SetState(typeof(StateErrorInitialize));
    }

    public void OnNeedyDeactivation()
    {
        TechSupportLog.Log("Module deactivated");
        SetState(typeof(StateCompleteModule));
    }

    public void OnTimerExpired()
    {
        TechSupportLog.Log("Module out of time");
        SetState(typeof(StateOutOfTime));
    }
}
