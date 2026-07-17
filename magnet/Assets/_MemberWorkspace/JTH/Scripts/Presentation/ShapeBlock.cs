using System;
using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Placement;
using LitMotion;
using Magnet.Contracts.BlockShapes;
using Magnet.Contracts.BlockSkins;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 형태(IBlockShape) 단위 블록 표시. Block 프리팹을 칸 수만큼 Instantiate·재사용한다.
    /// 보드 부착 후에는 <see cref="OccupiedCellView"/>로 분해된다.
    /// </summary>
    public sealed class ShapeBlock : MonoBehaviour
    {
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;
        [Tooltip("블록 칸 1개 프리팹(Block 컴포넌트 + SpriteRenderer). 필요 개수만큼 인스턴스 생성 후 재사용")]
        [SerializeField] private Block blockPrefab;
        [SerializeField] private EventChannelSO systemChannel;

        private readonly List<Block> _blocks = new();
        private readonly List<Vector2Int> _cellGridPositions = new();
        private readonly List<MotionHandle> _activeMotions = new();
        private bool _skinResolved;
        private Sprite _resolvedSprite;
        private BoardView _boardView;

        private BoardView BoardView
        {
            get
            {
                if (_boardView == null)
                {
                    _boardView = GetComponentInParent<BoardView>();
                }

                return _boardView;
            }
        }

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[ShapeBlock] boardConfig is not assigned.", this);
            Debug.Assert(placementConfig != null, "[ShapeBlock] placementConfig is not assigned.", this);
            Debug.Assert(blockPrefab != null, "[ShapeBlock] blockPrefab is not assigned.", this);
            Debug.Assert(systemChannel != null, "[ShapeBlock] systemChannel is not assigned.", this);
        }

        private void OnDestroy()
        {
            CancelMotions();
        }

        public void Show(IBlockShape shape, Vector2Int pivot, int sortingOrder)
        {
            ShowCells(pivot, shape.CellOffsets, sortingOrder);
            SetAlpha(1f);
        }

        /// <summary>
        /// 보드 격자 프리뷰(고스트). <see cref="BlockVisualConfigSO.PreviewAlpha"/> 적용.
        /// </summary>
        public void ShowPreview(IBlockShape shape, Vector2Int pivot, int sortingOrder)
        {
            ShowCells(pivot, shape.CellOffsets, sortingOrder);
            SetAlpha(placementConfig.Visual.PreviewAlpha);
        }

        public void ShowCells(Vector2Int pivot, IReadOnlyList<Vector2Int> cellOffsets, int sortingOrder)
        {
            EnsureBlockCount(cellOffsets.Count);

            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.Visual.CellFill;
            _cellGridPositions.Clear();

            for (int i = 0; i < cellOffsets.Count; i++)
            {
                Vector2Int cell = pivot + cellOffsets[i];
                _cellGridPositions.Add(cell);
                Vector2 boardLocal = BoardCoordinates.GridToWorld(cell.x, cell.y, cellSize);
                ApplyBlockVisual(i, boardLocal, cellSize, fill, sortingOrder);
            }

            HideExtraBlocks(cellOffsets.Count);
            ApplyResolvedSkin();
        }

        public void ShowAtWorldCenter(IBlockShape shape, float worldCenterX, int stagingGridY)
        {
            EnsureBlockCount(shape.CellOffsets.Count);

            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.Visual.CellFill;
            Vector2 centerOffset = BlockPlacementCells.GetShapeCenterOffset(shape.CellOffsets);
            int maxOffsetY = BlockPlacementCells.GetMaxOffsetY(shape.CellOffsets);
            float stagingWorldY = stagingGridY * cellSize;

            for (int i = 0; i < shape.CellOffsets.Count; i++)
            {
                Vector2Int offset = shape.CellOffsets[i];
                float cellWorldX = worldCenterX + (offset.x - centerOffset.x) * cellSize;
                // X는 기하 중심, Y는 최상단 칸이 staging 행에 오도록
                float cellWorldY = stagingWorldY + (offset.y - maxOffsetY) * cellSize;
                ApplyBlockVisual(i, new Vector2(cellWorldX, cellWorldY), cellSize, fill, sortingOrder: 2);
            }

            HideExtraBlocks(shape.CellOffsets.Count);
            ApplyResolvedSkin();
            SetAlpha(1f);
        }

        public void ShowAtSnapStart(IBlockShape shape, Vector2Int finalPivot, int stagingGridY, int sortingOrder = 2)
        {
            ShowAtSnapStartFromOffsets(shape.CellOffsets, finalPivot, stagingGridY, sortingOrder);
        }

        public void ShowAtSnapStartFromOffsets(
            IReadOnlyList<Vector2Int> cellOffsets,
            Vector2Int finalPivot,
            int stagingGridY,
            int sortingOrder = 2)
        {
            EnsureBlockCount(cellOffsets.Count);

            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.Visual.CellFill;
            _cellGridPositions.Clear();
            int stagingPivotY = BlockPlacementCells.GetStagingPivotY(stagingGridY, cellOffsets);

            for (int i = 0; i < cellOffsets.Count; i++)
            {
                Vector2Int offset = cellOffsets[i];
                Vector2Int cell = finalPivot + offset;
                _cellGridPositions.Add(cell);
                Vector2 boardLocal = BoardCoordinates.GridToWorld(cell.x, stagingPivotY + offset.y, cellSize);
                ApplyBlockVisual(i, boardLocal, cellSize, fill, sortingOrder);
            }

            HideExtraBlocks(cellOffsets.Count);
            ApplyResolvedSkin();
            SetAlpha(1f);
        }

        public void AnimateSnapY(
            IBlockShape shape,
            Vector2Int finalPivot,
            BoardConfigSO configSO,
            float durationPerCell,
            Ease snapEase,
            Action onComplete)
        {
            AnimateSnapYFromOffsets(shape.CellOffsets, finalPivot, configSO, durationPerCell, snapEase, onComplete);
        }

        public void AnimateSnapYFromOffsets(
            IReadOnlyList<Vector2Int> cellOffsets,
            Vector2Int finalPivot,
            BoardConfigSO configSO,
            float durationPerCell,
            Ease snapEase,
            Action onComplete)
        {
            CancelMotions();

            float cellSize = configSO.CellSize;
            int remaining = cellOffsets.Count;

            if (remaining == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _cellGridPositions.Clear();

            void OnCellComplete()
            {
                remaining--;
                if (remaining <= 0)
                {
                    onComplete?.Invoke();
                }
            }

            BoardView boardView = BoardView;
            if (boardView == null)
            {
                onComplete?.Invoke();
                return;
            }

            for (int i = 0; i < cellOffsets.Count; i++)
            {
                Vector2Int cell = finalPivot + cellOffsets[i];
                _cellGridPositions.Add(cell);
                Vector2 targetBoardLocal = BoardCoordinates.GridToWorld(cell.x, cell.y, cellSize);
                Block block = _blocks[i];
                Vector2 startBoardLocal = boardView.WorldToBoardLocal(block.transform.position);
                float startY = startBoardLocal.y;
                float targetY = targetBoardLocal.y;
                float boardLocalX = startBoardLocal.x;

                if (Mathf.Approximately(startY, targetY))
                {
                    boardView.SetAtBoardLocal(block.transform, targetBoardLocal);
                    OnCellComplete();
                    continue;
                }

                float cells = Mathf.Abs(targetY - startY) / Mathf.Max(cellSize, 0.0001f);
                float duration = Mathf.Max(0.01f, durationPerCell * Mathf.Max(cells, 1f));

                MotionHandle handle = LMotion.Create(startY, targetY, duration)
                    .WithEase(snapEase)
                    .WithOnComplete(OnCellComplete)
                    .Bind(y => boardView.SetAtBoardLocal(block.transform, new Vector2(boardLocalX, y)));
                _activeMotions.Add(handle);
            }
        }

        /// <summary>
        /// 활성 칸 Block을 분리해 반환한다. ShapeBlock은 비운다.
        /// </summary>
        public List<(Block block, Vector2Int grid)> DetachActiveBlocks()
        {
            CancelMotions();
            var detached = new List<(Block block, Vector2Int grid)>(_cellGridPositions.Count);

            for (int i = 0; i < _cellGridPositions.Count; i++)
            {
                Block block = _blocks[i];
                if (block == null || !block.gameObject.activeSelf)
                {
                    continue;
                }

                block.transform.SetParent(null, worldPositionStays: true);
                detached.Add((block, _cellGridPositions[i]));
            }

            _blocks.Clear();
            _cellGridPositions.Clear();
            return detached;
        }

        public void CancelMotions()
        {
            for (int i = 0; i < _activeMotions.Count; i++)
            {
                if (_activeMotions[i].IsActive())
                {
                    _activeMotions[i].Cancel();
                }
            }

            _activeMotions.Clear();
        }

        public void Clear()
        {
            CancelMotions();

            for (int i = 0; i < _blocks.Count; i++)
            {
                _blocks[i].SetActive(false);
            }

            _cellGridPositions.Clear();
        }

        public void ApplySkin(IBlockSkin skin)
        {
            if (skin == null)
            {
                return;
            }

            _resolvedSprite = skin.Sprite;
            _skinResolved = true;
            ApplyVisualToActiveBlocks(_resolvedSprite);
        }

        private void ApplyResolvedSkin()
        {
            if (!_skinResolved)
            {
                return;
            }

            ApplyVisualToActiveBlocks(_resolvedSprite);
        }

        internal void ShareSkinWith(ShapeBlock target)
        {
            if (!_skinResolved || target == null)
            {
                return;
            }

            target._resolvedSprite = _resolvedSprite;
            target._skinResolved = true;
        }

        public void SetAlpha(float alpha)
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                if (_blocks[i].gameObject.activeSelf)
                {
                    _blocks[i].SetAlpha(alpha);
                }
            }
        }

        private void ApplyVisualToActiveBlocks(Sprite sprite)
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                if (_blocks[i].gameObject.activeSelf)
                {
                    _blocks[i].ApplyVisual(sprite);
                }
            }
        }

        private void EnsureBlockCount(int count)
        {
            while (_blocks.Count < count)
            {
                Block instance = Instantiate(blockPrefab, transform);
                instance.name = $"Block_{_blocks.Count}";
                _blocks.Add(instance);
            }
        }

        private void ApplyBlockVisual(int index, Vector2 boardLocal, float cellSize, float fill, int sortingOrder)
        {
            Block block = _blocks[index];
            block.SetActive(true);

            BoardView boardView = BoardView;
            if (boardView != null)
            {
                boardView.SetAtBoardLocal(block.transform, boardLocal);
            }
            else
            {
                block.SetLocalPosition(new Vector3(boardLocal.x, boardLocal.y, 0f));
            }

            block.SetLocalScale(new Vector3(cellSize * fill, cellSize * fill, 1f));
            block.SetSortingOrder(sortingOrder);
        }

        private void HideExtraBlocks(int activeCount)
        {
            for (int i = activeCount; i < _blocks.Count; i++)
            {
                _blocks[i].SetActive(false);
            }
        }
    }
}
