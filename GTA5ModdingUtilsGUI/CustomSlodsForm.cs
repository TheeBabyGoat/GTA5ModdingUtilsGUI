using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    /// <summary>
    /// Simple helper window that lets the user define "custom slod" archetype
    /// names. These are props that should have specific SLOD models managed
    /// manually or via specific overrides.
    /// 
    /// This form manages "custom_slods.json" and "custom_slod_overrides.json".
    /// </summary>
    public partial class CustomSlodsForm : Form
    {
        private readonly string _toolRoot;
        private readonly string _defaultJsonPath;

        // Theme + selection highlighting
        private ThemePalette? _palette;
        private Color _lineHighlightBack = Color.Empty;
        private int _highlightStart = -1;
        private int _highlightLength = 0;
        private int _suppressSelectionSync = 0;

        private bool IsSelectionSyncSuppressed => _suppressSelectionSync > 0;

        private void WithSelectionSyncSuppressed(Action action)
        {
            _suppressSelectionSync++;
            try { action(); }
            finally { _suppressSelectionSync--; }
        }

        // Track the most recent ODR -> OBJ conversion
        private string? _lastConvertedObjPath;
        private string? _lastConvertedObjArchetype;

        private readonly string _overridesJsonPath;
        private readonly string _overridesFolder;
        private Dictionary<string, MeshOverrideEntry> _overrides = new(StringComparer.OrdinalIgnoreCase);

        private sealed class MeshOverrideEntry
        {
            public string? obj { get; set; }
            public string? diffuseSampler { get; set; }
        }

        public CustomSlodsForm(string toolRoot, string defaultJsonPath)
        {
            _toolRoot = toolRoot;
            _defaultJsonPath = defaultJsonPath;

            // Define specific paths for SLOD overrides
            _overridesJsonPath = Path.Combine(_toolRoot, "custom_slod_overrides.json");
            _overridesFolder = Path.Combine(_toolRoot, "custom_slod_overrides");

            InitializeComponent();

            txtConfigPath.Text = defaultJsonPath;
            lblStatus.Text = string.Empty;

            LoadCustomSlodsFromFile(defaultJsonPath);
            LoadOverridesFromFile(_overridesJsonPath);
            UpdateSelectionInfo();

            // Event wiring
            txtMeshes.MouseUp += (_, __) => UpdateSelectionInfo();
            txtMeshes.Click += (_, __) => UpdateSelectionInfo();
            txtMeshes.KeyUp += (_, __) => UpdateSelectionInfo();
            txtMeshes.Enter += (_, __) => UpdateSelectionInfo();
            txtMeshes.TextChanged += (_, __) => UpdateSelectionInfo();
            txtMeshes.SelectionChanged += (_, __) =>
            {
                if (IsSelectionSyncSuppressed) return;
                UpdateSelectionInfo();
            };

            ApplyTheme(SettingsManager.Current.Theme);
            this.Shown += (_, __) => ApplyTheme(SettingsManager.Current.Theme);
        }

        private void btnBrowseConfig_Click(object? sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog();
            dlg.Title = "Select custom slods JSON path";
            dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            dlg.FileName = Path.GetFileName(_defaultJsonPath);
            var initialDir = Path.GetDirectoryName(_defaultJsonPath);
            if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir))
                dlg.InitialDirectory = initialDir;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                txtConfigPath.Text = dlg.FileName;
                LoadCustomSlodsFromFile(dlg.FileName);
                UpdateSelectionInfo();
            }
        }

        private void btnClearList_Click(object? sender, EventArgs e)
        {
            if (txtMeshes == null) return;

            if (txtMeshes.TextLength == 0)
            {
                lblStatus.Text = "List is already empty.";
                UpdateSelectionInfo();
                return;
            }

            var result = MessageBox.Show(this,
                "Clear all archetype names from the list?\n\n" +
                "This will not update the JSON file until you click Save.",
                "Custom SLODs",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            WithSelectionSyncSuppressed(() =>
            {
                ClearLineHighlight();
                txtMeshes.Clear();
            });

            lblStatus.Text = "Cleared archetype list. Click Save to persist changes.";
            UpdateSelectionInfo();
        }

		private void btnAddFromResources_Click(object? sender, EventArgs e)
        {
            try
            {
                // 1. Locate the directory containing the game's YTYP definitions in the toolkit
                string ytypDir = Path.Combine(_toolRoot, "resources", "ytyp");

                if (!Directory.Exists(ytypDir))
                {
                    MessageBox.Show(this,
                        "Could not find the toolkit YTYP directory:\n\n" + ytypDir +
                        "\n\nPlease ensure the gta5-modding-utils folder is complete.",
                        "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. Scan that directory to get the list of archetype names
                Cursor.Current = Cursors.WaitCursor;
                var archetypes = YtypArchetypeScanner.LoadArchetypeNamesFromYtypDirectory(ytypDir);
                Cursor.Current = Cursors.Default;

                if (archetypes.Count == 0)
                {
                    MessageBox.Show(this,
                        "No archetypes were found in:\n" + ytypDir,
                        "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 3. Pass the LIST (archetypes) to the form, not the path string
                using var dlg = new SelectArchetypesForm(archetypes);

                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                // 4. Add the selected items to the text box (checking for duplicates)
                var selections = dlg.SelectedArchetypes;
                if (selections == null || selections.Count == 0) 
                    return;

                int addedCount = 0;
                var currentLines = new List<string>(txtMeshes.Lines);
                
                // Clean up existing empty lines
                currentLines.RemoveAll(string.IsNullOrWhiteSpace);

                foreach (var name in selections)
                {
                    string trimmed = name.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    // Avoid duplicates
                    bool exists = currentLines.Exists(x => x.Trim().Equals(trimmed, StringComparison.OrdinalIgnoreCase));
                    
                    if (!exists)
                    {
                        currentLines.Add(trimmed);
                        addedCount++;
                    }
                }

                if (addedCount > 0)
                {
                    txtMeshes.Lines = currentLines.ToArray();
                    UpdateSelectionInfo();
                    lblStatus.Text = $"Added {addedCount} archetype(s) from resources.";
                }
                else
                {
                    lblStatus.Text = "No new archetypes were added (duplicates skipped).";
                }
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(this,
                    "Failed to load archetypes from toolkit resources: " + ex.Message,
                    "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                string jsonPath = txtConfigPath.Text.Trim();
                if (string.IsNullOrEmpty(jsonPath))
                {
                    MessageBox.Show(this, "Please specify an output JSON path.", "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var names = new List<string>();
                foreach (var raw in txtMeshes.Lines)
                {
                    var name = (raw ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(name)) continue;

                    if (!names.Exists(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        names.Add(name);
                    }
                }

                if (names.Count == 0)
                {
                    var result = MessageBox.Show(this,
                        "The list of custom slods is empty.\n\n" +
                        "Do you want to clear the JSON file (if it exists)?",
                        "Custom SLODs",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.No) return;
                }

                string? dir = Path.GetDirectoryName(jsonPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(names, options);
                File.WriteAllText(jsonPath, json);

                // Keep OBJ override mappings tidy
                PruneOverridesToNames(names);
                SaveOverridesToFile(_overridesJsonPath);

                lblStatus.Text = $"Saved {names.Count} custom slod name(s) to: {jsonPath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to save custom slods: " + ex.Message, "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadCustomSlodsFromFile(string path)
        {
            txtMeshes.Clear();

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                lblStatus.Text = "No existing custom_slods.json found. Enter archetype names to create one.";
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                if (!string.IsNullOrWhiteSpace(json) && json.TrimStart().StartsWith("["))
                {
                    var names = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                    txtMeshes.Lines = names.ToArray();
                    lblStatus.Text = $"Loaded {names.Count} custom slod name(s) from: {path}";
                }
                else
                {
                    var lines = json.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    txtMeshes.Lines = lines;
                    lblStatus.Text = $"Loaded custom slod list from: {path}";
                }
            }
            catch
            {
                lblStatus.Text = "Failed to parse existing custom_slods file.";
            }
        }

        private void LoadOverridesFromFile(string path)
        {
            _overrides.Clear();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

            try
            {
                string json = File.ReadAllText(path);
                var dict = JsonSerializer.Deserialize<Dictionary<string, MeshOverrideEntry>>(json);
                if (dict != null)
                {
                    _overrides = new Dictionary<string, MeshOverrideEntry>(dict, StringComparer.OrdinalIgnoreCase);
                }
            }
            catch { /* ignore */ }
        }

        private void SaveOverridesToFile(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return;
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_overrides, options);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to save SLOD overrides: " + ex.Message, "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PruneOverridesToNames(List<string> names)
        {
            var set = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            var toRemove = new List<string>();

            foreach (var kvp in _overrides)
            {
                if (!set.Contains(kvp.Key))
                    toRemove.Add(kvp.Key);
            }

            foreach (var k in toRemove)
                _overrides.Remove(k);
        }

        private string GetSelectedArchetypeName()
        {
            try
            {
                int lineIndex = txtMeshes.GetLineFromCharIndex(txtMeshes.SelectionStart);
                var lines = txtMeshes.Lines;
                if (lineIndex < 0 || lineIndex >= lines.Length)
                    return string.Empty;

                return (lines[lineIndex] ?? string.Empty).Trim();
            }
            catch { return string.Empty; }
        }

        private void UpdateSelectionInfo()
        {
            if (txtSelectedArchetype == null || txtOverrideObjPath == null) return;

            string name = GetSelectedArchetypeName();
            txtSelectedArchetype.Text = name;

            if (string.IsNullOrWhiteSpace(name))
            {
                txtOverrideObjPath.Text = string.Empty;
                SetButtonsEnabled(false);
                return;
            }

            if (_overrides.TryGetValue(name, out var entry) && entry != null && !string.IsNullOrWhiteSpace(entry.obj))
            {
                txtOverrideObjPath.Text = entry.obj;
                if (btnClearObjOverride != null) btnClearObjOverride.Enabled = true;

                if (btnConvertObjToOdr != null)
                {
                    string objPath = entry.obj!.Trim();
                    if (!Path.IsPathRooted(objPath))
                        objPath = Path.Combine(_toolRoot, objPath.Replace('/', Path.DirectorySeparatorChar));
                    btnConvertObjToOdr.Enabled = File.Exists(objPath);
                }
            }
            else
            {
                txtOverrideObjPath.Text = "(none)";
                if (btnClearObjOverride != null) btnClearObjOverride.Enabled = false;
                if (btnConvertObjToOdr != null) btnConvertObjToOdr.Enabled = false;
            }

            if (btnImportObjOverride != null) btnImportObjOverride.Enabled = true;
            if (btnOpenOverridesFolder != null) btnOpenOverridesFolder.Enabled = true;
            if (btnConvertOdrToObj != null) btnConvertOdrToObj.Enabled = true;
            if (btnOpenIn3DPreview != null) btnOpenIn3DPreview.Enabled = true;

            HighlightCurrentLine();
        }

        private void SetButtonsEnabled(bool enabled)
        {
            if (btnImportObjOverride != null) btnImportObjOverride.Enabled = enabled;
            if (btnClearObjOverride != null) btnClearObjOverride.Enabled = enabled;
            if (btnConvertObjToOdr != null) btnConvertObjToOdr.Enabled = enabled;
            if (btnConvertOdrToObj != null) btnConvertOdrToObj.Enabled = enabled;
            if (btnOpenIn3DPreview != null) btnOpenIn3DPreview.Enabled = enabled;
        }

        private static Color Blend(Color a, Color b, float t)
        {
            int r = (int)(a.R + (b.R - a.R) * t);
            int g = (int)(a.G + (b.G - a.G) * t);
            int bl = (int)(a.B + (b.B - a.B) * t);
            return Color.FromArgb(r, g, bl);
        }

        private void HighlightCurrentLine()
        {
            if (_palette == null || txtMeshes == null || txtMeshes.IsDisposed) return;

            try
            {
                int caret = txtMeshes.SelectionStart;
                int lineIndex = txtMeshes.GetLineFromCharIndex(caret);
                if (lineIndex < 0) { ClearLineHighlight(); return; }

                int lineStart = txtMeshes.GetFirstCharIndexFromLine(lineIndex);
                if (lineStart < 0) { ClearLineHighlight(); return; }

                int nextLineStart = (lineIndex + 1 < txtMeshes.Lines.Length)
                    ? txtMeshes.GetFirstCharIndexFromLine(lineIndex + 1)
                    : txtMeshes.TextLength;

                int lineLength = Math.Max(0, nextLineStart - lineStart);

                if (_highlightStart == lineStart && _highlightLength == lineLength) return;

                WithSelectionSyncSuppressed(() =>
                {
                    int selStart = txtMeshes.SelectionStart;
                    int selLength = txtMeshes.SelectionLength;
                    ClearLineHighlight();

                    if (lineLength > 0)
                    {
                        txtMeshes.Select(lineStart, lineLength);
                        txtMeshes.SelectionBackColor = _lineHighlightBack;
                        _highlightStart = lineStart;
                        _highlightLength = lineLength;
                    }
                    txtMeshes.Select(selStart, selLength);
                });
            }
            catch { /* ignore */ }
        }

        private void ClearLineHighlight()
        {
            if (_highlightStart < 0 || _highlightLength <= 0) return;
            try
            {
                WithSelectionSyncSuppressed(() =>
                {
                    int selStart = txtMeshes.SelectionStart;
                    int selLength = txtMeshes.SelectionLength;
                    txtMeshes.Select(_highlightStart, _highlightLength);
                    txtMeshes.SelectionBackColor = _palette != null ? _palette.LogBack : txtMeshes.BackColor;
                    txtMeshes.Select(selStart, selLength);
                });
            }
            catch { /* ignore */ }
            finally
            {
                _highlightStart = -1;
                _highlightLength = 0;
            }
        }

        private void btnImportObjOverride_Click(object? sender, EventArgs e)
        {
            string name = GetSelectedArchetypeName();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(this, "Please select a non-empty archetype line first.", "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new OpenFileDialog();
            dlg.Title = "Import custom SLOD OBJ override";
            dlg.Filter = "Wavefront OBJ (*.obj)|*.obj|All files (*.*)|*.*";
            dlg.CheckFileExists = true;

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                Directory.CreateDirectory(_overridesFolder);
                string nameLower = name.Trim().ToLowerInvariant();
                string destPath = Path.Combine(_overridesFolder, nameLower + ".obj");

                if (File.Exists(destPath))
                {
                    if (MessageBox.Show(this, $"An override OBJ already exists for '{name}'.\n\nOverwrite?", "Custom SLODs", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                }

                if (!string.Equals(Path.GetFullPath(dlg.FileName), Path.GetFullPath(destPath), StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(dlg.FileName, destPath, true);
                }

                string rel = Path.GetRelativePath(_toolRoot, destPath);

                // IMPORTANT: Default sampler for SLODs uses "slod_" prefix to differentiate from standard LODs
                _overrides[name] = new MeshOverrideEntry
                {
                    obj = rel.Replace('\\', '/'),
                    diffuseSampler = "slod_" + nameLower
                };

                SaveOverridesToFile(_overridesJsonPath);
                UpdateSelectionInfo();
                lblStatus.Text = $"Imported SLOD override for '{name}': {rel}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to import SLOD override: " + ex.Message, "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearObjOverride_Click(object? sender, EventArgs e)
        {
            string name = GetSelectedArchetypeName();
            if (string.IsNullOrWhiteSpace(name)) return;

            if (_overrides.Remove(name))
            {
                SaveOverridesToFile(_overridesJsonPath);
                UpdateSelectionInfo();
                lblStatus.Text = $"Cleared SLOD override for '{name}'.";
            }
        }

        private void btnOpenOverridesFolder_Click(object? sender, EventArgs e)
        {
            try
            {
                Directory.CreateDirectory(_overridesFolder);
                Process.Start(new ProcessStartInfo { FileName = _overridesFolder, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to open folder: " + ex.Message, "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConvertObjToOdr_Click(object? sender, EventArgs e)
        {
            string name = GetSelectedArchetypeName();
            if (string.IsNullOrWhiteSpace(name)) return;

            if (!_overrides.TryGetValue(name, out var entry) || entry == null || string.IsNullOrWhiteSpace(entry.obj))
            {
                MessageBox.Show(this, "No OBJ override found for this SLOD.", "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string objPath = entry.obj.Trim();
            if (!Path.IsPathRooted(objPath))
                objPath = Path.Combine(_toolRoot, objPath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(objPath))
            {
                MessageBox.Show(this, "OBJ override file not found:\n" + objPath, "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string scriptPath = Path.Combine(_toolRoot, "obj_to_odr.py");
            if (!File.Exists(scriptPath))
            {
                MessageBox.Show(this, "Could not find obj_to_odr.py.", "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string outDir = Path.GetDirectoryName(objPath) ?? _overridesFolder;
            string nameLower = name.Trim().ToLowerInvariant();
            // Default sampler prefix for SLODs is "slod_"
            string sampler = (entry.diffuseSampler ?? ("slod_" + nameLower)).Trim();

            try
            {
                lblStatus.Text = "Converting OBJ -> ODR (SLOD)...";
                lblStatus.Refresh();

                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _toolRoot
                };

                psi.ArgumentList.Add(scriptPath);
                psi.ArgumentList.Add("--obj");
                psi.ArgumentList.Add(objPath);
                psi.ArgumentList.Add("--outDir");
                psi.ArgumentList.Add(outDir);
                psi.ArgumentList.Add("--name");
                psi.ArgumentList.Add(name);
                psi.ArgumentList.Add("--diffuseSampler");
                psi.ArgumentList.Add(sampler);

                using var proc = Process.Start(psi);
                if (proc == null) throw new Exception("Failed to start python process.");

                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    string stderr = proc.StandardError.ReadToEnd();
                    MessageBox.Show(this, "Conversion failed:\n" + stderr, "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Conversion failed.";
                    return;
                }

                string odrOut = Path.Combine(outDir, nameLower + ".odr");
                lblStatus.Text = "Converted SLOD: " + Path.GetFileName(odrOut);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error: " + ex.Message, "Custom SLODs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- MISSING METHODS ADDED BELOW ---

        private void btnConvertOdrToObj_Click(object? sender, EventArgs e)
        {
            string name = GetSelectedArchetypeName();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(this,
                    "Please place the caret on (or select) a non-empty archetype line first.",
                    "Custom SLODs",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string scriptPath = Path.Combine(_toolRoot, "odr_to_obj.py");
            if (!File.Exists(scriptPath))
            {
                MessageBox.Show(this,
                    $"Could not find odr_to_obj.py in the tool folder.\r\n\r\nExpected:\r\n{scriptPath}",
                    "Custom SLODs",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string nameLower = name.Trim().ToLowerInvariant();

            using var odrDlg = new OpenFileDialog();
            odrDlg.Title = "Select ODR to convert to OBJ";
            odrDlg.Filter = "OpenFormats Drawable (*.odr)|*.odr|All files (*.*)|*.*";
            odrDlg.CheckFileExists = true;
            odrDlg.Multiselect = false;
            if (Directory.Exists(_overridesFolder))
                odrDlg.InitialDirectory = _overridesFolder;
            odrDlg.FileName = nameLower + ".odr";

            if (odrDlg.ShowDialog(this) != DialogResult.OK)
                return;

            string odrPath = odrDlg.FileName;

            using var objDlg = new SaveFileDialog();
            objDlg.Title = "Save OBJ";
            objDlg.Filter = "Wavefront OBJ (*.obj)|*.obj|All files (*.*)|*.*";
            objDlg.FileName = nameLower + "_uv.obj";
            string outDir = Path.GetDirectoryName(odrPath) ?? _overridesFolder;
            if (!string.IsNullOrEmpty(outDir) && Directory.Exists(outDir))
                objDlg.InitialDirectory = outDir;

            if (objDlg.ShowDialog(this) != DialogResult.OK)
                return;

            string outObj = objDlg.FileName;

            try
            {
                lblStatus.Text = "Converting ODR -> OBJ...";
                lblStatus.Refresh();

                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _toolRoot
                };

                psi.ArgumentList.Add(scriptPath);
                psi.ArgumentList.Add("--odr");
                psi.ArgumentList.Add(odrPath);
                psi.ArgumentList.Add("--outObj");
                psi.ArgumentList.Add(outObj);

                using var proc = Process.Start(psi);
                if (proc == null)
                    throw new Exception("Failed to start python process.");

                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    MessageBox.Show(this,
                        "ODR -> OBJ conversion failed:\r\n\r\n" + stderr,
                        "Custom SLODs",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    lblStatus.Text = "Conversion failed.";
                    return;
                }

                lblStatus.Text = "Converted: " + Path.GetFileName(outObj);

                // Remember this output so the user can open it immediately in the 3D preview.
                _lastConvertedObjPath = outObj;
                _lastConvertedObjArchetype = name;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to convert ODR to OBJ: " + ex.Message,
                    "Custom SLODs",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                lblStatus.Text = "Conversion failed.";
            }
        }

        private string? ResolvePreviewObjPath(string archetypeName)
        {
            if (string.IsNullOrWhiteSpace(archetypeName))
                return null;

            // Prefer the most recent conversion for this archetype (ODR -> OBJ).
            if (!string.IsNullOrWhiteSpace(_lastConvertedObjPath)
                && !string.IsNullOrWhiteSpace(_lastConvertedObjArchetype)
                && string.Equals(_lastConvertedObjArchetype, archetypeName, StringComparison.OrdinalIgnoreCase)
                && File.Exists(_lastConvertedObjPath))
            {
                return _lastConvertedObjPath;
            }

            // Fall back to the registered override entry.
            if (_overrides.TryGetValue(archetypeName, out var entry) && entry != null && !string.IsNullOrWhiteSpace(entry.obj))
            {
                string objPath = entry.obj!.Trim();
                if (!Path.IsPathRooted(objPath))
                    objPath = Path.Combine(_toolRoot, objPath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(objPath))
                    return objPath;
            }

            // Final fall-back: expected default override name in the overrides folder.
            string candidate = Path.Combine(_overridesFolder, archetypeName.Trim().ToLowerInvariant() + ".obj");
            if (File.Exists(candidate))
                return candidate;

            return null;
        }

        private void btnOpenIn3DPreview_Click(object? sender, EventArgs e)
        {
            string name = GetSelectedArchetypeName();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(this,
                    "Please place the caret on (or select) a non-empty archetype line first.",
                    "Custom SLODs",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string? objPath = ResolvePreviewObjPath(name);

            if (string.IsNullOrWhiteSpace(objPath) || !File.Exists(objPath))
            {
                using var dlg = new OpenFileDialog();
                dlg.Title = "Select OBJ to preview";
                dlg.Filter = "Wavefront OBJ (*.obj)|*.obj|All files (*.*)|*.*";
                dlg.CheckFileExists = true;
                dlg.Multiselect = false;
                if (Directory.Exists(_overridesFolder))
                    dlg.InitialDirectory = _overridesFolder;

                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                objPath = dlg.FileName;
            }

            try
            {
                using var preview = new LodAtlasPreviewForm();

                // Populate the mesh dropdown using the folder containing this OBJ.
                try
                {
                    var folder = Path.GetDirectoryName(objPath);
                    preview.TryPopulateMeshListFromFolder(folder);
                }
                catch
                {
                    // non-fatal
                }

                preview.TryOpenMeshFromPath(objPath);
                preview.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to open 3D preview: " + ex.Message,
                    "Custom SLODs",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme(AppTheme theme)
        {
            ThemePalette palette = ThemeHelper.GetPalette(theme);

            // Cache palette for selection highlighting.
            _palette = palette;
            _lineHighlightBack = Blend(palette.AccentColor, palette.LogBack, 0.80f);

            Color windowBack = palette.WindowBack;
            Color inputBack = palette.InputBack;
            Color textColor = palette.TextColor;
            Color accentColor = palette.AccentColor;
            Color secondaryButton = palette.SecondaryButton;
            Color borderColor = palette.BorderColor;

            this.BackColor = windowBack;
            this.ForeColor = textColor;

            if (lblInfo != null) lblInfo.ForeColor = textColor;
            if (lblConfigPath != null) lblConfigPath.ForeColor = textColor;
            if (lblMeshes != null) lblMeshes.ForeColor = textColor;

            if (txtConfigPath != null)
            {
                txtConfigPath.BackColor = inputBack;
                txtConfigPath.ForeColor = textColor;
                txtConfigPath.BorderStyle = BorderStyle.FixedSingle;
            }

            if (txtMeshes != null)
            {
                txtMeshes.BackColor = palette.LogBack;
                txtMeshes.ForeColor = palette.LogText;
                txtMeshes.BorderStyle = BorderStyle.FixedSingle;
            }

            if (grpObjOverride != null)
            {
                grpObjOverride.ForeColor = textColor;
                grpObjOverride.BackColor = windowBack;
            }

            if (txtSelectedArchetype != null)
            {
                txtSelectedArchetype.BackColor = inputBack;
                txtSelectedArchetype.ForeColor = textColor;
                txtSelectedArchetype.BorderStyle = BorderStyle.FixedSingle;
            }

            if (txtOverrideObjPath != null)
            {
                txtOverrideObjPath.BackColor = inputBack;
                txtOverrideObjPath.ForeColor = textColor;
                txtOverrideObjPath.BorderStyle = BorderStyle.FixedSingle;
            }

            if (lblStatus != null)
            {
                lblStatus.ForeColor = accentColor;
            }

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
                btnConvertOdrToObj
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
    }
}