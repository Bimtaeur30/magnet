using GameLib.EventChannelSystem;
using PMS.Scripts.Skin;

namespace PMS.Scripts.Events
{
    public static class SkinEvents
    {
        public static readonly SkinSelectRequestEvent SkinSelectRequestEvent = new();
        public static readonly SkinChangedEvent SkinChangedEvent = new();
        public static readonly SkinUnlockCheckEvent SkinUnlockCheckEvent = new();
        public static readonly SkinUnlockedEvent SkinUnlockedEvent = new();
    }

    public class SkinSelectRequestEvent : GameEvent
    {
        public SkinDataSO SkinData { get; private set; }

        public SkinSelectRequestEvent Init(SkinDataSO skinData)
        {
            SkinData = skinData;
            return this;
        }
    }

    public class SkinChangedEvent : GameEvent
    {
        public SkinDataSO CurrentSkin { get; private set; }

        public SkinChangedEvent Init(SkinDataSO currentSkin)
        {
            CurrentSkin = currentSkin;
            return this;
        }
    }

    public class SkinUnlockCheckEvent : GameEvent
    {
        public SkinUnlockTypeEnum UnlockType { get; private set; }
        public int Value { get; private set; }

        public SkinUnlockCheckEvent Init(SkinUnlockTypeEnum unlockType, int value)
        {
            UnlockType = unlockType;
            Value = value;
            return this;
        }
    }
    public class SkinUnlockedEvent : GameEvent
    {
        public string SkinId { get; private set; }

        public SkinUnlockedEvent Init(string skinId)
        {
            SkinId = skinId;
            return this;
        }
    }
}