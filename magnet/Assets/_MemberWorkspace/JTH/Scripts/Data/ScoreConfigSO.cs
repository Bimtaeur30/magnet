using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "ScoreConfig", menuName = "Magnet/Score Config")]
    public sealed class ScoreConfigSO : ScriptableObject
    {
        [Tooltip("세션 base 랜덤 하한(포함). ScoreSession 시작 시 한 번 추출")]
        [field: SerializeField] public int BaseMin { get; private set; } = 30;

        [Tooltip("세션 base 랜덤 상한(포함). ScoreSession 시작 시 한 번 추출")]
        [field: SerializeField] public int BaseMax { get; private set; } = 55;
    }
}
