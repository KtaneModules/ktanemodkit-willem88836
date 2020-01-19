using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KModkit;


[RequireComponent(typeof(KMBombInfo))]
public class PinHoleModule : MonoBehaviour
{
	public int Rows;
	public int Columns;
	public float ButtonSpread;

	public KMSelectable PinholeButton;
	public Transform ButtonContainer;

	KMBombModule bombModule;
	KMBombInfo bombInfo;

	public void Awake()
	{
		bombModule = GetComponent<KMBombModule>();
		bombInfo = GetComponent<KMBombInfo>();
		InstantiateButtons();
	}

	/// <summary>
	///		Spawns all buttons.
	/// </summary>
	private void InstantiateButtons()
	{
		KMSelectable selectableSelf = GetComponent<KMSelectable>();

		selectableSelf.Children = new KMSelectable[Rows * Columns - 4];

		int k = 0;
		for (int i = 0; i < Rows; i++)
		{
			for (int j = 0; j < Columns; j++)
			{
				if ((i == 0 || i == Rows - 1) && (j == 0 || j == Columns - 1))
					continue;

				KMSelectable newButton = Instantiate(PinholeButton, ButtonContainer, false);
				newButton.transform.position = ButtonContainer.transform.position + (new Vector3(j + 0.5f - Columns / 2f, 0, i + 0.5f - Rows / 2f) * ButtonSpread);
				newButton.name = string.Format("pinholebutton_{0}_{1}", i, j);
				newButton.Parent = gameObject.GetComponent<KMSelectable>();


				Truple<int, int, int> t = new Truple<int, int, int>(i, j, k);
				newButton.OnInteract += delegate { return OnButtonSelected(t); };

				selectableSelf.Children[k] = newButton;
				k++;
			}
		}
	}


	private bool OnButtonSelected(Truple<int, int, int> t)
	{
		Debug.Log(string.Format("Selected_{0}_{1}_{2}", t.A, t.B, t.C));

		string serial = KMBombInfoExtensions.GetSerialNumber(bombInfo);

		int formattedTime = int.Parse(bombInfo.GetFormattedTime().Substring(3, 2));
		int batteries = KMBombInfoExtensions.GetBatteryCount(bombInfo);
		int batteryHolders = KMBombInfoExtensions.GetBatteryHolderCount(bombInfo);
		int moduleCount = bombInfo.GetModuleNames().Count();

		HandleRequest((int)t.C, serial, formattedTime, batteries, batteryHolders, moduleCount);

		return false;
	}


	private void HandleRequest(
		int s, 
		string serial, 
		int seconds, 
		int batteries, 
		int batteryHolders, 
		int moduleCount)
	{
		/**
		 * Every 5 Seconds the index offset changes of the serial number used for the formula parameters. 
		 * Add the number of modules to each number.
		 * The odd-indexed numbers are multiplied by the number of batteries (min = 1).
		 * The even-index numbers are multiplied by the number of battery holders (min = 1).
		 * Take the MSI of this number (%10).
		 * 
		 * Take the outcome through the following formula: 
		 */

		int[] enumeratedSerial = EnumerateSerial(serial);
		int variableOffSet = seconds / 5;
		Debug.Log(string.Format("t={0}, o={1}", seconds, variableOffSet));
		for (int i = 0; i < enumeratedSerial.Length; i++)
		{
			int j = (i + variableOffSet) % enumeratedSerial.Length;
			enumeratedSerial[j] += j % 2 == 0 ? batteries : batteryHolders;
			enumeratedSerial[j] += moduleCount;
			enumeratedSerial[j] %= 10;
		}

		int u = enumeratedSerial[(variableOffSet) % enumeratedSerial.Length];
		int v = enumeratedSerial[(1 + variableOffSet) % enumeratedSerial.Length];
		int w = enumeratedSerial[(2 + variableOffSet) % enumeratedSerial.Length];
		int x = enumeratedSerial[(3 + variableOffSet) % enumeratedSerial.Length];
		int y = enumeratedSerial[(4 + variableOffSet) % enumeratedSerial.Length];
		int z = enumeratedSerial[(5 + variableOffSet) % enumeratedSerial.Length];

		int A = Mathf.Abs(u * v - (w > x ? y : z) * x) % (Columns * Rows - 4);

		Debug.Log(string.Format("{0} = {1} * {2} - ({3} > {4} ? {5} : {6}) * {4}", A, u, v, w, x, y, z));

		if (s == A)
		{
			bombModule.HandlePass();
		}
		else
		{
			bombModule.HandleStrike();
		}
	}



	string[] alphabet = new string[26] {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};

	public int[] EnumerateSerial(string serial)
	{
		int[] enumeratedSerial = new int[serial.Length];

		for (int i = 0; i < serial.Length; i++)
		{
			string c = serial[i].ToString();
			int j;

			if (!int.TryParse(c, out j))
			{
				j = alphabet.IndexOf((a) => a == c.ToLower()) + 1;
			}

			enumeratedSerial[i] = j;
		}

		return enumeratedSerial;
	}
}