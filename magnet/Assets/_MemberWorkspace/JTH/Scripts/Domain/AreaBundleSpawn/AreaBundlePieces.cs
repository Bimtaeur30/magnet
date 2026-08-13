using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockBlast;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public static class AreaBundlePieces
    {
        public static List<IReadOnlyList<Vector2Int>> Build(AreaBundleEntry entry)
        {
            return new List<IReadOnlyList<Vector2Int>>(3)
            {
                BlockBlastCatalog.GetOffsets(entry.Id0),
                BlockBlastCatalog.GetOffsets(entry.Id1),
                BlockBlastCatalog.GetOffsets(entry.Id2),
            };
        }
    }
}
