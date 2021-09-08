using UnityEngine;
using UnityEngine.Events;

namespace RMMondrian
{
	[RequireComponent(typeof(KMSelectable))]
	public class InteractableButton : MonoBehaviour
	{
		[SerializeField] private UnityEvent onClick;
		[SerializeField] private KMAudio audioSource;
		 
		private void Start()
		{
			KMSelectable selectable = GetComponent<KMSelectable>();
			selectable.OnInteract += OnClick;
		}

		private bool OnClick()
		{
			audioSource.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

			if (onClick != null)
				onClick.Invoke();

			return false;
		}
	}
}
