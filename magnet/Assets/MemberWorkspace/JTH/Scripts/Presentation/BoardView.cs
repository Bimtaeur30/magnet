using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private BoardConfigSO _config;
        [SerializeField] private Transform _cellsRoot;

        private static Sprite _sharedCellSprite;

        private void Start()
        {
            if (_config == null)
            {
                Debug.LogError("[BoardView] BoardConfigSO is not assigned.", this);
                return;
            }

            BuildGrid();
        }

        private void BuildGrid()
        {
            if (_cellsRoot == null)
            {
                var root = new GameObject("Cells");
                root.transform.SetParent(transform, false);
                _cellsRoot = root.transform;
            }

            ClearCells();

            int size = _config.BoardSize;
            float cellSize = _config.CellSize;
            int half = BoardCoordinates.HalfExtent(size);
            Sprite sprite = GetCellSprite();

            for (int gy = -half; gy <= half; gy++)
            {
                for (int gx = -half; gx <= half; gx++)
                {
                    bool isMagnet = BoardCoordinates.IsMagnetCell(gx, gy);
                    Vector2 world = BoardCoordinates.GridToWorld(gx, gy, cellSize);

                    var cell = new GameObject(isMagnet ? "MagnetAxis" : $"Cell_{gx}_{gy}");
                    cell.transform.SetParent(_cellsRoot, false);
                    cell.transform.localPosition = new Vector3(world.x, world.y, 0f);
                    cell.transform.localScale = Vector3.one * cellSize;

                    var renderer = cell.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.color = isMagnet ? _config.MagnetAxisColor : _config.CellColor;
                    renderer.sortingOrder = isMagnet ? 1 : 0;
                }
            }
        }

        private void ClearCells()
        {
            for (int i = _cellsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_cellsRoot.GetChild(i).gameObject);
            }
        }

        private static Sprite GetCellSprite()
        {
            if (_sharedCellSprite != null)
            {
                return _sharedCellSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            _sharedCellSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);

            return _sharedCellSprite;
        }
    }
}
