using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using Mvvm;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class ScoreUIView : MvvmView<ScoreUIViewModel>
    {
        [SerializeField] private EventChannelSO MagnetGameChannel;

        protected override void Awake()
        {
            base.Awake();
            MagnetGameChannel.AddListener<ScoreChangedEvent>(HandleScoreChangedEvent);
        }

        private void HandleScoreChangedEvent(ScoreChangedEvent @event)
        {
            ViewModel.SetCurrentScore(@event.TotalScore);
        }
    }
}
