using System.Collections.Generic;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    public static class LineClearDetector
    {
        public static ClearedLineResult Detect(BoardGrid grid, IReadOnlyList<Vector2Int> changedGridPositions)
        {
            HashSet<int> candidateRows = new HashSet<int>();
            HashSet<int> candidateColumns = new HashSet<int>();

            foreach (Vector2Int changedPos in changedGridPositions)
            {
                candidateRows.Add(changedPos.y);
                candidateColumns.Add(changedPos.x);
            }

            List<Line> clearedLines = new List<Line>();

            foreach (int y in candidateRows)
            {
                TryAddClearedLine(grid, Line.Row(y), clearedLines);
            }

            foreach (int x in candidateColumns)
            {
                TryAddClearedLine(grid, Line.Column(x), clearedLines);
            }

            return new ClearedLineResult(clearedLines);
        }

        private static void TryAddClearedLine(BoardGrid grid, Line line, List<Line> clearedLines)
        {
            if (IsLineFull(grid, line))
            {
                clearedLines.Add(line);
            }
        }

        private static bool IsLineFull(BoardGrid grid, Line line)
        {
            int boardSize = grid.BoardSize;

            if (line.Orientation == Line.Axis.Row)
            {
                for (int x = 0; x < boardSize; ++x)
                {
                    if (!grid.IsOccupied(new Vector2Int(x, line.Index)))
                    {
                        Debug.Log(x);
                        return false;
                    }
                }
            }
            else
            {
                for (int y = 0; y < boardSize; ++y)
                {
                    if (!grid.IsOccupied(new Vector2Int(line.Index, y)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
