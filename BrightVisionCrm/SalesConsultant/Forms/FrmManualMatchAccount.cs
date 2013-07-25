
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SalesConsultant.PublicProperties;
using BrightVision.Common.UI;
using BrightVision.Common.Business;
using BrightVision.Model;
using BrightVision.Common.Events.Core;
using SalesConsultant.Facade;
using SalesConsultant.Events;

namespace SalesConsultant.Forms
{
    public partial class FrmManualMatchAccount : XtraForm
    {
        #region Constructors
        public FrmManualMatchAccount()
        {
            InitializeComponent();
            tbxShowCount.Text = "10";
        }
        #endregion

        #region Public Properties
        public List<ClassesProperty.ManualMatchAccount> lstMatchAccounts = null;
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;

        private class MatchComparison {
            public string field_name { get; set; }
            public string import_data { get; set; }
            public string master_data { get; set; }
        }

        private Dictionary<string, string> m_lstFields = new Dictionary<string, string>() { 
            {"company_name", "Company Name"},
            {"org_no", "Org No"},
            {"address", "Address"},
            {"city", "City"},
            {"zip_code", "Zip Code"},
            {"country", "Country"},
            {"telephone", "Telephone"},
            {"validated", "Validated"}
        };
        #endregion

        #region Public Methods
        public void RenderMatchAccounts()
        {
            gcAccountsList.BeginUpdate();
            gcAccountsList.DataSource = null;
            gcAccountsList.DataSource = lstMatchAccounts;
            gcAccountsList.EndUpdate();
        }
        #endregion

        #region Private Methods
        private void GetMatchResults()
        {
            if (tbxKeyWord.Text.Length < 1) {
                NotificationDialog.Information("Bright Sales", "Please enter a search keyword.");
                tbxKeyWord.Focus();
                return;
            }
            else if (Convert.ToInt32(tbxShowCount.Text) < 1) {
                NotificationDialog.Information("Bright Sales", "Please enter the max no. of records to show.");
                tbxShowCount.Focus();
                return;
            }
            
            gcPossibleMatches.BeginUpdate();
            gcPossibleMatches.DataSource = null;
            gcPossibleMatches.DataSource = ObjectCompany.GetCompanyListing(tbxKeyWord.Text, Convert.ToInt32(tbxShowCount.Text));
            gvPossibleMatches.BestFitColumns();
            gcPossibleMatches.EndUpdate();
        }
        #endregion

        #region Control Events
        private void gvPossibleMatches_DoubleClick(object sender, EventArgs e)
        {
            if (gvPossibleMatches.RowCount < 1)
                return;

            WaitDialog.Show("Loading match ...");
            ClassesProperty.ManualMatchAccount _ImportData = gvAccountsList.GetFocusedRow() as ClassesProperty.ManualMatchAccount;
            CTCompany _MasterData = gvPossibleMatches.GetFocusedRow() as CTCompany;

            List<MatchComparison> _lstMatchComparison = new List<MatchComparison>();
            _lstMatchComparison.Add(new MatchComparison() { field_name = "Company Name", import_data = _ImportData.import_data_company_name, master_data = _MasterData.company_name });
            _lstMatchComparison.Add(new MatchComparison() { field_name = "Org No", import_data = _ImportData.org_no, master_data = _MasterData.org_no });
            _lstMatchComparison.Add(new MatchComparison() { field_name = "Address", import_data = _ImportData.address, master_data = _MasterData.address });
            _lstMatchComparison.Add(new MatchComparison() { field_name = "City", import_data = _ImportData.city, master_data = _MasterData.city });
            _lstMatchComparison.Add(new MatchComparison() { field_name = "Zip Code", import_data = _ImportData.zip_code, master_data = _MasterData.zip_code });
            _lstMatchComparison.Add(new MatchComparison() { field_name = "Country", import_data = _ImportData.country, master_data = _MasterData.country });
            _lstMatchComparison.Add(new MatchComparison() { field_name = "Telephone", import_data = _ImportData.telephone, master_data = _MasterData.telephone });
            _lstMatchComparison.Add(new MatchComparison() { field_name = "Validated", import_data = string.Empty, master_data = _MasterData.validated ? "Yes" : "No" });

            gcComparison.BeginUpdate();
            gcComparison.DataSource = null;
            gcComparison.DataSource = _lstMatchComparison;
            gcComparison.EndUpdate();
            WaitDialog.Close();
        }
        private void gvComparison_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (gvComparison.RowCount < 1 || gvComparison.GetRowCellValue(e.RowHandle, "field_name") == null)
                return;

            if (gvComparison.GetRowCellValue(e.RowHandle, "field_name").ToString().Equals("Validated"))
                return;
            
            string _ImportData = string.Empty;
            if (gvComparison.GetRowCellValue(e.RowHandle, "import_data") != null)
                _ImportData = gvComparison.GetRowCellValue(e.RowHandle, "import_data").ToString();

            string _MasterData = string.Empty;
            if (gvComparison.GetRowCellValue(e.RowHandle, "master_data") != null)
                _MasterData = gvComparison.GetRowCellValue(e.RowHandle, "master_data").ToString();

            Color _color = Color.LightGreen;
            if (_ImportData != _MasterData || (string.IsNullOrEmpty(_ImportData) && string.IsNullOrEmpty(_MasterData)))
                _color = Color.Coral;

            e.Appearance.BackColor = _color;
        }
        private void tbxKeyWord_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            WaitDialog.Show("Searching ...");
            this.GetMatchResults();
            WaitDialog.Close();
        }
        private void tbxShowCount_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            WaitDialog.Show("Searching ...");
            this.GetMatchResults();
            WaitDialog.Close();
        }
        private void btnClearMatch_Click(object sender, EventArgs e)
        {
            gvAccountsList.BeginUpdate();
            gvAccountsList.SetRowCellValue(gvAccountsList.FocusedRowHandle, "master_data_company_name", string.Empty);
            gvAccountsList.SetRowCellValue(gvAccountsList.FocusedRowHandle, "match_account_id", 0);
            gvAccountsList.SetRowCellValue(gvAccountsList.FocusedRowHandle, "is_match", false);
            gvAccountsList.EndUpdate();
        }
        private void btnMarkAsMatch_Click(object sender, EventArgs e)
        {
            CTCompany _MasterData = gvPossibleMatches.GetFocusedRow() as CTCompany;
            gvAccountsList.BeginUpdate();
            gvAccountsList.SetRowCellValue(gvAccountsList.FocusedRowHandle, "master_data_company_name", _MasterData.company_name);
            gvAccountsList.SetRowCellValue(gvAccountsList.FocusedRowHandle, "match_account_id", _MasterData.id);
            gvAccountsList.SetRowCellValue(gvAccountsList.FocusedRowHandle, "is_match", true);
            gvAccountsList.EndUpdate();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Updating match lists.");
            m_EventBus.Notify(new FrmManualMatchAccountEvents.OnClose() {
                lstMatchAccounts = gvAccountsList.DataSource as List<ClassesProperty.ManualMatchAccount>
            });
            WaitDialog.Close();
            this.Close();
        }
        private void gvAccountsList_DoubleClick(object sender, EventArgs e)
        {
            WaitDialog.Show("Searching ...");
            ClassesProperty.ManualMatchAccount _Account = gvAccountsList.GetFocusedRow() as ClassesProperty.ManualMatchAccount;
            tbxKeyWord.Text = _Account.import_data_company_name;
            this.GetMatchResults();
            WaitDialog.Close();
        }
        #endregion
    }
}