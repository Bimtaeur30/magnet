using System.Collections.Generic;
using _Shared.Magnet.Core.Events;
using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
using Magnet.Core.Events;
using Magnet.Core.SO.Skin;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class LineClearHintEffector : MonoBehaviour
    {
        [SerializeField] private PlacedBlocksView placedBlocksView;
        [SerializeField] private EventChannelSO skinChannel;
        [SerializeField] private EventChannelSO presentationChannel;

        private readonly HashSet<Block> _hintedBlocks = new HashSet<Block>();
        private readonly HashSet<Block> _desired = new HashSet<Block>();
        private readonly Dictionary<Block, int> _desiredSeeds = new Dictionary<Block, int>();
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
            Debug.Assert(presentationChannel != null, "[LineClearHintEffector] presentationChannel is not assigned.", this);
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
            Vector2Int previewPivot,
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
            _desiredSeeds.Clear();
            foreach (Vector2Int cell in clearedCells)
            {
                if (placedBlocksView.TryGetBlock(cell, out Block placed))
                {
                    RememberDesired(placed, cell);
                }
            }

            if (previewBlocks != null)
            {
                for (int i = 0; i < previewBlocks.Count; ++i)
                {
                    Block preview = previewBlocks[i];
                    if (preview != null)
                    {
                        RememberDesired(preview, previewPivot + preview.Offset);
                    }
                }
            }

            Sprite unifiedSprite = _currentSkin.RandomizeSprites
                ? null
                : _currentSkin.GetSprite(skinId);
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
            _desiredSeeds.Clear();
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
                if (_desiredSeeds.TryGetValue(block, out int seed))
                {
                    block.SetShatterSeed(seed);
                }

                if (current.Contains(block))
                {
                    continue;
                }

                enable(block);
                current.Add(block);
            }
        }

        private void RememberDesired(Block block, Vector2Int cell)
        {
            _desired.Add(block);
            _desiredSeeds[block] = BlockShatterHint.SeedFromCell(cell);
        }

        public void PlayBurstForBlock(Block block)
        {
            if (block == null || _currentSkin == null || _currentSkin.FireCenteredLineClear)
            {
                return;
            }

            int effectId = _currentSkin.RandomizeSprites
                ? ResolveSpriteIndex(block.PlacedSprite)
                : (_appliedSkinId != int.MinValue
                    ? _appliedSkinId
                    : ResolveSpriteIndex(block.PlacedSprite));
            PoolItemSO effect = _currentSkin.GetLineClearEffect(effectId);
            if (effect == null)
            {
                return;
            }

            presentationChannel.RaiseEvent(
                PresentationEvents.PlayParticleEffectEvent.Init(
                    effect,
                    block.VisualCenter,
                    Quaternion.identity));
        }

        private int ResolveSpriteIndex(Sprite sprite)
        {
            if (sprite == null || _currentSkin == null || _currentSkin.Sprites == null)
            {
                return 0;
            }

            for (int i = 0; i < _currentSkin.Sprites.Length; i++)
            {
                if (_currentSkin.Sprites[i] == sprite)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
