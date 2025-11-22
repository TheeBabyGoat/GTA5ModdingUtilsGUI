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
            Application.Run(new IntroForm());
        }
    }
}
