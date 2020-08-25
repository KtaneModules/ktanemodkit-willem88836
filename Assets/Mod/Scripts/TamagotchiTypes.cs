using UnityEngine;

[CreateAssetMenu(menuName = "Tamagotchi/Types")]
public class TamagotchiTypes : ScriptableObject
{
	[System.Serializable]
	public class TamagotchiSubtypes
	{
		public Texture[] Subtypes;
	}

	public TamagotchiSubtypes[] Types;


	public int Length()
	{
		return Types.Length;
	}

	public int Length(int i)
	{
		return Types[i].Subtypes.Length;
	}

	public Texture Get(int i, int j)
	{
		return Types[i].Subtypes[j];
	}
}
