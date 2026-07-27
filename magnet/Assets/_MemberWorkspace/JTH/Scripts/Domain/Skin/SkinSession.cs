using System.Collections.Generic;
using Magnet.Core.SO.Skin;
using UnityEngine;

namespace JTH.Scripts.Domain.Skin
{
    public class SkinSession
    {
        private readonly SkinDataListSO _skinDataList;
        
        private readonly int _maxVariant;

        public SkinSession(SkinDataListSO skinDataList)
        {
            _skinDataList = skinDataList;

            foreach (SkinDataSO data in skinDataList.Skins)
            {
                if (data.Sprites.Length == 0)
                    Debug.LogError("스킨엔 스프라이트가 한개 이상 있어야 합니다.");
                if (data.Sprites.Length > _maxVariant)
                    _maxVariant = data.Sprites.Length;
            }
        }

        public IReadOnlyList<int> DrawSkinIds(int drawCount)
        {
            List<int> variants = new List<int>();
            Fill(variants);
            
            while (variants.Count > drawCount)
            {
                if (variants.Count <= 0)
                    Fill(variants);
                int idx = Random.Range(0, variants.Count);
                variants.RemoveAt(idx);
            }
            
            return variants;
        }

        private void Fill(List<int> variants)
        {
            variants.Clear();
            for (int i = 0; i < _maxVariant; i++)
            {
                variants.Add(i);
            }
        }
    }
}