using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class BlockShatterHint : MonoBehaviour
    {
        public const int SeedMin = 1;
        public const int SeedCount = 8;

        static readonly int ShatterId = Shader.PropertyToID("_Shatter");
        static readonly int ShatterSeedId = Shader.PropertyToID("_ShatterSeed");
        static readonly int SpriteUVRectId = Shader.PropertyToID("_SpriteUVRect");
        static readonly int WaterWobbleId = Shader.PropertyToID("_WaterWobble");

        [SerializeField] private SpriteRenderer skinRenderer;

        [Tooltip("클리어 예고 클립이 조절하는 쩌적 세기. 0이면 원본 스프라이트")]
        public float shatter;

        [Tooltip("물풍선 스킨의 클리어 예고 클립이 조절하는 말랑거림 세기")]
        public float waterWobble;

        private MaterialPropertyBlock _propertyBlock;
        private int _shatterSeed = SeedMin;

        private void Reset()
        {
            skinRenderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            Apply();
        }

        public void Apply()
        {
            if (skinRenderer == null)
            {
                return;
            }

            _propertyBlock ??= new MaterialPropertyBlock();
            skinRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(ShatterId, shatter);
            _propertyBlock.SetFloat(ShatterSeedId, _shatterSeed);
            _propertyBlock.SetFloat(WaterWobbleId, waterWobble);
            ApplySpriteUVRect(_propertyBlock);
            skinRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetSeed(int seed)
        {
            int wrapped = seed - SeedMin;
            wrapped %= SeedCount;
            if (wrapped < 0)
            {
                wrapped += SeedCount;
            }

            _shatterSeed = wrapped + SeedMin;
            Apply();
        }

        public static int SeedFromCell(Vector2Int cell)
        {
            int wrapped = cell.x + cell.y * 3;
            wrapped %= SeedCount;
            if (wrapped < 0)
            {
                wrapped += SeedCount;
            }

            return wrapped + SeedMin;
        }

        public void ResetShatter()
        {
            shatter = 0f;
            waterWobble = 0f;
            Apply();
        }

        private void ApplySpriteUVRect(MaterialPropertyBlock propertyBlock)
        {
            Sprite sprite = skinRenderer.sprite;
            if (sprite == null || sprite.texture == null)
            {
                propertyBlock.SetVector(SpriteUVRectId, new Vector4(0f, 0f, 1f, 1f));
                return;
            }

            Texture texture = sprite.texture;
            Rect rect = sprite.textureRect;
            propertyBlock.SetVector(
                SpriteUVRectId,
                new Vector4(
                    rect.x / texture.width,
                    rect.y / texture.height,
                    rect.width / texture.width,
                    rect.height / texture.height));
        }
    }
}
