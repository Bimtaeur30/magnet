using System.Collections.Generic;
using UnityEngine;

namespace _Shared.Magnet.Core.SO.Skin
{
    [CreateAssetMenu(fileName = "Skin data list", menuName = "Skin/SkinDataList")]
    public class SkinDataListSO : ScriptableObject
    {
        [field: SerializeField] public List<SkinDataSO> Skins {  get; private set; }
    }
}