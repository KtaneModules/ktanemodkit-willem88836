

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