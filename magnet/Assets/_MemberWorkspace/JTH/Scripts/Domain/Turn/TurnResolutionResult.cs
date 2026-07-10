using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;

namespace JTH.Scripts.Domain.Turn
{
    public sealed class TurnResolutionResult
    {
        private TurnResolutionResult(PlacementResult placement, ClearDetectionResult clearResult, bool boardRotated)
        {
            Placement = placement;
            ClearResult = clearResult;
            BoardRotated = boardRotated;
        }

        public PlacementResult Placement { get; }
        public ClearDetectionResult ClearResult { get; }
        public bool BoardRotated { get; }

        public static TurnResolutionResult Create(
            PlacementResult placement,
            ClearDetectionResult clearResult,
            bool boardRotated)
        {
            return new TurnResolutionResult(placement, clearResult, boardRotated);
        }
    }
}
