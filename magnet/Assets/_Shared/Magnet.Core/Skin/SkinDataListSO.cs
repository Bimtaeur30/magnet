using PMS.Scripts.Skin;
using System.Collections.Generic;
using UnityEngine;

namespace PMS.Scripts.Skin
{
    [CreateAssetMenu(fileName = "Skin data list", menuName = "Skin/SkinDataList")]
    public class SkinDataListSO : ScriptableObject
    {
        [field: SerializeField] public List<SkinDataSO> Skins {  get; private set; }
    }
}