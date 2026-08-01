using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection;
using JTH.Scripts.Domain.BlockSelection.Blame;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Skin;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Events;
using JTH.Scripts.Input;
using JTH.Scripts.Presentation;
using Magnet.Contracts;
using Magnet.Core.Events;
using Magnet.Core.SO.Block;
using Magnet.Core.SO.Skin;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class BlockSpawnBootstrap : MonoBehaviour
    {
        /// <summary>1x1은 Relife 번들 전용 — 배치 자유도 프로브에서도 제외 (SPEC §12.1)</summary>
        private const string ExcludedProbeShapeId = "1x1";

        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private MagnetInputSO inputSO;
        [SerializeField] private SkinDataListSO skinDataListSO;
        [SerializeField] private BlockShapeSourceSO shapeSourceSO;
        [SerializeField, Tooltip("블록 선택 알고리즘 수치 튜닝 SO")]
        private BlockSelectionTuningSO selectionTuningSO;
        [SerializeField, Tooltip("티어별 번들 모음 SO")]
        private BlockBundlePoolSO bundlePoolSO;

        [Inject] private GameBoard _gameBoard;
        
        private BlockSupply _supply;
        private BlockSelectionDrawer _drawer;
        private BlameTracker _blameTracker;
        private List<IReadOnlyList<Vector2Int>> _probePieces;

        private int _turnIndex;
        private BoardGrid _turnStartBoard;
        private BoardHealthResult _turnStartHealth;

        // 재시작(다시 하기) 감지는 씬 리로드 방식이라 크로스-씬 상태가 필요 — 연동 전까지 Relife 게이트는 닫힘.
        private bool _isRetrySession;

        public IReadOnlyList<ShapeBlockData> Candidates => _supply.Candidates;

        /// <summary>직전 리필의 선택 결과 (티어·유일해 등). UI 훅 소비용.</summary>
        public BlockSelectionResult LastSelection => _drawer.LastResult;

        /// <summary>직전 턴 종료의 blame 판정 (GoodTurn 등). UI 훅 소비용.</summary>
        public TurnFeedback LastTurnFeedback { get; private set; }

        private void Awake()
        {
            Debug.Assert(magnetGameChannel != null, "[BlockSpawnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(inputSO != null, "[BlockSpawnBootstrap] inputSO is not assigned.", this);
            Debug.Assert(selectionTuningSO != null, "[BlockSpawnBootstrap] selectionTuningSO is not assigned.", this);
            Debug.Assert(bundlePoolSO != null, "[BlockSpawnBootstrap] bundlePoolSO is not assigned.", this);

            #if UNITY_EDITOR
            inputSO.OnSlotSelected += OnBlockSelected;
            #endif
            magnetGameChannel.AddListener<BlockSelectedOnUIEvent>(OnBlockSelected);

            _probePieces = BuildProbePieces();
            _blameTracker = new BlameTracker(selectionTuningSO);
            _drawer = new BlockSelectionDrawer(
                new BlockSelectionOrchestrator(selectionTuningSO, bundlePoolSO, new System.Random()));
            _supply = new BlockSupply(_drawer, new SkinSession(skinDataListSO));
        }
        
        private void Start()
        {
            Fill();
        }
        
        private void OnDestroy()
        {
            #if UNITY_EDITOR
            inputSO.OnSlotSelected -= OnBlockSelected;
            #endif
            magnetGameChannel.RemoveListener<BlockSelectedOnUIEvent>(OnBlockSelected);
        }

        public void Consume(int slotIndex)
        {
            _supply.Consume(slotIndex);
            magnetGameChannel.RaiseEvent(
                MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(_supply.Candidates));
        }
        
        private void OnBlockSelected(BlockSelectedOnUIEvent data)
            => OnBlockSelected(data.Index);
        
        private void OnBlockSelected(int index)
        {
            if (index < 0 || index >= BlockSupply.SlotCount)
            {
                return;
            }
        
            if (_supply.Candidates == null || index >= _supply.Candidates.Count)
            {
                return;
            }
        
            ShapeBlockData block = _supply.Candidates[index];
            if (block == null)
            {
                return;
            }
        
            inGameChannel.RaiseEvent(InGameEvents.BlockSelectedEvent.Init(index, block));
        }

        public void Fill()
        {
            BoardGrid grid = _gameBoard.Grid;
            BoardHealthResult health = BoardHealthCalculator.Compute(grid, _probePieces, selectionTuningSO);

            // 첫 리필이 아니면 직전 턴(3피스 라운드)의 blame 정산 — Fill은 lastDrop 시점에만 불리므로 전부 배치됨
            if (_turnStartBoard != null)
            {
                float blameBefore = _blameTracker.Total;
                LastTurnFeedback = _blameTracker.OnTurnEnded(
                    _turnStartBoard, grid, _turnStartHealth, health, allPiecesPlaced: true);

                LogBlameChange(blameBefore, LastTurnFeedback);
                LogHealthChange(_turnStartHealth, health);
            }

            _turnStartBoard = grid.Clone();
            _turnStartHealth = health;

            BlockSpawnContext context = new(shapeSourceSO, grid, 0)
            {
                Health = health,
                BlameTotal = _blameTracker.Total,
                IsRetrySession = _isRetrySession,
                TurnIndex = _turnIndex,
            };

            _supply.Fill(context);
            LogSelection();
            ++_turnIndex;

            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(_supply.Candidates));
        }

        /// <summary>
        /// 매 리필 1줄 진단 로그 (SPEC §20) + 티어 강조 로그(선택 이유 포함).
        /// </summary>
        private void LogSelection()
        {
            BlockSelectionResult result = _drawer.LastResult;
            string source = result.WasGenerated ? "generated" : result.BundleId;
            Debug.Log($"[BlockSelect] turn={_turnIndex} zone={result.Zone} health={result.HealthScore:F2}"
                + $" blame={result.Blame:F1} tier={result.Tier} bundle={source}");

            (string label, string color) = TierStyle(result.Tier);
            Debug.Log($"<color={color}><b>[뽑기] {label} ({result.Tier})</b> bundle={source}</color>\n"
                + result.SelectionReason);
        }

        private static (string label, string color) TierStyle(SelectionTier tier) => tier switch
        {
            SelectionTier.Relife => ("부활 접대", "#C792EA"),
            SelectionTier.Trap => ("죽음(함정)", "#FF5252"),
            SelectionTier.ComboBreak => ("콤보 브레이크", "#FFD54F"),
            SelectionTier.Hospitality => ("접대", "#69F0AE"),
            SelectionTier.Easy => ("이지", "#40C4FF"),
            SelectionTier.Pressure => ("유일수", "#FFAB40"),
            SelectionTier.Normal => ("노말", "#FFFFFF"),
            _ => ("폴백", "#B0BEC5"),
        };

        /// <summary>
        /// 턴 정산 시 Blame 증감 강조 로그 — 아랫줄에 증감 사유.
        /// </summary>
        private void LogBlameChange(float blameBefore, TurnFeedback feedback)
        {
            float netChange = feedback.TotalBlame - blameBefore;
            string color = netChange > 0f ? "#FF6E6E" : "#7DFF9E";

            List<string> reasons = new();
            if (feedback.NewDeadZones > 0)
            {
                reasons.Add($"새 dead zone {feedback.NewDeadZones}개"
                    + $" → +{feedback.NewDeadZones * selectionTuningSO.BlamePerDeadZone:F1}");
            }

            if (feedback.CenterCellsGained > 0)
            {
                reasons.Add($"중앙 2×2 점유 {feedback.CenterCellsGained}칸"
                    + $" → +{feedback.CenterCellsGained * selectionTuningSO.BlamePerCenterCell:F1}");
            }

            if (feedback.BigSlotLost)
            {
                reasons.Add($"큰 블록 슬롯 감소 → +{selectionTuningSO.BlamePerBigSlotLost:F1}");
            }

            if (feedback.FreedomDrop > 0f)
            {
                reasons.Add($"배치 자유도 {feedback.FreedomDrop:F1} 하락"
                    + $" → +{feedback.FreedomDrop * selectionTuningSO.BlamePerFreedomDrop:F1}");
            }

            if (feedback.DecayLoss > 0.05f)
            {
                reasons.Add($"감쇠 ×{selectionTuningSO.BlameDecayRate:F2} → -{feedback.DecayLoss:F1}");
            }

            if (reasons.Count == 0)
            {
                reasons.Add("변동 요인 없음");
            }

            Debug.Log($"<color={color}><b>[Blame] {blameBefore:F1} → {feedback.TotalBlame:F1}"
                + $" ({netChange:+0.0;-0.0;+0.0})</b> 이번 턴 획득 +{feedback.LastTurnDelta:F1}</color>\n"
                + string.Join(" · ", reasons));
        }

        /// <summary>
        /// 턴 정산 시 BoardHp(health) 증감 강조 로그 — 아랫줄에 증감 사유.
        /// </summary>
        private static void LogHealthChange(BoardHealthResult before, BoardHealthResult after)
        {
            float scoreChange = after.Score - before.Score;
            string color = scoreChange < 0f ? "#FF6E6E" : "#7DFF9E";

            List<string> reasons = new();
            if (!Mathf.Approximately(before.FillRate, after.FillRate))
            {
                reasons.Add($"채움률 {before.FillRate:P0} → {after.FillRate:P0}");
            }

            if (before.DeadZoneCount != after.DeadZoneCount)
            {
                reasons.Add($"dead zone {before.DeadZoneCount} → {after.DeadZoneCount}개");
            }

            if (before.BigPieceSlots != after.BigPieceSlots)
            {
                reasons.Add($"큰 블록 슬롯 {before.BigPieceSlots} → {after.BigPieceSlots}");
            }

            if (!Mathf.Approximately(before.PlacementFreedom, after.PlacementFreedom))
            {
                reasons.Add($"배치 자유도 {before.PlacementFreedom:F1} → {after.PlacementFreedom:F1}");
            }

            if (reasons.Count == 0)
            {
                reasons.Add("변동 없음");
            }

            Debug.Log($"<color={color}><b>[BoardHp] {before.Score:F2} → {after.Score:F2}"
                + $" ({scoreChange:+0.00;-0.00;+0.00})</b> zone {before.Zone} → {after.Zone}</color>\n"
                + string.Join(" · ", reasons));
        }

        /// <summary>
        /// 배치 자유도 프로브 피스 집합 — 1x1 제외 전 모양 (SPEC §12.1).
        /// </summary>
        private List<IReadOnlyList<Vector2Int>> BuildProbePieces()
        {
            List<IReadOnlyList<Vector2Int>> probePieces = new();
            foreach (BlockShapeSO shape in shapeSourceSO.Shapes)
            {
                if (shape.ShapeId == ExcludedProbeShapeId)
                {
                    continue;
                }

                probePieces.Add(shape.CellOffsets);
            }

            return probePieces;
        }
    }
}
