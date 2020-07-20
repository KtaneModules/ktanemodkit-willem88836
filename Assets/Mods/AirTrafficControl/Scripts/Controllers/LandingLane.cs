using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMAirTrafficControl
{
	public class LandingLane : MonoBehaviour
	{
		// The data state of this landing lane (hangar, shuttle, luggage). 
		public int State { get; private set; }
		public bool ContainsPlane { get; private set; }
		public bool IsWrecked { get; private set; }


		[SerializeField] private LinearAnimator planeAnimator;
		[SerializeField] private LuggageCartFactory luggageFactory;

		[Space, SerializeField] private GameObject[] planePrefabs;
		[SerializeField] private Transform planeContainer;

		[Space, SerializeField] private Color32 selectedColor;
		[SerializeField] private Color32 unselectedColor;
		[SerializeField] private Image image;

		[Space, SerializeField] private int animatorEndNode;
		[SerializeField] private float crashDistance;


		private AirTrafficControl airTrafficControl;
		private LandingLane[] allLanes;
		private int laneIndex;

		private int hangar;
		private int shuttleCar;
		private int luggageCar;

		private Tuple<PlaneData, Transform> incomingPlane 
			= new Tuple<PlaneData, Transform>(null, null);
		private Tuple<PlaneData, Transform> occupyingPlane 
			= new Tuple<PlaneData, Transform>(null, null);
		private Tuple<PlaneData,Transform> departingPlane
			= new Tuple<PlaneData, Transform>(null, null);


		private void Update()
		{
			if (TestCrash(incomingPlane.B, occupyingPlane.B)
			|| TestCrash(occupyingPlane.B, departingPlane.B)
			|| TestCrash(departingPlane.B, incomingPlane.B))
			{
				Debug.Log("Crash on lane" + laneIndex);

				IsWrecked = true;
				planeAnimator.Remove(incomingPlane.B);
				planeAnimator.Remove(occupyingPlane.B);
				planeAnimator.Remove(departingPlane.B);

				airTrafficControl.OnPlaneCrash();
			}
		}

		private bool TestCrash(Transform planeA, Transform planeB)
		{
			if (planeA == null || planeB == null)
			{
				return false;
			}

			float delta = (planeA.position
				- planeB.position).magnitude;

			return delta < crashDistance;
		}


		public void Intialize(int laneIndex, LandingLane[] lanes, AirTrafficControl airTrafficControl)
		{
			this.allLanes = lanes;
			this.laneIndex = laneIndex;
			this.airTrafficControl = airTrafficControl;
		}


		public void Incoming(PlaneData plane)
		{
			incomingPlane.A = plane;

			GameObject nextPlane = planePrefabs.PickRandom();
			GameObject planeObject = Instantiate(nextPlane, planeContainer);
			planeObject.name = "Plane - " + incomingPlane.A.Serial;
			planeObject.transform.position = Positions.FarAway;

			incomingPlane.B = planeObject.transform;

			Action onComplete = delegate
			{
				occupyingPlane.A = incomingPlane.A;
				occupyingPlane.B = incomingPlane.B;
				incomingPlane.A = null;
				incomingPlane.B = null;

				Debug.LogFormat("Answers for lane {3}: (Hangar: {0}, Shuttle: {1}, Luggage: {2})", 
					CalculateCorrectHangar(), 
					CalculateCorrectShuttle(), 
					CalculateCorrectLuggage(), 
					laneIndex);
			};

			planeAnimator.Animate(planeObject.transform, 0, animatorEndNode, onComplete);
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
				Launch();
			}
		}

		public void Launch()
		{
			Action onLuggageComplete = delegate
			{
				Debug.Log("Starting launch");
				State = 0;
				Action onLaunchComplete = delegate
				{
					Debug.Log("Launch Completed!");
					departingPlane.A = null;
					departingPlane.B = null;
				};

				departingPlane.A = occupyingPlane.A;
				departingPlane.B = occupyingPlane.B;
				occupyingPlane.A = null;
				occupyingPlane.B = null;

				planeAnimator.Animate(departingPlane.B, animatorEndNode, -1, onLaunchComplete);
			};

			luggageFactory.CreateLuggageCart(luggageCar, onLuggageComplete);
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
			return AirTrafficControlData.OriginSerialCrossTable[occupyingPlane.A.SerialIndex, occupyingPlane.A.OriginIndex];
		}
	
		private int CalculateCorrectShuttle()
		{
			PlaneData data = occupyingPlane.A;

			// 7) However, if the plane's origin is {0}, {1}, {2}, 
			// then ignore all rules above. 
			if (data.OriginIndex == 6 
				|| data.OriginIndex == 8 
				|| data.OriginIndex == 23)
			{
				return 5;
			}

			// 1) If one of the plane's serial letters is contained in its origin's.
			string serialNumber = data.Serial;
			char l1 = serialNumber[0];
			char l2 = serialNumber[1];
			char l3 = serialNumber[2] == '-'
				? serialNumber[3]
				: serialNumber[2];

			string origin = data.Origin;
			if (origin.Contains(l1.ToString())
				|| origin.Contains(l2.ToString())
				|| origin.Contains(l3.ToString()))
			{
				return 3;
			}

			// 2) Alternatively, if the plane's passenger and 
			// luggage count are both even. 
			if (data.PassengerCount % 2 == 0
				&& data.LuggageCount % 2 == 0)
			{
				return 2;
			}

			// 3) Alternatively, if the origin's name consists of multiple 
			// words, or the plane has more luggage than passengers. 
			string[] split = origin.Split(
				new char[] { ' ' }, 
				StringSplitOptions.RemoveEmptyEntries);

			if(split.Length > 1)
			{
				return 4;
			}

			// 4) Alternatively, if the plane's serial number's first letter is 
			// in the last fourth of the alphabet.
			int ax = StringManipulation.AlphabetToIntPlusOne(serialNumber[0]);
			if(ax > 18)
			{
				return 0;
			}

			// 5) Alternatively, if the plane came in on lane 3 and it has more than 200 luggage.
			if(laneIndex == 3 && data.LuggageCount > 200)
			{
				return 2;
			}

			// 6) Alternatively, if there is a plane present, departing from, 
			// or currently approaching all landing lanes, and the cumulative 
			// passenger count of their occupying planes is over 800, excluding wrecked lanes.
			int totalPassengers = 0;
			bool allInUse = true;
			foreach(LandingLane lane in allLanes)
			{
				if(lane.occupyingPlane.A == null && !lane.IsWrecked)
				{
					allInUse = false;
				}
				else
				{
					totalPassengers += lane.occupyingPlane.A.PassengerCount;
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
			string serial = occupyingPlane.A.Serial;
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
					(predash * occupyingPlane.A.PassengerCount
						+ postdash * occupyingPlane.A.LuggageCount)
					^ (predash + postdash)
				)
				% AirTrafficControlData.LuggageSerials.Length;

			return correctLuggage;
		}
	}
}
