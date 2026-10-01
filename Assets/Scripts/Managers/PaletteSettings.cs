using System;
using UnityEngine;

public static class PaletteSettings
{
    private const string PreferenceKey = "Display.PaletteMode";
    private const string LegacySoftPreferenceKey = "Display.SoftPalette";
    private const string CustomAccentRedKey = "Display.CustomAccent.Red";
    private const string CustomAccentGreenKey = "Display.CustomAccent.Green";
    private const string CustomAccentBlueKey = "Display.CustomAccent.Blue";
    private static readonly Color DefaultAccent = new Color(0.933f, 0.290f, 0.224f);

    public static PaletteMode Mode
    {
        get
        {
            if (PlayerPrefs.HasKey(PreferenceKey))
                return (PaletteMode)Mathf.Clamp(PlayerPrefs.GetInt(PreferenceKey), 0, 2);
            return PlayerPrefs.GetInt(LegacySoftPreferenceKey, 0) == 1
                ? PaletteMode.Soft
                : PaletteMode.Standard;
        }
    }

    public static Color CustomAccent
    {
        get
        {
            if (!PlayerPrefs.HasKey(CustomAccentRedKey)) return DefaultAccent;
            return new Color(
                PlayerPrefs.GetFloat(CustomAccentRedKey),
                PlayerPrefs.GetFloat(CustomAccentGreenKey),
                PlayerPrefs.GetFloat(CustomAccentBlueKey));
        }
    }

    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetEvents() => Changed = null;

    public static void SetMode(PaletteMode mode)
    {
        if (Mode == mode && PlayerPrefs.HasKey(PreferenceKey)) return;
        PlayerPrefs.SetInt(PreferenceKey, (int)mode);
        PlayerPrefs.DeleteKey(LegacySoftPreferenceKey);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static void SetCustomAccent(Color color)
    {
        PlayerPrefs.SetFloat(CustomAccentRedKey, Mathf.Clamp01(color.r));
        PlayerPrefs.SetFloat(CustomAccentGreenKey, Mathf.Clamp01(color.g));
        PlayerPrefs.SetFloat(CustomAccentBlueKey, Mathf.Clamp01(color.b));
        PlayerPrefs.SetInt(PreferenceKey, (int)PaletteMode.Custom);
        PlayerPrefs.DeleteKey(LegacySoftPreferenceKey);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static bool TrySetCustomAccentHex(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor)) return false;

        string normalized = htmlColor.Trim();
        if (!normalized.StartsWith("#")) normalized = "#" + normalized;
        if (!ColorUtility.TryParseHtmlString(normalized, out Color color)) return false;
        SetCustomAccent(color);
        return true;
    }
}

public enum PaletteMode
{
    Standard,
    Soft,
    Custom
}
