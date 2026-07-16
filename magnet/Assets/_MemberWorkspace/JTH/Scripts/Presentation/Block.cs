using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 블록 칸 1개. SpriteRenderer에 색·스프라이트를 적용한다.
    /// </summary>
    public sealed class Block : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            Debug.Assert(spriteRenderer != null, "[Block] spriteRenderer is not assigned.", this);
        }

        public void ApplyVisual(Sprite sprite)
        {
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
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
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }
}
