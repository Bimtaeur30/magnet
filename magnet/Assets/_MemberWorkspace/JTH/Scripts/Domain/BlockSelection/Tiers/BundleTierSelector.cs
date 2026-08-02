using System;
using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.BlockSelection.Tiers
{
    /// <summary>
    /// 태그별 번들 목록에서 가중 랜덤 → 회전 적용 → 솔버 검증을 거쳐 1개를 확정한다.
    /// 검증 실패 번들은 제외하고 재추첨, probeCount 초과 시 null (fallthrough).
    /// </summary>
    public static class BundleTierSelector
    {
        public static BundleDraw TryPick(
            BoardGrid board,
            IReadOnlyList<BlockBundleSO> bundles,
            BundleValidation validation,
            Random rng,
            int probeCount,
            Func<BlockBundleSO, float> weightMultiplier = null)
        {
            if (bundles == null || bundles.Count == 0)
            {
                return null;
            }

            List<BlockBundleSO> remaining = new(bundles);
            int probes = Mathf.Min(probeCount, remaining.Count);

            for (int attempt = 0; attempt < probes; ++attempt)
            {
                BlockBundleSO bundle = TakeWeighted(remaining, rng, weightMultiplier);
                if (bundle == null)
                {
                    return null;
                }

                List<IReadOnlyList<Vector2Int>> pieces = ToRotatedPieces(bundle, rng);
                if (pieces == null)
                {
                    continue;
                }

                if (Validate(board, pieces, validation))
                {
                    return new BundleDraw(bundle.BundleId, pieces);
                }
            }

            return null;
        }

        /// <summary>
        /// 번들의 canonical 모양 3개에 각각 랜덤 회전을 적용. 모양이 3개 미만이면 null.
        /// </summary>
        private static List<IReadOnlyList<Vector2Int>> ToRotatedPieces(BlockBundleSO bundle, Random rng)
        {
            IReadOnlyList<Magnet.Core.SO.Block.BlockShapeSO> shapes = bundle.Shapes;
            if (shapes == null || shapes.Count < 3)
            {
                return null;
            }

            List<IReadOnlyList<Vector2Int>> pieces = new(3);
            for (int i = 0; i < 3; ++i)
            {
                if (shapes[i] == null)
                {
                    return null;
                }

                pieces.Add(ShapeRotator.Rotate(shapes[i].CellOffsets, rng.Next(4)));
            }

            return pieces;
        }

        /// <summary>핸드(3피스) 단위 검증 — 번들 없이 샘플한 핸드에도 쓴다 (Normal·Easy 독립 추첨, phase9).</summary>
        public static bool IsValid(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces, BundleValidation validation)
        {
            return Validate(board, pieces, validation);
        }

        private static bool Validate(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces, BundleValidation validation)
        {
            if (!PlacementSolver.HasAnyPlacement(board, pieces))
            {
                return false;
            }

            switch (validation)
            {
                case BundleValidation.AnyPlaceable:
                    return true;

                case BundleValidation.Passable:
                    return PlacementSolver.FullSequenceExists(board, pieces);

                case BundleValidation.Trap:
                    return !PlacementSolver.FullSequenceExists(board, pieces);

                case BundleValidation.ComboBreak:
                    return PlacementSolver.FullSequenceExists(board, pieces)
                        && !PlacementSolver.ComboMaintainable(board, pieces);

                case BundleValidation.Easy:
                    return PlacementSolver.ComboMaintainable(board, pieces);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 가중 랜덤으로 1개 뽑고 목록에서 제거 (재추첨 시 중복 방지).
        /// weightMultiplier가 있으면 번들별 유효 가중 = weight × 배수 (쏙 맞춤 부스트 등).
        /// </summary>
        private static BlockBundleSO TakeWeighted(
            List<BlockBundleSO> remaining, Random rng, Func<BlockBundleSO, float> weightMultiplier)
        {
            int totalWeight = 0;
            foreach (BlockBundleSO bundle in remaining)
            {
                totalWeight += EffectiveWeight(bundle, weightMultiplier);
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = rng.Next(totalWeight);
            int accumulated = 0;

            for (int i = 0; i < remaining.Count; ++i)
            {
                accumulated += EffectiveWeight(remaining[i], weightMultiplier);
                if (roll < accumulated)
                {
                    BlockBundleSO picked = remaining[i];
                    remaining.RemoveAt(i);
                    return picked;
                }
            }

            BlockBundleSO last = remaining[^1];
            remaining.RemoveAt(remaining.Count - 1);
            return last;
        }

        private static int EffectiveWeight(BlockBundleSO bundle, Func<BlockBundleSO, float> weightMultiplier)
        {
            int baseWeight = Mathf.Max(1, bundle.Weight);
            if (weightMultiplier == null)
            {
                return baseWeight;
            }

            return Mathf.Max(1, Mathf.RoundToInt(baseWeight * weightMultiplier(bundle)));
        }
    }
}
