using GameLib.EventChannelSystem;
using System;
using _Shared.Magnet.Core.Events;
using Magnet.Core.Events;
using UnityEngine;

public class BlockSlotContainer : MonoBehaviour
{
    [SerializeField] private EventChannelSO MagnetGameChannel;
    [SerializeField] private BlockSlot_UI[] Slots;

    private void OnEnable()
    {
        MagnetGameChannel.AddListener<BlockCandidatesUpdatedEvent>(HandleBlockCandidatesUpdatedEvent);
    }

    private void OnDisable()
    {
        MagnetGameChannel.RemoveListener<BlockCandidatesUpdatedEvent>(HandleBlockCandidatesUpdatedEvent);
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
