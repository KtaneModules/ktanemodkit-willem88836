using UnityEngine;
using System;


public class SelectableBook : MonoBehaviour
{
	[SerializeField] private KMSelectable selectable;
	[SerializeField] private Transform body;
	[SerializeField] private MeshRenderer meshRenderer;

	private static Vector3 selectedAngle;
	private static Material[] typeMaterials;

	private Action onLeverPulled;
	private int myType;

	private bool isSelected;
	private bool isKey;
	private bool isLever;


	public static void Set(Material[] typeMaterials, Vector3 selectedAngle)
	{
		SelectableBook.typeMaterials = typeMaterials;
		SelectableBook.selectedAngle = selectedAngle;
	}


	public void Initialize(int type, Action onLeverPulled)
	{
		this.myType = type;
		this.onLeverPulled = onLeverPulled;

		this.isSelected = false;
		this.isKey = false;
		this.isLever = false;

		meshRenderer.material = typeMaterials[myType];

		selectable.OnInteract -= ToggleSelection;
		selectable.OnInteract += ToggleSelection;
	}

	public void Reset(bool isKey, bool isLever)
	{
		Deselect();
		this.isKey = isKey;
		this.isLever = isLever;
	}

	public bool ToggleSelection()
	{
		if (isSelected)
		{
			Deselect();
		}
		else
		{
			Select();
		}

		return false;
	}
	public void Select()
	{
		if (isLever)
		{
			onLeverPulled.Invoke();
			return;
		}

		if (!isSelected)
		{
			body.Rotate(selectedAngle);
		}

		isSelected = true;
	}

	public void Deselect()
	{
		if (isSelected)
		{
			body.Rotate(-selectedAngle);
		}

		isSelected = false;
	}

	public bool IsCorrect()
	{
		return isKey == isSelected == true;
	}
}
