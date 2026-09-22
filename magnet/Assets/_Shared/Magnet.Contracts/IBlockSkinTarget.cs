using UnityEngine;

namespace Magnet.Contracts
{
    /// <summary>Skin operations shared by in-game block lifecycle events.</summary>
    public interface IBlockSkinTarget
    {
        void ApplySkin(Sprite skinSprite);
    }
}