using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Bundles;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.BlockSelection.Solution;
using JTH.Scripts.Domain.Board;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.BlockSelection.Generation
{
    /// <summary>
    /// 의도적 유일수 실시간 생성 (SPEC §11). full sequence가 정확히 1개이고
    /// 난이도 하한을 넘는 조합만 채택, 유일해는 결과에 보관한다 (엄지척 UI 판정용).
    /// </summary>
    public static class PressureGenerator
    {
        public sealed class PressureDraw
        {
            public List<IReadOnlyList<Vector2Int>> Pieces { get; }
            public UniqueSolution Solution { get; }
            public float Difficulty { get; }

            public PressureDraw(List<IReadOnlyList<Vector2Int>> pieces, UniqueSolution solution, float difficulty)
            {
                Pieces = pieces;
                Solution = solution;
                Difficulty = difficulty;
            }
        }

        public static PressureDraw TryGenerate(
            BoardGrid board,
            IReadOnlyList<WeightedShape> pool,
            BlockSelectionTuningSO tuning,
            Random rng)
        {
            PressureDraw best = null;

            for (int sample = 0; sample < tuning.PressureSampleCount; ++sample)
            {
                List<IReadOnlyList<Vector2Int>> pieces = ShapeSampler.Sample3Rotated(pool, rng);
                if (pieces == null)
                {
                    return null;
                }

                UniqueSolution solution = PlacementSolver.TryFindUniqueFullSequence(board, pieces);
                if (solution == null)
                {
                    continue;
                }

                float difficulty = ComputeDifficulty(solution, tuning);
                if (difficulty < tuning.PressureDifficultyMin)
                {
                    continue;
                }

                if (best == null || difficulty > best.Difficulty)
                {
                    best = new PressureDraw(pieces, solution, difficulty);
                }
            }

            return best;
        }

        /// <summary>
        /// 유일해 시나리오의 난이도 (SPEC §11.2 단순화):
        /// 마지막 스텝이 큰 블록이면 가산, 앞 두 스텝에서 라인 클리어가 필요하면 가산.
        /// </summary>
        private static float ComputeDifficulty(UniqueSolution solution, BlockSelectionTuningSO tuning)
        {
            IReadOnlyList<SolutionStep> steps = solution.Steps;
            float difficulty = 0f;

            if (steps[^1].CellOffsets.Count >= tuning.PressureBigFinishMinCells)
            {
                difficulty += tuning.PressureBigFinishWeight;
            }

            int setupClears = 0;
            for (int i = 0; i < steps.Count - 1; ++i)
            {
                setupClears += steps[i].ClearedLines;
            }

            if (setupClears >= 1)
            {
                difficulty += tuning.PressureSetupClearWeight;
            }

            return difficulty;
        }
    }
}
