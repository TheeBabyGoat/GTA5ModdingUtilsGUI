using System;

using System.Drawing;

using System.Drawing.Imaging;

using System.IO;

using System.Diagnostics;

using System.Collections.Generic;

using System.Windows.Forms;

using GTA5ModdingUtilsGUI.Rendering;



namespace GTA5ModdingUtilsGUI

{

    /// <summary>

    /// Simple 3D preview window that lets the user pick a LOD mesh (.obj)

    /// and an atlas texture, and then shows them mapped together in a small

    /// software renderer.

    /// </summary>

    public class LodAtlasPreviewForm : Form

    {

        private readonly SoftwareMeshViewerControl _viewer;

        private readonly UvEditorControl _uvEditor;

        private readonly ComboBox _cmbMeshPath;

        private readonly ComboBox _cmbUvMode;

        private readonly ComboBox _cmbPropTarget;

        private readonly Button _btnBrowseMesh;

        private string? _meshFolder;

        private readonly TextBox _txtTexturePath;

        private readonly Button _btnBrowseTexture;

        private readonly Button _btnReload;

        private readonly NumericUpDown _nudTextureOrigin;

        private readonly NumericUpDown _nudPlaneZ;



        /// <summary>

        /// Current texture origin in [0,1].

        /// </summary>

        public double TextureOrigin

        {

            get => (double)_nudTextureOrigin.Value;

            set

            {

                decimal v = (decimal)Math.Max(0.0, Math.Min(1.0, value));

                _nudTextureOrigin.Value = v;

            }

        }



        /// <summary>

        /// Current plane Z in [0,1].

        /// </summary>

        public double PlaneZ

        {

            get => (double)_nudPlaneZ.Value;

            set

            {

                decimal v = (decimal)Math.Max(0.0, Math.Min(1.0, value));

                _nudPlaneZ.Value = v;

            }

        }







        /// <summary>

        /// Name of the prop/mapping row that should receive the edited UV parameters

        /// when the user clicks Save in this preview window. This is populated by

        /// the LOD atlas helper based on the current mapping grid.

        /// </summary>

        public string? SelectedPropTarget

        {

            get

            {

                return _cmbPropTarget.SelectedItem as string;

            }

        }



        /// <summary>

        /// Populate the "apply edits to prop" drop-down with all prop names loaded

        /// from the mapping grid. The currently selected row's prop name is used

        /// as the initial selection when possible.

        /// </summary>

        public void SetPropTargets(IEnumerable<string> propNames, string? currentPropName)

        {

            if (_cmbPropTarget == null)

                return;



            _cmbPropTarget.BeginUpdate();

            try

            {

                _cmbPropTarget.Items.Clear();



                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);



                foreach (var raw in propNames)

                {

                    var name = raw?.Trim();

                    if (string.IsNullOrEmpty(name))

                        continue;



                    if (seen.Contains(name))

                        continue;



                    seen.Add(name);

                    _cmbPropTarget.Items.Add(name);

                }



                if (!string.IsNullOrWhiteSpace(currentPropName))

                {

                    int idx = _cmbPropTarget.FindStringExact(currentPropName);

                    if (idx >= 0)

                        _cmbPropTarget.SelectedIndex = idx;

                }



                if (_cmbPropTarget.Items.Count > 0 && _cmbPropTarget.SelectedIndex < 0)

                    _cmbPropTarget.SelectedIndex = 0;

            }

            finally

            {

                _cmbPropTarget.EndUpdate();

            }

        }



        public LodAtlasPreviewForm()

        {

            Text = "LOD Atlas Mesh Preview";

            StartPosition = FormStartPosition.CenterParent;

            MinimumSize = new Size(640, 480);



            // Main layout for the top controls.

            var headerLayout = new TableLayoutPanel

            {

                Dock = DockStyle.Top,

                AutoSize = true,

                AutoSizeMode = AutoSizeMode.GrowAndShrink,

                ColumnCount = 3,

                RowCount = 5,

                Padding = new Padding(8, 8, 8, 4)

            };



            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));          // labels

            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));    // text boxes

            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));         // buttons



            headerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            headerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            headerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            headerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            headerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));



            var lblMesh = new Label

            {

                AutoSize = true,

                Text = "Mesh (.obj):",

                Margin = new Padding(0, 6, 6, 6)

            };



            _cmbMeshPath = new ComboBox

            {

                Dock = DockStyle.Fill,

                Margin = new Padding(0, 3, 6, 3),

                DropDownStyle = ComboBoxStyle.DropDown

            };

            _cmbMeshPath.SelectedIndexChanged += MeshPath_SelectedIndexChanged;



            _btnBrowseMesh = new Button

            {

                Text = "Browse...",

                AutoSize = true,

                AutoSizeMode = AutoSizeMode.GrowAndShrink,

                Margin = new Padding(0, 3, 0, 3)

            };

            _btnBrowseMesh.Click += BtnBrowseMesh_Click;



            _btnReload = new Button

            {

                Text = "Reload",

                AutoSize = true,

                AutoSizeMode = AutoSizeMode.GrowAndShrink,

                Margin = new Padding(0, 3, 6, 3)

            };

            _btnReload.Click += BtnReload_Click;



            // Put Reload + Browse for mesh into a small flow panel.

            var meshButtonsPanel = new FlowLayoutPanel

            {

                FlowDirection = FlowDirection.LeftToRight,

                AutoSize = true,

                AutoSizeMode = AutoSizeMode.GrowAndShrink,

                WrapContents = false,

                Margin = new Padding(0, 0, 0, 0)

            };

            meshButtonsPanel.Controls.Add(_btnReload);

            meshButtonsPanel.Controls.Add(_btnBrowseMesh);



            var lblTexture = new Label

            {

                AutoSize = true,

                Text = "Atlas texture (PNG/JPG/DDS):",

                Margin = new Padding(0, 6, 6, 6)

            };



            _txtTexturePath = new TextBox

            {

                Dock = DockStyle.Fill,

                Margin = new Padding(0, 3, 6, 3)

            };



            _btnBrowseTexture = new Button

            {

                Text = "Browse...",

                AutoSize = true,

                AutoSizeMode = AutoSizeMode.GrowAndShrink,

                Margin = new Padding(0, 3, 0, 3)

            };

            _btnBrowseTexture.Click += BtnBrowseTexture_Click;



            headerLayout.Controls.Add(lblMesh, 0, 0);

            headerLayout.Controls.Add(_cmbMeshPath, 1, 0);

            headerLayout.Controls.Add(meshButtonsPanel, 2, 0);



            headerLayout.Controls.Add(lblTexture, 0, 1);

            headerLayout.Controls.Add(_txtTexturePath, 1, 1);

            headerLayout.Controls.Add(_btnBrowseTexture, 2, 1);



            // Row 2: texture origin / plane Z controls

            var lblParams = new Label

            {

                AutoSize = true,

                Text = "LOD params:",

                Margin = new Padding(0, 6, 6, 6)

            };



            var paramsPanel = new FlowLayoutPanel

            {

                Dock = DockStyle.Fill,

                AutoSize = true,

                AutoSizeMode = AutoSizeMode.GrowAndShrink,

                FlowDirection = FlowDirection.LeftToRight,

                WrapContents = false,

                Margin = new Padding(0, 0, 0, 0),

                Padding = new Padding(0)

            };



            var lblTexOrigin = new Label

            {

                AutoSize = true,

                Text = "Texture origin:",

                Margin = new Padding(0, 6, 4, 6)

            };



            _nudTextureOrigin = new NumericUpDown

            {

                Minimum = 0,

                Maximum = 1,

                DecimalPlaces = 3,

                Increment = 0.01M,

                Width = 80,

                Margin = new Padding(0, 3, 12, 3)

            };



            var lblPlaneZ = new Label

            {

                AutoSize = true,

                Text = "Plane Z:",

                Margin = new Padding(0, 6, 4, 6)

            };



            _nudPlaneZ = new NumericUpDown

            {

                Minimum = 0,

                Maximum = 1,

                DecimalPlaces = 3,

                Increment = 0.01M,

                Width = 80,

                Margin = new Padding(0, 3, 0, 3)

            };



            paramsPanel.Controls.Add(lblTexOrigin);

            paramsPanel.Controls.Add(_nudTextureOrigin);

            paramsPanel.Controls.Add(lblPlaneZ);

            paramsPanel.Controls.Add(_nudPlaneZ);



            // Default values

            _nudTextureOrigin.Value = 0.5M;

            _nudPlaneZ.Value = 0.5M;



            _nudTextureOrigin.ValueChanged += (_, __) =>

            {

                _viewer?.Invalidate();

            };

            _nudPlaneZ.ValueChanged += (_, __) =>

            {

                _viewer?.Invalidate();

            };



            headerLayout.Controls.Add(lblParams, 0, 2);

            headerLayout.Controls.Add(paramsPanel, 1, 2);

            // leave column 2 empty for this row.

            // Row 3: UV transform mode (Move / Scale / Rotate).

            var lblUvMode = new Label

            {

                AutoSize = true,

                Text = "UV mode:",

                Margin = new Padding(0, 6, 6, 6)

            };



            _cmbUvMode = new ComboBox

            {

                Dock = DockStyle.Left,

                DropDownStyle = ComboBoxStyle.DropDownList,

                Margin = new Padding(0, 3, 6, 3),

                Width = 120

            };



            _cmbUvMode.Items.AddRange(new object[] { "Move", "Scale", "Rotate" });

            _cmbUvMode.SelectedIndex = 0;

            _cmbUvMode.SelectedIndexChanged += (_, __) =>

            {

                if (_uvEditor != null && _cmbUvMode.SelectedItem is string modeName)

                {

                    _uvEditor.TransformModeName = modeName;

                }

            };



            headerLayout.Controls.Add(lblUvMode, 0, 3);

            headerLayout.Controls.Add(_cmbUvMode, 1, 3);



            // Optional edit mode for the 3D view: when enabled, right-click

            // on the mesh will pick faces and drive the UV editor selection.

            var chk3dEditMode = new CheckBox

            {

                AutoSize = true,

                Text = "3D edit mode (RMB picks faces)",

                Checked = true,

                Margin = new Padding(6, 6, 0, 6)

            };

            chk3dEditMode.CheckedChanged += (_, __) =>

            {

                if (_viewer != null)

                    _viewer.EditMode = chk3dEditMode.Checked;

            };

            headerLayout.Controls.Add(chk3dEditMode, 2, 3);



            var lblPropTarget = new Label

            {

                AutoSize = true,

                Text = "Apply edits to prop:",

                Margin = new Padding(0, 6, 6, 6)

            };



            _cmbPropTarget = new ComboBox

            {

                Dock = DockStyle.Fill,

                DropDownStyle = ComboBoxStyle.DropDownList,

                Margin = new Padding(0, 3, 6, 3)

            };



            headerLayout.Controls.Add(lblPropTarget, 0, 4);

            headerLayout.Controls.Add(_cmbPropTarget, 1, 4);









            _viewer = new SoftwareMeshViewerControl

            {

                Dock = DockStyle.Fill

            };



            _uvEditor = new UvEditorControl

            {

                Dock = DockStyle.Fill

            };



            var splitContainer = new SplitContainer

            {

                Dock = DockStyle.Fill,

                Orientation = Orientation.Vertical,

                FixedPanel = FixedPanel.None,

                BorderStyle = BorderStyle.None

            };

            splitContainer.Panel1.Controls.Add(_viewer);

            splitContainer.Panel2.Controls.Add(_uvEditor);



            // When UVs change in the 2D editor, re-render the 3D preview.

            _uvEditor.UvChanged += (_, __) => _viewer.Invalidate();



            // When faces are picked in the 3D preview, select the corresponding

            // vertices in the UV editor (Shift+right-click makes the selection additive).

            _viewer.MeshSelectionChanged += (_, args) =>

            {

                if (args?.VertexIndices != null)

                    _uvEditor.SetSelectedVertices(args.VertexIndices, args.Additive);

            };



            Controls.Add(splitContainer);

            Controls.Add(headerLayout);

            // Bottom panel with Save / Close buttons so the caller can

            // decide whether to persist edited values back into the atlas helper.

            var bottomPanel = new FlowLayoutPanel

            {

                Dock = DockStyle.Bottom,

                AutoSize = true,

                AutoSizeMode = AutoSizeMode.GrowAndShrink,

                FlowDirection = FlowDirection.RightToLeft,

                Padding = new Padding(8, 4, 8, 8)

            };



            var btnClose = new Button

            {

                Text = "Close",

                AutoSize = true,

                DialogResult = DialogResult.Cancel,

                Margin = new Padding(6, 3, 0, 3)

            };



            var btnSave = new Button

            {

                Text = "Save && Close",

                AutoSize = true,

                DialogResult = DialogResult.OK,

                Margin = new Padding(6, 3, 0, 3)

            };



            bottomPanel.Controls.Add(btnClose);

            bottomPanel.Controls.Add(btnSave);



            Controls.Add(bottomPanel);



            AcceptButton = btnSave;

            CancelButton = btnClose;





            // Set a reasonable default client size after controls are added.

            ClientSize = new Size(800, 600);

            ApplyTheme(SettingsManager.Current.Theme);
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

            this.BackColor = windowBack;
            this.ForeColor = textColor;

            void ThemeControl(Control c)
            {
                // Keep the mesh preview and UV editor using their own internal styling.
                if (ReferenceEquals(c, _viewer) || ReferenceEquals(c, _uvEditor))
                {
                    return;
                }

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
                    cb.FlatStyle = FlatStyle.Flat;
                }
                else if (c is NumericUpDown nud)
                {
                    nud.BackColor = inputBack;
                    nud.ForeColor = textColor;
                }
                else if (c is Button btn)
                {
                    btn.BackColor = secondaryButton;
                    btn.ForeColor = textColor;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = borderColor;
                }
                else if (c is CheckBox chk)
                {
                    chk.ForeColor = textColor;
                    chk.BackColor = windowBack;
                }
                else if (c is Panel || c is SplitContainer || c is TableLayoutPanel)
                {
                    c.BackColor = groupBack;
                }

                foreach (Control child in c.Controls)
                {
                    ThemeControl(child);
                }
            }

            foreach (Control c in Controls)
            {
                ThemeControl(c);
            }

            // Highlight the primary action button (Save & Close) with the accent color.
            if (AcceptButton is Button okButton)
            {
                okButton.BackColor = accentColor;
                okButton.ForeColor = Color.White;
                okButton.FlatStyle = FlatStyle.Flat;
                okButton.FlatAppearance.BorderSize = 1;
                okButton.FlatAppearance.BorderColor = borderColor;
            }
        }
        /// <summary>

        /// Called by the main LOD helper so that the preview window can

        /// automatically pick up the current atlas the user has selected.

        /// </summary>

        public void TrySetAtlasFromPath(string? atlasPath)

        {

            if (!string.IsNullOrWhiteSpace(atlasPath) && File.Exists(atlasPath))

            {

                _txtTexturePath.Text = atlasPath!;

                LoadTexture(atlasPath!);

            }

        }





        public void TryPopulateMeshListFromFolder(string? folderPath)

        {

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))

                return;



            _meshFolder = folderPath;



            _cmbMeshPath.BeginUpdate();

            _cmbMeshPath.Items.Clear();



            try

            {

                string[] files = Directory.GetFiles(folderPath, "*.obj", SearchOption.AllDirectories);

                Array.Sort(files, StringComparer.OrdinalIgnoreCase);



                foreach (var path in files)

                {

                    string name = Path.GetFileNameWithoutExtension(path);

                    _cmbMeshPath.Items.Add(new MeshListItem(name, path));

                }

            }

            catch

            {

                // ignore I/O errors

            }

            finally

            {

                _cmbMeshPath.EndUpdate();

            }



            if (_cmbMeshPath.Items.Count > 0)

            {

                _cmbMeshPath.SelectedIndex = 0;

            }

        }



        private void MeshPath_SelectedIndexChanged(object? sender, EventArgs e)

        {

            if (_cmbMeshPath.SelectedItem is MeshListItem item && File.Exists(item.Path))

            {

                _cmbMeshPath.Text = item.Path;

                LoadMesh(item.Path);

            }

        }







        private string? TryConvertOpenFormatsToObj(string sourcePath)

        {

            try

            {

                var exeDir = AppDomain.CurrentDomain.BaseDirectory;

                string[] candidates =

                {

            Path.Combine(exeDir, "OpenFormatObjConverter.py"),

            Path.Combine(exeDir, "openformat-to-obj.py")

        };



                string? scriptPath = null;

                foreach (var candidate in candidates)

                {

                    if (File.Exists(candidate))

                    {

                        scriptPath = candidate;

                        break;

                    }

                }



                if (scriptPath == null)

                {

                    MessageBox.Show(this,

                        "No OpenFormats converter script found.\n" +

                        "Place OpenFormatObjConverter.py or openformat-to-obj.py next to the EXE.",

                        Text,

                        MessageBoxButtons.OK,

                        MessageBoxIcon.Warning);

                    return null;

                }



                var ext = Path.GetExtension(sourcePath)?.ToLowerInvariant();

                string argumentPath = sourcePath;

                string? expectedObjPath = null;

                string? oddDirectory = null;



                if (ext == ".odr")

                {

                    // Simple case: single drawable ? OBJ shares same base name.

                    expectedObjPath = Path.ChangeExtension(sourcePath, ".obj");

                }

                else if (ext == ".odd")

                {

                    // Dictionary case: well run the converter on all ODRs in this folder.

                    oddDirectory = Path.GetDirectoryName(sourcePath);

                    if (string.IsNullOrEmpty(oddDirectory) || !Directory.Exists(oddDirectory))

                    {

                        MessageBox.Show(this,

                            "The selected .odd file is not in a valid directory.",

                            Text,

                            MessageBoxButtons.OK,

                            MessageBoxIcon.Warning);

                        return null;

                    }



                    argumentPath = Path.Combine(oddDirectory, "*.odr");

                }

                else

                {

                    // Fallback: treat like a single ODR.

                    expectedObjPath = Path.ChangeExtension(sourcePath, ".obj");

                }



                var psi = new ProcessStartInfo

                {

                    FileName = "python",

                    Arguments = $"\"{scriptPath}\" \"{argumentPath}\"",

                    WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? exeDir,

                    UseShellExecute = false,

                    RedirectStandardOutput = true,

                    RedirectStandardError = true,

                    CreateNoWindow = true

                };



                using (var proc = Process.Start(psi))

                {

                    if (proc == null)

                    {

                        MessageBox.Show(this,

                            "Failed to start Python process.",

                            Text,

                            MessageBoxButtons.OK,

                            MessageBoxIcon.Error);

                        return null;

                    }



                    string stdout = proc.StandardOutput.ReadToEnd();

                    string stderr = proc.StandardError.ReadToEnd();

                    proc.WaitForExit();



                    if (proc.ExitCode != 0)

                    {

                        MessageBox.Show(this,

                            "OpenFormats converter failed:\n\n" + stderr,

                            Text,

                            MessageBoxButtons.OK,

                            MessageBoxIcon.Error);

                        return null;

                    }

                }



                // If the user picked an .odd, pick the best OBJ from that folder.

                if (ext == ".odd")

                {

                    try

                    {

                        var dir = oddDirectory!;

                        string[] objs = Directory.GetFiles(dir, "*.obj");

                        if (objs.Length == 0)

                        {

                            MessageBox.Show(this,

                                "Conversion finished but no OBJ files were found in:\n" + dir,

                                Text,

                                MessageBoxButtons.OK,

                                MessageBoxIcon.Warning);

                            return null;

                        }



                        string dictName = Path.GetFileNameWithoutExtension(sourcePath) ?? string.Empty;

                        string? bestMatch = null;



                        // Prefer an OBJ whose name matches/contains the dictionary name.

                        foreach (var obj in objs)

                        {

                            var baseName = Path.GetFileNameWithoutExtension(obj) ?? string.Empty;

                            if (baseName.Equals(dictName, StringComparison.OrdinalIgnoreCase) ||

                                (!string.IsNullOrEmpty(dictName) &&

                                 baseName.IndexOf(dictName, StringComparison.OrdinalIgnoreCase) >= 0))

                            {

                                bestMatch = obj;

                                break;

                            }

                        }



                        // Fallback: most recently modified OBJ.

                        if (bestMatch == null)

                        {

                            DateTime latest = DateTime.MinValue;

                            foreach (var obj in objs)

                            {

                                DateTime t;

                                try

                                {

                                    t = File.GetLastWriteTimeUtc(obj);

                                }

                                catch

                                {

                                    continue;

                                }



                                if (t > latest)

                                {

                                    latest = t;

                                    bestMatch = obj;

                                }

                            }

                        }



                        if (bestMatch == null)

                        {

                            MessageBox.Show(this,

                                "Conversion finished but a suitable OBJ file could not be determined in:\n" + dir,

                                Text,

                                MessageBoxButtons.OK,

                                MessageBoxIcon.Warning);

                            return null;

                        }



                        return bestMatch;

                    }

                    catch (Exception ex)

                    {

                        MessageBox.Show(this,

                            "Conversion finished but failed to locate OBJ files for the dictionary:\n" + ex.Message,

                            Text,

                            MessageBoxButtons.OK,

                            MessageBoxIcon.Warning);

                        return null;

                    }

                }

                else

                {

                    // .odr (or fallback) case: prefer OBJ with same base name, but

                    // fall back to "best" OBJ in the same directory if needed.

                    var objPath = expectedObjPath!;

                    if (File.Exists(objPath))

                        return objPath;



                    try

                    {

                        var dir = Path.GetDirectoryName(sourcePath);

                        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))

                        {

                            MessageBox.Show(this,

                                "Conversion finished but OBJ not found:\n" + objPath,

                                Text,

                                MessageBoxButtons.OK,

                                MessageBoxIcon.Warning);

                            return null;

                        }



                        var baseName = Path.GetFileNameWithoutExtension(sourcePath) ?? string.Empty;

                        string[] objs = Directory.GetFiles(dir, "*.obj");

                        if (objs.Length == 0)

                        {

                            MessageBox.Show(this,

                                "Conversion finished but no OBJ files were found in:\n" + dir,

                                Text,

                                MessageBoxButtons.OK,

                                MessageBoxIcon.Warning);

                            return null;

                        }



                        string? bestMatch = null;



                        // 1) Prefer OBJ whose name matches or contains the ODR base name.

                        foreach (var obj in objs)

                        {

                            var name = Path.GetFileNameWithoutExtension(obj) ?? string.Empty;

                            if (name.Equals(baseName, StringComparison.OrdinalIgnoreCase) ||

                                (!string.IsNullOrEmpty(baseName) &&

                                 name.IndexOf(baseName, StringComparison.OrdinalIgnoreCase) >= 0))

                            {

                                bestMatch = obj;

                                break;

                            }

                        }



                        // 2) Fallback: most recently modified OBJ in that folder.

                        if (bestMatch == null)

                        {

                            DateTime latest = DateTime.MinValue;

                            foreach (var obj in objs)

                            {

                                DateTime t;

                                try { t = File.GetLastWriteTimeUtc(obj); }

                                catch { continue; }



                                if (t > latest)

                                {

                                    latest = t;

                                    bestMatch = obj;

                                }

                            }

                        }



                        if (bestMatch == null)

                        {

                            MessageBox.Show(this,

                                "Conversion finished but a suitable OBJ file could not be determined in:\n" + dir,

                                Text,

                                MessageBoxButtons.OK,

                                MessageBoxIcon.Warning);

                            return null;

                        }



                        return bestMatch;

                    }

                    catch (Exception ex)

                    {

                        MessageBox.Show(this,

                            "Conversion finished but failed to locate OBJ files:\n" + ex.Message,

                            Text,

                            MessageBoxButtons.OK,

                            MessageBoxIcon.Warning);

                        return null;

                    }

                }



            }

            catch (Exception ex)

            {

                MessageBox.Show(this,

                    "Failed to convert OpenFormats file:\n" + ex.Message,

                    Text,

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Error);

                return null;

            }

        }





        private void BtnBrowseMesh_Click(object? sender, EventArgs e)

        {

            using var ofd = new OpenFileDialog

            {

                Filter = "Meshes (*.obj;*.odr;*.odd)|*.obj;*.odr;*.odd|Wavefront OBJ (*.obj)|*.obj|OpenFormats (*.odr;*.odd)|*.odr;*.odd|All files (*.*)|*.*",

                Title = "Select LOD mesh or OpenFormats file"

            };



            if (ofd.ShowDialog(this) == DialogResult.OK)

            {

                var path = ofd.FileName;

                var ext = Path.GetExtension(path)?.ToLowerInvariant();



                if (ext == ".odr" || ext == ".odd")

                {

                    var objPath = TryConvertOpenFormatsToObj(path);

                    if (!string.IsNullOrWhiteSpace(objPath) && File.Exists(objPath))

                    {

                        _cmbMeshPath.Text = objPath;

                        LoadMesh(objPath);

                    }

                }

                else

                {

                    _cmbMeshPath.Text = path;

                    LoadMesh(path);

                }

            }

        }



        private void BtnBrowseTexture_Click(object? sender, EventArgs e)

        {

            using var ofd = new OpenFileDialog

            {

                Filter = "Image files (*.png;*.jpg;*.jpeg;*.dds)|*.png;*.jpg;*.jpeg;*.dds|All files (*.*)|*.*",

                Title = "Select atlas texture"

            };



            if (ofd.ShowDialog(this) == DialogResult.OK)

            {

                _txtTexturePath.Text = ofd.FileName;

                LoadTexture(ofd.FileName);

            }

        }



        private void BtnReload_Click(object? sender, EventArgs e)

        {

            if (File.Exists(_cmbMeshPath.Text))

            {

                LoadMesh(_cmbMeshPath.Text);

            }



            if (File.Exists(_txtTexturePath.Text))

            {

                LoadTexture(_txtTexturePath.Text);

            }

        }



        private void LoadMesh(string path)

        {

            try

            {

                var mesh = Mesh.LoadFromObj(path);

                _viewer.Mesh = mesh;

                _uvEditor.Mesh = mesh;

            }

            catch (Exception ex)

            {

                MessageBox.Show(this, "Failed to load mesh: " + ex.Message, Text,

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        private 

void LoadTexture(string path)

        {

            try

            {

                // Dispose previous texture if we created it.

                if (_viewer.Texture != null)

                {

                    // The UV editor shares the same Bitmap instance; clear its reference

                    // before disposing to avoid holding on to a disposed image.

                    _uvEditor.Texture = null;



                    _viewer.Texture.Dispose();

                    _viewer.Texture = null;

                }



                Bitmap? texture = null;

                var ext = Path.GetExtension(path)?.ToLowerInvariant();

                if (ext == ".dds")

                {

                    texture = LoadDdsTexture(path);

                }

                else

                {

                    texture = new Bitmap(path);

                }



                _viewer.Texture = texture;

                _uvEditor.Texture = texture;

            }

            catch (Exception ex)

            {

                MessageBox.Show(this, "Failed to load texture: " + ex.Message, Text,

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }









        private static Bitmap LoadDdsTexture(string path)

        {

            using var fs = File.OpenRead(path);

            using var br = new BinaryReader(fs);



            // Verify magic 'DDS '

            var magic = br.ReadBytes(4);

            if (magic.Length != 4 || magic[0] != 'D' || magic[1] != 'D' || magic[2] != 'S' || magic[3] != ' ')

                throw new InvalidDataException("Not a DDS file.");



            // DDS header (124 bytes)

            int headerSize = br.ReadInt32(); // size

            int flags = br.ReadInt32();

            int height = br.ReadInt32();

            int width = br.ReadInt32();

            int pitchOrLinearSize = br.ReadInt32();

            int depth = br.ReadInt32(); // volume depth, unused

            int mipMapCount = br.ReadInt32();



            // reserved1[11]

            br.ReadBytes(44);



            // Pixel format (DDS_PIXELFORMAT)

            int pfSize = br.ReadInt32();

            int pfFlags = br.ReadInt32();

            int fourCC = br.ReadInt32();

            int rgbBitCount = br.ReadInt32();

            int rMask = br.ReadInt32();

            int gMask = br.ReadInt32();

            int bMask = br.ReadInt32();

            int aMask = br.ReadInt32();



            // caps

            br.ReadInt32(); // caps

            br.ReadInt32(); // caps2

            br.ReadInt32(); // caps3

            br.ReadInt32(); // caps4

            br.ReadInt32(); // reserved2



            string fourCCStr = new string(new[]

            {

                (char)(fourCC & 0xFF),

                (char)((fourCC >> 8) & 0xFF),

                (char)((fourCC >> 16) & 0xFF),

                (char)((fourCC >> 24) & 0xFF)

            });



            if (fourCCStr == "DXT1")

                return DecompressDxt1(br, width, height);

            if (fourCCStr == "DXT3")

                return DecompressDxt3(br, width, height);

            if (fourCCStr == "DXT5")

                return DecompressDxt5(br, width, height);



            const int DDPF_RGB = 0x40;

            if ((pfFlags & DDPF_RGB) != 0 && rgbBitCount == 32)

                return LoadUncompressed32(br, width, height, rMask, gMask, bMask, aMask);



            throw new NotSupportedException("Unsupported DDS format: " + fourCCStr);

        }



        private static Bitmap LoadUncompressed32(BinaryReader br, int width, int height,

            int rMask, int gMask, int bMask, int aMask)

        {

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);



            for (int y = 0; y < height; y++)

            {

                for (int x = 0; x < width; x++)

                {

                    uint value = br.ReadUInt32();



                    byte r = ExtractColorComponent(value, rMask);

                    byte g = ExtractColorComponent(value, gMask);

                    byte b = ExtractColorComponent(value, bMask);

                    byte a = aMask != 0 ? ExtractColorComponent(value, aMask) : (byte)255;



                    bmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));

                }

            }



            return bmp;

        }



        private static byte ExtractColorComponent(uint value, int mask)

        {

            if (mask == 0)

                return 0;



            int shift = 0;

            int tempMask = mask;

            while ((tempMask & 1) == 0)

            {

                tempMask >>= 1;

                shift++;

            }



            int component = (int)((value & (uint)mask) >> shift);



            int bitCount = 0;

            tempMask = mask >> shift;

            while (tempMask != 0)

            {

                bitCount++;

                tempMask >>= 1;

            }



            if (bitCount == 0)

                return 0;



            // Scale to 0-255

            return (byte)(component * 255 / ((1 << bitCount) - 1));

        }



        private static Color ColorFromRgb565(ushort v)

        {

            int r = (v >> 11) & 0x1F;

            int g = (v >> 5) & 0x3F;

            int b = v & 0x1F;



            // Expand to 0-255

            r = (r * 255 + 15) / 31;

            g = (g * 255 + 31) / 63;

            b = (b * 255 + 15) / 31;



            return Color.FromArgb(255, r, g, b);

        }



        private static Bitmap DecompressDxt1(BinaryReader br, int width, int height)

        {

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);



            int blocksWide = (width + 3) / 4;

            int blocksHigh = (height + 3) / 4;



            for (int by = 0; by < blocksHigh; by++)

            {

                for (int bx = 0; bx < blocksWide; bx++)

                {

                    ushort c0 = br.ReadUInt16();

                    ushort c1 = br.ReadUInt16();

                    uint codes = br.ReadUInt32();



                    Color color0 = ColorFromRgb565(c0);

                    Color color1 = ColorFromRgb565(c1);

                    Color[] palette = new Color[4];

                    palette[0] = color0;

                    palette[1] = color1;



                    if (c0 > c1)

                    {

                        palette[2] = Color.FromArgb(

                            255,

                            (2 * color0.R + color1.R) / 3,

                            (2 * color0.G + color1.G) / 3,

                            (2 * color0.B + color1.B) / 3);

                        palette[3] = Color.FromArgb(

                            255,

                            (2 * color1.R + color0.R) / 3,

                            (2 * color1.G + color0.G) / 3,

                            (2 * color1.B + color0.B) / 3);

                    }

                    else

                    {

                        palette[2] = Color.FromArgb(

                            255,

                            (color0.R + color1.R) / 2,

                            (color0.G + color1.G) / 2,

                            (color0.B + color1.B) / 2);

                        palette[3] = Color.FromArgb(0, 0, 0, 0); // transparent

                    }



                    for (int row = 0; row < 4; row++)

                    {

                        for (int col = 0; col < 4; col++)

                        {

                            int pixelIndex = row * 4 + col;

                            int code = (int)((codes >> (2 * pixelIndex)) & 0x03);

                            Color c = palette[code];



                            int px = bx * 4 + col;

                            int py = by * 4 + row;

                            if (px < width && py < height)

                            {

                                bmp.SetPixel(px, py, c);

                            }

                        }

                    }

                }

            }



            return bmp;

        }



        private static Bitmap DecompressDxt3(BinaryReader br, int width, int height)

        {

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);



            int blocksWide = (width + 3) / 4;

            int blocksHigh = (height + 3) / 4;



            for (int by = 0; by < blocksHigh; by++)

            {

                for (int bx = 0; bx < blocksWide; bx++)

                {

                    // Alpha: 4 bits per pixel (64 bits)

                    ulong alphaBlock = br.ReadUInt64();



                    ushort c0 = br.ReadUInt16();

                    ushort c1 = br.ReadUInt16();

                    uint codes = br.ReadUInt32();



                    Color color0 = ColorFromRgb565(c0);

                    Color color1 = ColorFromRgb565(c1);

                    Color[] palette = new Color[4];

                    palette[0] = color0;

                    palette[1] = color1;

                    palette[2] = Color.FromArgb(

                        255,

                        (2 * color0.R + color1.R) / 3,

                        (2 * color0.G + color1.G) / 3,

                        (2 * color0.B + color1.B) / 3);

                    palette[3] = Color.FromArgb(

                        255,

                        (2 * color1.R + color0.R) / 3,

                        (2 * color1.G + color0.G) / 3,

                        (2 * color1.B + color0.B) / 3);



                    for (int row = 0; row < 4; row++)

                    {

                        for (int col = 0; col < 4; col++)

                        {

                            int pixelIndex = row * 4 + col;



                            // 4-bit alpha

                            int alphaShift = pixelIndex * 4;

                            byte a = (byte)((alphaBlock >> alphaShift) & 0xF);

                            a = (byte)(a * 17); // scale 0-15 -> 0-255



                            int code = (int)((codes >> (2 * pixelIndex)) & 0x03);

                            Color c = palette[code];

                            c = Color.FromArgb(a, c.R, c.G, c.B);



                            int px = bx * 4 + col;

                            int py = by * 4 + row;

                            if (px < width && py < height)

                            {

                                bmp.SetPixel(px, py, c);

                            }

                        }

                    }

                }

            }



            return bmp;

        }



        private static Bitmap DecompressDxt5(BinaryReader br, int width, int height)

        {

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);



            int blocksWide = (width + 3) / 4;

            int blocksHigh = (height + 3) / 4;



            for (int by = 0; by < blocksHigh; by++)

            {

                for (int bx = 0; bx < blocksWide; bx++)

                {

                    byte alpha0 = br.ReadByte();

                    byte alpha1 = br.ReadByte();

                    byte[] alphaBits = br.ReadBytes(6);



                    // Build alpha palette

                    byte[] alphaPalette = new byte[8];

                    alphaPalette[0] = alpha0;

                    alphaPalette[1] = alpha1;

                    if (alpha0 > alpha1)

                    {

                        alphaPalette[2] = (byte)((6 * alpha0 + 1 * alpha1) / 7);

                        alphaPalette[3] = (byte)((5 * alpha0 + 2 * alpha1) / 7);

                        alphaPalette[4] = (byte)((4 * alpha0 + 3 * alpha1) / 7);

                        alphaPalette[5] = (byte)((3 * alpha0 + 4 * alpha1) / 7);

                        alphaPalette[6] = (byte)((2 * alpha0 + 5 * alpha1) / 7);

                        alphaPalette[7] = (byte)((1 * alpha0 + 6 * alpha1) / 7);

                    }

                    else

                    {

                        alphaPalette[2] = (byte)((4 * alpha0 + 1 * alpha1) / 5);

                        alphaPalette[3] = (byte)((3 * alpha0 + 2 * alpha1) / 5);

                        alphaPalette[4] = (byte)((2 * alpha0 + 3 * alpha1) / 5);

                        alphaPalette[5] = (byte)((1 * alpha0 + 4 * alpha1) / 5);

                        alphaPalette[6] = 0;

                        alphaPalette[7] = 255;

                    }



                    // Read 16 alpha indices, 3 bits each (48 bits = 6 bytes)

                    uint alphaCode1 = (uint)(alphaBits[0] | (alphaBits[1] << 8) | (alphaBits[2] << 16));

                    uint alphaCode2 = (uint)(alphaBits[3] | (alphaBits[4] << 8) | (alphaBits[5] << 16));



                    ushort c0 = br.ReadUInt16();

                    ushort c1 = br.ReadUInt16();

                    uint codes = br.ReadUInt32();



                    Color color0 = ColorFromRgb565(c0);

                    Color color1 = ColorFromRgb565(c1);

                    Color[] palette = new Color[4];

                    palette[0] = color0;

                    palette[1] = color1;

                    palette[2] = Color.FromArgb(

                        255,

                        (2 * color0.R + color1.R) / 3,

                        (2 * color0.G + color1.G) / 3,

                        (2 * color0.B + color1.B) / 3);

                    palette[3] = Color.FromArgb(

                        255,

                        (2 * color1.R + color0.R) / 3,

                        (2 * color1.G + color0.G) / 3,

                        (2 * color1.B + color0.B) / 3);



                    for (int row = 0; row < 4; row++)

                    {

                        for (int col = 0; col < 4; col++)

                        {

                            int pixelIndex = row * 4 + col;



                            int alphaCode;

                            if (pixelIndex < 8)

                            {

                                alphaCode = (int)((alphaCode1 >> (pixelIndex * 3)) & 0x7);

                            }

                            else

                            {

                                alphaCode = (int)((alphaCode2 >> ((pixelIndex - 8) * 3)) & 0x7);

                            }



                            byte a = alphaPalette[alphaCode];



                            int code = (int)((codes >> (2 * pixelIndex)) & 0x03);

                            Color c = palette[code];

                            c = Color.FromArgb(a, c.R, c.G, c.B);



                            int px = bx * 4 + col;

                            int py = by * 4 + row;

                            if (px < width && py < height)

                            {

                                bmp.SetPixel(px, py, c);

                            }

                        }

                    }

                }

            }



            return bmp;

        }



        private sealed class MeshListItem

        {

            public string Name { get; }

            public string Path { get; }



            public MeshListItem(string name, string path)

            {

                Name = name;

                Path = path;

            }



            public override string ToString()

            {

                return Name;

            }

        }



        protected override void Dispose(bool disposing)

        {

            if (disposing)

            {

                _viewer?.Dispose();

            }

            base.Dispose(disposing);

        }

    }

}