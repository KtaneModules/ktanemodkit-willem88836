using UnityEngine;

public class FoodMenu : MonoBehaviour
{
	[SerializeField] private GameObject arrow;
	[SerializeField] private Vector3 up;
	[SerializeField] private Vector3 down;

	private bool isDown = false;

	public void Enable()
	{
		gameObject.SetActive(true);

		if (isDown)
		{
			Toggle();
		}
	}

	public void Disable()
	{
		gameObject.SetActive(false);
	}

	public void Toggle()
	{
		isDown = !isDown;
		arrow.transform.localPosition = isDown ? down : up;
	}

	public bool Confirm()
	{
		Disable();
		return isDown;
	}
}
