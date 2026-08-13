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
    /// 방금 뽑은 패 티어 + 히트맵 추천 Explain(#1…) Scene 기즈모.
    /// </summary>
    public sealed class AreaBundleSelectionGizmo : MonoBehaviour
    {
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private Color uniqueBlockedColor = new(1f, 0.2f, 0.35f, 0.85f);
        [SerializeField] private Color tierUnique = new(0.70f, 0.53f, 1f, 0.9f);
        [SerializeField] private Color tierNormal = new(0.40f, 0.75f, 0.45f, 0.9f);
        [SerializeField] private Color tierEasy = new(0.31f, 0.76f, 0.97f, 0.9f);
        [SerializeField] private Color tierKill = new(1f, 0.67f, 0.25f, 0.9f);
        [SerializeField] private bool drawModeLabel = true;
        [SerializeField] private float explainWireScale = 0.5f;
        [SerializeField] private float explainWireAlpha = 0.45f;
        [SerializeField] private float modeBarHeightCells = 0.35f;
        [SerializeField] private float modeBarAboveCells = 1.25f;

        /// <summary>넣을 순서 0→1→2 = 빨→노→파.</summary>
        private static readonly float[] PieceRainbowHues = { 0f, 1f / 6f, 2f / 3f };

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

            AreaBundleSelectionResult selection = _spawnBootstrap.LastSelection;
            if (selection == null)
            {
                return;
            }

            float cellSize = boardConfig.CellSize;
            bool unique = selection.Tier == AreaBundleTier.Unique;
            DrawModeBanner(selection, cellSize);
            DrawExplainSteps(selection, cellSize, unique);
        }

        private void DrawModeBanner(AreaBundleSelectionResult selection, float cellSize)
        {
            (string label, Color color) = ResolveModeStyle(selection);
            if (selection.Tier != AreaBundleTier.Unique && !float.IsNaN(selection.HeatScore))
            {
                label = $"{label} | heat {selection.HeatScore:F0}";
            }

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

            if (selection.Reason != null && selection.Reason.Contains("AllClear"))
            {
                return ("올클리어", new Color(1f, 0.84f, 0.31f, 0.9f));
            }

            return selection.Tier switch
            {
                AreaBundleTier.Unique => ("유일수", tierUnique),
                AreaBundleTier.Easy => ("Easy", tierEasy),
                AreaBundleTier.Normal => ("Normal", tierNormal),
                _ => (selection.Tier.ToString(), tierNormal),
            };
        }
    }
}
