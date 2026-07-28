using GameLib.EventChannelSystem;
using System;
using _Shared.Magnet.Core.Events;
using Magnet.Core.Events;
using UnityEngine;

public class BlockSlotContainer : MonoBehaviour
{
    [SerializeField] private EventChannelSO MagnetGameChannel;
    [SerializeField] private BlockSlot_UI[] Slots;

    private void Awake()
    {
        MagnetGameChannel.AddListener<BlockCandidatesUpdatedEvent>(HandleBlockCandidatesUpdatedEvent);
    }

    private void OnDisable()
    {
        MagnetGameChannel.RemoveListener<BlockCandidatesUpdatedEvent>(HandleBlockCandidatesUpdatedEvent);
    }
    private void HandleBlockCandidatesUpdatedEvent(BlockCandidatesUpdatedEvent evt)
    {
        for (int i = 0; i < evt.Candidates.Count; i++)
        {
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
