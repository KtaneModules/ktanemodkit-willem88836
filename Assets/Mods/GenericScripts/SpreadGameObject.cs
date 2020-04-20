using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpreadGameObject : MonoBehaviour {

	public float count;

	// Update is called once per frame
	void Update () {
		Transform parent = transform.parent;

		if (parent.childCount == 0)
		{
			return;
		}

		Vector3 origin = parent.GetChild(0).position;

		for (int i = 1; i < parent.childCount; i++)
		{
			Transform tr = parent.GetChild(i);
			if (tr == transform)
			{
				return;
			}

			tr.position = Vector3.Lerp(origin, transform.position, i/count);
		}
	}
}
