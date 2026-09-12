using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
/*    public Color TextColor;
    public Color BackgroundColor;
    public Color LineColor;*/

    public List<TextMeshProUGUI> RewardAmount;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI WaveText;

    public UnityEngine.UI.Slider waveExpSlider;

    public GameObject PausePanel;
    public GameObject RewardPanel;
    public GameObject UpgradePanel;

    public List<UnityEngine.UI.Button> upgradeButtons;
    public List<TextMeshProUGUI> upgradeTexts;

    public AttackRuntimeData attack;

    public void OnEnable()
    {
        waveExpSlider.value = 0;
    }
/*    // 호출될 시 해당 요소의 색을 지정된 색으로 변경
    public void ChangeTextColor(TextMeshProUGUI textElement)
    {
        textElement.color = TextColor;
    }
    public void ChangeBackgroundColor(Image backgroundElement)
    {
        backgroundElement.tintColor = BackgroundColor;
    }
    public void ChangeLineColor(Image lineElement)
    {
        lineElement.tintColor = LineColor;
    }*/

    public void UpdateRewardUI(List<int> Amount)
    {
        string rewardText = "";

        for (int i = 0; i < Amount.Count; i++)
        {
            rewardText += Amount[i].ToString() + "\n";
        }

        for (int i = 0; i < RewardAmount.Count; i++)
        {
            RewardAmount[i].text = rewardText;
        }
    }

    public void UpdateScoreUI(int score)
    {
        scoreText.text = score.ToString();
    }

    public void UpdateWaveProgressUI(float waveExperience, int maxWaves)
    {
        int currentWave = Mathf.FloorToInt(waveExperience);
        float waveProgress = waveExperience - currentWave;

        if (currentWave <= maxWaves - 1)
        {
            int displayWave = currentWave + 1;
            WaveText.text =
                displayWave.ToString("D2") + " / " + maxWaves.ToString("D2");
            waveExpSlider.value = waveProgress;
        }
        else
        {
            WaveText.text = maxWaves + " / " + maxWaves;
            waveExpSlider.value = 1f;
        }
    }

    public void UpdateUpgradeList()
    {
        attack = StageManager.Instance.context.RuntimeAttack;
        // 업그레이드 목록 UI 업데이트 로직 구현
        // StageManager 에서 업그레이드 목록 3개 받아 UI 에 전달
        UpgradePanel.SetActive(true);

        upgradeTexts[0].text = "공격력 +10%";
        upgradeTexts[1].text = "공격 속도 +10%";
        upgradeTexts[2].text = "공격 범위 +10%";

        upgradeButtons[0].onClick.RemoveAllListeners();
        upgradeButtons[0].onClick.AddListener(() => AddDamageMultiplier(1.1f));

        upgradeButtons[1].onClick.RemoveAllListeners();
        upgradeButtons[1].onClick.AddListener(() => AddAttackIntervalMultiplier(0.9f));

        upgradeButtons[2].onClick.RemoveAllListeners();
        upgradeButtons[2].onClick.AddListener(() => AddRangeMultiplier(1.1f));
    }

    public void AddDamageMultiplier(float multiplier)
    {
        attack.damageMultiplier *= multiplier;
        CloseUpgradePanel();
    }

    public void AddAttackIntervalMultiplier(float multiplier)
    {
        attack.attackIntervalMultiplier *= multiplier;
        CloseUpgradePanel();
    }

    public void AddRangeMultiplier(float multiplier)
    {
        attack.rangeMultiplier *= multiplier;
        CloseUpgradePanel();
    }

    public void CloseUpgradePanel()
    {
        UpgradePanel.SetActive(false);
        Time.timeScale = 1f; // 게임 재개
    }
}
