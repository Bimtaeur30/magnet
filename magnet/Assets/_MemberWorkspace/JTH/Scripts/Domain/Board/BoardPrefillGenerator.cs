using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockBlast;
using UnityEngine;

namespace JTH.Scripts.Domain.Board
{
    /// <summary>
    /// 새 게임 시작 보드 생성.
    /// 1) 칸마다 독립 확률로 채운다(피스 단위 배치가 아니라 칸 단위 확률).
    /// 2) Normal 번들 중 큼지막하지 않은 것 하나를 골라, 그 모양대로 구멍을 뚫어 시작 직후 막히지 않게 한다.
    /// 완성된 줄이 생겨도 그대로 둔다(허용).
    /// </summary>
    public static class BoardPrefillGenerator
    {
        /// <summary>채울 칸 목록을 만든다. 실패하면 빈 목록(= 빈 보드로 시작).</summary>
        public static List<Vector2Int> Generate(
            int boardSize,
            BoardPrefillConfigSO config,
            IReadOnlyList<AreaBundleEntry> normalBundles,
            System.Random rng)
        {
            List<Vector2Int> result = new List<Vector2Int>();
            if (config == null || !config.Enabled || boardSize <= 0 || rng == null)
            {
                return result;
            }

            List<AreaBundleEntry> holeCandidates = CollectHoleCandidates(normalBundles, config.HoleBundleMaxCells);
            int cellTotal = boardSize * boardSize;

            for (int attempt = 0; attempt < config.MaxGenerateAttempts; ++attempt)
            {
                bool[,] filled = new bool[boardSize, boardSize];
                for (int x = 0; x < boardSize; ++x)
                {
                    for (int y = 0; y < boardSize; ++y)
                    {
                        filled[x, y] = rng.NextDouble() < config.FillProbability;
                    }
                }

                PunchBundleHole(filled, boardSize, holeCandidates, config.HoleClusterRadius, rng);

                int occupied = CountOccupied(filled, boardSize);
                if (cellTotal - occupied < config.MinEmptyCellsAfterHole)
                {
                    continue;
                }

                Collect(filled, boardSize, result);
                return result;
            }

            return result;
        }

        /// <summary>3피스 합산 셀 수가 상한 이하인 번들만. 큼지막한 번들을 제외한다.</summary>
        private static List<AreaBundleEntry> CollectHoleCandidates(
            IReadOnlyList<AreaBundleEntry> normalBundles,
            int maxCells)
        {
            List<AreaBundleEntry> candidates = new List<AreaBundleEntry>();
            if (normalBundles == null)
            {
                return candidates;
            }

            for (int i = 0; i < normalBundles.Count; ++i)
            {
                AreaBundleEntry entry = normalBundles[i];
                if (entry == null)
                {
                    continue;
                }

                int cells = 0;
                bool valid = true;
                foreach (int id in entry.Ids)
                {
                    IReadOnlyList<Vector2Int> offsets = SafeOffsets(id);
                    if (offsets == null)
                    {
                        valid = false;
                        break;
                    }

                    cells += offsets.Count;
                }

                if (valid && cells > 0 && cells <= maxCells)
                {
                    candidates.Add(entry);
                }
            }

            return candidates;
        }

        private static void PunchBundleHole(
            bool[,] filled,
            int boardSize,
            List<AreaBundleEntry> holeCandidates,
            int clusterRadius,
            System.Random rng)
        {
            if (holeCandidates.Count == 0)
            {
                return;
            }

            AreaBundleEntry bundle = holeCandidates[rng.Next(holeCandidates.Count)];
            Vector2Int center = new Vector2Int(rng.Next(boardSize), rng.Next(boardSize));

            foreach (int id in bundle.Ids)
            {
                IReadOnlyList<Vector2Int> offsets = SafeOffsets(id);
                if (offsets == null)
                {
                    continue;
                }

                Vector2Int anchor = ResolveAnchor(center, offsets, boardSize, clusterRadius, rng);
                for (int i = 0; i < offsets.Count; ++i)
                {
                    Vector2Int cell = anchor + offsets[i];
                    if (cell.x >= 0 && cell.x < boardSize && cell.y >= 0 && cell.y < boardSize)
                    {
                        filled[cell.x, cell.y] = false;
                    }
                }
            }
        }

        /// <summary>중심 주변으로 흩되, 모양이 보드 안에 들어오도록 클램프한다.</summary>
        private static Vector2Int ResolveAnchor(
            Vector2Int center,
            IReadOnlyList<Vector2Int> offsets,
            int boardSize,
            int clusterRadius,
            System.Random rng)
        {
            int jitterX = clusterRadius > 0 ? rng.Next(-clusterRadius, clusterRadius + 1) : 0;
            int jitterY = clusterRadius > 0 ? rng.Next(-clusterRadius, clusterRadius + 1) : 0;

            int minOffX = int.MaxValue, maxOffX = int.MinValue;
            int minOffY = int.MaxValue, maxOffY = int.MinValue;
            for (int i = 0; i < offsets.Count; ++i)
            {
                Vector2Int o = offsets[i];
                if (o.x < minOffX) minOffX = o.x;
                if (o.x > maxOffX) maxOffX = o.x;
                if (o.y < minOffY) minOffY = o.y;
                if (o.y > maxOffY) maxOffY = o.y;
            }

            int x = Mathf.Clamp(center.x + jitterX, -minOffX, boardSize - 1 - maxOffX);
            int y = Mathf.Clamp(center.y + jitterY, -minOffY, boardSize - 1 - maxOffY);
            return new Vector2Int(x, y);
        }

        private static IReadOnlyList<Vector2Int> SafeOffsets(int id)
        {
            if (id < BlockBlastCatalog.MinId || id > BlockBlastCatalog.MaxId)
            {
                return null;
            }

            return BlockBlastCatalog.GetOffsets(id);
        }

        private static int CountOccupied(bool[,] filled, int boardSize)
        {
            int count = 0;
            for (int x = 0; x < boardSize; ++x)
            {
                for (int y = 0; y < boardSize; ++y)
                {
                    if (filled[x, y])
                    {
                        ++count;
                    }
                }
            }

            return count;
        }

        private static void Collect(bool[,] filled, int boardSize, List<Vector2Int> into)
        {
            into.Clear();
            for (int x = 0; x < boardSize; ++x)
            {
                for (int y = 0; y < boardSize; ++y)
                {
                    if (filled[x, y])
                    {
                        into.Add(new Vector2Int(x, y));
                    }
                }
            }
        }
    }
}
