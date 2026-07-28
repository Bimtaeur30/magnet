using Game.UI;
using System;
using _Shared.Magnet.Core.SO.Skin;
using Magnet.Core.SO.Skin;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkinBox : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject Check;
    [SerializeField] private SkinBoxView View;
    public Action<int> EquipSkinBoxEvent;
    int _myIdx;

    public void Init(SkinDataSO skin, int idx)
    {
        if (skin == null)
            throw new ArgumentNullException(nameof(skin));

        if (View == null)
            throw new InvalidOperationException("SkinBoxView is not assigned.");

        if (View.ViewModel == null)
            View.SetViewModel(new SkinBoxViewModel());

        _myIdx = idx;
        
        iew.ViewModel.SkinNameTxt = skin.SkinName;
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
