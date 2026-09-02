using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Bootstrap;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Contracts;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Input
{
    public sealed class BlockDragInput : MonoBehaviour
    {
        [SerializeField] private MagnetInputSO inputSO;
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;
        [SerializeField] private EventChannelSO inGameChannel;

        [Inject] private readonly BoardPlacementBootstrap _placementBootstrap;
        [Inject] private readonly GameBoard _gameBoard;

        private BlockDragDrawer _drawer;
        private DragSensitivityRamp _sensitivityRamp;

        private const float MinDragSqrDistanceToPlace = 0.0001f;
        private const float DragClampMarginCells = 2f;

        private ShapeBlockData _selectedBlockData;
        private int _selectedSlotIndex;
        private Vector2 _currentPivot;
        private Vector2Int? _lastBoardPivot;
        private bool _hasMoved;

        private void Awake()
        {
            Debug.Assert(placementConfig != null, "[BlockDragInput] placementConfig is not assigned.", this);
            Debug.Assert(boardConfig != null, "[BlockDragInput] boardConfig is not assigned.", this);
            Debug.Assert(inGameChannel != null, "[BlockDragInput] inGameChannel is not assigned.", this);
            Debug.Assert(_placementBootstrap != null, "[BlockDragInput] BoardPlacementBootstrap was not injected.", this);
            Debug.Assert(_gameBoard != null, "[BlockDragInput] _gameBoard was not injected.", this);

            _drawer = GetComponent<BlockDragDrawer>();
            _sensitivityRamp = new DragSensitivityRamp(
                placementConfig.Drag.SensitivityRampPerUnit);
        }

        private void OnEnable()
        {
            inputSO.OnPointerReleased += OnPointerReleased;
            inputSO.OnPointerChange += OnPointerMoved;
            inGameChannel.AddListener<BlockSelectedEvent>(OnBlockSelected);
        }

        private void OnDisable()
        {
            if (inputSO != null)
            {
                inputSO.OnPointerReleased -= OnPointerReleased;
                inputSO.OnPointerChange -= OnPointerMoved;
            }

            inGameChannel?.RemoveListener<BlockSelectedEvent>(OnBlockSelected);
            _gameBoard?.ClearLineClearHints();
        }

        private void Update()
        {
            inputSO.Tick();
        }

        private void OnBlockSelected(BlockSelectedEvent evt)
        {
            _selectedBlockData = evt.BlockData;
            _selectedSlotIndex = evt.SlotIndex;
            _hasMoved = false;
            _sensitivityRamp.Reset();
            _drawer.ClearAll();
            _gameBoard.ClearLineClearHints();

            _drawer.ShowStaging(evt.BlockData);

            Vector2 worldPointerPos = inputSO.GetWorldPointerPosition();
            float startXPosition = placementConfig.Drag.StagingBlockStartXPositions[_selectedSlotIndex];
            Vector2 startPosition = new Vector2(startXPosition, StagingStartY());

            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (Vector2Int offset in _selectedBlockData.CellOffsets)
            {
                if (maxX < offset.x)
                    maxX = offset.x;
                if (maxY < offset.y)
                    maxY = offset.y;
            }

            Vector2 shapeBlockOffset = new Vector2(maxX / 2f, maxY / 2f);
            startPosition -= shapeBlockOffset;

            _sensitivityRamp.Begin(worldPointerPos);
            _currentPivot = startPosition;

            UpdateViews();
        }

        private void OnPointerMoved(Vector2 _)
        {
            if (_selectedBlockData == null || !inputSO.IsPointerPressed)
            {
                return;
            }

            Vector2 delta = _sensitivityRamp.UpdateDelta(inputSO.GetWorldPointerPosition());
            if (delta.sqrMagnitude > MinDragSqrDistanceToPlace)
            {
                _hasMoved = true;
            }

            _currentPivot = ClampPivot(_currentPivot + delta);

            UpdateViews();
        }

        private void OnPointerReleased()
        {
            _sensitivityRamp.Reset();

            if (_selectedBlockData == null)
            {
                return;
            }

            // 드래그 없이 탭만 한 경우: 배치하지 않고 슬롯으로 되돌린다.
            if (!_hasMoved
                || _lastBoardPivot == null
                || !PlacementService.CanPlace(
                    _selectedBlockData.CellOffsets, _lastBoardPivot.Value, _gameBoard.Grid))
            {
                DisconnectSelection();
                return;
            }

            List<Vector2Int> gridOffsets = new List<Vector2Int>();
            foreach (Vector2Int cellOffset in _selectedBlockData.CellOffsets)
            {
                gridOffsets.Add(cellOffset + _lastBoardPivot.Value);
            }

            _placementBootstrap.PlaceBlock(
                _drawer.GetStagingBlocks(),
                gridOffsets,
                _selectedSlotIndex,
                _selectedBlockData.SkinId);

            DisconnectSelection();
        }

        private void DisconnectSelection()
        {
            _selectedBlockData = null;
            _selectedSlotIndex = -1;
            _lastBoardPivot = null;
            _hasMoved = false;
            _gameBoard.ClearLineClearHints();
            _drawer.ClearAll();
        }

        /// <summary>
        /// 스테이징(선택 직후) 블록의 시작 Y. 보드 최하단보다 StagingDropCells 칸만큼 아래에 둬서
        /// 탭·손떨림만으로는 스냅 존에 닿지 않게 한다. 실제 스냅까지 필요한 의도적 드래그는
        /// (StagingDropCells - LastPivotSnapThreshold)칸.
        /// </summary>
        private float StagingStartY()
        {
            return _gameBoard.GetStartStagingY()
                   - boardConfig.CellSize * placementConfig.Drag.StagingDropCells;
        }

        /// <summary>
        /// 드래그 중 블록이 보드 밖으로 한없이 밀려나 화면 밑으로 사라지지 않도록,
        /// 보드 영역 + 여유 칸만큼으로 피벗 이동 범위를 제한한다.
        /// </summary>
        private Vector2 ClampPivot(Vector2 pivot)
        {
            int lastCell = boardConfig.CellCount - 1;
            Vector2 boardMin = _gameBoard.GridToWorld(Vector2Int.zero);
            Vector2 boardMax = _gameBoard.GridToWorld(new Vector2Int(lastCell, lastCell));

            float margin = boardConfig.CellSize * DragClampMarginCells;
            float minX = Mathf.Min(boardMin.x, boardMax.x) - margin;
            float maxX = Mathf.Max(boardMin.x, boardMax.x) + margin;
            float minY = Mathf.Min(StagingStartY(), Mathf.Min(boardMin.y, boardMax.y)) - margin;
            float maxY = Mathf.Max(boardMin.y, boardMax.y) + margin;

            return new Vector2(
                Mathf.Clamp(pivot.x, minX, maxX),
                Mathf.Clamp(pivot.y, minY, maxY));
        }

        private void UpdateViews()
        {
            _drawer.MoveStaging(_currentPivot);
            bool hadPreview = _lastBoardPivot != null;

            float threshold = placementConfig.Drag.LastPivotSnapThreshold;
            Vector2 boardLocal = _gameBoard.WorldToBoardLocal(_currentPivot);
            if (PlacementService.TryGetBoardPivot(boardLocal, _selectedBlockData.CellOffsets,
                    _gameBoard.Grid, _lastBoardPivot, threshold,
                    out Vector2Int boardPivot))
            {
                _lastBoardPivot = boardPivot;

                if (!hadPreview)
                {
                    _drawer.ShowPreview(_selectedBlockData);
                }
                _drawer.MovePreview(_gameBoard.GridToWorld(boardPivot));
                UpdateLineClearHints(boardPivot);
                return;
            }

            _lastBoardPivot = null;
            _drawer.ClearPreview();
            _gameBoard.ClearLineClearHints();
        }

        private void UpdateLineClearHints(Vector2Int boardPivot)
        {
            ClearedLineResult cleared = LineClearPreviewDetector.Detect(
                _gameBoard.Grid,
                _selectedBlockData.CellOffsets,
                boardPivot);

            if (cleared.ClearedLineCount <= 0)
            {
                _gameBoard.ClearLineClearHints();
                return;
            }

            HashSet<Vector2Int> clearedCells = new HashSet<Vector2Int>(
                cleared.CollectClearedCells(_gameBoard.Grid.BoardSize));

            List<Block> previewOnLine = new List<Block>();
            IReadOnlyList<Block> previewBlocks = _drawer.GetPreviewBlocks();
            for (int i = 0; i < previewBlocks.Count; ++i)
            {
                Block preview = previewBlocks[i];
                if (preview != null && clearedCells.Contains(boardPivot + preview.Offset))
                {
                    previewOnLine.Add(preview);
                }
            }

            _gameBoard.SetLineClearHints(clearedCells, previewOnLine, boardPivot, _selectedBlockData.SkinId);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_currentPivot, 0.1f);

            if (_lastBoardPivot.HasValue)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_gameBoard.GridToWorld(_lastBoardPivot.Value), 0.1f);
            }
        }
    }
}
