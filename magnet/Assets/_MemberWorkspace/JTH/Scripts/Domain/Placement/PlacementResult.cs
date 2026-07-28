using System.Collections.Generic;
using JTH.Scripts.Domain.Clear;
using Magnet.Contracts;

namespace JTH.Scripts.Domain.Placement
{
    public sealed class PlacementResult
    {
        public PlacementResult(
            IReadOnlyList<ShapeBlockData> candidates,
            int cellsPlaced,
            ClearedLineResult clearedLineResult,
            bool firstDrop,
            bool lastDrop)
        {
            Candidates = candidates;
            CellsPlaced = cellsPlaced;
            ClearedLineResult = clearedLineResult;
            FirstDrop = firstDrop;
            LastDrop = lastDrop;
        }

        public IReadOnlyList<ShapeBlockData> Candidates { get; }
        public int CellsPlaced { get; }
        public ClearedLineResult ClearedLineResult { get; }
        public bool FirstDrop { get; }
        public bool LastDrop { get; }
    }
}