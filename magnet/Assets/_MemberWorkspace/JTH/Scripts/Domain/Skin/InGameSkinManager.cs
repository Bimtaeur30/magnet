using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Core.Events;
using Magnet.Core.SO.Skin;
using UnityEngine;

namespace JTH.Scripts.Domain.Skin
{
    public class InGameSkinManager : MonoBehaviour
    {
        [SerializeField] private SkinDataListSO skinDataListSO;
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private EventChannelSO magnetGameChannel;

        private Dictionary<Block, int> _blockDict;

        private void Awake()
        {
            _blockDict = new Dictionary<Block, int>();
            
            inGameChannel.AddListener<ShapeBlockCreatedEvent>(BlockCreatedHandler);
            magnetGameChannel.AddListener<SkinChangedEvent>(SkinChangedHandler);
        }

        private void OnDestroy()
        {
            inGameChannel.RemoveListener<ShapeBlockCreatedEvent>(BlockCreatedHandler);
            magnetGameChannel.RemoveListener<SkinChangedEvent>(SkinChangedHandler);
        }

        private void BlockCreatedHandler(ShapeBlockCreatedEvent evt)
        {
            foreach (Block block in evt.Blocks)
            {
                _blockDict.Add(block, evt.SkinId);
            }
        }

        private void SkinChangedHandler(SkinChangedEvent evt)
        {
            foreach (Block block in _blockDict.Keys)
            {
                block.ApplySkin(evt.CurrentSkin.Sprites[_blockDict[block]]);
            }
        }
    }
}
