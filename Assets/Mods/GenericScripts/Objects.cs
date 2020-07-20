using System;
using UnityEngine;

[Serializable]
public class Tuple<T, T1>
{
	public T A;
	public T1 B;

	public Tuple(T a, T1 b)
	{
		this.A = a;
		this.B = b;
	}
}

[Serializable]
public class Truple<T, T1, T2>
{
	public T A;
	public T1 B;
	public T2 C;

	public Truple(T a, T1 b, T2 c)
	{
		this.A = a;
		this.B = b;
		this.C = c;
	}
}

[System.Serializable]
public struct Int2
{
	public int X;
	public int Y;

	public Int2(int x, int y)
	{
		this.X = x;
		this.Y = y;
	}
}

public static class Extentions
{
	public static T GetPseudoRandom<T>(this T[] array, MonoRandom mRandom)
	{
		return array[mRandom.Next(0, array.Length)];
	}
}

public static class Positions
{
	public static Vector3 FarAway
	{
		get { return Vector3.right * 1000; }
	}
}

public struct CountableString : IComparable<CountableString>
{
	public string S;
	public int C;

	public int CompareTo(CountableString other)
	{
		return other.C > this.C ? -1 : 1;
	}

	public override string ToString()
	{
		return string.Format("{0} - {1}", S, C);
	}

	public static CountableString operator +(CountableString a, int d)
	{
		a.C += d;
		return a;
	}
	public static CountableString operator -(CountableString a, int d)
	{
		a.C -= d;
		return a;
	}
	public static CountableString operator ++(CountableString a)
	{
		a.C++;
		return a;
	}
	public static CountableString operator --(CountableString a)
	{
		a.C--;
		return a;
	}
}


public static class StringManipulation
{
	/// <summary>
	///		Converts a letter to an integer regardles of capitalization. 
	///		A = 0, B = 1, etc.
	/// </summary>
	public static int AlphabetToInt(char c)
	{
		// offsets are derived from ascii table.
		int i = (int)c;
		
		// char is capitalized. 
		if(i > 90)
		{
			// lowercase offset is subtracted.
			i -= 97;
		}
		else
		{
			// uppercase offset is subtracted.
			i -= 65;
		}

		return i;
	}

	/// <summary>
	///		Converts a letter to an integer regardles of capitalization. 
	///		A = 1, B = 2, etc.
	/// </summary>
	public static int AlphabetToIntPlusOne(char c)
	{
		return AlphabetToInt(c) + 1;
	}

	/// <summary>
	///		Converts a an integer to a capital letter. 
	///		0 = A, 1 = B, etc.
	/// </summary>
	public static char IntToAlphabet(int i)
	{
		i += 65;
		return (char)i;
	}

	/// <summary>
	///		Converts a an integer to a capital letter. 
	///		1 = A, 2 = B, etc.
	/// </summary>
	public static char IntToAlphabetPlusOne(int i)
	{
		i += 64;
		return (char)i;
	}
}
