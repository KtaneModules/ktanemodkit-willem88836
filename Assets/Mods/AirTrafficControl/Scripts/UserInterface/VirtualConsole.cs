using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WillemMeijer.NMTechSupport
{
	public class VirtualConsole : MonoBehaviour
	{
		public Text ConsoleText;
		public int messageCount;

		private List<Tuple<string, int>> messages;


		public void Start() 
		{
			messages = new List<Tuple<string, int>>();
			Clear();
		}


		public int Show(string message)
		{
			Tuple<string, int> tupleMeHa = new Tuple<string, int>(message, message.GetHashCode());
			messages.Add(tupleMeHa);
			UpdateConsole();
			return tupleMeHa.B;
		}

		public void Remove(int hash)
		{
			for(int i = messages.Count; i >= 0; i--)
			{
				Tuple<string, int> message = messages[i];
				if (message.B == hash)
				{
					messages.RemoveAt(i);
					UpdateConsole();
					return;
				}
			}

			Debug.LogWarning("Couldn't find message with hash: " + hash);
		}

		public int Replace(int hash, string message)
		{
			for(int i = messages.Count; i >= 0; i--)
			{
				Tuple<string, int> current = messages[i];
				if(current.B == hash)
				{
					current.A = message;
					current.B = message.GetHashCode();
					messages[i] = current;
					UpdateConsole();
					return current.B;
				}
			}

			Debug.LogWarning("Couldn't find message with hash: " + hash);
			return -1;
		}


		private void UpdateConsole()
		{
			string o = "";

			int low = Mathf.Max(0, messages.Count - messageCount);

			for(int i = messages.Count - 1; i >= low; i--)
			{
				string message = messages[i].A;
				o += message + "\n";
			}

			ConsoleText.text = o;
		}
	

		private void Clear()
		{
			messages.Clear();
			UpdateConsole();
		}
	}
}
