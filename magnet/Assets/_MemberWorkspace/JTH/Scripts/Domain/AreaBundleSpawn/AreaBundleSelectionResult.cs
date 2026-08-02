using System.Collections.Generic;
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
            int deathCount,
            bool isKillHand,
            string reason)
        {
            Pieces = pieces;
            BlockIds = blockIds;
            Tier = tier;
            BundleId = bundleId;
            BoardAreaScore = boardAreaScore;
            PredictedAreaScore = predictedAreaScore;
            SequenceCount = sequenceCount;
            DeathCount = deathCount;
            IsKillHand = isKillHand;
            Reason = reason;
        }

        public List<IReadOnlyList<Vector2Int>> Pieces { get; }
        public int[] BlockIds { get; }
        public AreaBundleTier Tier { get; }
        public string BundleId { get; }
        public float BoardAreaScore { get; }
        public float PredictedAreaScore { get; }
        public int SequenceCount { get; }
        public int DeathCount { get; }
        public bool IsKillHand { get; }
        public string Reason { get; }
    }
}
