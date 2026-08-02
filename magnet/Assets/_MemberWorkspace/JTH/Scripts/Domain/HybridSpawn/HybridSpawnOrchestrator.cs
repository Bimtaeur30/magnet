using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockBlast;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.BlockSelection.Tiers;
using JTH.Scripts.Domain.Board;
using Random = System.Random;

namespace JTH.Scripts.Domain.HybridSpawn
{
    /// <summary>
    /// 병합 스폰 오케스트레이터 — 특수 티어 게이트(Relife → Trap → ComboBreak → Hospitality → Pressure)를
    /// 위에서부터 시도하고, 전부 미발동이면 BlockBlast 핸드오프 체인(BaseChain)이 기본 리듬을 담당한다.
    /// 특수 티어 트리플은 반복 억제 트레이트를 우회하되(솔버 보장 보호) 체인 히스토리에는 기록한다.
    /// </summary>
    public sealed class HybridSpawnOrchestrator
    {
        private readonly HybridTuningSO _tuning;
        private readonly BlockBlastAlgorithm _baseChain;
        private readonly Random _rng;

        private readonly HybridPiecePool _relifePool;
        private readonly HybridPiecePool _trapPool;
        private readonly HybridPiecePool _comboBreakPool;
        private readonly HybridPiecePool _hospitalityPool;
        private readonly HybridPiecePool _pressurePool;

        /// <summary>이번 호출에서 스킵·실패한 티어의 경과 (진단 로그용).</summary>
        private readonly List<string> _trace = new();

        /// <summary>직전 턴 트리플 — 특수 티어 생성기의 샘플링 단계 반복 회피 입력.</summary>
        private int[] _lastTriple;

        public HybridSpawnOrchestrator(HybridTuningSO tuning, BlockBlastAlgorithm baseChain, Random rng)
        {
            _tuning = tuning;
            _baseChain = baseChain;
            _rng = rng;

            int[] fillPoolIds = BlockBlastCatalog.FillPoolIds;
            _relifePool = new HybridPiecePool(BuildRelifeIds(fillPoolIds), id => tuning.RelifeWeights.WeightOf(CellCount(id)));
            _trapPool = new HybridPiecePool(fillPoolIds, id => tuning.TrapWeights.WeightOf(CellCount(id)));
            _comboBreakPool = new HybridPiecePool(fillPoolIds, id => tuning.ComboBreakWeights.WeightOf(CellCount(id)));
            _hospitalityPool = new HybridPiecePool(fillPoolIds, id => tuning.HospitalityWeights.WeightOf(CellCount(id)));
            _pressurePool = new HybridPiecePool(fillPoolIds, id => tuning.PressureWeights.WeightOf(CellCount(id)));
        }

        /// <summary>1x1(ID 1)은 접대용으로 Relife 풀에만 들어간다 (다른 풀은 fillPool = 2..42 − 대각3).</summary>
        private static IReadOnlyList<int> BuildRelifeIds(int[] fillPoolIds)
        {
            List<int> ids = new(fillPoolIds.Length + 1) { 1 };
            ids.AddRange(fillPoolIds);
            return ids;
        }

        private static int CellCount(int id)
        {
            return BlockBlastCatalog.GetOffsets(id).Count;
        }

        public HybridSelectionResult SelectPieces(
            BoardGrid board,
            BoardHealthResult health,
            float blame,
            bool isRetrySession,
            int turnIndex)
        {
            BoardGrid snapshot = board.Clone();
            _trace.Clear();

            // 0 Relife — 재시작 직후 접대. 1x1은 여기서만 (IsRetrySession 배선은 game-over 구현 후)
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
                int[] relife = HybridConstraintGenerator.TryGenerate(
                    snapshot, _relifePool, _tuning.RelifeSampleTries, BundleValidation.Passable, _rng, _lastTriple);
                if (relife != null)
                {
                    return SpecialResult(HybridTier.Relife, relife, null, health, blame,
                        $"재시작 세션 turn {turnIndex} < {_tuning.RelifeTurnCount} → 접대 트리플 생성");
                }

                _trace.Add("Relife 실패: 게이트 통과했으나 통과 가능 트리플 생성 실패");
            }

            // 1 Trap — 극희귀. TooDirty + blame 매우 높음 + 확률
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
                int[] trap = HybridConstraintGenerator.TryGenerate(
                    snapshot, _trapPool, _tuning.TrapSampleTries, BundleValidation.Trap, _rng, _lastTriple);
                if (trap != null)
                {
                    return SpecialResult(HybridTier.Trap, trap, null, health, blame,
                        $"zone TooDirty + blame {blame:F1} ≥ {_tuning.BlameTrapThreshold:F0}"
                        + $" + 확률 {_tuning.TrapProbability:P1} 통과");
                }

                _trace.Add("Trap 실패: 게이트 통과했으나 Trap 검증 트리플 생성 실패");
            }

            // 2 ComboBreak — TooEmpty + blame 중간 이상 + 확률
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
                int[] comboBreak = HybridConstraintGenerator.TryGenerate(
                    snapshot, _comboBreakPool, _tuning.ComboBreakSampleTries, BundleValidation.ComboBreak, _rng, _lastTriple);
                if (comboBreak != null)
                {
                    return SpecialResult(HybridTier.ComboBreak, comboBreak, null, health, blame,
                        $"zone TooEmpty + blame {blame:F1} ≥ {_tuning.BlameComboBreakThreshold:F0}"
                        + $" + 확률 {_tuning.ComboBreakProbability:P1} 통과");
                }

                _trace.Add("ComboBreak 실패: 게이트 통과했으나 ComboBreak 검증 트리플 생성 실패");
            }

            // 3 Hospitality — 기회 게이트는 생성기 내부, 변덕 확률은 여기
            if (!Roll(_tuning.HospitalityProbability))
            {
                _trace.Add($"Hospitality 스킵: 확률 {_tuning.HospitalityProbability:P0} 굴림 실패");
            }
            else
            {
                int[] hospitality = HybridHospitalityGenerator.TryGenerate(
                    snapshot, health, _hospitalityPool, _tuning, _rng, _lastTriple);
                if (hospitality != null)
                {
                    return SpecialResult(HybridTier.Hospitality, hospitality, null, health, blame,
                        $"확률 {_tuning.HospitalityProbability:P0} 통과 + 기회 게이트"
                        + $"(opportunity ≥ {_tuning.OpportunityHighThreshold:F2}) 통과 + 후보 품질 충족");
                }

                _trace.Add("Hospitality 실패: 기회 게이트 미달 또는 후보 품질 미달");
            }

            // 4 Pressure — 의도적 유일수
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
                HybridPressureGenerator.PressureDraw pressure = HybridPressureGenerator.TryGenerate(
                    snapshot, _pressurePool, _tuning, _rng, _lastTriple);
                if (pressure != null)
                {
                    string gate = health.Zone == HealthZone.TooDirty
                        ? "zone TooDirty"
                        : $"health {health.Score:F2} < {_tuning.PressureHealthThreshold:F2}";
                    return SpecialResult(HybridTier.Pressure, pressure.Triple, pressure.Solution, health, blame,
                        $"{gate} + 확률 {_tuning.PressureProbability:P0} 통과"
                        + $" + 유일해 생성 성공 (난이도 {pressure.Difficulty:F2})");
                }

                _trace.Add("Pressure 실패: 유일수 후보 생성 실패 (난이도 미달 포함)");
            }

            // 5 BaseChain — 핸드오프 체인 (7 → 1370 근사 → 반복 억제 트레이트, 자체 히스토리 기록)
            BlockBlastSelection baseSelection = _baseChain.Select(snapshot);
            _lastTriple = ToArray(baseSelection.BlockIds);

            return new HybridSelectionResult(
                HybridTier.BaseChain, baseSelection.BlockIds, baseSelection.Pieces, null,
                health.Score, health.Zone, blame, baseSelection,
                ComposeReason($"특수 티어 전부 미발동 → 핸드오프 체인 · {baseSelection.Reason}"));
        }

        private HybridSelectionResult SpecialResult(
            HybridTier tier,
            int[] triple,
            BlockSelection.Solution.UniqueSolution solution,
            BoardHealthResult health,
            float blame,
            string selectedReason)
        {
            // 트레이트 우회 + 히스토리 기록 (grill 확정) — 다음 base 선택의 반복 회피 입력이 된다
            _baseChain.RecordExternalRound(triple);
            _lastTriple = triple;

            return new HybridSelectionResult(
                tier, triple, HybridPiecePool.BuildPieces(triple), solution,
                health.Score, health.Zone, blame, null,
                ComposeReason(selectedReason));
        }

        private bool Roll(float probability)
        {
            return _rng.NextDouble() < probability;
        }

        private static int[] ToArray(IReadOnlyList<int> ids)
        {
            int[] result = new int[ids.Count];
            for (int i = 0; i < ids.Count; ++i)
            {
                result[i] = ids[i];
            }

            return result;
        }

        /// <summary>선택 이유 1줄 + 그 위 티어들의 스킵·실패 경과.</summary>
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
