using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class TextMeshBox : MonoBehaviour {

	private readonly char[] textSplitCharacters = new char[] { '\n', '\r' };
	private readonly char[] lineSplitCharacters = new char[] { ' ' };
	private readonly char[] trimEndCharacters = new char[] { ' ', '\n', '\r' };


	public int HorizontalCharacterLimit;
	public int VerticalLineLimit;
	public int VerticalAlignBottom; //TODO: Do stuff with this.
	public bool TrimOutputEnd;
	public bool IgnoreEmptyEntries;

	private TextMesh textMesh;


	private void Awake() 
	{
		this.textMesh = GetComponent<TextMesh>();
		SetText(this.textMesh.text);
	}

	public void SetText(string text)
	{
		string[] lines = IgnoreEmptyEntries 
			? text.Split(textSplitCharacters, System.StringSplitOptions.RemoveEmptyEntries)
			: text.Split(textSplitCharacters);

		List<string> splitLines = new List<string>();

		int lineLength = 0;
		int lineStartIndex = 0;

		for(int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			string[] words = IgnoreEmptyEntries
				? line.Split(lineSplitCharacters, System.StringSplitOptions.RemoveEmptyEntries)
				: line.Split(lineSplitCharacters);

			for(int j = 0; j < words.Length; j++)
			{
				string word = words[j];
				int wordLength = word.Length;
				if (lineLength + wordLength > HorizontalCharacterLimit
					|| j == words.Length - 1)
				{
					string newLine = "";
					for (; lineStartIndex < j; lineStartIndex++)
					{
						newLine += words[lineStartIndex] + " ";
					}

					if (j == words.Length - 1)
					{
						newLine += words[j] + " ";
						lineLength = 0;
						lineStartIndex = 0;
					}
					else
					{
						lineLength = wordLength;
					}

					splitLines.Add(newLine);
				}
				else
				{
					lineLength += wordLength;
				}
			}
		}

		lines = splitLines.ToArray();

		string output = "";
		for (int i = 0; i < lines.Length
			&& i < VerticalLineLimit; i++)
		{
			string line = lines[i];
			output += line + "\n";
		}

		if (TrimOutputEnd)
		{
			output = output.TrimEnd(trimEndCharacters);
		}

		this.textMesh.text = output;
	}
}
