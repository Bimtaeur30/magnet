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
        [Tooltip("손 놓은 뒤 Y축 자석 스냅 LitMotion 시간(초)")]
        [field: SerializeField] public float SnapDuration { get; private set; } = 0.12f;
        [Tooltip("폭발 처리 후 보드·블록 90° 회전 LitMotion 시간(초)")]
        [field: SerializeField] public float RotationDuration { get; private set; } = 0.2f;
        [Tooltip("재조립 연출이 끝난 뒤 회전 시작 전 대기(초)")]
        [field: SerializeField] public float PreRotationDelay { get; private set; } = 0.25f;

        [Header("Clear Reassembly Motion")]
        [Tooltip("폭발 후 바깥으로 튕기는 거리(칸)")]
        [field: SerializeField] public float BounceCells { get; private set; } = 3f;
        [Tooltip("튕김 LitMotion 시간(초)")]
        [field: SerializeField] public float BounceDuration { get; private set; } = 0.15f;
        [Tooltip("착지(목표 칸 이동) 시간(초)")]
        [field: SerializeField] public float LandDuration { get; private set; } = 0.12f;
        [Tooltip("비행 중 자전 각속도(도/초)")]
        [field: SerializeField] public float SpinDegreesPerSecond { get; private set; } = 720f;
        [Tooltip("같은 링 칸 사이 스태거(초). 시계방향 촤라락 부착 간격")]
        [field: SerializeField] public float StaggerPerCell { get; private set; } = 0.04f;
        [Tooltip("다음 링 시작 지연(초). 이전 링 완료를 기다리지 않음")]
        [field: SerializeField] public float StaggerPerRing { get; private set; } = 0.12f;

        /// <summary>보드 half 아래로 더 내릴 칸 수. stagingY = -(CellsPerSide + this).</summary>
        public int StagingYExtraBelow => stagingYExtraBelow;

        public int GetStagingY(int cellsPerSide) => -(cellsPerSide + stagingYExtraBelow);

        private void OnValidate()
        {
            stagingYExtraBelow = Mathf.Max(1, stagingYExtraBelow);
            CellFill = Mathf.Clamp(CellFill, 0.1f, 1f);
            DragSensitivityRampPerUnit = Mathf.Max(0f, DragSensitivityRampPerUnit);
            DragSensitivityMaxMultiplier = Mathf.Max(1f, DragSensitivityMaxMultiplier);
            SnapDuration = Mathf.Max(0.01f, SnapDuration);
            RotationDuration = Mathf.Max(0.01f, RotationDuration);
            PreRotationDelay = Mathf.Max(0f, PreRotationDelay);
            BounceCells = Mathf.Max(0f, BounceCells);
            BounceDuration = Mathf.Max(0.01f, BounceDuration);
            LandDuration = Mathf.Max(0.01f, LandDuration);
            SpinDegreesPerSecond = Mathf.Max(0f, SpinDegreesPerSecond);
            StaggerPerCell = Mathf.Max(0f, StaggerPerCell);
            StaggerPerRing = Mathf.Max(0f, StaggerPerRing);
        }
    }
}
