using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    public readonly struct Line
    {
        public enum Axis
        {
            Row,
            Column
        }

        public Axis Orientation { get; }
        public int Index { get; }

        private Line(Axis orientation, int index)
        {
            Orientation = orientation;
            Index = index;
        }

        public static Line Row(int y) => new Line(Axis.Row, y);

        public static Line Column(int x) => new Line(Axis.Column, x);

        public List<Vector2Int> GetCells(int boardSize)
        {
            List<Vector2Int> cells = new List<Vector2Int>(boardSize);

            if (Orientation == Axis.Row)
            {
                for (int x = 0; x < boardSize; ++x)
                {
                    cells.Add(new Vector2Int(x, Index));
                }
            }
            else
            {
                for (int y = 0; y < boardSize; ++y)
                {
                    cells.Add(new Vector2Int(Index, y));
                }
            }

            return cells;
        }
    }
}
