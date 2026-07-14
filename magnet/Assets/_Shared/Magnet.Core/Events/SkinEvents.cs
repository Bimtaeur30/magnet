using System.Collections.Generic;
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

        public static readonly SkinInventoryRequestEvent SkinInventoryRequestEvent = new();
        public static readonly SkinInventoryResponseEvent SkinInventoryResponseEvent = new();
    }

    public class SkinInventoryRequestEvent : GameEvent
    {
    }

    public class SkinInventoryResponseEvent : GameEvent
    {
        public IReadOnlyList<SkinDataSO> UnlockedSkins { get; private set; }
        public int SelectedIndex { get; private set; }

        public SkinInventoryResponseEvent Init(IReadOnlyList<SkinDataSO> unlockedSkins, int selectedIndex)
        {
            UnlockedSkins = unlockedSkins;
            SelectedIndex = selectedIndex;
            return this;
        }
    }

    public class SkinSelectRequestEvent : GameEvent
    {
        public int SkinIndex { get; private set; }

        public SkinSelectRequestEvent Init(int skinIndex)
        {
            SkinIndex = skinIndex;
            return this;
        }
    }

    public class SkinChangedEvent : GameEvent
    {
        public SkinDataSO CurrentSkin { get; private set; }
        public int SelectedIndex { get; private set; }

        public SkinChangedEvent Init(SkinDataSO currentSkin, int selectedIndex)
        {
            CurrentSkin = currentSkin;
            SelectedIndex = selectedIndex;
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