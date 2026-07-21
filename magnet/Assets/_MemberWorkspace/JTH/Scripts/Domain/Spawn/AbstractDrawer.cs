using Magnet.Contracts;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// 추첨용 난수 공급 계약. 테스트·재현을 위해 난수 생성을 로직에서 분리한다.
    /// </summary>
    public abstract class AbstractDrawer
    {
        /// <summary>0 이상 maxExclusive 미만의 정수를 반환.</summary>
        public abstract IBlockShape_ Next(BlockSpawnContext context);
    }
}
