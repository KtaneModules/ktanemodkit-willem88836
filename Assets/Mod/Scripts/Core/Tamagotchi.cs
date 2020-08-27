using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tamagotchi")]
public class Tamagotchi : ScriptableObject
{
	[NonSerialized] public int Index;

	public Texture Texture;
	public string Name;

	public int LifeSpan;
	public int AgeStage;

	public bool SleepsIn;
	public bool ResetStatsOnCreation;

	public int HungerRate;
	public int HappinessRate;
	public int HealthRate;
	public int MisbehavingRate;
}
