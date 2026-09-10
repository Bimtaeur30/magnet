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
        [Tooltip("시작 보드 프리필에서 칸 Block을 꺼낼 풀 아이템. ShapeBlock이 쓰는 것과 같은 것")]
        [SerializeField] private PoolItemSO blockItemSO;
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
            Vector2Int previewPivot,
            int skinId)
        {
            lineClearHintEffector.SetHints(clearedCells, previewBlocks, previewPivot, skinId);
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

        /// <summary>
        /// 시작 보드 프리필용. 칸마다 Block을 풀에서 꺼내 skinId(색 변형)로 등록하고 배치한다.
        /// </summary>
        public void SpawnCells(IReadOnlyList<Vector2Int> cells, IReadOnlyList<int> skinIds)
        {
            if (cells == null || skinIds == null || blockItemSO == null)
            {
                return;
            }

            int count = Mathf.Min(cells.Count, skinIds.Count);
            List<Block> spawned = new List<Block>(count);
            Dictionary<int, List<Block>> bySkinId = new Dictionary<int, List<Block>>();

            for (int i = 0; i < count; i++)
            {
                Block block = poolManagerSO.Pop<Block>(blockItemSO);
                block.transform.SetParent(transform);
                block.name = $"Prefill_{cells[i].x}_{cells[i].y}";
                spawned.Add(block);

                int skinId = skinIds[i];
                if (!bySkinId.TryGetValue(skinId, out List<Block> group))
                {
                    group = new List<Block>();
                    bySkinId[skinId] = group;
                }

                group.Add(block);
            }

            // 색 변형별로 묶어서 알린다. 스킨 매니저가 이 시점에 스프라이트를 입힌다.
            foreach (KeyValuePair<int, List<Block>> pair in bySkinId)
            {
                inGameChannel.RaiseEvent(InGameEvents.BlockCreatedEvent.Init(pair.Value, pair.Key));
            }

            for (int i = 0; i < count; i++)
            {
                ReplaceCell(cells[i], spawned[i]);
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

                lineClearHintEffector.PlayBurstForBlock(block);
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
