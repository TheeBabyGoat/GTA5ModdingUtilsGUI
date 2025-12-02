namespace GTA5ModdingUtilsGUI
{
    partial class IntroForm
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
            this.panelMain = new System.Windows.Forms.Panel();
            this.txtInstructions = new System.Windows.Forms.RichTextBox();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblImportant = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnContinue = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.panelMain.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                                                          | System.Windows.Forms.AnchorStyles.Left) 
                                                                         | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMain.BackColor = System.Drawing.Color.White;
            this.panelMain.Controls.Add(this.txtInstructions);
            this.panelMain.Controls.Add(this.lblSubtitle);
            this.panelMain.Controls.Add(this.lblImportant);
            this.panelMain.Controls.Add(this.lblTitle);
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(24, 24, 24, 16);
            this.panelMain.Size = new System.Drawing.Size(1024, 660);
            this.panelMain.TabIndex = 0;
            // 
            // txtInstructions
            // 
            this.txtInstructions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                                                                | System.Windows.Forms.AnchorStyles.Left) 
                                                                               | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInstructions.BackColor = System.Drawing.Color.White;
            this.txtInstructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInstructions.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtInstructions.Location = new System.Drawing.Point(34, 130);
            this.txtInstructions.Name = "txtInstructions";
            this.txtInstructions.ReadOnly = true;
            this.txtInstructions.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtInstructions.Size = new System.Drawing.Size(950, 480);
            this.txtInstructions.TabIndex = 3;
            this.txtInstructions.TabStop = false;
            this.txtInstructions.Text = 
"SETUP OVERVIEW\n" +
"------------------------------\n" +
"This graphical tool is a thin wrapper around the original Python project \"gta5-modding-utils\".\n" +
"The heavy lifting is still done by the Python scripts – the GUI only builds the\n" +
"command line for you and shows the log output in a convenient way.\n" +
"\n" +
"1. Install Python / Miniconda\n" +
"   • Install Python or Miniconda (64-bit).\n" +
"   • If you use conda, open an Anaconda Prompt in the folder that contains\n" +
"     gta5-modding-utils-main.\n" +
"   • Create the environment from the provided environment.yml file, e.g.:\n" +
"       conda env create -f environment.yml\n" +
"       conda activate gta5-modding-utils   (or the name used in that file)\n" +
"\n" +
"2. Confirm required Python packages\n" +
"   The environment.yml installs required dependencies such as:\n" +
"   • numpy, scikit-learn, shapely, transforms3d, matplotlib, natsort\n" +
"   • miniball (via pip)\n" +
"   • Install glob2 using python -m pip install glob2\n" +
"   • glob2 is required for in app UV editing\n" +
"   • REPLACE LodMapCreator.py with the provided edited copy gta5-modding-utils-main\\worker\\lod_map_creator\n" +
"   • This is needed to allow the JSON file to communicate with the script.\n" +
"\n" +
"3. Run the GUI executable\n" +
"   • Extract the release package to a folder of your choice.\n" +
"   • Make sure the gta5-modding-utils-main folder is located next to\n" +
"     GTA5ModdingUtilsGUI.exe (this is the default layout of the release).\n" +
"   • Double-click GTA5ModdingUtilsGUI.exe to start the application.\n" +
"\n" +
"4. Set gta5-modding-utils to the path of your folder containing the enviorment.\n" +
"   On the main screen you will see a field \"Gta5-Modding-Utils\".\n" +
"\n" +
"5. Choose your input and output folders\n" +
"   • Input folder: directory containing your .ymap.xml files and related data.\n" +
"   • Output folder: where all generated files will be written. If you leave it\n" +
"     empty, the tool uses <inputFolder>\\generated by default.\n" +
"   If the chosen output folder already exists, the GUI will ask whether it\n" +
"   should be cleared before running (matching the behavior of main.py).\n" +
"\n" +
"6. Set a project prefix\n" +
"   • This is the same prefix expected by the Python scripts (e.g. forest01).\n" +
"   • It is used when naming generated maps, collision files and metadata.\n" +
"\n" +
"7. Select processing steps\n" +
"   The main form mirrors the flags of main.py:\n" +
"   • Vegetation      → --vegetationCreator=on\n" +
"   • Entropy         → --entropy=on\n" +
"   • Reducer         → --reducer=on and optional reducer resolution / scaling\n" +
"   • Clustering      → --clustering=on with optional cluster count, polygon\n" +
"                        region, clustering prefix and exclusions\n" +
"   • Static col      → --staticCol=on (static collision generation)\n" +
"   • LOD map         → --lodMap=on (LOD/SLOD map generation)\n" +
"   • Clear LOD       → --clearLod=on\n" +
"   • Reflection      → --reflection=on (requires LOD map)\n" +
"   • Sanitizer       → --sanitizer=on (data cleanup)\n" +
"   • Statistics      → --statistics=on (summary of entities and maps)\n" +
"\n" +
"8. Advanced options\n" +
"   • Reducer resolution and \"Adapt scaling\" map directly to the reducer\n" +
"     arguments of main.py.\n" +
"   • Clustering options allow you to fix the number of clusters, restrict\n" +
"     clustering to a polygon region, specify a prefix, and provide a list of\n" +
"     maps to exclude from clustering.\n" +
"\n" +
"9. Running and reading the log\n" +
"   • Click RUN to start the Python pipeline with the selected options.\n" +
"   • All console output from main.py appears in the log window at the bottom\n" +
"     of the main form. Error lines are prefixed with [ERR].\n" +
"   • Some steps may open matplotlib windows (for example, clustering plots);\n" +
"     interact with them as you normally would in Python.\n" +
"\n" +
"10. Cancelling\n" +
"   • While a pipeline is running, the Cancel button will terminate the Python\n" +
"     process if you need to stop early.\n" +
"\n" +
"When you are ready, click CONTINUE to open the main tool window.\n";
            this.txtInstructions.Enter += new System.EventHandler(this.txtInstructions_Enter);
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSubtitle.ForeColor = System.Drawing.Color.DimGray;
            this.lblSubtitle.Location = new System.Drawing.Point(30, 86);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(433, 19);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "A desktop front-end for the original gta5-modding-utils Python toolkit.";
            // 
            // lblImportant
            // 
            this.lblImportant.AutoSize = true;
            this.lblImportant.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblImportant.ForeColor = System.Drawing.Color.DarkRed;
            this.lblImportant.Location = new System.Drawing.Point(30, 62);
            this.lblImportant.Name = "lblImportant";
            this.lblImportant.Size = new System.Drawing.Size(356, 19);
            this.lblImportant.TabIndex = 1;
            this.lblImportant.Text = "Important: Please read this overview before using it.";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(27, 24);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(320, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "GTA5 Modding Utils – GUI";
            // 
            // btnContinue
            // 
            this.btnContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnContinue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnContinue.FlatAppearance.BorderSize = 0;
            this.btnContinue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnContinue.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnContinue.ForeColor = System.Drawing.Color.White;
            this.btnContinue.Location = new System.Drawing.Point(576, 14);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(96, 32);
            this.btnContinue.TabIndex = 0;
            this.btnContinue.Text = "Continue";
            this.btnContinue.UseVisualStyleBackColor = false;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Location = new System.Drawing.Point(678, 14);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(80, 32);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "Close";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // panelBottom
            // 
            this.panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                                                                            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBottom.BackColor = System.Drawing.Color.Gainsboro;
            this.panelBottom.Controls.Add(this.btnExit);
            this.panelBottom.Controls.Add(this.btnContinue);
            this.panelBottom.Location = new System.Drawing.Point(0, 660);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
            this.panelBottom.Size = new System.Drawing.Size(1024, 60);
            this.panelBottom.TabIndex = 1;
            // 
            // IntroForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1024, 720);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelMain);
            this.MinimumSize = new System.Drawing.Size(1024, 720);
            this.Name = "IntroForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Welcome – GTA5 Modding Utils GUI";
            this.Shown += new System.EventHandler(this.IntroForm_Shown);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblImportant;
        private System.Windows.Forms.RichTextBox txtInstructions;
        private System.Windows.Forms.Button btnContinue;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel panelBottom;
    }
}
