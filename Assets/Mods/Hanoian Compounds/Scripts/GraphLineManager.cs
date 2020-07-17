using UnityEngine;

[System.Serializable]
public class GraphLineManager
{
	[SerializeField] private GraphLine[] graphLines;
	[SerializeField] private float lerpSpeed;
	[SerializeField] private Vector2 heightEdges;



	public int LineCount { get { return graphLines.Length; } }
	
	public int NodeCount 
	{ 
		get 
		{
			int c = int.MaxValue;
			foreach (GraphLine l in graphLines)
			{
				int lc = l.Count;
				if (lc < c)
				{
					c = lc;
				}
			}
			return c; 
		} 
	}

	public void SetMaterial(int i, Material material)
	{
		graphLines[i].SetMaterial(material);
	}

	public void SetValues(int[,] values, int scalar = 1)
	{
		for(int i = 0; i < values.GetLength(0); i++)
		{
			for (int j = 0; j < values.GetLength(1); j++)
			{
				float a = Mathf.Lerp(heightEdges.x, heightEdges.y, (float)values[i, j] / scalar);
				SetValue(i, j, a);
			} 
		}

		Apply();
	}

	public void SetValue(int i, int j, float value)
	{
		graphLines[i].SetValue(j, value);
	}

	public void Apply()
	{
		// Lerp
		foreach (GraphLine line in graphLines)
		{
			line.Apply();
		}
	}
}
