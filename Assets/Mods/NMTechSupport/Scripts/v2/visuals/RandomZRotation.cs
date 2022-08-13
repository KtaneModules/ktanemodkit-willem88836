using UnityEngine;

public class RandomZRotation : MonoBehaviour
{
    private void Start()
    {
        this.transform.Rotate(new Vector3(0, 0, Random.Range(0, 360f)));
    }
}
