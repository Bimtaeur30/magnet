using System.Collections.Generic;
using System.Text;
using JTH.Scripts.Domain.BlockSelection.Solution;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Simulation
{
    public static class PlacementSolver
    {
        /// <summary>
        /// 첫 번째로 완성된 full sequence의 스텝(슬롯·피벗·클리어 수)을 보관하는 기록기.
        /// </summary>
        private sealed class SequenceRecorder
        {
            public readonly List<SolutionStep> Stack = new();
            public SolutionStep[] Captured;

            public void Capture()
            {
                Captured ??= Stack.ToArray();
            }
        }

        /// <summary>
        /// 피스들 중 하나라도 넣을 수 있는지 검사하는 메서드
        /// </summary>
        public static bool HasAnyPlacement(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            foreach (IReadOnlyList<Vector2Int> cellOffsets in pieces)
            {
                if (PlacementService.CanPlaceAnywhere(cellOffsets, board))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool FullSequenceExists(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            return CountSequences(board, pieces, cap: 1, requireClear: false) >= 1;
        }

        public static bool ComboMaintainable(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            return CountSequences(board, pieces, cap: 1, requireClear: true) >= 1;
        }

        public static int CountFullSequences(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces, int cap = int.MaxValue)
        {
            return CountSequences(board, pieces, cap, requireClear: false);
        }

        /// <summary>
        /// full sequence가 정확히 1개일 때 그 유일해를 반환, 아니면 null (SPEC §11.5).
        /// cap=2로 조기 종료하며 첫 완주 시퀀스의 스텝을 기록해 재탐색 없이 보관한다.
        /// </summary>
        public static UniqueSolution TryFindUniqueFullSequence(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            SequenceRecorder recorder = new();
            int count = CountSequences(board, pieces, cap: 2, requireClear: false, recorder);
            return count == 1 ? new UniqueSolution(recorder.Captured) : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cap">found가 cap이 된다면 반복을 멈춘다. 상한선</param>
        /// <param name="requireClear">true라면 라인 클리어가 되어야지 found를 1 올린다.</param>
        /// <param name="recorder">null이 아니면 첫 완주 시퀀스의 스텝을 기록한다.</param>
        /// <returns></returns>
        private static int CountSequences(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            int cap,
            bool requireClear,
            SequenceRecorder recorder = null)
        {
            if (pieces.Count == 0)
            {
                return 0;
            }

            string[] signatures = BuildSignatures(pieces);
            bool[] used = new bool[pieces.Count];
            int found = 0;
            Search(board, pieces, signatures, used, placedCount: 0, clearsSoFar: 0, requireClear, cap, ref found, recorder);
            return found;
        }
        
        /// <summary>
        /// IReadOnlyList는 HashSet에 담았을 때 같은 pieces를 가지더라도 객체다 다르면 다르다고 판단하기 때문에 list를 string으로
        /// 바꾸는 작업이 필요하다. 그래서 piece를 IReadOnlyList Vector2Int가 아닌 요소들을 string으로 바꿔서 stringBuilder로
        /// 쌓아넣은 string으로 변환해서 반환한다. 이러면 같은 모양을 같은 string으로 만들 수 있기 때문에 HashSet에 넣었을 때 제대로
        /// 작동한다.
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

        private static void Search(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            string[] signatures,
            bool[] used,
            int placedCount,
            int clearsSoFar,
            bool requireClear,
            int cap,
            ref int found,
            SequenceRecorder recorder)
        {
            if (found >= cap)
            {
                return;
            }

            //piece 하나를 다 놓았을 때, requireClear가 false면 그냥 ++를 하면 되고, 아니라면 한 번의 클리어라도 있을 때 ++를 한다.
            if (placedCount == pieces.Count)
            {
                if (!requireClear || clearsSoFar > 0)
                {
                    ++found;
                    recorder?.Capture();
                }

                return;
            }

            HashSet<string> triedAtDepth = new();

            // pieces의 수만큼 used[i]을 true로 한 상태에서 TryPlacements를 돌리고 재귀함수 -> Placements에서 board에 넣을 piece를 
            // 넣는다(맨 마지막 매개 변수). 그리고 넣은 상태의 보드와 방금 사용한 pieces의 i를 false로 한 상태로 다시 Search에 넣는다.
            // 그럼 board에 넣은 상태와 사용한 pieces가 유지된 상태로 다시 Search가 처음부터 돌아가고, 이게 반복된다. triedAtDepth에선 
            // board는 TryPlacements에서 board.Clone()을 통해 바뀌지 않기 때문에 HashSet을 만들어서 1번 블럭과 2번 블럭이 같다면 굳이 
            // 2번 블럭을 넣어보지 않는다. (짜피 결과는 같기 때문, 그래서 순서를 고려하지 않은 found 값이 나온다(뭘 먼저 넣든 갇다면 found는 1)
            for (int i = 0; i < pieces.Count; ++i)
            {
                if (used[i] || !triedAtDepth.Add(signatures[i]))
                {
                    continue;
                }

                used[i] = true;
                TryPlacements(board, pieces, signatures, used, placedCount, clearsSoFar, requireClear, cap, ref found, i, recorder);
                used[i] = false;

                if (found >= cap)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// board와 cellOffsets(piece)를 받아서 넣을 수 있는 모든 경우의 수에 board를 복사해서 piece를 넣는다. 넣은 상태로 다시
        /// Search를 돌린다.
        /// </summary>
        private static void TryPlacements(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            string[] signatures,
            bool[] used,
            int placedCount,
            int clearsSoFar,
            bool requireClear,
            int cap,
            ref int found,
            int pieceIndex,
            SequenceRecorder recorder)
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
                    int cleared = PlacementSimulator.PlaceAndClear(next, cellOffsets, pivot);

                    recorder?.Stack.Add(new SolutionStep(pieceIndex, pivot, cellOffsets, cleared));
                    Search(next, pieces, signatures, used, placedCount + 1, clearsSoFar + cleared, requireClear, cap, ref found, recorder);
                    recorder?.Stack.RemoveAt(recorder.Stack.Count - 1);

                    if (found >= cap)
                    {
                        return;
                    }
                }
            }
        }
    }
}
