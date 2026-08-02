using System.Collections.Generic;
using Magnet.Core.SO.Skin;
using UnityEngine;

namespace JTH.Scripts.Domain.Skin
{
    public class SkinSession
    {
        private readonly int _maxVariant;

        public SkinSession(SkinDataListSO skinDataList)
        {
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
            Fill(variants, drawCount);
            for (int i = variants.Count - 1; i > 0; --i)
            {
                int j = Random.Range(0, i + 1);
                (variants[i], variants[j]) = (variants[j], variants[i]);
            }
            
            return variants.GetRange(0, drawCount);
        }


        private void Fill(List<int> variants, int drawCount)
        {
            variants.Clear();
            while (variants.Count < drawCount)
            {
                for (int i = 0; i < _maxVariant; i++)
                    variants.Add(i);
            }
        }
    }
}