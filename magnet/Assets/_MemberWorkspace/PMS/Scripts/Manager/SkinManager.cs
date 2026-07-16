using System;
using GameLib.EventChannelSystem;
using Magnet.Contracts.Save;
using PMS.Scripts.Events;
using PMS.Scripts.Skin;
using Reflex.Attributes;
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

        private readonly List<SkinDataSO> _unlockedSkins = new();

        [Inject] private ISaveService _saveService;

        private int _currentSkinIndex;

        public SkinDataSO CurrentSkin
        {
            get
            {
                if (_currentSkinIndex < 0 || _currentSkinIndex >= _unlockedSkins.Count)
                {
                    return null;
                }

                return _unlockedSkins[_currentSkinIndex];
            }
        }

        private void Awake()
        {
            eventChannel.AddListener<SkinSelectRequestEvent>(OnSkinSelectRequest);
            eventChannel.AddListener<SkinUnlockCheckEvent>(OnSkinUnlockCheck);
            eventChannel.AddListener<SkinInventoryRequestEvent>(OnSkinInventoryRequest);

            if (_saveService != null)
            {
                InitializeFromSave();
            }
            else
            {
                InitializeWithoutSave();
            }
        }

        private void Start()
        {
            eventChannel.RaiseEvent(SkinEvents.SkinInitializedEvent.Init(_unlockedSkins[_currentSkinIndex]));
        }

        private void OnDestroy()
        {
            eventChannel.RemoveListener<SkinSelectRequestEvent>(OnSkinSelectRequest);
            eventChannel.RemoveListener<SkinUnlockCheckEvent>(OnSkinUnlockCheck);
            eventChannel.RemoveListener<SkinInventoryRequestEvent>(OnSkinInventoryRequest);
        }

        private void InitializeFromSave()
        {
            ValidateSaveData();
            LoadUnlockedSkinsFromSave();
            LoadEquippedSkinFromSave();

            RaiseInventoryResponse();
        }

        private void InitializeWithoutSave()
        {
            UnlockDefaultSkins();

            if (_unlockedSkins.Count > 0)
            {
                _currentSkinIndex = 0;

                eventChannel.RaiseEvent(
                    SkinEvents.SkinChangedEvent.Init(CurrentSkin, _currentSkinIndex)
                );
            }

            RaiseInventoryResponse();
        }

        private void ValidateSaveData()
        {
            if (skinList == null) return;

            List<string> validSkinIds = skinList
                .Where(skin => skin != null)
                .Select(skin => skin.SkinId)
                .ToList();

            _saveService.ValidateUnlockedSkins(validSkinIds);
        }

        private void LoadUnlockedSkinsFromSave()
        {
            _unlockedSkins.Clear();

            foreach (string skinId in _saveService.UnlockedSkinIds)
            {
                SkinDataSO skinData = FindSkinDataById(skinId);

                if (skinData != null)
                {
                    _unlockedSkins.Add(skinData);
                }
            }

            UnlockDefaultSkins();
        }

        private void LoadEquippedSkinFromSave()
        {
            _currentSkinIndex = 0;

            if (_unlockedSkins.Count == 0) return;

            string equippedSkinId = _saveService.EquippedSkinId;

            int index = _unlockedSkins.FindIndex(skin =>
                skin != null &&
                skin.SkinId == equippedSkinId
            );

            if (index >= 0)
            {
                _currentSkinIndex = index;
            }
            else
            {
                _saveService.EquipSkin(_unlockedSkins[0].SkinId);
            }

            eventChannel.RaiseEvent(
                SkinEvents.SkinChangedEvent.Init(CurrentSkin, _currentSkinIndex)
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
            if (skinIndex < 0 || skinIndex >= _unlockedSkins.Count) return;

            _currentSkinIndex = skinIndex;

            if (CurrentSkin != null)
            {
                _saveService?.EquipSkin(CurrentSkin.SkinId);
            }

            eventChannel.RaiseEvent(
                SkinEvents.SkinChangedEvent.Init(CurrentSkin, _currentSkinIndex)
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

            _unlockedSkins.Add(skinData);

            _saveService?.UnlockSkin(skinData.SkinId);

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
                SkinEvents.SkinInventoryResponseEvent.Init(_unlockedSkins, _currentSkinIndex)
            );
        }

        private bool IsUnlocked(SkinDataSO skinData)
        {
            if (skinData == null) return false;

            return _unlockedSkins.Any(unlockedSkin =>
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