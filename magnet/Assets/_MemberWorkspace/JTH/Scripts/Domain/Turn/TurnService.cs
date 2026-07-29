using System.Collections.Generic;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using Magnet.Contracts;
using UnityEngine;

namespace JTH.Scripts.Domain.Turn
{
    public static class TurnService
    {
        public static bool IsGameOver(BoardGrid grid, IReadOnlyList<ShapeBlockData> candidates)
        {
            if (candidates.Count == 0)
                return false;
            
            foreach (ShapeBlockData shapeBlock in candidates)
            {
                if (shapeBlock == null)
                    continue;
                
                Vector2Int pivot = Vector2Int.zero;

                for (int x = 0; x < grid.BoardSize; ++x)
                {
                    for (int y = 0; y < grid.BoardSize; ++y)
                    {
                        pivot.x = x;
                        pivot.y = y;

                        if (PlacementService.CanPlace(shapeBlock.CellOffsets, pivot, grid))
                        {
                            return false;
                        }
                    }   
                }
            }

            return true;
        }
    }
}