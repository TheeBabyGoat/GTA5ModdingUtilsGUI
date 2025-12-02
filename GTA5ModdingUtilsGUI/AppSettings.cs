
using System;
using System.IO;
using System.Text.Json;

namespace GTA5ModdingUtilsGUI
{
    /// <summary>
    /// Available UI themes for the application.
    /// </summary>
    public enum AppTheme
    {
        DarkTeal,
        Light,
        DarkGray
    }

    /// <summary>
    /// Serializable container for user settings that should persist between runs.
    /// </summary>
    public class UserSettings
    {
        public string? Gta5ModdingUtilsPath { get; set; }
        public AppTheme Theme { get; set; } = AppTheme.DarkTeal;
    }

    /// <summary>
    /// Simple JSON based persistence for <see cref="UserSettings"/>.
    /// </summary>
    public static class SettingsManager
    {
        private static readonly string SettingsFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GTA5ModdingUtilsGUI.settings.json");

        public static UserSettings Current { get; private set; } = new UserSettings();

        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var loaded = JsonSerializer.Deserialize<UserSettings>(json);
                    if (loaded != null)
                    {
                        Current = loaded;
                    }
                }
            }
            catch
            {
                // If anything goes wrong, fall back to defaults.
                Current = new UserSettings();
            }
        }

        public static void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Current, options);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
                // Ignore persistence errors – the tool should still run.
            }
        }
    }
}
