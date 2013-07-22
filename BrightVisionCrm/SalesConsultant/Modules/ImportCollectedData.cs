
#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Model;
using SalesConsultant.Business;
#endregion

namespace SalesConsultant.Modules
{
    public partial class ImportCollectedData : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Properties
        public int ImportFileId { get; set; }
        public int CustomerId { get; set; }
        #endregion

        #region Private Members
        #endregion

        #region Constructors
        public ImportCollectedData()
        {
            InitializeComponent();
        }
        #endregion

        #region Object Events
        private void cmdSaveCollectedData_Click(object sender, EventArgs e)
        {
            if (!chkCustomerOwned.Checked && !chkBrightvisionOwned.Checked)
            {
                MessageBox.Show("Please check at least one.", "Import Collected Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            WaitDialog.Show(ParentForm, "Saving...");
            DataImportUtility.CreateImportListCollectedData(ImportFileId, chkCustomerOwned.Checked, chkBrightvisionOwned.Checked, CustomerId);
            WaitDialog.Close();
            MessageBox.Show("Colllected data saved.", "Import File Collected Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ParentForm.Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            ParentForm.Close();
        }
        #endregion
    }
}
