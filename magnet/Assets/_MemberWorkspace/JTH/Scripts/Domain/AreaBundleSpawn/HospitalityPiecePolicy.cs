using JTH.Scripts.Domain.BlockBlast;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public static class HospitalityPiecePolicy
    {
        public const float HalfWeight = 0.5f;
        public const float FullWeight = 1f;

        public static float FitWeight(int id)
        {
            if (id < BlockBlastCatalog.MinId || id > BlockBlastCatalog.MaxId)
            {
                return 0f;
            }

            // 2×2, 3×3, 2×3, 3×2
            if (id is 9 or 13 or 35 or 36)
            {
                return 0f;
            }

            int cells = BlockBlastCatalog.GetOffsets(id).Count;
            if (cells <= 2)
            {
                return 0f;
            }

            // ㄱ/L 계열 — Exact 접대에서 과다 (작은 L3 + L4 + 큰 L5)
            if (IsSmallL(id)
                || id is 8 or 29 or 30 or 31 or 32 or 33 or 34 or 42
                or 12 or 21 or 23 or 24)
            {
                return 0f;
            }

            // 1×4 — 일자 구멍 Exact 접대 과다
            if (id is 7 or 17)
            {
                return 0f;
            }

            // 1×3·3×1 — 접대 Exact 과다
            if (id is 4 or 5)
            {
                return 0f;
            }

            if (cells == 3)
            {
                return HalfWeight;
            }

            if (cells is 4 or 5)
            {
                return FullWeight;
            }

            return 0f;
        }

        public static bool IsAllowed(int id) => FitWeight(id) > 0f;

        /// <summary>3칸 ㄱ (L tromino) 4방향.</summary>
        public static bool IsSmallL(int id) => id is 6 or 15 or 27 or 28;
    }
}
