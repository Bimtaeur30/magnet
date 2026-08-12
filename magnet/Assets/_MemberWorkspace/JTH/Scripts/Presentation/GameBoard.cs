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

        public bool TryGetPlacedBlock(Vector2Int cell, out Block block)
        {
            return _blocksView.TryGetBlock(cell, out block);
        }

        public void SetLineClearHints(
            IReadOnlyCollection<Vector2Int> clearedCells,
            LineClearPreviewConfigSO config)
        {
            _blocksView.SetLineClearHints(clearedCells, config);
        }

        public void ClearLineClearHints()
        {
            _blocksView.ClearLineClearHints();
        }

        public void AddBlock(IReadOnlyList<Block> detached, IReadOnlyList<Vector2Int> gridOffsets)
        {
            _blocksView.PlaceStagingBlock(detached, gridOffsets);

            int count = Mathf.Min(detached.Count, gridOffsets.Count);
            for (int i = 0; i < count; i++)
            {
                Grid.SetOccupied(gridOffsets[i], true);
            }
        }

        public void ReturnUnplacedBlocks(IReadOnlyList<Block> detached) =>
            _blocksView.ReturnBlocks(detached);

        public void RemoveCellsAt(IReadOnlyCollection<Vector2Int> gridPositions)
        {
            if (gridPositions == null)
            {
                return;
            }

            foreach (Vector2Int cellPos in gridPositions)
            {
                Grid.SetOccupied(cellPos, false);
            }

            _blocksView.DestroyCellViews(gridPositions);
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
