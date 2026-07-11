using UnityEngine;

namespace PMS.Scripts.Skin
{
    [CreateAssetMenu(fileName = "Skin data", menuName = "Skin/SkinData")]
    public class SkinDataSO : ScriptableObject
    {
        public string skinId;
        public string skinName;

        public Sprite icon;
        public Sprite blockCellSprite;

        public SkinUnlockTypeEnum unlockType;
        public int unlockValue;
        public string unlockDescription;
    }
}