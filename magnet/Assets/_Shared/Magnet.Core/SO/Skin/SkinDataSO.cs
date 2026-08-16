using _Shared.Magnet.Core.SO.Skin;
using GameLib.ObjectPool.Runtime;
using UnityEngine;

namespace Magnet.Core.SO.Skin
{
    [CreateAssetMenu(fileName = "Skin data", menuName = "Skin/SkinData")]
    public class SkinDataSO : ScriptableObject
    {
        [field: SerializeField] public string SkinName {  get; private set; }
        [field: SerializeField] public string SkinId {  get; private set; }
        [field: SerializeField] public Sprite[] Sprites { get; private set; }

        [Tooltip("바리에이션별 클리어 예고 클립. Sprites와 같은 인덱스. 비어 있으면 스킵")]
        [field: SerializeField] public AnimationClip[] HintClips { get; private set; }

        [Tooltip("바리에이션별 라인클리어 칸 이펙트. Sprites와 같은 인덱스. 비어 있으면 스킵")]
        [field: SerializeField] public PoolItemSO[] LineClearEffects { get; private set; }

        [Tooltip("켜면 클리어 시 칸마다 안 쏘고 줄 가운데에 길쭉한 이펙트 1발")]
        [field: SerializeField] public bool FireCenteredLineClear { get; private set; }

        [Tooltip("가운데 1발용 길쭉한 이펙트. FireCenteredLineClear가 켜져 있을 때만 사용")]
        [field: SerializeField] public PoolItemSO CenterLineClearEffect { get; private set; }

        public Sprite icon;
        public SkinUnlockTypeEnum unlockType;
        public int unlockValue;
        public string unlockDescription;

        public int ResolveVariationIndex(int skinId)
        {
            if (Sprites == null || Sprites.Length == 0)
            {
                return 0;
            }

            int index = skinId % Sprites.Length;
            return index < 0 ? index + Sprites.Length : index;
        }

        public Sprite GetSprite(int skinId)
        {
            if (Sprites == null || Sprites.Length == 0)
            {
                return null;
            }

            return Sprites[ResolveVariationIndex(skinId)];
        }

        public AnimationClip GetHintClip(int skinId)
        {
            return GetVariation(HintClips, skinId);
        }

        public PoolItemSO GetLineClearEffect(int skinId)
        {
            return GetVariation(LineClearEffects, skinId);
        }

        private T GetVariation<T>(T[] items, int skinId) where T : class
        {
            if (items == null || items.Length == 0 || Sprites == null || Sprites.Length == 0)
            {
                return null;
            }

            int index = ResolveVariationIndex(skinId);
            if (index >= items.Length)
            {
                return null;
            }

            return items[index];
        }
    }
}