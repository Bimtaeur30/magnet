using System.Collections.Generic;
using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Contracts;
using Magnet.Core.Events;
using Magnet.Core.SO.Skin;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class BoardPlacementBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private EventChannelSO soundChannel;
        [SerializeField] private EventChannelSO skinChannel;
        [SerializeField] private SoundClipSO blockPlaceSound;
        [SerializeField] private SoundClipSO blockExplodeSound;

        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        [Inject] private GameBoard _gameBoard;

        private SkinDataSO _currentSkin;

        private void Awake()
        {
            Debug.Assert(inGameChannel != null, "[BoardPlacementBootstrap] inGameChannel is not assigned.", this);
            Debug.Assert(skinChannel != null, "[BoardPlacementBootstrap] skinChannel is not assigned.", this);
            Debug.Assert(_blockSpawnBootstrap != null, "[BoardPlacementBootstrap] BlockSpawnBootstrap was not injected.", this);
            Debug.Assert(_gameBoard != null, "[BoardPlacementBootstrap] GameBoard was not injected.", this);
        }

        private void OnEnable()
        {
            skinChannel.AddListener<SkinChangedEvent>(OnSkinChanged);
            skinChannel.AddListener<SkinInitializedEvent>(OnSkinInitialized);
        }

        private void OnDisable()
        {
            skinChannel.RemoveListener<SkinChangedEvent>(OnSkinChanged);
            skinChannel.RemoveListener<SkinInitializedEvent>(OnSkinInitialized);
        }

        public void PlaceBlock(
            IReadOnlyList<Block> detached,
            IReadOnlyList<Vector2Int> gridOffsets,
            int slotIndex,
            int skinId)
        {
            if (!IsPlacementFree(gridOffsets))
            {
                _gameBoard.ReturnUnplacedBlocks(detached);
                return;
            }

            int filledBefore = CountFilledSlots();
            bool firstDrop = filledBefore == BlockSupply.SlotCount;
            bool lastDrop = filledBefore == 1;

            _gameBoard.AddBlock(detached, gridOffsets);
            _blockSpawnBootstrap.Consume(slotIndex);

            ClearedLineResult clearedLineResult = LineClearService.DetectAndApply(_gameBoard);
            _blockSpawnBootstrap.RecordPlayerMove(slotIndex, gridOffsets, lastDrop);

            PlaySound(ResolvePlaceSound());
            if (clearedLineResult.ClearedLineCount > 0)
                PlaySound(ResolveLineClearSound());

            PlacementResult placementResult = new PlacementResult(
                _blockSpawnBootstrap.Candidates,
                gridOffsets,
                clearedLineResult,
                firstDrop,
                lastDrop,
                skinId);

            inGameChannel.RaiseEvent(InGameEvents.BlockPlacedEvent.Init(placementResult));
        }

        private bool IsPlacementFree(IReadOnlyList<Vector2Int> gridOffsets)
        {
            for (int i = 0; i < gridOffsets.Count; ++i)
            {
                Vector2Int cell = gridOffsets[i];
                if (!_gameBoard.Grid.IsInBounds(cell) || _gameBoard.Grid.IsOccupied(cell))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnSkinChanged(SkinChangedEvent evt)
        {
            _currentSkin = evt.CurrentSkin;
        }

        private void OnSkinInitialized(SkinInitializedEvent evt)
        {
            _currentSkin = evt.Skin;
        }

        private SoundClipSO ResolvePlaceSound()
        {
            if (_currentSkin != null && _currentSkin.PlaceSound != null)
            {
                return _currentSkin.PlaceSound;
            }

            return blockPlaceSound;
        }

        private SoundClipSO ResolveLineClearSound()
        {
            if (_currentSkin != null && _currentSkin.LineClearSound != null)
            {
                return _currentSkin.LineClearSound;
            }

            return blockExplodeSound;
        }

        private void PlaySound(SoundClipSO clip)
        {
            if (soundChannel == null || clip == null)
                return;

            soundChannel.RaiseEvent(SoundSystemEvents.PlaySoundEvent.Init(clip));
        }

        private int CountFilledSlots()
        {
            IReadOnlyList<ShapeBlockData> candidates = _blockSpawnBootstrap.Candidates;

            int filled = 0;
            foreach (var block in candidates)
            {
                if (block != null)
                {
                    filled++;
                }
            }

            return filled;
        }
    }
}
