using System.Collections.Generic;
using JTH.Scripts.Presentation;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    public static class LineClearService
    {
        public static ClearedLineResult DetectAndApply(GameBoard gameBoard, IReadOnlyList<Vector2Int> changedPositions)
        {
            ClearedLineResult result = LineClearDetector.Detect(gameBoard.Grid, changedPositions);
            gameBoard.RemoveCellsAt(result.CollectClearedCells(gameBoard.Grid.BoardSize));
            return result;
        }
        
        public static ClearedLineResult DetectAndApply(GameBoard gameBoard)
        {
            List<Vector2Int> changedPositions = new List<Vector2Int>();
            for (int i = 0; i < gameBoard.Grid.BoardSize; ++i)
            {
                for (int j = 0; j < gameBoard.Grid.BoardSize; ++j)
                {
                    changedPositions.Add(new Vector2Int(i, j));
                }
            }
            
            return DetectAndApply(gameBoard, changedPositions);
        }
    }
}
