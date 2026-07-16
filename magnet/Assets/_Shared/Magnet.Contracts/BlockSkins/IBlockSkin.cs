using System.Collections.Generic;
using UnityEngine;

namespace Magnet.Contracts.BlockSkins
{
    /// <summary>
    /// 블록 비주얼 스킨 읽기 전용 계약. 멤버 Workspace가 아닌 공용 어셈블리에 둔다.
    /// </summary>
    public interface IBlockSkin
    {
        string SkinName { get; }
        string SkinId { get; }
        
        Sprite Sprite { get; }
    }
}
