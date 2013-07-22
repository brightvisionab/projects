
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SalesConsultant.Modules;
using BrightVision.Common.Modules;
using BrightVision.Common.Business;
using SalesConsultant.PublicProperties;
using BrightVision.Common.Events.Core;
using SalesConsultant.Facade;
using DevExpress.XtraGrid.Views.Grid;
using SalesConsultant.Events;

namespace SalesConsultant.Modules
{
    public partial class CampaignExtraDetail : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Event Handlers
        //public delegate void CallLogAfterDeleteEventHandler(object sender, CallLogAfterDeleteEventArgs e);
        //public event CallLogAfterDeleteEventHandler CallLogAfterDelete;
        //public class CallLogAfterDeleteEventArgs : EventArgs {
        //    public int DeletedId { get; set; }
        //}

        //public delegate void CompanyInformationSavedEventHandler(object sender, CompanyInformation.CompanyInformationArgs e);
        //public event CompanyInformationSavedEventHandler OnCompanyInformationSaved;

        //public delegate void CompanyInformationRefreshEventHandler(object sender, CompanyInformation.CompanyInformationArgs e);
        //public event CompanyInformationRefreshEventHandler OnCompanyInformationRefresh;
        
        public delegate void tcCampaignExtraDetailOnSelectedPageChangedHandler(object sender, CampaignExtraDetailArgs e);
        public event tcCampaignExtraDetailOnSelectedPageChangedHandler tcCampaignExtraDetail_OnSelectedPageChange;

        //public delegate void OnAddContactHandler(object sender, ContactInformation.ContactInformationArgs e);
        //public event OnAddContactHandler OnAddContact;

        //public delegate void OnSaveContactHandler(object sender, ContactInformation.ContactInformationArgs e);
        //public event OnSaveContactHandler OnSaveContact;

        //public delegate void OnCancelContactHandler(object sender, ContactInformation.ContactInformationArgs e);
        //public event OnCancelContactHandler OnCancelContact;

        //public delegate void OnDeleteContactHandler(object sender, ContactInformation.ContactInformationArgs e);
        //public event OnDeleteContactHandler OnDeleteContact;

        //public delegate void CallLogWorkNurtureEventEventHandler(MyFollowUps.CampaignBookingArgs e);
        //public event CallLogWorkNurtureEventEventHandler CallLog_WorkNurtureEvent;
        #endregion

        #region Public Event Arguments
        public class CampaignExtraDetailArgs : EventArgs
        {
            public eSelectedPage SelectedPage { get; set; }
        }
        #endregion

        #region Subscribed Events
        #region Contact Information
        //private void m_ContactInformation_btnDelete_OnClick(object sender, ContactInformation.ContactInformationArgs e)
        //{
        //    /**
        //     * event raiser
        //     */
        //    if (OnDeleteContact != null)
        //        OnDeleteContact(sender, e);
        //}
        //private void m_ContactInformation_btnCancel_OnClick(object sender, ContactInformation.ContactInformationArgs e)
        //{
        //    /**
        //     * event raiser
        //     */
        //    if (OnCancelContact != null)
        //        OnCancelContact(sender, e);
        //}
        //private void m_ContactInformation_btnSave_OnClick(object sender, ContactInformation.ContactInformationArgs e)
        //{
        //    /**
        //     * event raiser
        //     */
        //    if (OnSaveContact != null)
        //        OnSaveContact(sender, e);
        //}
        //private void m_ContactInformation_btnNew_OnClick(object sender, ContactInformation.ContactInformationArgs e)
        //{
        //    if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList)
        //        m_ContactInformation.SetAccountId(m_BrightSalesProperty.CommonProperty.AccountId);

        //    else if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignBooking)
        //        m_ContactInformation.SetAccountId(m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId);

        //    if (OnAddContact != null)
        //        OnAddContact(sender, e);
        //}
        #endregion

        #region Company Information
        //private void m_CompanyInformation_OnCompanyInformationSaved(object sender, CompanyInformation.CompanyInformationArgs e)
        //{
        //    if (m_CompanyWebsite != null)
        //        m_CompanyWebsite.ReloadPage = true;

        //    if (m_BrightSalesProperty.CampaignBooking.Appointment != null)
        //        m_BrightSalesProperty.CampaignBooking.Appointment.CompanyWebsite = e.Account.www;

        //    //if (m_CampaignBookingArgs != null)
        //    //    m_CampaignBookingArgs.CampaignBookingAppointment.CompanyWebsite = e.Account.www;

        //    if (OnCompanyInformationSaved != null)
        //        OnCompanyInformationSaved(sender, e);
        //}


        //private void m_CompanyInformation_OnCompanyInformationRefresh(object sender, CompanyInformation.CompanyInformationArgs e)
        //{
        //    if (OnCompanyInformationRefresh != null)
        //        OnCompanyInformationRefresh(sender, e);
        //}
        #endregion

        #region Event & Follow Up Logs
        //private void m_CallLog_AfterDelete(object sender, EventFollowUpLog.AfterDeleteArgs e)
        //{
        //    if (CallLogAfterDelete != null)
        //        CallLogAfterDelete(this, new CallLogAfterDeleteEventArgs() { DeletedId = e.DeletedId });
        //}
        #endregion
        #endregion

        #region Constructors
        public CampaignExtraDetail(bool IsCampaignListModule = false)
        {
            InitializeComponent();
            this.RegisterEvents();
            this.SetModuleState();

            tcCampaignExtraDetail.TabPages[8].PageVisible = false; // bv standard questions
            if (!IsCampaignListModule) {
                tcCampaignExtraDetail.TabPages[9].PageVisible = false; // map company
                tcCampaignExtraDetail.TabPages[2].PageVisible = false; // call log
                tcCampaignExtraDetail.SelectedTabPage = tabCollectedData;                
                this.DisplaySelectedTabPage();
            }
            else {
                tcCampaignExtraDetail.TabPages.Move(1, tabCompanyInformation);
                tcCampaignExtraDetail.SelectedTabPage = tabCompanyWebsite;
            }
        }
        #endregion

        #region Public Properties
        public bool ReloadCallLog = false;
        public bool ReloadCollectedData = false;
        public bool ReloadContacInformationtData { get; set; }

        /**
         * lets this module know when to load specific company/contact map location
         */
        public bool ShowSpecificCompanyMapLocation { get; set; }
        public bool ShowSpecificContactMapLocation { get; set; }

        public enum eSelectedPage
        {
            ContactInformation,
            CompanyInfortmation,
            CallLog,
            BookingSchedule,
            CollectedData,
            CompanyWebsite,
            GoogleSearch,
            LinkedIn,
            BvStandardQuestion,
            MapCompany,
            MapContact,
            BvSalesScript,
            MySalesScript,
            None
        }

        /**
         * we should know what module is calling the methods here
         */
        //public CallSources CallSource = CallSources.None;
        //public enum CallSources
        //{
        //    CampaignList,
        //    CampaignBooking,
        //    None
        //}

        public int CompanyInformation_AccountId {
            get {
                if (m_CompanyInformation != null)
                    return m_CompanyInformation.AccountId;
                else
                    return 0;
            }
        }
        public int CompanyInformation_FinalListId {
            get {
                if (m_CompanyInformation != null)
                    return m_CompanyInformation.FinalListId;
                else
                    return 0;
            }
        }

        private SelectionProperty.CallingEnvironment m_CallingEnvironment = SelectionProperty.CallingEnvironment.None;
        public SelectionProperty.CallingEnvironment CallingEnvironment
        {
            get { return m_CallingEnvironment; }
            set { m_CallingEnvironment = value; }
        }
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private ContactInformation m_ContactInformation = null;
        private CompanyInformation m_CompanyInformation = null;
        private ContactView m_ContactView = null;
        private EventFollowUpLog m_CallLog = null;
        private BookingSchedules m_BookingSchedule = null;
        private CollectedData m_CollectedData = null;
        private CompanyWebsite m_CompanyWebsite = null;
        private GoogleSearch m_GoogleSearch = null;
        private LinkedIn m_LinkedIn = null;
        private BvStandardQuestion m_BvStandardQuestion = null;
        private MapCompany m_MapCompany = null;
        private MapContact m_MapContact = null;
        private BvSalesScript m_BvSalesScript = null;
        private MySalesScript m_MySalesScript = null;
        private eSelectedPage m_SelectedPage = eSelectedPage.None;
        //private ManageCampaignBooking.CampaignBookingArgs m_CampaignBookingArgs = null;
        //private ManageCampaignBooking.eCampaignBookingMode m_BookingMode = ManageCampaignBooking.eCampaignBookingMode.None;
        private SelectionProperty.DialogSaveMode m_DialogEditorSaveMode = SelectionProperty.DialogSaveMode.Unspecified;
        private bool m_AllowSave = true;
        #endregion

        #region Public Methods
        /**
         * this object method calls
         */
        public void SetModuleSaving(bool pAllowSave)
        {
            if (m_ContactInformation != null)
                m_ContactInformation.AllowSaving = pAllowSave;

            if (m_CompanyInformation != null)
                m_CompanyInformation.AllowSaving = pAllowSave;

            if (m_BvSalesScript != null)
                m_BvSalesScript.AllowSaving = pAllowSave;

            if (m_MySalesScript != null)
                m_MySalesScript.AllowSaving = pAllowSave;

            m_AllowSave = pAllowSave;
        }
        public void LoadSelectedPage(eSelectedPage pSelectedTabPage = eSelectedPage.None)
        {
            /**
             * if reload collected data, and collected data is not the current tab,
             * just bypass loading of selected page.
             */
            if (ReloadCollectedData && tcCampaignExtraDetail.SelectedTabPage != tabCollectedData) {
                ReloadCollectedData = false;
                return;
            }

            switch (pSelectedTabPage) 
            {
                case eSelectedPage.ContactInformation:
                    tcCampaignExtraDetail.SelectedTabPage = tabContactInformation;
                    break;
                case eSelectedPage.CompanyInfortmation:
                    tcCampaignExtraDetail.SelectedTabPage = tabCompanyInformation;
                    break;
                case eSelectedPage.CallLog:
                    tcCampaignExtraDetail.SelectedTabPage = tabCallLog;
                    break;
                case eSelectedPage.BookingSchedule:
                    tcCampaignExtraDetail.SelectedTabPage = tabBooking;
                    break;
                case eSelectedPage.CollectedData:
                    tcCampaignExtraDetail.SelectedTabPage = tabCollectedData;
                    break;
                case eSelectedPage.CompanyWebsite:
                    tcCampaignExtraDetail.SelectedTabPage = tabCompanyWebsite;
                    break;
                case eSelectedPage.GoogleSearch:
                    tcCampaignExtraDetail.SelectedTabPage = tabGoogleSearch;
                    break;
                //case eSelectedPage.BvStandardQuestion:
                //    tcCampaignExtraDetail.SelectedTabPage = tabBvStandardQuestion;
                //    break;
                case eSelectedPage.MapCompany:
                    tcCampaignExtraDetail.SelectedTabPage = tabMapCompany;
                    break;
                case eSelectedPage.MapContact:
                    tcCampaignExtraDetail.SelectedTabPage = tabMapContact;
                    break;
                case eSelectedPage.BvSalesScript:
                    tcCampaignExtraDetail.SelectedTabPage = tabBvSalesScript;
                    break;
                case eSelectedPage.MySalesScript:
                    tcCampaignExtraDetail.SelectedTabPage = tabMySalesScript;
                    break;
            }

            /**
             * important: SetCampaignBookingArgs() must be set first on the calling environment 
             */
            this.DisplaySelectedTabPage();
        }
        //public void SetCampaignBookingArgs()
        //{
        //    m_CampaignBookingArgs = new ManageCampaignBooking.CampaignBookingArgs();
        //    m_CampaignBookingArgs.ContactId = pContactId;
        //    m_CampaignBookingArgs.CampaignBookingAppointment = pAppointment;
        //    //m_CampaignBookingArgs.CampaignBookingMode = ;
        //}
        //public void SetCampaignBookingArgs(SubCampaignAppointment pAppointment, int pContactId = 0)
        //{
        //    m_CampaignBookingArgs = new ManageCampaignBooking.CampaignBookingArgs();
        //    m_CampaignBookingArgs.ContactId = pContactId;
        //    m_CampaignBookingArgs.CampaignBookingAppointment = pAppointment;
        //    m_CampaignBookingArgs.CampaignBookingMode = SelectionProperty.CampaignListMode.None;
        //}
        public void SetModuleState()
        {
            if (m_BrightSalesProperty.CommonProperty.IsEmptyCampaignList)
                this.Enabled = false;
            else
                this.Enabled = true;

            //if (m_CampaignBookingArgs == null)
            //    this.Enabled = false;
            //else
            //    this.Enabled = true;
        }
        public void SetDialogEditorSaveMode(SelectionProperty.DialogSaveMode pSaveMode)
        {
            m_DialogEditorSaveMode = pSaveMode;
            if (m_ContactInformation != null)
                m_ContactInformation.SetDialogEditorSaveMode(pSaveMode);
        }

        public void SetModulesAsReadOnly(bool status) 
        {
            if (m_ContactInformation != null)
                m_ContactInformation.SetAsReadOnly(status);

            if (m_CompanyInformation != null)
                m_CompanyInformation.SetAsReadOnly(status);

            if (m_BvStandardQuestion != null)
                m_BvStandardQuestion.SetAsReadOnly(status);

            if (m_CollectedData != null)
                m_CollectedData.SetAsReadOnly(status);

            if (m_MapCompany != null)
                m_MapCompany.SetAsReadOnly(status);

            if (m_MapContact != null)
                m_MapContact.SetAsReadOnly(status);

            if (m_BvSalesScript != null)
                m_BvSalesScript.SetAsReadOnly(status);

            if (m_MySalesScript != null)
                m_MySalesScript.SetAsReadOnly(status);
        }
        public void ResetContactParams()
        {
            if (m_ContactInformation != null)
                m_ContactInformation.ResetParamaters();
        }

        public void SetState()
        {
            //if (pMode == ManageCampaignBooking.eCampaignBookingMode.BrowseMode)
            //    tcCampaignExtraDetail.SelectedTabPage = tabContactInformation;

            //m_BookingMode = pMode;
            if (m_ContactInformation != null)
                m_ContactInformation.SetState();
        }
        public void SetDefaultTab(bool CalledByManageCampaignList = false)
        {
            if (!CalledByManageCampaignList)
                tcCampaignExtraDetail.SelectedTabPage = tabCollectedData;
                //tcCampaignExtraDetail.SelectedTabPage = tabContactInformation;
            else
                tcCampaignExtraDetail.SelectedTabPage = tabCompanyWebsite;
        }
        public void SetCompanyDetails(int pAccountId)
        {
            if (m_CompanyInformation != null)
                m_CompanyInformation.SetCompanyDetails(pAccountId);
        }
        public void SetCompanyRemarks(string pRemarks)
        {
            if (m_CompanyInformation != null)
                m_CompanyInformation.SetCompanyRemarks(pRemarks);
        }

        public void ContactActiveChanged(bool pIsActive)
        {
            if (m_ContactInformation != null)
                m_ContactInformation.ActiveChanged(pIsActive);
        }
        public void UpdateStatisticsData()
        {
            if (tcCampaignExtraDetail.SelectedTabPage == tabCompanyInformation)
                m_CompanyInformation.UpdateStatisticsData();
            else if (tcCampaignExtraDetail.SelectedTabPage == tabContactInformation)
                m_ContactInformation.UpdateStatisticsData();
        }
        public void DeleteCallLog(int pCallLogId)
        {
            if (m_CallLog != null)
                m_CallLog.Delete(pCallLogId);
        }
        public void UpdateContactData(int pContactId)
        {
            if (m_ContactInformation != null)
                m_ContactInformation.UpdateContactData(pContactId);
        }

        public void RefreshBvStandardQuestion()
        {
            if (m_BvStandardQuestion != null && m_BrightSalesProperty.CommonProperty != null)
            {
                int _AccountId = 0;
                int _ContactId = 0;
                int _FinalListId = m_BrightSalesProperty.CommonProperty.FinalListId;
                int _SubCampaignId = m_BrightSalesProperty.CommonProperty.SubCampaignId;
                int _CampaignId = m_BrightSalesProperty.CommonProperty.CampaignId;
                int _CustomerId = m_BrightSalesProperty.CommonProperty.CustomerId;

                if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList)
                {
                    _AccountId = m_BrightSalesProperty.CommonProperty.AccountId;
                    _ContactId = m_BrightSalesProperty.CommonProperty.ContactId;
                }
                else if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignBooking)
                {
                    _AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId;
                    _ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId;
                }

                m_BvStandardQuestion.Show(_SubCampaignId, _AccountId, _ContactId);
            }
        }

        public void ClearData()
        {
            if (m_CompanyInformation != null)
                m_CompanyInformation.Clear();

            if (m_BookingSchedule != null)
                m_BookingSchedule.Clear();

            if (m_BvSalesScript != null)
                m_BvSalesScript.Clear();

            if (m_BvStandardQuestion != null)
                m_BvStandardQuestion.Clear();

            if (m_CollectedData != null)
                m_CollectedData.Clear();

            if (m_CompanyWebsite != null)
                m_CompanyWebsite.Clear();

            if (m_ContactInformation != null)
                m_ContactInformation.Clear();

            if (m_MapCompany != null)
                m_MapCompany.Clear();

            if (m_MapContact != null)
                m_MapContact.Clear();

            if (m_MySalesScript != null)
                m_MySalesScript.Clear();
        }
        #endregion

        #region Private Methods
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<CompanyInformationEvents.OnSave.CampaignExtraDetail>().Subscribe(CompanyInformationOnSave);
            m_EventBus.GetEvent<ContactInformationEvents.OnAddNew>().Subscribe(ContactInformationOnAdd);
        }
        private void ContactInformationOnAdd(ContactInformationEvents.OnAddNew e)
        {
            if (m_ContactInformation != null)
            {
                if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList)
                    m_ContactInformation.SetAccountId(m_BrightSalesProperty.CommonProperty.AccountId);

                else if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignBooking)
                    m_ContactInformation.SetAccountId(m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId);
            }

            //if (OnAddContact != null)
            //    OnAddContact(sender, e);
        }
        private void CompanyInformationOnSave(CompanyInformationEvents.OnSave.CampaignExtraDetail e)
        {
            if (m_CompanyWebsite != null)
                m_CompanyWebsite.ReloadPage = true;

            if (m_BrightSalesProperty.CampaignBooking.Appointment != null)
                m_BrightSalesProperty.CampaignBooking.Appointment.CompanyWebsite = e.OnSaveArgs.Account.www;

            //if (OnCompanyInformationSaved != null)
            //    OnCompanyInformationSaved(new object(), e);
        }        

        private void DisplaySelectedTabPage()
        {
            m_SelectedPage = eSelectedPage.None;
            WaitDialog.Show(ParentForm, "Loading components...");

            int _AccountId = 0;
            int _ContactId = 0;
            int _FinalListId = m_BrightSalesProperty.CommonProperty.FinalListId;
            int _SubCampaignId = m_BrightSalesProperty.CommonProperty.SubCampaignId;
            int _CampaignId = m_BrightSalesProperty.CommonProperty.CampaignId;
            int _CustomerId = m_BrightSalesProperty.CommonProperty.CustomerId;

            if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList) {
                _AccountId = m_BrightSalesProperty.CommonProperty.AccountId;
                _ContactId = m_BrightSalesProperty.CommonProperty.ContactId;
            }
            else if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignBooking) {
                _AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId;
                _ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId;
            }

            #region Contact Information
            if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabContactInformation)) {
                m_SelectedPage = eSelectedPage.ContactInformation;
                if (m_ContactInformation == null) {
                    m_ContactInformation = new ContactInformation(m_CallingEnvironment);
                    //m_ContactInformation.btnNew_OnClick += new ContactInformation.btnNewOnClickHandler(m_ContactInformation_btnNew_OnClick);
                    //m_ContactInformation.btnSave_OnClick += new ContactInformation.btnSaveOnClickHandler(m_ContactInformation_btnSave_OnClick);
                    //m_ContactInformation.btnCancel_OnClick += new ContactInformation.btnCancelOnClickHandler(m_ContactInformation_btnCancel_OnClick);
                    //m_ContactInformation.btnDelete_OnClick += new ContactInformation.btnDeleteOnClickHandler(m_ContactInformation_btnDelete_OnClick);
                    m_ContactInformation.Dock = DockStyle.Fill;
                    pnlContact.Dock = DockStyle.Fill;
                    pnlContact.Controls.Clear();
                    pnlContact.Controls.Add(m_ContactInformation);
                }

                m_ContactInformation.AllowSaving = m_AllowSave;
                if (m_BrightSalesProperty.CommonProperty != null)
                {
                    ///*DAN: https://brightvision.jira.com/browse/PLATFORM-2448
                    // * Added instance on the ContactView modules so that it gets actualy focused row values
                    //*/
                    ////-----------------------------------------------------------------
                    //if (m_ContactView == null)
                    //{
                    //    m_ContactView = ContactView.Instance();
                    //    if (m_ContactView.gvContact.DataRowCount > 0)
                    //    {
                    //        GridView view = m_ContactView.gcContact.FocusedView as GridView;
                    //        int.TryParse(view.GetRowCellValue(view.FocusedRowHandle, "id").ToString(), out _ContactId);
                    //    }
                    //}
                    ////-----------------------------------------------------------------

                    if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList && m_BrightSalesProperty.CampaignList.CampaignListMode == SelectionProperty.CampaignListMode.CompaniesOnly)
                        m_ContactInformation.Disable();
                    else
                    {
                        m_ContactInformation.ResetParamaters();
                        m_ContactInformation.Show(_ContactId, _FinalListId, ReloadContacInformationtData, _AccountId);
                    }
                }

                if (m_DialogEditorSaveMode == SelectionProperty.DialogSaveMode.Edit)
                    m_ContactInformation.SetAsReadOnly(true);

                ReloadContacInformationtData = false;
            }
            #endregion
            #region Company Information
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabCompanyInformation)) {
                m_SelectedPage = eSelectedPage.CompanyInfortmation;
                if (m_CompanyInformation == null) {
                    m_CompanyInformation = new CompanyInformation(m_CallingEnvironment);
                    //m_CompanyInformation.OnCompanyInformationSaved += new CompanyInformation.CompanyInformationSavedEventHandler(m_CompanyInformation_OnCompanyInformationSaved);
                    //m_CompanyInformation.OnCompanyInformationRefresh += new CompanyInformation.CompanyInformationRefreshEventHandler(m_CompanyInformation_OnCompanyInformationRefresh);
                    m_CompanyInformation.Dock = DockStyle.Fill;
                    pnlCompany.Dock = DockStyle.Fill;
                    pnlCompany.Controls.Clear();
                    pnlCompany.Controls.Add(m_CompanyInformation);
                }

                m_CompanyInformation.AllowSaving = m_AllowSave;
                if (m_BrightSalesProperty.CommonProperty != null)
                    m_CompanyInformation.Show(_AccountId, _FinalListId);

                if (m_DialogEditorSaveMode == SelectionProperty.DialogSaveMode.Edit)
                    m_CompanyInformation.SetAsReadOnly(true);
            }
            #endregion
            #region Event & Follow Up Log
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabCallLog))  {
                m_SelectedPage = eSelectedPage.CallLog;
                if (m_CallLog == null) {
                    m_CallLog = new EventFollowUpLog();
                    m_CallLog.Dock = DockStyle.Fill;
                    m_CallLog.HideAdditionalPanels();
                    //m_CallLog.AfterDelete += new EventFollowUpLog.AfterDeleteEventHandler(m_CallLog_AfterDelete);
                    //m_CallLog.WorkNurtureEvent += new EventFollowUpLog.WorkNurtureEventEventHandler(m_CallLog_WorkNurtureEvent);
                    pnlCallLog.Dock = DockStyle.Fill;
                    pnlCallLog.Controls.Clear();
                    pnlCallLog.Controls.Add(m_CallLog);
                }
                
                if (m_BrightSalesProperty.CommonProperty != null) {
                    if (ReloadCallLog) {
                        m_CallLog.ReloadData = true;
                        ReloadCallLog = false;
                    }
                    m_CallLog.AllowSaving = m_AllowSave;
                    m_CallLog.Show(_SubCampaignId, _AccountId);
                }
            }
            #endregion
            #region Booking Schedule
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabBooking))
            {
                /**
                 * if no sub-campaign assigned yet, just bypass
                 */
                if (m_BrightSalesProperty.CommonProperty == null) {
                    WaitDialog.Close();
                    return;
                }

                m_SelectedPage = eSelectedPage.BookingSchedule;
                if (m_BookingSchedule == null) {
                    m_BookingSchedule = new BookingSchedules();
                    m_BookingSchedule.Dock = DockStyle.Fill;
                    pnlBookingSchedule.Dock = DockStyle.Fill;
                    pnlBookingSchedule.Controls.Clear();
                    pnlBookingSchedule.Controls.Add(m_BookingSchedule);
                }

                if (m_BrightSalesProperty.CommonProperty != null)
                    m_BookingSchedule.Show(_SubCampaignId);
            }
            #endregion
            #region Collected Data
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabCollectedData))
            {
                /**
                 * reload logic should always come first, not unless with reason to put
                 * in betwwen the logic or in last. 
                 * 
                 * as for like contact information, reloading of data happens on the Show() method.
                 * here we implement it as a separate call for easier tracing ang maintenance.
                 */
                if (ReloadCollectedData) {
                    if (m_BrightSalesProperty.CommonProperty != null && m_CollectedData != null)
                        m_CollectedData.Reload();

                    ReloadCollectedData = false;
                }
                else {
                    m_SelectedPage = eSelectedPage.CollectedData;
                    if (m_CollectedData == null) {
                        m_CollectedData = new CollectedData();
                        m_CollectedData.Dock = DockStyle.Fill;
                        pnlCollectedData.Dock = DockStyle.Fill;
                        pnlCollectedData.Controls.Clear();
                        pnlCollectedData.Controls.Add(m_CollectedData);
                    }

                    if (m_BrightSalesProperty.CommonProperty != null) {
                        m_CollectedData.AccountId = _AccountId;
                        m_CollectedData.SubCampaignId = _SubCampaignId;
                        m_CollectedData.CampaignId = _CampaignId;
                        m_CollectedData.CustomerId = _CustomerId;
                        m_CollectedData.ContactId = null;
                        if (_ContactId > 0)
                            m_CollectedData.ContactId = _ContactId;
                        
                        if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList &&
                            m_BrightSalesProperty.CampaignList.CampaignListMode == SelectionProperty.CampaignListMode.CampaniesAndContacts) {
                            m_CollectedData.CollectedDataFilterByContact = true;
                            m_CollectedData.SetContactCollectedDataButton(false);
                        }

                        if (_ContactId > 0)
                            m_CollectedData.ContactId = _ContactId;

                        m_CollectedData.Activate();
                        m_CollectedData.Show();
                    }

                    if (m_DialogEditorSaveMode == SelectionProperty.DialogSaveMode.Edit)
                        m_CollectedData.SetAsReadOnly(true);
                }
            }
            #endregion
            #region Company Website
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabCompanyWebsite))
            {
                m_SelectedPage = eSelectedPage.CompanyWebsite;
                if (m_CompanyWebsite == null) {
                    m_CompanyWebsite = new CompanyWebsite();
                    m_CompanyWebsite.Dock = DockStyle.Fill;
                    pnlCompanyWebsite.Dock = DockStyle.Fill;
                    pnlCompanyWebsite.Controls.Clear();
                    pnlCompanyWebsite.Controls.Add(m_CompanyWebsite);
                }

                if (m_BrightSalesProperty.CommonProperty != null)
                    m_CompanyWebsite.Show(_AccountId);
            }
            #endregion
            #region Google Search
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabGoogleSearch))
            {
                m_SelectedPage = eSelectedPage.GoogleSearch;
                if (m_GoogleSearch == null)
                {
                    m_GoogleSearch = new GoogleSearch();
                    m_GoogleSearch.Dock = DockStyle.Fill;
                    pnlGoogleSearch.Dock = DockStyle.Fill;
                    pnlGoogleSearch.Controls.Clear();
                    pnlGoogleSearch.Controls.Add(m_GoogleSearch);
                    m_GoogleSearch.LoadGoogleSearch();
                }
            }
            #endregion
            #region LinkedIn
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabLinkedIn))
            {
                m_SelectedPage = eSelectedPage.LinkedIn;
                if (m_LinkedIn == null)
                {
                    m_LinkedIn = new LinkedIn();
                    m_LinkedIn.Dock = DockStyle.Fill;
                    pnlLinkedIn.Dock = DockStyle.Fill;
                    pnlLinkedIn.Controls.Clear();
                    pnlLinkedIn.Controls.Add(m_LinkedIn);
                }

                 if (m_BrightSalesProperty.CommonProperty != null)
                     m_LinkedIn.LoadLinkedIn(_AccountId, _ContactId);
            }
            #endregion
            #region Brightvision Standard Question
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabBvStandardQuestion)) {
                //m_SelectedPage = eSelectedPage.BvStandardQuestion;
                //if (m_BvStandardQuestion == null) {
                //    m_BvStandardQuestion = new BvStandardQuestion();
                //    m_BvStandardQuestion.Dock = DockStyle.Fill;
                //    pnlBvStandardQuestion.Dock = DockStyle.Fill;
                //    pnlBvStandardQuestion.Controls.Clear();
                //    pnlBvStandardQuestion.Controls.Add(m_BvStandardQuestion);
                //}

                //if (m_BrightSalesProperty.CommonProperty != null)
                //    m_BvStandardQuestion.Show(_SubCampaignId, _AccountId, _ContactId);
            }
            #endregion
            #region Map Company
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabMapCompany)) {
                m_SelectedPage = eSelectedPage.MapCompany;
                if (m_MapCompany == null) {
                    m_MapCompany = new MapCompany();
                    m_MapCompany.Dock = DockStyle.Fill;
                    pnlMapCompany.Dock = DockStyle.Fill;
                    pnlMapCompany.Controls.Clear();
                    pnlMapCompany.Controls.Add(m_MapCompany);
                }

                if (m_BrightSalesProperty.CommonProperty != null) {
                    m_MapCompany.SetParameters(_FinalListId);
                    m_MapCompany.PrepareLocations();
                }

                if (!ShowSpecificCompanyMapLocation)
                    m_MapCompany.Show(false, null);
                else
                    m_MapCompany.Show(true, _AccountId);

                ShowSpecificCompanyMapLocation = false;
            }
            #endregion
            #region Map Contact
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabMapContact)) {
                m_SelectedPage = eSelectedPage.MapContact;
                if (m_MapContact == null) {
                    m_MapContact = new MapContact();
                    m_MapContact.Dock = DockStyle.Fill;
                    pnlMapContact.Dock = DockStyle.Fill;
                    pnlMapContact.Controls.Clear();
                    pnlMapContact.Controls.Add(m_MapContact);
                }

                if (m_BrightSalesProperty.CommonProperty != null) {
                    m_MapContact.SetParameters(_FinalListId, _AccountId);
                    m_MapContact.PrepareLocations();
                }

                if (!ShowSpecificContactMapLocation)
                    m_MapContact.Show(false, null);
                else {
                    /**
                     * if call has been made from campaign list
                     */
                    if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList) {
                        if (m_BrightSalesProperty.CampaignList.CampaignListMode == SelectionProperty.CampaignListMode.CompaniesOnly)
                            m_MapContact.Show(false, null);
                        else
                            m_MapContact.Show(true, _ContactId);
                    }

                    /**
                     * if call has been made from campaign booking
                     */
                    else if (CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignBooking)
                        m_MapContact.Show(true, _ContactId);
                }

                ShowSpecificContactMapLocation = false;
            }
            #endregion
            #region Brightvision Sales Script
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabBvSalesScript)) {
                m_SelectedPage = eSelectedPage.BvSalesScript;
                if (m_BvSalesScript == null) {
                    m_BvSalesScript = new BvSalesScript();
                    m_BvSalesScript.Dock = DockStyle.Fill;
                    pnlBvSalesScript.Dock = DockStyle.Fill;
                    pnlBvSalesScript.Controls.Clear();
                    pnlBvSalesScript.Controls.Add(m_BvSalesScript);
                }

                m_BvSalesScript.AllowSaving = m_AllowSave;
                if (m_BrightSalesProperty.CommonProperty != null)
                    m_BvSalesScript.Show(_FinalListId);
            }
            #endregion
            #region My Sales Script
            else if (tcCampaignExtraDetail.SelectedTabPage.Equals(tabMySalesScript))
            {
                m_SelectedPage = eSelectedPage.MySalesScript;
                if (m_MySalesScript == null)
                {
                    m_MySalesScript = new MySalesScript();
                    m_MySalesScript.Dock = DockStyle.Fill;
                    pnlMySalesScript.Dock = DockStyle.Fill;
                    pnlMySalesScript.Controls.Clear();
                    pnlMySalesScript.Controls.Add(m_MySalesScript);
                }

                m_MySalesScript.AllowSaving = m_AllowSave;
                if (m_BrightSalesProperty.CommonProperty != null)
                    m_MySalesScript.Show(m_BrightSalesProperty.CommonProperty.FinalListId);
            }
            #endregion

            WaitDialog.Close();
        }
        private CampaignExtraDetailArgs GetArguments()
        {
            CampaignExtraDetailArgs _Args = new CampaignExtraDetailArgs();
            _Args.SelectedPage = m_SelectedPage;
            return _Args;
        }
        #endregion

        #region Object Events
        //public void m_CallLog_WorkNurtureEvent(MyFollowUps.CampaignBookingArgs e)
        //{
        //    if (CallLog_WorkNurtureEvent != null)
        //        CallLog_WorkNurtureEvent(e);
        //}
        private void tcCampaignExtraDetail_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            this.DisplaySelectedTabPage();

            /**
             * event raiser
             */
            if (tcCampaignExtraDetail_OnSelectedPageChange != null)
                tcCampaignExtraDetail_OnSelectedPageChange(this, this.GetArguments());
        }
        private void tcCampaignExtraDetail_SelectedPageChanging(object sender, DevExpress.XtraTab.TabPageChangingEventArgs e)
        {
            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse)
                e.Cancel = true;

            /**
             * [jeff 05.14.2012]: https://brightvision.jira.com/browse/PLATFORM-1401
             * commented as per ticket description
             */
            //else if (m_DialogEditorSaveMode == SaveModeEnum.Edit)
            //    e.Cancel = true;
        }
        #endregion
    }
}