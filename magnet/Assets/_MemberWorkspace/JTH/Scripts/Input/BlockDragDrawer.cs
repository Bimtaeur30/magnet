using GameLib.EventChannelSystem;
using JTH.Scripts.Presentation;
using Magnet.Contracts.BlockShapes;
using Magnet.Contracts.BlockSkins;
using PMS.Scripts.Events;
using UnityEngine;

namespace JTH.Scripts.Input
{
    /// <summary>
    /// 드래그 중 스테이징·프리뷰 ShapeBlock 표시. BlockDragInput은 입력·좌표만 담당한다.
    /// </summary>
    public sealed class BlockDragDrawer : MonoBehaviour
    {
        [Tooltip("스테이징·프리뷰 표시용 ShapeBlock 프리팹. Awake에서 2개 Instantiate")]
        [SerializeField] private ShapeBlock shapeBlockPrefab;
        [SerializeField] private EventChannelSO skinChannel;

        private IBlockSkin _currentSkin;

        private ShapeBlock _stagingBlock;
        private ShapeBlock _previewBlock;

        private void Awake()
        {
            Debug.Assert(shapeBlockPrefab != null, "[BlockDragDrawer] shapeBlockPrefab is not assigned.", this);
            Debug.Assert(skinChannel != null, "[BlockDragDrawer] skinChannel is not assigned.", this);

            _stagingBlock = Instantiate(shapeBlockPrefab, transform);
            _stagingBlock.name = "StagingBlock";
            _previewBlock = Instantiate(shapeBlockPrefab, transform);
            _previewBlock.name = "PreviewBlock";

            skinChannel.AddListener<SkinInitializedEvent>(OnSkinInitialized);
            skinChannel.AddListener<SkinChangedEvent>(OnSkinChanged);
        }

        private void OnDestroy()
        {
            skinChannel.RemoveListener<SkinInitializedEvent>(OnSkinInitialized);
            skinChannel.RemoveListener<SkinChangedEvent>(OnSkinChanged);
        }

        private void OnSkinInitialized(SkinInitializedEvent evt)
        {
            SetCurrentSkin(evt.Skin);
        }

        private void OnSkinChanged(SkinChangedEvent evt)
        {
            SetCurrentSkin(evt.CurrentSkin);
        }

        private void SetCurrentSkin(IBlockSkin skin)
        {
            _currentSkin = skin;
            ApplyCurrentSkinTo(_stagingBlock);
            ApplyCurrentSkinTo(_previewBlock);
        }

        private void ApplyCurrentSkinTo(ShapeBlock block)
        {
            if (_currentSkin == null || block == null)
            {
                return;
            }

            block.ApplySkin(_currentSkin);
        }

        public void ShowStaging(IBlockShape shape, float worldCenterX, int stagingGridY)
        {
            ApplyCurrentSkinTo(_stagingBlock);
            _stagingBlock.ShowAtWorldCenter(shape, worldCenterX, stagingGridY);
        }

        public void ShowPreview(IBlockShape shape, Vector2Int pivot)
        {
            _stagingBlock.ShareSkinWith(_previewBlock);
            _previewBlock.ShowPreview(shape, pivot, sortingOrder: 3);
        }

        public void ClearPreview()
        {
            _previewBlock.Clear();
        }

        public void ClearAll()
        {
            _stagingBlock.Clear();
            _previewBlock.Clear();
        }

        public ShapeBlock TakeStagingForPlacement()
        {
            ShapeBlock taken = _stagingBlock;
            _stagingBlock = Instantiate(shapeBlockPrefab, transform);
            _stagingBlock.name = "StagingBlock";
            ApplyCurrentSkinTo(_stagingBlock);
            return taken;
        }
    }
}
