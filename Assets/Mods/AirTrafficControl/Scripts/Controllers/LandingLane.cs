using System;
using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMAirTrafficControl
{
	public class LandingLane : MonoBehaviour
	{
		// The data state of this landing lane (hangar, shuttle, luggage). 
		public int State { get; private set; }
		public bool ContainsPlane { get { return occupyingPlane.A != null; } }
		public bool IsWrecked { get; private set; }


		[SerializeField] private LinearAnimator planeAnimator;
		[SerializeField] private TrainCartFactory luggageFactory;
		[SerializeField] private TrainCartFactory shuttleFactory;

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
		private bool moduleEnded;

		private int hangar;
		private int shuttleCar;
		private int luggageCar;

		private Tuple<PlaneData, Transform> incomingPlane 
			= new Tuple<PlaneData, Transform>(null, null);
		private Tuple<PlaneData, Transform> occupyingPlane 
			= new Tuple<PlaneData, Transform>(null, null);
		private Tuple<PlaneData,Transform> departingPlane
			= new Tuple<PlaneData, Transform>(null, null);

		private string status = "";

		private void Update()
		{
			status = "";
			status += incomingPlane.A == null
				? "Incoming: null, "
				: string.Format("Incoming: {0}, ", incomingPlane.A.Serial);
			status += occupyingPlane.A == null
				? "Occupying: null, "
				: string.Format("Occupying: {0}, ", occupyingPlane.A.Serial);
			status += departingPlane.A == null
				? "Departing: null, "
				: string.Format("Departing: {0}, ", departingPlane.A.Serial);



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
			State = 0;
		}


		public void Incoming(PlaneData plane)
		{
			GameObject planeObject = Instantiate(
				planePrefabs.PickRandom(), 
				Positions.FarAway,
				Quaternion.identity,
				planeContainer);

			planeObject.name = "Plane - " + plane.Serial;

			incomingPlane.A = plane;
			incomingPlane.B = planeObject.transform;

			Action onComplete = delegate
			{
				occupyingPlane.A = incomingPlane.A;
				occupyingPlane.B = incomingPlane.B;
				incomingPlane.A = null;
				incomingPlane.B = null;

				int correctHangar = CalculateCorrectHangar();
				int correctShuttle = CalculateCorrectShuttle();
				int correctLuggage = CalculateCorrectLuggage();

				Debug.LogFormat("Answers for lane {3}: (Hangar: {0}, Shuttle: {1}, Luggage: {2})",
					correctHangar,
					correctShuttle,
					correctLuggage, 
					laneIndex);

				if (moduleEnded)
				{
					hangar = correctHangar;
					shuttleCar = correctShuttle;
					luggageCar = correctLuggage;

					Launch();
				}
			};

			planeAnimator.Animate(planeObject.transform, 0, animatorEndNode, onComplete);
		}

		public void ModuleEnded()
		{
			moduleEnded = true;
		}

		public void Select()
		{
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
			if (IsCorrectSelection())
			{
				CorrectLaunch();
			}
			else
			{
				IncorrectLaunch();
			}
		}

		private void IncorrectLaunch()
		{
			int c = 0;
			Action onComplete = delegate
			{
				c += 1;
				if (c == 2)
				{
					CorrectLaunch();
				};
			};

			Debug.LogFormat("Incorrect Launch Started: {0}", occupyingPlane.A.Serial);
			luggageFactory.CreateTrain(luggageCar, onComplete);
			shuttleFactory.CreateTrain(shuttleCar, onComplete);
		}

		private void CorrectLaunch()
		{
			Action onLuggageComplete = delegate
			{
				State = 0;
				Action onLaunchComplete = delegate
				{
					Debug.LogFormat("Launch Completed: {0}", departingPlane.A.Serial);
					Destroy(departingPlane.B.gameObject);
					departingPlane.A = null;
					departingPlane.B = null;
				};

				departingPlane.A = occupyingPlane.A;
				departingPlane.B = occupyingPlane.B;
				occupyingPlane.A = null;
				occupyingPlane.B = null;

				departingPlane.B.SetAsLastSibling();
				planeAnimator.Animate(departingPlane.B, animatorEndNode, -1, onLaunchComplete);

				Debug.LogFormat("Starting launch: {0}", departingPlane.A.Serial);
			};

			luggageFactory.CreateTrain(luggageCar, null);
			shuttleFactory.CreateTrain(shuttleCar, onLuggageComplete);
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
				|| data.OriginIndex == 13)
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

			// 5) Alternatively, if the plane came in on lane 3 and it has more than 20 luggage.
			if(laneIndex == 3 && data.LuggageCount > 20)
			{
				return 2;
			}

			// 6) Alternatively, if there is a plane present, departing from, 
			// or currently approaching all landing lanes, and the cumulative 
			// passenger count of their occupying planes is over 80, excluding wrecked lanes.
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

			if(allInUse && totalPassengers >= 80)
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
					Mathf.Abs(predash * occupyingPlane.A.PassengerCount
						- postdash * occupyingPlane.A.LuggageCount)
					^ (predash * postdash)
				)
				% AirTrafficControlData.LuggageSerials.Length;

			return correctLuggage;
		}
	}
}
