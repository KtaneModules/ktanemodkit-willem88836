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
        private Dictionary<int, int> shuffledOptionMap = new Dictionary<int, int>();
        private int currentOption = 0;

        public void Start() {
            buttonPublisher.Subscribe(this);
            Clear();
        }

        public int GetCurrentOption() {
            return this.shuffledOptionMap[this.currentOption];
        }
        
        public void Write(string message, string[] options, bool isShuffled) {
            WriteMessage(message);
            WriteOptions(options, isShuffled);
        }

        public void WriteMessage(string message) {
            message = string.Format(messageFormat, message);
            messageMemento.AddLast(message);
            if (messageMemento.Count >= this.maxMessageCount)
                messageMemento.RemoveFirst();
            RefreshMessages();
        }

        public void WriteOptions(string[] options, bool isShuffled) {
            this.options = options;
            currentOption = 0;
            shuffledOptionMap.Clear();
            if (isShuffled) {
                int[] shuffledIndices = new int[options.Length]; 
                for (int i = 0; i < options.Length; i++) 
                    shuffledIndices[i] = i;
                shuffledIndices = shuffledIndices.Shuffle();
                for (int i = 0; i < options.Length; i++)
                    shuffledOptionMap.Add(i, shuffledIndices[i]);
            }
            else {
                for (int i = 0; i < options.Length; i++) 
                    shuffledOptionMap.Add(i, i);
            }
            RefreshOptions();
        }

        public void Refresh() {
            RefreshMessages();
            RefreshOptions();
        }

        public void RefreshMessages() {
            textBox.SetText(messageMemento.Join("\n"));
        }

        public void RefreshOptions() {
            string text = this.text.text;
            for (int i = 0; i < options.Length; i++) {
                string opt = options[this.shuffledOptionMap[i]];
                opt = i == currentOption
                    ? string.Format(selectedOptionFormat, opt)
                    : string.Format(unselectedOptionFormat, opt);
                text= string.Format("{0}\n{1}", text, opt);
            }
            textBox.SetText(text);
        }

        public void Clear() {
            textBox.SetText("");
            messageMemento.Clear();
            shuffledOptionMap.Clear();
            currentOption = 0;
        }

        public void OnOkButtonClicked()
        {   
            RefreshMessages();
        }

        public void OnUpButtonClicked()
        {
            currentOption = options.Length > 0 
                ? currentOption = (currentOption + options.Length - 1) % options.Length
                : 0;
            Refresh();
        }

        public void OnDownButtonClicked()
        {
            currentOption = options.Length > 0
                ? (currentOption + 1) % options.Length
                : 0;
            Refresh();
        }
    }
}
