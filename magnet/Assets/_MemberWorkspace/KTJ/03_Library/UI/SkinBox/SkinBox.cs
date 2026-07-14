using Game.UI;
using Magnet.Contracts.BlockSkins;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkinBox : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject Check;
    [SerializeField] private SkinBoxView View;
    public Action<int> EquipSkinBoxEvent;
    int _myIdx;

    public void Init(IBlockSkin skin, int idx)
    {
        _myIdx = idx;
        View.ViewModel.SkinNameTxt = skin.SkinName;
        View.ViewModel.Pattern = skin.Sprites[0];
    }

    public void SetSkinBoxEquip(bool value)
    {
        Check.SetActive(value);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        EquipSkinBoxEvent?.Invoke(_myIdx);
    }
}