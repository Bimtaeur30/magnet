using System.Collections.Generic;
using JTH.Scripts.Domain.Clear;
using Magnet.Contracts;
using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    public sealed class PlacementResult
    {
        public PlacementResult(
            IReadOnlyList<ShapeBlockData> candidates,
            IReadOnlyList<Vector2Int> placedGridPositions,
            ClearedLineResult clearedLineResult,
            bool firstDrop,
            bool lastDrop)
        {
            Candidates = candidates;
            PlacedGridPositions = placedGridPositions;
            ClearedLineResult = clearedLineResult;
            FirstDrop = firstDrop;
            LastDrop = lastDrop;
        }

        public IReadOnlyList<ShapeBlockData> Candidates { get; }
        public IReadOnlyList<Vector2Int> PlacedGridPositions { get; }
        public ClearedLineResult ClearedLineResult { get; }
        public bool FirstDrop { get; }
        public bool LastDrop { get; }
    }
}