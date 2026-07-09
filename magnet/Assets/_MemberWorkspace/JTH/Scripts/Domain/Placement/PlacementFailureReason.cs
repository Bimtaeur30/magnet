namespace JTH.Scripts.Domain.Placement
{
    public enum PlacementFailureReason
    {
        None = 0,
        OverlapsOccupied,
        OverlapsMagnet,
        /// <summary>흡착 경로에서 블록·자석을 만나지 못하고 경계에만 닿음 → 놓을 수 없음.</summary>
        NoSnapTarget,
    }
}
