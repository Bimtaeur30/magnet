using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// Area 점수식 튜닝 — 4-연결 size/변 + 직사각 greedy 개수 패널티.
    /// </summary>
    [System.Serializable]
    public sealed class AreaScoreTuning
    {
        [Header("Empty Area")]
        [Tooltip("이 칸 수 이하 빈 Area 패널티")]
        public int emptyTinyMaxSize = 3;

        [Tooltip("tiny 빈 Area 점수 (음수)")]
        public float emptyTinyPenalty = -15f;

        [Tooltip("보드 전부 빈 Area일 때 점수")]
        public float emptyFullScore = 107f;

        [Header("Filled Area")]
        [Tooltip("이 칸 수 이하 찬 Area 패널티")]
        public int filledTinyMaxSize = 2;

        [Tooltip("tiny 찬 Area 점수 (음수)")]
        public float filledTinyPenalty = -8f;

        [Tooltip("보드 전부 찬 Area일 때 점수")]
        public float filledFullScore = 67f;

        [Header("Side bonus (filled, base≥0 only)")]
        [Tooltip("변 개수 ≤ 이 값이면 최대 보너스")]
        public int sideBonusIdealMax = 4;

        [Tooltip("이상 변 개수일 때 보너스")]
        public float sideBonusAtIdeal = 14f;

        [Tooltip("변 +2마다 깎는 양")]
        public float sideBonusPerTwoSides = 5f;

        [Header("Rectangle count")]
        [Tooltip("직사각(찬+빈 greedy) 1개당 점수에서 빼는 양")]
        public float rectCountPenalty = 3f;

        public static AreaScoreTuning GrillDefault() => new();
    }
}
