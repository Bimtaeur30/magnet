using GameLib.EventChannelSystem;
using Magnet.Core.Events;
using Mvvm;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class ComboUIView : MvvmView<ComboUIViewModel>
    {
        [SerializeField] private EventChannelSO MagnetGameChannel;

        protected override void OnEnable()
        {
            base.OnEnable();
            MagnetGameChannel.AddListener<ComboChangedEvent>(OnComboChanged);
        }

        protected override void OnDisable()
        {
            MagnetGameChannel.RemoveListener<ComboChangedEvent>(OnComboChanged);
            ViewModel.StopComboAnimation();
            base.OnDisable();
        }

        private void OnComboChanged(ComboChangedEvent evt)
        {
            ViewModel.ShowCombo(evt.Combo);
        }
    }
}
