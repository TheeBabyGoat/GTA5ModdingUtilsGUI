using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    public partial class IntroForm : Form
    {
        public IntroForm()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ApplyTheme(SettingsManager.Current.Theme);
            AddLogoBanner();
        }

        private void IntroForm_Shown(object? sender, EventArgs e)
        {
            // Move focus away from the instructions box so no caret is shown.
            txtInstructions.SelectionStart = 0;
            txtInstructions.SelectionLength = 0;
            btnContinue.Focus();
            this.ActiveControl = btnContinue;
        }

        private void txtInstructions_Enter(object? sender, EventArgs e)
        {
            // Keep focus on the Continue button to avoid showing a text caret.
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



        private AppTheme _currentTheme = AppTheme.DarkTeal;

        private void ApplyTheme(AppTheme theme)
        {
            _currentTheme = theme;

            ThemePalette palette = ThemeHelper.GetPalette(theme);

            var windowBack = palette.WindowBack;
            var panelBack = palette.GroupBack;
            var inputBack = palette.InputBack;
            var textColor = palette.TextColor;
            var accentColor = palette.AccentColor;
            var secondaryButton = palette.SecondaryButton;
            var borderColor = palette.BorderColor;

            this.BackColor = windowBack;
            this.ForeColor = textColor;

            if (panelMain != null)
            {
                panelMain.BackColor = windowBack;
            }

            if (panelBottom != null)
            {
                panelBottom.BackColor = panelBack;
            }

            if (txtInstructions != null)
            {
                txtInstructions.BackColor = panelBack;
                txtInstructions.ForeColor = textColor;
                txtInstructions.BorderStyle = BorderStyle.None;
            }

            if (lblTitle != null)
            {
                lblTitle.ForeColor = textColor;
            }

            if (lblSubtitle != null)
            {
                // Slightly dimmer text for subtitle in dark themes.
                lblSubtitle.ForeColor = (theme == AppTheme.Light)
                    ? Color.DimGray
                    : Color.FromArgb(190, 210, 220);
            }

            if (lblImportant != null)
            {
                // Accent color draws attention to the warning / important text.
                lblImportant.ForeColor = accentColor;
            }
            // Buttons
            StylePrimaryButton(btnContinue, accentColor, Color.White);
            StyleSecondaryButton(btnExit, secondaryButton, textColor, borderColor);
        }

        private static void StylePrimaryButton(Button? button, Color backColor, Color foreColor)
        {
            if (button == null) return;

            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
        }

        private static void StyleSecondaryButton(Button? button, Color backColor, Color foreColor, Color borderColor)
        {
            if (button == null) return;

            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = borderColor;
        }

                        private void AddLogoBanner()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string assetsDir = Path.Combine(baseDir, "Assets");
                string logoPath = Path.Combine(assetsDir, "gta5_modding_utils_logo.png");

                if (!File.Exists(logoPath) || panelMain == null)
                {
                    return;
                }

                var logo = new PictureBox
                {
                    Name = "picIntroLogo",
                    SizeMode = PictureBoxSizeMode.Zoom,
                    // More compact so it stays subtle in the corner.
                    Width = 140,
                    Height = 96,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                int marginRight = 16;
                int marginTop = 12;
                int panelWidth = panelMain.ClientSize.Width;
                logo.Location = new Point(panelWidth - logo.Width - marginRight, marginTop);

                logo.Image = Image.FromFile(logoPath);
                panelMain.Controls.Add(logo);
                logo.BringToFront();

                this.Resize += (s, e) =>
                {
                    if (!logo.IsDisposed && panelMain != null)
                    {
                        int pw = panelMain.ClientSize.Width;
                        logo.Left = pw - logo.Width - marginRight;
                    }
                };
            }
            catch
            {
                // If anything goes wrong we just skip the banner;
                // the rest of the intro window still works.
            }
        }
    }
}
