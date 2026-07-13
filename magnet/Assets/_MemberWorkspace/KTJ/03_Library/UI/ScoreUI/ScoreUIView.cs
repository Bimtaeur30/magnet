using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using Mvvm;
using PTY.Scripts.Events;
using PTY.Scripts.Save;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class ScoreUIView : MvvmView<ScoreUIViewModel>
    {
        [SerializeField] private EventChannelSO MagnetGameChannel;
        [Inject] private ISaveService _saveService;

        protected override void Awake()
        {
            base.Awake();
            MagnetGameChannel.AddListener<ScoreChangedEvent>(HandleScoreChangedEvent);
            MagnetGameChannel.AddListener<BestScoreUpdatedEvent>(OnBestScoreUpdated);

            UpdateText(_saveService.BestScore);
        }
        protected override void OnDisable()
        {
            MagnetGameChannel.RemoveListener<BestScoreUpdatedEvent>(OnBestScoreUpdated);
        }

        private void HandleScoreChangedEvent(ScoreChangedEvent @event)
        {
            ViewModel.SetCurrentScore(@event.TotalScore);
        }

        private void OnBestScoreUpdated(BestScoreUpdatedEvent evt)
        {
            UpdateText(evt.NewBestScore);
        }

        private void UpdateText(int bestScore)
        {
            ViewModel.BestScoreTxt = bestScore.ToString();
        }
    }
}
