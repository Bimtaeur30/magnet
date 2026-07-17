using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using PMS.Scripts.Events;
using System;
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
            Slots[i].SetSlot(evt.Candidates[i], evt.CandidateDegreesClockwise[i], i);
        };
    }
}