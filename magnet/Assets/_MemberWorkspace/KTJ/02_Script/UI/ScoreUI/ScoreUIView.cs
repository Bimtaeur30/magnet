using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using Magnet.Contracts.Save;
using Mvvm;
using PTY.Scripts.Events;
using Reflex.Attributes;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class ScoreUIView : MvvmView<ScoreUIViewModel>
    {
        [SerializeField] private EventChannelSO MagnetGameChannel;
        [SerializeField, Min(0.001f)] private float secondsPerNumber = 0.02f;
        [Inject] private ISaveService _saveService;

        private Coroutine _currentScoreAnimation;
        private Coroutine _bestScoreAnimation;
        private int _displayedCurrentScore;
        private int _displayedBestScore;

        protected override void Awake()
        {
            base.Awake();

            _displayedBestScore = _saveService?.BestScore ?? 0;
            ViewModel.SetBestScore(_displayedBestScore);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            MagnetGameChannel.AddListener<ScoreChangedEvent>(HandleScoreChangedEvent);
            MagnetGameChannel.AddListener<BestScoreUpdatedEvent>(OnBestScoreUpdated);
            MagnetGameChannel.AddListener<GameOverEvent>(HandleGameOverEvent);
        }

        protected override void OnDisable()
        {
            MagnetGameChannel.RemoveListener<ScoreChangedEvent>(HandleScoreChangedEvent);
            MagnetGameChannel.RemoveListener<BestScoreUpdatedEvent>(OnBestScoreUpdated);
            MagnetGameChannel.RemoveListener<GameOverEvent>(HandleGameOverEvent);

            StopScoreAnimations();
            base.OnDisable();
        }

        private void HandleScoreChangedEvent(ScoreChangedEvent @event)
        {
            ViewModel.PlayCurrentScoreScaleAnimation();

            if (_currentScoreAnimation != null)
            {
                StopCoroutine(_currentScoreAnimation);
            }

            _currentScoreAnimation = StartCoroutine(AnimateCurrentScore(@event.TotalScore));
        }

        private void OnBestScoreUpdated(BestScoreUpdatedEvent evt)
        {
            if (_bestScoreAnimation != null)
            {
                StopCoroutine(_bestScoreAnimation);
            }

            _bestScoreAnimation = StartCoroutine(AnimateBestScore(evt.NewBestScore));
        }

        private void HandleGameOverEvent(GameOverEvent evt)
        {
            _saveService?.SubmitScore(evt.FinalScore);
        }

        private IEnumerator AnimateCurrentScore(int targetScore)
        {
            if (targetScore <= _displayedCurrentScore)
            {
                _displayedCurrentScore = targetScore;
                ViewModel.SetCurrentScore(_displayedCurrentScore);
                _currentScoreAnimation = null;
                yield break;
            }

            while (_displayedCurrentScore < targetScore)
            {
                _displayedCurrentScore++;
                ViewModel.SetCurrentScore(_displayedCurrentScore);
                yield return new WaitForSecondsRealtime(GetSecondsPerNumber(
                    targetScore - _displayedCurrentScore));
            }

            _currentScoreAnimation = null;
        }

        private IEnumerator AnimateBestScore(int targetScore)
        {
            if (targetScore <= _displayedBestScore)
            {
                _displayedBestScore = targetScore;
                ViewModel.SetBestScore(_displayedBestScore);
                _bestScoreAnimation = null;
                yield break;
            }

            while (_displayedBestScore < targetScore)
            {
                _displayedBestScore++;
                ViewModel.SetBestScore(_displayedBestScore);
                yield return new WaitForSecondsRealtime(GetSecondsPerNumber(
                    targetScore - _displayedBestScore));
            }

            _bestScoreAnimation = null;
        }

        private float GetSecondsPerNumber(int scoreDifference)
        {
            int difference = Mathf.Max(1, scoreDifference);
            return secondsPerNumber / Mathf.Sqrt(difference);
        }

        private void StopScoreAnimations()
        {
            ViewModel.StopCurrentScoreScaleAnimation();

            if (_currentScoreAnimation != null)
            {
                StopCoroutine(_currentScoreAnimation);
                _currentScoreAnimation = null;
            }

            if (_bestScoreAnimation != null)
            {
                StopCoroutine(_bestScoreAnimation);
                _bestScoreAnimation = null;
            }
        }
    }
}
