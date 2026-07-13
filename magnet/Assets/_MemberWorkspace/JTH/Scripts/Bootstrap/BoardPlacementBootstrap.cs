using Cysharp.Threading.Tasks;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Domain.Rotation;
using JTH.Scripts.Domain.Turn;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Contracts.BlockShapes;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    /// <summary>
    /// BoardSession·BlockPlacementService 생성, 부착 후 클리어·회전·Consume까지 턴 Domain·연출을 순차 처리한다.
    /// </summary>
    public sealed class BoardPlacementBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private BoardConfigSO boardConfig;

        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        [Inject] private readonly PlacedBlocksView _placedBlocksView;

        private BoardSession _session;
        private BlockPlacementService _placementService;
        private readonly SquareClearService _clearService = new();
        private readonly BoardRotationService _rotationService = new();

        public BoardSession Session => _session;
        public BlockPlacementService PlacementService => _placementService;

        public BlockSpawnBootstrap BlockSpawn => _blockSpawnBootstrap;

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[BoardPlacementBootstrap] BoardConfigSO is not assigned.", this);
            Debug.Assert(_blockSpawnBootstrap != null, "[BoardPlacementBootstrap] BlockSpawnBootstrap was not injected.", this);
            Debug.Assert(_placedBlocksView != null, "[BoardPlacementBootstrap] PlacedBlocksView was not injected.", this);

            _session = new BoardSession(boardConfig.BoardSize);
            _placementService = new BlockPlacementService(_session);
        }

        /// <summary>
        /// Place → Clear → Rotate 를 Domain → Raise → await FX 순으로 실행한다.
        /// </summary>
        public async UniTask<TurnResolutionResult> TryConfirmPlacement(
            IBlockShape shape,
            Vector2Int startPivot,
            int slotIndex,
            ShapeBlock staging)
        {
            PlacementResult result = _placementService.TryPlace(shape, startPivot);
            if (!result.Success)
            {
                if (staging != null)
                {
                    staging.Clear();
                    Destroy(staging.gameObject);
                }

                return new TurnResolutionResult(result, ClearDetectionResult.None, boardRotated: false);
            }

            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockPlacedEvent.Init(
                result.BlockId,
                slotIndex,
                shape.ShapeId,
                result.FinalPivot,
                result.CellPositions));

            await _placedBlocksView.PlayPlaceAsync(staging, result.BlockId);

            ClearDetectionResult clearResult = _clearService.DetectAndApply(_session);
            RaiseSquareClearedEvents(clearResult);
            await _placedBlocksView.PlayClearAsync();

            if (clearResult.HasCellsOutsideBounds)
            {
                magnetGameChannel.RaiseEvent(MagnetGameEvents.BoundaryViolationEvent);
                return new TurnResolutionResult(result, clearResult, boardRotated: false);
            }

            _rotationService.RotateClockwise(_session);
            magnetGameChannel.RaiseEvent(MagnetGameEvents.BoardRotatedEvent.Init(
                BoardRotationService.DefaultDegreesClockwise));

            await _placedBlocksView.PlayRotateAsync();

            _blockSpawnBootstrap.Consume(slotIndex);

            return new TurnResolutionResult(result, clearResult, boardRotated: true);
        }

        private void RaiseSquareClearedEvents(ClearDetectionResult clearResult)
        {
            foreach (ClearedSquareInfo square in clearResult.ClearedSquares)
            {
                magnetGameChannel.RaiseEvent(MagnetGameEvents.SquareClearedEvent.Init(
                    square.SquareSize,
                    scoreAwarded: 0,
                    square.ClearedCells));
            }
        }
    }
}
