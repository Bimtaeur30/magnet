using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Domain.BlockBlast;
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

        [Inject] private GameBoard _gameBoard;

        private BlockSupply _supply;
        private BlockBlastDrawer _drawer;

        public IReadOnlyList<ShapeBlockData> Candidates => _supply.Candidates;

        /// <summary>직전 리필의 선택 결과 (알고리즘 ID 체인·블록 ID). UI 훅 소비용.</summary>
        public BlockBlastSelection LastSelection => _drawer.LastSelection;

        private void Awake()
        {
            Debug.Assert(magnetGameChannel != null, "[BlockSpawnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(inputSO != null, "[BlockSpawnBootstrap] inputSO is not assigned.", this);

            #if UNITY_EDITOR
            inputSO.OnSlotSelected += OnBlockSelected;
            #endif
            magnetGameChannel.AddListener<BlockSelectedOnUIEvent>(OnBlockSelected);

            _drawer = new BlockBlastDrawer(new BlockBlastAlgorithm(new System.Random()));
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
            BlockSpawnContext context = new(shapeSourceSO, _gameBoard.Grid, 0);
            _supply.Fill(context);
            LogSelection();

            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(_supply.Candidates));
        }

        /// <summary>매 리필 진단 로그 — 라운드·알고리즘 ID 체인·블록 ID·선택 사유.</summary>
        private void LogSelection()
        {
            BlockBlastSelection selection = _drawer.LastSelection;
            (string label, string color) = AlgoStyle(selection.ActualAlgoId);

            Debug.Log($"<color={color}><b>[BlockBlast] {label} ({selection.ActualAlgoId})</b>"
                + $" round={selection.Round} blocks=[{string.Join(",", selection.BlockIds)}]</color>\n"
                + selection.Reason);
        }

        private static (string label, string color) AlgoStyle(int actualAlgoId) => actualAlgoId switch
        {
            BlockBlastAlgorithm.AlgoAllCombinationFill => ("채움-제거(1370)", "#69F0AE"),
            BlockBlastAlgorithm.AlgoRoundLimitReplace => ("반복 방지(2100)", "#FFD54F"),
            _ => ("무사망 랜덤(7)", "#FFFFFF"),
        };
    }
}
