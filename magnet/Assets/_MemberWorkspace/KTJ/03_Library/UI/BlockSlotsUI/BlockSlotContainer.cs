using GameLib.EventChannelSystem;
using UnityEngine;
using System;
using JTH.Scripts.Events;

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
    private void HandleBlockCandidatesUpdatedEvent(BlockCandidatesUpdatedEvent @event)
    {
        for (int i = 0; i < @event.Candidates.Count; i++)
        {
            Slots[i].SetSlot(@event.Candidates[i], i);
        };
    }
}