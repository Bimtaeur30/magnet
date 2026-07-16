using Game.UI;
using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using Magnet.Contracts.BlockShapes;
using PMS.Scripts.Events;
using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static LitMotion.LMotion;

public class BlockSlot_UI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private EventChannelSO MagnetChannel;
    [SerializeField] private EventChannelSO SkinEventChannel;

    [SerializeField] private BlockSlotView SlotView;
    private int _index;
    private IBlockShape _shape;

    private void Awake()
    {
        SkinEventChannel.AddListener<SkinChangedResponseEvent>(HandleSkinChangedResponseEvent);
    }

    private void OnDestroy()
    {
        SkinEventChannel.AddListener<SkinChangedResponseEvent>(HandleSkinChangedResponseEvent);
    }

    public void SetSlot(IBlockShape shape, int index)
    {
        if (shape == null)
            return;

        SlotView.ViewModel.BlockImage1Texture = shape.Icon;
        _index = index;
        _shape = shape;
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
        SkinEventChannel.RaiseEvent(SkinEvents.SkinChangedRequestEvent);
        SetBlockImageAlpha(0.2f);
    }
    private void HandleSkinChangedResponseEvent(SkinChangedResponseEvent @event)
    {
        SetSlot(_shape, _index);
    }
}
