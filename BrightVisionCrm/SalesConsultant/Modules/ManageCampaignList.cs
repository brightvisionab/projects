
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Common.Business;
using BrightVision.Common.Events.Core;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using SalesConsultant.Events;

namespace SalesConsultant.Modules
{
    public partial class ManageCampaignList : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public ManageCampaignList()
        {
            this.Visible = false;
            InitializeComponent();
            this.InitializeCampaignListModule();
            this.Visible = true;

            this.RegisterEvents();
        }
        #endregion

        #region Public Event Handlers
        public delegate void CampaignExtraDetailOnContactSavedEventHandler();
        public event CampaignExtraDetailOnContactSavedEventHandler CampaignExtraDetail_OnContactSaved;

        //public delegate void CampaignListCompanyOnWorkByAnotherConsultantEventHandler(CampaignList.CompanyOnWorkByAnotherConsultantArgs e);
        //public event CampaignListCompanyOnWorkByAnotherConsultantEventHandler CampaignList_CompanyOnWorkByAnotherConsultant;
        
        //public delegate void CampaignExtraDetail_OnCompanyInformationSavedEventHandler();
        //public event CampaignExtraDetail_OnCompanyInformationSavedEventHandler CampaignExtraDetail_OnCompanyInformationSaved;

        public delegate bool HasPendingCallAndLogEventHandler();
        public event HasPendingCallAndLogEventHandler HasPendingCallAndLog;

        public delegate bool CampaignList_btnReleaseLockOnClickEventHandler(object sender, EventArgs e);
        public event CampaignList_btnReleaseLockOnClickEventHandler CampaignList_btnReleaseLock_OnClick;

        public delegate void CallLogAfterDeleteEventHandler(object sender, CallLogAfterDeleteEventArgs e);
        public event CallLogAfterDeleteEventHandler CallLogAfterDelete;
        public class CallLogAfterDeleteEventArgs : EventArgs {
            public int DeletedId { get; set; }
        }

        public delegate void btnRefreshSubCampaignsOnClickEventHandler(object sender, EventArgs e);
        public event btnRefreshSubCampaignsOnClickEventHandler btnRefreshSubCampaigns_OnClick;

        public delegate void CampaignList_OnCampaignListEmptyEventHandler();
        public event CampaignList_OnCampaignListEmptyEventHandler CampaignList_OnCampaignListEmpty;

        public delegate void cboSubCampaignListEditValueChangedHandler(object sender, CampaignList.CampaignListArgs e);
        public event cboSubCampaignListEditValueChangedHandler cboSubCampaignList_OnEditValueChange;

        public delegate void btnRemoveCompanyOnClickEventHandler(object sender, CampaignList.CampaignListArgs e);
        public event btnRemoveCompanyOnClickEventHandler btnRemoveCompany_OnClick;

        public delegate void btnSaveAsNotQualifiedOnClickEventHandler(object sender, CampaignList.CampaignListArgs e);
        public event btnSaveAsNotQualifiedOnClickEventHandler btnSaveAsNotQualified_OnClick;

        public delegate void gvCampaignListColumnFilterChangedEventHandler(object sender, CampaignList.CampaignListArgs e);
        public event gvCampaignListColumnFilterChangedEventHandler gvCampaignList_OnColumnFilterChange;

        public delegate void gvCampaignListFocusedRowChangedEventHandler(object sender, CampaignList.CampaignListArgs e);
        public event gvCampaignListFocusedRowChangedEventHandler gvCampaignList_OnFocusedRowChange;

        //public delegate void gvCampaignListOnEnterEventHandler(object sender, CampaignList.CampaignListArgs e);
        //public event gvCampaignListOnEnterEventHandler gvCampaignList_OnEnter;

        //public delegate void gvCampaignListDoubleClickEventHandler(object sender, CampaignList.CampaignListArgs e);
        //public event gvCampaignListDoubleClickEventHandler gvCampaignList_OnDoubleClick;

        //public delegate void btnWorkOnCompanyOnClickEventHandler(object sender, CampaignList.CampaignListArgs e);
        //public event btnWorkOnCompanyOnClickEventHandler btnWorkOnCompany_OnClick;

        //public delegate void AccountModificationInfoChangeEventhandler(object sender, CampaignList.CampaignListArgs e);
        //public event AccountModificationInfoChangeEventhandler OnAccountModificationInfoChange;

        public delegate void tcCampaignExtraDetailOnSelectedPageChangedHandler(object sender, CampaignExtraDetail.CampaignExtraDetailArgs e);
        public event tcCampaignExtraDetailOnSelectedPageChangedHandler tcCampaignExtraDetail_OnSelectedPageChange;

        //public delegate void CampaignExtraDetailCallLogWorkNurtureEventEventHandler(MyFollowUps.CampaignBookingArgs e);
        //public event CampaignExtraDetailCallLogWorkNurtureEventEventHandler CampaignExtraDetail_CallLog_WorkNurtureEvent;
        #endregion

        #region Public Properties
        public int CampaignExtraDetail_CompanyInformation_AccountId {
            get {
                if (m_CampaignExtraDetail != null)
                    return m_CampaignExtraDetail.CompanyInformation_AccountId;
                else
                    return 0;
            }
        }
        public int CampaignExtraDetail_CompanyInformation_FinalListId {
            get {
                if (m_CampaignExtraDetail != null)
                    return m_CampaignExtraDetail.CompanyInformation_FinalListId;
                else
                    return 0;
            }
        }
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private CampaignList m_CampaignList = null;
        private CampaignExtraDetail m_CampaignExtraDetail = null;
        //private bool m_CampaignListOnDataSourceChange_BeenCalled = false;
        #endregion

        #region Public Methods
        public void UpdateSpecificCompany(Forms.FrmSalesConsultant.StatusArgs pArgs)
        {
            m_CampaignList.UpdateSpecificCompany(pArgs);
        }
        public bool CanWorkCompany()
        {
            return m_CampaignList.CanWorkCompany();
        }
        public CampaignList.CampaignListArgs GetCampaignListArgs()
        {
            return m_CampaignList.GetCampaignListArguments();
        }
        //public CampaignList.eCampaignListMode GetCampaignListMode()
        //{
        //    return m_CampaignList.CampaignListMode;
        //}
        public string GetCompanyModificationInfo()
        {
            return m_CampaignList.GetCompanyModificationInfo();
        }
        public void UpdateSelectedCompany(CompanyInformation.CompanyInformationArgs e)
        {
            m_CampaignList.UpdateSelectedRow(e.Account);
        }
        //public bool MoveNext()
        //{
        //    return m_CampaignList.MoveNext();
        //}
        //public bool MovePrevious()
        //{
        //    return m_CampaignList.MovePrevious();
        //}
        //public bool IsFirstRow()
        //{
        //    return m_CampaignList.IsFirstRow;
        //}
        //public bool IsLastRow()
        //{
        //    return m_CampaignList.IsLastRow;
        //}
        public void InitializeModules()
        {
            m_CampaignList.InitializeModule();
            m_CampaignExtraDetail.SetModulesAsReadOnly(true);
            m_CampaignExtraDetail.SetDefaultTab(true);
            m_CampaignExtraDetail.ClearData();
            m_CampaignExtraDetail.Enabled = false;
        }
        public void ReleaseCurrentCompanyLock()
        {
            m_CampaignList.ReleaseCurrentCompanyLock();
        }
        /** /
        public void SaveCompanyAppointment(SubCampaignAppointment pAccountAppointment)
        {
            m_CampaignList.SaveCompanyAppointment(pAccountAppointment);
        }
        /**/
        //public void SetWorkModeState(bool pWorkModeState)
        //{
        //    m_CampaignList.UserOnWorkMode = pWorkModeState;
        //}
        public void SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage pSelectedPage)
        {
            if (m_CampaignExtraDetail != null)
            //if (m_CampaignExtraDetail != null && m_CampaignList.HasAccountsAndContacts())
                    m_CampaignExtraDetail.LoadSelectedPage(pSelectedPage);
        }
        public void CallAttemptMade()
        {
            if (m_CampaignExtraDetail != null)
                m_CampaignExtraDetail.UpdateStatisticsData();
        }
        public void CloseCompany()
        {
            if (m_CampaignList != null)
                m_CampaignList.CloseCompany();
        }
        public void ReloadCallLogTab()
        {
            if (m_CampaignExtraDetail != null) {
                m_CampaignExtraDetail.ReloadCallLog = true;
                m_CampaignExtraDetail.LoadSelectedPage();
            }
        }
        public void DeleteCallLog(int pCallLogId)
        {
            if (m_CampaignExtraDetail != null)
                m_CampaignExtraDetail.DeleteCallLog(pCallLogId);
        }
        public void SetCompanyDetails(int pAccountId)
        {
            if (m_CampaignExtraDetail != null)
                m_CampaignExtraDetail.SetCompanyDetails(pAccountId);
        }

        public void SetDoneLoadingCampaignList(bool pState)
        {
            if (m_CampaignList != null)
                m_CampaignList.DoneLoadingCampaignList = pState;
        }
        public void InitializeCampaignListButtons(bool pState)
        {
            if (m_CampaignList != null)
                m_CampaignList.InitializeCampaignListControls(pState);
        }
        public void NullifyCampaignListDataSource()
        {
            if (m_CampaignList != null)
                m_CampaignList.NullifyDataSource();
        }
        //public bool UserOnWorkMode()
        //{
        //    if (m_CampaignList != null)
        //        return m_CampaignList.UserOnWorkMode;

        //    return false;
        //}
        public void LoadSelectedCampaign(string pSelectedCampaignListParams)
        {
            if (m_CampaignList != null)
                m_CampaignList.LoadSelectedCampaign(pSelectedCampaignListParams);
        }
        public void SetExtraDetailModuleAsReadOnly(bool pIsReadOnly)
        {
            if (m_CampaignExtraDetail != null)
                m_CampaignExtraDetail.SetModulesAsReadOnly(pIsReadOnly);
        }

        public void UpdateContactData(int pContactId)
        {
            if (m_CampaignExtraDetail != null)
                m_CampaignExtraDetail.UpdateContactData(pContactId);
        }

        public bool CampaignListHasRows()
        {
            if (m_CampaignList == null)
                return false;

            return m_CampaignList.CampaignListHasRows;
        }
        //public void SubCampaignList_OnEditvalueChanged(object sender, CampaignList.CampaignListArgs e)
        //{
        //    if (!e.IsEmptyList) {
        //        if (!m_CampaignExtraDetail.Enabled)
        //            m_CampaignExtraDetail.Enabled = true;

        //        //m_CampaignExtraDetail.SetCampaignBookingArgs(e.CampaignBookingAppointment, e.ContactId);
        //        m_CampaignExtraDetail.SetModuleState();
        //        m_CampaignExtraDetail.SetModuleSaving(true);
        //        m_CampaignExtraDetail.ResetContactParams();
        //        m_CampaignExtraDetail.LoadSelectedPage();
        //    }
        //    else {
        //        m_CampaignExtraDetail.SetModuleSaving(false);
        //        m_CampaignExtraDetail.ResetContactParams();
        //    }

        //    if (cboSubCampaignList_OnEditValueChange != null)
        //        cboSubCampaignList_OnEditValueChange(sender, e);
        //}
        public void WorkCompanyOnCampaignListLoad()
        {
            if (m_CampaignList != null)
                m_CampaignList.WorkCompanyOnCampaignListLoad();
        }

        public void CompanySetFocus(int pAcctId)
        {
            m_CampaignList.CompanySetFocus(pAcctId);
        }
        #endregion

        #region Private Methods
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<CampaignListEvents.PrepareExtraDetail>().Subscribe(PrepareExtraDetail);
            m_EventBus.GetEvent<FrmSalesConsultantEvents.OnSubCampaignChange>().Subscribe(SubCampaignChanged);
            m_EventBus.GetEvent<CompanyInformationEvents.OnSave.ManageCampaignList>().Subscribe(CompanyInformationOnSave);
            m_EventBus.GetEvent<ContactInformationEvents.OnSave.ManageCampaignList>().Subscribe(ContactInformationOnSave);
            m_EventBus.GetEvent<ContactInformationEvents.OnDelete.ManageCampaignList>().Subscribe(ContactInformationOnDelete);
        }
        private void ContactInformationOnDelete(ContactInformationEvents.OnDelete.ManageCampaignList e)
        {
            m_CampaignList.UpdateSelectedRow();
        }
        private void ContactInformationOnSave(ContactInformationEvents.OnSave.ManageCampaignList e)
        {
            if (!e.OnSaveArgs.IsNewContact)
                m_CampaignList.UpdateSelectedRow(e.OnSaveArgs.ContactDetail);
            else
                m_CampaignList.ReloadCampaignList(e.OnSaveArgs.NewContactId);

            if (CampaignExtraDetail_OnContactSaved != null)
                CampaignExtraDetail_OnContactSaved();
        }
        private void CompanyInformationOnSave(CompanyInformationEvents.OnSave.ManageCampaignList e)
        {
            m_CampaignList.UpdateSelectedRow(e.OnSaveArgs.Account);
            //if (CampaignExtraDetail_OnCompanyInformationSaved != null)
            //    CampaignExtraDetail_OnCompanyInformationSaved();
        }
        private void SubCampaignChanged(FrmSalesConsultantEvents.OnSubCampaignChange e)
        {
            if (!m_BrightSalesProperty.CommonProperty.IsEmptyCampaignList) {
                if (!m_CampaignExtraDetail.Enabled)
                    m_CampaignExtraDetail.Enabled = true;

                m_CampaignExtraDetail.SetModuleState();
                m_CampaignExtraDetail.SetModuleSaving(true);
                m_CampaignExtraDetail.ResetContactParams();
                m_CampaignExtraDetail.LoadSelectedPage();
            }
            else {
                m_CampaignExtraDetail.SetModuleSaving(false);
                m_CampaignExtraDetail.ResetContactParams();
            }

            if (cboSubCampaignList_OnEditValueChange != null)
                cboSubCampaignList_OnEditValueChange(null, null);
        }
        private void PrepareExtraDetail(CampaignListEvents.PrepareExtraDetail e)
        {
            if (m_CampaignExtraDetail != null) {
                m_CampaignExtraDetail.ResetContactParams();
                m_CampaignExtraDetail.SetModuleSaving(true);
            }
        }

        private void InitializeCampaignListModule()
        {
            m_CampaignList = new CampaignList { 
                Dock = DockStyle.Fill 
            };
            m_CampaignList.btnReleaseLock_OnClick += new CampaignList.btnReleaseLockOnClickEventHandler(m_CampaignList_btnReleaseLock_OnClick);
            m_CampaignList.HasPendingCallAndLog += new CampaignList.HasPendingCallAndLogEventHandler(m_CampaignList_HasPendingCallAndLog);
            //m_CampaignList.CompanyOnWorkByAnotherConsultant += new CampaignList.CompanyOnWorkByAnotherConsultantEventHandler(m_CampaignList_CompanyOnWorkByAnotherConsultant);
            pnlCampaignList.Controls.Clear();
            pnlCampaignList.Controls.Add(m_CampaignList);

            m_CampaignExtraDetail = new CampaignExtraDetail(true) { 
                CallingEnvironment = SelectionProperty.CallingEnvironment.CampaignList,
                Dock = DockStyle.Fill 
            };

            //m_CampaignExtraDetail.CallLog_WorkNurtureEvent += new CampaignExtraDetail.CallLogWorkNurtureEventEventHandler(m_CampaignExtraDetail_CallLog_WorkNurtureEvent);
            pnlCampaignExtraDetail.Controls.Clear();
            pnlCampaignExtraDetail.Controls.Add(m_CampaignExtraDetail);

            #region Campaign List Subscribed Events
            //m_CampaignList.btnWorkOnCompany_OnClick += new CampaignList.btnWorkOnCompanyOnClickEventHandler(m_CampaignList_btnWorkOnCompany_OnClick);
            //m_CampaignList.AccountModificationInfoChange += new CampaignList.AccountModificationInfoChangeEventhandler(m_CampaignList_OnAccountModificationInfoChange);
            //m_CampaignList.gvCampaignList_OnDoubleClick += new CampaignList.gvCampaignListDoubleClickEventHandler(m_CampaignList_gvCampaignList_OnDoubleClick);
            //m_CampaignList.gvCampaignList_OnEnter += new CampaignList.gvCampaignListOnEnterEventHandler(m_CampaignList_gvCampaignList_OnEnter);
            m_CampaignList.gvCampaignList_OnFocusedRowChange += new CampaignList.gvCampaignListFocusedRowChangedEventHandler(m_CampaignList_gvCampaignList_OnFocusedRowChange);
            m_CampaignList.gvCampaignList_OnColumnFilterChange += new CampaignList.gvCampaignListColumnFilterChangedEventHandler(m_CampaignList_gvCampaignList_OnColumnFilterChange);
            m_CampaignList.btnRemoveCompany_OnClick += new CampaignList.btnRemoveCompanyOnClickEventHandler(m_CampaignList_btnRemoveCompany_OnClick);
            //m_CampaignList.cboSubCampaignList_OnEditValueChange += new CampaignList.cboSubCampaignListEditValueChangedHandler(m_CampaignList_cboSubCampaignList_OnEditValueChange);
            m_CampaignList.btnChangeView_OnClick += new CampaignList.btnChangeViewOnClickkEventHandler(m_CampaignList_btnChangeView_OnClick);
            //m_CampaignList.gvCampaignList_OnDataSourceChange += new CampaignList.gvCampaignListDataSourceChangedEventHandler(m_CampaignList_gvCampaignList_OnDataSourceChange);
            m_CampaignList.btnRefresh_OnClick += new CampaignList.btnRefreshOnClickkEventHandler(m_CampaignList_btnRefresh_OnClick);
            m_CampaignList.btnSaveAsNotQualified_OnClick += new CampaignList.btnSaveAsNotQualifiedOnClickEventHandler(m_CampaignList_btnSaveAsNotQualified_OnClick);
            m_CampaignList.OnCampaignListEmpty += new CampaignList.OnCampaignListEmptyEventHandler(m_CampaignList_OnCampaignListEmpty);
            //m_CampaignList.btnRefreshSubCampaigns_OnClick += new CampaignList.btnRefreshSubCampaignsOnClickEventHandler(m_CampaignList_btnRefreshSubCampaigns_OnClick);
            #endregion
            #region Campaign Extra Detail Subscribed Events
            m_CampaignExtraDetail.tcCampaignExtraDetail_OnSelectedPageChange += new CampaignExtraDetail.tcCampaignExtraDetailOnSelectedPageChangedHandler(m_CampaignExtraDetail_tcCampaignExtraDetail_OnSelectedPageChange);
            //m_CampaignExtraDetail.OnAddContact += new CampaignExtraDetail.OnAddContactHandler(m_CampaignExtraDetail_OnAddContact);
            //m_CampaignExtraDetail.OnSaveContact += new CampaignExtraDetail.OnSaveContactHandler(m_CampaignExtraDetail_OnSaveContact);
            //m_CampaignExtraDetail.OnCancelContact += new CampaignExtraDetail.OnCancelContactHandler(m_CampaignExtraDetail_OnCancelContact);
            //m_CampaignExtraDetail.OnDeleteContact += new CampaignExtraDetail.OnDeleteContactHandler(m_CampaignExtraDetail_OnDeleteContact);
            //m_CampaignExtraDetail.OnCompanyInformationSaved += new CampaignExtraDetail.CompanyInformationSavedEventHandler(m_CampaignExtraDetail_OnCompanyInformationSaved);
            //m_CampaignExtraDetail.CallLogAfterDelete += new CampaignExtraDetail.CallLogAfterDeleteEventHandler(m_CampaignExtraDetail_CallLogAfterDelete);
            #endregion

            /**
             * we will need to disable first the extra details,
             * since we currently dont have a selected subcamapign yet
             * at this point. extra detail will be enabled again after
             * selecting a new sub campaign.
             * enabled on method:
             * m_CampaignList_cboSubCampaignList_OnEditValueChange
             */
            m_CampaignExtraDetail.Enabled = false;
        }
        #endregion

        #region Subscribed Event Methods

        #region Contact Information
        //private void m_CampaignExtraDetail_OnDeleteContact(object sender, ContactInformation.ContactInformationArgs e)
        //{
        //    m_CampaignList.UpdateSelectedRow();
        //}
        //private void m_CampaignExtraDetail_OnSaveContact(object sender, ContactInformation.ContactInformationArgs e)
        //{
        //    if (!e.IsNewContact)
        //        m_CampaignList.UpdateSelectedRow(e.ContactDetail);
        //    else
        //        m_CampaignList.ReloadCampaignList(e.NewContactId);

        //    if (CampaignExtraDetail_OnContactSaved != null)
        //        CampaignExtraDetail_OnContactSaved();
        //}
        #endregion

        #region Campaign List
        

        //private void m_CampaignList_CompanyOnWorkByAnotherConsultant(CampaignList.CompanyOnWorkByAnotherConsultantArgs e)
        //{
        //    CampaignList_CompanyOnWorkByAnotherConsultant(e);
        //}
        private bool m_CampaignList_HasPendingCallAndLog()
        {
            return HasPendingCallAndLog();
        }
        private bool m_CampaignList_btnReleaseLock_OnClick(object sender, EventArgs e)
        {
            return CampaignList_btnReleaseLock_OnClick(this, new EventArgs());
        }
        private void m_CampaignList_OnCampaignListEmpty()
        {
            if (m_CampaignExtraDetail != null)
                m_CampaignExtraDetail.SetModuleSaving(false);

            if (CampaignList_OnCampaignListEmpty != null)
                CampaignList_OnCampaignListEmpty();
        }
        //private void m_CampaignList_OnAccountModificationInfoChange(object sender, CampaignList.CampaignListArgs e)
        //{
        //    if (OnAccountModificationInfoChange != null)
        //        OnAccountModificationInfoChange(sender, e);
        //}
        //private void m_CampaignList_btnRefreshSubCampaigns_OnClick(object sender, EventArgs e)
        //{
            //if (btnRefreshSubCampaigns_OnClick != null)
            //    btnRefreshSubCampaigns_OnClick(this, new EventArgs());
        //}
        private void m_CampaignList_btnChangeView_OnClick(object sender, CampaignList.CampaignListArgs e)
        {
            m_EventBus.Notify(new CampaignListEvents.GetLatestProperties() { ForWorkModePurpose = false });
            //m_CampaignExtraDetail.SetCampaignBookingArgs(e.CampaignBookingAppointment, e.ContactId);
            m_CampaignExtraDetail.SetModuleState();
            m_CampaignExtraDetail.SetModuleSaving(true);
            m_CampaignExtraDetail.LoadSelectedPage();
        }
        private void m_CampaignList_btnRemoveCompany_OnClick(object sender, CampaignList.CampaignListArgs e)
        {
            m_EventBus.Notify(new CampaignListEvents.GetLatestProperties() { ForWorkModePurpose = false });
            //m_CampaignExtraDetail.SetCampaignBookingArgs(e.CampaignBookingAppointment, e.ContactId);
            m_CampaignExtraDetail.SetModuleState();
            m_CampaignExtraDetail.SetModuleSaving(true);
            m_CampaignExtraDetail.LoadSelectedPage();

            /**
             * event raiser
             */
            if (btnRemoveCompany_OnClick != null)
                btnRemoveCompany_OnClick(sender, e);
        }
        private void m_CampaignList_btnRefresh_OnClick(object sender, CampaignList.CampaignListArgs e)
        {
            m_EventBus.Notify(new CampaignListEvents.GetLatestProperties() { ForWorkModePurpose = false });
            //m_CampaignExtraDetail.SetCampaignBookingArgs(e.CampaignBookingAppointment, e.ContactId);
            m_CampaignExtraDetail.LoadSelectedPage();
        }
        //private void m_CampaignList_btnWorkOnCompany_OnClick(object sender, CampaignList.CampaignListArgs e)
        //{
        //    if (m_CampaignExtraDetail != null)
        //        m_CampaignExtraDetail.SetModuleSaving(true);

        //    if (btnWorkOnCompany_OnClick != null)
        //        btnWorkOnCompany_OnClick(sender, e);
        //}
        //private void m_CampaignList_cboSubCampaignList_OnEditValueChange(object sender, CampaignList.CampaignListArgs e)
        //{
            //if (!e.IsEmptyList)
            //{
            //    if (!m_CampaignExtraDetail.Enabled)
            //        m_CampaignExtraDetail.Enabled = true;

            //    m_CampaignExtraDetail.SetCampaignBookingArgs(e.CampaignBookingAppointment, e.CampaignListMode, e.ContactId);
            //    m_CampaignExtraDetail.SetModuleState();
            //    m_CampaignExtraDetail.SetModuleSaving(true);
            //    m_CampaignExtraDetail.ResetContactParams();
            //    m_CampaignExtraDetail.LoadSelectedPage();
            //}
            //else
            //{
            //    m_CampaignExtraDetail.SetModuleSaving(false);
            //    m_CampaignExtraDetail.ResetContactParams();
            //}

            //if (cboSubCampaignList_OnEditValueChange != null)
            //    cboSubCampaignList_OnEditValueChange(sender, e);
        //}
        private void m_CampaignList_btnSaveAsNotQualified_OnClick(object sender, CampaignList.CampaignListArgs e)
        {
            if (btnSaveAsNotQualified_OnClick != null)
                btnSaveAsNotQualified_OnClick(sender, e);
        }
        private void m_CampaignList_gvCampaignList_OnColumnFilterChange(object sender, CampaignList.CampaignListArgs e)
        {
            m_EventBus.Notify(new CampaignListEvents.GetLatestProperties() { ForWorkModePurpose = false });
            //m_CampaignExtraDetail.SetCampaignBookingArgs(e.CampaignBookingAppointment, e.ContactId);
            m_CampaignExtraDetail.SetModuleSaving(true);
            m_CampaignExtraDetail.SetModuleState();
            m_CampaignExtraDetail.ShowSpecificCompanyMapLocation = false;
            m_CampaignExtraDetail.ShowSpecificContactMapLocation = false;
            m_CampaignExtraDetail.LoadSelectedPage();

            /**
             * event raiser
             */
            if (gvCampaignList_OnColumnFilterChange != null)
                gvCampaignList_OnColumnFilterChange(sender, e);
        }
        //private void m_CampaignList_gvCampaignList_OnDataSourceChange(object sender, CampaignList.CampaignListArgs e)
        //{
        //    m_CampaignListOnDataSourceChange_BeenCalled = true;
        //    //m_CampaignExtraDetail.SetCampaignBookingArgs(e.CampaignBookingAppointment, e.ContactId);
        //    //m_CampaignExtraDetail.SetModuleState();
        //    //m_CampaignExtraDetail.ShowSpecificCompanyMapLocation = false;
        //    //m_CampaignExtraDetail.LoadSelectedPage();
        //    //m_CampaignListOnDataSourceChange_BeenCalled = false;
        //}
        //private void m_CampaignList_gvCampaignList_OnDoubleClick(object sender, CampaignList.CampaignListArgs e)
        //{
        //    if (m_CampaignExtraDetail != null)
        //    {
        //        m_CampaignExtraDetail.ResetContactParams();
        //        m_CampaignExtraDetail.SetModuleSaving(true);
        //    }

        //    if (gvCampaignList_OnDoubleClick != null)
        //        gvCampaignList_OnDoubleClick(sender, e);
        //}
        //private void m_CampaignList_gvCampaignList_OnEnter(object sender, CampaignList.CampaignListArgs e)
        //{
        //    if (gvCampaignList_OnEnter != null)
        //        gvCampaignList_OnEnter(sender, e);
        //}
        private void m_CampaignList_gvCampaignList_OnFocusedRowChange(object sender, CampaignList.CampaignListArgs e)
        {
            m_EventBus.Notify(new CampaignListEvents.GetLatestProperties() { ForWorkModePurpose = false });
            //m_CampaignExtraDetail.SetCampaignBookingArgs(e.CampaignBookingAppointment, e.ContactId);
            m_CampaignExtraDetail.SetModuleState();
            //if (m_CampaignListOnDataSourceChange_BeenCalled)
            //{
            //    m_CampaignExtraDetail.ShowSpecificCompanyMapLocation = false;
            //    m_CampaignExtraDetail.ShowSpecificContactMapLocation = false;
            //}
            //else
            //{
            //    m_CampaignExtraDetail.ShowSpecificCompanyMapLocation = true;
            //    m_CampaignExtraDetail.ShowSpecificContactMapLocation = true;
            //}

            m_CampaignExtraDetail.ShowSpecificCompanyMapLocation = true;
            m_CampaignExtraDetail.ShowSpecificContactMapLocation = true;
            m_CampaignExtraDetail.SetModuleSaving(true);
            m_CampaignExtraDetail.LoadSelectedPage();
            //m_CampaignListOnDataSourceChange_BeenCalled = false;
            
            /**
             * event raiser
             */
            if (gvCampaignList_OnFocusedRowChange != null)
                gvCampaignList_OnFocusedRowChange(sender, e);
        }
        #endregion

        #region Campaign Extra Detail
        //private void m_CampaignExtraDetail_CallLog_WorkNurtureEvent(MyFollowUps.CampaignBookingArgs e)
        //{
        //    if (CampaignExtraDetail_CallLog_WorkNurtureEvent != null)
        //        CampaignExtraDetail_CallLog_WorkNurtureEvent(e);
        //}
        private void m_CampaignExtraDetail_tcCampaignExtraDetail_OnSelectedPageChange(object sender, CampaignExtraDetail.CampaignExtraDetailArgs e)
        {
            /**
             * event raiser
             */
            if (tcCampaignExtraDetail_OnSelectedPageChange != null)
                tcCampaignExtraDetail_OnSelectedPageChange(sender, e);
        }
        //private void m_CampaignExtraDetail_OnCompanyInformationSaved(object sender, CompanyInformation.CompanyInformationArgs e)
        //{
        //    m_CampaignList.UpdateSelectedRow(e.Account);
        //    if (CampaignExtraDetail_OnCompanyInformationSaved != null)
        //        CampaignExtraDetail_OnCompanyInformationSaved();
        //}
        //private void m_CampaignExtraDetail_CallLogAfterDelete(object sender, CampaignExtraDetail.CallLogAfterDeleteEventArgs e)
        //{
        //    if (CallLogAfterDelete != null)
        //        CallLogAfterDelete(this, new CallLogAfterDeleteEventArgs() { DeletedId = e.DeletedId });
        //}
        #endregion
        #endregion

        #region Object Events
        #endregion
    }
}
