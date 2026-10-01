using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public class PaletteColorApplicator : MonoBehaviour
{
    [SerializeField] private GameColorPalette palette;
    [SerializeField] private PaletteColor colorRole = PaletteColor.StageRed;
    [SerializeField] private bool applyOnEnable = true;

    public GameColorPalette Palette
    {
        get => palette;
        set
        {
            palette = value;
            Apply();
        }
    }

    public PaletteColor ColorRole
    {
        get => colorRole;
        set
        {
            colorRole = value;
            Apply();
        }
    }

    private void OnEnable()
    {
        if (Application.isPlaying) PaletteSettings.Changed += Apply;
        if (applyOnEnable) Apply();
    }

    private void OnDisable() => PaletteSettings.Changed -= Apply;

    private void OnValidate()
    {
        Apply();
    }

    [ContextMenu("Apply Palette Color")]
    public void Apply()
    {
        if (palette == null) return;

        Color color = palette.Get(colorRole);
        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null) graphic.color = color;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.color = color;
    }
}
