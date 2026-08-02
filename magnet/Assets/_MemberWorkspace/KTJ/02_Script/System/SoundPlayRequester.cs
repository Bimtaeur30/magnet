using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using UnityEngine;

/// <summary>
/// SoundSystem 이벤트를 통해 인스펙터에 지정된 사운드를 재생하는 컴포넌트입니다.
/// UnityEvent, Animation Event 또는 다른 스크립트에서 public 메서드를 호출해 사용합니다.
/// </summary>
public sealed class SoundPlayRequester : MonoBehaviour
{
    [Header("Sound System")]
    [SerializeField] private EventChannelSO soundEventChannel;
    [SerializeField] private SoundClipSO soundClip;

    [Header("Playback")]
    [Tooltip("활성화하면 씬 시작 시 자동으로 사운드를 재생합니다.")]
    [SerializeField] private bool playOnAwake;

    [Tooltip("활성화하면 이 GameObject의 월드 위치에서 3D 사운드를 재생합니다.")]
    [SerializeField] private bool playAtTransformPosition;

    private void Start()
    {
        if (playOnAwake)
            Play();
    }

    /// <summary>지정된 사운드를 재생합니다.</summary>
    public void Play()
    {
        if (!CanRaiseSoundEvent())
            return;

        PlaySoundEvent playEvent = playAtTransformPosition
            ? SoundSystemEvents.PlaySoundEvent.Init(transform.position, soundClip, GetLoopKey())
            : SoundSystemEvents.PlaySoundEvent.Init(soundClip, GetLoopKey());

        soundEventChannel.RaiseEvent(playEvent);
    }

    /// <summary>이 GameObject의 현재 월드 위치에서 사운드를 재생합니다.</summary>
    public void PlayAtCurrentPosition()
    {
        if (!CanRaiseSoundEvent())
            return;

        soundEventChannel.RaiseEvent(
            SoundSystemEvents.PlaySoundEvent.Init(transform.position, soundClip, GetLoopKey()));
    }

    /// <summary>재생 중인 루프 사운드 또는 이 컴포넌트가 시작한 BGM을 정지합니다.</summary>
    public void Stop()
    {
        if (!CanRaiseSoundEvent())
            return;

        if (!soundClip.isLoop && !soundClip.IsBgm)
        {
            Debug.LogWarning($"[SoundPlayRequester] 일회성 SFX는 중간에 정지할 수 없습니다. ({soundClip.name})", this);
            return;
        }

        soundEventChannel.RaiseEvent(SoundSystemEvents.StopSoundEvent.Init(soundClip));
    }

    private SoundClipSO GetLoopKey()
    {
        return soundClip.isLoop || soundClip.IsBgm ? soundClip : null;
    }

    private bool CanRaiseSoundEvent()
    {
        if (soundEventChannel == null)
        {
            Debug.LogError("[SoundPlayRequester] Sound Event Channel이 지정되지 않았습니다.", this);
            return false;
        }

        if (soundClip == null)
        {
            Debug.LogError("[SoundPlayRequester] Sound Clip이 지정되지 않았습니다.", this);
            return false;
        }

        return true;
    }
}
