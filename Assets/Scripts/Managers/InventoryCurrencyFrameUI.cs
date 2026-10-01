using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InventoryCurrencyFrameUI : MonoBehaviour
{
    [SerializeField] private CurrencyAsset currency;
    [SerializeField] private GameColorPalette palette;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text amountLabel;

    public CurrencyAsset Currency
    {
        get => currency;
        set
        {
            currency = value;
            Refresh();
        }
    }

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        if (currency == null) return;

        Color currencyColor = palette != null
            ? palette.Get(currency.ColorRole)
            : Color.white;

        if (icon != null)
        {
            icon.sprite = currency.Icon;
            icon.enabled = currency.Icon != null;
            icon.color = currencyColor;
        }

        if (nameLabel != null)
        {
            nameLabel.text = currency.DisplayName;
            nameLabel.color = Color.white;
        }
        if (amountLabel != null)
        {
            amountLabel.text = currency.Amount.ToString("N0");
            amountLabel.color = currencyColor;
        }
    }
}
