using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockBlast
{
    /// <summary>
    /// BlockBlastAlgorithm 1회 선택 결과. Pieces가 게임 파이프라인 출력이고
    /// 나머지는 진단·로그용이다.
    /// </summary>
    public sealed class BlockBlastSelection
    {
        public int Round { get; }

        /// <summary>Trait 교체 전 기본 전략 ID (7 = random-no-death).</summary>
        public int BaseAlgoId { get; }

        /// <summary>블록을 실제로 만든 공개 알고리즘 ID (7 / 1370 / 2100).</summary>
        public int ActualAlgoId { get; }

        public IReadOnlyList<int> BlockIds { get; }

        public List<IReadOnlyList<Vector2Int>> Pieces { get; }

        /// <summary>선택 경로 설명 (로그용).</summary>
        public string Reason { get; }

        public BlockBlastSelection(
            int round,
            int baseAlgoId,
            int actualAlgoId,
            IReadOnlyList<int> blockIds,
            List<IReadOnlyList<Vector2Int>> pieces,
            string reason)
        {
            Round = round;
            BaseAlgoId = baseAlgoId;
            ActualAlgoId = actualAlgoId;
            BlockIds = blockIds;
            Pieces = pieces;
            Reason = reason;
        }
    }
}
