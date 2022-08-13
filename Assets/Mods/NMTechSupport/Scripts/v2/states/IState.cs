
public interface IState {
    void Initialize(TechSupportController controller, GlobalState globalState);
    void Terminate();
}
