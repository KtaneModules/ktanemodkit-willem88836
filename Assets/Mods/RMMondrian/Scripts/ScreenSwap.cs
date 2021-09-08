using System.Collections.Generic;
using UnityEngine;

namespace RMMondrian
{
	public class ScreenSwap : MonoBehaviour
	{
		[SerializeField] private Transform[] shades;
		[SerializeField] private GameObject[] screens;
		[SerializeField] private KMAudio audioSource;

		[Space]
		[SerializeField] private int startScreen;
		[SerializeField] private float closeSpeed;
		[SerializeField] private float closeDuration;

		private bool isSwapping = false;
		private int currentScreen;

		public bool IsSwapping { get { return isSwapping; } }

		public void Start()
        {
			SwapToStart();
		}

		public void SwapToStart()
		{
			currentScreen = startScreen - 1;
			Swap();
		}

		public void Swap()
		{
			if (!isSwapping)
				StartCoroutine(SwapScreens());
		}

		private IEnumerator<YieldInstruction> SwapScreens()
		{
			audioSource.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
			isSwapping = true;

			// closes panels 
			float a = 0.0f;
			while (a < 1.0)
			{
				// interpolates scale of all shades 
				a = Mathf.Min(a + Time.deltaTime * closeSpeed, 1.0f);
				foreach (Transform shade in shades)
					shade.localScale = new Vector3(1, 1, a);
				yield return new WaitForEndOfFrame();
			}

			// swaps active screens
			screens[currentScreen].SetActive(false);
			currentScreen = (currentScreen + 1) % screens.Length;
			screens[currentScreen].SetActive(true);

			yield return new WaitForSeconds(closeDuration);

			// opens panels
			while (a > 0)
			{
				// interpolates scale of all shades 
				a = Mathf.Max(a - Time.deltaTime * closeSpeed, 0.0f);
				foreach (Transform shade in shades)
					shade.localScale = new Vector3(1, 1, a);
				yield return new WaitForEndOfFrame();
			}

			isSwapping = false;
		}
	}
}
