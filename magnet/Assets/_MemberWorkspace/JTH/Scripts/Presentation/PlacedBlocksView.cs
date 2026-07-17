using System;
using System.Collections.Generic;
using _Shared.Magnet.Core.Events;
using Cysharp.Threading.Tasks;
using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
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
        [SerializeField] private EventChannelSO presentationChannel;
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;
        [SerializeField] private PoolItemSO blockBlastEffect;

        [Inject] private readonly BoardPlacementBootstrap _placementBootstrap;

        private readonly Dictionary<int, OccupiedCellView> _cellsById = new();
        private IBlockSkin _currentSkin;
        private BoardView _boardView;

        private BoardView BoardView
        {
            get
            {
                if (_boardView == null)
                {
                    _boardView = GetComponentInParent<BoardView>();
                }

                return _boardView;
            }
        }

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[PlacedBlocksView] boardConfig is not assigned.", this);
            Debug.Assert(skinChannel != null, "[PlacedBlocksView] skinChannel is not assigned.", this);
            Debug.Assert(presentationChannel != null, "[PlacedBlocksView] presentationChannel is not assigned.", this);
            Debug.Assert(placementConfig != null, "[PlacedBlocksView] placementConfig is not assigned.", this);
            Debug.Assert(blockBlastEffect != null, "[PlacedBlocksView] blockBlastEffect is not assigned.", this);
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
            _currentSkin = evt.Skin;
            ApplySkinToAll(evt.Skin);
        }

        private void OnSkinChanged(SkinChangedEvent evt)
        {
            _currentSkin = evt.CurrentSkin;
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

            int stagingGridY = placementConfig.Visual.GetStagingY(boardConfig.CellsPerSide);
            var completion = new UniTaskCompletionSource();
            BlockSnapMotion.PlayFromOffsets(
                staging,
                offsets,
                placement.FinalPivot,
                stagingGridY,
                boardConfig,
                placementConfig.Snap,
                () =>
                {
                    SplitStagingIntoCells(staging, cellIds);
                    completion.TrySetResult();
                });
            return completion.Task;
        }

        public void DestroyWaveCellViews(IReadOnlyList<int> cellIds)
        {
            DestroyCellViews(cellIds);
        }

        public UniTask PlayWaveRelocationsAsync(ClearWave wave)
        {
            return PlayWaveRelocationsInternalAsync(wave);
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

                entry.Value.SnapToGrid(cell.Position, boardConfig.CellSize, placementConfig.Visual.CellFill);
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
            float fill = placementConfig.Visual.CellFill;

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
            Texture skinTexture = _currentSkin?.Sprite != null ? _currentSkin.Sprite.texture : null;

            for (int i = 0; i < cellIds.Count; i++)
            {
                int cellId = cellIds[i];
                if (!_cellsById.Remove(cellId, out OccupiedCellView view))
                {
                    continue;
                }

                if (view == null)
                {
                    continue;
                }

                presentationChannel.RaiseEvent(
                    PresentationEvents.PlayParticleEffectEvent.Init(
                        blockBlastEffect,
                        view.transform.position,
                        GetOutwardWorldRotation(view.GridPosition),
                        skinTexture));

                Destroy(view.gameObject);
            }
        }

        /// <summary>
        /// 정사각 테두리 기준: 변은 직교, 모서리는 대각. 보드 Transform 회전을 월드에 반영한다.
        /// </summary>
        private Quaternion GetOutwardWorldRotation(Vector2Int grid)
        {
            Vector2Int dir = GetOutwardGridDirection(grid);
            Quaternion localRotation = Quaternion.FromToRotation(
                Vector3.up,
                new Vector3(dir.x, dir.y, 0f));

            BoardView boardView = BoardView;
            if (boardView == null)
            {
                return localRotation;
            }

            return boardView.transform.rotation * localRotation;
        }

        private static Vector2Int GetOutwardGridDirection(Vector2Int grid)
        {
            int absX = Mathf.Abs(grid.x);
            int absY = Mathf.Abs(grid.y);
            if (absX == 0 && absY == 0)
            {
                return Vector2Int.up;
            }

            if (absX == absY)
            {
                return new Vector2Int(Sign(grid.x), Sign(grid.y));
            }

            if (absX > absY)
            {
                return new Vector2Int(Sign(grid.x), 0);
            }

            return new Vector2Int(0, Sign(grid.y));
        }

        private static int Sign(int value)
        {
            if (value > 0)
            {
                return 1;
            }

            if (value < 0)
            {
                return -1;
            }

            return 0;
        }

        private async UniTask PlayWaveRelocationsInternalAsync(ClearWave wave)
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
                        ringStartDelay += placementConfig.ClearReassemblyMotion.StaggerPerRing;
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
                    placementConfig.Visual,
                    placementConfig.ClearReassemblyMotion,
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
            float fill = placementConfig.Visual.CellFill;
            float duration = placementConfig.Rotation.Duration;

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
                    placementConfig.Rotation.Ease,
                    OnCellComplete);
            }
        }
    }
}
