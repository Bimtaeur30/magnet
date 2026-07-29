using System.Collections.Generic;
using _Shared.Magnet.Core.Events;
using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
using JTH.Scripts.Events;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class PlacedBlocksView : MonoBehaviour
    {
        [SerializeField] private EventChannelSO presentationChannel;
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private PoolItemSO blockBlastEffect;
        [SerializeField] private PoolManagerSO poolManagerSO;
        
        private Dictionary<Vector2Int, Block> _cellsDict;
        
        private void Awake()
        {
            Debug.Assert(presentationChannel != null, "[PlacedBlocksView] presentationChannel is not assigned.", this);
            Debug.Assert(blockBlastEffect != null, "[PlacedBlocksView] blockBlastEffect is not assigned.", this);
            
            _cellsDict = new Dictionary<Vector2Int, Block>();
        } //풀링을 SO로 만들면 싱글톤 방식의 문제는 단일 책임, 오픈 클로즈?, 의존관계 역전 
        
        /// <summary>
        /// 스테이징 ShapeBlock을 Y 스냅한 뒤 칸 View로 분해·등록한다.
        /// </summary>
        public void PlaceStagingBlock(IReadOnlyList<Block> detached
            , Vector2Int finalPivot, IReadOnlyList<Vector2Int> cellOffsets)
        {
            IReadOnlyList<Vector2Int> positions = cellOffsets;
        
            List<Vector2Int> gridPositions = new List<Vector2Int>(positions.Count);
            foreach (var position in positions)
            {
                gridPositions.Add(position + finalPivot);
            }
        
            SplitStagingIntoCells(detached, gridPositions);
        }
        
        private void SplitStagingIntoCells(IReadOnlyList<Block> detached, IReadOnlyList<Vector2Int> gridPositions)
        {
            for (int i = 0; i < detached.Count; i++)
            {
                Block block = detached[i];
                
                block.transform.SetParent(transform);
                block.Offset = gridPositions[i];
                
                _cellsDict.Add(gridPositions[i], block);
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
                //
                // presentationChannel.RaiseEvent(
                //     PresentationEvents.PlayParticleEffectEvent.Init(
                //         blockBlastEffect,
                //         block.transform.position,
                //         Quaternion.identity,
                //         block.Skin));
        
                poolManagerSO.Push(block);
                
                inGameChannel.RaiseEvent(InGameEvents.BlockDestroyedEvent.Init(block));
            }
        }
    }
}
