using GameLib.EventChannelSystem;
using PMS.Scripts.Events;
using PMS.Scripts.Skin;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PMS.Scripts.Manager
{
    public class SkinManager : MonoBehaviour
    {
        [SerializeField] private EventChannelSO eventChannel;

        [SerializeField] private List<SkinDataSO> skinList;
        private readonly List<string> unlockedSkinIds = new();

        public SkinDataSO CurrentSkin { get; private set; }

        private void Awake()
        {
            eventChannel.AddListener<SkinSelectRequestEvent>(OnSkinSelectRequest);
            eventChannel.AddListener<SkinUnlockCheckEvent>(OnSkinUnlockCheck);
        }

        private void OnDestroy()
        {
            eventChannel.RemoveListener<SkinSelectRequestEvent>(OnSkinSelectRequest);
            eventChannel.RemoveListener<SkinUnlockCheckEvent>(OnSkinUnlockCheck);
        }


        private void OnSkinSelectRequest(SkinSelectRequestEvent evt)
        {
            ChangeSkin(evt.SkinData);
        }

        private void Start()
        {
            UnlockDefaultSkins();
            SetDefaultSkin();
        }

        private void UnlockDefaultSkins()
        {
            if (skinList == null) return;

            foreach (SkinDataSO skinData in skinList)
            {
                if (skinData == null) continue;

                if (skinData.unlockType == SkinUnlockTypeEnum.Default)
                {
                    UnlockSkin(skinData);
                }
            }
        }

        private void SetDefaultSkin()
        {
            if (skinList == null || skinList.Count == 0) return;

            ChangeSkin(skinList[0]);
        }

        public bool IsUnlocked(SkinDataSO skinData)
        {
            if (skinData == null) return false;

            return unlockedSkinIds.Contains(skinData.skinId);
        }

        private void UnlockSkin(SkinDataSO skinData)
        {
            if (skinData == null || IsUnlocked(skinData)) return;

            unlockedSkinIds.Add(skinData.skinId);

            eventChannel.RaiseEvent(SkinEvents.SkinUnlockedEvent.Init(skinData.skinId));

            Debug.Log($"{skinData.skinName}스킨 해금됨");
        }

        private void OnSkinUnlockCheck(SkinUnlockCheckEvent evt)
        {
            if (skinList == null) return;

            var unlockableSkins = skinList
                .Where(skin => skin != null)
                .Where(skin => !IsUnlocked(skin))
                .Where(skin => skin.unlockType == evt.UnlockType)
                .Where(skin => evt.Value >= skin.unlockValue);

            foreach (SkinDataSO skinData in unlockableSkins)
            {
                UnlockSkin(skinData);
            }
        }

        private void ChangeSkin(SkinDataSO skinData)
        {
            if (skinData == null) return;
            if (!IsUnlocked(skinData)) return;

            CurrentSkin = skinData;

            eventChannel.RaiseEvent(SkinEvents.SkinChangedEvent.Init(CurrentSkin));

            Debug.Log($"{CurrentSkin.skinName}스킨으로 변경됨");
        }
    }
}