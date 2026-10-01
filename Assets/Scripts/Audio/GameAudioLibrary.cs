using System;
using UnityEngine;

public enum GameSfx
{
    UiConfirm,
    AttackFire,
    ProjectileHit,
    EnemyDefeated,
    StageClear,
    GameOver
}

[CreateAssetMenu(fileName = "GameAudioLibrary", menuName = "Protectors/Audio Library")]
public sealed class GameAudioLibrary : ScriptableObject
{
    [Header("Music")]
    [Tooltip("Home scene loop.")]
    public AudioClip homeMusic;
    [Tooltip("Mars/base loop. All stage stems must have exactly the same length and start point.")]
    public AudioClip stageBaseMusic;
    [Tooltip("Jupiter stem, exported in sync with the stage base loop.")]
    public AudioClip jupiterMusicLayer;
    [Tooltip("Saturn stem, exported in sync with the stage base loop.")]
    public AudioClip saturnMusicLayer;
    [Min(0f)] public float musicFadeSeconds = 0.5f;
    [Min(0f)] public float layerFadeSeconds = 0.35f;

    [Header("Sound effects")]
    public SoundEffectDefinition[] soundEffects = Array.Empty<SoundEffectDefinition>();
}

[Serializable]
public sealed class SoundEffectDefinition
{
    public GameSfx id;
    public AudioClip[] clips = Array.Empty<AudioClip>();
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 0.25f)] public float pitchVariation = 0.04f;
    [Min(0f), Tooltip("Prevents rapid events from producing an excessively loud stack.")]
    public float minimumInterval = 0.03f;
}
