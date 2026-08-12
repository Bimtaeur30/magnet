using System.Collections.Generic;
using JTH.Scripts.Bootstrap;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.AreaBundleSpawn;
using Magnet.Contracts;
using Reflex.Attributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 방금 뽑은 패 + 티어/모드 + 시뮬 배치를 Scene 기즈모로 표시.
    /// </summary>
    public sealed class AreaBundleSelectionGizmo : MonoBehaviour
    {
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private Color piece0Color = new(0.35f, 0.95f, 1f, 0.85f);
        [SerializeField] private Color piece1Color = new(1f, 0.85f, 0.2f, 0.85f);
        [SerializeField] private Color piece2Color = new(0.85f, 0.35f, 1f, 0.85f);
        [SerializeField] private Color uniqueBlockedColor = new(1f, 0.25f, 0.35f, 0.7f);
        [SerializeField] private Color tierUnique = new(0.70f, 0.53f, 1f, 0.9f);
        [SerializeField] private Color tierAllClear = new(1f, 0.84f, 0.31f, 0.9f);
        [SerializeField] private Color tierHospitality = new(1f, 0.09f, 0.27f, 0.9f);
        [SerializeField] private Color tierNormalClean = new(0.65f, 0.84f, 0.65f, 0.9f);
        [SerializeField] private Color tierNormalMain = new(0.40f, 0.75f, 0.45f, 0.9f);
        [SerializeField] private Color tierEasy = new(0.31f, 0.76f, 0.97f, 0.9f);
        [SerializeField] private Color tierKill = new(1f, 0.67f, 0.25f, 0.9f);
        [SerializeField] private bool drawHandFilled = true;
        [SerializeField] private bool drawHandPreview = true;
        [SerializeField] private bool drawModeLabel = true;
        [SerializeField] private float handGapCells = 1.5f;
        [SerializeField] private float handBelowCells = 2f;
        [SerializeField] private float explainWireScale = 0.75f;
        [SerializeField] private float modeBarHeightCells = 0.35f;

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

            AreaBundleSelectionResult selection = _spawnBootstrap.LastSelection;
            if (selection == null || _gameBoard.Grid == null)
            {
                return;
            }

            float cellSize = boardConfig.CellSize;
            bool unique = selection.Tier == AreaBundleTier.Unique;
            Vector3 rowOrigin = HandRowOrigin(cellSize);
            float handWidth = EstimateHandWidth(selection, cellSize);

            DrawModeBanner(selection, rowOrigin, handWidth, cellSize);

            if (drawHandPreview)
            {
                DrawHandPreview(selection, cellSize, unique, rowOrigin);
            }

            DrawExplainSteps(selection, cellSize, unique);
        }

        private void DrawModeBanner(
            AreaBundleSelectionResult selection,
            Vector3 rowOrigin,
            float handWidth,
            float cellSize)
        {
            (string label, Color color) = ResolveModeStyle(selection);
            float barH = modeBarHeightCells * cellSize;
            float width = Mathf.Max(handWidth, cellSize * 4f);
            Vector3 center = rowOrigin
                + new Vector3(width * 0.5f, -barH * 1.5f, 0f)
                + new Vector3(0f, 0f, 0f);
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

        private void DrawHandPreview(
            AreaBundleSelectionResult selection,
            float cellSize,
            bool unique,
            Vector3 rowOrigin)
        {
            IReadOnlyList<ShapeBlockData> candidates = _spawnBootstrap.Candidates;
            if (candidates != null && candidates.Count > 0)
            {
                DrawHandFromCandidates(candidates, cellSize, unique, rowOrigin);
                return;
            }

            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces = selection.Pieces;
            if (pieces == null || pieces.Count == 0)
            {
                return;
            }

            float cursorX = 0f;
            for (int slot = 0; slot < pieces.Count; ++slot)
            {
                cursorX += DrawPieceAt(pieces[slot], rowOrigin, cursorX, cellSize, ResolvePieceColor(slot, unique));
                cursorX += handGapCells * cellSize;
            }
        }

        private void DrawHandFromCandidates(
            IReadOnlyList<ShapeBlockData> candidates,
            float cellSize,
            bool unique,
            Vector3 rowOrigin)
        {
            float cursorX = 0f;
            for (int slot = 0; slot < candidates.Count; ++slot)
            {
                ShapeBlockData data = candidates[slot];
                if (data == null || data.CellOffsets == null || data.CellOffsets.Count == 0)
                {
                    cursorX += (2f + handGapCells) * cellSize;
                    continue;
                }

                cursorX += DrawPieceAt(
                    data.CellOffsets,
                    rowOrigin,
                    cursorX,
                    cellSize,
                    ResolvePieceColor(slot, unique));
                cursorX += handGapCells * cellSize;
            }
        }

        private float EstimateHandWidth(AreaBundleSelectionResult selection, float cellSize)
        {
            IReadOnlyList<ShapeBlockData> candidates = _spawnBootstrap.Candidates;
            float width = 0f;
            int count = 0;
            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count; ++i)
                {
                    ShapeBlockData data = candidates[i];
                    if (data?.CellOffsets == null || data.CellOffsets.Count == 0)
                    {
                        width += 2f * cellSize;
                    }
                    else
                    {
                        GetBounds(data.CellOffsets, out int minX, out _, out int maxX, out _);
                        width += (maxX - minX + 1) * cellSize;
                    }

                    ++count;
                }
            }
            else if (selection.Pieces != null)
            {
                for (int i = 0; i < selection.Pieces.Count; ++i)
                {
                    IReadOnlyList<Vector2Int> offsets = selection.Pieces[i];
                    if (offsets == null || offsets.Count == 0)
                    {
                        width += 2f * cellSize;
                    }
                    else
                    {
                        GetBounds(offsets, out int minX, out _, out int maxX, out _);
                        width += (maxX - minX + 1) * cellSize;
                    }

                    ++count;
                }
            }

            if (count > 1)
            {
                width += (count - 1) * handGapCells * cellSize;
            }

            return width;
        }

        private float DrawPieceAt(
            IReadOnlyList<Vector2Int> offsets,
            Vector3 rowOrigin,
            float cursorX,
            float cellSize,
            Color color)
        {
            GetBounds(offsets, out int minX, out int minY, out int maxX, out int maxY);
            Vector3 cubeSize = Vector3.one * (cellSize * 0.9f);
            Gizmos.color = color;

            for (int i = 0; i < offsets.Count; ++i)
            {
                Vector2Int local = offsets[i];
                float lx = cursorX + (local.x - minX) * cellSize;
                float ly = (local.y - minY) * cellSize;
                Vector3 world = rowOrigin + new Vector3(lx, ly, 0f);
                world += new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
                if (drawHandFilled)
                {
                    Gizmos.DrawCube(world, cubeSize);
                }

                Gizmos.DrawWireCube(world, cubeSize);
            }

            return (maxX - minX + 1) * cellSize;
        }

        private void DrawExplainSteps(AreaBundleSelectionResult selection, float cellSize, bool unique)
        {
            if (selection.ExplainSteps == null || selection.ExplainSteps.Count == 0)
            {
                return;
            }

            Vector3 cubeSize = Vector3.one * (cellSize * explainWireScale);
            for (int stepIndex = 0; stepIndex < selection.ExplainSteps.Count; ++stepIndex)
            {
                AreaBundleExplainStep step = selection.ExplainSteps[stepIndex];
                if (step.Cells == null)
                {
                    continue;
                }

                Gizmos.color = ResolvePieceColor(step.PieceSlotIndex, unique);
                for (int c = 0; c < step.Cells.Count; ++c)
                {
                    Vector3 world = _gameBoard.GridToWorld(step.Cells[c]);
                    world += new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
                    Gizmos.DrawWireCube(world, cubeSize);
                }
            }
        }

        private Vector3 HandRowOrigin(float cellSize)
        {
            Vector3 bottomLeft = _gameBoard.GridToWorld(Vector2Int.zero);
            float y = _gameBoard.GetStartStagingY() - handBelowCells * cellSize;
            return new Vector3(bottomLeft.x, y, bottomLeft.z);
        }

        private static void GetBounds(
            IReadOnlyList<Vector2Int> offsets,
            out int minX,
            out int minY,
            out int maxX,
            out int maxY)
        {
            minX = maxX = offsets[0].x;
            minY = maxY = offsets[0].y;
            for (int i = 1; i < offsets.Count; ++i)
            {
                Vector2Int o = offsets[i];
                if (o.x < minX)
                {
                    minX = o.x;
                }

                if (o.x > maxX)
                {
                    maxX = o.x;
                }

                if (o.y < minY)
                {
                    minY = o.y;
                }

                if (o.y > maxY)
                {
                    maxY = o.y;
                }
            }
        }

        private Color ResolvePieceColor(int pieceSlotIndex, bool unique)
        {
            if (unique && pieceSlotIndex == 0)
            {
                return uniqueBlockedColor;
            }

            return pieceSlotIndex switch
            {
                0 => piece0Color,
                1 => piece1Color,
                _ => piece2Color,
            };
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
