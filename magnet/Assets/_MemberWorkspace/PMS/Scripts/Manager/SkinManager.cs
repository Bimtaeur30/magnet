using GameLib.EventChannelSystem;
using Magnet.Contracts.Save;
using PMS.Scripts.Events;
using PMS.Scripts.Skin;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PMS.Scripts.Manager
{
    public class SkinManager : MonoBehaviour
    {
        [SerializeField] private EventChannelSO eventChannel;

        [Header("전체 스킨 목록")]
        [SerializeField] private List<SkinDataSO> skinList;

        private readonly List<SkinDataSO> unlockedSkins = new();

        private ISaveService saveService;
        private int currentSkinIndex;

        public SkinDataSO CurrentSkin
        {
            get
            {
                if (currentSkinIndex < 0 || currentSkinIndex >= unlockedSkins.Count)
                {
                    return null;
                }

                return unlockedSkins[currentSkinIndex];
            }
        }

        private void Awake()
        {
            eventChannel.AddListener<SkinSelectRequestEvent>(OnSkinSelectRequest);
            eventChannel.AddListener<SkinUnlockCheckEvent>(OnSkinUnlockCheck);
            eventChannel.AddListener<SkinInventoryRequestEvent>(OnSkinInventoryRequest);
        }

        private void Start()
        {
            if (saveService == null)
            {
                InitializeWithoutSave();
            }
        }

        private void OnDestroy()
        {
            eventChannel.RemoveListener<SkinSelectRequestEvent>(OnSkinSelectRequest);
            eventChannel.RemoveListener<SkinUnlockCheckEvent>(OnSkinUnlockCheck);
            eventChannel.RemoveListener<SkinInventoryRequestEvent>(OnSkinInventoryRequest);
        }

        public void Initialize(ISaveService saveService)
        {
            this.saveService = saveService;

            ValidateSaveData();
            LoadUnlockedSkinsFromSave();
            LoadEquippedSkinFromSave();

            RaiseInventoryResponse();
        }

        private void InitializeWithoutSave()
        {
            UnlockDefaultSkins();

            if (unlockedSkins.Count > 0)
            {
                currentSkinIndex = 0;

                eventChannel.RaiseEvent(
                    SkinEvents.SkinChangedEvent.Init(CurrentSkin, currentSkinIndex)
                );
            }

            RaiseInventoryResponse();
        }

        private void ValidateSaveData()
        {
            if (saveService == null) return;
            if (skinList == null) return;

            List<string> validSkinIds = skinList
                .Where(skin => skin != null)
                .Select(skin => skin.SkinId)
                .ToList();

            saveService.ValidateUnlockedSkins(validSkinIds);
        }

        private void LoadUnlockedSkinsFromSave()
        {
            unlockedSkins.Clear();

            if (saveService == null) return;

            foreach (string skinId in saveService.UnlockedSkinIds)
            {
                SkinDataSO skinData = FindSkinDataById(skinId);

                if (skinData != null)
                {
                    unlockedSkins.Add(skinData);
                }
            }

            UnlockDefaultSkins();
        }

        private void LoadEquippedSkinFromSave()
        {
            currentSkinIndex = 0;

            if (unlockedSkins.Count == 0) return;

            string equippedSkinId = saveService.EquippedSkinId;

            int index = unlockedSkins.FindIndex(skin =>
                skin != null &&
                skin.SkinId == equippedSkinId
            );

            if (index >= 0)
            {
                currentSkinIndex = index;
            }
            else
            {
                saveService.EquipSkin(unlockedSkins[0].SkinId);
            }

            eventChannel.RaiseEvent(
                SkinEvents.SkinChangedEvent.Init(CurrentSkin, currentSkinIndex)
            );
        }

        private void UnlockDefaultSkins()
        {
            if (skinList == null) return;

            foreach (SkinDataSO skinData in skinList)
            {
                if (skinData == null) continue;
                if (skinData.unlockType != SkinUnlockTypeEnum.Default) continue;

                UnlockSkin(skinData);
            }
        }

        private void OnSkinSelectRequest(SkinSelectRequestEvent evt)
        {
            ChangeSkin(evt.SkinIndex);
        }

        private void ChangeSkin(int skinIndex)
        {
            if (skinIndex < 0 || skinIndex >= unlockedSkins.Count) return;

            currentSkinIndex = skinIndex;

            if (CurrentSkin != null)
            {
                saveService?.EquipSkin(CurrentSkin.SkinId);
            }

            eventChannel.RaiseEvent(
                SkinEvents.SkinChangedEvent.Init(CurrentSkin, currentSkinIndex)
            );

            RaiseInventoryResponse();

            Debug.Log($"{CurrentSkin.SkinName} 스킨으로 변경됨");
        }

        private void OnSkinUnlockCheck(SkinUnlockCheckEvent evt)
        {
            if (skinList == null) return;

            foreach (SkinDataSO skinData in skinList)
            {
                if (skinData == null) continue;
                if (IsUnlocked(skinData)) continue;
                if (skinData.unlockType != evt.UnlockType) continue;
                if (evt.Value < skinData.unlockValue) continue;

                UnlockSkin(skinData);
            }

            RaiseInventoryResponse();
        }

        private void UnlockSkin(SkinDataSO skinData)
        {
            if (skinData == null) return;
            if (IsUnlocked(skinData)) return;

            unlockedSkins.Add(skinData);

            saveService?.UnlockSkin(skinData.SkinId);

            eventChannel.RaiseEvent(
                SkinEvents.SkinUnlockedEvent.Init(skinData)
            );

            Debug.Log($"{skinData.SkinName} 스킨 해금됨");
        }

        private void OnSkinInventoryRequest(SkinInventoryRequestEvent evt)
        {
            RaiseInventoryResponse();
        }

        private void RaiseInventoryResponse()
        {
            eventChannel.RaiseEvent(
                SkinEvents.SkinInventoryResponseEvent.Init(unlockedSkins, currentSkinIndex)
            );
        }

        private bool IsUnlocked(SkinDataSO skinData)
        {
            if (skinData == null) return false;

            return unlockedSkins.Any(unlockedSkin =>
                unlockedSkin != null &&
                unlockedSkin.SkinId == skinData.SkinId
            );
        }

        private SkinDataSO FindSkinDataById(string skinId)
        {
            if (string.IsNullOrEmpty(skinId)) return null;
            if (skinList == null) return null;

            return skinList.FirstOrDefault(skin =>
                skin != null &&
                skin.SkinId == skinId
            );
        }
    }
}