using UnityEngine;

[CreateAssetMenu(menuName = "Tamagotchi")]
public class Tamagotchi : ScriptableObject
{
	public Sprite Sprite;
	public string Name;

	public int LifeSpan;
	public int AgeStage;

	public bool SleepsIn;
	public bool ResetStatsOnCreation;

	public int HungerRate;
	public int HappinessRate;
	public int HealthRate;
}

public class TamagotchiStats
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

	public TamagotchiState State;

	public int Age;

	public int Hunger;
	public int Happiness;
	public int Cleanliness;
	public int Sickness;

	private Tamagotchi tamagotchi;

	public TamagotchiStats()
	{
		Hunger = 0;
		Happiness = 0;
		Cleanliness = 0;
		Sickness = 0;
	}

	public void SetTamagotchi(Tamagotchi tamagotchi)
	{
		this.tamagotchi = tamagotchi;
	}

	public void Tick()
	{
		Age++;
		Happiness += tamagotchi.HappinessRate;
		Cleanliness++;
		Hunger++;

		if (State != TamagotchiState.Neutral)
		{
			TimeImpaired++;
		}
	}
}
