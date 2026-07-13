using Game.UI;
using Magnet.Contracts.BlockShapes;
using Magnet.Contracts.BlockSkins;
using UnityEngine;
using UnityEngine.UI;

public class BlockSlot_UI : MonoBehaviour
{
    [SerializeField] private BlockSlotView SlotVIew;
    private IBlockShape _shape;
    private IBlockSkin _skin;

    public void SetSlot(IBlockShape shape, IBlockSkin skin)
    {
        _shape = shape;
        _skin = skin;
        SlotVIew.ViewModel.BlockImage1 = skin.Sprites[0];

        SetBlockImageAlpha(1f);
    }

    public void EmptySlot()
    {
        SetBlockImageAlpha(0f);
        SlotVIew.ViewModel.BlockImage1 = null;
    }

    private void SetBlockImageAlpha(float alpha)
    {
        Color c = SlotVIew.ViewModel.BlockImage1Color;
        c.a = alpha;
        SlotVIew.ViewModel.BlockImage1Color = c;
    }
}
