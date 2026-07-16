using Game.UI;
using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using Magnet.Contracts.BlockShapes;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockSlot_UI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private EventChannelSO MagnetChannel;
    [SerializeField] private BlockSlotView SlotView;
    private int _index;

    public void SetSlot(IBlockShape shape, int index)
    {
        if (shape == null)
            return;

        SlotView.ViewModel.BlockImage1Texture = shape.Icon;
        _index = index;
        SetBlockImageAlpha(1f);
    }

    public void EmptySlot()
    {
        SetBlockImageAlpha(0f);
        SlotView.ViewModel.BlockImage1Texture = null;
    }

    private void SetBlockImageAlpha(float alpha)
    {
        SlotView.ViewModel.BlockImage1Alpha = alpha;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("BlockSlot≈¨∏Øµ , ¿Œµ¶Ω∫: " + _index);
        MagnetChannel.RaiseEvent(MagnetGameEvents.BlockSelectedOnUIEvent.Init(_index));
        SetBlockImageAlpha(0.2f);
    }
}
