using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLib.EventChannelSystem;
using JTH.Scripts.Bootstrap;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;
using Magnet.Contracts.BlockSkins;
using PMS.Scripts.Events;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 부착 완료된 칸 View를 cellId로 추적하고, Place·Clear재조립·Rotate 연출을 재생한다.
    /// </summary>
    public sealed class PlacedBlocksView : MonoBehaviour
    {
        [SerializeField] private EventChannelSO skinChannel;
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;

        [Inject] private readonly BoardPlacementBootstrap _placementBootstrap;

        private readonly Dictionary<int, OccupiedCellView> _cellsById = new();

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[PlacedBlocksView] boardConfig is not assigned.", this);
            Debug.Assert(skinChannel != null, "[PlacedBlocksView] skinChannel is not assigned.", this);
            Debug.Assert(placementConfig != null, "[PlacedBlocksView] placementConfig is not assigned.", this);
            Debug.Assert(_placementBootstrap != null, "[PlacedBlocksView] BoardPlacementBootstrap was not injected.", this);

            skinChannel.AddListener<SkinInitializedEvent>(OnSkinInitialized);
            skinChannel.AddListener<SkinChangedEvent>(OnSkinChanged);
        }

        private void OnDestroy()
        {
            if (skinChannel == null)
            {
                return;
            }

            skinChannel.RemoveListener<SkinInitializedEvent>(OnSkinInitialized);
            skinChannel.RemoveListener<SkinChangedEvent>(OnSkinChanged);
        }

        private void OnSkinInitialized(SkinInitializedEvent evt)
        {
            ApplySkinToAll(evt.Skin);
        }

        private void OnSkinChanged(SkinChangedEvent evt)
        {
            ApplySkinToAll(evt.CurrentSkin);
        }

        private void ApplySkinToAll(IBlockSkin skin)
        {
            if (skin == null)
            {
                return;
            }

            Sprite sprite = skin.Sprite;
            foreach (KeyValuePair<int, OccupiedCellView> entry in _cellsById)
            {
                if (entry.Value != null)
                {
                    entry.Value.ApplyVisual(sprite);
                }
            }
        }

        /// <summary>
        /// 스테이징 ShapeBlock을 Y 스냅한 뒤 칸 View로 분해·등록한다.
        /// </summary>
        public UniTask PlayPlaceAsync(ShapeBlock staging, PlacementResult placement)
        {
            if (staging == null || placement == null || !placement.Success)
            {
                return UniTask.CompletedTask;
            }

            IReadOnlyList<int> cellIds = placement.CellIds;
            IReadOnlyList<Vector2Int> positions = placement.CellPositions;
            if (cellIds.Count == 0)
            {
                staging.Clear();
                Destroy(staging.gameObject);
                return UniTask.CompletedTask;
            }

            var offsets = new List<Vector2Int>(positions.Count);
            for (int i = 0; i < positions.Count; i++)
            {
                offsets.Add(positions[i] - placement.FinalPivot);
            }

            int stagingGridY = placementConfig.GetStagingY(boardConfig.CellsPerSide);
            var completion = new UniTaskCompletionSource();
            BlockSnapMotion.PlayFromOffsets(
                staging,
                offsets,
                placement.FinalPivot,
                stagingGridY,
                boardConfig,
                placementConfig,
                () =>
                {
                    SplitStagingIntoCells(staging, cellIds);
                    completion.TrySetResult();
                });
            return completion.Task;
        }

        public async UniTask PlayReassemblyAsync(ClearReassemblyResult reassembly)
        {
            if (reassembly == null || !reassembly.HasAnyWave)
            {
                return;
            }

            for (int w = 0; w < reassembly.Waves.Count; w++)
            {
                ClearWave wave = reassembly.Waves[w];
                DestroyCellViews(wave.DestroyedCellIds);

                await PlayWaveRelocationsAsync(wave);
            }
        }

        public UniTask PlayRotateAsync()
        {
            var completion = new UniTaskCompletionSource();
            AnimateBoardRotation(() => completion.TrySetResult());
            return completion.Task;
        }

        public void SyncWithSession()
        {
            BoardSession session = _placementBootstrap.Session;
            var staleIds = new List<int>();

            foreach (KeyValuePair<int, OccupiedCellView> entry in _cellsById)
            {
                if (!session.TryGetCell(entry.Key, out OccupiedCell cell))
                {
                    Destroy(entry.Value.gameObject);
                    staleIds.Add(entry.Key);
                    continue;
                }

                entry.Value.SnapToGrid(cell.Position, boardConfig.CellSize, placementConfig.CellFill);
            }

            for (int i = 0; i < staleIds.Count; i++)
            {
                _cellsById.Remove(staleIds[i]);
            }

            IReadOnlyList<OccupiedCell> cells = session.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                OccupiedCell cell = cells[i];
                if (_cellsById.ContainsKey(cell.CellId))
                {
                    continue;
                }

                // Domain에만 있는 칸은 시각 복구하지 않음(연출 경로에서 등록).
            }
        }

        private void SplitStagingIntoCells(ShapeBlock staging, IReadOnlyList<int> cellIds)
        {
            List<(Block block, Vector2Int grid)> detached = staging.DetachActiveBlocks();
            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.CellFill;

            int count = Mathf.Min(detached.Count, cellIds.Count);
            for (int i = 0; i < count; i++)
            {
                int cellId = cellIds[i];
                (Block block, Vector2Int grid) = detached[i];

                var go = new GameObject($"Cell_{cellId}");
                go.transform.SetParent(transform, worldPositionStays: false);
                OccupiedCellView view = go.AddComponent<OccupiedCellView>();
                view.Bind(block, grid, cellSize, fill);
                _cellsById[cellId] = view;
            }

            for (int i = count; i < detached.Count; i++)
            {
                Destroy(detached[i].block.gameObject);
            }

            Destroy(staging.gameObject);
        }

        private void DestroyCellViews(IReadOnlyList<int> cellIds)
        {
            for (int i = 0; i < cellIds.Count; i++)
            {
                int cellId = cellIds[i];
                if (!_cellsById.TryGetValue(cellId, out OccupiedCellView view))
                {
                    continue;
                }

                _cellsById.Remove(cellId);
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }
        }

        private async UniTask PlayWaveRelocationsAsync(ClearWave wave)
        {
            if (wave.Relocations.Count == 0)
            {
                return;
            }

            // Domain 배정 순서 = 링 안쪽→바깥. 같은 링은 동시, 링 사이만 StaggerPerRing.
            int currentRing = -1;
            float ringStartDelay = 0f;
            var tasks = new List<UniTask>(wave.Relocations.Count);

            for (int i = 0; i < wave.Relocations.Count; i++)
            {
                CellRelocation relocation = wave.Relocations[i];
                int ring = CellRelocationOrder.Chebyshev(relocation.From);
                if (ring != currentRing)
                {
                    if (currentRing >= 0)
                    {
                        ringStartDelay += placementConfig.StaggerPerRing;
                    }

                    currentRing = ring;
                }

                if (!_cellsById.TryGetValue(relocation.CellId, out OccupiedCellView view) || view == null)
                {
                    continue;
                }

                tasks.Add(view.PlayRelocationAsync(
                    relocation,
                    boardConfig,
                    placementConfig,
                    ringStartDelay));
            }

            if (tasks.Count > 0)
            {
                await UniTask.WhenAll(tasks);
            }
        }

        private void AnimateBoardRotation(Action onComplete)
        {
            BoardSession session = _placementBootstrap.Session;
            if (_cellsById.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int remaining = _cellsById.Count;
            void OnCellComplete()
            {
                remaining--;
                if (remaining <= 0)
                {
                    onComplete?.Invoke();
                }
            }

            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.CellFill;
            float duration = placementConfig.RotationDuration;

            foreach (KeyValuePair<int, OccupiedCellView> entry in _cellsById)
            {
                if (!session.TryGetCell(entry.Key, out OccupiedCell cell))
                {
                    Destroy(entry.Value.gameObject);
                    OnCellComplete();
                    continue;
                }

                entry.Value.AnimateMoveTo(
                    cell.Position,
                    cellSize,
                    fill,
                    duration,
                    placementConfig.RotationEase,
                    OnCellComplete);
            }
        }
    }
}
