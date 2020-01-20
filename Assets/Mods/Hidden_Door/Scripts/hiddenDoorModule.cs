using KModkit;
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
				}
			}

			sets[i] = ruleSet;
		}

		// TODO: write the rules' text to wherever they are needed for the pdf.
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
