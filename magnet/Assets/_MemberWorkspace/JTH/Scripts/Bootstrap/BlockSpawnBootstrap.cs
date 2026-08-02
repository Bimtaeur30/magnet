using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockBlast;
using JTH.Scripts.Domain.BlockSelection.Blame;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.HybridSpawn;
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
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private MagnetInputSO inputSO;
        [SerializeField] private SkinDataListSO skinDataListSO;
        [SerializeField] private BlockShapeSourceSO shapeSourceSO;
        [SerializeField] private HybridTuningSO hybridTuningSO;

        [Inject] private GameBoard _gameBoard;

        private BlockSupply _supply;
        private HybridDrawer _drawer;
        private BlameTracker _blame;

        /// <summary>직전 턴 시작(리필 직후) 시점의 보드·Health — 턴 종료 Blame 정산 입력.</summary>
        private BoardGrid _turnStartBoard;
        private BoardHealthResult _turnStartHealth;
        private bool _hasTurnStart;
        private int _turnIndex;

        public IReadOnlyList<ShapeBlockData> Candidates => _supply.Candidates;

        /// <summary>직전 리필의 선택 결과 (티어·블록 ID·유일해). UI 훅 소비용.</summary>
        public HybridSelectionResult LastSelection => _drawer.LastResult;

        private void Awake()
        {
            Debug.Assert(magnetGameChannel != null, "[BlockSpawnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(inputSO != null, "[BlockSpawnBootstrap] inputSO is not assigned.", this);
            Debug.Assert(hybridTuningSO != null, "[BlockSpawnBootstrap] hybridTuningSO is not assigned.", this);

            #if UNITY_EDITOR
            inputSO.OnSlotSelected += OnBlockSelected;
            #endif
            magnetGameChannel.AddListener<BlockSelectedOnUIEvent>(OnBlockSelected);

            System.Random rng = new();
            _blame = new BlameTracker(hybridTuningSO);
            _drawer = new HybridDrawer(
                new HybridSpawnOrchestrator(hybridTuningSO, new BlockBlastAlgorithm(rng), rng));
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
            BoardHealthResult healthNow = BoardHealthCalculator.Compute(
                grid, HybridSpawnProbes.FreedomProbePieces, hybridTuningSO);

            // 턴 종료 Blame 정산 — Fill은 3피스 소진(LastDrop) 후에만 불리므로 allPiecesPlaced = true
            if (_hasTurnStart)
            {
                TurnFeedback feedback = _blame.OnTurnEnded(
                    _turnStartBoard, grid, _turnStartHealth, healthNow, allPiecesPlaced: true);
                LogTurnFeedback(feedback);
            }

            BlockSpawnContext context = new(shapeSourceSO, grid, 0)
            {
                Health = healthNow,
                BlameTotal = _blame.Total,
                IsRetrySession = false, // 스텁 — game-over/다시 하기 흐름 구현 후 배선 (Relife 게이트)
                TurnIndex = _turnIndex,
            };
            ++_turnIndex;

            _supply.Fill(context);

            _turnStartBoard = grid.Clone();
            _turnStartHealth = healthNow;
            _hasTurnStart = true;

            LogSelection();

            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(_supply.Candidates));
        }

        /// <summary>매 리필 진단 로그 — 티어·보드 상태·블록 ID·선택 사유.</summary>
        private void LogSelection()
        {
            HybridSelectionResult result = _drawer.LastResult;
            (string label, string color) = TierStyle(result);

            Debug.Log($"<color={color}><b>[HybridSpawn] {label}</b>"
                + $" turn={_turnIndex - 1} zone={result.Zone} health={result.HealthScore:F2} blame={result.Blame:F1}"
                + $" blocks=[{string.Join(",", result.BlockIds)}]</color>\n"
                + result.Reason);
        }

        private void LogTurnFeedback(TurnFeedback feedback)
        {
            Debug.Log($"[HybridSpawn] 턴 정산 good={feedback.IsGoodTurn}"
                + $" delta={feedback.LastTurnDelta:F1} blame={feedback.TotalBlame:F1}"
                + $" (deadZone +{feedback.NewDeadZones} · center +{feedback.CenterCellsGained}"
                + $" · 개선 차감 {feedback.HealthGainRelief:F1})");
        }

        private static (string label, string color) TierStyle(HybridSelectionResult result)
        {
            switch (result.Tier)
            {
                case HybridTier.Relife:
                    return ("접대-재시작(Relife)", "#4FC3F7");
                case HybridTier.Trap:
                    return ("함정(Trap)", "#FF5252");
                case HybridTier.ComboBreak:
                    return ("콤보 차단(ComboBreak)", "#FFAB40");
                case HybridTier.Hospitality:
                    return ("접대(Hospitality)", "#69F0AE");
                case HybridTier.Pressure:
                    return ("압박-유일수(Pressure)", "#B388FF");
                default:
                    return BaseChainStyle(result.BaseSelection);
            }
        }

        private static (string label, string color) BaseChainStyle(BlockBlastSelection selection) =>
            selection.ActualAlgoId switch
            {
                BlockBlastAlgorithm.AlgoAllCombinationFill => ("체인 채움-제거(1370)", "#A5D6A7"),
                BlockBlastAlgorithm.AlgoRoundLimitReplace => ("체인 반복 방지(2100)", "#FFD54F"),
                _ => ("체인 무사망 랜덤(7)", "#FFFFFF"),
            };
    }
}
