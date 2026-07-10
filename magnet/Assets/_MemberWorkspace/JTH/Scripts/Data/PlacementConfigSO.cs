using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "PlacementConfig", menuName = "Magnet/Placement Config")]
    public sealed class PlacementConfigSO : ScriptableObject
    {
        [Tooltip("보드 하단에서 스테이징 영역까지 추가로 내릴 칸 수. stagingY = -(CellsPerSide + 이 값)")]
        [SerializeField] private int stagingYExtraBelow = 1;
        [Tooltip("블록 칸 스프라이트가 격자 칸 대비 차지하는 비율(0.1~1). 1이면 칸과 동일 크기")]
        [field: SerializeField] public float CellFill { get; private set; } = 0.9f;
        [Tooltip("Press 시작 포인터 X와의 거리(월드 유닛) 1당 블록 이동 배율 증가량. Block Blast식 감도 램프")]
        [field: SerializeField] public float DragSensitivityRampPerUnit { get; private set; } = 0.35f;
        [Tooltip("드래그 감도 배율 상한. 1이면 램프 없음")]
        [field: SerializeField] public float DragSensitivityMaxMultiplier { get; private set; } = 3f;
        
        /// <summary>보드 half 아래로 더 내릴 칸 수. stagingY = -(CellsPerSide + this).</summary>
        public int StagingYExtraBelow => stagingYExtraBelow;

        public int GetStagingY(int cellsPerSide) => -(cellsPerSide + stagingYExtraBelow);

        private void OnValidate()
        {
            stagingYExtraBelow = Mathf.Max(1, stagingYExtraBelow);
            CellFill = Mathf.Clamp(CellFill, 0.1f, 1f);
            DragSensitivityRampPerUnit = Mathf.Max(0f, DragSensitivityRampPerUnit);
            DragSensitivityMaxMultiplier = Mathf.Max(1f, DragSensitivityMaxMultiplier);
        }
    }
}
