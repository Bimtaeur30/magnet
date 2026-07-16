using GameLib.EventChannelSystem;
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
    private readonly List<SkinBox> _skinBoxes = new();
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
        ClearSkinBoxes();
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

        ClearSkinBoxes();

        for (int i = 0; i < _skinDataList.Count; i++)
        {
            SkinBox box = Instantiate(SkinBoxPrefab, SkinBoxParent);
            box.Init(_skinDataList[i], i);
            box.EquipSkinBoxEvent += HandleEquipSkinBoxEvent;
            _skinBoxes.Add(box);

            box.SetSkinBoxEquip(_currentSkinIndex == i);
        }
    }

    private void ClearSkinBoxes()
    {
        foreach (SkinBox box in _skinBoxes)
        {
            if (box == null) continue;

            box.EquipSkinBoxEvent -= HandleEquipSkinBoxEvent;
            Destroy(box.gameObject);
        }

        _skinBoxes.Clear();
    }

    private void HandleEquipSkinBoxEvent(int obj)
    {
        if (obj < 0 || obj >= _skinBoxes.Count) return;

        foreach (SkinBox box in _skinBoxes)
            box.SetSkinBoxEquip(false);

        _skinBoxes[obj].SetSkinBoxEquip(true);
        skinEventChannel.RaiseEvent(SkinEvents.SkinSelectRequestEvent.Init(obj));
    }
}
