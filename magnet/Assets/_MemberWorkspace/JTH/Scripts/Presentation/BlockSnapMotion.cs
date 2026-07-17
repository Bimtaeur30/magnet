using System;
using System.Collections.Generic;
using JTH.Scripts.Data;
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
            BlockSnapConfigSO snapConfig,
            Action onComplete)
        {
            shapeBlock.ShowAtSnapStart(shape, finalPivot, stagingGridY);
            shapeBlock.AnimateSnapY(
                shape,
                finalPivot,
                boardConfig,
                snapConfig.Duration,
                snapConfig.Ease,
                onComplete);
        }

        public static void PlayFromOffsets(
            ShapeBlock shapeBlock,
            IReadOnlyList<Vector2Int> cellOffsets,
            Vector2Int finalPivot,
            int stagingGridY,
            BoardConfigSO boardConfig,
            BlockSnapConfigSO snapConfig,
            Action onComplete)
        {
            shapeBlock.ShowAtSnapStartFromOffsets(cellOffsets, finalPivot, stagingGridY);
            shapeBlock.AnimateSnapYFromOffsets(
                cellOffsets,
                finalPivot,
                boardConfig,
                snapConfig.Duration,
                snapConfig.Ease,
                onComplete);
        }
    }
}
