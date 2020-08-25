using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class ButtonLid : MonoBehaviour
{
	[SerializeField] private KMSelectable child;
	[SerializeField] private Vector3 minRot;
	[SerializeField] private Vector3 maxRot;
	[SerializeField] private float acceleration = 2;

	private bool opened = false;
	private float alpha;

	private void Start()
	{
		KMSelectable self = GetComponent<KMSelectable>();
		self.OnInteract += OnInteract;

		child.enabled = false;
	}

	private void Update()
	{
		alpha += (opened ? 1 : -1)
			* Time.deltaTime
			* acceleration;

		alpha = Mathf.Clamp(alpha, 0f, 1f);

		Vector3 b = Vector3.Lerp(minRot, maxRot, alpha);
		transform.localRotation = Quaternion.Euler(b);
	}

	private bool OnInteract()
	{
		opened = !opened;
		child.enabled = opened;

		return true;
	}
}
