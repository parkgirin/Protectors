using UnityEngine;

[CreateAssetMenu(fileName = "Currency", menuName = "Inventory/Currency")]
public class CurrencyAsset : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField, Min(0)] private int amount;
    [SerializeField] private PaletteColor colorRole = PaletteColor.Unspecified;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public Sprite Icon => icon;
    public int Amount
    {
        get => amount;
        set => amount = Mathf.Max(0, value);
    }

    public PaletteColor ColorRole => colorRole == PaletteColor.Unspecified
        ? PaletteColor.UiWhite
        : colorRole;
}
