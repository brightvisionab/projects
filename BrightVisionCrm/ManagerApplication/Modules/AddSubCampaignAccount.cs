
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Common.Business;
using BrightVision.Model;
using DevExpress.XtraGrid.Views.Grid;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules
{
    public partial class AddSubCampaignAccount : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructor
        public AddSubCampaignAccount()
        {
            //this.Visible = false;
            InitializeComponent();
            //this.Visible = true;
        }
        #endregion

        #region Public Properties
        public ManageFinalCallList ParentController { get; set; }
        #endregion

        #region Private Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        #region Object Events
        private void gvAccount_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void cmdShowResult_Click(object sender, EventArgs e)
        {
            if (txtSearchKeyword.Text.Length < 1)
            {
                MessageBox.Show("Please enter a search keyword.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSearchKeyword.Focus();
                return;
            }
            else if (Convert.ToInt32(txtRecordToShow.Text) < 1)
            {
                MessageBox.Show("Please enter the max no. of records to show.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtRecordToShow.Focus();
                return;
            }

            try
            {
                WaitDialog.Show(ParentForm, "Loading companies...");
                lblRecordStat.Text = "Records: 0";
                gcAccount.DataSource = null;
                gcAccount.DataSource = ObjectCompany.GetCompanyListing(txtSearchKeyword.Text, Convert.ToInt32(txtRecordToShow.Text));
                gvAccount.BestFitColumns();
                lblRecordStat.Text = "Records: " + gvAccount.RowCount.ToString();
                cbxSelectAll.Checked = true;
               WaitDialog.Close();
            }
            catch { }
        }

        private void cbxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < gvAccount.RowCount; i++)
                gvAccount.SetRowCellValue(i, "select", cbxSelectAll.Checked);
        }

        private void cmdAddToSubCampaign_Click(object sender, EventArgs e)
        {
            List<int> _lstAcctIds = new List<int>();
            for (int i = 0; i < gvAccount.RowCount; i++)
            {
                if (Convert.ToBoolean(gvAccount.GetRowCellValue(i, "select")))
                    _lstAcctIds.Add(Convert.ToInt32(gvAccount.GetRowCellValue(i, "id")));
            }

            if (_lstAcctIds.Count < 1)
            {
                MessageBox.Show("No selected companies to add.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            
            WaitDialog.Show(ParentForm, "Loading...");
            ParentController.AddSubCampaignCompaniesAndContacts(_lstAcctIds);
            WaitDialog.Close();
            MessageBox.Show("Selected companies added to sub-campaign list. Just press the 'Close' button if you have no companies to add.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion
    }
}
