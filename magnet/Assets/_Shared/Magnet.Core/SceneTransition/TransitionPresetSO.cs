using LitMotion;
using UnityEngine;

namespace _Shared.Magnet.Core.SceneTransition
{
    [CreateAssetMenu(fileName = "New Transition Preset", menuName = "Magnet/Scene Transition/Transition Preset", order = 0)]
    public class TransitionPresetSO : ScriptableObject
    {
        [Tooltip("페이드 인/아웃 각각의 재생 시간(초)")]
        public float fadeDuration = 0.5f;

        [Tooltip("페이드 이징")]
        public Ease ease = Ease.OutQuad;
    }
}
