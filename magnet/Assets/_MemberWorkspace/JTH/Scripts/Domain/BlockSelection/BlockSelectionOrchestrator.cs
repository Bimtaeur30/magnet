using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Bundles;
using JTH.Scripts.Domain.BlockSelection.Generation;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.BlockSelection.Tiers;
using JTH.Scripts.Domain.Board;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.BlockSelection
{
    /// <summary>
    /// 티어 우선순위 스택 전체 (SPEC §9·부록 A). 위에서부터 시도해 첫 성공 티어로 3피스를 확정한다.
    /// 어떤 경우에도 null을 반환하지 않는다 (최후엔 Normal 번들 강제).
    /// </summary>
    public sealed class BlockSelectionOrchestrator
    {
        private readonly BlockSelectionTuningSO _tuning;
        private readonly BlockBundlePoolSO _bundles;
        private readonly Random _rng;

        private readonly List<WeightedShape> _normalShapes = new();
        private readonly List<WeightedShape> _hospitalityShapes = new();
        private readonly List<WeightedShape> _pressureShapes = new();

        public BlockSelectionOrchestrator(BlockSelectionTuningSO tuning, BlockBundlePoolSO bundles, Random rng)
        {
            _tuning = tuning;
            _bundles = bundles;
            _rng = rng;

            foreach (BlockShapeWeight entry in tuning.BlockWeights)
            {
                if (entry.Shape == null)
                {
                    continue;
                }

                _normalShapes.Add(new WeightedShape(entry.Shape.CellOffsets, entry.NormalWeight));
                _hospitalityShapes.Add(new WeightedShape(entry.Shape.CellOffsets, entry.HospitalityWeight));
                _pressureShapes.Add(new WeightedShape(entry.Shape.CellOffsets, entry.PressureWeight));
            }
        }

        public BlockSelectionResult SelectPieces(
            BoardGrid board,
            BoardHealthResult health,
            float blame,
            bool isRetrySession,
            int turnIndex)
        {
            BoardGrid snapshot = board.Clone();

            // 0 Relife — 재시작 직후 접대. 1x1은 Relife 번들에서만 나온다 (SPEC §9.0)
            if (isRetrySession && turnIndex < _tuning.RelifeTurnCount)
            {
                BundleDraw relife = TryPickBundle(snapshot, BundleTag.Relife, BundleValidation.Passable);
                if (relife != null)
                {
                    return FromBundle(SelectionTier.Relife, relife, health, blame);
                }
            }

            // 1 Trap — 극희귀. TooDirty + blame 매우 높음 + 확률 (SPEC §9.1)
            if (health.Zone == HealthZone.TooDirty
                && blame >= _tuning.BlameTrapThreshold
                && Roll(_tuning.TrapProbability))
            {
                BundleDraw trap = TryPickBundle(snapshot, BundleTag.Trap, BundleValidation.Trap);
                if (trap != null)
                {
                    return FromBundle(SelectionTier.Trap, trap, health, blame);
                }
            }

            // 2 ComboBreak — TooEmpty + blame 중간 이상 + 확률 (SPEC §9.2)
            if (health.Zone == HealthZone.TooEmpty
                && blame >= _tuning.BlameComboBreakThreshold
                && Roll(_tuning.ComboBreakProbability))
            {
                BundleDraw comboBreak = TryPickBundle(snapshot, BundleTag.ComboBreak, BundleValidation.ComboBreak);
                if (comboBreak != null)
                {
                    return FromBundle(SelectionTier.ComboBreak, comboBreak, health, blame);
                }
            }

            // 3 Hospitality — 기회 게이트는 생성기 내부, 변덕 확률은 여기 (SPEC §9.3·§10)
            if (Roll(_tuning.HospitalityProbability))
            {
                List<IReadOnlyList<Vector2Int>> hospitality =
                    HospitalityGenerator.TryGenerate(snapshot, health, _hospitalityShapes, _tuning, _rng);
                if (hospitality != null)
                {
                    return FromGenerated(SelectionTier.Hospitality, hospitality, null, health, blame);
                }
            }

            // 4 Easy — 판이 험한데 유저 탓이 아님 (SPEC §9.4)
            if (health.Score < _tuning.EasyHealthThreshold && blame < _tuning.EasyBlameMax)
            {
                BundleDraw easy = TryPickBundle(snapshot, BundleTag.Normal, BundleValidation.Easy);
                if (easy != null)
                {
                    return FromBundle(SelectionTier.Easy, easy, health, blame);
                }
            }

            // 5 Pressure — 의도적 유일수 (SPEC §9.5·§11)
            if ((health.Zone == HealthZone.TooDirty || health.Score < _tuning.PressureHealthThreshold)
                && Roll(_tuning.PressureProbability))
            {
                PressureGenerator.PressureDraw pressure =
                    PressureGenerator.TryGenerate(snapshot, _pressureShapes, _tuning, _rng);
                if (pressure != null)
                {
                    return FromGenerated(SelectionTier.Pressure, pressure.Pieces, pressure.Solution, health, blame);
                }
            }

            // 6 Normal — 통과 가능 번들 가중 랜덤 (SPEC §9.6)
            BundleDraw normal = TryPickBundle(snapshot, BundleTag.Normal, BundleValidation.Passable);
            if (normal != null)
            {
                return FromBundle(SelectionTier.Normal, normal, health, blame);
            }

            // 7 Fallback — 실시간 느슨한 조합 (SPEC §9.7)
            List<IReadOnlyList<Vector2Int>> fallback =
                FallbackGenerator.TryGenerate(snapshot, _normalShapes, _tuning, _rng);
            if (fallback != null)
            {
                return FromGenerated(SelectionTier.Fallback, fallback, null, health, blame);
            }

            return ForceNormalAny(snapshot, health, blame);
        }

        /// <summary>
        /// 최후 수단: Normal 번들 중 hasAny 만족 아무거나, 그마저 없으면 첫 Normal 번들 강제.
        /// </summary>
        private BlockSelectionResult ForceNormalAny(BoardGrid snapshot, BoardHealthResult health, float blame)
        {
            IReadOnlyList<BlockBundleSO> normals = _bundles.GetByTag(BundleTag.Normal);

            BundleDraw anyPlaceable = BundleTierSelector.TryPick(
                snapshot, normals, BundleValidation.AnyPlaceable, _rng, normals.Count);
            if (anyPlaceable != null)
            {
                return FromBundle(SelectionTier.Fallback, anyPlaceable, health, blame);
            }

            List<IReadOnlyList<Vector2Int>> forced = ShapeSampler.Sample3Rotated(_normalShapes, _rng);
            return FromGenerated(SelectionTier.Fallback, forced, null, health, blame);
        }

        private BundleDraw TryPickBundle(BoardGrid snapshot, BundleTag tag, BundleValidation validation)
        {
            return BundleTierSelector.TryPick(
                snapshot, _bundles.GetByTag(tag), validation, _rng, _tuning.BundleProbeCount);
        }

        private bool Roll(float probability)
        {
            return _rng.NextDouble() < probability;
        }

        private static BlockSelectionResult FromBundle(
            SelectionTier tier, BundleDraw draw, BoardHealthResult health, float blame)
        {
            return new BlockSelectionResult(tier, draw.BundleId, draw.Pieces, null, health.Score, health.Zone, blame);
        }

        private static BlockSelectionResult FromGenerated(
            SelectionTier tier,
            List<IReadOnlyList<Vector2Int>> pieces,
            Solution.UniqueSolution solution,
            BoardHealthResult health,
            float blame)
        {
            return new BlockSelectionResult(tier, null, pieces, solution, health.Score, health.Zone, blame);
        }
    }
}
