using System.Collections.Generic;
using Game.UI;
using GameLib.EventChannelSystem;
using Magnet.Core.Events;
using Magnet.Core.SO.Skin;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockSlot_UI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private EventChannelSO MagnetChannel;
    [SerializeField] private EventChannelSO SkinEventChannel;
    [SerializeField] private BlockSlotView SlotView;
    [SerializeField, Min(1f)] private float cellSize = 30f;
    [SerializeField, Min(0f)] private float cellSpacing = 2f;

    private readonly List<Image> _blockCells = new();

    private RectTransform _blockContainer;
    private SkinDataSO _currentSkin;
    private int _index;
    private int _skinId;
    private bool _isOccupied;

    private void Awake()
    {
        _blockContainer = SlotView.transform as RectTransform;

        SkinEventChannel.AddListener<SkinInitializedEvent>(HandleSkinInitialized);
        SkinEventChannel.AddListener<SkinChangedEvent>(HandleSkinChanged);

        // 기존 단일 이미지 바인딩은 동적 셀 UI와 겹치지 않도록 숨긴다.
        SetLegacyBlockImageAlpha(0f);
    }

    private void OnDestroy()
    {
        SkinEventChannel.RemoveListener<SkinInitializedEvent>(HandleSkinInitialized);
        SkinEventChannel.RemoveListener<SkinChangedEvent>(HandleSkinChanged);
    }

    public void SetSlot(IReadOnlyList<Vector2Int> cellOffsets, int skinId, int index)
    {
        _index = index;
        _skinId = skinId;
        _isOccupied = cellOffsets is { Count: > 0 };

        ClearBlockCells();

        if (!_isOccupied)
            return;

        CalculateBounds(
            cellOffsets,
            out int minX,
            out int maxX,
            out int minY,
            out int maxY);

        float centerX = (minX + maxX) * 0.5f;
        float centerY = (minY + maxY) * 0.5f;
        float visualCellSize = Mathf.Max(1f, cellSize - cellSpacing);

        foreach (Vector2Int offset in cellOffsets)
        {
            var cellObject = new GameObject(
                $"BlockCell_{offset.x}_{offset.y}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            var cellRect = (RectTransform)cellObject.transform;
            cellRect.SetParent(_blockContainer, false);
            cellRect.anchorMin = new Vector2(0.5f, 0.5f);
            cellRect.anchorMax = new Vector2(0.5f, 0.5f);
            cellRect.pivot = new Vector2(0.5f, 0.5f);
            cellRect.sizeDelta = Vector2.one * visualCellSize;
            cellRect.anchoredPosition = new Vector2(
                (offset.x - centerX) * cellSize,
                (offset.y - centerY) * cellSize);

            Image cellImage = cellObject.GetComponent<Image>();
            cellImage.raycastTarget = false;
            _blockCells.Add(cellImage);
        }

        ApplyCurrentSkin();
    }

    public void EmptySlot()
    {
        _isOccupied = false;
        ClearBlockCells();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isOccupied)
            return;

        MagnetChannel.RaiseEvent(
            MagnetGameEvents.BlockSelectedOnUIEvent.Init(_index));
    }

    private void HandleSkinInitialized(SkinInitializedEvent evt)
    {
        _currentSkin = evt.Skin;
        ApplyCurrentSkin();
    }

    private void HandleSkinChanged(SkinChangedEvent evt)
    {
        _currentSkin = evt.CurrentSkin;
        ApplyCurrentSkin();
    }

    private void ApplyCurrentSkin()
    {
        Sprite sprite = null;

        if (_currentSkin != null &&
            _currentSkin.Sprites != null &&
            _skinId >= 0 &&
            _skinId < _currentSkin.Sprites.Length)
        {
            sprite = _currentSkin.Sprites[_skinId];
        }

        foreach (Image blockCell in _blockCells)
        {
            blockCell.sprite = sprite;
            blockCell.color = Color.white;
            blockCell.preserveAspect = true;
        }
    }

    private void ClearBlockCells()
    {
        foreach (Image blockCell in _blockCells)
        {
            if (blockCell != null)
                Destroy(blockCell.gameObject);
        }

        _blockCells.Clear();
    }

    private void SetLegacyBlockImageAlpha(float alpha)
    {
        if (SlotView?.ViewModel != null)
            SlotView.ViewModel.BlockImage1Alpha = alpha;
    }

    private static void CalculateBounds(
        IReadOnlyList<Vector2Int> cellOffsets,
        out int minX,
        out int maxX,
        out int minY,
        out int maxY)
    {
        Vector2Int first = cellOffsets[0];
        minX = maxX = first.x;
        minY = maxY = first.y;

        for (int i = 1; i < cellOffsets.Count; i++)
        {
            Vector2Int offset = cellOffsets[i];
            minX = Mathf.Min(minX, offset.x);
            maxX = Mathf.Max(maxX, offset.x);
            minY = Mathf.Min(minY, offset.y);
            maxY = Mathf.Max(maxY, offset.y);
        }
    }
}
