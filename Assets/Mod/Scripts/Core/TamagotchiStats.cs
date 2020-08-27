using Newtonsoft.Json;
using UnityEngine;

public class TamagotchiStats
{
	[JsonIgnore] public const int MAX_DISCIPLINE = 100;
	[JsonIgnore] public const float MAX_HUNGER = 4;
	[JsonIgnore] public const float MAX_HAPPINESS = 4;
	[JsonIgnore] public const float MAX_MISBEHAVING = 100;

	[JsonIgnore] public Tamagotchi Base = null;
	public int TamagotchiIndex = -1;

	public int Age = 0;

	public float Hunger = 0;
	public float Happiness = 0;

	public int Weight = 0;
	public int Discipline = 0;
	public int CarePoints = 0;
	public float Sleepiness = 0;
	public float Misbehaving = MAX_MISBEHAVING; 

	public bool IsDead = false;
	public bool IsEvolving = false;
	public bool IsSleeping = false;

	public bool IsDirty = false;
	public bool IsSick = false;


	public TamagotchiStats() { }

	public void SetTamagotchi(Tamagotchi tamagotchi)
	{
		this.Base = tamagotchi;
		TamagotchiIndex = tamagotchi.Index;
	}

	public void Tick()
	{
		Age++;
		if (IsSleeping)
		{
			Sleepiness--;
		}
		else
		{
			Sleepiness++;
		}

		Hunger = Mathf.Clamp(Hunger - Base.HungerRate, 0, MAX_HUNGER);
		Happiness = Mathf.Clamp(Happiness - Base.HappinessRate, 0, MAX_HAPPINESS);
		// TODO: adjust this rate by discipline.
		Misbehaving = Mathf.Clamp(Misbehaving - Base.MisbehavingRate, 0, MAX_MISBEHAVING);
	}
}
