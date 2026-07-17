using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 블록 칸 1개. SpriteRenderer에 색·스프라이트를 적용한다.
    /// SpriteMask Custom Range로 인접 칸 마스크와 격리한다.
    /// </summary>
    public sealed class Block : MonoBehaviour
    {
        private const int LayerOrderBand = 10000;
        private const int MaskOrderStride = 3;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("칸 스킨 클리핑용. SetSortingOrder에서 Custom Range로 인접 마스크와 격리")]
        [SerializeField] private SpriteMask spriteMask;

        private static int nextMaskSlot;
        private int maskSlot = -1;
        private Color baseColor = Color.white;
        private bool dimmed;
        private float dimMultiply = 1f;

        private void Awake()
        {
            Debug.Assert(spriteRenderer != null, "[Block] spriteRenderer is not assigned.", this);
            Debug.Assert(spriteMask != null, "[Block] spriteMask is not assigned.", this);
            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
            }

            SetSortingOrder(0);
        }

        public void ApplyVisual(Sprite sprite)
        {
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }

            RefreshColor();
        }

        /// <summary>
        /// 비활성 링 UX용. RGB만 <paramref name="multiply"/> 배로 어둡게 하고, false면 기본색 복원.
        /// </summary>
        public void SetDimmed(bool isDimmed, float multiply)
        {
            dimmed = isDimmed;
            dimMultiply = multiply;
            RefreshColor();
        }

        private void RefreshColor()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (!dimmed)
            {
                spriteRenderer.color = baseColor;
                return;
            }

            Color color = baseColor;
            color.r *= dimMultiply;
            color.g *= dimMultiply;
            color.b *= dimMultiply;
            spriteRenderer.color = color;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetLocalPosition(Vector3 localPosition)
        {
            transform.localPosition = localPosition;
        }

        public void SetLocalScale(Vector3 localScale)
        {
            transform.localScale = localScale;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            EnsureMaskSlot();
            int order = sortingOrder * LayerOrderBand + maskSlot * MaskOrderStride;
            spriteRenderer.sortingOrder = order;
            ApplyMaskIsolation(order);
        }

        private void EnsureMaskSlot()
        {
            if (maskSlot >= 0)
            {
                return;
            }

            maskSlot = nextMaskSlot++;
        }

        private void ApplyMaskIsolation(int order)
        {
            if (spriteMask == null)
            {
                return;
            }

            spriteMask.isCustomRangeActive = true;
            spriteMask.backSortingLayerID = spriteRenderer.sortingLayerID;
            spriteMask.frontSortingLayerID = spriteRenderer.sortingLayerID;
            spriteMask.backSortingOrder = order - 1;
            spriteMask.frontSortingOrder = order + 1;
        }
    }
}
