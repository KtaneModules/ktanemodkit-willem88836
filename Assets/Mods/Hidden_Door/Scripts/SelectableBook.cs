using UnityEngine;
using System;

/// <summary>
///		Represents a Selectable book
/// </summary>
public class SelectableBook : MonoBehaviour
{
	[SerializeField] private KMSelectable selectable;
	[SerializeField] private Transform body;
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private GameObject[] symbols; 

	private static Material[] typeMaterials;
	private static Vector3 selectedAngle;

	private Action onLeverPulled;
	public int BookType { get; private set; }
	public int SymbolType { get; private set; }
	public int SymbolShape { get; private set; }

	public bool IsLever { get; private set; }
	private bool isKey;
	private bool isSelected;


	/// <summary>
	///		Sets the shared variables of all books.
	/// </summary>
	/// <param name="typeMaterials">Types of books.</param>
	/// <param name="selectedAngle">What direction a book rotates in once selected.</param>
	public static void Set(Material[] typeMaterials, Vector3 selectedAngle)
	{
		SelectableBook.typeMaterials = typeMaterials;
		SelectableBook.selectedAngle = selectedAngle;
	}


	public void Initialize(Action onLeverPulled)
	{
		this.onLeverPulled = onLeverPulled;

		this.isSelected = false;
		this.isKey = false;
		this.IsLever = false;

		selectable.OnInteract -= ToggleSelection;
		selectable.OnInteract += ToggleSelection;
	}

	/// <summary>
	///		Sets the puzzle element of the book.
	/// </summary>
	/// <param name="isKey">If the book should be selected.</param>
	/// <param name="isLever">If the book finishes the puzzle.</param>
	/// <param name="type">What type of book it is.</param>
	public void Set(bool isKey, bool isLever)
	{
		Deselect();
		this.isKey = isKey;
		this.IsLever = isLever;
	}

	public void Set(int bookType, int symbolShape, int symbolType)
	{
		this.BookType = bookType;
		meshRenderer.material = typeMaterials[this.BookType];
		
		if (this.SymbolShape != -1)
			symbols[this.SymbolShape].SetActive(false);
		this.SymbolShape = symbolShape;
		symbols[this.SymbolShape].SetActive(true);

		this.SymbolType = symbolType;
		symbols[this.SymbolShape].GetComponent<MeshRenderer>().material = typeMaterials[this.SymbolType];
	}

	/// <summary>
	///		Selects/Deselects book depending on its current selection.
	/// </summary>
	/// <returns>false</returns>
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
		if (!isSelected)
		{
			body.Rotate(selectedAngle);
		}

		isSelected = true;

		if (IsLever)
		{
			onLeverPulled.Invoke();
		}
	}

	public void Deselect()
	{
		if (isSelected)
		{
			body.Rotate(-selectedAngle);
		}

		isSelected = false;
	}

	/// <summary>
	///		Returns true if the book should be and is selected.
	/// </summary>
	/// <returns></returns>
	public bool IsCorrect()
	{
		return isKey == isSelected;
	}
}
