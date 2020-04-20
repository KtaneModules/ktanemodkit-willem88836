using KModkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class HanoianButton : MonoBehaviour {

	public Renderer lightRenderer;

	private HanoianCompounds parent;
	private int index;
	private Material clicked;
	private Material unclicked;
	private Material wrongClicked; 


	public void Start () {
		KMSelectable selectable = GetComponent<KMSelectable>();
		selectable.OnInteract += OnInteract;
	}


	public void Set(
		HanoianCompounds parent, 
		int index,
		Material clicked, 
		Material unclicked, 
		Material wrongClicked)
	{
		this.parent = parent;
		this.index = index;
		this.clicked = clicked;
		this.unclicked = unclicked;
		this.wrongClicked = wrongClicked;
	}


	private bool OnInteract()
	{
		parent.OnButtonClicked(index);
		return true;
	}


	public void OnClick()
	{
		lightRenderer.material = clicked;
	}

	public void OnUnclick()
	{
		lightRenderer.material = unclicked;
	}

	public void OnWrongClick()
	{
		lightRenderer.material = wrongClicked;
	}
}
