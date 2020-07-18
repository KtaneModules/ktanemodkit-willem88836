using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMAirTrafficControl
{
	public class LandingLane : MonoBehaviour
	{
		public int State { get; private set; }
		public bool ContainsPlane { get; private set; }

		[SerializeField] private Color32 selectedColor;
		[SerializeField] private Color32 unselectedColor;
		[SerializeField] private Image image;

		private int hangar;
		private int shuttleCar;
		private int luggageCar;


		public void Incoming(PlaneData plane)
		{

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
	}
}
