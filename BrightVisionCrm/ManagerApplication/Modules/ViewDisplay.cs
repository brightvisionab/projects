
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraLayout;
using DevExpress.XtraTab;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraReports.UI;

using BrightVision.Model;
using BrightVision.DQControl.Business;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Reporting.UI;

using ManagerApplication.Business;
using BrightVision.Reporting;
using BrightVision.Reporting.Utility;
using BrightVision.Reporting.Template;
using BrightVision.DQControl.Utilities;

namespace ManagerApplication.Modules 
{
    public partial class ViewDisplay : XtraUserControl 
    {
        #region Constructors
        public ViewDisplay()
        {
            this.Visible = false;
            InitializeComponent();
            this.SetControls();
            m_ViewConfigId = 0;
            this.Visible = true;
        }
        public ViewDisplay(int pViewConfigId)
        {
            this.Visible = false;
            InitializeComponent();
            this.SetControls();
            m_ViewConfigId = pViewConfigId;
            this.Visible = true;
        }
        #endregion

        #region Private Properties
        private BrightPlatformEntities BPContext = null;
        private List<ReportsUtility.SubcampaignData> listSubcampaignData = null;
        private BackgroundWorker worker = null;

        private view_configuration m_efoViewConfig = null;
        private int m_ViewConfigId = 0;
        #endregion

        #region Public Methods
        public void AutoLoadReport()
        {
            subcampaign _efoSubCampaign = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                m_efoViewConfig = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_ViewConfigId);
                _efoSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == m_efoViewConfig.subcampaign_id);
                _efDbContext.Detach(m_efoViewConfig);
            }
            lookUpEditCustomerCampaign.EditValue = _efoSubCampaign.campaign_id;
            for (int i = 0; i < ccbeSubcampaign.Properties.Items.Count; i++) {
                if (ccbeSubcampaign.Properties.Items[i].Description.Equals(_efoSubCampaign.title)) {
                    ccbeSubcampaign.Properties.Items[i].CheckState = CheckState.Checked;
                    break;
                }
            }
            cboDisplayMode.SelectedIndex = 0;
            this.btnLoad.PerformClick();
            m_ViewConfigId = 0;
        }
        #endregion

        #region Private Methods
        private void SetControls()
        {
            this.layoutControl1.AllowCustomizationMenu = false;
            DevExpress.Utils.ImageCollection stateImages = new DevExpress.Utils.ImageCollection();
            stateImages.ImageSize = new System.Drawing.Size(16, 16);
            stateImages.AddImage(Properties.Resources.loader);
            tcgView.Images = stateImages;

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            ccbeSubcampaign.Properties.ReadOnly = true;
            this.GetCampaigns();

            bbiExportXLSX.Enabled = true;
            bbiExportXLS.Enabled = true;
            bbiExportCSV.Enabled = true;
        }
        private void GetCampaigns()
        {
            WaitDialog.Show("Loading campaign list ...");
            this.ClearPages();
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                List<CTCustomerCampaign> listCCS = _efDbContext.FIGetCustomerCampaign(UserSession.CurrentUser.UserId).ToList();
                if (listCCS != null && listCCS.Count > 0) {                
                    lookUpEditCustomerCampaign.Properties.Columns.Clear();
                    lookUpEditCustomerCampaign.Properties.DataSource = listCCS;
                    lookUpEditCustomerCampaign.Properties.DisplayMember = "title";
                    lookUpEditCustomerCampaign.Properties.ValueMember = "campaign_id";
                    lookUpEditCustomerCampaign.Properties.Columns.Add(new LookUpColumnInfo("title"));
                    lookUpEditCustomerCampaign.Properties.ShowHeader = false;

                    if (listCCS.Count >= 20) 
                        lookUpEditCustomerCampaign.Properties.DropDownRows = 30;

                    lookUpEditCustomerCampaign.Properties.PopupWidth = 600;                
                    ccbeSubcampaign.Properties.DataSource = null;
                    ccbeSubcampaign.Properties.PopupControl = null;
                    ccbeSubcampaign.SetEditValue(null);
                    ccbeSubcampaign.RefreshEditValue();
                }
            }
            WaitDialog.Close();
        }
        private void GetCampaignViews()
        {
            WaitDialog.Show("Loading campaign views ...");
            this.ClearPages();
            var editVal = lookUpEditCustomerCampaign.EditValue;
            if (editVal != null) {
                int campaign_id = (int) editVal;
                bool isActive = checkEditActive.Checked;
                bool isArchived = checkEditArchived.Checked;
                bool isOnHold = checkEditOnHold.Checked;
                List<string> statuses = new List<string>();                                
                
                if(isArchived) 
                    statuses.Add("Archived");
                if(isActive)
                    statuses.Add("Active");
                if(isOnHold)
                    statuses.Add("On Hold");
                
                if(campaign_id > 0) {
                    listSubcampaignData = BPContext.subcampaigns
                        .Where(x => x.campaign_id == campaign_id && statuses.Contains(x.status))
                        .Select(x => new ReportsUtility.SubcampaignData { id = x.id, title = x.title }).ToList();

                    if (listSubcampaignData != null && listSubcampaignData.Count > 0) {
                        ccbeSubcampaign.Properties.DataSource = listSubcampaignData;
                        ccbeSubcampaign.Properties.DisplayMember = "title";
                        ccbeSubcampaign.Properties.ValueMember = "id";
                        ccbeSubcampaign.Properties.ReadOnly = false;                        
                    } 
                    else {
                        ccbeSubcampaign.Properties.DataSource = null;
                        ccbeSubcampaign.SetEditValue(null);
                        ccbeSubcampaign.Properties.ReadOnly = true;                       
                    }

                    ccbeSubcampaign.Properties.PopupControl = null;
                    ccbeSubcampaign.RefreshEditValue();
                }
                if (ccbeSubcampaign.EditValue == null || string.IsNullOrEmpty(ccbeSubcampaign.EditValue.ToString())) 
                    btnLoad.Enabled = false;
            }
            WaitDialog.Close();
        }
        private void GenerateReportPages(int[] pSubCampaignIds, int pViewConfigId = 0)
        {
            ReportsUtility _Reports = new ReportsUtility() {
                CampaignInfo = string.Format("{0}{1}", lookUpEditCustomerCampaign.Text, ccbeSubcampaign.Text),
                LSubCampaignData = ccbeSubcampaign.Properties.DataSource as List<ReportsUtility.SubcampaignData>,
                CallingEnvironment = ReportsUtility.eCallingEnvironment.BrightManager_ViewDisplay,
                CallingApplication = ReportsUtility.eCallingApplication.BrightManager
            };

            if (cboDisplayMode.Text.Equals("Accounts & contacts having dialog data"))
                _Reports.DisplayMode = ReportsUtility.eDisplayMode.AccountsContacts_WithDialogData;
            else if (cboDisplayMode.Text.Equals("Accounts & contacts that have made call attempts"))
                _Reports.DisplayMode = ReportsUtility.eDisplayMode.AccountsContacts_WithCallAttempts;

            _Reports.GenerateReportPages(ref tcgView, pSubCampaignIds, pViewConfigId);
        }
        private void ClearPages()
        {
            if (tcgView.TabPages.Count > 0)
                tcgView.TabPages.Clear();
        }
        #endregion

        #region Control Events
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bwAsync = sender as BackgroundWorker;
            if (bwAsync.CancellationPending)
                e.Cancel = true;
        }
        private void lookUpEditCustomerCampaign_EditValueChanged(object sender, EventArgs e)
        {
            this.GetCampaignViews();
        }
        private void ccbeSubcampaign_EditValueChanged(object sender, EventArgs e)
        {
            this.ClearPages();
            if (ccbeSubcampaign.EditValue == null || string.IsNullOrEmpty(ccbeSubcampaign.EditValue.ToString())) {
                btnLoad.Enabled = false;
                cboDisplayMode.Enabled = false;
            }
            else {
                btnLoad.Enabled = true;
                cboDisplayMode.Enabled = true;
            }
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy) {
                worker.CancelAsync();
                return;
            }

            WaitDialog.Show("Generating report pages ...");
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                _efDbContext.FIUpdateContactTitles();
            }
            if (m_ViewConfigId < 1) {
                string val = (string)ccbeSubcampaign.EditValue;
                if (!string.IsNullOrEmpty(val)) {
                    List<int> subcampaign_ids = new List<int>();
                    var strVals = val.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    strVals.ForEach(delegate(string x) {
                        subcampaign_ids.Add(int.Parse(x));
                    });
                    this.ClearPages();
                    this.GenerateReportPages(subcampaign_ids.ToArray());
                    worker.RunWorkerAsync();
                }
            }
            else {                
                List<int> subcampaign_ids = new List<int>();
                subcampaign_ids.Add(m_efoViewConfig.subcampaign_id);
                this.ClearPages();
                this.GenerateReportPages(subcampaign_ids.ToArray(), m_efoViewConfig.id);
                worker.RunWorkerAsync();
            }
            WaitDialog.Close();
        }
        private void checkEditActive_CheckedChanged(object sender, EventArgs e)
        {
            this.GetCampaignViews();
        }
        private void checkEditArchived_CheckedChanged(object sender, EventArgs e)
        {
            this.GetCampaignViews();
        }
        private void checkEditOnHold_CheckedChanged(object sender, EventArgs e)
        {
            this.GetCampaignViews();
        }
        private void bbiExportCSV_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try {
                if (tcgView.SelectedTabPage != null) {
                    var oView = tcgView.SelectedTabPage.Tag as ReportsUtility.ReportPage;
                    if (oView != null)
                        oView.Export(ViewExportType.CSV);
                }
                else
                    BrightVision.Common.UI.NotificationDialog.Information("Bright Manager", "No available report page.");
            }
            catch {
                BrightVision.Common.UI.NotificationDialog.Information("Bright Manager", "Report page not yet loaded or not available.");
            }

        }
        private void bbiExportXLS_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try {
                if (tcgView.SelectedTabPage != null) {
                    var oView = tcgView.SelectedTabPage.Tag as ReportsUtility.ReportPage;
                    if (oView != null)
                        oView.Export(ViewExportType.Excel2003);
                }
                else
                    BrightVision.Common.UI.NotificationDialog.Information("Bright Manager", "No available report page.");
            }
            catch {
                BrightVision.Common.UI.NotificationDialog.Information("Bright Manager", "Report page not yet loaded or not available.");
            }
        }
        private void bbiExportXLSX_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try {
                if (tcgView.SelectedTabPage != null) {
                    var oView = tcgView.SelectedTabPage.Tag as ReportsUtility.ReportPage;
                    if (oView != null)
                        oView.Export(ViewExportType.Excel2007);
                }
                else
                    BrightVision.Common.UI.NotificationDialog.Information("Bright Manager", "No available report page.");
            }
            catch {
                BrightVision.Common.UI.NotificationDialog.Information("Bright Manager", "Report page not yet loaded or not available.");
            }
        }
        #endregion
    }
}
