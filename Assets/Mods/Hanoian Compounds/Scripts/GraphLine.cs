using UnityEngine;

[System.Serializable]
public class GraphLine : MonoBehaviour
{
	[SerializeField] private LineRenderer graphLine;
	[SerializeField] private LineRenderer legendLine;
	[SerializeField] private TextMesh legendName;

	private Vector3[] positions;

	public int Count { get { return positions.Length; } }

	public void Start()
	{
		positions = new Vector3[graphLine.positionCount];
		graphLine.GetPositions(positions);
	}

	public void SetMaterial(Material material)
	{
		graphLine.material = material;
		legendLine.material = material;
		legendName.color = material.color;
	}

	public void SetValue(int i, float value)
	{
		positions[i].z = value;
	}

	public void Apply()
	{
		graphLine.SetPositions(positions);
	}
}