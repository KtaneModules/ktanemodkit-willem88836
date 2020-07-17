using UnityEngine;

[System.Serializable]
public class HanoianButtonManager
{
	[SerializeField] private HanoianButton[] buttons;
	[SerializeField] private Material[] buttonStates;

	private HanoianCompounds hanoianCompounds;

	public void Initialize(HanoianCompounds hanoianCompounds)
	{
		this.hanoianCompounds = hanoianCompounds;

		for (int i = 0; i < buttons.Length; i++)
		{
			HanoianButton button = buttons[i];
			button.Set(this, i, buttonStates[0], buttonStates[1], buttonStates[2]);
			button.OnUnclick();
		}
	}

	public void Unclick(int i)
	{
		if (i > 0 && i < buttons.Length)
		{
			buttons[i].OnUnclick();
		}
	}
	public void Click(int i)
	{
		if (i > 0 && i < buttons.Length)
		{
			buttons[i].OnClick();
		}
	}
	public void WrongClick(int i)
	{
		if (i > 0 && i < buttons.Length)
		{
			buttons[i].OnWrongClick();
		}
	}

	public void UnclickAll()
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			Unclick(i);
		}
	}
	public void ClickAll()
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			Click(i);
		}
	}
	public void WrongClickAll()
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			WrongClick(i);
		}
	}

	public void UnclickAll(params int[] indices)
	{
		foreach (int i in indices)
		{
			Unclick(i);
		}
	}
	public void ClickAll(params int[] indices)
	{
		foreach (int i in indices)
		{
			Click(i);
		}
	}
	public void WrongClickAll(params int[] indices)
	{
		foreach (int i in indices)
		{
			WrongClick(i);
		}
	}

	public void OnButtonClicked(int i)
	{
		hanoianCompounds.OnButtonClicked(i);
	}
}
