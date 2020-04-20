using System.Collections.Generic;
using System;

public class HanoianRules
{
	/**
	 * Rule 1: The Graph with the highest individual value: A, B, C, D, E, F;
	 * Rule 2: The graph with the lowest individual value: Black, Blue, Green, Orange, Purple, Red;
	 * Rule 3: The pattern that's not in the graph: Stripes, Squares, Solid, Squares-Stripes, Dots, Dots-Stripes;
	 */

	/** Order: 
	 * (0)Rule 1, 
	 * (1)Rule 2, 
	 * (2)Rule 3, 
	 * (3)Line, 
	 * (4)Node 1, 
	 * (5)Node 2, 
	 * (6)Node 3
	 */
	private static int[,] rules = new int[,]
	{ 
		{ 0, 1, 2, 2, 5, 3, 1 },
		{ 0, 1, 3, 0, 1, 3, 4 },
		{ 0, 1, 4, 3, 2, 0, 4 },
		{ 0, 1, 5, 3, 0, 5, 1 },
		{ 0, 1, 6, 4, 4, 1, 2 },
		{ 0, 2, 3, 0, 3, 5, 4 },
		{ 0, 2, 4, 3, 4, 1, 5 },
		{ 0, 2, 5, 4, 5, 1, 2 },
		{ 0, 2, 6, 2, 1, 0, 5 },
		{ 0, 3, 4, 4, 0, 3, 1 },
		{ 0, 3, 5, 3, 1, 4, 0 },
		{ 0, 3, 6, 0, 4, 1, 0 },
		{ 0, 4, 5, 3, 2, 3, 5 },
		{ 0, 4, 6, 3, 0, 1, 4 },
		{ 0, 5, 6, 0, 1, 5, 3 },
		{ 1, 2, 3, 3, 5, 3, 0 },
		{ 1, 2, 4, 3, 4, 5, 2 },
		{ 1, 2, 5, 2, 2, 5, 3 },
		{ 1, 2, 6, 2, 0, 5, 1 },
		{ 1, 3, 4, 1, 1, 3, 0 },
		{ 1, 3, 5, 3, 5, 4, 1 },
		{ 1, 3, 6, 2, 4, 1, 5 },
		{ 1, 4, 5, 0, 2, 1, 3 },
		{ 1, 4, 6, 4, 0, 1, 2 },
		{ 1, 5, 6, 0, 5, 4, 3 },
		{ 2, 3, 4, 1, 2, 3, 1 },
		{ 2, 3, 5, 3, 0, 3, 4 },
		{ 2, 3, 6, 1, 1, 2, 5 },
		{ 2, 4, 5, 0, 4, 0, 2 },
		{ 2, 4, 6, 0, 3, 5, 2 },
		{ 2, 5, 6, 3, 2, 3, 0 },
		{ 3, 4, 5, 4, 5, 3, 0 },
		{ 3, 4, 6, 1, 0, 5, 1 },
		{ 3, 5, 6, 0, 5, 0, 2 },
		{ 4, 5, 6, 3, 0, 4, 2 },
	};

	private static int optionCount = 6;
	private static int ruleOptions = 5;
	private static int ruleCount = 3;
	private static int nodeCount = 3;
	private static int lineCount = 5;


	public static int get(int i, int j)
	{
		return rules[i, j];
	}

	public static int[] GetRandomRuleSet()
	{
		string log = "Playing with ruleset: ";
		int j = UnityEngine.Random.Range(0, rules.GetLength(0) - 1);
		int l = rules.GetLength(1);
		int[] ruleSet = new int[l];
		for (int i = 0; i < l; i++)
		{
			ruleSet[i] = rules[j, i];
			log += ruleSet[i] + ", ";
		}

		UnityEngine.Debug.Log(log);

		return ruleSet;
	}

	public static void Set(int optionCount, int ruleOptions, int ruleCount, int lineCount)
	{
		HanoianRules.optionCount = optionCount;
		HanoianRules.ruleOptions = ruleOptions;
		HanoianRules.nodeCount = ruleCount;
		HanoianRules.lineCount = lineCount;
	}

	public static void Generate(int seed)
	{
		Random random = new Random(seed);
		int c = UniqueCombinations(optionCount + 1, ruleCount);
		rules = new int[c , nodeCount + 4];
		int i = 0;

		for (int j = 0; j <= optionCount - 2; j++)
		{
			for (int k = j + 1; k <= optionCount - 1; k++)
			{
				for (int l = k + 1; l <= optionCount; l++, i++)
				{
					rules[i, 0] = j;
					rules[i, 1] = k;
					rules[i, 2] = l;
					rules[i, 3] = random.Next(0, lineCount);

					List<int> b = new List<int>();
					for (int m = 0; m <= ruleOptions; m++)
					{
						b.Add(m);
					}

					for (int m = 0; m < nodeCount; m++)
					{
						int n = random.Next(0, b.Count);
						int o = b[n];
						b.RemoveAt(n);
						rules[i, 4 + m] = o;
					}
				}
			}
		}
	}

	public static string SerializeRuleSet()
	{
		// TODO: probably should do this with stringbuilder.
		string s = "{\n";
		for (int i = 0; i < rules.GetLength(0); i++)
		{
			s += "\t{";
			for (int j = 0; j < rules.GetLength(1); j++)
			{
				s += rules[i, j] + ",";
			}
			s = s.Insert(s.Length - 1, "}");
			s += "\n";
		}
		s = s.Insert(s.Length - 2, "\n}");
		s = s.Substring(0, s.Length - 2);
		return s;
	}

	public static void Shuffle<E> (ref E[] list, int seed)
	{
		Random random = new Random(seed);
		List<E> copy = new List<E>(list);

		for (int i = 0; i < list.Length; i++)
		{
			int j = random.Next(0, copy.Count);
			list[i] = copy[j];
			copy.RemoveAt(j);
		}
	}

	private static int Faculty(int n)
	{
		int f = n; 
		while (n > 1)
		{
			n--;
			f *= n;
		}
		return f;
	}
	private static int UniqueCombinations(int n, int k)
	{
		return Faculty(n) / (Faculty(k) * Faculty(n - k));
	}
}
