using GameLib.EventChannelSystem;
using JTH.Scripts.Domain.Skin;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Events;
using JTH.Scripts.Input;
using JTH.Scripts.Presentation;
using Magnet.Contracts;
using Magnet.Core.Events;
using Magnet.Core.SO.Block;
using Magnet.Core.SO.Skin;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class BlockSpawnBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private MagnetInputSO inputSO;
        [SerializeField] private SkinDataListSO skinDataListSO;
        [SerializeField] private BlockShapeSourceSO shapeSourceSO;
        
        [Inject] private GameBoard _gameBoard;
        
        private BlockSupply _supply;
        
        public BlockSupply Supply => _supply;

        private void Awake()
        {
            Debug.Assert(magnetGameChannel != null, "[BlockSpawnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(inputSO != null, "[BlockSpawnBootstrap] inputSO is not assigned.", this);

            #if UNITY_EDITOR
            inputSO.OnSlotSelected += OnBlockSelected;
            #endif
            magnetGameChannel.AddListener<BlockSelectedOnUIEvent>(OnBlockSelected);
        }
        
        private void Start()
        {
            _supply = new BlockSupply(new RandomDrawer(), new SkinSession(skinDataListSO));
        
            _supply.Fill(new BlockSpawnContext(shapeSourceSO, _gameBoard.Grid, 0));
        }
        
        private void OnDestroy()
        {
            #if UNITY_EDITOR
            inputSO.OnSlotSelected -= OnBlockSelected;
            #endif
            magnetGameChannel.RemoveListener<BlockSelectedOnUIEvent>(OnBlockSelected);
        }

        public void Consume(int slotIndex)
            => _supply.Consume(slotIndex);
        
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
        
            ShapeBlockData block = _supply.Candidates[index];
            if (block == null)
            {
                return;
            }
        
            inGameChannel.RaiseEvent(InGameEvents.BlockSelectedEvent.Init(index, block));
        }
    }
}
