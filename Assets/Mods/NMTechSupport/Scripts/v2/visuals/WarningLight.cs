
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

public class WarningLight: MonoBehaviour {

    [SerializeField] private int flickerInterval;

    private void Start() {
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker() {
        bool toggleOn = false;
        while (true) {
            yield return new WaitForSeconds(flickerInterval);
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(toggleOn);
            toggleOn = !toggleOn;
        }
    }
}
