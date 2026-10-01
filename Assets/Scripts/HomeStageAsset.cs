using UnityEngine;

[CreateAssetMenu(fileName = "HomeStage", menuName = "Protectors/Home Stage")]
public sealed class HomeStageAsset : ScriptableObject
{
    [SerializeField] private string stageNumber = "STAGE 01";
    [SerializeField] private string stageName;
    [SerializeField] private Sprite stageImage;
    [SerializeField, TextArea] private string infoText;
    [SerializeField] private string actionLabel;
    [SerializeField] private bool implemented;
    [SerializeField] private string sceneName = "Stage";

    public string StageNumber => stageNumber;
    public string StageName => stageName;
    public Sprite StageImage => stageImage;
    public string InfoText => infoText;
    public string ActionLabel => actionLabel;
    public bool Implemented => implemented;
    public string SceneName => string.IsNullOrWhiteSpace(sceneName) ? "Stage" : sceneName;
}
