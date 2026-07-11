using Mvvm;
using UnityEngine;
public enum BlockSlots
{
    First, Second, Third
}

namespace Game.UI
{
    public sealed partial class BlockSlotsUIView : MvvmView<BlockSlotsUIViewModel>
    {
        public void SetBlockSlotSprite(Sprite sprite, BlockSlots slots)
        {
            switch(slots)
            {
                case BlockSlots.First:
                    ViewModel.BlockImage1 = sprite;
                    break;
                case BlockSlots.Second:
                    ViewModel.BlockImage2 = sprite;
                    break;
                case BlockSlots.Third:
                    ViewModel.BlockImage3 = sprite;
                    break;
            }
        }
    }
}
