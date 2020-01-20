using KModkit;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class HiddenDoorModule : MonoBehaviour 
{
	[Header("References")]
	public KMBombModule BombModule;
	public KMRuleSeedable RuleSeedable;
	public SelectableBook BookPrefab;
	public Transform BookContainer;
	
	[Header("Module Settings")]
	public BookRange[] BookRanges;
	public float BookInterval;
	public int TRuleCount; // The number of alternatives per T rule section.

	public Int2 MinMaxLevers;

	[Header("Book Settings")]
	public Material[] BookTypes;
	public Vector3 BookSelectionRotation;


	private Random tRandom; // random per session.
	private MonoRandom mRandom; // random per manual version.
	private List<SelectableBook> selectableBooks = new List<SelectableBook>();


	private void Start()
	{
		tRandom = new Random();
		mRandom = RuleSeedable.GetRNG();
		SelectableBook.Set(BookTypes, BookSelectionRotation);

		SpawnBooks();
		SetBookTypes();
		SetRules();
		SetKeys();
		IterateActiveLever();
	}

	private void SpawnBooks()
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
		int leverType = tRandom.Next(0, BookTypes.Length);
		int leverCount = tRandom.Next(MinMaxLevers.X, MinMaxLevers.Y);

		Debug.LogFormat("Levertype: ({0}), Spawing ({1}) levers!", leverType, leverCount);

		for (int i = 0; i < leverCount; i++)
		{
			int j = tRandom.Next(0, selectableBooks.Count);
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
				j = tRandom.Next(0, BookTypes.Length);
			} while (j == leverType);

			current.Set(false, false, j);
		}
	}

	private void SetRules()
	{
		for (int i = 0; i < MinMaxLevers.Y - MinMaxLevers.X; i++)
		{
			for (int j = 0; j < TRuleCount; j++)
			{
				//string rule = j == 0 ? "" : RuleSets.NoninitialRulePrefix;
				//string clr = RuleSets.BookTypes[mRandom.Next(0, RuleSets.BookTypeCount)];


				
			}
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
