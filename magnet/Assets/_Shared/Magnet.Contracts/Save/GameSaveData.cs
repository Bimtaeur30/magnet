using System;
using System.Collections.Generic;

namespace Magnet.Contracts.Save
{
    [Serializable]
    public class GameSaveData
    {
        public int SchemaVersion = 2;
        public int BestStage;
        /// <summary>Schema v1 호환. 로드 시 BestStage로 이관한 뒤 비운다.</summary>
        public int BestScore;
        public List<string> UnlockedSkinIds = new();
        public string EquippedSkinId;
        public float TotalPlayTime;
        public int MaxExplosionCombo;
        public int GameOverCount;
    }
}
