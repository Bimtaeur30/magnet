using System.Collections.Generic;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Generation
{
    /// <summary>
    /// "쏙 들어간다" 판정. 피스를 합법 위치에 놓았을 때 둘레(피스 밖 4방향 인접 칸)가
    /// 벽(보드 밖)·기존 블록으로 막힌 비율의 최댓값. 1.0 = 사방 밀폐, 위만 뚫린 포켓 ≈ 0.75.
    /// </summary>
    public static class SnugFitScorer
    {
        private static readonly Vector2Int[] Directions =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        };

        /// <summary>4회전 중 가장 잘 맞는 자리의 둘레 막힘 비율 (합법 배치가 없으면 0).</summary>
        public static float BestEnclosureAnyRotation(BoardGrid board, IReadOnlyList<Vector2Int> piece)
        {
            float best = 0f;

            for (int rotation = 0; rotation < 4; ++rotation)
            {
                best = Mathf.Max(best, BestEnclosure(board, ShapeRotator.Rotate(piece, rotation)));
                if (best >= 1f)
                {
                    return 1f;
                }
            }

            return best;
        }

        /// <summary>주어진 방향 그대로(회전 없이) 모든 합법 배치 중 최고 둘레 막힘 비율.</summary>
        public static float BestEnclosure(BoardGrid board, IReadOnlyList<Vector2Int> cellOffsets)
        {
            int size = board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;
            float best = 0f;

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

                    best = Mathf.Max(best, EnclosureRatio(board, cellOffsets, pivot));
                    if (best >= 1f)
                    {
                        return 1f;
                    }
                }
            }

            return best;
        }

        private static float EnclosureRatio(BoardGrid board, IReadOnlyList<Vector2Int> cellOffsets, Vector2Int pivot)
        {
            int total = 0;
            int blocked = 0;

            foreach (Vector2Int offset in cellOffsets)
            {
                Vector2Int cell = pivot + offset;

                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int neighbor = cell + direction;
                    if (IsPieceCell(cellOffsets, pivot, neighbor))
                    {
                        continue;
                    }

                    ++total;
                    if (!board.IsInBounds(neighbor) || board.IsOccupied(neighbor))
                    {
                        ++blocked;
                    }
                }
            }

            return total == 0 ? 0f : blocked / (float)total;
        }

        private static bool IsPieceCell(IReadOnlyList<Vector2Int> cellOffsets, Vector2Int pivot, Vector2Int cell)
        {
            foreach (Vector2Int offset in cellOffsets)
            {
                if (pivot + offset == cell)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
