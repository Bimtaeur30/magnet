using Game.UI;
using Magnet.Contracts.BlockSkins;
using UnityEngine;

public class SkinBox : MonoBehaviour
{
    [SerializeField] private GameObject Check;
    [SerializeField] private SkinBoxView View;

    public void Init(bool isEquip, IBlockSkin skin)
    {
        View.ViewModel.Pattern = skin.Sprites[0];
    }
}