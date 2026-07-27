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
    }
}
