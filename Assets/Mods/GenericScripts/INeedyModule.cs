
namespace wmeijer {
    public interface INeedyModule {
        void OnNeedyActivation();
        void OnNeedyDeactivation();
        void OnTimerExpired();
    }

    public static class NeedyUtils {
        public static void Subscribe(INeedyModule module, KMNeedyModule needyController) {
            needyController.OnNeedyActivation += module.OnNeedyActivation;
            needyController.OnNeedyDeactivation += module.OnNeedyDeactivation; 
            needyController.OnTimerExpired += module.OnTimerExpired;
        }
    }
}
