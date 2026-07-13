using System.Collections.Generic;
using UnityEngine;

namespace Magnet.Contracts.BlockShapes
{
    /// <summary>
    /// 블록 형태 읽기 전용 계약. 멤버 Workspace가 아닌 공용 어셈블리에 둔다.
    /// BlockShapeSO(SCRUM-25)는 이 인터페이스를 구현한다.
    /// </summary>
    public interface IBlockShape
    {
        /// <summary>에디터·풀에서 구분용 ID (예: "1x1", "L_3").</summary>
        string ShapeId { get; }

        /// <summary>배치 피벗 (0,0) 기준 상대 격자 좌표. 자석 축 좌표계와 동일.</summary>
        IReadOnlyList<Vector2Int> CellOffsets { get; }

        Texture2D Icon { get; }
    }
}
