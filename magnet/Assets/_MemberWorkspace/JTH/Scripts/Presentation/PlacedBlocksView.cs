using System.Collections.Generic;
using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
using JTH.Scripts.Events;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class PlacedBlocksView : MonoBehaviour
    {
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private PoolItemSO blockBlastEffect;
        [SerializeField] private PoolManagerSO poolManagerSO;
        [SerializeField] private LineClearHintEffector lineClearHintEffector;

        private Dictionary<Vector2Int, Block> _cellsDict;

        private void Awake()
        {
            Debug.Assert(blockBlastEffect != null, "[PlacedBlocksView] blockBlastEffect is not assigned.", this);

            if (lineClearHintEffector == null)
            {
                lineClearHintEffector = GetComponent<LineClearHintEffector>();
            }

            if (lineClearHintEffector == null)
            {
                lineClearHintEffector = gameObject.AddComponent<LineClearHintEffector>();
            }

            _cellsDict = new Dictionary<Vector2Int, Block>();
        }

        public bool TryGetBlock(Vector2Int cell, out Block block)
        {
            return _cellsDict.TryGetValue(cell, out block);
        }

        public void SetLineClearHints(
            IReadOnlyCollection<Vector2Int> clearedCells,
            IReadOnlyList<Block> previewBlocks,
            int skinId)
        {
            lineClearHintEffector.SetHints(clearedCells, previewBlocks, skinId);
        }

        public void ClearLineClearHints()
        {
            lineClearHintEffector.ClearHints();
        }

        /// <summary>
        /// 스테이징 ShapeBlock을 Y 스냅한 뒤 칸 View로 분해·등록한다.
        /// </summary>
        public void PlaceStagingBlock(IReadOnlyList<Block> detached, IReadOnlyList<Vector2Int> gridOffsets)
        {
            int count = Mathf.Min(detached.Count, gridOffsets.Count);
            for (int i = 0; i < count; i++)
            {
                Vector2Int cell = gridOffsets[i];
                ReplaceCell(cell, detached[i]);
            }
        }

        public void DestroyCellViews(IEnumerable<Vector2Int> positions)
        {
            if (positions == null)
            {
                return;
            }

            foreach (Vector2Int position in positions)
            {
                if (!_cellsDict.Remove(position, out Block block))
                {
                    continue;
                }

                PushBlock(block);
            }
        }

        public void ReturnBlocks(IReadOnlyList<Block> blocks)
        {
            if (blocks == null)
            {
                return;
            }

            for (int i = 0; i < blocks.Count; ++i)
            {
                Block block = blocks[i];
                if (block != null)
                {
                    PushBlock(block);
                }
            }
        }

        private void ReplaceCell(Vector2Int cell, Block block)
        {
            if (_cellsDict.Remove(cell, out Block previous))
            {
                PushBlock(previous);
            }

            block.transform.SetParent(transform);
            block.Offset = cell;
            block.ApplySortingBand(Block.SortingBandPlaced);
            _cellsDict[cell] = block;
        }

        private void PushBlock(Block block)
        {
            poolManagerSO.Push(block);
            inGameChannel.RaiseEvent(InGameEvents.BlockDestroyedEvent.Init(block));
        }
    }
}
