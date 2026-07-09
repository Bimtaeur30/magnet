using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 선택·스테이징 블록 칸 표시. 칸마다 SpriteRenderer.
    /// </summary>
    public sealed class BlockPieceView : MonoBehaviour
    {
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;
        [Tooltip("블록 피스 칸 SpriteRenderer의 부모 Transform. 비우면 자동 생성")]
        [SerializeField] private Transform cellsRoot;
        [Tooltip("블록 칸 1개 프리팹(SpriteRenderer 포함). 필요 개수만큼 인스턴스 생성 후 재사용")]
        [SerializeField] private GameObject cellPrefab;

        private static Sprite _whiteSprite;
        private readonly List<SpriteRenderer> _cellRenderers = new();

        public void Show(IBlockShape shape, Vector2Int pivot)
        {
            Debug.Assert(boardConfig != null, "[BlockPieceView] BoardConfigSO is not assigned.", this);
            Debug.Assert(placementConfig != null, "[BlockPieceView] PlacementConfigSO is not assigned.", this);
            Debug.Assert(shape != null, "[BlockPieceView] shape is null.", this);
            Debug.Assert(cellPrefab != null, "[BlockPieceView] cellPrefab is not assigned.", this);

            EnsureCellsRoot();
            EnsureCellCount(shape.CellOffsets.Count);

            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.CellFill;
            Color color = placementConfig.PieceColor;

            for (int i = 0; i < shape.CellOffsets.Count; i++)
            {
                Vector2Int cell = pivot + shape.CellOffsets[i];
                Vector2 world = BoardCoordinates.GridToWorld(cell.x, cell.y, cellSize);
                SpriteRenderer cellRenderer = _cellRenderers[i];
                cellRenderer.gameObject.SetActive(true);
                cellRenderer.transform.localPosition = new Vector3(world.x, world.y, 0f);
                cellRenderer.transform.localScale = new Vector3(cellSize * fill, cellSize * fill, 1f);
                cellRenderer.color = color;
                cellRenderer.sortingOrder = 2;
            }

            for (int i = shape.CellOffsets.Count; i < _cellRenderers.Count; i++)
            {
                _cellRenderers[i].gameObject.SetActive(false);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _cellRenderers.Count; i++)
            {
                _cellRenderers[i].gameObject.SetActive(false);
            }
        }

        private void EnsureCellsRoot()
        {
            if (cellsRoot == null)
            {
                var root = new GameObject("Cells");
                root.transform.SetParent(transform, false);
                cellsRoot = root.transform;
            }
        }

        private void EnsureCellCount(int count)
        {
            while (_cellRenderers.Count < count)
            {
                GameObject instance = Instantiate(cellPrefab, cellsRoot);
                instance.name = $"Cell_{_cellRenderers.Count}";

                SpriteRenderer cellRenderer = instance.GetComponent<SpriteRenderer>();
                Debug.Assert(cellRenderer != null, "[BlockPieceView] cellPrefab must have SpriteRenderer.", instance);
                _cellRenderers.Add(cellRenderer);
            }
        }
    }
}
