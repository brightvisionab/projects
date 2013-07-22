namespace SalesConsultant.Forms
{
    partial class FrmRelease
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
            this.panContainer = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnRelease = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.btnPreviewReport = new DevExpress.XtraEditors.SimpleButton();
            this.cboReports = new DevExpress.XtraEditors.ComboBoxEdit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboReports.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panContainer
            // 
            this.panContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panContainer.Location = new System.Drawing.Point(12, 7);
            this.panContainer.Name = "panContainer";
            this.panContainer.Size = new System.Drawing.Size(497, 299);
            this.panContainer.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.btnRelease);
            this.panel3.Controls.Add(this.btnCancel);
            this.panel3.Controls.Add(this.btnPreviewReport);
            this.panel3.Controls.Add(this.cboReports);
            this.panel3.Location = new System.Drawing.Point(9, 309);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(500, 31);
            this.panel3.TabIndex = 121;
            // 
            // btnRelease
            // 
            this.btnRelease.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRelease.Location = new System.Drawing.Point(405, 7);
            this.btnRelease.Name = "btnRelease";
            this.btnRelease.Size = new System.Drawing.Size(92, 22);
            this.btnRelease.TabIndex = 120;
            this.btnRelease.Text = "&Release";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(308, 7);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(92, 22);
            this.btnCancel.TabIndex = 119;
            this.btnCancel.Text = "&Cancel";
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(145, 7);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(87, 22);
            this.btnPreviewReport.TabIndex = 118;
            this.btnPreviewReport.Text = "&Preview Report";
            // 
            // cboReports
            // 
            this.cboReports.Location = new System.Drawing.Point(3, 7);
            this.cboReports.Name = "cboReports";
            this.cboReports.Properties.Appearance.BackColor = System.Drawing.SystemColors.Info;
            this.cboReports.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.cboReports.Properties.Appearance.ForeColor = System.Drawing.Color.Maroon;
            this.cboReports.Properties.Appearance.Options.UseBackColor = true;
            this.cboReports.Properties.Appearance.Options.UseFont = true;
            this.cboReports.Properties.Appearance.Options.UseForeColor = true;
            this.cboReports.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboReports.Properties.PopupFormMinSize = new System.Drawing.Size(0, 150);
            this.cboReports.Properties.PopupFormSize = new System.Drawing.Size(0, 150);
            this.cboReports.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboReports.Size = new System.Drawing.Size(136, 20);
            this.cboReports.TabIndex = 117;
            // 
            // FrmRelease
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 348);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panContainer);
            this.MinimizeBox = false;
            this.Name = "FrmRelease";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Send Email";
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboReports.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panContainer;
        private System.Windows.Forms.Panel panel3;
        private DevExpress.XtraEditors.SimpleButton btnRelease;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private DevExpress.XtraEditors.SimpleButton btnPreviewReport;
        private DevExpress.XtraEditors.ComboBoxEdit cboReports;

    }
}