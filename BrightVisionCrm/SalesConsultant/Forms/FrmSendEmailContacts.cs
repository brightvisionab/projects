
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Model;
using BrightVision.Common.Business;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using System.Linq;
using DevExpress.XtraEditors.Controls;
using BrightVision.Common.Events.Core;
using SalesConsultant.Events;
using BrightVision.Common.UI;
using SalesConsultant.Modules;
using BrightVision.Reporting.Utility;
using System.Threading;
using DevExpress.XtraTab;

namespace SalesConsultant.Forms
{
    public partial class FrmSendEmailContacts : DevExpress.XtraEditors.XtraForm
    {
        #region Constructors
        public FrmSendEmailContacts()
        {
            WaitDialog.Show("Loading send email ...");
            InitializeComponent();
        }
        public FrmSendEmailContacts(eCallMode pCallMode = eCallMode.None)
        {
            WaitDialog.Show("Loading send email ...");
            InitializeComponent();
            m_CallMode = pCallMode;

            if (pCallMode == eCallMode.OnSendEmail) {
                btnReleaseAndEmail.Text = "Send Email";
                this.Text = "Preview Report";
                lciReleaseButton.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                lciNote.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            }
        }
        #endregion

        #region Public Properties
        public enum eCallMode {
            OnRelease,
            OnSendEmail,
            None
        }
        #endregion

        #region Private Properties
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private List<int> m_lstSubCampaignIds = null;
        private ViewDisplay m_oPreview = null;
        private bool m_ReleaseButtonPressed = false;
        private bool m_ReleaseEmailButtonPressed = false;
        private ReportsUtility m_Reports = null;
        private eCallMode m_CallMode = eCallMode.None;

        private class ViewCofigData {
            public int id { get; set; }
            public int subcampaign_id { get; set; }
            public string name { get; set; }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void GetContacts()
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                gcContact.DataSource = _efDbContext.FIGetSubCampaignEmailContacts(
                    m_BrightSalesProperty.CommonProperty.SubCampaignId,
                    m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                    m_BrightSalesProperty.CommonProperty.FinalListId
                );
                gvContact.BestFitColumns();
            }
        }
        private void GetReports()
        {
            List<ViewCofigData> _lstData = new List<ViewCofigData>();
            m_lstSubCampaignIds = new List<int>();
            m_lstSubCampaignIds.Add(m_BrightSalesProperty.CommonProperty.SubCampaignId);
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _lstData = this.GetViewConfigInfo(m_lstSubCampaignIds.ToArray());
            }
            if (_lstData.Count > 0) {
                cboReports.Properties.DataSource = _lstData;
                cboReports.Properties.DisplayMember = "name";
                cboReports.Properties.ValueMember = "id";
                cboReports.Properties.Columns.Add(new LookUpColumnInfo("name"));
                cboReports.ItemIndex = 0;
            }
        }
        private List<ViewCofigData> GetViewConfigInfo(int[] pSubCampaignIds)
        {
            List<ViewCofigData> listViewConfig = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                if (pSubCampaignIds.Length <= 0) return null;
                listViewConfig = _efDbContext.view_configuration.Where(i =>
                    pSubCampaignIds.Contains(i.subcampaign_id) &&
                    i.MGC == false && 
                    i.report_layout_config != null
                ).Select(x =>
                    new ViewCofigData { id = x.id, name = x.name }
                ).ToList();
            }
            return listViewConfig;
        }
        #endregion
        
        #region Control Events
        private void FrmSendEmailContacts_Load(object sender, EventArgs e)
        {
            this.GetContacts();
            this.GetReports();
            WaitDialog.Close();
        }
        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(cboReports.EditValue) < 1) {
                NotificationDialog.Information("Bright Sales", "Please select a report to preview.");
                return;
            }

            WaitDialog.Show("Sending web service request ...");
            List<int> _SubCampaignIds = new List<int>();
            _SubCampaignIds.Add(m_BrightSalesProperty.CommonProperty.SubCampaignId);

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efDbContext.FIClearUserReuests(UserSession.CurrentUser.UserId);
                Guid _RequestId = Guid.NewGuid();
                serverside_report_requests _eftRequest = new serverside_report_requests() {
                    id = _RequestId,
                    calling_environment = (short)ReportsUtility.eCallingEnvironment.BrightSales_SendEmail,
                    display_mode = (short)ReportsUtility.eDisplayMode.AccountsContacts_WithDialogData,
                    campaign_info = string.Format("{0}>{1}>{2}",
                        m_BrightSalesProperty.CommonProperty.CustomerName,
                        m_BrightSalesProperty.CommonProperty.CampaignName,
                        m_BrightSalesProperty.CommonProperty.SubCampaignName
                    ),
                    sub_campaign_ids = string.Join(",", _SubCampaignIds),
                    view_config_id = Convert.ToInt32(cboReports.EditValue),
                    account_id = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                    requested_by = UserSession.CurrentUser.UserId,
                    requested_on = DateTime.Now
                };
                _efDbContext.serverside_report_requests.AddObject(_eftRequest);
                _efDbContext.SaveChanges();
                _efDbContext.Detach(_eftRequest);
                ReportsUtility.SendReportRequest(_RequestId.ToString());
            }
            WaitDialog.Close();

            //this.Cursor = Cursors.WaitCursor;
            //WaitDialog.Show("Loading report preview ...");
            //m_Reports = new ReportsUtility()
            //{
            //    CampaignInfo = string.Format("{0}>{1}>{2}",
            //        m_BrightSalesProperty.CommonProperty.CustomerName,
            //        m_BrightSalesProperty.CommonProperty.CampaignName,
            //        m_BrightSalesProperty.CommonProperty.SubCampaignName
            //    ),
            //    LSubCampaignData = new List<ReportsUtility.SubcampaignData>(),
            //    CallingEnvironment = ReportsUtility.eCallingEnvironment.BrightSales_SendEmail,
            //    DisplayMode = ReportsUtility.eDisplayMode.AccountsContacts_WithDialogData,
            //    AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId
            //};
            //List<int> _SubCampaignIds = new List<int>();
            //_SubCampaignIds.Add(m_BrightSalesProperty.CommonProperty.SubCampaignId);
            //m_Reports.OnReportPageCompleted += m_Reports_OnReportPageCompleted;
            //XtraTabControl _DummyTab = new XtraTabControl();
            //m_Reports.GenerateReportPages(ref _DummyTab, _SubCampaignIds.ToArray(), Convert.ToInt32(cboReports.EditValue));
        }
        private void m_Reports_OnReportPageCompleted(object sender, EventArgs e)
        {
            m_Reports.OnReportPageCompleted -= m_Reports_OnReportPageCompleted;
            m_Reports.ReportPagePreview();
            this.Cursor = Cursors.Default;
            WaitDialog.Close();
        }
        private void btnRelease_Click(object sender, EventArgs e)
        {
            /**
             * [@jeff 05.16.2013]
             * commented for ticket: https://brightvision.jira.com/browse/PLATFORM-2908
             */
            WaitDialog.Show("Saving campaign booking ...");
            m_EventBus.Notify(new FrmReleaseEvents.SaveCampaignBooking());
            WaitDialog.Close();
            m_ReleaseButtonPressed = true;
            this.Close();
        }
        private void FrmSendEmailContacts_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_ReleaseButtonPressed)
                this.DialogResult = System.Windows.Forms.DialogResult.Ignore;

            else if (m_ReleaseEmailButtonPressed)
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;

            else
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
        private void btnReleaseAndEmail_Click(object sender, EventArgs e)
        {
            /**
             * release and send email.
             */
            if (m_CallMode == eCallMode.OnRelease) {
                WaitDialog.Show("Saving campaign booking ...");
                m_EventBus.Notify(new FrmReleaseEvents.SaveCampaignBooking());
                WaitDialog.Close();
                m_ReleaseEmailButtonPressed = true;
                this.Close();
            }

            /**
             * send email only.
             */
            else if (m_CallMode == eCallMode.OnSendEmail) {
                m_ReleaseEmailButtonPressed = true;
                this.Close();
            }
        }
        #endregion
    }
}