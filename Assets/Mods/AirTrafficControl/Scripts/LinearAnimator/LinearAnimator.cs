using System;
using System.Collections.Generic;
using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
	[ExecuteInEditMode]
	public class LinearAnimator : MonoBehaviour
	{
		[Serializable]
		private struct Actor
		{
			public Transform actor;
			public int start;
			public int end;
			public int current;
			public float alpha;
			public Action onComplete;


			public Actor(Transform actor, int start, int end, float alpha, Action onComplete)
			{
				this.actor = actor;
				this.start = start;
				this.end = end;
				this.current = start;
				this.alpha = alpha;
				this.onComplete = onComplete;
			}
		}


		[SerializeField] private float progressionSpeed;
		[SerializeField] private AnimationCurve progressionCurve;

		private List<Transform> nodes = new List<Transform>();
		private List<Actor> liveActors = new List<Actor>();


		private void Awake()
		{
			int c = transform.childCount;
			for(int i = 0; i < c; i++)
			{
				nodes.Add(transform.GetChild(i));
			}
		}

		private void Update()
		{
			#if UNITY_EDITOR
			for(int i = 0; i < nodes.Count; i++)
			{
				int j = (i + 1) % nodes.Count;

				Transform current = nodes[i];

				Debug.DrawLine(current.position, nodes[j].position, Color.red);
				Debug.DrawLine(current.position, current.position + current.forward, Color.blue);
			}
			#endif


			for (int i = liveActors.Count - 1; i >= 0; i--)
			{
				Actor actor = liveActors[i];

				float b = progressionCurve.Evaluate(actor.alpha);

				Transform start = nodes[actor.current];
				Transform end = nodes[actor.current + 1];

				Vector3 position = Vector3.Lerp(start.position, end.position, b);
				Quaternion rotation = Quaternion.Lerp(start.rotation, end.rotation, b);

				actor.actor.position = position;
				actor.actor.rotation = rotation;

				actor.alpha = actor.alpha + progressionSpeed * Time.deltaTime;

				if (actor.alpha >= 1)
				{
					if (actor.current < actor.end - 1)
					{
						actor.alpha -= 1;
						actor.current += 1;
					}
					else
					{
						liveActors.RemoveAt(i);

						if(actor.onComplete != null)
						{
							actor.onComplete.Invoke();
						}

						continue;
					}
				}

				liveActors[i] = actor;
			}
		}


		public void Animate(Transform actor, int start, int end, Action onComplete)
		{
			if (end == -1)
			{
				end = nodes.Count - 1;
			}

			Actor newActor = new Actor(actor, start, end, 0, onComplete);
			liveActors.Add(newActor);
		}
	}
}
