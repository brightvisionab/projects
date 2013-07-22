
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Data.Objects;
using System.IO;
using System.Threading;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using BrightVision.Common.Events.Core;

using SalesConsultant.Business;
using SalesConsultant.Utils;
using SalesConsultant.Forms;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using SalesConsultant.Events;

namespace SalesConsultant.Modules 
{
    public partial class EventFollowUpLog : DevExpress.XtraEditors.XtraUserControl 
    {
        #region Enumeration
        enum CheckEditType {
            Done,
            UserTaken
        }
        #endregion

        #region Member Variables
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private List<CTScEventAndFollowUpLog> m_EventFollowUpList = null;
        private CTScEventAndFollowUpLog m_EventFollowUp = null;
        private CheckEditType m_eCheckEditType { get; set; }
        private bool m_IsChecked = false;
        private bool m_RemarksChanged = false;

        private sub_campaign_account_remarks m_efeCompanyRemark = null;
        private string m_AudioFileUrl = string.Empty;

        private CallLogPlayer _player = null;
        private CTScEventAndFollowUpLog prevEventFollowUp;
        private List<CTScEventAndFollowUpLog> eventFollowUpGetEventFollowUpLogs;
        private string m_OriginalRemarks = string.Empty;
        private bool m_RemarksSaved = true;
        #endregion

        #region Constructor
        public EventFollowUpLog() 
        {
            InitializeComponent();
            
            m_EventFollowUpList = new List<CTScEventAndFollowUpLog>();
            axWindowsMediaPlayer.enableContextMenu = false;
            lciAudioControl.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            this.Load += new EventHandler(EventFollowUpLog_Load);

            m_Timer.Stop();
            m_Timer.Enabled = false;
            m_Timer.Interval = 1000;
            m_Timer.Tick += new EventHandler(m_Timer_Tick);
        }
        #endregion

        #region Public Events & Args
        //public delegate bool UserOnCallOrHasPendingCallLogEventHandler();
        //public event UserOnCallOrHasPendingCallLogEventHandler UserOnCallOrHasPendingCallLog;

        //public delegate void SelectContactOnClickEventHandler(SelectContactArgs e);
        //public event SelectContactOnClickEventHandler SelectContact_OnClick;
        //public class SelectContactArgs : EventArgs {
        //    public int ContactId { get; set; }
        //}

        //public delegate void EventLogOnSaveEventHandler(FollowUpEditor.btnSaveOnClickArgs e);
        //public event EventLogOnSaveEventHandler EventLog_OnSave;

        //public delegate bool CanWorkOnCompanyEventHandler();
        //public event CanWorkOnCompanyEventHandler CanWorkOnCompany;

        //public delegate void WorkNurtureEventEventHandler(MyFollowUps.CampaignBookingArgs e);
        //public event WorkNurtureEventEventHandler WorkNurtureEvent;
        #endregion

        #region Properties
        public bool ReloadData = false;
        public bool AllowSaving = false;
        public int SubCampaignId { get; set; }
        public int AccountId { get; set; }
        public int FinalListId { get; set; }


        public bool RemarksSaved {
            get {
                if (!m_OriginalRemarks.Equals(tbxCompanyRemarks.Text) && !m_RemarksSaved)
                    return false;

                return true;
            }
        }
        public string CompanyRemarks {
            get { return tbxCompanyRemarks.Text; }
        }

        private string PrevCompanyRemarks = "";
        private System.Windows.Forms.Timer m_Timer = new System.Windows.Forms.Timer();
        private int m_SecondElapse = 5;
        #endregion 

        #region Public Methods
        public void UpdateCompanyRemarks()
        {
            this.LoadCompanyRemarks();
        }
        public void SetState()
        {
            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse) {
                tbxCompanyRemarks.Properties.ReadOnly = true;
                gvEventLog.OptionsBehavior.Editable = false;
                //this.Enabled = false;
            }
            else {
                tbxCompanyRemarks.Properties.ReadOnly = false;
                gvEventLog.OptionsBehavior.Editable = true;
                //this.Enabled = true;
            }
        }
        public void InitializeView() 
        {
            PopulateAssignToComboList();
            PopulateEventFollowUpLogView();
            this.LoadCompanyRemarks();
            m_RemarksChanged = false;
        }        
        public void PopulateEventFollowUpLogView(int eventId = 0)
        {
            
            if (eventFollowUpGetEventFollowUpLogs != null)
                eventFollowUpGetEventFollowUpLogs.Clear();

            eventFollowUpGetEventFollowUpLogs = EventFollowUp.GetEventFollowUpLogs(SubCampaignId, AccountId).OrderByDescending(i => i.date_of_transaction).ToList();
            gridColumn10.DisplayFormat.Format = new GridColumnCustomDateFormat();
            gridColumn10.DisplayFormat.FormatString = "yyyy-MM-dd HH:mm";
            gridColumn10.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            gcEventLog.DataSource = null;
            gvEventLog.ActiveFilterString = string.Empty;
            gcEventLog.DataSource = eventFollowUpGetEventFollowUpLogs;
            if (!AllowSaving) {
                gvEventLog.OptionsBehavior.Editable = false;
                return;
            }
            gvEventLog.OptionsBehavior.Editable = true;
            gvEventLog.BestFitColumns();
            gvEventLog.Columns["done"].Width = 32;
            gvEventLog.Columns["audio_id"].Width = 22;
            //gvEventLog.Columns["short_message"].Width = 22;

            if (eventId > 0) {
                for (int i = 0; i < gvEventLog.RowCount; i++) {
                    CTScEventAndFollowUpLog _item = gvEventLog.GetRow(i) as CTScEventAndFollowUpLog;
                    if (_item.id == eventId) {
                        gvEventLog.FocusedRowHandle = i;
                        break;
                    }
                }
            }

            gvEventLog.ActiveFilterString = "[hidden] = 'False'";
            //gvEventLog.Columns["is_saved"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo("[Saved] = 'True'");
            //if (eventId > 0) { 
            //    gvEventLog.SelectRow("id", eventId);
            //}
        }
        public void Show(int pSubCampaignId, int pAccountId)
        {
            this.RunAssync(() => {
                bool _ReloadTabPage = false;
                if (pSubCampaignId != SubCampaignId) {
                    SubCampaignId = pSubCampaignId;
                    _ReloadTabPage = true;
                }

                if (pAccountId != AccountId) {
                    AccountId = pAccountId;
                    _ReloadTabPage = true;
                }

                /**
                 * reload data indicates that this has been forced loaded
                 * even though there were no parameter value changes.
                 */
                if (_ReloadTabPage || ReloadData) {
                    layoutControlItem1.TextVisible = false;
                    this.PopulateAssignToComboList();
                    this.PopulateEventFollowUpLogView();
                    ReloadData = false;
                }
            });
        }
        public void Delete(int pCallLogId)
        {
            if (gvEventLog.RowCount < 1)
                return;

            for (int i = 0; i < gvEventLog.RowCount; i++) {
                CTScEventAndFollowUpLog _item = gvEventLog.GetRow(i) as CTScEventAndFollowUpLog;
                if (_item.id == pCallLogId) {
                    gvEventLog.DeleteRow(i);
                    break;
                }
            }
            //this.PopulateAssignToComboList();
            //this.PopulateEventFollowUpLogView();
        }
        public void HideAdditionalPanels()
        {
            lciMemoPanel.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            lciAudioControl.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
        }
        public void UnHideAll()
        {
            bool bolHasHidden = false;
            gvEventLog.ActiveFilterString = string.Empty;

            if (gvEventLog.RowCount < 1)
                return;

            for (int x = 0; x < gvEventLog.RowCount; x++)
            {
                CTScEventAndFollowUpLog _item = gvEventLog.GetRow(x) as CTScEventAndFollowUpLog;

                if (_item == null) continue;

                event_followup_log _log;
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
                {
                    _log = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _item.id);
                    if (_log == null)
                        return;
                    else if (_log.is_saved == null || _log.is_saved == false)
                    {
                        bolHasHidden = true;
                        //CheckEdit _control = sender as CheckEdit;
                        _log.is_saved = true;
                        _efDbContext.event_followup_log.ApplyCurrentValues(_log);
                        _efDbContext.SaveChanges();
                        _efDbContext.Detach(_log);
                    }
                }
            }
            if (bolHasHidden)
            {
                WaitDialog.Show("Loading event logs ...");
                this.PopulateEventFollowUpLogView();
                WaitDialog.Close();
            }

        }
        #endregion

        #region Private Methods
        private void LoadCompanyRemarks()
        {
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                tbxCompanyRemarks.Text = "";
                PrevCompanyRemarks = "";
                m_efeCompanyRemark = _efDbModel.sub_campaign_account_remarks.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId && i.account_id == AccountId);

                if (m_efeCompanyRemark == null) {
                    m_efeCompanyRemark = new sub_campaign_account_remarks() {
                        sub_campaign_id = SubCampaignId,
                        account_id = AccountId
                    };
                    _efDbModel.sub_campaign_account_remarks.AddObject(m_efeCompanyRemark);
                    _efDbModel.SaveChanges();
                }
                else {
                    if (m_efeCompanyRemark.remarks != null && m_efeCompanyRemark.remarks.Length > 0)
                        tbxCompanyRemarks.Text = m_efeCompanyRemark.remarks;
                }

                PrevCompanyRemarks = tbxCompanyRemarks.Text;
                _efDbModel.Detach(m_efeCompanyRemark);
            }
            m_OriginalRemarks = tbxCompanyRemarks.Text;
            m_RemarksSaved = true;
        }
        private void PopulateAssignToComboList() 
        {
            try {
                cboAssignTo.Columns.Clear();
                cboAssignTo.DataSource = null;
                cboAssignTo.DataSource = SalesScript.GetUsers(FinalListId, true);
                cboAssignTo.DisplayMember = "name";
                cboAssignTo.ValueMember = "id";
                cboAssignTo.Columns.Add(new LookUpColumnInfo("name"));
            } catch { 
            }
        }
        private bool IsEventTypeCallog() 
        {
            m_EventFollowUp = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            if (m_EventFollowUp == null)
                return false;

            if (m_EventFollowUp.event_type.Equals("Call Log"))
                return true;

            return false;
        }
        //private void QueueUpdatedEventFollowUp() 
        //{
        //    this.SetSelectedCheckedValue();
        //    m_EventFollowUpList.Clear();
        //    m_EventFollowUpList.Add(m_EventFollowUp);
        //    this.SaveUpdatedEventFollowUps();
        //}
        //private void SaveUpdatedEventFollowUps() 
        //{
        //    WaitDialog.Show(ParentForm, "Saving...");
        //    EventFollowUp.Save(m_EventFollowUpList);
        //    //PopulateEventFollowUpLogView();

        //    //if (OnEventFollowUpLogSaved != null)
        //    //    OnEventFollowUpLogSaved(m_EventFollowUp, new EventArgs());
            
        //    WaitDialog.Close();
        //}
        private void SetSelectedCheckedValue() 
        {
            if (m_EventFollowUp == null)
                return;

            switch (m_eCheckEditType) {
                case CheckEditType.Done:
                    m_EventFollowUp.done = m_IsChecked;
                    break;

                case CheckEditType.UserTaken:
                    m_EventFollowUp.user_taken = m_IsChecked;
                    break;
            }
        }
        private void GetProperties(bool pForWorkModePurpose)
        {
            if (gvEventLog.RowCount < 1)
                return;

            CTScEventAndFollowUpLog _item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            if (_item == null || !_item.event_type.Equals("Nurture Log"))
                return;

            /**
             * get sub-campaign status lists
             */
            string _XmlData;
            event_followup_log _efoLog;
            account _eftAccount;
            subcampaign _eftSubCampaign;
            campaign _eftCampaign;
            customer _eftCustomer;
            final_lists _eftFinalList;
            sub_campaign_account_appointments _eftAccountAppointment;
            contact _eftContact;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efoLog = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _item.id);
                _eftSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == _efoLog.subcampaign_id);
                _eftCampaign = _efDbContext.campaigns.FirstOrDefault(i => i.id == _eftSubCampaign.campaign_id);
                _eftCustomer = _efDbContext.customers.FirstOrDefault(i => i.id == _eftCampaign.customer_id);
                _eftAccount = _efDbContext.accounts.FirstOrDefault(i => i.id == _efoLog.account_id);
                _eftFinalList = _efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == _eftSubCampaign.id);
                _efDbContext.Detach(_efoLog);
                _efDbContext.Detach(_eftAccount);
                _efDbContext.Detach(_eftSubCampaign);
                _efDbContext.Detach(_eftCampaign);
                _efDbContext.Detach(_eftCustomer);
                _efDbContext.Detach(_eftFinalList);

                _eftContact = _efDbContext.contacts.FirstOrDefault(i => i.id == _efoLog.contact_id);
                if (_eftContact != null)
                    _efDbContext.Detach(_eftContact);

                _eftAccountAppointment = _efDbContext.sub_campaign_account_appointments.FirstOrDefault(i => i.final_list_id == _eftFinalList.id && i.account_id == _eftAccount.id);
                if (_eftAccountAppointment != null)
                    _efDbContext.Detach(_eftAccountAppointment);

                _XmlData = _eftSubCampaign.xml_config_data;
                //_efDbContext.Detach(_efoLog);
            }

            //int _ItemSelectedIndex = 0;
            //int _NotQualifiedIndex = -1;
            //int _SendEmailIndex = -1;

            List<XmlUtility.SubCampaignConfig> _lstAccountLeadStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_lead_status_dropdown");
            //List<string> _lstAccountLeadStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_lead_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
            //int _AccountLeadStatusSelectedIndex = _ItemSelectedIndex;
            //int _AccountLeadStatusNotQualifiedIndex = _NotQualifiedIndex;
            List<XmlUtility.SubCampaignConfig> _lstAccountStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_status_dropdown");
            //List<string> _lstAccountStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
            //int _AccountStatusSelectedIndex = _ItemSelectedIndex;
            //int _AccountStatusNotQualifiedIndex = _NotQualifiedIndex;
            //int _AccountStatusSendEmailIndex = _SendEmailIndex;
            List<XmlUtility.SubCampaignConfig> _lstContactStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/contact/contact_status_dropdown");
            //List<string> _lstContactStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/contact/contact_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
            //int _ContactStatusSelectedIndex = _ItemSelectedIndex;
            //int _ContactStatusNotQualifiedIndex = _NotQualifiedIndex;

            m_BrightSalesProperty.CommonProperty.ContactId = _efoLog.contact_id == null ? 0 : Convert.ToInt32(_efoLog.contact_id);
            m_BrightSalesProperty.CommonProperty.CompanyName = _eftAccount.company_name;
            m_BrightSalesProperty.CommonProperty.CustomerId = _eftCustomer.id;
            m_BrightSalesProperty.CommonProperty.CampaignId = _eftCampaign.id;
            m_BrightSalesProperty.CommonProperty.SubCampaignId = Convert.ToInt32(_efoLog.subcampaign_id);
            m_BrightSalesProperty.CommonProperty.AccountId = Convert.ToInt32(_efoLog.account_id);
            m_BrightSalesProperty.CommonProperty.FinalListId = _eftFinalList.id;

            if (pForWorkModePurpose) {
                sub_campaign_account_lists _eftAccountList;
                sub_campaign_account_appointments _eftAccountAppt;
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    /**
                     * get company locked property.
                     */
                    _eftAccountList = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                        i.account_id == m_BrightSalesProperty.CommonProperty.AccountId &&
                        i.final_list_id == m_BrightSalesProperty.CommonProperty.FinalListId
                    );
                    m_BrightSalesProperty.CommonProperty.CompanyLocked = false;
                    if (_eftAccountList.locked && _eftAccountList.locked_by != UserSession.CurrentUser.UserId)
                        m_BrightSalesProperty.CommonProperty.CompanyLocked = true;

                    /**
                     * get company status and lead status indexes for dropdown.
                     */
                    _eftAccountAppt = _efDbContext.sub_campaign_account_appointments.FirstOrDefault(i =>
                        i.account_id == m_BrightSalesProperty.CommonProperty.AccountId &&
                        i.final_list_id == m_BrightSalesProperty.CommonProperty.FinalListId
                    );

                    //_AccountStatusSelectedIndex = 0;
                    //_AccountLeadStatusSelectedIndex = 0;

                    if (_eftAccountAppt != null) {
                        int _idx = _lstAccountStatuses.FindIndex(i => i.status.Equals(_eftAccountAppt.status));
                        if (_idx != null && _idx > 0) {
                            for (int i = 0; i < _lstAccountStatuses.Count; i++)
                                _lstAccountStatuses[i].selected = false;
                            _lstAccountStatuses[_idx].selected = true;
                            //_AccountStatusSelectedIndex = _idx;
                        }

                        _idx = _lstAccountLeadStatuses.FindIndex(i => i.status.Equals(_eftAccountAppt.lead_status));
                        if (_idx != null && _idx > 0) {
                            for (int i = 0; i < _lstAccountLeadStatuses.Count; i++)
                                _lstAccountLeadStatuses[i].selected = false;
                            _lstAccountLeadStatuses[_idx].selected = true;
                            //_AccountLeadStatusSelectedIndex = _idx;
                        }

                        _efDbContext.Detach(_eftAccountAppt);
                    }

                    /**
                     * get contact/dialog status index for dropdown.
                     */
                    sub_campaign_contact_appointments _eftContactAppt = _efDbContext.sub_campaign_contact_appointments.FirstOrDefault(i =>
                        i.contact_id == m_BrightSalesProperty.CommonProperty.ContactId &&
                        i.final_list_id == m_BrightSalesProperty.CommonProperty.FinalListId
                    );

                    //_ContactStatusSelectedIndex = 0;
                    if (_eftContactAppt != null) {
                        int _idx = _lstContactStatuses.FindIndex(i => i.status.Equals(_eftContactAppt.status));
                        if (_idx != null && _idx > 0) {
                            for (int i = 0; i < _lstContactStatuses.Count; i++)
                                _lstContactStatuses[i].selected = false;
                            _lstContactStatuses[_idx].selected = true;
                            //_ContactStatusSelectedIndex = _idx;
                        }

                        _efDbContext.Detach(_eftContactAppt);
                    }

                    _efDbContext.Detach(_eftAccountList);
                }

                m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId = Convert.ToInt32(_eftAccount.id);
                m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId = _eftContact.id == null ? 0 : Convert.ToInt32(_eftContact.id);
                m_BrightSalesProperty.CampaignBooking.Appointment = new CampaignBookingProperty.AppointmentProperty() {
                    CompanyWebsite = _eftAccount.www,
                    CompanyAppointmentStatus = _eftAccountAppt.status,
                    CompanyBoardNumber = _eftAccount.telephone,
                    CompanyAppointmentLeadStatus = _eftAccountAppt.lead_status
                };

                m_BrightSalesProperty.CampaignBooking.AccountStatus = new CampaignBookingProperty.AccountStatusProperty() {
                    AccountLeadStatuses = _lstAccountLeadStatuses,
                    AccountStatuses = _lstAccountStatuses,
                    //AccountLeadStatusSelectedIndex = _AccountLeadStatusSelectedIndex,
                    //AccountStatusSelectedIndex = _AccountStatusSelectedIndex,
                    //AccountLeadStatusNotQualifiedIndex = _AccountLeadStatusNotQualifiedIndex,
                    //AccountStatusNotQualifiedIndex = _AccountStatusNotQualifiedIndex,
                    //AccountStatusSendEmailIndex = _AccountStatusSendEmailIndex
                };
                m_BrightSalesProperty.CampaignBooking.ContactStatus = new CampaignBookingProperty.ContactStatusProperty() {
                    ContactStatuses = _lstContactStatuses
                };
                //m_BrightSalesProperty.CampaignBooking.ContactStatus = new CampaignBookingProperty.ContactStatusProperty() {
                //    ContactStatuses = _lstContactStatuses,
                //    ContactStatusSelectedIndex = _ContactStatusSelectedIndex,
                //    ContactStatusNotQualifiedIndex = _ContactStatusNotQualifiedIndex
                //};
                string _LastUpdateInfo = ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(_eftFinalList.id, m_BrightSalesProperty.CommonProperty.AccountId);
                m_BrightSalesProperty.CampaignBooking.BreadCrumb = string.Format("{0}{1}{2}{3}",
                    string.IsNullOrEmpty(_eftAccount.company_name) ? "" : _eftAccount.company_name,
                    string.IsNullOrEmpty(_eftAccount.city) ? "" : ", " + _eftAccount.city,
                    string.IsNullOrEmpty(_eftAccount.country) ? "" : ", " + _eftAccount.country,
                    string.IsNullOrEmpty(_LastUpdateInfo) ? string.Empty : string.Format(" ({0})", _LastUpdateInfo)
                );
                m_BrightSalesProperty.CampaignBooking.CampaignInformation = string.Format("{0}, {1}>{2}>{3}",
                    _eftAccount.company_name,
                    _eftCustomer.customer_name,
                    _eftCampaign.campaign_name,
                    _eftSubCampaign.title
                );
                m_BrightSalesProperty.CampaignBooking.ToolTipInformation = string.Format("Customer: {5}{0}Campaign: {6}{0}SubCampaign: {7}{0}{0}Date: {1:yyyy-MM-dd}{0}Assigned User: {2}{0}Company Name: {3}{0}Contact: {8}{0}{0}Remarks: {4}",
                    Environment.NewLine,
                    (DateTime)_efoLog.date_of_transaction,
                    _efoLog.assigned_user,
                    _eftAccount.company_name,
                    _efoLog.short_message,
                    _eftCustomer.customer_name,
                    _eftCampaign.campaign_name,
                    _eftSubCampaign.title,
                    _efoLog.contact_name
                );
            }
        }
        private void SuspendEventCellValueChange(bool pSuspendEvent)
        {
            if (pSuspendEvent)
                this.gvEventLog.CellValueChanged -= new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gvEventLog_CellValueChanged);
            else
                this.gvEventLog.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gvEventLog_CellValueChanged);
        }
        private void RunAssync(Action pAction)
        {
            if (!IsHandleCreated)
                CreateHandle();

            this.Invoke(pAction);
        }
        #endregion

        #region Control Events
        private void EventFollowUpLog_Load(object sender, EventArgs e)
        {
            this.ParentForm.FormClosing += new FormClosingEventHandler(ParentForm_FormClosing);
        }
        private void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (axWindowsMediaPlayer != null) {
                axWindowsMediaPlayer.URL = "";
                axWindowsMediaPlayer.Ctlcontrols.stop();
                axWindowsMediaPlayer.Parent = null;
                axWindowsMediaPlayer = null;
                lciAudioControl.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            }

            //if (e.CloseReason == CloseReason.ApplicationExitCall)
            //    return;

            //try
            //{
            //    if (axWindowsMediaPlayer != null)
            //    {
            //        axWindowsMediaPlayer.URL = "";
            //        axWindowsMediaPlayer.Ctlcontrols.stop();
            //        axWindowsMediaPlayer.Parent = null;
            //        axWindowsMediaPlayer = null;
            //    }
            //}
            //catch { }
        }
        private void cbxUserTaken_EditValueChanging(object sender, ChangingEventArgs e) 
        {
            if (IsEventTypeCallog())
                e.Cancel = true;
        }
        private void cbxUserTaken_CheckedChanged(object sender, EventArgs e) 
        {
            if (IsEventTypeCallog())
                return;

            WaitDialog.Show("Saving changes ...");
            m_eCheckEditType = CheckEditType.UserTaken;
            m_IsChecked = (sender as CheckEdit).Checked;

            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var objEventFollowUp = objDbModel.event_followup_log.FirstOrDefault(i => i.id == m_EventFollowUp.id);
                if (objEventFollowUp == null)
                    return;

                objEventFollowUp.assigned_user = Convert.ToBoolean(m_EventFollowUp.user_taken) ? UserSession.CurrentUser.UserId : 0;
                objDbModel.SaveChanges();
                objDbModel.Detach(objEventFollowUp);
            }
            this.SetSelectedCheckedValue();
            WaitDialog.Close();

            //this.QueueUpdatedEventFollowUp();
        }
        private void cbxDone_EditValueChanging(object sender, ChangingEventArgs e) 
        {
            if (IsEventTypeCallog())
                e.Cancel = true;

            CTScEventAndFollowUpLog _item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            if (_item == null || _item.event_type.Equals("Call Log") || _item.event_type.Equals("Nurture Log"))
                e.Cancel = true;

            //if (_item.event_type.Equals("Nurture Log") || _item.event_type.Equals("Nurture Event"))
            //if (_item.event_type.Equals("Call Log") || _item.event_type.Equals("Nurture Log"))
            //    e.Cancel = true;
        }
        private void cbxDone_CheckedChanged(object sender, EventArgs e) 
        {
            if (IsEventTypeCallog())
                return;

            WaitDialog.Show("Saving changes ...");
            m_eCheckEditType = CheckEditType.Done;
            m_IsChecked = (sender as CheckEdit).Checked;

            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var objEventFollowUp = objDbModel.event_followup_log.FirstOrDefault(i => i.id == m_EventFollowUp.id);
                if (objEventFollowUp == null)
                    return;

                objEventFollowUp.done = (sender as CheckEdit).Checked;
                objDbModel.SaveChanges();
                objDbModel.Detach(objEventFollowUp);
            }
            this.SetSelectedCheckedValue();
            WaitDialog.Close();

            //this.QueueUpdatedEventFollowUp();
        }
        private void gvEventLog_CellValueChanged(object sender, CellValueChangedEventArgs e) 
        {
            if (!IsEventTypeCallog()) {
                WaitDialog.Show("Saving changes ...");
                using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    var objEventFollowUp = objDbModel.event_followup_log.FirstOrDefault(i => i.id == m_EventFollowUp.id);
                    if (objEventFollowUp == null)
                        return;

                    objEventFollowUp.date_of_transaction = m_EventFollowUp.date_of_transaction;
                    objEventFollowUp.start_time = Convert.ToDateTime(m_EventFollowUp.start_time).TimeOfDay;
                    objEventFollowUp.end_time = Convert.ToDateTime(m_EventFollowUp.end_time).TimeOfDay;
                    objEventFollowUp.title = m_EventFollowUp.title;
                    objEventFollowUp.event_type = m_EventFollowUp.event_type;
                    objEventFollowUp.short_message = m_EventFollowUp.short_message;
                    objDbModel.event_followup_log.ApplyCurrentValues(objEventFollowUp);
                    objDbModel.SaveChanges();
                    objDbModel.Detach(objEventFollowUp);
                }
                //this.QueueUpdatedEventFollowUp();
                WaitDialog.Close();
                return;
            }

            if (e.Column.FieldName.Equals("short_message")) {
                WaitDialog.Show("Saving changes ...");
                var _item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
                if (_item != null) {
                    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                        var _log = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _item.id);
                        if (_log != null) {
                            _log.short_message = _item.short_message;
                            _efDbContext.event_followup_log.ApplyCurrentValues(_log);
                            _efDbContext.SaveChanges();
                            _efDbContext.Detach(_log);
                        }
                    }
                }
                WaitDialog.Close();
            }

            //this.QueueUpdatedEventFollowUp();
        }
        private void gvEventLog_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e) 
        {
            //m_EventFollowUp = (CTScEventAndFollowUpLog)gvEventLog.GetFocusedRow();
            //if (m_EventFollowUp != null) {
            //    if (m_EventFollowUp.audio_id != null && m_EventFollowUp.audio_id != Guid.Empty) {
            //        try
            //        {
            //            CommonApplicationData commonFolder = new CommonApplicationData("BrightVision", "BrightSales");
            //            m_AudioFileUrl = string.Format(@"{0}\tmpwav\{1}.wav", commonFolder.ApplicationFolderPath, m_EventFollowUp.audio_id);
            //            //axWindowsMediaPlayer.URL = string.Format(@"{0}\{1}.wav", commonFolder.ApplicationFolderPath, m_EventFollowUp.audio_id);
            //            //axWindowsMediaPlayer.Ctlcontrols.stop();
            //            //lciAudioControl.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            //        }
            //        catch { }
            //    }
            //    else {
            //        m_AudioFileUrl = string.Empty;
            //        //axWindowsMediaPlayer.URL = "";
            //        //axWindowsMediaPlayer.Ctlcontrols.stop();
            //        //lciAudioControl.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            //    }
            //}
            //else {
            //    m_AudioFileUrl = string.Empty;
            //    //axWindowsMediaPlayer.URL = "";
            //    //axWindowsMediaPlayer.Ctlcontrols.stop();
            //    //lciAudioControl.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            //}
        }
        private void gvEventLog_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e) 
        {
            CTScEventAndFollowUpLog _item = gvEventLog.GetRow(e.RowHandle) as CTScEventAndFollowUpLog;
            if (e == null || e.Value == null)
                return;

            if (e.Column.FieldName.Equals("date_of_transaction") && _item != null) {
                if (_item.event_type.Equals("Call Log"))
                    e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyyy-MM-dd HH:mm");
                else
                    e.DisplayText = string.Format("{0:yyyy-MM-dd} {1:HH:mm}", e.Value, _item.start_time);
            }

            if (e.Column.FieldName.Equals("created_on"))
                e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyyy-MM-dd HH:mm");

            if (e.Column.FieldName.Equals("call_length") && _item != null) {
                TimeSpan _StartTime = Convert.ToDateTime(_item.start_time).TimeOfDay;
                TimeSpan _EndTime = Convert.ToDateTime(_item.end_time).TimeOfDay;
                TimeSpan _CallDuration = _EndTime - _StartTime;
                e.DisplayText = _CallDuration.TotalSeconds > 0 ? 
                    string.Format("{0}:{1}", _CallDuration.Minutes, _CallDuration.Seconds) : 
                    "0:0"; 
            }

            /**
             * captioned as type, but the event_type resides still but hidden.
             * we did not fully remove the event_type field because we may need this on purpose.
             */
            if (_item != null && e.Column.FieldName.Equals("event_status")) {
                if (!string.IsNullOrEmpty(_item.event_status)) {
                    if (_item.event_status.Equals("Successfull"))
                        e.DisplayText = "Completed";
                    else if (_item.event_status.Equals("Call Diverted To") || _item.event_status.Equals("Call Referal To"))
                        e.DisplayText = "Call Forwarding";
                    else if (_item.event_status.Equals("Don't Have Time") || _item.event_status.Equals("Contact not found"))
                        e.DisplayText = "Other";
                    else if (_item.event_status.Equals("Tech Problem"))
                        e.DisplayText = "Remove from List";
                }

                if (!string.IsNullOrEmpty(_item.event_type)) {
                    if (_item.event_type.Equals("Nurture Log"))
                        e.DisplayText = "Nurture Log";
                    else if (_item.event_type.Equals("Nurture Event"))
                        e.DisplayText = "Nurture Event";
                }
            }
        }
        private void gvEventLog_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) 
        {
            GridView view = sender as GridView;
            SalesConsultant.Business.BrightSalesGridUtility.CreateGridContextMenu(view, e, true, new SalesConsultant.Business.BrightSalesGridUtility.AdditionalMenuItems() {
                LoadNurtureEvent = true,
                EditEvent = true,
                SelectContact = true,
                RefreshGrid = true
            });

            SalesConsultant.Business.BrightSalesGridUtility.SelectContactOnClick -= new SalesConsultant.Business.BrightSalesGridUtility.SelectContactOnClickEventHandler(GridUtilityMenu_SelectContact_OnClick);
            SalesConsultant.Business.BrightSalesGridUtility.SelectContactOnClick += new SalesConsultant.Business.BrightSalesGridUtility.SelectContactOnClickEventHandler(GridUtilityMenu_SelectContact_OnClick);
            SalesConsultant.Business.BrightSalesGridUtility.EditEventOnClick -= new SalesConsultant.Business.BrightSalesGridUtility.EditEventOnClickEventHandler(GridUtilityMenu_EditEventOnClick);
            SalesConsultant.Business.BrightSalesGridUtility.EditEventOnClick += new SalesConsultant.Business.BrightSalesGridUtility.EditEventOnClickEventHandler(GridUtilityMenu_EditEventOnClick);
            SalesConsultant.Business.BrightSalesGridUtility.LoadNurtureEventOnClick -= new SalesConsultant.Business.BrightSalesGridUtility.LoadNurtureEventOnClickEventHandler(GridUtilityMenu_LoadNurtureEventOnClick);
            SalesConsultant.Business.BrightSalesGridUtility.LoadNurtureEventOnClick += new SalesConsultant.Business.BrightSalesGridUtility.LoadNurtureEventOnClickEventHandler(GridUtilityMenu_LoadNurtureEventOnClick);
            SalesConsultant.Business.BrightSalesGridUtility.RefreshOnClick -= new SalesConsultant.Business.BrightSalesGridUtility.RefreshClickEventHandler(GridUtilityMenu_RefreshOnClick);
            SalesConsultant.Business.BrightSalesGridUtility.RefreshOnClick += new SalesConsultant.Business.BrightSalesGridUtility.RefreshClickEventHandler(GridUtilityMenu_RefreshOnClick);

            //DAN: Disable selection on context menu if no data on grid because it has no use if no data to be like Exported...
            SalesConsultant.Business.BrightSalesGridUtility.DisableGridContextMenu(view, e);
        }
        private void GridUtilityMenu_RefreshOnClick()
        {
            SalesConsultant.Business.BrightSalesGridUtility.RefreshOnClick -= new SalesConsultant.Business.BrightSalesGridUtility.RefreshClickEventHandler(GridUtilityMenu_RefreshOnClick);
            WaitDialog.Show("Refreshing ...");
            CTScEventAndFollowUpLog _item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            this.PopulateEventFollowUpLogView(_item.id);
            WaitDialog.Close();
            SalesConsultant.Business.BrightSalesGridUtility.RefreshOnClick += new SalesConsultant.Business.BrightSalesGridUtility.RefreshClickEventHandler(GridUtilityMenu_RefreshOnClick);
        }
        private void gvEventLog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            if (gvEventLog.RowCount < 1)
                return;

            var Item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            if (Item.event_type.Equals("Call Log"))
                return;

            if (Item.assigned_user != UserSession.CurrentUser.UserId) {
                NotificationDialog.Information("Bright Sales", "You do not own this entry. You cannot delete this call log / follow-up.");
                return;
            }

            DialogResult _dlg = MessageBox.Show("Are you sure to delete this call log / follow-up?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlg == DialogResult.No)
                return;

            WaitDialog.Show("Deleting ...");
            EventFollowUp.Delete(Item.id);
            gvEventLog.DeleteRow(gvEventLog.FocusedRowHandle);

            m_EventBus.Notify(new EventFollowUpLogEvents.AfterDelete() {
                DeletedCallLogId = Item.id
            });

            //if (AfterDelete != null)
            //    AfterDelete(this, new AfterDeleteArgs() { 
            //        DeletedId = Item.id 
            //    });

            WaitDialog.Close();
        }
        private void tbxCompanyRemarks_EditValueChanged(object sender, EventArgs e)
        {
            m_RemarksChanged = true;
            m_RemarksSaved = false;
        }

        private void tbxCompanyRemarks_EditValueChanging(object sender, ChangingEventArgs e)
        {
            if (PrevCompanyRemarks != tbxCompanyRemarks.Text)
            {
                m_SecondElapse = 5;
                m_Timer.Enabled = true;
                m_Timer.Start();
            }
            else
            {
                m_Timer.Enabled = false;
                m_Timer.Stop();
                m_SecondElapse = 5;
            }
        }

        void m_Timer_Tick(object sender, EventArgs e)
        {
            if (m_SecondElapse == 0)
            {
                m_Timer.Enabled = false;
                m_Timer.Stop();
                m_SecondElapse = 5;
                m_RemarksChanged = true;
                m_RemarksSaved = false;
                tbxCompanyRemarks_Leave(null, null);
                
            }
            m_SecondElapse -= 1;
        }

        private void tbxCompanyRemarks_Leave(object sender, EventArgs e)
        {
            if (!m_RemarksChanged)
                return;

            this.RunAssync(() => {
                BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
                m_efeCompanyRemark = _efDbModel.sub_campaign_account_remarks.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId && i.account_id == AccountId);
                m_efeCompanyRemark.remarks = tbxCompanyRemarks.Text;
                _efDbModel.sub_campaign_account_remarks.ApplyCurrentValues(m_efeCompanyRemark);
                _efDbModel.SaveChanges();

                PrevCompanyRemarks = tbxCompanyRemarks.Text;

                m_EventBus.Notify(new EventFollowUpLogEvents.CompanyRemarksSaved.ManageCampaignBooking() {
                    CompanyRemarks = tbxCompanyRemarks.Text
                });
                m_EventBus.Notify(new EventFollowUpLogEvents.CompanyRemarksSaved.FrmSalesConsultant() {
                    CompanyRemarks = tbxCompanyRemarks.Text
                });

                //if (CompanyRemark_Saved != null)
                //    CompanyRemark_Saved(this, new CompanyRemarkSavedArgs() {
                //        CompanyRemarks = tbxCompanyRemarks.Text,
                //        SubCampaignId = SubCampaignId,
                //        AccountId = AccountId,
                //        FinalListId = FinalListId
                //    });
            });

            m_RemarksSaved = true;
            m_RemarksChanged = false;

            m_Timer.Stop();
            m_Timer.Enabled = false;
            m_SecondElapse = 5;
        }
        private void gvEventLog_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            //if (e.Column.FieldName == "azure_blob_audio_id") {
            //    CTScEventAndFollowUpLog _Row = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            //    if (_Row == null) {
            //        e.RepositoryItem = ribePlayHidden;
            //        return;
            //    }

            //    if (!string.IsNullOrEmpty(_Row.azure_blob_audio_id) && Convert.ToBoolean(_Row.is_azure_blob))
            //        e.RepositoryItem = ribePlay;
            //    else
            //        e.RepositoryItem = ribePlayHidden;
            //}

            if (e.Column.FieldName == "audio_id") {
                CTScEventAndFollowUpLog _Row = gvEventLog.GetRow(e.RowHandle) as CTScEventAndFollowUpLog;
                if (_Row == null) {
                    e.RepositoryItem = ribePlayHidden;
                    return;
                }

                if (_Row.start_time == _Row.end_time)
                {
                    e.RepositoryItem = ribePlayHidden;
                    return;
                }

                /**
                 * if azure blob.
                 */
                else if (Convert.ToBoolean(_Row.is_azure_blob)) {
                    //if (!string.IsNullOrEmpty(_Row.azure_blob_audio_id))
                        e.RepositoryItem = ribePlay;
                    //else
                    //    e.RepositoryItem = ribePlayHidden;
                }

                /**
                 * if old audio.
                 */
                else {
                    if (_Row.audio_id == null) {
                        e.RepositoryItem = ribePlayHidden;
                            return;
                    }

                    if (string.IsNullOrEmpty(_Row.audio_id.ToString())) {
                        e.RepositoryItem = ribePlayHidden;
                        return;
                    }

                    Guid _AudioId = (Guid)_Row.audio_id;
                    if (_AudioId == Guid.Empty) {
                        e.RepositoryItem = ribePlayHidden;
                        return;
                    }

                    e.RepositoryItem = ribePlay;

                    //if (e.CellValue == null) {
                    //    e.RepositoryItem = ribePlayHidden;
                    //    return;
                    //}

                    //var audioId = (Guid)e.CellValue;
                    //if (audioId == Guid.Empty) {
                    //    e.RepositoryItem = ribePlayHidden;
                    //    return;
                    //}

                    ////If audio exist in the tmpwav show play
                    //CommonApplicationData commonFolder = new CommonApplicationData("BrightVision", "BrightSales");
                    ////string filePathTmpWav = String.Format(@"{0}\tmpwav\{1}_.wav", commonFolder.ApplicationFolderPath, audioId);
                    //string filePathCachWav = String.Format(@"{0}\cachewav\{1}_.wav", commonFolder.ApplicationFolderPath, audioId);
                    //if (File.Exists(filePathCachWav)) {
                    //    e.RepositoryItem = ribePlay;
                    //    return;
                    //}

                    //var source = gcEventLog.DataSource as List<CTScEventAndFollowUpLog>;
                    //var followup = source.FirstOrDefault(param => param.audio_id == audioId);
                    //if (followup != null) {
                    //    if (followup.main_uploaded.HasValue && followup.main_uploaded.Value)
                    //        e.RepositoryItem = ribePlay;
                    //    else
                    //        e.RepositoryItem = ribePlayHidden;
                    //}
                    //else
                    //    e.RepositoryItem = ribePlayHidden;
                }
            }
            else if (e.Column.FieldName == "done") {
                if (gvEventLog.GetRowCellValue(e.RowHandle, "event_type") != null) {
                    bool _DisplayCheckBox = false;
                    if (gvEventLog.GetRowCellValue(e.RowHandle, "event_type").Equals("Nurture Event") ||
                        gvEventLog.GetRowCellValue(e.RowHandle, "event_type").Equals("Make Call") ||
                        gvEventLog.GetRowCellValue(e.RowHandle, "event_type").Equals("Send Mail") ||
                        gvEventLog.GetRowCellValue(e.RowHandle, "event_type").Equals("Todo"))
                        _DisplayCheckBox = true;                        

                    if (!_DisplayCheckBox)
                        e.RepositoryItem = new DevExpress.XtraEditors.Repository.RepositoryItem();
                }
            }
        }
        private void gvEventLog_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e == null || e.Column.Tag == null)
                return;
            
            if (e.Column.Tag.Equals("assigned_to")) {
                CTScEventAndFollowUpLog _item = gvEventLog.GetRow(e.RowHandle) as CTScEventAndFollowUpLog;
                if (_item != null && _item.assigned_user != null) {
                    if (_item.assigned_user == UserSession.CurrentUser.UserId)
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.assigned_to_me, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.assigned_user == 0)
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.assigned_to_team, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.assigned_user != UserSession.CurrentUser.UserId && _item.assigned_user != 0 && _item.assigned_user != null)
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.assigned_to_other, e.Bounds.X, e.Bounds.Y, 16, 16);
                    e.Handled = true;
                }
            }
            else if (e.Column.Tag.Equals("call_method")) {
                CTScEventAndFollowUpLog _item = gvEventLog.GetRow(e.RowHandle) as CTScEventAndFollowUpLog;
                if (_item != null && !string.IsNullOrEmpty(_item.call_method)) {
                    if (_item.call_method.Equals("Call Board"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.call_board_2, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.call_method.Equals("Call Direct"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.call_direct, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.call_method.Equals("Call Mobile"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.call_mobile, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.call_method.Equals("Call Anonymous") || !string.IsNullOrEmpty(_item.contact_no))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.call_anonymous, e.Bounds.X, e.Bounds.Y, 16, 16);
                    e.Handled = true;
                }
                else {
                    if (_item != null && !string.IsNullOrEmpty(_item.contact_no)) {
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.call_anonymous, e.Bounds.X, e.Bounds.Y, 16, 16);
                        e.Handled = true;
                    }
                }
            }
            else if (e.Column.Tag.Equals("event_status_icon")) {
                CTScEventAndFollowUpLog _item = gvEventLog.GetRow(e.RowHandle) as CTScEventAndFollowUpLog;
                if (_item != null && !string.IsNullOrEmpty(_item.event_status)) {
                    if (_item.event_status.Equals("Successfull"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.completed, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.event_status.Equals("No Answer"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.no_answer, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.event_status.Equals("Busy Signal"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.busy_signal, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.event_status.Equals("Call Diverted To") || _item.event_status.Equals("Call Referal To"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.call_refered_to, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.event_status.Equals("Don't Have Time") || _item.event_status.Equals("Contact not found"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.flag_purple, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.event_type.Equals("Nurture Log"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.nurture_log, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.event_type.Equals("Nurture Event"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.nurture, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.event_type.Equals("Todo"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.todo, e.Bounds.X, e.Bounds.Y, 16, 16);
                    else if (_item.event_type.Equals("Make Call"))
                        e.Graphics.DrawImage(global::SalesConsultant.Properties.Resources.make_call, e.Bounds.X, e.Bounds.Y, 16, 16);
                }
            }
        }
        private void gvEventLog_ShowingEditor(object sender, CancelEventArgs e)
        {
            CTScEventAndFollowUpLog _item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            if (_item == null)
                return;

            if (gvEventLog.FocusedColumn.FieldName.Equals("short_message"))
                if (_item.event_type.Equals("Nurture Log") || _item.event_type.Equals("Nurture Event"))
                    e.Cancel = true;
        }
        private void cbxIsHidden_CheckedChanged(object sender, EventArgs e)
        {
            CTScEventAndFollowUpLog _item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            if (_item == null)
                return;

            event_followup_log _log;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _log = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _item.id);
                if (_log == null)
                    return;
                
                CheckEdit _control = sender as CheckEdit;
                _log.is_saved = !_control.Checked;
                _efDbContext.event_followup_log.ApplyCurrentValues(_log);
                _efDbContext.SaveChanges();
                _efDbContext.Detach(_log);

                WaitDialog.Show("Loading event logs ...");
                this.PopulateEventFollowUpLogView();
                WaitDialog.Close();
            }           
        }
        private void ribePlay_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if (m_BrightSalesProperty.CommonProperty.OnCallMode || !m_BrightSalesProperty.CommonProperty.CallLogSaved) {
                NotificationDialog.Information("Bright Sales", "A call is currently in progress or a pending call log needs to be saved first.");
                return;
            }

            /**
             * if audio_id is not null, then its an old audio file.
             * else, load the azure_blob_audio_id.
             */
            string _FileUrl = string.Empty;
            CTScEventAndFollowUpLog _row = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            event_followup_log _eftCallLog = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftCallLog = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _row.id);
                if (_eftCallLog != null)
                    _efDbContext.Detach(_eftCallLog);
            }

            if (_eftCallLog.is_azure_blob != null && _eftCallLog.is_azure_blob == true)
            {
                if (_eftCallLog.main_uploaded == null)
                {
                    NotificationDialog.Error("Bright Sales", "File is not yet uploaded\nPlease try again in a minute", _eftCallLog.id);
                    return;
                }
            }

            string audioId = "";
            bool IsNew = true;
            if (!string.IsNullOrEmpty(_eftCallLog.azure_blob_audio_id))
            {
                _FileUrl = string.Format("{0}/{1}.mp3", ConfigManager.AppSettings["AzureBlobStorageNewAudioUrl"].ToString(), _eftCallLog.azure_blob_audio_id.ToString());
                _row.azure_blob_audio_id = _eftCallLog.azure_blob_audio_id;
                _row.audio_id = null;
                audioId = _eftCallLog.azure_blob_audio_id;
            }
            else
            {
                _FileUrl = string.Format("{0}/{1}_.mp3", ConfigManager.AppSettings["AzureBlobStorageOldAudioUrl"].ToString(), _eftCallLog.audio_id.ToString());
                audioId = _eftCallLog.audio_id.ToString();
                IsNew = false;
            }
                
            //if (!string.IsNullOrEmpty(_eftCallLog.audio_id.ToString())) {
            //    _FileUrl = "https://lii.blob.core.windows.net/old/0a4b23f7-eab8-4043-ad84-36b4b5d23f1e_.mp3";
            //    //_FileUrl = string.Format("{0}/{1}_.mp3", ConfigManager.AppSettings["AzureBlobStorageOldAudioUrl"].ToString(), _eftCallLog.audio_id.ToString());
            //}
            //else
            //    _FileUrl = string.Format("{0}/{1}.mp3", ConfigManager.AppSettings["AzureBlobStorageNewAudioUrl"].ToString(), _eftCallLog.azure_blob_audio_id.ToString());

            WaitDialog.Show("Loading audio stream ...");
            CallLogPlayer _Player = new CallLogPlayer(_eftCallLog.id, audioId, _FileUrl, true, IsNew);
            if (!_Player.IsDisposed && _Player.CanBePlayed) {
                WaitDialog.Close();
                _Player.Show(this);
            }
            else
                WaitDialog.Close();

            //BrightVision.VlcMediaPlayer.UI.FrmVlcMediaPlayer _Player = new BrightVision.VlcMediaPlayer.UI.FrmVlcMediaPlayer("https://lii.blob.core.windows.net/old/0a4b23f7-eab8-4043-ad84-36b4b5d23f1e_.mp3");
            //_Player.ShowDialog(this.ParentForm);

            /** /
            Guid audioId = Guid.Parse(gvEventLog.GetFocusedRowCellValue("audio_id").ToString());
            var commonFolder = new CommonApplicationData("BrightVision", "BrightSales");
            //string filePathTmpWav = String.Format(@"{0}\tmpwav\{1}_.wav", commonFolder.ApplicationFolderPath, audioId);
            string filePathCachWav = String.Format(@"{0}\cachewav\{1}_.wav", commonFolder.ApplicationFolderPath, audioId);
            string filePathCachWav2 = String.Format(@"{0}\cachewav\{1}_.mp3", commonFolder.ApplicationFolderPath, audioId);
            object objMainUploaded = gvEventLog.GetFocusedRowCellValue("main_uploaded");
            bool mainUploaded = false;
            //bool isTmWavExist = File.Exists(filePathTmpWav);
            bool isCacheWavExist = File.Exists(filePathCachWav);
            bool isCacheWav2Exist = File.Exists(filePathCachWav2);
            if (objMainUploaded != null)
                mainUploaded = bool.Parse(objMainUploaded.ToString());
           
            if (isCacheWavExist) 
                PlayAudio(filePathCachWav);
            else if (isCacheWav2Exist)                
                PlayAudio(filePathCachWav2);
            else if (mainUploaded) {
                if (_player != null) {
                    _player = null;
                }
                WaitDialog.Show("Downloading audio files....");
                Guid audioID = Guid.Parse(gvEventLog.GetFocusedRowCellValue("audio_id").ToString());
                string fileServerUrl = FileManagerUtility.GetServerUrl(audioID);
                string m_AudioFileUrl = BrightSalesFacade.WebDavFile.AudioToCacheFolder(fileServerUrl);
                if (string.IsNullOrEmpty(m_AudioFileUrl)) {
                    NotificationDialog.Error("Bright Sales", "Cannot download audio file. Please contact administrator.");
                    return;
                }
                Thread.Sleep(2000);
                WaitDialog.Close();
                PlayAudio(m_AudioFileUrl);
            }
            /**/
        }
        private void PlayAudio(string url)
        {
            FileInfo info = new FileInfo(url);
            if (info.Length == 0)
                NotificationDialog.Information("Bright Sales", "Cannot play audio. Audio file don't have content.");
            else {
                _player = new CallLogPlayer(url);
                if (!_player.IsDisposed) {
                    //_player.PlayAudio();
                    _player.Show(this);
                }
                //_player.Stop();
            }
        }
        private void toolTipController1_GetActiveObjectInfo(object sender, DevExpress.Utils.ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            var view = gvEventLog;
            if (view == null) return;

            var info = view.CalcHitInfo(e.ControlMousePosition);
            CTScEventAndFollowUpLog followUp = gvEventLog.GetRow(info.RowHandle) as CTScEventAndFollowUpLog;

            if (info.InRow && prevEventFollowUp != followUp) {
                int x = info.HitPoint.X + 20;
                int y = info.HitPoint.Y + 20;
                Point location = new Point { X = x, Y = y };
                string htmlToolTipContent = TooltipUtility.GetEventFollowUpTooltip(followUp);
                htmlToolTip1.Show("Loading.......", htmlToolTipContent, this, location);
            }
            prevEventFollowUp = followUp;
        }
        private void gcEventLog_MouseLeave(object sender, EventArgs e)
        {
            htmlToolTip1.Hide(this);
        }
        private void gvEventLog_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e == null)
                return;

            CTScEventAndFollowUpLog _item = gvEventLog.GetRow(e.RowHandle) as CTScEventAndFollowUpLog;
            if (_item == null)
                return;

            if (!_item.done)
                e.Appearance.BackColor = Color.YellowGreen;

        }
        private void cbxIsHidden_EditValueChanging(object sender, ChangingEventArgs e)
        {
            CTScEventAndFollowUpLog _item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            if (_item == null || !_item.done)
                e.Cancel = true;
        }
        #endregion

        #region Subscribed Events
        private void GridUtilityMenu_LoadNurtureEventOnClick()
        {
            if (m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountLockedByOtherUser()) {
                NotificationDialog.Warning("Bright Sales", "This company is currently worked by another user.");
                return;
            }

            if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit) {
                NotificationDialog.Warning("Bright Sales", "This dialog is currently being edited.");
                return;
            }

            //if (CanWorkOnCompany != null)
            //    if (!CanWorkOnCompany())
            //        return;

            if (m_BrightSalesProperty.CommonProperty.OnCallMode || !m_BrightSalesProperty.CommonProperty.CallLogSaved) {
                NotificationDialog.Warning("Bright Sales", "A call is currently in progress or a pending call log needs to be saved first.");
                return;
            }

            //if (UserOnCallOrHasPendingCallLog != null)
            //    if (UserOnCallOrHasPendingCallLog())
            //        return;

            CTScEventAndFollowUpLog _item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            if (_item == null || !_item.event_type.Equals("Nurture Log"))
                return;

            /**
             * prepare required objects.
             */
            WaitDialog.Show("Loading data ...");
            this.GetProperties(true);

            event_followup_log _efoEventLog;
            account _efoAccount;
            subcampaign _efoSubCampaign;
            campaign _efoCampaign;
            customer _efoCustomer;
            final_lists _efoFinalList;
            sub_campaign_account_appointments _efoAccountAppointment;
            contact _efoContact;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                event_followup_log _efoLog = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _item.id); // the nurture log
                _efoEventLog = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _efoLog.nurture_event_followup_log_id);
                _efoSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == _efoEventLog.subcampaign_id);
                _efoCampaign = _efDbContext.campaigns.FirstOrDefault(i => i.id == _efoSubCampaign.campaign_id);
                _efoCustomer = _efDbContext.customers.FirstOrDefault(i => i.id == _efoCampaign.customer_id);
                _efoAccount = _efDbContext.accounts.FirstOrDefault(i => i.id == _efoEventLog.account_id);
                _efoFinalList = _efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == _efoSubCampaign.id);
                _efDbContext.Detach(_efoEventLog);
                _efDbContext.Detach(_efoAccount);
                _efDbContext.Detach(_efoSubCampaign);
                _efDbContext.Detach(_efoCampaign);
                _efDbContext.Detach(_efoCustomer);
                _efDbContext.Detach(_efoFinalList);                
                
                _efoContact = _efDbContext.contacts.FirstOrDefault(i => i.id == _efoEventLog.contact_id);
                if (_efoContact != null)
                    _efDbContext.Detach(_efoContact);

                _efoAccountAppointment = _efDbContext.sub_campaign_account_appointments.FirstOrDefault(i => i.final_list_id == _efoFinalList.id && i.account_id == _efoAccount.id);
                if (_efoAccountAppointment != null)
                    _efDbContext.Detach(_efoAccountAppointment);

                _efDbContext.Detach(_efoLog);
            }

            /**
             * prepare xml data.
             */
            //int _ItemSelectedIndex = 0;
            //int _NotQualifiedIndex = -1;


            //string _XmlData = _efoSubCampaign.xml_config_data;
            //List<string> _lstAccountLeadStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_lead_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex);
            //int _AccountLeadStatusSelectedIndex = _ItemSelectedIndex;
            //int _AccountLeadStatusNotQualifiedIndex = _NotQualifiedIndex;
            //List<string> _lstAccountStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex);
            //int _AccountStatusSelectedIndex = _ItemSelectedIndex;
            //int _AccountStatusNotQualifiedIndex = _NotQualifiedIndex;
            //List<string> _lstContactStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/contact/contact_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex);
            //int _ContactStatusSelectedIndex = _ItemSelectedIndex;
            //int _ContactStatusNotQualifiedIndex = _NotQualifiedIndex;

            //int _ItemSelectedIndex = 0;
            //int _NotQualifiedIndex = -1;
            //int _SendEmailIndex = -1;

            string _XmlData = _efoSubCampaign.xml_config_data;
            List<XmlUtility.SubCampaignConfig> _lstAccountLeadStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_lead_status_dropdown");
            //List<string> _lstAccountLeadStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_lead_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
            //int _AccountLeadStatusSelectedIndex = _ItemSelectedIndex;
            //int _AccountLeadStatusNotQualifiedIndex = _NotQualifiedIndex;
            List<XmlUtility.SubCampaignConfig> _lstAccountStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_status_dropdown");
            //List<string> _lstAccountStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
            //int _AccountStatusSelectedIndex = _ItemSelectedIndex;
            //int _AccountStatusNotQualifiedIndex = _NotQualifiedIndex;
            //int _AccountStatusSendEmailIndex = _SendEmailIndex;
            List<XmlUtility.SubCampaignConfig> _lstContactStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/contact/contact_status_dropdown");
            //List<string> _lstContactStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/contact/contact_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
            //int _ContactStatusSelectedIndex = _ItemSelectedIndex;
            //int _ContactStatusNotQualifiedIndex = _NotQualifiedIndex;

            /**
             * prepare campaign booking args.
             */
            string _LastUpdatedInfo = ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(_efoFinalList.id, _efoAccount.id);
            CampaignBookingProperty.CampaignBoookingArguments _args = new CampaignBookingProperty.CampaignBoookingArguments() {
                Id = _efoEventLog.id,
                IsAssignedToTeam = _efoEventLog.assigned_user == 0 ? true : false,
                ContactId = _efoEventLog.contact_id == null ? 0 : Convert.ToInt32(_efoEventLog.contact_id),
                CustomerName = _efoCustomer.customer_name,
                CampaignName = _efoCampaign.campaign_name,
                SubCampaignName = _efoSubCampaign.title,
                CompanyName = _efoAccount.company_name,
                City = _efoAccount.city,
                Remarks = _efoEventLog.short_message,
                IsDone = _efoEventLog.done,
                //AccountLeadStatusSelectedIndex = _AccountLeadStatusSelectedIndex,
                //AccountStatusSelectedIndex = _AccountStatusSelectedIndex,
                //ContactStatusSelectedIndex = _ContactStatusSelectedIndex,
                //AccountLeadStatusNotQualifiedIndex = _AccountLeadStatusNotQualifiedIndex,
                //AccountStatusNotQualifiedIndex = _AccountStatusNotQualifiedIndex,
                //ContactStatusNotQualifiedIndex = _ContactStatusNotQualifiedIndex,
                AccountLeadStatuses = _lstAccountLeadStatuses,
                AccountStatuses = _lstAccountStatuses,
                ContactStatuses = _lstContactStatuses,
                oAppointment = new SubCampaignAppointment(_efoCustomer.id, _efoCampaign.id, _efoSubCampaign.id) {
                    AccountId = (int)_efoEventLog.account_id,
                    FinalListId = _efoFinalList.id,
                    CompanyWebsite = _efoAccount.www,
                    CompanyBoardNumber = _efoAccount.telephone,
                    CompanyAppointmentStatus = _efoAccountAppointment == null? "Open": _efoAccountAppointment.status,
                    CompanyAppointmentLeadStatus = _efoAccountAppointment == null? string.Empty: _efoAccountAppointment.lead_status
                },
                BreadCrumb = string.Format("{0}{1}{2}{3}",
                    string.IsNullOrEmpty(_efoAccount.company_name) ? "" : _efoAccount.company_name,
                    string.IsNullOrEmpty(_efoAccount.city) ? "" : ", " + _efoAccount.city,
                    string.IsNullOrEmpty(_efoAccount.country) ? "" : ", " + _efoAccount.country,
                    string.IsNullOrEmpty(_LastUpdatedInfo) ? "" : String.Format(" ({0})", _LastUpdatedInfo)
                ),
                ToolTipInfo = string.Format("Customer: {5}{0}Campaign: {6}{0}SubCampaign: {7}{0}{0}Date: {1:yyyy-MM-dd}{0}Assigned User: {2}{0}Company Name: {3}{0}Contact: {8}{0}{0}Remarks: {4}",
                    Environment.NewLine,
                    (DateTime)_efoEventLog.date_of_transaction,
                    _efoEventLog.assigned_user,
                    _efoAccount.company_name,
                    _efoEventLog.short_message,
                    _efoCustomer.customer_name,
                    _efoCampaign.campaign_name,
                    _efoSubCampaign.title,
                    string.Format("{0} {1}", _efoContact.first_name, _efoContact.last_name)
                ),
                CampaignInfo = string.Format("{0}, {1}>{2}>{3}",
                    _efoAccount.company_name,
                    _efoCustomer.customer_name,
                    _efoCampaign.campaign_name,
                    _efoSubCampaign.title
                )
            };

            if (_args == null)
                return;

            m_EventBus.Notify(new EventFollowUpLogEvents.OnWorkNurtureEvent() {
                OnWorkArgs = _args
            });
            //if (WorkNurtureEvent != null)
            //    WorkNurtureEvent(_args);

            WaitDialog.Close();
        }
        private void GridUtilityMenu_EditEventOnClick()
        {
            if (gvEventLog.RowCount < 1)
                return;

            if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit) {
                NotificationDialog.Warning("Bright Sales", "This dialog is currently being edited.");
                return;
            }

            SalesConsultant.Business.BrightSalesGridUtility.EditEventOnClick -= new SalesConsultant.Business.BrightSalesGridUtility.EditEventOnClickEventHandler(GridUtilityMenu_EditEventOnClick);
            CTScEventAndFollowUpLog _item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            if (_item == null || _item.event_type.Equals("Call Log") || _item.event_type.Equals("Nurture Log"))
                return;

            bool _EditAllowed = false;
            if (_item.assigned_user == UserSession.CurrentUser.UserId || _item.created_by.Equals(UserSession.CurrentUser.UserFullName))
                _EditAllowed = true;

            if (!_EditAllowed)
                return;

            /** /
            bool _EditAllowed = false;
            if (_item.event_type.Equals("Nurture Event") || _item.event_type.Equals("Make Call"))
                _EditAllowed = true;

            if (!_EditAllowed)
                return;
            /**/

            int _FinalListId;
            event_followup_log _data;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _data = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _item.id);
                _FinalListId = (int)_efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == _data.subcampaign_id).id;
                _efDbContext.Detach(_data);
            }
            
            if (_data == null)
                return;

            #region Initialize Editor
            FollowUpEditor _control = new FollowUpEditor() {
                Dock = DockStyle.Fill,
                IsNurtureEvent = false
            };
            _control.btnSave_OnClick += new FollowUpEditor.btnSaveOnClickEventHandler(_control_btnSave_OnClick);
            
            PopupDialog _dlg = new PopupDialog() {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Edit Task",
                ClientSize = new Size(_control.Width + 2, _control.Height + 2),
                CloseBox = false
            };
            _dlg.Controls.Add(_control);
            #endregion

            int _EventType = 0;
            if (_item.event_type.Equals("Nurture Event")) {
                _EventType = (int)_data.subcampaign_id;
                _control.IsNurtureEvent = true;
            }
            else if (_item.event_type.Equals("Make Call"))
                _EventType = -1;
            else if (_item.event_type.Equals("Todo"))
                _EventType = -3;

            _control.SubCampaignId = SubCampaignId;
            _control.Prepare();
            _control.GetEventTypes(SubCampaignId);
            _control.SetSelectedEventType(_EventType);
            _control.LoadSalesUsers((int)_data.subcampaign_id, (int)_data.assigned_user);            
            _control.SetCampaignInfo(_data);            

            List<CTScSubCampaignContactList> _ContactList = ObjectSubCampaign.GetSubCampaignContacts((int)_data.subcampaign_id, (int)_data.account_id, _FinalListId);
            if (_ContactList.Count > 0) {
                _control.LoadContactPersons(_ContactList);
                CTScSubCampaignContactList _contact = _ContactList.Find(i => i.id == (int)_data.contact_id);
                if (_contact != null) {
                    _control.ContactPerson = _contact;
                    _control.LoadSelectedContact(false);
                }
            }
            
            _dlg.ShowDialog(this);
        }
        private void _control_btnSave_OnClick(object sender, EventFollowUpLogEvents.OnSaveArguments e)
        {
            this.PopulateEventFollowUpLogView(e.EventLog.id);
            m_EventBus.Notify(new EventFollowUpLogEvents.OnSave() { 
                OnSaveArgs = e
            });

            //if (EventLog_OnSave != null)
            //    EventLog_OnSave(e);
        }
        private void GridUtilityMenu_SelectContact_OnClick()
        {
            if (gvEventLog.RowCount < 1)
                return;

            if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit) {
                NotificationDialog.Warning("Bright Sales", "This dialog is currently being edited.");
                return;
            }

            SalesConsultant.Business.BrightSalesGridUtility.SelectContactOnClick -= new SalesConsultant.Business.BrightSalesGridUtility.SelectContactOnClickEventHandler(GridUtilityMenu_SelectContact_OnClick);
            CTScEventAndFollowUpLog _item = gvEventLog.GetFocusedRow() as CTScEventAndFollowUpLog;
            if (_item == null)
                return;

            event_followup_log _data;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _data = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _item.id);
                _efDbContext.Detach(_data);
            }

            if (_data.contact_id != null)
                m_EventBus.Notify(new EventFollowUpLogEvents.OnSelectContactGridMenuClick() {
                    ContactId = Convert.ToInt32(_data.contact_id)
                });

            //if (_data.contact_id != null && SelectContact_OnClick != null) {
            //    SelectContact_OnClick(new SelectContactArgs() {
            //        ContactId = Convert.ToInt32(_data.contact_id)
            //    });
            //}
        }
        #endregion

        #region Events & Delegates
        //public delegate void CompanyRemarkSavedEventHandler(object sender, CompanyRemarkSavedArgs e);
        //public event CompanyRemarkSavedEventHandler CompanyRemark_Saved;
        //public class CompanyRemarkSavedArgs : EventArgs {
        //    public string CompanyRemarks { get; set; }
        //    public int SubCampaignId { get; set; }
        //    public int AccountId { get; set; }
        //    public int FinalListId { get; set; }
        //}
        
        //public delegate void EventFollowUpLogSavedEventHandler(object sender, EventArgs e);
        //public event EventFollowUpLogSavedEventHandler OnEventFollowUpLogSaved;

        //public delegate void AfterDeleteEventHandler(object sender, AfterDeleteArgs e);
        //public event AfterDeleteEventHandler AfterDelete;
        //public class AfterDeleteArgs : EventArgs {
        //    public int DeletedId { get; set; }            
        //}
        #endregion
    }
}
