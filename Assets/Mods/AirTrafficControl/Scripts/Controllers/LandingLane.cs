using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMAirTrafficControl
{
	public class LandingLane : MonoBehaviour
	{
		public int State { get; private set; }
		public bool ContainsPlane { get; private set; }

		[SerializeField] private int incomingPlaneDelay;

		[SerializeField] private GameObject[] planePrefabs;
		[SerializeField] private Transform planeContainer;
		[SerializeField] private LinearAnimator animator;
		[SerializeField] private Color32 selectedColor;
		[SerializeField] private Color32 unselectedColor;
		[SerializeField] private Image image;

		private LandingLane[] allLanes;
		private int laneIndex;

		private int hangar;
		private int shuttleCar;
		private int luggageCar;

		private PlaneData incomingPlane;
		private PlaneData occupyingPlane;


		public void Intialize(int laneIndex, LandingLane[] lanes)
		{
			this.allLanes = lanes;
			this.laneIndex = laneIndex;
		}


		public void Incoming(PlaneData plane)
		{
			incomingPlane = plane;
			//occupyingPlane = plane;

			StartCoroutine(LandPlane());
		}

		private IEnumerator LandPlane()
		{
			yield return new WaitForSeconds(incomingPlaneDelay);
			GameObject nextPlane = planePrefabs.PickRandom();
			GameObject planeObject = Instantiate(nextPlane, planeContainer);
			animator.Animate(planeObject.transform, 0, -1, null); // TODO: add oncomplete.
		}



		public void Select()
		{
			ContainsPlane = true;
			image.color = selectedColor;
		}

		public void Deselect()
		{
			image.color = unselectedColor;
		}

		public void SetNext(int selection)
		{
			switch (State)
			{
				case 0:
					hangar = selection;
					break;
				case 1:
					shuttleCar = selection;
					break;
				case 2:
					luggageCar = selection;
					break;
			}

			State++;

			if(State == 3)
			{
				// Start animations.
			}
		}

		public void Launch()
		{
			State = 0;
			// Test selection.
		}
	
		

		private bool IsCorrectSelection()
		{
			// Hangar Test.
			int correctHangar = CalculateCorrectHangar();
			if (correctHangar != hangar)
			{
				return false;
			}

			// Shuttle Test.
			int correctShuttle = CalculateCorrectShuttle();
			if(correctShuttle != shuttleCar)
			{
				return false;
			}


			// Luggage Test.
			int correctLuggage = CalculateCorrectLuggage();
			if(correctLuggage != luggageCar)
			{
				return false;
			}

			return true;
		}
	
	
		private int CalculateCorrectHangar()
		{
			return AirTrafficControlData.OriginSerialCrossTable[occupyingPlane.SerialIndex, occupyingPlane.OriginIndex];
		}
	
		private int CalculateCorrectShuttle()
		{
			// 7) However, if the plane's origin is {0}, {1}, {2}, then ignore all rules above. 
			if (occupyingPlane.OriginIndex == 6 || occupyingPlane.OriginIndex == 8 || occupyingPlane.OriginIndex == 23)
			{
				return 5;
			}

			// 1) If one of the plane's serial letters is contained in its origin's.
			string serialNumber = occupyingPlane.Serial;
			char l1 = serialNumber[0];
			char l2 = serialNumber[1];
			char l3 = serialNumber[2] == '-'
				? serialNumber[3]
				: serialNumber[2];

			string origin = occupyingPlane.Origin;
			if (origin.Contains(l1.ToString())
				|| origin.Contains(l2.ToString())
				|| origin.Contains(l3.ToString()))
			{
				return 3;
			}

			// 2) Alternatively, if the plane's passenger and luggage count are both even. 
			if (occupyingPlane.PassengerCount % 2 == 0
				&& occupyingPlane.LuggageCount % 2 == 0)
			{
				return 2;
			}

			// 3) Alternatively, if the origin's name consists of multiple words, or the plane has more luggage than passengers. 
			string[] split = origin.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if(split.Length > 1)
			{
				return 4;
			}

			// 4) Alternatively, if the plane's serial number's first letter is in the last fourth of the alphabet.
			int ax = StringManipulation.AlphabetToIntPlusOne(serialNumber[0]);
			if(ax > 18)
			{
				return 0;
			}

			// 5) Alternatively, if the plane came in on lane 3 and it has more than 200 luggage.
			if(laneIndex == 3 && occupyingPlane.LuggageCount > 200)
			{
				return 2;
			}

			// 6) Alternatively, if there is a plane present, departing from, or currently approaching all landing lanes, and their cumulative passenger count is over 800
			int totalPassengers = 0;
			bool allInUse = true;
			foreach(LandingLane lane in allLanes)
			{
				if(lane.occupyingPlane == null)
				{
					allInUse = false;
				}
				else
				{
					totalPassengers += lane.occupyingPlane.PassengerCount;
				}
			}

			if(allInUse && totalPassengers >= 800)
			{
				return 1;
			}

			return 3;
		}
	
		private int CalculateCorrectLuggage()
		{
			string serial = occupyingPlane.Serial;
			int predash = 0;
			int postdash = 0;
			bool isPostDash = false;
			for (int i = 0; i < serial.Length; i++)
			{
				char current = serial[i];

				if (isPostDash)
				{
					int v;

					if (!int.TryParse(current.ToString(), out v))
					{
						v = StringManipulation.AlphabetToIntPlusOne(current);
					}

					postdash += v;
				}
				else
				{
					if (serial[i] == '-')
					{
						isPostDash = true;
						continue;
					}

					int v;

					if (!int.TryParse(current.ToString(), out v))
					{
						v = StringManipulation.AlphabetToIntPlusOne(current);
					}

					predash += v;
				}
			}

			int correctLuggage =
				(
					(predash * occupyingPlane.PassengerCount
						+ postdash * occupyingPlane.LuggageCount)
					^ (predash + postdash)
				)
				% AirTrafficControlData.LuggageSerials.Length;

			return correctLuggage;
		}
	}
}
