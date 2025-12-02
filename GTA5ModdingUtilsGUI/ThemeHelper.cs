
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
