using UnityEngine;

public class HomeCurrencyPopupUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private InventoryCurrencyFrameUI currencyFramePrefab;
    [SerializeField] private CurrencyAsset[] currencies;

    private bool framesBuilt;

    private void Awake() => BuildFrames();

    private void OnEnable()
    {
        BuildFrames();
        Refresh();
    }

    private void BuildFrames()
    {
        if (framesBuilt || contentRoot == null || currencyFramePrefab == null) return;

        foreach (InventoryCurrencyFrameUI existing in contentRoot.GetComponentsInChildren<InventoryCurrencyFrameUI>(true))
        {
            existing.gameObject.SetActive(false);
            Destroy(existing.gameObject);
        }

        if (currencies != null)
        {
            foreach (CurrencyAsset currency in currencies)
            {
                if (currency == null) continue;
                InventoryCurrencyFrameUI frame = Instantiate(currencyFramePrefab, contentRoot);
                frame.name = currency.name + "Item";
                frame.Currency = currency;
            }
        }

        framesBuilt = true;
    }

    public void Refresh()
    {
        foreach (InventoryCurrencyFrameUI frame in contentRoot != null
                     ? contentRoot.GetComponentsInChildren<InventoryCurrencyFrameUI>(true)
                     : GetComponentsInChildren<InventoryCurrencyFrameUI>(true))
            frame.Refresh();
    }
}
