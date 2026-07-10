using System.Collections.Generic;
using UnityEngine;

namespace Magnet.Contracts.BlockSkins
{
    /// <summary>
    /// 블록 비주얼 스킨 읽기 전용 계약. 멤버 Workspace가 아닌 공용 어셈블리에 둔다.
    /// </summary>
    public interface IBlockSkin
    {
        /// <summary>에디터·인벤토리에서 구분용 ID.</summary>
        string SkinId { get; }

        /// <summary>블록 피스에 적용 가능한 색상 풀.</summary>
        IReadOnlyList<Color> Colors { get; }

        /// <summary>블록 피스에 적용 가능한 스프라이트 풀.</summary>
        IReadOnlyList<Sprite> Sprites { get; }
    }
}
