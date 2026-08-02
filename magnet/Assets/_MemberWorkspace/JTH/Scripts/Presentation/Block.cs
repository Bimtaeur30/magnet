using GameLib.ObjectPool.Runtime;
using JTH.Scripts.Data;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class Block : AbstractMonoPoolable
    {
        [SerializeField] private SpriteRenderer skinRenderer;
        [Tooltip("칸 스킨 클리핑용. SetSortingOrder에서 Custom Range로 인접 마스크와 격리")]
        [SerializeField] private SpriteMask spriteMask;
        [SerializeField] private BoardConfigSO boardConfigSO;

        private Vector2Int _offset;

        public Vector2Int Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                transform.localPosition = new Vector3(value.x, value.y, 1) * boardConfigSO.CellSize;
            }
        }

        public Texture Skin => skinRenderer.sprite.texture;

        public const int SortingBandPreview = 1000;
        public const int SortingBandPlaced = 10000;
        public const int SortingBandStaging = 20000;

        private static int _nextMaskSlot;
        private bool _dimmed;
        private float _dimMultiply = 1f;
        private float _alpha = 1f;

        private bool _clearHint;
        private float _clearHintTime;
        private float _hintBrightMin = 1f;
        private float _hintBrightMax = 1.35f;
        private float _hintAlphaMin = 1f;
        private float _hintAlphaMax = 1f;
        private float _hintPeriod = 1.2f;

        private void Awake()
        {
            float cellSize = boardConfigSO.CellSize;
            float fillAmount = boardConfigSO.CellFill;
            spriteMask.transform.localScale = new Vector3(cellSize * fillAmount, cellSize * fillAmount, 1);

            Vector3 visualOffset = Vector2.one * cellSize / 2;
            spriteMask.transform.localPosition = visualOffset;
        }

        private void Update()
        {
            if (!_clearHint)
            {
                return;
            }

            _clearHintTime += Time.deltaTime;
            RefreshColor();
        }

        public override void ResetItem()
        {
            base.ResetItem();

            Debug.Assert(skinRenderer != null, "[Block] spriteRenderer is not assigned.", this);
            Debug.Assert(spriteMask != null, "[Block] spriteMask is not assigned.", this);

            SetClearHint(false);
            ApplySortingBand(SortingBandPlaced);
            SetDimmed(false, 0);
            SetAlpha(Color.white.a);
        }

        public void ApplySkin(Sprite skinSprite)
        {
            if (skinSprite != null)
            {
                skinRenderer.sprite = skinSprite;
            }

            RefreshColor();
        }

        /// <summary>
        /// 비활성 링 UX용. RGB만 <paramref name="multiply"/> 배로 어둡게 하고, false면 기본색 복원.
        /// </summary>
        public void SetDimmed(bool isDimmed, float multiply)
        {
            _dimmed = isDimmed;
            _dimMultiply = multiply;
            RefreshColor();
        }

        /// <summary>
        /// 스프라이트 알파 배율. 프리뷰 고스트 등에서 사용.
        /// </summary>
        public void SetAlpha(float value)
        {
            _alpha = Mathf.Clamp01(value);
            RefreshColor();
        }

        /// <summary>
        /// 라인클리어 프리뷰 힌트. 밝기·알파를 숨쉬게 한다 (스프라이트 틴트, HDR Emission 아님).
        /// </summary>
        public void SetClearHint(
            bool enabled,
            float brightnessMin = 1f,
            float brightnessMax = 1.35f,
            float alphaMin = 1f,
            float alphaMax = 1f,
            float period = 1.2f)
        {
            _clearHint = enabled;
            if (!enabled)
            {
                _clearHintTime = 0f;
                RefreshColor();
                return;
            }

            _hintBrightMin = brightnessMin;
            _hintBrightMax = brightnessMax;
            _hintAlphaMin = Mathf.Clamp01(alphaMin);
            _hintAlphaMax = Mathf.Clamp01(alphaMax);
            _hintPeriod = Mathf.Max(0.05f, period);
            RefreshColor();
        }

        private void RefreshColor()
        {
            if (skinRenderer == null)
            {
                return;
            }

            float bright = 1f;
            float alpha = _alpha;
            if (_clearHint)
            {
                float t = (_clearHintTime % _hintPeriod) / _hintPeriod;
                float wave = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f);
                bright = Mathf.Lerp(_hintBrightMin, _hintBrightMax, wave);
                alpha = Mathf.Lerp(_hintAlphaMin, _hintAlphaMax, wave);
            }

            Color color = Color.white * bright;
            if (_dimmed)
            {
                color.r *= _dimMultiply;
                color.g *= _dimMultiply;
                color.b *= _dimMultiply;
            }

            color.a = alpha;
            skinRenderer.color = color;
        }

        /// <summary>
        /// Preview &lt; Placed &lt; Staging 그리기 순서. 밴드 내에서 마스크 격리용 슬롯을 부여한다.
        /// </summary>
        public void ApplySortingBand(int bandBase)
        {
            SetSortingOrder(bandBase);
        }

        private void SetSortingOrder(int sortingOrder)
        {
            int order = sortingOrder + _nextMaskSlot++;
            skinRenderer.sortingOrder = order;
            ApplyMaskIsolation(order);
        }

        private void ApplyMaskIsolation(int order)
        {
            if (spriteMask == null)
            {
                return;
            }

            spriteMask.isCustomRangeActive = true;
            spriteMask.backSortingLayerID = spriteMask.frontSortingLayerID = skinRenderer.sortingLayerID;
            spriteMask.frontSortingOrder = order;
            spriteMask.backSortingOrder = order - 1;
        }
    }
}
