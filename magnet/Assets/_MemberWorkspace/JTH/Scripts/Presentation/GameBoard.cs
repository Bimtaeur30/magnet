using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class GameBoard : MonoBehaviour
    {
        [SerializeField] private BoardConfigSO boardConfigSO;
        [SerializeField] private PlacedBlocksView blocksViewPrefab;
        [SerializeField] private BoardView boardViewPrefab;
        
        public BoardGrid Grid { get; private set; }
        private PlacedBlocksView _blocksView;
        
        public void Awake()
        {
            Grid = new BoardGrid(boardConfigSO.CellCount);

            float offset = boardConfigSO.CellCount * boardConfigSO.CellSize / 2;
            _blocksView = Instantiate(blocksViewPrefab, Vector3.one * offset, Quaternion.identity, transform);
            Instantiate(boardViewPrefab, Vector3.one * offset, Quaternion.identity, transform);
        }
        
        public void AddBlock(IReadOnlyList<Block> detached
            , Vector2Int finalPivot, IReadOnlyList<Vector2Int> cellOffsets)
        {
            foreach (Vector2Int cellPos in cellOffsets)
            {
                Grid.SetOccupied(cellPos, true);
            }
            _blocksView.PlaceStagingBlock(detached, finalPivot, cellOffsets);
        }
        
        /// <summary>
        /// 좌표 집합에 해당하는 칸을 제거하고, 삭제된 cellId 목록을 반환한다.
        /// </summary>
        public void RemoveCellsAt(IReadOnlyCollection<Vector2Int> gridPositions)
        {
            foreach (Vector2Int cellPos in gridPositions)
            {
                Grid.SetOccupied(cellPos, false);
            }
            _blocksView.DestroyCellViews(gridPositions as IReadOnlyList<Vector2Int>);
        }
        
        public Vector2 WorldToBoardLocal(Vector2 world)
        {
            return _blocksView.transform.InverseTransformPoint(world) / boardConfigSO.CellSize;
        }
        
        public Vector2 BoardLocalToWorld(Vector2Int boardPivot)
        {
            Vector2 boardLocal = (Vector2)boardPivot * boardConfigSO.CellSize;
            return _blocksView.transform.TransformPoint(boardLocal);
        }
        
        /// <summary>
        /// 보드 최하단 좌표 리턴
        /// </summary>
        public float GetStartStagingY()
        {
            return transform.position.y - boardConfigSO.CellCount * boardConfigSO.CellSize / 2;
        }
    }
}
