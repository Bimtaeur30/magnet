using GameLib.EventChannelSystem;
using JTH.Scripts.Bootstrap;
using JTH.Scripts.Data;
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
        private Vector2 _blockWorldCenter;
        private Vector2 _halfSize;
        private Vector2Int? _lastBoardPivot;

        private void Awake()
        {
            Debug.Assert(placementConfig != null, "[BlockDragInput] placementConfig is not assigned.", this);
            Debug.Assert(boardConfig != null, "[BlockDragInput] boardConfig is not assigned.", this);
            Debug.Assert(_placementBootstrap != null, "[BlockDragInput] BoardPlacementBootstrap was not injected.", this);
            Debug.Assert(_gameBoard != null, "[BlockDragInput] _gameBoard was not injected.", this);
        
            _drawer = GetComponent<BlockDragDrawer>();
            _sensitivityRamp = new DragSensitivityRamp(
                placementConfig.Drag.SensitivityRampPerUnit,
                placementConfig.Drag.SensitivityMaxMultiplier);
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
        }
        
        private void OnBlockSelected(BlockSelectedEvent evt)
        {
            _selectedBlockData = evt.BlockData;
            _selectedSlotIndex = evt.SlotIndex;
            _sensitivityRamp.Reset();
            _drawer.ClearAll();
        
            _drawer.ShowStaging(evt.BlockData);

            Vector2 worldPointerPos = inputSO.GetWorldPointerPosition();
            float startXPosition = placementConfig.Drag.StagingBlockStartXPositions[_selectedSlotIndex];
            Vector2 startPosition = new Vector2(startXPosition, _gameBoard.GetStartStagingY());
            
            _sensitivityRamp.Begin(worldPointerPos);
            _blockWorldCenter = startPosition;
            
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (Vector2Int offset in _selectedBlockData.CellOffsets)
            {
                if (maxX < offset.x)
                    maxX = offset.x;
                if (maxY < offset.y)
                    maxY = offset.y;
            }
            
            _halfSize = new Vector2(maxX / 2f, maxY / 2f);
            
            UpdateViews();
        }
        
        private void OnPointerMoved(Vector2 _)
        {
            if (_selectedBlockData == null || !inputSO.IsPointerPressed)
            {
                return;
            }
        
            Vector2 delta = _sensitivityRamp.UpdateDelta(inputSO.GetWorldPointerPosition());
            _blockWorldCenter += delta;
        
            UpdateViews();
        }
        
        private void OnPointerReleased()
        {
            _sensitivityRamp.Reset();
        
            if (_lastBoardPivot == null)
            {
                DisconnectSelection();
                return;
            }
        
            _placementBootstrap.PlaceBlock(
                _drawer.GetStagingBlocks(),
                _lastBoardPivot.Value,
                _selectedBlockData.CellOffsets,
                _selectedSlotIndex);
            
            DisconnectSelection();
        }
        
        private void DisconnectSelection()
        {
            _selectedBlockData = null;
            _selectedSlotIndex = -1;
            _drawer.ClearAll();
        }
        
        private void UpdateViews()
        {
            _drawer.MoveStaging(_blockWorldCenter);
            bool hadPreview = _lastBoardPivot != null;

            float threshold = placementConfig.Drag.LastPivotSnapThreshold * boardConfig.CellSize;
            Vector2 boardLocal = _gameBoard.WorldToBoardLocal(_blockWorldCenter - _halfSize);
            if (!PlacementService.TryGetBoardPivot(boardLocal, _selectedBlockData.CellOffsets,
                    _gameBoard.Grid, _lastBoardPivot, threshold,
                    out Vector2Int boardPivot))
            {
                _lastBoardPivot = null;
                _drawer.ClearPreview();
                return;
            }
            
            _lastBoardPivot = boardPivot;

            if (!hadPreview)
            {
                _drawer.ShowPreview(_selectedBlockData);
            }
            _drawer.MovePreview(_gameBoard.BoardLocalToWorld(boardPivot));
        }
    }
}
