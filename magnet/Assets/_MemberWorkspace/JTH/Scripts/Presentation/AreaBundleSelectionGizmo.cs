using System.Collections.Generic;
using JTH.Scripts.Bootstrap;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.AreaBundleSpawn;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using Reflex.Attributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 방금 뽑은 패 + 티어/모드 + 시뮬 배치 + Area를 Scene 기즈모로 표시.
    /// </summary>
    public sealed class AreaBundleSelectionGizmo : MonoBehaviour
    {
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private Color uniqueBlockedColor = new(1f, 0.2f, 0.35f, 0.85f);
        [SerializeField] private Color tierUnique = new(0.70f, 0.53f, 1f, 0.9f);
        [SerializeField] private Color tierAllClear = new(1f, 0.84f, 0.31f, 0.9f);
        [SerializeField] private Color tierHospitality = new(1f, 0.09f, 0.27f, 0.9f);
        [SerializeField] private Color tierNormalClean = new(0.65f, 0.84f, 0.65f, 0.9f);
        [SerializeField] private Color tierNormalMain = new(0.40f, 0.75f, 0.45f, 0.9f);
        [SerializeField] private Color tierEasy = new(0.31f, 0.76f, 0.97f, 0.9f);
        [SerializeField] private Color tierKill = new(1f, 0.67f, 0.25f, 0.9f);
        [SerializeField] private bool drawModeLabel = true;
        [SerializeField] private bool drawAreas = true;
        [SerializeField] private bool drawOccupiedAreas = true;
        [SerializeField] private bool drawEmptyAreas = false;
        [SerializeField] private float areaCubeScale = 0.88f;
        [SerializeField] private float occupiedAreaAlpha = 0.35f;
        [SerializeField] private float emptyAreaAlpha = 0.12f;
        [SerializeField] private float areaBorderThickness = 5f;
        [SerializeField] private float explainWireScale = 0.5f;
        [SerializeField] private float explainWireAlpha = 0.45f;
        [SerializeField] private float modeBarHeightCells = 0.35f;
        [SerializeField] private float modeBarAboveCells = 1.25f;

        /// <summary>넣을 순서 0→1→2 = 빨→노→파 (무지개).</summary>
        private static readonly float[] PieceRainbowHues = { 0f, 1f / 6f, 2f / 3f };

        /// <summary>Area 구분용 — 피스(빨/노/파)와 겹치지 않는 고대비 팔레트.</summary>
        private static readonly Color[] OccupiedAreaPalette =
        {
            new(1f, 0f, 1f, 1f), // magenta
            new(0f, 1f, 1f, 1f), // cyan
            new(0.2f, 1f, 0.2f, 1f), // lime
            new(1f, 1f, 1f, 1f), // white
            new(1f, 0.5f, 0f, 1f), // orange
            new(0.6f, 0.2f, 1f, 1f), // violet
        };

        private static readonly Vector2Int[] Cardinals =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
        };

        [Inject] private BlockSpawnBootstrap _spawnBootstrap;
        [Inject] private GameBoard _gameBoard;

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[AreaBundleSelectionGizmo] boardConfig is not assigned.", this);
            Debug.Assert(_spawnBootstrap != null, "[AreaBundleSelectionGizmo] BlockSpawnBootstrap was not injected.", this);
            Debug.Assert(_gameBoard != null, "[AreaBundleSelectionGizmo] GameBoard was not injected.", this);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _spawnBootstrap == null || _gameBoard == null || boardConfig == null)
            {
                return;
            }

            if (_gameBoard.Grid == null)
            {
                return;
            }

            float cellSize = boardConfig.CellSize;
            IReadOnlyList<AreaPartition> partitions = AreaScoreCalculator.Partition(_gameBoard.Grid);
            int occupiedAreaCount = 0;
            for (int i = 0; i < partitions.Count; ++i)
            {
                if (partitions[i].Occupied)
                {
                    ++occupiedAreaCount;
                }
            }

            if (drawAreas)
            {
                DrawAreaPartitions(partitions, cellSize);
            }

            AreaBundleSelectionResult selection = _spawnBootstrap.LastSelection;
            if (selection == null)
            {
                return;
            }

            bool unique = selection.Tier == AreaBundleTier.Unique;
            DrawModeBanner(selection, cellSize, occupiedAreaCount);
            DrawExplainSteps(selection, cellSize, unique);
        }

        private void DrawModeBanner(
            AreaBundleSelectionResult selection,
            float cellSize,
            int occupiedAreaCount)
        {
            (string label, Color color) = ResolveModeStyle(selection);
            label = $"{label} | 찬Area {occupiedAreaCount}";
            float barH = modeBarHeightCells * cellSize;
            float width = cellSize * 8f;
            Vector3 origin = _gameBoard.GridToWorld(Vector2Int.zero);
            float topY = _gameBoard.GridToWorld(new Vector2Int(0, _gameBoard.Grid.BoardSize - 1)).y
                + cellSize
                + modeBarAboveCells * cellSize;
            Vector3 center = new Vector3(origin.x + width * 0.5f, topY, origin.z);
            Vector3 size = new Vector3(width, barH, cellSize * 0.2f);

            Gizmos.color = color;
            Gizmos.DrawCube(center, size);
            Gizmos.DrawWireCube(center, size);

#if UNITY_EDITOR
            if (drawModeLabel)
            {
                Handles.color = color;
                Handles.Label(
                    center + new Vector3(-width * 0.45f, barH, 0f),
                    label);
            }
#endif
        }

        private void DrawExplainSteps(AreaBundleSelectionResult selection, float cellSize, bool unique)
        {
            if (selection.ExplainSteps == null || selection.ExplainSteps.Count == 0)
            {
                return;
            }

            // 추천 경로는 "순서대로 두고 클리어한 뒤" 기준이라, 현재 보드에 한꺼번에 그리면
            // 지금 못 넣는 칸(이미 참 / 클리어 전제)이 빨간 자리처럼 보인다.
            BoardGrid sim = _gameBoard.Grid.Clone();
            Vector3 cubeSize = Vector3.one * (cellSize * explainWireScale);

            for (int stepIndex = 0; stepIndex < selection.ExplainSteps.Count; ++stepIndex)
            {
                AreaBundleExplainStep step = selection.ExplainSteps[stepIndex];
                if (step.Cells == null || step.Cells.Count == 0)
                {
                    continue;
                }

                if (!CanPlaceExplainStepOn(sim, step))
                {
                    continue;
                }

                Gizmos.color = ResolvePieceColor(step.PieceSlotIndex, unique);
                for (int c = 0; c < step.Cells.Count; ++c)
                {
                    Vector3 world = CellCenter(step.Cells[c], cellSize);
                    Gizmos.DrawWireCube(world, cubeSize);
                }

#if UNITY_EDITOR
                Handles.color = Gizmos.color;
                Handles.Label(CellCenter(step.Cells[0], cellSize), $"#{stepIndex + 1}");
#endif

                ApplyExplainStep(sim, step);
            }
        }

        private static bool CanPlaceExplainStepOn(BoardGrid board, AreaBundleExplainStep step)
        {
            for (int i = 0; i < step.Cells.Count; ++i)
            {
                Vector2Int cell = step.Cells[i];
                if (!board.IsInBounds(cell) || board.IsOccupied(cell))
                {
                    return false;
                }
            }

            return true;
        }

        private static void ApplyExplainStep(BoardGrid board, AreaBundleExplainStep step)
        {
            Vector2Int[] offsets = new Vector2Int[step.Cells.Count];
            for (int i = 0; i < step.Cells.Count; ++i)
            {
                offsets[i] = step.Cells[i] - step.Pivot;
            }

            PlacementSimulator.PlaceAndClear(board, offsets, step.Pivot);
        }

        private void DrawAreaPartitions(IReadOnlyList<AreaPartition> partitions, float cellSize)
        {
            Vector3 cubeSize = Vector3.one * (cellSize * areaCubeScale);
            int occupiedIndex = 0;
            int emptyIndex = 0;

            for (int i = 0; i < partitions.Count; ++i)
            {
                AreaPartition part = partitions[i];
                if (part.Occupied)
                {
                    if (!drawOccupiedAreas)
                    {
                        continue;
                    }

                    Color accent = OccupiedAreaPalette[occupiedIndex % OccupiedAreaPalette.Length];
                    DrawAreaPartition(part, accent, occupiedAreaAlpha, cubeSize, cellSize, $"F{occupiedIndex}");
                    ++occupiedIndex;
                }
                else
                {
                    if (!drawEmptyAreas)
                    {
                        continue;
                    }

                    Color accent = new Color(0.7f, 0.7f, 0.7f, 1f);
                    DrawAreaPartition(part, accent, emptyAreaAlpha, cubeSize, cellSize, $"E{emptyIndex}");
                    ++emptyIndex;
                }
            }
        }

        private void DrawAreaPartition(
            AreaPartition part,
            Color accent,
            float fillAlpha,
            Vector3 cubeSize,
            float cellSize,
            string label)
        {
            if (part.Cells.Count == 0)
            {
                return;
            }

            HashSet<Vector2Int> set = new(part.Cells);
            Color fill = accent;
            fill.a = fillAlpha;

            Vector2Int min = part.Cells[0];
            Vector2Int max = part.Cells[0];
            Vector3 sum = Vector3.zero;

            for (int i = 0; i < part.Cells.Count; ++i)
            {
                Vector2Int grid = part.Cells[i];
                if (grid.x < min.x)
                {
                    min.x = grid.x;
                }

                if (grid.y < min.y)
                {
                    min.y = grid.y;
                }

                if (grid.x > max.x)
                {
                    max.x = grid.x;
                }

                if (grid.y > max.y)
                {
                    max.y = grid.y;
                }

                Vector3 world = CellCenter(grid, cellSize);
                sum += world;
                Gizmos.color = fill;
                Gizmos.DrawCube(world, cubeSize);
            }

            // 덩어리 실루엣(이웃 없는 변만) — 합쳐짐/쪼개짐이 한눈에
            DrawAreaSilhouette(set, accent, cellSize);

            // 영역 AABB — Area가 둘이면 박스도 둘
            Vector3 minWorld = CellCenter(min, cellSize) - new Vector3(cellSize, cellSize, 0f) * 0.5f;
            Vector3 maxWorld = CellCenter(max, cellSize) + new Vector3(cellSize, cellSize, 0f) * 0.5f;
            Vector3 boundsCenter = (minWorld + maxWorld) * 0.5f;
            Vector3 boundsSize = maxWorld - minWorld;
            boundsSize.z = cellSize * 0.15f;
            Gizmos.color = accent;
            Gizmos.DrawWireCube(boundsCenter, boundsSize);

#if UNITY_EDITOR
            Vector3 labelPos = sum / part.Cells.Count;
            Handles.color = accent;
            Handles.Label(labelPos, $"{label} n={part.Size}");
#endif
        }

        private void DrawAreaSilhouette(HashSet<Vector2Int> set, Color accent, float cellSize)
        {
            float half = cellSize * 0.5f;
#if UNITY_EDITOR
            Handles.color = accent;
#endif
            foreach (Vector2Int cell in set)
            {
                Vector3 center = CellCenter(cell, cellSize);
                Vector3 bl = center + new Vector3(-half, -half, 0f);
                Vector3 br = center + new Vector3(half, -half, 0f);
                Vector3 tl = center + new Vector3(-half, half, 0f);
                Vector3 tr = center + new Vector3(half, half, 0f);

                // 오른쪽 이웃 없으면 오른쪽 변
                if (!set.Contains(cell + Cardinals[0]))
                {
                    DrawThickEdge(br, tr);
                }

                // 왼쪽
                if (!set.Contains(cell + Cardinals[1]))
                {
                    DrawThickEdge(bl, tl);
                }

                // 위
                if (!set.Contains(cell + Cardinals[2]))
                {
                    DrawThickEdge(tl, tr);
                }

                // 아래
                if (!set.Contains(cell + Cardinals[3]))
                {
                    DrawThickEdge(bl, br);
                }
            }
        }

        private void DrawThickEdge(Vector3 a, Vector3 b)
        {
#if UNITY_EDITOR
            Handles.DrawAAPolyLine(areaBorderThickness, a, b);
#else
            Gizmos.DrawLine(a, b);
#endif
        }

        private Vector3 CellCenter(Vector2Int grid, float cellSize)
        {
            Vector3 world = _gameBoard.GridToWorld(grid);
            world += new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
            return world;
        }

        private Color ResolvePieceColor(int pieceSlotIndex, bool unique)
        {
            if (unique && pieceSlotIndex == 0)
            {
                Color blocked = uniqueBlockedColor;
                blocked.a = explainWireAlpha;
                return blocked;
            }

            int slot = Mathf.Clamp(pieceSlotIndex, 0, PieceRainbowHues.Length - 1);
            Color color = Color.HSVToRGB(PieceRainbowHues[slot], 0.95f, 1f);
            color.a = explainWireAlpha;
            return color;
        }

        private (string label, Color color) ResolveModeStyle(AreaBundleSelectionResult selection)
        {
            if (selection.IsKillHand)
            {
                return selection.Tier == AreaBundleTier.Easy
                    ? ("Easy-랜덤", tierKill)
                    : ("Kill", tierKill);
            }

            return selection.Tier switch
            {
                AreaBundleTier.Unique => ("유일수", tierUnique),
                AreaBundleTier.AllClear => ("올클리어", tierAllClear),
                AreaBundleTier.Hospitality => ("접대", tierHospitality),
                AreaBundleTier.Easy => ("Easy", tierEasy),
                AreaBundleTier.Normal when selection.Profile == ShapeWeightProfile.Clean =>
                    ("Normal-Clean", tierNormalClean),
                AreaBundleTier.Normal => ("Normal-Main", tierNormalMain),
                _ => (selection.Tier.ToString(), tierNormalMain),
            };
        }
    }
}
