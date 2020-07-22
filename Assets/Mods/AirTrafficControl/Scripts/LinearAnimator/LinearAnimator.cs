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

		#if UNITY_EDITOR
		[SerializeField] private Color32 debugColor;
		[SerializeField] private bool closeDebugLoop;
		#endif

		private List<LinearAnimationNode> nodes;
		private List<Actor> liveActors = new List<Actor>();


		private void OnValidate()
		{
			Initialize();
		}

		private void Awake()
		{
			Initialize();
		}

		private void Initialize()
		{
			nodes = new List<LinearAnimationNode>();
			int c = transform.childCount;
			for (int i = 0; i < c; i++)
			{
				Transform child = transform.GetChild(i);
				LinearAnimationNode node = child.GetComponent<LinearAnimationNode>();
				if (node != null)
				{
					nodes.Add(node);
				}
			}
		}

		private void Update()
		{
			#if UNITY_EDITOR
			// Shows the nodes in the editor.
			for(int i = 0; i < nodes.Count; i++)
			{
				if (i == nodes.Count - 1 && !closeDebugLoop)
					continue;

				int j = (i + 1) % nodes.Count;

				LinearAnimationNode current = nodes[i];

				Debug.DrawLine(current.Position, nodes[j].Position, debugColor);
				//Debug.DrawLine(current.Position, current.Position + current.Rotation.eulerAngles, Color.blue);
			}
			#endif


			for (int i = liveActors.Count - 1; i >= 0; i--)
			{
				Actor actor = liveActors[i];

				// the start and end nodes are selected.
				LinearAnimationNode start = nodes[actor.current];
				LinearAnimationNode end = nodes[actor.current + 1];

				// the start's animationcurve is used for progressing.
				float a = actor.alpha - start.Delay;
				float positionAlpha = start.PositionCurve.Evaluate(a);
				float rotationAlpha = start.RotationCurve.Evaluate(a);
				float scaleAlpha = start.ScaleCurve.Evaluate(a);

				// interpolating values based on evaluated alpha.
				Vector3 position = Vector3.Lerp(start.Position, end.Position, positionAlpha);
				Quaternion rotation = Quaternion.Lerp(start.Rotation, end.Rotation, rotationAlpha);
				Vector3 scale = Vector3.Lerp(start.Scale, end.Scale, scaleAlpha);

				// sets values.
				actor.actor.position = position;
				actor.actor.rotation = rotation;
				actor.actor.localScale = scale;

				// the alpha is increased. the speed is altered by the 
				// distance between the current and next node. 
				actor.alpha = actor.alpha 
					+ (progressionSpeed / (end.Position - start.Position).magnitude)		
					* Time.deltaTime;


				// if the progression exceeds 1 it's either at the 
				// next node, or the end of the sequence. 
				if (actor.alpha - start.Delay >= 1)
				{
					// when at the next node, the next node is selected.
					if (actor.current < actor.end - 1)
					{
						actor.alpha = 0;
						actor.current += 1;
					}
					// when the end is reached, the node is removed, and 
					// its owner notified. 
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

				// the alpha doesn't update unless you set the value again. 
				// is this a bug? 
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

		public void Remove(Transform actor)
		{
			int i = liveActors.IndexOf((Actor a) => { return a.actor == actor; });
			if(i >= 0)
			{
				liveActors.RemoveAt(i);
			}
		}
	}
}
