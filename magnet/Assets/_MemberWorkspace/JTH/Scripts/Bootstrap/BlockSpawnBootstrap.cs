using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.AreaBundleSpawn;
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
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private MagnetInputSO inputSO;
        [SerializeField] private SkinDataListSO skinDataListSO;
        [SerializeField] private BlockShapeSourceSO shapeSourceSO;
        [SerializeField] private AreaBundlePoolSO areaBundlePoolSO;

        [Inject] private GameBoard _gameBoard;

        private BlockSupply _supply;
        private AreaBundleDrawer _drawer;
        private int _turnIndex;

        public IReadOnlyList<ShapeBlockData> Candidates => _supply.Candidates;

        public AreaBundleSelectionResult LastSelection => _drawer?.LastResult;

        private void Awake()
        {
            Debug.Assert(magnetGameChannel != null, "[BlockSpawnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(inputSO != null, "[BlockSpawnBootstrap] inputSO is not assigned.", this);
            Debug.Assert(areaBundlePoolSO != null, "[BlockSpawnBootstrap] areaBundlePoolSO is not assigned.", this);

            #if UNITY_EDITOR
            inputSO.OnSlotSelected += OnBlockSelected;
            #endif
            magnetGameChannel.AddListener<BlockSelectedOnUIEvent>(OnBlockSelected);

            _drawer = new AreaBundleDrawer(new AreaBundleOrchestrator(areaBundlePoolSO));
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
            BlockSpawnContext context = new(shapeSourceSO, grid, 0)
            {
                TurnIndex = _turnIndex,
                IsRetrySession = false,
            };
            ++_turnIndex;

            _supply.Fill(context);
            LogSelection();
            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(_supply.Candidates));
        }

        private void LogSelection()
        {
            AreaBundleSelectionResult result = _drawer.LastResult;
            (string label, string color) = TierStyle(result);

            Debug.Log($"<color={color}><b>[AreaBundle] {label}</b>"
                + $" turn={_turnIndex - 1} boardArea={result.BoardAreaScore:F1}"
                + $" predArea={result.PredictedAreaScore:F1}"
                + $" seq={result.SequenceCount}"
                + $" kill={result.IsKillHand}"
                + $" bundle={result.BundleId}"
                + $" blocks=[{string.Join(",", result.BlockIds)}]\n"
                + result.Reason
                + "</color>");
        }

        private static (string label, string color) TierStyle(AreaBundleSelectionResult result) =>
            (result.Tier, result.IsKillHand, result.Profile) switch
            {
                (AreaBundleTier.Unique, _, _) => ("유일수", "#B388FF"),
                (AreaBundleTier.AllClear, _, _) => ("올클리어", "#FFD54F"),
                (AreaBundleTier.Hospitality, _, _) => ("접대", "#FF1744"),
                (AreaBundleTier.Easy, true, _) => ("Easy-랜덤", "#FFAB40"),
                (AreaBundleTier.Easy, false, _) => ("Easy", "#4FC3F7"),
                (AreaBundleTier.Normal, true, _) => ("Normal-폴백중", "#FFAB40"),
                (AreaBundleTier.Normal, false, ShapeWeightProfile.Clean) => ("Normal-Clean", "#A5D6A7"),
                (AreaBundleTier.Normal, false, _) => ("Normal-Main", "#66BB6A"),
                _ => ("Normal", "#A5D6A7"),
            };
    }
}