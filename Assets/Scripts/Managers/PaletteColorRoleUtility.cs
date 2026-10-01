public static class PaletteColorRoleUtility
{
    /// <summary>
    /// Returns the palette role for content that is either registered or still unregistered.
    /// </summary>
    public static PaletteColor GetRegistrationColor(PaletteColor colorRole, bool isRegistered)
    {
        if (isRegistered) return colorRole;

        switch (colorRole)
        {
            case PaletteColor.StageRed:
            case PaletteColor.StageRedBright:
                return PaletteColor.PlanetUnregistered;
            case PaletteColor.StageRedDark:
                return PaletteColor.PlanetUnregisteredDark;
            case PaletteColor.UiSubText:
                return PaletteColor.UiWhite;
            default:
                return colorRole;
        }
    }
}
