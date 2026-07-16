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

        private Queue<SkinDataSO> _queue;

        protected override void Awake()
        {
            //magnetEventChannel.AddListener<GameOverEvent>(HandleGameOverEvent);

            //equipBtn.onClick.AddListener(() => HandleEquipBtnClick());
            continueBtn.onClick.AddListener(() => HandleContinueBtnClick());
            uiEventChannel.AddListener<UIPlayNewSkinEvent>(HandleUIPlayNewSkinEvent);
        }

        protected override void OnDisable()
        {
            //magnetEventChannel.RemoveListener<GameOverEvent>(HandleGameOverEvent);

            //equipBtn.onClick.RemoveListener(() => HandleEquipBtnClick());
            continueBtn.onClick.RemoveListener(() => HandleContinueBtnClick());
            uiEventChannel.RemoveListener<UIPlayNewSkinEvent>(HandleUIPlayNewSkinEvent);
        }

        private void Start() // Start나 Awake 알아서
        {
            skinEventChannel.AddListener<SkinUnlockedEvent>(HandleSkinUnlockedEvent);
            // SkinEvents에 SkinSelectRequestEvent를 스킨 바꿨을때 그 스킨 인덱스 넣어서 Raise해주기
            // 위에 주석은 여기서 하는 게 아닐 수도 있음 그냥 스킨 바꾸는 곳에서 이벤트 발행해주면 됨 ㅇㅇ
            
        }

        private void OnDestroy()
        {
            skinEventChannel.RemoveListener<SkinUnlockedEvent>(HandleSkinUnlockedEvent);
        }

        private void HandleSkinUnlockedEvent(SkinUnlockedEvent evt)
        {
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
                container.gameObject.SetActive(false);
                return;
            }

            SkinDataSO data = _queue.Dequeue();
            ViewModel.Skim = data.icon;
            ViewModel.TitleTxt = data.SkinName;
        }

        private void HandleUIPlayNewSkinEvent(UIPlayNewSkinEvent @event)
        {
            container.gameObject.SetActive(true);
            PlayNewSkin();
        }
    }
}
