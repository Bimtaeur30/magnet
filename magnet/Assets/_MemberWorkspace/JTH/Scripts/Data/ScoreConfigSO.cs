using System;
using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "ScoreConfig", menuName = "Magnet/Score Config")]
    public sealed class ScoreConfigSO : ScriptableObject
    {
        [Serializable]
        public struct KTier
        {
            [Tooltip("이 구간에 포함되는 최대 콤보(이상이면 다음 구간). 오름차순이어야 함")]
            public int maxComboInclusive;

            [Tooltip("해당 콤보 구간의 배율 k")]
            public float k;
        }

        [Tooltip("콤보 구간별 k. maxComboInclusive 오름차순. 마지막 구간이 60+ 등으로 나머지 처리")]
        [SerializeField] private KTier[] kTiers =
        {
            new KTier { maxComboInclusive = 5, k = 53.33f },
            new KTier { maxComboInclusive = 12, k = 44f },
            new KTier { maxComboInclusive = 59, k = 30f },
            new KTier { maxComboInclusive = 9999, k = 24f },
        };

        [Tooltip("같은 배치 안 웨이브 순번(1-based)별 연쇄 배율. 길이 부족 시 마지막 값 사용")]
        [SerializeField] private float[] streakMultipliers =
        {
            1.00f,
            1.35f,
            1.60f,
            1.80f,
        };

        public float GetK(int combo)
        {
            if (combo < 1 || kTiers == null || kTiers.Length == 0)
            {
                return 0f;
            }

            for (int i = 0; i < kTiers.Length; i++)
            {
                if (combo <= kTiers[i].maxComboInclusive)
                {
                    return kTiers[i].k;
                }
            }

            return kTiers[kTiers.Length - 1].k;
        }

        public float GetStreakMultiplier(int waveIndex1Based)
        {
            if (waveIndex1Based < 1 || streakMultipliers == null || streakMultipliers.Length == 0)
            {
                return 1f;
            }

            int index = waveIndex1Based - 1;
            if (index >= streakMultipliers.Length)
            {
                return streakMultipliers[streakMultipliers.Length - 1];
            }

            return streakMultipliers[index];
        }

        private void OnValidate()
        {
            if (kTiers != null)
            {
                for (int i = 0; i < kTiers.Length; i++)
                {
                    kTiers[i].maxComboInclusive = Mathf.Max(1, kTiers[i].maxComboInclusive);
                    kTiers[i].k = Mathf.Max(0f, kTiers[i].k);
                }
            }

            if (streakMultipliers != null)
            {
                for (int i = 0; i < streakMultipliers.Length; i++)
                {
                    streakMultipliers[i] = Mathf.Max(0f, streakMultipliers[i]);
                }
            }
        }
    }
}
