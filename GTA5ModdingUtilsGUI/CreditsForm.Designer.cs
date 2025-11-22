namespace GTA5ModdingUtilsGUI
{
    partial class CreditsForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblUiAuthor = new System.Windows.Forms.Label();
            this.lblCoreAuthor = new System.Windows.Forms.Label();
            this.linkGithub = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(24, 18);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(247, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "GTA5 Modding Utils – GUI";
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblHeader.Location = new System.Drawing.Point(26, 56);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(62, 19);
            this.lblHeader.TabIndex = 1;
            this.lblHeader.Text = "Credits";
            // 
            // lblUiAuthor
            // 
            this.lblUiAuthor.AutoSize = true;
            this.lblUiAuthor.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblUiAuthor.Location = new System.Drawing.Point(27, 86);
            this.lblUiAuthor.Name = "lblUiAuthor";
            this.lblUiAuthor.Size = new System.Drawing.Size(359, 17);
            this.lblUiAuthor.TabIndex = 2;
            this.lblUiAuthor.Text = "User interface and Windows GUI front-end: TheBabyGoat";
            // 
            // lblCoreAuthor
            // 
            this.lblCoreAuthor.AutoSize = true;
            this.lblCoreAuthor.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblCoreAuthor.Location = new System.Drawing.Point(27, 113);
            this.lblCoreAuthor.Name = "lblCoreAuthor";
            this.lblCoreAuthor.Size = new System.Drawing.Size(354, 17);
            this.lblCoreAuthor.TabIndex = 3;
            this.lblCoreAuthor.Text = "Core implementation and Python tools: Larcius (original repo)";
            // 
            // linkGithub
            // 
            this.linkGithub.AutoSize = true;
            this.linkGithub.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.linkGithub.Location = new System.Drawing.Point(27, 145);
            this.linkGithub.Name = "linkGithub";
            this.linkGithub.Size = new System.Drawing.Size(387, 17);
            this.linkGithub.TabIndex = 4;
            this.linkGithub.TabStop = true;
            this.linkGithub.Text = "Original project on GitHub: https://github.com/Larcius/gta5-modding-utils";
            this.linkGithub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkGithub_LinkClicked);
            // 
            // CreditsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 200);
            this.Controls.Add(this.linkGithub);
            this.Controls.Add(this.lblCoreAuthor);
            this.Controls.Add(this.lblUiAuthor);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.lblTitle);
            this.Name = "CreditsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Credits – GTA5 Modding Utils GUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblUiAuthor;
        private System.Windows.Forms.Label lblCoreAuthor;
        private System.Windows.Forms.LinkLabel linkGithub;
    }
}
