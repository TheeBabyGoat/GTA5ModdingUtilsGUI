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
            lblInfo = new System.Windows.Forms.Label();
            lblConfigPath = new System.Windows.Forms.Label();
            txtConfigPath = new System.Windows.Forms.TextBox();
            btnBrowseConfig = new System.Windows.Forms.Button();
            lblMeshes = new System.Windows.Forms.Label();
            btnClearList = new System.Windows.Forms.Button();
            btnAddFromResources = new System.Windows.Forms.Button();
            txtMeshes = new System.Windows.Forms.RichTextBox();
            btnSave = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            grpObjOverride = new System.Windows.Forms.GroupBox();
            btnConvertOdrToObj = new System.Windows.Forms.Button();
            btnConvertObjToOdr = new System.Windows.Forms.Button();
            btnOpenIn3DPreview = new System.Windows.Forms.Button();
            btnOpenOverridesFolder = new System.Windows.Forms.Button();
            btnClearObjOverride = new System.Windows.Forms.Button();
            btnImportObjOverride = new System.Windows.Forms.Button();
            txtOverrideObjPath = new System.Windows.Forms.TextBox();
            lblOverrideObj = new System.Windows.Forms.Label();
            txtSelectedArchetype = new System.Windows.Forms.TextBox();
            lblSelectedArchetype = new System.Windows.Forms.Label();
            grpObjOverride.SuspendLayout();
            SuspendLayout();
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.Location = new System.Drawing.Point(12, 9);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new System.Drawing.Size(479, 30);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Custom SLODs are props that require specific SLOD models managed manually.\r\nThey are exported to 'custom_slods/' and use 'slod_' texture samplers.";
            // 
            // lblConfigPath
            // 
            lblConfigPath.AutoSize = true;
            lblConfigPath.Location = new System.Drawing.Point(12, 52);
            lblConfigPath.Name = "lblConfigPath";
            lblConfigPath.Size = new System.Drawing.Size(104, 15);
            lblConfigPath.TabIndex = 1;
            lblConfigPath.Text = "Config file (JSON):";
            // 
            // txtConfigPath
            // 
            txtConfigPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtConfigPath.Location = new System.Drawing.Point(119, 49);
            txtConfigPath.Name = "txtConfigPath";
            txtConfigPath.Size = new System.Drawing.Size(412, 23);
            txtConfigPath.TabIndex = 2;
            // 
            // btnBrowseConfig
            // 
            btnBrowseConfig.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnBrowseConfig.Location = new System.Drawing.Point(537, 48);
            btnBrowseConfig.Name = "btnBrowseConfig";
            btnBrowseConfig.Size = new System.Drawing.Size(75, 23);
            btnBrowseConfig.TabIndex = 3;
            btnBrowseConfig.Text = "Browse...";
            btnBrowseConfig.UseVisualStyleBackColor = true;
            btnBrowseConfig.Click += btnBrowseConfig_Click;
            // 
            // lblMeshes
            // 
            lblMeshes.AutoSize = true;
            lblMeshes.Location = new System.Drawing.Point(12, 86);
            lblMeshes.Name = "lblMeshes";
            lblMeshes.Size = new System.Drawing.Size(250, 15);
            lblMeshes.TabIndex = 4;
            lblMeshes.Text = "Custom SLOD archetype names (one per line):";
            // 
            // btnClearList
            // 
            btnClearList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnClearList.Location = new System.Drawing.Point(268, 78);
            btnClearList.Name = "btnClearList";
            btnClearList.Size = new System.Drawing.Size(80, 23);
            btnClearList.TabIndex = 6;
            btnClearList.Text = "Clear";
            btnClearList.UseVisualStyleBackColor = true;
            btnClearList.Click += btnClearList_Click;
            // 
            // btnAddFromResources
            // 
            btnAddFromResources.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnAddFromResources.Location = new System.Drawing.Point(354, 78);
            btnAddFromResources.Name = "btnAddFromResources";
            btnAddFromResources.Size = new System.Drawing.Size(160, 23);
            btnAddFromResources.TabIndex = 7;
            btnAddFromResources.Text = "Add from Resources...";
            btnAddFromResources.UseVisualStyleBackColor = true;
            // Note: If you have an event handler for this in CustomSlodsForm.cs, uncomment the next line:
            btnAddFromResources.Click += btnAddFromResources_Click; 
            // Otherwise, we leave it without an event for now to avoid errors if the method is missing.
            btnAddFromResources.Enabled = true; // Disabled by default since we didn't add logic for it in Slods
            // 
            // txtMeshes
            // 
            txtMeshes.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtMeshes.DetectUrls = false;
            txtMeshes.Location = new System.Drawing.Point(12, 104);
            txtMeshes.Name = "txtMeshes";
            txtMeshes.Size = new System.Drawing.Size(600, 240);
            txtMeshes.TabIndex = 5;
            txtMeshes.Text = "";
            txtMeshes.WordWrap = false;
            // 
            // btnSave
            // 
            btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnSave.Location = new System.Drawing.Point(456, 511);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(75, 27);
            btnSave.TabIndex = 8;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnClose.Location = new System.Drawing.Point(537, 511);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(75, 27);
            btnClose.TabIndex = 9;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.Location = new System.Drawing.Point(12, 514);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(438, 33);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Status";
            // 
            // grpObjOverride
            // 
            grpObjOverride.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
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
            grpObjOverride.Location = new System.Drawing.Point(12, 350);
            grpObjOverride.Name = "grpObjOverride";
            grpObjOverride.Size = new System.Drawing.Size(600, 150);
            grpObjOverride.TabIndex = 6;
            grpObjOverride.TabStop = false;
            grpObjOverride.Text = "SLOD OBJ Override (selected line)";
            // 
            // btnConvertOdrToObj
            // 
            btnConvertOdrToObj.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnConvertOdrToObj.Location = new System.Drawing.Point(492, 110);
            btnConvertOdrToObj.Name = "btnConvertOdrToObj";
            btnConvertOdrToObj.Size = new System.Drawing.Size(98, 25);
            btnConvertOdrToObj.TabIndex = 9;
            btnConvertOdrToObj.Text = "ODR -> OBJ";
            btnConvertOdrToObj.UseVisualStyleBackColor = true;
            btnConvertOdrToObj.Click += btnConvertOdrToObj_Click;
            // 
            // btnConvertObjToOdr
            // 
            btnConvertObjToOdr.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnConvertObjToOdr.Location = new System.Drawing.Point(390, 110);
            btnConvertObjToOdr.Name = "btnConvertObjToOdr";
            btnConvertObjToOdr.Size = new System.Drawing.Size(98, 25);
            btnConvertObjToOdr.TabIndex = 8;
            btnConvertObjToOdr.Text = "OBJ -> ODR";
            btnConvertObjToOdr.UseVisualStyleBackColor = true;
            btnConvertObjToOdr.Click += btnConvertObjToOdr_Click;
            // 
            // btnOpenIn3DPreview
            // 
            btnOpenIn3DPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnOpenIn3DPreview.Location = new System.Drawing.Point(492, 80);
            btnOpenIn3DPreview.Name = "btnOpenIn3DPreview";
            btnOpenIn3DPreview.Size = new System.Drawing.Size(98, 25);
            btnOpenIn3DPreview.TabIndex = 7;
            btnOpenIn3DPreview.Text = "3D Preview";
            btnOpenIn3DPreview.UseVisualStyleBackColor = true;
            btnOpenIn3DPreview.Click += btnOpenIn3DPreview_Click;
            // 
            // btnOpenOverridesFolder
            // 
            btnOpenOverridesFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnOpenOverridesFolder.Location = new System.Drawing.Point(390, 80);
            btnOpenOverridesFolder.Name = "btnOpenOverridesFolder";
            btnOpenOverridesFolder.Size = new System.Drawing.Size(98, 25);
            btnOpenOverridesFolder.TabIndex = 6;
            btnOpenOverridesFolder.Text = "Open Folder";
            btnOpenOverridesFolder.UseVisualStyleBackColor = true;
            btnOpenOverridesFolder.Click += btnOpenOverridesFolder_Click;
            // 
            // btnClearObjOverride
            // 
            btnClearObjOverride.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnClearObjOverride.Location = new System.Drawing.Point(390, 50);
            btnClearObjOverride.Name = "btnClearObjOverride";
            btnClearObjOverride.Size = new System.Drawing.Size(200, 25);
            btnClearObjOverride.TabIndex = 5;
            btnClearObjOverride.Text = "Clear Override";
            btnClearObjOverride.UseVisualStyleBackColor = true;
            btnClearObjOverride.Click += btnClearObjOverride_Click;
            // 
            // btnImportObjOverride
            // 
            btnImportObjOverride.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnImportObjOverride.Location = new System.Drawing.Point(390, 20);
            btnImportObjOverride.Name = "btnImportObjOverride";
            btnImportObjOverride.Size = new System.Drawing.Size(200, 25);
            btnImportObjOverride.TabIndex = 4;
            btnImportObjOverride.Text = "Import OBJ Override...";
            btnImportObjOverride.UseVisualStyleBackColor = true;
            btnImportObjOverride.Click += btnImportObjOverride_Click;
            // 
            // txtOverrideObjPath
            // 
            txtOverrideObjPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtOverrideObjPath.Location = new System.Drawing.Point(74, 51);
            txtOverrideObjPath.Name = "txtOverrideObjPath";
            txtOverrideObjPath.ReadOnly = true;
            txtOverrideObjPath.Size = new System.Drawing.Size(300, 23);
            txtOverrideObjPath.TabIndex = 3;
            // 
            // lblOverrideObj
            // 
            lblOverrideObj.AutoSize = true;
            lblOverrideObj.Location = new System.Drawing.Point(10, 54);
            lblOverrideObj.Name = "lblOverrideObj";
            lblOverrideObj.Size = new System.Drawing.Size(30, 15);
            lblOverrideObj.TabIndex = 2;
            lblOverrideObj.Text = "OBJ:";
            // 
            // txtSelectedArchetype
            // 
            txtSelectedArchetype.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtSelectedArchetype.Location = new System.Drawing.Point(74, 21);
            txtSelectedArchetype.Name = "txtSelectedArchetype";
            txtSelectedArchetype.ReadOnly = true;
            txtSelectedArchetype.Size = new System.Drawing.Size(300, 23);
            txtSelectedArchetype.TabIndex = 1;
            // 
            // lblSelectedArchetype
            // 
            lblSelectedArchetype.AutoSize = true;
            lblSelectedArchetype.Location = new System.Drawing.Point(10, 24);
            lblSelectedArchetype.Name = "lblSelectedArchetype";
            lblSelectedArchetype.Size = new System.Drawing.Size(54, 15);
            lblSelectedArchetype.TabIndex = 0;
            lblSelectedArchetype.Text = "Selected:";
            // 
            // CustomSlodsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new System.Drawing.Size(624, 550);
            Controls.Add(grpObjOverride);
            Controls.Add(lblStatus);
            Controls.Add(btnClose);
            Controls.Add(btnSave);
            Controls.Add(txtMeshes);
            Controls.Add(btnClearList);
            Controls.Add(btnAddFromResources);
            Controls.Add(lblMeshes);
            Controls.Add(btnBrowseConfig);
            Controls.Add(txtConfigPath);
            Controls.Add(lblConfigPath);
            Controls.Add(lblInfo);
            MinimumSize = new System.Drawing.Size(640, 420);
            Name = "CustomSlodsForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Custom SLODs";
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
    }
}