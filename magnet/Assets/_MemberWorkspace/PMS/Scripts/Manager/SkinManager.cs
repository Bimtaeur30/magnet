using GameLib.EventChannelSystem;
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

        private void OnDestroy()
        {
            eventChannel.RemoveListener<SkinSelectRequestEvent>(OnSkinSelectRequest);
            eventChannel.RemoveListener<SkinUnlockCheckEvent>(OnSkinUnlockCheck);
            eventChannel.RemoveListener<SkinInventoryRequestEvent>(OnSkinInventoryRequest);
        }

        private void Start()
        {
            UnlockDefaultSkins();
            SetDefaultSkin();
            RaiseInventoryResponse();
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
            if (unlockedSkins.Count == 0) return;

            currentSkinIndex = 0;

            eventChannel.RaiseEvent(
                SkinEvents.SkinChangedEvent.Init(CurrentSkin, currentSkinIndex)
            );
        }

        private void OnSkinSelectRequest(SkinSelectRequestEvent evt)
        {
            ChangeSkin(evt.SkinIndex);
        }

        private void ChangeSkin(int skinIndex)
        {
            if (skinIndex < 0 || skinIndex >= unlockedSkins.Count)
            {
                Debug.LogWarning($"잘못된 스킨 인덱스입니다: {skinIndex}");
                return;
            }

            currentSkinIndex = skinIndex;

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

            eventChannel.RaiseEvent(
                SkinEvents.SkinUnlockedEvent.Init(skinData.SkinId)
            );

            Debug.Log($"{skinData.SkinName} 스킨 해금됨");
        }

        private bool IsUnlocked(SkinDataSO skinData)
        {
            if (skinData == null) return false;

            return unlockedSkins.Any(unlockedSkin =>
                unlockedSkin != null &&
                unlockedSkin.SkinId == skinData.SkinId
            );
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
    }
}