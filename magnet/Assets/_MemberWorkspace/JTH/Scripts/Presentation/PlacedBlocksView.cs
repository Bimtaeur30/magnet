using System;
using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Bootstrap;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Events;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 부착 완료된 ShapeBlock을 blockId로 추적하고, 클리어·회전 이벤트에 맞춰 갱신한다.
    /// </summary>
    public sealed class PlacedBlocksView : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;

        [Inject] private readonly BoardPlacementBootstrap _placementBootstrap;

        private readonly Dictionary<int, ShapeBlock> _blocksById = new();

        private void Awake()
        {
            Debug.Assert(magnetGameChannel != null, "[PlacedBlocksView] magnetGameChannel is not assigned.", this);
            Debug.Assert(boardConfig != null, "[PlacedBlocksView] boardConfig is not assigned.", this);
            Debug.Assert(placementConfig != null, "[PlacedBlocksView] placementConfig is not assigned.", this);
            Debug.Assert(_placementBootstrap != null, "[PlacedBlocksView] BoardPlacementBootstrap was not injected.", this);
        }

        private void OnEnable()
        {
            magnetGameChannel.AddListener<SquareClearedEvent>(OnSquareCleared);
            magnetGameChannel.AddListener<BoardRotatedEvent>(OnBoardRotated);
        }

        private void OnDisable()
        {
            magnetGameChannel?.RemoveListener<SquareClearedEvent>(OnSquareCleared);
            magnetGameChannel?.RemoveListener<BoardRotatedEvent>(OnBoardRotated);
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

        private void OnSquareCleared(SquareClearedEvent _)
        {
            SyncWithSession();
        }

        private void OnBoardRotated(BoardRotatedEvent _)
        {
            AnimateBoardRotation(onComplete: null);
        }
    }
}
