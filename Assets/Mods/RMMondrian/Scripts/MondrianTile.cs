using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RMMondrian
{
    public class MondrianTile
    {
        public int CoordinateX;
        public int CoordinateY;
        public int Width;
        public int Height;
        public int Color;
        public List<MondrianTile> neighbours = new List<MondrianTile>();
        public Image PuzzleTile;
        public Image GoalTile;

        public MondrianTile() { }

        public MondrianTile(int x, int y, int z, int w)
        {
            CoordinateX = x;
            CoordinateY = y;
            Width = z;
            Height = w;
            Color = -1;
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
            Color = colorIndex;
            if (isGoal)
                GoalTile.color = colors[Color];
            else 
                PuzzleTile.color = colors[Color];
        }
    }
}
