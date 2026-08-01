using System.Collections.Generic;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Simulation
{
    /// <summary>
    /// 3피스를 전부 놓는 시나리오 중 "가장 많이 클리어되는" 결과를 빔 서치로 추정 (SPEC §10.3 SimulateBestOutcome).
    /// 전수 탐색은 빈 보드에서 조합 폭발이라 beamWidth개의 유망 상태만 유지한다.
    /// 빔이 유지한 경로에서 완주가 나오면 full sequence 존재도 함께 증명된다.
    /// </summary>
    public static class SequenceOutcomeEstimator
    {
        public readonly struct SequenceOutcome
        {
            /// <summary>빔 탐색 범위 안에서 3피스 완주 경로를 찾았는가.</summary>
            public bool SequenceFound { get; }

            /// <summary>찾은 경로 중 최대 총 클리어 라인 수.</summary>
            public int TotalClears { get; }

            /// <summary>최선 경로 종료 시 보드가 완전히 비는가 (올클리어).</summary>
            public bool BoardEmptied { get; }

            public SequenceOutcome(bool sequenceFound, int totalClears, bool boardEmptied)
            {
                SequenceFound = sequenceFound;
                TotalClears = totalClears;
                BoardEmptied = boardEmptied;
            }
        }

        private sealed class BeamState
        {
            public BoardGrid Board;
            public int UsedMask;
            public int TotalClears;
            public int OccupiedCount;
        }

        public static SequenceOutcome Estimate(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            int beamWidth)
        {
            List<BeamState> frontier = new()
            {
                new BeamState
                {
                    Board = board,
                    UsedMask = 0,
                    TotalClears = 0,
                    OccupiedCount = CountOccupied(board),
                },
            };

            List<BeamState> nextFrontier = new();

            for (int depth = 0; depth < pieces.Count; ++depth)
            {
                nextFrontier.Clear();

                foreach (BeamState state in frontier)
                {
                    ExpandState(state, pieces, nextFrontier);
                }

                if (nextFrontier.Count == 0)
                {
                    return new SequenceOutcome(sequenceFound: false, totalClears: 0, boardEmptied: false);
                }

                // 클리어 많은 순, 동률이면 점유 칸 적은 순으로 상위 beamWidth 유지
                nextFrontier.Sort(static (a, b) => a.TotalClears != b.TotalClears
                    ? b.TotalClears.CompareTo(a.TotalClears)
                    : a.OccupiedCount.CompareTo(b.OccupiedCount));

                if (nextFrontier.Count > beamWidth)
                {
                    nextFrontier.RemoveRange(beamWidth, nextFrontier.Count - beamWidth);
                }

                (frontier, nextFrontier) = (nextFrontier, frontier);
            }

            BeamState best = frontier[0];
            return new SequenceOutcome(
                sequenceFound: true,
                totalClears: best.TotalClears,
                boardEmptied: best.OccupiedCount == 0);
        }

        private static void ExpandState(
            BeamState state,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            List<BeamState> nextFrontier)
        {
            int size = state.Board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;

            for (int pieceIndex = 0; pieceIndex < pieces.Count; ++pieceIndex)
            {
                if ((state.UsedMask & (1 << pieceIndex)) != 0)
                {
                    continue;
                }

                IReadOnlyList<Vector2Int> cellOffsets = pieces[pieceIndex];

                for (int x = 0; x < size; ++x)
                {
                    for (int y = 0; y < size; ++y)
                    {
                        pivot.x = x;
                        pivot.y = y;

                        if (!PlacementService.CanPlace(cellOffsets, pivot, state.Board))
                        {
                            continue;
                        }

                        BoardGrid next = state.Board.Clone();
                        int cleared = PlacementSimulator.PlaceAndClear(next, cellOffsets, pivot);

                        nextFrontier.Add(new BeamState
                        {
                            Board = next,
                            UsedMask = state.UsedMask | (1 << pieceIndex),
                            TotalClears = state.TotalClears + cleared,
                            OccupiedCount = CountOccupied(next),
                        });
                    }
                }
            }
        }

        private static int CountOccupied(BoardGrid board)
        {
            int size = board.BoardSize;
            int occupied = 0;
            Vector2Int cell = Vector2Int.zero;

            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    cell.x = x;
                    cell.y = y;

                    if (board.IsOccupied(cell))
                    {
                        ++occupied;
                    }
                }
            }

            return occupied;
        }
    }
}
