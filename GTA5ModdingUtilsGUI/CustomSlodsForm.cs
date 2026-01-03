using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    public partial class CustomSlodsForm : Form
    {
        private readonly string _toolRoot;
        // Theme + selection highlighting
        private ThemePalette? _palette;
        private Color _lineHighlightBack = Color.Empty;
        private int _suppressSelectionSync = 0;

        // Track the most recent ODR -> OBJ conversion
        private string? _lastConvertedObjPath;
        private string? _lastConvertedObjArchetype;

        private bool IsSelectionSyncSuppressed => _suppressSelectionSync > 0;

        public CustomSlodsForm(string toolRoot, string configPath)
        {
            InitializeComponent();
            _toolRoot = toolRoot;

            // Use the passed config path
            txtConfigPath.Text = configPath;

            LoadJson();
            LoadOverrideJson();

            // Apply the active theme immediately
            try
            {
                ApplyTheme(ThemeHelper.GetPalette(SettingsManager.Current.Theme));
            }
            catch
            {
                // Fallback if settings aren't loaded yet
            }
        }

        public void ApplyTheme(ThemePalette palette)
        {
            _palette = palette;

            this.BackColor = palette.WindowBack;
            this.ForeColor = palette.TextColor;

            Color accentColor = palette.AccentColor;
            Color secondaryButton = palette.SecondaryButton;
            Color textColor = palette.TextColor;
            Color borderColor = palette.BorderColor;

            _lineHighlightBack = Color.FromArgb(60, accentColor);

            if (txtMeshes != null)
            {
                txtMeshes.BackColor = palette.InputBack;
                txtMeshes.ForeColor = textColor;
                txtMeshes.BorderStyle = BorderStyle.FixedSingle;
            }

            if (lblStatus != null)
            {
                lblStatus.ForeColor = accentColor;
            }

            // Primary action buttons
            Button[] primaryButtons =
            {
                btnSave
            };

            foreach (var btn in primaryButtons)
            {
                if (btn == null) continue;
                btn.BackColor = accentColor;
                btn.ForeColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = borderColor;
                btn.UseVisualStyleBackColor = false;
            }

            // Secondary / Utility buttons
            Button[] secondaryButtons =
            {
                btnBrowseConfig,
                btnClearList,
                btnAddFromResources,
                btnClose,
                btnImportObjOverride,
                btnClearObjOverride,
                btnOpenOverridesFolder,
                btnOpenIn3DPreview,
                btnConvertObjToOdr,
                btnConvertOdrToObj,
                btnBrowseSourceOdr
            };

            foreach (var btn in secondaryButtons)
            {
                if (btn == null) continue;
                btn.UseVisualStyleBackColor = false;
                btn.BackColor = secondaryButton;
                btn.ForeColor = textColor;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = borderColor;
            }

            HighlightCurrentLine();
        }

        private void LoadJson()
        {
            txtMeshes.Clear();
            string path = txtConfigPath.Text;
            if (!File.Exists(path))
            {
                lblStatus.Text = "Config file not found (will be created on save).";
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        List<string> loaded = new List<string>();
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            loaded.Add(el.ToString());
                        }
                        txtMeshes.Text = string.Join(Environment.NewLine, loaded);
                        lblStatus.Text = $"Loaded {loaded.Count} custom SLOD items.";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading JSON:\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOverrideJson()
        {
            // Placeholder for loading overrides if needed in future
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (string line in txtMeshes.Lines)
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        if (!lines.Contains(trimmed)) lines.Add(trimmed);
                    }
                }

                var opts = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(lines, opts);
                File.WriteAllText(txtConfigPath.Text, json);

                lblStatus.Text = "Saved successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving JSON:\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBrowseConfig_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtConfigPath.Text = ofd.FileName;
                    LoadJson();
                }
            }
        }

        private void btnClearList_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear all items?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                txtMeshes.Clear();
            }
        }

        // [UPDATED] Matches logic from CustomMeshesForm
        private void btnAddFromResources_Click(object sender, EventArgs e)
        {
            try
            {
                // Target the resources/ytyp folder, same as Custom Meshes
                string ytypDir = Path.Combine(_toolRoot, "resources", "ytyp");

                if (!Directory.Exists(ytypDir))
                {
                    MessageBox.Show(this,
                        "Could not find the toolkit YTYP directory:\n\n" + ytypDir + "\n\n" +
                        "To use this picker, place your extracted *.ytyp.xml files under:\n" +
                        "<toolRoot>\\resources\\ytyp",
                        "Custom SLODs",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                lblStatus.Text = "Scanning for archetypes...";
                Application.DoEvents();
                Cursor.Current = Cursors.WaitCursor;

                // Use the shared scanner
                var archetypes = YtypArchetypeScanner.LoadArchetypeNamesFromYtypDirectory(ytypDir);
                Cursor.Current = Cursors.Default;

                lblStatus.Text = $"Found {archetypes.Count} archetypes.";

                if (archetypes.Count == 0)
                {
                    MessageBox.Show(this,
                        "No archetypes were found.\n\n" +
                        "Expected files matching: *.ytyp.xml\n" +
                        "In: " + ytypDir,
                        "Custom SLODs",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using (var form = new SelectArchetypesForm(archetypes))
                {
                    if (_palette != null) form.ApplyTheme(_palette);

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        var existing = new List<string>(txtMeshes.Lines);
                        int addedCount = 0;
                        foreach (var s in form.SelectedArchetypes)
                        {
                            if (!string.IsNullOrWhiteSpace(s) && !existing.Contains(s, StringComparer.OrdinalIgnoreCase))
                            {
                                existing.Add(s);
                                addedCount++;
                            }
                        }
                        txtMeshes.Lines = existing.ToArray();
                        lblStatus.Text = $"Added {addedCount} archetypes.";
                    }
                }
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show($"Failed to load archetypes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtMeshes_Click(object sender, EventArgs e) => HighlightCurrentLine();
        private void txtMeshes_KeyUp(object sender, KeyEventArgs e) => HighlightCurrentLine();
        private void txtMeshes_MouseDown(object sender, MouseEventArgs e) => HighlightCurrentLine();
        private void txtMeshes_KeyDown(object sender, KeyEventArgs e) { }
        private void txtMeshes_TextChanged(object sender, EventArgs e) { }

        private void HighlightCurrentLine()
        {
            if (IsSelectionSyncSuppressed) return;

            int index = txtMeshes.GetFirstCharIndexOfCurrentLine();
            int lineIndex = txtMeshes.GetLineFromCharIndex(index);
            if (lineIndex < 0 || lineIndex >= txtMeshes.Lines.Length) return;

            string lineText = txtMeshes.Lines[lineIndex].Trim();

            txtSelectedArchetype.Text = lineText;
            RefreshOverrideInfo(lineText);
        }

        private void RefreshOverrideInfo(string archetypeName)
        {
            string overridesPath = Path.Combine(_toolRoot, "custom_mesh_overrides.json");
            txtOverrideObjPath.Text = "";

            if (string.IsNullOrEmpty(archetypeName) || !File.Exists(overridesPath)) return;

            try
            {
                string json = File.ReadAllText(overridesPath);
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            if (prop.Name.Equals(archetypeName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (prop.Value.TryGetProperty("obj", out var objVal))
                                {
                                    txtOverrideObjPath.Text = objVal.ToString();
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void btnImportObjOverride_Click(object sender, EventArgs e)
        {
            string arch = txtSelectedArchetype.Text.Trim();
            if (string.IsNullOrEmpty(arch))
            {
                MessageBox.Show("Please select an archetype name in the list first.", "No Archetype Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Wavefront OBJ (*.obj)|*.obj";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string overridesDir = Path.Combine(_toolRoot, "custom_mesh_overrides");
                    Directory.CreateDirectory(overridesDir);

                    string destName = arch + ".obj";
                    string destPath = Path.Combine(overridesDir, destName);

                    try
                    {
                        File.Copy(ofd.FileName, destPath, true);
                        txtOverrideObjPath.Text = Path.Combine("custom_mesh_overrides", destName);
                        UpdateOverrideJson(arch, txtOverrideObjPath.Text);
                        lblStatus.Text = $"Imported OBJ for '{arch}'";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to import OBJ: {ex.Message}");
                    }
                }
            }
        }

        private void btnClearObjOverride_Click(object sender, EventArgs e)
        {
            string arch = txtSelectedArchetype.Text.Trim();
            if (string.IsNullOrEmpty(arch)) return;

            txtOverrideObjPath.Text = "";
            UpdateOverrideJson(arch, null);
            lblStatus.Text = $"Cleared override for '{arch}'";
        }

        private void UpdateOverrideJson(string arch, string? objPath)
        {
            string overridesPath = Path.Combine(_toolRoot, "custom_mesh_overrides.json");
            Dictionary<string, object> data = new Dictionary<string, object>();

            if (File.Exists(overridesPath))
            {
                try
                {
                    string raw = File.ReadAllText(overridesPath);
                    var existing = JsonSerializer.Deserialize<Dictionary<string, object>>(raw);
                    if (existing != null) data = existing;
                }
                catch { }
            }

            string? existingKey = null;
            foreach (var k in data.Keys)
            {
                if (k.Equals(arch, StringComparison.OrdinalIgnoreCase))
                {
                    existingKey = k;
                    break;
                }
            }
            if (existingKey != null) data.Remove(existingKey);

            if (!string.IsNullOrEmpty(objPath))
            {
                data[arch] = new { obj = objPath };
            }

            var opts = new JsonSerializerOptions { WriteIndented = true };
            string outJson = JsonSerializer.Serialize(data, opts);
            File.WriteAllText(overridesPath, outJson);
        }

        private void btnOpenOverridesFolder_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(_toolRoot, "custom_mesh_overrides");
            Directory.CreateDirectory(path);
            Process.Start("explorer.exe", path);
        }

        private void btnOpenIn3DPreview_Click(object sender, EventArgs e)
        {
            string arch = txtSelectedArchetype.Text.Trim();
            if (string.IsNullOrEmpty(arch)) return;

            string objPath = "";

            if (!string.IsNullOrEmpty(txtOverrideObjPath.Text))
            {
                string fullPath = Path.Combine(_toolRoot, txtOverrideObjPath.Text);
                if (File.Exists(fullPath)) objPath = fullPath;
            }

            if (string.IsNullOrEmpty(objPath) &&
                _lastConvertedObjArchetype == arch &&
                !string.IsNullOrEmpty(_lastConvertedObjPath) &&
                File.Exists(_lastConvertedObjPath))
            {
                objPath = _lastConvertedObjPath;
            }

            if (string.IsNullOrEmpty(objPath))
            {
                MessageBox.Show("No OBJ found to preview.\n\n1. Import an OBJ override,\nOR\n2. Select an ODR file and click 'Convert ODR->OBJ'.", "Preview Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var viewForm = new Form())
            {
                viewForm.Text = $"3D Preview - {arch}";
                viewForm.Size = new Size(800, 600);

                if (_palette != null)
                {
                    viewForm.BackColor = _palette.WindowBack;
                    viewForm.ForeColor = _palette.TextColor;
                }

                var viewer = new GTA5ModdingUtilsGUI.Rendering.SoftwareMeshViewerControl();
                viewer.Dock = DockStyle.Fill;
                viewForm.Controls.Add(viewer);

                try
                {
                    var mesh = GTA5ModdingUtilsGUI.Rendering.Mesh.LoadFromObj(objPath);
                    viewer.Mesh = mesh;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load mesh: " + ex.Message);
                    return;
                }

                viewForm.ShowDialog();
            }
        }

        private void btnConvertOdrToObj_Click(object sender, EventArgs e)
        {
            string arch = txtSelectedArchetype.Text.Trim();
            if (string.IsNullOrEmpty(arch)) return;

            string[] searchPaths = {
                Path.Combine(_toolRoot, "generated", "custom_slods", "slod1", arch + ".odr"),
                Path.Combine(_toolRoot, "generated", "custom_slods", "slod2", arch + ".odr"),
                Path.Combine(_toolRoot, "generated", "custom_slods", "slod3", arch + ".odr"),
                Path.Combine(_toolRoot, "generated", "custom_slods", "slod4", arch + ".odr"),
                Path.Combine(_toolRoot, "generated", "custom_meshes", arch + ".odr")
            };

            string sourceOdr = "";
            foreach (var p in searchPaths)
            {
                if (File.Exists(p)) { sourceOdr = p; break; }
            }

            if (string.IsNullOrEmpty(sourceOdr))
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = $"Find ODR for {arch}";
                    ofd.Filter = "ODR files (*.odr)|*.odr";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        sourceOdr = ofd.FileName;
                    }
                    else return;
                }
            }

            string script = Path.Combine(_toolRoot, "odr_to_obj.py");
            if (!File.Exists(script))
            {
                script = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "odr_to_obj.py");
                if (!File.Exists(script))
                {
                    MessageBox.Show("odr_to_obj.py script not found.");
                    return;
                }
            }

            string outDir = Path.Combine(_toolRoot, "custom_mesh_overrides", "converted");
            Directory.CreateDirectory(outDir);
            string outObj = Path.Combine(outDir, arch + ".obj");

            RunPythonScript(script, $"\"{sourceOdr}\" \"{outObj}\"");

            if (File.Exists(outObj))
            {
                lblStatus.Text = "Converted ODR -> OBJ";
                _lastConvertedObjPath = outObj;
                _lastConvertedObjArchetype = arch;

                if (MessageBox.Show("Conversion successful.\n\nDo you want to use this OBJ as the override for this archetype?", "Use as Override?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string finalPath = Path.Combine(_toolRoot, "custom_mesh_overrides", arch + ".obj");
                    File.Copy(outObj, finalPath, true);
                    txtOverrideObjPath.Text = Path.Combine("custom_mesh_overrides", arch + ".obj");
                    UpdateOverrideJson(arch, txtOverrideObjPath.Text);
                }
                else
                {
                    Process.Start("explorer.exe", "/select,\"" + outObj + "\"");
                }
            }
            else
            {
                MessageBox.Show("Conversion failed (output file not created).");
            }
        }

        private void btnConvertObjToOdr_Click(object sender, EventArgs e)
        {
            string arch = txtSelectedArchetype.Text.Trim();
            if (string.IsNullOrEmpty(arch)) return;

            string objPath = txtOverrideObjPath.Text;
            if (string.IsNullOrEmpty(objPath))
            {
                MessageBox.Show("No override OBJ defined to convert.");
                return;
            }

            string fullObjPath = Path.Combine(_toolRoot, objPath);
            if (!File.Exists(fullObjPath))
            {
                MessageBox.Show("OBJ file not found: " + fullObjPath);
                return;
            }

            string script = Path.Combine(_toolRoot, "OpenFormatObjConverter.py");
            if (!File.Exists(script))
            {
                script = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenFormatObjConverter.py");
                if (!File.Exists(script))
                {
                    MessageBox.Show("OpenFormatObjConverter.py script not found.");
                    return;
                }
            }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select output folder for ODR";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    RunPythonScript(script, $"\"{fullObjPath}\" \"{fbd.SelectedPath}\" \"{arch}\"");
                    lblStatus.Text = "Ran OBJ -> ODR conversion.";
                }
            }
        }

        private void RunPythonScript(string scriptPath, string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "python";
                psi.Arguments = $"\"{scriptPath}\" {args}";
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                using (Process p = Process.Start(psi))
                {
                    string outText = p.StandardOutput.ReadToEnd();
                    string errText = p.StandardError.ReadToEnd();
                    p.WaitForExit();

                    if (p.ExitCode != 0)
                    {
                        MessageBox.Show($"Script Error:\n{errText}\n\nOutput:\n{outText}", "Python Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to run python: " + ex.Message);
            }
        }

        private void btnBrowseSourceOdr_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select ODR File to Edit/Convert";
                ofd.Filter = "ODR Files (*.odr)|*.odr|All Files (*.*)|*.*";

                string generatedPath = Path.Combine(_toolRoot, "generated", "custom_slods");
                if (Directory.Exists(generatedPath)) ofd.InitialDirectory = generatedPath;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string filename = Path.GetFileNameWithoutExtension(ofd.FileName);
                    txtSelectedArchetype.Text = filename;
                    RefreshOverrideInfo(filename);
                    lblStatus.Text = $"Selected '{filename}' from {Path.GetFileName(ofd.FileName)}";
                }
            }
        }
    }
}