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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewReadmeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.creditsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblPythonPath = new System.Windows.Forms.Label();
            this.txtPythonPath = new System.Windows.Forms.TextBox();
            this.btnBrowsePython = new System.Windows.Forms.Button();
            this.lblInputDir = new System.Windows.Forms.Label();
            this.txtInputDir = new System.Windows.Forms.TextBox();
            this.btnBrowseInputDir = new System.Windows.Forms.Button();
            this.lblOutputDir = new System.Windows.Forms.Label();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.btnBrowseOutputDir = new System.Windows.Forms.Button();
            this.lblPrefix = new System.Windows.Forms.Label();
            this.txtPrefix = new System.Windows.Forms.TextBox();
            this.grpFeatures = new System.Windows.Forms.GroupBox();
            this.chkStatistics = new System.Windows.Forms.CheckBox();
            this.chkSanitizer = new System.Windows.Forms.CheckBox();
            this.chkReflection = new System.Windows.Forms.CheckBox();
            this.chkClearLod = new System.Windows.Forms.CheckBox();
            this.chkLodMap = new System.Windows.Forms.CheckBox();
            this.chkStaticCol = new System.Windows.Forms.CheckBox();
            this.chkClustering = new System.Windows.Forms.CheckBox();
            this.chkReducer = new System.Windows.Forms.CheckBox();
            this.chkEntropy = new System.Windows.Forms.CheckBox();
            this.chkVegetation = new System.Windows.Forms.CheckBox();
            this.grpAdvanced = new System.Windows.Forms.GroupBox();
            this.txtPolygon = new System.Windows.Forms.TextBox();
            this.lblPolygon = new System.Windows.Forms.Label();
            this.txtClusteringExcluded = new System.Windows.Forms.TextBox();
            this.lblClusteringExcluded = new System.Windows.Forms.Label();
            this.txtClusteringPrefix = new System.Windows.Forms.TextBox();
            this.lblClusteringPrefix = new System.Windows.Forms.Label();
            this.nudNumClusters = new System.Windows.Forms.NumericUpDown();
            this.lblNumClusters = new System.Windows.Forms.Label();
            this.chkReducerAdaptScaling = new System.Windows.Forms.CheckBox();
            this.nudReducerResolution = new System.Windows.Forms.NumericUpDown();
            this.lblReducerResolution = new System.Windows.Forms.Label();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.grpFeatures.SuspendLayout();
            this.grpAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumClusters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudReducerResolution)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem,
            this.creditsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(709, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Top;
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewReadmeToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            // 
            // viewReadmeToolStripMenuItem
            // 
            this.viewReadmeToolStripMenuItem.Name = "viewReadmeToolStripMenuItem";
            this.viewReadmeToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.viewReadmeToolStripMenuItem.Text = "View Readme";
            this.viewReadmeToolStripMenuItem.Click += new System.EventHandler(this.viewReadmeToolStripMenuItem_Click);
            // 
            // creditsToolStripMenuItem
            // 
            this.creditsToolStripMenuItem.Name = "creditsToolStripMenuItem";
            this.creditsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.creditsToolStripMenuItem.Text = "Credits";
            this.creditsToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.creditsToolStripMenuItem.Click += new System.EventHandler(this.creditsToolStripMenuItem_Click);
            // 
            // lblPythonPath
            // 
            this.lblPythonPath.AutoSize = true;
            this.lblPythonPath.Location = new System.Drawing.Point(12, 15);
            this.lblPythonPath.Name = "lblPythonPath";
            this.lblPythonPath.Size = new System.Drawing.Size(174, 15);
            this.lblPythonPath.TabIndex = 0;
            this.lblPythonPath.Text = "Gta5-Modding-Utils:";
            // 
            // txtPythonPath
            // 
            this.txtPythonPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                                                          | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPythonPath.Location = new System.Drawing.Point(192, 12);
            this.txtPythonPath.Name = "txtPythonPath";
            this.txtPythonPath.Size = new System.Drawing.Size(424, 23);
            this.txtPythonPath.TabIndex = 1;
            // 
            // btnBrowsePython
            // 
            this.btnBrowsePython.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowsePython.Location = new System.Drawing.Point(622, 11);
            this.btnBrowsePython.Name = "btnBrowsePython";
            this.btnBrowsePython.Size = new System.Drawing.Size(75, 23);
            this.btnBrowsePython.TabIndex = 2;
            this.btnBrowsePython.Text = "Browse...";
            this.btnBrowsePython.UseVisualStyleBackColor = true;
            this.btnBrowsePython.Click += new System.EventHandler(this.btnBrowsePython_Click);
            // 
            // lblInputDir
            // 
            this.lblInputDir.AutoSize = true;
            this.lblInputDir.Location = new System.Drawing.Point(12, 49);
            this.lblInputDir.Name = "lblInputDir";
            this.lblInputDir.Size = new System.Drawing.Size(76, 15);
            this.lblInputDir.TabIndex = 3;
            this.lblInputDir.Text = "Input folder:";
            // 
            // txtInputDir
            // 
            this.txtInputDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                                                         | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInputDir.Location = new System.Drawing.Point(192, 46);
            this.txtInputDir.Name = "txtInputDir";
            this.txtInputDir.Size = new System.Drawing.Size(424, 23);
            this.txtInputDir.TabIndex = 4;
            // 
            // btnBrowseInputDir
            // 
            this.btnBrowseInputDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseInputDir.Location = new System.Drawing.Point(622, 45);
            this.btnBrowseInputDir.Name = "btnBrowseInputDir";
            this.btnBrowseInputDir.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseInputDir.TabIndex = 5;
            this.btnBrowseInputDir.Text = "Browse...";
            this.btnBrowseInputDir.UseVisualStyleBackColor = true;
            this.btnBrowseInputDir.Click += new System.EventHandler(this.btnBrowseInputDir_Click);
            // 
            // lblOutputDir
            // 
            this.lblOutputDir.AutoSize = true;
            this.lblOutputDir.Location = new System.Drawing.Point(12, 83);
            this.lblOutputDir.Name = "lblOutputDir";
            this.lblOutputDir.Size = new System.Drawing.Size(85, 15);
            this.lblOutputDir.TabIndex = 6;
            this.lblOutputDir.Text = "Output folder:";
            // 
            // txtOutputDir
            // 
            this.txtOutputDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                                                          | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputDir.Location = new System.Drawing.Point(192, 80);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(424, 23);
            this.txtOutputDir.TabIndex = 7;
            // 
            // btnBrowseOutputDir
            // 
            this.btnBrowseOutputDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseOutputDir.Location = new System.Drawing.Point(622, 79);
            this.btnBrowseOutputDir.Name = "btnBrowseOutputDir";
            this.btnBrowseOutputDir.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseOutputDir.TabIndex = 8;
            this.btnBrowseOutputDir.Text = "Browse...";
            this.btnBrowseOutputDir.UseVisualStyleBackColor = true;
            this.btnBrowseOutputDir.Click += new System.EventHandler(this.btnBrowseOutputDir_Click);
            // 
            // lblPrefix
            // 
            this.lblPrefix.AutoSize = true;
            this.lblPrefix.Location = new System.Drawing.Point(12, 117);
            this.lblPrefix.Name = "lblPrefix";
            this.lblPrefix.Size = new System.Drawing.Size(96, 15);
            this.lblPrefix.TabIndex = 9;
            this.lblPrefix.Text = "Project prefix:";
            // 
            // txtPrefix
            // 
            this.txtPrefix.Location = new System.Drawing.Point(192, 114);
            this.txtPrefix.Name = "txtPrefix";
            this.txtPrefix.Size = new System.Drawing.Size(200, 23);
            this.txtPrefix.TabIndex = 10;
            // 
            // grpFeatures
            // 
            this.grpFeatures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                                                          | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFeatures.Controls.Add(this.chkStatistics);
            this.grpFeatures.Controls.Add(this.chkSanitizer);
            this.grpFeatures.Controls.Add(this.chkReflection);
            this.grpFeatures.Controls.Add(this.chkClearLod);
            this.grpFeatures.Controls.Add(this.chkLodMap);
            this.grpFeatures.Controls.Add(this.chkStaticCol);
            this.grpFeatures.Controls.Add(this.chkClustering);
            this.grpFeatures.Controls.Add(this.chkReducer);
            this.grpFeatures.Controls.Add(this.chkEntropy);
            this.grpFeatures.Controls.Add(this.chkVegetation);
            this.grpFeatures.Location = new System.Drawing.Point(12, 151);
            this.grpFeatures.Name = "grpFeatures";
            this.grpFeatures.Size = new System.Drawing.Size(685, 78);
            this.grpFeatures.TabIndex = 11;
            this.grpFeatures.TabStop = false;
            this.grpFeatures.Text = "Steps to run";
            // 
            // chkStatistics
            // 
            this.chkStatistics.AutoSize = true;
            this.chkStatistics.Location = new System.Drawing.Point(545, 47);
            this.chkStatistics.Name = "chkStatistics";
            this.chkStatistics.Size = new System.Drawing.Size(77, 19);
            this.chkStatistics.TabIndex = 9;
            this.chkStatistics.Text = "Statistics";
            this.chkStatistics.UseVisualStyleBackColor = true;
            // 
            // chkSanitizer
            // 
            this.chkSanitizer.AutoSize = true;
            this.chkSanitizer.Location = new System.Drawing.Point(449, 47);
            this.chkSanitizer.Name = "chkSanitizer";
            this.chkSanitizer.Size = new System.Drawing.Size(74, 19);
            this.chkSanitizer.TabIndex = 8;
            this.chkSanitizer.Text = "Sanitizer";
            this.chkSanitizer.UseVisualStyleBackColor = true;
            // 
            // chkReflection
            // 
            this.chkReflection.AutoSize = true;
            this.chkReflection.Location = new System.Drawing.Point(353, 47);
            this.chkReflection.Name = "chkReflection";
            this.chkReflection.Size = new System.Drawing.Size(83, 19);
            this.chkReflection.TabIndex = 7;
            this.chkReflection.Text = "Reflection";
            this.chkReflection.UseVisualStyleBackColor = true;
            // 
            // chkClearLod
            // 
            this.chkClearLod.AutoSize = true;
            this.chkClearLod.Location = new System.Drawing.Point(257, 47);
            this.chkClearLod.Name = "chkClearLod";
            this.chkClearLod.Size = new System.Drawing.Size(78, 19);
            this.chkClearLod.TabIndex = 6;
            this.chkClearLod.Text = "Clear LOD";
            this.chkClearLod.UseVisualStyleBackColor = true;
            // 
            // chkLodMap
            // 
            this.chkLodMap.AutoSize = true;
            this.chkLodMap.Location = new System.Drawing.Point(171, 47);
            this.chkLodMap.Name = "chkLodMap";
            this.chkLodMap.Size = new System.Drawing.Size(74, 19);
            this.chkLodMap.TabIndex = 5;
            this.chkLodMap.Text = "LOD map";
            this.chkLodMap.UseVisualStyleBackColor = true;
            // 
            // chkStaticCol
            // 
            this.chkStaticCol.AutoSize = true;
            this.chkStaticCol.Location = new System.Drawing.Point(85, 47);
            this.chkStaticCol.Name = "chkStaticCol";
            this.chkStaticCol.Size = new System.Drawing.Size(75, 19);
            this.chkStaticCol.TabIndex = 4;
            this.chkStaticCol.Text = "Static col";
            this.chkStaticCol.UseVisualStyleBackColor = true;
            // 
            // chkClustering
            // 
            this.chkClustering.AutoSize = true;
            this.chkClustering.Location = new System.Drawing.Point(449, 22);
            this.chkClustering.Name = "chkClustering";
            this.chkClustering.Size = new System.Drawing.Size(83, 19);
            this.chkClustering.TabIndex = 3;
            this.chkClustering.Text = "Clustering";
            this.chkClustering.UseVisualStyleBackColor = true;
            // 
            // chkReducer
            // 
            this.chkReducer.AutoSize = true;
            this.chkReducer.Location = new System.Drawing.Point(353, 22);
            this.chkReducer.Name = "chkReducer";
            this.chkReducer.Size = new System.Drawing.Size(71, 19);
            this.chkReducer.TabIndex = 2;
            this.chkReducer.Text = "Reducer";
            this.chkReducer.UseVisualStyleBackColor = true;
            // 
            // chkEntropy
            // 
            this.chkEntropy.AutoSize = true;
            this.chkEntropy.Location = new System.Drawing.Point(257, 22);
            this.chkEntropy.Name = "chkEntropy";
            this.chkEntropy.Size = new System.Drawing.Size(69, 19);
            this.chkEntropy.TabIndex = 1;
            this.chkEntropy.Text = "Entropy";
            this.chkEntropy.UseVisualStyleBackColor = true;
            // 
            // chkVegetation
            // 
            this.chkVegetation.AutoSize = true;
            this.chkVegetation.Location = new System.Drawing.Point(17, 22);
            this.chkVegetation.Name = "chkVegetation";
            this.chkVegetation.Size = new System.Drawing.Size(91, 19);
            this.chkVegetation.TabIndex = 0;
            this.chkVegetation.Text = "Vegetation";
            this.chkVegetation.UseVisualStyleBackColor = true;
            // 
            // grpAdvanced
            // 
            this.grpAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                                                          | System.Windows.Forms.AnchorStyles.Right)));
            this.grpAdvanced.Controls.Add(this.txtPolygon);
            this.grpAdvanced.Controls.Add(this.lblPolygon);
            this.grpAdvanced.Controls.Add(this.txtClusteringExcluded);
            this.grpAdvanced.Controls.Add(this.lblClusteringExcluded);
            this.grpAdvanced.Controls.Add(this.txtClusteringPrefix);
            this.grpAdvanced.Controls.Add(this.lblClusteringPrefix);
            this.grpAdvanced.Controls.Add(this.nudNumClusters);
            this.grpAdvanced.Controls.Add(this.lblNumClusters);
            this.grpAdvanced.Controls.Add(this.chkReducerAdaptScaling);
            this.grpAdvanced.Controls.Add(this.nudReducerResolution);
            this.grpAdvanced.Controls.Add(this.lblReducerResolution);
            this.grpAdvanced.Location = new System.Drawing.Point(12, 235);
            this.grpAdvanced.Name = "grpAdvanced";
            this.grpAdvanced.Size = new System.Drawing.Size(685, 152);
            this.grpAdvanced.TabIndex = 12;
            this.grpAdvanced.TabStop = false;
            this.grpAdvanced.Text = "Advanced options";
            // 
            // txtPolygon
            // 
            this.txtPolygon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                                                         | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPolygon.Location = new System.Drawing.Point(140, 104);
            this.txtPolygon.Multiline = true;
            this.txtPolygon.Name = "txtPolygon";
            this.txtPolygon.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPolygon.Size = new System.Drawing.Size(533, 38);
            this.txtPolygon.TabIndex = 10;
            // 
            // lblPolygon
            // 
            this.lblPolygon.AutoSize = true;
            this.lblPolygon.Location = new System.Drawing.Point(14, 107);
            this.lblPolygon.Name = "lblPolygon";
            this.lblPolygon.Size = new System.Drawing.Size(114, 15);
            this.lblPolygon.TabIndex = 9;
            this.lblPolygon.Text = "Polygon (JSON list):";
            // 
            // txtClusteringExcluded
            // 
            this.txtClusteringExcluded.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                                                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.txtClusteringExcluded.Location = new System.Drawing.Point(486, 63);
            this.txtClusteringExcluded.Name = "txtClusteringExcluded";
            this.txtClusteringExcluded.Size = new System.Drawing.Size(187, 23);
            this.txtClusteringExcluded.TabIndex = 8;
            // 
            // lblClusteringExcluded
            // 
            this.lblClusteringExcluded.AutoSize = true;
            this.lblClusteringExcluded.Location = new System.Drawing.Point(343, 66);
            this.lblClusteringExcluded.Name = "lblClusteringExcluded";
            this.lblClusteringExcluded.Size = new System.Drawing.Size(137, 15);
            this.lblClusteringExcluded.TabIndex = 7;
            this.lblClusteringExcluded.Text = "Excluded maps (comma):";
            // 
            // txtClusteringPrefix
            // 
            this.txtClusteringPrefix.Location = new System.Drawing.Point(140, 63);
            this.txtClusteringPrefix.Name = "txtClusteringPrefix";
            this.txtClusteringPrefix.Size = new System.Drawing.Size(187, 23);
            this.txtClusteringPrefix.TabIndex = 6;
            // 
            // lblClusteringPrefix
            // 
            this.lblClusteringPrefix.AutoSize = true;
            this.lblClusteringPrefix.Location = new System.Drawing.Point(14, 66);
            this.lblClusteringPrefix.Name = "lblClusteringPrefix";
            this.lblClusteringPrefix.Size = new System.Drawing.Size(102, 15);
            this.lblClusteringPrefix.TabIndex = 5;
            this.lblClusteringPrefix.Text = "Clustering prefix:";
            // 
            // nudNumClusters
            // 
            this.nudNumClusters.Location = new System.Drawing.Point(486, 24);
            this.nudNumClusters.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudNumClusters.Name = "nudNumClusters";
            this.nudNumClusters.Size = new System.Drawing.Size(80, 23);
            this.nudNumClusters.TabIndex = 4;
            // 
            // lblNumClusters
            // 
            this.lblNumClusters.AutoSize = true;
            this.lblNumClusters.Location = new System.Drawing.Point(343, 26);
            this.lblNumClusters.Name = "lblNumClusters";
            this.lblNumClusters.Size = new System.Drawing.Size(123, 15);
            this.lblNumClusters.TabIndex = 3;
            this.lblNumClusters.Text = "Number of clusters:";
            // 
            // chkReducerAdaptScaling
            // 
            this.chkReducerAdaptScaling.AutoSize = true;
            this.chkReducerAdaptScaling.Location = new System.Drawing.Point(226, 25);
            this.chkReducerAdaptScaling.Name = "chkReducerAdaptScaling";
            this.chkReducerAdaptScaling.Size = new System.Drawing.Size(100, 19);
            this.chkReducerAdaptScaling.TabIndex = 2;
            this.chkReducerAdaptScaling.Text = "Adapt scaling";
            this.chkReducerAdaptScaling.UseVisualStyleBackColor = true;
            // 
            // nudReducerResolution
            // 
            this.nudReducerResolution.DecimalPlaces = 1;
            this.nudReducerResolution.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nudReducerResolution.Location = new System.Drawing.Point(140, 24);
            this.nudReducerResolution.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudReducerResolution.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudReducerResolution.Name = "nudReducerResolution";
            this.nudReducerResolution.Size = new System.Drawing.Size(72, 23);
            this.nudReducerResolution.TabIndex = 1;
            this.nudReducerResolution.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // lblReducerResolution
            // 
            this.lblReducerResolution.AutoSize = true;
            this.lblReducerResolution.Location = new System.Drawing.Point(14, 26);
            this.lblReducerResolution.Name = "lblReducerResolution";
            this.lblReducerResolution.Size = new System.Drawing.Size(113, 15);
            this.lblReducerResolution.TabIndex = 0;
            this.lblReducerResolution.Text = "Reducer resolution:";
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.Location = new System.Drawing.Point(541, 393);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 27);
            this.btnRun.TabIndex = 13;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(622, 393);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 27);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                                                       | System.Windows.Forms.AnchorStyles.Left) 
                                                                      | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(12, 426);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(685, 163);
            this.txtLog.TabIndex = 15;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(709, 601);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.grpAdvanced);
            this.Controls.Add(this.grpFeatures);
            this.Controls.Add(this.txtPrefix);
            this.Controls.Add(this.lblPrefix);
            this.Controls.Add(this.btnBrowseOutputDir);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.lblOutputDir);
            this.Controls.Add(this.btnBrowseInputDir);
            this.Controls.Add(this.txtInputDir);
            this.Controls.Add(this.lblInputDir);
            this.Controls.Add(this.btnBrowsePython);
            this.Controls.Add(this.txtPythonPath);
            this.Controls.Add(this.lblPythonPath);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(725, 640);
            this.Name = "MainForm";
            this.Text = "GTA5 Modding Utils GUI";
            this.grpFeatures.ResumeLayout(false);
            this.grpFeatures.PerformLayout();
            this.grpAdvanced.ResumeLayout(false);
            this.grpAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumClusters)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudReducerResolution)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.GroupBox grpAdvanced;
        private System.Windows.Forms.TextBox txtPolygon;
        private System.Windows.Forms.Label lblPolygon;
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
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewReadmeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem creditsToolStripMenuItem;
    }
}
