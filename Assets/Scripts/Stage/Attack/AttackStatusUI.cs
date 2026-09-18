using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Runtime-built HUD so existing scenes do not need manual hierarchy changes.
public class AttackStatusUI : MonoBehaviour
{
    private readonly List<TextMeshProUGUI> rows = new List<TextMeshProUGUI>();
    private RectTransform root;
    private float refreshTimer;

    private void Awake()
    {
        CreatePanel();
    }

    private void Update()
    {
        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f) return;
        refreshTimer = 0.1f;
        Refresh();
    }

    private void CreatePanel()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        GameObject panel = new GameObject("Attack Status Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        root = panel.GetComponent<RectTransform>();
        root.SetParent(canvas.transform, false);
        root.anchorMin = new Vector2(1f, 1f);
        root.anchorMax = new Vector2(1f, 1f);
        root.pivot = new Vector2(1f, 1f);
        root.anchoredPosition = new Vector2(-24f, -120f);
        root.sizeDelta = new Vector2(290f, 0f);

        Image image = panel.GetComponent<Image>();
        image.color = new Color(0.04f, 0.06f, 0.1f, 0.86f);
        VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 7f;
        layout.childAlignment = TextAnchor.UpperLeft;
        ContentSizeFitter fitter = panel.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateRow("활성 공격", 24, new Color(1f, 0.82f, 0.35f));
    }

    private TextMeshProUGUI CreateRow(string text, int size, Color color)
    {
        GameObject row = new GameObject("Attack Status", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        row.transform.SetParent(root, false);
        TextMeshProUGUI label = row.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.fontSize = size;
        label.color = color;
        label.enableWordWrapping = false;
        label.text = text;
        LayoutElement element = row.GetComponent<LayoutElement>();
        element.minHeight = size + 8;
        rows.Add(label);
        return label;
    }

    private void Refresh()
    {
        if (root == null || StageManager.Instance == null || StageManager.Instance.context == null) return;
        List<AttackRuntimeData> attacks = StageManager.Instance.context.RuntimeAttacks;
        AttackController controller = FindObjectOfType<AttackController>();
        while (rows.Count < attacks.Count + 1) CreateRow(string.Empty, 18, Color.white);

        for (int i = 0; i < attacks.Count; i++)
        {
            AttackRuntimeData attack = attacks[i];
            rows[i + 1].gameObject.SetActive(true);
            float remaining = controller != null ? controller.GetCooldownRemaining(attack) : 0f;
            bool active = controller != null && controller.IsActive(attack);
            string name = attack.source != null ? attack.source.attackName : "이름 없는 공격";
            rows[i + 1].text = active
                ? name + "  <color=#8DFF9A>활성</color>  " + remaining.ToString("0.0") + "초"
                : name + "  <color=#A0A0A0>대기</color>";
        }
        for (int i = attacks.Count + 1; i < rows.Count; i++) rows[i].gameObject.SetActive(false);
    }
}
