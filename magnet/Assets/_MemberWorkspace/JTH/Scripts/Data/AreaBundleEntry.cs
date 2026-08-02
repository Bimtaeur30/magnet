using System;
using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>42-ID 고정 3블록 번들 (스폰 회전 없음).</summary>
    [Serializable]
    public sealed class AreaBundleEntry
    {
        [SerializeField] private string bundleId;
        [SerializeField] private int id0 = 5;
        [SerializeField] private int id1 = 9;
        [SerializeField] private int id2 = 3;
        [SerializeField] private int weight = 1;

        public string BundleId => string.IsNullOrEmpty(bundleId) ? $"{id0}-{id1}-{id2}" : bundleId;
        public int Id0 => id0;
        public int Id1 => id1;
        public int Id2 => id2;
        public int Weight => weight < 1 ? 1 : weight;

        public AreaBundleEntry()
        {
        }

        public AreaBundleEntry(string bundleId, int id0, int id1, int id2, int weight = 1)
        {
            this.bundleId = bundleId;
            this.id0 = id0;
            this.id1 = id1;
            this.id2 = id2;
            this.weight = weight;
        }

        public int[] Ids => new[] { id0, id1, id2 };
    }
}
