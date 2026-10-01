using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public sealed class AudioManager : MonoBehaviour
{
    public const string MasterVolumeKey = "Audio.MasterVolume";
    public const string MusicVolumeKey = "Audio.MusicVolume";
    public const string SfxVolumeKey = "Audio.SfxVolume";

    private const string LibraryResourcePath = "Audio/GameAudioLibrary";
    private const int SfxVoiceCount = 8;
    private const double ScheduledLeadTime = 0.05d;

    public static AudioManager Instance { get; private set; }
    public bool IsJupiterLayerEnabled => jupiterEnabled;
    public bool IsSaturnLayerEnabled => saturnEnabled;

    private readonly Dictionary<GameSfx, SoundEffectDefinition> effects =
        new Dictionary<GameSfx, SoundEffectDefinition>();
    private readonly Dictionary<GameSfx, float> lastPlayTimes =
        new Dictionary<GameSfx, float>();
    private readonly List<AudioSource> sfxSources = new List<AudioSource>();

    private GameAudioLibrary library;
    private AudioSource baseMusicSource;
    private AudioSource jupiterMusicSource;
    private AudioSource saturnMusicSource;
    private Coroutine musicRoutine;
    private Coroutine jupiterRoutine;
    private Coroutine saturnRoutine;
    private int nextSfxSource;
    private bool jupiterEnabled;
    private bool saturnEnabled;
    private float musicVolume;
    private float sfxVolume;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject audioObject = new GameObject("AudioManager");
        audioObject.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        library = Resources.Load<GameAudioLibrary>(LibraryResourcePath);
        CreateSources();
        BuildEffectLookup();
        ApplySavedVolumes();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    private void CreateSources()
    {
        baseMusicSource = CreateSource("Music - Base", true);
        jupiterMusicSource = CreateSource("Music - Jupiter", true);
        saturnMusicSource = CreateSource("Music - Saturn", true);

        for (int i = 0; i < SfxVoiceCount; i++)
            sfxSources.Add(CreateSource($"SFX Voice {i + 1}", false));
    }

    private AudioSource CreateSource(string sourceName, bool loop)
    {
        GameObject sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform, false);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        return source;
    }

    private void BuildEffectLookup()
    {
        effects.Clear();
        if (library == null || library.soundEffects == null) return;
        foreach (SoundEffectDefinition effect in library.soundEffects)
            if (effect != null) effects[effect.id] = effect;
    }

    private void ApplySavedVolumes()
    {
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name, false);
        StartCoroutine(AddButtonSoundsAfterSceneLoad());
    }

    private IEnumerator AddButtonSoundsAfterSceneLoad()
    {
        yield return null;
        foreach (Button button in FindObjectsOfType<Button>(true))
            button.onClick.AddListener(() => PlaySfx(GameSfx.UiConfirm));
    }

    public void PlayMusicForScene(string sceneName, bool immediate = false)
    {
        if (library == null) return;
        bool isStage = sceneName == "Stage";
        AudioClip targetBase = isStage ? library.stageBaseMusic : library.homeMusic;
        AudioClip targetJupiter = isStage ? library.jupiterMusicLayer : null;
        AudioClip targetSaturn = isStage ? library.saturnMusicLayer : null;

        if (musicRoutine != null) StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(SwitchMusic(
            targetBase, targetJupiter, targetSaturn,
            immediate ? 0f : library.musicFadeSeconds));
    }

    private IEnumerator SwitchMusic(
        AudioClip baseClip, AudioClip jupiterClip, AudioClip saturnClip, float duration)
    {
        yield return FadeAllMusic(0f, duration);
        StopMusicSources();

        baseMusicSource.clip = baseClip;
        jupiterMusicSource.clip = jupiterClip;
        saturnMusicSource.clip = saturnClip;
        baseMusicSource.volume = baseClip != null ? musicVolume : 0f;
        jupiterMusicSource.volume = jupiterClip != null && jupiterEnabled ? musicVolume : 0f;
        saturnMusicSource.volume = saturnClip != null && saturnEnabled ? musicVolume : 0f;

        double startTime = AudioSettings.dspTime + ScheduledLeadTime;
        PlayScheduled(baseMusicSource, startTime);
        PlayScheduled(jupiterMusicSource, startTime);
        PlayScheduled(saturnMusicSource, startTime);

        float baseTarget = baseClip != null ? musicVolume : 0f;
        float jupiterTarget = jupiterClip != null && jupiterEnabled ? musicVolume : 0f;
        float saturnTarget = saturnClip != null && saturnEnabled ? musicVolume : 0f;
        baseMusicSource.volume = duration > 0f ? 0f : baseTarget;
        jupiterMusicSource.volume = duration > 0f ? 0f : jupiterTarget;
        saturnMusicSource.volume = duration > 0f ? 0f : saturnTarget;
        yield return FadeMusic(baseTarget, jupiterTarget, saturnTarget, duration);
        musicRoutine = null;
    }

    private static void PlayScheduled(AudioSource source, double startTime)
    {
        if (source.clip != null) source.PlayScheduled(startTime);
    }

    private IEnumerator FadeAllMusic(float target, float duration)
    {
        yield return FadeMusic(target, target, target, duration);
    }

    private IEnumerator FadeMusic(float baseTarget, float jupiterTarget, float saturnTarget, float duration)
    {
        float baseStart = baseMusicSource.volume;
        float jupiterStart = jupiterMusicSource.volume;
        float saturnStart = saturnMusicSource.volume;
        if (duration <= 0f)
        {
            baseMusicSource.volume = baseTarget;
            jupiterMusicSource.volume = jupiterTarget;
            saturnMusicSource.volume = saturnTarget;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            baseMusicSource.volume = Mathf.Lerp(baseStart, baseTarget, t);
            jupiterMusicSource.volume = Mathf.Lerp(jupiterStart, jupiterTarget, t);
            saturnMusicSource.volume = Mathf.Lerp(saturnStart, saturnTarget, t);
            yield return null;
        }
    }

    private void StopMusicSources()
    {
        baseMusicSource.Stop();
        jupiterMusicSource.Stop();
        saturnMusicSource.Stop();
    }

    public void SetCharacterLayer(string characterName, bool enabled)
    {
        if (string.IsNullOrWhiteSpace(characterName)) return;
        if (characterName.Equals("Jupiter", System.StringComparison.OrdinalIgnoreCase))
            SetJupiterLayer(enabled);
        else if (characterName.Equals("Saturn", System.StringComparison.OrdinalIgnoreCase))
            SetSaturnLayer(enabled);
    }

    public void SetJupiterLayer(bool enabled)
    {
        jupiterEnabled = enabled;
        if (jupiterRoutine != null) StopCoroutine(jupiterRoutine);
        float target = enabled && jupiterMusicSource.clip != null ? musicVolume : 0f;
        jupiterRoutine = StartCoroutine(FadeSingleSource(jupiterMusicSource, target));
    }

    public void SetSaturnLayer(bool enabled)
    {
        saturnEnabled = enabled;
        if (saturnRoutine != null) StopCoroutine(saturnRoutine);
        float target = enabled && saturnMusicSource.clip != null ? musicVolume : 0f;
        saturnRoutine = StartCoroutine(FadeSingleSource(saturnMusicSource, target));
    }

    private IEnumerator FadeSingleSource(AudioSource source, float target)
    {
        float duration = library != null ? library.layerFadeSeconds : 0f;
        float start = source.volume;
        if (duration <= 0f)
        {
            source.volume = target;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        source.volume = target;
    }

    public void PlaySfx(GameSfx id)
    {
        if (!effects.TryGetValue(id, out SoundEffectDefinition effect) ||
            effect.clips == null || effect.clips.Length == 0) return;

        float now = Time.unscaledTime;
        if (lastPlayTimes.TryGetValue(id, out float lastTime) && now - lastTime < effect.minimumInterval) return;

        AudioClip clip = effect.clips[Random.Range(0, effect.clips.Length)];
        if (clip == null) return;
        lastPlayTimes[id] = now;

        AudioSource source = GetSfxSource();
        source.clip = clip;
        source.volume = Mathf.Clamp01(effect.volume) * sfxVolume;
        source.pitch = 1f + Random.Range(-effect.pitchVariation, effect.pitchVariation);
        source.Play();
    }

    private AudioSource GetSfxSource()
    {
        foreach (AudioSource source in sfxSources)
            if (!source.isPlaying) return source;
        AudioSource fallback = sfxSources[nextSfxSource];
        nextSfxSource = (nextSfxSource + 1) % sfxSources.Count;
        return fallback;
    }

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;
        SaveVolume(MasterVolumeKey, value);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        baseMusicSource.volume = baseMusicSource.clip != null ? musicVolume : 0f;
        jupiterMusicSource.volume = jupiterMusicSource.clip != null && jupiterEnabled ? musicVolume : 0f;
        saturnMusicSource.volume = saturnMusicSource.clip != null && saturnEnabled ? musicVolume : 0f;
        SaveVolume(MusicVolumeKey, musicVolume);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveVolume(SfxVolumeKey, sfxVolume);
    }

    private static void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }
}
