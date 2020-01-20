using KModkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RuleSets
{
	//public int BookTypeCount { get { return BookTypes.Length; } }
	//public string[] BookTypes = new string[]
	//{
	//	"Red",
	//	"Green",
	//	"Blue",
	//	"Brown",
	//	"Black"
	//};

	private char[] serialUniqueSymbols = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
	private char[] serialVowels = new char[] { 'A', 'E', 'I', 'O', 'U' };
	private char[] serialEvens = new char[] { '2', '4', '6', '8', '0' };
	private char[] serialOdds = new char[] { '1', '3', '5', '7', '9' };

	private MonoRandom mRandom;

	public void Initialize(MonoRandom mRandom)
	{
		this.mRandom = mRandom;
	}


	public Rule[] GenerateRuleSet(double a, int l)
	{
		Rule[] ruleSet = new Rule[l];

		for (int i = 0; i < l; i++)
		{
			double j = mRandom.NextDouble();
			Rule rule = j >= a 
				? GenerateSingleRule() 
				: GenerateDoubleRule();
			
			if (i > 0)
			{
				rule.Text = "Otherwise, " + rule.Text;
			}
			else
			{
				rule.Text = rule.Text[0].ToString().ToUpper() + rule.Text.Substring(1);
			}

			ruleSet[i] = rule;
		}

		return ruleSet;
	}

	public Rule GenerateDoubleRule()
	{
		Rule rule = GenerateSingleRule();
		Rule subRule = GenerateSingleRule();
		rule.Text = string.Format("{0}, and {1}", rule.Text, subRule.Text);
		rule.SubRule = subRule;
		return rule;
	}

	public Rule GenerateSingleRule()
	{
		int r = mRandom.Next(0, 4);
		Rule rule;

		if (r == 0)
		{
			rule = GenerateIndicatorRule();
		}
		else if (r == 1)
		{
			rule = GeneratePortRule();
		}
		else if (r == 2)
		{
			rule = GenerateBatteryRule();
		}
		else
		{
			rule = GenerateSerialRule();
		}

		return rule;
	}
	

	// TODO: Make each Rule an individual object? Gives opportunity to just list the options, and thus let custom modules add own rules. 
	/// <summary>
	///		Returns a pseudo-random Indicator Rule.
	/// </summary>
	/// <param name="mRandom"></param>
	/// <returns></returns>
	public Rule GenerateIndicatorRule()
	{
		int r = mRandom.Next(0, 5);
		Rule rule = new Rule();

		if (r == 0)
		{
			// Simple Indicator Check.
			Indicator label = GetRandomIndicator();
			rule.Text = string.Format("if there is an indicator with label {0}", label.ToString());
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.IsIndicatorPresent(b, label); }; 
		}
		else if (r == 1)
		{
			// Lit Indicator Check.
			Indicator label = GetRandomIndicator();
			rule.Text = string.Format("if there is a lit indicator with label {0}", label.ToString());
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.IsIndicatorOn(b, label); };
		}
		else if (r == 2)
		{
			// Unlit Indicator Check.
			Indicator label = GetRandomIndicator();
			rule.Text = string.Format("if there is an unlit indicator with label {0}", label.ToString());
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.IsIndicatorOff(b, label); };
		}
		else if (r == 3)
		{
			// Simple On Indicator Check.
			rule.Text = "if there is a lit indicator";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetOnIndicators(b).Any(); };
		}
		else if (r == 4)
		{
			// Simple Off Indicator Check.
			rule.Text = "if there is an unlit indicator";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetOffIndicators(b).Any(); };
		}

		return rule;
	}
	public Indicator GetRandomIndicator()
	{
		// TODO: This might load modded indicators. Make sure to filter these. 
		return (Indicator)mRandom.Next(0, (int)Indicator.NLL + 1);
	}


	/// <summary>
	///		Generates a pseudo-random Port Rule.
	/// </summary>
	/// <param name="mRandom"></param>
	/// <returns></returns>
	public Rule GeneratePortRule()
	{
		int r = mRandom.Next(0, 6);
		Rule rule = new Rule();

		if (r == 0)
		{
			// Single Does not have port.
			Port port = GetRandomPort();
			rule.Text = string.Format("if the bomb contains a {0} port", port.ToString());
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.IsPortPresent(b, port); };
		}
		else if (r == 1)
		{
			// Single Does Have Port.
			Port port = GetRandomPort();
			rule.Text = string.Format("if the bomb does not contain a {0} port", port.ToString());
			rule.RuleTest = (KMBombInfo b) => { return !KMBombInfoExtensions.IsPortPresent(b, port); };
		}
		else if (r == 2)
		{
			// Has specific duplicate port. 
			Port port = GetRandomPort();
			rule.Text = string.Format("if the bomb has two or more {0} ports", port.ToString());
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.IsDuplicatePortPresent(b, port); };
		}
		else if (r == 3)
		{
			// Has Duplicate Port
			rule.Text = "if the bomb has a duplicate port";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.IsDuplicatePortPresent(b); };
		}
		else if (r == 4)
		{
			// Has one portplate.
			rule.Text = "if the bomb as exactly one port plate";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetPortPlateCount(b) == 1; };
		}
		else if (r == 5)
		{
			rule.Text = "if the bomb as two or more port plates";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetPortPlateCount(b) >= 2; };
		}

		return rule;
	}
	public Port GetRandomPort()
	{
		// TODO: This might load modded ports. Make sure to filter these. 
		return (Port)mRandom.Next(0, (int)Port.PCMCIA + 1);
	}


	public Rule GenerateBatteryRule()
	{
		int r = mRandom.Next(0, 5);
		Rule rule = new Rule();

		if (r == 0)
		{
			// One battery
			rule.Text = "if the bomb has exactly one battery attached to it";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetBatteryCount(b) == 1; };
		}
		else if (r == 1)
		{
			// Two or more batteries.
			rule.Text = "if the bomb has two or more batteries attached to it";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetBatteryCount(b) >= 2; };
		}
		else if (r == 2)
		{
			rule.Text = "if the bomb has no batteries attached to it";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetBatteryCount(b) == 0; };
		}
		else if (r == 3)
		{
			// One Battery holder.
			rule.Text = "if the bomb has one battery holder";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetBatteryHolderCount(b) == 1; };
		}
		else if (r == 4)
		{
			// Two or more holders.
			rule.Text = "if the bomb has two or more battery holders";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetBatteryHolderCount(b) >= 2; };
		}

		return rule;
	}
	public Battery GetRandomBattery()
	{
		// TODO: this returns all kinds of strange batteries. Make sure to filter these. 
		return (Battery)mRandom.Next(0, (int)Battery.AAx4 + 1);
	}

	
	public Rule GenerateSerialRule()
	{
		int r = mRandom.Next(0, 11);
		Rule rule = new Rule();

		if (r == 0)
		{
			// contains a vowel.
			rule.Text = "if the serial number contains a vowel";
			rule.RuleTest = (KMBombInfo b) =>
			{
				foreach (char c in KMBombInfoExtensions.GetSerialNumberLetters(b))
				{
					foreach (char d in serialVowels)
					{
						if (c == d)
						{
							return true;
						}
					}
				}
				return false;
			};
		}
		else if (r == 1)
		{
			// does not contain a vowel.
			rule.Text = "if the serial number does not contain a a vowel";
			rule.RuleTest = (KMBombInfo b) =>
			{
				foreach (char c in KMBombInfoExtensions.GetSerialNumberLetters(b))
				{
					foreach (char d in serialVowels)
					{
						if (c == d)
						{
							return false;
						}
					}
				}
				return true;
			};
		}
		else if (r == 2)
		{
			// contains even numbers.
			rule.Text = "if the serial number contains an even number";
			rule.RuleTest = (KMBombInfo b) =>
			{
				foreach (char c in KMBombInfoExtensions.GetSerialNumberNumbers(b))
				{
					foreach (char d in serialEvens)
					{
						if (c == d)
						{
							return true;
						}
					}
				}
				return false;
			};
		}
		else if (r == 3)
		{
			rule.Text = "if the serial number contains an odd number";
			rule.RuleTest = (KMBombInfo b) =>
			{
				foreach (char c in KMBombInfoExtensions.GetSerialNumberNumbers(b))
				{
					foreach (char d in serialOdds)
					{
						if (c == d)
						{
							return true;
						}
					}
				}
				return false;
			};
		}
		else if (r == 4)
		{
			// numbers sum to even
			rule.Text = "if the sum of all numbers in the serial number is even";
			rule.RuleTest = (KMBombInfo b) => 
			{ 
				int t = 0;
				foreach (char c in KMBombInfoExtensions.GetSerialNumber(b))
				{
					int d = 0;
					int.TryParse(c.ToString(), out d);
					t += d;
				}
				return t % 2 == 0;
			};
		}
		else if (r == 5)
		{
			// numbers sum to even
			rule.Text = "if the sum of all numbers in the serial number is odd";
			rule.RuleTest = (KMBombInfo b) =>
			{
				int t = 0;
				foreach (char c in KMBombInfoExtensions.GetSerialNumber(b))
				{
					int d = 0;
					int.TryParse(c.ToString(), out d);
					t += d;
				}
				return t % 2 == 1;
			};
		}
		else if (r == 6)
		{
			// odd number of numbers.
			rule.Text = "if the number of numbers in the serial number is an odd number";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetSerialNumberNumbers(b).Count() % 2 == 1; };
		}
		else if (r == 7)
		{
			// even number of numbers.
			rule.Text = "if the number of letters in the serial number is an odd number";
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetSerialNumberNumbers(b).Count() % 2 == 0; };
		}
		else if (r == 8)
		{
			// so-manieth icon is a letter
			int i = mRandom.Next(0, 7);
			rule.Text = string.Format("if the {0}th symbol in the serial number is a letter", i);
			rule.RuleTest = (KMBombInfo b) => { int a;  return !int.TryParse(KMBombInfoExtensions.GetSerialNumber(b).ElementAt(i).ToString(), out a); };
		}
		else if (r == 9)
		{
			// so-manieth icon is a number.
			int i = mRandom.Next(0, 7);
			rule.Text = string.Format("if the {0}th symbol in the serial number is a number", i);
			rule.RuleTest = (KMBombInfo b) => { int a; return int.TryParse(KMBombInfoExtensions.GetSerialNumber(b).ElementAt(i).ToString(), out a); };
		}
		else if (r == 10)
		{
			// so-maniet icon is a specific char.
			int i = mRandom.Next(0, 7);
			char c = serialUniqueSymbols[mRandom.Next(0, serialUniqueSymbols.Length)];
			rule.Text = string.Format("if the {0}th symbol of the serial number is a \"{1}\"", i, c);
			rule.RuleTest = (KMBombInfo b) => { return KMBombInfoExtensions.GetSerialNumber(b).ElementAt(i) == c; };
		}

		return rule;
	}
}

public class Rule
{
	public string Text;

	public Func<KMBombInfo, bool> RuleTest;
	public Rule SubRule;

	public Consequence ConcequenceIfRight;


	public bool IsAdhered(KMBombInfo bombInfo)
	{
		return (SubRule == null || SubRule.IsAdhered(bombInfo)) 
			&& RuleTest.Invoke(bombInfo);
	}
}

public class Consequence
{
	public string Text;
	public Action OnInvoke;
}
