using GameLib.EventChannelSystem;
using UnityEngine;
using System;

public class BlockSlotContainer : MonoBehaviour
{
    [SerializeField] private EventChannelSO UIChannel;
    [SerializeField] private BlockSlot_UI[] Slots;

    private void Awake()
    {
        UIChannel.AddListener<BlockSlotSetEvent>(HandleBlockSlotSetEvent);
    }

    private void HandleBlockSlotSetEvent(BlockSlotSetEvent @event)
    {
        if (@event.Index >= Slots.Length || @event.Index < 0)
        {
            Debug.LogAssertion("BlockSlotSetEventÀÇ ÀÎµ¦½º°¡ ½ÇÁ¦ ½½·Ô Å©±â¸¦ ¹þ¾î³µ½À´Ï´Ù.");
        }

        Slots[@event.Index].SetSlot(@event.Shape, @event.Skin);
    }
}