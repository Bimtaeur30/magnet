using GameLib.EventChannelSystem;
using Magnet.Contracts.BlockSkins;
using PMS.Scripts.Events;
using PMS.Scripts.Skin;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SkinBoxContainer : MonoBehaviour
{
    [SerializeField] private EventChannelSO skinEventChannel;

    [SerializeField] private SkinBox SkinBoxPrefab;
    [SerializeField] private RectTransform SkinBoxParent;

    private IReadOnlyList<SkinDataSO> _skinDataList;
    private List<SkinBox> _skinBoxs;
    private int _currentSkinIndex = -1;

    private void Awake()
    {
        skinEventChannel.AddListener<SkinInventoryResponseEvent>(HandleSkinInventoryResponseEvent);
        // 스킨 로딩 이벤트 구독
    }

    private void Start()
    {
        skinEventChannel.RaiseEvent(SkinEvents.SkinInventoryRequestEvent);
    }

    private void OnDestroy()
    {
        skinEventChannel.RemoveListener<SkinInventoryResponseEvent>(HandleSkinInventoryResponseEvent);
    }

    private void HandleSkinInventoryResponseEvent(SkinInventoryResponseEvent evt)
    {
        Debug.Log("HandleSkinInventoryResponseEvent 받음");

        _skinDataList = evt.UnlockedSkins;
        _currentSkinIndex = evt.SelectedIndex;

        InitSkinBoxs();
    }

    private void InitSkinBoxs()
    {
        Debug.Log("스킨박스 초기화 시작");

        for (int i = 0; i < _skinDataList.Count; i++)
        {
            SkinBox box = Instantiate(SkinBoxPrefab, SkinBoxParent);
            box.Init((_skinDataList[i] as IBlockSkin), i);
            box.EquipSkinBoxEvent += HandleEquipSkinBoxEvent;

            if (_currentSkinIndex == i)
                box.SetSkinBoxEquip(true);
            else
                box.SetSkinBoxEquip(false);
        }
    }

    private void HandleEquipSkinBoxEvent(int obj)
    {
        foreach(var box in _skinBoxs)
            box.SetSkinBoxEquip(false);

        _skinBoxs[obj].SetSkinBoxEquip(true);
        skinEventChannel.RaiseEvent(SkinEvents.SkinSelectRequestEvent.Init(obj));
    }
}
