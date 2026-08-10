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
    }
}
