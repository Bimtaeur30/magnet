using System.Collections.Generic;
using Magnet.Contracts.BlockShapes;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// SCRUM-25 SO 연동(Phase 5) 전 개발용 형태 소스. BlockShapePresets를 그대로 노출.
    /// </summary>
    public sealed class PresetShapeSource : IBlockShapeSource
    {
        public IReadOnlyList<IBlockShape> Shapes { get; }

        public PresetShapeSource()
        {
            Shapes = BlockShapePresets.All;
        }
    }
}
