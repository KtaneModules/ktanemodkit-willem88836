using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
	public class LinearAnimationNode : MonoBehaviour
	{
		public AnimationCurve PositionCurve = AnimationCurve.Linear(0,0,1,1);
		public AnimationCurve ScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
		public AnimationCurve RotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
		public float Delay;

		public Vector3 Position
		{
			get { return transform.position; }
		}

		public Quaternion Rotation
		{
			get { return transform.rotation; }
		}

		public Vector3 Scale
		{
			get { return transform.localScale; }
		}
	}
}
