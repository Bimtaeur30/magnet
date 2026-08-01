using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Bundles;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.BlockSelection.Generation
{
    /// <summary>
    /// 모든 티어 실패 시 느슨한 실시간 조합 (SPEC §9.7).
    /// 1차: 통과 가능(hasAny + fullSequence) 조합, 2차: hasAny만 만족하는 조합.
    /// </summary>
    public static class FallbackGenerator
    {
        public static List<IReadOnlyList<Vector2Int>> TryGenerate(
            BoardGrid board,
            IReadOnlyList<WeightedShape> pool,
            BlockSelectionTuningSO tuning,
            Random rng)
        {
            List<IReadOnlyList<Vector2Int>> anyPlaceable = null;

            for (int sample = 0; sample < tuning.FallbackSampleCount; ++sample)
            {
                List<IReadOnlyList<Vector2Int>> pieces = ShapeSampler.Sample3Rotated(pool, rng);
                if (pieces == null)
                {
                    return null;
                }

                if (!PlacementSolver.HasAnyPlacement(board, pieces))
                {
                    continue;
                }

                if (PlacementSolver.FullSequenceExists(board, pieces))
                {
                    return pieces;
                }

                anyPlaceable ??= pieces;
            }

            return anyPlaceable;
        }
    }
}
