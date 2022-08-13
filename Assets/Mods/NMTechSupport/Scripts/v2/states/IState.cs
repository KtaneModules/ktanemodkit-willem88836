
/// <summary>Base class for Tech Support states.</summary>
public interface IState
{
    /// <summary>Called when the state is activated</summary>
    void Initialize(TechSupportController controller, GlobalState globalState);
    /// <summary>Called when the state is deactivated</summary>
    void Terminate();
}
