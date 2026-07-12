using Magnet.Contracts.BlockSkins;
using UnityEngine;

public class SkinBox_UI : MonoBehaviour
{
    private IBlockSkin _blockSkin;
    public void Init(IBlockSkin blockSkin)
    {
        _blockSkin = blockSkin;
    }
}
