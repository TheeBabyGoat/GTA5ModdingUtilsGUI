
using System.Drawing;

namespace GTA5ModdingUtilsGUI
{
    /// <summary>
    /// Color palette for a theme.
    /// </summary>
    public class ThemePalette
    {
        public Color WindowBack { get; set; }
        public Color GroupBack { get; set; }
        public Color InputBack { get; set; }
        public Color TextColor { get; set; }
        public Color AccentColor { get; set; }
        public Color SecondaryButton { get; set; }
        public Color BorderColor { get; set; }
        public Color LogBack { get; set; }
        public Color LogText { get; set; }
    }

    /// <summary>
    /// Central place where we define all visual themes.
    /// </summary>
    public static class ThemeHelper
    {
        /// <summary>
        /// Friendly display name for UI pickers.
        /// </summary>
        public static string GetDisplayName(AppTheme theme)
        {
            return theme switch
            {
                AppTheme.DarkTeal => "Dark Teal",
                AppTheme.Light => "Light",
                AppTheme.DarkGray => "Dark Gray",
                AppTheme.Maroon => "Maroon",
                AppTheme.MidnightPurple => "Midnight Purple",
                AppTheme.TurquoiseBlue => "Turquoise Blue",
                AppTheme.WoodGrain => "Wood Grain",
                AppTheme.SkyClouds => "Sky Clouds",
                AppTheme.Volcanic => "Volcanic",
                AppTheme.Ashes => "Ashes",
                _ => theme.ToString()
            };
        }

        /// <summary>
        /// Whether a theme should be treated as "light" for contrast heuristics.
        /// </summary>
        public static bool IsLightTheme(AppTheme theme)
        {
            // Use palette brightness so new themes don't require special-casing.
            try
            {
                var p = GetPalette(theme);
                return p.WindowBack.GetBrightness() >= 0.55f;
            }
            catch
            {
                return theme == AppTheme.Light;
            }
        }

        public static ThemePalette GetPalette(AppTheme theme)
        {
            switch (theme)
            {
                case AppTheme.Light:
                    return new ThemePalette
                    {
                        WindowBack = SystemColors.ControlLightLight,
                        GroupBack = SystemColors.Control,
                        InputBack = Color.White,
                        TextColor = Color.Black,
                        AccentColor = Color.FromArgb(0, 120, 215),
                        SecondaryButton = Color.FromArgb(230, 230, 230),
                        BorderColor = Color.FromArgb(200, 200, 200),
                        LogBack = Color.White,
                        LogText = Color.Black
                    };

                case AppTheme.DarkGray:
                    return new ThemePalette
                    {
                        WindowBack = Color.FromArgb(32, 32, 32),
                        GroupBack = Color.FromArgb(45, 45, 48),
                        InputBack = Color.FromArgb(51, 51, 55),
                        TextColor = Color.Gainsboro,
                        AccentColor = Color.FromArgb(0, 122, 204),
                        SecondaryButton = Color.FromArgb(63, 63, 70),
                        BorderColor = Color.FromArgb(80, 80, 80),
                        LogBack = Color.FromArgb(15, 15, 15),
                        LogText = Color.FromArgb(220, 220, 220)
                    };

                case AppTheme.Maroon:
                    return new ThemePalette
                    {
                        WindowBack = Color.FromArgb(43, 10, 18),
                        GroupBack = Color.FromArgb(58, 15, 25),
                        InputBack = Color.FromArgb(74, 23, 34),
                        TextColor = Color.Gainsboro,
                        AccentColor = Color.FromArgb(163, 13, 45),
                        SecondaryButton = Color.FromArgb(85, 33, 45),
                        BorderColor = Color.FromArgb(122, 59, 73),
                        LogBack = Color.FromArgb(26, 6, 11),
                        LogText = Color.FromArgb(255, 214, 223)
                    };

                case AppTheme.MidnightPurple:
                    return new ThemePalette
                    {
                        WindowBack = Color.FromArgb(17, 8, 31),
                        GroupBack = Color.FromArgb(26, 13, 46),
                        InputBack = Color.FromArgb(35, 18, 61),
                        TextColor = Color.Gainsboro,
                        AccentColor = Color.FromArgb(124, 58, 237),
                        SecondaryButton = Color.FromArgb(42, 22, 72),
                        BorderColor = Color.FromArgb(75, 43, 117),
                        LogBack = Color.FromArgb(11, 4, 20),
                        LogText = Color.FromArgb(233, 213, 255)
                    };

                case AppTheme.TurquoiseBlue:
                    return new ThemePalette
                    {
                        WindowBack = Color.FromArgb(4, 28, 44),
                        GroupBack = Color.FromArgb(8, 48, 70),
                        InputBack = Color.FromArgb(11, 59, 86),
                        TextColor = Color.Gainsboro,
                        AccentColor = Color.FromArgb(0, 168, 181),
                        SecondaryButton = Color.FromArgb(13, 66, 94),
                        BorderColor = Color.FromArgb(44, 107, 135),
                        LogBack = Color.FromArgb(3, 19, 31),
                        LogText = Color.FromArgb(191, 252, 255)
                    };

                case AppTheme.WoodGrain:
                    return new ThemePalette
                    {
                        WindowBack = Color.FromArgb(42, 27, 14),
                        GroupBack = Color.FromArgb(58, 37, 19),
                        InputBack = Color.FromArgb(74, 47, 24),
                        TextColor = Color.FromArgb(243, 231, 211),
                        AccentColor = Color.FromArgb(196, 127, 58),
                        SecondaryButton = Color.FromArgb(85, 53, 27),
                        BorderColor = Color.FromArgb(122, 90, 58),
                        LogBack = Color.FromArgb(28, 17, 8),
                        LogText = Color.FromArgb(255, 232, 199)
                    };

                case AppTheme.SkyClouds:
                    return new ThemePalette
                    {
                        WindowBack = Color.FromArgb(232, 244, 255),
                        GroupBack = Color.FromArgb(243, 250, 255),
                        InputBack = Color.White,
                        TextColor = Color.FromArgb(16, 42, 67),
                        AccentColor = Color.FromArgb(47, 128, 237),
                        SecondaryButton = Color.FromArgb(217, 236, 255),
                        BorderColor = Color.FromArgb(182, 215, 255),
                        LogBack = Color.White,
                        LogText = Color.FromArgb(16, 42, 67)
                    };

                case AppTheme.Volcanic:
                    return new ThemePalette
                    {
                        WindowBack = Color.FromArgb(26, 15, 11),
                        GroupBack = Color.FromArgb(36, 20, 15),
                        InputBack = Color.FromArgb(45, 26, 20),
                        TextColor = Color.Gainsboro,
                        AccentColor = Color.FromArgb(255, 77, 28),
                        SecondaryButton = Color.FromArgb(51, 32, 26),
                        BorderColor = Color.FromArgb(90, 58, 49),
                        LogBack = Color.FromArgb(15, 8, 6),
                        LogText = Color.FromArgb(255, 210, 196)
                    };

                case AppTheme.Ashes:
                    return new ThemePalette
                    {
                        WindowBack = Color.FromArgb(28, 29, 31),
                        GroupBack = Color.FromArgb(42, 44, 47),
                        InputBack = Color.FromArgb(51, 54, 58),
                        TextColor = Color.FromArgb(225, 225, 225),
                        AccentColor = Color.FromArgb(138, 160, 181),
                        SecondaryButton = Color.FromArgb(59, 62, 67),
                        BorderColor = Color.FromArgb(85, 88, 94),
                        LogBack = Color.FromArgb(17, 18, 20),
                        LogText = Color.FromArgb(215, 221, 228)
                    };

                case AppTheme.DarkTeal:
                default:
                    return new ThemePalette
                    {
                        WindowBack = Color.FromArgb(6, 29, 36),
                        GroupBack = Color.FromArgb(13, 43, 51),
                        InputBack = Color.FromArgb(21, 47, 56),
                        TextColor = Color.Gainsboro,
                        AccentColor = Color.FromArgb(0, 168, 135),
                        SecondaryButton = Color.FromArgb(30, 52, 60),
                        BorderColor = Color.FromArgb(70, 92, 100),
                        LogBack = Color.FromArgb(6, 19, 26),
                        LogText = Color.FromArgb(180, 255, 220)
                    };
            }
        }
    }
}
