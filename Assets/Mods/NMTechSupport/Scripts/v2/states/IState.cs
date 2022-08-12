
namespace wmeijer.techsupport.v2.states {
    public interface IState {
        void Initialize(TechSupportController controller, GlobalState globalState);
        void Terminate();
    }
}
