namespace GTA5ModdingUtilsGUI
{
    partial class TextureCreationForm
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
            grpTextureVariants = new GroupBox();
            lblTextureStatus = new Label();
            pnlTextureProgressBar = new Panel();
            pnlTextureProgressFill = new Panel();
            lblTextureProgressPercent = new Label();
            btnGenerateTextures = new Button();
            btnEditAnchors = new Button();
            chkSeasonWinter = new CheckBox();
            chkSeasonFall = new CheckBox();
            chkSeasonSpring = new CheckBox();
            btnOpenTextureOutputDir = new Button();
            btnBrowseTextureOutputDir = new Button();
            txtTextureOutputDir = new TextBox();
            lblTextureOutputDir = new Label();
            btnBrowseTextureSource = new Button();
            txtTextureSource = new TextBox();
            lblTextureSource = new Label();
            grpTextureVariants.SuspendLayout();
            SuspendLayout();
            // 
            // lblInfo
            // 
            lblInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblInfo.Location = new Point(12, 16);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(600, 32);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Generate seasonal texture variants (Spring / Fall / Winter) from a source foliage texture using the gta5-modding-utils texture_variants.py helper.";
            // 
            // grpTextureVariants
            // 
            grpTextureVariants.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpTextureVariants.Controls.Add(lblTextureStatus);
            grpTextureVariants.Controls.Add(lblTextureProgressPercent);
            grpTextureVariants.Controls.Add(pnlTextureProgressBar);
            grpTextureVariants.Controls.Add(btnGenerateTextures);
            grpTextureVariants.Controls.Add(btnEditAnchors);
            grpTextureVariants.Controls.Add(chkSeasonWinter);
            grpTextureVariants.Controls.Add(chkSeasonFall);
            grpTextureVariants.Controls.Add(chkSeasonSpring);
            grpTextureVariants.Controls.Add(btnOpenTextureOutputDir);
            grpTextureVariants.Controls.Add(btnBrowseTextureOutputDir);
            grpTextureVariants.Controls.Add(txtTextureOutputDir);
            grpTextureVariants.Controls.Add(lblTextureOutputDir);
            grpTextureVariants.Controls.Add(btnBrowseTextureSource);
            grpTextureVariants.Controls.Add(txtTextureSource);
            grpTextureVariants.Controls.Add(lblTextureSource);
            grpTextureVariants.Location = new Point(12, 52);
            grpTextureVariants.Name = "grpTextureVariants";
            grpTextureVariants.Size = new Size(600, 157);
            grpTextureVariants.TabIndex = 1;
            grpTextureVariants.TabStop = false;
            grpTextureVariants.Text = "Texture Variants (Season Presets)";
            grpTextureVariants.Resize += grpTextureVariants_Resize;
            // 
            // pnlTextureProgressBar
            // 
            pnlTextureProgressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlTextureProgressBar.BorderStyle = BorderStyle.FixedSingle;
            pnlTextureProgressBar.Controls.Add(pnlTextureProgressFill);
            pnlTextureProgressBar.Location = new Point(100, 107);
            pnlTextureProgressBar.Name = "pnlTextureProgressBar";
            pnlTextureProgressBar.Size = new Size(404, 10);
            pnlTextureProgressBar.TabIndex = 12;
            // 
            // pnlTextureProgressFill
            // 
            pnlTextureProgressFill.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            pnlTextureProgressFill.BackColor = Color.LimeGreen;
            pnlTextureProgressFill.Location = new Point(0, 0);
            pnlTextureProgressFill.Name = "pnlTextureProgressFill";
            pnlTextureProgressFill.Size = new Size(0, 10);
            pnlTextureProgressFill.TabIndex = 0;
            // 
            // lblTextureProgressPercent
            // 
            lblTextureProgressPercent.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblTextureProgressPercent.Location = new Point(510, 104);
            lblTextureProgressPercent.Name = "lblTextureProgressPercent";
            lblTextureProgressPercent.Size = new Size(80, 15);
            lblTextureProgressPercent.TabIndex = 13;
            lblTextureProgressPercent.Text = "0%";
            lblTextureProgressPercent.TextAlign = ContentAlignment.MiddleRight;

            // 
            // lblTextureStatus
            // 
            lblTextureStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            
            // pnlTextureProgressBar
            //
            pnlTextureProgressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlTextureProgressBar.BorderStyle = BorderStyle.FixedSingle;
            pnlTextureProgressBar.Location = new Point(100, 107);
            pnlTextureProgressBar.Name = "pnlTextureProgressBar";
            pnlTextureProgressBar.Size = new Size(404, 10);
            pnlTextureProgressBar.TabIndex = 12;
            //
            // pnlTextureProgressFill
            //
            pnlTextureProgressFill.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            pnlTextureProgressFill.BackColor = Color.LimeGreen;
            pnlTextureProgressFill.Location = new Point(0, 0);
            pnlTextureProgressFill.Name = "pnlTextureProgressFill";
            pnlTextureProgressFill.Size = new Size(0, 10);
            pnlTextureProgressFill.TabIndex = 0;
            pnlTextureProgressBar.Controls.Add(pnlTextureProgressFill);
            //
            // lblTextureProgressPercent
            //
            lblTextureProgressPercent.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblTextureProgressPercent.Location = new Point(510, 103);
            lblTextureProgressPercent.Name = "lblTextureProgressPercent";
            lblTextureProgressPercent.Size = new Size(80, 15);
            lblTextureProgressPercent.TabIndex = 13;
            lblTextureProgressPercent.Text = "0%";
            lblTextureProgressPercent.TextAlign = ContentAlignment.MiddleRight;

            lblTextureStatus.AutoEllipsis = true;
            lblTextureStatus.Location = new Point(100, 124);
            lblTextureStatus.Name = "lblTextureStatus";
            lblTextureStatus.Size = new Size(490, 19);
            lblTextureStatus.TabIndex = 12;
            lblTextureStatus.Text = "Status";
            // 
            // btnGenerateTextures
            // 
            btnGenerateTextures.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnGenerateTextures.Location = new Point(10, 122);
            btnGenerateTextures.Name = "btnGenerateTextures";
            btnGenerateTextures.Size = new Size(80, 23);
            btnGenerateTextures.TabIndex = 11;
            btnGenerateTextures.Text = "Generate";
            btnGenerateTextures.UseVisualStyleBackColor = true;
            btnGenerateTextures.Click += btnGenerateTextures_Click;
            // 
            // btnEditAnchors
            // 
            btnEditAnchors.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEditAnchors.Location = new Point(510, 78);
            btnEditAnchors.Name = "btnEditAnchors";
            btnEditAnchors.Size = new Size(80, 23);
            btnEditAnchors.TabIndex = 10;
            btnEditAnchors.Text = "Anchors...";
            btnEditAnchors.UseVisualStyleBackColor = true;
            btnEditAnchors.Click += btnEditAnchors_Click;
            // 
            // chkSeasonWinter
            // 
            chkSeasonWinter.AutoSize = true;
            chkSeasonWinter.Location = new Point(214, 82);
            chkSeasonWinter.Name = "chkSeasonWinter";
            chkSeasonWinter.Size = new Size(61, 19);
            chkSeasonWinter.TabIndex = 9;
            chkSeasonWinter.Text = "Winter";
            chkSeasonWinter.UseVisualStyleBackColor = true;
            // 
            // chkSeasonFall
            // 
            chkSeasonFall.AutoSize = true;
            chkSeasonFall.Location = new Point(165, 82);
            chkSeasonFall.Name = "chkSeasonFall";
            chkSeasonFall.Size = new Size(44, 19);
            chkSeasonFall.TabIndex = 8;
            chkSeasonFall.Text = "Fall";
            chkSeasonFall.UseVisualStyleBackColor = true;
            // 
            // chkSeasonSpring
            // 
            chkSeasonSpring.AutoSize = true;
            chkSeasonSpring.Location = new Point(100, 82);
            chkSeasonSpring.Name = "chkSeasonSpring";
            chkSeasonSpring.Size = new Size(60, 19);
            chkSeasonSpring.TabIndex = 7;
            chkSeasonSpring.Text = "Spring";
            chkSeasonSpring.UseVisualStyleBackColor = true;
            //
            // 
            // btnOpenTextureOutputDir
            // 
            btnOpenTextureOutputDir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOpenTextureOutputDir.Location = new Point(424, 51);
            btnOpenTextureOutputDir.Name = "btnOpenTextureOutputDir";
            btnOpenTextureOutputDir.Size = new Size(80, 23);
            btnOpenTextureOutputDir.TabIndex = 5;
            btnOpenTextureOutputDir.Text = "Open";
            btnOpenTextureOutputDir.UseVisualStyleBackColor = true;
            btnOpenTextureOutputDir.Click += btnOpenTextureOutputDir_Click;

            // btnBrowseTextureOutputDir
            // 
            btnBrowseTextureOutputDir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseTextureOutputDir.Location = new Point(510, 51);
            btnBrowseTextureOutputDir.Name = "btnBrowseTextureOutputDir";
            btnBrowseTextureOutputDir.Size = new Size(80, 23);
            btnBrowseTextureOutputDir.TabIndex = 6;
            btnBrowseTextureOutputDir.Text = "Browse...";
            btnBrowseTextureOutputDir.UseVisualStyleBackColor = true;
            btnBrowseTextureOutputDir.Click += btnBrowseTextureOutputDir_Click;
            // 
            // txtTextureOutputDir
            // 
            txtTextureOutputDir.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTextureOutputDir.Location = new Point(100, 50);
            txtTextureOutputDir.Name = "txtTextureOutputDir";
            txtTextureOutputDir.Size = new Size(318, 23);
            txtTextureOutputDir.TabIndex = 4;
            // 
            // lblTextureOutputDir
            // 
            lblTextureOutputDir.AutoSize = true;
            lblTextureOutputDir.Location = new Point(10, 53);
            lblTextureOutputDir.Name = "lblTextureOutputDir";
            lblTextureOutputDir.Size = new Size(82, 15);
            lblTextureOutputDir.TabIndex = 3;
            lblTextureOutputDir.Text = "Output folder:";
            // 
            // btnBrowseTextureSource
            // 
            btnBrowseTextureSource.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseTextureSource.Location = new Point(510, 22);
            btnBrowseTextureSource.Name = "btnBrowseTextureSource";
            btnBrowseTextureSource.Size = new Size(80, 23);
            btnBrowseTextureSource.TabIndex = 2;
            btnBrowseTextureSource.Text = "Browse...";
            btnBrowseTextureSource.UseVisualStyleBackColor = true;
            btnBrowseTextureSource.Click += btnBrowseTextureSource_Click;
            // 
            // txtTextureSource
            // 
            txtTextureSource.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTextureSource.Location = new Point(100, 21);
            txtTextureSource.Name = "txtTextureSource";
            txtTextureSource.Size = new Size(404, 23);
            txtTextureSource.TabIndex = 1;
            // 
            // lblTextureSource
            // 
            lblTextureSource.AutoSize = true;
            lblTextureSource.Location = new Point(10, 24);
            lblTextureSource.Name = "lblTextureSource";
            lblTextureSource.Size = new Size(85, 15);
            lblTextureSource.TabIndex = 0;
            lblTextureSource.Text = "Source texture:";
            // 
            // TextureCreationForm
            // 
            AcceptButton = btnGenerateTextures;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(624, 221);
            Controls.Add(grpTextureVariants);
            Controls.Add(lblInfo);
            MinimumSize = new Size(640, 220);
            Name = "TextureCreationForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Texture Creation";
            grpTextureVariants.ResumeLayout(false);
            grpTextureVariants.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.GroupBox grpTextureVariants;
        private System.Windows.Forms.Label lblTextureSource;
        private System.Windows.Forms.TextBox txtTextureSource;
        private System.Windows.Forms.Button btnBrowseTextureSource;
        private System.Windows.Forms.Label lblTextureOutputDir;
        private System.Windows.Forms.TextBox txtTextureOutputDir;
        private System.Windows.Forms.Button btnOpenTextureOutputDir;
        private System.Windows.Forms.Button btnBrowseTextureOutputDir;
        private System.Windows.Forms.CheckBox chkSeasonSpring;
        private System.Windows.Forms.CheckBox chkSeasonFall;
        private System.Windows.Forms.CheckBox chkSeasonWinter;
        private System.Windows.Forms.Button btnEditAnchors;
        private System.Windows.Forms.Button btnGenerateTextures;
        private System.Windows.Forms.Label lblTextureStatus;
        private System.Windows.Forms.Panel pnlTextureProgressBar;
        private System.Windows.Forms.Panel pnlTextureProgressFill;
        private System.Windows.Forms.Label lblTextureProgressPercent;
    }
}
