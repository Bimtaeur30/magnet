using System.Collections.Generic;
using _Shared.Magnet.Core.SO.Skin;
using UnityEngine;

namespace Magnet.Core.SO.Skin
{
    [CreateAssetMenu(fileName = "Skin data list", menuName = "Skin/SkinDataList")]
    public class SkinDataListSO : ScriptableObject
    {
        [field: SerializeField] public List<SkinDataSO> Skins {  get; private set; }
    }
}