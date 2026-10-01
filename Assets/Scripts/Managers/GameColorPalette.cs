using UnityEngine;

[CreateAssetMenu(fileName = "GameColorPalette", menuName = "ColorPalette/Game Color Palette")]
public class GameColorPalette : ScriptableObject
{
    [Header("Alternative Palette")]
    public GameColorPalette softPalette;

    [Header("Enemy Sprites")]
    [Tooltip("Shared tint for grayscale enemies in story mode.")]
    public Color storyEnemy = new Color(0.600f, 0.169f, 0.200f); // #992B33
    [Tooltip("White preserves the authored colors of a fully colored sprite.")]
    public Color originalSpriteColor = Color.white;
    [Header("Stage UI")]
    [Tooltip("Stage panels, primary calls to action, and registered planets.")]
    public Color stageRed = new Color(0.933f, 0.290f, 0.224f);
    public Color stageRedBright = new Color(1.000f, 0.416f, 0.341f);
    public Color stageRedDark = new Color(0.663f, 0.184f, 0.220f);

    [Header("Planet Archive")]
    [Tooltip("Line color for planets that have not been registered yet.")]
    public Color planetUnregistered = new Color(0.655f, 0.725f, 0.792f);
    public Color planetUnregisteredDark = new Color(0.251f, 0.325f, 0.431f);

    [Header("Neutral UI")]
    public Color uiWhite = Color.white;
    public Color uiSubText = new Color(0.969f, 0.843f, 0.831f);
    public Color uiInactive = new Color(0.435f, 0.518f, 0.608f);
    public Color spaceBackground = new Color(0.063f, 0.094f, 0.153f);
    public Color spaceBackgroundDeep = new Color(0.145f, 0.071f, 0.110f);

    [Header("Popup Overlay")]
    [Tooltip("Semi-transparent black backdrop that dims the UI behind an open popup.")]
    public Color popupBackdrop = new Color(0f, 0f, 0f, 0.78f);

    [Header("Stage Entities")]
    [Tooltip("Lower-stage barrier behind the player. Keeps player red readable.")]
    public Color shieldBarrier = new Color(0.420f, 0.553f, 0.675f);
    public Color shieldBarrierHighlight = new Color(0.733f, 0.847f, 0.914f);
    [Tooltip("Reference tint for character sprite materials when reducing saturation.")]
    public Color characterSpriteTint = Color.white;
    public Color donutMint = new Color(0.435f, 0.878f, 0.761f);
    public Color chocolateBrown = new Color(0.780f, 0.400f, 0.235f);

    [Header("Currency")]
    public Color solarGold = new Color(1.000f, 0.816f, 0.345f);
    public Color cosmicGem = new Color(0.545f, 0.459f, 0.910f);
    public Color planetShard = new Color(0.690f, 0.702f, 0.741f);

    public Color Get(PaletteColor color)
    {
        if (Application.isPlaying)
        {
            if (PaletteSettings.Mode == PaletteMode.Soft && softPalette != null)
                return softPalette.GetRaw(color);
            if (PaletteSettings.Mode == PaletteMode.Custom)
                return GetCustom(color, PaletteSettings.CustomAccent);
        }
        return GetRaw(color);
    }

    /// <summary>
    /// Transforms the red accent family by preserving each color's HSV relation to stageRed.
    /// Other colors retain their hue and receive only a restrained saturation/value adjustment.
    /// </summary>
    private Color GetCustom(PaletteColor color, Color customAccent)
    {
        Color source = GetRaw(color);
        if (color == PaletteColor.PopupBackdrop || color == PaletteColor.UiWhite ||
            color == PaletteColor.OriginalSpriteColor || color == PaletteColor.CharacterSpriteTint ||
            IsCurrencyColor(color))
            return source;

        Color.RGBToHSV(stageRed, out float baseHue, out float baseSaturation, out float baseValue);
        Color.RGBToHSV(customAccent, out float customHue, out float customSaturation, out float customValue);
        float hueOffset = Mathf.Repeat(customHue - baseHue + 0.5f, 1f) - 0.5f;
        float saturationScale = baseSaturation > 0.001f ? customSaturation / baseSaturation : 1f;
        float valueScale = baseValue > 0.001f ? customValue / baseValue : 1f;

        return IsAccentFamily(color)
            ? ShiftAccent(source, hueOffset, saturationScale, valueScale)
            : AdjustSupportingColor(source, saturationScale, valueScale);
    }

    private static bool IsAccentFamily(PaletteColor color)
    {
        return color == PaletteColor.StoryEnemy ||
               color == PaletteColor.StageRed ||
               color == PaletteColor.StageRedBright ||
               color == PaletteColor.StageRedDark;
    }

    private static bool IsCurrencyColor(PaletteColor color)
    {
        return color == PaletteColor.DonutMint ||
               color == PaletteColor.ChocolateBrown ||
               color == PaletteColor.SolarGold ||
               color == PaletteColor.CosmicGem ||
               color == PaletteColor.PlanetShard;
    }

    private static Color ShiftAccent(Color source, float hueOffset, float saturationScale, float valueScale)
    {
        Color.RGBToHSV(source, out float hue, out float saturation, out float value);
        Color result = Color.HSVToRGB(
            Mathf.Repeat(hue + hueOffset, 1f),
            Mathf.Clamp01(saturation * saturationScale),
            Mathf.Clamp01(value * valueScale));
        result.a = source.a;
        return result;
    }

    private static Color AdjustSupportingColor(Color source, float saturationScale, float valueScale)
    {
        Color.RGBToHSV(source, out float hue, out float saturation, out float value);
        float restrainedSaturation = Mathf.Lerp(1f, saturationScale, 0.15f);
        float restrainedValue = Mathf.Lerp(1f, valueScale, 0.25f);
        Color result = Color.HSVToRGB(
            hue,
            Mathf.Clamp01(saturation * restrainedSaturation),
            Mathf.Clamp01(value * restrainedValue));
        result.a = source.a;
        return result;
    }

    private Color GetRaw(PaletteColor color)
    {
        switch (color)
        {
            case PaletteColor.Unspecified: return uiWhite;
            case PaletteColor.StoryEnemy: return storyEnemy;
            case PaletteColor.OriginalSpriteColor: return originalSpriteColor;
            case PaletteColor.StageRed: return stageRed;
            case PaletteColor.StageRedBright: return stageRedBright;
            case PaletteColor.StageRedDark: return stageRedDark;
            case PaletteColor.PlanetUnregistered: return planetUnregistered;
            case PaletteColor.PlanetUnregisteredDark: return planetUnregisteredDark;
            case PaletteColor.UiWhite: return uiWhite;
            case PaletteColor.UiSubText: return uiSubText;
            case PaletteColor.UiInactive: return uiInactive;
            case PaletteColor.SpaceBackground: return spaceBackground;
            case PaletteColor.SpaceBackgroundDeep: return spaceBackgroundDeep;
            case PaletteColor.PopupBackdrop: return popupBackdrop;
            case PaletteColor.ShieldBarrier: return shieldBarrier;
            case PaletteColor.ShieldBarrierHighlight: return shieldBarrierHighlight;
            case PaletteColor.CharacterSpriteTint: return characterSpriteTint;
            case PaletteColor.DonutMint: return donutMint;
            case PaletteColor.ChocolateBrown: return chocolateBrown;
            case PaletteColor.SolarGold: return solarGold;
            case PaletteColor.CosmicGem: return cosmicGem;
            case PaletteColor.PlanetShard: return planetShard;
            default: return Color.magenta;
        }
    }
}

public enum PaletteColor
{
    Unspecified = -1,
    StageRed,
    StageRedBright,
    StageRedDark,
    PlanetUnregistered,
    PlanetUnregisteredDark,
    UiWhite,
    UiSubText,
    UiInactive,
    SpaceBackground,
    SpaceBackgroundDeep,
    PopupBackdrop,
    ShieldBarrier,
    ShieldBarrierHighlight,
    CharacterSpriteTint,
    DonutMint,
    ChocolateBrown,
    SolarGold,
    CosmicGem,
    PlanetShard,
    StoryEnemy,
    OriginalSpriteColor
}
