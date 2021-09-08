using System.Collections.Generic;
using UnityEngine;

namespace RMMondrian
{
	[RequireComponent(typeof(KMBombModule))]
	public class RMMondrian : MonoBehaviour
	{
		[SerializeField] private KMAudio audioSource;
		[SerializeField] private Canvas mondrianCanvas;
		[SerializeField] private GameObjectCache goalTiles;
		[SerializeField] private GameObjectCache puzzleTiles;

		[Space]
		[SerializeField] private SpriteRenderer colorDisplay;
		[SerializeField] private TextMesh clicksLeft;


		[Space]
		[SerializeField] private int resolution; 
		[SerializeField] private int maxTileWidth;
		[SerializeField] private int maxTileHeight;
		[SerializeField] private int clicks;
		[SerializeField] private Color32[] colors;

		private KMBombModule bombModule;
		private int currentColor;
		private List<MondrianTile> activeTiles;
		private int clickCount;
		private bool passed = false;


		public void Start()
        {
			bombModule = GetComponent<KMBombModule>();
			activeTiles = GenerateMondrian();
			PaintMondrian(activeTiles);
			DisableUnusedTiles();
			Debug.Log("[Mondrian] Spawned (" + activeTiles.Count + ") tiles.");
        }

		private List<MondrianTile> GenerateMondrian()
        {
			RectTransform rect = mondrianCanvas.GetComponent<RectTransform>();
			Vector2 size = rect.sizeDelta;

			int columns = Mathf.FloorToInt(size.x / resolution);
			int rows = Mathf.FloorToInt(size.y / resolution);

			List<MondrianTile> tiles = new List<MondrianTile>();
			Queue<MondrianTile> work = new Queue<MondrianTile>();
			work.Enqueue(puzzleTiles.GetNext().GetComponent<MondrianTile>());

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
                {
					puzzleTiles.GiveBack(tile.gameObject);
					continue;
                }

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
				tile.Instantiate(columns, rows, goalTiles.GetNext());

				// adds new work
				if (tile.CoordinateX + tile.Width < columns && tile.Width != maxZ)
				{
					MondrianTile taskTile = puzzleTiles.GetNext().GetComponent<MondrianTile>();
					taskTile.CoordinateX = tile.CoordinateX + tile.Width;
					taskTile.CoordinateY = tile.CoordinateY;
					work.Enqueue(taskTile);
                }

				if (tile.CoordinateY + tile.Height < rows && tile.Height != maxW)
				{
					MondrianTile taskTile = puzzleTiles.GetNext().GetComponent<MondrianTile>();
					taskTile.CoordinateX = tile.CoordinateX;
					taskTile.CoordinateY = tile.CoordinateY + tile.Height;
					work.Enqueue(taskTile);
				}
            }

			return tiles;
        }

		private bool AreNeighbours(MondrianTile tileA, MondrianTile tileB)
        {
			return (tileA.CoordinateX + tileA.Width == tileB.CoordinateX && tileA.CoordinateY + tileA.Height > tileB.CoordinateY && tileA.CoordinateY < tileB.CoordinateY + tileB.Height)
				|| (tileB.CoordinateX + tileB.Width == tileA.CoordinateX && tileB.CoordinateY + tileB.Height > tileA.CoordinateY && tileB.CoordinateY < tileA.CoordinateY + tileA.Height)
				|| (tileA.CoordinateY + tileA.Height == tileB.CoordinateY && tileA.CoordinateX + tileA.Width > tileB.CoordinateX && tileA.CoordinateX < tileB.CoordinateX + tileB.Width)
				|| (tileB.CoordinateY + tileB.Height == tileA.CoordinateY && tileB.CoordinateX + tileB.Width > tileA.CoordinateX && tileB.CoordinateX < tileA.CoordinateX + tileA.Width); 
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
			if (passed)
				return;

			clickCount++;
			tile.Click(colors, currentColor, false);
			currentColor = (currentColor + 1) % colors.Length;
			UpdateColorDisplay();
			CheckCorrectness();
		}

		private void CheckCorrectness()
        {
			bool correct = true;
			foreach(MondrianTile tile in activeTiles)
            {
				if (tile.Color != tile.GoalColor)
                {
					Debug.Log("[Mondrian] Not yet correct");
					correct = false;
					break;
                }
            }

			if (clickCount >= clicks)
            {
				audioSource.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.LightBuzzShort, transform);
			}
			else if (correct)
            {
				Debug.Log("[Mondrian] Correct!");
				passed = true;
				bombModule.HandlePass();
            }
        }

		private void UpdateColorDisplay()
        {
			colorDisplay.color = colors[currentColor];
			clicksLeft.text = (clicks - clickCount).ToString();
		}

		private void DisableUnusedTiles()
        {
			puzzleTiles.DisableUnused();
			goalTiles.DisableUnused();
        }

		public void ResetModule()
		{
			if (passed)
				return;

			bombModule.HandleStrike();
			foreach(MondrianTile tile in activeTiles)
				tile.ClearPuzzle();

			clickCount = 0;

			UpdateColorDisplay();
        }
	}
}
