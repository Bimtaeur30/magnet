using System;
using System.Collections.Generic;

namespace PTY.Scripts.Save
{
    [Serializable]
    public class GameSaveData
    {
        public int SchemaVersion = 1;
        public int BestScore;
        public List<string> UnlockedSkinIds = new();
    }
}
