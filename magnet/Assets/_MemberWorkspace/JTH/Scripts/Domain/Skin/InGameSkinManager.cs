using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Core.Events;
using UnityEngine;

namespace JTH.Scripts.Domain.Skin
{
    public class InGameSkinManager : MonoBehaviour
    {
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private EventChannelSO skinChannel;

        private Dictionary<Block, int> _blockDict;
        
        private Sprite[] _currentSkin;

        private void Awake()
        {
            _blockDict = new Dictionary<Block, int>();

            inGameChannel.AddListener<BlockCreatedEvent>(BlockCreatedHandler);
            inGameChannel.AddListener<BlockDestroyedEvent>(BlockDestroyedHandler);
            skinChannel.AddListener<SkinChangedEvent>(SkinChangedHandler);
            skinChannel.AddListener<SkinInitializedEvent>(SkinInitializedHandler);
        }

        private void OnDestroy()
        {
            inGameChannel.RemoveListener<BlockCreatedEvent>(BlockCreatedHandler);
            inGameChannel.RemoveListener<BlockDestroyedEvent>(BlockDestroyedHandler);
            skinChannel.RemoveListener<SkinChangedEvent>(SkinChangedHandler);
            skinChannel.RemoveListener<SkinInitializedEvent>(SkinInitializedHandler);
        }

        private void BlockCreatedHandler(BlockCreatedEvent evt)
        {
            foreach (Block block in evt.Blocks)
            {
                _blockDict.Add(block, evt.SkinId);
                block.ApplySkin(_currentSkin[evt.SkinId]);
            }
        }

        private void BlockDestroyedHandler(BlockDestroyedEvent evt) { _blockDict.Remove(evt.Block); }

        private void SkinChangedHandler(SkinChangedEvent evt)
        {
            _currentSkin = evt.CurrentSkin.Sprites;
            ApplySkin();
        }

        private void SkinInitializedHandler(SkinInitializedEvent evt)
        {
            _currentSkin = evt.Skin.Sprites;
            ApplySkin();
        }

        private void ApplySkin()
        {
            foreach (Block block in _blockDict.Keys)
            {
                block.ApplySkin(_currentSkin[_blockDict[block]]);
            }
        }
    }
}
