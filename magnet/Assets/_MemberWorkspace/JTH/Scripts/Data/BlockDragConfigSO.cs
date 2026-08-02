using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BlockDragConfig", menuName = "Magnet/Block Drag Config")]
    public sealed class BlockDragConfigSO : ScriptableObject
    {
        [Tooltip("Press 시작 포인터 X와의 거리(월드 유닛) 1당 블록 이동 배율 증가량. Block Blast식 감도 램프")]
        [field: SerializeField] public float SensitivityRampPerUnit { get; private set; } = 0.35f;
        
        [Tooltip("인덱스에 따른 스테이징 블록 시작 x좌표들")]
        [field: SerializeField] public List<float> StagingBlockStartXPositions { get; private set; } = new List<float>();

        [Tooltip("마지막 피봇이 존재할 때 얼마나 떨어져도 유지되는지에 대한 값(칸 단위)")]
        [field: SerializeField] public float LastPivotSnapThreshold { get; private set; } = 1.5f;
    }
}
