using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// 배치·클리어 연출 설정 묶음. 실제 값은 역할별 하위 SO에 둔다.
    /// </summary>
    [CreateAssetMenu(fileName = "PlacementConfig", menuName = "Magnet/Placement Config")]
    public sealed class PlacementConfigSO : ScriptableObject
    {
        [field: SerializeField] public BlockVisualConfigSO Visual { get; private set; }
        [field: SerializeField] public BlockDragConfigSO Drag { get; private set; }
    }
}
