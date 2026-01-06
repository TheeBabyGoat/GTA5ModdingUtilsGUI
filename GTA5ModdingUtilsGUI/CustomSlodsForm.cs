using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel; // Required for Win32Exception

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

            // Ensure the GroupBox text matches the theme
            if (grpObjOverride != null)
            {
                grpObjOverride.ForeColor = textColor;
            }

            // New Inputs
            if (txtSourceOdrPath != null)
            {
                txtSourceOdrPath.BackColor = palette.InputBack;
                txtSourceOdrPath.ForeColor = textColor;
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
            if (string.IsNullOrEmpty(arch)) arch = "Preview";

            string safeArch = string.Join("_", arch.Split(Path.GetInvalidFileNameChars()));
            string objPath = "";
            bool isTempConvert = false;

            // 1. Resolve OBJ Path (Check Temp Conversion first)
            if (chkPreviewSourceOdr.Checked && !string.IsNullOrEmpty(txtSourceOdrPath.Text))
            {
                if (File.Exists(txtSourceOdrPath.Text))
                {
                    string tempDir = Path.Combine(_toolRoot, "temp_preview");
                    Directory.CreateDirectory(tempDir);
                    string tempObj = Path.Combine(tempDir, safeArch + "_preview.obj");

                    bool success = ConvertOdrToObj(txtSourceOdrPath.Text, tempObj, silent: false);
                    if (success && File.Exists(tempObj))
                    {
                        objPath = tempObj;
                        isTempConvert = true;
                    }
                    else return; // Conversion failed
                }
            }

            // 2. Fallback: Check overrides or last converted file
            if (string.IsNullOrEmpty(objPath))
            {
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
            }

            if (string.IsNullOrEmpty(objPath))
            {
                MessageBox.Show("No OBJ found to preview.\n\n1. Check 'Preview Selected ODR' and select an ODR,\n2. Import an OBJ override,\nOR\n3. Select an ODR file and click 'Convert ODR->OBJ'.", "Preview Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 3. Open the Standard Preview Form (LodAtlasPreviewForm)
            try
            {
                using (var preview = new LodAtlasPreviewForm())
                {
                    // Attempt to list other OBJs in the same folder (for easy browsing)
                    try
                    {
                        var folder = Path.GetDirectoryName(objPath);
                        if (!string.IsNullOrEmpty(folder))
                        {
                            preview.TryPopulateMeshListFromFolder(folder);
                        }
                    }
                    catch { }

                    // Open the target mesh
                    preview.TryOpenMeshFromPath(objPath);
                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open 3D preview: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Cleanup temp file after preview closes
            if (isTempConvert)
            {
                try { File.Delete(objPath); } catch { }
            }
        }

        private void btnConvertOdrToObj_Click(object sender, EventArgs e)
        {
            string arch = txtSelectedArchetype.Text.Trim();
            if (string.IsNullOrEmpty(arch)) return;

            string sourceOdr = txtSourceOdrPath.Text.Trim();

            // If text box empty, look in default paths
            if (string.IsNullOrEmpty(sourceOdr))
            {
                string[] searchPaths = {
                    Path.Combine(_toolRoot, "generated", "custom_slods", "slod1", arch + ".odr"),
                    Path.Combine(_toolRoot, "generated", "custom_slods", "slod2", arch + ".odr"),
                    Path.Combine(_toolRoot, "generated", "custom_slods", "slod3", arch + ".odr"),
                    Path.Combine(_toolRoot, "generated", "custom_slods", "slod4", arch + ".odr"),
                    Path.Combine(_toolRoot, "generated", "custom_meshes", arch + ".odr")
                };

                foreach (var p in searchPaths)
                {
                    if (File.Exists(p)) { sourceOdr = p; break; }
                }
            }

            // If still empty, browse
            if (string.IsNullOrEmpty(sourceOdr) || !File.Exists(sourceOdr))
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = $"Find ODR for {arch}";
                    ofd.Filter = "ODR files (*.odr)|*.odr";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        sourceOdr = ofd.FileName;
                        txtSourceOdrPath.Text = sourceOdr;
                    }
                    else return;
                }
            }

            string outDir = Path.Combine(_toolRoot, "custom_mesh_overrides", "converted");
            Directory.CreateDirectory(outDir);
            string outObj = Path.Combine(outDir, arch + ".obj");

            // Explicit conversion click -> always show errors (silent=false)
            bool success = ConvertOdrToObj(sourceOdr, outObj, silent: false);

            if (success)
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
        }

        private bool ConvertOdrToObj(string inputOdr, string outputObj, bool silent)
        {
            string script = Path.Combine(_toolRoot, "odr_to_obj.py");
            if (!File.Exists(script))
            {
                // Fallback: check "Python Toolkit" subfolder
                script = Path.Combine(_toolRoot, "Python Toolkit", "odr_to_obj.py");
            }

            if (!File.Exists(script))
            {
                script = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "odr_to_obj.py");
                if (!File.Exists(script))
                {
                    if (!silent) MessageBox.Show("odr_to_obj.py script not found.\nExpected in root or 'Python Toolkit' folder.", "Script Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            // Check for the .mesh file before running script
            try
            {
                string odrText = File.ReadAllText(inputOdr);
                Match m = Regex.Match(odrText, @"\b([^\s{}]+\.mesh)\b", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    string meshName = m.Groups[1].Value;
                    string meshPath = Path.Combine(Path.GetDirectoryName(inputOdr) ?? "", meshName);
                    if (!File.Exists(meshPath))
                    {
                        if (!silent)
                        {
                            MessageBox.Show($"The ODR file references a mesh file that is missing:\n\n{meshName}\n\nExpected at:\n{meshPath}\n\nPlease ensure the .odr and .mesh files are in the same folder.",
                                "Missing Mesh File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        return false;
                    }
                }
            }
            catch { }

            // PASS NAMED ARGUMENTS --odr and --outObj
            RunPythonScript(script, $"--odr \"{inputOdr}\" --outObj \"{outputObj}\"", silent);
            return File.Exists(outputObj);
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
                // Fallback
                script = Path.Combine(_toolRoot, "Python Toolkit", "OpenFormatObjConverter.py");
            }

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
                    RunPythonScript(script, $"\"{fullObjPath}\" \"{fbd.SelectedPath}\" \"{arch}\"", silent: false);
                    lblStatus.Text = "Ran OBJ -> ODR conversion.";
                }
            }
        }

        private void RunPythonScript(string scriptPath, string args, bool silent)
        {
            string[] pythonCommands = { "python", "py", "python3" };
            string lastError = "";

            foreach (var pyCmd in pythonCommands)
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = pyCmd;
                    psi.Arguments = $"\"{scriptPath}\" {args}";
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.WorkingDirectory = Path.GetDirectoryName(scriptPath);

                    using (Process p = Process.Start(psi))
                    {
                        string outText = p.StandardOutput.ReadToEnd();
                        string errText = p.StandardError.ReadToEnd();
                        p.WaitForExit();

                        if (p.ExitCode != 0)
                        {
                            if (!silent)
                            {
                                MessageBox.Show($"Script Error (Code {p.ExitCode}):\n{errText}\n\nOutput:\n{outText}", "Python Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        return;
                    }
                }
                catch (Win32Exception ex)
                {
                    lastError = ex.Message;
                }
                catch (Exception ex)
                {
                    if (!silent) MessageBox.Show("Unexpected error: " + ex.Message);
                    return;
                }
            }

            if (!silent)
            {
                MessageBox.Show(
                    "Could not find a valid Python installation.\n\n" +
                    "Tried: python, py, python3\n" +
                    "System Error: " + lastError + "\n\n" +
                    "Please verify that Python is installed and 'Add to PATH' was checked during installation.",
                    "Python Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    // Update the new ODR Text Box
                    txtSourceOdrPath.Text = ofd.FileName;

                    lblStatus.Text = $"Selected '{filename}'";
                }
            }
        }
    }
}