using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
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

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;
        private const int HTBOTTOMRIGHT = 17;


        public MainForm()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

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

            // Menu strip for Help / Credits
            var menu = new MenuStrip
            {
                AutoSize = true
            };
            var helpItem = new ToolStripMenuItem("Help");
            var readmeItem = new ToolStripMenuItem("View Readme");
            readmeItem.Click += viewReadmeToolStripMenuItem_Click;
            helpItem.DropDownItems.Add(readmeItem);

            var creditsItem = new ToolStripMenuItem("Credits");
            creditsItem.Click += creditsToolStripMenuItem_Click;

            // Order: Credits | Help  (closest to standard caption buttons)
            menu.Items.Add(creditsItem);
            menu.Items.Add(helpItem);

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

            if (string.IsNullOrEmpty(inputDir) || !Directory.Exists(inputDir))
            {
                MessageBox.Show("Please select a valid input directory.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(prefix))
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
    }
}
