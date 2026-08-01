using System;
using Magnet.Core.SO.Block;
using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// 모양 1종의 티어별 추첨 가중치 (SPEC §14.2). 0이면 그 티어에서 절대 안 나옴.
    /// </summary>
    [Serializable]
    public sealed class BlockShapeWeight
    {
        [SerializeField, Tooltip("가중치를 적용할 블록 모양")]
        private BlockShapeSO shape;

        [SerializeField, Tooltip("Normal 티어(번들 외 실시간 Fallback 포함) 가중치. 0이면 제외")]
        private float normalWeight = 10f;

        [SerializeField, Tooltip("Hospitality(접대) 실시간 생성 가중치. 큰·긴 블록일수록 높게")]
        private float hospitalityWeight = 10f;

        [SerializeField, Tooltip("Pressure(의도적 유일수) 실시간 생성 가중치")]
        private float pressureWeight = 10f;

        public BlockShapeSO Shape => shape;
        public float NormalWeight => normalWeight;
        public float HospitalityWeight => hospitalityWeight;
        public float PressureWeight => pressureWeight;
    }
}
