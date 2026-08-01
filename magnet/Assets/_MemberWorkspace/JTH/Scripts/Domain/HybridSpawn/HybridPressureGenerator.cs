using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.BlockSelection.Solution;
using JTH.Scripts.Domain.Board;
using Random = System.Random;

namespace JTH.Scripts.Domain.HybridSpawn
{
    /// <summary>
    /// 의도적 유일수 실시간 생성의 42-ID 이식판 (구 PressureGenerator와 동일 원칙 — SPEC §11).
    /// full sequence가 정확히 1개이고 난이도 하한을 넘는 트리플만 채택,
    /// 유일해는 결과에 보관한다 (엄지척 UI 판정용).
    /// </summary>
    public static class HybridPressureGenerator
    {
        public sealed class PressureDraw
        {
            public int[] Triple { get; }
            public UniqueSolution Solution { get; }
            public float Difficulty { get; }

            public PressureDraw(int[] triple, UniqueSolution solution, float difficulty)
            {
                Triple = triple;
                Solution = solution;
                Difficulty = difficulty;
            }
        }

        public static PressureDraw TryGenerate(
            BoardGrid board,
            HybridPiecePool pool,
            HybridTuningSO tuning,
            Random rng,
            int[] avoidTriple)
        {
            PressureDraw best = null;

            for (int sample = 0; sample < tuning.PressureSampleCount; ++sample)
            {
                int[] triple = pool.SampleTriple(rng, avoidTriple);
                if (triple == null)
                {
                    return null;
                }

                UniqueSolution solution = PlacementSolver.TryFindUniqueFullSequence(
                    board, HybridPiecePool.BuildPieces(triple));
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
                    best = new PressureDraw(triple, solution, difficulty);
                }
            }

            return best;
        }

        /// <summary>
        /// 유일해 시나리오의 난이도 (SPEC §11.2 단순화):
        /// 마지막 스텝이 큰 블록이면 가산, 앞 두 스텝에서 라인 클리어가 필요하면 가산.
        /// </summary>
        private static float ComputeDifficulty(UniqueSolution solution, HybridTuningSO tuning)
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
