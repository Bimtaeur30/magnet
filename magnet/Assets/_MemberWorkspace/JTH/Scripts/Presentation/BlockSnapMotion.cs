using System;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Placement;
using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 손 놓은 뒤 X는 프리뷰(최종 pivot)로 순간이동, Y만 LitMotion으로 스냅한다.
    /// </summary>
    public static class BlockSnapMotion
    {
        public static void Play(
            ShapeBlock shapeBlock,
            IBlockShape shape,
            Vector2Int finalPivot,
            int stagingGridY,
            BoardConfigSO boardConfig,
            PlacementConfigSO placementConfig,
            Action onComplete)
        {
            shapeBlock.ShowAtSnapStart(shape, finalPivot, stagingGridY);
            shapeBlock.AnimateSnapY(shape, finalPivot, boardConfig, placementConfig.SnapDuration, onComplete);
        }

        public static void PlayFromPlaced(
            ShapeBlock shapeBlock,
            PlacedBlock placedBlock,
            int stagingGridY,
            BoardConfigSO boardConfig,
            PlacementConfigSO placementConfig,
            Action onComplete)
        {
            shapeBlock.ShowAtSnapStartFromPlaced(placedBlock, stagingGridY);
            shapeBlock.AnimateSnapYFromPlaced(placedBlock, stagingGridY, boardConfig, placementConfig.SnapDuration, onComplete);
        }
    }
}
