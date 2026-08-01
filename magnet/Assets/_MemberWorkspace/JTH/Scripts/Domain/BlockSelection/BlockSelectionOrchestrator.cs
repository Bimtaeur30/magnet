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

        /// <summary>이번 SelectPieces 호출에서 스킵·실패한 티어의 경과 (진단 로그용).</summary>
        private readonly List<string> _trace = new();

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
            _trace.Clear();

            // 0 Relife — 재시작 직후 접대. 1x1은 Relife 번들에서만 나온다 (SPEC §9.0)
            if (!isRetrySession)
            {
                _trace.Add("Relife 스킵: 재시작 세션 아님");
            }
            else if (turnIndex >= _tuning.RelifeTurnCount)
            {
                _trace.Add($"Relife 스킵: turn {turnIndex} ≥ 접대 턴 수 {_tuning.RelifeTurnCount}");
            }
            else
            {
                BundleDraw relife = TryPickBundle(snapshot, BundleTag.Relife, BundleValidation.Passable);
                if (relife != null)
                {
                    return FromBundle(SelectionTier.Relife, relife, health, blame,
                        $"재시작 세션 turn {turnIndex} < {_tuning.RelifeTurnCount} → 접대 번들");
                }

                _trace.Add("Relife 실패: 게이트 통과했으나 통과 가능 번들 없음");
            }

            // 1 Trap — 극희귀. TooDirty + blame 매우 높음 + 확률 (SPEC §9.1)
            if (health.Zone != HealthZone.TooDirty)
            {
                _trace.Add($"Trap 스킵: zone {health.Zone} (TooDirty 아님)");
            }
            else if (blame < _tuning.BlameTrapThreshold)
            {
                _trace.Add($"Trap 스킵: blame {blame:F1} < 문턱 {_tuning.BlameTrapThreshold:F0}");
            }
            else if (!Roll(_tuning.TrapProbability))
            {
                _trace.Add($"Trap 스킵: 확률 {_tuning.TrapProbability:P1} 굴림 실패");
            }
            else
            {
                BundleDraw trap = TryPickBundle(snapshot, BundleTag.Trap, BundleValidation.Trap);
                if (trap != null)
                {
                    return FromBundle(SelectionTier.Trap, trap, health, blame,
                        $"zone TooDirty + blame {blame:F1} ≥ {_tuning.BlameTrapThreshold:F0}"
                        + $" + 확률 {_tuning.TrapProbability:P1} 통과");
                }

                _trace.Add("Trap 실패: 게이트 통과했으나 번들 검증 실패");
            }

            // 2 ComboBreak — TooEmpty + blame 중간 이상 + 확률 (SPEC §9.2)
            if (health.Zone != HealthZone.TooEmpty)
            {
                _trace.Add($"ComboBreak 스킵: zone {health.Zone} (TooEmpty 아님)");
            }
            else if (blame < _tuning.BlameComboBreakThreshold)
            {
                _trace.Add($"ComboBreak 스킵: blame {blame:F1} < 문턱 {_tuning.BlameComboBreakThreshold:F0}");
            }
            else if (!Roll(_tuning.ComboBreakProbability))
            {
                _trace.Add($"ComboBreak 스킵: 확률 {_tuning.ComboBreakProbability:P1} 굴림 실패");
            }
            else
            {
                BundleDraw comboBreak = TryPickBundle(snapshot, BundleTag.ComboBreak, BundleValidation.ComboBreak);
                if (comboBreak != null)
                {
                    return FromBundle(SelectionTier.ComboBreak, comboBreak, health, blame,
                        $"zone TooEmpty + blame {blame:F1} ≥ {_tuning.BlameComboBreakThreshold:F0}"
                        + $" + 확률 {_tuning.ComboBreakProbability:P1} 통과");
                }

                _trace.Add("ComboBreak 실패: 게이트 통과했으나 번들 검증 실패");
            }

            // 3 Hospitality — 기회 게이트는 생성기 내부, 변덕 확률은 여기 (SPEC §9.3·§10)
            if (!Roll(_tuning.HospitalityProbability))
            {
                _trace.Add($"Hospitality 스킵: 확률 {_tuning.HospitalityProbability:P0} 굴림 실패");
            }
            else
            {
                List<IReadOnlyList<Vector2Int>> hospitality =
                    HospitalityGenerator.TryGenerate(snapshot, health, _hospitalityShapes, _tuning, _rng);
                if (hospitality != null)
                {
                    return FromGenerated(SelectionTier.Hospitality, hospitality, null, health, blame,
                        $"확률 {_tuning.HospitalityProbability:P0} 통과 + 기회 게이트"
                        + $"(opportunity ≥ {_tuning.OpportunityHighThreshold:F2}) 통과 + 후보 품질 충족");
                }

                _trace.Add("Hospitality 실패: 기회 게이트 미달 또는 후보 품질 미달");
            }

            // 4 Easy — 판이 험한데 유저 탓이 아님 (SPEC §9.4)
            if (health.Score >= _tuning.EasyHealthThreshold)
            {
                _trace.Add($"Easy 스킵: health {health.Score:F2} ≥ {_tuning.EasyHealthThreshold:F2} (판 괜찮음)");
            }
            else if (blame >= _tuning.EasyBlameMax)
            {
                _trace.Add($"Easy 스킵: blame {blame:F1} ≥ {_tuning.EasyBlameMax:F0} (유저 탓)");
            }
            else
            {
                BundleDraw easy = TryPickBundle(snapshot, BundleTag.Normal, BundleValidation.Easy);
                if (easy != null)
                {
                    return FromBundle(SelectionTier.Easy, easy, health, blame,
                        $"health {health.Score:F2} < {_tuning.EasyHealthThreshold:F2} (판 험함)"
                        + $" + blame {blame:F1} < {_tuning.EasyBlameMax:F0} (유저 탓 아님)");
                }

                _trace.Add("Easy 실패: 게이트 통과했으나 번들 검증 실패");
            }

            // 5 Pressure — 의도적 유일수 (SPEC §9.5·§11)
            if (health.Zone != HealthZone.TooDirty && health.Score >= _tuning.PressureHealthThreshold)
            {
                _trace.Add($"Pressure 스킵: zone {health.Zone}"
                    + $" + health {health.Score:F2} ≥ {_tuning.PressureHealthThreshold:F2}");
            }
            else if (!Roll(_tuning.PressureProbability))
            {
                _trace.Add($"Pressure 스킵: 확률 {_tuning.PressureProbability:P0} 굴림 실패");
            }
            else
            {
                PressureGenerator.PressureDraw pressure =
                    PressureGenerator.TryGenerate(snapshot, _pressureShapes, _tuning, _rng);
                if (pressure != null)
                {
                    string gate = health.Zone == HealthZone.TooDirty
                        ? "zone TooDirty"
                        : $"health {health.Score:F2} < {_tuning.PressureHealthThreshold:F2}";
                    return FromGenerated(SelectionTier.Pressure, pressure.Pieces, pressure.Solution, health, blame,
                        $"{gate} + 확률 {_tuning.PressureProbability:P0} 통과 + 유일해 후보 생성 성공");
                }

                _trace.Add("Pressure 실패: 유일수 후보 생성 실패 (난이도 미달 포함)");
            }

            // 6 Normal — 통과 가능 번들 가중 랜덤 (SPEC §9.6)
            BundleDraw normal = TryPickBundle(snapshot, BundleTag.Normal, BundleValidation.Passable);
            if (normal != null)
            {
                return FromBundle(SelectionTier.Normal, normal, health, blame,
                    "상위 티어 전부 미발동 → 통과 가능 Normal 번들 가중 랜덤");
            }

            _trace.Add("Normal 실패: 통과 가능 번들 없음");

            // 7 Fallback — 실시간 느슨한 조합 (SPEC §9.7)
            List<IReadOnlyList<Vector2Int>> fallback =
                FallbackGenerator.TryGenerate(snapshot, _normalShapes, _tuning, _rng);
            if (fallback != null)
            {
                return FromGenerated(SelectionTier.Fallback, fallback, null, health, blame,
                    "Normal 번들까지 실패 → 실시간 느슨한 조합 생성");
            }

            _trace.Add("Fallback 실패: 실시간 조합 생성 실패");
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
                return FromBundle(SelectionTier.Fallback, anyPlaceable, health, blame,
                    "최후 수단: 하나라도 놓을 수 있는 Normal 번들 강제");
            }

            List<IReadOnlyList<Vector2Int>> forced = ShapeSampler.Sample3Rotated(_normalShapes, _rng);
            return FromGenerated(SelectionTier.Fallback, forced, null, health, blame,
                "최후 수단: 배치 가능 번들조차 없음 → Normal 가중치로 3피스 강제 샘플");
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

        private BlockSelectionResult FromBundle(
            SelectionTier tier, BundleDraw draw, BoardHealthResult health, float blame, string selectedReason)
        {
            return new BlockSelectionResult(
                tier, draw.BundleId, draw.Pieces, null, health.Score, health.Zone, blame,
                ComposeReason(selectedReason));
        }

        private BlockSelectionResult FromGenerated(
            SelectionTier tier,
            List<IReadOnlyList<Vector2Int>> pieces,
            Solution.UniqueSolution solution,
            BoardHealthResult health,
            float blame,
            string selectedReason)
        {
            return new BlockSelectionResult(
                tier, null, pieces, solution, health.Score, health.Zone, blame,
                ComposeReason(selectedReason));
        }

        /// <summary>선택 이유 1줄 + 그 위 티어들의 스킵·실패 경과를 여러 줄로 합친다.</summary>
        private string ComposeReason(string selectedReason)
        {
            if (_trace.Count == 0)
            {
                return $"선택 이유: {selectedReason}";
            }

            return $"선택 이유: {selectedReason}\n상위 티어 경과: {string.Join(" · ", _trace)}";
        }
    }
}
