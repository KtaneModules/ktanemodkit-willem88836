using System;
using System.Collections.Generic;
using UnityEngine;

namespace RMMondrian
{
    [RequireComponent(typeof(RectTransform))]
    public class MondrianTile : MonoBehaviour
    {
        [NonSerialized] public int CoordinateX;
        [NonSerialized] public int CoordinateY;
        [NonSerialized] public int Width;
        [NonSerialized] public int Height;
        [NonSerialized] public int Color;
        [NonSerialized] public int GoalColor; 
        [NonSerialized] public List<MondrianTile> neighbours;
        [NonSerialized] public SpriteRenderer GoalTile;
        [SerializeField] private SpriteRenderer PuzzleTile;
        [SerializeField] private BoxCollider Collider;
        [SerializeField] private RMMondrian mondrian;

        public void Awake()
        {
            neighbours = new List<MondrianTile>();
        }

        public void Instantiate(int columns, int rows, GameObject goalTile)
        {
            Vector2 rectMin = new Vector2(CoordinateX / (float)columns, CoordinateY / (float)rows);
            Vector2 rectMax = new Vector2((CoordinateX + Width) / (float)columns, (CoordinateY + Height) / (float)rows);

            RectTransform prect = gameObject.GetComponent<RectTransform>();
            prect.anchorMin = rectMin;
            prect.anchorMax = rectMax;
            gameObject.SetActive(true);
            Collider.size = new Vector3(Width * 0.5f, Height * 0.5f, 1.0f);
            PuzzleTile.size = new Vector2(Width * 50, Height * 50);

            RectTransform grect = goalTile.GetComponent<RectTransform>();
            grect.anchorMin = rectMin;
            grect.anchorMax = rectMax;
            goalTile.SetActive(true);
            GoalTile = goalTile.GetComponent<SpriteRenderer>();
            GoalTile.size = new Vector2(Width * 50, Height * 50);
        }

        public void Click()
        {
            mondrian.PaintNext(this);
            Debug.Log("[Mondrian] Clicked " + ToString());
        }

        public override string ToString()
        {
            return "(" + CoordinateX + ", " + CoordinateY + ", " + Width + ", " + Height + ")";
        }

        public void Click(Color32[] colors, int colorIndex, bool isGoal)
        {
            foreach(MondrianTile tile in neighbours)
                tile.Paint(colors, colorIndex, isGoal);
        }

        private void Paint(Color32[] colors, int colorIndex, bool isGoal)
        {
            if (isGoal)
            {
                GoalTile.color = colors[colorIndex];
                GoalColor = colorIndex;
            }
            else
            {
                PuzzleTile.color = colors[colorIndex];
                Color = colorIndex;
            }
        }
        
        public void ClearPuzzle()
        {
            Color = -1;
            PuzzleTile.color = UnityEngine.Color.white; 
        }
    }
}
