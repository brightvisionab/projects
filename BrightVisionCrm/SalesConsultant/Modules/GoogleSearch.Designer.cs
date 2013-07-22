namespace SalesConsultant.Modules
{
    partial class GoogleSearch
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.wbGoogleSearch = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // wbGoogleSearch
            // 
            this.wbGoogleSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbGoogleSearch.Location = new System.Drawing.Point(0, 0);
            this.wbGoogleSearch.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbGoogleSearch.Name = "wbGoogleSearch";
            this.wbGoogleSearch.ScriptErrorsSuppressed = true;
            this.wbGoogleSearch.Size = new System.Drawing.Size(505, 312);
            this.wbGoogleSearch.TabIndex = 65;
            this.wbGoogleSearch.Url = new System.Uri("", System.UriKind.Relative);
            // 
            // GoogleSearchView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.wbGoogleSearch);
            this.Name = "GoogleSearchView";
            this.Size = new System.Drawing.Size(505, 312);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser wbGoogleSearch;
    }
}
