using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoSingleton<SoundManager>
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Groups")]
    [SerializeField] private AudioMixerGroup SFX;
    [SerializeField] private AudioMixerGroup BGM;

    [Header("Pool")]
    [SerializeField] private PoolManagerSO poolManager;
    [SerializeField] private PoolItemSO soundItem;

    [field: SerializeField]
    public EventChannelSO SoundChannel { get; private set; }

    private readonly Dictionary<int, SoundPlayer> _soundPlayerDict = new();

    #region Volume

    public float MasterVolume { get; private set; } = 100f;
    public float BGMVolume { get; private set; } = 100f;
    public float SFXVolume { get; private set; } = 100f;

    private const string MASTER_VOLUME_KEY = "MASTER_VOLUME";
    private const string BGM_VOLUME_KEY = "BGM_VOLUME";
    private const string SFX_VOLUME_KEY = "SFX_VOLUME";

    #endregion

    protected override void Awake()
    {
        base.Awake();

        SoundChannel.AddListener<PlaySoundEvent>(HandlePlaySoundEvent);
        SoundChannel.AddListener<StopSoundEvent>(HandleStopSoundEvent);

        LoadVolume();
    }

    private void OnDestroy()
    {
        SoundChannel.RemoveListener<PlaySoundEvent>(HandlePlaySoundEvent);
        SoundChannel.RemoveListener<StopSoundEvent>(HandleStopSoundEvent);
    }

    #region Volume

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp(value, 0f, 100f);

        mixer.SetFloat("MasterVolume", VolumeToDB(MasterVolume));

        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, MasterVolume);
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float value)
    {
        BGMVolume = Mathf.Clamp(value, 0f, 100f);

        mixer.SetFloat("BGMVolume", VolumeToDB(BGMVolume));

        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGMVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp(value, 0f, 100f);

        mixer.SetFloat("SFXVolume", VolumeToDB(SFXVolume));

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SFXVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolume()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 100));
        SetBGMVolume(PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 100));
        SetSFXVolume(PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 100));
    }

    private float VolumeToDB(float volume)
    {
        if (volume <= 0f)
            return -80f;

        return Mathf.Log10(volume / 100f) * 20f;
    }

    #endregion

    private void HandlePlaySoundEvent(PlaySoundEvent evt)
    {
        SoundPlayer player = poolManager.Pop<SoundPlayer>(soundItem);

        if (evt.Trans != null)
        {
            player.transform.SetParent(evt.Trans);
            player.transform.position = evt.Trans.position;
        }

        player.PlaySound(evt.ClipData, evt.Trans);
        player.OnSoundFinished += HandleSoundFinish;

        if (evt.ChannelNumber > 0 && evt.ClipData.loop)
        {
            if (_soundPlayerDict.TryGetValue(evt.ChannelNumber, out SoundPlayer beforePlayer))
            {
                beforePlayer.ForceStopSound();
                beforePlayer.OnSoundFinished -= HandleSoundFinish;

                poolManager.Push(beforePlayer);
                _soundPlayerDict.Remove(evt.ChannelNumber);
            }

            _soundPlayerDict.Add(evt.ChannelNumber, player);
        }
        else if (evt.ChannelNumber <= 0 && evt.ClipData.loop)
        {
            Debug.LogWarning(
                $"Channel must be greater than 0 when loop is enabled : {evt.ClipData.name}");
        }
    }

    private void HandleSoundFinish(SoundPlayer player)
    {
        player.OnSoundFinished -= HandleSoundFinish;
        poolManager.Push(player);
    }

    private void HandleStopSoundEvent(StopSoundEvent evt)
    {
        if (_soundPlayerDict.TryGetValue(evt.ChannelNumber, out SoundPlayer beforePlayer))
        {
            beforePlayer.ForceStopSound();
            beforePlayer.OnSoundFinished -= HandleSoundFinish;

            poolManager.Push(beforePlayer);
            _soundPlayerDict.Remove(evt.ChannelNumber);
        }
    }
}