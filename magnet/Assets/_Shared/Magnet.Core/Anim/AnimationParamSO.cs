using UnityEngine;

namespace GGMLib.Anim
{
    [CreateAssetMenu(fileName = "Animation Param", menuName = "Lib/Animation Param")]
    public sealed class AnimationParamSO : ScriptableObject
    {
        [field: SerializeField]
        public string AnimationName { get; private set; }

        [field: SerializeField]
        public int Hash { get; private set; }

        private void OnValidate()
        {
            Hash = Animator.StringToHash(AnimationName ?? string.Empty);
        }
    }
}
