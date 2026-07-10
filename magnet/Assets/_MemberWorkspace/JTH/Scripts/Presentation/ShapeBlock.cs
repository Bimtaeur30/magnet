using System;
using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Placement;
using LitMotion;
using Magnet.Contracts.BlockShapes;
using Magnet.Contracts.BlockSkins;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 형태(IBlockShape) 단위 블록 표시. Block 프리팹을 칸 수만큼 Instantiate·재사용한다.
    /// </summary>
    public sealed class ShapeBlock : MonoBehaviour
    {
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;
        [Tooltip("블록 칸 1개 프리팹(Block 컴포넌트 + SpriteRenderer). 필요 개수만큼 인스턴스 생성 후 재사용")]
        [SerializeField] private Block blockPrefab;
        [SerializeField] private EventChannelSO systemChannel;

        [Header("Temp Skin (inline until system channel events)")]
        [SerializeField] private Color[] skinColors;
        [SerializeField] private Sprite[] skinSprites;

        private readonly List<Block> _blocks = new();
        private readonly List<MotionHandle> _activeMotions = new();
        private bool _skinResolved;
        private Color _resolvedColor;
        private Sprite _resolvedSprite;

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[ShapeBlock] boardConfig is not assigned.", this);
            Debug.Assert(placementConfig != null, "[ShapeBlock] placementConfig is not assigned.", this);
            Debug.Assert(blockPrefab != null, "[ShapeBlock] blockPrefab is not assigned.", this);
            Debug.Assert(systemChannel != null, "[ShapeBlock] systemChannel is not assigned.", this);
        }

        private void OnDestroy()
        {
            CancelMotions();
        }

        public void Show(IBlockShape shape, Vector2Int pivot, int sortingOrder)
        {
            EnsureBlockCount(shape.CellOffsets.Count);

            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.CellFill;

            for (int i = 0; i < shape.CellOffsets.Count; i++)
            {
                Vector2Int cell = pivot + shape.CellOffsets[i];
                Vector2 world = BoardCoordinates.GridToWorld(cell.x, cell.y, cellSize);
                ApplyBlockVisual(i, world, cellSize, fill, sortingOrder);
            }

            HideExtraBlocks(shape.CellOffsets.Count);
            ApplyResolvedSkin();
        }

        public void ShowAtWorldCenter(IBlockShape shape, float worldCenterX, int stagingGridY)
        {
            EnsureBlockCount(shape.CellOffsets.Count);

            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.CellFill;
            Vector2 centerOffset = BlockPlacementCells.GetShapeCenterOffset(shape.CellOffsets);
            float stagingWorldY = stagingGridY * cellSize;

            for (int i = 0; i < shape.CellOffsets.Count; i++)
            {
                Vector2Int offset = shape.CellOffsets[i];
                float cellWorldX = worldCenterX + (offset.x - centerOffset.x) * cellSize;
                float cellWorldY = stagingWorldY + (offset.y - centerOffset.y) * cellSize;
                ApplyBlockVisual(i, new Vector2(cellWorldX, cellWorldY), cellSize, fill, sortingOrder: 2);
            }

            HideExtraBlocks(shape.CellOffsets.Count);
            ApplyResolvedSkin();
        }

        /// <summary>프리뷰 X(최종 pivot 열)로 순간이동, Y는 스테이징 높이 유지.</summary>
        public void ShowAtSnapStart(IBlockShape shape, Vector2Int finalPivot, int stagingGridY, int sortingOrder = 2)
        {
            EnsureBlockCount(shape.CellOffsets.Count);

            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.CellFill;

            for (int i = 0; i < shape.CellOffsets.Count; i++)
            {
                Vector2Int offset = shape.CellOffsets[i];
                float worldX = BoardCoordinates.GridToWorld(finalPivot.x + offset.x, 0, cellSize).x;
                float worldY = BoardCoordinates.GridToWorld(0, stagingGridY + offset.y, cellSize).y;
                ApplyBlockVisual(i, new Vector2(worldX, worldY), cellSize, fill, sortingOrder);
            }

            HideExtraBlocks(shape.CellOffsets.Count);
            ApplyResolvedSkin();
        }

        public void AnimateSnapY(
            IBlockShape shape,
            Vector2Int finalPivot,
            BoardConfigSO configSO,
            float duration,
            Action onComplete)
        {
            CancelMotions();

            float cellSize = configSO.CellSize;
            int remaining = shape.CellOffsets.Count;

            if (remaining == 0)
            {
                onComplete?.Invoke();
                return;
            }

            void OnCellComplete()
            {
                remaining--;
                if (remaining <= 0)
                {
                    onComplete?.Invoke();
                }
            }

            for (int i = 0; i < shape.CellOffsets.Count; i++)
            {
                Vector2Int cell = finalPivot + shape.CellOffsets[i];
                float targetY = BoardCoordinates.GridToWorld(cell.x, cell.y, cellSize).y;
                Block block = _blocks[i];
                float startY = block.transform.localPosition.y;

                MotionHandle handle = LMotion.Create(startY, targetY, duration)
                    .WithEase(Ease.OutQuad)
                    .WithOnComplete(OnCellComplete)
                    .Bind(y =>
                    {
                        Vector3 position = block.transform.localPosition;
                        position.y = y;
                        block.SetLocalPosition(position);
                    });
                _activeMotions.Add(handle);
            }
        }

        public void CancelMotions()
        {
            for (int i = 0; i < _activeMotions.Count; i++)
            {
                if (_activeMotions[i].IsActive())
                {
                    _activeMotions[i].Cancel();
                }
            }

            _activeMotions.Clear();
        }

        public void Clear()
        {
            CancelMotions();

            for (int i = 0; i < _blocks.Count; i++)
            {
                _blocks[i].SetActive(false);
            }

            _skinResolved = false;
        }

        public void ApplySkin(IBlockSkin skin)
        {
            if (skin == null || skin.Sprites.Count == 0)
            {
                return;
            }

            _resolvedColor = skin.Colors.Count > 0 ? skin.Colors[UnityEngine.Random.Range(0, skin.Colors.Count)] : Color.white;
            _resolvedSprite = skin.Sprites[UnityEngine.Random.Range(0, skin.Sprites.Count)];
            _skinResolved = true;
            ApplyVisualToActiveBlocks(_resolvedColor, _resolvedSprite);
        }

        private void ApplyResolvedSkin()
        {
            ResolveSkinFromSerializedIfNeeded();
            if (!_skinResolved)
            {
                return;
            }

            ApplyVisualToActiveBlocks(_resolvedColor, _resolvedSprite);
        }

        private void ResolveSkinFromSerializedIfNeeded()
        {
            if (_skinResolved)
            {
                return;
            }

            if (skinColors == null || skinColors.Length == 0 || skinSprites == null || skinSprites.Length == 0)
            {
                return;
            }

            _resolvedColor = skinColors.Length > 0 ? skinColors[UnityEngine.Random.Range(0, skinColors.Length)] : Color.white;
            _resolvedSprite = skinSprites[UnityEngine.Random.Range(0, skinSprites.Length)];
            _skinResolved = true;
        }

        internal void ShareSkinWith(ShapeBlock target)
        {
            ResolveSkinFromSerializedIfNeeded();
            if (!_skinResolved || target == null)
            {
                return;
            }

            target._resolvedColor = _resolvedColor;
            target._resolvedSprite = _resolvedSprite;
            target._skinResolved = true;
        }

        private void ApplyVisualToActiveBlocks(Color color, Sprite sprite)
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                if (_blocks[i].gameObject.activeSelf)
                {
                    _blocks[i].ApplyVisual(color, sprite);
                }
            }
        }

        private void EnsureBlockCount(int count)
        {
            while (_blocks.Count < count)
            {
                Block instance = Instantiate(blockPrefab, transform);
                instance.name = $"Block_{_blocks.Count}";
                _blocks.Add(instance);
            }
        }

        private void ApplyBlockVisual(int index, Vector2 world, float cellSize, float fill, int sortingOrder)
        {
            Block block = _blocks[index];
            block.SetActive(true);
            block.SetLocalPosition(new Vector3(world.x, world.y, 0f));
            block.SetLocalScale(new Vector3(cellSize * fill, cellSize * fill, 1f));
            block.SetSortingOrder(sortingOrder);
        }

        private void HideExtraBlocks(int activeCount)
        {
            for (int i = activeCount; i < _blocks.Count; i++)
            {
                _blocks[i].SetActive(false);
            }
        }
    }
}
