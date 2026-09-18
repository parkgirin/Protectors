using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private enum UpgradeType { Damage, AttackSpeed, Range, BulletCount, Pierce, BulletSpeed, RapidFire, PiercingVolley, SniperShot, ActivateAttack }

    private class UpgradeOption
    {
        public UpgradeType type;
        public string label;
        public string description;
        public float value;
        public AttackGrowthNode node;
        public AttackRuntimeData targetAttack;
        public AttackDataSO attackToActivate;

        public UpgradeOption(UpgradeType type, string label, float value, AttackGrowthNode node = null, AttackRuntimeData targetAttack = null, AttackDataSO attackToActivate = null, string description = "")
        {
            this.type = type;
            this.label = label;
            this.description = description;
            this.value = value;
            this.node = node;
            this.targetAttack = targetAttack;
            this.attackToActivate = attackToActivate;
        }
    }
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
    public AttackRuntimeData attack;

    private void Awake()
    {
        if (GetComponent<ActiveAttackPanelUI>() == null)
            gameObject.AddComponent<ActiveAttackPanelUI>();
    }

    public void OnEnable()
    {
        if (waveExpSlider != null) waveExpSlider.value = 0;
    }
/*    // ?∏Ï∂ú?????¥Îãπ ?îÏÜå???âÏùÑ ÏßÄ?ïÎêú ?âÏúºÎ°?Î≥ÄÍ≤?
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

    public bool UpdateUpgradeList()
    {
        if (StageManager.Instance == null || StageManager.Instance.context == null) return false;
        attack = StageManager.Instance.context.RuntimeAttack;
        if (attack == null || UpgradePanel == null) return false;

        List<UpgradeOption> options = BuildUpgradeOptions();
        if (options.Count == 0)
        {
            UpgradePanel.SetActive(false);
            return false;
        }
        int count = Mathf.Min(options.Count, upgradeButtons.Count);
        for (int i = 0; i < count; i++)
        {
            UpgradeOption option = options[i];
            SetCardText(upgradeButtons[i], option.label, option.description);
            upgradeButtons[i].gameObject.SetActive(true);
            upgradeButtons[i].onClick.RemoveAllListeners();
            upgradeButtons[i].onClick.AddListener(() => ApplyUpgrade(option));
        }
        for (int i = count; i < upgradeButtons.Count; i++) upgradeButtons[i].gameObject.SetActive(false);
        UpgradePanel.SetActive(true);
        return count > 0;
    }

    private void SetCardText(UnityEngine.UI.Button button, string name, string description)
    {
        Transform nameTransform = button.transform.Find("Name");
        Transform descriptionTransform = button.transform.Find("Description");
        TextMeshProUGUI nameText = nameTransform != null ? nameTransform.GetComponent<TextMeshProUGUI>() : null;
        TextMeshProUGUI descriptionText = descriptionTransform != null ? descriptionTransform.GetComponent<TextMeshProUGUI>() : null;

        if (nameText == null || descriptionText == null)
        {
            Debug.LogError("Upgrade button requires TextMeshProUGUI children named Name and Description.", button);
            return;
        }

        nameText.text = name;
        descriptionText.text = description;
    }

    private List<UpgradeOption> BuildUpgradeOptions()
    {
        List<UpgradeOption> options = new List<UpgradeOption>();
        foreach (AttackRuntimeData runtimeAttack in StageManager.Instance.context.RuntimeAttacks)
        {
            AttackGrowthTreeSO tree = runtimeAttack.source != null ? runtimeAttack.source.growthTree : null;
            if (tree == null) continue;

            foreach (AttackGrowthNode node in tree.nodes)
            {
                if (!CanSelectNode(runtimeAttack, node)) continue;
                options.Add(new UpgradeOption((UpgradeType)node.effect, node.displayName, node.value, node, runtimeAttack, null, node.description));
            }
        }

        foreach (AttackDataSO attackData in StageManager.Instance.UnlockableAttacks)
        {
            if (attackData == null || HasActiveAttack(attackData)) continue;
            options.Add(new UpgradeOption(UpgradeType.ActivateAttack,
                $"ªı ∞¯∞› »∞º∫»≠: {attackData.attackName}", 0f, null, null, attackData, attackData.description));
        }

        // Once the one-time nodes are consumed, reusable non-branch nodes keep every choice panel full.
        List<(AttackRuntimeData runtimeAttack, AttackGrowthNode node)> repeatableNodes = new List<(AttackRuntimeData, AttackGrowthNode)>();
        foreach (AttackRuntimeData runtimeAttack in StageManager.Instance.context.RuntimeAttacks)
        {
            AttackGrowthTreeSO tree = runtimeAttack.source != null ? runtimeAttack.source.growthTree : null;
            if (tree == null) continue;

            foreach (AttackGrowthNode node in tree.nodes)
                if (string.IsNullOrEmpty(node.exclusiveGroup) && MeetsNodeRequirements(runtimeAttack, node))
                    repeatableNodes.Add((runtimeAttack, node));
        }
        int repeatIndex = 0;
        while (options.Count < 3 && repeatableNodes.Count > 0)
        {
            (AttackRuntimeData runtimeAttack, AttackGrowthNode node) repeated = repeatableNodes[repeatIndex++ % repeatableNodes.Count];
            options.Add(new UpgradeOption((UpgradeType)repeated.node.effect, repeated.node.displayName, repeated.node.value, repeated.node, repeated.runtimeAttack, null, repeated.node.description));
        }
        return options;
    }

    private bool HasActiveAttack(AttackDataSO attackData)
    {
        foreach (AttackRuntimeData runtimeAttack in StageManager.Instance.context.RuntimeAttacks)
            if (runtimeAttack.source == attackData) return true;
        return false;
    }

    private bool CanSelectNode(AttackRuntimeData runtimeAttack, AttackGrowthNode node)
    {
        if (node == null || string.IsNullOrEmpty(node.id)) return false;
        if (runtimeAttack.selectedGrowthNodeIds.Contains(node.id)) return false;
        if (!string.IsNullOrEmpty(node.exclusiveGroup) && runtimeAttack.selectedGrowthGroups.Contains(node.exclusiveGroup)) return false;
        return MeetsNodeRequirements(runtimeAttack, node);
    }

    private bool MeetsNodeRequirements(AttackRuntimeData runtimeAttack, AttackGrowthNode node)
    {
        if (runtimeAttack.level < node.minimumLevel) return false;
        if (runtimeAttack.EffectiveDamage < node.minimumDamage || runtimeAttack.EffectiveRange < node.minimumRange) return false;
        if (runtimeAttack.bulletCount < node.minimumBulletCount || runtimeAttack.bulletHP < node.minimumPierce) return false;
        return node.requiredBranch == AttackBranch.None || runtimeAttack.branch == node.requiredBranch;
    }

    private void ApplyUpgrade(UpgradeOption option)
    {
        AttackRuntimeData upgradeTarget = option.targetAttack ?? attack;
        switch (option.type)
        {
            case UpgradeType.Damage: upgradeTarget.damageMultiplier *= option.value; break;
            case UpgradeType.AttackSpeed: upgradeTarget.attackIntervalMultiplier *= option.value; break;
            case UpgradeType.Range: upgradeTarget.rangeMultiplier *= option.value; break;
            case UpgradeType.BulletCount:
                upgradeTarget.bulletCount += Mathf.Max(1, Mathf.RoundToInt(option.value));
                break;
            case UpgradeType.Pierce: upgradeTarget.bulletHP++; break;
            case UpgradeType.BulletSpeed: upgradeTarget.bulletSpeedMultiplier *= option.value; break;
            case UpgradeType.RapidFire:
                upgradeTarget.branch = AttackBranch.RapidFire;
                upgradeTarget.attackIntervalMultiplier *= 0.65f;
                upgradeTarget.damageMultiplier *= 0.8f;
                break;
            case UpgradeType.PiercingVolley:
                upgradeTarget.branch = AttackBranch.PiercingVolley;
                upgradeTarget.bulletCount++;
                upgradeTarget.damageMultiplier *= 0.85f;
                break;
            case UpgradeType.SniperShot:
                upgradeTarget.branch = AttackBranch.SniperShot;
                upgradeTarget.damageMultiplier *= 1.5f;
                upgradeTarget.attackIntervalMultiplier *= 1.25f;
                break;
            case UpgradeType.ActivateAttack:
                StageManager.Instance.context.ActivateAttack(option.attackToActivate);
                break;
        }
        if (option.node != null)
        {
            upgradeTarget.selectedGrowthNodeIds.Add(option.node.id);
            if (!string.IsNullOrEmpty(option.node.exclusiveGroup))
                upgradeTarget.selectedGrowthGroups.Add(option.node.exclusiveGroup);
            if (option.node.resultBranch != AttackBranch.None)
                upgradeTarget.branch = option.node.resultBranch;
        }
        upgradeTarget.level++;
        CloseUpgradePanel();
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
        if (UpgradePanel != null) UpgradePanel.SetActive(false);
        Time.timeScale = 1f; // Í≤åÏûÑ ?¨Í∞ú
    }
}
