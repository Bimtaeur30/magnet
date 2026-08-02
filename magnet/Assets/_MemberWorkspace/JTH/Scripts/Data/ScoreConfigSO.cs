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

        [Tooltip("블럭을 설치했을 때 셀 하나당 점수")]
        [field: SerializeField] public int CellScore { get; private set; } = 1;

        [Tooltip("적에게 들어가는 데미지 전역 배수. 1=그대로, 0.5=절반. 점수 UI와 무관하게 공격 데미지만 조절")]
        [field: SerializeField, Min(0f)] public float EnemyDamageMultiplier { get; private set; } = 1f;
    }
}
