
namespace GTA5ModdingUtilsGUI
{
    partial class CustomMeshesForm
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
            lblInfo.Location = new Point(12, 9);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(479, 30);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Custom meshes are props not covered by the built-in LOD / SLOD atlas candidate list.\r\nThey can be exported (custom_meshes/) and are eligible for SLOD2+ generation when you run LOD Map.";
            // 
            // lblConfigPath
            // 
            lblConfigPath.AutoSize = true;
            lblConfigPath.Location = new Point(12, 52);
            lblConfigPath.Name = "lblConfigPath";
            lblConfigPath.Size = new Size(104, 15);
            lblConfigPath.TabIndex = 1;
            lblConfigPath.Text = "Config file (JSON):";
            // 
            // txtConfigPath
            // 
            txtConfigPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtConfigPath.Location = new Point(119, 49);
            txtConfigPath.Name = "txtConfigPath";
            txtConfigPath.Size = new Size(412, 23);
            txtConfigPath.TabIndex = 2;
            // 
            // btnBrowseConfig
            // 
            btnBrowseConfig.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseConfig.Location = new Point(537, 48);
            btnBrowseConfig.Name = "btnBrowseConfig";
            btnBrowseConfig.Size = new Size(75, 23);
            btnBrowseConfig.TabIndex = 3;
            btnBrowseConfig.Text = "Browse...";
            btnBrowseConfig.UseVisualStyleBackColor = true;
            btnBrowseConfig.Click += btnBrowseConfig_Click;
            // 
            // lblMeshes
            // 
            lblMeshes.AutoSize = true;
            lblMeshes.Location = new Point(12, 86);
            lblMeshes.Name = "lblMeshes";
            lblMeshes.Size = new Size(250, 15);
            lblMeshes.TabIndex = 4;
            lblMeshes.Text = "Custom mesh archetype names (one per line):";
            // 
            // btnClearList
            // 
            btnClearList.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearList.Location = new Point(268, 78);
            btnClearList.Name = "btnClearList";
            btnClearList.Size = new Size(80, 23);
            btnClearList.TabIndex = 6;
            btnClearList.Text = "Clear";
            btnClearList.UseVisualStyleBackColor = true;
            btnClearList.Click += btnClearList_Click;
            // 
            // btnAddFromResources
            // 
            btnAddFromResources.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddFromResources.Location = new Point(354, 78);
            btnAddFromResources.Name = "btnAddFromResources";
            btnAddFromResources.Size = new Size(160, 23);
            btnAddFromResources.TabIndex = 7;
            btnAddFromResources.Text = "Add from Resources...";
            btnAddFromResources.UseVisualStyleBackColor = true;
            btnAddFromResources.Click += btnAddFromResources_Click;
            // 
            // txtMeshes
            // 
            txtMeshes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtMeshes.DetectUrls = false;
            txtMeshes.Location = new Point(12, 104);
            txtMeshes.Name = "txtMeshes";
            txtMeshes.Size = new Size(600, 240);
            txtMeshes.TabIndex = 5;
            txtMeshes.Text = "";
            txtMeshes.WordWrap = false;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(456, 511);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 27);
            btnSave.TabIndex = 8;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(537, 511);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 27);
            btnClose.TabIndex = 9;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Location = new Point(12, 514);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(438, 33);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Status";
            // 
            // grpObjOverride
            // 
            grpObjOverride.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
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
            grpObjOverride.Location = new Point(12, 350);
            grpObjOverride.Name = "grpObjOverride";
            grpObjOverride.Size = new Size(600, 150);
            grpObjOverride.TabIndex = 6;
            grpObjOverride.TabStop = false;
            grpObjOverride.Text = "OBJ Override (selected line)";
            // 
            // btnConvertOdrToObj
            // 
            btnConvertOdrToObj.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConvertOdrToObj.Location = new Point(492, 110);
            btnConvertOdrToObj.Name = "btnConvertOdrToObj";
            btnConvertOdrToObj.Size = new Size(98, 25);
            btnConvertOdrToObj.TabIndex = 9;
            btnConvertOdrToObj.Text = "ODR -> OBJ";
            btnConvertOdrToObj.UseVisualStyleBackColor = true;
            btnConvertOdrToObj.Click += btnConvertOdrToObj_Click;
            // 
            // btnConvertObjToOdr
            // 
            btnConvertObjToOdr.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConvertObjToOdr.Location = new Point(390, 110);
            btnConvertObjToOdr.Name = "btnConvertObjToOdr";
            btnConvertObjToOdr.Size = new Size(98, 25);
            btnConvertObjToOdr.TabIndex = 8;
            btnConvertObjToOdr.Text = "OBJ -> ODR";
            btnConvertObjToOdr.UseVisualStyleBackColor = true;
            btnConvertObjToOdr.Click += btnConvertObjToOdr_Click;
            // 
            // btnOpenIn3DPreview
            // 
            btnOpenIn3DPreview.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOpenIn3DPreview.Location = new Point(492, 80);
            btnOpenIn3DPreview.Name = "btnOpenIn3DPreview";
            btnOpenIn3DPreview.Size = new Size(98, 25);
            btnOpenIn3DPreview.TabIndex = 7;
            btnOpenIn3DPreview.Text = "3D Preview";
            btnOpenIn3DPreview.UseVisualStyleBackColor = true;
            btnOpenIn3DPreview.Click += btnOpenIn3DPreview_Click;
            // 
            // btnOpenOverridesFolder
            // 
            btnOpenOverridesFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOpenOverridesFolder.Location = new Point(390, 80);
            btnOpenOverridesFolder.Name = "btnOpenOverridesFolder";
            btnOpenOverridesFolder.Size = new Size(98, 25);
            btnOpenOverridesFolder.TabIndex = 6;
            btnOpenOverridesFolder.Text = "Open Folder";
            btnOpenOverridesFolder.UseVisualStyleBackColor = true;
            btnOpenOverridesFolder.Click += btnOpenOverridesFolder_Click;
            // 
            // btnClearObjOverride
            // 
            btnClearObjOverride.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearObjOverride.Location = new Point(390, 50);
            btnClearObjOverride.Name = "btnClearObjOverride";
            btnClearObjOverride.Size = new Size(200, 25);
            btnClearObjOverride.TabIndex = 5;
            btnClearObjOverride.Text = "Clear Override";
            btnClearObjOverride.UseVisualStyleBackColor = true;
            btnClearObjOverride.Click += btnClearObjOverride_Click;
            // 
            // btnImportObjOverride
            // 
            btnImportObjOverride.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnImportObjOverride.Location = new Point(390, 20);
            btnImportObjOverride.Name = "btnImportObjOverride";
            btnImportObjOverride.Size = new Size(200, 25);
            btnImportObjOverride.TabIndex = 4;
            btnImportObjOverride.Text = "Import OBJ Override...";
            btnImportObjOverride.UseVisualStyleBackColor = true;
            btnImportObjOverride.Click += btnImportObjOverride_Click;
            // 
            // txtOverrideObjPath
            // 
            txtOverrideObjPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOverrideObjPath.Location = new Point(74, 51);
            txtOverrideObjPath.Name = "txtOverrideObjPath";
            txtOverrideObjPath.ReadOnly = true;
            txtOverrideObjPath.Size = new Size(300, 23);
            txtOverrideObjPath.TabIndex = 3;
            // 
            // lblOverrideObj
            // 
            lblOverrideObj.AutoSize = true;
            lblOverrideObj.Location = new Point(10, 54);
            lblOverrideObj.Name = "lblOverrideObj";
            lblOverrideObj.Size = new Size(30, 15);
            lblOverrideObj.TabIndex = 2;
            lblOverrideObj.Text = "OBJ:";
            // 
            // txtSelectedArchetype
            // 
            txtSelectedArchetype.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSelectedArchetype.Location = new Point(74, 21);
            txtSelectedArchetype.Name = "txtSelectedArchetype";
            txtSelectedArchetype.ReadOnly = true;
            txtSelectedArchetype.Size = new Size(300, 23);
            txtSelectedArchetype.TabIndex = 1;
            // 
            // lblSelectedArchetype
            // 
            lblSelectedArchetype.AutoSize = true;
            lblSelectedArchetype.Location = new Point(10, 24);
            lblSelectedArchetype.Name = "lblSelectedArchetype";
            lblSelectedArchetype.Size = new Size(54, 15);
            lblSelectedArchetype.TabIndex = 0;
            lblSelectedArchetype.Text = "Selected:";
            // 
            // CustomMeshesForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(624, 550);
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
            MinimumSize = new Size(640, 420);
            Name = "CustomMeshesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Custom Meshes";
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
