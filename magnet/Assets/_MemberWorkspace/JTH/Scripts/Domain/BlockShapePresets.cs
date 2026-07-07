using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace JTH.Scripts.Domain
{
    /// <summary>
    /// SCRUM-25 SO 연동 전 개발·테스트용 기본 형태. 피벗=(0,0) 최소 코너 기준.
    /// </summary>
    public static class BlockShapePresets
    {
        public static readonly IBlockShape Single = new BlockShapeData(
            "1x1",
            new[] { Vector2Int.zero });

        public static readonly IBlockShape Line2Horizontal = new BlockShapeData(
            "1x2_h",
            new[] { Vector2Int.zero, new Vector2Int(1, 0) });

        public static readonly IBlockShape Line2Vertical = new BlockShapeData(
            "2x1_v",
            new[] { Vector2Int.zero, new Vector2Int(0, 1) });

        public static readonly IBlockShape Line3Horizontal = new BlockShapeData(
            "1x3_h",
            new[] { Vector2Int.zero, new Vector2Int(1, 0), new Vector2Int(2, 0) });

        public static readonly IBlockShape Square2 = new BlockShapeData(
            "2x2",
            new[]
            {
                Vector2Int.zero,
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
            });

        public static readonly IBlockShape L3 = new BlockShapeData(
            "L_3",
            new[]
            {
                Vector2Int.zero,
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
            });

        public static readonly IBlockShape[] All =
        {
            Single,
            Line2Horizontal,
            Line2Vertical,
            Line3Horizontal,
            Square2,
            L3,
        };
    }
}
