using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Events;
using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace JTH.Scripts.Input
{
    /// <summary>
    /// MagnetInputSO 슬롯 선택 이벤트 → BlockSelectedEvent Raise.
    /// </summary>
    public sealed class BlockSelectionInput : MonoBehaviour
    {
        [SerializeField] private MagnetInputSO magnetInput;
        [SerializeField] private EventChannelSO magnetGameChannel;

        private IReadOnlyList<IBlockShape> _candidates;

        private void OnEnable()
        {
            Debug.Assert(magnetInput != null, "[BlockSelectionInput] magnetInput is not assigned.", this);
            Debug.Assert(magnetGameChannel != null, "[BlockSelectionInput] magnetGameChannel is not assigned.", this);
            magnetInput.OnSlotSelected += OnSlotSelected;
            magnetGameChannel.AddListener<BlockCandidatesUpdatedEvent>(OnCandidatesUpdated);
        }

        private void OnDisable()
        {
            if (magnetInput != null)
            {
                magnetInput.OnSlotSelected -= OnSlotSelected;
            }

            if (magnetGameChannel != null)
            {
                magnetGameChannel.RemoveListener<BlockCandidatesUpdatedEvent>(OnCandidatesUpdated);
            }
        }

        private void OnCandidatesUpdated(BlockCandidatesUpdatedEvent evt)
        {
            _candidates = evt.Candidates;
        }

        private void OnSlotSelected(int slotIndex)
        {
            TrySelect(slotIndex);
        }

        private void TrySelect(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= BlockSupply.SlotCount)
            {
                return;
            }

            if (_candidates == null || slotIndex >= _candidates.Count)
            {
                return;
            }

            IBlockShape shape = _candidates[slotIndex];
            if (shape == null)
            {
                return;
            }

            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockSelectedEvent.Init(slotIndex, shape));
            Debug.Log($"[BlockSelection] Slot {slotIndex} selected ({shape.ShapeId})");
        }
    }
}
