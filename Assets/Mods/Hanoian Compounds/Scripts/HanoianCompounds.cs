using System.Collections.Generic;
using UnityEngine;

public class HanoianCompounds : MonoBehaviour
{
	[Header("References")]
	public KMBombModule Bombmodule;
	public KMRuleSeedable RuleSeedable;
	public KMAudio Audio;
	public LineRenderer[] LineRenderers;
	public HanoianButton[] Buttons;

	[Header("Visuals")]
	public Material ButtonClicked;
	public Material ButtonUnclicked;
	public Material ButtonWrong;
	public Material[] LineColors;
	public Texture[] LinePatterns;

	[Header("Parameters")]
	public int LineCount;
	public int NodeCount;
	public Vector2 HeightEdges;
	public int ValueBuffer;
	public int HanoiMin; 
	public int HanoiMax;


	private int scalar; // Maximum value of a live node. Also used to scale the LineRenderers. 
	private int[,] values; // The values of all nodes. 
	private int[] tiles; // To what nodes the game tiles are assigned (-1 = it doesn't exist).
	private int[] ruleSet; // The game's ruleset. 
	private int currentNode; // Index of the node that is last/curently selected.


	/// <summary>
	///		Initializes Hanoian Compounds.
	/// </summary>
	public void Start()
	{
		int seed = RuleSeedable.GetRNG().Seed;
		if (seed != 1)
		{
			HanoianRules.Generate(seed);
			if (Application.platform == RuntimePlatform.WindowsEditor 
				|| Application.platform == RuntimePlatform.LinuxEditor 
				|| Application.platform == RuntimePlatform.OSXEditor)
			{
				Debug.Log(HanoianRules.SerializeRuleSet());
			}

			HanoianRules.Shuffle(ref LineColors, seed);
			HanoianRules.Shuffle(ref LinePatterns, seed);
		}

		values = new int[LineCount, NodeCount];
		tiles = new int[HanoiMax + 1];
		ruleSet = HanoianRules.GetRandomRuleSet();
		currentNode = -1;

		// Initializing, respectively, scalar, tiles, and buttons.
		scalar = 0;
		for (int i = HanoiMax; i >= HanoiMin; i--)
		{
			scalar += (int)Mathf.Pow(2, i);
			tiles[i] = -1;
		}

		for (int i = HanoiMin; i >= 0; i--)
		{
			tiles[i] = -1;
		}

		for (int i = 0; i < Buttons.Length; i++)
		{
			HanoianButton button = Buttons[i];
			button.Set(this, i, ButtonClicked, ButtonUnclicked, ButtonWrong);
			button.OnUnclick();
		}

		SetColors();
		SetStartingValues();
		SetLines();
	}


	/// <summary>
	///		Applies patterns and colors to all lines, 
	///		respecting the current ruleSet. 
	/// </summary>
	private void SetColors()
	{
		// Setting the colors. 
		// color rule is first ignored and then explicitely added,
		// to ensure it is in there (and there are no duplicates). 
		int colorRule = ruleSet[1];

		List<int> availableColors = new List<int>();
		for (int i = 0; i < LineColors.Length; i++)
		{
			if (i == colorRule)
				continue;

			availableColors.Add(i);
		}

		int ruledLine = Random.Range(0, availableColors.Count - 1);
		availableColors[ruledLine] = colorRule;


		// Setting Patterns. Ditto as before, but without adding. 
		int patternRule = ruleSet[0];

		List<int> availablePatterns = new List<int>();
		for (int i = 0; i < LinePatterns.Length; i++)
		{
			if (i == patternRule)
			{
				continue;
			}
			availablePatterns.Add(i);
		}

		// Setting all the colors and patterns. 
		for (int k = 0; k < LineRenderers.Length; k++) 
		{
			LineRenderer line = LineRenderers[k];
			int i = Random.Range(0, availableColors.Count - 1);
			int j = availableColors[i];
			availableColors.RemoveAt(i);
			if (j == colorRule)
			{
				// The ruleset now refers to the line instead of the pattern index. 
				ruleSet[1] = k;
			}
			Material material = LineColors[j];

			i = Random.Range(0, availablePatterns.Count - 1);
			j = availablePatterns[i];
			availablePatterns.RemoveAt(i);
			material.mainTexture = LinePatterns[j];

			line.material = LineColors[j];
		}
	}
	/// <summary>
	///		Sets the initial values of all lines, respecting the ruleSet.
	/// </summary>
	private void SetStartingValues()
	{
		// Gives all nodes a random value.
		for (int i = 0; i < values.GetLength(0); i++)
		{
			for (int j = 0; j < values.GetLength(1); j++)
			{
				values[i, j] = Random.Range(ValueBuffer, scalar - ValueBuffer);
			}
		}

		ResetLiveNodes();

		// Distributes tiles randomly, in legal order, across the live nodes.
		for (int i = HanoiMax; i >= HanoiMin; i--)
		{
			int j = GetRandomLiveNode();
			tiles[i] = j;
			values[ruleSet[3], j] += (int)Mathf.Pow(2, i);
		}

		// Applying rule 1 and 2
		SetRandomNonLiveNode(ruleSet[0], scalar + ValueBuffer); 
		SetRandomNonLiveNode(ruleSet[1], -ValueBuffer);
	}
	/// <summary>
	///		Applies the given value to a random node in the provided line
	///		and ensures a non-alive node is selected if the line is the 
	///		game line. 
	/// </summary>
	/// <param name="lineIndex">Index of the line</param>
	/// <param name="value">The new value of the chosen node</param>
	private void SetRandomNonLiveNode(int lineIndex, int value)
	{
		if (lineIndex == ruleSet[3])
		{
			// Ensures that the live nodes aren't affected. 
			int a;
			do
			{
				a = Random.Range(0, values.GetLength(1) - 1);
			} while (a == ruleSet[4] || a == ruleSet[5] || a == ruleSet[6]);
			values[lineIndex, a] = value;
		}
		else
		{
			int a = -1;
			try
			{
				a = Random.Range(0, values.GetLength(1) - 1);
				values[lineIndex, a] = value;
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.Message);
				Debug.LogWarning(a);
			}
		}
	}


	/// <summary>
	///		Called by the HanoianButtons when they are clicked. 
	/// </summary>
	/// <param name="i">Node-index of the button</param>
	public void OnButtonClicked(int i)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		HanoianButton button = Buttons[i];

		if (currentNode == -1)
		{
			currentNode = i;

			foreach (HanoianButton b in Buttons)
			{
				b.OnUnclick();
			}

			button.OnClick();
		}
		else if (currentNode == i)
		{
			button.OnUnclick();
			currentNode = -1;
		}
		else
		{
			AttemptMove(currentNode, i);
		}
	}


	/// <summary>
	///		Tests if the provided move is legal, and acts 
	///		accordingly if it is (not).
	/// </summary>
	/// <param name="from">Node from which a tile is selected.</param>
	/// <param name="to">Node to which a tile is moved.</param>
	private void AttemptMove(int from, int to)
	{
		if (!IsAliveNode(from) || !IsAliveNode(to))
		{
			OnMoveFail(from, to);
			return;
		}

		int k = -1;

		for (int i = 0; i < tiles.Length; i++)
		{
			if (tiles[i] == to)
			{
				if (k == -1)
				{
					OnMoveFail(from, to);
					return; 
				}
				else
				{
					break;
				}
			}

			if (k == -1 && tiles[i] == from)
			{
				k = i;
			}
		}

		MoveTile(from, to, k);
		currentNode = -1;
	}
	/// <summary>
	///		Virtually moves a selected tile from a node to another,
	///		tests if the game is complete, shifts all non-alive nodes, 
	///		and resets the current state.
	/// </summary>
	/// <param name="from">Node-index from which the tile is taken.</param>
	/// <param name="to">Node-index to which the tile is moved.</param>
	/// <param name="t">Index of the tile.</param>
	private void MoveTile(int from, int to, int t)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Switch, transform);

		tiles[t] = to;
		int val = (int)Mathf.Pow(2, t);
		values[ruleSet[3], from] -= val;
		values[ruleSet[3], to] += val;
		Buttons[from].OnUnclick();
		Buttons[to].OnUnclick();
		SetLines();

		Debug.LogFormat("Moved ({0}) to ({1}).", from, to);

		int height = values[ruleSet[3], ruleSet[6]];

		Debug.LogFormat("New value of ({0}) is: ({1})", ruleSet[6], height);

		if (height == scalar)
		{
			Bombmodule.HandlePass();
		}

		ShiftNonAliveNodes();
	}
	/// <summary>
	///		Handles strike, and resets the current state.
	/// </summary>
	/// <param name="from">(optional) Node-index from which the tile was attempted to be moved.</param>
	/// <param name="to">(optional) Node-index to which the tile was attempted to be moved.</param>
	private void OnMoveFail(int from = -1, int to = -1)
	{
		if (from >= 0)
		{
			Buttons[from].OnWrongClick();
		}
		if (to >= 0)
		{
			Buttons[to].OnWrongClick();
		}
		Bombmodule.HandleStrike();
		currentNode = -1;
	}


	/// <summary>
	///		Shifts all non-alive nodes to a new value.
	/// </summary>
	private void ShiftNonAliveNodes()
	{
		// TODO: this.
	}
	/// <summary>
	///		Adjusts the LineRenderers' values to correspond with the current node-values.
	/// </summary>
	private void SetLines()
	{
		// TODO: do something with lerping.
		for (int i = 0; i < LineRenderers.Length; i++)
		{
			LineRenderer line = LineRenderers[i];
			Vector3[] positions = new Vector3[values.GetLength(1)];
			int c = line.GetPositions(positions);

			for (int j = 0; j < c; j++)
			{
				float a = (float)values[i, j] / (scalar + ValueBuffer);
				float b = Mathf.Lerp(HeightEdges.x, HeightEdges.y, a);
				positions[j].z = b;
			}

			line.SetPositions(positions);
		}
	}


	/// <summary>
	///		Returns true if the provided index belongs to an alive node.
	/// </summary>
	/// <param name="i">Node-index</param>
	/// <returns></returns>
	private bool IsAliveNode(int i)
	{
		return i == ruleSet[4] || i == ruleSet[5] || i == ruleSet[6];
	}
	/// <summary>
	///		Sets the values of all live nodes to 0.
	/// </summary>
	/// <returns></returns>
	private void ResetLiveNodes()
	{
		int line = ruleSet[3];
		values[line, ruleSet[4]] = 0;
		values[line, ruleSet[5]] = 0;
		values[line, ruleSet[6]] = 0;
	}
	/// <summary>
	///		Returns the index of a random live node.
	/// </summary>
	/// <returns></returns>
	private int GetRandomLiveNode()
	{
		return ruleSet[Random.Range(4, 7)];
	}
}
