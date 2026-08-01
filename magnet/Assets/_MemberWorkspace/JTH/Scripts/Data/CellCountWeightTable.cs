using System;
using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// 칸 수 → 추첨 가중치 테이블. 42-ID 카탈로그는 회전형이 별도 ID라 모양 단위 가중표 대신
    /// 칸 수 단위로 티어별 분포(대형 선호·소형 선호 등)를 조절한다.
    /// 카탈로그에 존재하는 칸 수는 1·2·3·4·5·6·9뿐이다.
    /// </summary>
    [Serializable]
    public sealed class CellCountWeightTable
    {
        [SerializeField, Tooltip("인덱스 = 블록 칸 수 (0은 미사용). 0이면 그 칸 수 블록은 이 티어에서 안 나옴")]
        private float[] weightByCellCount;

        // Unity 직렬화용 (Activator 생성 경로)
        public CellCountWeightTable()
        {
        }

        public CellCountWeightTable(float[] weights)
        {
            weightByCellCount = weights;
        }

        public float WeightOf(int cellCount)
        {
            if (weightByCellCount == null || cellCount < 0 || cellCount >= weightByCellCount.Length)
            {
                return 0f;
            }

            return Mathf.Max(0f, weightByCellCount[cellCount]);
        }
    }
}
