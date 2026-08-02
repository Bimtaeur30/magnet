using System.Collections.Generic;
using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
using JTH.Scripts.Data;
using JTH.Scripts.Events;
using Magnet.Contracts;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class ShapeBlock : MonoBehaviour
    {
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private PoolManagerSO poolManagerSO;
        [SerializeField] private PoolItemSO blockItemSO;
        [SerializeField] private PlacementConfigSO placementConfig;
        [SerializeField] private Block blockPrefab;

        public IReadOnlyList<Vector2Int> CellOffsets { get; private set; }

        private readonly List<Block> _blocks = new();
        private int _skinId;
        private int _sortingBand = Block.SortingBandStaging;

        private void Awake()
        {
            Debug.Assert(placementConfig != null, "[ShapeBlock] placementConfig is not assigned.", this);
            Debug.Assert(blockPrefab != null, "[ShapeBlock] blockPrefab is not assigned.", this);
        }

        public void Show(ShapeBlockData data)
        {
            CellOffsets = data.CellOffsets;
            _skinId = data.SkinId;
            _sortingBand = Block.SortingBandStaging;
            ShowCells();
        }

        public void ShowPreview(ShapeBlockData data)
        {
            CellOffsets = data.CellOffsets;
            _skinId = data.SkinId;
            _sortingBand = Block.SortingBandPreview;
            ShowCells();
            SetAlpha(placementConfig.Visual.PreviewAlpha);
        }

        private void ShowCells()
        {
            Clear();
            while (_blocks.Count < CellOffsets.Count)
            {
                Block block = poolManagerSO.Pop<Block>(blockItemSO);
                block.transform.SetParent(transform);
                block.name = $"Block_{_blocks.Count}";
                _blocks.Add(block);
            }

            inGameChannel.RaiseEvent(InGameEvents.BlockCreatedEvent.Init(_blocks, _skinId));

            for (int i = 0; i < CellOffsets.Count; i++)
            {
                _blocks[i].Offset = CellOffsets[i];
                _blocks[i].ApplySortingBand(_sortingBand);
            }
        }

        public void Clear()
        {
            foreach (Block block in _blocks)
            {
                poolManagerSO.Push(block);

                inGameChannel.RaiseEvent(InGameEvents.BlockDestroyedEvent.Init(block));
            }
            _blocks.Clear();
        }

        public IReadOnlyList<Block> Blocks => _blocks;

        private void SetAlpha(float alpha)
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                if (_blocks[i].gameObject.activeSelf)
                {
                    _blocks[i].SetAlpha(alpha);
                }
            }
        }

        public IReadOnlyList<Block> DetachBlocks()
        {
            List<Block> detached = new List<Block>(_blocks.Count);

            foreach (Block block in _blocks)
                detached.Add(block);
            _blocks.Clear();

            return detached;
        }
    }
}
