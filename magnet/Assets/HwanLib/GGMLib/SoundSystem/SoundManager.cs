using System.Collections.Generic;
using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
using HwanLib.Util;
using UnityEngine;

namespace HwanLib.GGMLib.SoundSystem
{
    public class SoundManager : LightSingleton<SoundManager>
    {
        [SerializeField] private PoolManagerSO poolManagerSO;
        [SerializeField] private PoolItemSO soundItemSO;

        [field: SerializeField] public EventChannelSO SoundEventChannel { get; private set; }

        // BGM이 아닌 루프 사운드(SFX 루프)만 LoopKey로 추적한다. BGM은 dict에 넣지 않는다.
        private readonly Dictionary<SoundClipSO, SoundPlayer> _soundPlayerDict = new();

        // BGM은 한 번에 하나 — 전용 슬롯. _currentBgmClip은 StopSoundEvent가 BGM을 가리켰는지 식별용.
        private SoundPlayer _currentBgm;
        private SoundClipSO _currentBgmClip;

        protected override void Initialize()
        {
            base.Initialize();
            
            SoundEventChannel.AddListener<PlaySoundEvent>(HandlePlaySoundEvent);
            SoundEventChannel.AddListener<StopSoundEvent>(HandleStopSoundEvent);

        }

        private void OnDestroy()
        {
            SoundEventChannel.RemoveListener<PlaySoundEvent>(HandlePlaySoundEvent);
            SoundEventChannel.RemoveListener<StopSoundEvent>(HandleStopSoundEvent);
        }

        private void HandlePlaySoundEvent(PlaySoundEvent evt)
        {
            SoundClipSO clip = evt.ClipData;
            Debug.Log(clip.IsBgm);
            // SO 자체가 없으면 아무것도 하지 않는다(보통 호출부에서 이미 걸러짐).
            if (clip == null) return;

            // BGM은 한 번에 하나 — 전용 슬롯에서 교체 관리(여기서 BGM 분기 끝).
            if (clip.IsBgm) { PlayBgm(clip); return; }

            // 여기는 BGM이 아닌 경우만 도달. 오디오 클립 없는 SFX는 재생만 건너뛴다(BGM 등 다른 소리는 그대로).
            if (clip.audioClip == null) return;

            PlayLoopOrOneShot(evt);
        }

        // BGM 재생: 기존 BGM을 끄고(항상 하나) 새 BGM으로 교체. 클립이 없으면 무음(정지만).
        private void PlayBgm(SoundClipSO clip)
        {
            StopBgm();
            if (clip.audioClip == null) return;

            _currentBgm = poolManagerSO.Pop<SoundPlayer>(soundItemSO);
            _currentBgm.transform.position = Vector3.zero; // BGM은 2D — 위치 무관.
            _currentBgm.PlaySound(clip);
            _currentBgm.OnSoundFinished += HandleBgmFinished; // 비루프 BGM(스팅어) 종료 대비.
            _currentBgmClip = clip;
        }

        // BGM이 아닌 사운드(SFX 일회성/루프) 재생. 루프는 LoopKey로 dict에 추적.
        private void PlayLoopOrOneShot(PlaySoundEvent evt)
        {
            SoundClipSO clip = evt.ClipData;

            SoundPlayer soundPlayer = poolManagerSO.Pop<SoundPlayer>(soundItemSO);
            soundPlayer.transform.position = evt.Position;
            soundPlayer.PlaySound(clip);
            soundPlayer.OnSoundFinished += HandleSoundFinish;

            if (evt.LoopKey != null && clip.isLoop)
            {
                StopLoop(evt.LoopKey); // 같은 키가 돌고 있으면 끊고 교체.
                _soundPlayerDict.Add(evt.LoopKey, soundPlayer);
            }
            else if (evt.LoopKey == null && clip.isLoop)
            {
                Debug.LogWarning($"[SoundManager] Loop 클립에는 LoopKey가 필요합니다. ({clip.name})");
            }
        }

        private void HandleSoundFinish(SoundPlayer soundPlayer)
        {
            soundPlayer.OnSoundFinished -= HandleSoundFinish;
            poolManagerSO.Push(soundPlayer);
        }

        // 비루프 BGM이 자연 종료되면 슬롯을 비운다.
        private void HandleBgmFinished(SoundPlayer soundPlayer)
        {
            soundPlayer.OnSoundFinished -= HandleBgmFinished;
            if (_currentBgm == soundPlayer)
            {
                _currentBgm = null;
                _currentBgmClip = null;
            }
            poolManagerSO.Push(soundPlayer);
        }

        private void HandleStopSoundEvent(StopSoundEvent evt)
        {
            // BGM을 가리킨 정지 요청이면 BGM 슬롯을, 아니면 루프 dict를 정리.
            if (evt.LoopKey != null && evt.LoopKey == _currentBgmClip)
                StopBgm();
            else
                StopLoop(evt.LoopKey);
        }

        // 현재 BGM을 끄고 슬롯을 비운다(없으면 no-op).
        private void StopBgm()
        {
            if (_currentBgm == null) return;
            _currentBgm.ForceStopSound();
            _currentBgm.OnSoundFinished -= HandleBgmFinished;
            poolManagerSO.Push(_currentBgm);
            _currentBgm = null;
            _currentBgmClip = null;
        }

        // 해당 루프 키로 돌고 있는 사운드를 끊고 풀에 반납한다(없으면 no-op).
        private void StopLoop(SoundClipSO loopKey)
        {
            if (loopKey == null) return;
            if (_soundPlayerDict.TryGetValue(loopKey, out SoundPlayer soundPlayer))
            {
                soundPlayer.ForceStopSound();
                soundPlayer.OnSoundFinished -= HandleSoundFinish;
                poolManagerSO.Push(soundPlayer);
                _soundPlayerDict.Remove(loopKey);
            }
        }
    }
}
