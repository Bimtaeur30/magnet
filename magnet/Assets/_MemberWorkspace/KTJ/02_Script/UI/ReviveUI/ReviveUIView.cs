using System.Collections.Generic;
using GameLib.EventChannelSystem;
using Magnet.Core.Events;
using Magnet.Core.SO.Skin;
using Mvvm;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed partial class ReviveUIView : MvvmView<ReviveUIViewModel>
    {
        [SerializeField, Tooltip("부활을 수락하는 버튼")]
        private Button sureButton;
        [SerializeField, Tooltip("부활을 거절하고 게임 오버로 진행하는 버튼")]
        private Button noThanksButton;
        [SerializeField, Tooltip("부활 제안 UI 전체를 감싸는 오브젝트")]
        private GameObject container;
        [SerializeField, Tooltip("점수 및 부활 관련 이벤트 채널")]
        private EventChannelSO magnetGameChannel;
        [SerializeField, Tooltip("부활 블록 이미지에 현재 스킨을 적용하는 이벤트 채널")]
        private EventChannelSO skinEventChannel;
        [SerializeField, Min(1f), Tooltip("부활 블록 이미지 한 칸의 배치 크기")]
        private float cellSize = 30f;
        [SerializeField, Min(0f), Tooltip("부활 블록 이미지 각 칸 사이의 간격")]
        private float cellSpacing = 2f;

        private readonly List<GameObject> blockPieces = new();
        private readonly List<Image> blockCells = new();
        private RectTransform blockSlots;
        private SkinDataSO currentSkin;
        private int currentScore;

        protected override void Awake()
        {
            base.Awake();
            Debug.Assert(sureButton != null, "[ReviveUIView] sureButton is not assigned.", this);
            Debug.Assert(noThanksButton != null, "[ReviveUIView] noThanksButton is not assigned.", this);
            Debug.Assert(container != null, "[ReviveUIView] container is not assigned.", this);
            Debug.Assert(magnetGameChannel != null, "[ReviveUIView] magnetGameChannel is not assigned.", this);
            Debug.Assert(skinEventChannel != null, "[ReviveUIView] skinEventChannel is not assigned.", this);

            blockSlots = container.transform.Find("BlockSlots") as RectTransform;
            Debug.Assert(blockSlots != null, "[ReviveUIView] Container/BlockSlots was not found.", this);
            container.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            sureButton.onClick.AddListener(HandleSureButtonClicked);
            noThanksButton.onClick.AddListener(HandleNoThanksButtonClicked);
            magnetGameChannel.AddListener<ScoreChangedEvent>(HandleScoreChanged);
            magnetGameChannel.AddListener<RelifeOfferedEvent>(HandleRelifeOffered);
            skinEventChannel.AddListener<SkinInitializedEvent>(HandleSkinInitialized);
            skinEventChannel.AddListener<SkinChangedEvent>(HandleSkinChanged);
        }

        protected override void OnDisable()
        {
            sureButton.onClick.RemoveListener(HandleSureButtonClicked);
            noThanksButton.onClick.RemoveListener(HandleNoThanksButtonClicked);
            magnetGameChannel.RemoveListener<ScoreChangedEvent>(HandleScoreChanged);
            magnetGameChannel.RemoveListener<RelifeOfferedEvent>(HandleRelifeOffered);
            skinEventChannel.RemoveListener<SkinInitializedEvent>(HandleSkinInitialized);
            skinEventChannel.RemoveListener<SkinChangedEvent>(HandleSkinChanged);
            base.OnDisable();
        }

        private void HandleScoreChanged(ScoreChangedEvent evt) => currentScore = evt.TotalScore;

        private void HandleRelifeOffered(RelifeOfferedEvent evt)
        {
            container.SetActive(true);
            CreateBlockPieces(evt.CellOffsetsList);
        }

        private void HandleSureButtonClicked()
        {
            CloseOffer();
            magnetGameChannel.RaiseEvent(MagnetGameEvents.RelifeAcceptedEvent.Init());
        }

        private void HandleNoThanksButtonClicked()
        {
            CloseOffer();
            magnetGameChannel.RaiseEvent(MagnetGameEvents.GameOverEvent.Init(currentScore));
        }

        private void CloseOffer()
        {
            container.SetActive(false);
            ClearBlockPieces();
        }

        private void CreateBlockPieces(IReadOnlyList<IReadOnlyList<Vector2Int>> cellOffsetsList)
        {
            ClearBlockPieces();
            if (cellOffsetsList == null)
                return;

            for (int i = 0; i < cellOffsetsList.Count; i++)
            {
                IReadOnlyList<Vector2Int> offsets = cellOffsetsList[i];
                if (offsets != null && offsets.Count > 0)
                    CreateBlockPiece(offsets, i);
            }

            ApplyCurrentSkin();
        }

        private void CreateBlockPiece(IReadOnlyList<Vector2Int> offsets, int index)
        {
            CalculateBounds(offsets, out int minX, out int maxX, out int minY, out int maxY);
            float centerX = (minX + maxX) * 0.5f;
            float centerY = (minY + maxY) * 0.5f;
            float visualCellSize = Mathf.Max(1f, cellSize - cellSpacing);

            var pieceObject = new GameObject($"RelifeBlock_{index}", typeof(RectTransform));
            var pieceRect = (RectTransform)pieceObject.transform;
            pieceRect.SetParent(blockSlots, false);
            pieceRect.sizeDelta = new Vector2(
                (maxX - minX + 1) * cellSize,
                (maxY - minY + 1) * cellSize);
            blockPieces.Add(pieceObject);

            foreach (Vector2Int offset in offsets)
            {
                var cellObject = new GameObject(
                    $"BlockCell_{offset.x}_{offset.y}",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                var cellRect = (RectTransform)cellObject.transform;
                cellRect.SetParent(pieceRect, false);
                cellRect.anchorMin = cellRect.anchorMax = cellRect.pivot = new Vector2(0.5f, 0.5f);
                cellRect.sizeDelta = Vector2.one * visualCellSize;
                cellRect.anchoredPosition = new Vector2(
                    (offset.x - centerX) * cellSize,
                    (offset.y - centerY) * cellSize);

                Image cellImage = cellObject.GetComponent<Image>();
                cellImage.raycastTarget = false;
                blockCells.Add(cellImage);
            }
        }

        private void HandleSkinInitialized(SkinInitializedEvent evt)
        {
            currentSkin = evt.Skin;
            ApplyCurrentSkin();
        }

        private void HandleSkinChanged(SkinChangedEvent evt)
        {
            currentSkin = evt.CurrentSkin;
            ApplyCurrentSkin();
        }

        private void ApplyCurrentSkin()
        {
            Sprite sprite = currentSkin != null && currentSkin.Sprites is { Length: > 0 }
                ? currentSkin.Sprites[0]
                : null;
            foreach (Image blockCell in blockCells)
            {
                blockCell.sprite = sprite;
                blockCell.color = Color.white;
                blockCell.preserveAspect = true;
            }
        }

        private void ClearBlockPieces()
        {
            foreach (GameObject blockPiece in blockPieces)
            {
                if (blockPiece != null)
                    Destroy(blockPiece);
            }
            blockPieces.Clear();
            blockCells.Clear();
        }

        private static void CalculateBounds(
            IReadOnlyList<Vector2Int> offsets,
            out int minX, out int maxX, out int minY, out int maxY)
        {
            Vector2Int first = offsets[0];
            minX = maxX = first.x;
            minY = maxY = first.y;
            for (int i = 1; i < offsets.Count; i++)
            {
                Vector2Int offset = offsets[i];
                minX = Mathf.Min(minX, offset.x);
                maxX = Mathf.Max(maxX, offset.x);
                minY = Mathf.Min(minY, offset.y);
                maxY = Mathf.Max(maxY, offset.y);
            }
        }
    }
}
