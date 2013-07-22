
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace SalesConsultant.Modules
{
    public partial class CreateBlankView : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public CreateBlankView(bool pIsNew, string pViewName)
        {
            InitializeComponent();
            tbxViewName.Text = pViewName;
            m_IsNew = pIsNew;
        }
        public CreateBlankView(bool pIsNew)
        {
            InitializeComponent();
            m_IsNew = pIsNew;
        }
        public CreateBlankView()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Events & Args
        public delegate void AfterSaveEventHandler(AfterSaveArgs e);
        public event AfterSaveEventHandler AfterSave;
        public class AfterSaveArgs : EventArgs {
            public string ViewName { get; set; }
            public bool IsNew { get; set; }
        }
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private bool m_IsNew = true;
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        #region Control Events
        private void btnCancel_Click(object sender, EventArgs e)
        {
            ParentForm.Close();
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (tbxViewName.Text.Length < 1) {
                MessageBox.Show("Please enter a view name.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (AfterSave != null)
                AfterSave(new AfterSaveArgs() {
                    ViewName = tbxViewName.Text,
                    IsNew = m_IsNew
                });

            ParentForm.Close();
        }
        private void tbxViewName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnSave.PerformClick();
        }
        #endregion
    }
}
