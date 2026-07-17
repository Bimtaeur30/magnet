using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// 배치·클리어 연출 설정 묶음. 실제 값은 역할별 하위 SO에 둔다.
    /// </summary>
    [CreateAssetMenu(fileName = "PlacementConfig", menuName = "Magnet/Placement Config")]
    public sealed class PlacementConfigSO : ScriptableObject
    {
        [SerializeField] private BlockVisualConfigSO visual;
        [SerializeField] private BlockDragConfigSO drag;
        [SerializeField] private BlockSnapConfigSO snap;
        [SerializeField] private BoardRotationConfigSO rotation;
        [SerializeField] private ClearReassemblyRuleConfigSO clearReassemblyRule;
        [SerializeField] private ClearReassemblyMotionConfigSO clearReassemblyMotion;
        [SerializeField] private ExplosionBorderConfigSO explosionBorder;

        public BlockVisualConfigSO Visual => visual;
        public BlockDragConfigSO Drag => drag;
        public BlockSnapConfigSO Snap => snap;
        public BoardRotationConfigSO Rotation => rotation;
        public ClearReassemblyRuleConfigSO ClearReassemblyRule => clearReassemblyRule;
        public ClearReassemblyMotionConfigSO ClearReassemblyMotion => clearReassemblyMotion;
        public ExplosionBorderConfigSO ExplosionBorder => explosionBorder;
    }
}
