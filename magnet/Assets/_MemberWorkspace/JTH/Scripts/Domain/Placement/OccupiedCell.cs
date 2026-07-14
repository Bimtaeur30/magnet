using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    /// <summary>
    /// 보드에 부착된 뒤의 유일한 유닛. 멀티칸 블록 연결 개념 없음.
    /// </summary>
    public sealed class OccupiedCell
    {
        public OccupiedCell(int cellId, Vector2Int position)
        {
            CellId = cellId;
            Position = position;
        }

        public int CellId { get; }
        public Vector2Int Position { get; private set; }

        public void SetPosition(Vector2Int position)
        {
            Position = position;
        }
    }
}
