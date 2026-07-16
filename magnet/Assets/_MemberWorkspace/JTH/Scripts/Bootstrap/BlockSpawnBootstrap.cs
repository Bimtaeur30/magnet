using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Domain.Turn;
using JTH.Scripts.Events;
using Magnet.Contracts.BlockShapes;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    /// <summary>
    /// BlockSupply 초기화·소모 후 후보 갱신·턴(핸드 소진) 이벤트를 방송한다.
    /// </summary>
    public sealed class BlockSpawnBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private MagnetInputSO inputSO;

        [Inject] private readonly IBlockShapeSource _blockShapeSource;

        private BlockSupply _supply;
        private TurnState _turnState;

        public BlockSupply Supply => _supply;
        public TurnState Turn => _turnState;

        private void Start()
        {
            Debug.Assert(magnetGameChannel != null, "[BlockSpawnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(inputSO != null, "[BlockSpawnBootstrap] inputSO is not assigned.", this);

            var drawer = new BlockDrawer(_blockShapeSource, new SystemRandom(1));
            _supply = new BlockSupply(drawer);
            _turnState = new TurnState();

            _supply.Fill();
            _turnState.BeginFirstTurn();
            RaiseTurnStarted();
            RaiseCandidatesUpdated();
        }

        private void OnEnable()
        {
            inputSO.OnSlotSelected += OnBlockSelected;
            magnetGameChannel.AddListener<BlockSelectedOnUIEvent>(OnBlockSelected);
        }

        private void OnDisable()
        {
            inputSO.OnSlotSelected -= OnBlockSelected;
            magnetGameChannel.RemoveListener<BlockSelectedOnUIEvent>(OnBlockSelected);
        }

        public void Consume(int slotIndex)
        {
            _supply.Consume(slotIndex);

            if (_supply.AreAllSlotsEmpty())
            {
                RaiseTurnEnded();
                _supply.Fill();
                _turnState.AdvanceAfterHandExhausted();
                RaiseTurnStarted();
            }

            RaiseCandidatesUpdated();
        }

        private void OnBlockSelected(BlockSelectedOnUIEvent data)
            => OnBlockSelected(data.Index);

        private void OnBlockSelected(int index)
        {
            if (index < 0 || index >= BlockSupply.SlotCount)
            {
                return;
            }

            if (_supply.Candidates == null || index >= _supply.Candidates.Count)
            {
                return;
            }

            IBlockShape shape = _supply.Candidates[index];
            if (shape == null)
            {
                return;
            }

            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockSelectedEvent.Init(index, shape));
        }

        private void RaiseCandidatesUpdated()
        {
            IBlockShape[] candidates = _supply.CreateSnapshot();
            int[] candidateDegreesClockwise = new int[candidates.Length];
            for (int i = 0; i < candidates.Length; i++)
            {
                candidateDegreesClockwise[i] = candidates[i] is RotatedBlockShape rotatedShape
                    ? rotatedShape.DegreesClockwise
                    : 0;
            }

            magnetGameChannel.RaiseEvent(
                MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(
                    candidates,
                    candidateDegreesClockwise));
        }

        private void RaiseTurnStarted()
        {
            magnetGameChannel.RaiseEvent(
                MagnetGameEvents.TurnStartedEvent.Init(_turnState.TurnIndex));
        }

        private void RaiseTurnEnded()
        {
            magnetGameChannel.RaiseEvent(
                MagnetGameEvents.TurnEndedEvent.Init(_turnState.TurnIndex));
        }
    }
}
