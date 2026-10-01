using UnityEditor;
using UnityEngine;

public sealed class AudioLayerPreviewWindow : EditorWindow
{
    private const string LibraryPath = "Assets/Resources/Audio/GameAudioLibrary.asset";
    private const double StartDelay = 0.1d;

    private GameAudioLibrary library;
    private GameObject previewRoot;
    private AudioSource marsSource;
    private AudioSource jupiterSource;
    private AudioSource saturnSource;
    private bool jupiterEnabled;
    private bool saturnEnabled;
    private bool previewing;
    private double previewStartedAt;
    private string lastError;

    [MenuItem("Tools/Protectors/오디오 레이어 미리듣기")]
    private static void OpenWindow()
    {
        AudioLayerPreviewWindow window = GetWindow<AudioLayerPreviewWindow>();
        window.titleContent = new GUIContent("오디오 레이어");
        window.minSize = new Vector2(440f, 320f);
        window.Show();
    }

    private void OnEnable()
    {
        library = AssetDatabase.LoadAssetAtPath<GameAudioLibrary>(LibraryPath);
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
        DestroyPreviewSources();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("실제 Stem 즉시 비교", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "GameAudioLibrary에 현재 연결된 세 AudioClip을 Editor 전용 AudioSource로 " +
            "동시에 재생합니다. 변환 파일을 만들지 않으며 플레이모드도 필요하지 않습니다.",
            MessageType.Info);

        if (library == null)
        {
            EditorGUILayout.HelpBox($"오디오 라이브러리를 찾지 못했습니다.\n{LibraryPath}", MessageType.Error);
            if (GUILayout.Button("다시 불러오기")) ReloadLibrary();
            return;
        }

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("Mars 기본", library.stageBaseMusic, typeof(AudioClip), false);
            EditorGUILayout.ObjectField("Jupiter", library.jupiterMusicLayer, typeof(AudioClip), false);
            EditorGUILayout.ObjectField("Saturn", library.saturnMusicLayer, typeof(AudioClip), false);
        }

        EditorGUILayout.Space(8f);
        EditorGUI.BeginChangeCheck();
        jupiterEnabled = EditorGUILayout.ToggleLeft("Jupiter ON", jupiterEnabled);
        saturnEnabled = EditorGUILayout.ToggleLeft("Saturn ON", saturnEnabled);
        if (EditorGUI.EndChangeCheck()) ApplyLayerVolumes();

        EditorGUILayout.Space(6f);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("MARS", GUILayout.Height(38f))) ApplyPreset(false, false);
            if (GUILayout.Button("MARS + JUPITER", GUILayout.Height(38f))) ApplyPreset(true, false);
            if (GUILayout.Button("전체 3레이어", GUILayout.Height(38f))) ApplyPreset(true, true);
        }

        EditorGUILayout.Space(6f);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = new Color(0.45f, 0.9f, 0.65f);
            if (GUILayout.Button("▶ 동기 재생", GUILayout.Height(38f))) StartPreview();
            GUI.backgroundColor = new Color(1f, 0.55f, 0.5f);
            if (GUILayout.Button("■ 정지", GUILayout.Height(38f))) StopPreview();
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.Space(8f);
        string description = BuildDescription();
        string time = previewing ? $"  {EditorApplication.timeSinceStartup - previewStartedAt:0.0}초" : string.Empty;
        EditorGUILayout.LabelField($"현재: {(previewing ? description : "정지됨")}{time}");
        if (!string.IsNullOrEmpty(lastError))
            EditorGUILayout.HelpBox(lastError, MessageType.Error);
    }

    private void ReloadLibrary()
    {
        library = AssetDatabase.LoadAssetAtPath<GameAudioLibrary>(LibraryPath);
        if (previewing) StartPreview();
    }

    private void ApplyPreset(bool jupiter, bool saturn)
    {
        jupiterEnabled = jupiter;
        saturnEnabled = saturn;
        if (!previewing) StartPreview();
        else ApplyLayerVolumes();
    }

    private void StartPreview()
    {
        lastError = null;
        StopPreview();

        if (library == null || library.stageBaseMusic == null)
        {
            lastError = "GameAudioLibrary의 Stage Base Music이 비어 있습니다.";
            return;
        }

        CreatePreviewSources();
        marsSource.clip = library.stageBaseMusic;
        jupiterSource.clip = library.jupiterMusicLayer;
        saturnSource.clip = library.saturnMusicLayer;
        ApplyLayerVolumes();

        double scheduledTime = AudioSettings.dspTime + StartDelay;
        marsSource.PlayScheduled(scheduledTime);
        if (jupiterSource.clip != null) jupiterSource.PlayScheduled(scheduledTime);
        if (saturnSource.clip != null) saturnSource.PlayScheduled(scheduledTime);
        previewing = true;
        previewStartedAt = EditorApplication.timeSinceStartup + StartDelay;
        EditorApplication.QueuePlayerLoopUpdate();
        Repaint();
    }

    private void CreatePreviewSources()
    {
        if (previewRoot != null) return;
        previewRoot = EditorUtility.CreateGameObjectWithHideFlags(
            "Protectors Audio Layer Preview",
            HideFlags.HideAndDontSave,
            typeof(AudioSource));
        marsSource = previewRoot.GetComponent<AudioSource>();
        jupiterSource = previewRoot.AddComponent<AudioSource>();
        saturnSource = previewRoot.AddComponent<AudioSource>();
        ConfigureSource(marsSource);
        ConfigureSource(jupiterSource);
        ConfigureSource(saturnSource);
    }

    private static void ConfigureSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.hideFlags = HideFlags.HideAndDontSave;
    }

    private void ApplyLayerVolumes()
    {
        if (marsSource == null) return;
        marsSource.volume = 1f;
        jupiterSource.volume = jupiterEnabled ? 1f : 0f;
        saturnSource.volume = saturnEnabled ? 1f : 0f;
        Repaint();
    }

    private void StopPreview()
    {
        if (marsSource != null) marsSource.Stop();
        if (jupiterSource != null) jupiterSource.Stop();
        if (saturnSource != null) saturnSource.Stop();
        previewing = false;
        Repaint();
    }

    private void DestroyPreviewSources()
    {
        StopPreview();
        if (previewRoot != null) DestroyImmediate(previewRoot);
        previewRoot = null;
        marsSource = null;
        jupiterSource = null;
        saturnSource = null;
    }

    private string BuildDescription()
    {
        string result = "MARS";
        if (jupiterEnabled) result += " + JUPITER";
        if (saturnEnabled) result += " + SATURN";
        return result;
    }

    private void EditorUpdate()
    {
        if (!previewing) return;
        EditorApplication.QueuePlayerLoopUpdate();
        Repaint();
    }
}
