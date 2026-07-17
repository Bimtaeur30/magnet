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
    private int _candidateDegreesClockwise;

    private void Awake()
    {
        SkinEventChannel.AddListener<SkinChangedResponseEvent>(HandleSkinChangedResponseEvent);
    }

    private void OnDestroy()
    {
        SkinEventChannel.RemoveListener<SkinChangedResponseEvent>(HandleSkinChangedResponseEvent);
    }

    public void SetSlot(IBlockShape shape, int candidateDegreesClockwise, int index)
    {
        SlotView.ViewModel.BlockImage1Texture = shape.Icon;
        _index = index;
        //Shape가 null일 때 리턴하면 이게 사용한 _shape인지 사용하지 않은 shape인지 알 수가 없어서 일단 null로 만듦.
        _shape = shape;
        // DegreesClockwise → Unity UI Z: +Z는 반시계이므로 부호 반전.
        _candidateDegreesClockwise = candidateDegreesClockwise;
        SlotView.ViewModel.BlockImage1RotationZ = -_candidateDegreesClockwise;
        SetBlockImageAlpha(1f);
    }

    public void EmptySlot()
    {
        _shape = null;
        SetBlockImageAlpha(0f);
        SlotView.ViewModel.BlockImage1Texture = null;
    }

    private void SetBlockImageAlpha(float alpha)
    {
        SlotView.ViewModel.BlockImage1Alpha = alpha;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("BlockSlotŬ����, �ε���: " + _index);
        MagnetChannel.RaiseEvent(MagnetGameEvents.BlockSelectedOnUIEvent.Init(_index));
    }
    private void HandleSkinChangedResponseEvent(SkinChangedResponseEvent @event)
    {
        SetSlot(_shape, _candidateDegreesClockwise, _index);
    }
}
