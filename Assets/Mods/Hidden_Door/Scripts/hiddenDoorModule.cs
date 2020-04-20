using KModkit;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class HiddenDoorModule : MonoBehaviour 
{
	[Header("References")]
	public KMBombModule BombModule;
	public KMBombInfo BombInfo;
	public KMRuleSeedable RuleSeedable;
	public SelectableBook BookPrefab;
	public Transform BookContainer;
	
	[Header("Module Settings")]
	public BookRange[] BookRanges;
	public float BookInterval;
	public int TRuleCount; // The number of alternatives per T rule section.
	public double DoubleRulesRatio;
	public Int2 MinMaxLevers;

	[Header("Book Settings")]
	public Material[] BookTypes;
	public string[] BookTypeNames;
	public string[] BookShapes;
	public Vector3 BookSelectionRotation;

	[Header("Sentences")]
	public string[] ConsequenceSentences;

	private Random tRandom; // random per session.
	private MonoRandom mRandom; // random per manual version.
	private RuleSets ruleSets;
	private List<SelectableBook> selectableBooks = new List<SelectableBook>();

	private int leverCount;
	private int leverType;
	private int tType = -1;
	private int t1Type = -1;


	private void Start()
	{
		tRandom = new Random();
		mRandom = RuleSeedable.GetRNG();
		ruleSets = new RuleSets();
		ruleSets.Initialize(mRandom);
		SelectableBook.Set(BookTypes, BookSelectionRotation);

		SpawnBooks();
		SetBookTypes();
		SetRules();
		SetKeys();
		IterateActiveLever();

		Debug.LogFormat("T = {0}, T1 = {3}, L = {1}, Lc = {2}", BookTypeNames[tType], BookTypeNames[leverType], leverCount, BookTypeNames[t1Type]);
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
		leverType = tRandom.Next(0, BookTypes.Length);
		leverCount = tRandom.Next(MinMaxLevers.X, MinMaxLevers.Y);

		for (int i = 0; i < leverCount; i++)
		{
			int j = tRandom.Next(0, selectableBooks.Count);
			selectableBooks[j].Set(false, true);

			int s = tRandom.Next(0, BookShapes.Length);
			int t;

			do
			{
				t = tRandom.Next(0, BookTypes.Length);
			} while (t == j);

			selectableBooks[j].Set(leverType, s, t);
		}

		for (int i = 0; i < selectableBooks.Count; i++)
		{
			SelectableBook current = selectableBooks[i];

			if (current.IsLever)
				continue;

			int j; // I've written this code twice now -> merge? 
			do
			{
				// Note: should I ensure a minimum number of books for each type? 
				j = tRandom.Next(0, BookTypes.Length);
			} while (j == leverType);

			int s = tRandom.Next(0, BookShapes.Length);
			int t;

			do
			{
				t = tRandom.Next(0, BookTypes.Length);
			} while (t == j);

			current.Set(false, false);
			current.Set(j, s, t); // TODO: change this to something random. 
		}
	}

	private void SetRules()
	{
		SetTRules();
		SetDirectionRules();
	}

	private void SetTRules()
	{
		Rule[][] sets = new Rule[MinMaxLevers.Y - MinMaxLevers.X][];

		for (int i = 0; i < sets.Length; i++)
		{
			Rule[] ruleSet = ruleSets.GenerateRuleSet(DoubleRulesRatio, TRuleCount, true);

			for (int j = 0; j < ruleSet.Length; j++)
			{
				Consequence consequenceIfRight = new Consequence();

				int k;
				do
				{
					k = mRandom.Next(0, BookTypes.Length);
				} while (k == leverType);

				int l;
				do
				{
					l = mRandom.Next(0, BookTypes.Length);
				} while (l == k || l == leverType);

				ruleSet[j].Text = string.Format(ConsequenceSentences.GetPseudoRandom(mRandom), BookTypeNames[k], BookTypeNames[l], ruleSet[j].Text);
				ruleSet[j].ConcequenceIfRight = consequenceIfRight;

				if (i + MinMaxLevers.X == leverCount && tType == -1 && ruleSet[j].IsAdhered(BombInfo))
				{
					tType = k;
					t1Type = l;
					Debug.Log(ruleSet[j].Text);
				}
			}

			sets[i] = ruleSet;
		}

		// TODO: write the rules' text to wherever they are needed for the pdf.
	}

	private void SetDirectionRules()
	{
		CountableString[] bookTypeCount = new CountableString[BookTypes.Length];
		CountableString[] bookSymbolTypeCount = new CountableString[BookTypes.Length];

		for(int i = 0; i < BookTypeNames.Length; i++)
		{
			bookTypeCount[i].S = BookTypeNames[i];
			bookSymbolTypeCount[i].S = BookTypeNames[i];
		}

		foreach (SelectableBook book in selectableBooks)
		{
			bookTypeCount[book.BookType]++;
			bookSymbolTypeCount[book.SymbolType]++;
		}

		Array.Sort(bookTypeCount);
		Array.Sort(bookSymbolTypeCount);

		bool bookTypeIsAlphabetical = true;
		bool symbolTypeIsAlphabetical = true;
		for (int i = 0; i < bookTypeCount.Length - 1; i++)
		{
			Debug.Log(bookTypeCount[i].ToString());
			Debug.Log(bookSymbolTypeCount[i].ToString());
			// HACK: it doesn't check all the letters, but hey, it works :p 
			bookTypeIsAlphabetical = bookTypeIsAlphabetical 
				&& bookTypeCount[i].S[0] <= bookTypeCount[i + 1].S[0] 
				&& bookTypeCount[i].S[1] < bookTypeCount[i + 1].S[1];

			symbolTypeIsAlphabetical = symbolTypeIsAlphabetical
				&& bookSymbolTypeCount[i].S[0] <= bookSymbolTypeCount[i + 1].S[0]
				&& bookSymbolTypeCount[i].S[1] < bookSymbolTypeCount[i + 1].S[1];
		}

		Debug.Log(bookTypeIsAlphabetical);
		Debug.Log(symbolTypeIsAlphabetical);	
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


/**
 * 
 * 
 * 
 * Pull every book with intervals that follow the following sequence, iterating between that sequence and the next number in the serial number. 
 * If the current nserial number number is 0, use the previously used interval as the new interval.
 *	- Number of [Square, star, circle] Icons on the [top, upper-middle, lower-middle, bottom] shelf. 
 *	- ^ with different iteration.
 *	- ^ with different iteration. 
 *	
 *	However, if the selected book is of type T or is a lever, it is skipped. in case it is of type T, the next interval is dequal to the number of occurances of T modulus the initially calculated interval. 
 *	If, however, that outcome is 0, the number of occurances of T is the new interval. 
 *	
 *	The direction in which should be iterated through the book case is not static either. 
 *	If the list of the total books per type, ordered descendingly, excluding T and the levertype, are in alphabetical order, the horizontal navigation direction is left to right. 
 *	Otherwise, it is in the reversed order. 
 *	The vertical navigation direction is determined by the total number of books per marking type, ordered ascendingly, excluding T and the levertype colors.
 *	If these are in non-alphabetical order, the direction is from top to bottom.
 *	Otherwise, the other way around.
 *	
 */
