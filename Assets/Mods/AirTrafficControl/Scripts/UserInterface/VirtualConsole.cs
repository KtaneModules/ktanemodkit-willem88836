using UnityEngine;

namespace WillemMeijer.NMTechSupport
{
	public class VirtualConsole : MonoBehaviour
	{
		public VirtualTextElement[] elements;



		public void Start() 
		{
			foreach(VirtualTextElement element in elements)
			{
				element.SetText("dfkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjj");
			}
		}


	}
}
