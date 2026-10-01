using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI experimentLabDataText;
    [SerializeField] private HomeStageSelectionUI stageSelection;
    [SerializeField] private Transform experimentLabArea;

    private readonly Dictionary<PaletteColorApplicator, PaletteColor> experimentLabBaseColors =
        new Dictionary<PaletteColorApplicator, PaletteColor>();

    private void Awake()
    {
        CacheExperimentLabColors();
    }

    private void Start()
    {
        if (stageSelection != null)
        {
            stageSelection.Refresh();
        }
        else
        {
            RefreshHighScore();
        }
        RefreshEndlessLabData();
        RefreshExperimentLabColors();
    }

    private void RefreshHighScore()
    {
        if (highScoreText == null || GameManager.Instance == null) return;

        GameManager gameManager = GameManager.Instance;
        int completionPercent = gameManager.highScore > 0
            ? 100
            : 0;
        highScoreText.text = $"최고 달성률 {completionPercent}%";
    }

    public void SetupStage()
    {
        if (stageSelection != null && !stageSelection.CanStartSelectedStage) return;

        string sceneName = stageSelection != null
            ? stageSelection.SelectedStage.SceneName
            : "Stage";
        StartStage(GameManager.PlayMode.Stage, sceneName);
    }

    public void SetupEndlessLab()
    {
        RefreshEndlessLabData();
        StartStage(GameManager.PlayMode.EndlessLab, "Stage");
    }

    private void RefreshEndlessLabData()
    {
        if (experimentLabDataText == null || GameManager.Instance == null) return;

        GameManager gameManager = GameManager.Instance;
        experimentLabDataText.text = $"최고 점수 {gameManager.endlessHighScore:N0}\n최고 웨이브 {gameManager.endlessBestWave}";
    }

    private void CacheExperimentLabColors()
    {
        if (experimentLabArea == null) return;

        foreach (PaletteColorApplicator colorApplicator in
                 experimentLabArea.GetComponentsInChildren<PaletteColorApplicator>(true))
        {
            experimentLabBaseColors[colorApplicator] = colorApplicator.ColorRole;
        }
    }

    private void RefreshExperimentLabColors()
    {
        if (experimentLabBaseColors.Count == 0) CacheExperimentLabColors();

        bool isRegistered = GameManager.Instance != null && GameManager.Instance.HasClearedStage01;
        foreach (KeyValuePair<PaletteColorApplicator, PaletteColor> entry in experimentLabBaseColors)
        {
            if (entry.Key == null) continue;

            entry.Key.ColorRole = PaletteColorRoleUtility.GetRegistrationColor(
                entry.Value,
                isRegistered);
        }
    }

    private void StartStage(GameManager.PlayMode playMode, string sceneName)
    {
        // 공개 빌드는 별도 해금이나 장비 효과 없이 포격 하나로 시작한다.
        GameManager.Instance.SetPlayMode(playMode);
        GameManager.Instance.InitializeStage();
        SceneManager.LoadScene(sceneName);
    }
}
