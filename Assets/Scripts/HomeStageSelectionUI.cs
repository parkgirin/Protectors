using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HomeStageSelectionUI : MonoBehaviour
{
    [SerializeField] private HomeStageAsset[] stages;
    [SerializeField] private TMP_Text stageNumberText;
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text actionLabelText;
    [SerializeField] private Image stageImage;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Transform stageSelectionArea;
    [SerializeField] private PaletteColorApplicator stagePanelBackground;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button startButton;

    private int currentIndex;
    private readonly Dictionary<PaletteColorApplicator, PaletteColor> stageSelectionBaseColors =
        new Dictionary<PaletteColorApplicator, PaletteColor>();

    public HomeStageAsset SelectedStage =>
        stages != null && currentIndex >= 0 && currentIndex < stages.Length
            ? stages[currentIndex]
            : null;

    public bool CanStartSelectedStage => SelectedStage != null && SelectedStage.Implemented;

    private void Awake()
    {
        CacheStageSelectionColors();

        if (previousButton != null) previousButton.onClick.AddListener(ShowPreviousStage);
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextStage);
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        if (previousButton != null) previousButton.onClick.RemoveListener(ShowPreviousStage);
        if (nextButton != null) nextButton.onClick.RemoveListener(ShowNextStage);
    }

    public void ShowPreviousStage()
    {
        if (currentIndex <= 0) return;

        currentIndex--;
        Refresh();
    }

    public void ShowNextStage()
    {
        if (stages == null || currentIndex >= stages.Length - 1) return;

        currentIndex++;
        Refresh();
    }

    public void Refresh()
    {
        HomeStageAsset stage = SelectedStage;
        if (stage == null) return;

        if (stageNumberText != null) stageNumberText.text = stage.StageNumber;
        if (stageNameText != null) stageNameText.text = stage.StageName;
        if (infoText != null) infoText.text = stage.InfoText;
        if (actionLabelText != null) actionLabelText.text = stage.ActionLabel;

        if (stageImage != null && stage.StageImage != null)
            stageImage.sprite = stage.StageImage;

        RefreshStageSelectionColors(stage.Implemented);

        if (progressText != null)
        {
            int completionPercent = GameManager.Instance != null && GameManager.Instance.highScore > 0
                ? 100
                : 0;
            progressText.text = stage.Implemented
                ? $"최고 달성률 {completionPercent}%"
                : "미등록";
        }

        if (previousButton != null) previousButton.interactable = currentIndex > 0;
        if (nextButton != null) nextButton.interactable = currentIndex < stages.Length - 1;
        if (startButton != null) startButton.interactable = stage.Implemented;
    }

    private void CacheStageSelectionColors()
    {
        if (stageSelectionArea != null)
        {
            foreach (PaletteColorApplicator colorApplicator in
                     stageSelectionArea.GetComponentsInChildren<PaletteColorApplicator>(true))
            {
                stageSelectionBaseColors[colorApplicator] = colorApplicator.ColorRole;
            }
        }

        if (stagePanelBackground != null)
            stageSelectionBaseColors[stagePanelBackground] = stagePanelBackground.ColorRole;
    }

    private void RefreshStageSelectionColors(bool stageImplemented)
    {
        if (stageSelectionBaseColors.Count == 0) CacheStageSelectionColors();

        foreach (KeyValuePair<PaletteColorApplicator, PaletteColor> entry in stageSelectionBaseColors)
        {
            if (entry.Key == null) continue;

            entry.Key.ColorRole = PaletteColorRoleUtility.GetRegistrationColor(
                entry.Value,
                stageImplemented);
        }
    }
}
