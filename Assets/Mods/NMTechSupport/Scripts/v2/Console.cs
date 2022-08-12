using UnityEngine;
using System.Collections.Generic;

using wmeijer.techsupport.v2.buttons;

namespace wmeijer.techsupport.v2 {
    public class Console: MonoBehaviour, IButtonSubscriber {
        [SerializeField] private TextMeshBox textBox;
        [SerializeField] private TextMesh text;
        [SerializeField] private int maxMessageCount; 
        [SerializeField] private ButtonPublisher buttonPublisher;

        [Space]
        [SerializeField] private string messageFormat; 
        [SerializeField] private string selectedOptionFormat;
        [SerializeField] private string unselectedOptionFormat;

        private LinkedList<string> messageMemento = new LinkedList<string>();

        private string[] options;
        private int currentOption = 0;

        public void Start() {
            buttonPublisher.Subscribe(this);
            Clear();
        }

        public int GetCurrentOption() {
            return this.currentOption;
        }

        public void Write(string message) {
            message = string.Format(messageFormat, message);
            messageMemento.AddLast(message);
            if (messageMemento.Count >= this.maxMessageCount)
                messageMemento.RemoveFirst();
            RefreshMessages();
        }
        
        public void WriteOptions(string message, string[] options) {
            Write(message);
            WriteOptions(options);
        }

        public void WriteOptions(string[] options) {
            this.options = options;
            currentOption = 0;
            RefreshOptions();
        }

        private void RefreshMessages() {
            textBox.SetText(messageMemento.Join("\n"));
        }

        private void RefreshOptions() {
            string text = this.text.text;
            for (int i = 0; i < options.Length; i ++) {
                string opt = options[i];
                opt = i == currentOption 
                    ? string.Format(selectedOptionFormat, options[i])
                    : string.Format(unselectedOptionFormat, options[i]);
                text= string.Format("{0}\n{1}", text, opt);
            }
            textBox.SetText(text);
        }

        public void Clear() {
            textBox.SetText("");
            messageMemento.Clear();
            currentOption = 0;
        }

        public void OnOkButtonClicked()
        {   
            options = new string[0];
            RefreshMessages();
        }

        public void OnUpButtonClicked()
        {
            currentOption = options.Length > 0 
                ? currentOption = (currentOption + options.Length - 1) % options.Length
                : 0;
            RefreshMessages();
            RefreshOptions();
        }

        public void OnDownButtonClicked()
        {
            currentOption = options.Length > 0
                ? (currentOption + 1) % options.Length
                : 0;
            RefreshMessages();
            RefreshOptions();
        }
    }
}
