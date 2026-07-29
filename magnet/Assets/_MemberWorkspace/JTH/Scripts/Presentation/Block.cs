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
        
        private static int _nextMaskSlot;
        private bool _dimmed;
        private float _dimMultiply = 1f;
        private float _alpha = 1f;

        private void Awake()
        {
            float cellSize = boardConfigSO.CellSize;
            float fillAmount = boardConfigSO.CellFill;
            spriteMask.transform.localScale = new Vector3(cellSize * fillAmount, cellSize * fillAmount, 1);
            
            Vector3 visualOffset = Vector2.one * cellSize / 2;
            spriteMask.transform.localPosition = visualOffset;
        }

        public override void ResetItem()
        {
            base.ResetItem();
            
            Debug.Assert(skinRenderer != null, "[Block] spriteRenderer is not assigned.", this);
            Debug.Assert(spriteMask != null, "[Block] spriteMask is not assigned.", this);

            SetSortingOrder(10000);
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

        private void RefreshColor()
        {
            if (skinRenderer == null)
            {
                return;
            }

            Color color = Color.white;
            if (_dimmed)
            {
                color.r *= _dimMultiply;
                color.g *= _dimMultiply;
                color.b *= _dimMultiply;
            }

            color.a = _alpha;
            skinRenderer.color = color;
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
