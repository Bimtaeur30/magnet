using System;
using GameLib.EventChannelSystem;
using Magnet.Core.Events;
using TMPro;
using UnityEngine;

public class Combo_UI : MonoBehaviour
{
    [SerializeField] private EventChannelSO magentGameChannel;
    [SerializeField] private TextMeshProUGUI comboTxt;
    [SerializeField] private RectTransform comboRect;

    private void Awake()
    {
        magentGameChannel.AddListener<ComboChangedEvent>(HandleComboChangedEvent);
    }

    private void OnDisable()
    {
        magentGameChannel.RemoveListener<ComboChangedEvent>(HandleComboChangedEvent); 
    }

    private void HandleComboChangedEvent(ComboChangedEvent obj)
    {
        comboTxt.text = obj.Combo.ToString();
    }
}
