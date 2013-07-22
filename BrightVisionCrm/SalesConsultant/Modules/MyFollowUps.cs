
using System;
using System.Linq;
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
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using SalesConsultant.Forms;
using SalesConsultant.Business;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
using SalesConsultant.Utils;
using SalesConsultant.Properties;
using DevExpress.Utils;
using BrightVision.DQControl.Utilities;
using BrightVision.Common.Events.Core;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using SalesConsultant.Events;

namespace SalesConsultant.Modules 
{
    public partial class MyFollowUps : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructor
        public MyFollowUps()
        {
            InitializeComponent();
            this.Visible = false;
            this.Visible = true;
            gridColumnDate.DisplayFormat.Format = new GridColumnCustomDateFormat();
            DevExpress.Utils.ImageCollection images = new DevExpress.Utils.ImageCollection();
            images.AddImage(Resources.make_call);
            images.AddImage(Resources.todo);
            images.AddImage(Resources.nurture);
           
            riicEventType.SmallImages = images;
            webBrowser1 = new WebBrowser();
            this.RegisterEvents();
        }
        #endregion

        #region Public Properties
        public bool IsLastRow 
        {
            get {
                if (gvFollowUpList.RowCount < 2)
                    return true;

                return gvFollowUpList.IsLastRow;
            }
        }
        public bool IsFirstRow 
        {
            get {
                if (gvFollowUpList.RowCount < 2)
                    return true;

                return gvFollowUpList.IsFirstRow;
            }
        }
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private string m_CampaignName = string.Empty;
        private SubCampaignAppointment m_objAppointment = null;
        private CTMyFollowUps m_objFollowUp = null;
        private int m_SelectedFollowUpId = 0;
        private sub_campaign_account_lists m_CurrentAccount = null;
        private bool m_IsLoadingGridView = false;
        private int m_MyFollowUpLastWorkedAccountId = 0;
        private bool m_MarkSelectedRowsAsDonePerformed = false;

        //private int m_AccountLeadStatusSelectedIndex = 0;
        //private int m_AccountStatusSelectedIndex = 0;
        //private int m_ContactStatusSelectedIndex = 0;
        //private int m_AccountLeadStatusNotQualifiedIndex = -1;
        //private int m_AccountStatusNotQualifiedIndex = -1;
        //private int m_AccountStatusSendEmailIndex = 0;
        //private int m_ContactStatusNotQualifiedIndex = -1;
        //private List<string> m_lstAccountLeadStatuses = null;
        //private List<string> m_lstAccountStatuses = null;
        //private List<string> m_lstContactStatuses = null;
        private List<XmlUtility.SubCampaignConfig> m_lstAccountLeadStatuses = null;
        private List<XmlUtility.SubCampaignConfig> m_lstAccountStatuses = null;
        private List<XmlUtility.SubCampaignConfig> m_lstContactStatuses = null;
        private CTMyFollowUps prevFollowUp = null;

        public int m_FollowUpBar_FollowUpId = 0;
        private WebBrowser webBrowser1;
        private int iTime = 0;
        #endregion

        #region Public Methods
        public SubCampaignAppointment GetAppointmentData()
        {
            this.SetCampaignBookingParams();
            return m_objAppointment;
        }
        public string GetCompanyModificationInfo()
        {
            if (m_objAppointment == null)
                return string.Empty;

            return String.Format("{0} - last updated on: {1}", 
                m_objFollowUp.company_name, 
                ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(m_objAppointment.FinalListId, m_objAppointment.AccountId)
            );
        }
        public CampaignBookingProperty.CampaignBoookingArguments GetCampaignBookingArgs(bool pForWorkModePurpose)
        {
            this.SetCampaignBookingParams();
            if (m_objFollowUp == null)
                return null;

            /**
             * get sub-campaign status lists
             */
            if (pForWorkModePurpose)
                this.GetProperties(true);
            else
                this.GetProperties(false);

            event_followup_log _data;
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _data = _efDbModel.event_followup_log.FirstOrDefault(i => i.id == m_objFollowUp.id);
                if (_data != null)
                    _efDbModel.Detach(_data);
            }

            string _LastUpdatedInfo = ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo((int)m_objFollowUp.final_list_id, (int)m_objFollowUp.account_id);
            CampaignBookingProperty.CampaignBoookingArguments _args = new CampaignBookingProperty.CampaignBoookingArguments() {
                Id = m_objFollowUp.id,
                IsAssignedToTeam = _data.assigned_user == 0 ? true : false,
                ContactId = _data.contact_id == null ? 0 : Convert.ToInt32(_data.contact_id),
                CustomerName = m_objFollowUp.customer_name,
                CampaignName = m_objFollowUp.campaign_name,
                SubCampaignName = m_objFollowUp.sub_campaign_name,
                oAppointment = m_objAppointment,
                CompanyName = m_objFollowUp.company_name,
                City = m_objFollowUp.city,
                Remarks = m_objFollowUp.short_message,
                IsDone = m_objFollowUp.done,
                //AccountLeadStatusSelectedIndex = m_AccountLeadStatusSelectedIndex,
                //AccountStatusSelectedIndex = m_AccountStatusSelectedIndex,
                //ContactStatusSelectedIndex = m_ContactStatusSelectedIndex,
                //AccountLeadStatusNotQualifiedIndex = m_AccountLeadStatusNotQualifiedIndex,
                //AccountStatusNotQualifiedIndex = m_AccountStatusNotQualifiedIndex,
                //ContactStatusNotQualifiedIndex = m_ContactStatusNotQualifiedIndex,
                AccountLeadStatuses = m_lstAccountLeadStatuses,
                AccountStatuses = m_lstAccountStatuses,
                ContactStatuses = m_lstContactStatuses,
                BreadCrumb = string.Format("{0}{1}{2}{3}",
                    string.IsNullOrEmpty(m_objFollowUp.company_name) ? "" : m_objFollowUp.company_name,
                    string.IsNullOrEmpty(m_objFollowUp.city) ? "" : ", " + m_objFollowUp.city,
                    string.IsNullOrEmpty(m_objFollowUp.country) ? "" : ", " + m_objFollowUp.country,
                    string.IsNullOrEmpty(_LastUpdatedInfo) ? "" : String.Format(" ({0})", _LastUpdatedInfo)
                ),
                ToolTipInfo = string.Format("Customer: {5}{0}Campaign: {6}{0}SubCampaign: {7}{0}{0}Date: {1:yyyy-MM-dd}{0}Assigned User: {2}{0}Company Name: {3}{0}Contact: {8}{0}{0}Remarks: {4}",
                    Environment.NewLine,
                    (DateTime)m_objFollowUp.date_of_transaction,
                    m_objFollowUp.assigned_user,
                    m_objFollowUp.company_name,
                    m_objFollowUp.short_message,
                    m_objFollowUp.customer_name,
                    m_objFollowUp.campaign_name,
                    m_objFollowUp.sub_campaign_name,
                    m_objFollowUp.contact_name
                ),
                CampaignInfo = string.Format("{0}, {1}>{2}>{3}",
                    m_objFollowUp.company_name,
                    m_objFollowUp.customer_name,
                    m_objFollowUp.campaign_name,
                    m_objFollowUp.sub_campaign_name
                )
            };

            return _args;
        }
        public void LoadEvents(int? subcampaignid = null)
        {
            m_IsLoadingGridView = true;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                gcFollowUpList.BeginUpdate();
                gcFollowUpList.DataSource = null;
                gcFollowUpList.DataSource = _efDbContext.FIGetMyFollowUps(UserSession.CurrentUser.UserId, subcampaignid, false).ToList();
                gvFollowUpList.BestFitColumns();
                gcFollowUpList.EndUpdate();
            }
               

            m_IsLoadingGridView = false;
            this.ApplyFilter();
            this.SetDefaultSelectedRow();
        }
        public void LoadEventsCallLogCaller(int? subcampaignid = null)
        {
            m_IsLoadingGridView = true;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                gcFollowUpList.BeginUpdate();
                gcFollowUpList.DataSource = null;
                gcFollowUpList.DataSource = _efDbContext.FIGetMyFollowUps(UserSession.CurrentUser.UserId, subcampaignid, true).ToList();
                gvFollowUpList.BestFitColumns();
                gcFollowUpList.EndUpdate();
            }


            m_IsLoadingGridView = false;
        }
        public bool MoveFirst()
        {
            this.LoadEvents();
            if (gvFollowUpList.RowCount < 1)
                return false;

            gvFollowUpList.MoveFirst();
            m_FollowUpBar_FollowUpId = m_SelectedFollowUpId;
            return true;
        }
        public bool MoveNext()
        {
            if (gvFollowUpList.RowCount < 1)
                return false;

            gvFollowUpList.MoveNext();
            m_FollowUpBar_FollowUpId = m_SelectedFollowUpId;
            return true;
        }
        public bool MovePrevious()
        {
            if (gvFollowUpList.RowCount < 1)
                return false;

            gvFollowUpList.MovePrev();
            m_FollowUpBar_FollowUpId = m_SelectedFollowUpId;
            return true;
        }
        public void ReleaseCompanyLock()
        {
            try
            {
                m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
                if (m_CurrentAccount != null) {
                    int _iRowHandle = -1;
                    for (int i = 0; i < gvFollowUpList.DataRowCount; i++) {
                        if (ValidationUtility.TryParseInt(gvFollowUpList.GetRowCellValue(i, "account_id").ToString()) == m_MyFollowUpLastWorkedAccountId) {
                            _iRowHandle = i;
                            break;
                        }
                    }

                    int final_list_id = ValidationUtility.TryParseInt(gvFollowUpList.GetRowCellValue(_iRowHandle, "final_list_id").ToString());
                    ObjectLocking.ReleaseLock(final_list_id);
                    if (_iRowHandle > -1) {
                        gvFollowUpList.SetRowCellValue(_iRowHandle, "locked", false);
                        gvFollowUpList.SetRowCellValue(_iRowHandle, "locked_by", null);
                        gvFollowUpList.SetRowCellValue(_iRowHandle, "locked_user", null);
                        gvFollowUpList.SetRowCellValue(_iRowHandle, "locked_timestamp", null);
                    }
                    m_CurrentAccount = null;
                }
            }
            catch (Exception e) {
                NotificationDialog.Warning("Bright Sales", "Error Catched: " + e.Message);
            }
        }
        public void SaveCompanyAppointment()
        {
            if (m_CurrentAccount == null)
                return;

            ObjectSubCampaign.SaveSubCampaignCompanyAppointmentStatus(
                m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                m_BrightSalesProperty.CommonProperty.FinalListId,
                m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus,
                m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus
            );
            gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "company_appoitment_status", m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus);
            gvFollowUpList.BestFitColumns();
        }
        public void CloseCompany()
        {
            m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
        }
        public int GetId()
        {
            if (gvFollowUpList.RowCount < 1)
                return 0;

            CTMyFollowUps _item = gvFollowUpList.GetFocusedRow() as CTMyFollowUps;
            if (_item != null)
                return _item.id;
            else
                return 0;
        }
        public bool HasBrowsableData()
        {
            if (gvFollowUpList == null || gvFollowUpList.RowCount < 1)
                return false;

            return true;
        }
        public void UpdateFocusedRow()
        {
            if (gvFollowUpList.RowCount < 1)
                return;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                CTMyFollowUps _row = gvFollowUpList.GetFocusedRow() as CTMyFollowUps;
                event_followup_log _data = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _row.id);
                if (_data == null)
                    return;

                contact _contact = _efDbContext.contacts.FirstOrDefault(i => i.id == _data.contact_id);

                _efDbContext.Detach(_data);
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "done", _data.done);
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "date_of_transaction", _data.date_of_transaction);
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "event_type", _data.event_type);
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "start_time", _data.start_time);
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "end_time", _data.end_time);
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "contact_name", string.Format("{0} {1}", _contact.first_name, _contact.last_name));
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "title", _contact.title);
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "email", _contact.email);
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "direct_phone", _contact.direct_phone);
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "mobile", _contact.mobile);
                gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "short_message", _data.short_message);

                if (_data.assigned_user != null) {
                    if (_data.assigned_user > 0) {
                        user _user = _efDbContext.users.FirstOrDefault(i => i.id == _data.assigned_user);
                        if (_user != null)
                            _efDbContext.Detach(_user);

                        gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "assigned_user", _user.fullname);
                    }
                }
            }
        }
        public void SetCampaignInfo(string CampaignName)
        {
            m_CampaignName = CampaignName;
        }
        public void SetFocusRow()
        {
            //DAN: Set grid focus row handle to where the follow up selected as jeff advice
            m_SelectedFollowUpId = m_FollowUpBar_FollowUpId;
            this.SetDefaultSelectedRow();
        }
        public void UpdateGrid()
        {
            //LoadEvents(m_BrightSalesProperty.CommonProperty.SubCampaignId);
            cmbShowEvents.SelectedIndex = 0;
            //this.ApplyFilter();
        }
        #endregion

        #region Private Methods
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<FollowUpBarEvents.OnLoad>().Subscribe(OnLoadButtonPressed);
        }
        private void OnLoadButtonPressed(FollowUpBarEvents.OnLoad e)
        {
            this.WorkOnSelectedAccount();
        }
        private void SetDefaultSelectedRow()
        {
            if (m_SelectedFollowUpId < 1)
                return;

            int RowNo = 0;
            CTMyFollowUps CurrentItem = null;
            GridView objGridView = (GridView)gcFollowUpList.FocusedView;
            for (int i = 0; i < objGridView.RowCount; i++) {
                CurrentItem = null;
                CurrentItem = (CTMyFollowUps)objGridView.GetRow(i);
                if (CurrentItem.id == m_SelectedFollowUpId) {
                    RowNo = i;
                    break;
                }
            }

            gvFollowUpList.FocusedRowHandle = RowNo;
        }
        private void SetDoneFlag(bool IsDone)
        {
            WaitDialog.Show(ParentForm, "Updating ...");
            m_SelectedFollowUpId = m_objFollowUp.id;
            EventFollowUp.Save(m_objFollowUp.id, IsDone);
            gvFollowUpList.CloseEditor();
            gvFollowUpList.RefreshData();
            WaitDialog.Close();
        }
        private void SetCampaignBookingParams()
        {
            m_objAppointment = null;
            m_objFollowUp = gvFollowUpList.GetFocusedRow() as CTMyFollowUps;
            if (m_objFollowUp == null)
                return;

            m_objAppointment = new SubCampaignAppointment((int)m_objFollowUp.customer_id, (int)m_objFollowUp.campaign_id, (int)m_objFollowUp.subcampaign_id);
            m_objAppointment.AccountId = (int)m_objFollowUp.account_id;
            m_objAppointment.FinalListId = m_objFollowUp.final_list_id == null ? 0 : (int)m_objFollowUp.final_list_id;
            m_objAppointment.CompanyWebsite = m_objFollowUp.company_website;
            m_objAppointment.CompanyBoardNumber = m_objFollowUp.company_board_no;
            m_objAppointment.CompanyAppointmentStatus = m_objFollowUp.company_appoitment_status == null ? "Open" : m_objFollowUp.company_appoitment_status;
            m_objAppointment.CompanyAppointmentLeadStatus = m_objFollowUp.lead_status == null ? "" : m_objFollowUp.lead_status;
            m_SelectedFollowUpId = m_objFollowUp.id;
        }
        private void LoadCampaignBooking()
        {
            if (gvFollowUpList.RowCount < 1)
                return;

            /**
             * check if company is workable or not.
             */
            if (m_BrightSalesProperty.CommonProperty.OnCallMode || !m_BrightSalesProperty.CommonProperty.CallLogSaved) {
                NotificationDialog.Information("Bright Sales", "A call is in progress or there is pending call log to be saved first.");
                return;
            }

            this.GetProperties(true);
            m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = true;
            this.SetCampaignBookingParams();
            CampaignBookingProperty.CampaignBoookingArguments _args = new CampaignBookingProperty.CampaignBoookingArguments() {
                Id = m_objFollowUp.id,
                ContactId = m_objFollowUp.contact_id == null ? 0 : Convert.ToInt32(m_objFollowUp.contact_id),
                CustomerName = m_objFollowUp.customer_name,
                CampaignName = m_objFollowUp.campaign_name,
                SubCampaignName = m_objFollowUp.sub_campaign_name,
                oAppointment = m_objAppointment,
                CompanyName = m_objFollowUp.company_name,
                City = m_objFollowUp.city,
                Remarks = m_objFollowUp.short_message,
                IsDone = m_objFollowUp.done,
                //AccountLeadStatusSelectedIndex = m_AccountLeadStatusSelectedIndex,
                //AccountStatusSelectedIndex = m_AccountStatusSelectedIndex,
                //ContactStatusSelectedIndex = m_ContactStatusSelectedIndex,
                //AccountLeadStatusNotQualifiedIndex = m_AccountLeadStatusNotQualifiedIndex,
                //AccountStatusNotQualifiedIndex = m_AccountStatusNotQualifiedIndex,
                //ContactStatusNotQualifiedIndex = m_ContactStatusNotQualifiedIndex,
                AccountLeadStatuses = m_lstAccountLeadStatuses,
                AccountStatuses = m_lstAccountStatuses,
                ContactStatuses = m_lstContactStatuses,
                BreadCrumb = string.Format("{0}{1}{2} ({3})",
                    string.IsNullOrEmpty(m_objFollowUp.company_name) ? "" : m_objFollowUp.company_name,
                    string.IsNullOrEmpty(m_objFollowUp.city) ? "" : ", " + m_objFollowUp.city,
                    string.IsNullOrEmpty(m_objFollowUp.country) ? "" : ", " + m_objFollowUp.country,
                    ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                        (int)m_objFollowUp.final_list_id,
                        (int)m_objFollowUp.account_id
                    )
                ),
                CampaignInfo = string.Format("{0}, {1}>{2}>{3}",
                    m_objFollowUp.company_name,
                    m_objFollowUp.customer_name,
                    m_objFollowUp.campaign_name,
                    m_objFollowUp.sub_campaign_name
                ),
                ToolTipInfo = string.Format("Customer: {5}{0}Campaign: {6}{0}SubCampaign: {7}{0}{0}Date: {1:yyyy-MM-dd}{0}Assigned User: {2}{0}Company Name: {3}{0}Contact: {8}{0}{0}Remarks: {4}",
                    Environment.NewLine,
                    (DateTime)m_objFollowUp.date_of_transaction,
                    m_objFollowUp.assigned_user,
                    m_objFollowUp.company_name,
                    m_objFollowUp.short_message,
                    m_objFollowUp.customer_name,
                    m_objFollowUp.campaign_name,
                    m_objFollowUp.sub_campaign_name,
                    m_objFollowUp.contact_name
                )
            };

            m_BrightSalesProperty.CommonProperty.IsLoadEventLog = true;
            m_EventBus.Notify(new MyFollowUpsEvents.OnCampaignBookingLoad() {
                OnCampaignBookingLoadArgs = _args
            });

            #region Old Code
            //if (m_objFollowUp != null)
            //    m_objMainForm.m_CampaignBookingModule.SelectedContactId = (int) m_objFollowUp.contact_id;

            //m_objMainForm.m_CampaignBookingModule.Enabled = true;
            //m_objMainForm.m_CampaignBookingModule.MyFollowUpView = this;
            //m_objMainForm.m_CampaignBookingModule.ParentView = ManageCampaignBooking.eParentView.MyFollowUp;
            //m_objMainForm.m_CampaignBookingModule.CampaignBookingMode = ManageCampaignBooking.eCampaignBookingMode.WorkMode;
            //this.GetCampaignBookingRequiredData();
            //todo: to be handled
            //m_objMainForm.m_CampaignBookingModule.LoadCampaignBookingData();
            //m_objMainForm.m_CampaignBookingModule.OnLoadInitialization();
            //m_objMainForm.m_CampaignBookingModule.WorkOnSelectedCompany();
            //m_objMainForm.m_CampaignBookingModule.SetDefaultNavigationSettings();
            //m_objMainForm.tcSalesConsultant.SelectedTabPage = m_objMainForm.tpCampaignBooking;
            //m_objMainForm.objCampaignBookingTab.Enabled = true;
            #endregion
        }
        //private void GetEventLogProperties()
        //{
        //    if (gvFollowUpList.RowCount < 1)
        //        return;

        //    /**
        //     * get sub-campaign status lists
        //     */
        //    string _XmlData;
        //    using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection))
        //    {
        //        _XmlData = _efDbModel.subcampaigns.FirstOrDefault(i => i.id == m_objFollowUp.subcampaign_id).xml_config_data;
        //    }
        //    int _ItemSelectedIndex = 0;
        //    int _NotQualifiedIndex = -1;
        //    int _SendEmailIndex = -1;

        //    m_lstAccountLeadStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_lead_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
        //    m_AccountLeadStatusSelectedIndex = _ItemSelectedIndex;
        //    m_AccountLeadStatusNotQualifiedIndex = _NotQualifiedIndex;
        //    m_lstAccountStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
        //    m_AccountStatusSelectedIndex = _ItemSelectedIndex;
        //    m_AccountStatusNotQualifiedIndex = _NotQualifiedIndex;
        //    m_AccountStatusSendEmailIndex = _SendEmailIndex;
        //    m_lstContactStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/contact/contact_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
        //    m_ContactStatusSelectedIndex = _ItemSelectedIndex;
        //    m_ContactStatusNotQualifiedIndex = _NotQualifiedIndex;

        //    m_BrightSalesProperty.EventLogProperty.ContactId = m_objFollowUp.contact_id == null ? 0 : Convert.ToInt32(m_objFollowUp.contact_id);
        //    m_BrightSalesProperty.EventLogProperty.CompanyName = m_objFollowUp.company_name;
        //    m_BrightSalesProperty.EventLogProperty.CustomerId = m_objFollowUp.customer_id;
        //    m_BrightSalesProperty.EventLogProperty.CampaignId = m_objFollowUp.campaign_id;
        //    m_BrightSalesProperty.EventLogProperty.SubCampaignId = Convert.ToInt32(m_objFollowUp.subcampaign_id);
        //    m_BrightSalesProperty.EventLogProperty.AccountId = Convert.ToInt32(m_objFollowUp.account_id);
        //    m_BrightSalesProperty.EventLogProperty.FinalListId = m_objFollowUp.final_list_id;

        //    m_MyFollowUpLastWorkedAccountId = Convert.ToInt32(m_objFollowUp.account_id);
        //    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
        //    {
        //        /**
        //         * get company locked property.
        //         */
        //        sub_campaign_account_lists _eftAccount = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
        //            i.account_id == m_BrightSalesProperty.EventLogProperty.AccountId &&
        //            i.final_list_id == m_BrightSalesProperty.EventLogProperty.FinalListId
        //        );
        //        m_BrightSalesProperty.EventLogProperty.CompanyLocked = false;
        //        if (_eftAccount.locked && _eftAccount.locked_by != UserSession.CurrentUser.UserId)
        //            m_BrightSalesProperty.EventLogProperty.CompanyLocked = true;

        //        /**
        //         * get company status and lead status indexes for dropdown.
        //         */
        //        sub_campaign_account_appointments _eftAccountAppt = _efDbContext.sub_campaign_account_appointments.FirstOrDefault(i =>
        //            i.account_id == m_BrightSalesProperty.EventLogProperty.AccountId &&
        //            i.final_list_id == m_BrightSalesProperty.EventLogProperty.FinalListId
        //        );

        //        m_AccountStatusSelectedIndex = 0;
        //        m_AccountLeadStatusSelectedIndex = 0;

        //        if (_eftAccountAppt != null)
        //        {
        //            int _idx = m_lstAccountStatuses.FindIndex(i => i.Equals(_eftAccountAppt.status));
        //            if (_idx != null && _idx > 0)
        //                m_AccountStatusSelectedIndex = _idx;

        //            _idx = m_lstAccountLeadStatuses.FindIndex(i => i.Equals(_eftAccountAppt.lead_status));
        //            if (_idx != null && _idx > 0)
        //                m_AccountLeadStatusSelectedIndex = _idx;

        //            _efDbContext.Detach(_eftAccountAppt);
        //        }

        //        /**
        //         * get contact/dialog status index for dropdown.
        //         */
        //        m_ContactStatusSelectedIndex = 0;
        //        if (m_BrightSalesProperty.EventLogProperty.ContactId > 0)
        //        {
        //            sub_campaign_contact_appointments _eftContactAppt = _efDbContext.sub_campaign_contact_appointments.FirstOrDefault(i =>
        //                i.contact_id == m_BrightSalesProperty.EventLogProperty.ContactId &&
        //                i.final_list_id == m_BrightSalesProperty.EventLogProperty.FinalListId
        //            );

        //            if (_eftContactAppt != null)
        //            {
        //                int _idx = m_lstContactStatuses.FindIndex(i => i.Equals(_eftContactAppt.status));
        //                if (_idx != null && _idx > 0)
        //                    m_ContactStatusSelectedIndex = _idx;

        //                _efDbContext.Detach(_eftContactAppt);
        //            }
        //        }
        //        _efDbContext.Detach(_eftAccount);
        //    }

        //    m_BrightSalesProperty.EventLogProperty.CurrentWorkedAccountId = Convert.ToInt32(m_objFollowUp.account_id);
        //    m_BrightSalesProperty.EventLogProperty.CurrentWorkedContactId = m_objFollowUp.contact_id == null ? 0 : Convert.ToInt32(m_objFollowUp.contact_id);
        //    m_BrightSalesProperty.CampaignBooking.Appointment = new CampaignBookingProperty.AppointmentProperty()
        //    {
        //        CompanyWebsite = m_objFollowUp.company_website,
        //        CompanyAppointmentStatus = m_objFollowUp.company_appoitment_status,
        //        CompanyBoardNumber = m_objFollowUp.company_board_no,
        //        CompanyAppointmentLeadStatus = m_objFollowUp.lead_status
        //    };

        //    m_BrightSalesProperty.CampaignBooking.AccountStatus = new CampaignBookingProperty.AccountStatusProperty()
        //    {
        //        AccountLeadStatuses = m_lstAccountLeadStatuses,
        //        AccountStatuses = m_lstAccountStatuses,
        //        AccountLeadStatusSelectedIndex = m_AccountLeadStatusSelectedIndex,
        //        AccountStatusSelectedIndex = m_AccountStatusSelectedIndex,
        //        AccountLeadStatusNotQualifiedIndex = m_AccountLeadStatusNotQualifiedIndex,
        //        AccountStatusNotQualifiedIndex = m_AccountStatusNotQualifiedIndex,
        //        AccountStatusSendEmailIndex = m_AccountStatusSendEmailIndex
        //    };
        //    m_BrightSalesProperty.CampaignBooking.ContactStatus = new CampaignBookingProperty.ContactStatusProperty()
        //    {
        //        ContactStatuses = m_lstContactStatuses,
        //        ContactStatusSelectedIndex = m_ContactStatusSelectedIndex,
        //        ContactStatusNotQualifiedIndex = m_ContactStatusNotQualifiedIndex
        //    };
        //    string _LastUpdateInfo = ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(m_objFollowUp.final_list_id, m_BrightSalesProperty.EventLogProperty.AccountId);
        //    m_BrightSalesProperty.CampaignBooking.BreadCrumb = string.Format("{0}{1}{2}{3}",
        //        string.IsNullOrEmpty(m_objFollowUp.company_name) ? "" : m_objFollowUp.company_name,
        //        string.IsNullOrEmpty(m_objFollowUp.city) ? "" : ", " + m_objFollowUp.city,
        //        string.IsNullOrEmpty(m_objFollowUp.country) ? "" : ", " + m_objFollowUp.country,
        //        string.IsNullOrEmpty(_LastUpdateInfo) ? string.Empty : string.Format(" ({0})", _LastUpdateInfo)
        //    );
        //    m_BrightSalesProperty.CampaignBooking.CampaignInformation = string.Format("{0}, {1}>{2}>{3}",
        //        m_objFollowUp.company_name,
        //        m_objFollowUp.customer_name,
        //        m_objFollowUp.campaign_name,
        //        m_objFollowUp.sub_campaign_name
        //    );
        //    m_BrightSalesProperty.CampaignBooking.ToolTipInformation = string.Format("Customer: {5}{0}Campaign: {6}{0}SubCampaign: {7}{0}{0}Date: {1:yyyy-MM-dd}{0}Assigned User: {2}{0}Company Name: {3}{0}Contact: {8}{0}{0}Remarks: {4}",
        //        Environment.NewLine,
        //        (DateTime)m_objFollowUp.date_of_transaction,
        //        m_objFollowUp.assigned_user,
        //        m_objFollowUp.company_name,
        //        m_objFollowUp.short_message,
        //        m_objFollowUp.customer_name,
        //        m_objFollowUp.campaign_name,
        //        m_objFollowUp.sub_campaign_name,
        //        m_objFollowUp.contact_name
        //    );
        //}
        private void GetProperties(bool pForWorkModePurpose)
        {
            if (gvFollowUpList.RowCount < 1)
                return;

            /**
             * get sub-campaign status lists
             */
            string _XmlData;
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _XmlData = _efDbModel.subcampaigns.FirstOrDefault(i => i.id == m_objFollowUp.subcampaign_id).xml_config_data;
            }
            //int _ItemSelectedIndex = 0;
            //int _NotQualifiedIndex = -1;
            //int _SendEmailIndex = -1;

            m_lstAccountLeadStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_lead_status_dropdown");
            //m_lstAccountLeadStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_lead_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
            //m_AccountLeadStatusSelectedIndex = _ItemSelectedIndex;
            //m_AccountLeadStatusNotQualifiedIndex = _NotQualifiedIndex;
            m_lstAccountStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_status_dropdown");
            //m_lstAccountStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
            //m_AccountStatusSelectedIndex = _ItemSelectedIndex;
            //m_AccountStatusNotQualifiedIndex = _NotQualifiedIndex;
            //m_AccountStatusSendEmailIndex = _SendEmailIndex;
            m_lstContactStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/contact/contact_status_dropdown");
            //m_lstContactStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/contact/contact_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex, ref _SendEmailIndex);
            //m_ContactStatusSelectedIndex = _ItemSelectedIndex;
            //m_ContactStatusNotQualifiedIndex = _NotQualifiedIndex;

            if (pForWorkModePurpose) {
                m_BrightSalesProperty.CommonProperty.ContactId = m_objFollowUp.contact_id == null ? 0 : Convert.ToInt32(m_objFollowUp.contact_id);
                m_BrightSalesProperty.CommonProperty.CompanyName = m_objFollowUp.company_name;
                m_BrightSalesProperty.CommonProperty.CustomerId = m_objFollowUp.customer_id;
                m_BrightSalesProperty.CommonProperty.CampaignId = m_objFollowUp.campaign_id;
                m_BrightSalesProperty.CommonProperty.SubCampaignId = Convert.ToInt32(m_objFollowUp.subcampaign_id);
                m_BrightSalesProperty.CommonProperty.AccountId = Convert.ToInt32(m_objFollowUp.account_id);
                m_BrightSalesProperty.CommonProperty.FinalListId = m_objFollowUp.final_list_id;
                m_MyFollowUpLastWorkedAccountId = Convert.ToInt32(m_objFollowUp.account_id);

                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {

                    /**
                     * get company locked property.
                     */
                    sub_campaign_account_lists _eftAccount = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                        i.account_id == m_BrightSalesProperty.CommonProperty.AccountId &&
                        i.final_list_id == m_BrightSalesProperty.CommonProperty.FinalListId
                    );
                    m_BrightSalesProperty.CommonProperty.CompanyLocked = false;
                    if (_eftAccount.locked && _eftAccount.locked_by != UserSession.CurrentUser.UserId)
                        m_BrightSalesProperty.CommonProperty.CompanyLocked = true;

                    /**
                     * get company status and lead status indexes for dropdown.
                     */
                    sub_campaign_account_appointments _eftAccountAppt = _efDbContext.sub_campaign_account_appointments.FirstOrDefault(i =>
                        i.account_id == m_BrightSalesProperty.CommonProperty.AccountId &&
                        i.final_list_id == m_BrightSalesProperty.CommonProperty.FinalListId
                    );

                    //m_AccountStatusSelectedIndex = 0;
                    //m_AccountLeadStatusSelectedIndex = 0;

                    if (_eftAccountAppt != null) {
                        int _idx = m_lstAccountStatuses.FindIndex(i => i.status.Equals(_eftAccountAppt.status));
                        if (_idx != null && _idx > 0) {
                            for (int i = 0; i < m_lstAccountStatuses.Count; i++)
                                m_lstAccountStatuses[i].selected = false;
                            m_lstAccountStatuses[_idx].selected = true;
                            //m_AccountStatusSelectedIndex = _idx;
                        }

                        _idx = m_lstAccountLeadStatuses.FindIndex(i => i.status.Equals(_eftAccountAppt.lead_status));
                        if (_idx != null && _idx > 0) {
                            for (int i = 0; i < m_lstAccountLeadStatuses.Count; i++)
                                m_lstAccountLeadStatuses[i].selected = false;
                            m_lstAccountLeadStatuses[_idx].selected = true;
                            //m_AccountLeadStatusSelectedIndex = _idx;
                        }

                        _efDbContext.Detach(_eftAccountAppt);
                    }

                    /**
                     * get contact/dialog status index for dropdown.
                     */
                    //m_ContactStatusSelectedIndex = 0;
                    if (m_BrightSalesProperty.CommonProperty.ContactId > 0) {
                        sub_campaign_contact_appointments _eftContactAppt = _efDbContext.sub_campaign_contact_appointments.FirstOrDefault(i =>
                            i.contact_id == m_BrightSalesProperty.CommonProperty.ContactId &&
                            i.final_list_id == m_BrightSalesProperty.CommonProperty.FinalListId
                        );

                        if (_eftContactAppt != null) {
                            int _idx = m_lstContactStatuses.FindIndex(i => i.status.Equals(_eftContactAppt.status));
                            if (_idx != null && _idx > 0) {
                                for (int i = 0; i < m_lstContactStatuses.Count; i++)
                                    m_lstContactStatuses[i].selected = false;
                                m_lstContactStatuses[_idx].selected = true;
                                //m_ContactStatusSelectedIndex = _idx;
                            }

                            _efDbContext.Detach(_eftContactAppt);
                        }
                    }

                    _efDbContext.Detach(_eftAccount);
                }

                m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId = Convert.ToInt32(m_objFollowUp.account_id);
                m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId = m_objFollowUp.contact_id == null ? 0 : Convert.ToInt32(m_objFollowUp.contact_id);
                m_BrightSalesProperty.CampaignBooking.Appointment = new CampaignBookingProperty.AppointmentProperty() {
                    CompanyWebsite = m_objFollowUp.company_website,
                    CompanyAppointmentStatus = m_objFollowUp.company_appoitment_status,
                    CompanyBoardNumber = m_objFollowUp.company_board_no,
                    CompanyAppointmentLeadStatus = m_objFollowUp.lead_status
                };

                m_BrightSalesProperty.CampaignBooking.AccountStatus = new CampaignBookingProperty.AccountStatusProperty() {
                    AccountLeadStatuses = m_lstAccountLeadStatuses,
                    AccountStatuses = m_lstAccountStatuses,
                    //AccountLeadStatusSelectedIndex = m_AccountLeadStatusSelectedIndex,
                    //AccountStatusSelectedIndex = m_AccountStatusSelectedIndex,
                    //AccountLeadStatusNotQualifiedIndex = m_AccountLeadStatusNotQualifiedIndex,
                    //AccountStatusNotQualifiedIndex = m_AccountStatusNotQualifiedIndex,
                    //AccountStatusSendEmailIndex = m_AccountStatusSendEmailIndex
                };
                m_BrightSalesProperty.CampaignBooking.ContactStatus = new CampaignBookingProperty.ContactStatusProperty() {
                    ContactStatuses = m_lstContactStatuses
                };
                //m_BrightSalesProperty.CampaignBooking.ContactStatus = new CampaignBookingProperty.ContactStatusProperty() {
                //    ContactStatuses = m_lstContactStatuses,
                //    ContactStatusSelectedIndex = m_ContactStatusSelectedIndex,
                //    ContactStatusNotQualifiedIndex = m_ContactStatusNotQualifiedIndex
                //};
                string _LastUpdateInfo = ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(m_objFollowUp.final_list_id, m_BrightSalesProperty.CommonProperty.AccountId);
                m_BrightSalesProperty.CampaignBooking.BreadCrumb = string.Format("{0}{1}{2}{3}",
                    string.IsNullOrEmpty(m_objFollowUp.company_name) ? "" : m_objFollowUp.company_name,
                    string.IsNullOrEmpty(m_objFollowUp.city) ? "" : ", " + m_objFollowUp.city,
                    string.IsNullOrEmpty(m_objFollowUp.country) ? "" : ", " + m_objFollowUp.country,
                    string.IsNullOrEmpty(_LastUpdateInfo) ? string.Empty : string.Format(" ({0})", _LastUpdateInfo)
                );
                m_BrightSalesProperty.CampaignBooking.CampaignInformation = string.Format("{0}, {1}>{2}>{3}",
                    m_objFollowUp.company_name,
                    m_objFollowUp.customer_name,
                    m_objFollowUp.campaign_name,
                    m_objFollowUp.sub_campaign_name
                );
                m_BrightSalesProperty.CampaignBooking.ToolTipInformation = string.Format("Customer: {5}{0}Campaign: {6}{0}SubCampaign: {7}{0}{0}Date: {1:yyyy-MM-dd}{0}Assigned User: {2}{0}Company Name: {3}{0}Contact: {8}{0}{0}Remarks: {4}",
                    Environment.NewLine,
                    (DateTime)m_objFollowUp.date_of_transaction,
                    m_objFollowUp.assigned_user,
                    m_objFollowUp.company_name,
                    m_objFollowUp.short_message,
                    m_objFollowUp.customer_name,
                    m_objFollowUp.campaign_name,
                    m_objFollowUp.sub_campaign_name,
                    m_objFollowUp.contact_name
                );
            }
        }
        private bool IsAccountLocked(GridView view, int row)
        {
            try {
                bool isLocked = (bool)view.GetRowCellValue(row, "locked");
                if (isLocked) return true;
                return false;
            }
            catch {
                return false;
            }
        }
        private byte UpdateFollowUpTotalTries(int id)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var log = _efDbContext.event_followup_log.FirstOrDefault(x => x.id == id);
                if (log != null) {
                    log.total_tries++;
                    _efDbContext.SaveChanges();
                    return log.total_tries;
                }
                return 0;
            }
        }
        private void EditEvent()
        {
            if (gvFollowUpList.RowCount < 1)
                return;

            CampaignBookingProperty.CampaignBoookingArguments _args = this.GetCampaignBookingArgs(false);
            if (_args == null || _args.oAppointment == null)
                return;

            CTMyFollowUps _item = gvFollowUpList.GetFocusedRow() as CTMyFollowUps;
            if (_item == null || _item.event_type.Equals("Call Log") || _item.event_type.Equals("Nurture Log"))
                return;

            bool _EditAllowed = false;
            if (_item.assigned_user.Equals(UserSession.CurrentUser.UserFullName) || _item.created_by.Equals(UserSession.CurrentUser.UserFullName))
                _EditAllowed = true;

            if (!_EditAllowed)
                return;

            WaitDialog.Show("Loading Data ...");

            #region Initialize Editor
            List<CTScSubCampaignContactList> _ContactList = new List<CTScSubCampaignContactList>();
            FollowUpEditor _control = new FollowUpEditor() {
                Dock = DockStyle.Fill,
                IsNurtureEvent = false
            };
            _control.btnSave_OnClick += new FollowUpEditor.btnSaveOnClickEventHandler(_control_btnSave_OnClick);
            PopupDialog _dlgEditor = new PopupDialog() {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Edit Current Follow Up",
                ClientSize = new Size(_control.Width + 2, _control.Height + 2),
                CloseBox = false
            };
            _dlgEditor.Controls.Add(_control);
            #endregion

            int _FinalListId;
            event_followup_log _data;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _data = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _args.Id);
                _FinalListId = (int)_efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == _data.subcampaign_id).id;
                _efDbContext.Detach(_data);
            }

            if (_data.event_type.Equals("Nurture Event"))
                _control.IsNurtureEvent = true;

            _control.SubCampaignId = _args.oAppointment.SubCampaignId;
            _control.AccountId = _args.oAppointment.AccountId;
            _control.Prepare();

            if (!_data.event_type.Equals("Nurture Event")) {
                _control.GetEventTypes(0, _data.event_type);
                _control.LoadSalesUsers((int)_data.subcampaign_id, (int)_data.assigned_user);
                _control.SetCampaignInfo(_args);
                _ContactList = ObjectSubCampaign.GetSubCampaignContacts(_args.oAppointment.SubCampaignId, _args.oAppointment.AccountId, _args.oAppointment.FinalListId);
            }
            else {
                _control.GetEventTypes((int)_data.source_sub_campaign_id);
                _control.SetSelectedEventType((int)_data.subcampaign_id);
                _control.LoadSalesUsers((int)_data.subcampaign_id, (int)_data.assigned_user);
                _control.SetCampaignInfo(_data);
                _ContactList = ObjectSubCampaign.GetSubCampaignContacts((int)_data.subcampaign_id, (int)_data.account_id, _FinalListId);
            }

            if (_ContactList.Count > 0) {
                _control.LoadContactPersons(_ContactList);
                CTScSubCampaignContactList _contact = _ContactList.Find(i => i.id == (int)_data.contact_id);
                if (_contact != null) {
                    _control.ContactPerson = _contact;
                    _control.LoadSelectedContact(false);
                }
            }

            WaitDialog.Close();
            _dlgEditor.ShowDialog(this);
        }
        private void GetAccountData()
        {
            m_CurrentAccount = null;
            m_objFollowUp = gvFollowUpList.GetFocusedRow() as CTMyFollowUps;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                m_CurrentAccount = _efDbContext.sub_campaign_account_lists.FirstOrDefault(p =>
                    p.account_id == m_objFollowUp.account_id &&
                    p.final_list_id == m_objFollowUp.final_list_id &&
                    p.active == true
                );
                if (m_CurrentAccount != null)
                    _efDbContext.Detach(m_CurrentAccount);
            }
        }
        private void WorkOnSelectedAccount()
        {
            if (gvFollowUpList.DataRowCount < 1)
                return;

            /**
             * validate if user is member of the sub campaign allowed users.
             */
            bool _AccountDeActivated = false;
            m_objFollowUp = gvFollowUpList.GetFocusedRow() as CTMyFollowUps;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                subcampaign_users _User = _efDbContext.subcampaign_users.FirstOrDefault(i =>
                    i.subcampaign_id == m_objFollowUp.subcampaign_id &&
                    i.user_id == UserSession.CurrentUser.UserId &&
                    i.internal_user == true
                );
                if (_User != null)
                    _efDbContext.Detach(_User);
                else {
                    NotificationDialog.Warning("Bright Sales", string.Format("You are not a member of this sub-campaign.{0}Please contact your administrator.", Environment.NewLine));
                    return;
                }

                _AccountDeActivated = false;
                m_objFollowUp = gvFollowUpList.GetFocusedRow() as CTMyFollowUps;
                account _eftAccountData = _efDbContext.accounts.FirstOrDefault(i => i.id == m_objFollowUp.account_id);
                sub_campaign_account_lists _eftListData = _efDbContext.sub_campaign_account_lists.FirstOrDefault(p =>
                    p.account_id == m_objFollowUp.account_id &&
                    p.final_list_id == m_objFollowUp.final_list_id &&
                    p.active == true
                );

                if (_eftAccountData != null) {
                    _efDbContext.Detach(_eftAccountData);
                    if (!_eftAccountData.active)
                        _AccountDeActivated = true;
                }
                if (_eftListData != null) {
                    _efDbContext.Detach(_eftListData);
                    if (!_eftListData.active)
                        _AccountDeActivated = true;
                }

                if (_AccountDeActivated) {
                    NotificationDialog.Warning("Bright Sales", "The account you are trying to load is already de-activated.");
                    return;
                }

                this.ReleaseCompanyLock();
                try {
                    this.GetAccountData();
                    bool islocked = m_CurrentAccount.locked;
                    int? lockedBy = m_CurrentAccount.locked_by;
                    cmdWorkWithCompany.Enabled = islocked && lockedBy != UserSession.CurrentUser.UserId ? false : true;

                    if ((islocked && lockedBy == UserSession.CurrentUser.UserId) || !islocked) {
                        m_CurrentAccount.locked = true;
                        m_CurrentAccount.locked_by = UserSession.CurrentUser.UserId;
                        m_CurrentAccount.locked_timestamp = _efDbContext.FIUpdateUserLock(
                            m_objFollowUp.final_list_id,
                            m_objFollowUp.account_id,
                            m_CurrentAccount.locked,
                            m_CurrentAccount.locked_by
                        ).FirstOrDefault();

                        gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "locked", m_CurrentAccount.locked);
                        gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "locked_by", m_CurrentAccount.locked_by);
                        gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "locked_user", UserSession.CurrentUser.UserFullName);
                        gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "locked_timestamp", m_CurrentAccount.locked_timestamp);

                        byte _totalTries = this.UpdateFollowUpTotalTries(m_objFollowUp.id);
                        if (_totalTries > 0)
                            gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "total_tries", _totalTries);

                        this.LoadCampaignBooking();
                    }
                    else {
                        gvFollowUpList.SetRowCellValue(gvFollowUpList.FocusedRowHandle, "locked", true);
                        NotificationDialog.Information("Bright Sales", "The selected company is currently edited by another user. Please try again later.");
                        m_CurrentAccount = null;
                    }
                }
                catch {
                    NotificationDialog.Error("Bright Sales", "Unable to load account. Perhaps already de-activated. Please consult System Administrator.");
                }
            }
        }
        private void ApplyFilter()
        {
            WaitDialog.Show(this.ParentForm, "Loading data ...");

            if (checkEditFilterDone.Checked)
                gvFollowUpList.Columns["done"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo("[done] = 'False'");
            else
                gvFollowUpList.Columns["done"].ClearFilter();

            if (checkEditCampaignOnly.Checked && m_BrightSalesProperty.CommonProperty.SubCampaignId > 0)
                gvFollowUpList.Columns["subcampaign_id"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo(string.Format("[subcampaign_id] = {0}", m_BrightSalesProperty.CommonProperty.SubCampaignId));
            else
                gvFollowUpList.Columns["subcampaign_id"].ClearFilter();

            /*
            if (checkEditShowMyEvent.Checked)
                gvFollowUpList.Columns["assigned_user"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo(string.Format("[assigned_user] = '{0}'", UserSession.CurrentUser.UserFullName));
            else if (checkEditShowTeamEvent.Checked)
                gvFollowUpList.Columns["assigned_user"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo("[assigned_user] = 'Team'");
            else
                gvFollowUpList.Columns["assigned_user"].ClearFilter();
            */

            /*
             * https://brightvision.jira.com/browse/PLATFORM-3146
             */
            if (cmbShowEvents.Text == "Show My Tasks")
                gvFollowUpList.Columns["assigned_user"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo(string.Format("[assigned_user] = '{0}'", UserSession.CurrentUser.UserFullName));
            else if (cmbShowEvents.Text == "Show Team Tasks")
                gvFollowUpList.Columns["assigned_user"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo("[assigned_user] = 'Team'");
            else
                gvFollowUpList.Columns["assigned_user"].ClearFilter();

            WaitDialog.Close();
        }
        #endregion

        #region Control Events
        private void checkEditApplyFilter_CheckedChanged(object sender, EventArgs e)
        {
            this.ApplyFilter();
        }
        public void cmdWorkWithCompany_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading data ...");
            this.WorkOnSelectedAccount();
            WaitDialog.Close();

            #region Old Code
            //if (gvFollowUpList.DataRowCount < 1)
            //    return;

            //if (!UserMemberOfSubCampaign()) {
            //    this.UserNotMemberOfSubCampaign();
            //    return;
            //}

            ////if (m_UserOnWorkMode)
            ////{
            ////    MessageBox.Show("You are currently working on a company. Please kindly close it first before working another company.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ////    return;
            ////}
            
            //WaitDialog.Show("Loading data ...");
            //this.ReleaseCurrentCompanyLock();
            //var view = gvFollowUpList;
            ////set enable property for button work company
            //try {
            //    this.GetAccountData();
            //    bool islocked = m_CurrentAccount.locked;
            //    int? lockedBy = m_CurrentAccount.locked_by;
            //    if (islocked && lockedBy != UserSession.CurrentUser.UserId)
            //        cmdWorkWithCompany.Enabled = false;
            //    else
            //        cmdWorkWithCompany.Enabled = true;

            //    if ((islocked && lockedBy == UserSession.CurrentUser.UserId) || !islocked) {
            //        //this.LoadCampaignBooking();
            //        m_CurrentAccount.locked = true;
            //        m_CurrentAccount.locked_by = UserSession.CurrentUser.UserId;
            //        m_CurrentAccount.locked_timestamp =
            //            BPContext.FIUpdateUserLock(
            //                    m_objFollowUp.final_list_id,
            //                    m_objFollowUp.account_id,
            //                    m_CurrentAccount.locked,
            //                    m_CurrentAccount.locked_by).FirstOrDefault();
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked", m_CurrentAccount.locked);
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked_by", m_CurrentAccount.locked_by);
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked_user", UserSession.CurrentUser.UserFullName);
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked_timestamp", m_CurrentAccount.locked_timestamp);
            //        byte totalTries = UpdateFollowUpTotalTries(m_objFollowUp.id);
            //        if (totalTries > 0)
            //            view.SetRowCellValue(view.FocusedRowHandle, "total_tries", totalTries);

            //        this.LoadCampaignBooking();
            //    }
            //    else {
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked", true);
            //        NotificationDialog.Information("Bright Sales", "The selected company is currently edited by another user. Please try again later.");
            //        m_CurrentAccount = null;
            //        //btnRefreshList_Click(null, null);
            //    }
            //}
            //catch { }
            //WaitDialog.Close(true);
            #endregion
        }
        private void cbxDone_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            CTMyFollowUps _item = gvFollowUpList.GetFocusedRow() as CTMyFollowUps;
            if (_item == null || _item.event_type.Equals("Call Log") || _item.event_type.Equals("Nurture Log"))
                e.Cancel = true;
        }
        private void cbxDone_CheckedChanged(object sender, EventArgs e)
        {
            if (m_MarkSelectedRowsAsDonePerformed)
                return;

            WaitDialog.Show(this.ParentForm, "Saving data...");
            CheckEdit Item = sender as CheckEdit;
            this.SetDoneFlag(Item.Checked);
            WaitDialog.Close();
        }
        private void btnRefreshList_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(this.ParentForm, "Loading data ...");
            if (cmbShowEvents.Text == "Show All Tasks In Sub Campaign")
            {
                LoadEvents(m_BrightSalesProperty.CommonProperty.SubCampaignId);
            }
            else
            {
                this.LoadEvents();
            }

            WaitDialog.Close();
        }
        private void btnReleaseLock_Click(object sender, EventArgs e)
        {
            m_objFollowUp = gvFollowUpList.GetFocusedRow() as CTMyFollowUps;
            if (m_objFollowUp == null)
                return;

            WaitDialog.Show(this.ParentForm, "Loading data...");
            if (gvFollowUpList.FocusedRowHandle < 0) {
                NotificationDialog.Information("Bright Sales", "There are no locked follow-ups to release.");
                WaitDialog.Close();
                return;
            }

            sub_campaign_account_lists _eftAccount = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftAccount = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                    i.account_id == m_objFollowUp.account_id &&
                    i.final_list_id == m_objFollowUp.final_list_id
                );
                if (_eftAccount != null)
                    _efDbContext.Detach(_eftAccount);
            }
            if (_eftAccount.locked && _eftAccount.locked_by == UserSession.CurrentUser.UserId) {
                _eftAccount = ObjectLocking.ReleaseLock((int)m_objFollowUp.final_list_id, (int)m_objFollowUp.account_id);
                m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
                btnRefreshList_Click(null, null);
            }
            else {
                WaitDialog.Close();
                NotificationDialog.Information("Bright Sales", "You can only release a lock if you worked on that company.");
                return;
            }

            WaitDialog.Close();
        }
        private void btnMarkAsDone_Click(object sender, EventArgs e)
        {
            if (gvFollowUpList.RowCount < 1)
                return;

            CTMyFollowUps _item = gvFollowUpList.GetFocusedRow() as CTMyFollowUps;
            if (_item == null || _item.event_type.Equals("Call Log") || _item.event_type.Equals("Nurture Log")) {
                NotificationDialog.Information("Bright Sales", "Logs cannot be marked as done.");
                return;
            }

            DialogResult _dlg = MessageBox.Show("Would you really like to mark these selected task as done?", "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlg == DialogResult.No)
                return;

            int[] _SelectedRows = gvFollowUpList.GetSelectedRows();
            if (_SelectedRows.Count() < 1) {
                NotificationDialog.Information("Bright Sales", "No selected rows.");
                return;
            }

            WaitDialog.Show(this.ParentForm, "Saving data ...");
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                m_MarkSelectedRowsAsDonePerformed = true;
                List<string> _lstFollowUpIds = new List<string>();
                for (int i = _SelectedRows.Count() - 1; i >= 0; i--) {
                    if (Convert.ToBoolean(gvFollowUpList.GetRowCellValue(_SelectedRows[i], "done")))
                        continue;

                    int _id = Convert.ToInt32(gvFollowUpList.GetRowCellValue(_SelectedRows[i], "id"));
                    _lstFollowUpIds.Add(_id.ToString());
                    gvFollowUpList.SetRowCellValue(_SelectedRows[i], "done", true);
                }
                gvFollowUpList.RefreshData();
                if (gvFollowUpList.RowCount > 0)
                    gvFollowUpList.SelectRows(0, 0);

                _efDbModel.FIUpdateMyFollowUps(string.Join(",", _lstFollowUpIds.ToArray()));
            }
            m_MarkSelectedRowsAsDonePerformed = false;
            WaitDialog.Close();
        }
        private void gridViewMyFollowups_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            SalesConsultant.Business.BrightSalesGridUtility.CreateGridContextMenu(view, e, true, new SalesConsultant.Business.BrightSalesGridUtility.AdditionalMenuItems() {
                EditEvent = true
            });

            SalesConsultant.Business.BrightSalesGridUtility.EditEventOnClick -= new SalesConsultant.Business.BrightSalesGridUtility.EditEventOnClickEventHandler(GridUtilityMenu_EditEventOnClick);
            SalesConsultant.Business.BrightSalesGridUtility.EditEventOnClick += new SalesConsultant.Business.BrightSalesGridUtility.EditEventOnClickEventHandler(GridUtilityMenu_EditEventOnClick);
        }
        private void gridViewMyFollowups_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || gvFollowUpList.DataRowCount < 1)
                return;

            WaitDialog.Show("Loading data ...");
            this.WorkOnSelectedAccount();
            WaitDialog.Close();
        }
        private void gridViewMyFollowups_DoubleClick(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading data ...");
            this.WorkOnSelectedAccount();
            WaitDialog.Close();
        }
        private void gridViewMyFollowups_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e == null || e.Value == null)
                return;

            if (e.Column.FieldName.Equals("date_of_transaction")) {
                e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyyy-MM-dd");
            }
            if (e.Column.FieldName.Equals("start_time") || e.Column.FieldName.Equals("end_time")) {
                var time = ((TimeSpan?)e.Value).Value.ToString();
                e.DisplayText = time.Substring(0, 5);
            }
        }
        private void gridViewMyFollowups_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (m_IsLoadingGridView)
                return;

            this.SetCampaignBookingParams();
            try {
                var view = (GridView)sender;
                bool islocked = (bool)view.GetRowCellValue(e.FocusedRowHandle, "locked");
                int? lockedBy = (int?)view.GetRowCellValue(e.FocusedRowHandle, "locked_by");
                if (islocked && lockedBy != UserSession.CurrentUser.UserId)
                    cmdWorkWithCompany.Enabled = false;
                else
                    cmdWorkWithCompany.Enabled = true;
            }
            catch { 
            }
        }
        private void gridViewMyFollowups_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (this.IsAccountLocked(gvFollowUpList, e.RowHandle)) {
                e.Appearance.BackColor = Color.LightGray;
                if (m_MyFollowUpLastWorkedAccountId == 0)
                    m_MyFollowUpLastWorkedAccountId = ValidationUtility.TryParseInt(gvFollowUpList.GetRowCellValue(e.RowHandle, "account_id").ToString());
            }
        }
        private void simpleButtonFind_Click(object sender, EventArgs e)
        {
            gvFollowUpList.FindFilterText = textEditSearch.Text;
        }
        private void simpleButtonClear_Click(object sender, EventArgs e)
        {
            gvFollowUpList.FindFilterText = "";
            textEditSearch.Text = string.Empty;
        }
        private void textEditSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                simpleButtonFind.PerformClick();
        }

        private void cmbShowEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbShowEvents.Text == "Show All Tasks In Sub Campaign")
            {
                if (m_BrightSalesProperty.CommonProperty.SubCampaignId < 1)
                {
                    NotificationDialog.Warning("Bright Sales", "No selected sub-campaign from campaign list combo.");
                    //LoadEvents(0);
                    this.cmbShowEvents.SelectedIndexChanged -= new System.EventHandler(this.cmbShowEvents_SelectedIndexChanged);
                    cmbShowEvents.SelectedIndex = int.Parse(ValidationUtility.IFNullString(cmbShowEvents.Tag, "0"));
                    this.cmbShowEvents.SelectedIndexChanged += new System.EventHandler(this.cmbShowEvents_SelectedIndexChanged);
                }
                else
                {
                    LoadEvents(m_BrightSalesProperty.CommonProperty.SubCampaignId);
                    checkEditCampaignOnly.Enabled = false;
                }
            }
            else
            {
                if (ValidationUtility.IFNullString(cmbShowEvents.Tag, "0") == "2") LoadEvents();

                checkEditCampaignOnly.Enabled = true;
                this.ApplyFilter();
            }

            cmbShowEvents.Tag = cmbShowEvents.SelectedIndex;
        }

        private void checkEditShowMyEvent_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkEditShowMyEvent.Checked)
            //    checkEditShowTeamEvent.Checked = false;

            //this.ApplyFilter();
        }
        private void checkEditShowTeamEvent_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkEditShowTeamEvent.Checked)
            //    checkEditShowMyEvent.Checked = false;

            //this.ApplyFilter();
        }
        private void toolTipController1_GetActiveObjectInfo(object sender, ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            var view = gvFollowUpList;
            if (view == null) 
                return;

            var info = view.CalcHitInfo(e.ControlMousePosition);
            CTMyFollowUps followUp = gvFollowUpList.GetRow(info.RowHandle) as CTMyFollowUps;
            if (followUp == null)
                return;

            if (info.InRow && prevFollowUp != followUp) {
                timer1.Stop();
                iTime = 0;

                int x = info.HitPoint.X + 20;
                int y = info.HitPoint.Y + 20;
                Point location = new Point { X = x, Y = y };
                string htmlToolTipContent = TooltipUtility.GetEventTooltip(followUp);
                htmlToolTip1.Show("Loading ...", htmlToolTipContent, this, location);
            }
            prevFollowUp = followUp;
        }
		private void btnEditEvent_Click(object sender, EventArgs e)
        {
            this.EditEvent();
        }
        private void checkEditCampaignOnly_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            if (m_BrightSalesProperty.CommonProperty.SubCampaignId < 1) {
                NotificationDialog.Warning("Bright Sales", "No selected sub-campaign from campaign list combo.");
                e.Cancel = true;
            }
        }
        private void GridUtilityMenu_EditEventOnClick()
        {
            this.EditEvent();
        }
		private void gcFollowUpList_MouseLeave(object sender, EventArgs e)
        {
            htmlToolTip1.Hide(this);
            timer1.Stop();
            iTime = 0;
        }
        private void htmlToolTip1_Popup(object sender, PopupEventArgs e)
        {
            iTime = 0;
            timer1.Start();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (iTime == 5) {
                timer1.Stop();
                htmlToolTip1.Hide(this);
                iTime = 0;
            }
            else iTime++;
        }
        #endregion    

        #region Subscribed Events
        private void _control_btnSave_OnClick(object sender, EventFollowUpLogEvents.OnSaveArguments e)
        {
            WaitDialog.Show("Loading data ...");
            this.LoadEvents();
            WaitDialog.Close();
        }
        #endregion        

    }
}
