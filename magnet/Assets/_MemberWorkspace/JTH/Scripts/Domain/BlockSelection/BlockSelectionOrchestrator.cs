using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Bundles;
using JTH.Scripts.Domain.BlockSelection.Generation;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.BlockSelection.Tiers;
using JTH.Scripts.Domain.Board;
using Magnet.Core.SO.Block;
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

        /// <summary>Normal 후보의 결과 Health 예측에 쓰는 배치 자유도 프로브 피스 (Bootstrap과 동일 집합).</summary>
        private readonly IReadOnlyList<IReadOnlyList<Vector2Int>> _freedomProbePieces;

        private readonly List<WeightedShape> _normalShapes = new();
        private readonly List<WeightedShape> _hospitalityShapes = new();
        private readonly List<WeightedShape> _pressureShapes = new();

        /// <summary>Normal 독립 추첨용 (SO 참조 보존 — 쏙·밀도 배수를 모양 단위로 적용).</summary>
        private readonly List<(BlockShapeSO shape, IReadOnlyList<Vector2Int> offsets, float weight)> _normalEntries = new();

        /// <summary>이번 리필의 동적 Normal 추첨 풀 = normalWeight × 쏙 배수 × 밀도 배수. 매 호출 재구성.</summary>
        private readonly List<WeightedShape> _dynamicNormalPool = new();

        /// <summary>이번 SelectPieces 호출에서 스킵·실패한 티어의 경과 (진단 로그용).</summary>
        private readonly List<string> _trace = new();

        /// <summary>이번 리필 기준 모양별 "쏙 들어감" 점수 (회전 포함 최고 둘레 막힘 비율). 매 호출 갱신.</summary>
        private readonly Dictionary<BlockShapeSO, float> _snugByShape = new();

        /// <summary>모양별 정적 특성 (얇음 = 1xN, 큼 = 6칸 이상). 생성자에서 1회 캐시 — 밀도 바이어스용.</summary>
        private readonly Dictionary<BlockShapeSO, (bool slim, bool big)> _shapeTraits = new();

        /// <summary>이번 리필의 보드 채움률 (밀도 바이어스 배수 계산용). 매 호출 갱신.</summary>
        private float _fillRate;

        public BlockSelectionOrchestrator(
            BlockSelectionTuningSO tuning,
            BlockBundlePoolSO bundles,
            IReadOnlyList<IReadOnlyList<Vector2Int>> freedomProbePieces,
            Random rng)
        {
            _tuning = tuning;
            _bundles = bundles;
            _freedomProbePieces = freedomProbePieces;
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
                _normalEntries.Add((entry.Shape, entry.Shape.CellOffsets, entry.NormalWeight));
                _shapeTraits[entry.Shape] = ComputeTraits(entry.Shape.CellOffsets);
            }
        }

        /// <summary>얇음 = 모든 칸이 한 행/한 열 (1xN 계열) · 큼 = 6칸 이상 (3x2·3x3·L3x3).</summary>
        private static (bool slim, bool big) ComputeTraits(IReadOnlyList<Vector2Int> offsets)
        {
            bool sameX = true;
            bool sameY = true;

            foreach (Vector2Int offset in offsets)
            {
                sameX &= offset.x == offsets[0].x;
                sameY &= offset.y == offsets[0].y;
            }

            return (slim: sameX || sameY, big: offsets.Count >= 6);
        }

        public BlockSelectionResult SelectPieces(
            BoardGrid board,
            BoardHealthResult health,
            float blame,
            bool isRetrySession,
            int turnIndex,
            int lastTurnClearedCells = 0)
        {
            BoardGrid snapshot = board.Clone();
            _trace.Clear();
            _fillRate = health.FillRate;
            BuildSnugScores(snapshot);
            BuildDynamicNormalPool();

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

            // 3.5 Momentum — 직전 턴 멀티라인급 클리어 흐름 유지: 큼직한 사각 위주 "기분 좋은 패" (사진 분석 phase9)
            if (lastTurnClearedCells < _tuning.MomentumMinClearedCells)
            {
                _trace.Add($"Momentum 스킵: 직전 턴 클리어 {lastTurnClearedCells}칸"
                    + $" < 문턱 {_tuning.MomentumMinClearedCells}칸");
            }
            else if (health.Zone == HealthZone.TooDirty)
            {
                _trace.Add("Momentum 스킵: zone TooDirty (큰 블록 줄 공간 없음)");
            }
            else if (!Roll(_tuning.MomentumProbability))
            {
                _trace.Add($"Momentum 스킵: 확률 {_tuning.MomentumProbability:P0} 굴림 실패");
            }
            else
            {
                BundleDraw momentum = TryPickBundle(snapshot, BundleTag.Momentum, BundleValidation.Passable, CombinedMultiplier);
                if (momentum != null)
                {
                    return FromBundle(SelectionTier.Momentum, momentum, health, blame,
                        $"직전 턴 클리어 {lastTurnClearedCells}칸 + zone {health.Zone}"
                        + $" + 확률 {_tuning.MomentumProbability:P0} 통과 → 큼직한 흐름 유지 번들");
                }

                _trace.Add("Momentum 실패: 게이트 통과했으나 통과 가능 번들 없음");
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
                List<List<IReadOnlyList<Vector2Int>>> easyHands = SampleValidHands(
                    snapshot, BundleValidation.Easy, maxCandidates: 1);
                if (easyHands.Count > 0)
                {
                    return FromGenerated(SelectionTier.Easy, easyHands[0], null, health, blame,
                        $"health {health.Score:F2} < {_tuning.EasyHealthThreshold:F2} (판 험함)"
                        + $" + blame {blame:F1} < {_tuning.EasyBlameMax:F0} (유저 탓 아님) → 콤보 유지 가능 핸드 추첨");
                }

                _trace.Add("Easy 실패: 게이트 통과했으나 콤보 유지 가능 핸드 샘플 실패");
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

            // 6 Normal — 독립 추첨 핸드 후보 중 결과 BoardHealth가 가장 좋은 핸드 (SPEC §9.6 · phase9 개편)
            BlockSelectionResult healthyNormal = TrySelectHealthiestNormal(snapshot, health, blame);
            if (healthyNormal != null)
            {
                return healthyNormal;
            }

            _trace.Add("Normal 실패: 통과 가능 핸드 샘플 실패");

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
        /// Normal 티어 (phase9 개편): 고정 번들 대신 모양 가중표에서 슬롯 3개를 독립 추첨(중복 허용 —
        /// 실게임과 동일)한 핸드 후보를 여러 개 모아 각각 "최선 플레이 후 보드"의 Health를 예측,
        /// 가장 높은 후보를 준다. 응징은 상위 티어(Trap·Pressure) 몫이므로 Normal은 항상 건강한 판을 지향.
        /// </summary>
        private BlockSelectionResult TrySelectHealthiestNormal(BoardGrid snapshot, BoardHealthResult health, float blame)
        {
            List<List<IReadOnlyList<Vector2Int>>> candidates = SampleValidHands(
                snapshot, BundleValidation.Passable, Mathf.Max(1, _tuning.NormalHealthCandidateCount));

            if (candidates.Count == 0)
            {
                return null;
            }

            List<IReadOnlyList<Vector2Int>> best = candidates[0];
            float bestRank = float.MinValue;
            float bestSnug = 0f;

            foreach (List<IReadOnlyList<Vector2Int>> candidate in candidates)
            {
                float predicted = PredictHealthAfterBestPlay(snapshot, candidate);
                float snug = BestSnugOfPieces(snapshot, candidate);
                float rank = predicted + _tuning.SnugNormalRankBonus * NormalizedSnug(snug);

                if (rank > bestRank)
                {
                    bestRank = rank;
                    bestSnug = snug;
                    best = candidate;
                }
            }

            string snugNote = NormalizedSnug(bestSnug) > 0f
                ? $" + 쏙 맞춤(둘레 막힘 {bestSnug:P0}) 보너스"
                : string.Empty;
            string reason = candidates.Count == 1
                ? $"상위 티어 전부 미발동 → 독립 추첨 통과 핸드 (후보 1개){snugNote}"
                : $"상위 티어 전부 미발동 → 독립 추첨 통과 후보 {candidates.Count}개 중"
                  + $" 예측 BoardHealth 최고({bestRank:F2}) 핸드 선택{snugNote}";

            return FromGenerated(SelectionTier.Normal, best, null, health, blame, reason);
        }

        /// <summary>
        /// 동적 Normal 풀에서 핸드(3피스)를 샘플해 검증 통과분만 최대 maxCandidates개 수집.
        /// 시도 예산은 NormalSampleTries (검증 실패분 포함).
        /// </summary>
        private List<List<IReadOnlyList<Vector2Int>>> SampleValidHands(
            BoardGrid snapshot, BundleValidation validation, int maxCandidates)
        {
            List<List<IReadOnlyList<Vector2Int>>> hands = new();
            int tries = Mathf.Max(maxCandidates, _tuning.NormalSampleTries);

            for (int attempt = 0; attempt < tries && hands.Count < maxCandidates; ++attempt)
            {
                List<IReadOnlyList<Vector2Int>> hand = ShapeSampler.Sample3Rotated(_dynamicNormalPool, _rng);
                if (hand == null)
                {
                    break;
                }

                if (BundleTierSelector.IsValid(snapshot, hand, validation))
                {
                    hands.Add(hand);
                }
            }

            return hands;
        }

        /// <summary>
        /// 이번 리필의 Normal 추첨 풀 재구성: 모양별 유효 가중 = normalWeight × 쏙 배수 × 밀도 배수.
        /// 배수를 모양 단위로 적용해 "포켓에 맞는 모양"·"판 상태에 맞는 크기"가 슬롯마다 더 자주 나온다.
        /// </summary>
        private void BuildDynamicNormalPool()
        {
            _dynamicNormalPool.Clear();

            foreach ((BlockShapeSO shape, IReadOnlyList<Vector2Int> offsets, float weight) in _normalEntries)
            {
                if (weight <= 0f)
                {
                    continue;
                }

                _dynamicNormalPool.Add(new WeightedShape(offsets, weight * ShapeMultiplier(shape)));
            }
        }

        /// <summary>모양 단위 추첨 배수 = 쏙 맞춤 × 밀도 바이어스.</summary>
        private float ShapeMultiplier(BlockShapeSO shape)
        {
            float multiplier = 1f;

            if (_snugByShape.TryGetValue(shape, out float snug))
            {
                multiplier += _tuning.SnugWeightBoost * NormalizedSnug(snug);
            }

            (bool slim, bool big) traits = _shapeTraits[shape];
            if (_fillRate > _tuning.DenseFillMin)
            {
                if (traits.slim)
                {
                    multiplier *= Mathf.Max(1f, _tuning.DenseSlimBoost);
                }

                if (traits.big)
                {
                    multiplier *= Mathf.Clamp01(_tuning.DenseBigPenalty);
                }
            }
            else if (_fillRate < _tuning.SparseFillMax && traits.big)
            {
                multiplier *= Mathf.Max(1f, _tuning.SparseBigBoost);
            }

            return multiplier;
        }

        /// <summary>
        /// 이번 리필 기준 모양별 쏙 점수 갱신. 보드에 그 모양이 꼭 맞는 포켓이 있으면 1에 가까움.
        /// </summary>
        private void BuildSnugScores(BoardGrid snapshot)
        {
            _snugByShape.Clear();

            foreach (BlockShapeWeight entry in _tuning.BlockWeights)
            {
                if (entry.Shape == null)
                {
                    continue;
                }

                _snugByShape[entry.Shape] = SnugFitScorer.BestEnclosureAnyRotation(snapshot, entry.Shape.CellOffsets);
            }
        }

        /// <summary>
        /// 번들 추첨 가중 배수: 쏙 맞는 모양(둘레 막힘 ≥ SnugEnclosureMin)이 들어 있으면
        /// 가중 ×(1 + SnugWeightBoost × 정규화 쏙 점수) — 포켓에 맞는 블록이 패에 더 자주 나온다.
        /// </summary>
        private float SnugMultiplier(BlockBundleSO bundle)
        {
            float maxSnug = 0f;

            foreach (BlockShapeSO shape in bundle.Shapes)
            {
                if (shape != null && _snugByShape.TryGetValue(shape, out float snug))
                {
                    maxSnug = Mathf.Max(maxSnug, snug);
                }
            }

            return 1f + _tuning.SnugWeightBoost * NormalizedSnug(maxSnug);
        }

        /// <summary>번들 추첨 최종 배수 = 쏙 맞춤 × 밀도 바이어스 (Normal·Easy·Momentum 공용).</summary>
        private float CombinedMultiplier(BlockBundleSO bundle)
        {
            return SnugMultiplier(bundle) * DensityMultiplier(bundle);
        }

        /// <summary>
        /// 밀도 바이어스 (사진 분석 phase9): 빽빽하면 얇은 블록(1xN) 포함 번들 ↑, 널널하면 큰 블록(6칸+) 포함 번들 ↑.
        /// 실제 게임의 "밀도 역상관" 재현 — 꽉 찬 판엔 빠져나갈 얇은 조각, 빈 판엔 큼직한 조각.
        /// </summary>
        private float DensityMultiplier(BlockBundleSO bundle)
        {
            bool hasSlim = false;
            bool hasBig = false;

            foreach (BlockShapeSO shape in bundle.Shapes)
            {
                if (shape != null && _shapeTraits.TryGetValue(shape, out (bool slim, bool big) traits))
                {
                    hasSlim |= traits.slim;
                    hasBig |= traits.big;
                }
            }

            if (_fillRate > _tuning.DenseFillMin)
            {
                float multiplier = 1f;
                if (hasSlim)
                {
                    multiplier *= Mathf.Max(1f, _tuning.DenseSlimBoost);
                }

                if (hasBig)
                {
                    multiplier *= Mathf.Clamp01(_tuning.DenseBigPenalty);
                }

                return multiplier;
            }

            if (_fillRate < _tuning.SparseFillMax && hasBig)
            {
                return Mathf.Max(1f, _tuning.SparseBigBoost);
            }

            return 1f;
        }

        /// <summary>SnugEnclosureMin 미만은 0, 사방 밀폐(1.0)는 1로 정규화.</summary>
        private float NormalizedSnug(float snug)
        {
            if (snug < _tuning.SnugEnclosureMin)
            {
                return 0f;
            }

            float range = Mathf.Max(0.0001f, 1f - _tuning.SnugEnclosureMin);
            return Mathf.Clamp01((snug - _tuning.SnugEnclosureMin) / range);
        }

        /// <summary>후보 3피스(실제 회전 상태) 중 가장 쏙 들어가는 피스의 둘레 막힘 비율.</summary>
        private static float BestSnugOfPieces(BoardGrid snapshot, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            float best = 0f;

            foreach (IReadOnlyList<Vector2Int> piece in pieces)
            {
                best = Mathf.Max(best, SnugFitScorer.BestEnclosure(snapshot, piece));
            }

            return best;
        }

        /// <summary>
        /// 3피스를 최선으로 플레이했을 때의 보드 Health 점수 예측. 빔이 완주 경로를 못 찾으면 최저점.
        /// </summary>
        private float PredictHealthAfterBestPlay(BoardGrid snapshot, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            SequenceOutcomeEstimator.SequenceOutcome outcome =
                SequenceOutcomeEstimator.Estimate(snapshot, pieces, _tuning.OutcomeBeamWidth);

            if (!outcome.SequenceFound)
            {
                return float.MinValue;
            }

            return BoardHealthCalculator.Compute(outcome.FinalBoard, _freedomProbePieces, _tuning).Score;
        }

        /// <summary>
        /// 최후 수단: 하나라도 놓을 수 있는 샘플 핸드, 그마저 없으면 무검증 강제 샘플.
        /// </summary>
        private BlockSelectionResult ForceNormalAny(BoardGrid snapshot, BoardHealthResult health, float blame)
        {
            List<List<IReadOnlyList<Vector2Int>>> anyPlaceable = SampleValidHands(
                snapshot, BundleValidation.AnyPlaceable, maxCandidates: 1);
            if (anyPlaceable.Count > 0)
            {
                return FromGenerated(SelectionTier.Fallback, anyPlaceable[0], null, health, blame,
                    "최후 수단: 하나라도 놓을 수 있는 독립 추첨 핸드 강제");
            }

            List<IReadOnlyList<Vector2Int>> forced = ShapeSampler.Sample3Rotated(_normalShapes, _rng);
            return FromGenerated(SelectionTier.Fallback, forced, null, health, blame,
                "최후 수단: 배치 가능 핸드조차 없음 → Normal 가중치로 3피스 강제 샘플");
        }

        private BundleDraw TryPickBundle(
            BoardGrid snapshot,
            BundleTag tag,
            BundleValidation validation,
            System.Func<BlockBundleSO, float> weightMultiplier = null)
        {
            return BundleTierSelector.TryPick(
                snapshot, _bundles.GetByTag(tag), validation, _rng, _tuning.BundleProbeCount, weightMultiplier);
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
