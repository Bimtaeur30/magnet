using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JTH.Scripts.Bootstrap;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Placement;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 부착 완료된 ShapeBlock을 blockId로 추적하고, Bootstrap이 요청하는 Place·Clear·Rotate 연출을 재생한다.
    /// </summary>
    public sealed class PlacedBlocksView : MonoBehaviour
    {
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;

        [Inject] private readonly BoardPlacementBootstrap _placementBootstrap;

        private readonly Dictionary<int, ShapeBlock> _blocksById = new();

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[PlacedBlocksView] boardConfig is not assigned.", this);
            Debug.Assert(placementConfig != null, "[PlacedBlocksView] placementConfig is not assigned.", this);
            Debug.Assert(_placementBootstrap != null, "[PlacedBlocksView] BoardPlacementBootstrap was not injected.", this);
        }

        public void Register(int blockId, ShapeBlock block)
        {
            if (block == null)
            {
                return;
            }

            _blocksById[blockId] = block;
        }

        public void Adopt(ShapeBlock block, string displayName)
        {
            if (block == null)
            {
                return;
            }

            block.transform.SetParent(transform, worldPositionStays: true);
            block.name = displayName;
        }

        public void SyncWithSession()
        {
            var fullyRemovedIds = new List<int>();

            foreach (KeyValuePair<int, ShapeBlock> entry in _blocksById)
            {
                int blockId = entry.Key;
                ShapeBlock view = entry.Value;

                if (!_placementBootstrap.Session.TryGetPlacedBlock(blockId, out PlacedBlock placedBlock))
                {
                    view.Clear();
                    Destroy(view.gameObject);
                    fullyRemovedIds.Add(blockId);
                    continue;
                }

                view.ShowPlaced(placedBlock, sortingOrder: 0);
            }

            for (int i = 0; i < fullyRemovedIds.Count; i++)
            {
                _blocksById.Remove(fullyRemovedIds[i]);
            }
        }

        /// <summary>
        /// 회전 전 부착 좌표로 Y 스냅 후 Register/Adopt. LitMotion 자리는 <see cref="BlockSnapMotion"/>에 있다.
        /// </summary>
        public UniTask PlayPlaceAsync(ShapeBlock staging, int blockId)
        {
            if (staging == null)
            {
                return UniTask.CompletedTask;
            }

            if (!_placementBootstrap.Session.TryGetPlacedBlock(blockId, out PlacedBlock placedBlock))
            {
                staging.Clear();
                Destroy(staging.gameObject);
                return UniTask.CompletedTask;
            }

            int stagingGridY = placementConfig.GetStagingY(boardConfig.CellsPerSide);
            var completion = new UniTaskCompletionSource();
            BlockSnapMotion.PlayFromPlaced(
                staging,
                placedBlock,
                stagingGridY,
                boardConfig,
                placementConfig,
                () =>
                {
                    Register(blockId, staging);
                    Adopt(staging, $"Placed_{blockId}");
                    completion.TrySetResult();
                });
            return completion.Task;
        }

        /// <summary>
        /// Domain 클리어 반영 후 뷰 동기화. 사라짐 LitMotion이 생기면 이 메서드에서 await하면 된다.
        /// </summary>
        public UniTask PlayClearAsync()
        {
            SyncWithSession();
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 보드 90° 회전 연출. Domain 회전 이후 Session 좌표 기준으로 재생한다.
        /// </summary>
        public UniTask PlayRotateAsync()
        {
            var completion = new UniTaskCompletionSource();
            AnimateBoardRotation(() => completion.TrySetResult());
            return completion.Task;
        }

        public void AnimateBoardRotation(Action onComplete)
        {
            if (_blocksById.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int remaining = _blocksById.Count;
            void OnBlockComplete()
            {
                remaining--;
                if (remaining <= 0)
                {
                    onComplete?.Invoke();
                }
            }

            foreach (KeyValuePair<int, ShapeBlock> entry in _blocksById)
            {
                int blockId = entry.Key;
                ShapeBlock view = entry.Value;

                if (!_placementBootstrap.Session.TryGetPlacedBlock(blockId, out PlacedBlock placedBlock))
                {
                    view.Clear();
                    Destroy(view.gameObject);
                    OnBlockComplete();
                    continue;
                }

                view.AnimateRotateClockwise90(placedBlock, boardConfig, placementConfig.RotationDuration, OnBlockComplete);
            }
        }
    }
}
