using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Events;
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
        [SerializeField] private MagnetInputSO inputSO;

        [Inject] private readonly IBlockShapeSource _blockShapeSource;

        private BlockSupply _supply;
        private int _selectedSlotIndex = -1;
        private IBlockShape _selectedShape;

        private void Start()
        {
            Debug.Assert(magnetGameChannel != null, "[BlockSpawnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(inputSO != null, "[BlockSpawnBootstrap] inputSO is not assigned.", this);
            
            var drawer = new BlockDrawer(_blockShapeSource, new SystemRandom(1));
            _supply = new BlockSupply(drawer);
            _supply.Fill();
        }

        private void OnEnable()
        {
            inputSO.OnSlotSelected += OnBlockSelected;
        }

        private void OnDisable()
        {
            inputSO.OnSlotSelected -= OnBlockSelected;
        }
        
        public void Consume() => _supply.Consume(_selectedSlotIndex);

        private void OnBlockSelected(int index)
        {
            if (index < 0 || index >= BlockSupply.SlotCount)
                return;
            if (_supply.Candidates == null || index >= _supply.Candidates.Count)
                return;
            
            _selectedSlotIndex = index;
            _selectedShape = _supply.Candidates[index];
            
            if (_selectedShape == null)
                return;

            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockSelectedEvent
                .Init(_selectedSlotIndex, _selectedShape));
        }
    }
}
