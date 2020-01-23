
using System;

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