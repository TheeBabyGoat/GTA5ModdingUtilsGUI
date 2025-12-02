using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    public partial class ReadmeForm : Form
    {
        private readonly string _content;

        // Runtime-only controls used to turn the single readme box
        // into a tabbed help window (Readme | Commands).
        private TabControl? _tabHelp;
        private TabPage? _tabReadme;
        private TabPage? _tabCommands;
        private RichTextBox? _txtCommands;

        public ReadmeForm(string content)
        {
            _content = content;
            InitializeComponent();
            InitializeTabs();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ApplyDarkTheme();
        }

        /// <summary>
        /// Re-parents the designer-created txtReadme into a TabControl and
        /// adds a second tab that documents all available commands.
        /// </summary>
        private void InitializeTabs()
        {
            if (txtReadme == null)
            {
                // Designer did not create the control for some reason;
                // in that case we simply fall back to the original layout.
                return;
            }

            // Remove the original readme box from the form and host it in a tab.
            this.Controls.Remove(txtReadme);
            txtReadme.ReadOnly = true;
            txtReadme.BorderStyle = BorderStyle.None;
            txtReadme.Dock = DockStyle.Fill;

            _tabHelp = new TabControl
            {
                Dock = DockStyle.Fill
            };

            _tabReadme = new TabPage("Readme");
            _tabReadme.Controls.Add(txtReadme);

            _tabCommands = new TabPage("Commands");

            _txtCommands = new RichTextBox
            {
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                BackColor = txtReadme.BackColor,
                Font = txtReadme.Font,
                HideSelection = false
            };
            _txtCommands.Text = BuildCommandsHelpText();
            _txtCommands.SelectionStart = 0;
            _txtCommands.SelectionLength = 0;

            _tabCommands.Controls.Add(_txtCommands);

            _tabHelp.TabPages.Add(_tabReadme);
            _tabHelp.TabPages.Add(_tabCommands);

            this.Controls.Add(_tabHelp);
            _tabHelp.BringToFront();
        }

        /// <summary>
        /// Returns the text that is shown on the "Commands" tab of the help window.
        /// The descriptions are based on the flags passed to main.py in the
        /// underlying gta5-modding-utils project.
        /// </summary>
        private static string BuildCommandsHelpText()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Commands overview");
            sb.AppendLine("========================================");
            sb.AppendLine();
            sb.AppendLine("Each checkbox in the main window toggles a command-line flag");
            sb.AppendLine("for the underlying Python tool (main.py from gta5-modding-utils).");
            sb.AppendLine("The GUI itself never modifies files directly – it just forwards");
            sb.AppendLine("your choices as arguments.");
            sb.AppendLine();

            sb.AppendLine("General options");
            sb.AppendLine("---------------");
            sb.AppendLine("Prefix ( --prefix=<PREFIX> )");
            sb.AppendLine("    Common name prefix that is used for all generated output");
            sb.AppendLine("    directories and files.");
            sb.AppendLine();

            sb.AppendLine("Vegetation ( --vegetationCreator=on )");
            sb.AppendLine("    Runs the VegetationCreator worker.");
            sb.AppendLine("    Creates new vegetation .ymap.xml files based on your");
            sb.AppendLine("    input maps and YTYP data and writes them to a generated");
            sb.AppendLine("    subdirectory.");
            sb.AppendLine();

            sb.AppendLine("Entropy ( --entropy=on )");
            sb.AppendLine("    Runs EntropyCreator.");
            sb.AppendLine("    Builds entropy-based LOD/SLOD vegetation maps so distant");
            sb.AppendLine("    areas can be rendered with fewer but better-chosen props.");
            sb.AppendLine();

            sb.AppendLine("Reducer ( --reducer=on )");
            sb.AppendLine("    Runs the Reducer worker.");
            sb.AppendLine("    Groups and merges dense vegetation to reduce the overall");
            sb.AppendLine("    number of entities while keeping visual density similar.");
            sb.AppendLine();

            sb.AppendLine("Reducer → Resolution ( --reducerResolution=<float> )");
            sb.AppendLine("    Target resolution used by the reducer. Smaller values");
            sb.AppendLine("    keep more detail, larger values merge more aggressively.");
            sb.AppendLine("    Only used when Reducer is enabled.");
            sb.AppendLine();

            sb.AppendLine("Reducer → Adapt scaling ( --reducerAdaptScaling=on )");
            sb.AppendLine("    Automatically adapts the scale of merged entities so that");
            sb.AppendLine("    the resulting vegetation keeps a natural look.");
            sb.AppendLine();

            sb.AppendLine("Clustering ( --clustering=on )");
            sb.AppendLine("    Runs the Clustering worker.");
            sb.AppendLine("    Reads entities from all input maps and splits them into");
            sb.AppendLine("    new .ymap.xml files, each containing one cluster.");
            sb.AppendLine();

            sb.AppendLine("Clustering → Prefix ( --clusteringPrefix=<CLUSTERING_PREFIX> )");
            sb.AppendLine("    Name prefix for the generated clustered ymap files.");
            sb.AppendLine();

            sb.AppendLine("Clustering → Excluded ymaps ( --clusteringExcluded=<list> )");
            sb.AppendLine("    Comma-separated list of ymap names that are not used when");
            sb.AppendLine("    building clusters.");
            sb.AppendLine();

            sb.AppendLine("Clustering → Num. clusters ( --numClusters=<integer> )");
            sb.AppendLine("    Optional fixed number of clusters. If omitted the tool");
            sb.AppendLine("    chooses a value that keeps map extents reasonable.");
            sb.AppendLine();

            sb.AppendLine("Static col ( --staticCol=on )");
            sb.AppendLine("    Runs StaticCollisionCreator.");
            sb.AppendLine("    Creates static collision models and updated maps that");
            sb.AppendLine("    reference them. The collision models are written into");
            sb.AppendLine("    a <PREFIX>_col directory.");
            sb.AppendLine();

            sb.AppendLine("LOD map ( --lodMap=on )");
            sb.AppendLine("    Runs the LodMapCreator worker.");
            sb.AppendLine("    Generates LOD and SLOD maps and the corresponding meshes");
            sb.AppendLine("    for your vegetation so distant areas can be rendered");
            sb.AppendLine("    efficiently.");
            sb.AppendLine();

            sb.AppendLine("Clear LOD ( --clearLod=on )");
            sb.AppendLine("    Removes existing LOD/SLOD data from the input maps before");
            sb.AppendLine("    creating new ones. Useful when rebuilding LOD data from");
            sb.AppendLine("    scratch.");
            sb.AppendLine();

            sb.AppendLine("Reflection ( --reflection=on )");
            sb.AppendLine("    When LOD map creation is enabled this switch also builds");
            sb.AppendLine("    additional reflection meshes and places them into a");
            sb.AppendLine("    <PREFIX>_refl output directory.");
            sb.AppendLine();

            sb.AppendLine("Sanitizer ( --sanitizer=on )");
            sb.AppendLine("    Runs the Sanitizer worker.");
            sb.AppendLine("    Fixes common problems in your input .ymap.xml files such");
            sb.AppendLine("    as inconsistent archetype names, flags and rotations.");
            sb.AppendLine();

            sb.AppendLine("Statistics ( --statistics=on )");
            sb.AppendLine("    Runs the StatisticsPrinter worker.");
            sb.AppendLine("    Prints statistics about your maps (for example counts of");
            sb.AppendLine("    archetypes per YTYP and LOD level) into the console.");
            sb.AppendLine();

            sb.AppendLine("Notes");
            sb.AppendLine("-----");
            sb.AppendLine("* The GUI combines the selected options into a single");
            sb.AppendLine("  main.py call. The order of the steps follows the original");
            sb.AppendLine("  tool chain from gta5-modding-utils.");
            sb.AppendLine("* Some options depend on others. For example, reflection");
            sb.AppendLine("  requires LOD map generation to be enabled.");

            return sb.ToString();
        }

        private void ReadmeForm_Load(object? sender, EventArgs e)
        {
            if (txtReadme != null)
            {
                txtReadme.Text = _content ?? string.Empty;
                txtReadme.SelectionStart = 0;
                txtReadme.SelectionLength = 0;
            }
        }


        private void ApplyDarkTheme()
        {
            Color windowBack = Color.FromArgb(6, 29, 36);
            Color pageBack = Color.FromArgb(13, 43, 51);
            Color textColor = Color.Gainsboro;
            Color accentColor = Color.FromArgb(0, 168, 135);

            this.BackColor = windowBack;
            this.ForeColor = textColor;

            if (_tabHelp != null)
            {
                _tabHelp.BackColor = windowBack;
            }

            if (_tabReadme != null)
            {
                _tabReadme.BackColor = pageBack;
            }

            if (_tabCommands != null)
            {
                _tabCommands.BackColor = pageBack;
            }

            if (txtReadme != null)
            {
                txtReadme.BackColor = pageBack;
                txtReadme.ForeColor = textColor;
            }

            if (_txtCommands != null)
            {
                _txtCommands.BackColor = pageBack;
                _txtCommands.ForeColor = textColor;
            }

            // Style tab headers by setting the default colors on the control.
            if (_tabHelp != null)
            {
                _tabHelp.DrawMode = TabDrawMode.OwnerDrawFixed;
                _tabHelp.DrawItem += (s, e) =>
                {
                    var tab = _tabHelp.TabPages[e.Index];
                    var bounds = e.Bounds;
                    bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

                    using var backBrush = new SolidBrush(isSelected ? pageBack : windowBack);
                    using var textBrush = new SolidBrush(isSelected ? accentColor : textColor);

                    e.Graphics.FillRectangle(backBrush, bounds);
                    var text = tab.Text;
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString(text, this.Font, textBrush, bounds, format);
                };
            }
        }

    }
}
