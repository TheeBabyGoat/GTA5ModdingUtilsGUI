namespace GTA5ModdingUtilsGUI
{
    partial class CustomSlodsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support – do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblInfo = new Label();
            lblConfigPath = new Label();
            txtConfigPath = new TextBox();
            btnBrowseConfig = new Button();
            lblMeshes = new Label();
            btnClearList = new Button();
            btnAddFromResources = new Button();
            txtMeshes = new RichTextBox();
            btnSave = new Button();
            btnClose = new Button();
            lblStatus = new Label();
            grpObjOverride = new GroupBox();
            chkPreviewSourceOdr = new CheckBox();
            lblSourceOdr = new Label();
            txtSourceOdrPath = new TextBox();
            btnBrowseSourceOdr = new Button();
            btnConvertOdrToObj = new Button();
            btnConvertObjToOdr = new Button();
            btnOpenIn3DPreview = new Button();
            btnOpenOverridesFolder = new Button();
            btnClearObjOverride = new Button();
            btnImportObjOverride = new Button();
            txtOverrideObjPath = new TextBox();
            lblOverrideObj = new Label();
            txtSelectedArchetype = new TextBox();
            lblSelectedArchetype = new Label();
            grpObjOverride.SuspendLayout();
            SuspendLayout();
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.ForeColor = Color.Gray;
            lblInfo.Location = new Point(12, 9);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(467, 30);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Define which archetypes are 'Custom SLODs'.\r\nThese are typically vegetation or props for which you want to manually manage SLODs.";
            // 
            // lblConfigPath
            // 
            lblConfigPath.AutoSize = true;
            lblConfigPath.Location = new Point(12, 53);
            lblConfigPath.Name = "lblConfigPath";
            lblConfigPath.Size = new Size(104, 15);
            lblConfigPath.TabIndex = 1;
            lblConfigPath.Text = "Config file (JSON):";
            // 
            // txtConfigPath
            // 
            txtConfigPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtConfigPath.BackColor = Color.WhiteSmoke;
            txtConfigPath.Location = new Point(122, 50);
            txtConfigPath.Name = "txtConfigPath";
            txtConfigPath.ReadOnly = true;
            txtConfigPath.Size = new Size(449, 23);
            txtConfigPath.TabIndex = 2;
            // 
            // btnBrowseConfig
            // 
            btnBrowseConfig.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseConfig.Location = new Point(576, 48);
            btnBrowseConfig.Name = "btnBrowseConfig";
            btnBrowseConfig.Size = new Size(76, 25);
            btnBrowseConfig.TabIndex = 3;
            btnBrowseConfig.Text = "Browse...";
            btnBrowseConfig.UseVisualStyleBackColor = true;
            btnBrowseConfig.Click += btnBrowseConfig_Click;
            // 
            // lblMeshes
            // 
            lblMeshes.AutoSize = true;
            lblMeshes.Location = new Point(12, 85);
            lblMeshes.Name = "lblMeshes";
            lblMeshes.Size = new Size(146, 15);
            lblMeshes.TabIndex = 4;
            lblMeshes.Text = "Custom SLOD Archetypes:";
            // 
            // btnClearList
            // 
            btnClearList.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearList.Location = new Point(335, 81);
            btnClearList.Name = "btnClearList";
            btnClearList.Size = new Size(75, 23);
            btnClearList.TabIndex = 6;
            btnClearList.Text = "Clear List";
            btnClearList.UseVisualStyleBackColor = true;
            btnClearList.Click += btnClearList_Click;
            // 
            // btnAddFromResources
            // 
            btnAddFromResources.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddFromResources.Location = new Point(416, 81);
            btnAddFromResources.Name = "btnAddFromResources";
            btnAddFromResources.Size = new Size(155, 23);
            btnAddFromResources.TabIndex = 5;
            btnAddFromResources.Text = "Add from Resources";
            btnAddFromResources.UseVisualStyleBackColor = true;
            btnAddFromResources.Click += btnAddFromResources_Click;
            // 
            // txtMeshes
            // 
            txtMeshes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtMeshes.BorderStyle = BorderStyle.FixedSingle;
            txtMeshes.Location = new Point(15, 110);
            txtMeshes.Name = "txtMeshes";
            txtMeshes.Size = new Size(637, 257);
            txtMeshes.TabIndex = 7;
            txtMeshes.Text = "";
            txtMeshes.Click += txtMeshes_Click;
            txtMeshes.TextChanged += txtMeshes_TextChanged;
            txtMeshes.KeyDown += txtMeshes_KeyDown;
            txtMeshes.KeyUp += txtMeshes_KeyUp;
            txtMeshes.MouseDown += txtMeshes_MouseDown;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(471, 567);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 30);
            btnSave.TabIndex = 10;
            btnSave.Text = "Save JSONs";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(576, 567);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(76, 30);
            btnClose.TabIndex = 11;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(15, 582);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(39, 15);
            lblStatus.TabIndex = 9;
            lblStatus.Text = "Status";
            // 
            // grpObjOverride
            // 
            grpObjOverride.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpObjOverride.Controls.Add(chkPreviewSourceOdr);
            grpObjOverride.Controls.Add(lblSourceOdr);
            grpObjOverride.Controls.Add(txtSourceOdrPath);
            grpObjOverride.Controls.Add(btnBrowseSourceOdr);
            grpObjOverride.Controls.Add(btnConvertOdrToObj);
            grpObjOverride.Controls.Add(btnConvertObjToOdr);
            grpObjOverride.Controls.Add(btnOpenIn3DPreview);
            grpObjOverride.Controls.Add(btnOpenOverridesFolder);
            grpObjOverride.Controls.Add(btnClearObjOverride);
            grpObjOverride.Controls.Add(btnImportObjOverride);
            grpObjOverride.Controls.Add(txtOverrideObjPath);
            grpObjOverride.Controls.Add(lblOverrideObj);
            grpObjOverride.Controls.Add(txtSelectedArchetype);
            grpObjOverride.Controls.Add(lblSelectedArchetype);
            grpObjOverride.Location = new Point(12, 373);
            grpObjOverride.Name = "grpObjOverride";
            grpObjOverride.Size = new Size(640, 190);
            grpObjOverride.TabIndex = 8;
            grpObjOverride.TabStop = false;
            grpObjOverride.Text = "OBJ Override (Selected line)";
            // 
            // chkPreviewSourceOdr
            // 
            chkPreviewSourceOdr.AutoSize = true;
            chkPreviewSourceOdr.Location = new Point(70, 110);
            chkPreviewSourceOdr.Name = "chkPreviewSourceOdr";
            chkPreviewSourceOdr.Size = new Size(132, 19);
            chkPreviewSourceOdr.TabIndex = 13;
            chkPreviewSourceOdr.Text = "Preview Selected ODR";
            chkPreviewSourceOdr.UseVisualStyleBackColor = true;
            // 
            // lblSourceOdr
            // 
            lblSourceOdr.AutoSize = true;
            lblSourceOdr.Location = new Point(16, 83);
            lblSourceOdr.Name = "lblSourceOdr";
            lblSourceOdr.Size = new Size(33, 15);
            lblSourceOdr.TabIndex = 12;
            lblSourceOdr.Text = "ODR";
            // 
            // txtSourceOdrPath
            // 
            txtSourceOdrPath.BackColor = Color.WhiteSmoke;
            txtSourceOdrPath.Location = new Point(70, 80);
            txtSourceOdrPath.Name = "txtSourceOdrPath";
            txtSourceOdrPath.ReadOnly = true;
            txtSourceOdrPath.Size = new Size(328, 23);
            txtSourceOdrPath.TabIndex = 11;
            // 
            // btnBrowseSourceOdr
            // 
            btnBrowseSourceOdr.Location = new Point(416, 80);
            btnBrowseSourceOdr.Name = "btnBrowseSourceOdr";
            btnBrowseSourceOdr.Size = new Size(209, 27);
            btnBrowseSourceOdr.TabIndex = 10;
            btnBrowseSourceOdr.Text = "Select ODR File...";
            btnBrowseSourceOdr.UseVisualStyleBackColor = true;
            btnBrowseSourceOdr.Click += btnBrowseSourceOdr_Click;
            // 
            // btnConvertOdrToObj
            // 
            btnConvertOdrToObj.Location = new Point(526, 146);
            btnConvertOdrToObj.Name = "btnConvertOdrToObj";
            btnConvertOdrToObj.Size = new Size(99, 27);
            btnConvertOdrToObj.TabIndex = 9;
            btnConvertOdrToObj.Text = "ODR->OBJ";
            btnConvertOdrToObj.UseVisualStyleBackColor = true;
            btnConvertOdrToObj.Click += btnConvertOdrToObj_Click;
            // 
            // btnConvertObjToOdr
            // 
            btnConvertObjToOdr.Location = new Point(416, 146);
            btnConvertObjToOdr.Name = "btnConvertObjToOdr";
            btnConvertObjToOdr.Size = new Size(104, 27);
            btnConvertObjToOdr.TabIndex = 8;
            btnConvertObjToOdr.Text = "OBJ->ODR";
            btnConvertObjToOdr.UseVisualStyleBackColor = true;
            btnConvertObjToOdr.Click += btnConvertObjToOdr_Click;
            // 
            // btnOpenIn3DPreview
            // 
            btnOpenIn3DPreview.Location = new Point(526, 113);
            btnOpenIn3DPreview.Name = "btnOpenIn3DPreview";
            btnOpenIn3DPreview.Size = new Size(99, 27);
            btnOpenIn3DPreview.TabIndex = 7;
            btnOpenIn3DPreview.Text = "3D Preview";
            btnOpenIn3DPreview.UseVisualStyleBackColor = true;
            btnOpenIn3DPreview.Click += btnOpenIn3DPreview_Click;
            // 
            // btnOpenOverridesFolder
            // 
            btnOpenOverridesFolder.Location = new Point(416, 113);
            btnOpenOverridesFolder.Name = "btnOpenOverridesFolder";
            btnOpenOverridesFolder.Size = new Size(104, 27);
            btnOpenOverridesFolder.TabIndex = 6;
            btnOpenOverridesFolder.Text = "Open Folder";
            btnOpenOverridesFolder.UseVisualStyleBackColor = true;
            btnOpenOverridesFolder.Click += btnOpenOverridesFolder_Click;
            // 
            // btnClearObjOverride
            // 
            btnClearObjOverride.Location = new Point(416, 51);
            btnClearObjOverride.Name = "btnClearObjOverride";
            btnClearObjOverride.Size = new Size(209, 27);
            btnClearObjOverride.TabIndex = 5;
            btnClearObjOverride.Text = "Clear Override";
            btnClearObjOverride.UseVisualStyleBackColor = true;
            btnClearObjOverride.Click += btnClearObjOverride_Click;
            // 
            // btnImportObjOverride
            // 
            btnImportObjOverride.Location = new Point(416, 20);
            btnImportObjOverride.Name = "btnImportObjOverride";
            btnImportObjOverride.Size = new Size(209, 27);
            btnImportObjOverride.TabIndex = 4;
            btnImportObjOverride.Text = "Import OBJ Override...";
            btnImportObjOverride.UseVisualStyleBackColor = true;
            btnImportObjOverride.Click += btnImportObjOverride_Click;
            // 
            // txtOverrideObjPath
            // 
            txtOverrideObjPath.BackColor = Color.WhiteSmoke;
            txtOverrideObjPath.Location = new Point(70, 49);
            txtOverrideObjPath.Name = "txtOverrideObjPath";
            txtOverrideObjPath.ReadOnly = true;
            txtOverrideObjPath.Size = new Size(328, 23);
            txtOverrideObjPath.TabIndex = 3;
            // 
            // lblOverrideObj
            // 
            lblOverrideObj.AutoSize = true;
            lblOverrideObj.Location = new Point(16, 52);
            lblOverrideObj.Name = "lblOverrideObj";
            lblOverrideObj.Size = new Size(27, 15);
            lblOverrideObj.TabIndex = 2;
            lblOverrideObj.Text = "OBJ";
            // 
            // txtSelectedArchetype
            // 
            txtSelectedArchetype.Location = new Point(70, 22);
            txtSelectedArchetype.Name = "txtSelectedArchetype";
            txtSelectedArchetype.ReadOnly = true;
            txtSelectedArchetype.Size = new Size(328, 23);
            txtSelectedArchetype.TabIndex = 1;
            // 
            // lblSelectedArchetype
            // 
            lblSelectedArchetype.AutoSize = true;
            lblSelectedArchetype.Location = new Point(16, 25);
            lblSelectedArchetype.Name = "lblSelectedArchetype";
            lblSelectedArchetype.Size = new Size(54, 15);
            lblSelectedArchetype.TabIndex = 0;
            lblSelectedArchetype.Text = "Selected:";
            // 
            // CustomSlodsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(664, 609);
            Controls.Add(grpObjOverride);
            Controls.Add(lblStatus);
            Controls.Add(btnClose);
            Controls.Add(btnSave);
            Controls.Add(txtMeshes);
            Controls.Add(btnAddFromResources);
            Controls.Add(btnClearList);
            Controls.Add(lblMeshes);
            Controls.Add(btnBrowseConfig);
            Controls.Add(txtConfigPath);
            Controls.Add(lblConfigPath);
            Controls.Add(lblInfo);
            MinimumSize = new Size(533, 525);
            Name = "CustomSlodsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Custom SLODs Manager";
            grpObjOverride.ResumeLayout(false);
            grpObjOverride.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblConfigPath;
        private System.Windows.Forms.TextBox txtConfigPath;
        private System.Windows.Forms.Button btnBrowseConfig;
        private System.Windows.Forms.Label lblMeshes;
        private System.Windows.Forms.RichTextBox txtMeshes;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox grpObjOverride;
        private System.Windows.Forms.Label lblSelectedArchetype;
        private System.Windows.Forms.TextBox txtSelectedArchetype;
        private System.Windows.Forms.Label lblOverrideObj;
        private System.Windows.Forms.TextBox txtOverrideObjPath;
        private System.Windows.Forms.Button btnImportObjOverride;
        private System.Windows.Forms.Button btnClearObjOverride;
        private System.Windows.Forms.Button btnOpenOverridesFolder;
        private System.Windows.Forms.Button btnOpenIn3DPreview;
        private System.Windows.Forms.Button btnConvertObjToOdr;
        private System.Windows.Forms.Button btnConvertOdrToObj;
        private System.Windows.Forms.Button btnAddFromResources;
        private System.Windows.Forms.Button btnClearList;
        private System.Windows.Forms.Button btnBrowseSourceOdr;
        private System.Windows.Forms.Label lblSourceOdr;
        private System.Windows.Forms.TextBox txtSourceOdrPath;
        private System.Windows.Forms.CheckBox chkPreviewSourceOdr;
    }
}