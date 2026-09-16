using System.Collections.Generic;
using GameLib.EventChannelSystem;
using Magnet.Contracts;
using Magnet.Core.Events;
using Magnet.Core.SO.Skin;
using UnityEngine;

namespace JTH.Scripts.Domain.Skin
{
    public class InGameSkinManager : MonoBehaviour
    {
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private EventChannelSO skinChannel;

        private Dictionary<IBlockSkinTarget, int> _blockDict;
        private readonly Dictionary<IBlockSkinTarget, int> _visualIndex = new Dictionary<IBlockSkinTarget, int>();
        
        private SkinDataSO _currentSkin;

        private void Awake()
        {
            _blockDict = new Dictionary<IBlockSkinTarget, int>();

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
            foreach (IBlockSkinTarget block in evt.Blocks)
            {
                _blockDict.Add(block, evt.SkinId);
                if (_currentSkin != null)
                {
                    block.ApplySkin(_currentSkin.GetSprite(ResolveVisualIndex(block, evt.SkinId)));
                }
            }
        }

        private void BlockDestroyedHandler(BlockDestroyedEvent evt)
        {
            _blockDict.Remove(evt.Block);
            _visualIndex.Remove(evt.Block);
        }

        private void SkinChangedHandler(SkinChangedEvent evt)
        {
            _currentSkin = evt.CurrentSkin;
            _visualIndex.Clear();
            ApplySkin();
        }

        private void SkinInitializedHandler(SkinInitializedEvent evt)
        {
            _currentSkin = evt.Skin;
            _visualIndex.Clear();
            ApplySkin();
        }

        private void ApplySkin()
        {
            if (_currentSkin == null)
            {
                return;
            }

            foreach (IBlockSkinTarget block in _blockDict.Keys)
            {
                block.ApplySkin(_currentSkin.GetSprite(ResolveVisualIndex(block, _blockDict[block])));
            }
        }

        private int ResolveVisualIndex(IBlockSkinTarget block, int skinId)
        {
            if (_currentSkin != null && _currentSkin.RandomizeSprites)
            {
                if (_visualIndex.TryGetValue(block, out int stored))
                {
                    return stored;
                }

                int picked = _currentSkin.PickVisualIndex(skinId);
                _visualIndex[block] = picked;
                return picked;
            }

            return skinId;
        }
    }
}
