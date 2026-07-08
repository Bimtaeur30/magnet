using GameLib.EventChannelSystem;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Events;
using Magnet.Contracts.BlockShapes;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace JTH.Scripts.Bootstrap
{
    /// <summary>
    /// BlockSupply 초기화·소모 후 후보 갱신 이벤트를 방송한다.
    /// </summary>
    public sealed class BlockSpawnBootstrap : MonoBehaviour
    {
        [FormerlySerializedAs("mainEventChannelSO")]
        [FormerlySerializedAs("_eventChannel")]
        [SerializeField] private EventChannelSO magnetGameChannel;

        [Inject] private readonly IBlockShapeSource _blockShapeSource;

        private BlockSupply _supply;

        public BlockSupply Supply => _supply;

        private void Start()
        {
            Debug.Assert(magnetGameChannel != null, "[BlockSpawnBootstrap] magnetGameChannel is not assigned.", this);

            var drawer = new BlockDrawer(_blockShapeSource, new SystemRandom());
            _supply = new BlockSupply(drawer);
            _supply.Fill();
            RaiseCandidatesUpdated();
        }

        public void Consume(int slotIndex)
        {
            _supply.Consume(slotIndex);
            RaiseCandidatesUpdated();
        }

        private void RaiseCandidatesUpdated()
        {
            var snapshot = _supply.CreateSnapshot();
            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(snapshot));
            Debug.Log($"[BlockSpawn] BlockCandidatesUpdatedEvent raised ({BlockSupply.SlotCount} slots)");
        }
    }
}
