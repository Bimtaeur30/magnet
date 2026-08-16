using System.Collections.Generic;
using JTH.Scripts.Data;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public sealed class AreaBundleSelectionResult
    {
        public AreaBundleSelectionResult(
            List<IReadOnlyList<Vector2Int>> pieces,
            int[] blockIds,
            AreaBundleTier tier,
            string bundleId,
            float heatScore,
            bool isKillHand,
            string reason,
            IReadOnlyList<AreaBundleExplainStep> explainSteps = null,
            ShapeWeightProfile profile = ShapeWeightProfile.Main)
        {
            Pieces = pieces;
            BlockIds = blockIds;
            Tier = tier;
            BundleId = bundleId;
            HeatScore = heatScore;
            IsKillHand = isKillHand;
            Reason = reason;
            ExplainSteps = explainSteps ?? System.Array.Empty<AreaBundleExplainStep>();
            Profile = profile;
        }

        public List<IReadOnlyList<Vector2Int>> Pieces { get; }
        public int[] BlockIds { get; }
        public AreaBundleTier Tier { get; }
        public string BundleId { get; }
        public float HeatScore { get; }
        public bool IsKillHand { get; }
        public string Reason { get; }
        public IReadOnlyList<AreaBundleExplainStep> ExplainSteps { get; }

        /// <summary>Unique면 Unique 가중. Normal/Easy는 Main.</summary>
        public ShapeWeightProfile Profile { get; }
    }
}
