using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// 새 게임 시작 시 보드를 미리 채우는 설정.
    /// 칸마다 독립 확률로 채우고(피스 단위 배치가 아님), 채운 뒤 번들 모양으로 구멍을 뚫어
    /// 시작하자마자 막히지 않게 한다.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardPrefillConfig", menuName = "Magnet/Board Prefill Config")]
    public sealed class BoardPrefillConfigSO : ScriptableObject
    {
        [Tooltip("끄면 예전처럼 빈 보드로 시작한다")]
        [field: SerializeField] public bool Enabled { get; private set; } = true;

        [Tooltip("칸 하나가 채워질 확률. 0.6이면 64칸 중 평균 약 38칸")]
        [field: SerializeField, Range(0f, 1f)] public float FillProbability { get; private set; } = 0.6f;

        [Tooltip("-1이면 매판 랜덤. 0 이상이면 그 값으로 고정(QA·디버그 재현용)")]
        [field: SerializeField] public int Seed { get; private set; } = -1;

        [Tooltip("구멍 뚫기에 쓸 Normal 번들의 셀 수 상한. 큼지막한 번들을 걸러낸다")]
        [field: SerializeField, Min(1)] public int HoleBundleMaxCells { get; private set; } = 10;

        [Tooltip("번들의 피스 3개를 이 반경 안에 흩어 뚫는다. 0이면 모두 같은 앵커")]
        [field: SerializeField, Min(0)] public int HoleClusterRadius { get; private set; } = 2;

        [Tooltip("구멍을 뚫고도 남은 칸이 이보다 적으면 다시 생성한다(전멸 방지 하한)")]
        [field: SerializeField, Min(0)] public int MinEmptyCellsAfterHole { get; private set; } = 12;

        [Tooltip("위 조건을 못 맞췄을 때 재생성 시도 횟수")]
        [field: SerializeField, Min(1)] public int MaxGenerateAttempts { get; private set; } = 8;
    }
}
