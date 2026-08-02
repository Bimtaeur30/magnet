using System;
using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using UnityEngine;

/// <summary>
/// Animation Event에서 문자열 키를 받아 해당 SoundClipSO를 재생합니다.
/// Animator와 같은 GameObject에 배치해 사용합니다.
/// </summary>
public sealed class AnimationSoundPlayer : MonoBehaviour
{
    [Serializable]
    private struct SoundEntry
    {
        [Tooltip("Animation Event의 문자열 파라미터와 일치해야 합니다.")]
        public string key;

        public SoundClipSO clip;

        [Tooltip("활성화하면 이 GameObject의 현재 월드 위치에서 재생합니다.")]
        public bool playAtTransformPosition;
    }

    [Header("Sound System")]
    [SerializeField] private EventChannelSO soundChannel;

    [Header("Animation Sounds")]
    [SerializeField] private SoundEntry[] sounds;

    /// <summary>Animation Event의 문자열 파라미터와 같은 키의 사운드를 재생합니다.</summary>
    public void PlaySound(string key)
    {
        if (!CanRaiseEvent())
            return;

        if (!TryFindSound(key, out SoundEntry entry))
        {
            Debug.LogWarning($"[AnimationSoundPlayer] '{key}' 키에 해당하는 사운드가 없습니다.", this);
            return;
        }

        SoundClipSO loopKey = GetLoopKey(entry.clip);
        PlaySoundEvent playEvent = entry.playAtTransformPosition
            ? SoundSystemEvents.PlaySoundEvent.Init(transform.position, entry.clip, loopKey)
            : SoundSystemEvents.PlaySoundEvent.Init(entry.clip, loopKey);

        soundChannel.RaiseEvent(playEvent);
    }

    /// <summary>Animation Event의 문자열 파라미터와 같은 키의 루프/BGM을 정지합니다.</summary>
    public void StopSound(string key)
    {
        if (!CanRaiseEvent())
            return;

        if (!TryFindSound(key, out SoundEntry entry))
        {
            Debug.LogWarning($"[AnimationSoundPlayer] '{key}' 키에 해당하는 사운드가 없습니다.", this);
            return;
        }

        if (!entry.clip.isLoop && !entry.clip.IsBgm)
        {
            Debug.LogWarning($"[AnimationSoundPlayer] 일회성 SFX는 중간에 정지할 수 없습니다. ({entry.clip.name})", this);
            return;
        }

        soundChannel.RaiseEvent(SoundSystemEvents.StopSoundEvent.Init(entry.clip));
    }

    private bool TryFindSound(string key, out SoundEntry result)
    {
        if (sounds != null)
        {
            foreach (SoundEntry entry in sounds)
            {
                if (entry.clip != null && string.Equals(entry.key, key, StringComparison.Ordinal))
                {
                    result = entry;
                    return true;
                }
            }
        }

        result = default;
        return false;
    }

    private bool CanRaiseEvent()
    {
        if (soundChannel != null)
            return true;

        Debug.LogError("[AnimationSoundPlayer] Sound Channel이 지정되지 않았습니다.", this);
        return false;
    }

    private static SoundClipSO GetLoopKey(SoundClipSO clip)
    {
        return clip.isLoop || clip.IsBgm ? clip : null;
    }
}
