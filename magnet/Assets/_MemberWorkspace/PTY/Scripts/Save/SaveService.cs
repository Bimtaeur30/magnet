using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameLib.EventChannelSystem;
using PTY.Scripts.Events;

namespace PTY.Scripts.Save
{
    /// <summary>
    /// 로컬 저장 오케스트레이션. 클라우드 로그인 미구현 상태라 로컬 저장소만 사용한다.
    /// (클라우드 로그인 구현 시 ICloudSaveProvider를 다시 주입해 초기 병합/비동기 push를 추가할 자리)
    /// </summary>
    public sealed class SaveService : ISaveService
    {
        private readonly ILocalSaveRepository _localRepository;
        private readonly EventChannelSO _magnetGameChannel;
        private GameSaveData _data;

        public SaveService(ILocalSaveRepository localRepository, EventChannelSO magnetGameChannel)
        {
            _localRepository = localRepository;
            _magnetGameChannel = magnetGameChannel;
            _data = _localRepository.Load() ?? new GameSaveData();
        }

        public int BestScore => _data.BestScore;
        public IReadOnlyList<string> UnlockedSkinIds => _data.UnlockedSkinIds;
        public string EquippedSkinId => _data.EquippedSkinId;
        public float TotalPlayTime => _data.TotalPlayTime;
        public int MaxExplosionCombo => _data.MaxExplosionCombo;
        public int GameOverCount => _data.GameOverCount;

        public UniTask InitializeAsync()
        {
            return UniTask.CompletedTask;
        }

        public void SubmitScore(int score)
        {
            if (score <= _data.BestScore)
            {
                return;
            }

            int previousBestScore = _data.BestScore;
            _data.BestScore = score;
            Save();
            _magnetGameChannel.RaiseEvent(SaveEvents.BestScoreUpdatedEvent.Init(score, previousBestScore));
        }

        public void UnlockSkin(string skinId)
        {
            if (_data.UnlockedSkinIds.Contains(skinId))
            {
                return;
            }

            _data.UnlockedSkinIds.Add(skinId);
            Save();
        }

        public void EquipSkin(string skinId)
        {
            _data.EquippedSkinId = skinId;
            Save();
        }

        public void AddPlayTime(float seconds)
        {
            _data.TotalPlayTime += seconds;
            Save();
        }

        public void SubmitExplosionCombo(int comboCount)
        {
            if (comboCount <= _data.MaxExplosionCombo)
            {
                return;
            }

            _data.MaxExplosionCombo = comboCount;
            Save();
        }

        public void RecordGameOver()
        {
            _data.GameOverCount++;
            Save();
        }

        public void ValidateUnlockedSkins(IReadOnlyCollection<string> validSkinIds)
        {
            int removedCount = _data.UnlockedSkinIds.RemoveAll(skinId => !validSkinIds.Contains(skinId));

            bool equippedSkinInvalid = !string.IsNullOrEmpty(_data.EquippedSkinId) && !validSkinIds.Contains(_data.EquippedSkinId);
            if (equippedSkinInvalid)
            {
                _data.EquippedSkinId = null;
            }

            if (removedCount > 0 || equippedSkinInvalid)
            {
                Save();
            }
        }

        private void Save()
        {
            _localRepository.Save(_data);
            _magnetGameChannel.RaiseEvent(SaveEvents.SaveSyncCompletedEvent);
        }
    }
}
