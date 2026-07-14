using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;

namespace JTH.Scripts.Domain.Turn
{
    public sealed class TurnResolutionResult
    {
        public PlacementResult Placement { get; }
        public ClearReassemblyResult Reassembly { get; }
        public bool BoardRotated { get; }

        public TurnResolutionResult(
            PlacementResult placement,
            ClearReassemblyResult reassembly,
            bool boardRotated)
        {
            Placement = placement;
            Reassembly = reassembly;
            BoardRotated = boardRotated;
        }
    }
}
