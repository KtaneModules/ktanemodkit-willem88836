
using UnityEngine;

public class Tamagotchi
{
	public enum TamagotchiState
	{
		Neutral,
		Hungry,
		Sick,
		Bored,
		Messy,
		Dead
	}

	public int Type;
	public int Subtype;
	public TamagotchiState State;

	public int AgeState;
	public int Age;

	public int TimeImpaired;
	public int Hunger;
	public int Boredness;
	public int Cleanliness;
	public int Sickness;

	public Tamagotchi(TamagotchiTypes tamagotchiTypes)
	{
		Type = Random.Range(0, tamagotchiTypes.Length());
		Subtype = Random.Range(0, tamagotchiTypes.Length(Type));
		State = TamagotchiState.Neutral;

		AgeState = 0;
		Age = 0;

		Hunger = 0;
		Boredness = 0;
		Cleanliness = 0;
		Sickness = 0;
	}

	public void Tick()
	{
		Age++;
		Boredness++;
		Cleanliness++;
		Hunger++;

		if(State != TamagotchiState.Neutral)
		{
			TimeImpaired++;
		}
	}
}
