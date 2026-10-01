using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class UIManager : MonoBehaviour
{
    private const int UpgradeChoiceCount = 3;
    private static readonly System.Random upgradeRandom = new System.Random();
    private class UpgradeOption
    {
        public string label;
        public string description;
        public AttackGrowthNode node;
        public AttackRuntimeData targetAttack;
        public AttackDataSO attackToActivate;

        public UpgradeOption(string label, string description, AttackGrowthNode node = null,
            AttackRuntimeData targetAttack = null, AttackDataSO attackToActivate = null)
        {
            this.label = label;
            this.description = description;
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
    public TextMeshProUGUI stageTitleText;

    public UnityEngine.UI.Slider waveExpSlider;
    public UnityEngine.UI.Slider barrierHP;

    public GameObject PausePanel;
    public GameObject RewardPanel;
    public GameObject UpgradePanel;

    public List<UnityEngine.UI.Button> upgradeButtons;
    public AttackRuntimeData attack;

    private void Awake()
    {
        if (stageTitleText != null && GameManager.Instance != null &&
            GameManager.Instance.CurrentPlayMode == GameManager.PlayMode.EndlessLab)
            stageTitleText.text = "실험실";
        if (GetComponent<ActiveAttackPanelUI>() == null)
            gameObject.AddComponent<ActiveAttackPanelUI>();
        if (PausePanel != null && PausePanel.GetComponent<PauseInfoUI>() == null)
            PausePanel.AddComponent<PauseInfoUI>();
        AddBackdrop(PausePanel);
        AddBackdrop(RewardPanel);
        AddBackdrop(UpgradePanel);
    }

    public void OnEnable()
    {
        if (waveExpSlider != null)
        {
            waveExpSlider.value = 0;
            waveExpSlider.interactable = false;
        }
        if (barrierHP != null) barrierHP.interactable = false;
        SetRewardRowsVisible(false);
    }

    public void PrepareResultPanel(bool cleared)
    {
        SetRewardRowsVisible(false);
        if (RewardPanel == null) return;
        foreach (TextMeshProUGUI label in RewardPanel.GetComponentsInChildren<TextMeshProUGUI>(true))
            if (label.text == "점수" || label.text == "획득 보상" || label.text == "방어 실패" || label.text == "스테이지 클리어")
                label.text = cleared ? "스테이지 클리어" : "방어 실패";
    }

    private void SetRewardRowsVisible(bool visible)
    {
        foreach (TextMeshProUGUI label in RewardAmount)
            if (label != null) label.gameObject.SetActive(visible);
        if (RewardPanel == null) return;
        foreach (Transform child in RewardPanel.GetComponentsInChildren<Transform>(true))
            if (child.name == "RewardName" || child.name == "RewardAmount")
                child.gameObject.SetActive(visible);
    }

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

    public void UpdateScoreUI(int score, int highScore, bool isEndlessMode, int completedWaves)
    {
        if (scoreText == null) return;

        if (isEndlessMode)
        {
            scoreText.text = $"점수 {score:N0}\n통과 웨이브 {completedWaves}";
            return;
        }

        // 결과 팝업은 실제 점수나 최고 점수를 노출하지 않고, 최고 기록 대비 달성률만 표시한다.
        int completionPercent = highScore > 0
            ? Mathf.Clamp(Mathf.RoundToInt(score * 100f / highScore), 0, 100)
            : 0;
        scoreText.text = $"달성률\n{completionPercent}%";
    }

    public void UpdateWaveProgressUI(float waveExperience, int maxWaves, bool isEndlessMode = false)
    {
        int currentWave = Mathf.FloorToInt(waveExperience);
        float waveProgress = waveExperience - currentWave;

        if (isEndlessMode)
        {
            WaveText.text = $"웨이브 {currentWave + 1:D2}";
            waveExpSlider.value = waveProgress;
            return;
        }

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

    public void UpdateBarrierHealthUI(float currentHealth, float maxHealth)
    {
        if (barrierHP == null) return;
        barrierHP.minValue = 0f;
        barrierHP.maxValue = Mathf.Max(1f, maxHealth);
        barrierHP.value = Mathf.Clamp(currentHealth, 0f, barrierHP.maxValue);
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
        int count = Mathf.Min(UpgradeChoiceCount, Mathf.Min(options.Count, upgradeButtons.Count));
        for (int i = 0; i < count; i++)
        {
            UpgradeOption option = options[i];
            SetCardText(upgradeButtons[i], option.label, option.description);
            upgradeButtons[i].gameObject.SetActive(true);
            upgradeButtons[i].onClick.RemoveAllListeners();
            upgradeButtons[i].onClick.AddListener(() => ApplyUpgrade(option));
        }
        for (int i = count; i < upgradeButtons.Count; i++) upgradeButtons[i].gameObject.SetActive(false);
        if (count > 0)
        {
            PopupBackdropOverlay.Show(UpgradePanel);
            UpgradePanel.SetActive(true);
        }
        else UpgradePanel.SetActive(false);
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
                options.Add(new UpgradeOption(node.displayName, node.description, node, runtimeAttack));
            }
        }

        foreach (AttackDataSO attackData in StageManager.Instance.UnlockableAttacks)
        {
            // 아직 구현되지 않은 소환 및 임시 실행기는 공격 활성화 선택지에서 제외한다.
            if (attackData == null || !AttackExecutorFactory.IsSupported(attackData.attackType) || HasActiveAttack(attackData)) continue;
            options.Add(new UpgradeOption(
                $"새 공격 활성화: {attackData.attackName}", attackData.description, attackToActivate: attackData));
        }

        return DrawUpgradeOptions(options, attack, upgradeRandom);
    }

    private static List<UpgradeOption> DrawUpgradeOptions(List<UpgradeOption> options,
        AttackRuntimeData fallbackTarget, System.Random random)
    {
        // 제한을 소진한 노드를 되살리지 않고 부족한 자리만 반복 가능한 보충 강화로 채운다.
        if (options.Count < UpgradeChoiceCount)
        {
            var supplements = new List<UpgradeOption>
            {
                CreateSupplement("supplement_damage", "반복 화력 강화", "공격력 +5%", AttackUpgradeEffect.DamageMultiplier, fallbackTarget),
                CreateSupplement("supplement_speed", "반복 탄속 강화", "탄속 +5%", AttackUpgradeEffect.BulletSpeedMultiplier, fallbackTarget),
                CreateSupplement("supplement_range", "반복 사거리 강화", "사거리 +5%", AttackUpgradeEffect.RangeMultiplier, fallbackTarget)
            };
            ShuffleOptions(supplements, random);
            int missing = UpgradeChoiceCount - options.Count;
            for (int i = 0; i < missing; i++) options.Add(supplements[i]);
        }
        // 후보를 섞은 뒤 세 장만 남겨 한 화면에서 같은 카드를 중복 추첨하지 않는다.
        ShuffleOptions(options, random);
        if (options.Count > UpgradeChoiceCount)
            options.RemoveRange(UpgradeChoiceCount, options.Count - UpgradeChoiceCount);
        return options;
    }

    private static UpgradeOption CreateSupplement(string id, string name, string description,
        AttackUpgradeEffect effect, AttackRuntimeData target)
    {
        var node = new AttackGrowthNode
        {
            id = id,
            displayName = name,
            description = description + " · 반복 선택 가능",
            maxSelectionCount = int.MaxValue,
            modifiers = new List<AttackGrowthModifier> { new AttackGrowthModifier { effect = effect, value = 1.05f } }
        };
        return new UpgradeOption(node.displayName, node.description, node, target);
    }

    private static void ShuffleOptions(List<UpgradeOption> options, System.Random random)
    {
        for (int i = options.Count - 1; i > 0; i--)
        {
            int index = random.Next(i + 1);
            UpgradeOption option = options[i];
            options[i] = options[index];
            options[index] = option;
        }
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
        // 합체 폭발탄은 첫 적중 후 소멸하므로 주 포탄 관통 강화는 더 이상 후보에 넣지 않는다.
        if (runtimeAttack.HasBehavior(AttackBehavior.SiegeMergedProjectile) && node.modifiers != null)
            foreach (AttackGrowthModifier modifier in node.modifiers)
                if (modifier != null && modifier.effect == AttackUpgradeEffect.Pierce) return false;
        if (node.grantedBehaviors != null)
            foreach (AttackBehavior behavior in node.grantedBehaviors)
                if (!runtimeAttack.CanGrantBehavior(behavior)) return false;
        if (runtimeAttack.lockedGrowthNodeIds.Contains(node.id)) return false;
        if (runtimeAttack.GetGrowthNodeSelectionCount(node.id) >= Mathf.Max(1, node.maxSelectionCount)) return false;
        if (!string.IsNullOrEmpty(node.exclusiveGroup) && runtimeAttack.selectedGrowthGroups.Contains(node.exclusiveGroup)) return false;
        if (HasReachedChanceCap(runtimeAttack, node)) return false;
        return MeetsNodeRequirements(runtimeAttack, node);
    }

    private static bool HasReachedChanceCap(AttackRuntimeData runtimeAttack, AttackGrowthNode node)
    {
        if (node.modifiers == null) return false;
        const float epsilon = 0.0001f;
        foreach (AttackGrowthModifier modifier in node.modifiers)
        {
            if (modifier == null || modifier.value <= 0f) continue;
            switch (modifier.effect)
            {
                case AttackUpgradeEffect.CriticalChance:
                    if (runtimeAttack.criticalChance >= AttackRuntimeData.MaxCriticalChance - epsilon) return true;
                    break;
                case AttackUpgradeEffect.FragmentChance:
                    if (runtimeAttack.fragmentChance >= AttackRuntimeData.MaxFragmentChance - epsilon) return true;
                    break;
                case AttackUpgradeEffect.FragmentExplosionChance:
                    if (runtimeAttack.fragmentExplosionChance >= AttackRuntimeData.MaxFragmentExplosionChance - epsilon) return true;
                    break;
            }
        }
        return false;
    }

    private bool MeetsNodeRequirements(AttackRuntimeData runtimeAttack, AttackGrowthNode node)
    {
        if (node == null || string.IsNullOrEmpty(node.id)) return false;
        if (runtimeAttack.level < node.minimumLevel) return false;
        if (runtimeAttack.bulletCount < node.minimumBulletCount || runtimeAttack.ProjectilePierce < node.minimumPierce) return false;
        if (node.requirements != null) foreach (AttackGrowthRequirement requirement in node.requirements)
        {
            if (requirement == null || string.IsNullOrEmpty(requirement.nodeId)) continue;
            if (runtimeAttack.GetGrowthNodeSelectionCount(requirement.nodeId) < Mathf.Max(1, requirement.selectionCount)) return false;
        }
        if (node.requiredBehaviors != null)
            foreach (AttackBehavior behavior in node.requiredBehaviors)
                if (!runtimeAttack.HasBehavior(behavior)) return false;
        return true;
    }

    private void ApplyUpgrade(UpgradeOption option)
    {
        AttackRuntimeData upgradeTarget = option.targetAttack ?? attack;
        if (option.attackToActivate != null)
            StageManager.Instance.context.ActivateAttack(option.attackToActivate);
        else if (option.node != null)
        {
            if (!CanSelectNode(upgradeTarget, option.node)) return;
            if (option.node.modifiers != null)
                foreach (AttackGrowthModifier modifier in option.node.modifiers)
                    ApplyUpgradeEffect(upgradeTarget, modifier.effect, modifier.value);
        }
        if (option.node != null)
        {
            upgradeTarget.RecordGrowthNodeSelection(option.node.id);
            if (!string.IsNullOrEmpty(option.node.exclusiveGroup))
                upgradeTarget.selectedGrowthGroups.Add(option.node.exclusiveGroup);
            if (option.node.grantedBehaviors != null)
                foreach (AttackBehavior behavior in option.node.grantedBehaviors)
                    upgradeTarget.behaviors.Add(behavior);
            if (option.node.lockedNodeIds != null)
                foreach (string lockedNodeId in option.node.lockedNodeIds)
                    if (!string.IsNullOrEmpty(lockedNodeId)) upgradeTarget.lockedGrowthNodeIds.Add(lockedNodeId);
        }
        // 별도 공격 활성화는 기존 주 공격의 레벨을 올리지 않는다.
        if (option.attackToActivate == null) upgradeTarget.level++;
        CloseUpgradePanel();
    }

    private static void ApplyUpgradeEffect(AttackRuntimeData upgradeTarget, AttackUpgradeEffect effect, float value)
    {
        switch (effect)
        {
            // 배수 증감은 기본 배수에 합산해 강화 순서에 따라 결과가 달라지지 않게 한다.
            case AttackUpgradeEffect.DamageMultiplier: upgradeTarget.damageMultiplier += value - 1f; break;
            case AttackUpgradeEffect.AttackIntervalMultiplier: upgradeTarget.attackIntervalMultiplier += value - 1f; break;
            case AttackUpgradeEffect.RangeMultiplier: upgradeTarget.rangeMultiplier += value - 1f; break;
            case AttackUpgradeEffect.BulletCount:
                upgradeTarget.bulletCount += Mathf.Max(1, Mathf.RoundToInt(value));
                break;
            case AttackUpgradeEffect.Pierce: upgradeTarget.bulletHP += Mathf.Max(1, Mathf.RoundToInt(value)); break;
            case AttackUpgradeEffect.BulletSpeedMultiplier: upgradeTarget.bulletSpeedMultiplier += value - 1f; break;
            case AttackUpgradeEffect.SpreadAngle:
                upgradeTarget.bulletSpreadAngle = Mathf.Clamp(upgradeTarget.bulletSpreadAngle + value, 1f, 45f);
                break;
            case AttackUpgradeEffect.SpriteScaleMultiplier: upgradeTarget.attackSpriteScaleMultiplier += value - 1f; break;
            case AttackUpgradeEffect.MergeDamageCoefficient: upgradeTarget.mergeDamageCoefficient += value; break;
            case AttackUpgradeEffect.FragmentCount: upgradeTarget.fragmentCount += Mathf.RoundToInt(value); break;
            case AttackUpgradeEffect.FragmentDamageRatio: upgradeTarget.fragmentDamageRatio += value; break;
            case AttackUpgradeEffect.FragmentPierce: upgradeTarget.fragmentPierce += Mathf.RoundToInt(value); break;
            case AttackUpgradeEffect.ExplosionDamageRatio: upgradeTarget.explosionDamageRatio += value; break;
            case AttackUpgradeEffect.ExplosionRadiusMultiplier: upgradeTarget.explosionRadiusMultiplier += value - 1f; break;
            case AttackUpgradeEffect.LifeStealRatio: upgradeTarget.lifeStealRatio += value; break;
            case AttackUpgradeEffect.CriticalChance:
                upgradeTarget.criticalChance = Mathf.Clamp(upgradeTarget.criticalChance + value, 0f, AttackRuntimeData.MaxCriticalChance);
                break;
            case AttackUpgradeEffect.CriticalDamageMultiplier: upgradeTarget.criticalDamageMultiplier += value; break;
            case AttackUpgradeEffect.FragmentChance:
                upgradeTarget.fragmentChance = Mathf.Clamp(upgradeTarget.fragmentChance + value, 0f, AttackRuntimeData.MaxFragmentChance);
                break;
            case AttackUpgradeEffect.FragmentCountVariance:
                upgradeTarget.fragmentCountVariance = Mathf.Max(0, upgradeTarget.fragmentCountVariance + Mathf.RoundToInt(value));
                break;
            case AttackUpgradeEffect.FragmentExplosionChance:
                upgradeTarget.fragmentExplosionChance = Mathf.Clamp(upgradeTarget.fragmentExplosionChance + value, 0f,
                    AttackRuntimeData.MaxFragmentExplosionChance);
                break;
        }
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
        if (StageManager.Instance != null) StageManager.Instance.ResumeGameplay();
        else Time.timeScale = 1f;
    }

    private static void AddBackdrop(GameObject popup)
    {
        if (popup != null && popup.GetComponent<PopupBackdropOverlay>() == null)
            popup.AddComponent<PopupBackdropOverlay>();
    }
}

public class PauseInfoUI : MonoBehaviour
{
    private TextMeshProUGUI enemyTitle;
    private TextMeshProUGUI enemyBody;
    private TextMeshProUGUI multiplierTitle;
    private TextMeshProUGUI multiplierBody;

    private void Awake()
    {
        Transform enemyArea = transform.Find("InnerPanelTop/EnemyArea");
        Transform multiplierArea = transform.Find("InnerPanelTop/RewardArea");
        BindArea(enemyArea, out enemyTitle, out enemyBody);
        BindArea(multiplierArea, out multiplierTitle, out multiplierBody);
    }

    private void OnEnable()
    {
        Refresh();
    }

    private static void BindArea(Transform area, out TextMeshProUGUI title, out TextMeshProUGUI body)
    {
        title = null;
        body = null;
        if (area == null) return;
        TextMeshProUGUI[] labels = area.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (labels.Length > 0) title = labels[0];
        for (int i = labels.Length - 1; i > 0; i--)
            if (labels[i].gameObject.activeSelf)
            {
                body = labels[i];
                break;
            }
        if (body != null)
        {
            body.enableWordWrapping = true;
            body.alignment = TextAlignmentOptions.TopLeft;
            body.fontSize = 22f;
            RectTransform rect = body.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(35f, 20f);
            rect.offsetMax = new Vector2(-35f, -85f);
        }
    }

    public void Refresh()
    {
        if (enemyTitle != null) enemyTitle.text = "적 정보";
        if (multiplierTitle != null) multiplierTitle.text = "현재 배수";
        if (enemyBody != null) enemyBody.text = BuildEnemyInfo();
        if (multiplierBody != null) multiplierBody.text = BuildMultiplierInfo();
    }

    private static string BuildEnemyInfo()
    {
        EnemyManager manager = EnemyManager.Instance;
        Enemy sample = null;
        if (manager != null && manager.enemyPrefab != null && manager.enemyPrefab.Count > 0 && manager.enemyPrefab[0] != null)
            sample = manager.enemyPrefab[0].GetComponent<Enemy>();
        int active = manager != null ? manager.activeEnemies.Count : 0;
        int waiting = manager != null ? manager.PendingEnemyCount : 0;
        StageManager stage = StageManager.Instance;
        float healthMultiplier = stage != null ? stage.CurrentEnemyHealthMultiplier : 1f;
        float attackMultiplier = stage != null ? stage.CurrentEnemyAttackMultiplier : 1f;
        float countMultiplier = stage != null ? stage.CurrentEnemyCountMultiplier : 1f;
        int wave = stage != null ? stage.currentWave : 1;
        if (sample == null) return $"기본 적\n활성 {active} · 대기 {waiting}";
        return $"기본 적 · 웨이브 {wave}\n체력 {sample.HP_Set * healthMultiplier:0.#} · 이동 속도 {sample.speed:0.##}\n배리어 피해 {attackMultiplier:0.##} · 등장 수 ×{countMultiplier:0.##}\n활성 {active} · 대기 {waiting}";
    }

    private static string BuildMultiplierInfo()
    {
        if (StageManager.Instance == null || StageManager.Instance.context == null) return "공격 정보 없음";
        var builder = new StringBuilder();
        foreach (AttackRuntimeData attack in StageManager.Instance.context.RuntimeAttacks)
        {
            if (builder.Length > 0) builder.AppendLine();
            string attackName = attack.source != null ? attack.source.attackName : "공격";
            builder.AppendLine(attackName);
            builder.Append($"공격력 ×{attack.damageMultiplier:0.##} · 공격 속도 ×{1f / Mathf.Max(0.01f, attack.attackIntervalMultiplier):0.##}\n");
            builder.Append($"사거리 ×{attack.rangeMultiplier:0.##} · 탄속 ×{attack.bulletSpeedMultiplier:0.##} · 크기 ×{attack.attackSpriteScaleMultiplier:0.##}\n");
            builder.Append($"기본 치명타 {attack.EffectiveCriticalChance * 100f:0.#}% / ×{attack.EffectiveCriticalDamageMultiplier:0.##} · 활공시간·탄속 보정 · 흡혈 {attack.lifeStealRatio * 100f:0.#}%");
        }
        return builder.Length > 0 ? builder.ToString() : "공격 정보 없음";
    }
}
