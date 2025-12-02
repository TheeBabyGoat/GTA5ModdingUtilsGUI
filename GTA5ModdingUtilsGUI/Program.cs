using System;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Load user settings (theme, default tool path, etc.) before any forms are shown.
            SettingsManager.Load();

            Application.Run(new IntroForm());
        }
    }
}
