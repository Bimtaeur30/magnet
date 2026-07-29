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
            
            _blocksView = Instantiate(blocksViewPrefab, transform);
            BoardView boardView = Instantiate(boardViewPrefab, transform);
            
            float offset = boardConfigSO.CellCount * boardConfigSO.CellSize / 2;
            _blocksView.transform.localPosition = -Vector3.one * offset;
            boardView.transform.localPosition = -Vector3.one * offset;
        }
        
        public void AddBlock(IReadOnlyList<Block> detached
            , IReadOnlyList<Vector2Int> gridOffsets)
        {
            foreach (Vector2Int grid in gridOffsets)
            {
                Grid.SetOccupied(grid, true);
            }
            _blocksView.PlaceStagingBlock(detached, gridOffsets);
        }
        
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
        
        public Vector2 GridToWorld(Vector2Int grid)
        {
            Vector2 boardLocal = (Vector2)grid * boardConfigSO.CellSize;
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
