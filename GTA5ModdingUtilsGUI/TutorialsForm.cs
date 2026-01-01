using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    internal sealed class TutorialsForm : Form
    {
        private readonly Panel _header;
        private readonly Label _lblTitle;
        private readonly Label _lblSub;
        private readonly FlowLayoutPanel _flow;
        private readonly List<TutorialLink> _tutorials = new();

        private AppTheme _currentTheme = AppTheme.DarkTeal;

        public TutorialsForm()
        {
            Text = "Tutorials";
            MinimumSize = new Size(900, 600);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                Padding = new Padding(16, 14, 16, 8)
            };

            var lblTitle = new Label
            {
                AutoSize = true,
                Text = "Tutorials",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point)
            };

            var lblSub = new Label
            {
                AutoSize = true,
                Top = 38,
                Left = 2,
                Text = "Click a thumbnail or button to open the YouTube tutorial.",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point)
            };

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSub);

            _header = header;
            _lblTitle = lblTitle;
            _lblSub = lblSub;

            _flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(16, 10, 16, 16)
            };

            Controls.Add(_flow);
            Controls.Add(header);

            BuildTutorialList();
            PopulateCards();

            // Apply the app's currently selected theme.
            ApplyTheme(SettingsManager.Current.Theme);
            Shown += (_, __) => ApplyTheme(SettingsManager.Current.Theme);

            // Keep cards sized nicely on resize.
            _flow.SizeChanged += (_, __) => ResizeCards();
        }

        private void ApplyTheme(AppTheme theme)
        {
            _currentTheme = theme;
            ThemePalette palette = ThemeHelper.GetPalette(theme);

            var windowBack = palette.WindowBack;
            var groupBack = palette.GroupBack;
            var inputBack = palette.InputBack;
            var textColor = palette.TextColor;
            var accentColor = palette.AccentColor;
            var secondaryButton = palette.SecondaryButton;
            var borderColor = palette.BorderColor;

            BackColor = windowBack;
            ForeColor = textColor;

            if (_header != null)
            {
                _header.BackColor = windowBack;
            }

            if (_flow != null)
            {
                _flow.BackColor = windowBack;
            }

            if (_lblTitle != null)
            {
                _lblTitle.ForeColor = textColor;
            }

            if (_lblSub != null)
            {
                _lblSub.ForeColor = ThemeHelper.IsLightTheme(theme)
                    ? Color.DimGray
                    : Color.FromArgb(190, 210, 220);
            }

            // Theme tutorial cards.
            foreach (Control c in _flow.Controls)
            {
                if (c is ThemedBorderPanel card)
                {
                    card.BackColor = groupBack;
                    card.ForeColor = textColor;
                    card.BorderColor = borderColor;

                    foreach (Control child in card.Controls)
                    {
                        if (child is Label lbl)
                        {
                            lbl.ForeColor = textColor;
                        }
                        else if (child is PictureBox pb)
                        {
                            pb.BackColor = inputBack;
                        }
                        else if (child is Button btn)
                        {
                            // Tag-based role: "primary" (Open) vs "secondary" (Copy).
                            var role = btn.Tag as string;
                            if (string.Equals(role, "primary", StringComparison.OrdinalIgnoreCase))
                            {
                                StylePrimaryButton(btn, accentColor, Color.White, borderColor);
                            }
                            else
                            {
                                StyleSecondaryButton(btn, secondaryButton, textColor, borderColor);
                            }
                        }
                    }

                    card.Invalidate();
                }
            }
        }

        private static void StylePrimaryButton(Button? button, Color backColor, Color foreColor, Color borderColor)
        {
            if (button == null) return;

            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = borderColor;
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

        private void BuildTutorialList()
        {
            _tutorials.Clear();

            _tutorials.Add(new TutorialLink
            {
                Title = "Pipeline Setup",
                Description = "Walkthrough for setting up the Python toolkit and UI.",
                Url = "https://www.youtube.com/watch?v=YusnR7epWU8&list=PLxBWETS7YxfJm2R8_mupcwT5OTkB2iZDG",
                ThumbnailFileName = "pipeline_setup.png"
            });
            _tutorials.Add(new TutorialLink
            {
                Title = "Custom LOD Texture Creation",
                Description = "Walkthrough for creating custom LOD textures with the Texture Creation workflow.",
                Url = "https://www.youtube.com/watch?v=YusnR7epWU8&list=PLxBWETS7YxfJm2R8_mupcwT5OTkB2iZDG",
                ThumbnailFileName = "custom_lod_texture_creation.png"
            });
            _tutorials.Add(new TutorialLink
            {
                Title = "Custom Mesh Lod/Slod Creation",
                Description = "Walkthrough for creating custom mesh lod/slod workflow.",
                Url = "https://www.youtube.com/watch?v=-OhFPuagcXo",
                ThumbnailFileName = "custom_lod_mesh_tutorial.png"
            });
            _tutorials.Add(new TutorialLink
            {
                Title = "Custom Seasons Texture Creation",
                Description = "Walkthrough for creating custom season textures with the Texture Creation workflow.",
                Url = "https://www.youtube.com/watch?v=Ttpxkp8ESE0&list=PLxBWETS7YxfJm2R8_mupcwT5OTkB2iZDG&index=2",
                ThumbnailFileName = "custom_season_textures.png"
            });
        }

        private void PopulateCards()
        {
            _flow.SuspendLayout();
            _flow.Controls.Clear();

            foreach (var t in _tutorials)
            {
                _flow.Controls.Add(CreateCard(t));
            }

            _flow.ResumeLayout(true);
            ResizeCards();
        }

        private void ResizeCards()
        {
            // Leave room for the flow panel padding and scrollbar.
            int targetWidth = Math.Max(300, _flow.ClientSize.Width - 28);

            foreach (Control c in _flow.Controls)
            {
                if (c is Panel p)
                {
                    p.Width = targetWidth;
                }
            }
        }

        private Control CreateCard(TutorialLink t)
        {
            var card = new ThemedBorderPanel
            {
                Height = 250,
                Margin = new Padding(0, 0, 0, 12),
                Padding = new Padding(12),
                BorderStyle = BorderStyle.None
            };

            // Thumbnail
            var thumb = new PictureBox
            {
                Size = new Size(320, 213),
                Location = new Point(12, 12),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };

            var loaded = TryLoadThumbnail(t.ThumbnailFileName);
            if (loaded != null)
            {
                thumb.Image = loaded;
            }

            // Text area
            int textLeft = thumb.Right + 14;

            var lblTitle = new Label
            {
                AutoSize = false,
                Text = t.Title,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),
                Location = new Point(textLeft, 12),
                Size = new Size(600, 28),
                Cursor = Cursors.Hand
            };

            var lblDesc = new Label
            {
                AutoSize = false,
                Text = t.Description,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(textLeft, 46),
                Size = new Size(600, 64)
            };

            var btnOpen = new Button
            {
                Text = "Open on YouTube",
                Location = new Point(textLeft, 128),
                Size = new Size(150, 30)
            };

            var btnCopy = new Button
            {
                Text = "Copy Link",
                Location = new Point(textLeft + 160, 128),
                Size = new Size(110, 30)
            };

            // Tag buttons so the theme pass can style them consistently.
            btnOpen.Tag = "primary";
            btnCopy.Tag = "secondary";

            void Open() => OpenUrl(t.Url);

            thumb.Click += (_, __) => Open();
            lblTitle.Click += (_, __) => Open();
            btnOpen.Click += (_, __) => Open();

            btnCopy.Click += (_, __) =>
            {
                try
                {
                    Clipboard.SetText(t.Url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to copy link: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            card.Controls.Add(thumb);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblDesc);
            card.Controls.Add(btnOpen);
            card.Controls.Add(btnCopy);

            // Ensure title/desc/button widths follow card width.
            card.SizeChanged += (_, __) =>
            {
                int available = Math.Max(200, card.ClientSize.Width - textLeft - 16);
                lblTitle.Width = available;
                lblDesc.Width = available;
            };

            return card;
        }

        private sealed class ThemedBorderPanel : Panel
        {
            public Color BorderColor { get; set; } = SystemColors.ControlDark;

            public ThemedBorderPanel()
            {
                DoubleBuffered = true;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                var rect = ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;

                using var pen = new Pen(BorderColor, 1);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private static Image? TryLoadThumbnail(string thumbnailFileName)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(baseDir, "Assets", "Tutorials", thumbnailFileName);
                if (!File.Exists(path))
                {
                    return null;
                }

                // Load without locking the file.
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var img = Image.FromStream(fs);
                return new Bitmap(img);
            }
            catch
            {
                return null;
            }
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open link: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private sealed class TutorialLink
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string ThumbnailFileName { get; set; } = string.Empty;
        }
    }
}
