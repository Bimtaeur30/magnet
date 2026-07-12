// SoundPlayer.cs
using GameLib.ObjectPool.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : AbstractMonoPoolable
{
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    private AudioSource _audioSource;
    private CancellationTokenSource _cts;
    public event Action<SoundPlayer> OnSoundFinished;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void PlaySound(SoundClipSO clipData, Transform trans = null)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        if (clipData.audioType == AudioTypes.Sfx)
            _audioSource.outputAudioMixerGroup = sfxGroup;
        else if (clipData.audioType == AudioTypes.Music)
            _audioSource.outputAudioMixerGroup = bgmGroup;

        _audioSource.volume = clipData.volume;
        _audioSource.pitch = clipData.pitch;

        if (clipData.randomizePitch)
        {
            _audioSource.pitch += Random.Range(-clipData.randomPitchModifier, clipData.randomPitchModifier);
            _audioSource.pitch = Mathf.Clamp(_audioSource.pitch, 0.1f, 3f);
        }

        _audioSource.clip = clipData.clip;
        _audioSource.loop = clipData.loop;
        _audioSource.minDistance = clipData.minDistance;
        _audioSource.maxDistance = clipData.maxDistance;
        _audioSource.spatialBlend = (clipData.audioType == AudioTypes.Sfx && trans != null) ? 1f : 0f;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;

        if (!clipData.loop)
        {
            float time = _audioSource.clip.length + .2f;
            _ = DisableSoundTimer(time, _cts.Token);
        }

        _audioSource.Play();
    }

    public override void ResetItem()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        OnSoundFinished = null;
        _audioSource.Stop();
        _audioSource.clip = null;
        transform.SetParent(null);
        transform.position = Vector3.zero;
    }

    private async Task DisableSoundTimer(float time, CancellationToken token)
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(time, token);
            OnSoundFinished?.Invoke(this);
        }
        catch (OperationCanceledException)
        {
            // 사운드가 강제 중지되어 타이머가 취소된 경우, 정상 동작이므로 무시
        }
    }

    public void ForceStopSound()
    {
        _cts?.Cancel();
        _audioSource.Stop();
    }
}