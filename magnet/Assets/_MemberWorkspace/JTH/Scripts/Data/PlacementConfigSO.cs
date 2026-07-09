using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "PlacementConfig", menuName = "Magnet/Placement Config")]
    public sealed class PlacementConfigSO : ScriptableObject
    {
        [Tooltip("보드 하단에서 스테이징 영역까지 추가로 내릴 칸 수. stagingY = -(CellsPerSide + 이 값)")]
        [SerializeField] private int stagingYExtraBelow = 1;
        [SerializeField] private Color pieceColor = new(0.35f, 0.7f, 1f, 0.95f);
        [Tooltip("블록 칸 스프라이트가 격자 칸 대비 차지하는 비율(0.1~1). 1이면 칸과 동일 크기")]
        [SerializeField] private float cellFill = 0.9f;

        /// <summary>보드 half 아래로 더 내릴 칸 수. stagingY = -(CellsPerSide + this).</summary>
        public int StagingYExtraBelow => stagingYExtraBelow;

        public Color PieceColor => pieceColor;
        public float CellFill => cellFill;

        public int GetStagingY(int cellsPerSide) => -(cellsPerSide + stagingYExtraBelow);

        private void OnValidate()
        {
            stagingYExtraBelow = Mathf.Max(1, stagingYExtraBelow);
            cellFill = Mathf.Clamp(cellFill, 0.1f, 1f);
        }
    }
}
