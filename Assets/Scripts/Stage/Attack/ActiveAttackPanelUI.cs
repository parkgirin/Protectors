using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 활성 공격을 미리 구성된 SidePanel > Filled > Icon UI 계층에 연결한다.
public class ActiveAttackPanelUI : MonoBehaviour
{
    private sealed class AttackSlot
    {
        public GameObject root;
        public Image filled;
        public Image icon;
        public AttackRuntimeData attack;
    }

    private readonly List<AttackSlot> slots = new List<AttackSlot>();
    private RectTransform sidePanel;
    private Image filledTemplate;
    private float refreshTimer;
    private LineRenderer rangeRing;
    private Material rangeRingMaterial;
    private AttackRuntimeData displayedRangeAttack;
    private const int RangeRingSegments = 64;

    private void Awake()
    {
        InitializeSidePanel();
    }

    private void Update()
    {
        UpdateRangeRing();
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
            return;
        }

        ConfigureCooldownGauge(filledTemplate);
    }

    private void Refresh()
    {
        if (sidePanel == null || filledTemplate == null) InitializeSidePanel();
        if (sidePanel == null || filledTemplate == null) return;

        // 강화, 일시정지, 게임 오버, 클리어 화면에서는 다른 UI를 가리지 않도록 HUD를 숨긴다.
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
            slot.attack = attack;
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
        ConfigureCooldownGauge(filled);

        Image icon = filled.transform.Find("Icon")?.GetComponent<Image>();
        if (icon == null)
        {
            Debug.LogError("Filled requires an Icon child with an Image component.", filled);
            return null;
        }

        AttackSlot slot = new AttackSlot
        {
            root = filled.gameObject,
            filled = filled,
            icon = icon
        };
        BindRangeHold(icon, slot);
        return slot;
    }

    private void BindRangeHold(Image icon, AttackSlot slot)
    {
        EventTrigger trigger = icon.GetComponent<EventTrigger>();
        if (trigger == null) trigger = icon.gameObject.AddComponent<EventTrigger>();
        if (trigger.triggers == null) trigger.triggers = new List<EventTrigger.Entry>();
        AddTrigger(trigger, EventTriggerType.PointerDown, _ => ShowRange(slot.attack));
        AddTrigger(trigger, EventTriggerType.PointerUp, _ => HideRange());
        AddTrigger(trigger, EventTriggerType.PointerExit, _ => HideRange());
    }

    private static void AddTrigger(EventTrigger trigger, EventTriggerType type,
        UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    private void ShowRange(AttackRuntimeData attack)
    {
        if (attack == null || StageManager.Instance == null) return;
        displayedRangeAttack = attack;
        EnsureRangeRing();
        if (rangeRing != null) rangeRing.gameObject.SetActive(true);
        UpdateRangeRing();
    }

    private void HideRange()
    {
        displayedRangeAttack = null;
        if (rangeRing != null) rangeRing.gameObject.SetActive(false);
    }

    private void EnsureRangeRing()
    {
        if (rangeRing != null) return;
        GameObject rangeObject = new GameObject("공격 사거리 표시");
        rangeRing = rangeObject.AddComponent<LineRenderer>();
        rangeRing.useWorldSpace = false;
        rangeRing.loop = true;
        rangeRing.positionCount = RangeRingSegments;
        rangeRing.widthMultiplier = 0.035f;
        rangeRing.sortingOrder = 4;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            rangeRingMaterial = new Material(shader);
            rangeRing.sharedMaterial = rangeRingMaterial;
        }
    }

    private void UpdateRangeRing()
    {
        if (displayedRangeAttack == null || rangeRing == null || !rangeRing.gameObject.activeSelf) return;
        AttackController controller = FindObjectOfType<AttackController>();
        if (controller == null) { HideRange(); return; }

        rangeRing.transform.position = controller.transform.position;
        float range = Mathf.Max(0.01f, displayedRangeAttack.EffectiveRange);
        for (int i = 0; i < RangeRingSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / RangeRingSegments;
            rangeRing.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * range);
        }
        Color color = StageManager.Instance != null && StageManager.Instance.colorPalette != null
            ? StageManager.Instance.colorPalette.Get(PaletteColor.StageRedBright)
            : new Color(1f, 0.416f, 0.341f, 0.9f);
        color.a = 0.9f;
        rangeRing.startColor = rangeRing.endColor = color;
    }

    private void OnDisable() => HideRange();

    private void OnDestroy()
    {
        if (rangeRing != null) Destroy(rangeRing.gameObject);
        if (rangeRingMaterial != null) Destroy(rangeRingMaterial);
    }

    private Transform FindSidePanel()
    {
        Transform inOwnRoot = FindDescendant(transform.root, "SidePanel");
        if (inOwnRoot != null) return inOwnRoot;

        // SidePanel은 별도 Canvas 아래에 있거나 현재 비활성 상태일 수 있다.
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

    // The scene template was authored as a Sliced Image. Image.fillAmount is
    // ignored for that type, so normalize every slot to a radial cooldown gauge.
    private static void ConfigureCooldownGauge(Image gauge)
    {
        gauge.type = Image.Type.Filled;
        gauge.fillMethod = Image.FillMethod.Radial360;
        gauge.fillOrigin = (int)Image.Origin360.Top;
        gauge.fillClockwise = true;
    }
}
