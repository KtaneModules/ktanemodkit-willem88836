using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class HiddenDoorModule : MonoBehaviour 
{
	private Random random;

	[Header("References")]
	public KMBombModule BombModule;
	public SelectableBook BookPrefab;
	public Transform BookContainer;
	
	[Header("Module Settings")]
	public BookRange[] BookRanges;
	public float BookInterval;

	public Int2 MinMaxLevers;

	[Header("Book Settings")]
	public Material[] BookTypes;
	public Vector3 BookSelectionRotation;


	private List<SelectableBook> selectableBooks = new List<SelectableBook>();

	public void Start()
	{
		random = new Random(); // Should I use some sort of seed here? Does the bomb have a seed? 
		SelectableBook.Set(BookTypes, BookSelectionRotation);

		SpawnBooks();
		SetBookTypes();
		SetKeys();
		IterateActiveLever();
	}

	public void SpawnBooks()
	{
		int k = 0;

		for(int i = 0; i < BookRanges.Length; i++)
		{
			BookRange range = BookRanges[i];
			int n = Mathf.FloorToInt((range.End.position.x - range.Start.position.x) / BookInterval);

			for (int j = 0; j < n; j++)
			{
				SelectableBook newBook = Instantiate(BookPrefab, BookContainer);
				newBook.transform.position = range.Start.position + Vector3.right * j * BookInterval;
				newBook.name = "book_" + k++;

				newBook.Initialize(OnLeverPulled);

				selectableBooks.Add(newBook);
			}
		}
	}

	private void SetBookTypes()
	{
		int leverType = random.Next(0, BookTypes.Length);
		int leverCount = random.Next(MinMaxLevers.X, MinMaxLevers.Y);

		Debug.LogFormat("Levertype: ({0}), Spawing ({1}) levers!", leverType, leverCount);

		for (int i = 0; i < leverCount; i++)
		{
			int j = random.Next(0, selectableBooks.Count);
			selectableBooks[j].Set(false, true, leverType);
		}

		for (int i = 0; i < selectableBooks.Count; i++)
		{
			SelectableBook current = selectableBooks[i];

			if (current.IsLever)
				continue;

			int j;
			do
			{
				// Note: should I ensure a minimum number of books for each type? 
				j = random.Next(0, BookTypes.Length);
			} while (j == leverType);

			current.Set(false, false, j);
		}
	}

	private void SetKeys()
	{

	}

	private void IterateActiveLever()
	{

	}

	public void OnLeverPulled()
	{
		bool isCorrect = true;

		foreach(SelectableBook book in selectableBooks)
		{
			if (!book.IsCorrect())
			{
				isCorrect = false;
				break;
			}
		}

		if (isCorrect)
		{
			BombModule.HandlePass();
		}
		else
		{
			BombModule.HandleStrike();

			// Resets all book selections.
			foreach (SelectableBook book in selectableBooks)
			{
				book.Deselect();
			}
		}
	}
}
