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
            float boardAreaScore,
            float predictedAreaScore,
            int sequenceCount,
            bool isKillHand,
            string reason,
            IReadOnlyList<AreaBundleExplainStep> explainSteps = null,
            ShapeWeightProfile profile = ShapeWeightProfile.Main)
        {
            Pieces = pieces;
            BlockIds = blockIds;
            Tier = tier;
            BundleId = bundleId;
            BoardAreaScore = boardAreaScore;
            PredictedAreaScore = predictedAreaScore;
            SequenceCount = sequenceCount;
            IsKillHand = isKillHand;
            Reason = reason;
            ExplainSteps = explainSteps ?? System.Array.Empty<AreaBundleExplainStep>();
            Profile = profile;
        }

        public List<IReadOnlyList<Vector2Int>> Pieces { get; }
        public int[] BlockIds { get; }
        public AreaBundleTier Tier { get; }
        public string BundleId { get; }
        public float BoardAreaScore { get; }
        public float PredictedAreaScore { get; }
        public int SequenceCount { get; }
        public bool IsKillHand { get; }
        public string Reason { get; }
        public IReadOnlyList<AreaBundleExplainStep> ExplainSteps { get; }

        /// <summary>Normal Clean/Main 구분. Unique 티어면 Unique 가중 의미.</summary>
        public ShapeWeightProfile Profile { get; }
    }
}
