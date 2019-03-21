namespace EQEmu_Patcher
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.txtList = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.splashLogo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.splashLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // txtList
            // 
            this.txtList.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtList.HideSelection = false;
            this.txtList.Location = new System.Drawing.Point(0, 0);
            this.txtList.Multiline = true;
            this.txtList.Name = "txtList";
            this.txtList.ReadOnly = true;
            this.txtList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtList.Size = new System.Drawing.Size(512, 512);
            this.txtList.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnStart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue;
            this.btnStart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Location = new System.Drawing.Point(-2, 511);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(512, 90);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Patch";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // splashLogo
            // 
            this.splashLogo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.splashLogo.Image = global::EQEmu_Patcher.Properties.Resources.eqemupatcher;
            this.splashLogo.Location = new System.Drawing.Point(0, 0);
            this.splashLogo.Margin = new System.Windows.Forms.Padding(0);
            this.splashLogo.MinimumSize = new System.Drawing.Size(400, 450);
            this.splashLogo.Name = "splashLogo";
            this.splashLogo.Size = new System.Drawing.Size(512, 512);
            this.splashLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.splashLogo.TabIndex = 4;
            this.splashLogo.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(507, 596);
            this.Controls.Add(this.txtList);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.splashLogo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(305, 371);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EQEmu Patcher";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.splashLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtList;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.PictureBox splashLogo;
    }
}

