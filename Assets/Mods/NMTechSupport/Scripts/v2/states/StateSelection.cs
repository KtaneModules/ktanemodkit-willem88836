using UnityEngine;
using wmeijer.techsupport.v2.buttons;

namespace wmeijer.techsupport.v2.states {
    public abstract class StateSelection : MonoBehaviour, IState, IButtonSubscriber
    {
        [SerializeField] protected TechSupportData techSupportData;
        [SerializeField] protected Console console;
        [SerializeField] protected ButtonPublisher buttonPublisher;
        [SerializeField] protected KMNeedyModule needyModule;

        [Space]
        [SerializeField] protected string message;
        [SerializeField] protected string correctAnswerMessage;
        [SerializeField] protected string incorrectAnswerMessage;

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
        }

        public virtual void OnUpButtonClicked()
        {
        }

        public virtual void OnDownButtonClicked()
        {
        }
    }
}
