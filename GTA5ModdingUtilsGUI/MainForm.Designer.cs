namespace GTA5ModdingUtilsGUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            customAssetsToolStripMenuItem = new ToolStripMenuItem();
            customMeshesToolStripMenuItem = new ToolStripMenuItem();
            textureCreationToolStripMenuItem = new ToolStripMenuItem();
            preview3DToolStripMenuItem = new ToolStripMenuItem();
            creditsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            tutorialsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripSeparator1 = new ToolStripSeparator();
            viewReadmeToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            lblPythonPath = new Label();
            txtPythonPath = new TextBox();
            btnBrowsePython = new Button();
            lblInputDir = new Label();
            txtInputDir = new TextBox();
            btnBrowseInputDir = new Button();
            lblOutputDir = new Label();
            txtOutputDir = new TextBox();
            btnBrowseOutputDir = new Button();
            btnOpenOutputDir = new Button();
            lblPrefix = new Label();
            txtPrefix = new TextBox();
            grpFeatures = new GroupBox();
            chkStatistics = new CheckBox();
            chkSanitizer = new CheckBox();
            chkReflection = new CheckBox();
            chkClearLod = new CheckBox();
            chkLodMap = new CheckBox();
            chkCustomMeshes = new CheckBox();
            chkStaticCol = new CheckBox();
            chkClustering = new CheckBox();
            chkReducer = new CheckBox();
            chkEntropy = new CheckBox();
            chkVegetation = new CheckBox();
            grpAdvanced = new GroupBox();
            txtPolygon = new TextBox();
            lblPolygon = new Label();
            txtClusteringExcluded = new TextBox();
            lblClusteringExcluded = new Label();
            txtClusteringPrefix = new TextBox();
            lblClusteringPrefix = new Label();
            nudNumClusters = new NumericUpDown();
            lblNumClusters = new Label();
            chkReducerAdaptScaling = new CheckBox();
            nudReducerResolution = new NumericUpDown();
            lblReducerResolution = new Label();
            grpLodMultipliers = new GroupBox();
            nudLodMultiplierPalms = new NumericUpDown();
            lblLodPalms = new Label();
            nudLodMultiplierBushes = new NumericUpDown();
            lblLodBushes = new Label();
            nudLodMultiplierTrees = new NumericUpDown();
            lblLodTrees = new Label();
            nudLodMultiplierCacti = new NumericUpDown();
            lblLodCacti = new Label();
            chkEnableLodMultipliers = new CheckBox();
            btnRun = new Button();
            btnCancel = new Button();
            txtLog = new TextBox();
            chkUseOriginalNames = new CheckBox();
            menuStrip1.SuspendLayout();
            grpFeatures.SuspendLayout();
            grpAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudNumClusters).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudReducerResolution).BeginInit();
            grpLodMultipliers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudLodMultiplierPalms).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLodMultiplierBushes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLodMultiplierTrees).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLodMultiplierCacti).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { customAssetsToolStripMenuItem, preview3DToolStripMenuItem, creditsToolStripMenuItem, helpToolStripMenuItem, settingsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1024, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // customAssetsToolStripMenuItem
            // 
            customAssetsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { customMeshesToolStripMenuItem, textureCreationToolStripMenuItem });
            customAssetsToolStripMenuItem.Name = "customAssetsToolStripMenuItem";
            customAssetsToolStripMenuItem.Size = new Size(97, 20);
            customAssetsToolStripMenuItem.Text = "Custom Assets";
            // 
            // customMeshesToolStripMenuItem
            // 
            customMeshesToolStripMenuItem.Name = "customMeshesToolStripMenuItem";
            customMeshesToolStripMenuItem.Size = new Size(169, 22);
            customMeshesToolStripMenuItem.Text = "Custom Meshes...";
            customMeshesToolStripMenuItem.Click += btnCustomMeshes_Click;
            // 
            // textureCreationToolStripMenuItem
            // 
            textureCreationToolStripMenuItem.Name = "textureCreationToolStripMenuItem";
            textureCreationToolStripMenuItem.Size = new Size(169, 22);
            textureCreationToolStripMenuItem.Text = "Texture Creation...";
            textureCreationToolStripMenuItem.Click += textureCreationToolStripMenuItem_Click;
            // 
            // preview3DToolStripMenuItem
            // 
            preview3DToolStripMenuItem.Name = "preview3DToolStripMenuItem";
            preview3DToolStripMenuItem.Size = new Size(77, 20);
            preview3DToolStripMenuItem.Text = "3D Preview";
            preview3DToolStripMenuItem.Click += preview3DToolStripMenuItem_Click;
            // 
            // creditsToolStripMenuItem
            // 
            creditsToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
            creditsToolStripMenuItem.Name = "creditsToolStripMenuItem";
            creditsToolStripMenuItem.Size = new Size(56, 20);
            creditsToolStripMenuItem.Text = "Credits";
            creditsToolStripMenuItem.Click += creditsToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tutorialsToolStripMenuItem, helpToolStripSeparator1, viewReadmeToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // tutorialsToolStripMenuItem
            // 
            tutorialsToolStripMenuItem.Name = "tutorialsToolStripMenuItem";
            tutorialsToolStripMenuItem.Size = new Size(145, 22);
            tutorialsToolStripMenuItem.Text = "Tutorials...";
            tutorialsToolStripMenuItem.Click += tutorialsToolStripMenuItem_Click;
            // 
            // helpToolStripSeparator1
            // 
            helpToolStripSeparator1.Name = "helpToolStripSeparator1";
            helpToolStripSeparator1.Size = new Size(142, 6);
            // 
            // viewReadmeToolStripMenuItem
            // 
            viewReadmeToolStripMenuItem.Name = "viewReadmeToolStripMenuItem";
            viewReadmeToolStripMenuItem.Size = new Size(145, 22);
            viewReadmeToolStripMenuItem.Text = "View Readme";
            viewReadmeToolStripMenuItem.Click += viewReadmeToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(61, 20);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // lblPythonPath
            // 
            lblPythonPath.AutoSize = true;
            lblPythonPath.Location = new Point(12, 38);
            lblPythonPath.Name = "lblPythonPath";
            lblPythonPath.Size = new Size(116, 15);
            lblPythonPath.TabIndex = 0;
            lblPythonPath.Text = "Gta5-Modding-Utils:";
            // 
            // txtPythonPath
            // 
            txtPythonPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPythonPath.Location = new Point(192, 35);
            txtPythonPath.Name = "txtPythonPath";
            txtPythonPath.Size = new Size(424, 23);
            txtPythonPath.TabIndex = 1;
            // 
            // btnBrowsePython
            // 
            btnBrowsePython.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowsePython.Location = new Point(622, 34);
            btnBrowsePython.Name = "btnBrowsePython";
            btnBrowsePython.Size = new Size(75, 23);
            btnBrowsePython.TabIndex = 2;
            btnBrowsePython.Text = "Browse...";
            btnBrowsePython.UseVisualStyleBackColor = true;
            btnBrowsePython.Click += btnBrowsePython_Click;
            // 
            // lblInputDir
            // 
            lblInputDir.AutoSize = true;
            lblInputDir.Location = new Point(12, 67);
            lblInputDir.Name = "lblInputDir";
            lblInputDir.Size = new Size(72, 15);
            lblInputDir.TabIndex = 3;
            lblInputDir.Text = "Input folder:";
            // 
            // txtInputDir
            // 
            txtInputDir.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtInputDir.Location = new Point(192, 64);
            txtInputDir.Name = "txtInputDir";
            txtInputDir.Size = new Size(424, 23);
            txtInputDir.TabIndex = 4;
            // 
            // btnBrowseInputDir
            // 
            btnBrowseInputDir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseInputDir.Location = new Point(622, 63);
            btnBrowseInputDir.Name = "btnBrowseInputDir";
            btnBrowseInputDir.Size = new Size(75, 23);
            btnBrowseInputDir.TabIndex = 5;
            btnBrowseInputDir.Text = "Browse...";
            btnBrowseInputDir.UseVisualStyleBackColor = true;
            btnBrowseInputDir.Click += btnBrowseInputDir_Click;
            // 
            // lblOutputDir
            // 
            lblOutputDir.AutoSize = true;
            lblOutputDir.Location = new Point(9, 96);
            lblOutputDir.Name = "lblOutputDir";
            lblOutputDir.Size = new Size(82, 15);
            lblOutputDir.TabIndex = 6;
            lblOutputDir.Text = "Output folder:";
            // 
            // txtOutputDir
            // 
            txtOutputDir.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOutputDir.Location = new Point(192, 93);
            txtOutputDir.Name = "txtOutputDir";
            txtOutputDir.Size = new Size(424, 23);
            txtOutputDir.TabIndex = 7;
            // 
            // btnBrowseOutputDir
            // 
            btnBrowseOutputDir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseOutputDir.Location = new Point(622, 92);
            btnBrowseOutputDir.Name = "btnBrowseOutputDir";
            btnBrowseOutputDir.Size = new Size(75, 23);
            btnBrowseOutputDir.TabIndex = 8;
            btnBrowseOutputDir.Text = "Browse...";
            btnBrowseOutputDir.UseVisualStyleBackColor = true;
            btnBrowseOutputDir.Click += btnBrowseOutputDir_Click;
            // 
            // btnOpenOutputDir
            // 
            btnOpenOutputDir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOpenOutputDir.Location = new Point(703, 92);
            btnOpenOutputDir.Name = "btnOpenOutputDir";
            btnOpenOutputDir.Size = new Size(75, 23);
            btnOpenOutputDir.TabIndex = 9;
            btnOpenOutputDir.Text = "Open";
            btnOpenOutputDir.UseVisualStyleBackColor = true;
            btnOpenOutputDir.Click += btnOpenOutputDir_Click;
            // 
            // lblPrefix
            // 
            lblPrefix.AutoSize = true;
            lblPrefix.Location = new Point(12, 127);
            lblPrefix.Name = "lblPrefix";
            lblPrefix.Size = new Size(79, 15);
            lblPrefix.TabIndex = 9;
            lblPrefix.Text = "Project prefix:";
            // 
            // txtPrefix
            // 
            txtPrefix.Location = new Point(192, 122);
            txtPrefix.Name = "txtPrefix";
            txtPrefix.Size = new Size(200, 23);
            txtPrefix.TabIndex = 10;
            // 
            // grpFeatures
            // 
            grpFeatures.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpFeatures.Controls.Add(chkStatistics);
            grpFeatures.Controls.Add(chkSanitizer);
            grpFeatures.Controls.Add(chkReflection);
            grpFeatures.Controls.Add(chkClearLod);
            grpFeatures.Controls.Add(chkLodMap);
            grpFeatures.Controls.Add(chkCustomMeshes);
            grpFeatures.Controls.Add(chkStaticCol);
            grpFeatures.Controls.Add(chkClustering);
            grpFeatures.Controls.Add(chkReducer);
            grpFeatures.Controls.Add(chkEntropy);
            grpFeatures.Controls.Add(chkVegetation);
            grpFeatures.Location = new Point(12, 151);
            grpFeatures.Name = "grpFeatures";
            grpFeatures.Size = new Size(685, 78);
            grpFeatures.TabIndex = 11;
            grpFeatures.TabStop = false;
            grpFeatures.Text = "Steps to run";
            // 
            // chkStatistics
            // 
            chkStatistics.AutoSize = true;
            chkStatistics.Location = new Point(499, 47);
            chkStatistics.Name = "chkStatistics";
            chkStatistics.Size = new Size(72, 19);
            chkStatistics.TabIndex = 9;
            chkStatistics.Text = "Statistics";
            chkStatistics.UseVisualStyleBackColor = true;
            // 
            // chkSanitizer
            // 
            chkSanitizer.AutoSize = true;
            chkSanitizer.Location = new Point(423, 47);
            chkSanitizer.Name = "chkSanitizer";
            chkSanitizer.Size = new Size(70, 19);
            chkSanitizer.TabIndex = 8;
            chkSanitizer.Text = "Sanitizer";
            chkSanitizer.UseVisualStyleBackColor = true;
            chkSanitizer.CheckedChanged += chkSanitizer_CheckedChanged;
            // 
            // chkReflection
            // 
            chkReflection.AutoSize = true;
            chkReflection.Location = new Point(338, 47);
            chkReflection.Name = "chkReflection";
            chkReflection.Size = new Size(79, 19);
            chkReflection.TabIndex = 7;
            chkReflection.Text = "Reflection";
            chkReflection.UseVisualStyleBackColor = true;
            // 
            // chkClearLod
            // 
            chkClearLod.AutoSize = true;
            chkClearLod.Location = new Point(253, 47);
            chkClearLod.Name = "chkClearLod";
            chkClearLod.Size = new Size(79, 19);
            chkClearLod.TabIndex = 6;
            chkClearLod.Text = "Clear LOD";
            chkClearLod.UseVisualStyleBackColor = true;
            // 
            // chkLodMap
            // 
            chkLodMap.AutoSize = true;
            chkLodMap.Location = new Point(171, 47);
            chkLodMap.Name = "chkLodMap";
            chkLodMap.Size = new Size(76, 19);
            chkLodMap.TabIndex = 5;
            chkLodMap.Text = "LOD map";
            chkLodMap.UseVisualStyleBackColor = true;
            // 
            // chkCustomMeshes
            // 
            chkCustomMeshes.AutoSize = true;
            chkCustomMeshes.Location = new Point(449, 22);
            chkCustomMeshes.Name = "chkCustomMeshes";
            chkCustomMeshes.Size = new Size(111, 19);
            chkCustomMeshes.TabIndex = 10;
            chkCustomMeshes.Text = "Custom Lods";
            chkCustomMeshes.UseVisualStyleBackColor = true;
            chkCustomMeshes.CheckedChanged += chkCustomMeshes_CheckedChanged;
            // 
            // chkStaticCol
            // 
            chkStaticCol.AutoSize = true;
            chkStaticCol.Location = new Point(91, 47);
            chkStaticCol.Name = "chkStaticCol";
            chkStaticCol.Size = new Size(74, 19);
            chkStaticCol.TabIndex = 4;
            chkStaticCol.Text = "Static col";
            chkStaticCol.UseVisualStyleBackColor = true;
            // 
            // chkClustering
            // 
            chkClustering.AutoSize = true;
            chkClustering.Location = new Point(353, 22);
            chkClustering.Name = "chkClustering";
            chkClustering.Size = new Size(80, 19);
            chkClustering.TabIndex = 3;
            chkClustering.Text = "Clustering";
            chkClustering.UseVisualStyleBackColor = true;
            // 
            // chkReducer
            // 
            chkReducer.AutoSize = true;
            chkReducer.Location = new Point(278, 22);
            chkReducer.Name = "chkReducer";
            chkReducer.Size = new Size(69, 19);
            chkReducer.TabIndex = 2;
            chkReducer.Text = "Reducer";
            chkReducer.UseVisualStyleBackColor = true;
            // 
            // chkEntropy
            // 
            chkEntropy.AutoSize = true;
            chkEntropy.Location = new Point(205, 22);
            chkEntropy.Name = "chkEntropy";
            chkEntropy.Size = new Size(67, 19);
            chkEntropy.TabIndex = 1;
            chkEntropy.Text = "Entropy";
            chkEntropy.UseVisualStyleBackColor = true;
            // 
            // chkVegetation
            // 
            chkVegetation.AutoSize = true;
            chkVegetation.Location = new Point(117, 22);
            chkVegetation.Name = "chkVegetation";
            chkVegetation.Size = new Size(82, 19);
            chkVegetation.TabIndex = 0;
            chkVegetation.Text = "Vegetation";
            chkVegetation.UseVisualStyleBackColor = true;
            // 
            // grpAdvanced
            // 
            grpAdvanced.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpAdvanced.Controls.Add(txtPolygon);
            grpAdvanced.Controls.Add(lblPolygon);
            grpAdvanced.Controls.Add(txtClusteringExcluded);
            grpAdvanced.Controls.Add(lblClusteringExcluded);
            grpAdvanced.Controls.Add(txtClusteringPrefix);
            grpAdvanced.Controls.Add(lblClusteringPrefix);
            grpAdvanced.Controls.Add(nudNumClusters);
            grpAdvanced.Controls.Add(lblNumClusters);
            grpAdvanced.Controls.Add(chkReducerAdaptScaling);
            grpAdvanced.Controls.Add(nudReducerResolution);
            grpAdvanced.Controls.Add(lblReducerResolution);
            grpAdvanced.Location = new Point(12, 235);
            grpAdvanced.Name = "grpAdvanced";
            grpAdvanced.Size = new Size(685, 152);
            grpAdvanced.TabIndex = 12;
            grpAdvanced.TabStop = false;
            grpAdvanced.Text = "Advanced options";
            // 
            // txtPolygon
            // 
            txtPolygon.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPolygon.Location = new Point(140, 104);
            txtPolygon.Multiline = true;
            txtPolygon.Name = "txtPolygon";
            txtPolygon.ScrollBars = ScrollBars.Vertical;
            txtPolygon.Size = new Size(408, 38);
            txtPolygon.TabIndex = 10;
            // 
            // lblPolygon
            // 
            lblPolygon.AutoSize = true;
            lblPolygon.Location = new Point(14, 107);
            lblPolygon.Name = "lblPolygon";
            lblPolygon.Size = new Size(111, 15);
            lblPolygon.TabIndex = 9;
            lblPolygon.Text = "Polygon (JSON list):";
            // 
            // txtClusteringExcluded
            // 
            txtClusteringExcluded.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtClusteringExcluded.Location = new Point(486, 63);
            txtClusteringExcluded.Name = "txtClusteringExcluded";
            txtClusteringExcluded.Size = new Size(187, 23);
            txtClusteringExcluded.TabIndex = 8;
            // 
            // lblClusteringExcluded
            // 
            lblClusteringExcluded.AutoSize = true;
            lblClusteringExcluded.Location = new Point(343, 66);
            lblClusteringExcluded.Name = "lblClusteringExcluded";
            lblClusteringExcluded.Size = new Size(141, 15);
            lblClusteringExcluded.TabIndex = 7;
            lblClusteringExcluded.Text = "Excluded maps (comma):";
            // 
            // txtClusteringPrefix
            // 
            txtClusteringPrefix.Location = new Point(140, 63);
            txtClusteringPrefix.Name = "txtClusteringPrefix";
            txtClusteringPrefix.Size = new Size(187, 23);
            txtClusteringPrefix.TabIndex = 6;
            // 
            // lblClusteringPrefix
            // 
            lblClusteringPrefix.AutoSize = true;
            lblClusteringPrefix.Location = new Point(14, 66);
            lblClusteringPrefix.Name = "lblClusteringPrefix";
            lblClusteringPrefix.Size = new Size(96, 15);
            lblClusteringPrefix.TabIndex = 5;
            lblClusteringPrefix.Text = "Clustering prefix:";
            // 
            // nudNumClusters
            // 
            nudNumClusters.Location = new Point(486, 24);
            nudNumClusters.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudNumClusters.Name = "nudNumClusters";
            nudNumClusters.Size = new Size(80, 23);
            nudNumClusters.TabIndex = 4;
            // 
            // lblNumClusters
            // 
            lblNumClusters.AutoSize = true;
            lblNumClusters.Location = new Point(343, 26);
            lblNumClusters.Name = "lblNumClusters";
            lblNumClusters.Size = new Size(111, 15);
            lblNumClusters.TabIndex = 3;
            lblNumClusters.Text = "Number of clusters:";
            // 
            // chkReducerAdaptScaling
            // 
            chkReducerAdaptScaling.AutoSize = true;
            chkReducerAdaptScaling.Location = new Point(226, 25);
            chkReducerAdaptScaling.Name = "chkReducerAdaptScaling";
            chkReducerAdaptScaling.Size = new Size(98, 19);
            chkReducerAdaptScaling.TabIndex = 2;
            chkReducerAdaptScaling.Text = "Adapt scaling";
            chkReducerAdaptScaling.UseVisualStyleBackColor = true;
            // 
            // nudReducerResolution
            // 
            nudReducerResolution.DecimalPlaces = 1;
            nudReducerResolution.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            nudReducerResolution.Location = new Point(140, 24);
            nudReducerResolution.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudReducerResolution.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudReducerResolution.Name = "nudReducerResolution";
            nudReducerResolution.Size = new Size(72, 23);
            nudReducerResolution.TabIndex = 1;
            nudReducerResolution.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblReducerResolution
            // 
            lblReducerResolution.AutoSize = true;
            lblReducerResolution.Location = new Point(14, 26);
            lblReducerResolution.Name = "lblReducerResolution";
            lblReducerResolution.Size = new Size(109, 15);
            lblReducerResolution.TabIndex = 0;
            lblReducerResolution.Text = "Reducer resolution:";
            // 
            // grpLodMultipliers
            // 
            grpLodMultipliers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpLodMultipliers.Controls.Add(nudLodMultiplierPalms);
            grpLodMultipliers.Controls.Add(lblLodPalms);
            grpLodMultipliers.Controls.Add(nudLodMultiplierBushes);
            grpLodMultipliers.Controls.Add(lblLodBushes);
            grpLodMultipliers.Controls.Add(nudLodMultiplierTrees);
            grpLodMultipliers.Controls.Add(lblLodTrees);
            grpLodMultipliers.Controls.Add(nudLodMultiplierCacti);
            grpLodMultipliers.Controls.Add(lblLodCacti);
            grpLodMultipliers.Controls.Add(chkEnableLodMultipliers);
            grpLodMultipliers.Location = new Point(707, 163);
            grpLodMultipliers.Name = "grpLodMultipliers";
            grpLodMultipliers.Size = new Size(305, 224);
            grpLodMultipliers.TabIndex = 20;
            grpLodMultipliers.TabStop = false;
            grpLodMultipliers.Text = "LOD Distance Overrides";
            // 
            // nudLodMultiplierPalms
            // 
            nudLodMultiplierPalms.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            nudLodMultiplierPalms.Location = new Point(140, 136);
            nudLodMultiplierPalms.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudLodMultiplierPalms.Name = "nudLodMultiplierPalms";
            nudLodMultiplierPalms.Size = new Size(80, 23);
            nudLodMultiplierPalms.TabIndex = 8;
            // 
            // lblLodPalms
            // 
            lblLodPalms.AutoSize = true;
            lblLodPalms.Location = new Point(14, 138);
            lblLodPalms.Name = "lblLodPalms";
            lblLodPalms.Size = new Size(42, 15);
            lblLodPalms.TabIndex = 7;
            lblLodPalms.Text = "Palms:";
            // 
            // nudLodMultiplierBushes
            // 
            nudLodMultiplierBushes.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            nudLodMultiplierBushes.Location = new Point(140, 106);
            nudLodMultiplierBushes.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudLodMultiplierBushes.Name = "nudLodMultiplierBushes";
            nudLodMultiplierBushes.Size = new Size(80, 23);
            nudLodMultiplierBushes.TabIndex = 6;
            // 
            // lblLodBushes
            // 
            lblLodBushes.AutoSize = true;
            lblLodBushes.Location = new Point(14, 108);
            lblLodBushes.Name = "lblLodBushes";
            lblLodBushes.Size = new Size(47, 15);
            lblLodBushes.TabIndex = 5;
            lblLodBushes.Text = "Bushes:";
            // 
            // nudLodMultiplierTrees
            // 
            nudLodMultiplierTrees.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            nudLodMultiplierTrees.Location = new Point(140, 76);
            nudLodMultiplierTrees.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudLodMultiplierTrees.Name = "nudLodMultiplierTrees";
            nudLodMultiplierTrees.Size = new Size(80, 23);
            nudLodMultiplierTrees.TabIndex = 4;
            // 
            // lblLodTrees
            // 
            lblLodTrees.AutoSize = true;
            lblLodTrees.Location = new Point(14, 78);
            lblLodTrees.Name = "lblLodTrees";
            lblLodTrees.Size = new Size(37, 15);
            lblLodTrees.TabIndex = 3;
            lblLodTrees.Text = "Trees:";
            // 
            // nudLodMultiplierCacti
            // 
            nudLodMultiplierCacti.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            nudLodMultiplierCacti.Location = new Point(140, 46);
            nudLodMultiplierCacti.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudLodMultiplierCacti.Name = "nudLodMultiplierCacti";
            nudLodMultiplierCacti.Size = new Size(80, 23);
            nudLodMultiplierCacti.TabIndex = 2;
            // 
            // lblLodCacti
            // 
            lblLodCacti.AutoSize = true;
            lblLodCacti.Location = new Point(14, 48);
            lblLodCacti.Name = "lblLodCacti";
            lblLodCacti.Size = new Size(37, 15);
            lblLodCacti.TabIndex = 1;
            lblLodCacti.Text = "Cacti:";
            // 
            // chkEnableLodMultipliers
            // 
            chkEnableLodMultipliers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkEnableLodMultipliers.AutoSize = true;
            chkEnableLodMultipliers.Location = new Point(220, 22);
            chkEnableLodMultipliers.Name = "chkEnableLodMultipliers";
            chkEnableLodMultipliers.Size = new Size(61, 19);
            chkEnableLodMultipliers.TabIndex = 0;
            chkEnableLodMultipliers.Text = "Enable";
            chkEnableLodMultipliers.UseVisualStyleBackColor = true;
            // 
            // btnRun
            // 
            btnRun.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRun.Location = new Point(541, 393);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(75, 27);
            btnRun.TabIndex = 13;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Enabled = false;
            btnCancel.Location = new Point(622, 393);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 27);
            btnCancel.TabIndex = 14;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Location = new Point(12, 426);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(1000, 260);
            txtLog.TabIndex = 15;
            // 
            // chkUseOriginalNames
            // 
            chkUseOriginalNames.AutoSize = true;
            chkUseOriginalNames.Location = new Point(406, 126);
            chkUseOriginalNames.Name = "chkUseOriginalNames";
            chkUseOriginalNames.Size = new Size(210, 19);
            chkUseOriginalNames.TabIndex = 11;
            chkUseOriginalNames.Text = "Use original map names (no prefix)";
            chkUseOriginalNames.UseVisualStyleBackColor = true;
            chkUseOriginalNames.CheckedChanged += chkUseOriginalNames_CheckedChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1024, 720);
            Controls.Add(txtLog);
            Controls.Add(btnCancel);
            Controls.Add(btnRun);
            Controls.Add(grpLodMultipliers);
            Controls.Add(grpAdvanced);
            Controls.Add(grpFeatures);
            Controls.Add(txtPrefix);
            Controls.Add(chkUseOriginalNames);
            Controls.Add(lblPrefix);
            Controls.Add(btnBrowseOutputDir);
            Controls.Add(btnOpenOutputDir);
            Controls.Add(txtOutputDir);
            Controls.Add(lblOutputDir);
            Controls.Add(btnBrowseInputDir);
            Controls.Add(txtInputDir);
            Controls.Add(lblInputDir);
            Controls.Add(btnBrowsePython);
            Controls.Add(txtPythonPath);
            Controls.Add(lblPythonPath);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(1024, 720);
            Name = "MainForm";
            Text = "GTA5 Modding Utils GUI";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            grpFeatures.ResumeLayout(false);
            grpFeatures.PerformLayout();
            grpAdvanced.ResumeLayout(false);
            grpAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudNumClusters).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudReducerResolution).EndInit();
            grpLodMultipliers.ResumeLayout(false);
            grpLodMultipliers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudLodMultiplierPalms).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLodMultiplierBushes).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLodMultiplierTrees).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLodMultiplierCacti).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPythonPath;
        private System.Windows.Forms.TextBox txtPythonPath;
        private System.Windows.Forms.Button btnBrowsePython;
        private System.Windows.Forms.Label lblInputDir;
        private System.Windows.Forms.TextBox txtInputDir;
        private System.Windows.Forms.Button btnBrowseInputDir;
        private System.Windows.Forms.Label lblOutputDir;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.Button btnBrowseOutputDir;
        private System.Windows.Forms.Button btnOpenOutputDir;
        private System.Windows.Forms.Label lblPrefix;
        private System.Windows.Forms.TextBox txtPrefix;
        private System.Windows.Forms.GroupBox grpFeatures;
        private System.Windows.Forms.CheckBox chkStatistics;
        private System.Windows.Forms.CheckBox chkSanitizer;
        private System.Windows.Forms.CheckBox chkReflection;
        private System.Windows.Forms.CheckBox chkClearLod;
        private System.Windows.Forms.CheckBox chkLodMap;
        private System.Windows.Forms.CheckBox chkStaticCol;
        private System.Windows.Forms.CheckBox chkClustering;
        private System.Windows.Forms.CheckBox chkReducer;
        private System.Windows.Forms.CheckBox chkEntropy;
        private System.Windows.Forms.CheckBox chkVegetation;
        private System.Windows.Forms.CheckBox chkCustomMeshes;
        private System.Windows.Forms.GroupBox grpAdvanced;
        private System.Windows.Forms.TextBox txtPolygon;        private System.Windows.Forms.Label lblPolygon;
        private System.Windows.Forms.TextBox txtClusteringExcluded;
        private System.Windows.Forms.Label lblClusteringExcluded;
        private System.Windows.Forms.TextBox txtClusteringPrefix;
        private System.Windows.Forms.Label lblClusteringPrefix;
        private System.Windows.Forms.NumericUpDown nudNumClusters;
        private System.Windows.Forms.Label lblNumClusters;
        private System.Windows.Forms.CheckBox chkReducerAdaptScaling;
        private System.Windows.Forms.NumericUpDown nudReducerResolution;
        private System.Windows.Forms.Label lblReducerResolution;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtLog;
        /// <summary>
        /// Checkbox for toggling use of original map names.
        /// </summary>
        private System.Windows.Forms.CheckBox chkUseOriginalNames;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem customAssetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem preview3DToolStripMenuItem;        private System.Windows.Forms.ToolStripMenuItem customMeshesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textureCreationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tutorialsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator helpToolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem viewReadmeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem creditsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.GroupBox grpLodMultipliers;
        private System.Windows.Forms.CheckBox chkEnableLodMultipliers;
        private System.Windows.Forms.Label lblLodCacti;
        private System.Windows.Forms.Label lblLodTrees;
        private System.Windows.Forms.Label lblLodBushes;
        private System.Windows.Forms.Label lblLodPalms;
        private System.Windows.Forms.NumericUpDown nudLodMultiplierCacti;
        private System.Windows.Forms.NumericUpDown nudLodMultiplierTrees;
        private System.Windows.Forms.NumericUpDown nudLodMultiplierBushes;
        private System.Windows.Forms.NumericUpDown nudLodMultiplierPalms;
    }
}
