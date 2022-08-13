
using UnityEngine;

public class ShuffleColors : MonoBehaviour
{
    [SerializeField] Material[] colors;
    [SerializeField] MeshRenderer[] renderers;

    private void Start()
    {
        Material[] shuffledMaterials = colors.Shuffle();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = shuffledMaterials[i];
        }
    }
}
