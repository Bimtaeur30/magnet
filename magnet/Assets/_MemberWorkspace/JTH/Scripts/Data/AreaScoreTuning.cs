using UnityEngine;

namespace JTH.Scripts.Data
{
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
        public float filledTinyPenalty = -12f;

        [Tooltip("보드 전부 찬 Area일 때 점수")]
        public float filledFullScore = 20f;

        [Header("Corner cover rectangle")]
        [Tooltip("네 모서리 기준·전 찬칸 덮개 직사각 중 최소 면적에 곱하는 패널티 계수")]
        public float cornerRectPenalty = 0.6f;

        [Header("Area count")]
        [Tooltip("Area(찬=4연결+깊은홈절단·빈=4연결) 1개당 점수에서 빼는 양. 영역이 적을수록 Total이 높아짐")]
        public float areaCountPenalty = 8f;
    }
}
