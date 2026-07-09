using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Contracts.BlockShapes;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    /// <summary>
    /// BlockSupply 초기화·소모 후 후보 갱신 이벤트를 방송한다.
    /// </summary>
    public sealed class BlockSpawnBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;

        [Inject] private readonly IBlockShapeSource _blockShapeSource;
        [Inject] private readonly BlockPieceView _stagingBlockView;

        private BlockSupply _supply;
        private int _selectedSlotIndex = -1;
        private IBlockShape _selectedShape;
        private Vector2Int _stagingPivot;
        
        public BlockSupply Supply => _supply;

        private void Start()
        {
            Debug.Assert(magnetGameChannel != null, "[BlockSpawnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(_stagingBlockView != null, "[BoardPlacementBootstrap] BlockPieceView was not injected.", this);
            Debug.Assert(boardConfig != null, "[BoardPlacementBootstrap] BoardConfigSO is not assigned.", this);
            Debug.Assert(placementConfig != null, "[BoardPlacementBootstrap] PlacementConfigSO is not assigned.", this);

            _stagingPivot = new Vector2Int(0, placementConfig.GetStagingY(boardConfig.CellsPerSide));
            var drawer = new BlockDrawer(_blockShapeSource, new SystemRandom(1));
            _supply = new BlockSupply(drawer);
            _supply.Fill();
            RaiseCandidatesUpdated();
        }

        private void OnEnable()
        {
            Debug.Assert(magnetGameChannel != null, "[BoardPlacementBootstrap] magnetGameChannel is not assigned.", this);
            magnetGameChannel.AddListener<BlockSelectedEvent>(OnBlockSelected);
        }

        private void OnDisable()
        {
            magnetGameChannel?.RemoveListener<BlockSelectedEvent>(OnBlockSelected);
        }

        private void OnBlockSelected(BlockSelectedEvent evt)
        {
            _selectedSlotIndex = evt.SlotIndex;
            _selectedShape = evt.Shape;
            _stagingBlockView.Show(_selectedShape, _stagingPivot);
        }
        
        public void Consume()
        {
            _supply.Consume(_selectedSlotIndex);
            RaiseCandidatesUpdated();
        }

        private void RaiseCandidatesUpdated()
        {
            var snapshot = _supply.CreateSnapshot();
            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(snapshot));
        }
    }
}
