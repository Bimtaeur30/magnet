using System.Collections.Generic;
using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using JTH.Scripts.Domain.Board;
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
        /// <summary>방금 놓은 블럭의 8방향 인접 칸이 이 비율 이상 차 있으면 따봉 이벤트를 발행한다.</summary>
        private const float NeighborFillThreshold = 0.75f;

        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO soundChannel;
        [SerializeField] private EventChannelSO skinChannel;
        [SerializeField] private SoundClipSO blockPlaceSound;

        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        [Inject] private GameBoard _gameBoard;

        private SkinDataSO _currentSkin;

        private void Awake()
        {
            Debug.Assert(inGameChannel != null, "[BoardPlacementBootstrap] inGameChannel is not assigned.", this);
            Debug.Assert(magnetGameChannel != null, "[BoardPlacementBootstrap] magnetGameChannel is not assigned.", this);
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

            // 라인 클리어 반영 전, 방금 놓은 그 자리 기준으로 인접 칸 점유율을 본다.
            bool neighborsMostlyFilled = AreNeighborsMostlyFilled(gridOffsets);

            ClearedLineResult clearedLineResult = LineClearService.DetectAndApply(_gameBoard);
            _blockSpawnBootstrap.RecordPlayerMove(
                slotIndex, gridOffsets, lastDrop, clearedLineResult.ClearedLineCount, neighborsMostlyFilled);

            RaiseClearMilestone(lastDrop);

            if (clearedLineResult.ClearedLineCount > 0)
                PlaySound(ResolveLineClearSound());
            else
                PlaySound(ResolvePlaceSound());

            PlacementResult placementResult = new PlacementResult(
                _blockSpawnBootstrap.Candidates,
                gridOffsets,
                clearedLineResult,
                firstDrop,
                lastDrop,
                skinId);

            inGameChannel.RaiseEvent(InGameEvents.BlockPlacedEvent.Init(placementResult));
        }

        /// <summary>
        /// 라인 클리어까지 반영된 시점에 올클리어 / 퍼펙트를 판정해 알린다.
        /// 둘 다 성립하면 올클리어만 발행한다.
        /// </summary>
        private void RaiseClearMilestone(bool lastDrop)
        {
            if (_gameBoard.Grid.IsEmpty())
            {
                magnetGameChannel.RaiseEvent(MagnetGameEvents.AllClearEvent);
                return;
            }

            if (lastDrop && _blockSpawnBootstrap.LastHandWasPerfect)
            {
                magnetGameChannel.RaiseEvent(MagnetGameEvents.PerfectClearEvent);
            }
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

        /// <summary>
        /// 방금 놓은 블럭 칸들의 8방향(대각선 포함) 인접 칸 중 블럭 자신을 뺀 나머지가
        /// <see cref="NeighborFillThreshold"/> 이상 채워져 있는지. 보드 밖(벽)은 채워진 것으로 본다.
        /// 라인 클리어가 반영되기 전 상태에서 판정한다.
        /// </summary>
        private bool AreNeighborsMostlyFilled(IReadOnlyList<Vector2Int> placedCells)
        {
            BoardGrid grid = _gameBoard.Grid;
            if (grid == null || placedCells == null || placedCells.Count == 0)
            {
                return false;
            }

            HashSet<Vector2Int> placed = new HashSet<Vector2Int>(placedCells.Count);
            for (int i = 0; i < placedCells.Count; ++i)
            {
                placed.Add(placedCells[i]);
            }

            HashSet<Vector2Int> neighbors = new HashSet<Vector2Int>();
            for (int i = 0; i < placedCells.Count; ++i)
            {
                Vector2Int origin = placedCells[i];
                for (int dx = -1; dx <= 1; ++dx)
                {
                    for (int dy = -1; dy <= 1; ++dy)
                    {
                        if (dx == 0 && dy == 0)
                        {
                            continue;
                        }

                        Vector2Int cell = new Vector2Int(origin.x + dx, origin.y + dy);
                        if (!placed.Contains(cell))
                        {
                            neighbors.Add(cell);
                        }
                    }
                }
            }

            if (neighbors.Count == 0)
            {
                return false;
            }

            int filled = 0;
            foreach (Vector2Int cell in neighbors)
            {
                if (!grid.IsInBounds(cell) || grid.IsOccupied(cell))
                {
                    ++filled;
                }
            }

            return (float)filled / neighbors.Count >= NeighborFillThreshold;
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

            return null;
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
