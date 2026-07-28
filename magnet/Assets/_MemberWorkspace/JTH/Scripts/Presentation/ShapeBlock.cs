using System.Collections.Generic;
using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Placement;
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
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;
        [SerializeField] private Block blockPrefab;

        public IReadOnlyList<Vector2Int> CellOffsets { get; private set; }

        private readonly List<Block> _blocks = new();
        private Vector2 _centerOffset;
        private int _skinId;

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[ShapeBlock] boardConfig is not assigned.", this);
            Debug.Assert(placementConfig != null, "[ShapeBlock] placementConfig is not assigned.", this);
            Debug.Assert(blockPrefab != null, "[ShapeBlock] blockPrefab is not assigned.", this);
        }

        public void Show(ShapeBlockData data)
        {
            CellOffsets = data.CellOffsets;
            _skinId = data.SkinId;
            ShowCells();
        }

        public void ShowPreview(ShapeBlockData data)
        {
            SetAlpha(placementConfig.Visual.PreviewAlpha);
            Show(data);
        }

        private void ShowCells()
        {
            Clear();
            while (_blocks.Count < CellOffsets.Count)
            {
                Block instance = poolManagerSO.Pop<Block>(blockItemSO);
                instance.name = $"Block_{_blocks.Count}";
                _blocks.Add(instance);
            }
            
            inGameChannel.RaiseEvent(InGameEvents.BlockCreatedEvent.Init(_blocks, _skinId));
            
            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.Visual.CellFill;

            for (int i = 0; i < CellOffsets.Count; i++)
            {
                _blocks[i].Offset = CellOffsets[i];
                _blocks[i].SetLocalScale(new Vector3(cellSize * fill, cellSize * fill, 1f));
            }
            
            _centerOffset = PlacementService.GetShapeCenterOffset(CellOffsets);
        }

        public void ShowAtWorldCenter(Vector2 position)
        {
            transform.position = _centerOffset + position;
        }

        public void Clear()
        {
            foreach (Block block in _blocks)
            {
                poolManagerSO.Push(block);
            }
            _blocks.Clear();
        }

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
