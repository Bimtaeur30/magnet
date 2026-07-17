using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BlockVisualConfig", menuName = "Magnet/Block Visual Config")]
    public sealed class BlockVisualConfigSO : ScriptableObject
    {
        [Tooltip("보드 하단에서 스테이징 영역까지 추가로 내릴 칸 수. stagingY = -(CellsPerSide + 이 값)")]
        [SerializeField] private int stagingYExtraBelow = 1;
        [Tooltip("블록 칸 스프라이트가 격자 칸 대비 차지하는 비율(0.1~1). 1이면 칸과 동일 크기")]
        [field: SerializeField] public float CellFill { get; private set; } = 0.9f;
        [Tooltip("드래그 중 보드 격자 프리뷰(고스트) 블록 알파(0~1). 스테이징에는 적용되지 않음")]
        [field: SerializeField] public float PreviewAlpha { get; private set; } = 0.4f;

        public int StagingYExtraBelow => stagingYExtraBelow;

        public int GetStagingY(int cellsPerSide) => -(cellsPerSide + stagingYExtraBelow);

        private void OnValidate()
        {
            stagingYExtraBelow = Mathf.Max(1, stagingYExtraBelow);
            CellFill = Mathf.Clamp(CellFill, 0.1f, 1f);
            PreviewAlpha = Mathf.Clamp01(PreviewAlpha);
        }
    }
}
