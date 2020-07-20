using System;
using System.Collections.Generic;
using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
	public class LuggageCartFactory : MonoBehaviour
	{
		[SerializeField] private LinearAnimator animator;
		
		[Space, SerializeField] private GameObject cartCarPrefab;
		[SerializeField] private GameObject[] cartPrefabs;
		[SerializeField] private Transform cartContainer;
		[SerializeField] private Transform spawnLocation;
		
		[Space, SerializeField] private int maxCarts;

		[Space, SerializeField] private float followSpeed;
		[SerializeField] private float minCartDistance;


		private Transform car;
		private List<Transform> carts = new List<Transform>();


		private void Update()
		{
			if (carts.Count == 0)
			{
				return;
			}

			ApproachPreceding(car, carts[0]);
			for (int i = 1; i < carts.Count; i++)
			{
				ApproachPreceding(carts[i - 1], carts[i]);
			}
		}

		private void ApproachPreceding(Transform p, Transform f)
		{
			float delta = (p.position - f.position).magnitude;

			if(delta < minCartDistance)
			{
				return;
			}

			Vector3 newPosition = Vector3.Lerp(f.position, p.position, followSpeed);
			Quaternion newRotation = Quaternion.Lerp(f.rotation, p.rotation, followSpeed);

			f.position = newPosition;
			f.rotation = newRotation;
		}


		public void CreateLuggageCart(int type, Action onComplete)
		{
			onComplete += StopFolow;

			carts.Clear();
			System.Random random = new System.Random(type);

			int c = random.Next(0, maxCarts);

			GameObject cart = Instantiate(cartCarPrefab, cartContainer);
			car = cart.transform;
			car.transform.position = spawnLocation.position;
			animator.Animate(car, 0, -1, onComplete);

			for (int i = 0; i < c; i++)
			{
				int j = random.Next(0, cartPrefabs.Length);
				GameObject nextPrefab = cartPrefabs[j];
				GameObject spawn = Instantiate(nextPrefab, cartContainer);
				spawn.transform.position = spawnLocation.position;
				carts.Add(spawn.transform);
			}
		}

		private void StopFolow()
		{
			for(int i = 0; i < carts.Count; i++)
			{
				Destroy(carts[i].gameObject);
			}

			Destroy(car.gameObject);

			carts.Clear();
			car = null;
		}
	}
}
