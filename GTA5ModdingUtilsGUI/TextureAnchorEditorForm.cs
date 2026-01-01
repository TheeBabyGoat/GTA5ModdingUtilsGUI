
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    /// <summary>
    /// Simple texture anchor editor for seasonal foliage thinning.
    ///
    /// This form lets the user load a texture atlas, place circular anchors,
    /// and save them to a JSON file that the Python tools can consume.
    /// </summary>
    public class TextureAnchorEditorForm : Form
    {
        private readonly string _imagePath;
        private readonly string _configPath;

        private Bitmap? _bitmap;
        private readonly List<TextureAnchor> _anchors = new();

        private PictureBox _picture;
        private ListBox _listAnchors;
        private NumericUpDown _numRadius;
        private NumericUpDown _numStrength;
        private CheckBox _chkSpring;
        private CheckBox _chkFall;
        private CheckBox _chkWinter;
        private Button _btnDelete;
        private Button _btnClear;
        private Button _btnSave;
        private Button _btnClose;
        private Label _lblInfo;
        private Label _lblPath;

        private bool _updatingControls;

        private TextureAnchor? _pendingPolygonAnchor;
        private float _zoom = 1.0f;
        private PointF _panOffset = new PointF(0f, 0f);
        private bool _isPanning;
        private Point _panStartMouse;
        private PointF _panStartOffset;




        private class TextureAnchor
        {
            public double X;
            public double Y;
            public double Radius;
            public double Strength;
            public bool SeasonSpring;
            public bool SeasonFall;
            public bool SeasonWinter;

            // Normalized polygon points in [0,1]x[0,1] defining the anchor region.
            public List<PointF> Points { get; } = new List<PointF>();

            public bool HasPolygon => Points != null && Points.Count > 0;

            public void RecomputeFromPolygon()
            {
                if (!HasPolygon)
                    return;

                double sumX = 0.0;
                double sumY = 0.0;
                foreach (PointF p in Points)
                {
                    sumX += p.X;
                    sumY += p.Y;
                }

                double cx = sumX / Points.Count;
                double cy = sumY / Points.Count;

                X = cx;
                Y = cy;

                double maxR2 = 0.0;
                foreach (PointF p in Points)
                {
                    double dx = p.X - cx;
                    double dy = p.Y - cy;
                    double r2 = dx * dx + dy * dy;
                    if (r2 > maxR2)
                        maxR2 = r2;
                }

                Radius = Math.Sqrt(maxR2);
            }

            public void RescalePolygonRadius(double newRadius)
            {
                if (!HasPolygon)
                {
                    Radius = newRadius;
                    return;
                }

                if (Radius <= 0.0)
                    return;

                double scale = newRadius / Radius;
                for (int i = 0; i < Points.Count; i++)
                {
                    PointF p = Points[i];
                    double dx = p.X - X;
                    double dy = p.Y - Y;
                    double newX = X + dx * scale;
                    double newY = Y + dy * scale;

                    // Clamp back into [0, 1].
                    newX = Math.Max(0.0, Math.Min(1.0, newX));
                    newY = Math.Max(0.0, Math.Min(1.0, newY));
                    Points[i] = new PointF((float)newX, (float)newY);
                }

                // Update stored radius based on new geometry.
                RecomputeFromPolygon();
            }

            public override string ToString()
            {
                string seasons = string.Empty;
                if (SeasonSpring) seasons += "S";
                if (SeasonFall) seasons += string.IsNullOrEmpty(seasons) ? "F" : ",F";
                if (SeasonWinter) seasons += string.IsNullOrEmpty(seasons) ? "W" : ",W";
                if (string.IsNullOrEmpty(seasons))
                    seasons = "-";

                string shape = HasPolygon ? string.Format("Poly({0})", Points.Count) : "Circle";
                return string.Format("{0} ({1:0.00}, {2:0.00}) r={3:0.00} [{4}]",
                    shape, X, Y, Radius, seasons);
            }
        }

        private class AnchorPointRecord
        {
            public double x { get; set; }
            public double y { get; set; }
        }

        private class AnchorRecord
        {
            public double x { get; set; }
            public double y { get; set; }
            public double radius { get; set; }
            public double strength { get; set; }
            public string[] seasons { get; set; } = Array.Empty<string>();
            public AnchorPointRecord[]? polygon { get; set; }
        }


        private class AnchorConfig
        {
            public List<AnchorRecord> anchors { get; set; } = new List<AnchorRecord>();
        }

        public TextureAnchorEditorForm(string imagePath)
        {
            _imagePath = imagePath;
            string dir = Path.GetDirectoryName(imagePath) ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(imagePath);
            _configPath = Path.Combine(dir, baseName + "_anchors.json");

            Text = "Texture Anchor Editor";
            Width = 900;
            Height = 640;
            MinimumSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterParent;

            InitializeUi();
            LoadImage();
            LoadAnchorsFromFile();

            ApplyTheme(SettingsManager.Current.Theme);
        }

        private void InitializeUi()
        {
            _picture = new PictureBox
            {
                Left = 10,
                Top = 10,
                Width = 560,
                Height = ClientSize.Height - 20,
                SizeMode = PictureBoxSizeMode.Normal,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom
            };
            _picture.Paint += Picture_Paint;
            _picture.MouseClick += Picture_MouseClick;
            _picture.MouseDown += Picture_MouseDown;
            _picture.MouseMove += Picture_MouseMove;
            _picture.MouseUp += Picture_MouseUp;
            _picture.MouseWheel += Picture_MouseWheel;
            _picture.MouseEnter += (s, e) => _picture.Focus();

            _lblPath = new Label
            {
                Left = 580,
                Top = 10,
                Width = ClientSize.Width - 590,
                Height = 36,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Text = _imagePath
            };

            _lblInfo = new Label
            {
                Left = 580,
                Top = 52,
                Width = ClientSize.Width - 590,
                Height = 48,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = false,
                Text = "Left-click on the texture preview to add polygon points; right-click to close the polygon. " +
                       "Use the mouse wheel to zoom and hold the middle mouse button to pan while zoomed. " +
                       "Anchors thin foliage inside the polygon for the selected seasons (Spring/Fall/Winter)."
            };

            _listAnchors = new ListBox
            {
                Left = 580,
                Top = 110,
                Width = ClientSize.Width - 590,
                Height = 180,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _listAnchors.SelectedIndexChanged += ListAnchors_SelectedIndexChanged;

            Label lblRadius = new Label
            {
                Left = 580,
                Top = 300,
                Width = 80,
                Text = "Radius:"
            };

            _numRadius = new NumericUpDown
            {
                Left = 660,
                Top = 296,
                Width = 80,
                DecimalPlaces = 2,
                Increment = 0.01M,
                Minimum = 0.01M,
                Maximum = 0.50M
            };
            _numRadius.ValueChanged += AnchorNumeric_ValueChanged;

            Label lblStrength = new Label
            {
                Left = 580,
                Top = 330,
                Width = 80,
                Text = "Strength:"
            };

            _numStrength = new NumericUpDown
            {
                Left = 660,
                Top = 326,
                Width = 80,
                DecimalPlaces = 2,
                Increment = 0.1M,
                Minimum = 0.10M,
                Maximum = 2.00M,
                Value = 1.00M
            };
            _numStrength.ValueChanged += AnchorNumeric_ValueChanged;

            _chkSpring = new CheckBox
            {
                Left = 580,
                Top = 360,
                Width = 80,
                Text = "Spring"
            };
            _chkSpring.CheckedChanged += AnchorSeason_CheckedChanged;

            _chkFall = new CheckBox
            {
                Left = 660,
                Top = 360,
                Width = 80,
                Text = "Fall"
            };
            _chkFall.CheckedChanged += AnchorSeason_CheckedChanged;

            _chkWinter = new CheckBox
            {
                Left = 740,
                Top = 360,
                Width = 80,
                Text = "Winter"
            };
            _chkWinter.CheckedChanged += AnchorSeason_CheckedChanged;

            _btnDelete = new Button
            {
                Left = 580,
                Top = 400,
                Width = 80,
                Text = "Delete"
            };
            _btnDelete.Click += (s, e) => DeleteSelectedAnchor();

            _btnClear = new Button
            {
                Left = 670,
                Top = 400,
                Width = 80,
                Text = "Clear"
            };
            _btnClear.Click += (s, e) =>
            {
                _anchors.Clear();
                _listAnchors.Items.Clear();
                _pendingPolygonAnchor = null;
                _picture.Invalidate();
            };

            _btnSave = new Button
            {
                Left = ClientSize.Width - 190,
                Top = ClientSize.Height - 40,
                Width = 80,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Text = "Save"
            };
            _btnSave.Click += (s, e) =>
            {
                SaveAnchorsToFile();
                DialogResult = DialogResult.OK;
            };

            _btnClose = new Button
            {
                Left = ClientSize.Width - 100,
                Top = ClientSize.Height - 40,
                Width = 80,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Text = "Close"
            };
            _btnClose.Click += (s, e) => Close();

            Controls.Add(_picture);
            Controls.Add(_lblPath);
            Controls.Add(_lblInfo);
            Controls.Add(_listAnchors);
            Controls.Add(lblRadius);
            Controls.Add(_numRadius);
            Controls.Add(lblStrength);
            Controls.Add(_numStrength);
            Controls.Add(_chkSpring);
            Controls.Add(_chkFall);
            Controls.Add(_chkWinter);
            Controls.Add(_btnDelete);
            Controls.Add(_btnClear);
            Controls.Add(_btnSave);
            Controls.Add(_btnClose);

            Resize += TextureAnchorEditorForm_Resize;
        }

        private void TextureAnchorEditorForm_Resize(object? sender, EventArgs e)
        {
            if (_picture != null)
            {
                _picture.Height = ClientSize.Height - 20;
                _picture.Invalidate();
            }

            if (_lblPath != null)
            {
                _lblPath.Width = ClientSize.Width - 590;
            }

            if (_lblInfo != null)
            {
                _lblInfo.Width = ClientSize.Width - 590;
            }

            if (_listAnchors != null)
            {
                _listAnchors.Width = ClientSize.Width - 590;
            }

            if (_btnSave != null)
            {
                _btnSave.Left = ClientSize.Width - 190;
                _btnSave.Top = ClientSize.Height - 40;
            }

            if (_btnClose != null)
            {
                _btnClose.Left = ClientSize.Width - 100;
                _btnClose.Top = ClientSize.Height - 40;
            }
        }

        private void LoadImage()
        {
            if (!File.Exists(_imagePath))
            {
                MessageBox.Show(this, "Texture file not found:\n" + _imagePath,
                    "Texture Anchor Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            try
            {
                _bitmap = new Bitmap(_imagePath);
                _picture.Image = null;
                _zoom = 1.0f;
                _panOffset = new PointF(0f, 0f);
                _picture.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to load texture:\n" + ex.Message,
                    "Texture Anchor Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void LoadAnchorsFromFile()
        {
            _anchors.Clear();
            _listAnchors.Items.Clear();

            if (!File.Exists(_configPath))
                return;

            try
            {
                string json = File.ReadAllText(_configPath);
                AnchorConfig? cfg = JsonSerializer.Deserialize<AnchorConfig>(json);
                if (cfg == null || cfg.anchors == null)
                    return;

                foreach (AnchorRecord rec in cfg.anchors)
                {
                    TextureAnchor a = new TextureAnchor
                    {
                        X = rec.x,
                        Y = rec.y,
                        Radius = rec.radius,
                        Strength = rec.strength,
                        SeasonSpring = HasSeason(rec, "spring"),
                        SeasonFall = HasSeason(rec, "fall"),
                        SeasonWinter = HasSeason(rec, "winter")
                    };

                    if (rec.polygon != null && rec.polygon.Length >= 3)
                    {
                        foreach (AnchorPointRecord pt in rec.polygon)
                        {
                            double px = Math.Max(0.0, Math.Min(1.0, pt.x));
                            double py = Math.Max(0.0, Math.Min(1.0, pt.y));
                            a.Points.Add(new PointF((float)px, (float)py));
                        }
                        a.RecomputeFromPolygon();
                    }

                    _anchors.Add(a);
                    _listAnchors.Items.Add(a);
                }
            }
            catch
            {
                // Ignore parse errors; the user can overwrite with new anchors.
            }
        }

        private static bool HasSeason(AnchorRecord rec, string season)
        {
            if (rec.seasons == null) return false;
            foreach (string s in rec.seasons)
            {
                if (string.Equals(s?.Trim(), season, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void SaveAnchorsToFile()
        {
            AnchorConfig cfg = new AnchorConfig();

            foreach (TextureAnchor a in _anchors)
            {
                // Keep center / radius consistent with polygon geometry, if present.
                if (a.HasPolygon)
                    a.RecomputeFromPolygon();

                List<string> seasons = new List<string>();
                if (a.SeasonSpring) seasons.Add("spring");
                if (a.SeasonFall) seasons.Add("fall");
                if (a.SeasonWinter) seasons.Add("winter");

                AnchorRecord rec = new AnchorRecord
                {
                    x = a.X,
                    y = a.Y,
                    radius = a.Radius,
                    strength = a.Strength,
                    seasons = seasons.ToArray()
                };

                if (a.HasPolygon && a.Points.Count >= 3)
                {
                    List<AnchorPointRecord> poly = new List<AnchorPointRecord>();
                    foreach (PointF p in a.Points)
                    {
                        poly.Add(new AnchorPointRecord
                        {
                            x = p.X,
                            y = p.Y
                        });
                    }
                    rec.polygon = poly.ToArray();
                }

                cfg.anchors.Add(rec);
            }

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(cfg, options);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to save anchor config:\n" + ex.Message,
                    "Texture Anchor Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void Picture_MouseClick(object? sender, MouseEventArgs e)
        {
            if (_bitmap == null || _picture.Width <= 0 || _picture.Height <= 0)
                return;

            if (!TryGetImageTransform(out float originX, out float originY, out float scale))
                return;

            int texW = _bitmap.Width;
            int texH = _bitmap.Height;

            // Convert from screen space back into normalized texture coordinates.
            float wx = (e.X - originX) / scale;
            float wy = (e.Y - originY) / scale;
            double xNorm = Math.Max(0.0, Math.Min(1.0, wx / (double)texW));
            double yNorm = Math.Max(0.0, Math.Min(1.0, wy / (double)texH));

            if (e.Button == MouseButtons.Left)
            {
                // Start or extend a polygon anchor.
                if (_pendingPolygonAnchor == null)
                {
                    _pendingPolygonAnchor = new TextureAnchor
                    {
                        X = xNorm,
                        Y = yNorm,
                        Radius = 0.08,
                        Strength = 1.0,
                        SeasonSpring = false,
                        SeasonFall = true,
                        SeasonWinter = true
                    };
                }

                _pendingPolygonAnchor.Points.Add(new PointF((float)xNorm, (float)yNorm));
                _pendingPolygonAnchor.RecomputeFromPolygon();

                _picture.Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Finish the current polygon, if any.
                if (_pendingPolygonAnchor != null)
                {
                    if (_pendingPolygonAnchor.Points.Count >= 3)
                    {
                        _pendingPolygonAnchor.RecomputeFromPolygon();
                        _anchors.Add(_pendingPolygonAnchor);
                        _listAnchors.Items.Add(_pendingPolygonAnchor);
                        _listAnchors.SelectedItem = _pendingPolygonAnchor;
                    }

                    _pendingPolygonAnchor = null;
                    _picture.Invalidate();
                }
            }
        }

        private bool TryGetImageTransform(out float originX, out float originY, out float scale)
        {
            originX = 0f;
            originY = 0f;
            scale = 0f;

            if (_bitmap == null || _picture == null)
                return false;

            int viewW = _picture.ClientSize.Width;
            int viewH = _picture.ClientSize.Height;
            if (viewW <= 0 || viewH <= 0)
                return false;

            int texW = _bitmap.Width;
            int texH = _bitmap.Height;
            if (texW <= 0 || texH <= 0)
                return false;

            float baseScale = Math.Min(viewW / (float)texW, viewH / (float)texH);
            if (baseScale <= 0f)
                return false;

            float s = baseScale * _zoom;
            float contentW = texW * s;
            float contentH = texH * s;

            originX = (viewW - contentW) / 2.0f + _panOffset.X;
            originY = (viewH - contentH) / 2.0f + _panOffset.Y;
            scale = s;
            return true;
        }

        private void Picture_Paint(object? sender, PaintEventArgs e)
        {
            if (_bitmap == null)
                return;

            if (!TryGetImageTransform(out float originX, out float originY, out float scale))
                return;

            int texW = _bitmap.Width;
            int texH = _bitmap.Height;

            // Clear background so panning does not leave trails.
            e.Graphics.Clear(_picture.BackColor);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float destW = texW * scale;
            float destH = texH * scale;
            e.Graphics.DrawImage(_bitmap, originX, originY, destW, destH);

            using Pen pen = new Pen(Color.Lime, 2.0f);
            using Pen penSelected = new Pen(Color.Orange, 2.0f);

            int selectedIndex = _listAnchors.SelectedIndex;

            foreach (TextureAnchor a in _anchors)
            {
                int index = _anchors.IndexOf(a);
                Pen usePen = (index == selectedIndex) ? penSelected : pen;
                DrawAnchorShape(e.Graphics, a, usePen, originX, originY, scale, texW, texH);
            }

            // Draw in-progress polygon (if any) on top.
            if (_pendingPolygonAnchor != null && _pendingPolygonAnchor.Points.Count >= 1)
            {
                DrawAnchorShape(e.Graphics, _pendingPolygonAnchor, penSelected, originX, originY, scale, texW, texH);
            }
        }

        private void DrawAnchorShape(
            Graphics g,
            TextureAnchor a,
            Pen pen,
            float originX,
            float originY,
            float scale,
            int texWidth,
            int texHeight)
        {
            float cx = originX + (float)(a.X * texWidth * scale);
            float cy = originY + (float)(a.Y * texHeight * scale);

            if (a.HasPolygon && a.Points.Count >= 2)
            {
                PointF[] pts = new PointF[a.Points.Count];
                for (int i = 0; i < a.Points.Count; i++)
                {
                    PointF p = a.Points[i];
                    float sx = originX + p.X * texWidth * scale;
                    float sy = originY + p.Y * texHeight * scale;
                    pts[i] = new PointF(sx, sy);
                }

                if (pts.Length >= 2)
                {
                    g.DrawPolygon(pen, pts);
                }
            }
            else
            {
                float radiusPx = (float)(a.Radius * Math.Max(texWidth, texHeight) * scale);
                g.DrawEllipse(pen, cx - radiusPx, cy - radiusPx, radiusPx * 2, radiusPx * 2);
            }

            g.DrawLine(pen, cx - 3, cy, cx + 3, cy);
            g.DrawLine(pen, cx, cy - 3, cx, cy + 3);
        }

        private void Picture_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _isPanning = true;
                _panStartMouse = e.Location;
                _panStartOffset = _panOffset;
                _picture.Cursor = Cursors.Hand;
            }
        }

        private void Picture_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                int dx = e.X - _panStartMouse.X;
                int dy = e.Y - _panStartMouse.Y;
                _panOffset = new PointF(_panStartOffset.X + dx, _panStartOffset.Y + dy);
                _picture.Invalidate();
            }
        }

        private void Picture_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle && _isPanning)
            {
                _isPanning = false;
                _picture.Cursor = Cursors.Default;
            }
        }

        private void Picture_MouseWheel(object? sender, MouseEventArgs e)
        {
            float oldZoom = _zoom;

            if (e.Delta > 0)
            {
                _zoom *= 1.1f;
            }
            else if (e.Delta < 0)
            {
                _zoom /= 1.1f;
            }

            _zoom = Math.Max(0.25f, Math.Min(8.0f, _zoom));

            if (Math.Abs(_zoom - oldZoom) > float.Epsilon)
            {
                _picture.Invalidate();
            }
        }

        private void ListAnchors_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_listAnchors.SelectedIndex < 0 || _listAnchors.SelectedIndex >= _anchors.Count)
                return;

            TextureAnchor a = _anchors[_listAnchors.SelectedIndex];

            _updatingControls = true;
            try
            {
                _numRadius.Value = (decimal)Math.Max(0.01, Math.Min(0.5, a.Radius));
                _numStrength.Value = (decimal)Math.Max(0.10, Math.Min(2.0, a.Strength));
                _chkSpring.Checked = a.SeasonSpring;
                _chkFall.Checked = a.SeasonFall;
                _chkWinter.Checked = a.SeasonWinter;
            }
            finally
            {
                _updatingControls = false;
            }

            _picture.Invalidate();
        }


        private void AnchorNumeric_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            int idx = _listAnchors.SelectedIndex;
            if (idx < 0 || idx >= _anchors.Count)
                return;

            TextureAnchor a = _anchors[idx];

            double newRadius = (double)_numRadius.Value;
            a.RescalePolygonRadius(newRadius);
            a.Strength = (double)_numStrength.Value;

            _listAnchors.Items[idx] = a;
            _picture.Invalidate();
        }

        private void AnchorSeason_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingControls)
                return;

            int idx = _listAnchors.SelectedIndex;
            if (idx < 0 || idx >= _anchors.Count)
                return;

            TextureAnchor a = _anchors[idx];
            a.SeasonSpring = _chkSpring.Checked;
            a.SeasonFall = _chkFall.Checked;
            a.SeasonWinter = _chkWinter.Checked;
            _listAnchors.Items[idx] = a;
        }

        private void DeleteSelectedAnchor()
        {
            int idx = _listAnchors.SelectedIndex;
            if (idx < 0 || idx >= _anchors.Count)
                return;

            _anchors.RemoveAt(idx);
            _listAnchors.Items.RemoveAt(idx);
            _picture.Invalidate();
        }

        private void ApplyTheme(AppTheme theme)
        {
            ThemePalette palette = ThemeHelper.GetPalette(theme);

            Color windowBack = palette.WindowBack;
            Color groupBack = palette.GroupBack;
            Color inputBack = palette.InputBack;
            Color textColor = palette.TextColor;
            Color accentColor = palette.AccentColor;
            Color secondaryButton = palette.SecondaryButton;
            Color borderColor = palette.BorderColor;

            BackColor = windowBack;
            ForeColor = textColor;

            if (_lblInfo != null)
                _lblInfo.ForeColor = textColor;
            if (_lblPath != null)
                _lblPath.ForeColor = textColor;

            if (_picture != null)
                _picture.BackColor = groupBack;

            _listAnchors.BackColor = palette.LogBack;
            _listAnchors.ForeColor = palette.LogText;

            _numRadius.BackColor = inputBack;
            _numRadius.ForeColor = textColor;

            _numStrength.BackColor = inputBack;
            _numStrength.ForeColor = textColor;

            Button[] primaryButtons =
            {
                _btnSave
            };

            foreach (var btn in primaryButtons)
            {
                if (btn == null) continue;
                btn.BackColor = accentColor;
                btn.ForeColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = borderColor;
            }

            Button[] secondaryButtons =
            {
                _btnClose,
                _btnDelete,
                _btnClear
            };

            foreach (var btn in secondaryButtons)
            {
                if (btn == null) continue;
                btn.BackColor = secondaryButton;
                btn.ForeColor = textColor;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = borderColor;
            }
        }
    }
}