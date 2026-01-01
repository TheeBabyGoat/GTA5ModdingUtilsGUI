using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    public partial class MainForm : Form
    {
        private Process? _currentProcess;
        private Panel? _titleBarPanel;
        private MenuStrip? _titleMenuStrip;
        private Button? _btnClose;
        private Button? _btnMaximize;
        private Button? _btnMinimize;

        private const int TitleBarHeight = 30;
        // Win32 ReleaseCapture removed; stub to maintain compatibility.
        private static void ReleaseCapture()
        {
        }
        // Win32 SendMessage removed; stub to maintain compatibility.
        private static IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam)
        {
            return IntPtr.Zero;
        }

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;
        private const int HTBOTTOMRIGHT = 17;

        // Added fields for custom LOD distance overrides per vegetation category.
        // These controls will be created in the constructor and allow the user
        // to specify scaling factors for cacti, trees, bushes and palms. When the
        // Python script is invoked these values are passed as command line arguments.
        public MainForm()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            InitializeLodMultiplierControls();


            // Shift all controls down by the menu strip height so nothing overlaps.
            if (menuStrip1 != null)
            {
                int offset = menuStrip1.Height;
                foreach (Control c in this.Controls)
                {
                    if (!object.ReferenceEquals(c, menuStrip1))
                    {
                        c.Top += offset;
                    }
                }
            }
            ApplyTheme(SettingsManager.Current.Theme);
            ApplySavedToolRoot();
            AddLogoToUi();

            // When the "Use original map names" checkbox is toggled, update the
            // prefix textbox enabled state accordingly.  If no prefix is
            // required, disable the prefix textbox so the user knows it is
            // ignored.  Otherwise, re-enable it.
            // Initialize the enabled state of the prefix textbox based on
            // whether the "Use original map names" option is checked. The
            // designer already wires up the CheckedChanged event so we
            // simply invoke the handler once here to set the initial state.
            if (chkUseOriginalNames != null)
            {
                chkUseOriginalNames_CheckedChanged(chkUseOriginalNames, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Create and add controls that allow the user to specify custom LOD distance
        /// override distances for different vegetation categories. These controls are grouped
        /// together in a dedicated group box. Default values are 0 (no override).
        /// </summary>


        private void InitializeLodMultiplierControls()
        {
            // Wire up the enable checkbox to toggle the numeric controls.
            if (chkEnableLodMultipliers != null)
            {
                chkEnableLodMultipliers.CheckedChanged += (s, e) => UpdateLodMultiplierControlState();
            }

            // Apply the initial enabled/disabled state.
            UpdateLodMultiplierControlState();
        }

        private void UpdateLodMultiplierControlState()
        {
            bool enabled = chkEnableLodMultipliers != null && chkEnableLodMultipliers.Checked;

            if (nudLodMultiplierCacti != null)
                nudLodMultiplierCacti.Enabled = enabled;

            if (nudLodMultiplierTrees != null)
                nudLodMultiplierTrees.Enabled = enabled;

            if (nudLodMultiplierBushes != null)
                nudLodMultiplierBushes.Enabled = enabled;

            if (nudLodMultiplierPalms != null)
                nudLodMultiplierPalms.Enabled = enabled;
        }




        private void BuildCustomTitleBar()
        {
            // Custom title bar disabled; using standard system title bar.
            return;

            // Use a custom title bar so we can place Help/Credits next to the app title.
            this.FormBorderStyle = FormBorderStyle.None;

            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = TitleBarHeight,
                BackColor = SystemColors.Control
            };
            _titleBarPanel = panel;

            // Icon
            var iconBox = new PictureBox
            {
                Size = new Size(16, 16),
                Location = new Point(8, 7),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            if (this.Icon != null)
            {
                iconBox.Image = this.Icon.ToBitmap();
            }
            panel.Controls.Add(iconBox);

            // Title text
            var lblTitle = new Label
            {
                AutoSize = true,
                Text = this.Text,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(28, 7)
            };
            panel.Controls.Add(lblTitle);

            // Menu strip for Settings / Help / Credits
            var menu = new MenuStrip
            {
                AutoSize = true
            };

            // Settings opens the configuration dialog (theme, default paths, etc.)
            var settingsItem = new ToolStripMenuItem("Settings");
            settingsItem.Click += settingsToolStripMenuItem_Click;

            var helpItem = new ToolStripMenuItem("Help");
            var tutorialsItem = new ToolStripMenuItem("Tutorials...");
            tutorialsItem.Click += tutorialsToolStripMenuItem_Click;
            var sep = new ToolStripSeparator();
            var readmeItem = new ToolStripMenuItem("View Readme");
            readmeItem.Click += viewReadmeToolStripMenuItem_Click;
            helpItem.DropDownItems.Add(tutorialsItem);
            helpItem.DropDownItems.Add(sep);
            helpItem.DropDownItems.Add(readmeItem);

            var creditsItem = new ToolStripMenuItem("Credits");
            creditsItem.Click += creditsToolStripMenuItem_Click;

            // Order: Settings | Help | Credits  (closest to standard caption buttons)
            menu.Items.Add(settingsItem);
            menu.Items.Add(helpItem);
            menu.Items.Add(creditsItem);

            _titleMenuStrip = menu;
            panel.Controls.Add(menu);

            // Window control buttons
            _btnClose = new Button
            {
                Text = "X",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 22)
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.Click += (s, e) => this.Close();
            panel.Controls.Add(_btnClose);

            _btnMaximize = new Button
            {
                Text = "▢",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 22)
            };
            _btnMaximize.FlatAppearance.BorderSize = 0;
            _btnMaximize.Click += (s, e) =>
            {
                this.WindowState = this.WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
            };
            panel.Controls.Add(_btnMaximize);

            _btnMinimize = new Button
            {
                Text = "_",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 22)
            };
            _btnMinimize.FlatAppearance.BorderSize = 0;
            _btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            panel.Controls.Add(_btnMinimize);

            // Layout positions when the panel is resized
            panel.Resize += (s, e) =>
            {
                int right = panel.Width - 4;

                if (_btnClose != null)
                {
                    _btnClose.Location = new Point(right - _btnClose.Width, 4);
                    right = _btnClose.Left - 2;
                }

                if (_btnMaximize != null)
                {
                    _btnMaximize.Location = new Point(right - _btnMaximize.Width, 4);
                    right = _btnMaximize.Left - 2;
                }

                if (_btnMinimize != null)
                {
                    _btnMinimize.Location = new Point(right - _btnMinimize.Width, 4);
                    right = _btnMinimize.Left - 8;
                }

                if (_titleMenuStrip != null)
                {
                    _titleMenuStrip.Location = new Point(
                        right - _titleMenuStrip.PreferredSize.Width,
                        (panel.Height - _titleMenuStrip.Height) / 2);
                }
            };

            // Allow dragging the window by the title area
            panel.MouseDown += TitleBar_MouseDown;
            lblTitle.MouseDown += TitleBar_MouseDown;
            iconBox.MouseDown += TitleBar_MouseDown;

            // Insert the panel and push existing controls down
            this.Controls.Add(panel);
            panel.BringToFront();

            foreach (Control c in this.Controls)
            {
                if (c == panel) continue;
                c.Top += TitleBarHeight;
            }

            if (this.MainMenuStrip != null)
            {
                this.MainMenuStrip.Visible = false;
            }

            // Trigger initial layout
            panel.PerformLayout();
        }

        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                MessageBox.Show("The tool is already running.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string toolRoot = txtPythonPath.Text.Trim();
            string pythonExe = "python"; // rely on active environment / PATH

            string inputDir = txtInputDir.Text.Trim();
            string outputDir = txtOutputDir.Text.Trim();
            string prefix = txtPrefix.Text.Trim();
            bool useOriginalNames = chkUseOriginalNames != null && chkUseOriginalNames.Checked;

            if (string.IsNullOrEmpty(inputDir) || !Directory.Exists(inputDir))
            {
                MessageBox.Show("Please select a valid input directory.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Require a project prefix only when not using original names. If the
            // user opts to preserve existing ymap names, an empty prefix is
            // acceptable and will be ignored by the underlying script. Otherwise
            // enforce a non‑empty prefix.
            if (!useOriginalNames && string.IsNullOrEmpty(prefix))
            {
                MessageBox.Show("Please provide a prefix for this project.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = Path.Combine(inputDir, "generated");
                txtOutputDir.Text = outputDir;
            }

            if (chkReflection.Checked && !chkLodMap.Checked)
            {
                MessageBox.Show("Reflection requires LOD Map to be enabled.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (Directory.Exists(outputDir))
                {
                    var result = MessageBox.Show(
                        $"The output directory:\n{outputDir}\n\nalready exists.\n\n" +
                        "Do you want to clear it before running? This will delete all files inside.",
                        "Clear output directory?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        Directory.Delete(outputDir, true);
                    }
                    else
                    {
                        AppendLog("Operation cancelled: output directory was not cleared.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to prepare output directory: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnRun.Enabled = false;
            btnCancel.Enabled = true;
            txtLog.Clear();
            AppendLog("Starting gta5-modding-utils...");
            AppendLog("");

            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string scriptPath;

            if (!string.IsNullOrEmpty(toolRoot))
            {
                // Allow the user to point at their own gta5-modding-utils checkout.
                // We accept either the folder that directly contains main.py or a parent
                // folder that contains a gta5-modding-utils-main subfolder.
                string candidate1 = Path.Combine(toolRoot, "main.py");
                string candidate2 = Path.Combine(toolRoot, "gta5-modding-utils-main", "main.py");

                if (File.Exists(candidate1))
                {
                    scriptPath = candidate1;
                }
                else if (File.Exists(candidate2))
                {
                    scriptPath = candidate2;
                }
                else
                {
                    MessageBox.Show(
                        "Could not find main.py in the selected gta5-modding-utils folder.\n\n" +
                        "Please select the folder that contains main.py, or leave this field empty " +
                        "to use the gta5-modding-utils-main copy next to this tool.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnRun.Enabled = true;
                    btnCancel.Enabled = false;
                    return;
                }
            }
            else
            {
                // Default: use the bundled gta5-modding-utils-main folder next to the executable.
                scriptPath = Path.Combine(appDir, "gta5-modding-utils-main", "main.py");

                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show("Could not find main.py in the gta5-modding-utils-main folder next to the executable.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnRun.Enabled = true;
                    btnCancel.Enabled = false;
                    return;
                }
            }

            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? appDir
            };

            // Arguments
            psi.ArgumentList.Add(scriptPath);
            psi.ArgumentList.Add("--inputDir");
            psi.ArgumentList.Add(inputDir);
            psi.ArgumentList.Add("--outputDir");
            psi.ArgumentList.Add(outputDir);
            // Always supply the prefix argument, even if empty. The script
            // determines whether to use it based on the useOriginalNames flag.
            psi.ArgumentList.Add("--prefix");
            psi.ArgumentList.Add(prefix);

            if (chkVegetation.Checked)
            {
                psi.ArgumentList.Add("--vegetationCreator");
                psi.ArgumentList.Add("on");
            }

            if (chkEntropy.Checked)
            {
                psi.ArgumentList.Add("--entropy");
                psi.ArgumentList.Add("on");
            }

            if (chkReducer.Checked)
            {
                psi.ArgumentList.Add("--reducer");
                psi.ArgumentList.Add("on");

                if (nudReducerResolution.Value > 0)
                {
                    psi.ArgumentList.Add("--reducerResolution");
                    psi.ArgumentList.Add(nudReducerResolution.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }

                if (chkReducerAdaptScaling.Checked)
                {
                    psi.ArgumentList.Add("--reducerAdaptScaling");
                    psi.ArgumentList.Add("on");
                }
            }

            if (chkClustering.Checked)
            {
                psi.ArgumentList.Add("--clustering");
                psi.ArgumentList.Add("on");

                if (nudNumClusters.Value > 0)
                {
                    psi.ArgumentList.Add("--numClusters");
                    psi.ArgumentList.Add(((int)nudNumClusters.Value).ToString());
                }

                string polygonText = txtPolygon.Text.Trim();
                if (!string.IsNullOrEmpty(polygonText))
                {
                    polygonText = polygonText.Replace("\r", " ").Replace("\n", " ");
                    psi.ArgumentList.Add("--polygon");
                    psi.ArgumentList.Add(polygonText);
                }

                string clusteringPrefix = txtClusteringPrefix.Text.Trim();
                if (!string.IsNullOrEmpty(clusteringPrefix))
                {
                    psi.ArgumentList.Add("--clusteringPrefix");
                    psi.ArgumentList.Add(clusteringPrefix);
                }

                string clusteringExcluded = txtClusteringExcluded.Text.Trim();
                if (!string.IsNullOrEmpty(clusteringExcluded))
                {
                    psi.ArgumentList.Add("--clusteringExcluded");
                    psi.ArgumentList.Add(clusteringExcluded);
                }
            }

            if (chkStaticCol.Checked)
            {
                psi.ArgumentList.Add("--staticCol");
                psi.ArgumentList.Add("on");
            }

            if (chkLodMap.Checked)
            {
                psi.ArgumentList.Add("--lodMap");
                psi.ArgumentList.Add("on");
            }

            if (chkCustomMeshes.Checked)
            {
                psi.ArgumentList.Add("--customMeshesOnly");
                psi.ArgumentList.Add("on");
            }

            if (chkClearLod.Checked)
            {
                psi.ArgumentList.Add("--clearLod");
                psi.ArgumentList.Add("on");
            }

            if (chkReflection.Checked)
            {
                psi.ArgumentList.Add("--reflection");
                psi.ArgumentList.Add("on");
            }

            if (chkSanitizer.Checked)
            {
                psi.ArgumentList.Add("--sanitizer");
                psi.ArgumentList.Add("on");
            }

            if (chkStatistics.Checked)
            {
                psi.ArgumentList.Add("--statistics");
                psi.ArgumentList.Add("on");
            }

            // Indicate whether the tool should operate directly on existing map names.
            // Pass "on" when the checkbox is checked and "off" otherwise. The
            // underlying Python script defaults to false when the option is omitted,
            // but explicitly setting it clarifies intent and makes behaviour
            // deterministic.
            psi.ArgumentList.Add("--useOriginalNames");
            psi.ArgumentList.Add(useOriginalNames ? "on" : "off");

            // Pass the custom LOD distance overrides to the Python script only when the
            // feature is enabled in the UI.
            if (chkEnableLodMultipliers != null && chkEnableLodMultipliers.Checked)
            {
                // Always pass the custom LOD distance overrides to the Python script. The values
                // are taken from the numeric controls in the LOD Distance Overrides group.
                if (nudLodMultiplierCacti != null)
                {
                    psi.ArgumentList.Add("--lodDistanceCacti");
                    psi.ArgumentList.Add(nudLodMultiplierCacti.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if (nudLodMultiplierTrees != null)
                {
                    psi.ArgumentList.Add("--lodDistanceTrees");
                    psi.ArgumentList.Add(nudLodMultiplierTrees.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if (nudLodMultiplierBushes != null)
                {
                    psi.ArgumentList.Add("--lodDistanceBushes");
                    psi.ArgumentList.Add(nudLodMultiplierBushes.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if (nudLodMultiplierPalms != null)
                {
                    psi.ArgumentList.Add("--lodDistancePalms");
                    psi.ArgumentList.Add(nudLodMultiplierPalms.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }

            }

            try
            {
                var process = new Process();
                process.StartInfo = psi;
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;

                _currentProcess = process;

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                AppendLog($"Started python process (PID {process.Id}).");
                await process.WaitForExitAsync();

                AppendLog("");
                AppendLog($"Process finished with exit code {process.ExitCode}.");

                if (process.ExitCode == 0)
                {
                    AppendLog("Done. Check the output directory for generated files.");
                }
                else
                {
                    AppendLog("The script exited with a non-zero code. Please review the log above.");
                }
            }
            catch (Exception ex)
            {
                AppendLog("Failed to start or run python process: " + ex.Message);
                MessageBox.Show("Failed to run python: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _currentProcess = null;
                btnRun.Enabled = true;
                btnCancel.Enabled = false;
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                AppendLog(e.Data);
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                AppendLog("[ERR] " + e.Data);
            }
        }

        private void AppendLog(string text)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action<string>(AppendLog), text);
            }
            else
            {
                if (txtLog.TextLength == 0)
                {
                    txtLog.AppendText(text);
                }
                else
                {
                    txtLog.AppendText(Environment.NewLine + text);
                }
            }
        }

        private void btnBrowsePython_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select the folder that contains gta5-modding-utils (main.py)";
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    txtPythonPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnBrowseInputDir_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select the input directory containing .ymap.xml files";
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    txtInputDir.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnBrowseOutputDir_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select or create an output directory";
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    txtOutputDir.Text = fbd.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Event handler for toggling the "Use original map names" option.
        /// When enabled, the prefix text box is disabled and cleared so that
        /// the tool operates directly on existing map names without a project
        /// prefix. When disabled, the prefix text box is re‑enabled to
        /// accept a new project prefix.
        /// </summary>
        private void chkUseOriginalNames_CheckedChanged(object? sender, EventArgs e)
        {
            // Guard against null references since the designer can reinstantiate
            // controls before this handler is attached.
            if (this.chkUseOriginalNames == null || this.txtPrefix == null)
            {
                return;
            }
            bool useOriginal = this.chkUseOriginalNames.Checked;
            // Disable or enable the prefix textbox based on the toggle.
            this.txtPrefix.Enabled = !useOriginal;
            if (useOriginal)
            {
                // Clear any existing prefix to avoid accidental reuse.
                this.txtPrefix.Text = string.Empty;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                try
                {
                    _currentProcess.Kill(true);
                    AppendLog("Process cancelled by user.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to cancel process: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void tutorialsToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var frm = new TutorialsForm())
                {
                    frm.StartPosition = FormStartPosition.CenterParent;
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open tutorials: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void viewReadmeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string readmePath = Path.Combine(baseDir, "README-GUI.txt");
                string content;
                if (File.Exists(readmePath))
                {
                    content = File.ReadAllText(readmePath);
                }
                else
                {
                    content = "README-GUI.txt was not found next to the executable.\r\n\r\n" +
                              "Make sure it is included in the release package.";
                }

                using (var frm = new ReadmeForm(content))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load readme: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void settingsToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using (var dlg = new SettingsForm())
            {
                dlg.StartPosition = FormStartPosition.CenterParent;
                var result = dlg.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    // Persist settings and apply them to the main window.
                    SettingsManager.Save();

                    // Apply theme immediately.
                    ApplyTheme(SettingsManager.Current.Theme);

                    // Update the default gta5-modding-utils path, if valid.
                    try
                    {
                        string? saved = SettingsManager.Current.Gta5ModdingUtilsPath;
                        if (!string.IsNullOrWhiteSpace(saved) && Directory.Exists(saved))
                        {
                            txtPythonPath.Text = saved;
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new CreditsForm())
            {
                frm.ShowDialog(this);
            }
        }


        protected override void WndProc(ref Message m)
        {
            // Enable resizing from the bottom-right corner, since we are using a borderless window
            // to host a custom title bar.
            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                if ((int)m.Result == HTCLIENT)
                {
                    const int grip = 10;
                    var clientPoint = this.PointToClient(new Point(m.LParam.ToInt32()));
                    if (clientPoint.X >= this.ClientSize.Width - grip &&
                        clientPoint.Y >= this.ClientSize.Height - grip)
                    {
                        m.Result = (IntPtr)HTBOTTOMRIGHT;
                        return;
                    }
                }
                return;
            }

            base.WndProc(ref m);
        }

        private void btnOpenOutputDir_Click(object sender, EventArgs e)
        {
            try
            {
                var path = txtOutputDir.Text.Trim();
                if (string.IsNullOrEmpty(path))
                {
                    MessageBox.Show("Output folder is empty. Please select or enter a folder first.",
                        "Open output folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!System.IO.Directory.Exists(path))
                {
                    MessageBox.Show("The specified output folder does not exist yet:\n\n" + path,
                        "Open output folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open output folder: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private AppTheme _currentTheme = AppTheme.DarkTeal;

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
            var logBack = palette.LogBack;
            var logText = palette.LogText;

            this.BackColor = windowBack;
            this.ForeColor = textColor;

            if (menuStrip1 != null)
            {
                menuStrip1.BackColor = groupBack;
                menuStrip1.ForeColor = textColor;
            }

            if (_titleBarPanel != null)
            {
                _titleBarPanel.BackColor = groupBack;
            }

            if (_titleMenuStrip != null)
            {
                _titleMenuStrip.BackColor = groupBack;
                _titleMenuStrip.ForeColor = textColor;
            }

            GroupBox[] groups =
            {
                grpFeatures,
                grpAdvanced,
                grpLodMultipliers
            };

            foreach (var g in groups)
            {
                if (g == null) continue;
                g.BackColor = groupBack;
                g.ForeColor = textColor;
            }

            TextBox[] textBoxes =
            {
                txtPythonPath,
                txtInputDir,
                txtOutputDir,
                txtPrefix,
                txtClusteringPrefix,
                txtClusteringExcluded,
                txtPolygon
            };

            foreach (var tb in textBoxes)
            {
                if (tb == null) continue;
                tb.BackColor = inputBack;
                tb.ForeColor = textColor;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }

            NumericUpDown[] numerics = { nudReducerResolution, nudNumClusters, nudLodMultiplierCacti, nudLodMultiplierTrees, nudLodMultiplierBushes, nudLodMultiplierPalms };
            foreach (var nud in numerics)
            {
                if (nud == null) continue;
                nud.BackColor = inputBack;
                nud.ForeColor = textColor;
            }

            CheckBox[] steps =
            {
                chkVegetation,
                chkEntropy,
                chkReducer,
                chkClustering,
                chkStaticCol,
                chkLodMap,
                chkClearLod,
                chkReflection,
                chkSanitizer,
                chkStatistics,
                chkEnableLodMultipliers
            };

            foreach (var chk in steps)
            {
                if (chk == null) continue;
                chk.ForeColor = textColor;
                chk.BackColor = groupBack;
            }

            if (txtLog != null)
            {
                txtLog.BackColor = logBack;
                txtLog.ForeColor = logText;
                txtLog.BorderStyle = BorderStyle.FixedSingle;
            }

            StylePrimaryButton(btnRun, accentColor, textColor, borderColor);

            Button[] secondaryButtons =
            {
                btnCancel,
                btnBrowsePython,
                btnBrowseInputDir,
                btnBrowseOutputDir,
                btnOpenOutputDir,
            };

            foreach (var btn in secondaryButtons)
            {
                StyleSecondaryButton(btn, secondaryButton, textColor, borderColor);
            }
        }


        /// <summary>
        /// If the user has configured a default gta5-modding-utils location,
        /// apply it to the main textbox on startup.
        /// </summary>
        private void ApplySavedToolRoot()
        {
            try
            {
                string? saved = SettingsManager.Current.Gta5ModdingUtilsPath;
                if (!string.IsNullOrWhiteSpace(saved) && Directory.Exists(saved))
                {
                    txtPythonPath.Text = saved;
                }
            }
            catch
            {
                // Ignore any issues; the user can still set the path manually.
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

        private void AddLogoToUi()
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
                    Name = "picLogo",
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Width = 80,
                    Height = 80,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                int topInset = (menuStrip1 != null ? menuStrip1.Height : SystemInformation.MenuHeight) + 8;
                logo.Location = new Point(this.ClientSize.Width - logo.Width - 16, topInset);

                logo.Image = Image.FromFile(logoPath);
                this.Controls.Add(logo);
                logo.BringToFront();

                this.Resize += (s, e) =>
                {
                    if (!logo.IsDisposed)
                    {
                        logo.Left = this.ClientSize.Width - logo.Width - 16;
                    }
                };
            }
            catch
            {
                // If anything goes wrong we just skip showing the logo;
                // the core functionality of the tool should not be affected.
            }
        }


        private void preview3DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Open the preview as a standalone window.
                string toolRoot = txtPythonPath.Text.Trim();
                if (string.IsNullOrEmpty(toolRoot))
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    string candidate = Path.Combine(appDir, "gta5-modding-utils-main");
                    toolRoot = candidate;
                }

                using var preview = new LodAtlasPreviewForm();

                // Best-effort: populate the mesh dropdown from the tool root folder.
                // Users can still browse manually if their meshes live elsewhere.
                if (!string.IsNullOrWhiteSpace(toolRoot) && Directory.Exists(toolRoot))
                {
                    preview.TryPopulateMeshListFromFolder(toolRoot);
                }

                preview.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open 3D preview: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void convertOdrToObjToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string toolRoot = txtPythonPath.Text.Trim();
                if (string.IsNullOrWhiteSpace(toolRoot))
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    toolRoot = Path.Combine(appDir, "gta5-modding-utils-main");
                }

                if (!Directory.Exists(toolRoot))
                {
                    MessageBox.Show(this,
                        "Could not locate the gta5-modding-utils-main folder.\n\nPlease set it in the main window first.",
                        "Convert ODR -> OBJ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string scriptPath = Path.Combine(toolRoot, "odr_to_obj.py");
                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show(this,
                        "Could not find odr_to_obj.py in the gta5-modding-utils-main folder.\n\nExpected:\n" + scriptPath,
                        "Convert ODR -> OBJ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                using var ofd = new OpenFileDialog
                {
                    Title = "Select OpenFormats Drawable (.odr)",
                    Filter = "OpenFormats Drawable (*.odr)|*.odr|All Files (*.*)|*.*",
                    InitialDirectory = toolRoot
                };

                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                string odrPath = ofd.FileName;
                string defaultObj = Path.Combine(Path.GetDirectoryName(odrPath) ?? toolRoot,
                    Path.GetFileNameWithoutExtension(odrPath) + ".obj");

                using var sfd = new SaveFileDialog
                {
                    Title = "Save OBJ As",
                    Filter = "Wavefront OBJ (*.obj)|*.obj|All Files (*.*)|*.*",
                    FileName = Path.GetFileName(defaultObj),
                    InitialDirectory = Path.GetDirectoryName(defaultObj) ?? toolRoot
                };

                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                string outObj = sfd.FileName;

                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = toolRoot
                };

                psi.ArgumentList.Add(scriptPath);
                psi.ArgumentList.Add("--odr");
                psi.ArgumentList.Add(odrPath);
                psi.ArgumentList.Add("--outObj");
                psi.ArgumentList.Add(outObj);

                using var proc = Process.Start(psi);
                if (proc == null)
                    throw new Exception("Failed to start python process.");

                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    MessageBox.Show(this,
                        "ODR -> OBJ conversion failed:\n\n" + stderr,
                        "Convert ODR -> OBJ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show(this,
                    "Exported OBJ:\n\n" + outObj,
                    "Convert ODR -> OBJ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to convert ODR to OBJ: " + ex.Message,
                    "Convert ODR -> OBJ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        private void btnCustomMeshes_Click(object? sender, EventArgs e)
        {
            try
            {
                string toolRoot = txtPythonPath.Text.Trim();
                if (string.IsNullOrEmpty(toolRoot))
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    string candidate = Path.Combine(appDir, "gta5-modding-utils-main");
                    toolRoot = candidate;
                }

                string defaultJsonPath = Path.Combine(toolRoot, "custom_meshes.json");

                using (var frm = new CustomMeshesForm(toolRoot, defaultJsonPath))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open custom meshes helper: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void textureCreationToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                string toolRoot = txtPythonPath.Text.Trim();
                if (string.IsNullOrEmpty(toolRoot))
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    string candidate = Path.Combine(appDir, "gta5-modding-utils-main");
                    toolRoot = candidate;
                }

                using (var frm = new TextureCreationForm(toolRoot))
                {
                    frm.StartPosition = FormStartPosition.CenterParent;
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open texture creation helper: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void chkSanitizer_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkCustomMeshes_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
