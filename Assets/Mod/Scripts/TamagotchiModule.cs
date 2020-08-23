using UnityEngine;


[RequireComponent(typeof(KMBombInfo), typeof(KMAudio), typeof(KMBombModule))]
[RequireComponent(typeof(KMBossModule))]
public class TamagotchiModule : MonoBehaviour 
{
	[SerializeField] private float completionAlpha;
	[SerializeField] private BombButton[] buttons;

	private KMBombInfo bombInfo;
	private KMAudio bombAudio;
	private KMBombModule bombModule;
	private KMBossModule bossModule;


	private void Start () 
	{
		// Sets up basic references.
		bombInfo = GetComponent<KMBombInfo>();
		bombAudio = GetComponent<KMAudio>();
		bombModule = GetComponent<KMBombModule>();
		bossModule = GetComponent<KMBossModule>();

		// Sets up interaction.
		buttons[0].AddListener(OnFoodButtonClicked);
		buttons[1].AddListener(OnMedicineButtonClicked);
		buttons[2].AddListener(OnDisciplineButtonClicked);
		buttons[3].AddListener(OnPlayButtonClicked);
		buttons[4].AddListener(OnButtonLidClicked);
		buttons[5].AddListener(OnCompleteButtonClicked);
	}


	#region ModuleInteraction 

	private void OnFoodButtonClicked()
	{

	}

	private void OnMedicineButtonClicked()
	{

	}

	private void OnDisciplineButtonClicked()
	{

	}

	private void OnPlayButtonClicked()
	{

	}

	private void OnButtonLidClicked()
	{

	}

	private void OnCompleteButtonClicked()
	{

	}

	#endregion
}
