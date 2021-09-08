using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RMMondrian
{
	[RequireComponent(typeof(KMBombModule))]
	public class RMMondrian : MonoBehaviour
	{
		[SerializeField] private ScreenSwap screenSwap;
		[SerializeField] private Canvas mondrianCanvas;

		[SerializeField] private Transform goalTileParent;
		[SerializeField] private GameObject goalTile;
		[SerializeField] private Transform puzzleTileParent;
		[SerializeField] private GameObject puzzleTile;

		[Space]
		[SerializeField] private Image colorDisplay; 

		[Space]
		[SerializeField] private int resolution; 
		[SerializeField] private int maxTileWidth;
		[SerializeField] private int maxTileHeight;
		[SerializeField] private int clicks;
		[SerializeField] private Color32[] colors;

		private KMBombModule bombModule;
		private int currentColor; 


		public void Start()
        {
			bombModule = GetComponent<KMBombModule>();
			List<MondrianTile> tiles = GenerateMondrian();
			PaintMondrian(tiles);
        }

		private List<MondrianTile> GenerateMondrian()
        {
			RectTransform rect = mondrianCanvas.GetComponent<RectTransform>();
			Vector2 size = rect.sizeDelta;

			int columns = Mathf.FloorToInt(size.x / resolution);
			int rows = Mathf.FloorToInt(size.y / resolution);

			List<MondrianTile> tiles = new List<MondrianTile>();
			Queue<MondrianTile> work = new Queue<MondrianTile>();
			work.Enqueue(new MondrianTile());

			while(work.Count > 0)
            {
				MondrianTile tile = work.Dequeue();

				int maxZ = columns - tile.CoordinateX;
				int maxW = rows - tile.CoordinateY;

				// checks if it needs to consider other tiles. 
				foreach(MondrianTile other in tiles)
                {
					if (other.CoordinateY <= tile.CoordinateY && other.CoordinateY + other.Height > tile.CoordinateY && other.CoordinateX >= tile.CoordinateX)
						maxZ = Mathf.Min(maxZ, other.CoordinateX - tile.CoordinateX);

					if (other.CoordinateX <= tile.CoordinateX && other.CoordinateX + other.Width > tile.CoordinateX && other.CoordinateY >= tile.CoordinateY)
						maxW = Mathf.Min(maxW, other.CoordinateY - tile.CoordinateY);
                }

				if (maxZ == 0 || maxW == 0)
					continue;

				// generates tile width and height
				tile.Width = Random.Range(1, Mathf.Min(maxZ, maxTileWidth));
				tile.Height = Random.Range(1, Mathf.Min(maxW, maxTileHeight));

				//determines neighboring tiles
				foreach(MondrianTile other in tiles)
                {
					if (AreNeighbours(tile, other))
                    {
						tile.neighbours.Add(other);
						other.neighbours.Add(tile);
                    }
                }

				// adds tile. 
				tiles.Add(tile);
				InstantiateTiles(tile, columns, rows);

				// adds new work
				if (tile.CoordinateX + tile.Width < columns && tile.Width != maxZ)
					work.Enqueue(new MondrianTile(tile.CoordinateX + tile.Width, tile.CoordinateY, 0, 0));

				if (tile.CoordinateY + tile.Height < rows && tile.Height != maxW)
					work.Enqueue(new MondrianTile(tile.CoordinateX, tile.CoordinateY + tile.Height, 0, 0));
            }

			return tiles;
        }

		private void InstantiateTiles(MondrianTile tile, int columns, int rows)
		{
			Vector2 rectMin = new Vector2(tile.CoordinateX / (float)columns, tile.CoordinateY / (float)rows);
			Vector2 rectMax = new Vector2((tile.CoordinateX + tile.Width) / (float)columns, (tile.CoordinateY + tile.Height) / (float)rows);

			GameObject newPuzzleTile = Instantiate(puzzleTile, puzzleTileParent);
			RectTransform prect = newPuzzleTile.GetComponent<RectTransform>();
			prect.anchorMin = rectMin;
			prect.anchorMax = rectMax;
			newPuzzleTile.SetActive(true);
			tile.PuzzleTile = newPuzzleTile.GetComponent<Image>();

			GameObject newGoalTile = Instantiate(goalTile, goalTileParent);
			RectTransform grect = newGoalTile.GetComponent<RectTransform>();
			grect.anchorMin = rectMin;
			grect.anchorMax = rectMax;
			newGoalTile.SetActive(true);
			tile.GoalTile = newGoalTile.GetComponent<Image>();
		}

		private bool AreNeighbours(MondrianTile tileA, MondrianTile tileB)
        {
			return (tileA.CoordinateX + tileA.Width == tileB.CoordinateX && tileA.CoordinateY + tileA.Height >= tileB.CoordinateY && tileA.CoordinateY <= tileB.CoordinateY + tileB.Height)
				|| (tileB.CoordinateX + tileB.Width == tileA.CoordinateX && tileB.CoordinateY + tileB.Height >= tileA.CoordinateY && tileB.CoordinateY <= tileA.CoordinateY + tileA.Height)
				|| (tileA.CoordinateY + tileA.Height == tileB.CoordinateY && tileA.CoordinateX + tileA.Width >= tileB.CoordinateX && tileA.CoordinateX <= tileB.CoordinateX + tileB.Width)
				|| (tileB.CoordinateY + tileB.Height == tileA.CoordinateY && tileB.CoordinateX + tileB.Width >= tileA.CoordinateX && tileB.CoordinateX <= tileA.CoordinateX + tileA.Width); 
		}

		private void PaintMondrian(List<MondrianTile> tiles)
		{
			currentColor = 0;
			for (int i = 0; i < clicks; i++)
            {
				int r = Random.Range(0, tiles.Count);
				MondrianTile tile = tiles[r];
				tile.Click(colors, currentColor, true);
				currentColor = (currentColor + 1) % colors.Length;
				Debug.Log("[Mondrian] Click (" + i + ") is tile (" + r + ").");
            }
			currentColor = 0;
			UpdateColorDisplay();
		}

		public void PaintNext(MondrianTile tile)
        {
			tile.Click(colors, currentColor, false);
			currentColor = (currentColor + 1) % colors.Length;
			UpdateColorDisplay();
		}

		private void UpdateColorDisplay()
        {
			colorDisplay.color = colors[currentColor];
        }

		public void ResetModule()
        {

        }
	}
}
