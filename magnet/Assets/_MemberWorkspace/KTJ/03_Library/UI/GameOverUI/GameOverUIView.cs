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
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            MagnetGameChannel.AddListener<ScoreChangedEvent>(HandleScoreChangedEvent);
            MagnetGameChannel.AddListener<GameOverEvent>(HandleGameOverEvent);
            UIChannel.AddListener<UIShowGameOverEvent>(HandleUIShowGameOverEvent);
        }

        protected override void OnDisable()
        {
            MagnetGameChannel.RemoveListener<ScoreChangedEvent>(HandleScoreChangedEvent);
            MagnetGameChannel.RemoveListener<GameOverEvent>(HandleGameOverEvent);
            UIChannel.RemoveListener<UIShowGameOverEvent>(HandleUIShowGameOverEvent);

            base.OnDisable();
        }

        private void HandleGameOverEvent(GameOverEvent @event)
        {
            ViewModel.ScoreTxt = @event.FinalScore.ToString();
            UIChannel.RaiseEvent(UIEvents.UIPlayNewSkinEvent);
        }

        private void HandleUIShowGameOverEvent(UIShowGameOverEvent @event)
        {
            Container.SetActive(true);
        }

        private void HandleScoreChangedEvent(ScoreChangedEvent @event)
        {
            ViewModel.ScoreTxt = @event.TotalScore.ToString();
        }
    }
}
