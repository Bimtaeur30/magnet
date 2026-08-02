using System.Collections.Generic;
using JTH.Scripts.Presentation;
using Magnet.Contracts;
using UnityEngine;

namespace JTH.Scripts.Input
{
    public sealed class BlockDragDrawer : MonoBehaviour
    {
        [Tooltip("스테이징·프리뷰 표시용 ShapeBlock 프리팹. Awake에서 2개 Instantiate")]
        [SerializeField] private ShapeBlock shapeBlockPrefab;

        private ShapeBlock _previewBlock;
        private ShapeBlock _stagingBlock;

        private void Awake()
        {
            Debug.Assert(shapeBlockPrefab != null, "[BlockDragDrawer] shapeBlockPrefab is not assigned.", this);

            _stagingBlock = Instantiate(shapeBlockPrefab, transform);
            _stagingBlock.name = "StagingBlock";
            _previewBlock = Instantiate(shapeBlockPrefab, transform);
            _previewBlock.name = "PreviewBlock";
        }

        public void ShowStaging(ShapeBlockData data) => _stagingBlock.Show(data);
        public void ShowPreview(ShapeBlockData data) => _previewBlock.ShowPreview(data);
        
        public void MoveStaging(Vector2 position)
        {
            _stagingBlock.transform.position = position;
        }

        public void MovePreview(Vector2 position)
        {
            _previewBlock.transform.position = position;
        }
        
        public void ClearPreview() => _previewBlock.Clear();

        public void ClearAll()
        {
            _stagingBlock.Clear();
            _previewBlock.Clear();
        }

        public IReadOnlyList<Block> GetStagingBlocks()
        {
            return _stagingBlock.DetachBlocks();
        }
    }
}