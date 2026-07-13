using Magnet.Contracts.BlockSkins;
using System.Collections.Generic;
using UnityEngine;

namespace PMS.Scripts.Skin
{
    [CreateAssetMenu(fileName = "Skin data", menuName = "Skin/SkinData")]
    public class SkinDataSO : ScriptableObject, IBlockSkin
    {
        [field:SerializeField] public string SkinId {  get; private set; }
        [SerializeField] private List<Sprite> sprites = new();
        [SerializeField] private List<Color> colors = new() { Color.white };

        IReadOnlyList<Sprite> IBlockSkin.Sprites => sprites;
        IReadOnlyList<Color> IBlockSkin.Colors => colors;

        public string skinName;
        public Sprite icon;

        public SkinUnlockTypeEnum unlockType;
        public int unlockValue;
        public string unlockDescription;



    }
}