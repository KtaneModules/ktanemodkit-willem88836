
using UnityEngine;
using System.Collections.Generic;

namespace wmeijer.techsupport.v2.buttons {
    public sealed class ButtonPublisher: MonoBehaviour {
        private List<IButtonSubscriber> subscribers = new List<IButtonSubscriber>();

        public void Subscribe(IButtonSubscriber newSubscriber) {
            if (!subscribers.Contains(newSubscriber)) 
                subscribers.Add(newSubscriber);
        }

        public void Unsubscribe(IButtonSubscriber oldSubscriber) {
            if(subscribers.Contains(oldSubscriber))
                subscribers.Remove(oldSubscriber);
        }

        public void OnOkClicked() {
            foreach(IButtonSubscriber sub in subscribers.ToArray())
                sub.OnOkButtonClicked();
        }

        public void OnUpButtonClicked() {
            foreach(IButtonSubscriber sub in subscribers.ToArray())
                sub.OnUpButtonClicked();
        }

        public void OnDownButtonClicked() {
            foreach(IButtonSubscriber sub in subscribers.ToArray())
                sub.OnDownButtonClicked();
        }
    }
}
