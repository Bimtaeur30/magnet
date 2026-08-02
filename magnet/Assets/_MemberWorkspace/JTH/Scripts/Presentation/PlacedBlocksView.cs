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
        
        private Dictionary<Vector2Int, Block> _cellsDict;
        
        private void Awake()
        {
            Debug.Assert(blockBlastEffect != null, "[PlacedBlocksView] blockBlastEffect is not assigned.", this);
            
            _cellsDict = new Dictionary<Vector2Int, Block>();
        } //풀링을 SO로 만들면 싱글톤 방식의 문제는 단일 책임, 오픈 클로즈?, 의존관계 역전 
        
        /// <summary>
        /// 스테이징 ShapeBlock을 Y 스냅한 뒤 칸 View로 분해·등록한다.
        /// </summary>
        public void PlaceStagingBlock(IReadOnlyList<Block> detached
            , IReadOnlyList<Vector2Int> gridOffsets)
        {
            for (int i = 0; i < detached.Count; i++)
            {
                Block block = detached[i];
                
                block.transform.SetParent(transform);
                block.Offset = gridOffsets[i];
                
                _cellsDict.Add(gridOffsets[i], block);
            }
        }
        public void DestroyCellViews(IReadOnlyList<Vector2Int> positions)
        {
            foreach (Vector2Int position in positions)
            {
                if (!_cellsDict.Remove(position, out Block block))
                {
                    continue;
                }

                poolManagerSO.Push(block);
                
                inGameChannel.RaiseEvent(InGameEvents.BlockDestroyedEvent.Init(block));
            }
        }
    }
}
