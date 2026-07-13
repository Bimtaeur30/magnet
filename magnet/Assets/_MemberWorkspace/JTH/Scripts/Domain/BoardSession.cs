using System.Collections.Generic;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Domain.Rotation;
using UnityEngine;

namespace JTH.Scripts.Domain
{
    public sealed class BoardSession
    {
        private readonly BoardGrid _grid;
        private readonly List<OccupiedCell> _cells = new();
        private readonly Dictionary<int, OccupiedCell> _cellsById = new();
        private int _nextCellId = 1;

        public BoardSession(int boardSize)
        {
            _grid = new BoardGrid(boardSize);
        }

        public BoardGrid Grid => _grid;
        public IReadOnlyList<OccupiedCell> Cells => _cells;

        /// <summary>
        /// 절대 좌표 칸들을 등록하고 cellId 목록을 반환한다.
        /// </summary>
        public IReadOnlyList<int> AddCells(IReadOnlyList<Vector2Int> absoluteCells)
        {
            var ids = new List<int>(absoluteCells.Count);
            for (int i = 0; i < absoluteCells.Count; i++)
            {
                int cellId = _nextCellId++;
                var cell = new OccupiedCell(cellId, absoluteCells[i]);
                _cells.Add(cell);
                _cellsById[cellId] = cell;
                _grid.SetOccupied(absoluteCells[i], true);
                ids.Add(cellId);
            }

            return ids;
        }

        public bool TryGetCell(int cellId, out OccupiedCell cell)
        {
            return _cellsById.TryGetValue(cellId, out cell);
        }

        public bool TryGetCellAt(Vector2Int position, out OccupiedCell cell)
        {
            for (int i = 0; i < _cells.Count; i++)
            {
                if (_cells[i].Position == position)
                {
                    cell = _cells[i];
                    return true;
                }
            }

            cell = null;
            return false;
        }

        /// <summary>
        /// 좌표 집합에 해당하는 칸을 제거하고, 삭제된 cellId 목록을 반환한다.
        /// </summary>
        public IReadOnlyList<int> RemoveCellsAt(IReadOnlyCollection<Vector2Int> cellsToRemove)
        {
            if (cellsToRemove == null || cellsToRemove.Count == 0)
            {
                return System.Array.Empty<int>();
            }

            var removeSet = cellsToRemove as HashSet<Vector2Int> ?? new HashSet<Vector2Int>(cellsToRemove);
            var removedIds = new List<int>();

            for (int i = _cells.Count - 1; i >= 0; i--)
            {
                OccupiedCell cell = _cells[i];
                if (!removeSet.Contains(cell.Position))
                {
                    continue;
                }

                _grid.SetOccupied(cell.Position, false);
                _cellsById.Remove(cell.CellId);
                removedIds.Add(cell.CellId);
                _cells.RemoveAt(i);
            }

            return removedIds;
        }

        /// <summary>
        /// 격자 점유만 해제한다. 셀 엔티티·좌표는 유지 (재배치 배정 전 이륙용).
        /// </summary>
        public void ReleaseOccupancy(int cellId)
        {
            if (!_cellsById.TryGetValue(cellId, out OccupiedCell cell))
            {
                return;
            }

            _grid.SetOccupied(cell.Position, false);
        }

        public void MoveCell(int cellId, Vector2Int to)
        {
            if (!_cellsById.TryGetValue(cellId, out OccupiedCell cell))
            {
                return;
            }

            Vector2Int from = cell.Position;
            if (from != to)
            {
                _grid.SetOccupied(from, false);
            }

            cell.SetPosition(to);
            _grid.SetOccupied(to, true);
        }

        public void RotateAllClockwise90()
        {
            for (int i = 0; i < _cells.Count; i++)
            {
                OccupiedCell cell = _cells[i];
                cell.SetPosition(GridRotation.RotateClockwise90(cell.Position));
            }

            RebuildOccupancyFromCells();
        }

        private void RebuildOccupancyFromCells()
        {
            _grid.ClearOccupancy();
            for (int i = 0; i < _cells.Count; i++)
            {
                _grid.SetOccupied(_cells[i].Position, true);
            }
        }
    }
}
