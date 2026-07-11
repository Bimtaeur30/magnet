using System.Collections.Generic;

namespace Magnet.Contracts.BlockShapes
{
    /// <summary>
    /// 추첨 대상 블록 형태 풀 제공 계약.
    /// 개발용은 코드 프리셋(PresetShapeSource), 실사용은 BlockShapeSO 기반 소스(SCRUM-25 / PTY)가 구현한다.
    /// </summary>
    public interface IBlockShapeSource
    {
        /// <summary>추첨 후보 형태 목록. 비어 있지 않아야 한다.</summary>
        IReadOnlyList<IBlockShape> Shapes { get; }
    }
}
