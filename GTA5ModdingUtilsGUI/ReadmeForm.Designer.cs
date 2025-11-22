namespace GTA5ModdingUtilsGUI
{
    partial class ReadmeForm
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
            this.txtReadme = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtReadme
            // 
            this.txtReadme.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtReadme.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtReadme.Location = new System.Drawing.Point(0, 0);
            this.txtReadme.Name = "txtReadme";
            this.txtReadme.ReadOnly = true;
            this.txtReadme.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtReadme.Size = new System.Drawing.Size(800, 600);
            this.txtReadme.TabIndex = 0;
            this.txtReadme.Text = "";
            // 
            // ReadmeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.txtReadme);
            this.Name = "ReadmeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Readme – GTA5 Modding Utils GUI";
            this.Load += new System.EventHandler(this.ReadmeForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtReadme;
    }
}
