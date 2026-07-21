using UnityEngine;

namespace _Shared.Magnet.Core.SO.Skin
{
    [CreateAssetMenu(fileName = "Skin data", menuName = "Skin/SkinData")]
    public class SkinDataSO : ScriptableObject
    {
        [field: SerializeField] public string SkinName {  get; private set; }
        [field: SerializeField] public string SkinId {  get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }

        public Sprite icon;
        public SkinUnlockTypeEnum unlockType;
        public int unlockValue;
        public string unlockDescription;
    }
}