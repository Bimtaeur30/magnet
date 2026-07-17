using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using Mvvm;
using PMS.Scripts.Events;
using PMS.Scripts.Skin;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;
namespace Game.UI
{
    public sealed partial class NewSkinUIView : MvvmView<NewSkinUIViewModel>
    {
        [SerializeField] private GameObject container;
        [SerializeField] private EventChannelSO skinEventChannel;
        [SerializeField] private EventChannelSO uiEventChannel;
        [SerializeField] private Button continueBtn;
        //[SerializeField] private Button equipBtn;

        private readonly Queue<SkinDataSO> _queue = new();

        protected override void Awake()
        {
            base.Awake();

            //magnetEventChannel.AddListener<GameOverEvent>(HandleGameOverEvent);
            skinEventChannel.AddListener<SkinUnlockedEvent>(HandleSkinUnlockedEvent);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            //equipBtn.onClick.AddListener(HandleEquipBtnClick);
            continueBtn.onClick.AddListener(HandleContinueBtnClick);
            uiEventChannel.AddListener<UIPlayNewSkinEvent>(HandleUIPlayNewSkinEvent);
        }

        protected override void OnDisable()
        {
            //magnetEventChannel.RemoveListener<GameOverEvent>(HandleGameOverEvent);

            //equipBtn.onClick.RemoveListener(HandleEquipBtnClick);
            continueBtn.onClick.RemoveListener(HandleContinueBtnClick);
            uiEventChannel.RemoveListener<UIPlayNewSkinEvent>(HandleUIPlayNewSkinEvent);

            base.OnDisable();
        }

        private void OnDestroy()
        {
            skinEventChannel.RemoveListener<SkinUnlockedEvent>(HandleSkinUnlockedEvent);
        }

        private void HandleSkinUnlockedEvent(SkinUnlockedEvent evt)
        {
            if (evt.SkinData == null) return;

            _queue.Enqueue(evt.SkinData);
        }

        private void HandleContinueBtnClick()
        {
            PlayNewSkin();
        }

        private void PlayNewSkin()
        {
            if (_queue.Count == 0)
            {
                CompleteNewSkinSequence();
                return;
            }

            SkinDataSO data = _queue.Dequeue();
            ViewModel.Skim = data.icon;
            ViewModel.TitleTxt = data.SkinName;
        }

        private void HandleUIPlayNewSkinEvent(UIPlayNewSkinEvent @event)
        {
            if (_queue.Count == 0)
            {
                CompleteNewSkinSequence();
                return;
            }

            container.SetActive(true);
            PlayNewSkin();
        }

        private void CompleteNewSkinSequence()
        {
            container.SetActive(false);
            uiEventChannel.RaiseEvent(UIEvents.UIShowGameOverEvent);
        }
    }
}
