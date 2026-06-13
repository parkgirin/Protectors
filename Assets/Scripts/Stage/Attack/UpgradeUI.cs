using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("Child UI")]
    private Image iconImage;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI descriptionText;

    [Header("Resource Path")]
    public string spritePath = "Sprites/AttackIcons/";

    private void Awake()
    {
        Transform iconTr = transform.Find("Icon");
        Transform nameTr = transform.Find("Name");
        Transform descTr = transform.Find("Description");

        if (iconTr == null || nameTr == null || descTr == null)
        {
            Debug.LogError("UpgradeUI 하위 UI 구조가 잘못되었습니다.");
            return;
        }

        iconImage = iconTr.GetComponent<Image>();
        nameText = nameTr.GetComponent<TextMeshProUGUI>();
        descriptionText = descTr.GetComponent<TextMeshProUGUI>();
    }

    public void SetUI(string icon, string name, string description)
    {
        SetIcon(icon);
        nameText.text = name;
        descriptionText.text = description;
    }

    private void SetIcon(string icon)
    {
        if (string.IsNullOrEmpty(spritePath)) return;

        Sprite sprite = Resources.Load<Sprite>(spritePath + icon);

        if (sprite != null)
        {
            iconImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"Sprite not found: {spritePath + icon}");
        }
    }
}
