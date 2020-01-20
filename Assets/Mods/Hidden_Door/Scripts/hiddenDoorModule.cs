using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class HiddenDoorModule : MonoBehaviour 
{
	private Random random; 

	[Header("Module Settings")]
	public BookRange[] BookRanges;
	public float BookInterval;
	public SelectableBook bookPrefab;
	public Transform BookContainer;

	[Header("Book Settings")]
	public Material[] BookTypes;
	public Vector3 BookSelectionRotation;



	// TODO: figure out a way to do this neatly. 
	private List<SelectableBook> selectableBooks = new List<SelectableBook>();

	public void Start()
	{
		random = new System.Random();
		KMSelectable selectable = GetComponent<KMSelectable>();
		SelectableBook.Set(BookTypes, BookSelectionRotation);
		SpawnBooks();
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
				SelectableBook newBook = Instantiate(bookPrefab, BookContainer);
				newBook.transform.position = range.Start.position + Vector3.right * j * BookInterval;
				newBook.name = "book_" + k++;

				newBook.Initialize(random.Next(0, BookTypes.Length), OnLeverPulled);

				selectableBooks.Add(newBook);
			}
		}
	}

	public void OnLeverPulled()
	{
		Debug.Log("Lever pulled!");

	}
}
