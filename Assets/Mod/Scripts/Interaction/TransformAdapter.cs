using System.Collections;
using UnityEngine;

public class TransformAdapter : MonoBehaviour
{
	[SerializeField] private Vector3 position;
	[SerializeField] private Vector3 rotation;
	[SerializeField] private Vector3 localScale;

	private IEnumerator Start()
	{
		yield return new WaitForEndOfFrame();

		Transform child = transform.GetChild(0);

		child.position = position;
		child.rotation = Quaternion.Euler(rotation);
		child.localScale = localScale;
	}
}
