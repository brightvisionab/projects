
#region Namespace
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
#endregion

namespace SalesConsultant.Forms
{
    public partial class PopupDialog : DevExpress.XtraEditors.XtraForm
    {
        #region Public Members
        public bool IsLogin = false;
        #endregion

        #region Constructors
        public PopupDialog()
        {
            InitializeComponent();
            this.KeyPreview = true;
        }
        public PopupDialog(UserControl control, string title = "")
        {
            InitializeComponent();
            control.Dock = DockStyle.Fill;
            this.ClientSize = control.ClientSize;
            this.Padding = new System.Windows.Forms.Padding(0);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = title;
            this.Controls.Add(control);
           
            this.Load += new EventHandler(PopupDialog_Load);
            this.Shown += new EventHandler(PopupDialog_Shown);
        }
        #endregion

        #region Form Control Events
        
        void PopupDialog_Shown(object sender, EventArgs e)
        {
            WaitDialog.Close();
        }

        void PopupDialog_Load(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Loading components...");
        }

        public void PopupDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (e.CloseReason == CloseReason.ApplicationExitCall)
            //    Application.Exit();
            if (e.CloseReason == CloseReason.UserClosing)
                return;
            if (IsLogin) {
                DialogResult objResult = new DialogResult();
                objResult = MessageBox.Show("Are you sure to terminate the sales consultant application?", "Sales Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (objResult == DialogResult.Yes) {
                    IsLogin = false;
                    Application.Exit();
                }
                else
                    e.Cancel = true;
            }

            if (e.CloseReason != CloseReason.ApplicationExitCall && this.DialogResult == DialogResult.None)
                e.Cancel = true;
        }
        #endregion

        #region Keyboard Shortcuts

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (FormUtility.ShortCutKeysHandled(keyData))
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
    }
}