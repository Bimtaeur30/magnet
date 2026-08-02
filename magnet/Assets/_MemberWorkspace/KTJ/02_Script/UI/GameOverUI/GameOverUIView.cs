using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using Mvvm;
using Magnet.Core.Events;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class GameOverUIView : MvvmView<GameOverUIViewModel>
    {
        [SerializeField] private GameObject Container;
        [SerializeField] private EventChannelSO MagnetGameChannel;
        [SerializeField] private EventChannelSO UIChannel;
        [SerializeField] private EventChannelSO soundChannel;
        [SerializeField] private SoundClipSO gameOverSound;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            MagnetGameChannel.AddListener<GameOverEvent>(HandleGameOverEvent);
            UIChannel.AddListener<UIShowGameOverEvent>(HandleUIShowGameOverEvent);
        }

        protected override void OnDisable()
        {
            MagnetGameChannel.RemoveListener<GameOverEvent>(HandleGameOverEvent);
            UIChannel.RemoveListener<UIShowGameOverEvent>(HandleUIShowGameOverEvent);

            base.OnDisable();
        }

        private void HandleGameOverEvent(GameOverEvent @event)
        {
            if (soundChannel != null && gameOverSound != null)
                soundChannel.RaiseEvent(SoundSystemEvents.PlaySoundEvent.Init(gameOverSound));

            ViewModel.StageTxt = @event.FinalStage.ToString();
            UIChannel.RaiseEvent(UIEvents.UIPlayNewSkinEvent);
        }

        private void HandleUIShowGameOverEvent(UIShowGameOverEvent @event)
        {
            Container.SetActive(true);
        }
    }
}
