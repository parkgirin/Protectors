using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Binds active attacks to the authored SidePanel > Filled > Icon UI hierarchy.
public class ActiveAttackPanelUI : MonoBehaviour
{
    private sealed class AttackSlot
    {
        public GameObject root;
        public Image filled;
        public Image icon;
    }

    private readonly List<AttackSlot> slots = new List<AttackSlot>();
    private RectTransform sidePanel;
    private Image filledTemplate;
    private float refreshTimer;

    private void Awake()
    {
        InitializeSidePanel();
    }

    private void Update()
    {
        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f) return;
        refreshTimer = 0.05f;
        Refresh();
    }

    private void InitializeSidePanel()
    {
        Transform panelTransform = FindSidePanel();
        if (panelTransform == null) return;

        sidePanel = panelTransform as RectTransform;
        Transform filledTransform = sidePanel.Find("Filled");
        filledTemplate = filledTransform != null ? filledTransform.GetComponent<Image>() : null;
        if (filledTemplate == null || filledTemplate.transform.Find("Icon") == null)
        {
            Debug.LogError("SidePanel requires a Filled child with an Icon child.", sidePanel);
            sidePanel = null;
            filledTemplate = null;
        }
    }

    private void Refresh()
    {
        if (sidePanel == null || filledTemplate == null) InitializeSidePanel();
        if (sidePanel == null || filledTemplate == null) return;

        // Upgrade, pause, game-over, and clear screens stop time. Keep their UI unobstructed.
        bool isGameplayRunning = Time.timeScale > 0f;
        if (sidePanel.gameObject.activeSelf != isGameplayRunning)
            sidePanel.gameObject.SetActive(isGameplayRunning);

        if (!isGameplayRunning || StageManager.Instance == null || StageManager.Instance.context == null) return;

        List<AttackRuntimeData> attacks = StageManager.Instance.context.RuntimeAttacks;
        while (slots.Count < attacks.Count)
        {
            AttackSlot slot = CreateSlot();
            if (slot == null) break;
            slots.Add(slot);
        }

        AttackController controller = FindObjectOfType<AttackController>();
        int displayedCount = Mathf.Min(attacks.Count, slots.Count);
        for (int i = 0; i < displayedCount; i++)
        {
            AttackRuntimeData attack = attacks[i];
            AttackSlot slot = slots[i];
            slot.root.SetActive(true);

            float interval = Mathf.Max(0.01f, attack.EffectiveAttackInterval);
            float remaining = controller != null ? controller.GetCooldownRemaining(attack) : interval;
            float charge = controller != null && controller.IsActive(attack)
                ? Mathf.Clamp01(1f - remaining / interval)
                : 0f;
            slot.filled.fillAmount = charge;

            Sprite sprite = attack.source != null ? attack.source.icon : null;
            if (sprite == null && attack.source != null) sprite = attack.source.attackSprite;
            slot.icon.sprite = sprite;
            slot.icon.enabled = sprite != null;
        }

        for (int i = displayedCount; i < slots.Count; i++) slots[i].root.SetActive(false);
    }

    private AttackSlot CreateSlot()
    {
        Image filled = slots.Count == 0
            ? filledTemplate
            : Instantiate(filledTemplate, sidePanel);
        filled.gameObject.name = "Filled";
        filled.gameObject.SetActive(true);

        Image icon = filled.transform.Find("Icon")?.GetComponent<Image>();
        if (icon == null)
        {
            Debug.LogError("Filled requires an Icon child with an Image component.", filled);
            return null;
        }

        return new AttackSlot
        {
            root = filled.gameObject,
            filled = filled,
            icon = icon
        };
    }

    private Transform FindSidePanel()
    {
        Transform inOwnRoot = FindDescendant(transform.root, "SidePanel");
        if (inOwnRoot != null) return inOwnRoot;

        // SidePanel can be under a separate Canvas or currently inactive.
        foreach (RectTransform candidate in FindObjectsOfType<RectTransform>(true))
            if (candidate.name == "SidePanel") return candidate;
        return null;
    }

    private static Transform FindDescendant(Transform root, string targetName)
    {
        if (root.name == targetName) return root;
        foreach (Transform child in root)
        {
            Transform found = FindDescendant(child, targetName);
            if (found != null) return found;
        }
        return null;
    }
}
