using GameLib.EventChannelSystem;
using JTH.Scripts.Bootstrap;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Domain.Turn;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Contracts.BlockShapes;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Input
{
    /// <summary>
    /// 키보드로 선택한 블록을 포인터 드래그로 x축 이동. 감도 램프·Simulate 프리뷰·부착 확정.
    /// 표시는 <see cref="BlockDragDrawer"/>에 위임한다.
    /// </summary>
    [RequireComponent(typeof(BlockDragDrawer))]
    public sealed class BlockDragInput : MonoBehaviour
    {
        [SerializeField] private MagnetInputSO magnetInput;
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;
        [SerializeField] private EventChannelSO magnetGameChannel;

        [Inject] private readonly BoardPlacementBootstrap _placementBootstrap;
        [Inject] private readonly PlacedBlocksView _placedBlocksView;

        private BlockDragDrawer _drawer;
        private DragSensitivityRamp _sensitivityRamp;

        private IBlockShape _selectedShape;
        private int _selectedSlotIndex = -1;
        private int _stagingGridY;
        private float _shapeCenterOffsetX;
        private int _minPivotX;
        private int _maxPivotX;
        private float _minWorldCenterX;
        private float _maxWorldCenterX;

        private float _blockWorldCenterX;
        private bool _isPlacing;

        private void Awake()
        {
            Debug.Assert(placementConfig != null, "[BlockDragInput] placementConfig is not assigned.", this);
            Debug.Assert(boardConfig != null, "[BlockDragInput] boardConfig is not assigned.", this);
            Debug.Assert(_placementBootstrap != null, "[BlockDragInput] BoardPlacementBootstrap was not injected.", this);
            Debug.Assert(_placedBlocksView != null, "[BlockDragInput] PlacedBlocksView was not injected.", this);

            _drawer = GetComponent<BlockDragDrawer>();
            _sensitivityRamp = new DragSensitivityRamp(
                placementConfig.DragSensitivityRampPerUnit,
                placementConfig.DragSensitivityMaxMultiplier);
            _stagingGridY = placementConfig.GetStagingY(boardConfig.CellsPerSide);
        }

        private void OnEnable()
        {
            magnetInput.OnPointerPressed += OnPointerPressed;
            magnetInput.OnPointerReleased += OnPointerReleased;
            magnetInput.OnPointerChange += OnPointerMoved;
            magnetGameChannel.AddListener<BlockSelectedEvent>(OnBlockSelected);
        }

        private void OnDisable()
        {
            if (magnetInput != null)
            {
                magnetInput.OnPointerPressed -= OnPointerPressed;
                magnetInput.OnPointerReleased -= OnPointerReleased;
                magnetInput.OnPointerChange -= OnPointerMoved;
            }

            magnetGameChannel?.RemoveListener<BlockSelectedEvent>(OnBlockSelected);
        }

        private void OnBlockSelected(BlockSelectedEvent evt)
        {
            if (_isPlacing)
            {
                return;
            }

            _selectedShape = evt.Shape;
            _selectedSlotIndex = evt.SlotIndex;
            _sensitivityRamp.Reset();
            _drawer.ClearPreview();

            Vector2 centerOffset = BlockPlacementCells.GetShapeCenterOffset(_selectedShape.CellOffsets);
            _shapeCenterOffsetX = centerOffset.x;
            BlockPlacementCells.GetPivotXRange(_selectedShape, boardConfig.BoardSize, out _minPivotX, out _maxPivotX);
            BlockPlacementCells.GetWorldCenterXRange(
                _minPivotX,
                _maxPivotX,
                boardConfig.CellSize,
                _shapeCenterOffsetX,
                out _minWorldCenterX,
                out _maxWorldCenterX);

            _blockWorldCenterX = BlockPlacementCells.PivotXToWorldCenterX(0, boardConfig.CellSize, _shapeCenterOffsetX);
            _drawer.ShowStaging(_selectedShape, _blockWorldCenterX, _stagingGridY);
        }

        private void OnPointerPressed()
        {
            if (_selectedShape == null || _isPlacing)
            {
                return;
            }

            BeginDrag();
        }

        private void BeginDrag()
        {
            float pointerWorldX = magnetInput.GetWorldPointerPosition().x;
            _sensitivityRamp.Begin(pointerWorldX);
            _blockWorldCenterX = Mathf.Clamp(pointerWorldX, _minWorldCenterX, _maxWorldCenterX);

            UpdateViews();
        }

        private void OnPointerMoved(Vector2 _)
        {
            if (_selectedShape == null || !magnetInput.IsPointerPressed || _isPlacing)
            {
                return;
            }

            float pointerWorldX = magnetInput.GetWorldPointerPosition().x;
            float blockDeltaX = _sensitivityRamp.UpdateDelta(pointerWorldX);
            _blockWorldCenterX = Mathf.Clamp(_blockWorldCenterX + blockDeltaX, _minWorldCenterX, _maxWorldCenterX);

            UpdateViews();
        }

        private void OnPointerReleased()
        {
            if (_selectedShape == null || _isPlacing)
            {
                return;
            }

            _sensitivityRamp.Reset();

            Vector2Int pivot = GetCurrentPivot();
            TurnResolutionResult turn = _placementBootstrap.TryConfirmPlacement(_selectedShape, pivot, _selectedSlotIndex);
            if (!turn.Placement.Success)
            {
                DisconnectSelection();
                return;
            }

            _drawer.ClearPreview();
            ShapeBlock staging = _drawer.TakeStagingForPlacement();
            int blockId = turn.Placement.BlockId;

            DisconnectSelection();
            _isPlacing = true;

            if (_placementBootstrap.Session.TryGetPlacedBlock(blockId, out PlacedBlock placedBlock))
            {
                BlockSnapMotion.PlayFromPlaced(
                    staging,
                    placedBlock,
                    _stagingGridY,
                    boardConfig,
                    placementConfig,
                    () =>
                    {
                        _placedBlocksView.Register(blockId, staging);
                        _placedBlocksView.Adopt(staging, $"Placed_{blockId}");
                        _isPlacing = false;
                    });
            }
            else
            {
                staging.Clear();
                Destroy(staging.gameObject);
                _isPlacing = false;
            }
        }

        private void DisconnectSelection()
        {
            _selectedShape = null;
            _selectedSlotIndex = -1;
            _drawer.ClearAll();
        }

        private void UpdateViews()
        {
            _drawer.ShowStaging(_selectedShape, _blockWorldCenterX, _stagingGridY);

            Vector2Int pivot = GetCurrentPivot();
            PlacementResult result = _placementBootstrap.PlacementService.Simulate(_selectedShape, pivot);
            if (result.Success)
            {
                _drawer.ShowPreview(_selectedShape, result.FinalPivot);
            }
            else
            {
                _drawer.ClearPreview();
            }
        }

        private Vector2Int GetCurrentPivot()
        {
            int pivotX = BlockPlacementCells.WorldCenterXToPivotX(
                _blockWorldCenterX,
                boardConfig.CellSize,
                _shapeCenterOffsetX,
                _minPivotX,
                _maxPivotX);
            return new Vector2Int(pivotX, _stagingGridY);
        }
    }
}
