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

                if (PlacementService.CanPlaceAnywhere(shapeBlock.CellOffsets, grid))
                {
                    return false;
                }
            }

            return true;
        }
    }
}