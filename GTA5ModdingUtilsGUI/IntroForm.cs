using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    public partial class IntroForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        public IntroForm()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void IntroForm_Shown(object? sender, EventArgs e)
        {
            // Ensure no caret is visible in the instructions box when the form first shows.
            HideCaret(txtInstructions.Handle);
        }

        private void txtInstructions_Enter(object? sender, EventArgs e)
        {
            // Prevent the blinking text caret by immediately hiding it and shifting focus.
            HideCaret(txtInstructions.Handle);
            btnContinue.Focus();
        }

        private void btnContinue_Click(object? sender, EventArgs e)
        {
            // Open the main tool window and close this intro page.
            using (var main = new MainForm())
            {
                Hide();
                main.ShowDialog(this);
            }
            Close();
        }

        private void btnExit_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}
