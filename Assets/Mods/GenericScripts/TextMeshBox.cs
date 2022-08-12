using System.Collections.Generic;
using UnityEngine;

namespace wmeijer {
	///		Potential Improvements: 
	///			* There's a lot of string manipulation. 
	///			* If the TextMesh' text has changed, it is automatically formatted (e.g. in Update()). 
	///				This makes the component easer to use, and less laborous to integrate.
	///			* Calculate the width and height based on the component's RectTransform and TextMesh settings.


	/// <summary>
	///		TextMeshbox is an extention to TextMesh that allows you to control text overflow. 
	/// </summary>
	[RequireComponent(typeof(TextMesh))]
	public sealed class TextMeshBox : MonoBehaviour 
	{
		private readonly char[] textSplitCharacters = new char[] { '\n', '\r' };
		private readonly char[] lineSplitCharacters = new char[] { ' ' };
		private readonly char[] trimEndCharacters = new char[] { ' ', '\n', '\r' };


		public int HorizontalCharacterLimit;
		public int VerticalLineLimit;
		public bool CutOffLastLines;
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
				// Each word in each line is dealt with separately. 
				string line = lines[i];
				string[] words = IgnoreEmptyEntries
					? line.Split(lineSplitCharacters, System.StringSplitOptions.RemoveEmptyEntries)
					: line.Split(lineSplitCharacters);

				for(int j = 0; j < words.Length; j++)
				{
					string word = words[j];
					int wordLength = word.Length;

					// Each line of text can only take somany characters. 
					// if the current word exceeds that amount, or is the last word. 
					// A new string is created using all the words between 
					// lineStartIndex, and the previous word.
					int nextLength = lineLength + wordLength;
					if (nextLength > HorizontalCharacterLimit
						|| j == words.Length - 1)
					{
						string newLine = "";
						for (; lineStartIndex < j; lineStartIndex++)
						{
							newLine += words[lineStartIndex] + " ";
						}

						if (j == words.Length - 1)
						{
							if(nextLength > HorizontalCharacterLimit)
							{
								splitLines.Add(newLine);
								splitLines.Add(words[j]);
								break;
							}
							else
							{
								newLine += words[j] + " ";
								lineLength = 0;
								lineStartIndex = 0;
							}
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

			// Depending on the settings either the first or last sentences 
			// are cut off, if there are too many.
			int k = CutOffLastLines
				? 0
				: Mathf.Max(0, lines.Length - VerticalLineLimit);

			int l = 0;
			while (l < VerticalLineLimit
				&& k < lines.Length)
			{
				string line = lines[k];
				output += line + "\n";
				l++;
				k++;
			}

			// the output string ends with a newline, and occasionally spaces. 
			// These are trimmed.
			if (TrimOutputEnd)
			{
				output = output.TrimEnd(trimEndCharacters);
			}

			this.textMesh.text = output;
		}
	}
}
