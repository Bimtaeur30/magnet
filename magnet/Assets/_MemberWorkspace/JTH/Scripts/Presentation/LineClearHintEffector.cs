using System.Collections.Generic;
using GameLib.EventChannelSystem;
using Magnet.Core.Events;
using Magnet.Core.SO.Skin;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class LineClearHintEffector : MonoBehaviour
    {
        [SerializeField] private PlacedBlocksView placedBlocksView;
        [SerializeField] private EventChannelSO skinChannel;

        private readonly HashSet<Block> _hintedBlocks = new HashSet<Block>();
        private readonly HashSet<Block> _desired = new HashSet<Block>();
        private readonly List<Block> _removeBuffer = new List<Block>(32);

        private SkinDataSO _currentSkin;
        private int _appliedSkinId = int.MinValue;

        private void Awake()
        {
            if (placedBlocksView == null)
            {
                placedBlocksView = GetComponent<PlacedBlocksView>();
            }

            Debug.Assert(placedBlocksView != null, "[LineClearHintEffector] PlacedBlocksView is missing.", this);
            Debug.Assert(skinChannel != null, "[LineClearHintEffector] skinChannel is not assigned.", this);
        }

        private void OnEnable()
        {
            skinChannel.AddListener<SkinChangedEvent>(OnSkinChanged);
            skinChannel.AddListener<SkinInitializedEvent>(OnSkinInitialized);
        }

        private void OnDisable()
        {
            skinChannel.RemoveListener<SkinChangedEvent>(OnSkinChanged);
            skinChannel.RemoveListener<SkinInitializedEvent>(OnSkinInitialized);
            ClearHints();
        }

        public void SetHints(
            IReadOnlyCollection<Vector2Int> clearedCells,
            IReadOnlyList<Block> previewBlocks,
            int skinId)
        {
            if (clearedCells == null || clearedCells.Count == 0 || _currentSkin == null)
            {
                ClearHints();
                return;
            }

            if (_appliedSkinId != skinId)
            {
                ClearHints();
                _appliedSkinId = skinId;
            }

            _desired.Clear();
            foreach (Vector2Int cell in clearedCells)
            {
                if (placedBlocksView.TryGetBlock(cell, out Block placed))
                {
                    _desired.Add(placed);
                }
            }

            if (previewBlocks != null)
            {
                for (int i = 0; i < previewBlocks.Count; ++i)
                {
                    Block preview = previewBlocks[i];
                    if (preview != null)
                    {
                        _desired.Add(preview);
                    }
                }
            }

            Sprite unifiedSprite = _currentSkin.GetSprite(skinId);
            AnimationClip clip = _currentSkin.GetHintClip(skinId);

            SyncSet(
                _hintedBlocks,
                _desired,
                enable: block => block.SetClearHint(true, unifiedSprite, clip));
        }

        public void ClearHints()
        {
            foreach (Block block in _hintedBlocks)
            {
                if (block != null)
                {
                    block.SetClearHint(false);
                }
            }

            _hintedBlocks.Clear();
            _desired.Clear();
            _appliedSkinId = int.MinValue;
        }

        private void OnSkinChanged(SkinChangedEvent evt)
        {
            _currentSkin = evt.CurrentSkin;
        }

        private void OnSkinInitialized(SkinInitializedEvent evt)
        {
            _currentSkin = evt.Skin;
        }

        private void SyncSet(HashSet<Block> current, HashSet<Block> desired, System.Action<Block> enable)
        {
            _removeBuffer.Clear();
            foreach (Block hinted in current)
            {
                if (hinted == null || !desired.Contains(hinted))
                {
                    _removeBuffer.Add(hinted);
                }
            }

            for (int i = 0; i < _removeBuffer.Count; ++i)
            {
                Block block = _removeBuffer[i];
                if (block != null)
                {
                    block.SetClearHint(false);
                }

                current.Remove(block);
            }

            foreach (Block block in desired)
            {
                if (current.Contains(block))
                {
                    continue;
                }

                enable(block);
                current.Add(block);
            }
        }
    }
}
