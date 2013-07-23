
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using System.Data.Objects;
using System.Data.SqlClient;
using System.Globalization;

using BrightVision.Common.Business;
using BrightVision.Common.Modules;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
using BrightVision.Common.Events.Core;
using BrightVision.DQControl.UI;
using BrightVision.Model;
using SalesConsultant.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraRichEdit;
using DevExpress.XtraGrid.Columns;
using DevExpress.Data.Linq;
using DevExpress.XtraGrid;
using DevExpress.Data.PLinq;
using System.Text.RegularExpressions;
using SalesConsultant.Business;
using BrightVision.Logging.Enums;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using SalesConsultant.Events;

namespace SalesConsultant.Modules
{
    public partial class CampaignList : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public CampaignList()
        {
            InitializeComponent();
            this.InitializeModule();
            this.RegisterEvents();
        }
        #endregion

        #region Public Event Handlers
        //public delegate void CompanyOnWorkByAnotherConsultantEventHandler(CompanyOnWorkByAnotherConsultantArgs e);
        //public event CompanyOnWorkByAnotherConsultantEventHandler CompanyOnWorkByAnotherConsultant;
        //public class CompanyOnWorkByAnotherConsultantArgs : EventArgs {
        //    public string UserName { get; set; }
        //}

        public delegate bool HasPendingCallAndLogEventHandler();
        public event HasPendingCallAndLogEventHandler HasPendingCallAndLog; 

        public delegate bool btnReleaseLockOnClickEventHandler(object sender, EventArgs e);
        public event btnReleaseLockOnClickEventHandler btnReleaseLock_OnClick;

        public delegate void btnRefreshSubCampaignsOnClickEventHandler(object sender, EventArgs e);
        public event btnRefreshSubCampaignsOnClickEventHandler btnRefreshSubCampaigns_OnClick;

        public delegate void OnCampaignListEmptyEventHandler();
        public event OnCampaignListEmptyEventHandler OnCampaignListEmpty;

        public delegate void btnRefreshOnClickkEventHandler(object sender, CampaignListArgs e);
        public event btnRefreshOnClickkEventHandler btnRefresh_OnClick;

        public delegate void btnChangeViewOnClickkEventHandler(object sender, CampaignListArgs e);
        public event btnChangeViewOnClickkEventHandler btnChangeView_OnClick;

        public delegate void cboSubCampaignListEditValueChangedHandler(object sender, CampaignListArgs e);
        public event cboSubCampaignListEditValueChangedHandler cboSubCampaignList_OnEditValueChange;

        public delegate void btnRemoveCompanyOnClickEventHandler(object sender, CampaignListArgs e);
        public event btnRemoveCompanyOnClickEventHandler btnRemoveCompany_OnClick;

        public delegate void gvCampaignListColumnFilterChangedEventHandler(object sender, CampaignListArgs e);
        public event gvCampaignListColumnFilterChangedEventHandler gvCampaignList_OnColumnFilterChange;

        //public delegate void gvCampaignListDataSourceChangedEventHandler(object sender, CampaignListArgs e);
        //public event gvCampaignListDataSourceChangedEventHandler gvCampaignList_OnDataSourceChange;

        public delegate void gvCampaignListFocusedRowChangedEventHandler(object sender, CampaignListArgs e);
        public event gvCampaignListFocusedRowChangedEventHandler gvCampaignList_OnFocusedRowChange;

        public delegate void gvCampaignListOnEnterEventHandler();
        public event gvCampaignListOnEnterEventHandler gvCampaignList_OnEnter;

        public delegate void gvCampaignListDoubleClickEventHandler();
        public event gvCampaignListDoubleClickEventHandler gvCampaignList_OnDoubleClick;

        public delegate void btnWorkOnCompanyOnClickEventHandler();
        public event btnWorkOnCompanyOnClickEventHandler btnWorkOnCompany_OnClick;

        public delegate void AccountModificationInfoChangeEventhandler();
        public event AccountModificationInfoChangeEventhandler AccountModificationInfoChange;

        public delegate void btnSaveAsNotQualifiedOnClickEventHandler(object sender, CampaignListArgs e);
        public event btnSaveAsNotQualifiedOnClickEventHandler btnSaveAsNotQualified_OnClick;
        #endregion

        #region Public Event Handler Arguments
        public class CampaignListArgs: EventArgs {
            public string BreadCrumb { get; set; }
            public string CompanyName { get; set; }
            public int ContactId { get; set; }
            //public SelectionProperty.CampaignListMode CampaignListMode { get; set; }
            public SubCampaignAppointment CampaignBookingAppointment { get; set; }
            //public int AccountLeadStatusSelectedIndex { get; set; }
            //public int AccountStatusSelectedIndex { get; set; }
            //public int ContactStatusSelectedIndex { get; set; }
            //public int AccountLeadStatusNotQualifiedIndex { get; set; }
            //public int AccountStatusNotQualifiedIndex { get; set; }
            //public int ContactStatusNotQualifiedIndex { get; set; }
            //public List<string> AccountLeadStatuses { get; set; }
            //public List<string> AccountStatuses { get; set; }
            //public List<string> ContactStatuses { get; set; }
            public List<XmlUtility.SubCampaignConfig> AccountLeadStatuses { get; set; }
            public List<XmlUtility.SubCampaignConfig> AccountStatuses { get; set; }
            public List<XmlUtility.SubCampaignConfig> ContactStatuses { get; set; }
            public bool IsEmptyList { get; set; }
        }
        #endregion

        #region Subscribed Events
        private void _ucSubCampaignAccount_cmdAddToSubCampaign_OnClick(object sender, AddSubCampaignAccount.AddSubCampaignAccountEventArgs e)
        {
            /**
             * process accounts and contacts to be added to sub-campaign
             */
            List<int> _ToAddAcctIds = new List<int>();
            List<int> _ToAddContactIds = new List<int>();
            List<AccountContactIds> _lstSubCampaignContactIds = new List<AccountContactIds>();
            bool _PerformGridUpdate = false;

            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                m_FinalListId = (int)_efDbModel.final_lists.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId).id;

                /**
                 * get the list of account contacts when view mode is in company mode only
                 */
                //if (CampaignListMode == CampaignList.eCampaignListMode.CompaniesOnly) {
                _lstSubCampaignContactIds = (
                    from _efeSubCampaignContactList in _efDbModel.sub_campaign_contact_lists
                    join _efeAccountContact in _efDbModel.account_contacts on _efeSubCampaignContactList.contact_id equals _efeAccountContact.contact_id
                    where _efeSubCampaignContactList.final_list_id == m_FinalListId
                    select new AccountContactIds {
                        AccountId = _efeAccountContact.account_id,
                        ContactId = _efeSubCampaignContactList.contact_id
                    }
                ).ToList();
                //}

                foreach (int _AcctId in e.lstAcctIds) {
                    /**
                     * if account does not exist, add to list.
                     */

                    /*
                     * https://brightvision.jira.com/browse/PLATFORM-2583
                     * DAN: Commented as to allow adding of account even if CampaignListMode is Companies and Contacts
                     * 
                    */
                    //if (m_BrightSalesProperty.CampaignList.CampaignListMode == SelectionProperty.CampaignListMode.CompaniesOnly)
                        if (_efDbModel.sub_campaign_account_lists.FirstOrDefault(i =>
                            i.final_list_id == m_FinalListId &&
                            i.account_id == _AcctId) == null)
                            _ToAddAcctIds.Add(_AcctId);

                    /**
                     * contacts per account.
                     */
                    List<int> _lstContactIds = (
                        from _efAcctContact in _efDbModel.account_contacts
                        where _efAcctContact.account_id == _AcctId
                        select _efAcctContact.contact_id
                    ).ToList();

                    /**
                     * check if contact exist, if not, add to list.
                     */
                    foreach (int _ContactId in _lstContactIds) {
                        if (_lstSubCampaignContactIds.Find(i => i.AccountId == _AcctId && i.ContactId == _ContactId) == null)
                            if (_efDbModel.contacts.FirstOrDefault(i => i.id == _ContactId && i.active) != null)
                                _ToAddContactIds.Add(_ContactId);
                    }
                }
            }

            if (_ToAddAcctIds.Count > 0) {
                ObjectCallList.AddCompaniesToSubCampaign(_ToAddAcctIds, SubCampaignId, string.Format("Added by {0}", UserSession.CurrentUser.UserFullName));
                _PerformGridUpdate = true;
            }

            if (_ToAddContactIds.Count > 0) {
                ObjectCallList.AddContactsToSubCampaign(_ToAddContactIds, SubCampaignId);
                _PerformGridUpdate = true;
            }

            if (_PerformGridUpdate) {
                cbxFilterMeLastUser.Checked = false;
                cbxShowOnlyNonFinished.Checked = false;
                m_DoneLoadingCampaignList = false;
                //m_ChangingCampaignList = true;
                this.LoadCampaignList(eCampaignListLoadCallee.AddAccount);
                //m_ChangingCampaignList = false;
                m_DoneLoadingCampaignList = true;
            }
        }
        private void objDeActivateAccount_AfterSave(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Loading list.");
            var x = Business.ObjectLocking.ReleaseLock(m_FinalListId, m_AccountId);
            this.LoadCampaignList(eCampaignListLoadCallee.DeActivateAccount);
            if (gvCampaignList.RowCount > 0 && btnRemoveCompany_OnClick != null) {
                this.SetCampaignListAppointmentParams();
                CampaignListArgs _Args = new CampaignListArgs() {
                    ContactId = m_ContactId,
                    CompanyName = m_CompanyName,
                    CampaignBookingAppointment = m_objSubCampaignAppointmentParams,
                    //CampaignListMode = CampaignListMode,
                    BreadCrumb = string.Format("{0}{1}{2} ({3})",
                        string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                        string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                        string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
                        ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                            m_FinalListId,
                            m_AccountId
                        )
                    )
                    //BreadCrumb = string.Format("{0} > {1} > {2} > {3}{4}{5}",
                    //    CustomerName,
                    //    CampaignName,
                    //    SubCampaignName,
                    //    string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                    //    string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                    //    string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry
                    //)
                };
                btnRemoveCompany_OnClick(this, _Args);
            }
            else if (gvCampaignList.RowCount < 1 && OnCampaignListEmpty != null)
                OnCampaignListEmpty();
            WaitDialog.Close();
        }
        #endregion

        #region Public Properties
        public int CustomerId { get; set; }
        public int CampaignId { get; set; }
        public int SubCampaignId { get; set; }

        public string CustomerName { get; set; }
        public string CampaignName { get; set; }
        public string SubCampaignName { get; set; }

        //public bool UserOnWorkMode { get; set; } // to know if the user is currently on work mode on campaign booking

        //public eCampaignListMode CampaignListMode { get; set; }
        //public enum eCampaignListMode
        //{
        //    CompaniesOnly,
        //    CompaniesAndContacts,
        //    None
        //}

        public bool DoneLoadingCampaignList
        {
            get { return m_DoneLoadingCampaignList; }
            set { m_DoneLoadingCampaignList = value; }
        }
        public bool CampaignListHasRows
        {
            get {
                if (gvCampaignList.RowCount > 0)
                    return true;
                else 
                    return false;
            }
        }

        //DataTable dtCampaignList = null;
        #endregion

        #region Private Properties
        /**
         * for use on adding companies to subcampaign
         */
        private class AccountContactIds
        {
            public int AccountId { get; set; }
            public int ContactId { get; set; }
        }

        private enum ObjectEventSender
        {
            btnWorkOnCompany_Click,
            gvCampaignList_DoubleClick,
            gvCampaignList_Enter
        }
        private enum eCampaignListLoadCallee {
            AddAccount,
            DeActivateAccount,
            ContactInformationSave,
            CampaignListChange,
            ChangeView,
            Refresh,
            EventLogOnLoad,
            None
        }
        private bool m_DoneLoadingCampaignList = true;          // to know if the campaign list has finished loading
        //private bool m_ChangingCampaignList = false;            // to know if the grid is changing from one campaign list to another
        private bool m_ChangingCampaignListMode = false;        // to know if the campaign list is loading from (companies only) to (companies and contacts) (vice versa)
        //private bool m_ChangingCampaignListFocusedRow = false;  // to know if the grid is currently changing its focused row
        //private bool m_RefreshingCampaignList = false;          // to know if the refresh button has been pressed
        //private bool m_AllowRowChange = false;                  // to set if we would allow row change when pressing buttons
        //private bool m_CampaignListParamsLoaded = false;        // to know if the GetSelectedCampaignListRowCommonlyUsedFields() has been called already
        
        private SubCampaignAppointment m_objSubCampaignAppointmentParams = null;
        private FrmSalesConsultant m_objMainForm = null;
        private sub_campaign_account_lists m_CurrentCampaignListAccount = null;
        private int m_CampaignListSelectedRow = 0;
        private int m_CampaignListLastWorkedAccountId = 0;
        private bool m_CompanyWorkable = false;
        
        /**
         * for use on storing campaign list appointment values
         */
        private int m_AccountId = 0;
        private int m_ContactId = 0;
        private int m_FinalListId = 0;
        private long m_CampaignListId = 0;
        private string m_CompanyName = null;
        private string m_CompanyWebsite = null;
        private string m_CompanyBoardNumber = null;
        private string m_CompanyAppointmentStatus = null;
        private string m_CompanyAppointmentLeadStatus = null;
        private string m_CompanyCity = null;
        private string m_CompanyCountry = null;
        private bool m_AccountLocked = false;
        private int m_AccountLockedBy = 0;
        private string m_AccountLockUser = null;

        /**
         * list handlers for sub-campaign statuses xml config data.
         */
        //private int m_AccountLeadStatusSelectedIndex = 0;
        //private int m_AccountStatusSelectedIndex = 0;
        //private int m_ContactStatusSelectedIndex = 0;
        //private int m_AccountLeadStatusNotQualifiedIndex = -1;
        //private int m_AccountStatusNotQualifiedIndex = -1;
        //private int m_AccountStatusSendEmailIndex = -1;
        //private int m_ContactStatusNotQualifiedIndex = -1;
        //private List<string> m_lstAccountLeadStatuses = null;
        //private List<string> m_lstAccountStatuses = null;
        //private List<string> m_lstContactStatuses = null;
        private List<XmlUtility.SubCampaignConfig> m_lstAccountLeadStatuses = null;
        private List<XmlUtility.SubCampaignConfig> m_lstAccountStatuses = null;
        private List<XmlUtility.SubCampaignConfig> m_lstContactStatuses = null;

        private BrightPlatformEntities m_efDbModel = null;
        private LinqServerModeSource m_lsmsCampaignListData = null;
        private string m_CampaignListFindFilter = string.Empty;
        private string m_CampaignListSpecificColumnFilter = string.Empty;
        private string m_CampaignListCheckedFilter = string.Empty;
        //private bool m_CompanyWorkedByAnotherConsultant = false;
        private ConvertEditValueEventArgs m_OperatorArgs = null;
        private bool m_SalesConsultantCampaignListComboKeyPressEnter = false;

        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private bool IsSuccessFocus;
        #endregion

        #region Public Methods
        public void UpdateSpecificCompany(FrmSalesConsultant.StatusArgs pArgs)
        {
            try {
                int _AcctId = 0;
                if (pArgs != null) {
                    for (int i = 0; i < gvCampaignList.RowCount; i++) {
                        _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
                        if (_AcctId == pArgs.CompanyId) {
                            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Status", pArgs.CompanyStatus);
                            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_Contact", pArgs.CompanyLastContact.ToShortDateString());
                            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_User", pArgs.CompanyLastUser);
                            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Lead_Status", pArgs.CompanyLeadStatus);
                            break;
                        }
                    }
                }
                else {
                    _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
                    BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
                    int _FinalListId = _efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId).id;
                    //sub_campaign_account_appointments _item = _efDbContext.sub_campaign_account_appointments.FirstOrDefault(i =>
                    //    i.final_list_id == _FinalListId &&
                    //    i.account_id == _AcctId
                    //);
                    sub_campaign_account_appointments _item = ObjectSubCampaign.SaveSubCampaignCompanyAppointmentStatus(_AcctId, _FinalListId, "Open", null);
                    if (_item != null) {
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Status", _item.status);
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_Contact", _item.last_contact.ToShortDateString());
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_User", UserSession.CurrentUser.UserFullName);
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Lead_Status", _item.lead_status);
                    }
                }
            }
            catch (Exception e) {
                m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                    ErrorMessage = e.Message
                });
            }
        }
        public bool CanWorkCompany()
        {
            if (gvCampaignList.RowCount < 1)
                return false;

            this.GetCurrentCampaignListAccount();
            this.ReleaseCurrentCompanyLock();
            //this.GetCurrentCampaignListAccount();
            if (m_CurrentCampaignListAccount == null)
                return false;

            bool islocked = m_CurrentCampaignListAccount.locked;
            int? lockedBy = m_CurrentCampaignListAccount.locked_by;
            if ((islocked && lockedBy == UserSession.CurrentUser.UserId) || !islocked)
            {
                var BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
                m_CurrentCampaignListAccount.locked = true;
                m_CurrentCampaignListAccount.locked_by = UserSession.CurrentUser.UserId;
                m_CurrentCampaignListAccount.locked_timestamp =
                    BPContext.FIUpdateUserLock(
                                m_CurrentCampaignListAccount.final_list_id,
                                m_CurrentCampaignListAccount.account_id,
                                m_CurrentCampaignListAccount.locked,
                                m_CurrentCampaignListAccount.locked_by).FirstOrDefault();
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", m_CurrentCampaignListAccount.locked);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_by", m_CurrentCampaignListAccount.locked_by);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_user", UserSession.CurrentUser.UserFullName);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_timestamp", m_CurrentCampaignListAccount.locked_timestamp);
                return true;
            }
            else
            {
                NotificationDialog.Warning("Bright Sales", "The selected company is currently edited by another user. Please try again later.");
                m_CurrentCampaignListAccount = null;
                return false;
            }
        }
        public CampaignListArgs GetCampaignListArguments()
        {
            this.SetCampaignListAppointmentParams();
            CampaignListArgs _Args = new CampaignListArgs();
            _Args.ContactId = m_ContactId;
            _Args.CompanyName = m_CompanyName;
            _Args.CampaignBookingAppointment = m_objSubCampaignAppointmentParams;
            //_Args.CampaignListMode = CampaignListMode;

            //_Args.AccountLeadStatusSelectedIndex = m_AccountLeadStatusSelectedIndex;
            //_Args.AccountStatusSelectedIndex = m_AccountStatusSelectedIndex;
            //_Args.ContactStatusSelectedIndex = m_ContactStatusSelectedIndex;
            //_Args.AccountLeadStatusNotQualifiedIndex = m_AccountLeadStatusNotQualifiedIndex;
            //_Args.AccountStatusNotQualifiedIndex = m_AccountStatusNotQualifiedIndex;
            //_Args.ContactStatusNotQualifiedIndex = m_ContactStatusNotQualifiedIndex;
            _Args.AccountLeadStatuses = m_lstAccountLeadStatuses;
            _Args.AccountStatuses = m_lstAccountStatuses;
            _Args.ContactStatuses = m_lstContactStatuses;

            _Args.BreadCrumb = string.Format("{0}{1}{2} ({3})",
                string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
                ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                    m_FinalListId,
                    m_AccountId
                )
            );

            //_Args.BreadCrumb =
            //    string.Format("{0} > {1} > {2} > {3}{4}{5}",
            //        CustomerName,
            //        CampaignName,
            //        SubCampaignName,
            //        string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
            //        string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
            //        string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry
            //    );

            return _Args;
        }
        public string GetCompanyModificationInfo()
        {
            this.GetSelectedCampaignListRowCommonlyUsedFields();
            string _info = String.Format("{0} - last updated on: {1}", m_CompanyName, ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(m_FinalListId, m_AccountId));
            return _info;
        }
        //public bool IsFirstRow
        //{
        //    get {
        //        if (gvCampaignList.RowCount < 2)
        //            return true;

        //        return gvCampaignList.IsFirstRow;
        //    }
        //}
        //public bool IsLastRow
        //{
        //    get {
        //        if (gvCampaignList.RowCount < 2)
        //            return true;

        //        return gvCampaignList.IsLastRow;
        //    }
        //}
        public void InitializeModule()
        {
            CustomerId = 0;
            CampaignId = 0;
            SubCampaignId = 0;

            CustomerName = null;
            CampaignName = null;
            SubCampaignName = null;

            m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
            m_BrightSalesProperty.CampaignList.CampaignListMode = SelectionProperty.CampaignListMode.CompaniesOnly;
            cbxFilterMeLastUser.Enabled = false;
            cbxShowOnlyNonFinished.Enabled = false;

            this.LoadCampaignListSelection();
        }
        //public bool MoveNext()
        //private void MoveNext()
        //{
        //    m_BrightSalesProperty.CampaignBooking.LoadNextCompanySuccess = false;
        //    if (gvCampaignList.RowCount < 1)
        //        return;
        //    //return false;

        //    /**
        //     * check current company if being released by the the user before loading the next company.
        //     */
        //    int _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
        //    int _FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
        //    BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
        //    sub_campaign_account_lists _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
        //        i.account_id == _AcctId &&
        //        i.final_list_id == _FinalListId
        //    );
        //    if ((_item.locked && _item.locked_by == UserSession.CurrentUser.UserId) || !_item.locked)
        //    {
        //        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", false);
        //        this.GetCurrentCampaignListAccount();
        //        this.ReleaseCurrentCompanyLock();
        //    }

        //    /**
        //     * [@jeff 10.18.2013]: https://brightvision.jira.com/browse/PLATFORM-2638
        //     * added logic to go to the next company if campaign list is on companies and contacts mode.
        //     */
        //    int _Row = gvCampaignList.FocusedRowHandle + 1;
        //    for (; _Row < gvCampaignList.RowCount; _Row++)
        //    {
        //        int _NextAcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(_Row, "account_id").ToString());
        //        if (_AcctId != _NextAcctId)
        //            break;
        //    }

        //    /**
        //     * now, we will load the next company and re-check if this is currently worked by other consultants.
        //     */
        //    m_BrightSalesProperty.CommonProperty.CompanyLocked = false;
        //    //m_AllowRowChange = true;
        //    gvCampaignList.FocusedRowHandle = _Row;
        //    //gvCampaignList.MoveNext();
        //    _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
        //    _FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
        //    _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
        //        i.account_id == _AcctId &&
        //        i.final_list_id == _FinalListId
        //    );
        //    if (_item.locked && _item.locked_by > 0 && _item.locked_by != UserSession.CurrentUser.UserId)
        //    {
        //        user _user = _efDbContext.users.FirstOrDefault(i => i.id == _item.locked_by);
        //        m_BrightSalesProperty.CommonProperty.CompanyLocked = true;
        //        NotificationDialog.Warning("Bright Sales", string.Format("{0}This company is currently worked by{0}{1}", Environment.NewLine, _user.fullname));

        //        //CompanyOnWorkByAnotherConsultant(new CompanyOnWorkByAnotherConsultantArgs() {
        //        //    UserName = _user.fullname
        //        //});
        //        //m_CompanyWorkedByAnotherConsultant = true;
        //    }

        //    this.SetCampaignListAppointmentParams();
        //    if (!m_BrightSalesProperty.CommonProperty.CompanyLocked)
        //    {
        //        if (_item.locked && _item.locked_by != UserSession.CurrentUser.UserId)
        //        {
        //            btnWorkOnCompany.Enabled = false;
        //            btnRemoveCompany.Enabled = false;
        //        }
        //        else
        //        {
        //            btnWorkOnCompany.Enabled = true;
        //            btnRemoveCompany.Enabled = true;
        //        }
        //        if ((_item.locked && _item.locked_by == UserSession.CurrentUser.UserId) || !_item.locked)
        //        {
        //            var BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        //            m_CurrentCampaignListAccount.locked = true;
        //            m_CurrentCampaignListAccount.locked_by = UserSession.CurrentUser.UserId;
        //            m_CurrentCampaignListAccount.locked_timestamp =
        //                BPContext.FIUpdateUserLock(
        //                    m_FinalListId,
        //                    m_AccountId,
        //                    m_CurrentCampaignListAccount.locked,
        //                    m_CurrentCampaignListAccount.locked_by).FirstOrDefault();
        //            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", m_CurrentCampaignListAccount.locked);
        //            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_by", m_CurrentCampaignListAccount.locked_by);
        //            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Locked_By_User", UserSession.CurrentUser.UserFullName);
        //            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_timestamp", m_CurrentCampaignListAccount.locked_timestamp);
        //        }
        //        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", true);
        //    }
        //    //m_objMainForm.m_CampaignBookingModule.Enabled = false;
        //    //m_objMainForm.m_CampaignBookingModule.LoadCampaignBookingData();
        //    //m_objMainForm.m_CampaignBookingModule.OnLoadInitialization();
        //    //m_objMainForm.m_CampaignBookingModule.Enabled = true;
        //    m_CampaignListSelectedRow = gvCampaignList.FocusedRowHandle < 0 ? 0 : gvCampaignList.FocusedRowHandle;
        //    m_BrightSalesProperty.CampaignBooking.LoadNextCompanySuccess = true;
        //    return;
        //    //return true;
        //}
        //public bool MovePrevious()
        //{
        //    if (gvCampaignList.RowCount < 1)
        //        return false;
            
        //    /**
        //     * check current company if being released by the the user before loading the next company.
        //     */
        //    int _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
        //    int _FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
        //    BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
        //    sub_campaign_account_lists _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
        //        i.account_id == _AcctId &&
        //        i.final_list_id == _FinalListId
        //    );
        //    if ((_item.locked && _item.locked_by == UserSession.CurrentUser.UserId) || !_item.locked) {
        //        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", false);
        //        this.GetCurrentCampaignListAccount();
        //        this.ReleaseCurrentCompanyLock();
        //    }

        //    /**
        //    * [@jeff 10.18.2013]: https://brightvision.jira.com/browse/PLATFORM-2638
        //    * added logic to go to the next company if campaign list is on companies and contacts mode.
        //    */
        //    int _Row = 0;
        //    if (gvCampaignList.FocusedRowHandle > 0)
        //        _Row = gvCampaignList.FocusedRowHandle - 1;

        //    for (; _Row < gvCampaignList.RowCount; _Row--) {
        //        int _PrevAcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(_Row, "account_id").ToString());
        //        if (_AcctId != _PrevAcctId)
        //            break;
        //    }

        //    /**
        //     * now, we will load the next company and re-check if this is currently worked by other consultants.
        //     */
        //    m_BrightSalesProperty.CommonProperty.CompanyLocked = false;
        //    //m_AllowRowChange = true;
        //    gvCampaignList.FocusedRowHandle = _Row;
        //    //gvCampaignList.MovePrev();
        //    _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
        //    _FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
        //    _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
        //        i.account_id == _AcctId &&
        //        i.final_list_id == _FinalListId
        //    );
        //    if (_item.locked && _item.locked_by > 0 && _item.locked_by != UserSession.CurrentUser.UserId) {
        //        user _user = _efDbContext.users.FirstOrDefault(i => i.id == _item.locked_by);
        //        m_BrightSalesProperty.CommonProperty.CompanyLocked = true;
        //        NotificationDialog.Warning("Bright Sales", string.Format("{0}This company is currently worked by{0}{1}", Environment.NewLine, _user.fullname));
        //        //CompanyOnWorkByAnotherConsultant(new CompanyOnWorkByAnotherConsultantArgs() {
        //        //    UserName = _user.fullname
        //        //});
        //        //m_CompanyWorkedByAnotherConsultant = true;
        //    }

        //    this.SetCampaignListAppointmentParams();
        //    if (!m_BrightSalesProperty.CommonProperty.CompanyLocked) {
        //        bool islocked = m_CurrentCampaignListAccount.locked;
        //        int? lockedBy = m_CurrentCampaignListAccount.locked_by;
        //        if (islocked && lockedBy != UserSession.CurrentUser.UserId) {
        //            btnWorkOnCompany.Enabled = false;
        //            btnRemoveCompany.Enabled = false;
        //        }
        //        else {
        //            btnWorkOnCompany.Enabled = true;
        //            btnRemoveCompany.Enabled = true;
        //        }

        //        if ((islocked && lockedBy == UserSession.CurrentUser.UserId) || !islocked) {
        //            var BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        //            m_CurrentCampaignListAccount.locked = true;
        //            m_CurrentCampaignListAccount.locked_by = UserSession.CurrentUser.UserId;
        //            m_CurrentCampaignListAccount.locked_timestamp =
        //                BPContext.FIUpdateUserLock(
        //                    m_FinalListId,
        //                    m_AccountId,
        //                    m_CurrentCampaignListAccount.locked,
        //                    m_CurrentCampaignListAccount.locked_by).FirstOrDefault();
        //            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", m_CurrentCampaignListAccount.locked);
        //            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_by", m_CurrentCampaignListAccount.locked_by);
        //            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Locked_By_User", UserSession.CurrentUser.UserFullName);
        //            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_timestamp", m_CurrentCampaignListAccount.locked_timestamp);
        //        }
        //        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", true);
        //    }
        //    //m_objMainForm.m_CampaignBookingModule.Enabled = false;
        //    //m_objMainForm.m_CampaignBookingModule.LoadCampaignBookingData();
        //    //m_objMainForm.m_CampaignBookingModule.OnLoadInitialization();
        //    //m_objMainForm.m_CampaignBookingModule.Enabled = true;
        //    m_CampaignListSelectedRow = gvCampaignList.FocusedRowHandle < 0 ? 0 : gvCampaignList.FocusedRowHandle;
        //    return true;
        //}
        public void ReleaseCurrentCompanyLock()
        {
            try {
                m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
                if (m_CurrentCampaignListAccount != null) {
                    int _iRowHandle = -1;
                    for (int i = 0; i < gvCampaignList.DataRowCount; i++) {
                        if (ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(i, "account_id").ToString()) == m_CampaignListLastWorkedAccountId) {
                            _iRowHandle = i;
                            break;
                        }
                    }

                    Business.ObjectLocking.ReleaseLock(m_FinalListId);
                    if (_iRowHandle > -1) {
                        gvCampaignList.SetRowCellValue(_iRowHandle, "locked", false);
                        gvCampaignList.SetRowCellValue(_iRowHandle, "locked_by", null);
                        gvCampaignList.SetRowCellValue(_iRowHandle, "locked_user", null);
                        gvCampaignList.SetRowCellValue(_iRowHandle, "Locked_By_User", null);
                        gvCampaignList.SetRowCellValue(_iRowHandle, "locked_timestamp", null);
                    }
                }
            }
            catch (Exception e) {
                m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                    ErrorMessage = e.Message
                });
            }
        }
        public void ReloadCampaignList(int ContactId)
        {
            this.LoadCampaignList(eCampaignListLoadCallee.ContactInformationSave);
            //gcCampaignList.RefreshDataSource();
            gvCampaignList.FocusedRowHandle = 0;
            try {
                for (int i = 0; i < gvCampaignList.RowCount; i++) {
                    if (ContactId == ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(i, "contact_id"))) {
                        gvCampaignList.FocusedRowHandle = i;
                        break;
                    }
                }
            }
            catch (Exception e) {
                m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                    ErrorMessage = e.Message
                });
            }
        }
        public void UpdateSelectedRow(CTContactDetails pContactDetail = null)
        {
            if (gvCampaignList.RowCount < 1)
                return;

            if (pContactDetail == null) {
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Firstname", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Middlename", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Lastname", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Title", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Direct_Phone", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Mobile", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Email", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Address_1", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Address_2", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_City", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Zipcode", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Country", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "contact_id", 0);
            }
            else {
                if (ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(gvCampaignList.FocusedRowHandle, "contact_id").ToString()) != pContactDetail.id)
                    return;

                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Firstname", pContactDetail.first_name);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Middlename", pContactDetail.middle_name);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Lastname", pContactDetail.last_name);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Title", pContactDetail.title);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Direct_Phone", pContactDetail.direct_phone);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Mobile", pContactDetail.mobile);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Email", pContactDetail.email);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Address_1", pContactDetail.address_1);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Address_2", pContactDetail.address_2);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_City", pContactDetail.city);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Zipcode", pContactDetail.zipcode);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Country", pContactDetail.country);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Contact_Modified", DateTime.Now.ToString("yyyy-MM-dd") + " by " + UserSession.CurrentUser.UserFullName);
            }
        }
        public void UpdateSelectedRow(account pAccountDetail)
        {
            if (ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(gvCampaignList.FocusedRowHandle, "account_id").ToString()) != pAccountDetail.id)
                return;

            if (gvCampaignList.RowCount < 1)
                return;

            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Name", pAccountDetail.company_name);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Telephone", pAccountDetail.telephone);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Employees_Total_1", pAccountDetail.employees_total);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Employees_Total_2", pAccountDetail.employees_total_2);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Employees_Total_3", pAccountDetail.employees_total_3);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Site", pAccountDetail.www);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Org_No", pAccountDetail.org_no);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Box_Address", pAccountDetail.box_address);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Street_Address", pAccountDetail.street_address);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Zipcode", pAccountDetail.zipcode);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Municipality", pAccountDetail.municipality);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_City", pAccountDetail.city);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Country", pAccountDetail.country);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Telefax", pAccountDetail.telefax);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Year_Established", pAccountDetail.year_established);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Parent_Company", pAccountDetail.parent_company);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Activity_Code", pAccountDetail.activity_code);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Activity_Code_2", pAccountDetail.activity_code_2);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Currency", pAccountDetail.currency);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Fiscal_1", pAccountDetail.fiscal);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Fiscal_2", pAccountDetail.fiscal_2);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Fiscal_3", pAccountDetail.fiscal_3);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Turnover_1", pAccountDetail.turnover);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Turnover_2", pAccountDetail.turnover_2);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Turnover_3", pAccountDetail.turnover_3);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Export_1", pAccountDetail.export);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Export_2", pAccountDetail.export_2);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Export_3", pAccountDetail.export_3);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Result_1", pAccountDetail.result);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Result_2", pAccountDetail.result_2);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Result_3", pAccountDetail.result_3);                       
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Sales_Aborad_1", pAccountDetail.sales_abroad);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Sales_Aborad_2", pAccountDetail.sales_abroad_2);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Sales_Aborad_3", pAccountDetail.sales_abroad_3);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Category", pAccountDetail.category);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_County", pAccountDetail.county);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_BV Source", pAccountDetail.bv_source);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Regions", pAccountDetail.regions);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Modified", DateTime.Now.ToString("yyyy-MM-dd") + " by " + UserSession.CurrentUser.UserFullName);
        }
        /** /
        public void SaveCompanyAppointment(SubCampaignAppointment pAccountAppointment)
        {
            //if (m_CurrentCampaignListAccount == null)
            //    return;

            //ObjectSubCampaign.SaveSubCampaignCompanyAppointmentStatus(m_CurrentCampaignListAccount.account_id, m_CurrentCampaignListAccount.final_list_id, pAccountAppointment.CompanyAppointmentStatus, pAccountAppointment.CompanyAppointmentLeadStatus);

            ObjectSubCampaign.SaveSubCampaignCompanyAppointmentStatus(pAccountAppointment.AccountId, pAccountAppointment.FinalListId, pAccountAppointment.CompanyAppointmentStatus, pAccountAppointment.CompanyAppointmentLeadStatus);
            m_CompanyAppointmentStatus = pAccountAppointment.CompanyAppointmentStatus;
            m_CompanyAppointmentLeadStatus = pAccountAppointment.CompanyAppointmentLeadStatus;

            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Status", pAccountAppointment.CompanyAppointmentStatus);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_Contact", DateTime.Today.ToShortDateString());
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_User", UserSession.CurrentUser.UserFullName);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Lead_Status", pAccountAppointment.CompanyAppointmentLeadStatus);
            this.BestFitGridColumns();
            //gvCampaignList.BestFitColumns();
        }
        /**/
        public bool HasAccountsAndContacts()
        {
            if (gcCampaignList == null)
                return false;
            else if (gvCampaignList.RowCount < 1)
                return false;

            return true;
        }
        public void CloseCompany()
        {
            m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
        }
        public void InitializeCampaignListControls(bool ActiveState)
        {
            btnRemoveCompany.Enabled = ActiveState;
            btnWorkOnCompany.Enabled = ActiveState;
            btnChangeView.Enabled = ActiveState;
            btnRefreshList.Enabled = ActiveState;
            btnReleaseLock.Enabled = ActiveState;
            btnAddCompany.Enabled = ActiveState;
            cbxShowOnlyNonFinished.Enabled = ActiveState;
            cbxFilterMeLastUser.Enabled = ActiveState;
            gcCampaignList.Enabled = ActiveState;
            //gvCampaignList.OptionsFind.AlwaysVisible = ActiveState;
            lblTotalRows.Text = "Records: 0";
            btnSaveAsNotQualified.Enabled = ActiveState;
            //btnCampaignListFind.Enabled = ActiveState;
            //btnCampaignListClearFind.Enabled = ActiveState;
            //btnCampaignListClearColumnFilter.Enabled = ActiveState;
            //tbxCampaignListFind.Enabled = ActiveState;
        }
        public void NullifyDataSource()
        {
            gcCampaignList.DataSource = null;
        }
        public void LoadSelectedCampaign(string pSelectedCampaignListParams)
        {
            WaitDialog.Show("Loading data.");
            string[] _ids = pSelectedCampaignListParams.Split(';');
            string[] _customer = _ids[0].Split('|');
            string[] _campaign = _ids[1].Split('|');
            string[] _sub_campaign = _ids[2].Split('|');

            CustomerId = Convert.ToInt32(_customer[0]);
            CustomerName = _customer[1].ToString();
            CampaignId = Convert.ToInt32(_campaign[0]);
            CampaignName = _campaign[1].ToString();
            SubCampaignId = Convert.ToInt32(_sub_campaign[0]);
            SubCampaignName = _sub_campaign[1].ToString();

            UserSession objUser = UserSession.CurrentUser;
            UserSession.CurrentUser.IsCampaignOwner = ObjectUser.IsCampaignOwner(objUser.UserId, SubCampaignId);
            UserSession.CurrentUser.IsSubCampaignManager = ObjectUser.IsSubCampaignManager(objUser.UserId, SubCampaignId);
            UserSession.CurrentUser.IsSubCampaignSales = ObjectUser.IsSubCampaignSales(objUser.UserId, SubCampaignId);
            UserSession.CurrentUser.IsCustomerUser = ObjectUser.IsCustomerUser(objUser.UserId, SubCampaignId);
            cbxFilterMeLastUser.Checked = false;
            cbxShowOnlyNonFinished.Checked = false;
            m_DoneLoadingCampaignList = false;

            m_BrightSalesProperty.CampaignList.CampaignListMode = SelectionProperty.CampaignListMode.CompaniesOnly;
            btnChangeView.Text = "Show Companies && Contacts";
            //m_ChangingCampaignList = true;
            if (m_BrightSalesProperty.CommonProperty.IsLoadEventLog)
                this.LoadCampaignList(eCampaignListLoadCallee.EventLogOnLoad);
            else
                this.LoadCampaignList(eCampaignListLoadCallee.CampaignListChange);

            if (gvCampaignList.RowCount > 0) {
                //this.SetCampaignListAppointmentParams();

                /**
                 * get sub-campaign status lists
                 */
                string _XmlData;
                using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    _XmlData = _efDbModel.subcampaigns.FirstOrDefault(i => i.id == SubCampaignId).xml_config_data;
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
            }

            //m_ChangingCampaignList = false;
            m_DoneLoadingCampaignList = true;
            gvCampaignList.Focus();
            WaitDialog.Close();
        }
        public void WorkCompanyOnCampaignListLoad()
        {
            //this.GetCurrentCampaignListAccount();
            this.WorkOnSelectedAccount();

            //if (gvCampaignList.RowCount < 1)
            //    return;

            ////this.GetCurrentCampaignListAccount();
            ////this.ReleaseCurrentCompanyLock();
            //var view = gvCampaignList;
            //try {
            //    //this.GetCurrentCampaignListAccount();
            //    bool islocked = m_CurrentCampaignListAccount.locked;
            //    int? lockedBy = m_CurrentCampaignListAccount.locked_by;
            //    if (islocked && lockedBy != UserSession.CurrentUser.UserId) {
            //        btnWorkOnCompany.Enabled = false;
            //        btnRemoveCompany.Enabled = false;
            //    }
            //    else {
            //        btnWorkOnCompany.Enabled = true;
            //        btnRemoveCompany.Enabled = true;
            //    }

            //    if ((islocked && lockedBy == UserSession.CurrentUser.UserId) || !islocked) {
            //        var BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            //        m_CurrentCampaignListAccount.locked = true;
            //        m_CurrentCampaignListAccount.locked_by = UserSession.CurrentUser.UserId;
            //        m_CurrentCampaignListAccount.locked_timestamp =
            //            BPContext.FIUpdateUserLock(
            //                m_FinalListId,
            //                m_AccountId,
            //                m_CurrentCampaignListAccount.locked,
            //                m_CurrentCampaignListAccount.locked_by).FirstOrDefault();

            //        view.SetRowCellValue(view.FocusedRowHandle, "locked", m_CurrentCampaignListAccount.locked);
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked_by", m_CurrentCampaignListAccount.locked_by);
            //        //view.SetRowCellValue(view.FocusedRowHandle, "locked_user", UserSession.CurrentUser.UserFullName);
            //        view.SetRowCellValue(view.FocusedRowHandle, "Locked_By_User", UserSession.CurrentUser.UserFullName);
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked_timestamp", m_CurrentCampaignListAccount.locked_timestamp);
            //        this.LoadCampaignBooking(ObjectEventSender.btnWorkOnCompany_Click);
            //    }
            //    else {
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked", true);
            //        NotificationDialog.Warning("Bright Sales", "The selected company is currently edited by another user. Please try again later.");
            //        m_CurrentCampaignListAccount = null;
            //        //btnRefreshList_Click(null, null);
            //    }
            //}
            //catch { 
            //}
        }
        
        public void CompanySetFocus(int pAcctId)
        {
            IsSuccessFocus = false;
            m_AccountId = pAcctId;
            try {
                for (int i = 0; i < gvCampaignList.RowCount; i++) {
                    if (gvCampaignList.GetRowCellValue(i, "account_id") == null)
                        break;

                    if (ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(i, "account_id").ToString()) != pAcctId)
                        continue;

                    /**
                     * special case only: 
                     * will try to force trigger the grid event -> gvCampaignList_FocusedRowChanged(),
                     * when both FocusedRowHandle and i are equals to 0. because this means that the event
                     * will not trigger, thus skipping to get important transactions from the said event.
                     */
                    if (gvCampaignList.FocusedRowHandle == 0 && i == 0)
                        this.gvCampaignList_FocusedRowChanged(null, null);
                    else
                        gvCampaignList.FocusedRowHandle = i;

                    IsSuccessFocus = true;
                    //this.WorkCompanyOnCampaignListLoad();
                    break;
                }
            }
            catch (Exception e) {
                m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                    ErrorMessage = e.Message
                });
            }
        }
        //public void CompanySetFocusRow(int pAcctId)
        //{
        //    IsSuccessFocus = false;
        //    m_AccountId = pAcctId;
        //    try
        //    {
        //        for (int i = 0; i < gvCampaignList.RowCount; i++)
        //        {
        //            if (gvCampaignList.GetRowCellValue(i, "account_id") == null)
        //                break;

        //            if (ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(i, "account_id").ToString()) == pAcctId)
        //            {
        //                gvCampaignList.FocusedRowHandle = i;
        //                IsSuccessFocus = true;
        //                break;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return;
        //    }
        //}
        #endregion

        #region Private Methods
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<ManageCampaignBookingEvents.OnLoadNextCompany.CampaignList>().Subscribe(MoveNext);
            m_EventBus.GetEvent<ManageCampaignBookingEvents.OnLoadPreviousCompany.CampaignList>().Subscribe(MovePrevious);
            m_EventBus.GetEvent<ManageCampaignBookingEvents.OnStatusChange>().Subscribe(CampaignBookingOnStatusChange);
            m_EventBus.GetEvent<ManageCampaignBookingEvents.OnSave.ManageCampaignList>().Subscribe(CampaignBookingOnSave);
            m_EventBus.GetEvent<CampaignListEvents.GetLatestProperties>().Subscribe(GetProperties);
            m_EventBus.GetEvent<FrmSalesConsultantEvents.OnEnterCampaignListCombo>().Subscribe(SalesConsultantCampaignListComboOnPressEnter);
            m_EventBus.GetEvent<FrmSalesConsultantEvents.TryFocusRowChange>().Subscribe(TryFocusRowChange);
        }
        private void MoveNext(ManageCampaignBookingEvents.OnLoadNextCompany.CampaignList e)
        {
            m_BrightSalesProperty.CampaignBooking.LoadNextCompanySuccess = false;
            if (gvCampaignList.RowCount < 1)
                return;

            /**
             * check current company if being released by the the user before loading the next company.
             */
            sub_campaign_account_lists _item = null;
            int _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
            int _FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                    i.account_id == _AcctId &&
                    i.final_list_id == _FinalListId
                );
                if (_item != null)
                    _efDbContext.Detach(_item);

                if ((_item.locked && _item.locked_by == UserSession.CurrentUser.UserId) || !_item.locked) {
                    gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", false);
                    this.GetCurrentCampaignListAccount();
                    this.ReleaseCurrentCompanyLock();
                }

                /**
                 * [@jeff 10.18.2013]: https://brightvision.jira.com/browse/PLATFORM-2638
                 * added logic to go to the next company if campaign list is on companies and contacts mode.
                 */
                int _Row = gvCampaignList.FocusedRowHandle + 1;
                for (; _Row < gvCampaignList.RowCount; _Row++) {
                    int _NextAcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(_Row, "account_id").ToString());
                    if (_AcctId != _NextAcctId)
                        break;
                }

                /**
                 * now, we will load the next company and re-check if this is currently worked by other consultants.
                 */
                m_BrightSalesProperty.CommonProperty.CompanyLocked = false;
                gvCampaignList.FocusedRowHandle = _Row;
                _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
                _FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
                _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                    i.account_id == _AcctId &&
                    i.final_list_id == _FinalListId
                );
                if (_item.locked && _item.locked_by > 0 && _item.locked_by != UserSession.CurrentUser.UserId) {
                    user _user = _efDbContext.users.FirstOrDefault(i => i.id == _item.locked_by);
                    if (_user != null)
                        _efDbContext.Detach(_user);

                    m_BrightSalesProperty.CommonProperty.CompanyLocked = true;
                    NotificationDialog.Warning("Bright Sales", string.Format("{0}This company is currently worked by{0}{1}", Environment.NewLine, _user.fullname));
                }

                this.SetCampaignListAppointmentParams();
                if (!m_BrightSalesProperty.CommonProperty.CompanyLocked) {
                    bool _State = _item.locked && _item.locked_by != UserSession.CurrentUser.UserId ? false : true;
                    btnWorkOnCompany.Enabled = _State;
                    btnRemoveCompany.Enabled = _State;
                    if ((_item.locked && _item.locked_by == UserSession.CurrentUser.UserId) || !_item.locked) {
                        m_CurrentCampaignListAccount.locked = true;
                        m_CurrentCampaignListAccount.locked_by = UserSession.CurrentUser.UserId;
                        m_CurrentCampaignListAccount.locked_timestamp = _efDbContext.FIUpdateUserLock(
                            m_FinalListId,
                            m_AccountId,
                            m_CurrentCampaignListAccount.locked,
                            m_CurrentCampaignListAccount.locked_by
                        ).FirstOrDefault();
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", m_CurrentCampaignListAccount.locked);
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_by", m_CurrentCampaignListAccount.locked_by);
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Locked_By_User", UserSession.CurrentUser.UserFullName);
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_timestamp", m_CurrentCampaignListAccount.locked_timestamp);
                    }
                    gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", true);
                }
            }
            m_CampaignListSelectedRow = gvCampaignList.FocusedRowHandle < 0 ? 0 : gvCampaignList.FocusedRowHandle;
            m_BrightSalesProperty.CampaignBooking.LoadNextCompanySuccess = true;
            return;
        }
        private void MovePrevious(ManageCampaignBookingEvents.OnLoadPreviousCompany.CampaignList e)
        {
            m_BrightSalesProperty.CampaignBooking.LoadPreviousCompanySuccess = false;
            if (gvCampaignList.RowCount < 1)
                return;

            /**
             * check current company if being released by the the user before loading the next company.
             */
            sub_campaign_account_lists _item = null;
            int _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
            int _FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                    i.account_id == _AcctId &&
                    i.final_list_id == _FinalListId
                );
                if (_item != null)
                    _efDbContext.Detach(_item);

                if ((_item.locked && _item.locked_by == UserSession.CurrentUser.UserId) || !_item.locked) {
                    gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", false);
                    this.GetCurrentCampaignListAccount();
                    this.ReleaseCurrentCompanyLock();
                }

                /**
                * [@jeff 10.18.2013]: https://brightvision.jira.com/browse/PLATFORM-2638
                * added logic to go to the next company if campaign list is on companies and contacts mode.
                */
                int _Row = 0;
                if (gvCampaignList.FocusedRowHandle > 0)
                    _Row = gvCampaignList.FocusedRowHandle - 1;

                for (; _Row < gvCampaignList.RowCount; _Row--) {
                    int _PrevAcctId = ValidationUtility.TryParseInt(ValidationUtility.IFNullString(gvCampaignList.GetRowCellValue(_Row, "account_id"),"0"));
                    if (_AcctId != _PrevAcctId)
                        break;
                }

                /**
                 * now, we will load the next company and re-check if this is currently worked by other consultants.
                 */
                m_BrightSalesProperty.CommonProperty.CompanyLocked = false;
                gvCampaignList.FocusedRowHandle = _Row;
                _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
                _FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
                _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                    i.account_id == _AcctId &&
                    i.final_list_id == _FinalListId
                );
                if (_item.locked && _item.locked_by > 0 && _item.locked_by != UserSession.CurrentUser.UserId) {
                    user _user = _efDbContext.users.FirstOrDefault(i => i.id == _item.locked_by);
                    if (_user != null)
                        _efDbContext.Detach(_user);

                    m_BrightSalesProperty.CommonProperty.CompanyLocked = true;
                    NotificationDialog.Warning("Bright Sales", string.Format("{0}This company is currently worked by{0}{1}", Environment.NewLine, _user.fullname));
                }

                this.SetCampaignListAppointmentParams();
                if (!m_BrightSalesProperty.CommonProperty.CompanyLocked) {
                    bool islocked = m_CurrentCampaignListAccount.locked;
                    int? lockedBy = m_CurrentCampaignListAccount.locked_by;
                    bool _State = islocked && lockedBy != UserSession.CurrentUser.UserId ? false : true;
                    btnWorkOnCompany.Enabled = _State;
                    btnRemoveCompany.Enabled = _State;
                    if ((islocked && lockedBy == UserSession.CurrentUser.UserId) || !islocked) {
                        //var BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
                        m_CurrentCampaignListAccount.locked = true;
                        m_CurrentCampaignListAccount.locked_by = UserSession.CurrentUser.UserId;
                        m_CurrentCampaignListAccount.locked_timestamp = _efDbContext.FIUpdateUserLock(
                            m_FinalListId,
                            m_AccountId,
                            m_CurrentCampaignListAccount.locked,
                            m_CurrentCampaignListAccount.locked_by
                        ).FirstOrDefault();
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", m_CurrentCampaignListAccount.locked);
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_by", m_CurrentCampaignListAccount.locked_by);
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Locked_By_User", UserSession.CurrentUser.UserFullName);
                        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_timestamp", m_CurrentCampaignListAccount.locked_timestamp);
                    }
                    gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", true);
                }
            }
            m_CampaignListSelectedRow = gvCampaignList.FocusedRowHandle < 0 ? 0 : gvCampaignList.FocusedRowHandle;
            m_BrightSalesProperty.CampaignBooking.LoadPreviousCompanySuccess = true;
            return;
        }
        private void GetProperties(CampaignListEvents.GetLatestProperties e)
        {
            this.GetProperties(e.ForWorkModePurpose);
        }
        private void GetProperties(bool pForWorkModePurpose)
        {
            if (gvCampaignList.RowCount < 1)
                return;

            if (m_objSubCampaignAppointmentParams == null)
                this.SetCampaignListAppointmentParams();

            m_BrightSalesProperty.CommonProperty.ContactId = m_ContactId;
            m_BrightSalesProperty.CommonProperty.CustomerId = m_objSubCampaignAppointmentParams.CustomerId;
            m_BrightSalesProperty.CommonProperty.CampaignId = m_objSubCampaignAppointmentParams.CampaignId;
            m_BrightSalesProperty.CommonProperty.SubCampaignId = m_objSubCampaignAppointmentParams.SubCampaignId;
            m_BrightSalesProperty.CommonProperty.AccountId = m_objSubCampaignAppointmentParams.AccountId;
            m_BrightSalesProperty.CommonProperty.FinalListId = m_objSubCampaignAppointmentParams.FinalListId;
            m_BrightSalesProperty.CommonProperty.CompanyName = m_CompanyName;

            if (pForWorkModePurpose) {
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    /**
                     * get company locked property.
                     */
                    sub_campaign_account_lists _eftAccount = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                        i.account_id == m_BrightSalesProperty.CommonProperty.AccountId &&
                        i.final_list_id == m_BrightSalesProperty.CommonProperty.FinalListId
                    );
                    if (_eftAccount != null) {
                        _efDbContext.Detach(_eftAccount);
                        m_BrightSalesProperty.CommonProperty.CompanyLocked = false;
                        if (_eftAccount.locked && _eftAccount.locked_by != UserSession.CurrentUser.UserId)
                            m_BrightSalesProperty.CommonProperty.CompanyLocked = true;
                    }

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
                        _efDbContext.Detach(_eftAccountAppt);
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
                            _efDbContext.Detach(_eftContactAppt);
                            int _idx = m_lstContactStatuses.FindIndex(i => i.status.Equals(_eftContactAppt.status));
                            if (_idx != null && _idx > 0) {
                                for (int i = 0; i < m_lstContactStatuses.Count; i++)
                                    m_lstContactStatuses[i].selected = false;
                                m_lstContactStatuses[_idx].selected = true;
                                //m_ContactStatusSelectedIndex = _idx;
                            }
                        }
                    }
                }

                m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId = m_objSubCampaignAppointmentParams.AccountId;
                m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId = m_ContactId;
                m_BrightSalesProperty.CampaignBooking.Appointment = new CampaignBookingProperty.AppointmentProperty() {
                    CompanyWebsite = m_objSubCampaignAppointmentParams.CompanyWebsite,
                    CompanyAppointmentStatus = m_objSubCampaignAppointmentParams.CompanyAppointmentStatus,
                    CompanyBoardNumber = m_objSubCampaignAppointmentParams.CompanyBoardNumber,
                    CompanyAppointmentLeadStatus = m_objSubCampaignAppointmentParams.CompanyAppointmentLeadStatus
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

                string _LastUpdateInfo = ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(m_FinalListId, m_BrightSalesProperty.CommonProperty.AccountId);
                m_BrightSalesProperty.CampaignBooking.BreadCrumb = string.Format("{0}{1}{2}{3}",
                    string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                    string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                    string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
                    string.IsNullOrEmpty(_LastUpdateInfo) ? string.Empty : string.Format(" ({0})", _LastUpdateInfo)
                );
            }
        }
        private void TryFocusRowChange(FrmSalesConsultantEvents.TryFocusRowChange e)
        {
            if (gvCampaignList.RowCount < 1)
                return;

            if (gvCampaignList.FocusedRowHandle == 0) {
                this.SetGridChangeRowEvent(false);
                gvCampaignList.FocusedRowHandle = GridControl.InvalidRowHandle;
                this.SetGridChangeRowEvent(true);
            }
            this.gvCampaignList_FocusedRowChanged(null, null);
        }
        private void CampaignBookingOnStatusChange(ManageCampaignBookingEvents.OnStatusChange e)
        {
            ObjectSubCampaign.SaveSubCampaignCompanyAppointmentStatus(
                m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                m_BrightSalesProperty.CommonProperty.FinalListId,
                m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus,
                m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus
            );
            m_CompanyAppointmentStatus = m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus;
            m_CompanyAppointmentLeadStatus = m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus;

            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Status", m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_Contact", DateTime.Today.ToShortDateString());
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_User", UserSession.CurrentUser.UserFullName);
            gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Lead_Status", m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus);
            this.BestFitGridColumns();
        }
        private void CampaignBookingOnSave(ManageCampaignBookingEvents.OnSave.ManageCampaignList e)
        {
            if (m_CompanyAppointmentLeadStatus != m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus ||
                e.IsReleasedCancel == false)
            {
                ObjectSubCampaign.SaveSubCampaignCompanyAppointmentStatus(
                    m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                    m_BrightSalesProperty.CommonProperty.FinalListId,
                    m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus,
                    m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus
                );
                m_CompanyAppointmentStatus = m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus;
                m_CompanyAppointmentLeadStatus = m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus;

                /*
                 * https://brightvision.jira.com/browse/PLATFORM-3047
                 * DAN: As it is the reason why the issue occur.
                 */
                m_objSubCampaignAppointmentParams.CompanyAppointmentStatus = m_CompanyAppointmentStatus;
                m_objSubCampaignAppointmentParams.CompanyAppointmentLeadStatus = m_CompanyAppointmentLeadStatus;

                //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Status", m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus);
                //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_Contact", DateTime.Today.ToShortDateString());
                //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_User", UserSession.CurrentUser.UserFullName);
                //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Lead_Status", m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus);

                int _AcctId = 0;
                for (int i = 0; i < gvCampaignList.RowCount; i++)
                {
                    _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(i, "account_id"));
                    if (_AcctId == m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId)
                    {
                        gvCampaignList.SetRowCellValue(i, "Company_Status", m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus);
                        gvCampaignList.SetRowCellValue(i, "Company_Last_Contact", DateTime.Today.ToShortDateString());
                        gvCampaignList.SetRowCellValue(i, "Company_Last_User", UserSession.CurrentUser.UserFullName);
                        gvCampaignList.SetRowCellValue(i, "Lead_Status", m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus);
                    }
                }

                this.BestFitGridColumns();
            }

            //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_Contact", DateTime.Today.ToShortDateString());
            //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_User", UserSession.CurrentUser.UserFullName);
        }
        
        private void SalesConsultantCampaignListComboOnPressEnter(FrmSalesConsultantEvents.OnEnterCampaignListCombo e)
        {
            m_SalesConsultantCampaignListComboKeyPressEnter = true;
        }

        private void BestFitGridColumns()
        {
            this.gvCampaignList.RowCellStyle -= new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gvCampaignList_RowCellStyle);
            gvCampaignList.BestFitColumns();
            this.gvCampaignList.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gvCampaignList_RowCellStyle);
        }
        private void ClearGridFilter()
        {
            gvCampaignList.ClearColumnsFilter();
            gvCampaignList.ActiveFilterString = string.Empty;
            //tbxCampaignListFind.Text = string.Empty;
            cbxFilterMeLastUser.Checked = false;
            cbxShowOnlyNonFinished.Checked = false;
            m_CampaignListFindFilter = string.Empty;
            m_CampaignListSpecificColumnFilter = string.Empty;
            m_CampaignListCheckedFilter = string.Empty;
        }
        private void ApplyGridFilter()
        {
            if (!m_DoneLoadingCampaignList || gcCampaignList.DataSource == null)
                return;

            List<string> _filter = new List<string>();
            if (!string.IsNullOrEmpty(m_CampaignListFindFilter))
                _filter.Add(string.Format("({0})", m_CampaignListFindFilter));

            if (!string.IsNullOrEmpty(m_CampaignListCheckedFilter))
                _filter.Add(string.Format("({0})", m_CampaignListCheckedFilter));

            if (!string.IsNullOrEmpty(m_CampaignListSpecificColumnFilter))
                _filter.Add(string.Format("({0})", m_CampaignListSpecificColumnFilter));

            if (_filter.Count > 0) {
                string _ActiveFilter = string.Join(" And ", _filter.ToArray());
                gvCampaignList.ActiveFilterString = _ActiveFilter;
            }
            else
                gvCampaignList.ActiveFilterString = string.Empty;
        }
        private void ApplyCheckedFilter()
        {
            if (!m_DoneLoadingCampaignList || gcCampaignList.DataSource == null)
                return;

            m_DoneLoadingCampaignList = false;
            lblTotalRows.Text = "Records: 0";

            List<string> _filter = new List<string>();
            if (cbxFilterMeLastUser.Checked)
                _filter.Add(string.Format("[Company_Last_User] = '{0}'", UserSession.CurrentUser.UserFullName));
            if (cbxShowOnlyNonFinished.Checked)
                _filter.Add("[Company_Status] != 'Finished'");

            //if (!string.IsNullOrEmpty(m_CampaignListFindFilter))
            //    _filter.Add(m_CampaignListFindFilter);

            //string _CheckedFilter = this.GetCheckedFilters();
            //if (!string.IsNullOrEmpty(_CheckedFilter))
            //    _filter.Add(_CheckedFilter);

            //if (_filter.Count == 1)
            //    gvCampaignList.ActiveFilterString = _filter[0];
            //else if (_filter.Count == 2)
            //    gvCampaignList.ActiveFilterString = string.Format("({0}) And ({1})", _filter[0], _filter[1]);
            //else if (_filter.Count < 1)
            //    gvCampaignList.ActiveFilterString = string.Empty;

            gvCampaignList.ActiveFilterString = string.Join(" And ", _filter.ToArray());
            m_DoneLoadingCampaignList = true;

            if (gvCampaignList.RowCount > 0)
                lblTotalRows.Text = string.Format("Records: {0}", gvCampaignList.RowCount.ToString());
        }
        //private void CampaignListFocusedRowChange(object objSender, int iFocusedRowhandle)
        //{
        //    this.GetSelectedCampaignListRowCommonlyUsedFields();
        //    if (m_AccountId == 0)
        //        return;

        //    if (m_DoneLoadingCampaignList)
        //    {
        //        WaitDialog.Show(this.ParentForm, "", "Loading components...");
        //        //this.GetSelectedCampaignListRowCommonlyUsedFields();
        //        //this.SetCompanyContactInformation(); //todo: to be handled
        //        //m_DoneLoadingCollectedData = false; //todo: to be handled
        //        //this.InitializeContactButtonControls(false); //todo: to be handled
        //        //if (CampaignListMode == eCampaignListMode.CompaniesAndContacts) ////todo: to be handled
        //        //    this.InitializeContactButtonControls(true);
        //        //m_DoneLoadingCollectedData = true; //todo: to be handled

        //        // set selected company if its not changing current view mode
        //        //if (!m_ChangingCampaignListMode)
        //        //    m_SelectedCompanyId = m_objCompanyAndContact.account_id;

        //        //todo: to be handled
        //        //if (m_SelectedPageName.Equals(lcgCollectedData.Name) && m_objCompanyAndContact.contact_id > 0)
        //        //    m_objCollectedDataView.ContactId = m_objCompanyAndContact.contact_id;

        //        //if (m_SelectedPageName.Equals(lcgGeoMapViewerCompany.Name))
        //        //    this.ShowCompanyGeoLocation(false);

        //        //else if (m_SelectedPageName.Equals(lcgGeoMapViewerContact.Name))
        //        //    this.ShowContactGeoLocation(false);

        //        //else if (m_SelectedPageName.Equals(lcgCollectedData.Name) && m_DoneLoadingCollectedData)
        //        //    this.LoadSelectedPage(m_SelectedPageName);

        //        //else
        //        //    this.LoadSelectedPage(m_SelectedPageName);

        //        //set enable property for button work company
        //        try
        //        {
        //            var view = objSender as GridView;
        //            bool islocked = (bool)view.GetRowCellValue(iFocusedRowhandle, "locked");
        //            int? lockedBy = ValidationUtility.TryParseInt(ValidationUtility.IFNullString(view.GetRowCellValue(iFocusedRowhandle, "locked_by"), "0"));
        //            if (islocked && lockedBy != UserSession.CurrentUser.UserId) {
        //                btnWorkOnCompany.Enabled = false;
        //                btnRemoveCompany.Enabled = false;
        //            }
        //            else {
        //                btnWorkOnCompany.Enabled = true;
        //                btnRemoveCompany.Enabled = true;
        //            }
        //        }
        //        catch { 
        //        }

        //        WaitDialog.Close();
        //    }
        //}
        private void GetSelectedCampaignListRowCommonlyUsedFields()
        {
            m_AccountId = 0;
            m_ContactId = 0;
            m_FinalListId = 0;
            m_CampaignListId = 0;
            m_CompanyName = null;
            m_CompanyWebsite = null;
            m_CompanyBoardNumber = null;
            m_CompanyAppointmentStatus = null;
            m_CompanyAppointmentLeadStatus = null;
            m_CompanyCity = null;
            m_CompanyCountry = null;
            m_AccountLocked = false;
            m_AccountLockedBy = 0;
            m_AccountLockUser = null;
            m_CompanyAppointmentStatus = "";
            m_CompanyAppointmentLeadStatus = "";

            if (gcCampaignList == null)
                return;

            if (gvCampaignList.RowCount < 1 || m_CampaignListSelectedRow < 0)
                return;

            if (m_CampaignListSelectedRow >= gvCampaignList.RowCount)
                m_CampaignListSelectedRow = 0;

            /**
             * if companies only.
             */
            #region Code Logic
            if (m_BrightSalesProperty.CampaignList.CampaignListMode == SelectionProperty.CampaignListMode.CompaniesOnly) {
                vw_campaign_list_companies _data = gvCampaignList.GetRow(m_CampaignListSelectedRow) as vw_campaign_list_companies;
                if (_data == null || _data.id == null)
                    return;

                m_AccountId = ValidationUtility.TryParseInt(_data.account_id.ToString());
                m_FinalListId = ValidationUtility.TryParseInt(_data.final_list_id.ToString());

                try {
                    m_ContactId = ValidationUtility.TryParseInt(_data.contact_id.ToString());
                }
                catch (Exception e) {
                    m_ContactId = 0;
                    m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                        ErrorMessage = e.Message
                    });
                }

                m_CampaignListId = ValidationUtility.TryParseInt(_data.id.ToString());
                if (_data.Company_City != null)
                    m_CompanyCity = _data.Company_City.ToString();
                if (_data.Company_Country != null)
                    m_CompanyCountry = _data.Company_Country.ToString();
                if (_data.Company_Name != null)
                    m_CompanyName = _data.Company_Name.ToString();
                if (_data.Company_Site != null)
                    m_CompanyWebsite = _data.Company_Site.ToString();
                if (_data.Company_Telephone != null)
                    m_CompanyBoardNumber = _data.Company_Telephone.ToString();
                if (_data.Company_Status != null)
                    m_CompanyAppointmentStatus = _data.Company_Status.ToString();
                if (_data.Lead_Status != null)
                    m_CompanyAppointmentLeadStatus = _data.Lead_Status.ToString();
                if (_data.locked != null)
                    m_AccountLocked = Convert.ToBoolean(_data.locked);
                if (_data.locked_by != null)
                    m_AccountLockedBy = ValidationUtility.TryParseInt(_data.locked_by.ToString());
                if (_data.Locked_By_User != null)
                    m_AccountLockUser = _data.Locked_By_User.ToString();
            }
            #endregion

            /**
             * if companies and contacts.
             */
            #region Code Logic
            else if (m_BrightSalesProperty.CampaignList.CampaignListMode == SelectionProperty.CampaignListMode.CampaniesAndContacts) {
                vw_campaign_list_companies_and_contacts _data = gvCampaignList.GetRow(m_CampaignListSelectedRow) as vw_campaign_list_companies_and_contacts;
                if (_data == null || _data.id == null)
                    return;

                m_AccountId = ValidationUtility.TryParseInt(_data.account_id.ToString());
                m_FinalListId = ValidationUtility.TryParseInt(_data.final_list_id.ToString());

                try {
                    m_ContactId = ValidationUtility.TryParseInt(_data.contact_id.ToString());
                }
                catch (Exception e) {
                    m_ContactId = 0;
                    m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                        ErrorMessage = e.Message
                    });
                }

                m_CampaignListId = ValidationUtility.TryParseInt(_data.id.ToString());
                if (_data.Company_City != null)
                    m_CompanyCity = _data.Company_City.ToString();
                if (_data.Company_Country != null)
                    m_CompanyCountry = _data.Company_Country.ToString();
                if (_data.Company_Name != null)
                    m_CompanyName = _data.Company_Name.ToString();
                if (_data.Company_Site != null)
                    m_CompanyWebsite = _data.Company_Site.ToString();
                if (_data.Company_Telephone != null)
                    m_CompanyBoardNumber = _data.Company_Telephone.ToString();
                if (_data.Company_Status != null)
                    m_CompanyAppointmentStatus = _data.Company_Status.ToString();
                if (_data.Lead_Status != null)
                    m_CompanyAppointmentLeadStatus = _data.Lead_Status.ToString();
                if (_data.locked != null)
                    m_AccountLocked = Convert.ToBoolean(_data.locked);
                if (_data.locked_by != null)
                    m_AccountLockedBy = ValidationUtility.TryParseInt(_data.locked_by.ToString());
                if (_data.Locked_By_User != null)
                    m_AccountLockUser = _data.Locked_By_User.ToString();
            }
            #endregion

            #region Log
            var log = BrightSalesFacade.Logger;
            log.SetLogField(LoggingField.account_id, m_AccountId.ToString());
            log.SetLogField(LoggingField.account_name, m_CompanyName);
            #endregion

            #region Old Code
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "id") == null)
            //    return;

            //m_AccountId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
            //m_FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
            
            //try {
            //    m_ContactId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "contact_id").ToString());
            //}
            //catch (Exception e) {
            //    m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() { 
            //        ErrorMessage = e.Message 
            //    });
            //}

            //m_CampaignListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "id").ToString());

            ////if (!string.IsNullOrEmpty(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_City").ToString()))
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_City") != null)
            //    m_CompanyCity = gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_City").ToString();

            ////if (!string.IsNullOrEmpty(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Country").ToString()))
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Country") != null)
            //    m_CompanyCountry = gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Country").ToString();

            ////if (!string.IsNullOrEmpty(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Name").ToString()))
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Name") != null)
            //    m_CompanyName = gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Name").ToString();

            ////if (!string.IsNullOrEmpty(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Site").ToString()))
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Site") != null)
            //    m_CompanyWebsite = gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Site").ToString();

            ////if (!string.IsNullOrEmpty(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Telephone").ToString()))
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Telephone") != null)
            //    m_CompanyBoardNumber = gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Telephone").ToString();

            //m_CompanyAppointmentStatus = "";
            ////if (!string.IsNullOrEmpty(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Status").ToString()))
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Status") != null)
            //    m_CompanyAppointmentStatus = gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Company_Status").ToString();

            //m_CompanyAppointmentLeadStatus = "";
            ////if (!string.IsNullOrEmpty(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Lead_Status").ToString()))
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Lead_Status") != null)
            //    m_CompanyAppointmentLeadStatus = gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "Lead_Status").ToString();

            ////if (!string.IsNullOrEmpty(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "locked").ToString()))
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "locked") != null)
            //    m_AccountLocked = Convert.ToBoolean(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "locked"));

            ////if (!string.IsNullOrEmpty(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "locked_by").ToString()))
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "locked_by") != null)
            //    m_AccountLockedBy = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "locked_by").ToString());

            ////if (!string.IsNullOrEmpty(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "locked_user").ToString()))
            //if (gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "locked_user") != null)
            //    m_AccountLockUser = gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "locked_user").ToString();
            #endregion
        }
        private void GetCurrentCampaignListAccount()
        {
            m_CurrentCampaignListAccount = null;
            this.GetSelectedCampaignListRowCommonlyUsedFields();
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                m_CurrentCampaignListAccount = _efDbModel.sub_campaign_account_lists.FirstOrDefault(p =>
                    p.account_id == m_AccountId &&
                    p.final_list_id == m_FinalListId &&
                    p.active == true
                );

                if (m_CurrentCampaignListAccount != null)
                    _efDbModel.Detach(m_CurrentCampaignListAccount);
            }
        }
        private bool IsCampaignListItemLocked(GridView view, int row)
        {
            try
            {
                bool isLocked = (bool)view.GetRowCellValue(row, "locked");
                if (isLocked)
                    return true;

                return false;
            }
            catch (Exception e)
            {
                m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                    ErrorMessage = e.Message
                });
                return false;
            }
        }
        private void LoadCampaignBooking(ObjectEventSender pEventSender)
        {
            if (gvCampaignList.RowCount < 1)
                return;

            this.BestFitGridColumns();
            if (m_AccountId > 0) {
                m_CampaignListSelectedRow = gvCampaignList.FocusedRowHandle < 0 ? 0 : gvCampaignList.FocusedRowHandle;
                m_CampaignListLastWorkedAccountId = m_AccountId;
            }

            this.GetProperties(true);
            //m_EventBus.Notify(new CampaignListEvents.AccountInfoChange());
            m_EventBus.Notify(new CampaignListEvents.PrepareExtraDetail());
            m_EventBus.Notify(new CampaignListEvents.PrepareCampaignBooking());

            //CampaignListArgs _Args = new CampaignListArgs() {
            //    ContactId = m_ContactId,
            //    CompanyName = m_CompanyName,
            //    CampaignBookingAppointment = m_objSubCampaignAppointmentParams,
            //    //CampaignListMode = CampaignListMode,
            //    AccountLeadStatusSelectedIndex = m_AccountLeadStatusSelectedIndex,
            //    AccountStatusSelectedIndex = m_AccountStatusSelectedIndex,
            //    ContactStatusSelectedIndex = m_ContactStatusSelectedIndex,
            //    AccountLeadStatusNotQualifiedIndex = m_AccountLeadStatusNotQualifiedIndex,
            //    AccountStatusNotQualifiedIndex = m_AccountStatusNotQualifiedIndex,
            //    ContactStatusNotQualifiedIndex = m_ContactStatusNotQualifiedIndex,
            //    AccountLeadStatuses = m_lstAccountLeadStatuses,
            //    AccountStatuses = m_lstAccountStatuses,
            //    ContactStatuses = m_lstContactStatuses,
            //    BreadCrumb = string.Format("{0}{1}{2}{3}",
            //        string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
            //        string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
            //        string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
            //        string.IsNullOrEmpty(_LastUpdateInfo) ? string.Empty : string.Format(" ({0})", _LastUpdateInfo)
            //    )
            //};

            /** /
            if (AccountModificationInfoChange != null)
                AccountModificationInfoChange();

            if (pEventSender == ObjectEventSender.btnWorkOnCompany_Click && btnWorkOnCompany_OnClick != null)
                btnWorkOnCompany_OnClick();
            else if (pEventSender == ObjectEventSender.gvCampaignList_DoubleClick && gvCampaignList_OnDoubleClick != null)
                gvCampaignList_OnDoubleClick();
            else if (pEventSender == ObjectEventSender.gvCampaignList_Enter && gvCampaignList_OnEnter != null)
                gvCampaignList_OnEnter();
            /**/

            //WaitDialog.Close();
        }
        private void LoadCampaignList(eCampaignListLoadCallee pLoadType)
        {
            /**
             * unsubscribe this event first so that only data source change
             * will be triggered. this to avoid double calls to related modules
             * when changing views or rows or data source.
             */
            this.SetGridChangeRowEvent(false);
            int _FocusedRow = gvCampaignList.FocusedRowHandle;
            DateTime logDate1 = DateTime.UtcNow;
            m_CampaignListSelectedRow = 0;

            /**
             * set this flag to let know the dialog component that a new campaign list
             * has been loaded, thus, needing to create another dialog instance based
             * on this loaded campaign.
             */
            if (pLoadType == eCampaignListLoadCallee.CampaignListChange ||
                pLoadType == eCampaignListLoadCallee.EventLogOnLoad)
                m_BrightSalesProperty.CampaignBooking.Questionnaire.State = SelectionProperty.DialogEditorState.Empty;

            try {
                m_lsmsCampaignListData = null;
                m_efDbModel = null;
                m_efDbModel = new BrightPlatformEntities(UserSession.EntityConnection) {
                    CommandTimeout = 0
                };

                gcCampaignList.DataSource = null;
                gvCampaignList.Columns.Clear();
                gvCampaignList.FindFilterText = string.Empty;
                lblTotalRows.Text = "Records: 0";
                this.InitializeCampaignListControls(false);
                this.ClearGridFilter();

                if (m_BrightSalesProperty.CampaignList.CampaignListMode == SelectionProperty.CampaignListMode.CompaniesOnly) {
                    m_efDbModel.FIPopulateCampaignListCompanies(SubCampaignId, UserSession.CurrentUser.UserId);
                    int _final_list_id = m_efDbModel.final_lists.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId).id;
                    m_lsmsCampaignListData = new LinqServerModeSource() {
                        DefaultSorting = "id",
                        KeyExpression = "uid",
                        QueryableSource = m_efDbModel.vw_campaign_list_companies.Where(i =>
                            i.final_list_id == _final_list_id &&
                            i.user_id == UserSession.CurrentUser.UserId
                        ).AsQueryable()
                        //QueryableSource = m_efDbModel.FIGetVwCampaignListCompanies(_final_list_id, UserSession.CurrentUser.UserId).ToList().AsQueryable()
                    };
                    m_lsmsCampaignListData.ExceptionThrown += new LinqServerModeExceptionThrownEventHandler(_lsmsCampaignListData_ExceptionThrown);
                    m_lsmsCampaignListData.InconsistencyDetected += new LinqServerModeInconsistencyDetectedEventHandler(_lsmsCampaignListData_InconsistencyDetected);

                    //gcCampaignList.DataSource = m_efDbModel.FIGetVwCampaignListCompanies(_final_list_id, UserSession.CurrentUser.UserId);
                    /** /
                    m_lsmsCampaignListData = new LinqServerModeSource()
                    {
                        DefaultSorting = "id",
                        KeyExpression = "uid",
                        QueryableSource = m_efDbModel.FIGetVwCampaignListCompanies(_final_list_id, UserSession.CurrentUser.UserId).ToList().AsQueryable()
                    };
                    m_lsmsCampaignListData.ExceptionThrown += new LinqServerModeExceptionThrownEventHandler(_lsmsCampaignListData_ExceptionThrown);
                    m_lsmsCampaignListData.InconsistencyDetected += new LinqServerModeInconsistencyDetectedEventHandler(_lsmsCampaignListData_InconsistencyDetected);
                    /**/

                    gcCampaignList.DataSource = m_lsmsCampaignListData;
                    gvCampaignList.Columns["uid"].Visible = false;
                    gvCampaignList.Columns["user_id"].Visible = false;
                    gvCampaignList.Columns["id"].Visible = false;
                    gvCampaignList.Columns["account_id"].Visible = false;
                    gvCampaignList.Columns["locked"].Visible = false;
                    gvCampaignList.Columns["locked_by"].Visible = false;
                    gvCampaignList.Columns["locked_timestamp"].Visible = false;
                    gvCampaignList.Columns["final_list_id"].Visible = false;
                    gvCampaignList.Columns["account_latitude"].Visible = false;
                    gvCampaignList.Columns["account_longitude"].Visible = false;
                    gvCampaignList.Columns["contact_id"].Visible = false;
                    gvCampaignList.Columns["account_address"].Visible = false;
                    gvCampaignList.Columns["contact_status"].Visible = false;
                }
                else if (m_BrightSalesProperty.CampaignList.CampaignListMode == SelectionProperty.CampaignListMode.CampaniesAndContacts) {
                    int _final_list_id = m_efDbModel.final_lists.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId).id;
                    m_efDbModel.FIPopulateCampaignListCompaniesAndContacts(SubCampaignId, _final_list_id, UserSession.CurrentUser.UserId);
                    m_lsmsCampaignListData = new LinqServerModeSource() {
                        DefaultSorting = "id",
                        KeyExpression = "uid",
                        QueryableSource = m_efDbModel.vw_campaign_list_companies_and_contacts.Where(i =>
                            i.final_list_id == _final_list_id &&
                            i.user_id == UserSession.CurrentUser.UserId
                        ).AsQueryable()
                        //QueryableSource = m_efDbModel.FIGetVwCampaignListCompaniesAndContacts(_final_list_id, UserSession.CurrentUser.UserId).ToList().AsQueryable()
                    };
                    m_lsmsCampaignListData.ExceptionThrown += new LinqServerModeExceptionThrownEventHandler(_lsmsCampaignListData_ExceptionThrown);
                    m_lsmsCampaignListData.InconsistencyDetected += new LinqServerModeInconsistencyDetectedEventHandler(_lsmsCampaignListData_InconsistencyDetected);

                    //gcCampaignList.DataSource = m_efDbModel.FIGetVwCampaignListCompaniesAndContacts(_final_list_id, UserSession.CurrentUser.UserId);

                    /** /
                    m_lsmsCampaignListData = new LinqServerModeSource()
                    {
                        DefaultSorting = "id",
                        KeyExpression = "uid",
                        QueryableSource = m_efDbModel.FIGetVwCampaignListCompaniesAndContacts(_final_list_id, UserSession.CurrentUser.UserId).ToList().AsQueryable()
                    };
                    m_lsmsCampaignListData.ExceptionThrown += new LinqServerModeExceptionThrownEventHandler(_lsmsCampaignListData_ExceptionThrown);
                    m_lsmsCampaignListData.InconsistencyDetected += new LinqServerModeInconsistencyDetectedEventHandler(_lsmsCampaignListData_InconsistencyDetected);
                    /**/

                    gcCampaignList.DataSource = m_lsmsCampaignListData;
                    gvCampaignList.Columns["uid"].Visible = false;
                    gvCampaignList.Columns["user_id"].Visible = false;
                    gvCampaignList.Columns["id"].Visible = false;
                    gvCampaignList.Columns["account_id"].Visible = false;
                    gvCampaignList.Columns["locked"].Visible = false;
                    gvCampaignList.Columns["locked_by"].Visible = false;
                    gvCampaignList.Columns["locked_timestamp"].Visible = false;
                    gvCampaignList.Columns["final_list_id"].Visible = false;
                    gvCampaignList.Columns["account_latitude"].Visible = false;
                    gvCampaignList.Columns["account_longitude"].Visible = false;
                    gvCampaignList.Columns["contact_id"].Visible = false;
                    gvCampaignList.Columns["account_address"].Visible = false;
                    gvCampaignList.Columns["contact_latitude"].Visible = false;
                    gvCampaignList.Columns["contact_longitude"].Visible = false;
                    gvCampaignList.Columns["contact_address"].Visible = false;
                    gvCampaignList.Columns["contact_status"].Caption = "Dialog Status";
                    gvCampaignList.Columns["contact_status"].VisibleIndex = 7;
                }

                //if (gvCampaignList.FocusedRowHandle == 0)
                //    gvCampaignList.FocusedRowHandle = GridControl.InvalidRowHandle;

                gvCampaignList.BestFitColumns();
                //this.SetDefaultSelectedCampaignListItem();
                this.InitializeCampaignListControls(true);
                lblTotalRows.Text = "Records: " + gvCampaignList.RowCount.ToString();
                if (pLoadType == eCampaignListLoadCallee.Refresh)
                    gvCampaignList.FocusedRowHandle = _FocusedRow;
                else if (pLoadType == eCampaignListLoadCallee.EventLogOnLoad) {
                    //if (m_objSubCampaignAppointmentParams == null)
                    //    this.SetCampaignListAppointmentParams();
                }
                else
                    this.SetCampaignListAppointmentParams();
                
                /**
                 * re-subscribe the event after loading data.
                 */
                this.SetGridChangeRowEvent(true);
                m_DoneLoadingCampaignList = true;
            }
            catch (Exception e) {
                if (e.InnerException != null && e.InnerException.Message != null)
                    NotificationDialog.Information("Bright Sales", e.InnerException.Message);
                else
                    NotificationDialog.Information("Bright Sales", e.Message);

                return;
            }

            #region Log
            DateTime logDate2 = DateTime.UtcNow;
            string completeLoadingTime = logDate2.Subtract(logDate1).ToString("ss") + ":" + logDate2.Subtract(logDate1).Milliseconds;

            var log = BrightSalesFacade.Logger;
            log.SetLogField(LoggingField.campaign_id, CampaignId.ToString());
            log.SetLogField(LoggingField.campaign_name, CampaignName);
            log.SetLogField(LoggingField.sub_campaign_id, SubCampaignId.ToString());
            log.SetLogField(LoggingField.sub_campaign_name, SubCampaignName);
            log.SetLogField(LoggingField.customer_id, CustomerId.ToString());
            log.SetLogField(LoggingField.customer_name, CustomerName);
            log.SetLogField(LoggingField.contact_id, null);
            log.SetLogField(LoggingField.contact_name, null);
            log.SetLogField(LoggingField.dialog_id, null);
            log.SetLogField(LoggingField.account_id, null);
            log.SetLogField(LoggingField.account_name, null);
            log.SetLogField(LoggingField.complete_loading_time, completeLoadingTime);
            log.SetLogField(LoggingField.nr_companies_campagin_list, gvCampaignList.RowCount.ToString());

            if (m_BrightSalesProperty.CampaignList.CampaignListMode == SelectionProperty.CampaignListMode.CompaniesOnly)
                log.SetLogField(LoggingField.sub_campagin_list_type, "company");
            else if (m_BrightSalesProperty.CampaignList.CampaignListMode == SelectionProperty.CampaignListMode.CampaniesAndContacts)
                log.SetLogField(LoggingField.sub_campagin_list_type, "company_and_contact");
            else
                log.SetLogField(LoggingField.sub_campagin_list_type, "none");
            
            log.SendInfo("load_sub_campaign", "loading sub campaign details");
            #endregion
        }
        private void _lsmsCampaignListData_ExceptionThrown(object sender, LinqServerModeExceptionThrownEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate {
                WaitDialog.Show("Reloading data ...");
                ((LinqServerModeSource)sender).Reload();
                if (m_AccountId == 0) {
                    this.GetCurrentCampaignListAccount();
                    m_BrightSalesProperty.CommonProperty.AccountId = m_AccountId;
                    m_BrightSalesProperty.CommonProperty.FinalListId = m_FinalListId;
                    //GridView view = gcCampaignList.FocusedView as GridView;
                    //int.TryParse(view.GetRowCellValue(view.FocusedRowHandle, "account_id").ToString(), out AccountId);
                    //int.TryParse(view.GetRowCellValue(view.FocusedRowHandle, "final_list_id").ToString(), out m_FinalListId);
                    //m_BrightSalesProperty.CommonProperty.AccountId = AccountId;
                    //m_BrightSalesProperty.CommonProperty.FinalListId = m_FinalListId;
                }
                if (!IsSuccessFocus)
                    this.CompanySetFocus(m_AccountId);
                WaitDialog.Close();
            }));
        }
        private void _lsmsCampaignListData_InconsistencyDetected(object sender, LinqServerModeInconsistencyDetectedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate {
                WaitDialog.Show("Reloading data ...");
                ((LinqServerModeSource)sender).Reload();
                if (!IsSuccessFocus)
                    CompanySetFocus(m_AccountId);
                WaitDialog.Close();
            }));
        }
        private void LoadCampaignListSelection()
        {
            //try
            //{
            //    m_DoneLoadingCampaignList = false;
            //    gcCampaignList.DataSource = null;
            //    //m_objCompanyInformationForm.ClearCompanyInformation(); //todo: to be handled
            //    this.InitializeCampaignListButtons(false);
            //    //this.InitializeContactButtonControls(false); //todo: to be handled
            //    //tgTabs.Enabled = false; //todo: top be handled. this is the to be campaign booking details module

            //    List<string> _empty_list = new List<string>();
            //    cboSubCampaignList.Properties.DataSource = null;
            //    cboSubCampaignList.Properties.Columns.Clear();
            //    cboSubCampaignList.Properties.DataSource = ObjectSubCampaign.GetActiveSubCampaigns(UserSession.CurrentUser.UserId, _empty_list);
            //    cboSubCampaignList.Properties.DisplayMember = "item";
            //    cboSubCampaignList.Properties.ValueMember = "id";
            //    cboSubCampaignList.Properties.Columns.Add(new LookUpColumnInfo("item"));
            //    m_DoneLoadingCampaignList = true;
            //}
            //catch { }
        }
        private void SetCampaignListAppointmentParams()
        {
            m_objSubCampaignAppointmentParams = null;
            m_objSubCampaignAppointmentParams = new SubCampaignAppointment(CustomerId, CampaignId, SubCampaignId);

            if (gcCampaignList == null)
                return;

            if (gvCampaignList.RowCount < 1)
                return;

            this.GetCurrentCampaignListAccount();
            m_objSubCampaignAppointmentParams.AccountId = m_AccountId;
            m_objSubCampaignAppointmentParams.FinalListId = m_FinalListId;
            m_objSubCampaignAppointmentParams.CompanyWebsite = m_CompanyWebsite;
            m_objSubCampaignAppointmentParams.CompanyBoardNumber = m_CompanyBoardNumber;
            m_objSubCampaignAppointmentParams.CompanyAppointmentStatus = m_CompanyAppointmentStatus;
            m_objSubCampaignAppointmentParams.CompanyAppointmentLeadStatus = m_CompanyAppointmentLeadStatus;

            //DAN: Added as when selecting company tab, it does not show any data for the current focused row.
            //m_BrightSalesProperty.CommonProperty.ContactId = m_ContactId;
            //m_BrightSalesProperty.CommonProperty.CustomerId = m_objSubCampaignAppointmentParams.CustomerId;
            //m_BrightSalesProperty.CommonProperty.CampaignId = m_objSubCampaignAppointmentParams.CampaignId;
            //m_BrightSalesProperty.CommonProperty.SubCampaignId = m_objSubCampaignAppointmentParams.SubCampaignId;
            //m_BrightSalesProperty.CommonProperty.AccountId = m_objSubCampaignAppointmentParams.AccountId;
            //m_BrightSalesProperty.CommonProperty.FinalListId = m_objSubCampaignAppointmentParams.FinalListId;
            //m_BrightSalesProperty.CommonProperty.CompanyName = m_CompanyName;
        }
        private void SetDefaultSelectedCampaignListItem()
        {
            if (m_CampaignListSelectedRow < 0)
                m_CampaignListSelectedRow = 0;
            else if (m_CampaignListSelectedRow < 1 && m_CampaignListSelectedRow <= gvCampaignList.RowCount)
                return;
            else if (gvCampaignList.FocusedRowHandle == m_CampaignListSelectedRow)
                return;

            gvCampaignList.FocusedRowHandle = m_CampaignListSelectedRow;
        }
        private void SaveCompanyAppointment()
        {
            if (m_CurrentCampaignListAccount == null)
                return;

            string _AccountStatus = string.Empty;
            string _AccountLeadStatus = null;
            int _AccountStatusNotQualifiedIndex = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatuses.FindIndex(i => i.not_qualified_default == true);
            int _AccountLeadStatusNotQualifiedIndex = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountLeadStatuses.FindIndex(i => i.not_qualified_default == true);
            //if (m_AccountStatusNotQualifiedIndex > -1)
            //    _AccountStatus = m_lstAccountStatuses[m_AccountStatusNotQualifiedIndex];
            //if (m_AccountLeadStatusNotQualifiedIndex > -1)
            //    _AccountLeadStatus = m_lstAccountLeadStatuses[m_AccountLeadStatusNotQualifiedIndex];
            if (_AccountStatusNotQualifiedIndex > -1)
                _AccountStatus = m_lstAccountStatuses[_AccountStatusNotQualifiedIndex].status;
            if (_AccountLeadStatusNotQualifiedIndex > -1)
                _AccountLeadStatus = m_lstAccountLeadStatuses[_AccountLeadStatusNotQualifiedIndex].status;

            ObjectSubCampaign.SaveSubCampaignCompanyAppointmentStatus(m_CurrentCampaignListAccount.account_id, m_CurrentCampaignListAccount.final_list_id, _AccountStatus, _AccountLeadStatus);
            //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Status", _AccountStatus);
            //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Lead_Status", _AccountLeadStatus);
            //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_Contact", DateTime.Today.ToShortDateString());
            //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company_Last_User", UserSession.CurrentUser.UserFullName);

            m_objSubCampaignAppointmentParams.CompanyAppointmentStatus = _AccountStatus;
            m_objSubCampaignAppointmentParams.CompanyAppointmentLeadStatus = _AccountLeadStatus;

            int _AcctId = 0;
            for (int i = 0; i < gvCampaignList.RowCount; i++) {                
                _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(i, "account_id"));
                if (_AcctId == m_CurrentCampaignListAccount.account_id) {
                    gvCampaignList.SetRowCellValue(i, "Company_Status", _AccountStatus);
                    gvCampaignList.SetRowCellValue(i, "Company_Last_Contact", DateTime.Today.ToShortDateString());
                    gvCampaignList.SetRowCellValue(i, "Company_Last_User", UserSession.CurrentUser.UserFullName);
                    gvCampaignList.SetRowCellValue(i, "Lead_Status", _AccountLeadStatus);
                }
            }

            this.BestFitGridColumns();
        }
        private void SetGridChangeRowEvent(bool pEventState)
        {
            if (pEventState)
                this.gvCampaignList.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvCampaignList_FocusedRowChanged);
            else
                this.gvCampaignList.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvCampaignList_FocusedRowChanged);
        }
        private void SetRowProperty()
        {
            m_BrightSalesProperty.CampaignList.IsFirstRow = false;
            m_BrightSalesProperty.CampaignList.IsLastRow = false;

            if (gvCampaignList.RowCount < 2) {
                m_BrightSalesProperty.CampaignList.IsFirstRow = true;
                m_BrightSalesProperty.CampaignList.IsLastRow = true;
            }
            else if (gvCampaignList.IsFirstRow)
                m_BrightSalesProperty.CampaignList.IsFirstRow = true;
            else if (gvCampaignList.IsLastRow)
                m_BrightSalesProperty.CampaignList.IsLastRow = true;
        }
        private DataTable ConvertToDataTable(LinqServerModeSource data)
        {

            //GridView view = new GridView();
            //view.DataSource = data;
            GridView gv = new GridView();
            GridControl gc = new GridControl();
            gc.MainView = gv;
            gc.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gv });

            gc.DataSource = data;
            gc.RefreshDataSource();


            DataTable dt = new DataTable();

            // add the columns to the datatable      
            GridView view = gc.FocusedView as GridView;

            for (int i = 0; i < view.Columns.Count; i++)
            {
                dt.Columns.Add(view.Columns[i].FieldName);
            }
            dt.Columns["Company_Last_Contact"].DataType = typeof(DateTime);
            dt.Columns["Company_Last_Contact"].AllowDBNull = true;

            for (int i = 0; i < view.DataRowCount; i++)
            {
                DataRow dr;
                dr = dt.NewRow();
                for (int x = 0; x < view.Columns.Count; x++)
                {
                    if (view.Columns[x].FieldName == "Company_Last_Contact")
                    {
                        try
                        {
                            if (view.GetRowCellValue(i, dt.Columns[x].ColumnName) != null)
                            {
                                dr[x] = Convert.ToDateTime(view.GetRowCellValue(i, dt.Columns[x].ColumnName));
                            }
                            else
                            {
                                dr[x] = DBNull.Value;
                            }
                        }
                        catch (Exception e)
                        {
                            m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                                ErrorMessage = e.Message
                            });
                            dr[x] = DBNull.Value;
                        }
                    }
                    else
                    {
                        dr[x] = view.GetRowCellValue(i, dt.Columns[x].ColumnName);
                    }
                }
                dt.Rows.Add(dr);

            }
            gc.Dispose();
            gc = null;


            return dt;
        }

        /** /
        private void GetFindFilterString(string pFilterText)
        {
            m_CampaignListFindFilter = string.Empty;
            if (string.IsNullOrEmpty(pFilterText))
                return;

            List<string> _FilterString = new List<string>();
            for (int i = 0; i < gvCampaignList.VisibleColumns.Count; i++) {
                if (CampaignListMode == eCampaignListMode.CompaniesOnly) {
                    if (gvCampaignList.VisibleColumns[i].FieldName.Equals("id") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("contact_status") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("account_id") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("locked") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("locked_by") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("locked_timestamp") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("final_list_id") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("account_latitude") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("account_longitude") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("contact_id") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("account_address"))
                        continue;

                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Employees_Total_1") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Employees_Total_2") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Employees_Total_3") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Turnover_1") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Turnover_2") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Turnover_3") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Export_1") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Export_2") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Export_3") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Result_1") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Result_2") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Result_3") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Sales_Aborad_1") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Sales_Aborad_2") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Sales_Aborad_3") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Created") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Modified") ||
                        //gvCampaignList.VisibleColumns[i].FieldName.Equals("Company_Validated"))                        
                }
                else if (CampaignListMode == eCampaignListMode.CompaniesAndContacts) {
                    if (gvCampaignList.VisibleColumns[i].FieldName.Equals("id") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("contact_status") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("account_id") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("locked") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("locked_by") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("locked_timestamp") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("final_list_id") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("account_latitude") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("account_longitude") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("contact_latitude") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("contact_longitude") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("contact_address") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("contact_id") ||
                        gvCampaignList.VisibleColumns[i].FieldName.Equals("account_address"))
                        continue;
                }
                
                _FilterString.Add(string.Format("Contains(Lower({0}),'{1}')",
                    gvCampaignList.VisibleColumns[i].FieldName,
                    pFilterText.ToLowerInvariant()
                ));
            }

            if (_FilterString.Count > 0)
                m_CampaignListFindFilter = string.Join(" Or ", _FilterString.ToArray());
        }
        private void GetCheckedFilters()
        {
            m_CampaignListCheckedFilter = string.Empty;
            List<string> _FilterString = new List<string>();
            if (cbxFilterMeLastUser.Checked)
                _FilterString.Add(string.Format("Contains(Lower([Company_Last_User]),Lower('{0}'))",
                    UserSession.CurrentUser.UserFullName
                ));

            if (cbxShowOnlyNonFinished.Checked)
                _FilterString.Add("[Company_Status] != 'Finished'");

            if (_FilterString.Count > 0)
                m_CampaignListCheckedFilter = string.Join(" And ", _FilterString.ToArray());
        }
        private void GetSpecificColumnFilters()
        {
            m_CampaignListSpecificColumnFilter = string.Empty;
            if (string.IsNullOrEmpty(m_OperatorArgs.Value.ToString()))
                return;

            try {
                DevExpress.Data.Filtering.GroupOperator _Operators = (DevExpress.Data.Filtering.GroupOperator)m_OperatorArgs.Value;
                for (int i = 0; i < _Operators.Operands.Count; i++) {
                    string _Operator = _Operators.Operands[i].ToString();
                    if (_Operator.Contains("=") ||
                        _Operator.Contains(">") ||
                        _Operator.Contains(">=") ||
                        _Operator.Contains("<") ||
                        _Operator.Contains("<=") ||
                        _Operator.Contains("<>") ||
                        _Operator.Contains("Between") ||
                        _Operator.Contains("Is Null") ||
                        _Operator.Contains("Is Not Null")) {
                            string _operand_value = ((DevExpress.Data.Filtering.BinaryOperator)(_Operators.Operands[i])).RightOperand.ToString();
                            //if (ValidationUtility.IsCurrency(_Operators.Operands[i].ToString()))
                            if (ValidationUtility.IsCurrency(_operand_value.Replace("'", "")))
                                continue;
                    }

                    string _value = _Operators.Operands[i].ToString().Replace("'", "&quot;");
                    _Operators.Operands[i] = _value.Replace("[", "Lower(").Replace("]", ")");
                }
                m_CampaignListSpecificColumnFilter = _Operators.ToString().Replace("'L", "L").Replace(";'", ";");
                m_CampaignListSpecificColumnFilter = m_CampaignListSpecificColumnFilter.Replace("'", "").Replace("&quot;", "'");
            }
            catch {
                string _Val = m_OperatorArgs.Value.ToString();
                if (_Val.Contains("=") ||
                    _Val.Contains(">") ||
                    _Val.Contains(">=") ||
                    _Val.Contains("<") ||
                    _Val.Contains("<=") ||
                    _Val.Contains("<>") ||
                    _Val.Contains("Between") ||
                    _Val.Contains("Is Null") ||
                    _Val.Contains("Is Not Null")) {
                    string _operand_value = ((DevExpress.Data.Filtering.BinaryOperator)(m_OperatorArgs.Value)).RightOperand.ToString();
                    if (!ValidationUtility.IsCurrency(_operand_value.Replace("'", ""))) {
                        string _temp = _Val.Replace("'", "&quot;");
                        _Val = _temp.Replace("[", "Lower(").Replace("]", ")");
                    }
                    else
                        m_CampaignListSpecificColumnFilter = _Val;
                }
                else {
                    DevExpress.Data.Filtering.CriteriaOperatorCollection _operators = ((DevExpress.Data.Filtering.FunctionOperator)(((DevExpress.Data.Filtering.CriteriaOperator)(m_OperatorArgs.Value)))).Operands;
                    string _operand_value = _operators[1].ToString();
                    if (!ValidationUtility.IsCurrency(_operand_value.Replace("'", ""))) {
                        string _temp = _Val.Replace("'", "&quot;");
                        _Val = _temp.Replace("[", "Lower(").Replace("]", ")");
                    }
                    m_CampaignListSpecificColumnFilter = _Val; //.Replace("[", "Lower(").Replace("]", ")");
                }

                m_CampaignListSpecificColumnFilter = _Val.Replace("'", "").Replace("&quot;", "'");
            }

            //if (string.IsNullOrEmpty(gvCampaignList.FilterPanelText)) {
            //    m_CampaignListSpecificColumnFilter = string.Empty;
            //    return;
            //}

            //string[] _ColumnFilter = Regex.Split(gvCampaignList.FilterPanelText, " And ");
            //for (int i = 0; i < _ColumnFilter.Count(); i++) {
            //    if (_ColumnFilter[i].Contains(">") ||
            //        _ColumnFilter[i].Contains(">=") ||
            //        _ColumnFilter[i].Contains("<") ||
            //        _ColumnFilter[i].Contains("<=") ||
            //        _ColumnFilter[i].Contains("Between") ||
            //        _ColumnFilter[i].Contains("Is Null") ||
            //        _ColumnFilter[i].Contains("Is Not Null") ||
            //        _ColumnFilter[i].Contains("In"))
            //            continue;
            //}

            //m_CampaignListSpecificColumnFilter = string.Join(" A ", _ColumnFilter.ToArray());


            //if (!string.IsNullOrEmpty(m_CampaignListSpecificColumnFilter))
            //    m_CampaignListSpecificColumnFilter = m_CampaignListSpecificColumnFilter.Replace("[", "Lower(").Replace("]", ")");
        }
        /**/

        private void WorkOnSelectedAccount()
        {
            if (gvCampaignList.RowCount < 1)
                return;

            try {
                this.ReleaseCurrentCompanyLock();
                bool islocked = m_CurrentCampaignListAccount.locked;
                int? lockedBy = m_CurrentCampaignListAccount.locked_by;

                bool _State = islocked && lockedBy != UserSession.CurrentUser.UserId ? false : true;
                btnWorkOnCompany.Enabled = _State;
                btnRemoveCompany.Enabled = _State;
                if ((islocked && lockedBy == UserSession.CurrentUser.UserId) || !islocked) {
                    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                        m_CurrentCampaignListAccount.locked = true;
                        m_CurrentCampaignListAccount.locked_by = UserSession.CurrentUser.UserId;
                        m_CurrentCampaignListAccount.locked_timestamp =_efDbContext.FIUpdateUserLock(
                            m_FinalListId,
                            m_AccountId,
                            m_CurrentCampaignListAccount.locked,
                            m_CurrentCampaignListAccount.locked_by
                        ).FirstOrDefault();
                    }

                    gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", m_CurrentCampaignListAccount.locked);
                    gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_by", m_CurrentCampaignListAccount.locked_by);
                    gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Locked_By_User", UserSession.CurrentUser.UserFullName);
                    gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_timestamp", m_CurrentCampaignListAccount.locked_timestamp);
                    this.LoadCampaignBooking(ObjectEventSender.btnWorkOnCompany_Click);
                }
                else {
                    gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", true);
                    NotificationDialog.Warning("Bright Sales", "The selected company is currently edited by another user. Please try again later.");
                    m_CurrentCampaignListAccount = null;
                }
            }
            catch (Exception e) {
                m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                    ErrorMessage = e.Message
                });
            }
        }
        #endregion

        #region Control Events
        #region Combo Boxes & Lookup Edits
        private void cboSubCampaignList_EditValueChanging(object sender, ChangingEventArgs e)
        {
            //if (UserOnWorkMode)
            //{
            //    string _messagge = "You're currently working on a company. Please close it first before selecting another sub-campaign.";
            //    MessageBox.Show(_messagge, "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    e.Cancel = true;
            //}
        }
        private void cboSubCampaignList_EditValueChanged(object sender, EventArgs e)
        {
            //if (cboSubCampaignList.Text.Length < 1)
            //    return;

            //WaitDialog.Show(this.ParentForm, "Loading components...");

            //string[] _ids = cboSubCampaignList.EditValue.ToString().Split(';');
            //string[] _customer = _ids[0].Split('|');
            //string[] _campaign = _ids[1].Split('|');
            //string[] _sub_campaign = _ids[2].Split('|');

            //CustomerId = Convert.ToInt32(_customer[0]);
            //CustomerName = _customer[1].ToString();
            //CampaignId = Convert.ToInt32(_campaign[0]);
            //CampaignName = _campaign[1].ToString();
            //SubCampaignId = Convert.ToInt32(_sub_campaign[0]);
            //SubCampaignName = _sub_campaign[1].ToString();

            //UserSession objUser = UserSession.CurrentUser;
            //UserSession.CurrentUser.IsCampaignOwner = ObjectUser.IsCampaignOwner(objUser.UserId, SubCampaignId);
            //UserSession.CurrentUser.IsSubCampaignManager = ObjectUser.IsSubCampaignManager(objUser.UserId, SubCampaignId);
            //UserSession.CurrentUser.IsSubCampaignSales = ObjectUser.IsSubCampaignSales(objUser.UserId, SubCampaignId);
            //cbxFilterMeLastUser.Checked = false;
            //cbxShowOnlyNonFinished.Checked = false;
            //m_DoneLoadingCampaignList = false;

            //CampaignListMode = eCampaignListMode.CompaniesOnly;
            //btnChangeView.Text = "Show Companies && Contacts";
            //m_ChangingCampaignList = true;
            //this.LoadCampaignList();
            //if (gvCampaignList.RowCount > 0) {
            //    this.SetCampaignListAppointmentParams();

            //    /**
            //     * get sub-campaign status lists
            //     */
            //    BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            //    string _XmlData = _efDbModel.subcampaigns.FirstOrDefault(i => i.id == SubCampaignId).xml_config_data;
            //    int _ItemSelectedIndex = 0;
            //    int _NotQualifiedIndex = -1;

            //    m_lstAccountLeadStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_lead_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex);
            //    m_AccountLeadStatusSelectedIndex = _ItemSelectedIndex;
            //    m_AccountLeadStatusNotQualifiedIndex = _NotQualifiedIndex;
            //    m_lstAccountStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/account/account_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex);
            //    m_AccountStatusSelectedIndex = _ItemSelectedIndex;
            //    m_AccountStatusNotQualifiedIndex = _NotQualifiedIndex;
            //    m_lstContactStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/contact/contact_status_dropdown", ref _ItemSelectedIndex, ref _NotQualifiedIndex);
            //    m_ContactStatusSelectedIndex = _ItemSelectedIndex;
            //    m_ContactStatusNotQualifiedIndex = _NotQualifiedIndex;

            //    /**
            //     * event raiser
            //     */
            //    if (cboSubCampaignList_OnEditValueChange != null) {
            //        CampaignListArgs _Args = new CampaignListArgs();
            //        _Args.ContactId = m_ContactId;
            //        _Args.CompanyName = m_CompanyName;
            //        _Args.CampaignBookingAppointment = m_objSubCampaignAppointmentParams;
            //        _Args.CampaignListMode = CampaignListMode;
            //        _Args.IsEmptyList = false;
            //        _Args.BreadCrumb = string.Format("{0}{1}{2} ({3})",
            //            string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
            //            string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
            //            string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
            //            ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
            //                m_FinalListId,
            //                m_AccountId
            //            )
            //        );
            //        //_Args.BreadCrumb =
            //        //    string.Format("{0} > {1} > {2} > {3}{4}{5}",
            //        //        CustomerName,
            //        //        CampaignName,
            //        //        SubCampaignName,
            //        //        string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
            //        //        string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
            //        //        string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry
            //        //    );
            //        cboSubCampaignList_OnEditValueChange(this, _Args);
            //    }
            //}
            //else {
            //    if (cboSubCampaignList_OnEditValueChange != null) {
            //        CampaignListArgs _Args = new CampaignListArgs();
            //        _Args.IsEmptyList = true;
            //        cboSubCampaignList_OnEditValueChange(this, _Args);
            //    }
            //}

            ////SetCompanyContactInformation() //todo:  must be loaded only if currently selected. this part should load the selected tab on campaign list details
            //m_ChangingCampaignList = false;

            ////todo: load this selected tab using exposed method from the campaign list booking details module later ...
            ////if (m_SelectedPageName.Equals(lcgMySalesScript.Name))
            ////    this.InitializeMySalesScriptView();
            ////else if (m_SelectedPageName.Equals(lcgGeoMapViewerCompany.Name))
            ////    this.LoadSelectedPage(lcgGeoMapViewerCompany.Name);
            ////else if (m_SelectedPageName.Equals(lcgBvSalesScript.Name))
            ////    this.LoadSelectedPage(lcgBvSalesScript.Name);

            //m_DoneLoadingCampaignList = true;
            //WaitDialog.Close() ;
        }
        #endregion

        #region Check Boxes
        private void cbxFilterMeLastUser_CheckedChanged(object sender, EventArgs e)
        {
            if (!m_DoneLoadingCampaignList)
                return;

            //WaitDialog.Show("Filtering data ...");
            //this.GetCheckedFilters();
            //this.ApplyGridFilter();
            //WaitDialog.Close();

            WaitDialog.Show("Updating campaign list...");
            if (m_DoneLoadingCampaignList)
                this.ApplyCheckedFilter();
            WaitDialog.Close();
        }
        private void cbxShowOnlyNonFinished_CheckedChanged(object sender, EventArgs e)
        {
            if (!m_DoneLoadingCampaignList)
                return;

            //WaitDialog.Show("Filtering data ...");
            //this.GetCheckedFilters();
            //this.ApplyGridFilter();
            //WaitDialog.Close();

            WaitDialog.Show("Updating campaign list...");
            if (m_DoneLoadingCampaignList)
                this.ApplyCheckedFilter();
            WaitDialog.Close();
        }
        #endregion

        #region Buttons
        private void btnRefreshSubCampaigns_Click(object sender, EventArgs e)
        {
            //if (UserOnWorkMode)
            //{
            //    MessageBox.Show("You are currently working on a company. Please kindly close it first before refreshing the dropdown list.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            //DialogResult _dlg = MessageBox.Show("Are you sure to refresh the dropdown list? This will close any subcampains you are working on.", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (_dlg == DialogResult.No)
            //    return;

            //WaitDialog.Show(this.ParentForm, "Refreshing list...");

            //if (btnRefreshSubCampaigns_OnClick != null)
            //    btnRefreshSubCampaigns_OnClick(this, new EventArgs());
            //WaitDialog.Close();
        }
        private void btnAddCompany_Click(object sender, EventArgs e)
        {
            //if (UserOnWorkMode) {
            //    MessageBox.Show("You are currently working on a company. Please kindly close it first before selecting new companies.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            WaitDialog.Show(ParentForm, "Loading components...");
            AddSubCampaignAccount _ucSubCampaignAccount = new AddSubCampaignAccount();
            _ucSubCampaignAccount.cmdAddToSubCampaign_OnClick += new AddSubCampaignAccount.cmdAddToSubCampaignOnClickHandler(_ucSubCampaignAccount_cmdAddToSubCampaign_OnClick);
            PopupDialog _dlgPopup = new PopupDialog();
            //_ucSubCampaignAccount.CampaignListModule = this;
            _dlgPopup.FormBorderStyle = FormBorderStyle.FixedSingle;
            _dlgPopup.MinimizeBox = false;
            _dlgPopup.MaximizeBox = false;
            _dlgPopup.StartPosition = FormStartPosition.CenterScreen;
            _dlgPopup.Text = "Add Companies";
            _dlgPopup.Controls.Add(_ucSubCampaignAccount);
            _dlgPopup.ClientSize = new Size(_ucSubCampaignAccount.Width + 2, _ucSubCampaignAccount.Height + 2);
            WaitDialog.Close();
            _dlgPopup.ShowDialog(this.ParentForm);
        }
        private void btnChangeView_Click(object sender, EventArgs e)
        {
            //if (UserOnWorkMode) {
            //    MessageBox.Show("You are currently working on a company. Please kindly close it first before refreshing the dropdown list.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            m_ChangingCampaignListMode = true;
            if (btnChangeView.Text.Equals("Show Companies && Contacts")) {
                WaitDialog.Show(this.ParentForm, "Loading components...");
                btnChangeView.Text = "Show Companies Only";
                m_BrightSalesProperty.CampaignList.CampaignListMode = SelectionProperty.CampaignListMode.CampaniesAndContacts;
                //m_BrightSalesProperty.CampaignList.CampaignListMode = eCampaignListMode.CompaniesAndContacts;
            }
            else {
                WaitDialog.Show(this.ParentForm, "Loading components...");
                btnChangeView.Text = "Show Companies && Contacts";
                m_BrightSalesProperty.CampaignList.CampaignListMode = SelectionProperty.CampaignListMode.CompaniesOnly;
                //CampaignListMode = eCampaignListMode.CompaniesOnly;
            }

            this.LoadCampaignList(eCampaignListLoadCallee.ChangeView);
            m_ChangingCampaignListMode = false;
            if (gvCampaignList.RowCount > 0 && btnChangeView_OnClick != null) {
                //this.SetCampaignListAppointmentParams();
                CampaignListArgs _Args = new CampaignListArgs();
                _Args.ContactId = m_ContactId;
                _Args.CompanyName = m_CompanyName;
                _Args.CampaignBookingAppointment = m_objSubCampaignAppointmentParams;
                //_Args.CampaignListMode = CampaignListMode;
                _Args.BreadCrumb = string.Format("{0}{1}{2} ({3})",
                    string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                    string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                    string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
                    ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                        m_FinalListId,
                        m_AccountId
                    )
                );
                //_Args.BreadCrumb =
                //    string.Format("{0} > {1} > {2} > {3}{4}{5}",
                //        CustomerName,
                //        CampaignName,
                //        SubCampaignName,
                //        string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                //        string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                //        string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry
                //    );
                btnChangeView_OnClick(this, _Args);
            }
            else if (gvCampaignList.RowCount < 1 && OnCampaignListEmpty != null)
                OnCampaignListEmpty();

            //this.LoadSelectedPage(m_SelectedPageName); //todo:  get this on campaign booking details
            //m_EventBus.Notify(new CampaignListChangedViewEventNotifier());
            WaitDialog.Close();
        }
        private void btnRefreshList_Click(object sender, EventArgs e)
        {
            //if (UserOnWorkMode)
            //{
            //    MessageBox.Show("You are currently working on a company. Please kindly close it first before refreshing the dropdown list.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            //if (!m_RefreshingCampaignList)
            //    WaitDialog.Show("Refreshing list.");
            WaitDialog.Show("Refreshing list.");
            //m_RefreshingCampaignList = true;
            this.LoadCampaignList(eCampaignListLoadCallee.Refresh);
            this.gvCampaignList_FocusedRowChanged(null, null);
            //gcCampaignList.RefreshDataSource();
            if (btnRefresh_OnClick != null) {
                //this.SetCampaignListAppointmentParams();
                CampaignListArgs _Args = new CampaignListArgs();
                _Args.ContactId = m_ContactId;
                _Args.CompanyName = m_CompanyName;
                _Args.CampaignBookingAppointment = m_objSubCampaignAppointmentParams;
                //_Args.CampaignListMode = CampaignListMode;
                _Args.BreadCrumb = string.Format("{0}{1}{2} ({3})",
                    string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                    string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                    string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
                    ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                        m_FinalListId,
                        m_AccountId
                    )
                );
                //_Args.BreadCrumb =
                //    string.Format("{0} > {1} > {2} > {3}{4}{5}",
                //        CustomerName,
                //        CampaignName,
                //        SubCampaignName,
                //        string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                //        string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                //        string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry
                //    );
                btnRefresh_OnClick(this, _Args);
            }

            //m_RefreshingCampaignList = false;
            WaitDialog.Close();
        }
        private void btnReleaseLock_Click(object sender, EventArgs e)
        {
            if (gcCampaignList == null || gvCampaignList.RowCount < 1)
                return;

            if (!btnReleaseLock_OnClick(this, new EventArgs())) {
                NotificationDialog.Warning("Bright Sales", "Company cannot be released. There is an ongoing call or there is a pending call log to be saved first.");
                return;
            }

            //this.GetSelectedCampaignListRowCommonlyUsedFields();
            bool _IsLockedByCurrentUser = false;
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_account_lists _efeAccount = _efDbModel.sub_campaign_account_lists.FirstOrDefault(p => p.account_id == m_AccountId && p.final_list_id == m_FinalListId);
                if (_efeAccount != null) {
                    _efDbModel.Detach(_efeAccount);
                    if (_efeAccount.locked && _efeAccount.locked_by == UserSession.CurrentUser.UserId)
                        _IsLockedByCurrentUser = true;
                }
            }

            if (_IsLockedByCurrentUser) {
                WaitDialog.Show("Refreshing list.");
                Business.ObjectLocking.ReleaseLock(m_FinalListId, m_AccountId);
                m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", false);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_by", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_user", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Locked_By_User", null);
                gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_timestamp", null);
                gvCampaignList.RefreshRow(gvCampaignList.FocusedRowHandle);
                //m_ReleaseButtonPressed = true;
                //this.btnRefreshList_Click(null, null);
                //m_ReleaseButtonPressed = false;
                WaitDialog.Close();
                NotificationDialog.Warning("Bright Sales", "Company lock released.");
                //this.SetDefaultSelectedCampaignListItem();
            }
            else
                NotificationDialog.Warning("Bright Sales", "You can only release a lock if you worked on that company.");
        }
        private void btnRemoveCompany_Click(object sender, EventArgs e)
        {
            if (gvCampaignList.RowCount < 1)
                return;

            if (HasPendingCallAndLog()) {
                NotificationDialog.Information("Bright Sales", "A call is in progress or there's a pending call log to be saved.");
                return;
            }

            //if (UserOnWorkMode) {
            //    MessageBox.Show("You are currently working on a company. Please kindly close it first before refreshing the dropdown list.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            //m_CampaignListParamsLoaded = false;
            this.GetSelectedCampaignListRowCommonlyUsedFields();
            //m_CampaignListParamsLoaded = true;
            //if (m_AccountLocked && m_AccountLockedBy != UserSession.CurrentUser.UserId)

            int _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
            int _FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_account_lists _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                    i.account_id == _AcctId &&
                    i.final_list_id == _FinalListId
                );
                if (_item != null)
                    _efDbContext.Detach(_item);

                if (_item.locked && _item.locked_by == UserSession.CurrentUser.UserId) {
                    DialogResult _dlg = MessageBox.Show(
                        string.Format("You're currently working on this company.{0}Are you sure to de-activate this company?", Environment.NewLine),
                        "Bright Sales",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );
                    if (_dlg == DialogResult.No)
                        return;
                }
                else if (_item.locked && _item.locked_by != UserSession.CurrentUser.UserId) {
                    user _user = _efDbContext.users.FirstOrDefault(i => i.id == _item.locked_by);
                    if (_user != null)
                        _efDbContext.Detach(_user);

                    NotificationDialog.Error("Bright Sales", string.Format("This company is currently worked by {0}.", _user.fullname));
                    return;
                }
            }

            //if (UserOnWorkMode) {
            //    if (m_AccountLockedBy != UserSession.CurrentUser.UserId)
            //        MessageBox.Show(String.Format("This company is currently worked by {0}.", m_AccountLockUser), "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //    else
            //        MessageBox.Show("You're currently working on a company. Please close it first before de-activating.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //    return;
            //}

            string Message = String.Format("This is a global deletion from all campaigns and sub-campaigns.{0}Are you sure you want to de-activate this company?", Environment.NewLine);
            DialogResult objDialog = MessageBox.Show(Message, "De-activate Company", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objDialog == DialogResult.No)
                return;

            m_DoneLoadingCampaignList = false;
            DeActivateAccount objDeActivateAccount = new DeActivateAccount() {
                AccountId = m_AccountId,
                FinalListId = m_FinalListId
            };
            objDeActivateAccount.AfterSave += new DeActivateAccount.AfterSaveEventHandler(objDeActivateAccount_AfterSave);

            PopupDialog objPopupDialog = new PopupDialog() {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "De-Activate Account"
            };

            objPopupDialog.Controls.Add(objDeActivateAccount);
            objPopupDialog.ClientSize = new Size(objDeActivateAccount.Width + 2, objDeActivateAccount.Height + 2);
            objPopupDialog.ShowDialog();

            //if (objPopupDialog.ShowDialog(this.ParentForm) == DialogResult.OK) {
            //    WaitDialog.Show(ParentForm, "Loading list.");
            //    var x = Business.ObjectLocking.ReleaseLock(m_FinalListId, m_AccountId);
            //    //gcCampaignList.RefreshDataSource();
            //    this.LoadCampaignList();
            //    //this.LoadSelectedPage(m_SelectedPageName); //todo:  get this on campaign list booking details module
            //    //m_objMainForm.m_CampaignBookingModule.LoadCampaignBookingData(true); // todo: call this later using form event handler on sales consultant form

            //    /**
            //     * event raiser
            //     */
            //    if (gvCampaignList.RowCount > 0 && btnRemoveCompany_OnClick != null) {
            //        this.SetCampaignListAppointmentParams();
            //        CampaignListArgs _Args = new CampaignListArgs();
            //        _Args.ContactId = m_ContactId;
            //        _Args.CompanyName = m_CompanyName;
            //        _Args.CampaignBookingAppointment = m_objSubCampaignAppointmentParams;
            //        _Args.CampaignListMode = CampaignListMode;
            //        _Args.BreadCrumb =
            //            string.Format("{0} > {1} > {2} > {3}{4}{5}",
            //                CustomerName,
            //                CampaignName,
            //                SubCampaignName,
            //                string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
            //                string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
            //                string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry
            //            );
            //        btnRemoveCompany_OnClick(this, _Args);
            //    }
            //    else if (gvCampaignList.RowCount < 1 && OnCampaignListEmpty != null)
            //        OnCampaignListEmpty();

            //    WaitDialog.Close() ;
            //}
            
        }
        private void btnWorkOnCompany_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading data ...");
            this.GetCurrentCampaignListAccount();
            this.WorkOnSelectedAccount();
            WaitDialog.Close(true);

            //if (gvCampaignList.RowCount < 1)
            //    return;

            ////if (UserOnWorkMode) {
            ////    MessageBox.Show("You are currently working on a company. Please kindly close it first before working another company.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ////    return;
            ////}

            //WaitDialog.Show("Loading data ...");
            //this.GetCurrentCampaignListAccount();
            //this.ReleaseCurrentCompanyLock();
            //var view = gvCampaignList;
            //try {
            //    //this.GetCurrentCampaignListAccount();
            //    bool islocked = m_CurrentCampaignListAccount.locked;
            //    int? lockedBy = m_CurrentCampaignListAccount.locked_by;
            //    if (islocked && lockedBy != UserSession.CurrentUser.UserId) {
            //        btnWorkOnCompany.Enabled = false;
            //        btnRemoveCompany.Enabled = false;
            //    }
            //    else {
            //        btnWorkOnCompany.Enabled = true;
            //        btnRemoveCompany.Enabled = true;
            //    }

            //    if ((islocked && lockedBy == UserSession.CurrentUser.UserId) || !islocked) {
            //        var BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            //        m_CurrentCampaignListAccount.locked = true;
            //        m_CurrentCampaignListAccount.locked_by = UserSession.CurrentUser.UserId;
            //        m_CurrentCampaignListAccount.locked_timestamp =
            //            BPContext.FIUpdateUserLock(
            //                m_FinalListId,
            //                m_AccountId,
            //                m_CurrentCampaignListAccount.locked,
            //                m_CurrentCampaignListAccount.locked_by).FirstOrDefault();

            //        view.SetRowCellValue(view.FocusedRowHandle, "locked", m_CurrentCampaignListAccount.locked);
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked_by", m_CurrentCampaignListAccount.locked_by);
            //        view.SetRowCellValue(view.FocusedRowHandle, "Locked_By_User", UserSession.CurrentUser.UserFullName);
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked_timestamp", m_CurrentCampaignListAccount.locked_timestamp);
            //        this.LoadCampaignBooking(ObjectEventSender.btnWorkOnCompany_Click);
            //    }
            //    else
            //    {
            //        view.SetRowCellValue(view.FocusedRowHandle, "locked", true);
            //        NotificationDialog.Warning("Bright Sales", "The selected company is currently edited by another user. Please try again later.");
            //        m_CurrentCampaignListAccount = null;
            //    }
            //}
            //catch { 
            //}
            //WaitDialog.Close(true);
        }
        private void btnSaveAsNotQualified_Click(object sender, EventArgs e)
        {
            if (gvCampaignList.RowCount < 1)
                return;

            //if (UserOnWorkMode) {
            //    MessageBox.Show("You are currently working on a company. Please kindly close it first.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            int _AcctId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "account_id").ToString());
            int _FinalListId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(m_CampaignListSelectedRow, "final_list_id").ToString());
            BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            sub_campaign_account_lists _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                i.account_id == _AcctId &&
                i.final_list_id == _FinalListId
            );
            DialogResult _dlg;
            if (_item.locked && _item.locked_by == UserSession.CurrentUser.UserId) {
                _dlg = MessageBox.Show(
                    string.Format("You're currently working on this company.{0}Are you sure to save this company as Not Qualified?", Environment.NewLine),
                    "Bright Sales",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (_dlg == DialogResult.No)
                    return;
            }
            else if (_item.locked && _item.locked_by != UserSession.CurrentUser.UserId) {
                user _user = _efDbContext.users.FirstOrDefault(i => i.id == _item.locked_by);
                NotificationDialog.Error("Bright Sales", string.Format("This company is currently worked by {0}.", _user.fullname));
                return;
            }

            _dlg = MessageBox.Show("Are you sure to save this company as NOT QUALIFIED?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlg == DialogResult.No)
                return;

            WaitDialog.Show(ParentForm, "Saving...");
            this.GetCurrentCampaignListAccount();
            this.SaveCompanyAppointment();
            this.ReleaseCurrentCompanyLock();
            //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company Status", m_lstAccountStatuses[m_AccountStatusNotQualifiedIndex]);
            //if (m_AccountLeadStatusNotQualifiedIndex > -1)
            //    gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Company Lead Status", m_lstAccountLeadStatuses[m_AccountLeadStatusNotQualifiedIndex]);
            if (btnSaveAsNotQualified_OnClick != null) {
                this.SetCampaignListAppointmentParams();
                CampaignListArgs _Args = new CampaignListArgs();
                _Args.ContactId = m_ContactId;
                _Args.CompanyName = m_CompanyName;
                _Args.CampaignBookingAppointment = m_objSubCampaignAppointmentParams;
                //_Args.CampaignListMode = CampaignListMode;
                _Args.BreadCrumb = string.Format("{0}{1}{2} ({3})",
                    string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                    string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                    string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
                    ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                        m_FinalListId,
                        m_AccountId
                    )
                );
                //_Args.BreadCrumb =
                //    string.Format("{0} > {1} > {2} > {3}{4}{5}",
                //        CustomerName,
                //        CampaignName,
                //        SubCampaignName,
                //        string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                //        string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                //        string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry
                //    );
                btnSaveAsNotQualified_OnClick(this, _Args);
            }
            WaitDialog.Close();
        }
        //private void btnFind_Click(object sender, EventArgs e)
        //{
        //    WaitDialog.Show("Filtering data ...");
        //    this.GetFindFilterString(tbxCampaignListFind.Text);
        //    this.ApplyGridFilter();
        //    WaitDialog.Close();
        //}
        //private void btnCampaignListClearFind_Click(object sender, EventArgs e)
        //{
        //    WaitDialog.Show("Filtering data ...");
        //    tbxCampaignListFind.Text = string.Empty;
        //    m_CampaignListFindFilter = string.Empty;
        //    this.ApplyGridFilter();
        //    WaitDialog.Close();
        //}
        //private void btnCampaignListClearColumnFilter_Click(object sender, EventArgs e)
        //{
        //    WaitDialog.Show("Filtering data ...");
        //    //gvCampaignList.ClearColumnsFilter();
        //    m_CampaignListSpecificColumnFilter = string.Empty;
        //    this.ApplyGridFilter();
        //    WaitDialog.Close();
        //}
        //private void tbxCampaignListFind_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode != Keys.Enter || string.IsNullOrEmpty(tbxCampaignListFind.Text))
        //        return;

        //    WaitDialog.Show("Filtering data ...");
        //    this.GetFindFilterString(tbxCampaignListFind.Text);
        //    this.ApplyGridFilter();
        //    WaitDialog.Close();
        //}
        #endregion

        #region Campaign List Grid View
        private void gvCampaignList_FilterEditorCreated(object sender, DevExpress.XtraGrid.Views.Base.FilterControlEventArgs e)
        {
            e.FilterControl.PopupMenuShowing += FilterControl_PopupMenuShowing;
        }
        private void FilterControl_PopupMenuShowing(object sender, DevExpress.XtraEditors.Filtering.PopupMenuShowingEventArgs e)
        {
            for (int i = e.Menu.Items.Count - 1; i >= 0; i--) {
                if (e.Menu.Items[i].Caption == Localizer.Active.GetLocalizedString(StringId.FilterClauseLike) || 
                    e.Menu.Items[i].Caption == Localizer.Active.GetLocalizedString(StringId.FilterClauseNotLike))
                    e.Menu.Items.RemoveAt(i);
            }
        }
        private void gvCampaignList_ColumnFilterChanged(object sender, EventArgs e)
        {
            /**
             * will force the grid to fire focused row change when the
             * current position is 0.
             * when omitted, some needed focused row change logic will not
             * be fired.
             */
            gvCampaignList.FocusedRowHandle = DevExpress.XtraGrid.GridControl.InvalidRowHandle;

            if (gvCampaignList.RowCount > 0) {
                btnRemoveCompany.Enabled = true;
                btnWorkOnCompany.Enabled = true;
                lblTotalRows.Text = "Records: " + gvCampaignList.RowCount.ToString();
            }
            else {
                btnRemoveCompany.Enabled = false;
                btnWorkOnCompany.Enabled = false;
                lblTotalRows.Text = "Records: 0";
            }

            //if (m_ChangingCampaignListFocusedRow) {
            //    m_ChangingCampaignListFocusedRow = false;
            //    return;
            //}

            this.SetDefaultSelectedCampaignListItem();

            /**
             * event raiser
             */
            if (gvCampaignList.RowCount > 0 && gvCampaignList_OnColumnFilterChange != null) {
                CampaignListArgs _Args = new CampaignListArgs();
                _Args.ContactId = m_ContactId;
                _Args.CompanyName = m_CompanyName;
                _Args.CampaignBookingAppointment = m_objSubCampaignAppointmentParams;
                //_Args.CampaignListMode = CampaignListMode;
                _Args.BreadCrumb = string.Format("{0}{1}{2} ({3})",
                    string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                    string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                    string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
                    ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                        m_FinalListId,
                        m_AccountId
                    )
                );
                //_Args.BreadCrumb =
                //    string.Format("{0} > {1} > {2} > {3}{4}{5}",
                //        CustomerName,
                //        CampaignName,
                //        SubCampaignName,
                //        string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                //        string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                //        string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry
                //    );
                gvCampaignList_OnColumnFilterChange(this, _Args);
            }
            else if (gvCampaignList.RowCount < 1 && OnCampaignListEmpty != null)
                OnCampaignListEmpty();
        }
        private void gvCampaignList_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.Equals("Company_Zipcode")) {
                if (e.Value == null)
                    e.DisplayText = "0";
                else if (string.IsNullOrEmpty(e.Value.ToString()))
                    e.DisplayText = "0";
            }

            if (e.Column.FieldName.Equals("Company_Last_Contact")) {
                try {
                    if (e.Value == null)
                        e.DisplayText = string.Empty;
                    else if (!string.IsNullOrEmpty(e.Value.ToString()))
                        e.DisplayText = string.Format("{0:yyy-MM-dd}", Convert.ToDateTime(e.Value)); //e.Value.ToString("yyyy-MM-dd");
                }
                catch (Exception ex) {
                    m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                        ErrorMessage = ex.Message
                    });
                    e.DisplayText = string.Empty;
                }

                //if (e.Value != null) {
                //    DateTime dt = DateTime.MinValue;
                //    if (DateTime.TryParse(e.Value.ToString(), out dt))
                //        e.DisplayText = dt.ToString("yyyy-MM-dd");
                //}
            }
        }
        private void gvCampaignList_DataSourceChanged(object sender, EventArgs e)
        {
            //this.SetRowProperty();
            //if (gvCampaignList.RowCount > 0 && gvCampaignList_OnDataSourceChange != null) {
            //    this.SetCampaignListAppointmentParams();
            //    CampaignListArgs _Args = new CampaignListArgs();
            //    _Args.ContactId = m_ContactId == 0 ? -1 : m_ContactId;
            //    _Args.CompanyName = m_CompanyName;
            //    _Args.CampaignBookingAppointment = m_objSubCampaignAppointmentParams;
            //    //_Args.CampaignListMode = CampaignListMode;
            //    _Args.BreadCrumb = string.Format("{0}{1}{2} ({3})",
            //        string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
            //        string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
            //        string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
            //        ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
            //            m_FinalListId,
            //            m_AccountId
            //        )
            //    );
            //    //_Args.BreadCrumb =
            //    //    string.Format("{0} > {1} > {2} > {3}{4}{5}",
            //    //        CustomerName,
            //    //        CampaignName,
            //    //        SubCampaignName,
            //    //        string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
            //    //        string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
            //    //        string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry
            //    //    );
            //    gvCampaignList_OnDataSourceChange(this, _Args);
            //}
            //else 

            /*
             * https://brightvision.jira.com/browse/PLATFORM-2966
             * DAN: fix for the third scenario where in even if the datasource has been changed, the focus still does not back to first.
             */
            if (gvCampaignList.RowCount > 0) gvCampaignList.MoveFirst();

            this.SetRowProperty();
            if (gvCampaignList.RowCount < 1 && OnCampaignListEmpty != null)
                OnCampaignListEmpty();
        }
        private void gvCampaignList_DoubleClick(object sender, EventArgs e)
        {
            if (!m_CompanyWorkable) {
                NotificationDialog.Warning("Bright Sales", "The selected company is currently edited by another user. Please try again later.");
                return;
            }

            GridHitInfo _hitInfo = gvCampaignList.CalcHitInfo((e as MouseEventArgs).Location);
            if (!_hitInfo.InRow)
                return;

            WaitDialog.Show("Loading data ...");
            this.GetCurrentCampaignListAccount();
            this.WorkOnSelectedAccount();
            WaitDialog.Close(true);

            //this.SetRowProperty();
            //if (gvCampaignList.RowCount < 1)
            //    return;
            
            ////if (UserOnWorkMode) {
            ////    MessageBox.Show("You are currently working on a company. Please kindly close it first before working another company.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ////    return;
            ////}

            //WaitDialog.Show("Loading data ...");
            //GridHitInfo hitInfo = gvCampaignList.CalcHitInfo((e as MouseEventArgs).Location);
            //if (hitInfo.InRow) {
            //    this.GetCurrentCampaignListAccount();
            //    this.ReleaseCurrentCompanyLock();
            //    //m_CampaignListParamsLoaded = false;
            //    gvCampaignList.FocusedRowHandle = hitInfo.RowHandle;
            //    try {
            //        //this.GetCurrentCampaignListAccount();
            //        bool islocked = m_CurrentCampaignListAccount.locked;
            //        int? lockedBy = m_CurrentCampaignListAccount.locked_by;
            //        if (islocked && lockedBy != UserSession.CurrentUser.UserId) {
            //            btnWorkOnCompany.Enabled = false;
            //            btnRemoveCompany.Enabled = false;
            //        }
            //        else {
            //            btnWorkOnCompany.Enabled = true;
            //            btnRemoveCompany.Enabled = true;
            //        }

            //        if ((islocked && lockedBy == UserSession.CurrentUser.UserId) || !islocked) {
            //            var BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            //            m_CurrentCampaignListAccount.locked = true;
            //            m_CurrentCampaignListAccount.locked_by = UserSession.CurrentUser.UserId;
            //            m_CurrentCampaignListAccount.locked_timestamp =
            //                BPContext.FIUpdateUserLock(
            //                    m_FinalListId,
            //                    m_AccountId,
            //                    m_CurrentCampaignListAccount.locked,
            //                    m_CurrentCampaignListAccount.locked_by).FirstOrDefault();
            //            gvCampaignList.SetRowCellValue(hitInfo.RowHandle, "locked", m_CurrentCampaignListAccount.locked);
            //            gvCampaignList.SetRowCellValue(hitInfo.RowHandle, "locked_by", m_CurrentCampaignListAccount.locked_by);
            //            gvCampaignList.SetRowCellValue(hitInfo.RowHandle, "Locked_By_User", UserSession.CurrentUser.UserFullName);
            //            gvCampaignList.SetRowCellValue(hitInfo.RowHandle, "locked_timestamp", m_CurrentCampaignListAccount.locked_timestamp);
            //            this.LoadCampaignBooking(ObjectEventSender.gvCampaignList_DoubleClick);
            //        }
            //        else {
            //            gvCampaignList.SetRowCellValue(hitInfo.RowHandle, "locked", true);
            //            NotificationDialog.Warning("Bright Sales", "The selected company is currently edited by another user. Please try again later.");
            //            m_CurrentCampaignListAccount = null;
            //            //btnRefreshList_Click(null, null);
            //        }
            //    }
            //    catch { 
            //    }
            //    //m_CampaignListParamsLoaded = true;
            //}
            //WaitDialog.Close(true);
        }
        private void gvCampaignList_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            this.SetRowProperty();
            if (gvCampaignList.FocusedRowHandle == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;

            this.SetGridChangeRowEvent(false);
            m_CampaignListSelectedRow = gvCampaignList.FocusedRowHandle;

            //m_CampaignListSelectedRow = 0;
            //m_CampaignListSelectedRow = gvCampaignList.RowCount >= e.PrevFocusedRowHandle ? (gvCampaignList.RowCount > 0 ? gvCampaignList.RowCount - 1 : 0) : e.PrevFocusedRowHandle;

            //if (m_ChangingCampaignListFocusedRow)
            //    return;

            //m_ChangingCampaignListFocusedRow = true;
            
            //bool _isSameIndexes = false;

            //if (m_RefreshingCampaignList)
            //    m_CampaignListSelectedRow = e.PrevFocusedRowHandle;

            //else if (e.FocusedRowHandle == 0 && e.PrevFocusedRowHandle > 0) {
            //    if (m_AllowRowChange)
            //        m_CampaignListSelectedRow = 0;
            //    else
            //        m_CampaignListSelectedRow = gvCampaignList.RowCount >= e.PrevFocusedRowHandle ? (gvCampaignList.RowCount > 0 ? gvCampaignList.RowCount - 1 : 0) : e.PrevFocusedRowHandle;
            //}
            //else
            //{
            //    if (m_CampaignListSelectedRow == e.FocusedRowHandle)
            //        _isSameIndexes = true;
            //    m_CampaignListSelectedRow = e.FocusedRowHandle;
            //}

            //if (m_ChangingCampaignList)
            //    m_CampaignListSelectedRow = 0;

            //else {
            //    if (m_RefreshingCampaignList)
            //        m_CampaignListSelectedRow = e.PrevFocusedRowHandle;

            //    else if (e.FocusedRowHandle == 0 && e.PrevFocusedRowHandle > 0)
            //    {
            //        if (m_AllowRowChange)
            //            m_CampaignListSelectedRow = 0;
            //        else
            //            m_CampaignListSelectedRow = gvCampaignList.RowCount >= e.PrevFocusedRowHandle ? (gvCampaignList.RowCount > 0 ? gvCampaignList.RowCount - 1 : 0) : e.PrevFocusedRowHandle;
            //    }
            //    else
            //    {
            //        if (m_CampaignListSelectedRow == e.FocusedRowHandle)
            //            _isSameIndexes = true;
            //        m_CampaignListSelectedRow = e.FocusedRowHandle;
            //    }
            //}

            //this.GetSelectedCampaignListRowCommonlyUsedFields();
            //this.SetDefaultSelectedCampaignListItem();

            //---------
            //if (m_DoneLoadingCampaignList) {
            //    bool islocked = (bool)gvCampaignList.GetRowCellValue(gvCampaignList.FocusedRowHandle, "locked");
            //    int? lockedBy = ValidationUtility.TryParseInt(ValidationUtility.IFNullString(gvCampaignList.GetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_by"), "0"));
            //    if (islocked && lockedBy != UserSession.CurrentUser.UserId) {
            //        btnWorkOnCompany.Enabled = false;
            //        btnRemoveCompany.Enabled = false;
            //    }
            //}
            //else {
            //    if (gvCampaignList.RowCount > 0) {
            //        btnRemoveCompany.Enabled = true;
            //        btnWorkOnCompany.Enabled = true;
            //        lblTotalRows.Text = "Records: " + gvCampaignList.RowCount.ToString();
            //    }
            //    else {
            //        btnRemoveCompany.Enabled = false;
            //        btnWorkOnCompany.Enabled = false;
            //        lblTotalRows.Text = "Records: 0";
            //    }
            //}

            //-------------
            //this.CampaignListFocusedRowChange(sender, m_CampaignListSelectedRow);

            //if (gvCampaignList.RowCount > 0) {
            //    btnRemoveCompany.Enabled = true;
            //    btnWorkOnCompany.Enabled = true;
            //    lblTotalRows.Text = "Records: " + gvCampaignList.RowCount.ToString();
            //}
            //else {
            //    btnRemoveCompany.Enabled = false;
            //    btnWorkOnCompany.Enabled = false;
            //    lblTotalRows.Text = "Records: 0";
            //}

            //m_AllowRowChange = false;

            /**
             * m_ChangingCampaignList:
             * we will not fire the event in this case, since there will be another event that will
             * be raised when changing from sub-campaigns
             * 
             * m_RefreshingCampaignList:
             * if just refreshing the list, do not fire subscribed events
             */
            //if (m_ChangingCampaignList || m_RefreshingCampaignList)
            //if (m_RefreshingCampaignList)
            //{
            //    //m_ChangingCampaignListFocusedRow = false;
            //    m_CampaignListParamsLoaded = false;
            //    return;
            //}

            if (gvCampaignList.RowCount > 0 && gvCampaignList_OnFocusedRowChange != null) {
                this.SetCampaignListAppointmentParams();
                CampaignListArgs _Args = new CampaignListArgs();
                _Args.ContactId = m_ContactId == 0 ? -1 : m_ContactId;
                _Args.CompanyName = m_CompanyName;
                _Args.CampaignBookingAppointment = m_objSubCampaignAppointmentParams;
                _Args.BreadCrumb = string.Format("{0}{1}{2} ({3})",
                    string.IsNullOrEmpty(m_CompanyName) ? "" : m_CompanyName,
                    string.IsNullOrEmpty(m_CompanyCity) ? "" : ", " + m_CompanyCity,
                    string.IsNullOrEmpty(m_CompanyCountry) ? "" : ", " + m_CompanyCountry,
                    ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                        m_FinalListId,
                        m_AccountId
                    )
                );
                gvCampaignList_OnFocusedRowChange(this, _Args);
            }
            else if (gvCampaignList.RowCount < 1 && OnCampaignListEmpty != null)
                OnCampaignListEmpty();

            if (gvCampaignList.RowCount > 0) {
                btnRemoveCompany.Enabled = true;
                btnWorkOnCompany.Enabled = true;
                lblTotalRows.Text = "Records: " + gvCampaignList.RowCount.ToString();
                m_CompanyWorkable = true;
            }
            else {
                btnRemoveCompany.Enabled = false;
                btnWorkOnCompany.Enabled = false;
                lblTotalRows.Text = "Records: 0";
                m_CompanyWorkable = false;
            }

            if (m_DoneLoadingCampaignList) {
                try {
                    bool islocked = (bool)gvCampaignList.GetRowCellValue(gvCampaignList.FocusedRowHandle, "locked");
                    int? lockedBy = ValidationUtility.TryParseInt(ValidationUtility.IFNullString(gvCampaignList.GetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_by"), "0"));
                    if (islocked && lockedBy != UserSession.CurrentUser.UserId) {
                        btnWorkOnCompany.Enabled = false;
                        btnRemoveCompany.Enabled = false;
                        m_CompanyWorkable = false;
                    }
                    //else {
                    //    btnWorkOnCompany.Enabled = true;
                    //    btnRemoveCompany.Enabled = true;
                    //    m_CompanyWorkable = true;
                    //}
                }
                catch { 
                }
            }
            
            //m_ChangingCampaignListFocusedRow = false;
            //m_CampaignListParamsLoaded = false;
            this.SetGridChangeRowEvent(true);
        }
        private void gvCampaignList_KeyUp(object sender, KeyEventArgs e)
        {
            if (!m_CompanyWorkable) {
                NotificationDialog.Warning("Bright Sales", "The selected company is currently edited by another user. Please try again later.");
                return;
            }

            if (m_SalesConsultantCampaignListComboKeyPressEnter) {
                m_SalesConsultantCampaignListComboKeyPressEnter = false;
                return;
            }

            if (e.KeyCode != Keys.Enter || gvCampaignList.RowCount < 1)
                return;

            WaitDialog.Show("Loading data ...");
            this.GetCurrentCampaignListAccount();
            this.WorkOnSelectedAccount();
            WaitDialog.Close(true);

            //this.SetRowProperty();

            ////if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            ////    m_AllowRowChange = true;

            //if (e.KeyCode != Keys.Enter || gvCampaignList.RowCount < 1)
            //    return;

            //if (gvCampaignList.RowCount < 1)
            //    return;
            
            ////if (UserOnWorkMode)
            ////{
            ////    MessageBox.Show("You are currently working on a company. Please kindly close it first before working another company.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ////    return;
            ////}

            //WaitDialog.Show("Loading data ...");
            //try {
            //    this.GetCurrentCampaignListAccount();
            //    this.ReleaseCurrentCompanyLock();
            //    //this.GetCurrentCampaignListAccount();
            //    bool islocked = m_CurrentCampaignListAccount.locked;
            //    int? lockedBy = m_CurrentCampaignListAccount.locked_by;
            //    if (islocked && lockedBy != UserSession.CurrentUser.UserId)
            //    {
            //        btnWorkOnCompany.Enabled = false;
            //        btnRemoveCompany.Enabled = false;
            //    }
            //    else
            //    {
            //        btnWorkOnCompany.Enabled = true;
            //        btnRemoveCompany.Enabled = true;
            //    }

            //    if ((islocked && lockedBy == UserSession.CurrentUser.UserId) || !islocked)
            //    {
            //        BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            //        m_CurrentCampaignListAccount.locked = true;
            //        m_CurrentCampaignListAccount.locked_by = UserSession.CurrentUser.UserId;
            //        m_CurrentCampaignListAccount.locked_timestamp =
            //            BPContext.FIUpdateUserLock(
            //                m_FinalListId,
            //                m_AccountId,
            //                m_CurrentCampaignListAccount.locked,
            //                m_CurrentCampaignListAccount.locked_by).FirstOrDefault();
            //        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", m_CurrentCampaignListAccount.locked);
            //        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_by", m_CurrentCampaignListAccount.locked_by);
            //        //gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_user", UserSession.CurrentUser.UserFullName);
            //        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "Locked_By_User", UserSession.CurrentUser.UserFullName);
            //        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked_timestamp", m_CurrentCampaignListAccount.locked_timestamp);
            //        this.LoadCampaignBooking(ObjectEventSender.gvCampaignList_Enter);
            //    }
            //    else
            //    {
            //        gvCampaignList.SetRowCellValue(gvCampaignList.FocusedRowHandle, "locked", true);
            //        NotificationDialog.Warning("Bright Sales", "The selected company is currently edited by another user. Please try again later.");
            //        m_CurrentCampaignListAccount = null;
            //        //btnRefreshList_Click(null, null);
            //    }
            //}
            //catch { 
            //}
            //WaitDialog.Close(true);
        }
        //private void gvCampaignList_MouseDown(object sender, MouseEventArgs e)
        //{
        //    m_AllowRowChange = true;
        //}
        private void gvCampaignList_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            SalesConsultant.Business.BrightSalesGridUtility.CreateGridContextMenu(view, e);
        }
        private void gvCampaignList_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e == null || e.CellValue == null)
                return;

            if (IsCampaignListItemLocked(gvCampaignList, e.RowHandle)) {
                e.Appearance.BackColor = Color.LightGray;
                if (m_CampaignListLastWorkedAccountId == 0)
                    m_CampaignListLastWorkedAccountId = ValidationUtility.TryParseInt(gvCampaignList.GetRowCellValue(e.RowHandle, "account_id").ToString());
            }
            //if (!string.IsNullOrEmpty(tbxCampaignListFind.Text)) {
            //    if (e.CellValue.ToString().ToLowerInvariant().Contains(tbxCampaignListFind.Text.ToLowerInvariant()))
            //        e.Appearance.BackColor = Color.PaleGreen;
            //}
        }
        //private void gvCampaignList_FilterEditorCreated(object sender, DevExpress.XtraGrid.Views.Base.FilterControlEventArgs e)
        //{
        //    e.FilterBuilder.FormClosed += new FormClosedEventHandler(FilterBuilder_FormClosed);
        //}
        private void gvCampaignList_CustomFilterDisplayText(object sender, ConvertEditValueEventArgs e)
        {
            m_OperatorArgs = e;
            e.Handled = false;

            //DevExpress.Data.Filtering.BinaryOperator[] _Operators = e.Value as DevExpress.Data.Filtering.BinaryOperator;
            //for (int i = 0; i < e.)

            //DevExpress.Data.Filtering.CriteriaOperatorCollection _Operators = (DevExpress.Data.Filtering.GroupOperator)e.Value;

            //DevExpress.Data.Filtering.GroupOperator _Operators = (DevExpress.Data.Filtering.GroupOperator)e.Value;
            //for (int i = 0; i < _Operators.Operands.Count; i++) {
                
            //    //DevExpress.Data.Filtering.BinaryOperator _bo = (DevExpress.Data.Filtering.BinaryOperator)_Operators.Operands[i];
            //    string _Operator = _Operators.Operands[i].ToString();
            //    if (_Operator.Contains("=") || 
            //        _Operator.Contains(">") ||
            //        _Operator.Contains(">=") ||
            //        _Operator.Contains("<") ||
            //        _Operator.Contains("<=") ||
            //        _Operator.Contains("<>") ||
            //        _Operator.Contains("Between") ||
            //        _Operator.Contains("Is Null") ||
            //        _Operator.Contains("Is Not Null"))
            //            continue;

            //    _Operators.Operands[i] = _Operators.Operands[i].ToString().Replace("[", "Lower(").Replace("]", ")");
                
                //if (_bo.OperatorType == DevExpress.Data.Filtering.BinaryOperatorType.Equal ||
                //    _bo.OperatorType == DevExpress.Data.Filtering.BinaryOperatorType.Greater ||
                //    _bo.OperatorType == DevExpress.Data.Filtering.BinaryOperatorType.GreaterOrEqual ||
                //    _bo.OperatorType == DevExpress.Data.Filtering.BinaryOperatorType.Less ||
                //    _bo.OperatorType == DevExpress.Data.Filtering.BinaryOperatorType.LessOrEqual ||
                //    _bo.OperatorType == DevExpress.Data.Filtering.BinaryOperatorType.NotEqual)
                //        continue;

                //DevExpress.Data.Filtering.CriteriaOperator _co = _bo.LeftOperand;
                //_bo.LeftOperand = _co.ToString().Replace("[", "Lower(").Replace("]", ")");
                //_Operators.Operands[i] = _bo.LeftOperand.ToString().Replace("[", "Lower(").Replace("]", ")");
                //_bo.LeftOperand = DevExpress.Data.Filtering.CriteriaOperator.;
            //}

            //m_CampaignListSpecificColumnFilter = _Operators.ToString();
            //e.Value = _Operators.ToString();
            //e.Handled = true;            
        }
        private void gvCampaignList_EndSorting(object sender, EventArgs e)
        {
            gvCampaignList_FocusedRowChanged(null, null);
        }
        #endregion

        #region Filter Builder
        private void FilterBuilder_FormClosed(object sender, FormClosedEventArgs e)
        {
            //WaitDialog.Show("Filtering data ...");
            //this.GetSpecificColumnFilters();
            //this.ApplyGridFilter();
            //WaitDialog.Close();
        }
        #endregion
        #endregion
    }
}
