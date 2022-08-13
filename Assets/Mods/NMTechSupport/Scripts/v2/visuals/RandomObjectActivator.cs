using UnityEngine;

/// <summary>
/// Activates a random object in the list 
/// and deactivates all others.
/// </summary>
public sealed class RandomObjectActivator : MonoBehaviour
{
    [SerializeField] private GameObject[] objects;

    private void Start()
    {
        int activeIndex = Random.Range(0, objects.Length);
        for (int i = 0; i < objects.Length; i++)
            objects[i].SetActive(i == activeIndex);
    }
}
