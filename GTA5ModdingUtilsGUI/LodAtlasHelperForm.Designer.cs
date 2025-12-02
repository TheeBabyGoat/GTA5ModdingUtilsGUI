namespace GTA5ModdingUtilsGUI
{
    partial class LodAtlasHelperForm
    {
        private System.ComponentModel.IContainer? components = null;

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
            this.lblAtlas = new System.Windows.Forms.Label();
            this.lblAtlasHint = new System.Windows.Forms.Label();
            this.txtAtlasPath = new System.Windows.Forms.TextBox();
            this.btnBrowseAtlas = new System.Windows.Forms.Button();
            this.btnPreviewMesh = new System.Windows.Forms.Button();
            this.lblPropsXml = new System.Windows.Forms.Label();
            this.txtPropsXml = new System.Windows.Forms.TextBox();
            this.btnBrowsePropsXml = new System.Windows.Forms.Button();
            this.lblGrid = new System.Windows.Forms.Label();
            this.nudRows = new System.Windows.Forms.NumericUpDown();
            this.lblRows = new System.Windows.Forms.Label();
            this.nudCols = new System.Windows.Forms.NumericUpDown();
            this.lblCols = new System.Windows.Forms.Label();
            this.lblTemplateTree = new System.Windows.Forms.Label();
            this.cmbTemplateTree = new System.Windows.Forms.ComboBox();
            this.lblSplitMode = new System.Windows.Forms.Label();
            this.cmbSplitMode = new System.Windows.Forms.ComboBox();
            this.btnApplyTemplate = new System.Windows.Forms.Button();
            this.dgvMappings = new System.Windows.Forms.DataGridView();
            this.colPropName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRow = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTextureOrigin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPlaneZ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblOutputJson = new System.Windows.Forms.Label();
            this.txtOutputJson = new System.Windows.Forms.TextBox();
            this.btnBrowseOutputJson = new System.Windows.Forms.Button();
            this.btnGenerateJson = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudRows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCols)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMappings)).BeginInit();
            this.SuspendLayout();
            // 
            // lblAtlas
            // 
            this.lblAtlas.AutoSize = true;
            this.lblAtlas.Location = new System.Drawing.Point(12, 28);
            this.lblAtlas.Name = "lblAtlas";
            this.lblAtlas.Size = new System.Drawing.Size(90, 15);
            this.lblAtlas.TabIndex = 0;
            this.lblAtlas.Text = "Atlas texture file:";
            // 
            // lblAtlasHint
            // 
            this.lblAtlasHint.AutoSize = true;
            this.lblAtlasHint.Location = new System.Drawing.Point(12, 5);
            this.lblAtlasHint.Name = "lblAtlasHint";
            this.lblAtlasHint.Size = new System.Drawing.Size(760, 15);
            this.lblAtlasHint.TabIndex = 15;
            this.lblAtlasHint.Text = "Atlas PNG/DDS: this is the atlas you will import into vegetation_lod.ytd for your custom assets.";
            // 
            // txtAtlasPath
            // 
            this.txtAtlasPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAtlasPath.Location = new System.Drawing.Point(130, 25);
            this.txtAtlasPath.Name = "txtAtlasPath";
            this.txtAtlasPath.Size = new System.Drawing.Size(460, 23);
            this.txtAtlasPath.TabIndex = 1;
            // 
            // btnBrowseAtlas
            // 
            this.btnBrowseAtlas.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseAtlas.Location = new System.Drawing.Point(700, 24);
            this.btnBrowseAtlas.Name = "btnBrowseAtlas";
            this.btnBrowseAtlas.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseAtlas.TabIndex = 2;
            this.btnBrowseAtlas.Text = "Browse...";
            this.btnBrowseAtlas.UseVisualStyleBackColor = true;
            this.btnBrowseAtlas.Click += new System.EventHandler(this.btnBrowseAtlas_Click);
            // 
            // btnPreviewMesh
            // 
            this.btnPreviewMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPreviewMesh.Location = new System.Drawing.Point(600, 24);
            this.btnPreviewMesh.Name = "btnPreviewMesh";
            this.btnPreviewMesh.Size = new System.Drawing.Size(90, 23);
            this.btnPreviewMesh.TabIndex = 21;
            this.btnPreviewMesh.Text = "3D Preview...";
            this.btnPreviewMesh.UseVisualStyleBackColor = true;
            this.btnPreviewMesh.Click += new System.EventHandler(this.btnPreviewMesh_Click);
            // 
            // lblPropsXml
            // 
            this.lblPropsXml.AutoSize = true;
            this.lblPropsXml.Location = new System.Drawing.Point(12, 55);
            this.lblPropsXml.Name = "lblPropsXml";
            this.lblPropsXml.Size = new System.Drawing.Size(100, 15);
            this.lblPropsXml.TabIndex = 3;
            this.lblPropsXml.Text = "Props / YTYP XML:";
            // 
            // txtPropsXml
            // 
            this.txtPropsXml.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPropsXml.Location = new System.Drawing.Point(130, 52);
            this.txtPropsXml.Name = "txtPropsXml";
            this.txtPropsXml.Size = new System.Drawing.Size(580, 23);
            this.txtPropsXml.TabIndex = 4;
            // 
            // btnBrowsePropsXml
            // 
            this.btnBrowsePropsXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowsePropsXml.Location = new System.Drawing.Point(716, 51);
            this.btnBrowsePropsXml.Name = "btnBrowsePropsXml";
            this.btnBrowsePropsXml.Size = new System.Drawing.Size(75, 23);
            this.btnBrowsePropsXml.TabIndex = 5;
            this.btnBrowsePropsXml.Text = "Browse...";
            this.btnBrowsePropsXml.UseVisualStyleBackColor = true;
            this.btnBrowsePropsXml.Click += new System.EventHandler(this.btnBrowsePropsXml_Click);
            // 
            // lblGrid
            // 
            this.lblGrid.AutoSize = true;
            this.lblGrid.Location = new System.Drawing.Point(12, 90);
            this.lblGrid.Name = "lblGrid";
            this.lblGrid.Size = new System.Drawing.Size(66, 15);
            this.lblGrid.TabIndex = 6;
            this.lblGrid.Text = "Atlas grid:";
            // 
            // nudRows
            // 
            this.nudRows.Location = new System.Drawing.Point(130, 88);
            this.nudRows.Maximum = new decimal(new int[] {64, 0, 0, 0});
            this.nudRows.Minimum = new decimal(new int[] {1, 0, 0, 0});
            this.nudRows.Name = "nudRows";
            this.nudRows.Size = new System.Drawing.Size(60, 23);
            this.nudRows.TabIndex = 7;
            this.nudRows.Value = new decimal(new int[] {2, 0, 0, 0});
            // 
            // lblRows
            // 
            this.lblRows.AutoSize = true;
            this.lblRows.Location = new System.Drawing.Point(196, 90);
            this.lblRows.Name = "lblRows";
            this.lblRows.Size = new System.Drawing.Size(37, 15);
            this.lblRows.TabIndex = 8;
            this.lblRows.Text = "Rows";
            // 
            // nudCols
            // 
            this.nudCols.Location = new System.Drawing.Point(252, 88);
            this.nudCols.Maximum = new decimal(new int[] {64, 0, 0, 0});
            this.nudCols.Minimum = new decimal(new int[] {1, 0, 0, 0});
            this.nudCols.Name = "nudCols";
            this.nudCols.Size = new System.Drawing.Size(60, 23);
            this.nudCols.TabIndex = 9;
            this.nudCols.Value = new decimal(new int[] {1, 0, 0, 0});
            // 
            // lblCols
            // 
            this.lblCols.AutoSize = true;
            this.lblCols.Location = new System.Drawing.Point(318, 90);
            this.lblCols.Name = "lblCols";
            this.lblCols.Size = new System.Drawing.Size(63, 15);
            this.lblCols.TabIndex = 10;
            this.lblCols.Text = "Columns";
            // 
            
            // lblTemplateTree
            // 
            this.lblTemplateTree.AutoSize = true;
            this.lblTemplateTree.Location = new System.Drawing.Point(380, 120);
            this.lblTemplateTree.Name = "lblTemplateTree";
            this.lblTemplateTree.Size = new System.Drawing.Size(103, 15);
            this.lblTemplateTree.TabIndex = 11;
            this.lblTemplateTree.Text = "Preset from asset:";
            // 
            // cmbTemplateTree
            // 
            this.cmbTemplateTree.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTemplateTree.FormattingEnabled = true;
            this.cmbTemplateTree.Location = new System.Drawing.Point(489, 117);
            this.cmbTemplateTree.Name = "cmbTemplateTree";
            this.cmbTemplateTree.Size = new System.Drawing.Size(214, 23);
            this.cmbTemplateTree.TabIndex = 12;
            // 
            // lblSplitMode
            // 
            this.lblSplitMode.AutoSize = true;
            this.lblSplitMode.Location = new System.Drawing.Point(380, 145);
            this.lblSplitMode.Name = "lblSplitMode";
            this.lblSplitMode.Size = new System.Drawing.Size(85, 15);
            this.lblSplitMode.TabIndex = 13;
            this.lblSplitMode.Text = "Tile split mode:";
            // 
            // cmbSplitMode
            // 
            this.cmbSplitMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSplitMode.FormattingEnabled = true;
            this.cmbSplitMode.Items.AddRange(new object[] {
            "50% front / 50% top",
            "75% front / 25% top (vegetation_lod)",
            "100% front / 0% top",
            "25% front / 75% top",
            "0% front / 100% top",
            "100% top / 100% front"});
            this.cmbSplitMode.Location = new System.Drawing.Point(489, 142);
            this.cmbSplitMode.Name = "cmbSplitMode";
            this.cmbSplitMode.Size = new System.Drawing.Size(214, 23);
            this.cmbSplitMode.TabIndex = 14;

            // 
            // btnApplyTemplate
            // 
            this.btnApplyTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApplyTemplate.Location = new System.Drawing.Point(709, 116);
            this.btnApplyTemplate.Name = "btnApplyTemplate";
            this.btnApplyTemplate.Size = new System.Drawing.Size(82, 25);
            this.btnApplyTemplate.TabIndex = 13;
            this.btnApplyTemplate.Text = "Apply preset";
            this.btnApplyTemplate.UseVisualStyleBackColor = true;
            this.btnApplyTemplate.Click += new System.EventHandler(this.btnApplyTemplate_Click);
            // 
// dgvMappings
            // 
            this.dgvMappings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvMappings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMappings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colPropName,
                this.colRow,
                this.colCol,
                this.colTextureOrigin,
                this.colPlaneZ});
            this.dgvMappings.Location = new System.Drawing.Point(12, 190);
            this.dgvMappings.Name = "dgvMappings";
            this.dgvMappings.RowTemplate.Height = 25;
            this.dgvMappings.Size = new System.Drawing.Size(779, 277);
            this.dgvMappings.TabIndex = 14;
            // 
            // colPropName
            // 
            this.colPropName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colPropName.HeaderText = "Prop name";
            this.colPropName.Name = "colPropName";
            // 
            // colRow
            // 
            this.colRow.HeaderText = "Row (0-based)";
            this.colRow.Name = "colRow";
            this.colRow.Width = 90;
            // 
            // colCol
            // 
            this.colCol.HeaderText = "Column (0-based)";
            this.colCol.Name = "colCol";
            this.colCol.Width = 110;
            // 
            // colTextureOrigin
            // 
            this.colTextureOrigin.HeaderText = "Texture origin (0-1)";
            this.colTextureOrigin.Name = "colTextureOrigin";
            this.colTextureOrigin.Width = 130;
            // 
            // colPlaneZ
            // 
            this.colPlaneZ.HeaderText = "Plane Z (0-1)";
            this.colPlaneZ.Name = "colPlaneZ";
            this.colPlaneZ.Width = 110;
            // 
            // lblOutputJson
            // 
            this.lblOutputJson.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblOutputJson.AutoSize = true;
            this.lblOutputJson.Location = new System.Drawing.Point(12, 486);
            this.lblOutputJson.Name = "lblOutputJson";
            this.lblOutputJson.Size = new System.Drawing.Size(96, 15);
            this.lblOutputJson.TabIndex = 15;
            this.lblOutputJson.Text = "Output JSON file:";
            // 
            // txtOutputJson
            // 
            this.txtOutputJson.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputJson.Location = new System.Drawing.Point(130, 483);
            this.txtOutputJson.Name = "txtOutputJson";
            this.txtOutputJson.Size = new System.Drawing.Size(492, 23);
            this.txtOutputJson.TabIndex = 16;
            // 
            // btnBrowseOutputJson
            // 
            this.btnBrowseOutputJson.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseOutputJson.Location = new System.Drawing.Point(628, 482);
            this.btnBrowseOutputJson.Name = "btnBrowseOutputJson";
            this.btnBrowseOutputJson.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseOutputJson.TabIndex = 17;
            this.btnBrowseOutputJson.Text = "Browse...";
            this.btnBrowseOutputJson.UseVisualStyleBackColor = true;
            this.btnBrowseOutputJson.Click += new System.EventHandler(this.btnBrowseOutputJson_Click);
            // 
            // btnGenerateJson
            // 
            this.btnGenerateJson.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerateJson.Location = new System.Drawing.Point(709, 482);
            this.btnGenerateJson.Name = "btnGenerateJson";
            this.btnGenerateJson.Size = new System.Drawing.Size(82, 23);
            this.btnGenerateJson.TabIndex = 18;
            this.btnGenerateJson.Text = "Generate";
            this.btnGenerateJson.UseVisualStyleBackColor = true;
            this.btnGenerateJson.Click += new System.EventHandler(this.btnGenerateJson_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 515);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(698, 23);
            this.lblStatus.TabIndex = 19;
            this.lblStatus.Text = "Status";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(716, 515);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 20;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // LodAtlasHelperForm
            // 
            this.AcceptButton = this.btnGenerateJson;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnGenerateJson);
            this.Controls.Add(this.btnPreviewMesh);
            this.Controls.Add(this.btnBrowseOutputJson);
            this.Controls.Add(this.txtOutputJson);
            this.Controls.Add(this.lblOutputJson);
            this.Controls.Add(this.btnApplyTemplate);
            this.Controls.Add(this.cmbSplitMode);
            this.Controls.Add(this.lblSplitMode);
            this.Controls.Add(this.cmbTemplateTree);
            this.Controls.Add(this.lblTemplateTree);
            this.Controls.Add(this.dgvMappings);
            this.Controls.Add(this.lblCols);
            this.Controls.Add(this.nudCols);
            this.Controls.Add(this.lblRows);
            this.Controls.Add(this.nudRows);
            this.Controls.Add(this.lblGrid);
            this.Controls.Add(this.btnBrowsePropsXml);
            this.Controls.Add(this.txtPropsXml);
            this.Controls.Add(this.lblPropsXml);
            this.Controls.Add(this.btnBrowseAtlas);
            this.Controls.Add(this.txtAtlasPath);
            this.Controls.Add(this.lblAtlasHint);
            this.Controls.Add(this.lblAtlas);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "LodAtlasHelperForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "LOD / SLOD Atlas Helper";
            ((System.ComponentModel.ISupportInitialize)(this.nudRows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCols)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMappings)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblAtlas;
        private System.Windows.Forms.Label lblAtlasHint;
        private System.Windows.Forms.TextBox txtAtlasPath;
        private System.Windows.Forms.Button btnBrowseAtlas;
        private System.Windows.Forms.Label lblPropsXml;
        private System.Windows.Forms.TextBox txtPropsXml;
        private System.Windows.Forms.Button btnBrowsePropsXml;
        private System.Windows.Forms.Label lblGrid;
        private System.Windows.Forms.NumericUpDown nudRows;
        private System.Windows.Forms.Label lblRows;
        private System.Windows.Forms.NumericUpDown nudCols;
        private System.Windows.Forms.Label lblCols;
        private System.Windows.Forms.Label lblTemplateTree;
        private System.Windows.Forms.ComboBox cmbTemplateTree;
        private System.Windows.Forms.Label lblSplitMode;
        private System.Windows.Forms.ComboBox cmbSplitMode;
        private System.Windows.Forms.Button btnApplyTemplate;
        private System.Windows.Forms.DataGridView dgvMappings;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPropName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRow;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTextureOrigin;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPlaneZ;
        private System.Windows.Forms.Label lblOutputJson;
        private System.Windows.Forms.TextBox txtOutputJson;
        private System.Windows.Forms.Button btnBrowseOutputJson;
        private System.Windows.Forms.Button btnGenerateJson;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnPreviewMesh;
    }
}
