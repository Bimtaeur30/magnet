using System.Collections.Generic;
using JTH.Scripts.Domain.Skin;
using Magnet.Contracts;
using UnityEngine;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// 하단 4슬롯 블록 후보 상태. 추첨·이벤트는 담당하지 않는다.
    /// </summary>
    public sealed class BlockSupply
    {
        public const int SlotCount = 3;
        
        private readonly AbstractDrawer _drawer;
        private readonly SkinSession _skinSession;
        
        private readonly List<ShapeBlockData> _slots;
        
        public IReadOnlyList<ShapeBlockData> Candidates => _slots;
        
        public BlockSupply(AbstractDrawer drawer, SkinSession skinSession)
        {
            _drawer = drawer;
            _skinSession = skinSession;
            
            _slots = new List<ShapeBlockData>(SlotCount);
        }
        
        public void Fill(BlockSpawnContext context)
        {
            IReadOnlyList<IReadOnlyList<Vector2Int>> cellOffsetsList = _drawer.Draw(context, SlotCount);
            IReadOnlyList<int> skinVariationList = _skinSession.DrawSkinIds(SlotCount);

            Debug.Assert(cellOffsetsList.Count == SlotCount && skinVariationList.Count == SlotCount
                , $"배열의 수가 맞지 않습니다. cellOffsets={cellOffsetsList.Count}, skinVariationList={skinVariationList.Count}");
            
            _slots.Clear();
            for (int i = 0; i < SlotCount; i++)
            {
                ShapeBlockData data = new ShapeBlockData
                {
                    CellOffsets = cellOffsetsList[i],
                    SkinId = skinVariationList[i]
                };
                _slots.Add(data);
            }
        }

        public void FillFrom(IReadOnlyList<IReadOnlyList<Vector2Int>> cellOffsetsList)
        {
            IReadOnlyList<int> skinVariationList = _skinSession.DrawSkinIds(SlotCount);

            Debug.Assert(cellOffsetsList != null && cellOffsetsList.Count == SlotCount
                && skinVariationList.Count == SlotCount,
                $"배열의 수가 맞지 않습니다. cellOffsets={cellOffsetsList?.Count}, skinVariationList={skinVariationList.Count}");

            _slots.Clear();
            for (int i = 0; i < SlotCount; i++)
            {
                ShapeBlockData data = new ShapeBlockData
                {
                    CellOffsets = cellOffsetsList[i],
                    SkinId = skinVariationList[i]
                };
                _slots.Add(data);
            }
        }
        
        public void Consume(int slotIndex)
        {
            if (slotIndex is < 0 or >= SlotCount)
            {
                return;
            }
        
            _slots[slotIndex] = null;
        }
    }
}
