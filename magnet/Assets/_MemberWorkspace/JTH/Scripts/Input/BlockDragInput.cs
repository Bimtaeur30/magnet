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

        private ShapeBlockData _selectedBlockData;
        private int _selectedSlotIndex;
        private Vector2 _currentPivot;
        private Vector2Int? _lastBoardPivot;

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

        private void OnBlockSelected(BlockSelectedEvent evt)
        {
            _selectedBlockData = evt.BlockData;
            _selectedSlotIndex = evt.SlotIndex;
            _sensitivityRamp.Reset();
            _drawer.ClearAll();
            _gameBoard.ClearLineClearHints();

            _drawer.ShowStaging(evt.BlockData);

            Vector2 worldPointerPos = inputSO.GetWorldPointerPosition();
            float startXPosition = placementConfig.Drag.StagingBlockStartXPositions[_selectedSlotIndex];
            Vector2 startPosition = new Vector2(startXPosition, _gameBoard.GetStartStagingY());

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
            _currentPivot += delta;

            UpdateViews();
        }

        private void OnPointerReleased()
        {
            _sensitivityRamp.Reset();

            if (_selectedBlockData == null)
            {
                return;
            }

            if (_lastBoardPivot == null
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
                _selectedSlotIndex);

            DisconnectSelection();
        }

        private void DisconnectSelection()
        {
            _selectedBlockData = null;
            _selectedSlotIndex = -1;
            _lastBoardPivot = null;
            _gameBoard.ClearLineClearHints();
            _drawer.ClearAll();
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
            LineClearPreviewConfigSO previewConfig = placementConfig.LineClearPreview;
            if (previewConfig == null)
            {
                _gameBoard.ClearLineClearHints();
                return;
            }

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

            _gameBoard.SetLineClearHints(clearedCells, previewConfig);
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
