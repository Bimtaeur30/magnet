using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Events;
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

        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;

        private BoardSession _session;
        private BlockPlacementService _placementService;

        public BoardSession Session => _session;
        public BlockPlacementService PlacementService => _placementService;

        /// <summary>Phase 4에서 Consume 호출용. Phase 2에서는 참조만 보유.</summary>
        public BlockSpawnBootstrap BlockSpawn => _blockSpawnBootstrap;

        public PlacementResult TryConfirmPlacement(IBlockShape shape, Vector2Int startPivot, int slotIndex)
        {
            PlacementResult result = _placementService.TryPlace(shape, startPivot);
            if (!result.Success)
            {
                return result;
            }

            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockPlacedEvent.Init(
                result.BlockId,
                slotIndex,
                shape.ShapeId,
                result.FinalPivot,
                result.CellPositions));

            if (result.HasCellsOutsideBounds)
            {
                magnetGameChannel.RaiseEvent(MagnetGameEvents.BoundaryViolationEvent);
            }

            _blockSpawnBootstrap.Consume(slotIndex);
            return result;
        }

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[BoardPlacementBootstrap] BoardConfigSO is not assigned.", this);
            Debug.Assert(_blockSpawnBootstrap != null, "[BoardPlacementBootstrap] BlockSpawnBootstrap was not injected.", this);

            _session = new BoardSession(boardConfig.BoardSize);
            _placementService = new BlockPlacementService(_session);
        }
    }
}
