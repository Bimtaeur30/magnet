using System.Collections.Generic;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Simulation
{
    public static class SequenceOutcomeEstimator
    {
        /// <summary>
        /// 시퀀스를 완료했을 때의 결과를 담을 구조체
        /// </summary>
        public readonly struct SequenceOutcome
        {
            /// <summary>
            /// 빔이 마지막 depth까지 살아남아 완주 후보를 하나라도 남겼는지
            /// </summary>
            public bool SequenceFound { get; }

            /// <summary>
            /// 살아남은 후보 중 추정 최선(클리어 우선)의 누적 클리어 줄 수
            /// </summary>
            public int TotalClears { get; }

            /// <summary>
            /// 그 최선 후보의 최종 보드가 비었는지(올클 추정)
            /// </summary>
            public bool BoardEmptied { get; }

            /// <summary>
            /// 그 최선 후보의 최종 보드
            /// </summary>
            public BoardGrid FinalBoard { get; }

            public SequenceOutcome(bool sequenceFound, int totalClears, bool boardEmptied, BoardGrid finalBoard)
            {
                SequenceFound = sequenceFound;
                TotalClears = totalClears;
                BoardEmptied = boardEmptied;
                FinalBoard = finalBoard;
            }
        }

        private sealed class BeamState
        {
            public BoardGrid Board { get; set; }
            public int UsedMask { get; set; }
            public int TotalClears { get; set; }
            public int OccupiedCount { get; set; }
        }

        /// <summary>
        /// 보드와 피스 목록을 받아, 한 수씩 펼친 뒤 상위 beamWidth개만 남기는 빔 탐색으로 완주 결과를 추정한다.
        /// 각 depth에서 미사용 피스를 모든 칸에 놓아 보고(ExpandState), 클리어 많은 순·잔여 칸 적은 순으로 잘라
        /// 상위 beamWidth만 다음 depth로 넘긴다. 전수 탐색보다 빠르지만, 중간에 자른 가지의 올클 등은 놓칠 수 있다.
        /// </summary>
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
                    return new SequenceOutcome(sequenceFound: false, totalClears: 0, boardEmptied: false, finalBoard: null);
                }

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
                boardEmptied: best.OccupiedCount == 0,
                finalBoard: best.Board);
        }

        /// <summary>
        /// 아직 쓰지 않은 피스마다 보드 전 칸에 놓아 보고, 가능하면 자식 BeamState를 nextFrontier에 넣는다.
        /// </summary>
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
