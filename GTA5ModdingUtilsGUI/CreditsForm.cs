using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    public partial class CreditsForm : Form
    {
        public CreditsForm()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ApplyDarkTheme();
            AddLogoToHeader();
        }

        private void linkGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://github.com/Larcius/gta5-modding-utils",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open browser: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ApplyDarkTheme()
        {
            Color windowBack = Color.FromArgb(6, 29, 36);
            Color panelBack = Color.FromArgb(13, 43, 51);
            Color textColor = Color.Gainsboro;
            Color accentColor = Color.FromArgb(0, 168, 135);

            this.BackColor = windowBack;
            this.ForeColor = textColor;

            if (lblTitle != null)
            {
                lblTitle.ForeColor = Color.White;
            }

            if (lblHeader != null)
            {
                lblHeader.ForeColor = accentColor;
            }

            if (lblUiAuthor != null)
            {
                lblUiAuthor.ForeColor = textColor;
            }

            if (lblCoreAuthor != null)
            {
                lblCoreAuthor.ForeColor = textColor;
            }

            if (linkGithub != null)
            {
                linkGithub.LinkColor = accentColor;
                linkGithub.ActiveLinkColor = Color.LightGreen;
                linkGithub.VisitedLinkColor = Color.FromArgb(0, 130, 100);
            }
        }

        private void AddLogoToHeader()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string assetsDir = Path.Combine(baseDir, "Assets");
                string logoPath = Path.Combine(assetsDir, "gta5_modding_utils_logo.png");

                if (!File.Exists(logoPath))
                {
                    return;
                }

                var logo = new PictureBox
                {
                    Name = "picCreditsLogo",
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Width = 64,
                    Height = 64,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                int margin = 16;
                logo.Location = new Point(this.ClientSize.Width - logo.Width - margin, margin);
                logo.Image = Image.FromFile(logoPath);
                this.Controls.Add(logo);
                logo.BringToFront();

                this.Resize += (s, e) =>
                {
                    if (!logo.IsDisposed)
                    {
                        logo.Left = this.ClientSize.Width - logo.Width - margin;
                    }
                };
            }
            catch
            {
                // Ignore logo failures; credits text is still visible.
            }
        }

    }
}
