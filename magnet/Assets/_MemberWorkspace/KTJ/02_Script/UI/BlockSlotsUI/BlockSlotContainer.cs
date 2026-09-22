using GameLib.EventChannelSystem;
using System;
using _Shared.Magnet.Core.Events;
using Magnet.Core.Events;
using UnityEngine;

public class BlockSlotContainer : MonoBehaviour
{
    [SerializeField] private EventChannelSO MagnetGameChannel;
    [Tooltip("블록 선택 확정과 선택 종료를 수신하는 인게임 이벤트 채널.")]
    [SerializeField] private EventChannelSO inGameChannel;
    [SerializeField] private BlockSlot_UI[] Slots;

    private void OnEnable()
    {
        MagnetGameChannel.AddListener<BlockCandidatesUpdatedEvent>(HandleBlockCandidatesUpdatedEvent);
        if (inGameChannel != null)
        {
            inGameChannel.AddListener<BlockSelectedEvent>(HandleBlockSelected);
            inGameChannel.AddListener<BlockSelectionEndedEvent>(HandleSelectionEnded);
        }
    }

    private void OnDisable()
    {
        MagnetGameChannel.RemoveListener<BlockCandidatesUpdatedEvent>(HandleBlockCandidatesUpdatedEvent);
        if (inGameChannel != null)
        {
            inGameChannel.RemoveListener<BlockSelectedEvent>(HandleBlockSelected);
            inGameChannel.RemoveListener<BlockSelectionEndedEvent>(HandleSelectionEnded);
        }
        foreach (var slot in Slots)
            if (slot != null) slot.SetSelectionHidden(false);
    }

    private void HandleBlockSelected(BlockSelectedEvent evt)
    {
        for (int i = 0; i < Slots.Length; i++)
            if (Slots[i] != null) Slots[i].SetSelectionHidden(i == evt.SlotIndex);
    }

    private void HandleSelectionEnded(BlockSelectionEndedEvent evt)
    {
        if (evt.SlotIndex >= 0 && evt.SlotIndex < Slots.Length && Slots[evt.SlotIndex] != null)
            Slots[evt.SlotIndex].SetSelectionHidden(false);
    }
    private void HandleBlockCandidatesUpdatedEvent(BlockCandidatesUpdatedEvent evt)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == null)
                continue;

            if (i >= evt.Candidates.Count)
            {
                Slots[i].EmptySlot();
                continue;
            }

            var shape = evt.Candidates[i];

            if (shape == null)
            {
                Slots[i].EmptySlot();
                continue;
            }

            Slots[i].SetSlot(
                shape.CellOffsets,
                shape.SkinId,
                i);
        }
    }
}
