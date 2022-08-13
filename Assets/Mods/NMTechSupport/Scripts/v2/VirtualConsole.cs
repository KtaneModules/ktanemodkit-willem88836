using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TextMeshBox interface, implementing all functionalities 
/// for the Tech Support module's interface.
/// </summary>
public sealed class VirtualConsole : MonoBehaviour, IButtonSubscriber
{
    [SerializeField] private TextMeshBox textBox;
    [SerializeField] private TextMesh text;
    [SerializeField] private int maxMessageCount;
    [SerializeField] private ButtonPublisher buttonPublisher;
    [SerializeField] private GameObject screenDimmedOverlay;

    [Space]
    [SerializeField] private string messageFormat;
    [SerializeField] private string selectedOptionFormat;
    [SerializeField] private string unselectedOptionFormat;

    private LinkedList<string> messageMemento = new LinkedList<string>();

    private string[] options;
    private Dictionary<int, int> shuffledOptionMap = new Dictionary<int, int>();
    private int currentOption = 0;

    public void Start()
    {
        buttonPublisher.Subscribe(this);
        Clear();
    }

    /// <summary>
    /// Disables/enables the dark overlay of the console. 
    /// </summary>
    public void ToggleDim(bool isEnabled)
    {
        this.screenDimmedOverlay.SetActive(isEnabled);
    }

    /// <summary> 
    /// Returns the option that is currently selected
    /// or -1 if none is selected. 
    /// </summary>
    public int GetCurrentOption()
    {
        return this.options.Length > 0 
            ? this.shuffledOptionMap[this.currentOption]
            : -1;
    }

    /// <summary>
    /// Writes a message and an array of options. 
    /// </summary>
    /// <param name="message">The to-be printed message</param>
    /// <param name="options">The to-be printed options</param> 
    /// <param name="isShuffled">Whether the options are shown shuffled or not</param>
    public void Write(string message, string[] options, bool isShuffled)
    {
        WriteMessage(message);
        WriteOptions(options, isShuffled);
    }

    /// <summary>Writes a message</summary>
    /// <param name="message">The to-be printed message</param>
    public void WriteMessage(string message)
    {
        message = string.Format(messageFormat, message);
        messageMemento.AddLast(message);
        if (messageMemento.Count >= this.maxMessageCount)
            messageMemento.RemoveFirst();
        RefreshMessages();
    }

    /// <summary>Writes a list of options</summary>
    /// <param name="options">The to-be printed options</param> 
    /// <param name="isShuffled">Whether the options are shown shuffled</param>
    public void WriteOptions(string[] options, bool isShuffled)
    {
        this.options = options;
        currentOption = 0;
        shuffledOptionMap.Clear();
        if (isShuffled)
        {
            int[] shuffledIndices = new int[options.Length];
            for (int i = 0; i < options.Length; i++)
                shuffledIndices[i] = i;
            shuffledIndices = shuffledIndices.Shuffle();
            for (int i = 0; i < options.Length; i++)
                shuffledOptionMap.Add(i, shuffledIndices[i]);
        }
        else
        {
            for (int i = 0; i < options.Length; i++)
                shuffledOptionMap.Add(i, i);
        }
        RefreshOptions();
    }

    /// <summary>Refereshes both the messages and the options</summary>
    public void Refresh()
    {
        RefreshMessages();
        RefreshOptions();
    }

    /// <summary>Refreshes the messages, clearing the options</summary>
    public void RefreshMessages()
    {
        textBox.SetText(messageMemento.Join("\n"));
    }

    /// <summary>Refreshes the options, keeping the messages</summary>
    public void RefreshOptions()
    {
        string text = this.text.text;
        for (int i = 0; i < options.Length; i++)
        {
            string opt = options[this.shuffledOptionMap[i]];
            opt = i == currentOption
                ? string.Format(selectedOptionFormat, opt)
                : string.Format(unselectedOptionFormat, opt);
            text = string.Format("{0}\n{1}", text, opt);
        }
        textBox.SetText(text);
    }

    /// <summary>Completely clears the console, deleting all past messages</summary>
    public void Clear()
    {
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
