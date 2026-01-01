
using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    /// <summary>
    /// Simple settings dialog that lets the user pick a default gta5-modding-utils
    /// folder and choose a UI theme.
    /// </summary>
    public class SettingsForm : Form
    {
        private TextBox _txtToolRoot;
        private Button _btnBrowse;
        private ComboBox _cmbTheme;
        private Button _btnOk;
        private Button _btnCancel;

        public SettingsForm()
        {
            Text = "Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoSize = false;
            Width = 520;
            Height = 210;

            InitializeLayout();
            LoadFromSettings();

            // Apply the currently selected theme so this dialog matches the main UI.
            ApplyTheme(SettingsManager.Current.Theme);
        }

        private void InitializeLayout()
        {
            var lblPath = new Label
            {
                Text = "Default Gta5-Modding-Utils folder:",
                AutoSize = true,
                Left = 12,
                Top = 18
            };

            _txtToolRoot = new TextBox
            {
                Left = 12,
                Top = lblPath.Bottom + 4,
                Width = 380,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _btnBrowse = new Button
            {
                Text = "Browse...",
                Left = _txtToolRoot.Right + 8,
                Top = _txtToolRoot.Top - 1,
                Width = 90,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnBrowse.Click += OnBrowseClick;

            var lblTheme = new Label
            {
                Text = "UI theme:",
                AutoSize = true,
                Left = 12,
                Top = _txtToolRoot.Bottom + 16
            };

            _cmbTheme = new ComboBox
            {
                Left = 12,
                Top = lblTheme.Bottom + 4,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true
            };

            // Populate with available themes.
            _cmbTheme.Items.Add(AppTheme.DarkTeal);
            _cmbTheme.Items.Add(AppTheme.Light);
            _cmbTheme.Items.Add(AppTheme.DarkGray);
            _cmbTheme.Items.Add(AppTheme.Maroon);
            _cmbTheme.Items.Add(AppTheme.MidnightPurple);
            _cmbTheme.Items.Add(AppTheme.TurquoiseBlue);
            _cmbTheme.Items.Add(AppTheme.WoodGrain);
            _cmbTheme.Items.Add(AppTheme.SkyClouds);
            _cmbTheme.Items.Add(AppTheme.Volcanic);
            _cmbTheme.Items.Add(AppTheme.Ashes);

            // Render enums with user-friendly names while still storing AppTheme values.
            _cmbTheme.Format += (s, e) =>
            {
                if (e.ListItem is AppTheme t)
                {
                    e.Value = ThemeHelper.GetDisplayName(t);
                }
            };

            // When the selection changes, update the dialog's theme immediately
            // so the user can preview it without closing the window.
            _cmbTheme.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbTheme.SelectedItem is AppTheme t)
                {
                    ApplyTheme(t);
                }
            };

            _btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Width = 80,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            _btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width = 80,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            // Position buttons near the bottom-right corner.
            int btnBottom = ClientSize.Height - 20;
            _btnOk.Top = btnBottom - _btnOk.Height;
            _btnCancel.Top = btnBottom - _btnCancel.Height;

            _btnCancel.Left = ClientSize.Width - 20 - _btnCancel.Width;
            _btnOk.Left = _btnCancel.Left - 8 - _btnOk.Width;

            Controls.Add(lblPath);
            Controls.Add(_txtToolRoot);
            Controls.Add(_btnBrowse);
            Controls.Add(lblTheme);
            Controls.Add(_cmbTheme);
            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;

            // Handle OK click to persist to SettingsManager.
            _btnOk.Click += (_, __) =>
            {
                SaveToSettings();
            };

            // Adjust positions if the form is resized (mostly for DPI scaling).
            Resize += (s, e) =>
            {
                _txtToolRoot.Width = ClientSize.Width - 12 - 8 - _btnBrowse.Width - 12;
                _btnBrowse.Left = _txtToolRoot.Right + 8;

                int bottom = ClientSize.Height - 20;
                _btnOk.Top = bottom - _btnOk.Height;
                _btnCancel.Top = bottom - _btnCancel.Height;

                _btnCancel.Left = ClientSize.Width - 20 - _btnCancel.Width;
                _btnOk.Left = _btnCancel.Left - 8 - _btnOk.Width;
            };
        }


        private void ApplyTheme(AppTheme theme)
        {
            ThemePalette palette = ThemeHelper.GetPalette(theme);

            var windowBack = palette.WindowBack;
            var inputBack = palette.InputBack;
            var textColor = palette.TextColor;
            var accentColor = palette.AccentColor;
            var secondaryButton = palette.SecondaryButton;
            var borderColor = palette.BorderColor;

            // Form background / foreground
            this.BackColor = windowBack;
            this.ForeColor = textColor;

            // Walk all controls and apply basic styling.
            void ThemeControl(Control c)
            {
                if (c is Label lbl)
                {
                    lbl.ForeColor = textColor;
                }
                else if (c is TextBox tb)
                {
                    tb.BackColor = inputBack;
                    tb.ForeColor = textColor;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = inputBack;
                    cb.ForeColor = textColor;
                }
                else if (c is Button btn)
                {
                    // Default buttons to the secondary style; we'll override the OK button below.
                    btn.BackColor = secondaryButton;
                    btn.ForeColor = textColor;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = borderColor;
                }

                foreach (Control child in c.Controls)
                {
                    ThemeControl(child);
                }
            }

            foreach (Control c in this.Controls)
            {
                ThemeControl(c);
            }

            // Highlight the OK button with the accent color.
            if (_btnOk != null)
            {
                _btnOk.BackColor = accentColor;
                _btnOk.ForeColor = Color.White;
                _btnOk.FlatStyle = FlatStyle.Flat;
                _btnOk.FlatAppearance.BorderSize = 1;
                _btnOk.FlatAppearance.BorderColor = borderColor;
            }
        }

        private void LoadFromSettings()
        {
            _txtToolRoot.Text = SettingsManager.Current.Gta5ModdingUtilsPath ?? string.Empty;

            // Ensure the current theme exists in the combo box.
            var currentTheme = SettingsManager.Current.Theme;
            int index = -1;
            for (int i = 0; i < _cmbTheme.Items.Count; i++)
            {
                if (_cmbTheme.Items[i] is AppTheme t && t == currentTheme)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                _cmbTheme.SelectedIndex = index;
            }
            else
            {
                _cmbTheme.SelectedItem = AppTheme.DarkTeal;
            }
        }

        private void SaveToSettings()
        {
            string path = _txtToolRoot.Text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                SettingsManager.Current.Gta5ModdingUtilsPath = null;
            }
            else
            {
                SettingsManager.Current.Gta5ModdingUtilsPath = path;
            }

            if (_cmbTheme.SelectedItem is AppTheme theme)
            {
                SettingsManager.Current.Theme = theme;
            }
        }

        private void OnBrowseClick(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Select the gta5-modding-utils folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (!string.IsNullOrWhiteSpace(_txtToolRoot.Text) && Directory.Exists(_txtToolRoot.Text))
            {
                fbd.SelectedPath = _txtToolRoot.Text;
            }

            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                _txtToolRoot.Text = fbd.SelectedPath;
            }
        }
    }
}
