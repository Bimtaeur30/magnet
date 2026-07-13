using System.Collections.Generic;
using System.Linq;
using GameLib.EventChannelSystem;
using PMS.Scripts.Events;
using PMS.Scripts.Skin;
using PTY.Scripts.Events;
using Reflex.Attributes;
using UnityEngine;

namespace PTY.Scripts.Save
{
    /// <summary>
    /// 게임플레이 이벤트(스킨 해금/장착, 게임오버)를 ISaveService 호출로 연결하고,
    /// 로드된 저장 데이터를 SaveDataLoadedEvent로 알리는 저장 전용 브릿지.
    /// </summary>
    public class SaveBridge : MonoBehaviour
    {
        [Inject] private ISaveService _saveService;

        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private List<SkinDataSO> skinDefinitions;

        private void Awake()
        {
            magnetGameChannel.AddListener<SkinUnlockedEvent>(OnSkinUnlocked);
            magnetGameChannel.AddListener<SkinChangedEvent>(OnSkinChanged);
            magnetGameChannel.AddListener<JTH.Scripts.Events.GameOverEvent>(OnGameOver);

            IReadOnlyCollection<string> validSkinIds = skinDefinitions
                .Where(skin => skin != null)
                .Select(skin => skin.SkinId)
                .ToList();
            _saveService.ValidateUnlockedSkins(validSkinIds);

            RaiseSaveDataLoaded();
        }

        private void OnDisable()
        {
            magnetGameChannel.RemoveListener<SkinUnlockedEvent>(OnSkinUnlocked);
            magnetGameChannel.RemoveListener<SkinChangedEvent>(OnSkinChanged);
            magnetGameChannel.RemoveListener<JTH.Scripts.Events.GameOverEvent>(OnGameOver);
        }

        private void OnSkinUnlocked(SkinUnlockedEvent evt)
        {
            _saveService.UnlockSkin(evt.SkinId);
        }

        private void OnSkinChanged(SkinChangedEvent evt)
        {
            _saveService.EquipSkin(evt.CurrentSkin.SkinId);
        }

        private void OnGameOver(JTH.Scripts.Events.GameOverEvent evt)
        {
            _saveService.SubmitScore(evt.FinalScore);
            _saveService.RecordGameOver();
        }

        private void RaiseSaveDataLoaded()
        {
            magnetGameChannel.RaiseEvent(SaveEvents.SaveDataLoadedEvent.Init(
                _saveService.BestScore,
                _saveService.UnlockedSkinIds,
                _saveService.EquippedSkinId,
                _saveService.TotalPlayTime,
                _saveService.MaxExplosionCombo,
                _saveService.GameOverCount));
        }
    }
}
