using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Turn
{
    public sealed class RelifeSession
    {
        public RelifeSession(int minScore)
        {
            MinScore = minScore < 0 ? 0 : minScore;
        }

        public int MinScore { get; }

        public bool Used { get; private set; }

        public IReadOnlyList<IReadOnlyList<Vector2Int>> PendingPieces { get; private set; }

        public bool CanOffer(int totalScore)
        {
            return !Used && PendingPieces == null && totalScore >= MinScore;
        }

        public void Offer(IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            PendingPieces = pieces;
        }

        public IReadOnlyList<IReadOnlyList<Vector2Int>> Accept()
        {
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces = PendingPieces;
            PendingPieces = null;
            if (pieces == null)
            {
                return null;
            }

            Used = true;
            return pieces;
        }
    }
}
