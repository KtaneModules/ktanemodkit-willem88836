using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
	public class LinearAnimationNode : MonoBehaviour
	{
		public AnimationCurve ProgressionCurve;
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
