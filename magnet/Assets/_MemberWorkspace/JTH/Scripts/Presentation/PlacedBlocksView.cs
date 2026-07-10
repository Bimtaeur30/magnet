using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 부착 완료된 ShapeBlock 인스턴스를 보드 아래에 보관한다.
    /// </summary>
    public sealed class PlacedBlocksView : MonoBehaviour
    {
        public void Adopt(ShapeBlock block, string displayName)
        {
            if (block == null)
            {
                return;
            }

            block.transform.SetParent(transform, worldPositionStays: true);
            block.name = displayName;
        }
    }
}
