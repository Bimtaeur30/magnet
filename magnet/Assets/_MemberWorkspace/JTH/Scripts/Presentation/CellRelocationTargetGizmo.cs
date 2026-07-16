using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Clear;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 재배치 후보 기즈모. 원점–원래칸 복도·시드·후보·최종(또는 제자리)을 그린다.
    /// </summary>
    public sealed class CellRelocationTargetGizmo : MonoBehaviour
    {
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;

        [Tooltip("재배치될 칸의 원래 격자 위치")]
        [SerializeField] private Vector2Int originalCell = new(0, 4);

        [Tooltip("이미 점유된 칸 (이 점유 기준으로만 결과를 그림)")]
        [SerializeField] private Vector2Int[] seedOccupied =
        {
            new(0, 1),
        };

        [Tooltip("노랑 와이어 — 원래 칸(originalCell) 위치")]
        [SerializeField] private Color originalCellColor = new(1f, 0.85f, 0.1f, 0.95f);

        [Tooltip("주황/살몬 채움 — 시드 점유 칸(seedOccupied)")]
        [SerializeField] private Color seedOccupiedColor = new(1f, 0.35f, 0.2f, 0.9f);

        [Tooltip("민트 초록 와이어 — 복도 안 이동 가능 후보 칸")]
        [SerializeField] private Color candidateColor = new(0.2f, 0.95f, 0.55f, 0.55f);

        [Tooltip("하늘 파랑 채움 — TryFind가 고른 최종 목표 칸(안쪽만)")]
        [SerializeField] private Color chosenColor = new(0.3f, 0.55f, 1f, 0.85f);

        [Tooltip("회색 채움 — 안쪽이 막혀 원래 칸에 제자리")]
        [SerializeField] private Color stayColor = new(0.7f, 0.7f, 0.75f, 0.9f);

        [Tooltip("흰 선 — 원점~원래칸 수선 복도(CorridorHalfWidth)")]
        [SerializeField] private Color corridorColor = new(1f, 1f, 1f, 0.85f);

        [Tooltip("노란 축 선 — 원점(0,0) → 원래 칸 방향")]
        [SerializeField] private Color axisColor = new(1f, 0.9f, 0.2f, 0.9f);

        private void OnDrawGizmos()
        {
            if (boardConfig == null || placementConfig == null)
            {
                return;
            }

            if (BoardCoordinates.IsMagnetCell(originalCell.x, originalCell.y))
            {
                return;
            }

            float cellSize = boardConfig.CellSize;
            float halfWidth = placementConfig.CorridorHalfWidth;

            // 시드가 있으면 그 점유만 사용 (빈 보드 결과를 겹쳐 그리지 않음)
            BoardGrid grid = CreateGrid(boardConfig.BoardSize, seedOccupied);

            var candidates = new HashSet<Vector2Int>();
            CellRelocationTargetFinder.CollectCorridorCandidates(grid, originalCell, halfWidth, candidates);

            bool found = CellRelocationTargetFinder.TryFind(
                grid,
                originalCell,
                halfWidth,
                out Vector2Int target);

            DrawCorridor(originalCell, halfWidth, cellSize);
            DrawAxis(originalCell, cellSize);

            Gizmos.color = candidateColor;
            foreach (Vector2Int cell in candidates)
            {
                DrawCell(cell, cellSize, wire: true);
            }

            if (found && target != originalCell)
            {
                Gizmos.color = chosenColor;
                DrawCell(target, cellSize, wire: false);
            }
            else
            {
                // 제자리 — 회색으로 원래 칸 강조
                Gizmos.color = stayColor;
                DrawCell(originalCell, cellSize, wire: false);
            }

            if (seedOccupied != null)
            {
                Gizmos.color = seedOccupiedColor;
                for (int i = 0; i < seedOccupied.Length; i++)
                {
                    DrawCell(seedOccupied[i], cellSize, wire: false);
                }
            }

            // 원래 칸은 항상 마지막에 (노랑 와이어) — 안 보이던 문제 방지
            Gizmos.color = originalCellColor;
            DrawCell(originalCell, cellSize, wire: true);
        }

        private static BoardGrid CreateGrid(int boardSize, Vector2Int[] seedOccupied)
        {
            var grid = new BoardGrid(boardSize);
            if (seedOccupied == null)
            {
                return grid;
            }

            for (int i = 0; i < seedOccupied.Length; i++)
            {
                Vector2Int cell = seedOccupied[i];
                if (BoardCoordinates.IsMagnetCell(cell.x, cell.y))
                {
                    continue;
                }

                grid.SetOccupied(cell, true);
            }

            return grid;
        }

        private static void DrawCell(Vector2Int grid, float cellSize, bool wire)
        {
            Vector2 world = BoardCoordinates.GridToWorld(grid.x, grid.y, cellSize);
            var center = new Vector3(world.x, world.y, 0f);
            var size = new Vector3(cellSize * 0.85f, cellSize * 0.85f, 0.05f);
            if (wire)
            {
                Gizmos.DrawWireCube(center, size);
            }
            else
            {
                Gizmos.DrawCube(center, size);
            }
        }

        private void DrawAxis(Vector2Int original, float cellSize)
        {
            Vector2 tip = BoardCoordinates.GridToWorld(original.x, original.y, cellSize);
            Gizmos.color = axisColor;
            Gizmos.DrawLine(Vector3.zero, new Vector3(tip.x, tip.y, 0f));
        }

        private void DrawCorridor(Vector2Int original, float halfWidthGrid, float cellSize)
        {
            Vector2 tip = BoardCoordinates.GridToWorld(original.x, original.y, cellSize);
            float axisLen = tip.magnitude;
            if (axisLen < 0.0001f)
            {
                return;
            }

            Vector2 axisDir = tip / axisLen;
            Vector2 perp = new(-axisDir.y, axisDir.x);
            float halfW = halfWidthGrid * cellSize;

            Vector3 a = new Vector3(perp.x, perp.y, 0f) * halfW;
            Vector3 b = new Vector3(-perp.x, -perp.y, 0f) * halfW;
            Vector3 end = new(tip.x, tip.y, 0f);

            Gizmos.color = corridorColor;
            Gizmos.DrawLine(a, end + a);
            Gizmos.DrawLine(b, end + b);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(end + a, end + b);
            // 대각선도 한 줄 — 가늘어도 보이게
            Gizmos.DrawLine(a, end + b);
            Gizmos.DrawLine(b, end + a);
        }
    }
}
