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

        /// <summary>
        /// 유일수 손에서 이 슬롯을 정답 칸 집합에 놓았는지.
        /// </summary>
        public bool IsUniqueCorrectPlacement(int slotIndex, IReadOnlyList<Vector2Int> placedCells)
        {
            if (Tier != AreaBundleTier.Unique || ExplainSteps == null || placedCells == null)
            {
                return false;
            }

            for (int i = 0; i < ExplainSteps.Count; ++i)
            {
                AreaBundleExplainStep step = ExplainSteps[i];
                if (step.PieceSlotIndex != slotIndex)
                {
                    continue;
                }

                return SameCellSet(step.Cells, placedCells);
            }

            return false;
        }

        private static bool SameCellSet(IReadOnlyList<Vector2Int> a, IReadOnlyList<Vector2Int> b)
        {
            if (a == null || a.Count != b.Count)
            {
                return false;
            }

            var set = new HashSet<Vector2Int>(a.Count);
            for (int i = 0; i < a.Count; ++i)
            {
                set.Add(a[i]);
            }

            for (int i = 0; i < b.Count; ++i)
            {
                if (!set.Contains(b[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
