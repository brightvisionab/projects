
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using BrightVision.Model;
using DevExpress.XtraGrid.Views.Grid;

namespace SalesConsultant.Modules
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

        #region Public Event Handlers
        public delegate void cmdAddToSubCampaignOnClickHandler(object sender, AddSubCampaignAccountEventArgs e);
        public event cmdAddToSubCampaignOnClickHandler cmdAddToSubCampaign_OnClick;
        #endregion

        #region Public Event Args
        public class AddSubCampaignAccountEventArgs : EventArgs
        {
            public List<int> lstAcctIds = null;
            public AddSubCampaignAccountEventArgs(List<int> plstAcctIds)
            {
                lstAcctIds = plstAcctIds;
            }
        }
        #endregion

        #region Public Properties
        //public ManageCompanyContact ParentController { get; set; }
        //public CampaignList CampaignListModule { get; set; }
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
            SalesConsultant.Business.BrightSalesGridUtility.CreateGridContextMenu(view, e);
        }
        private void cmdShowResult_Click(object sender, EventArgs e)
        {
            if (txtSearchKeyword.Text.Length < 1) {
                NotificationDialog.Information("Bright Sales", "Please enter a search keyword.");
                txtSearchKeyword.Focus();
                return;
            }
            else if (Convert.ToInt32(txtRecordToShow.Text) < 1) {
                NotificationDialog.Information("Bright Sales", "Please enter the max no. of records to show.");
                txtRecordToShow.Focus();
                return;
            }

            try {
                WaitDialog.Show("Loading results ...");
                lblRecordStat.Text = "Records: 0";
                gcAccount.DataSource = null;
                gcAccount.DataSource = ObjectCompany.GetCompanyListing(txtSearchKeyword.Text, Convert.ToInt32(txtRecordToShow.Text));
                gvAccount.BestFitColumns();
                lblRecordStat.Text = "Records: " + gvAccount.RowCount.ToString();
                cbxSelectAll.Checked = false;
                this.cbxSelectAll_CheckedChanged(null, null);
                WaitDialog.Close();
            }
            catch { 
            }
        }
        private void cbxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < gvAccount.RowCount; i++)
                gvAccount.SetRowCellValue(i, "select", cbxSelectAll.Checked);
        }
        private void cmdAddToSubCampaign_Click(object sender, EventArgs e)
        {
            List<int> _lstAcctIds = new List<int>();
            for (int i = 0; i < gvAccount.RowCount; i++) {
                if (Convert.ToBoolean(gvAccount.GetRowCellValue(i, "select")))
                    _lstAcctIds.Add(Convert.ToInt32(gvAccount.GetRowCellValue(i, "id")));
            }

            if (_lstAcctIds.Count < 1) {
                NotificationDialog.Information("Bright Sales", "No selected companies to add.");
                return;
            }

            WaitDialog.Show(ParentForm, "Loading data...");
            //ParentController.AddSubCampaignCompaniesAndContacts(_lstAcctIds);
            if (cmdAddToSubCampaign_OnClick != null)
                cmdAddToSubCampaign_OnClick(this, new AddSubCampaignAccountEventArgs(_lstAcctIds));

            WaitDialog.Close();
            NotificationDialog.Information("Bright Sales", "Selected companies added to sub-campaign list. Just press the 'Close' button if you have no companies to add.");
        }
        #endregion
    }
}
