using UnityEngine;
using UnityEngine.Events;

public class ClickableButton : MonoBehaviour
{
    [SerializeField] private KMSelectable selectable;
    [SerializeField] private KMAudio bombAudio;
    [SerializeField] private UnityEvent onClick;

    private void Start()
    {
        selectable.OnInteract += OnInteract;
    }

    private bool OnInteract()
    {
        bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        onClick.Invoke();
        return false;
    }
}
