using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Contracts.BlockShapes;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    /// <summary>
    /// BoardSession·BlockPlacementService 생성, 선택 이벤트로 스테이징 뷰 갱신.
    /// </summary>
    public sealed class BoardPlacementBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;

        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        [Inject] private readonly BlockPieceView _stagingBlockView;

        private BoardSession _session;
        private BlockPlacementService _placementService;
        private int _selectedSlotIndex = -1;
        private IBlockShape _selectedShape;
        private Vector2Int _stagingPivot;

        public BoardSession Session => _session;
        public BlockPlacementService PlacementService => _placementService;
        public int SelectedSlotIndex => _selectedSlotIndex;
        public IBlockShape SelectedShape => _selectedShape;
        public Vector2Int StagingPivot => _stagingPivot;

        /// <summary>Phase 4에서 Consume 호출용. Phase 2에서는 참조만 보유.</summary>
        public BlockSpawnBootstrap BlockSpawn => _blockSpawnBootstrap;

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[BoardPlacementBootstrap] BoardConfigSO is not assigned.", this);
            Debug.Assert(placementConfig != null, "[BoardPlacementBootstrap] PlacementConfigSO is not assigned.", this);
            Debug.Assert(_blockSpawnBootstrap != null, "[BoardPlacementBootstrap] BlockSpawnBootstrap was not injected.", this);
            Debug.Assert(_stagingBlockView != null, "[BoardPlacementBootstrap] BlockPieceView was not injected.", this);

            _session = new BoardSession(boardConfig.BoardSize);
            _placementService = new BlockPlacementService(_session);
            _stagingPivot = new Vector2Int(0, placementConfig.GetStagingY(boardConfig.CellsPerSide));
        }

        private void OnEnable()
        {
            Debug.Assert(magnetGameChannel != null, "[BoardPlacementBootstrap] magnetGameChannel is not assigned.", this);
            magnetGameChannel.AddListener<BlockSelectedEvent>(OnBlockSelected);
        }

        private void OnDisable()
        {
            if (magnetGameChannel != null)
            {
                magnetGameChannel.RemoveListener<BlockSelectedEvent>(OnBlockSelected);
            }
        }

        private void Start()
        {
            Debug.Log($"[BoardPlacement] Session ready (boardSize={boardConfig.BoardSize}, stagingY={_stagingPivot.y})");
        }

        private void OnBlockSelected(BlockSelectedEvent evt)
        {
            _selectedSlotIndex = evt.SlotIndex;
            _selectedShape = evt.Shape;
            _stagingBlockView.Show(_selectedShape, _stagingPivot);
            Debug.Log($"[BoardPlacement] Staging slot {_selectedSlotIndex} at {_stagingPivot} ({_selectedShape.ShapeId})");
        }
    }
}
