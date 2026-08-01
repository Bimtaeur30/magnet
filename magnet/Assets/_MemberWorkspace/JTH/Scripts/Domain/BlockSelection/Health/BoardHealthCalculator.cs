using System.Collections.Generic;
using System.Text;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Health
{
    public static class BoardHealthCalculator
    {
        private const int MaxDeadZoneSize = 3;
        private const float EmptySideFillComponentMax = 0.5f;

        private static readonly Vector2Int[] Square3x3 =
        {
            new(0, 0), new(0, 1), new(0, 2),
            new(1, 0), new(1, 1), new(1, 2),
            new(2, 0), new(2, 1), new(2, 2),
        };

        private static readonly Vector2Int[] Line1x5Horizontal =
        {
            new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0),
        };

        private static readonly Vector2Int[] Line1x5Vertical =
        {
            new(0, 0), new(0, 1), new(0, 2), new(0, 3), new(0, 4),
        };

        private static readonly Vector2Int[] Directions =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        };

        public static BoardHealthResult Compute(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> freedomProbePieces,
            BlockSelectionTuningSO tuning)
        {
            float fillRate = ComputeFillRate(board);
            int deadZoneCount = CountDeadZones(board);
            int bigPieceSlots = CountBigPieceSlots(board);
            float placementFreedom = ComputePlacementFreedom(board, freedomProbePieces);
            float score = ComputeScore(fillRate, deadZoneCount, bigPieceSlots, placementFreedom, tuning);
            HealthZone zone = ResolveZone(fillRate, score, tuning);

            return new BoardHealthResult(fillRate, deadZoneCount, bigPieceSlots, placementFreedom, score, zone);
        }

        private static float ComputeFillRate(BoardGrid board)
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

            return occupied / (float)(size * size);
        }

        private static int CountDeadZones(BoardGrid board)
        {
            int size = board.BoardSize;
            bool[,] visited = new bool[size, size];
            Queue<Vector2Int> queue = new();
            int deadZones = 0;

            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    Vector2Int start = new(x, y);
                    if (visited[x, y] || board.IsOccupied(start))
                    {
                        continue;
                    }

                    int regionSize = FloodFillEmptyRegion(board, visited, start, queue);
                    if (regionSize <= MaxDeadZoneSize)
                    {
                        ++deadZones;
                    }
                }
            }

            return deadZones;
        }

        private static int FloodFillEmptyRegion(BoardGrid board, bool[,] visited, Vector2Int start, Queue<Vector2Int> queue)
        {
            visited[start.x, start.y] = true;
            queue.Enqueue(start);
            int count = 0;

            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                ++count;

                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int next = cell + direction;
                    if (!board.IsInBounds(next) || visited[next.x, next.y] || board.IsOccupied(next))
                    {
                        continue;
                    }

                    visited[next.x, next.y] = true;
                    queue.Enqueue(next);
                }
            }

            return count;
        }

        private static int CountBigPieceSlots(BoardGrid board)
        {
            return CountPlacements(board, Square3x3)
                + CountPlacements(board, Line1x5Horizontal)
                + CountPlacements(board, Line1x5Vertical);
        }

        private static float ComputePlacementFreedom(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> probePieces)
        {
            if (probePieces == null || probePieces.Count == 0)
            {
                return 0f;
            }

            StringBuilder builder = new();
            HashSet<string> rotationSignatures = new();
            int totalPlacements = 0;

            foreach (IReadOnlyList<Vector2Int> piece in probePieces)
            {
                rotationSignatures.Clear();

                for (int rotation = 0; rotation < 4; ++rotation)
                {
                    IReadOnlyList<Vector2Int> rotated = ShapeRotator.Rotate(piece, rotation);
                    if (!rotationSignatures.Add(BuildSignature(rotated, builder)))
                    {
                        continue;
                    }

                    totalPlacements += CountPlacements(board, rotated);
                }
            }

            return totalPlacements / (float)probePieces.Count;
        }

        private static string BuildSignature(IReadOnlyList<Vector2Int> offsets, StringBuilder builder)
        {
            List<Vector2Int> sorted = new(offsets);
            sorted.Sort(static (a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

            builder.Clear();
            foreach (Vector2Int offset in sorted)
            {
                builder.Append(offset.x).Append(',').Append(offset.y).Append(';');
            }

            return builder.ToString();
        }

        private static int CountPlacements(BoardGrid board, IReadOnlyList<Vector2Int> cellOffsets)
        {
            int size = board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;
            int count = 0;

            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    pivot.x = x;
                    pivot.y = y;

                    if (PlacementService.CanPlace(cellOffsets, pivot, board))
                    {
                        ++count;
                    }
                }
            }

            return count;
        }

        private static float ComputeScore(
            float fillRate,
            int deadZoneCount,
            int bigPieceSlots,
            float placementFreedom,
            BlockSelectionTuningSO tuning)
        {
            float fillComponent = FillComponent(fillRate, tuning);
            float deadZoneComponent = 1f - Mathf.Clamp01(deadZoneCount / (float)tuning.DeadZoneNormalizeMax);
            float bigSlotComponent = Mathf.Clamp01(bigPieceSlots / (float)tuning.BigSlotNormalizeMax);
            float freedomComponent = Mathf.Clamp01(placementFreedom / tuning.FreedomNormalizeMax);

            return tuning.FillWeight * fillComponent
                + tuning.DeadZoneWeight * deadZoneComponent
                + tuning.BigSlotWeight * bigSlotComponent
                + tuning.FreedomWeight * freedomComponent;
        }

        private static float FillComponent(float fillRate, BlockSelectionTuningSO tuning)
        {
            if (fillRate < tuning.TooEmptyFillMax)
            {
                return fillRate / tuning.TooEmptyFillMax * EmptySideFillComponentMax;
            }

            if (fillRate > tuning.TooDirtyFillMin)
            {
                return Mathf.Max(0f, 1f - (fillRate - tuning.TooDirtyFillMin) / tuning.FillDirtyFalloff);
            }

            return 1f;
        }

        private static HealthZone ResolveZone(float fillRate, float score, BlockSelectionTuningSO tuning)
        {
            if (fillRate < tuning.TooEmptyFillMax)
            {
                return HealthZone.TooEmpty;
            }

            if (fillRate > tuning.TooDirtyFillMin)
            {
                return HealthZone.TooDirty;
            }

            if (score < tuning.TooEmptyScoreMax)
            {
                return HealthZone.TooEmpty;
            }

            if (score < tuning.TooDirtyScoreMax)
            {
                return HealthZone.TooDirty;
            }

            return HealthZone.Sweet;
        }
    }
}
