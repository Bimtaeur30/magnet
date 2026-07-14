using UnityEngine;

namespace _Shared.Magnet.Core.SceneTransition
{
    [CreateAssetMenu(fileName = "New Scene Def", menuName = "Magnet/Scene Transition/Scene Def", order = 1)]
    public class SceneDefSO : ScriptableObject
    {
        [Tooltip("Build Settings에 등록된 실제 씬 이름")]
        public string sceneName;
    }
}
