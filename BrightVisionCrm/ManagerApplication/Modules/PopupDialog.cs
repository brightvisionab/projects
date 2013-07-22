using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using BrightVision.Common.Utilities;
using BrightVision.Common.Business;

namespace ManagerApplication.Modules
{
    public partial class PopupDialog : Form
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
        public PopupDialog(UserControl control, string title="") {
            control.Dock = DockStyle.Fill;
            this.Controls.Add(control);
            this.ClientSize = control.ClientSize;
            this.Padding = new System.Windows.Forms.Padding(0);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = title;
            InitializeComponent();
            this.Load += new EventHandler(PopupDialog_Load);
            this.Shown += new EventHandler(PopupDialog_Shown);
        }

        void PopupDialog_Shown(object sender, EventArgs e)
        {
            WaitDialog.Close();
        }

        void PopupDialog_Load(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Loading components...");
        }
        #endregion

        #region Form Control Events
        private void PopupDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsLogin)
            {
                DialogResult objResult = new DialogResult();
                objResult = MessageBox.Show("Are you sure to terminate the manager application?", "Manager Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (objResult == DialogResult.Yes)
                {
                    IsLogin = false;
                    Application.Exit();
                }
                else
                    e.Cancel = true;
            }
        }
        public void Show(Form form) {
            this.ShowDialog(form);
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
