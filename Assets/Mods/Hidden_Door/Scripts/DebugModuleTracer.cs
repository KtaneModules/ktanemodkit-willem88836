using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugModuleTracer : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		StartCoroutine(DelayedStart());
	}

	private IEnumerator<WaitForSeconds> DelayedStart()
	{
		yield return new WaitForSeconds(0.5f);
		//Camera.main.transform.position = GameObject.Find("Hidden_Door_Module_2").transform.position + Vector3.up * 0.15f;
	}
}
