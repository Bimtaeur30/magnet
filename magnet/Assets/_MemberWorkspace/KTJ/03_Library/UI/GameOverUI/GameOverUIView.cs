using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using Mvvm;
using PTY.Scripts.Events;
using System;
using UnityEngine;

namespace Game.UI
{
public sealed partial class GameOverUIView : MvvmView<GameOverUIViewModel>
    {
        [SerializeField] private GameObject Container;
        [SerializeField] private EventChannelSO MagnetGameChannel;
        [SerializeField] private EventChannelSO UIChannel;
        protected override void Awake()
        {
            base.Awake();
            MagnetGameChannel.AddListener<ScoreChangedEvent>(HandleScoreChangedEvent);
            MagnetGameChannel.AddListener<GameOverEvent>(HandleGameOverEvent);
        }
        protected override void OnDisable()
        {
            MagnetGameChannel.RemoveListener<ScoreChangedEvent>(HandleScoreChangedEvent);
            MagnetGameChannel.RemoveListener<GameOverEvent>(HandleGameOverEvent);

        }

        private void HandleGameOverEvent(GameOverEvent @event)
        {
            Container.SetActive(true);
            UIChannel.RaiseEvent(UIEvents.UIPlayNewSkinEvent);
        }

        private void HandleScoreChangedEvent(ScoreChangedEvent @event)
        {
            ViewModel.ScoreTxt = @event.TotalScore.ToString();
        }
    }
}
