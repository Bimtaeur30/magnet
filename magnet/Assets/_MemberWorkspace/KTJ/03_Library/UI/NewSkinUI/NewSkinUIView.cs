using GameLib.EventChannelSystem;
using Mvvm;
using PMS.Scripts.Events;
using PMS.Scripts.Skin;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Game.UI
{
    public sealed partial class NewSkinUIView : MvvmView<NewSkinUIViewModel>
    {
        [SerializeField] private EventChannelSO skinEventChannel;

        private Queue<SkinDataSO> _queue;

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
    }
}
