using UnityEngine;

public enum AudioTypes
{
    Sfx,
    Music
}

[CreateAssetMenu(fileName = "Sound Clip", menuName = "Bbang/SO/Sound/Sound Clip", order = 0)]
public class SoundClipSO : ScriptableObject
{
    public AudioTypes audioType;
    public AudioClip clip;
    public bool loop = false;
    public bool randomizePitch = false;

    [Range(0, 1f)]
    public float randomPitchModifier = 0.1f;
    [Range(0.1f, 2f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    [Header("3D Sound Settings")]
    public float minDistance = 1f;
    public float maxDistance = 30f;
}

