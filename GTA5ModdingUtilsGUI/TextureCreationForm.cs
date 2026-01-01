
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    /// <summary>
    /// Helper window that focuses solely on generating seasonal texture variants
    /// (spring / fall / winter) for vegetation textures.
    /// </summary>
    public partial class TextureCreationForm : Form
    {
        private readonly string _toolRoot;

        private int _textureProgressPercent;

        // If the Python helper does not emit incremental progress (common when stdout is buffered),
        // we still want the UI to feel responsive. We run a lightweight pseudo-progress ramp that
        // advances toward 95% while the process is running, and stops as soon as real progress is observed.
        // Explicitly qualify Timer to avoid ambiguity with System.Threading.Timer when ImplicitUsings is enabled.
        private readonly System.Windows.Forms.Timer _pseudoProgressTimer = new System.Windows.Forms.Timer();
        private DateTime _pseudoProgressStartUtc;
        private bool _sawRealProgress;

        private static readonly Regex RxProgressTagged = new Regex(@"(?:^|\b)progress\s*[:=]\s*(\d{1,3})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RxPercent = new Regex(@"(?<!\d)(\d{1,3})\s*%", RegexOptions.Compiled);
        private static readonly Regex RxFraction = new Regex(@"(?<!\d)(\d+)\s*/\s*(\d+)(?!\d)", RegexOptions.Compiled);

        // WinForms designer requires a parameterless constructor.
        public TextureCreationForm() : this(string.Empty)
        {
        }

        public TextureCreationForm(string toolRoot)
        {
            _toolRoot = toolRoot;

            InitializeComponent();

            // Initialize default output folder for texture variants, if possible.
            if (!string.IsNullOrWhiteSpace(_toolRoot))
            {
                try
                {
                    string defaultTextureOut = Path.Combine(_toolRoot, "texture_variants");
                    if (txtTextureOutputDir != null)
                        txtTextureOutputDir.Text = defaultTextureOut;
                }
                catch
                {
                    // ignore path errors
                }
            }

            if (lblTextureStatus != null)
                lblTextureStatus.Text = string.Empty;

            ResetTextureProgress();

            // Pseudo-progress timer (UI thread timer).
            _pseudoProgressTimer.Interval = 150;
            _pseudoProgressTimer.Tick += (_, __) => TickPseudoProgress();

            // Ensure the timer is cleaned up without adding a second Dispose override (Designer already generates one).
            this.Disposed += (_, __) =>
            {
                try { _pseudoProgressTimer.Stop(); } catch { }
                try { _pseudoProgressTimer.Dispose(); } catch { }
            };

            if (!IsInDesignMode())
            {
                ApplyTheme(SettingsManager.Current.Theme);
            }
        }

        private void StartPseudoProgress()
        {
            _sawRealProgress = false;
            _pseudoProgressStartUtc = DateTime.UtcNow;
            _pseudoProgressTimer.Start();
        }

        private void StopPseudoProgress()
        {
            _pseudoProgressTimer.Stop();
        }

        private void TickPseudoProgress()
        {
            if (_sawRealProgress) return;

            // Simple ramp toward 95%. This is intentionally conservative and non-linear.
            double elapsed = (DateTime.UtcNow - _pseudoProgressStartUtc).TotalSeconds;
            int target = (int)Math.Min(95, Math.Round(100.0 * (1.0 - Math.Exp(-elapsed / 8.0))));

            // Never regress and never hit 100% from pseudo-progress.
            if (target > _textureProgressPercent)
                SetTextureProgress(target);
        }

        private static bool IsInDesignMode()
            => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private void grpTextureVariants_Resize(object? sender, EventArgs e)
        {
            // Keep right-aligned buttons and bottom-aligned status in a consistent position.
            btnBrowseTextureSource.Left = grpTextureVariants.ClientSize.Width - btnBrowseTextureSource.Width - 10;
            btnBrowseTextureOutputDir.Left = grpTextureVariants.ClientSize.Width - btnBrowseTextureOutputDir.Width - 10;

            // Output "Open" button sits just left of the Browse button.
            if (btnOpenTextureOutputDir != null && txtTextureOutputDir != null)
            {
                btnOpenTextureOutputDir.Left = btnBrowseTextureOutputDir.Left - btnOpenTextureOutputDir.Width - 6;
                txtTextureOutputDir.Width = Math.Max(50, btnOpenTextureOutputDir.Left - txtTextureOutputDir.Left - 6);
            }

            btnEditAnchors.Left = grpTextureVariants.ClientSize.Width - btnEditAnchors.Width - 10;

            btnGenerateTextures.Top = grpTextureVariants.ClientSize.Height - btnGenerateTextures.Height - 10;
            lblTextureStatus.Left = btnGenerateTextures.Right + 10;
            lblTextureStatus.Top = btnGenerateTextures.Top + 2;
            lblTextureStatus.Width = grpTextureVariants.ClientSize.Width - btnGenerateTextures.Right - 20;

            // Progress bar (above the Generate button).
            if (pnlTextureProgressBar != null && lblTextureProgressPercent != null && txtTextureSource != null)
            {
                pnlTextureProgressBar.Left = txtTextureSource.Left;
                pnlTextureProgressBar.Width = grpTextureVariants.ClientSize.Width - pnlTextureProgressBar.Left - lblTextureProgressPercent.Width - 20;
                pnlTextureProgressBar.Top = btnGenerateTextures.Top - pnlTextureProgressBar.Height - 6;

                lblTextureProgressPercent.Left = pnlTextureProgressBar.Right + 10;
                lblTextureProgressPercent.Top = pnlTextureProgressBar.Top - 2;
            }

            UpdateTextureProgressVisual();

        }

        private void btnBrowseTextureSource_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.tga;*.bmp|All files|*.*",
                Title = "Select vegetation texture"
            };

            if (!string.IsNullOrWhiteSpace(txtTextureSource?.Text))
            {
                try
                {
                    string? dir = Path.GetDirectoryName(txtTextureSource.Text);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                        dlg.InitialDirectory = dir;
                }
                catch
                {
                    // ignore invalid path
                }
            }

            if (dlg.ShowDialog(this) == DialogResult.OK && txtTextureSource != null)
            {
                txtTextureSource.Text = dlg.FileName;
                if (lblTextureStatus != null)
                    lblTextureStatus.Text = string.Empty;
            }
        }

        private void btnBrowseTextureOutputDir_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Select output folder for seasonal texture variants"
            };

            if (!string.IsNullOrWhiteSpace(txtTextureOutputDir?.Text) && Directory.Exists(txtTextureOutputDir.Text))
            {
                dlg.SelectedPath = txtTextureOutputDir.Text;
            }

            if (dlg.ShowDialog(this) == DialogResult.OK && txtTextureOutputDir != null)
            {
                txtTextureOutputDir.Text = dlg.SelectedPath;
                if (lblTextureStatus != null)
                    lblTextureStatus.Text = string.Empty;
            }
        }


        private void btnOpenTextureOutputDir_Click(object? sender, EventArgs e)
        {
            string dir = txtTextureOutputDir?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(dir))
                return;

            if (!Directory.Exists(dir))
            {
                MessageBox.Show(this,
                    $"The output folder does not exist yet.\r\n\r\n{dir}",
                    "Open Output Folder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Failed to open output folder:\r\n{ex.Message}",
                    "Open Output Folder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void btnGenerateTextures_Click(object? sender, EventArgs e)
        {
            if (lblTextureStatus != null)
                lblTextureStatus.Text = string.Empty;

            ResetTextureProgress();

            string source = txtTextureSource?.Text.Trim() ?? string.Empty;
            string outputDir = txtTextureOutputDir?.Text.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(source) || !File.Exists(source))
            {
                MessageBox.Show(this,
                    "Please select a valid source texture file.",
                    "Texture Variants",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(outputDir))
            {
                MessageBox.Show(this,
                    "Please specify an output folder.",
                    "Texture Variants",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to create output folder:\n" + ex.Message,
                    "Texture Variants",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var seasons = new List<string>();
            if (chkSeasonSpring != null && chkSeasonSpring.Checked) seasons.Add("spring");
            if (chkSeasonFall != null && chkSeasonFall.Checked) seasons.Add("fall");
            if (chkSeasonWinter != null && chkSeasonWinter.Checked) seasons.Add("winter");

            if (seasons.Count == 0)
            {
                MessageBox.Show(this,
                    "Please select at least one season preset.",
                    "Texture Variants",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string toolRoot = _toolRoot;
            if (string.IsNullOrWhiteSpace(toolRoot) || !Directory.Exists(toolRoot))
            {
                MessageBox.Show(this,
                    "The gta5-modding-utils-main folder could not be resolved from the current tool root\n\n" +
                    "Please ensure the tool root is configured correctly in Settings.",
                    "Texture Variants",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string scriptPath = Path.Combine(toolRoot, "texture_variants.py");
            if (!File.Exists(scriptPath))
            {
                MessageBox.Show(this,
                    $"Could not find texture_variants.py in the GTA5 modding tools folder:\n{scriptPath}",
                    "Texture Variants",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string pythonExe = "python";

            try
            {
                if (lblTextureStatus != null)
                    lblTextureStatus.Text = "Generating seasonal variants...";

                SetTextureProgress(0);

                // Kick off pseudo-progress immediately so the user sees movement even if the helper
                // does not emit progress lines (or stdout is buffered).
                StartPseudoProgress();

                if (btnGenerateTextures != null)
                    btnGenerateTextures.Enabled = false;

                var psi = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = false,
                    CreateNoWindow = true,
                    WorkingDirectory = toolRoot
                };

                // Unbuffered output improves real-time progress updates.
                psi.Environment["PYTHONUNBUFFERED"] = "1";

                psi.ArgumentList.Add("-u");
                psi.ArgumentList.Add(scriptPath);
                psi.ArgumentList.Add("--input");
                psi.ArgumentList.Add(source);
                psi.ArgumentList.Add("--outputDir");
                psi.ArgumentList.Add(outputDir);
                psi.ArgumentList.Add("--seasons");
                psi.ArgumentList.Add(string.Join(",", seasons));

                var stderr = new StringBuilder();

                using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

                proc.OutputDataReceived += (_, args) =>
                {
                    if (args.Data == null) return;
                    string line = args.Data.Trim();
                    if (string.IsNullOrWhiteSpace(line)) return;

                    if (TryParseProgressPercent(line, out int percent))
                    {
                        if (percent > 100) percent = 100;
                        if (percent < 0) percent = 0;

                        // Once we see a real percent, stop pseudo-progress so we don't fight the updates.
                        _sawRealProgress = true;
                        StopPseudoProgress();

                        SetTextureProgressSafe(percent);
                    }
                    else
                    {
                        // Fallback: if the script logs season names (spring/fall/winter),
                        // show coarse stage progress even if no explicit percent is printed.
                        for (int i = 0; i < seasons.Count; i++)
                        {
                            if (line.IndexOf(seasons[i], StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                int stage = (int)Math.Round((i / (double)seasons.Count) * 100.0);
                                if (stage < 0) stage = 0;
                                if (stage > 99) stage = 99;
                                SetTextureProgressSafe(stage);
                                break;
                            }
                        }
                    }

                    UpdateTextureStatusSafe(line);
                };

                proc.ErrorDataReceived += (_, args) =>
                {
                    if (args.Data == null) return;
                    string line = args.Data.Trim();

                    // Some Python libs (e.g., tqdm) emit progress to stderr.
                    if (!string.IsNullOrWhiteSpace(line) && TryParseProgressPercent(line, out int percent))
                    {
                        _sawRealProgress = true;
                        StopPseudoProgress();
                        if (percent > 100) percent = 100;
                        if (percent < 0) percent = 0;
                        SetTextureProgressSafe(percent);
                    }

                    stderr.AppendLine(args.Data);
                };

                if (!proc.Start())
                {
                    if (lblTextureStatus != null)
                        lblTextureStatus.Text = "Failed to start Python process.";
                    return;
                }

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                await proc.WaitForExitAsync();

                if (proc.ExitCode == 0)
                {
                    StopPseudoProgress();
                    SetTextureProgress(100);
                    if (lblTextureStatus != null)
                        lblTextureStatus.Text = "Done. Seasonal variants generated.";
                }
                else
                {
                    StopPseudoProgress();
                    if (lblTextureStatus != null)
                        lblTextureStatus.Text = "Texture generation failed.";

                    string err = stderr.ToString();
                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        MessageBox.Show(this,
                            err,
                            "Python error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                StopPseudoProgress();
                if (lblTextureStatus != null)
                    lblTextureStatus.Text = "Error during texture generation.";
                MessageBox.Show(this,
                    ex.Message,
                    "Texture Variants",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                StopPseudoProgress();
                if (btnGenerateTextures != null)
                    btnGenerateTextures.Enabled = true;
            }
        }

        private void ResetTextureProgress()
        {
            _textureProgressPercent = 0;
            UpdateTextureProgressVisual();
        }

        private void SetTextureProgress(int percent)
        {
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;
            _textureProgressPercent = percent;
            UpdateTextureProgressVisual();
        }

        private void SetTextureProgressSafe(int percent)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetTextureProgress(percent)));
                return;
            }

            SetTextureProgress(percent);
        }

        private void UpdateTextureStatusSafe(string statusLine)
        {
            if (lblTextureStatus == null) return;
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => lblTextureStatus.Text = statusLine));
                return;
            }

            lblTextureStatus.Text = statusLine;
        }

        private void UpdateTextureProgressVisual()
        {
            if (pnlTextureProgressBar == null || pnlTextureProgressFill == null || lblTextureProgressPercent == null)
                return;

            int trackWidth = pnlTextureProgressBar.ClientSize.Width;
            if (trackWidth < 0) trackWidth = 0;

            int fillWidth = (int)Math.Round(trackWidth * (_textureProgressPercent / 100.0));
            if (fillWidth < 0) fillWidth = 0;
            if (fillWidth > trackWidth) fillWidth = trackWidth;

            pnlTextureProgressFill.Width = fillWidth;
            pnlTextureProgressFill.Height = pnlTextureProgressBar.ClientSize.Height;
            pnlTextureProgressFill.Top = 0;
            pnlTextureProgressFill.Left = 0;

            lblTextureProgressPercent.Text = _textureProgressPercent.ToString() + "%";
        }

        private static bool TryParseProgressPercent(string line, out int percent)
        {
            percent = 0;
            if (string.IsNullOrWhiteSpace(line)) return false;

            Match m = RxProgressTagged.Match(line);
            if (m.Success && int.TryParse(m.Groups[1].Value, out percent))
            {
                return true;
            }

            m = RxPercent.Match(line);
            if (m.Success && int.TryParse(m.Groups[1].Value, out percent))
            {
                return true;
            }

            m = RxFraction.Match(line);
            if (m.Success)
            {
                if (int.TryParse(m.Groups[1].Value, out int num) && int.TryParse(m.Groups[2].Value, out int den) && den > 0)
                {
                    percent = (int)Math.Round((num / (double)den) * 100.0);
                    return true;
                }
            }

            return false;
        }

        private void btnEditAnchors_Click(object? sender, EventArgs e)
        {
            string source = txtTextureSource?.Text.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(source) || !File.Exists(source))
            {
                MessageBox.Show(this,
                    "Please select a valid source texture file before editing anchors.",
                    "Texture Anchors",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using var editor = new TextureAnchorEditorForm(source);
            editor.StartPosition = FormStartPosition.CenterParent;
            editor.ShowDialog(this);
        }

        private void ApplyTheme(AppTheme theme)
        {
            ThemePalette palette = ThemeHelper.GetPalette(theme);

            Color windowBack = palette.WindowBack;
            Color inputBack = palette.InputBack;
            Color textColor = palette.TextColor;
            Color accentColor = palette.AccentColor;
            Color secondaryButton = palette.SecondaryButton;
            Color borderColor = palette.BorderColor;

            this.BackColor = windowBack;
            this.ForeColor = textColor;

            lblInfo.ForeColor = textColor;

            grpTextureVariants.ForeColor = textColor;
            grpTextureVariants.BackColor = windowBack;

            if (txtTextureSource != null)
            {
                txtTextureSource.BackColor = inputBack;
                txtTextureSource.ForeColor = textColor;
                txtTextureSource.BorderStyle = BorderStyle.FixedSingle;
            }

            if (txtTextureOutputDir != null)
            {
                txtTextureOutputDir.BackColor = inputBack;
                txtTextureOutputDir.ForeColor = textColor;
                txtTextureOutputDir.BorderStyle = BorderStyle.FixedSingle;
            }

            if (lblTextureStatus != null)
            {
                lblTextureStatus.ForeColor = accentColor;
            }

            if (pnlTextureProgressBar != null)
            {
                pnlTextureProgressBar.BackColor = inputBack;
                pnlTextureProgressBar.BorderStyle = BorderStyle.FixedSingle;
            }

            if (pnlTextureProgressFill != null)
            {
                // Bright green progress fill as requested.
                pnlTextureProgressFill.BackColor = Color.LimeGreen;
            }

            if (lblTextureProgressPercent != null)
            {
                lblTextureProgressPercent.ForeColor = textColor;
            }

            // Accent buttons (primary action)
            if (btnGenerateTextures != null)
            {
                btnGenerateTextures.BackColor = accentColor;
                btnGenerateTextures.ForeColor = Color.White;
                btnGenerateTextures.FlatStyle = FlatStyle.Flat;
                btnGenerateTextures.FlatAppearance.BorderColor = borderColor;
            }

            // Secondary buttons
            Button?[] secondaryButtons =
            {
                btnBrowseTextureSource,
                btnBrowseTextureOutputDir,
                btnOpenTextureOutputDir,
                btnEditAnchors
            };

            foreach (var btn in secondaryButtons)
            {
                if (btn == null) continue;
                btn.BackColor = secondaryButton;
                btn.ForeColor = textColor;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = borderColor;
                // Ensure the theme colors actually apply.
                btn.UseVisualStyleBackColor = false;
            }
        }
    }
}
