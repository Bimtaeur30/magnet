using System.Collections.Generic;
using System.Text;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Simulation
{
    public static class PlacementSolver
    {
        public static bool FullSequenceExists(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            return CountSequences(board, pieces, cap: 1) >= 1;
        }

        /// <summary>
        /// 피스들을 순서·위치를 바꿔가며 전부 배치(배치마다 라인클리어 반영)할 수 있는 완주 시퀀스 개수를 센다.
        /// found가 cap에 도달하면 더 찾지 않는다.
        /// </summary>
        public static int CountFullSequences(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces, int cap = int.MaxValue)
        {
            return CountSequences(board, pieces, cap);
        }

        /// <summary>
        /// CountFullSequences의 실제 구현. 공개 API는 cap 기본값이 int.MaxValue일 뿐이다.
        /// </summary>
        private static int CountSequences(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            int cap)
        {
            if (pieces.Count == 0)
            {
                return 0;
            }

            string[] signatures = BuildSignatures(pieces);
            bool[] used = new bool[pieces.Count];
            int found = 0;
            Search(board, pieces, signatures, used, placedCount: 0, cap, ref found);
            return found;
        }
        
        /// <summary>
        /// IReadOnlyList는 HashSet에 넣으면 내용이 같아도 참조가 다르면 다른 키로 본다.
        /// 그래서 각 피스의 오프셋을 같은 규칙의 문자열로 만들어, 같은 모양이면 같은 시그니처가 되게 한다.
        /// </summary>
        private static string[] BuildSignatures(IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            string[] signatures = new string[pieces.Count];
            StringBuilder builder = new();

            for (int i = 0; i < pieces.Count; ++i)
            {
                builder.Clear();
                foreach (Vector2Int offset in pieces[i])
                {
                    builder.Append(offset.x).Append(',').Append(offset.y).Append(';');
                }

                signatures[i] = builder.ToString();
            }

            return signatures;
        }

        /// <summary>
        /// 아직 쓰지 않은 피스 중 하나를 골라 TryPlacements로 모든 위치에 놓아 보고, 백트래킹한다.
        /// 예: 인덱스 2를 쓰면 used[2]=true로 TryPlacements 후, used[2]=false로 되돌린 뒤 다음 인덱스를 시도한다.
        /// 같은 모양 피스는 signatures + triedAtDepth로 이 depth에서 한 번만 시도한다.
        /// </summary>
        private static void Search(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            string[] signatures,
            bool[] used,
            int placedCount,
            int cap,
            ref int found)
        {
            if (found >= cap)
            {
                return;
            }

            if (placedCount == pieces.Count)
            {
                ++found;
                return;
            }

            HashSet<string> triedAtDepth = new();

            for (int i = 0; i < pieces.Count; ++i)
            {
                if (used[i] || !triedAtDepth.Add(signatures[i]))
                {
                    continue;
                }

                used[i] = true;
                TryPlacements(board, pieces, signatures, used, placedCount, cap, ref found, i);
                used[i] = false;

                if (found >= cap)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// pieceIndex 피스를 놓을 수 있는 모든 칸에 놓고(Clone + PlaceAndClear), 그 보드로 Search를 이어 간다.
        /// </summary>
        private static void TryPlacements(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            string[] signatures,
            bool[] used,
            int placedCount,
            int cap,
            ref int found,
            int pieceIndex)
        {
            IReadOnlyList<Vector2Int> cellOffsets = pieces[pieceIndex];
            int size = board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;

            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    pivot.x = x;
                    pivot.y = y;

                    if (!PlacementService.CanPlace(cellOffsets, pivot, board))
                    {
                        continue;
                    }

                    BoardGrid next = board.Clone();
                    PlacementSimulator.PlaceAndClear(next, cellOffsets, pivot);
                    Search(next, pieces, signatures, used, placedCount + 1, cap, ref found);

                    if (found >= cap)
                    {
                        return;
                    }
                }
            }
        }
    }
}
