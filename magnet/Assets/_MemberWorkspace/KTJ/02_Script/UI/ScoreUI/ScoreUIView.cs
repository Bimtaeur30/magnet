using GameLib.EventChannelSystem;
using Magnet.Contracts.Save;
using Mvvm;
using PTY.Scripts.Events;
using Reflex.Attributes;
using System.Collections;
using Magnet.Core.Events;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class ScoreUIView : MvvmView<ScoreUIViewModel>
    {
        [SerializeField] private EventChannelSO MagnetGameChannel;
        [SerializeField, Min(0.001f)] private float secondsPerNumber = 0.02f;
        [Inject] private ISaveService _saveService;

        private Coroutine _bestStageAnimation;
        private int _displayedBestStage;

        protected override void Awake()
        {
            base.Awake();

            _displayedBestStage = _saveService?.BestStage ?? 0;
            ViewModel.SetBestStage(_displayedBestStage);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            MagnetGameChannel.AddListener<BestStageUpdatedEvent>(OnBestStageUpdated);
        }

        protected override void OnDisable()
        {
            MagnetGameChannel.RemoveListener<BestStageUpdatedEvent>(OnBestStageUpdated);

            StopStageAnimations();
            base.OnDisable();
        }

        private void OnBestStageUpdated(BestStageUpdatedEvent evt)
        {
            if (_bestStageAnimation != null)
            {
                StopCoroutine(_bestStageAnimation);
            }

            _bestStageAnimation = StartCoroutine(AnimateBestStage(evt.NewBestStage));
        }

        private IEnumerator AnimateBestStage(int targetStage)
        {
            if (targetStage <= _displayedBestStage)
            {
                _displayedBestStage = targetStage;
                ViewModel.SetBestStage(_displayedBestStage);
                _bestStageAnimation = null;
                yield break;
            }

            while (_displayedBestStage < targetStage)
            {
                _displayedBestStage++;
                ViewModel.SetBestStage(_displayedBestStage);
                yield return new WaitForSecondsRealtime(GetSecondsPerNumber(
                    targetStage - _displayedBestStage));
            }

            _bestStageAnimation = null;
        }

        private float GetSecondsPerNumber(int stageDifference)
        {
            int difference = Mathf.Max(1, stageDifference);
            return secondsPerNumber / Mathf.Sqrt(difference);
        }

        private void StopStageAnimations()
        {
            ViewModel.StopCurrentStageScaleAnimation();

            if (_bestStageAnimation != null)
            {
                StopCoroutine(_bestStageAnimation);
                _bestStageAnimation = null;
            }
        }
    }
}
