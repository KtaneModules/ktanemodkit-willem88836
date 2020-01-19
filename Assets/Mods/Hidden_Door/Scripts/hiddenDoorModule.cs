using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hiddenDoorModule : MonoBehaviour 
{
	public BookRange[] BookRanges;
	public float BookInterval;
	public KMSelectable bookPrefab;
	public Transform BookContainer;

	private List<Truple<KMSelectable, bool, bool>> bookRequirements = new List<Truple<KMSelectable, bool, bool>>();

	public void Start()
	{
		KMSelectable selectable = GetComponent<KMSelectable>();
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
				KMSelectable newBook = Instantiate(bookPrefab, BookContainer);
				newBook.transform.position = range.Start.position + Vector3.right * j * BookInterval;
				newBook.name = "book_" + k++;

				Truple<KMSelectable, bool, bool> t = new Truple<KMSelectable, bool, bool>(newBook, false, false);
				newBook.OnInteract += delegate { return OnInteract(t); };

				bookRequirements.Add(t);
			}
		}
	}


	public bool OnInteract(Truple<KMSelectable, bool, bool> t)
	{
		Debug.Log(t.A.name + " Selected!");
		return false;
	}
}

[System.Serializable]
public class BookRange
{
	public Transform Start;
	public Transform End;
}
