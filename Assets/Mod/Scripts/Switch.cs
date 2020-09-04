using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class Switch : MonoBehaviour
{
	public Vector3 BaseRotation;
	public Vector3 TargetRotation;
	public float rotationSpeed;

	private ThreeSwitchesModule parent;
	private KMAudio bombAudio;
	private int index;

	private bool clicked;
	private float alpha = 0;

	private void Awake()
	{
		KMSelectable selectable = GetComponent<KMSelectable>();
		selectable.OnInteract += Click;
	}

	public void Initialize(ThreeSwitchesModule parent, KMAudio audio, int index)
	{
		this.parent = parent;
		this.bombAudio = audio;
		this.index = index;
	}

	private bool Click()
	{
		clicked = !clicked;
		parent.OnClick(index, clicked);
		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Switch, transform);
		return true;
	}

	private void Update()
	{
		alpha = Mathf.Clamp01(alpha + rotationSpeed * Time.deltaTime * (clicked ? 1: -1));
		Vector3 rotation = Vector3.Lerp(BaseRotation, TargetRotation, alpha);
		transform.localRotation = Quaternion.Euler(rotation);
	}
}
