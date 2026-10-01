using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PopupBackdropOverlay : MonoBehaviour
{
    private GameObject backdrop;

    public static void Show(GameObject popup)
    {
        if (popup == null) return;

        PopupBackdropOverlay overlay = popup.GetComponent<PopupBackdropOverlay>();
        if (overlay == null) overlay = popup.AddComponent<PopupBackdropOverlay>();
        overlay.ShowBackdrop();
        popup.transform.SetAsLastSibling();
    }

    private void OnEnable() => ShowBackdrop();

    private void OnDisable()
    {
        if (backdrop != null) backdrop.SetActive(false);
    }

    private void ShowBackdrop()
    {
        if (transform.parent == null) return;
        EnsureBackdrop();
        backdrop.SetActive(true);
        backdrop.transform.SetAsLastSibling();
        transform.SetAsLastSibling();
    }

    private void EnsureBackdrop()
    {
        if (backdrop != null) return;

        backdrop = new GameObject($"{name}Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backdrop.transform.SetParent(transform.parent, false);
        RectTransform rect = backdrop.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = backdrop.GetComponent<Image>();
        image.raycastTarget = true;

        GameColorPalette palette = FindPalette();
        if (palette != null)
        {
            PaletteColorApplicator colorApplicator = backdrop.AddComponent<PaletteColorApplicator>();
            colorApplicator.Palette = palette;
            colorApplicator.ColorRole = PaletteColor.PopupBackdrop;
        }
        else
        {
            image.color = new Color(0f, 0f, 0f, 0.78f);
        }
    }

    private static GameColorPalette FindPalette()
    {
        foreach (PaletteColorApplicator binding in FindObjectsOfType<PaletteColorApplicator>(true))
            if (binding.Palette != null) return binding.Palette;
        return null;
    }
}
