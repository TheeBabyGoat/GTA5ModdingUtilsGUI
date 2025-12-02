using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GTA5ModdingUtilsGUI
{
    public partial class LodAtlasHelperForm : Form
    {
        private readonly string _toolRoot;
        private readonly string _defaultJsonPath;

        public LodAtlasHelperForm(string toolRoot, string defaultJsonPath)
        {
            _toolRoot = toolRoot;
            _defaultJsonPath = defaultJsonPath;

            InitializeComponent();

            txtOutputJson.Text = defaultJsonPath;
            lblStatus.Text = string.Empty;

            InitializePresets();
            ApplyTheme(SettingsManager.Current.Theme);
        }

        private void btnBrowseAtlas_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog();
            dlg.Title = "Select LOD / SLOD atlas texture";
            dlg.Filter = "Image files (*.png;*.dds;*.jpg;*.jpeg;*.bmp;*.tga)|*.png;*.dds;*.jpg;*.jpeg;*.bmp;*.tga|All files (*.*)|*.*";
            dlg.CheckFileExists = true;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                txtAtlasPath.Text = dlg.FileName;
            }
        }

        private void btnBrowsePropsXml_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog();
            dlg.Title = "Select props / YTYP XML file";
            dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.CheckFileExists = true;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                txtPropsXml.Text = dlg.FileName;
                LoadPropsFromXml(dlg.FileName);
            }
        }


        
        
        private void btnPreviewMesh_Click(object sender, EventArgs e)
        {
            using var preview = new LodAtlasPreviewForm();

            // Pass the currently selected atlas texture, if any.
            if (!string.IsNullOrWhiteSpace(txtAtlasPath.Text) && File.Exists(txtAtlasPath.Text))
            {
                preview.TrySetAtlasFromPath(txtAtlasPath.Text);
            }

            // Try to populate the mesh list from the folder that contains the JSON output.
            if (!string.IsNullOrWhiteSpace(txtOutputJson.Text))
            {
                try
                {
                    var folder = Path.GetDirectoryName(txtOutputJson.Text);
                    preview.TryPopulateMeshListFromFolder(folder);
                }
                catch
                {
                    // Ignore any issues deriving the folder.
                }
            }

            // Figure out the currently active mapping row, if any, so we can
            // seed the preview with its settings and also use it as a fallback
            // when applying edited values.
            DataGridViewRow? currentRow = null;
            if (dgvMappings.SelectedRows.Count > 0)
            {
                currentRow = dgvMappings.SelectedRows[0];
            }
            else if (dgvMappings.CurrentRow != null && !dgvMappings.CurrentRow.IsNewRow)
            {
                currentRow = dgvMappings.CurrentRow;
            }

            string? currentPropName = null;

            if (currentRow != null)
            {
                try
                {
                    double textureOrigin = TryGetDouble(currentRow.Cells[colTextureOrigin.Index], 0.5);
                    double planeZ = TryGetDouble(currentRow.Cells[colPlaneZ.Index], 0.5);

                    preview.TextureOrigin = textureOrigin;
                    preview.PlaneZ = planeZ;

                    currentPropName = Convert.ToString(currentRow.Cells[colPropName.Index].Value)?.Trim();
                }
                catch
                {
                    // Ignore parsing issues; preview will keep default values.
                }
            }

            // Build the list of available prop names so the preview can show
            // a drop-down allowing the user to choose which mapping row
            // should receive the edited values when saving.
            var propNames = new List<string>();
            foreach (DataGridViewRow dgRow in dgvMappings.Rows)
            {
                if (dgRow.IsNewRow)
                    continue;

                string? propName = Convert.ToString(dgRow.Cells[colPropName.Index].Value)?.Trim();
                if (string.IsNullOrEmpty(propName))
                    continue;

                bool exists = false;
                foreach (string existing in propNames)
                {
                    if (string.Equals(existing, propName, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                    propNames.Add(propName);
            }

            if (propNames.Count > 0)
            {
                preview.SetPropTargets(propNames, currentPropName);
            }

            var dialogResult = preview.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                // Decide which mapping row to update based on the selected
                // prop name from the preview, falling back to the current
                // row if needed.
                DataGridViewRow? targetRow = currentRow;
                string? selectedProp = preview.SelectedPropTarget;

                if (!string.IsNullOrWhiteSpace(selectedProp))
                {
                    foreach (DataGridViewRow dgRow in dgvMappings.Rows)
                    {
                        if (dgRow.IsNewRow)
                            continue;

                        string? propName = Convert.ToString(dgRow.Cells[colPropName.Index].Value)?.Trim();
                        if (string.Equals(propName, selectedProp, StringComparison.OrdinalIgnoreCase))
                        {
                            targetRow = dgRow;
                            break;
                        }
                    }
                }

                if (targetRow != null)
                {
                    targetRow.Cells[colTextureOrigin.Index].Value =
                        preview.TextureOrigin.ToString("0.###", CultureInfo.InvariantCulture);
                    targetRow.Cells[colPlaneZ.Index].Value =
                        preview.PlaneZ.ToString("0.###", CultureInfo.InvariantCulture);
                }
            }
            else
            {
                // For non-OK closes we simply show the window without
                // pushing values back into the mapping grid.
            }
        }

        private void LoadPropsFromXml(string xmlPath)
        {
            try
            {
                dgvMappings.Rows.Clear();

                var doc = XDocument.Load(xmlPath);
                int count = 0;

                foreach (var item in doc.Descendants("Item"))
                {
                    var nameElem = item.Element("name");
                    if (nameElem == null)
                        continue;

                    var name = nameElem.Value?.Trim();
                    if (string.IsNullOrEmpty(name))
                        continue;

                    int idx = dgvMappings.Rows.Add();
                    var row = dgvMappings.Rows[idx];
                    row.Cells[colPropName.Index].Value = name;
                    row.Cells[colRow.Index].Value = 0;
                    row.Cells[colCol.Index].Value = 0;
                    row.Cells[colTextureOrigin.Index].Value = "0.5";
                    row.Cells[colPlaneZ.Index].Value = "0.5";

                    count++;
                }

                lblStatus.Text = $"Loaded {count} props from XML.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to load props XML: " + ex.Message,
                    "LOD Atlas Helper",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnBrowseOutputJson_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog();
            dlg.Title = "Select output JSON path";
            dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            dlg.FileName = Path.GetFileName(_defaultJsonPath);
            var initialDir = Path.GetDirectoryName(_defaultJsonPath);
            if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir))
                dlg.InitialDirectory = initialDir;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                txtOutputJson.Text = dlg.FileName;
            }
        }

        private void btnGenerateJson_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = (int)nudRows.Value;
                int cols = (int)nudCols.Value;
                if (rows <= 0 || cols <= 0)
                {
                    MessageBox.Show(this,
                        "Rows and columns must be greater than zero.",
                        "LOD Atlas Helper",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string jsonPath = txtOutputJson.Text.Trim();
                if (string.IsNullOrEmpty(jsonPath))
                {
                    MessageBox.Show(this,
                        "Please specify an output JSON path.",
                        "LOD Atlas Helper",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var result = new Dictionary<string, LodCandidateJson>(StringComparer.OrdinalIgnoreCase);

                foreach (DataGridViewRow dgRow in dgvMappings.Rows)
                {
                    if (dgRow.IsNewRow)
                        continue;

                    string? propName = Convert.ToString(dgRow.Cells[colPropName.Index].Value)?.Trim();
                    if (string.IsNullOrEmpty(propName))
                        continue;

                    int rowIndex = TryGetInt(dgRow.Cells[colRow.Index], 0);
                    int colIndex = TryGetInt(dgRow.Cells[colCol.Index], 0);

                    if (rowIndex < 0) rowIndex = 0;
                    if (rowIndex >= rows) rowIndex = rows - 1;
                    if (colIndex < 0) colIndex = 0;
                    if (colIndex >= cols) colIndex = cols - 1;

                    double textureOrigin = TryGetDouble(dgRow.Cells[colTextureOrigin.Index], 0.5);
                    double planeZ = TryGetDouble(dgRow.Cells[colPlaneZ.Index], 0.5);

                    double tileWidth = 1.0 / cols;
                    double tileHeight = 1.0 / rows;

                    double u0 = colIndex * tileWidth;
                    double u1 = (colIndex + 1) * tileWidth;
                    double v0 = rowIndex * tileHeight;
                    double v1 = (rowIndex + 1) * tileHeight;

                    double frontHeightRatio = GetFrontHeightRatio();
                    bool fullOverlap = IsFullOverlapMode();
                    double vSplit = v0 + tileHeight * frontHeightRatio;

                    var entry = new LodCandidateJson
                    {
                        TextureOrigin = textureOrigin,
                        PlaneZ = planeZ,
                        UVFrontMin = new[] { u0, v0 },
                        UVFrontMax = new[] { u1, fullOverlap ? v1 : vSplit },
                        UVTopMin = new[] { u0, fullOverlap ? v0 : vSplit },
                        UVTopMax = new[] { u1, v1 }
                    };

                    result[propName] = entry;
                }

                if (result.Count == 0)
                {
                    MessageBox.Show(this,
                        "No valid rows found in the mapping table. Please fill in prop names, rows and columns.",
                        "LOD Atlas Helper",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string? dir = Path.GetDirectoryName(jsonPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(result, options);
                File.WriteAllText(jsonPath, json);

                MessageBox.Show(this,
                    $"Saved {result.Count} LOD candidates to:\n\n{jsonPath}\n\n" +
                    "Make sure the Python LODMapCreator loads this JSON file.",
                    "LOD Atlas Helper",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to generate JSON: " + ex.Message,
                    "LOD Atlas Helper",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static double TryGetDouble(DataGridViewCell cell, double defaultValue)
        {
            string? s = Convert.ToString(cell.Value)?.Trim();
            if (string.IsNullOrEmpty(s))
                return defaultValue;

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                return value;

            if (double.TryParse(s, out value))
                return value;

            return defaultValue;
        }

        private static int TryGetInt(DataGridViewCell cell, int defaultValue)
        {
            string? s = Convert.ToString(cell.Value)?.Trim();
            if (string.IsNullOrEmpty(s))
                return defaultValue;

            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                return value;

            if (int.TryParse(s, out value))
                return value;

            return defaultValue;
        }


        private double GetFrontHeightRatio()
        {
            // Default to an even split if anything unexpected happens.
            double ratio = 0.5;

            try
            {
                if (cmbSplitMode != null && cmbSplitMode.SelectedItem != null)
                {
                    string text = cmbSplitMode.SelectedItem.ToString() ?? string.Empty;

                    // Try to parse the leading percentage before the '%%' sign, e.g. "75%% front / 25%% top"
                    int percentIndex = text.IndexOf('%');
                    if (percentIndex > 0)
                    {
                        string numberPart = text.Substring(0, percentIndex).Trim();
                        if (double.TryParse(numberPart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double parsedPercent))
                        {
                            ratio = Math.Clamp(parsedPercent / 100.0, 0.0, 1.0);
                            return ratio;
                        }
                    }

                    // Fallback based on known indexes.
                    switch (cmbSplitMode.SelectedIndex)
                    {
                        case 0:
                            // 50% front / 50% top
                            ratio = 0.5;
                            break;
                        case 1:
                            // 75% front / 25% top
                            ratio = 0.75;
                            break;
                        case 2:
                            // 100% front / 0% top
                            ratio = 1.0;
                            break;
                        case 3:
                            // 25% front / 75% top
                            ratio = 0.25;
                            break;
                        case 4:
                            // 0% front / 100% top
                            ratio = 0.0;
                            break;
                        case 5:
                            // 100% top / 100% front (full overlap)
                            ratio = 1.0;
                            break;
                        default:
                            ratio = 0.5;
                            break;
                    }
                }
            }
            catch
            {
                ratio = 0.5;
            }

            return ratio;
        }

        private bool IsFullOverlapMode()
        {
            try
            {
                if (cmbSplitMode != null && cmbSplitMode.SelectedItem != null)
                {
                    string text = cmbSplitMode.SelectedItem.ToString() ?? string.Empty;
                    if (text.IndexOf("100% top / 100% front", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore and fall back to false.
            }

            return false;
        }

        private static readonly Dictionary<string, (string TextureOrigin, string PlaneZ)> s_presets =
            new Dictionary<string, (string TextureOrigin, string PlaneZ)>(StringComparer.OrdinalIgnoreCase)
            {
                ["prop_bush_lrg_04b"] = ("0.375", "0.46875"),
                ["prop_bush_lrg_04c"] = ("0.38157894736", "0.453125"),
                ["prop_bush_lrg_04d"] = ("0.38970588235", "0.484375"),
                ["prop_palm_fan_02_b"] = ("0.515625", "0.13125"),
                ["prop_palm_fan_03_c"] = ("0.5", "0.08854166666"),
                ["prop_palm_fan_03_d"] = ("0.484375", "0.08173076923"),
                ["prop_palm_fan_04_b"] = ("0.484375", "0.20625"),
                ["prop_palm_fan_04_c"] = ("0.5", "0.140625"),
                ["prop_palm_fan_04_d"] = ("0.421875", "0.12019230769"),
                ["prop_palm_huge_01a"] = ("0.484375", "0.05092592592"),
                ["prop_palm_huge_01b"] = ("0.4765625", "0.04166666666"),
                ["prop_palm_med_01b"] = ("0.515625", "0.17613636363"),
                ["prop_palm_med_01c"] = ("0.515625", "0.16666666666"),
                ["prop_palm_med_01d"] = ("0.5", "0.11057692307"),
                ["prop_rus_olive"] = ("0.484375", "0.546875"),
                ["prop_s_pine_dead_01"] = ("0.40625", "0.4875"),
                ["prop_tree_birch_01"] = ("0.546875", "0.6484375"),
                ["prop_tree_birch_02"] = ("0.421875", "0.4765625"),
                ["prop_tree_birch_04"] = ("0.5625", "0.3515625"),
                ["prop_tree_cedar_02"] = ("0.5078125", "0.40104166666"),
                ["prop_tree_cedar_03"] = ("0.5234375", "0.46875"),
                ["prop_tree_cedar_04"] = ("0.484375", "0.34375"),
                ["prop_tree_cedar_s_01"] = ("0.484375", "0.66875"),
                ["prop_tree_cedar_s_04"] = ("0.5", "0.67307692307"),
                ["prop_tree_cypress_01"] = ("0.5", "0.66666666666"),
                ["prop_tree_eng_oak_01"] = ("0.5", "0.375"),
                ["prop_tree_eucalip_01"] = ("0.5", "0.28125"),
                ["prop_tree_jacada_01"] = ("0.484375", "0.421875"),
                ["prop_tree_jacada_02"] = ("0.515625", "0.34375"),
                ["prop_tree_lficus_02"] = ("0.4453125", "0.359375"),
                ["prop_tree_lficus_03"] = ("0.46875", "0.21875"),
                ["prop_tree_lficus_05"] = ("0.46875", "0.203125"),
                ["prop_tree_lficus_06"] = ("0.453125", "0.25"),
                ["prop_tree_oak_01"] = ("0.46875", "0.453125"),
                ["prop_tree_olive_01"] = ("0.5", "0.375"),
                ["prop_tree_pine_01"] = ("0.515625", "0.50625"),
                ["prop_tree_pine_02"] = ("0.546875", "0.63125"),
                ["prop_w_r_cedar_01"] = ("0.515625", "0.67708333333"),
                ["prop_w_r_cedar_dead"] = ("0.59375", "0.425"),
                ["test_tree_cedar_trunk_001"] = ("0.5234375", "0.54807692307"),
                ["test_tree_forest_trunk_01"] = ("0.515625", "0.54807692307"),
            };

        private void InitializePresets()
        {
            if (cmbTemplateTree == null)
                return;

            cmbTemplateTree.Items.Clear();

            var keys = new List<string>(s_presets.Keys);
            keys.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (var key in keys)
            {
                cmbTemplateTree.Items.Add(key);
            }
        }

        private void btnApplyTemplate_Click(object sender, EventArgs e)
        {
            string? assetName = cmbTemplateTree.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(assetName))
            {
                MessageBox.Show(this,
                    "Please choose a preset tree/asset first.",
                    "LOD Atlas Helper",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!s_presets.TryGetValue(assetName, out var preset))
            {
                MessageBox.Show(this,
                    "The selected preset could not be found in the internal preset table.",
                    "LOD Atlas Helper",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow? row = dgvMappings.CurrentRow;
            if (row == null || row.IsNewRow)
            {
                if (dgvMappings.Rows.Count == 0)
                {
                    MessageBox.Show(this,
                        "There is no row to apply the template to. Add or load a row in the mapping table first.",
                        "LOD Atlas Helper",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                row = dgvMappings.Rows[0];
            }

            row.Cells[colTextureOrigin.Index].Value = preset.TextureOrigin;
            row.Cells[colPlaneZ.Index].Value = preset.PlaneZ;

            lblStatus.Text = $"Applied preset '{assetName}' (texture_origin={preset.TextureOrigin}, plane_z={preset.PlaneZ}) to the current row.";
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

            this.BackColor = windowBack;
            this.ForeColor = textColor;

            Label[] labels =
            {
                lblAtlas,
                lblAtlasHint,
                lblPropsXml,
                lblGrid,
                lblRows,
                lblCols,
                lblTemplateTree,
                lblSplitMode,
                lblOutputJson,
                lblStatus
            };

            foreach (var lbl in labels)
            {
                if (lbl == null) continue;
                lbl.ForeColor = textColor;
            }

            TextBox[] textBoxes =
            {
                txtAtlasPath,
                txtPropsXml,
                txtOutputJson
            };

            foreach (var tb in textBoxes)
            {
                if (tb == null) continue;
                tb.BackColor = inputBack;
                tb.ForeColor = textColor;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }

            if (cmbTemplateTree != null)
            {
                cmbTemplateTree.BackColor = inputBack;
                cmbTemplateTree.ForeColor = textColor;
                cmbTemplateTree.FlatStyle = FlatStyle.Flat;
            }

            if (cmbSplitMode != null)
            {
                cmbSplitMode.BackColor = inputBack;
                cmbSplitMode.ForeColor = textColor;
                cmbSplitMode.FlatStyle = FlatStyle.Flat;
            }

            if (dgvMappings != null)
            {
                dgvMappings.BackgroundColor = groupBack;
                dgvMappings.GridColor = borderColor;
                dgvMappings.EnableHeadersVisualStyles = false;
                dgvMappings.ColumnHeadersDefaultCellStyle.BackColor = groupBack;
                dgvMappings.ColumnHeadersDefaultCellStyle.ForeColor = textColor;
                dgvMappings.DefaultCellStyle.BackColor = windowBack;
                dgvMappings.DefaultCellStyle.ForeColor = textColor;
                dgvMappings.DefaultCellStyle.SelectionBackColor = accentColor;
                dgvMappings.DefaultCellStyle.SelectionForeColor = Color.White;
            }

            NumericUpDown[] nums = { nudRows, nudCols };
            foreach (var nud in nums)
            {
                if (nud == null) continue;
                nud.BackColor = inputBack;
                nud.ForeColor = textColor;
            }

            Button[] primaryButtons = { btnGenerateJson };
            foreach (var btn in primaryButtons)
            {
                if (btn == null) continue;
                StylePrimaryButton(btn, accentColor, Color.White, borderColor);
            }

            Button[] secondaryButtons =
            {
                btnBrowseAtlas,
                btnPreviewMesh,
                btnBrowsePropsXml,
                btnApplyTemplate,
                btnBrowseOutputJson,
                btnClose
            };

            foreach (var btn in secondaryButtons)
            {
                if (btn == null) continue;
                StyleSecondaryButton(btn, secondaryButton, textColor, borderColor);
            }
        }
        private static void StylePrimaryButton(Button button, Color backColor, Color foreColor, Color borderColor)
        {
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = borderColor;
        }

        private static void StyleSecondaryButton(Button button, Color backColor, Color foreColor, Color borderColor)
        {
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = borderColor;
        }

}

    public class LodCandidateJson
    {
        [JsonPropertyName("texture_origin")]
        public double TextureOrigin { get; set; }

        [JsonPropertyName("plane_z")]
        public double PlaneZ { get; set; }

        [JsonPropertyName("uv_front_min")]
        public double[] UVFrontMin { get; set; } = Array.Empty<double>();

        [JsonPropertyName("uv_front_max")]
        public double[] UVFrontMax { get; set; } = Array.Empty<double>();

        [JsonPropertyName("uv_top_min")]
        public double[] UVTopMin { get; set; } = Array.Empty<double>();

        [JsonPropertyName("uv_top_max")]
        public double[] UVTopMax { get; set; } = Array.Empty<double>();
    }
}