namespace ManagerApplication.Forms
{
    partial class DemoWindow
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
            this.btnShowProgressBar = new DevExpress.XtraEditors.SimpleButton();
            this.pbProgressTotal = new DevExpress.XtraEditors.ProgressBarControl();
            ((System.ComponentModel.ISupportInitialize)(this.pbProgressTotal.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // btnShowProgressBar
            // 
            this.btnShowProgressBar.Location = new System.Drawing.Point(168, 123);
            this.btnShowProgressBar.Name = "btnShowProgressBar";
            this.btnShowProgressBar.Size = new System.Drawing.Size(161, 23);
            this.btnShowProgressBar.TabIndex = 0;
            this.btnShowProgressBar.Text = "Show Progress bar";
            this.btnShowProgressBar.Click += new System.EventHandler(this.btnShowProgressBar_Click);
            // 
            // pbProgressTotal
            // 
            this.pbProgressTotal.Location = new System.Drawing.Point(76, 163);
            this.pbProgressTotal.Name = "pbProgressTotal";
            this.pbProgressTotal.Size = new System.Drawing.Size(334, 18);
            this.pbProgressTotal.TabIndex = 5;
            // 
            // DemoWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 345);
            this.Controls.Add(this.pbProgressTotal);
            this.Controls.Add(this.btnShowProgressBar);
            this.Name = "DemoWindow";
            this.Text = "DemoWindow";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.pbProgressTotal.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnShowProgressBar;
        private DevExpress.XtraEditors.ProgressBarControl pbProgressTotal;

    }
}