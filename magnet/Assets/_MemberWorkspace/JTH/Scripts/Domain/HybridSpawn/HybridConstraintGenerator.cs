using JTH.Scripts.Domain.BlockSelection.Tiers;
using JTH.Scripts.Domain.Board;
using Random = System.Random;

namespace JTH.Scripts.Domain.HybridSpawn
{
    /// <summary>
    /// 솔버 제약 검증형 실시간 생성 — Relife(Passable)·Trap·ComboBreak가 공유하는 골격.
    /// 풀에서 트리플을 샘플해 검증을 통과하는 첫 조합을 반환, 예산 소진 시 null (fallthrough).
    /// </summary>
    public static class HybridConstraintGenerator
    {
        public static int[] TryGenerate(
            BoardGrid board,
            HybridPiecePool pool,
            int sampleTries,
            BundleValidation validation,
            Random rng,
            int[] avoidTriple)
        {
            for (int attempt = 0; attempt < sampleTries; ++attempt)
            {
                int[] triple = pool.SampleTriple(rng, avoidTriple);
                if (triple == null)
                {
                    return null;
                }

                if (BundleTierSelector.IsValid(board, HybridPiecePool.BuildPieces(triple), validation))
                {
                    return triple;
                }
            }

            return null;
        }
    }
}
