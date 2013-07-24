
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;

using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Threading;
using BrightVision.EventLog;
using BrightVision.Model;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

using SalesConsultant.Business;
using SalesConsultant.Modules;
using SalesConsultant.Utils;
using BrightVision.FileManagement;
using BrightVision.Telephony.Business;
using BrightVision.Logging.Enums;
using SalesConsultant.Events;
using BrightVision.Common.Events.Core;
using BrightVision.Common.UI;
using SalesConsultant.PublicProperties;
using System.Reactive.Linq;
using SalesConsultant.Facade;

namespace SalesConsultant.Forms 
{
    public partial class FrmSalesConsultant : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        #region Constructors
        public FrmSalesConsultant()
        {           
            InitializeComponent();

            #region Background Thread for Event Logging
            nextRefreshTime = DateTime.Now;
            refreshInterval = TimeSpan.FromSeconds(0.20);

            stats = new int[6];

            work = new WorkQueue();
            work.ConcurrentLimit = 100;
            work.AllWorkCompleted += new EventHandler(work_AllWorkCompleted);
            work.WorkerException += new ResourceExceptionEventHandler(work_WorkerException);
            work.ChangedWorkItemState += new ChangedWorkItemStateEventHandler(work_ChangedWorkItemState);

            minThreads = ((WorkThreadPool)work.WorkerPool).MinThreads;
            maxThreads = ((WorkThreadPool)work.WorkerPool).MaxThreads;
            concurrentLimit = work.ConcurrentLimit;
            #endregion
            #region Toggle Bar Menu
            m_oCallViewBar = new CallViewBar() {
                Dock = DockStyle.Fill
            };

            m_oCallLogBar = new CallLogBar() {
                Dock = DockStyle.Fill,
                Visible = false
            };

            pnlToggleBar.Controls.Clear();
            pnlToggleBar.Controls.AddRange(new Control[] { m_oCallViewBar, m_oCallLogBar });
            #endregion
            #region Follow Up Popup Window
            m_oFollowUp = new FollowUpEditor() {
                Dock = DockStyle.Fill
            };
            m_oFollowUp.btnSave_OnClick += new FollowUpEditor.btnSaveOnClickEventHandler(m_oFollowUp_btnSave_OnClick);
            m_oFollowUp.GetListSource += new FollowUpEditor.GetListSourceEventHandler(m_oFollowUp_GetListSource);
            m_oFollowUpDialog = new PopupDialog() {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Follow Up Window",
                ClientSize = new Size(m_oFollowUp.Width + 2, m_oFollowUp.Height + 2),
                CloseBox = false
            };
            m_oFollowUpDialog.Controls.Add(m_oFollowUp);
            //m_oFollowUpDialog.FormClosing += new FormClosingEventHandler(m_oFollowUpDialog_FormClosing);
            #endregion
            #region Call & Follow Up Toggle Bar
            m_oFollowUpBar = new FollowUpBar() {
                Dock = DockStyle.Fill
            };
            //m_oFollowUpBar.btnTop_OnClick += new FollowUpBar.btnTopOnClickEventHandler(m_oFollowUpBar_btnTop_OnClick);
            //m_oFollowUpBar.btnPrevious_OnClick += new FollowUpBar.btnPreviousOnClickEventHandler(m_oFollowUpBar_btnPrevious_OnClick);
            //m_oFollowUpBar.btnNext_OnClick += new FollowUpBar.btnNextOnClickEventHandler(m_oFollowUpBar_btnNext_OnClick);
            //m_oFollowUpBar.btnLoad_OnClick += new FollowUpBar.btnLoadOnClickEventHandler(m_oFollowUpBar_btnLoad_OnClick);
            //m_oFollowUpBar.GetCampaignBookingContactList += new FollowUpBar.GetCampaignBookingContactListEventHandler(m_oFollowUpBar_GetCampaignBookingContactList);
            //m_oFollowUpBar.GetCampaignBookingArgs += new FollowUpBar.GetCampaignBookingArgsEventHandler(m_oFollowUpBar_GetCampaignBookingArgs);
            //m_oFollowUpBar.HasBrowsableData += new FollowUpBar.HasBrowsableDataEventHandler(m_oFollowUpBar_HasBrowsableData);
            //m_oFollowUpBar.HasPendingCallAndLog += new FollowUpBar.HasPendingCallAndLogEventHandler(m_oFollowUpBar_HasPendingCallAndLog);
            //m_oFollowUpBar.btnSave_OnClick += new FollowUpBar.btnSaveOnClickEventHandler(m_oFollowUpBar_btnSave_OnClick);
            //m_oFollowUpBar.LoadFollowUps += new FollowUpBar.LoadFollowUpsEventHandler(m_oFollowUpBar_LoadFollowUps);
            //m_oFollowUpBar.CanWorkOnCompany += new FollowUpBar.CanWorkOnCompanyEventHandler(m_oFollowUpBar_CanWorkOnCompany); //+= new FollowUpBar.DialogOnEditModeEventHandler(m_oFollowUpBar_DialogOnEditMode);

            m_oCallLogRemarksBar = new CallLogRemarks() {
                Dock = DockStyle.Fill,
                Visible = false
            };
            m_oCallLogRemarksBar.btnSaveCallLog_OnClick += new CallLogRemarks.btnSaveCallLogOnClickEventHandler(m_oCallLogRemarksBar_btnSaveCallLog_OnClick);
            m_oCallLogRemarksBar.EndCall_Initiated += new CallLogRemarks.EndCallInitiatedEventHandler(m_oCallLogRemarksBar_EndCall_Initiated);
            m_oCallLogRemarksBar.GetContactPerson += new CallLogRemarks.GetContactPersonEventHandler(m_oCallLogRemarksBar_GetContactPerson);
            m_oCallLogRemarksBar.UserOnCall += new CallLogRemarks.UserOnCallEventHandler(m_oCallLogRemarksBar_UserOnCall);
            m_oCallLogRemarksBar.UserOnCallForceStop += new CallLogRemarks.UserOnCallForceStopEventHandler(m_oCallLogRemarksBar_UserOnCallForceStop);
            pnlCallLogAndFollowUp.Controls.Clear();
            pnlCallLogAndFollowUp.Controls.AddRange(new Control[] { 
                m_oFollowUpBar, 
                m_oCallLogRemarksBar 
            });
            #endregion
            #region Help Information Form
            m_HelpInfo.Shown += new EventHandler(m_HelpInfo_Shown);
            m_HelpInfo.FormClosed += new FormClosedEventHandler(m_HelpInfo_FormClosed);
            #endregion

            this.RegisterEvents();
            this.SetStateCallerBarGroup(false);

            tcSalesConsultant.CustomHeaderButtons[0].Enabled = false;
            tcSalesConsultant.CustomHeaderButtons[0].Visible = false;

            /**
             * https://brightvision.jira.com/browse/PLATFORM-3141
             * disable the mouse wheel scrolling.
             */
            cboCampaignList.MouseWheel += cboCampaignList_MouseWheel;
        }
        #endregion

        #region Public Properties
        public bool DoneLoggedIn = false;
        public enum TabPages {
            CampaignList,
            CampaignBooking,
            MyFollowUps,
            Reports,
            Help,
            MillTime,
            None
        }

        public bool CallLogBar_IsCallLogSaved {
            get { return m_BrightSalesProperty.CommonProperty.CallLogSaved; }
        }

        public CallLogArgs CallLogBarParams = null;

        public class StatusArgs {
            public int CompanyId { get; set; }
            public DateTime CompanyLastContact { get; set; }
            public string CompanyLastUser { get; set; }
            public string CompanyStatus { get; set; }
            public string CompanyLeadStatus { get; set; }
        }
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        private UserLogin m_UserLogin = null;
        private ManageCampaignBooking m_CampaignBookingModule = null;
        private ManageCampaignList m_CampaignListModule = null;
        private MyFollowUps m_MyFollowUpModule = null;
        private ViewDisplay m_ReportModule = null;
        private ToolStripMenuItem m_tsmiLogOut = null;
        private ToolStripMenuItem m_tsmiExit = null;
        private bool m_LogoutInitiated = false;
        private bool m_RefreshSubCampaignList = false;
        private WebDavFileManager fileUploadManager = null;
        #region Background Thread for Event Logging
        private int[] stats;
        private TimeSpan refreshInterval;
        private DateTime nextRefreshTime;
        private WorkQueue work;
        private int minThreads, maxThreads, concurrentLimit;
        private bool pausing = false, resuming = false;
        private int newWork = 10; //sample of work load
        #endregion

        private enum ToggleBarMenuType
        {
            CallView,
            CallLog
        }
        private ToggleBarMenuType m_SelectedToggleBarMenu = ToggleBarMenuType.CallView;
        private CallViewBar m_oCallViewBar = null;
        private CallLogBar m_oCallLogBar = null;

        private FollowUpBar m_oFollowUpBar = null;
        private CallLogRemarks m_oCallLogRemarksBar = null;

        private FollowUpEditor m_oFollowUp = null;
        private PopupDialog m_oFollowUpDialog = null;

        private int m_CustomerId = 0;
        private int m_CampaignId = 0;
        private int m_SubCampaignId = 0;
        private string m_CustomerName = string.Empty;
        private string m_CampaignName = string.Empty;
        private string m_SubCampaignName = string.Empty;

        private TimeSpan m_StartTime = new TimeSpan();
        private TimeSpan m_EndTime = new TimeSpan();
        private TimeSpan m_TimeElapsed = new TimeSpan();
        private TimeSpan m_TimeSpent = new TimeSpan();

        private eTimerStatus m_TimerStatus = eTimerStatus.Time_Stopped;
        private enum eTimerStatus {
            Time_Running,
            Time_Stopped
        }

        private List<CTScSubCampaignList> m_lstSubCampaigns = null;
        private FrmHelp m_HelpInfo = new FrmHelp();
        private bool m_HelpInfoShown = false;
        //private bool m_CompanyWorkedByAnotherConsultant = false;

        private Unlock m_frmUnlock = null;
        private ChangePassword m_frmChangePassword = null;

        private bool m_OutsideModuleCall = false; // means that the campaign booking locad call was made outside sales consultant.
        private int m_LoadSpecificDataAccountId = 0;
        private int m_LoadSpecificDataContactId = 0;
        private int m_ReportConfigurationId = 0;

        private ViewConfiguration m_eftReportConfiguration = null;
        #endregion

        #region Public Methods
        public void SetFormControls(bool IsVisible) 
        {
            tcSalesConsultant.Visible = IsVisible;
            if (IsVisible) {
                this.InitializeCampaignListModule();
                //this.InitCampaignBooking(); //todo: load if only called. work later.
                this.Show();
            }
            else
                this.Hide();
        }

        //public void LoadSpecificData(MyFollowUps.CampaignBookingArgs pCampaignBookingArgs)
        private void LoadSpecificData(CampaignBookingProperty.CampaignBoookingArguments pCampaignBookingArgs)
        {
            if (pCampaignBookingArgs == null || 
                pCampaignBookingArgs.oAppointment == null)
                    return;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                subcampaign_users _User = _efDbContext.subcampaign_users.FirstOrDefault(i =>
                    i.subcampaign_id == pCampaignBookingArgs.oAppointment.SubCampaignId &&
                    i.user_id == UserSession.CurrentUser.UserId &&
                    i.internal_user == true
                );
                if (_User != null)
                    _efDbContext.Detach(_User);
                else {
                    if (!pCampaignBookingArgs.IsAssignedToTeam) {
                        NotificationDialog.Warning("Bright Sales", string.Format("You are not a member of this sub-campaign.{0}Please contact your administrator.", Environment.NewLine));
                        return;
                    }
                }
            }

            bool _found = false;
            int _idx = 0;
            for (; _idx < m_lstSubCampaigns.Count; _idx++) {
                string[] _ids = m_lstSubCampaigns[_idx].id.Split(';');
                string[] _sub_campaign = _ids[2].Split('|');
                if (Convert.ToInt32(_sub_campaign[0]) == pCampaignBookingArgs.oAppointment.SubCampaignId) {
                    _found = true;
                    break;
                }
            }
            
            /**
             * if match is found, then load that sub campaign, only if not the same account and contact.
             * if the same, no reason to reload it. cost of process only.
             */
            if (_found) {
                //if (m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId == pCampaignBookingArgs.oAppointment.AccountId &&
                //    m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId == pCampaignBookingArgs.ContactId) {
                //    NotificationDialog.Error("Bright Sales", "You cannot reload the same subcampaign / account / contact.");
                //    return;
                //}

                //if (m_LoadSpecificDataAccountId == pCampaignBookingArgs.oAppointment.AccountId &&
                //    m_LoadSpecificDataContactId == pCampaignBookingArgs.ContactId) {
                //    NotificationDialog.Error("Bright Sales", "You cannot reload the same subcampaign / account / contact.");
                //    return;
                //}

                /**
                 * this code part is intended so that the campaign list will force
                 * trigger the EditValueChanged() event. Since if the last index is the same,
                 * it will not proceed to performming the EditValueChanged().
                 */
                m_OutsideModuleCall = true;
                this.cboCampaignList.EditValueChanged -= new System.EventHandler(this.cboCampaignList_EditValueChanged);
                this.cboCampaignList.EditValue = null;
                this.cboCampaignList.EditValueChanged += new System.EventHandler(this.cboCampaignList_EditValueChanged);
                
                m_LoadSpecificDataAccountId = pCampaignBookingArgs.oAppointment.AccountId;
                m_LoadSpecificDataContactId = pCampaignBookingArgs.ContactId;
                cboCampaignList.ItemIndex = _idx;
                
                //if (m_CampaignListModule != null)
                //    m_CampaignListModule.CompanySetFocus(pCampaignBookingArgs.oAppointment.AccountId);

                //if (m_CampaignBookingModule != null)
                //    m_CampaignBookingModule.ContactSetFocus(pCampaignBookingArgs.ContactId);
            }
            else
                NotificationDialog.Warning("Bright Sales", "This particular sub-campaign is not on your sub-campaign lists.");
        }
        #endregion

        #region Private Methods
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<LoginEvents.OnSuccess>().Subscribe(LoginSuccess);
            m_EventBus.GetEvent<CampaignListEvents.PrepareCampaignBooking>().Subscribe(PrepareCampaignBooking);
            m_EventBus.GetEvent<EventFollowUpLogEvents.CompanyRemarksSaved.FrmSalesConsultant>().Subscribe(CompanyRemarksSaved);
            m_EventBus.GetEvent<ManageCampaignBookingEvents.SetCallViewBarState>().Subscribe(SetStateCallerBarGroup);
            //m_EventBus.GetEvent<ManageCampaignBookingEvents.OnLoadNextCompany.FrmSalesConsultant>().Subscribe(LoadNextCompany);
            //m_EventBus.GetEvent<ManageCampaignBookingEvents.OnLoadPreviousCompany>().Subscribe(LoadPreviousCompany);
            m_EventBus.GetEvent<ManageCampaignBookingEvents.OnFormLoad>().Subscribe(OnCampaignBookingLoad);
            m_EventBus.GetEvent<ManageCampaignBookingEvents.OnCompanyRelease>().Subscribe(OnCompanyRelease);
            m_EventBus.GetEvent<ManageCampaignBookingEvents.OnSave.FrmSalesConsultant>().Subscribe(CampaignBookingOnSave);
            m_EventBus.GetEvent<PhoneCallSaveEventNotifier>().Subscribe(PhoneCallSave);
            m_EventBus.GetEvent<PhoneCallEndEventNotifier>().Subscribe(PhoneCallEnd);
            m_EventBus.GetEvent<PhoneRegisterSuccessEventNotifier>().Subscribe(PhoneRegisterSuccess);
            m_EventBus.GetEvent<EventFollowUpLogEvents.AfterDelete>().Subscribe(EventLogAfterDelete);
            m_EventBus.GetEvent<EventFollowUpLogEvents.OnSave>().Subscribe(EventLogOnSave);
            m_EventBus.GetEvent<EventFollowUpLogEvents.OnWorkNurtureEvent>().Subscribe(EventLogWorkNurtureEvent);
            m_EventBus.GetEvent<CompanyInformationEvents.OnSave.FrmSalesConsultant>().Subscribe(CompanyInformationOnSave);
            m_EventBus.GetEvent<CompanyInformationEvents.OnRefresh>().Subscribe(CompanyInformationOnRefresh);
            m_EventBus.GetEvent<CallViewBarEvents.PhoneCallStart.FrmSalesConsultant>().Subscribe(PhoneCallStart);
            m_EventBus.GetEvent<CallViewBarEvents.PhoneCallAttempt>().Subscribe(PhoneCallAttempt);
            m_EventBus.GetEvent<FrmSalesConsultantEvents.Tracer>().Subscribe(Tracer);
            m_EventBus.GetEvent<ManageCampaignBookingEvents.OnDialogEditorSaved>().Subscribe(DialogEditorOnSaveCompleted);
            m_EventBus.GetEvent<MyFollowUpsEvents.OnCampaignBookingLoad>().Subscribe(EventsTabOnCampaignBookingLoad);
            m_EventBus.GetEvent<FollowUpBarEvents.OnLoadPress>().Subscribe(EventsBarOnLoadPress);
            m_EventBus.GetEvent<FollowUpBarEvents.OnSave>().Subscribe(EventsBarOnSave);
            m_EventBus.GetEvent<FollowUpBarEvents.GetCampaignBookingArgs>().Subscribe(EventsBarGetEventCampaignBookingArgs);
            m_EventBus.GetEvent<FollowUpBarEvents.CheckForBrowsableData>().Subscribe(EventsBarCheckForBrowsableData);
            m_EventBus.GetEvent<FollowUpBarEvents.OnMoveFirst>().Subscribe(EventsBarOnMoveFirst);
            m_EventBus.GetEvent<FollowUpBarEvents.OnMovePrevious>().Subscribe(EventsBarOnMovePrevious);
            m_EventBus.GetEvent<FollowUpBarEvents.OnMoveNext>().Subscribe(EventsBarOnMoveNext);
            m_EventBus.GetEvent<ReportConfigurationEvents.OnReportPreview>().Subscribe(OnReportPreview);

            m_EventBus.GetEvent<CallLogBarVoidEvent>().Where(e => e == CallLogBarVoidEvent.UseContact).Subscribe(PhoneCallStarted);
            m_EventBus.GetEvent<CallLogBarVoidEvent>().Where(e => e == CallLogBarVoidEvent.PhoneCallStarted).Subscribe(PhoneCallStarted);
            m_EventBus.GetEvent<CallLogBarVoidEvent>().Where(e => e == CallLogBarVoidEvent.CallMobileInitiated).Subscribe(CallMobileInitiated);
            m_EventBus.GetEvent<CallLogBarVoidEvent>().Where(e => e == CallLogBarVoidEvent.CallDirectInitiated).Subscribe(CallDirectInitiated);
            m_EventBus.GetEvent<CallLogBarVoidEvent>().Where(e => e == CallLogBarVoidEvent.CallBoardInitiated).Subscribe(CallBoardInitiated);
            m_EventBus.GetEvent<CallLogBarVoidEvent>().Where(e => e == CallLogBarVoidEvent.CallAnonymousInitiated).Subscribe(CallAnonymousInitiated);
            m_EventBus.GetEvent<CallLogBarVoidEvent>().Where(e => e == CallLogBarVoidEvent.CallNotConnected).Subscribe(CallNotConnected);
        }
        private void OnReportPreview(ReportConfigurationEvents.OnReportPreview e)
        {
            m_ReportConfigurationId = e.ReportConfigId;
            tcSalesConsultant.SelectedTabPage = tabReport;
            m_ReportConfigurationId = 0;
        }

        private void EventsTabOnCampaignBookingLoad(MyFollowUpsEvents.OnCampaignBookingLoad e)
        {
            string _CustomerName = string.Empty;
            string _CampaignName = string.Empty;
            string _SubCampaignName = string.Empty;
            m_BrightSalesProperty.CampaignBooking.ParentView = SelectionProperty.ParentView.CampaignList; // this is because next and previous button is only for campaign list. when this is loaded, there are campaign list items anyway.
            //m_BrightSalesProperty.CampaignBooking.ParentView = SelectionProperty.ParentView.MyFollowUp;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                _CustomerName = _efDbContext.customers.FirstOrDefault(i => i.id == e.OnCampaignBookingLoadArgs.oAppointment.CustomerId).customer_name;
                _CampaignName = _efDbContext.campaigns.FirstOrDefault(i => i.id == e.OnCampaignBookingLoadArgs.oAppointment.CampaignId).campaign_name;
                _SubCampaignName = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == e.OnCampaignBookingLoadArgs.oAppointment.SubCampaignId).title;
            }
            m_oFollowUp.SubCampaignId = e.OnCampaignBookingLoadArgs.oAppointment.SubCampaignId;
            m_oFollowUp.Prepare();
            m_oFollowUp.LoadSalesUsers();
            m_oFollowUp.SetCampaignInfo(_CustomerName, _CampaignName, _SubCampaignName);
            this.LoadSpecificData(e.OnCampaignBookingLoadArgs);
            m_BrightSalesProperty.CommonProperty.IsLoadEventLog = false;
        }
        private void EventsBarOnLoadPress(FollowUpBarEvents.OnLoadPress e)
        {
            if (m_MyFollowUpModule == null)
                this.InitializeMyFollowUpModule();

            m_MyFollowUpModule.LoadEvents();
        }
        private void EventsBarOnSave(FollowUpBarEvents.OnSave e)
        {
            if (m_MyFollowUpModule != null)
                m_MyFollowUpModule.LoadEvents();
        }
        private void EventsBarGetEventCampaignBookingArgs(FollowUpBarEvents.GetCampaignBookingArgs e)
        {
            if (m_MyFollowUpModule == null)
                m_BrightSalesProperty.EventsProperty.CampaignBookingArgs = new CampaignBookingProperty.CampaignBoookingArguments();
            else {
                m_MyFollowUpModule.SetFocusRow();
                m_BrightSalesProperty.EventsProperty.CampaignBookingArgs = m_MyFollowUpModule.GetCampaignBookingArgs(e.ForWorkModePurpose);
            }
        }
        private void EventsBarCheckForBrowsableData(FollowUpBarEvents.CheckForBrowsableData e)
        {
            if (m_MyFollowUpModule == null) {
                WaitDialog.Show("Loading data ...");
                this.InitializeMyFollowUpModule();
                WaitDialog.Close(true);
            }

            m_BrightSalesProperty.EventsProperty.HasDataRows = m_MyFollowUpModule.HasBrowsableData();
        }
        private void EventsBarOnMoveFirst(FollowUpBarEvents.OnMoveFirst e)
        {
            if (m_MyFollowUpModule.MoveFirst()) {
                m_BrightSalesProperty.EventsProperty.CampaignBookingArgs = m_MyFollowUpModule.GetCampaignBookingArgs(false);
                m_oFollowUpBar.SetCampaignInfo(m_BrightSalesProperty.EventsProperty.CampaignBookingArgs.CampaignInfo, m_BrightSalesProperty.EventsProperty.CampaignBookingArgs.ToolTipInfo);
                m_oFollowUpBar.EventId = m_MyFollowUpModule.GetId();
            }
        }
        private void EventsBarOnMovePrevious(FollowUpBarEvents.OnMovePrevious e)
        {
            if (m_MyFollowUpModule.MovePrevious()) {
                m_BrightSalesProperty.EventsProperty.CampaignBookingArgs = m_MyFollowUpModule.GetCampaignBookingArgs(false);
                m_oFollowUpBar.SetCampaignInfo(m_BrightSalesProperty.EventsProperty.CampaignBookingArgs.CampaignInfo, m_BrightSalesProperty.EventsProperty.CampaignBookingArgs.ToolTipInfo);
                m_oFollowUpBar.EventId = m_MyFollowUpModule.GetId();
            }
        }
        private void EventsBarOnMoveNext(FollowUpBarEvents.OnMoveNext e)
        {
            if (m_MyFollowUpModule.MoveNext()) {
                m_BrightSalesProperty.EventsProperty.CampaignBookingArgs = m_MyFollowUpModule.GetCampaignBookingArgs(false);
                m_oFollowUpBar.SetCampaignInfo(m_BrightSalesProperty.EventsProperty.CampaignBookingArgs.CampaignInfo, m_BrightSalesProperty.EventsProperty.CampaignBookingArgs.ToolTipInfo);
                m_oFollowUpBar.EventId = m_MyFollowUpModule.GetId();
            }
        }

        private void Tracer(FrmSalesConsultantEvents.Tracer e)
        {
            //NotificationDialog.Information("Tracer", e.ErrorMessage);
        }
        private void CompanyInformationOnRefresh(CompanyInformationEvents.OnRefresh e)
        {
            if (!m_BrightSalesProperty.CommonProperty.OnCallMode) {
                m_oCallViewBar.SetContactNumbers();
                if (!string.IsNullOrEmpty(e.OnRefreshArgs.Account.telephone) && !string.IsNullOrEmpty(e.OnRefreshArgs.Account.telephone.Trim()))
                    m_oCallViewBar.CompanyBoardNo = e.OnRefreshArgs.Account.telephone.Trim();
                else
                    m_oCallViewBar.CompanyBoardNo = string.Empty;
            }

            m_oCallViewBar.CompanyBoardNo = e.OnRefreshArgs.Account.telephone;
            if (!m_BrightSalesProperty.CommonProperty.OnCallMode)
                m_oCallViewBar.SetContactNumbers();

            m_oFollowUp.SetCompanyInfo();
            if (m_CampaignBookingModule != null) {
                //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList) {
                if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList) {
                    m_CampaignListModule.UpdateSelectedCompany(new CompanyInformation.CompanyInformationArgs() {
                        Account = e.OnRefreshArgs.Account,
                        AccountSubCampaignRemarks = e.OnRefreshArgs.AccountSubCampaignRemarks
                    });
                    if (tcSalesConsultant.SelectedTabPage == tabCampaignBooking)
                        m_CampaignListModule.SetCompanyDetails(e.OnRefreshArgs.Account.id);
                }
            }
        }
        private void CompanyInformationOnSave(CompanyInformationEvents.OnSave.FrmSalesConsultant e)
        {
            /*
             * https://docs.google.com/a/myndconsulting.com/spreadsheet/ccc?key=0Ah8Xvlc0xaJKdG1CbEJEbWdYTkdPanpqeHJWNGtmYXc#gid=0
             */
            //this.SaveCompanyAppointment();
            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.UpdateCompanyInformation();

            if (!m_BrightSalesProperty.CommonProperty.OnCallMode) {
                m_oCallViewBar.SetContactNumbers();
                if (!string.IsNullOrEmpty(e.OnSaveArgs.Account.telephone) && !string.IsNullOrEmpty(e.OnSaveArgs.Account.telephone.Trim()))
                    m_oCallViewBar.CompanyBoardNo = e.OnSaveArgs.Account.telephone.Trim();
                else
                    m_oCallViewBar.CompanyBoardNo = string.Empty;
            }

            m_oCallViewBar.CompanyBoardNo = e.OnSaveArgs.Account.telephone;
            if (!m_BrightSalesProperty.CommonProperty.OnCallMode)
                m_oCallViewBar.SetContactNumbers();

            m_oFollowUp.SetCompanyInfo();
            if (m_CampaignBookingModule != null)
            {
                //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
                if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList)
                {
                    m_CampaignListModule.UpdateSelectedCompany(new CompanyInformation.CompanyInformationArgs()
                    {
                        Account = e.OnSaveArgs.Account,
                        AccountSubCampaignRemarks = e.OnSaveArgs.AccountSubCampaignRemarks
                    });
                    m_CampaignListModule.SetCompanyDetails(e.OnSaveArgs.Account.id);
                }
            }
        }
        private void EventLogWorkNurtureEvent(EventFollowUpLogEvents.OnWorkNurtureEvent e)
        {
            if (e == null)
                return;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_account_lists _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                    i.account_id == e.OnWorkArgs.oAppointment.AccountId &&
                    i.final_list_id == e.OnWorkArgs.oAppointment.FinalListId
                );
                if (_item != null)
                    _efDbContext.Detach(_item);

                if (_item.locked && _item.locked_by != UserSession.CurrentUser.UserId) {
                    user _user = _efDbContext.users.FirstOrDefault(i => i.id == _item.locked_by);
                    if (_user != null)
                        _efDbContext.Detach(_user);

                    NotificationDialog.Error("Bright Sales", string.Format("You cannot work on this follow up.{0}This follow up is currently worked by{0}{1}", Environment.NewLine, _user.fullname));
                    return;
                }
            }

            this.LoadSpecificData(e.OnWorkArgs);
        }
        private void EventLogOnSave(EventFollowUpLogEvents.OnSave e)
        {
            if (m_CampaignListModule != null)
                m_CampaignListModule.ReloadCallLogTab();
        }
        private void EventLogAfterDelete(EventFollowUpLogEvents.AfterDelete e)
        {
            if (m_CampaignListModule != null)
                m_CampaignListModule.DeleteCallLog(e.DeletedCallLogId);
        }
        private void CompanyRemarksSaved(EventFollowUpLogEvents.CompanyRemarksSaved.FrmSalesConsultant e)
        {
            this.SaveCompanyAppointment();
        }
        private void LoginSuccess(LoginEvents.OnSuccess e)
        {
            if (e.WorkingEnvironment == SelectionProperty.WorkingEnvironment.Demo)
                this.Text = string.Format("{0}  [ USING DEMO ENVIRONMENT ]", this.Text);
        }
        private void PrepareCampaignBooking(CampaignListEvents.PrepareCampaignBooking e)
        {
            if (m_MyFollowUpModule != null)
                m_MyFollowUpModule.CloseCompany();

            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.SetBreadCrumb(string.Empty);

            if (m_BrightSalesProperty.CommonProperty == null || m_BrightSalesProperty.CampaignBooking.Appointment == null)
                return;

            this.SetStateCallerBarGroup(true);
            m_oCallViewBar.Clear();
            m_oCallLogBar.Clear();
            m_oFollowUp.Prepare();

            this.InitializeCampaignBookingModule();
            m_CampaignBookingModule.IsLoadingCampaignBooking = true;
            m_CampaignBookingModule.ResetContactParameters();
            //m_CampaignBookingModule.PrepareStatusCombos();
            m_CampaignBookingModule.SelectedCompany = m_BrightSalesProperty.CommonProperty.CompanyName;
            m_CampaignBookingModule.SelectedContactId = m_BrightSalesProperty.CommonProperty.ContactId;
            m_CampaignBookingModule.SetBreadCrumb(m_BrightSalesProperty.CampaignBooking.BreadCrumb);
            //m_CampaignBookingModule.ParentView = ManageCampaignBooking.eParentView.CampaignList;
            m_BrightSalesProperty.CampaignBooking.ParentView = SelectionProperty.ParentView.CampaignList;
            m_BrightSalesProperty.CampaignBooking.Mode = SelectionProperty.CampaignBookingMode.Work;
            //m_CampaignBookingModule.CampaignBookingMode = ManageCampaignBooking.eCampaignBookingMode.WorkMode;

            m_CampaignBookingModule.InitializeCampaignBooking();
            m_CampaignBookingModule.LoadCampaignBookingData();
            m_CampaignBookingModule.SetDefaultTab();

            tcSalesConsultant.SelectedTabPage = tabCampaignBooking;
            m_CampaignBookingModule.IsLoadingCampaignBooking = false;
        }
        //private void LoadPreviousCompany(ManageCampaignBookingEvents.OnLoadPreviousCompany e)
        //{
        //    m_BrightSalesProperty.CampaignBooking.LoadPreviousCompanySuccess = false;
        //    //m_CompanyWorkedByAnotherConsultant = false;
        //    //m_CampaignBookingModule.CompanyWorkedByAnotherConsultant = false;

        //    //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //    if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList)
        //        m_BrightSalesProperty.CampaignBooking.LoadPreviousCompanySuccess = m_CampaignListModule.MovePrevious();

        //    if (!m_BrightSalesProperty.CommonProperty.CompanyLocked && m_BrightSalesProperty.CampaignBooking.LoadPreviousCompanySuccess)
        //        this.SetStateCallerBarGroup(true);
        //    else
        //        this.SetStateCallerBarGroup(false);
        //}
        //private void LoadNextCompany(ManageCampaignBookingEvents.OnLoadNextCompany e)
        //{
        //    //m_BrightSalesProperty.CampaignBooking.LoadNextCompanySuccess = false;
        //    //m_CompanyWorkedByAnotherConsultant = false;
        //    //m_CampaignBookingModule.CompanyWorkedByAnotherConsultant = false;

        //    //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //    if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList)
        //        m_CampaignListModule.MoveNext();
        //        //m_BrightSalesProperty.CampaignBooking.LoadNextCompanySuccess = m_CampaignListModule.MoveNext();

        //    if (!m_BrightSalesProperty.CommonProperty.CompanyLocked && m_BrightSalesProperty.CampaignBooking.LoadNextCompanySuccess)
        //        this.SetStateCallerBarGroup(true);
        //    else
        //        this.SetStateCallerBarGroup(false);
        //}
        private void SetStateCallerBarGroup(ManageCampaignBookingEvents.SetCallViewBarState e)
        {
            this.SetStateCallerBarGroup(e.State);
        }
        private void CampaignBookingOnSave(ManageCampaignBookingEvents.OnSave.FrmSalesConsultant e)
        {
            /**
             * execute only if not called by send email.
             * since we dont need to disable anything yet when just sending email, just saving data only.
             */
            if (e.CalledFromSendEmail) {
                //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
                if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList)
                    m_CampaignListModule.SetCompanyDetails(m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId);
                //else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp)
                else if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.MyFollowUp)
                    m_MyFollowUpModule.SaveCompanyAppointment();
                return;
            }

            this.CampaignBookingDataSaved();
            this.SetStateCallerBarGroup(false);
            //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList) {
            if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList) {
                m_CampaignListModule.SetCompanyDetails(m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId);
                m_CampaignListModule.ReleaseCurrentCompanyLock();
            }
            //else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp) {
            else if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.MyFollowUp) {
                m_MyFollowUpModule.SaveCompanyAppointment();
                m_MyFollowUpModule.ReleaseCompanyLock();
            }

            m_oCallLogBar.EndClock();
            m_oCallLogBar.Default();
            m_oCallViewBar.PhoneCallEnded();
            m_oCallViewBar.Default();
        }
        private void OnCampaignBookingLoad(ManageCampaignBookingEvents.OnFormLoad e)
        {
            if (m_BrightSalesProperty.CommonProperty.UserOnWorkModeState)
                this.SetStateCallerBarGroup(true);

            m_oCallViewBar.AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId;
            m_oCallViewBar.SubCampaignId = m_BrightSalesProperty.CommonProperty.SubCampaignId;
            m_oCallViewBar.FinalListId = m_BrightSalesProperty.CommonProperty.FinalListId;
            m_oCallViewBar.CompanyBoardNo = m_BrightSalesProperty.CampaignBooking.Appointment.CompanyBoardNumber;
            m_oCallViewBar.ContactPerson = e.ContactPerson;
            m_oCallViewBar.Default();
            m_oCallViewBar.SetContactNumbers();

            m_oCallLogBar.AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId;
            m_oCallLogBar.SubCampaignId = m_BrightSalesProperty.CommonProperty.SubCampaignId;
            m_oCallLogBar.Default();

            m_oFollowUp.AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId;
            m_oFollowUp.SetCompanyInfo();
        }
        private void OnCompanyRelease(ManageCampaignBookingEvents.OnCompanyRelease e)
        {
            //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList) {
            if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList) {
                m_CampaignListModule.ReleaseCurrentCompanyLock();
                m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
            }
            //else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp) {
            else if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.MyFollowUp) {
                m_MyFollowUpModule.ReleaseCompanyLock();
                m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
            }

            ////if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList) {
            //if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList)
            //    m_CampaignListModule.ReleaseCurrentCompanyLock();
            //    m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
            //}
            ////else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp) {
            //else if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.MyFollowUp) {
            //    m_MyFollowUpModule.ReleaseCurrentCompanyLock();
            //    m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = false;
            //}
        }
        private void PhoneRegisterSuccess(PhoneRegisterSuccessEventNotifier e)
        {
            NotificationDialog.Information("Bright Sales", e.NotificationMessage);
        }
        private void PhoneCallSave(PhoneCallSaveEventNotifier e)
        {
            m_oCallLogBar.Visible = false;
            m_oCallViewBar.Visible = true;
            //m_oCallViewBar.PhoneCallEnded();
            m_oCallViewBar.Default();
            m_oCallViewBar.SetContactNumbers();

            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.m_oCallLogBar_btnSaveCallLog_OnClick(e.EventLogId);

            if (m_CampaignListModule != null)
                m_CampaignListModule.ReloadCallLogTab();

            tcSalesConsultant.CustomHeaderButtons[0].Visible = false;
        }
        private void PhoneCallEnd(PhoneCallEndEventNotifier e)
        {
            m_oCallLogRemarksBar.CallLogBarParams = m_oCallLogBar.CallLogBarParams;
            //m_oCallLogRemarksBar.ContactPerson = m_oCallLogBar.ContactPerson;
            m_oCallLogRemarksBar.SubCampaignId = m_oCallLogBar.SubCampaignId;
            m_oCallLogRemarksBar.AccountId = m_oCallLogBar.AccountId;
            m_oCallLogRemarksBar.StartTime = e.CallStart;
            m_oCallLogRemarksBar.EndTime = e.CallEnd;
            //m_oCallLogRemarksBar.LoadContactPerson();

            //m_oCallLogRemarksBar.CallLogBarParams = m_oCallLogBar.CallLogBarParams;
            //m_oCallLogRemarksBar.ContactPerson = m_oCallLogBar.ContactPerson;
            //m_oCallLogRemarksBar.SubCampaignId = m_oCallLogBar.SubCampaignId;
            //m_oCallLogRemarksBar.AccountId = m_oCallLogBar.AccountId;
            //m_oCallLogRemarksBar.LoadContactPerson();

            m_oCallLogBar.Visible = false;
            m_oCallViewBar.Visible = true;
            m_oCallViewBar.PhoneCallEnded();
            //m_oCallLogRemarksBar.Visible = true;
            //m_oFollowUpBar.Visible = false;
            //m_oCallViewBar.Default();
            //m_oCallViewBar.SetContactNumbers();

            //if (m_CampaignBookingModule != null)
            //    m_CampaignBookingModule.m_oCallLogBar_btnHangUp_OnClick(e.ContactId);
            tcSalesConsultant.CustomHeaderButtons[0].Visible = false;
        }
        private void PhoneCallStart(CallViewBarEvents.PhoneCallStart.FrmSalesConsultant e)
        {
            m_oCallLogBar.Visible = true;
            m_oCallViewBar.Visible = false;
            m_oCallLogRemarksBar.Visible = true;
            m_oFollowUpBar.Visible = false;

            //m_oCallLogRemarksBar.CallMethod = m_oCallLogBar.CallLogBarParams.CallMethod;
            //m_oCallLogRemarksBar.ContactPerson = e.ContactPerson;
            //m_oCallLogRemarksBar.LoadContactPerson();

            //m_oCallLogBar.CallLogBarParams = new FrmSalesConsultant.CallLogArgs()
            //{
            //    ContactId = e.ContactId,
            //    ContactNo = e.ContactNo
            //};
            //m_oCallLogBar.CallLogBarParams.CallMethod = e.CallMethod;


            //m_oCallLogBar.ContactPerson = e.ContactPerson;
            //m_oCallLogBar.InitiatePhoneCall();

            #region Log
            var m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            audio_settings _item = m_efDbContext.audio_settings.FirstOrDefault(i => i.user_id == UserSession.CurrentUser.UserId);
            var log = BrightSalesFacade.Logger;

            log.SetLogField(LoggingField.called_number, e.PhoneCallArgs.ContactNo);
            log.SetLogField(LoggingField.called_number_type, e.PhoneCallArgs.CallMethod.GetEnumDescription());

            if (_item != null && _item.mode == 0) {
                log.SetLogField(LoggingField.call_engine, "ozeki");
                m_BrightSalesProperty.CommonProperty.CallModeAudioSettings = 0;
            }
            else {
                log.SetLogField(LoggingField.call_engine, "external");
            }

            user u = m_efDbContext.users.FirstOrDefault(i => i.id == UserSession.CurrentUser.UserId);
            sip_accounts sip = m_efDbContext.sip_accounts.FirstOrDefault(i => i.id == u.sip_id);


            int iTotalWidthTab = this.Width;
            string CallingDetails = string.Format("{0}({1}) -> {2}       Activity: Calling {0}.      Account: {3} ", e.PhoneCallArgs.ContactNo, e.PhoneCallArgs.CallMethod.GetEnumDescription(), e.PhoneCallArgs.ContactNo.ToSwedishPhoneNumber(), sip.username);

            if (iTotalWidthTab < 1600)
            {
                
            }
            else
            {
                int iTotalWidth = 0;
                Size tabSize;
                for (int i = 0; i < tcSalesConsultant.TabPages.Count; i++)
                {
                    if (tcSalesConsultant.TabPages[i].PageVisible)
                    {
                        tabSize = TextRenderer.MeasureText(tcSalesConsultant.TabPages[i].Text, tcSalesConsultant.Font);
                        iTotalWidth += tabSize.Width + 20;
                    }
                }


                int iDiff = (iTotalWidthTab - iTotalWidth);
                
                Size s = TextRenderer.MeasureText(CallingDetails, tcSalesConsultant.Font);
                int iSpaceWidth = 10;
                CallingDetails += new string(' ', int.Parse((Math.Ceiling(double.Parse((iDiff / iSpaceWidth).ToString()))).ToString()) + 50);
            }

            
            tcSalesConsultant.CustomHeaderButtons[0].Caption = CallingDetails;
            tcSalesConsultant.CustomHeaderButtons[0].Visible = true;
            log.SendInfo("call_start", "call details");
            #endregion
        }
        private void PhoneCallAttempt(CallViewBarEvents.PhoneCallAttempt e)
        {
            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.m_oCallViewBar_CallAttemptMade();

            if (m_CampaignListModule != null)
                m_CampaignListModule.CallAttemptMade();
        }
        private void DialogEditorOnSaveCompleted(ManageCampaignBookingEvents.OnDialogEditorSaved e)
        {
            SaveCompanyAppointment();
        }
        private void CallNotConnected(CallLogBarVoidEvent e)
        {
            NotificationDialog.Warning("Bright Sales", string.Format("Please ask your administrator for your sip account.{o}This may also be cause by the voip service provider not available.", Environment.NewLine));
        }
        private void CallBoardInitiated(CallLogBarVoidEvent e)
        {
        }
        private void CallDirectInitiated(CallLogBarVoidEvent e)
        {
            m_oFollowUp.ContactPerson = m_oCallViewBar.ContactPerson;
        }
        private void CallMobileInitiated(CallLogBarVoidEvent e)
        {
            m_oFollowUp.ContactPerson = m_oCallViewBar.ContactPerson;
        }
        private void CallAnonymousInitiated(CallLogBarVoidEvent e)
        {
        }
        private void PhoneCallStarted(CallLogBarVoidEvent e)
        {
            m_oCallLogRemarksBar.CallLogBarParams = m_oCallLogBar.CallLogBarParams;
            m_oCallLogRemarksBar.ContactPerson = m_oCallLogBar.ContactPerson;
            m_oCallLogRemarksBar.LoadContactPerson();
            m_oCallViewBar.PhoneCallStarted();
        }
        private CTScSubCampaignContactList UseContact(CallLogBarVoidEvent e)
        {
            if (m_CampaignBookingModule == null)
                return null;

            return m_CampaignBookingModule.m_oCallLogBar_btnUseContact_OnClick();
        }

        protected void SaveCompanyAppointment()
        {
            this.RunAssync(() => {
                if (m_CampaignBookingModule != null) {
                    //ManageCampaignBooking.CampaignBookingArgs _args = m_CampaignBookingModule.GetCampaignBookingArgs;
                    sub_campaign_account_appointments _item = ObjectSubCampaign.SaveSubCampaignCompanyAppointmentStatus(
                        m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                        m_BrightSalesProperty.CommonProperty.FinalListId,
                        m_CampaignBookingModule.CompanyStatus,
                        m_CampaignBookingModule.CompanyLeadStatus
                    );
                    m_CampaignListModule.UpdateSpecificCompany(new StatusArgs() {
                        CompanyId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                        CompanyLastContact = _item.last_contact,
                        CompanyLastUser = UserSession.CurrentUser.UserFullName,
                        CompanyStatus = _item.status,
                        CompanyLeadStatus = _item.lead_status
                    });
                }
                else
                    m_CampaignListModule.UpdateSpecificCompany(null);
                
                NotificationDialog.Information("Bright Sales", "Updated campaign list company information.");
            });
        }
        private void InitializeCampaignBookingModule() 
        {
            /**
             * if already added to the tab, just bypass
             */
            if (m_oCallViewBar != null)
                m_oCallViewBar.Enabled = true;
            if (m_CampaignBookingModule != null) {
                m_CampaignBookingModule.EnableCampaignBooking();
                return;
            }

            /**
             * https://brightvision.jira.com/browse/PLATFORM-2756
             * DAN 4.15.13: Enable custom header buttons Unhide All
             */
            tcSalesConsultant.CustomHeaderButtons[0].Enabled = true;
            tcSalesConsultant.CustomHeaderButtons[0].Visible = true;
            pcCampaignBooking.Controls.Clear();
            m_CampaignBookingModule = new ManageCampaignBooking() {
                Dock = DockStyle.Fill
            };

            //m_CampaignBookingModule.GetCampaignListArgs += new ManageCampaignBooking.GetCampaignListArgsHandler(m_CampaignBookingModule_GetCampaignListArgs);
            m_CampaignBookingModule.SetCompanyModificationInfo += new ManageCampaignBooking.SetCompanyModificationInfoHandler(m_CampaignBookingModule_SetCompanyModificationInfo);
            //m_CampaignBookingModule.CampaignBookingCallAttemptMade += new ManageCampaignBooking.CampaignBookingCallAttemptMadeEventHander(m_CampaignBookingModule_CampaignBookingCallAttemptMade);
            //m_CampaignBookingModule.CallerViewCallLogAdded += new ManageCampaignBooking.CallerViewCallLogAddedEventHandler(m_CampaignBookingModule_CallerViewCallLogAdded);
            m_CampaignBookingModule.ModuleSetStateInitiated += new ManageCampaignBooking.ModuleSetStateInitiatedEventHandler(m_CampaignBookingModule_ModuleSetStateInitiated);
            m_CampaignBookingModule.ContactView_FocusedRowChanged += new ManageCampaignBooking.ContactViewFocusedRowChangedEventHandler(m_CampaignBookingModule_ContactView_FocusedRowChanged);
            m_CampaignBookingModule.ContactView_OnContactSaved += new ManageCampaignBooking.ContactViewOnContactSavedEventHandler(m_CampaignBookingModule_ContactView_OnContactSaved);
            m_CampaignBookingModule.CampaignExtraDetail_OnContactSaved += new ManageCampaignBooking.CampaignExtraDetailOnContactSavedEventHandler(m_CampaignBookingModule_CampaignExtraDetail_OnContactSaved);
            m_CampaignBookingModule.oContactView_PopulateContactView_Initiated += new ManageCampaignBooking.ContactViewPopulateContactViewInitiatedEventHandler(m_CampaignBookingModule_oContactView_PopulateContactView_Initiated);
            //m_CampaignBookingModule.EventFollowupLog_CanWorkOnCompany += new ManageCampaignBooking.EventFollowupLogCanWorkOnCompanyEventHandler(m_CampaignBookingModule_EventFollowupLog_CanWorkOnCompany);
            m_CampaignBookingModule.EventFollowupLog_WorkNurtureEvent += new ManageCampaignBooking.EventFollowupLogWorkNurtureEventEventHandler(m_CampaignBookingModule_EventFollowupLog_WorkNurtureEvent);
            m_CampaignBookingModule.DialogOnEditMode += new ManageCampaignBooking.DialogOnEditModeEventHandler(m_CampaignBookingModule_DialogOnEditMode);

            //m_CampaignBookingModule.SalesUser_OnCallMode += new ManageCampaignBooking.SalesUserOnCallModeEventHandler(m_CampaignBookingModule_SalesUser_OnCallMode);
            //m_CampaignBookingModule.GetMyFollowUpCampaignBookingData += new ManageCampaignBooking.GetMyFollowUpAppointmentDataHandler(m_CampaignBookingModule_GetMyFollowUpCampaignBookingData);
            //m_CampaignBookingModule.btnSave_OnClick += new ManageCampaignBooking.btnSaveOnClickHandler(m_CampaignBookingModule_btnSave_OnClick);
            //m_CampaignBookingModule.CallLogAfterDelete += new ManageCampaignBooking.CallLogAfterDeleteEventHandler(m_CampaignBookingModule_CallLogAfterDelete);
            //m_CampaignBookingModule.LoadCampaignBookingData_Initiated += new ManageCampaignBooking.LoadCampaignBookingDataInitiatedEventHandler(m_CampaignBookingModule_LoadCampaignBookingData_Initiated);
            //m_CampaignBookingModule.btnCancel_OnClick += new ManageCampaignBooking.btnCancelOnClickEventHandler(m_CampaignBookingModule_btnCancel_OnClick);
            //m_CampaignBookingModule.EventFollowupLog_CompanyRemark_Saved += new ManageCampaignBooking.EventFollowupLogCompanyRemarkSavedEventHandler(m_CampaignBookingModule_EventFollowupLog_CompanyRemark_Saved);
            //m_CampaignBookingModule.CallLogBar_IsCallLogSaved += new ManageCampaignBooking.CallLogBarIsCallLogSavedEventHandler(m_CampaignBookingModule_CallLogBar_IsCallLogSaved);
            //m_CampaignBookingModule.OnSave += new ManageCampaignBooking.OnSaveEventHandler(m_CampaignBookingModule_OnSave);
            //m_CampaignBookingModule.EventFollowupLog_UserOnCallOrHasPendingCallLog += new ManageCampaignBooking.EventFollowupLogUserOnCallOrHasPendingCallLogEventHandler(m_CampaignBookingModule_EventFollowupLog_UserOnCallOrHasPendingCallLog);
            //m_CampaignBookingModule.EventFollowupLog_EventLog_OnSave += new ManageCampaignBooking.EventFollowupLogEventLogOnSaveEventHandler(m_CampaignBookingModule_EventFollowupLog_EventLog_OnSave);
            //m_CampaignBookingModule.SaveDialogAnswers += new ManageCampaignBooking.SaveDialogAnswersEventHandler(m_CampaignBookingModule_SaveDialogAnswers);
            //m_CampaignBookingModule.UserWorkModeChanged += new ManageCampaignBooking.UserWorkModeChangeEventHandler(m_CampaignBookingModule_UserWorkModeChanged);
            //m_CampaignBookingModule.CampaignListItemIsFirstRow += new ManageCampaignBooking.CampaignListItemIsFirstRowEventHandler(m_CampaignBookingModule_CampaignListItemIsFirstRow);
            //m_CampaignBookingModule.CampaignListItemIsLastRow += new ManageCampaignBooking.CampaignListItemIsLastRowEventHandler(m_CampaignBookingModule_CampaignListItemIsLastRow);
            //m_CampaignBookingModule.OnCompanyRelease += new ManageCampaignBooking.OnCompanyReleaseEventHandler(m_CampaignBookingModule_OnCompanyRelease);
            //m_CampaignBookingModule.btnClose_OnClick += new ManageCampaignBooking.btnCloseOnClickHandler(m_CampaignBookingModule_btnClose_OnClick);
            //m_CampaignBookingModule.btnPreviousRecord_OnClick += new ManageCampaignBooking.btnPreviousRecordOnClickHandler(m_CampaignBookingModule_btnPreviousRecord_OnClick);
            //m_CampaignBookingModule.btnNextRecord_OnClick += new ManageCampaignBooking.btnNextRecordOnClickHandler(m_CampaignBookingModule_btnNextRecord_OnClick);
            //m_CampaignBookingModule.OnCompanyInformationSaved += new ManageCampaignBooking.CompanyInformationSavedEventHandler(m_CampaignBookingModule_OnCompanyInformationSaved);
            //m_CampaignBookingModule.OnCompanyInformationRefresh += new ManageCampaignBooking.CompanyInformationRefreshEventHandler(m_CampaignBookingModule_OnCompanyInformationRefresh);
            //m_CampaignBookingModule.GetCampaignListViewMode += new ManageCampaignBooking.GetCampaignListViewModeEventHandler(m_CampaignBookingModule_GetCampaignListViewMode);
            //m_CampaignBookingModule.btnWork_OnClick += new ManageCampaignBooking.btnWorkOnClickHandler(m_CampaignBookingModule_btnWork_OnClick);
            //m_CampaignBookingModule.CanWorkOnCompany += new ManageCampaignBooking.CanWorkOnCompanyEventHandler(m_CampaignBookingModule_CanWorkOnCompany);
            
            pcCampaignBooking.Controls.Add(m_CampaignBookingModule);
        }        
        private void InitializeMyFollowUpModule() 
        {
            if (m_MyFollowUpModule == null) {
                m_MyFollowUpModule = new MyFollowUps() {
                    Dock = DockStyle.Fill
                };
                //m_MyFollowUpModule.OnLoadCampaignBooking += new MyFollowUps.OnLoadCampaignBookingHandler(m_MyFollowUpModule_OnLoadCampaignBooking);
                //m_MyFollowUpModule.SetCompanyModificationInfo += new MyFollowUps.SetCompanyModificationInfoHandler(m_MyFollowUpModule_SetCompanyModificationInfo);
                //m_MyFollowUpModule.CanWorkOnCompany += new MyFollowUps.CanWorkOnCompanyEventHandler(m_MyFollowUpModule_CanWorkOnCompany);
                //m_MyFollowUpModule.UserNotMemberOfSubCampaign += new MyFollowUps.UserNotMemberOfSubCampaignEventHandler(m_MyFollowUpModule_UserNotMemberOfSubCampaign);
                //m_MyFollowUpModule.GetSelectedCampaignInfo += new MyFollowUps.GetSelectedCampaignInfoEventHandler(m_MyFollowUpModule_GetSelectedCampaignInfo);
                pnlMyFollowups.Controls.Clear();
                pnlMyFollowups.Controls.Add(m_MyFollowUpModule);
            }

            m_MyFollowUpModule.LoadEvents();
        }        
        private void InitHelpWebsite() {
            if (pnlHelpWebsite.Controls.Count > 0) return;
            WaitDialog.Show(this, "Loading components...");
            pnlHelpWebsite.Controls.Clear();
            var website = new CompanyWebsite();
            website.Clear();
            website.LoadCompanyWebsite("brightsales.weebly.com");
            website.Dock = DockStyle.Fill;
            pnlHelpWebsite.Controls.Add(website);
            WaitDialog.Close();
        }
        private void InitMilltime() {
            if (pnlMilltime.Controls.Count > 0) return;
            WaitDialog.Show(this, "Loading components...");
            pnlMilltime.Controls.Clear();
            var website = new CompanyWebsite();
            website.Clear();
            website.LoadCompanyWebsite("tid.brightvision.se/milltime/cgi/milltime.cgi/login");
            website.Dock = DockStyle.Fill;
            pnlMilltime.Controls.Add(website);
            WaitDialog.Close();
        }

        private void InitializeInternalUsers()
        {
            WaitDialog.Show("Loading components...");
            ManageInternalUser objForm = new ManageInternalUser();
            objForm.Dock = DockStyle.Fill;
            pcInternalUsers.Controls.Clear();
            pcInternalUsers.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void InitializeCustomerUsers()
        {
            WaitDialog.Show("Loading components...");
            ManageCustomerUser objForm = new ManageCustomerUser();
            objForm.Dock = DockStyle.Fill;
            pcCustomerUsers.Controls.Clear();
            pcCustomerUsers.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void InitializeFinalCallLists()
        {
            WaitDialog.Show("Loading components...");
            ManageFinalCallList objForm = new ManageFinalCallList();
            objForm.Dock = DockStyle.Fill;
            pcFinalCallList.Controls.Clear();
            pcFinalCallList.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void InitializeSIPAccounts()
        {
            WaitDialog.Show("Loading components...");
            ManageSIPAccount objForm = new ManageSIPAccount();
            objForm.Dock = DockStyle.Fill;
            pcSIPAccounts.Controls.Clear();
            pcSIPAccounts.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void InitializeReportConfiguration()
        {
            WaitDialog.Show("Loading components ...");
            if (m_eftReportConfiguration == null)
                m_eftReportConfiguration = new ViewConfiguration();

            m_eftReportConfiguration.Dock = DockStyle.Fill;
            m_eftReportConfiguration.btnPreviewViewOnClick += new ViewConfiguration.btnPreviewViewOnClickEventHandler(objViewConfiguration_btnPreviewViewOnClick);
            pcReportConfiguration.Controls.Clear();
            pcReportConfiguration.Controls.Add(m_eftReportConfiguration);
            WaitDialog.Close();
        }
        private void InitializeSubCampaigns()
        {
            WaitDialog.Show("Loading components...");
            ManageSubCampaign objForm = new ManageSubCampaign();
            objForm.Dock = DockStyle.Fill;
            pcSubCampaigns.Controls.Clear();
            pcSubCampaigns.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void InitializeCompaniesAndContacts()
        {
            WaitDialog.Show("Loading components...");
            ManageCompanyContact objForm = new ManageCompanyContact();
            objForm.Dock = DockStyle.Fill;
            pcCompaniesAndContacts.Controls.Clear();
            pcCompaniesAndContacts.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void InitializeMasterDataImports()
        {
            WaitDialog.Show("Loading components...");
            MasterDataImport objForm = new MasterDataImport();
            objForm.Dock = DockStyle.Fill;
            pcMasterDataImport.Controls.Clear();
            pcMasterDataImport.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void InitializeImportAndViewList()
        {
            WaitDialog.Show("Loading components...");
            ManageImportList objForm = new ManageImportList();
            objForm.Dock = DockStyle.Fill;
            pcImportAndViewList.Controls.Clear();
            pcImportAndViewList.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void InitializeCallListCreation()
        {
            WaitDialog.Show("Loading components...");
            ManageCallList objForm = new ManageCallList();
            objForm.Dock = DockStyle.Fill;
            pcCallListCreation.Controls.Clear();
            pcCallListCreation.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void InitializeManageQuestions()
        {
            WaitDialog.Show("Loading components...");
            ManageQuestions objForm = new ManageQuestions();
            objForm.Dock = DockStyle.Fill;
            pcManageQuestions.Controls.Clear();
            pcManageQuestions.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void InitializeManageDialogs()
        {
            WaitDialog.Show("Loading components...");
            ManageDialogs objForm = new ManageDialogs();
            objForm.Dock = DockStyle.Fill;
            pcManageDialogs.Controls.Clear();
            pcManageDialogs.Controls.Add(objForm);
            WaitDialog.Close();
        }
        private void objViewConfiguration_btnPreviewViewOnClick(int pViewConfigId)
        {
            //navBarGroup5.SelectedLinkIndex = 1;
            //m_ViewConfigId = pViewConfigId;
            //this.navBarItemDisplayViews_LinkClicked(null, null);
        }
        private void InitializeViewEventLogs()
        {
            WaitDialog.Show("Loading components...");
            ManageEventLog objForm = new ManageEventLog();
            objForm.Dock = DockStyle.Fill;
            pcViewEventLogs.Controls.Clear();
            pcViewEventLogs.Controls.Add(objForm);
            //objForm.GetCallLogs();
            objForm.Visible = true;
            WaitDialog.Close();
        }
        private void InitializeCallLogs()
        {
            WaitDialog.Show("Loading call logs...");
            ViewCallLogs objForm = new ViewCallLogs();
            objForm.Dock = DockStyle.Fill;
            pcCallLogs.Controls.Clear();
            pcCallLogs.Controls.Add(objForm);
            //objForm.GetCallLogs();
            objForm.Visible = true;
            WaitDialog.Close();
        }
        private void InitializeViewMessageLogs()
        {
            WaitDialog.Show("Loading message logs...");
            ManageMessageLog objForm = new ManageMessageLog();
            objForm.Dock = DockStyle.Fill;
            pcViewMessageLogs.Controls.Clear();
            pcViewMessageLogs.Controls.Add(objForm);
            //objForm.GetCallLogs();
            objForm.Visible = true;
            WaitDialog.Close();
        }
        private void InitializeDashboard()
        {
            WaitDialog.Show("Loading dashbaord...");
            DashboardHighChart objForm = new DashboardHighChart();
            objForm.LoadDashboard(m_BrightSalesProperty.CommonProperty.SubCampaignId, UserSession.CurrentUser.UserId);
            objForm.Dock = DockStyle.Fill;
            pcDashboard.Controls.Clear();
            pcDashboard.Controls.Add(objForm);
            objForm.Visible = true;
            WaitDialog.Close();
        }
        private void InitializeTitleSettings()
        {
            WaitDialog.Show("Loading components...");
            ManageTitleSetting objForm = new ManageTitleSetting();
            objForm.Dock = DockStyle.Fill;
            pcTitlesSettings.Controls.Clear();
            pcTitlesSettings.Controls.Add(objForm);
            //objForm.GetCallLogs();
            objForm.Visible = true;
            WaitDialog.Close();
        }
        private void InitializeLanguageSettings()
        {
            WaitDialog.Show("Loading components...");
            ManageLanguageSetting objForm = new ManageLanguageSetting();
            objForm.Dock = DockStyle.Fill;
            pcLanguagesSettings.Controls.Clear();
            pcLanguagesSettings.Controls.Add(objForm);
            //objForm.GetCallLogs();
            objForm.Visible = true;
            WaitDialog.Close();
        }
        private void InitializeCustomersAndCampaigns()
        {
            WaitDialog.Show("Loading components...");
            ManageCustomerCampaign objForm = new ManageCustomerCampaign();
            objForm.Dock = DockStyle.Fill;
            pcCustomersAndCampaigns.Controls.Clear();
            pcCustomersAndCampaigns.Controls.Add(objForm);
            //objForm.GetCallLogs();
            objForm.Visible = true;
            WaitDialog.Close();
        }
        private void InitReports() 
        {
            if (pnlReports.Controls.Count > 0 && m_ReportConfigurationId < 1) 
                return;

            WaitDialog.Show(this, "Loading components ...");
            pnlReports.Controls.Clear();

            if (m_ReportConfigurationId > 0)
                m_ReportModule = new ViewDisplay(m_ReportConfigurationId);
            else
                m_ReportModule = new ViewDisplay();

            m_ReportModule.Dock = DockStyle.Fill;
            pnlReports.Controls.Add(m_ReportModule);
            WaitDialog.Close();
        }
        private void InitializeCampaignListModule() 
        {
            //if (m_CampaignListModule != null && !m_RefreshSubCampaignList)
            if (m_CampaignListModule != null) {
                m_CampaignListModule.InitializeModules();
                return;
            }

            WaitDialog.Show(this, "Loading components.");
            m_CampaignListModule = new ManageCampaignList();
            
            #region Event Subscriptions
            #region Campaign List Module
            //m_CampaignListModule.btnWorkOnCompany_OnClick += new ManageCampaignList.btnWorkOnCompanyOnClickEventHandler(m_CampaignList_btnWorkOnCompany_OnClick);
            //m_CampaignListModule.OnAccountModificationInfoChange += new ManageCampaignList.AccountModificationInfoChangeEventhandler(m_CampaignList_AccountModificationInfoChange);
            //m_CampaignListModule.gvCampaignList_OnDoubleClick += new ManageCampaignList.gvCampaignListDoubleClickEventHandler(m_CampaignList_gvCampaignList_OnDoubleClick);
            //m_CampaignListModule.gvCampaignList_OnEnter += new ManageCampaignList.gvCampaignListOnEnterEventHandler(m_CampaignList_gvCampaignList_OnEnter);
            m_CampaignListModule.gvCampaignList_OnFocusedRowChange += new ManageCampaignList.gvCampaignListFocusedRowChangedEventHandler(m_CampaignList_gvCampaignList_OnFocusedRowChange);
            m_CampaignListModule.gvCampaignList_OnColumnFilterChange += new ManageCampaignList.gvCampaignListColumnFilterChangedEventHandler(m_CampaignList_gvCampaignList_OnColumnFilterChange);
            m_CampaignListModule.btnRemoveCompany_OnClick += new ManageCampaignList.btnRemoveCompanyOnClickEventHandler(m_CampaignList_btnRemoveCompany_OnClick);
            m_CampaignListModule.cboSubCampaignList_OnEditValueChange += new ManageCampaignList.cboSubCampaignListEditValueChangedHandler(m_CampaignList_cboSubCampaignList_OnEditValueChange);
            m_CampaignListModule.btnSaveAsNotQualified_OnClick += new ManageCampaignList.btnSaveAsNotQualifiedOnClickEventHandler(m_CampaignList_btnSaveAsNotQualified_OnClick);
            m_CampaignListModule.CampaignList_OnCampaignListEmpty += new ManageCampaignList.CampaignList_OnCampaignListEmptyEventHandler(m_CampaignListModule_CampaignList_OnCampaignListEmpty);
            m_CampaignListModule.btnRefreshSubCampaigns_OnClick += new ManageCampaignList.btnRefreshSubCampaignsOnClickEventHandler(m_CampaignListModule_btnRefreshSubCampaigns_OnClick);
            m_CampaignListModule.CampaignList_btnReleaseLock_OnClick += new ManageCampaignList.CampaignList_btnReleaseLockOnClickEventHandler(m_CampaignListModule_CampaignList_btnReleaseLock_OnClick);
            m_CampaignListModule.HasPendingCallAndLog += new ManageCampaignList.HasPendingCallAndLogEventHandler(m_CampaignListModule_HasPendingCallAndLog);
            //m_CampaignListModule.CampaignList_CompanyOnWorkByAnotherConsultant += new ManageCampaignList.CampaignListCompanyOnWorkByAnotherConsultantEventHandler(m_CampaignListModule_CampaignList_CompanyOnWorkByAnotherConsultant);
            //m_CampaignListModule.CampaignExtraDetail_CallLog_WorkNurtureEvent += new ManageCampaignList.CampaignExtraDetailCallLogWorkNurtureEventEventHandler(m_CampaignListModule_CampaignExtraDetail_CallLog_WorkNurtureEvent);
            #endregion

            #region Campaign Extra Detail
            //m_CampaignListModule.CampaignExtraDetail_OnCompanyInformationSaved += new ManageCampaignList.CampaignExtraDetail_OnCompanyInformationSavedEventHandler(m_CampaignListModule_CampaignExtraDetail_OnCompanyInformationSaved);
            m_CampaignListModule.tcCampaignExtraDetail_OnSelectedPageChange += new ManageCampaignList.tcCampaignExtraDetailOnSelectedPageChangedHandler(m_CampaignExtraDetail_tcCampaignExtraDetail_OnSelectedPageChange);
            m_CampaignListModule.CallLogAfterDelete += new ManageCampaignList.CallLogAfterDeleteEventHandler(m_CampaignListModule_CallLogAfterDelete);
            m_CampaignListModule.CampaignExtraDetail_OnContactSaved += new ManageCampaignList.CampaignExtraDetailOnContactSavedEventHandler(m_CampaignListModule_CampaignExtraDetail_OnContactSaved);
            #endregion
            #endregion

            //m_CampaignList.Dock = DockStyle.Fill;
            //m_CampaignListModule.pnlCampaignList.Controls.Clear();
            //m_CampaignListModule.pnlCampaignList.Controls.Add(m_CampaignList);

            //m_CampaignExtraDetail.Dock = DockStyle.Fill;
            //m_CampaignListModule.pnlCampaignExtraDetail.Controls.Clear();
            //m_CampaignListModule.pnlCampaignExtraDetail.Controls.Add(m_CampaignExtraDetail);

            m_CampaignListModule.Dock = DockStyle.Fill;
            pcCompanyContact.Controls.Clear();
            pcCompanyContact.Controls.Add(m_CampaignListModule);
            WaitDialog.Close();

            //if (m_objFrmManageCompanyContact != null)
            //    return;

            //WaitDialog.Show(this.ParentForm, ("Loading components.");
            //m_objFrmManageCompanyContact = new ManageCompanyContact(this);
            //m_objFrmManageCompanyContact.Dock = DockStyle.Fill;
            //pcCompanyContact.Controls.Clear();
            //pcCompanyContact.Controls.Add(m_objFrmManageCompanyContact);
            //WaitDialog.Close();
        }
        private bool LogoutApplication()
        {
            DialogResult objResult = new DialogResult();
            objResult = MessageBox.Show("Are you sure you want to logout?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.Yes)
                return true;
            else
                return false;
        }
        private void LoadUserLoginForm() 
        {
            PopupDialog objPopupDialog = new PopupDialog();
            m_UserLogin = new UserLogin(objPopupDialog);
            m_UserLogin.AfterLogin += new UserLogin.AfterLoginEventHandler(objFrmUserLogin_AfterLogin);
            objPopupDialog.IsLogin = true;
            objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            objPopupDialog.MinimizeBox = false;
            objPopupDialog.MaximizeBox = false;
            objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            objPopupDialog.Text = "Login to Sales Consultant Application";
            objPopupDialog.Controls.Add(m_UserLogin);
            objPopupDialog.ClientSize = new Size(m_UserLogin.Width + 2, m_UserLogin.Height + 2);
            objPopupDialog.ShowDialog(this.ParentForm);
            objPopupDialog.Controls.Remove(m_UserLogin);
            m_UserLogin.AfterLogin -= new UserLogin.AfterLoginEventHandler(objFrmUserLogin_AfterLogin);
            m_UserLogin.Dispose();
            m_UserLogin = null;
        }
        //private void LoadCampaignBooking(CampaignBookingProperty.CampaignBoookingArguments pParams)
        //{
        //    /**
        //     * close the campaign list tab work mode.
        //     */
        //    if (m_CampaignListModule != null)
        //        m_CampaignListModule.CloseCompany();

        //    if (m_CampaignBookingModule != null) {
        //        m_CampaignBookingModule.SetBreadCrumb(string.Empty);
        //        //m_CampaignBookingModule.SaveCampaignBookingOnWorkAnotherCompany();
        //    }

        //    //barStaticItemCurrentWorkedCompany.Caption = "";
        //    if (pParams.oAppointment == null)
        //        return;

        //    //barStaticItemCurrentWorkedCompany.Caption = 
        //    //string _BreadCrumb = string.Format("{0} ({1})",
        //    //    pParams.CompanyName,
        //    //    ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
        //    //        pParams.oAppointment.FinalListId,
        //    //        pParams.oAppointment.AccountId
        //    //    )
        //    //);

        //        //pParams.CompanyName + " - last updated on: "
        //        //+ ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
        //        //    pParams.oAppointment.FinalListId,
        //        //    pParams.oAppointment.AccountId
        //        //);

        //    this.SetStateCallerBarGroup(true);
        //    m_oCallViewBar.Clear();
        //    m_oCallLogBar.Clear();
        //    m_oFollowUp.Prepare();
        //    this.InitializeCampaignBookingModule();
        //    m_CampaignBookingModule.ResetContactParameters();
        //    m_CampaignBookingModule.oAppointment = pParams.oAppointment;
        //    m_CampaignBookingModule.oAppointment.AccountStatus = new AccountStatus() {
        //        AccountLeadStatuses = pParams.AccountLeadStatuses,
        //        AccountStatuses = pParams.AccountStatuses,
        //        AccountLeadStatusSelectedIndex = pParams.AccountLeadStatusSelectedIndex,
        //        AccountStatusSelectedIndex = pParams.AccountStatusSelectedIndex,
        //        AccountLeadStatusNotQualifiedIndex = pParams.AccountLeadStatusNotQualifiedIndex,
        //        AccountStatusNotQualifiedIndex = pParams.AccountStatusNotQualifiedIndex
        //    };
        //    m_CampaignBookingModule.oAppointment.ContactStatus = new ContactStatus() {
        //        ContactStatuses = pParams.ContactStatuses,
        //        ContactStatusSelectedIndex = pParams.ContactStatusSelectedIndex,
        //        ContactStatusNotQualifiedIndex = pParams.ContactStatusNotQualifiedIndex
        //    };
        //    //m_CampaignBookingModule.oAppointment.AccountStatuses = pParams.AccountStatuses;
        //    //m_CampaignBookingModule.oAppointment.ContactStatuses = pParams.ContactStatuses;
        //    //m_CampaignBookingModule.oAppointment.AccountLeadStatusSelectedIndex = pParams.AccountLeadStatusSelectedIndex;
        //    //m_CampaignBookingModule.oAppointment.AccountStatusSelectedIndex = pParams.AccountStatusSelectedIndex;
        //    //m_CampaignBookingModule.oAppointment.ContactStatusSelectedIndex = pParams.ContactStatusSelectedIndex;
        //    //m_CampaignBookingModule.PrepareStatusCombos();
        //    m_CampaignBookingModule.SelectedCompany = pParams.CompanyName;
        //    m_CampaignBookingModule.SelectedContactId = pParams.ContactId;
        //    m_CampaignBookingModule.SetBreadCrumb(pParams.BreadCrumb);
        //    //m_CampaignBookingModule.Enabled = true;
        //    m_CampaignBookingModule.ParentView = ManageCampaignBooking.eParentView.MyFollowUp;
        //    m_BrightSalesProperty.CampaignBooking.Mode = SelectionProperty.CampaignBookingMode.Work;
        //    m_CampaignBookingModule.InitializeCampaignBooking();
        //    m_CampaignBookingModule.LoadCampaignBookingData();
        //    m_CampaignBookingModule.SetDefaultTab();

        //    m_oFollowUpBar.SetCampaignInfo(pParams.CampaignInfo, pParams.ToolTipInfo);
        //    m_oFollowUpBar.FollowUpId = m_MyFollowUpModule.GetId();

        //    tcSalesConsultant.SelectedTabPage = tabCampaignBooking;
        //}
        private void LoadCampaignListSelection()
        {
            try {
                if (m_CampaignListModule != null) {
                    m_CampaignListModule.SetDoneLoadingCampaignList(false);
                    m_CampaignListModule.InitializeCampaignListButtons(false);
                    m_CampaignListModule.NullifyCampaignListDataSource();
                }

                List<string> _empty_list = new List<string>();
                m_lstSubCampaigns = new List<CTScSubCampaignList>();
                m_lstSubCampaigns = ObjectSubCampaign.GetActiveSubCampaigns(UserSession.CurrentUser.UserId, _empty_list);
                cboCampaignList.Properties.DataSource = null;
                cboCampaignList.Properties.Columns.Clear();
                cboCampaignList.Properties.DataSource = m_lstSubCampaigns;
                cboCampaignList.Properties.DisplayMember = "item";
                cboCampaignList.Properties.ValueMember = "id";
                cboCampaignList.Properties.Columns.Add(new LookUpColumnInfo("item"));
                cboCampaignList.EditValue = null;
                
                if (m_CampaignListModule != null)
                    m_CampaignListModule.SetDoneLoadingCampaignList(true);
            }
            catch (Exception e) {
                m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                    ErrorMessage = e.Message
                });
            }
        }
        private bool TerminateApplication()
        {
            DialogResult objResult = new DialogResult();
            objResult = MessageBox.Show("Are you sure to terminate bright sales application?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.Yes)
                return true;
            else
                return false;
        }
        private void SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage pSelectedPage)
        {
            if (tcSalesConsultant.SelectedTabPage.Equals(tabCampaignList))
            {
                if (m_CampaignListModule != null)
                    m_CampaignListModule.SetCampaignExtraDetailSelectedPage(pSelectedPage);
            }
            else if (tcSalesConsultant.SelectedTabPage.Equals(tabCampaignBooking))
            {
                if (m_CampaignBookingModule != null)
                    m_CampaignBookingModule.SetCampaignExtraDetailSelectedPage(pSelectedPage);
            }
        }
        private void SetStateCallerBarGroup(bool pState)
        {
            //pnlCallLogAndFollowUp.Enabled = pState;
            pnlToggleBar.Enabled = pState;
            m_oCallViewBar.Enabled = pState;
            m_oCallLogBar.Enabled = pState;
            m_oFollowUp.Enabled = pState;
            //m_oFollowUpBar.Enabled = pState;
            m_oCallLogRemarksBar.Enabled = pState;
            btnOpenFollowUpWindow.Enabled = pState;
            if (!pState) {
                m_oFollowUpDialog.Hide();
            }
        }
        private bool CampaignBooking_DialogEditor_OnEditMode()
        {
            if (m_CampaignBookingModule != null)
                return m_CampaignBookingModule.DialogEditor_OnEditMode();

            return false;
        }
        private void RunAssync(Action pAction)
        {
            if (!IsHandleCreated)
                CreateHandle();

            this.Invoke(pAction);
        }
        private void CampaignBookingDataSaved()
        {
            if (m_CampaignBookingModule == null)
                return;

            if (m_CampaignBookingModule.DialogChanged && !m_CampaignBookingModule.StatusChanged)
                NotificationDialog.Warning("Bright Sales", "You saved changes to Dialog without changing Company Status or Lead Status. Please make sure it is correct.");
        }
        private bool CampaignListValueIsValid()
        {
            if (this.CampaignBooking_DialogEditor_OnEditMode())
                return false;

            if (m_CampaignListModule == null)
                return false;

            if (m_BrightSalesProperty.CommonProperty.OnCallMode) {
                NotificationDialog.Information("Bright Sales", "Call is already in progress. Please hang-up and save call log first.");
                return false;
            }
            else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved) {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return false;
            }

            return true;
        }

        private void SetBrightManagerTab()
        {
            //DEFAULT as false
            tabUserSettings.PageEnabled = false;
            tabUserSettings.PageVisible = false;
            tabSubUSCustomerUsers.PageEnabled = false;
            tabSubUSCustomerUsers.PageVisible = false;
            tabSubCampaigns.PageEnabled = false;
            tabSubCampaigns.PageVisible = false;
            tabMasterDataTools.PageEnabled = false;
            tabMasterDataTools.PageVisible = false;
            tabGenerateCampaignList.PageEnabled = false;
            tabGenerateCampaignList.PageVisible = false;
            tabSubGCLCallListCreation.PageEnabled = false;
            tabSubGCLCallListCreation.PageVisible = false;
            tabSubGCLFinalCallList.PageEnabled = false;
            tabSubGCLFinalCallList.PageVisible = false;
            tabManageQuestions.PageEnabled = false;
            tabManageQuestions.PageVisible = false;
            tabManageDialogs.PageEnabled = false;
            tabManageDialogs.PageVisible = false;
            tabSubUSSIPAccounts.PageEnabled = false;
            tabSubUSSIPAccounts.PageVisible = false;
            tabReportConfiguration.PageEnabled = false;
            tabReportConfiguration.PageVisible = false;
            tabViewEventLogs.PageEnabled = false;
            tabViewEventLogs.PageVisible = false;
            tabSubMDTTitles.PageEnabled = false;
            tabSubMDTTitles.PageVisible = false;
            tabSubMDTLanguages.PageEnabled = false;
            tabSubMDTLanguages.PageVisible = false;
            tabSubUSCustomersAndCampaigns.PageEnabled = false;
            tabSubUSCustomersAndCampaigns.PageVisible = false;     
            
            
            //tabCallLogs.PageEnabled = false;
            //tabCallLogs.PageVisible = false;
            //tabViewMessageLogs.PageEnabled = false;
            //tabViewMessageLogs.PageVisible = false;

            if (UserSession.CurrentUser.IsManagerAdmin)
            {
                tabUserSettings.PageEnabled = true;
                tabUserSettings.PageVisible = true;
                tabSubUSCustomerUsers.PageEnabled = true;
                tabSubUSCustomerUsers.PageVisible = true;
                tabSubCampaigns.PageEnabled = true;
                tabSubCampaigns.PageVisible = true;
                tabMasterDataTools.PageEnabled = true;
                tabMasterDataTools.PageVisible = true;
                tabGenerateCampaignList.PageEnabled = true;
                tabGenerateCampaignList.PageVisible = true;
                tabSubGCLCallListCreation.PageEnabled = true;
                tabSubGCLCallListCreation.PageVisible = true;
                tabSubGCLFinalCallList.PageEnabled = true;
                tabSubGCLFinalCallList.PageVisible = true;
                tabManageQuestions.PageEnabled = true;
                tabManageQuestions.PageVisible = true;
                tabManageDialogs.PageEnabled = true;
                tabManageDialogs.PageVisible = true;
                tabSubUSSIPAccounts.PageEnabled = true;
                tabSubUSSIPAccounts.PageVisible = true;
                tabReportConfiguration.PageEnabled = true;
                tabReportConfiguration.PageVisible = true;
                tabViewEventLogs.PageEnabled = true;
                tabViewEventLogs.PageVisible = true;
                tabSubMDTTitles.PageEnabled = true;
                tabSubMDTTitles.PageVisible = true;
                tabSubMDTLanguages.PageEnabled = true;
                tabSubMDTLanguages.PageVisible = true;
                tabSubUSCustomersAndCampaigns.PageEnabled = true;
                tabSubUSCustomersAndCampaigns.PageVisible = true;     
                //tabCallLogs.PageEnabled = true;
                //tabCallLogs.PageVisible = true;
                //tabViewMessageLogs.PageEnabled = true;
                //tabViewMessageLogs.PageVisible = true;
            }
            
            if (UserSession.CurrentUser.IsManagerUser)
            {
                tabSubUSCustomerUsers.PageEnabled = true;
                tabSubUSCustomerUsers.PageVisible = true;
                tabSubCampaigns.PageEnabled = true;
                tabSubCampaigns.PageVisible = true;
                tabMasterDataTools.PageEnabled = true;
                tabMasterDataTools.PageVisible = true;
                tabGenerateCampaignList.PageEnabled = true;
                tabGenerateCampaignList.PageVisible = true;
                tabSubGCLCallListCreation.PageEnabled = true;
                tabSubGCLCallListCreation.PageVisible = true;
                tabSubGCLFinalCallList.PageEnabled = true;
                tabSubGCLFinalCallList.PageVisible = true;
                tabManageQuestions.PageEnabled = true;
                tabManageQuestions.PageVisible = true;
                tabManageDialogs.PageEnabled = true;
                tabManageDialogs.PageVisible = true;
                tabSubUSSIPAccounts.PageEnabled = true;
                tabSubUSSIPAccounts.PageVisible = true;
                tabReportConfiguration.PageEnabled = true;
                tabReportConfiguration.PageVisible = true;
                tabViewEventLogs.PageEnabled = true;
                tabViewEventLogs.PageVisible = true;
                tabSubMDTTitles.PageEnabled = true;
                tabSubMDTTitles.PageVisible = true;
                tabSubMDTLanguages.PageEnabled = true;
                tabSubMDTLanguages.PageVisible = true;
                tabSubUSCustomersAndCampaigns.PageEnabled = true;
                tabSubUSCustomersAndCampaigns.PageVisible = true;      
                //tabCallLogs.PageEnabled = true;
                //tabCallLogs.PageVisible = true;
                //tabViewMessageLogs.PageEnabled = true;
                //tabViewMessageLogs.PageVisible = true;
            }
            
        }
        #endregion

        #region Control Events
        private void cboCampaignList_MouseWheel(object sender, MouseEventArgs e)
        {
            DevExpress.Utils.DXMouseEventArgs.GetMouseArgs(e).Handled = true;
        }
        private void m_HelpInfo_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_HelpInfoShown = false;
        }
        private void m_HelpInfo_Shown(object sender, EventArgs e)
        {
            m_HelpInfoShown = true;
        }
        private void btnHelp_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_HelpInfoShown)
                return;

            m_HelpInfo.ShowDialog(this);
        }

        private void btnRunStopTimeLogger_Click(object sender, EventArgs e)
        {
            if (m_TimerStatus == eTimerStatus.Time_Stopped) {
                btnRunStopTimeLogger.Image = Properties.Resources.stop_time;
                m_TimerStatus = eTimerStatus.Time_Running;
                m_StartTime = DateTime.Now.TimeOfDay;
                timerCallLog.Start();
            }

            else if (m_TimerStatus == eTimerStatus.Time_Running) {
                btnRunStopTimeLogger.Image = Properties.Resources.start_time;
                m_TimerStatus = eTimerStatus.Time_Stopped;
                timerCallLog.Stop();
                m_EndTime = DateTime.Now.TimeOfDay;
                m_TimeElapsed = m_EndTime.Subtract(m_StartTime);
                m_TimeSpent = m_TimeSpent.Add(m_TimeElapsed);
            }
        }
        private void timerCallLog_Tick(object sender, EventArgs e)
        {
            TimeSpan _time = m_TimeSpent;
            m_EndTime = DateTime.Now.TimeOfDay;
            m_TimeElapsed = m_EndTime.Subtract(m_StartTime);
            tbxTimeLogger.Text = string.Format("{0} : {1}",
                m_TimeSpent.Add(m_TimeElapsed).Minutes.ToString().PadLeft(2, '0'),
                m_TimeSpent.Add(m_TimeElapsed).Seconds.ToString().PadLeft(2, '0')
            );
        }
        private void btnRefreshTimeLogger_Click(object sender, EventArgs e)
        {
            m_StartTime = new TimeSpan();
            m_TimeSpent = new TimeSpan();

            if (m_TimerStatus == eTimerStatus.Time_Running)
                m_StartTime = DateTime.Now.TimeOfDay;
            else
                tbxTimeLogger.Text = "00 : 00";
        }

        private void FrmSalesConsultant_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.CampaignBooking_DialogEditor_OnEditMode()) {
                NotificationDialog.Information("Bright Sales", "Dialog is currently on edit mode.");
                e.Cancel = true;
                return;
            }

            if (m_BrightSalesProperty.CommonProperty.OnCallMode) {
                NotificationDialog.Information("Bright Sales", "Call is already in progress. Please hang-up and save call log first.");
                e.Cancel = true;
                return;
            }
            else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved) {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                e.Cancel = true;
                return;
            }

            if (m_CampaignBookingModule != null) {
                this.CampaignBookingDataSaved();
                m_CampaignBookingModule.SaveCampaignBooking();
            }

            if (DoneLoggedIn) {
                if (this.TerminateApplication()) {
                    try
                    {
                        //Stop BrightVision Audio Uploader
                        //((new Utilities.WindowsAzureStorageBlobUtility()).CheckBlobFileExists(followUpId, audioId, IsNew))
                        (new BrightVision.Common.Utilities.WindowsAzureStorageBlobUtility()).ForceCloseBrightVisionAudioUploaderApp();

                        var BPContext = new BrightVision.Model.BrightPlatformEntities(UserSession.EntityConnection);
                        //BPContext.FIRelaseUserLock(UserSession.CurrentUser.UserId);
                        Business.ObjectLocking.ReleaseUserLock();

                        //fileUploadManager.Stop();
                        m_oFollowUpDialog.Close();

                        if (m_oCallLogBar.PhoneRegisterSuccess)
                            m_oCallLogBar.UnRegisterPhone();

                        m_oCallLogBar.KillClockThread();
                        DoneLoggedIn = false;
                        Application.Exit();
                        //Environment.FailFast(string.Empty);
                    }
                    catch (Exception ex) {
                        m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                            ErrorMessage = ex.Message
                        });
                    }
                }
                else
                    e.Cancel = true;
            }
        }
        private void FrmSalesConsultant_Load(object sender, EventArgs e)
        {
            this.AddWorkToQueue(new EventMessage { IsLoadTitle = true });
            //AudioSettingUtility.SetLicense();
            AudioSettingUtility.SetDefaultAudio();
            this.KeyPreview = true;
            System.Threading.Thread.Sleep(4000);
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            barStaticApplication.Caption = "BrightSales";
            barStaticBuildVersion.Caption = "Version: " + curVersion.ToString();
            barStaticEnviroment.Caption = FormUtility.GetConfigSetting("BuildEnvironment");
            this.SetFormControls(false);
            this.LoadUserLoginForm();
            fileUploadManager = BrightSalesFacade.WebDavFile;
            fileUploadManager.Start();
            fileUploadManager.ConnectionFail += new WebDavFileManager.ConnectionFailHandler(fileUploadManager_ConnectionFail);
            //Populate Global Title List to User Session Object            
        }

        private void fileUploadManager_ConnectionFail(string message)
        {
            try {
                this.Invoke(new Action(() => {
                    NotificationDialog.Error("Bright Sales", message);
                }));
            }
            catch {
                NotificationDialog.Error("Bright Sales", message);
            }
        }
        private void barButtonLogout_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (this.CampaignBooking_DialogEditor_OnEditMode())
                return;

            if (m_BrightSalesProperty.CommonProperty.OnCallMode) {
                NotificationDialog.Information("Bright Sales", "Call is already in progress. Please hang-up and save call log first.");
                return;
            }
            else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved) {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return;
            }

            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.SaveCampaignBooking();

            if (!this.LogoutApplication())
                return;

            //m_CampaignListModule.ReleaseCurrentCompanyLock();
            Business.ObjectLocking.ReleaseUserLock();
            this.Hide();
            m_LogoutInitiated = true;
            m_CampaignListModule = null;
            m_MyFollowUpModule = null;
            m_CampaignBookingModule = null;
            m_ReportModule = null;
            pcCampaignBooking.Controls.Clear();
            pnlMyFollowups.Controls.Clear();
            pnlReports.Controls.Clear();

            m_oCallLogBar.EndClock();
            m_oCallLogBar.Default();
            m_oCallLogBar.UnRegisterPhone();
            m_oCallViewBar.PhoneCallEnded();
            m_oCallViewBar.Default();
            m_oFollowUpBar.Clear();
            this.SetStateCallerBarGroup(false);

            //m_oCallLogBar.Default();
            //m_oCallViewBar.Default();
            //m_oCallLogBar.Visible = false;
            //m_oCallViewBar.Visible = true;
            //pnlToggleBar.Enabled = false;
            //btnOpenFollowUpWindow.Enabled = false;
            m_BrightSalesProperty.CommonProperty.SubCampaignId = 0;
            tcSalesConsultant.SelectedTabPage = tabCampaignList;
            this.SetFormControls(false);
            this.LoadUserLoginForm();
            m_LogoutInitiated = false;
        }

        private void barButtonExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (this.CampaignBooking_DialogEditor_OnEditMode())
                return;

            if (m_BrightSalesProperty.CommonProperty.OnCallMode) {
                NotificationDialog.Information("Bright Sales", "Call is already in progress. Please hang-up and save call log first.");
                return;
            }
            else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved)
            {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return;
            }

            //if (m_CampaignBookingModule != null)
            //    m_CampaignBookingModule.SaveCampaignBooking();
            Business.ObjectLocking.ReleaseUserLock();

            this.Close();
        }
        private void btnOpenFollowUpWindow_Click(object sender, EventArgs e)
        {
            if (m_BrightSalesProperty.CommonProperty.OnCallMode)
            {
                NotificationDialog.Information("Bright Sales", "Call is already in progress. Please hang-up and save call log first.");
                return;
            }
            else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved)
            {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return;
            }else if(m_BrightSalesProperty.CommonProperty.CurrentTab == SelectionProperty.CurrentTab.CampaignBooking)
            {
                if (m_CampaignBookingModule.GetCampaignBookingContactList().Count == 0)
                {
                    NotificationDialog.Error("Bright Sales", "No contacts available.");
                    return;
                }
            }

            m_oFollowUpDialog.Text = "New Task";
            m_oFollowUp.GetEventTypes(m_SubCampaignId);
            m_oFollowUp.LoadSelectedContact();
            m_oFollowUpDialog.Hide();
            //m_oFollowUpDialog.Show(this);
            m_oFollowUpDialog.ShowDialog(this);
        }
        private void btnSwitchToggleBarMenu_Click(object sender, EventArgs e)
        {
            if (m_SelectedToggleBarMenu == ToggleBarMenuType.CallLog) {
                m_SelectedToggleBarMenu = ToggleBarMenuType.CallView;
                m_oCallLogBar.Visible = false;
                m_oCallViewBar.Visible = true;
            }
            else if (m_SelectedToggleBarMenu == ToggleBarMenuType.CallView) {
                m_SelectedToggleBarMenu = ToggleBarMenuType.CallLog;
                m_oCallLogBar.Visible = true;
                m_oCallViewBar.Visible = false;
            }
        }
        private void m_tsmiExit_Click(object sender, EventArgs e)
        {
            if (this.CampaignBooking_DialogEditor_OnEditMode())
                return;

            if (m_BrightSalesProperty.CommonProperty.OnCallMode)
            {
                NotificationDialog.Information("Bright Sales", "Call is already in progress. Please hang-up and save call log first.");
                return;
            }
            else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved)
            {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return;
            }

            //if (m_CampaignBookingModule != null)
            //    m_CampaignBookingModule.SaveCampaignBooking();
            Business.ObjectLocking.ReleaseUserLock();

            this.Close();
        }
        private void m_tsmiLogOut_Click(object sender, EventArgs e)
        {
            if (this.CampaignBooking_DialogEditor_OnEditMode()) {
                NotificationDialog.Information("Bright Sales", "Dialog is currently on edit mode.");
                return;
            }

            if (m_BrightSalesProperty.CommonProperty.OnCallMode)
            {
                NotificationDialog.Information("Bright Sales", "Call is already in progress. Please hang-up and save call log first.");
                return;
            }
            else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved)
            {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return;
            }

            if (m_CampaignBookingModule != null) {
                this.CampaignBookingDataSaved();
                m_CampaignBookingModule.SaveCampaignBooking();
            }

            if (!this.LogoutApplication())
                return;

            //m_CampaignListModule.ReleaseCurrentCompanyLock();
            Business.ObjectLocking.ReleaseUserLock();
            this.Hide();
            m_LogoutInitiated = true;
            m_CampaignListModule = null;
            m_MyFollowUpModule = null;
            m_CampaignBookingModule = null;
            m_ReportModule = null;
            pcCampaignBooking.Controls.Clear();
            pnlMyFollowups.Controls.Clear();
            pnlReports.Controls.Clear();
            tcSalesConsultant.SelectedTabPage = tabCampaignList;

            m_oCallLogBar.EndClock();
            m_oCallLogBar.Default();
            m_oCallLogBar.UnRegisterPhone();
            m_oCallViewBar.PhoneCallEnded();
            m_oCallViewBar.Default();
            m_oFollowUpBar.Clear();
            this.SetStateCallerBarGroup(false);

            //m_oCallLogBar.Default();
            //m_oCallViewBar.Default();
            //m_oCallLogBar.Visible = false;
            //m_oCallViewBar.Visible = true;
            //pnlToggleBar.Enabled = false;
            //btnOpenFollowUpWindow.Enabled = false;

            this.SetFormControls(false);
            this.LoadUserLoginForm();
            m_LogoutInitiated = false;
        }
        private void tcSalesConsultant_Selected(object sender, DevExpress.XtraTab.TabPageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Page.Name))
                return;

            tcSalesConsultant.CustomHeaderButtons[0].Enabled = false;
            tcSalesConsultant.CustomHeaderButtons[0].Visible = false;

            if (e.Page.Name.Equals(tabCampaignList.Name) && m_LogoutInitiated) {
                this.InitializeCampaignListModule();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.CampaignList;
            }
            else if(e.Page.Name.Equals(tabCampaignBooking.Name)) {
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.CampaignBooking;
                if (m_CampaignBookingModule != null) {
                    tcSalesConsultant.CustomHeaderButtons[0].Enabled = true;
                    tcSalesConsultant.CustomHeaderButtons[0].Visible = true;
                }
            }
            else if (e.Page.Name.Equals(tabMyFollowUp.Name))
            {
                WaitDialog.Show("Loading tasks ...");
                this.InitializeMyFollowUpModule();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.Events;
                WaitDialog.Close();
            }
            else if (e.Page.Name.Equals(tabHelpWebsite.Name))
            {
                this.InitHelpWebsite();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.Help;
            }
            else if (e.Page.Name.Equals(tabMilltime.Name))
            {
                this.InitMilltime();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.MillTime;
            }
            else if (e.Page.Name.Equals(tabReport.Name))
            {
                this.InitReports();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.Report;
            }
            else if (e.Page.Name.Equals(tabUserSettings.Name))
            {
                this.InitializeInternalUsers();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.InternalUsers;
            }
            else if (e.Page.Name.Equals(tabSubCampaigns.Name))
            {
                this.InitializeSubCampaigns();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.SubCampaigns;
            }
            else if (e.Page.Name.Equals(tabMasterDataTools.Name))
            {
                this.InitializeCompaniesAndContacts();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.CompaniesAndContacts;
            }
            else if (e.Page.Name.Equals(tabGenerateCampaignList.Name))
            {
                this.InitializeImportAndViewList();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.ImportAndViewList;
            }
            else if (e.Page.Name.Equals(tabManageQuestions.Name))
            {
                this.InitializeManageQuestions();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.ManageQuestions;
            }
            else if (e.Page.Name.Equals(tabManageDialogs.Name))
            {
                this.InitializeManageDialogs();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.ManageDialogs;
            }
            else if (e.Page.Name.Equals(tabReportConfiguration.Name))
            {
                this.InitializeReportConfiguration();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.ReportConfiguration;
            }
            else if (e.Page.Name.Equals(tabViewEventLogs.Name))
            {
                this.InitializeViewEventLogs();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.ViewEventLogs;
            }
            else if (e.Page.Name.Equals(tabCallLogs.Name))
            {
                this.InitializeCallLogs();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.Report;
            }
            else if (e.Page.Name.Equals(tabViewMessageLogs.Name))
            {
                this.InitializeViewMessageLogs();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.ViewMessageLogs;
            }
            else if (e.Page.Name.Equals(tabDashboard.Name))
            {
                this.InitializeDashboard();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.Dashboard;
            }
        }
        private void tcSalesConsultant_SelectedPageChanging(object sender, DevExpress.XtraTab.TabPageChangingEventArgs e)
        {
            if (this.CampaignBooking_DialogEditor_OnEditMode()) {
                NotificationDialog.Warning("Bright Sales", "Dialog on edit mode.");
                e.Cancel = true;
                return;
            }
            else if (m_BrightSalesProperty.CommonProperty.OnCallMode)
            {
                NotificationDialog.Warning("Bright Sales", "Currently on call mode.");
                e.Cancel = true;
                return;
            }
            else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved)
            {
                NotificationDialog.Warning("Bright Sales", "Please save your call log first.");
                e.Cancel = true;
                return;
            }

            if (e.PrevPage == tabCampaignBooking && m_CampaignBookingModule != null) {
                if (m_CampaignBookingModule.ShouldSave) {
                    WaitDialog.Show("Checking/saving changes.");
                    this.CampaignBookingDataSaved();
                    m_CampaignBookingModule.SaveCampaignBookingOnWorkAnotherCompany();
                    WaitDialog.Close();
                }
            }
            else if (e.Page == tabCampaignBooking && m_CampaignBookingModule != null) {
                if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Work)
                    this.SetStateCallerBarGroup(true);
            }
        }

        private void tcSubTabs_Selected(object sender, DevExpress.XtraTab.TabPageEventArgs e)
        {
            if (e.Page.Name.Equals(tabSubMDTCompaniesAndContacts.Name))
            {
                this.InitializeCompaniesAndContacts();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.CompaniesAndContacts;
            }
            else if (e.Page.Name.Equals(tabSubMDTMasterDataImport.Name))
            {
                this.InitializeMasterDataImports();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.MasterDataImport;
            }
            else if (e.Page.Name.Equals(tabSubUSInternalUsers.Name))
            {
                this.InitializeInternalUsers();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.InternalUsers;
            }
            else if (e.Page.Name.Equals(tabSubUSCustomerUsers.Name))
            {
                this.InitializeCustomerUsers();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.CustomerUsers;
            }
            else if (e.Page.Name.Equals(tabSubUSSIPAccounts.Name))
            {
                this.InitializeSIPAccounts();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.SIPAccounts;
            }
            else if (e.Page.Name.Equals(tabSubGCLImportAndViewList.Name))
            {
                this.InitializeImportAndViewList();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.ImportAndViewList;
            }
            else if (e.Page.Name.Equals(tabSubGCLCallListCreation.Name))
            {
                this.InitializeCallListCreation();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.CallListCreation;
            }
            else if (e.Page.Name.Equals(tabSubGCLFinalCallList.Name))
            {
                this.InitializeFinalCallLists();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.FinalCallList;
            }
            else if (e.Page.Name.Equals(tabSubMDTTitles.Name))
            {
                this.InitializeTitleSettings();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.TitleSettings;
            }
            else if (e.Page.Name.Equals(tabSubMDTLanguages.Name))
            {
                this.InitializeLanguageSettings();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.LanguagesSettings;
            }
            else if (e.Page.Name.Equals(tabSubUSCustomersAndCampaigns.Name))
            {
                this.InitializeCustomersAndCampaigns();
                m_BrightSalesProperty.CommonProperty.CurrentTab = SelectionProperty.CurrentTab.CustomersAndCampaigns;
            }
        }

        private void cboCampaignList_EditValueChanged(object sender, EventArgs e)
        {
            if (cboCampaignList.Text.Length < 1 || m_CampaignListModule == null || cboCampaignList.EditValue == null)
                return;

            WaitDialog.Show("Loading data ...");
            string cboCampaignListEditValueToString = cboCampaignList.EditValue.ToString();
            string[] _ids = cboCampaignListEditValueToString.Split(';');
            m_oCallLogBar.EndClock();
            m_oCallLogBar.Default();
            m_oCallViewBar.PhoneCallEnded();
            m_oCallViewBar.Default();
            this.SetStateCallerBarGroup(false);
            pcCampaignBooking.Controls.Clear();
            m_CampaignBookingModule = null;

            if (m_CampaignListModule != null) {
                string[] _customer = _ids[0].Split('|');
                string[] _campaign = _ids[1].Split('|');
                string[] _sub_campaign = _ids[2].Split('|');

                m_CustomerId = Convert.ToInt32(_customer[0]);
                m_CustomerName = _customer[1].ToString();
                m_CampaignId = Convert.ToInt32(_campaign[0]);
                m_CampaignName = _campaign[1].ToString();
                m_SubCampaignId = Convert.ToInt32(_sub_campaign[0]);
                m_SubCampaignName = _sub_campaign[1].ToString();

                m_BrightSalesProperty.CommonProperty.CustomerId = m_CustomerId;
                m_BrightSalesProperty.CommonProperty.CustomerName = m_CustomerName;
                m_BrightSalesProperty.CommonProperty.CampaignId = m_CampaignId;
                m_BrightSalesProperty.CommonProperty.CampaignName = m_CampaignName;
                m_BrightSalesProperty.CommonProperty.SubCampaignId = m_SubCampaignId;
                m_BrightSalesProperty.CommonProperty.SubCampaignName = m_SubCampaignName;

                m_CampaignListModule.LoadSelectedCampaign(cboCampaignListEditValueToString);
                if (m_CampaignBookingModule != null) {
                    WaitDialog.Close();
                    WaitDialog.Show("Checking/saving for changes.");
                    this.CampaignBookingDataSaved();
                    m_CampaignBookingModule.SaveCampaignBookingOnWorkAnotherCompany();
                    WaitDialog.Close();
                    WaitDialog.Show("Loading data ...");
                }

                m_oFollowUp.SubCampaignId = m_SubCampaignId;
                m_oFollowUp.Prepare();
                m_oFollowUp.LoadSalesUsers();
                m_oFollowUp.SetCampaignInfo(m_CustomerName, m_CampaignName, m_SubCampaignName);

                if (m_MyFollowUpModule != null)
                    m_MyFollowUpModule.SetCampaignInfo(m_CampaignName);

                m_BrightSalesProperty.CommonProperty.IsEmptyCampaignList = true;
                if (m_CampaignListModule.CampaignListHasRows())
                    m_BrightSalesProperty.CommonProperty.IsEmptyCampaignList = false;

                //m_EventBus.Notify(new CampaignListEvents.GetLatestProperties() { ForWorkModePurpose = false });
                if (m_OutsideModuleCall) {
                    m_OutsideModuleCall = false;
                    if (m_CampaignListModule != null) {
                        m_CampaignListModule.CompanySetFocus(m_LoadSpecificDataAccountId);
                        m_EventBus.Notify(new CampaignListEvents.GetLatestProperties() { ForWorkModePurpose = false });
                        m_CampaignListModule.WorkCompanyOnCampaignListLoad();
                        m_EventBus.Notify(new FrmSalesConsultantEvents.OnSubCampaignChange());
                    }

                    if (m_CampaignBookingModule != null)
                        m_CampaignBookingModule.ContactSetFocus(m_LoadSpecificDataContactId);
                }
                else if (!m_BrightSalesProperty.CommonProperty.IsEmptyCampaignList && tcSalesConsultant.SelectedTabPage == tabCampaignBooking) {
                    m_EventBus.Notify(new CampaignListEvents.GetLatestProperties() { ForWorkModePurpose = false });
                    m_CampaignListModule.WorkCompanyOnCampaignListLoad();
                    m_EventBus.Notify(new FrmSalesConsultantEvents.OnSubCampaignChange());
                }
                else
                    m_EventBus.Notify(new FrmSalesConsultantEvents.TryFocusRowChange());

                if (tcSalesConsultant.SelectedTabPage.Name.Equals(tabCallLogs.Name))
                {
                    this.InitializeCallLogs();
                }
                else if (tcSalesConsultant.SelectedTabPage.Name.Equals(tabViewMessageLogs.Name))
                {
                    this.InitializeViewMessageLogs();
                }
                else if (tcSalesConsultant.SelectedTabPage.Name.Equals(tabMyFollowUp.Name))
                {
                    //this.InitializeViewMessageLogs();
                    m_MyFollowUpModule.UpdateGrid();
                }
                else if (tcSalesConsultant.SelectedTabPage.Name.Equals(tabDashboard.Name))
                {
                    this.InitializeDashboard();
                    
                }

                /** /
                CampaignList.CampaignListArgs _args = m_CampaignListModule.GetCampaignListArgs();
                if (m_CampaignListModule.CampaignListHasRows())
                {
                    m_EventBus.Notify(new SalesConsultantSubCampaignChangeEventNotifier());
                    //m_CampaignListModule.SubCampaignList_OnEditvalueChanged(sender, _args);
                    if (tcSalesConsultant.SelectedTabPage == tabCampaignBooking)
                        m_CampaignListModule.WorkCompanyOnCampaignListLoad();
                }
                else
                {
                    //_args.IsEmptyList = true;
                    m_EventBus.Notify(new SalesConsultantSubCampaignChangeEventNotifier());
                    //m_CampaignListModule.SubCampaignList_OnEditvalueChanged(sender, _args);
                    //m_CampaignListModule.SubCampaignList_OnEditvalueChanged(sender, m_CampaignListModule.GetCampaignListArgs());
                }
                /**/
            }
            WaitDialog.Close();
        }
        private void cboCampaignList_EditValueChanging(object sender, ChangingEventArgs e)
        {
            if (cboCampaignList.IsPopupOpen)
                e.Cancel = true;

            if (!this.CampaignListValueIsValid())
                e.Cancel = true;
            
            //if (this.CampaignBooking_DialogEditor_OnEditMode()) {
            //    e.Cancel = true;
            //    return;
            //}

            //if (m_CampaignListModule == null)
            //    e.Cancel = true;

            //if (m_BrightSalesProperty.CommonProperty.OnCallMode) {
            //    NotificationDialog.Information("Bright Sales", "Call is already in progress. Please hang-up and save call log first.");
            //    e.Cancel = true;
            //}
            //else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved) {
            //    NotificationDialog.Information("Bright Sales", "Please kindly save your call log first.");
            //    e.Cancel = true;
            //}
        }
        
        private void barButtonItemPhoneSettings_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            PopupDialog dialog = new PopupDialog(new AudioSettings(), "Phone Settings");
            dialog.ShowDialog(this);
        }
        private void barButtonItemRefreshCampaignLists_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (this.CampaignBooking_DialogEditor_OnEditMode())
                return;

            if (m_CampaignListModule == null)
                return;

            //if (m_CampaignListModule.UserOnWorkMode()) {
            //    MessageBox.Show("You are currently working on a company. Please kindly close it first before refreshing the dropdown list.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}
            //else 

            if (m_BrightSalesProperty.CommonProperty.OnCallMode)
            {
                NotificationDialog.Information("Bright Sales", "Call is already in progress. Please hang-up and save call log first.");
                return;
            }
            else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved)
            {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return;
            }

            DialogResult _dlg = MessageBox.Show("Are you sure to refresh the dropdown list? This will close any subcampains you are working on.", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlg == DialogResult.No)
                return;

            WaitDialog.Show(this.ParentForm, "Refreshing list...");
            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.SaveCampaignBookingOnWorkAnotherCompany();
            //m_RefreshSubCampaignList = true;
            //m_MyFollowUpModule = null;
            m_CampaignBookingModule = null;
            pcCampaignBooking.Controls.Clear();
            //pnlMyFollowups.Controls.Clear();
            barStaticItemCurrentWorkedCompany.Caption = "";

            m_oCallLogBar.EndClock();
            m_oCallLogBar.Default();
            m_oCallViewBar.PhoneCallEnded();
            m_oCallViewBar.Default();
            this.SetStateCallerBarGroup(false);

            this.LoadCampaignListSelection();
            this.SetFormControls(true);
            this.Focus();

            if (m_CampaignListModule != null)
                m_CampaignListModule.SetExtraDetailModuleAsReadOnly(false);

            //DAN: Reset variable value as when on call logs or events tab, it still show results even though no subcampaign select which is needed by the modules
            m_BrightSalesProperty.CommonProperty.SubCampaignId = 0;
            if (tcSalesConsultant.SelectedTabPage.Name.Equals(tabCallLogs.Name))
            {
                this.InitializeCallLogs();
            }
            else if (tcSalesConsultant.SelectedTabPage.Name.Equals(tabViewMessageLogs.Name))
            {
                this.InitializeViewMessageLogs();
            }
            else if (tcSalesConsultant.SelectedTabPage.Name.Equals(tabMyFollowUp.Name))
            {
                m_MyFollowUpModule.UpdateGrid();
            }

            //m_RefreshSubCampaignList = false;
            WaitDialog.Close();
        }

        private void btnLock_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            m_frmUnlock = new Unlock() {
                StartPosition = FormStartPosition.CenterParent
            };
            m_frmUnlock.InvalidLogin -= m_frmUnlock_InvalidLogin;
            m_frmUnlock.InvalidLogin += m_frmUnlock_InvalidLogin;
            m_frmUnlock.ShowDialog(this);
        }
        private void btnChangePassword_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (UserSession.CurrentUser.UserId <= 0)
            {
                NotificationDialog.Error("Bright Sales", "You haven't yet log in properly.");
                return;
            }

            m_frmChangePassword = new ChangePassword() {
                StartPosition = FormStartPosition.CenterParent
            };
            m_frmChangePassword.InvalidOldPassword -= m_frmChangePassword_InvalidOldPassword;
            m_frmChangePassword.InvalidOldPassword += m_frmChangePassword_InvalidOldPassword;
            m_frmChangePassword.PasswordsDoesNotMatch -= m_frmChangePassword_PasswordsDoesNotMatch;
            m_frmChangePassword.PasswordsDoesNotMatch += m_frmChangePassword_PasswordsDoesNotMatch;
            m_frmChangePassword.AfterSave -= m_frmChangePassword_AfterSave;
            m_frmChangePassword.AfterSave += m_frmChangePassword_AfterSave;
            m_frmChangePassword.PasswordsMustBeDifferent -= m_frmChangePassword_PasswordsMustBeDifferent;
            m_frmChangePassword.PasswordsMustBeDifferent += m_frmChangePassword_PasswordsMustBeDifferent;
            m_frmChangePassword.ShowDialog(this);
        }

        private void tcSalesConsultant_CustomHeaderButtonClick(object sender, DevExpress.XtraTab.ViewInfo.CustomHeaderButtonEventArgs e)
        {
            if (e.Button.Caption == "Unhide All")
            {
                if (m_CampaignBookingModule != null)
                {
                    m_CampaignBookingModule.m_oEventFollowupLog_UnHideAll();
                }
            }
        }
        #endregion

        #region Subscribed Events
        #region Others
        //private CampaignList.eCampaignListMode m_CampaignBookingModule_GetCampaignListViewMode()
        //{
        //    return m_CampaignListModule.GetCampaignListMode();
        //}
        private void m_frmUnlock_InvalidLogin()
        {
            NotificationDialog.Error("Bright Sales", "Invalid unlock password.");
        }
        private void m_frmChangePassword_AfterSave()
        {
            NotificationDialog.Information("Bright Sales", "Your password has been successfully updated.");
        }
        private void m_frmChangePassword_PasswordsDoesNotMatch()
        {
            NotificationDialog.Error("Bright Sales", "Your new passwords does not match to each other or did not meet the minimum of 6 characters.");
        }
        private void m_frmChangePassword_InvalidOldPassword()
        {
            NotificationDialog.Error("Bright Sales", "Invalid old password.");
        }
        private void m_frmChangePassword_PasswordsMustBeDifferent()
        {
            NotificationDialog.Information("Bright Sales", "New password must be different from the old password.");
        }
        #endregion

        #region My Follow Ups
        //private int m_MyFollowUpModule_GetSelectedCampaignInfo()
        //{
        //    if (m_SubCampaignId > 0)
        //        return m_SubCampaignId;
        //    else
        //        NotificationDialog.Warning("Bright Sales", "No selected sub-campaign from campaign list combo.");

        //    return 0;
        //}
        //private void m_MyFollowUpModule_UserNotMemberOfSubCampaign()
        //{
        //    NotificationDialog.Warning("Bright Sales", string.Format("You are not a member of this sub-campaign.{0}Please contact your administrator.", Environment.NewLine));
        //}
        //private bool m_MyFollowUpModule_CanWorkOnCompany()
        //{
        //    if (m_BrightSalesProperty.CommonProperty.OnCallMode || !m_BrightSalesProperty.CommonProperty.CallLogSaved)
        //        return false;

        //    return true;
        //}
        //private void m_MyFollowUpModule_OnLoadCampaignBooking(object sender, MyFollowUps.CampaignBookingArgs e)
        //private void m_MyFollowUpModule_OnLoadCampaignBooking(object sender, CampaignBookingProperty.CampaignBoookingArguments e)
        //{
        //    string _CustomerName = string.Empty;
        //    string _CampaignName = string.Empty;
        //    string _SubCampaignName = string.Empty;
        //    m_BrightSalesProperty.CampaignBooking.ParentView = SelectionProperty.ParentView.CampaignList; // this is because next and previous button is only for campaign list. when this is loaded, there are campaign list items anyway.
        //    //m_BrightSalesProperty.CampaignBooking.ParentView = SelectionProperty.ParentView.MyFollowUp;
        //    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
        //        _CustomerName = _efDbContext.customers.FirstOrDefault(i => i.id == e.oAppointment.CustomerId).customer_name;
        //        _CampaignName = _efDbContext.campaigns.FirstOrDefault(i => i.id == e.oAppointment.CampaignId).campaign_name;
        //        _SubCampaignName = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == e.oAppointment.SubCampaignId).title;
        //    }
        //    m_oFollowUp.SubCampaignId = e.oAppointment.SubCampaignId;
        //    m_oFollowUp.Prepare();
        //    m_oFollowUp.LoadSalesUsers();
        //    m_oFollowUp.SetCampaignInfo(_CustomerName, _CampaignName, _SubCampaignName);
        //    this.LoadSpecificData(e);
        //    m_BrightSalesProperty.CommonProperty.IsLoadEventLog = false;
        //}
        private void m_MyFollowUpModule_SetCompanyModificationInfo(string pModificationInfo)
        {
            barStaticItemCurrentWorkedCompany.Caption = pModificationInfo;
        }
        #endregion

        #region Campaign Booking
        private void m_CampaignBookingModule_DialogOnEditMode()
        {
            NotificationDialog.Error("Bright Sales", "Cannot change contact. Dialog is on edit mode.");
        }
        //private void m_CampaignBookingModule_EventFollowupLog_WorkNurtureEvent(MyFollowUps.CampaignBookingArgs e)
        private void m_CampaignBookingModule_EventFollowupLog_WorkNurtureEvent(CampaignBookingProperty.CampaignBoookingArguments e)
        {
            if (e == null)
                return;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_account_lists _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                    i.account_id == e.oAppointment.AccountId &&
                    i.final_list_id == e.oAppointment.FinalListId
                );
                if (_item != null)
                    _efDbContext.Detach(_item);

                if (_item.locked && _item.locked_by != UserSession.CurrentUser.UserId) {
                    user _user = _efDbContext.users.FirstOrDefault(i => i.id == _item.locked_by);
                    if (_user != null)
                        _efDbContext.Detach(_user);

                    NotificationDialog.Error("Bright Sales", string.Format("You cannot work on this follow up.{0}This follow up is currently worked by{0}{1}", Environment.NewLine, _user.fullname));
                    return;
                }
            }

            this.LoadSpecificData(e);
        }
        //private bool m_CampaignBookingModule_EventFollowupLog_CanWorkOnCompany()
        //{
        //    if (this.CampaignBooking_DialogEditor_OnEditMode()) {
        //        NotificationDialog.Warning("Bright Sales", "Dialog on edit mode.");
        //        return false;
        //    }
        //    else if (m_BrightSalesProperty.CommonProperty.OnCallMode)
        //    {
        //        NotificationDialog.Warning("Bright Sales", "Currently on call mode.");
        //        return false;
        //    }
        //    else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved)
        //    {
        //        NotificationDialog.Warning("Bright Sales", "Please save your call log first.");
        //        return false;
        //    }

        //    return true;
        //}
        //private bool m_CampaignBookingModule_EventFollowupLog_UserOnCallOrHasPendingCallLog()
        //{
        //    if (m_BrightSalesProperty.CommonProperty.OnCallMode || !m_BrightSalesProperty.CommonProperty.CallLogSaved) {
        //        NotificationDialog.Information("Bright Sales", "A call is currently in progress or a pending call log needs to be saved first.");
        //        return true;
        //    }

        //    return false;
        //}
        //public void m_CampaignBookingModule_EventFollowupLog_EventLog_OnSave(FollowUpEditor.btnSaveOnClickArgs e)
        //{
        //    if (m_CampaignListModule != null)
        //        m_CampaignListModule.ReloadCallLogTab();
        //}

        //private void m_CampaignBookingModule_OnSave(object sender, ManageCampaignBooking.CampaignBookingArgs e)
        //{
        //    this.CampaignBookingDataSaved();
        //    this.SetStateCallerBarGroup(false);

        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList) {
        //        //m_CampaignListModule.SaveCompanyAppointment(e.CampaignBookingAppointment);
        //        m_CampaignListModule.SetCompanyDetails(e.CampaignBookingAppointment.AccountId);
        //        m_CampaignListModule.ReleaseCurrentCompanyLock();
        //    }
        //    else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp) {
        //        m_MyFollowUpModule.SaveCompanyAppointment(e.CampaignBookingAppointment);
        //        m_MyFollowUpModule.ReleaseCurrentCompanyLock();
        //    }

        //    m_oCallLogBar.EndClock();
        //    m_oCallLogBar.Default();
        //    m_oCallViewBar.PhoneCallEnded();
        //    m_oCallViewBar.Default();
        //}

        //private bool m_CampaignBookingModule_CallLogBar_IsCallLogSaved()
        //{
        //    return m_BrightSalesProperty.CallLogSaved;
        //}
        //private bool m_CampaignBookingModule_SalesUser_OnCallMode()
        //{
        //    if (m_BrightSalesProperty.OnCallMode)
        //        return true;

        //    return false;
        //}

        private void m_CampaignBookingModule_ContactView_FocusedRowChanged(object sender, ManageCampaignBooking.ContactViewFocusedRowChangedArgs e)
        {
            if (!m_BrightSalesProperty.CommonProperty.OnCallMode)
            {
                m_oCallViewBar.ContactPerson = e.ContactPerson;
                m_oFollowUp.ContactPerson = e.ContactPerson;

                if (m_oCallLogRemarksBar.Visible == false) {
                    m_oCallLogBar.ContactPerson = e.ContactPerson;
                    m_oCallLogBar.Default();
                }
                m_oCallViewBar.SetContactNumbers();
                m_oFollowUp.LoadSelectedContact();

                if (e.ContactPerson != null)
                    btnOpenFollowUpWindow.Enabled = true;
                else
                    btnOpenFollowUpWindow.Enabled = false;
            }
        }
        private void m_CampaignBookingModule_ContactView_OnContactSaved(object sender, ManageCampaignBooking.ContactViewOnContactSavedArgs e)
        {
            /*
             * https://docs.google.com/a/myndconsulting.com/spreadsheet/ccc?key=0Ah8Xvlc0xaJKdG1CbEJEbWdYTkdPanpqeHJWNGtmYXc#gid=0
             */
            //this.SaveCompanyAppointment();

            m_oFollowUp.UpdateContactPerson(e.ContactPerson);
            m_oFollowUp.ContactPerson = e.ContactPerson;
            m_oCallViewBar.ContactPerson = e.ContactPerson;
            m_oCallLogBar.ContactPerson = e.ContactPerson;
            if (!m_BrightSalesProperty.CommonProperty.OnCallMode)
                m_oCallViewBar.SetContactNumbers();

            //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
            if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList)
                m_CampaignListModule.UpdateContactData(e.ContactPerson.id);
        }
        private void m_CampaignBookingModule_CampaignExtraDetail_OnContactSaved(object sender, ManageCampaignBooking.CampaignExtraDetailOnContactSavedArgs e)
        {
            /*
             * https://docs.google.com/a/myndconsulting.com/spreadsheet/ccc?key=0Ah8Xvlc0xaJKdG1CbEJEbWdYTkdPanpqeHJWNGtmYXc#gid=0
             */
            //this.SaveCompanyAppointment();


            m_oFollowUp.UpdateContactPerson(e.ContactPerson);
            m_oFollowUp.ContactPerson = e.ContactPerson;
            m_oCallViewBar.ContactPerson = e.ContactPerson;
            m_oCallLogBar.ContactPerson = e.ContactPerson;
            if (!m_BrightSalesProperty.CommonProperty.OnCallMode)
                m_oCallViewBar.SetContactNumbers();

            //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
            if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList)
                m_CampaignListModule.UpdateContactData(e.ContactPerson.id);

            //m_oFollowUp.UpdateContactPerson(e.ContactPerson);
            //m_oCallViewBar.ContactPerson = e.ContactPerson;
            //m_oCallViewBar.SetContactNumbers();

            //if (!string.IsNullOrEmpty(e.ContactPerson.direct_phone))
            //    m_oCallViewBar.SetStateCallDirect = true;
            //else
            //    m_oCallViewBar.SetStateCallDirect = false;

            //if (!string.IsNullOrEmpty(e.ContactPerson.mobile))
            //    m_oCallViewBar.SetStateCallMoBile = true;
            //else
            //    m_oCallViewBar.SetStateCallMoBile = false;
        }
        //private void m_CampaignBookingModule_LoadCampaignBookingData_Initiated(object sender, ManageCampaignBooking.LoadCampaignBookingDataArgs e)
        //{
        //    m_oCallViewBar.AccountId = e.CampaignBookingArgs.CampaignBookingAppointment.AccountId;
        //    m_oCallViewBar.SubCampaignId = e.CampaignBookingArgs.CampaignBookingAppointment.SubCampaignId;
        //    m_oCallViewBar.FinalListId = e.CampaignBookingArgs.CampaignBookingAppointment.FinalListId;
        //    m_oCallViewBar.CompanyBoardNo = e.CampaignBookingArgs.CampaignBookingAppointment.CompanyBoardNumber;
        //    m_oCallViewBar.ContactPerson = e.ContactPerson;
        //    m_oCallViewBar.Default();
        //    m_oCallViewBar.SetContactNumbers();

        //    m_oCallLogBar.AccountId = e.CampaignBookingArgs.CampaignBookingAppointment.AccountId;
        //    m_oCallLogBar.SubCampaignId = e.CampaignBookingArgs.CampaignBookingAppointment.SubCampaignId;
        //    m_oCallLogBar.Default();

        //    m_oFollowUp.AccountId = e.CampaignBookingArgs.CampaignBookingAppointment.AccountId;
        //    m_oFollowUp.SetCompanyInfo();
        //}
        private void m_CampaignBookingModule_oContactView_PopulateContactView_Initiated(object sender, ManageCampaignBooking.ContactViewPopulateContactView e)
        {
            m_oFollowUp.LoadContactPersons(e.ContactPersons);
            if (e.ContactPersons.Count > 0) {
                m_oCallViewBar.ContactPerson = e.ContactPersons[e.SelectedContactPersonIndex];
                if (!m_BrightSalesProperty.CommonProperty.OnCallMode)
                    m_oCallViewBar.SetContactNumbers();
            }
        }
        //private void m_CampaignBookingModule_EventFollowupLog_CompanyRemark_Saved(object sender, EventFollowUpLog.CompanyRemarkSavedArgs e)
        //{
        //    this.SaveCompanyAppointment();
        //    //if (m_CampaignListModule != null) {
        //    //    if (m_CampaignListModule.CampaignExtraDetail_CompanyInformation_AccountId == e.AccountId && m_CampaignListModule.CampaignExtraDetail_CompanyInformation_FinalListId == e.FinalListId)
        //    //        m_CampaignListModule.SetCompanyRemarks(e.CompanyRemarks);
        //    //}
        //}

        //private void m_CampaignBookingModule_btnSave_OnClick(object sender, ManageCampaignBooking.CampaignBookingArgs e)
        //{
             //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList) {
             //    m_CampaignListModule.SaveCompanyAppointment(e.CampaignBookingAppointment);
             //    m_CampaignListModule.SetCompanyDetails(e.CampaignBookingAppointment.AccountId);
             //    m_CampaignListModule.ReleaseCurrentCompanyLock();
             //}
             //else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp) {
             //    m_MyFollowUpModule.SaveCompanyAppointment(e.CampaignBookingAppointment);
             //    m_MyFollowUpModule.ReleaseCurrentCompanyLock();
             //}
        //}
        //private void m_CampaignBookingModule_btnBrowse_OnClick(object sender, ManageCampaignBooking.CampaignBookingArgs e)
        //{
        //}
        //private void m_CampaignBookingModule_btnClose_OnClick(object sender, ManageCampaignBooking.CampaignBookingArgs e)
        //{
            //m_oCallLogBar.EndClock();
            //m_oCallLogBar.Default();
            //m_oCallViewBar.PhoneCallEnded();
            //m_oCallViewBar.Default();
            //this.SetStateCallerBarGroup(false);
            //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
            //    this.tcSalesConsultant.SelectedTabPage = tabCampaignList;
            //else
            //    this.tcSalesConsultant.SelectedTabPage = tabMyFollowUp;

            //if (ParentView == eParentView.CampaignList)
            //    CompanyContactView.ShowCompaniesAndContacts();
            //else if (ParentView == eParentView.MyFollowUp)
            //    MyFollowUpView.ShowMyFollwUps();
        //}
        //private void m_CampaignBookingModule_btnCancel_OnClick(object sender, EventArgs e)
        //{
            //this.SetStateCallerBarGroup(false);
        //}
        //private bool m_CampaignBookingModule_btnPreviousRecord_OnClick(object sender, ManageCampaignBooking.CampaignBookingArgs e)
        //{
        //    bool _HasData = false;
        //    m_CompanyWorkedByAnotherConsultant = false;
        //    m_CampaignBookingModule.CompanyWorkedByAnotherConsultant = false;

        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //        _HasData = m_CampaignListModule.MovePrevious();

        //    if (!m_CompanyWorkedByAnotherConsultant && _HasData)
        //        this.SetStateCallerBarGroup(true);
        //    else
        //        this.SetStateCallerBarGroup(false);

        //    return _HasData;

        //    /** /
        //    bool _HasData = false;
        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //        _HasData = m_CampaignListModule.MovePrevious();
        //    else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp)
        //        _HasData = m_MyFollowUpModule.MovePrevious();

        //    if (_HasData)
        //        this.SetStateCallerBarGroup(true);

        //    return _HasData;
        //    /**/

        //    /** /
        //        if (ParentView == eParentView.CampaignList) {
        //            CompanyContactView.MoveToPreviousCompanyRecord();
        //            this.SetNextAndPreviousButtons();
        //            m_oD:\BRIGHT VISION PROJECTS\Projects\SalesConsultant\Properties\AssemblyInfo.csContactView.lvContactEdit.OptionsBehavior.ReadOnly = true;
        //            //this.EditCampaignBooking(false);
        //            //if (CompanyContactView.IsFirstRow)
        //            //    cmdPreviousRecord.Enabled = false;
        //            //else
        //            //    cmdPreviousRecord.Enabled = true;
        //        } else if (ParentView == eParentView.MyFollowUp) {
        //            MyFollowUpView.MoveToPreviousCompanyRecord();
        //            this.SetNextAndPreviousButtons();
        //            m_oContactView.lvContactEdit.OptionsBehavior.ReadOnly = true;
        //            //this.EditCampaignBooking(false);
        //            //if (MyFollowUpView.IsFirstRow)
        //            //    cmdPreviousRecord.Enabled = false;
        //            //else
        //            //    cmdPreviousRecord.Enabled = true;
        //        }
        //    /**/
        //}
        //private bool m_CampaignBookingModule_btnNextRecord_OnClick(object sender, ManageCampaignBooking.CampaignBookingArgs e)
        //{
        //    bool _HasData = false;
        //    m_CompanyWorkedByAnotherConsultant = false;
        //    m_CampaignBookingModule.CompanyWorkedByAnotherConsultant = false;

        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //        _HasData = m_CampaignListModule.MoveNext();

        //    if (!m_CompanyWorkedByAnotherConsultant && _HasData)
        //        this.SetStateCallerBarGroup(true);
        //    else
        //        this.SetStateCallerBarGroup(false);

        //    return _HasData;

        //    /** /
        //    bool _HasData = false;
        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //        _HasData = m_CampaignListModule.MoveNext();
        //    else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp)
        //        _HasData = m_MyFollowUpModule.MoveNext();

        //    if (_HasData)
        //        this.SetStateCallerBarGroup(true);

        //    return _HasData;
        //    /**/
        //}
        //private void m_CampaignBookingModule_btnWork_OnClick(object sender, ManageCampaignBooking.CampaignBookingArgs e)
        //{
        //    this.SetStateCallerBarGroup(true);
        //    m_oCallViewBar.SetContactNumbers();
            //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
            //{
            //    if (m_CampaignListModule.CanWorkCompany())
            //        this
            //}
            //else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp)
            //{ 
            //}

            //if (ParentView == eParentView.CampaignList)
            //{
            //    if (CompanyContactView.CanWorkCompany())
            //        this.WorkOnSelectedCompany();
            //}
            //else if (ParentView == eParentView.MyFollowUp)
            //{
            //    if (MyFollowUpView.CanWorkCompany())
            //        this.WorkOnSelectedCompany();
            //}
        //}
        //private bool m_CampaignBookingModule_CanWorkOnCompany()
        //{
        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //        return m_CampaignListModule.CanWorkCompany();
        //    else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp)
        //        return m_MyFollowUpModule.CanWorkCompany();
        //    else
        //        return false;
        //}
        //private bool m_CampaignBookingModule_CampaignListItemIsLastRow()
        //{
        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //        return m_CampaignListModule.IsLastRow();
        //    else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp)
        //        return m_MyFollowUpModule.IsLastRow;
        //    else
        //        return false;
        //}
        //private bool m_CampaignBookingModule_CampaignListItemIsFirstRow()
        //{
        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //        return m_CampaignListModule.IsFirstRow();
        //    else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp)
        //        return m_MyFollowUpModule.IsFirstRow;
        //    else
        //        return false;
        //}
        private bool m_CampaignBookingModule_MyFollowUpIsLastRow()
        {
            return m_MyFollowUpModule.IsLastRow;
        }
        private bool m_CampaignBookingModule_MyFollowUpIsFirstRow()
        {
            return m_MyFollowUpModule.IsFirstRow;
        }
        //private CampaignList.CampaignListArgs m_CampaignBookingModule_GetCampaignListArgs()
        //{
        //    return m_CampaignListModule.GetCampaignListArgs();
        //}
        //private CampaignBookingProperty.CampaignBoookingArguments m_CampaignBookingModule_GetMyFollowUpCampaignBookingData()
        //{
        //    return m_MyFollowUpModule.GetCampaignBookingArgs();
        //}
        //private void m_CampaignBookingModule_OnCompanyInformationSaved(object sender, CompanyInformation.CompanyInformationArgs e)
        //{
        //    this.SaveCompanyAppointment();
        //    if (!m_BrightSalesProperty.CommonProperty.OnCallMode)
        //    {
        //        m_oCallViewBar.SetContactNumbers();
        //        if (!string.IsNullOrEmpty(e.Account.telephone) && !string.IsNullOrEmpty(e.Account.telephone.Trim()))
        //            m_oCallViewBar.CompanyBoardNo = e.Account.telephone.Trim();
        //        else
        //            m_oCallViewBar.CompanyBoardNo = string.Empty;
        //    }
            
        //    m_oCallViewBar.CompanyBoardNo = e.Account.telephone;

        //    if (!m_BrightSalesProperty.CommonProperty.OnCallMode)
        //        m_oCallViewBar.SetContactNumbers();
            
        //    m_oFollowUp.SetCompanyInfo();
        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList) {
        //        m_CampaignListModule.UpdateSelectedCompany(e);
        //        m_CampaignListModule.SetCompanyDetails(e.Account.id);
        //    }
        //}
        //private void m_CampaignBookingModule_OnCompanyInformationRefresh(object sender, CompanyInformation.CompanyInformationArgs e)
        //{
        //    if (!m_BrightSalesProperty.CommonProperty.OnCallMode)
        //    {
        //        m_oCallViewBar.SetContactNumbers();
        //        if (!string.IsNullOrEmpty(e.Account.telephone) && !string.IsNullOrEmpty(e.Account.telephone.Trim()))
        //            m_oCallViewBar.CompanyBoardNo = e.Account.telephone.Trim();
        //        else
        //            m_oCallViewBar.CompanyBoardNo = string.Empty;
        //    }

        //    m_oCallViewBar.CompanyBoardNo = e.Account.telephone;

        //    if (!m_BrightSalesProperty.CommonProperty.OnCallMode)
        //        m_oCallViewBar.SetContactNumbers();

        //    m_oFollowUp.SetCompanyInfo();
        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //    {
        //        m_CampaignListModule.UpdateSelectedCompany(e);
        //        m_CampaignListModule.SetCompanyDetails(e.Account.id);
        //    }
        //}
        
        private void m_CampaignBookingModule_SetCompanyModificationInfo()
        {
            string _info = string.Empty;
            //if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
            if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList)
                _info = m_CampaignListModule.GetCompanyModificationInfo();
            //else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp)
            else if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.MyFollowUp)
                _info = m_MyFollowUpModule.GetCompanyModificationInfo();

            barStaticItemCurrentWorkedCompany.Caption = _info;
        }
        //private void m_CampaignBookingModule_UserWorkModeChanged(bool WorkModeState)
        //{
        //    if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.CampaignList)
        //        m_CampaignListModule.SetWorkModeState(WorkModeState);
        //    else if (m_CampaignBookingModule.ParentView == ManageCampaignBooking.eParentView.MyFollowUp)
        //        m_MyFollowUpModule.SetWorkModeState(WorkModeState);
        //}
        //private void m_CampaignBookingModule_CampaignBookingCallAttemptMade(object sender, EventArgs e)
        //{
        //    if (m_CampaignListModule != null)
        //        m_CampaignListModule.CallAttemptMade();
        //}
        //private void m_CampaignBookingModule_CallerViewCallLogAdded(object sender, EventArgs e)
        //{
        //    //if (m_CampaignListModule != null)
        //    //    m_CampaignListModule.ReloadCallLogTab();
        //}
        //private void m_CampaignBookingModule_CallLogAfterDelete(object sender, ManageCampaignBooking.CallLogAfterDeleteEvent e)
        //{
        //    if (m_CampaignListModule != null)
        //        m_CampaignListModule.DeleteCallLog(e.DeletedId);
        //}
        private void m_CampaignBookingModule_ModuleSetStateInitiated(object sender, ManageCampaignBooking.ModuleSetStateArgs e)
        {
            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse)
                this.m_oCallLogBar_EndCall_Initiated(null, null);

            else if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Work)
                m_oCallViewBar.SetContactNumbers();

            m_oCallViewBar.SetState();
        }
        #endregion

        #region Campaign List
        

        private void m_CampaignListModule_CampaignExtraDetail_OnContactSaved()
        {
            //this.SaveCompanyAppointment();
        }
        //private void m_CampaignListModule_CampaignList_CompanyOnWorkByAnotherConsultant(CampaignList.CompanyOnWorkByAnotherConsultantArgs e)
        //{
        //    m_CompanyWorkedByAnotherConsultant = true;
        //    m_CampaignBookingModule.CompanyWorkedByAnotherConsultant = true;
        //    NotificationDialog.Warning("Bright Sales", string.Format("{0}This company is currently worked by{0}{1}", Environment.NewLine, e.UserName));
        //}
        //private void m_CampaignListModule_CampaignExtraDetail_OnCompanyInformationSaved()
        //{
        //    this.SaveCompanyAppointment();
        //    if (m_CampaignBookingModule != null)
        //        m_CampaignBookingModule.UpdateCompanyInformation();
        //}
        private bool m_CampaignListModule_HasPendingCallAndLog()
        {
            if (m_BrightSalesProperty.CommonProperty.OnCallMode || !m_BrightSalesProperty.CommonProperty.CallLogSaved)
                return true;

            return false;
        }
        private bool m_CampaignListModule_CampaignList_btnReleaseLock_OnClick(object sender, EventArgs e)
        {
            if (m_BrightSalesProperty.CommonProperty.OnCallMode || !m_BrightSalesProperty.CommonProperty.CallLogSaved)
                return false;

            return true;
        }
        private void m_CampaignList_AccountModificationInfoChange()
        {
            barStaticItemCurrentWorkedCompany.Caption = "";
            if (m_BrightSalesProperty.CommonProperty == null)
                return;

            barStaticItemCurrentWorkedCompany.Caption = 
                m_BrightSalesProperty.CommonProperty.CompanyName + " - last updated on: "
                + ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                    m_BrightSalesProperty.CommonProperty.FinalListId,
                    m_BrightSalesProperty.CommonProperty.AccountId
            );
        }
        private void m_CampaignListModule_btnRefreshSubCampaigns_OnClick(object sender, EventArgs e)
        {
            //m_RefreshSubCampaignList = true;
            //m_MyFollowUpModule = null;
            //m_CampaignBookingModule = null;
            //pcCampaignBooking.Controls.Clear();
            //pnlMyFollowups.Controls.Clear();
            //barStaticItemCurrentWorkedCompany.Caption = "";
            //this.SetFormControls(true);
            //this.Focus();
            //m_RefreshSubCampaignList = false;
        }
        private void m_CampaignList_btnRemoveCompany_OnClick(object sender, CampaignList.CampaignListArgs e)
        {
            if (m_CampaignBookingModule != null) {
                m_BrightSalesProperty.CampaignBooking.Mode = SelectionProperty.CampaignBookingMode.None;
                this.SetStateCallerBarGroup(false);
                m_CampaignBookingModule.DisableCampaignBooking();
                m_CampaignBookingModule.OnCampaignListHasRows();
            }

            if (m_oCallViewBar != null)
                m_oCallViewBar.Enabled = false;

            //if (m_MyFollowUpModule != null)
            //{
            //    m_MyFollowUpModule.LoadFollowUps();

            //    CampaignBookingProperty.CampaignBoookingArguments _args = m_MyFollowUpModule.GetCampaignBookingArgs(false);
            //    m_oFollowUpBar.SetCampaignInfo(_args.CampaignInfo, _args.ToolTipInfo, m_MyFollowUpModule.GetId());
            //}

            //m_CampaignBookingModule.InitializeCampaignBooking(true);
        }
        //private void m_CampaignList_btnWorkOnCompany_OnClick(object sender, CampaignList.CampaignListArgs e)
        //{
        //    this.LoadCampaignBooking(e);
        //}
        private void m_CampaignList_btnSaveAsNotQualified_OnClick(object sender, CampaignList.CampaignListArgs e)
        {
            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.SetLastUpdatedInfo();
        }
        private void m_CampaignList_cboSubCampaignList_OnEditValueChange(object sender, CampaignList.CampaignListArgs e)
        {
            //tabCampaignBooking.Controls.Clear();
            //pcCampaignBooking.Controls.Clear();
            //m_CampaignBookingModule = null;
            //if (m_CampaignBookingModule != null)
            //{
            //    m_CampaignBookingModule.ClearPage();
            //    //m_CampaignBookingModule.ResetContactParameters();
            //}
        }
        private void m_CampaignList_gvCampaignList_OnColumnFilterChange(object sender, CampaignList.CampaignListArgs e)
        {
            barStaticItemCurrentWorkedCompany.Caption = "";
            if (e.CampaignBookingAppointment == null)
                return;

            barStaticItemCurrentWorkedCompany.Caption = e.CompanyName + " - last updated on: "
                + ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                    e.CampaignBookingAppointment.FinalListId,
                    e.CampaignBookingAppointment.AccountId
                );

            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.OnCampaignListHasRows();
        }
        //private void m_CampaignList_gvCampaignList_OnDoubleClick(object sender, CampaignList.CampaignListArgs e)
        //{
        //    this.LoadCampaignBooking(e);
        //}
        //private void m_CampaignList_gvCampaignList_OnEnter(object sender, CampaignList.CampaignListArgs e)
        //{
        //    this.LoadCampaignBooking(e);
        //}
        private void m_CampaignList_gvCampaignList_OnFocusedRowChange(object sender, CampaignList.CampaignListArgs e)
        {
            barStaticItemCurrentWorkedCompany.Caption = "";
            if (e.CampaignBookingAppointment == null)
                return;

            barStaticItemCurrentWorkedCompany.Caption = e.CompanyName + " - last updated on: "
                + ObjectSubCampaign.GetSubCampaignAppointmentLastUpdtedInfo(
                    e.CampaignBookingAppointment.FinalListId,
                    e.CampaignBookingAppointment.AccountId
                );

            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.OnCampaignListHasRows();
        }
        private void m_CampaignListModule_CampaignList_OnCampaignListEmpty()
        {
            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.OnCampaignListEmpty();
        }
        private void m_CampaignListModule_CallLogAfterDelete(object sender, ManageCampaignList.CallLogAfterDeleteEventArgs e)
        {
            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.DeleteCallLog(e.DeletedId);
        }
        //private void m_CampaignListModule_CampaignExtraDetail_CallLog_WorkNurtureEvent(MyFollowUps.CampaignBookingArgs e)
        //{
        //    if (e == null)
        //        return;

        //    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
        //        sub_campaign_account_lists _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
        //            i.account_id == e.oAppointment.AccountId &&
        //            i.final_list_id == e.oAppointment.FinalListId
        //        );
        //        if (_item != null)
        //            _efDbContext.Detach(_item);

        //        if (_item.locked && _item.locked_by != UserSession.CurrentUser.UserId) {
        //            user _user = _efDbContext.users.FirstOrDefault(i => i.id == _item.locked_by);
        //            if (_user != null)
        //                _efDbContext.Detach(_user);

        //            NotificationDialog.Error("Bright Sales", string.Format("You cannot work on this follow up.{0}This follow up is currently worked by{0}{1}", Environment.NewLine, _user.fullname));
        //            return;
        //        }
        //    }

        //    this.LoadSpecificData(e);
        //}
        #endregion

        #region Campaign Extra Detail
        private void m_CampaignExtraDetail_tcCampaignExtraDetail_OnSelectedPageChange(object sender, CampaignExtraDetail.CampaignExtraDetailArgs e)
        {
        }
        #endregion

        #region Login
        private void objFrmUserLogin_AfterLogin()
        {
            /*
             * Start Bright Vision WindowsAzureStorageBlob Uploading Utility Application
             */
            (new Utils.WindowsAzureStorageBlobUtility()).RunWindowsAzureStorageBlob();
            (new Utils.WindowsAzureStorageBlobUtility()).RunMoveFailedAudioFile();

            //this.Text = "BrightSales     User: " + UserSession.CurrentUser.UserFullName;
            this.Text = FormUtility.GetConfigSetting("BuildEnvironment") + " - " + UserSession.CurrentUser.UserFullName;
            this.WindowState = FormWindowState.Maximized;
            this.DoneLoggedIn = true;
            this.SetFormControls(true);
            this.SetBrightManagerTab();
            this.LoadCampaignListSelection();
            if (m_CampaignListModule != null)
                m_CampaignListModule.SetExtraDetailModuleAsReadOnly(false);

            m_UserLogin.AfterLogin -= new UserLogin.AfterLoginEventHandler(objFrmUserLogin_AfterLogin);
            if (!m_oCallLogBar.PhoneRegisterSuccess) {
                BackgroundWorker _bw = new BackgroundWorker() {
                    WorkerSupportsCancellation = true
                };
                _bw.DoWork += new DoWorkEventHandler(_bw_DoWork);
                _bw.RunWorkerAsync();
                //m_oCallLogBar.RegisterPhone();
            }


            audio_settings _item = AudioSettingUtility.GetUserAudioSetting();

            /*
             * https://brightvision.jira.com/browse/PLATFORM-2375
             * Will only going to check if Phone setting is set to internal. 
            */
            if (_item != null && _item.mode == 0)
            {
                if (!FacadeSoftPhone.MicrophoneDeviceOk() || !FacadeSoftPhone.SpeakerDeviceOk())
                {
                    NotificationDialog.Information("Bright Sales", "No microphone/speaker device found.");
                    return;
                }

                if (_item.mic_volume == null || _item.speaker_volume == null)
                {
                    NotificationDialog.Information("Bright Sales", "Microphone/speaker settings not yet set.");
                    PopupDialog dialog = new PopupDialog(new AudioSettings(), "Phone Settings");
                    dialog.ShowDialog(this);
                    return;
                }
            }

            user _eftUser = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftUser = _efDbContext.users.FirstOrDefault(i => i.id == UserSession.CurrentUser.UserId);
                _efDbContext.Detach(_eftUser);
            }

            if (_eftUser.password.ToLower() == HashUtility.GetHashPassword("1234").ToLower()) {
                NotificationDialog.Information("Bright Sales", "Please kindly update your password first.");
                m_frmChangePassword = new ChangePassword(true) {
                    StartPosition = FormStartPosition.CenterParent
                };
                m_frmChangePassword.InvalidOldPassword -= m_frmChangePassword_InvalidOldPassword;
                m_frmChangePassword.InvalidOldPassword += m_frmChangePassword_InvalidOldPassword;
                m_frmChangePassword.PasswordsDoesNotMatch -= m_frmChangePassword_PasswordsDoesNotMatch;
                m_frmChangePassword.PasswordsDoesNotMatch += m_frmChangePassword_PasswordsDoesNotMatch;
                m_frmChangePassword.AfterSave -= m_frmChangePassword_AfterSave;
                m_frmChangePassword.AfterSave += m_frmChangePassword_AfterSave;
                m_frmChangePassword.PasswordsMustBeDifferent -= m_frmChangePassword_PasswordsMustBeDifferent;
                m_frmChangePassword.PasswordsMustBeDifferent += m_frmChangePassword_PasswordsMustBeDifferent;
                m_frmChangePassword.ShowDialog(this);
            }
        }
        private void _bw_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate  {
                m_oCallLogBar.RegisterPhone();
            }));
        }
        #endregion

        #region Toggle Menu Bar
        private void m_oCallLogBar_EndCall_Initiated(object sender, EventArgs e)
        {
            //m_oCallLogBar.HangUpCallIfExist();
            //m_oCallViewBar.PhoneCallEnded();
            //m_oCallLogBar.Visible = false;
            //m_oCallViewBar.Visible = true;
            //m_oCallViewBar.PhoneCallEnded();
            ////m_oCallViewBar.Default();
            ////m_oCallViewBar.SetContactNumbers();
        }
        private CTScSubCampaignContactList m_oCallLogBar_btnUseContact_OnClick(object sender, EventArgs e)
        {
            if (m_CampaignBookingModule == null)
                return null;

            return m_CampaignBookingModule.m_oCallLogBar_btnUseContact_OnClick();
        }
        
        private void m_oCallViewBar_CallAttemptMade(object sender, EventArgs e)
        {
            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.m_oCallViewBar_CallAttemptMade();

            if (m_CampaignListModule != null)
                m_CampaignListModule.CallAttemptMade();
        }
        
        /*private void m_oCallViewBar_PhoneCall_Initiated(object sender, CallViewBar.PhoneCallInitiatedArgs e)
        {
            m_oCallLogBar.Visible = true;
            m_oCallViewBar.Visible = false;
            m_oCallLogRemarksBar.Visible = true;
            m_oFollowUpBar.Visible = false;
            m_oCallLogBar.CallLogBarParams = new FrmSalesConsultant.CallLogArgs() {
                ContactId = e.ContactId,
                ContactNo = e.ContactNo
            };

            if (e.CallMethod == CallViewBar.eCallMethod.Call_Board)
                m_oCallLogBar.CallLogBarParams.CallMethod = FrmSalesConsultant.eCallMethod.Call_Board;

            else if (e.CallMethod == CallViewBar.eCallMethod.Call_Direct)
                m_oCallLogBar.CallLogBarParams.CallMethod = FrmSalesConsultant.eCallMethod.Call_Direct;

            else if (e.CallMethod == CallViewBar.eCallMethod.Call_Mobile)
                m_oCallLogBar.CallLogBarParams.CallMethod = FrmSalesConsultant.eCallMethod.Call_Mobile;

            else if (e.CallMethod == CallViewBar.eCallMethod.Call_Anonymous)
                m_oCallLogBar.CallLogBarParams.CallMethod = FrmSalesConsultant.eCallMethod.Call_Anonymous;

            m_oCallLogRemarksBar.CallMethod = m_oCallLogBar.CallLogBarParams.CallMethod;
            m_oCallLogRemarksBar.ContactPerson = e.ContactPerson;
            m_oCallLogRemarksBar.LoadContactPerson();
            m_oCallLogBar.ContactPerson = e.ContactPerson;
            m_oCallLogBar.InitiatePhoneCall();

            #region Log
            var m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            audio_settings _item = m_efDbContext.audio_settings.FirstOrDefault(i => i.user_id == UserSession.CurrentUser.UserId);
            var log = BrightSalesFacade.Logger;

            log.SetLogField(LoggingField.called_number, e.ContactNo);
            log.SetLogField(LoggingField.called_number_type, e.CallMethod.GetEnumDescription());
           
            if (_item != null && _item.mode == 0)
                log.SetLogField(LoggingField.call_engine,  "ozeki");
            else
                log.SetLogField(LoggingField.call_engine,  "external");

            log.SendInfo("call_start", "call details");
            #endregion
        }
         * */
        //private bool m_oCallViewBar_CallLogSaved()
        //{
        //    return m_BrightSalesProperty.IsCallLogSave;
        //}
        private bool m_oCallViewBar_IsOnCampaignBookingTab()
        {
            if (tcSalesConsultant.SelectedTabPage != tabCampaignBooking) {
                NotificationDialog.Information("Bright Sales", "Calling not allowed when not on campaign booking tab.");
                return false;
            }

            return true;
        }
        #endregion

        #region Follow Up window
        private void m_oFollowUp_btnSave_OnClick(object sender, EventFollowUpLogEvents.OnSaveArguments e)
        {
            /*
             * https://docs.google.com/a/myndconsulting.com/spreadsheet/ccc?key=0Ah8Xvlc0xaJKdG1CbEJEbWdYTkdPanpqeHJWNGtmYXc#gid=0
             */
            this.SaveCompanyAppointment();
            if(m_CampaignBookingModule != null)
                m_CampaignBookingModule.UpdateContactViewLastUser();

            m_oCallLogBar.Default();
            m_oCallViewBar.Default();
            
            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.m_oFollowUp_btnSave_OnClick();

            if (m_CampaignListModule != null)
                m_CampaignListModule.ReloadCallLogTab();
        }
        private string m_oFollowUp_GetListSource(FollowUpEditor.GetListSourceArgs e)
        {
            string _ListSource = string.Empty;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                int _FinalListId = (int)_efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == m_SubCampaignId).id;
                if (e.CompanyId > 0) {
                    sub_campaign_account_lists _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                        i.final_list_id == _FinalListId &&
                        i.account_id == e.CompanyId
                    );
                    if (_item != null) {
                        _ListSource = _item.list_source;
                        _efDbContext.Detach(_item);
                    }                    
                }
                else if (e.ContactId > 0) {
                    sub_campaign_contact_lists _item = _efDbContext.sub_campaign_contact_lists.FirstOrDefault(i =>
                        i.final_list_id == _FinalListId &&
                        i.contact_id == e.ContactId
                    );
                    if (_item != null) {
                        _ListSource = _item.list_source;
                        _efDbContext.Detach(_item);
                    }
                }
            }

            return string.Format("{0}>{1}>{2}{3}",
                m_CustomerName,
                m_CampaignName,
                m_SubCampaignName,
                string.IsNullOrEmpty(_ListSource) ? "" : " (" + _ListSource + ")"
            );
        }
        #endregion

        #region Call Log Remarks & Follow Up Bar
        private CTScSubCampaignContactList m_oCallLogRemarksBar_GetContactPerson()
        {
            if (m_CampaignBookingModule == null)
                return null;

            return m_CampaignBookingModule.m_oCallLogBar_btnUseContact_OnClick();
        }
        private void m_oCallLogRemarksBar_btnSaveCallLog_OnClick(object sernder, CallLogRemarks.SaveCallLogArgs e)
        {
            /*
             * https://brightvision.jira.com/browse/PLATFORM-2986
             * https://docs.google.com/a/myndconsulting.com/spreadsheet/ccc?key=0Ah8Xvlc0xaJKdG1CbEJEbWdYTkdPanpqeHJWNGtmYXc#gid=0
             */
            if (e.CallMethod != "Call Board" && e.CallMethod != "Call Anonymous" && e.CallMethod != null)
            {
                //this.SaveCompanyAppointment();
                //m_CampaignBookingModule.
                m_CampaignBookingModule.UpdateContactViewLastUser();
            }
            m_oCallLogBar.AudioId = Guid.Empty;
            m_BrightSalesProperty.CommonProperty.CallLogSaved = true;
            m_oCallLogBar.Default();
            m_oCallLogBar.Visible = false;
            m_oCallViewBar.Visible = true;
            //m_oCallViewBar.PhoneCallEnded();
            m_oCallViewBar.Default(false, true);
            m_oCallViewBar.SetContactNumbers();

            if (m_CampaignBookingModule != null)
                m_CampaignBookingModule.m_oCallLogBar_btnSaveCallLog_OnClick(e.Id);

            if (m_CampaignListModule != null)
                m_CampaignListModule.ReloadCallLogTab();

            m_oCallLogRemarksBar.Visible = false;
            m_oFollowUpBar.Visible = true;
        }
        private void m_oCallLogRemarksBar_EndCall_Initiated(object sender, EventArgs e)
        {
            m_oCallLogBar.HangUpCallIfExist();
            m_oCallLogBar.EndClock();
            m_oCallLogBar.Visible = false;
            m_oCallViewBar.Visible = true;
            m_oCallViewBar.PhoneCallEnded();

            //m_oCallLogRemarksBar.CallLogBarParams = m_oCallLogBar.CallLogBarParams;
            //m_oCallLogRemarksBar.ContactPerson = m_oCallLogBar.ContactPerson;
            m_oCallLogRemarksBar.AudioId = m_oCallLogBar.AudioId;
            if (m_oCallLogBar.AudioId.ToString().Equals("00000000-0000-0000-0000-000000000000") || m_oCallLogBar.AudioId == null) {
                m_oCallLogRemarksBar.StartTime = m_oCallViewBar.CallAttemptTime;
                m_oCallLogRemarksBar.EndTime = m_oCallViewBar.CallAttemptTime;
            }
            else {
                m_oCallLogRemarksBar.StartTime = m_oCallLogBar.StartTime;
                m_oCallLogRemarksBar.EndTime = m_oCallLogBar.EndTime;
            }
            //m_oCallLogRemarksBar.SubCampaignId = m_oCallLogBar.SubCampaignId;
            //m_oCallLogRemarksBar.AccountId = m_oCallLogBar.AccountId;
            //m_oCallLogRemarksBar.LoadContactPerson();
        }
        private bool m_oCallLogRemarksBar_UserOnCall()
        {
            if (m_BrightSalesProperty.CommonProperty.OnCallMode)
                return true;

            return false;
        }

        private void m_oCallLogRemarksBar_UserOnCallForceStop()
        {
            m_oCallLogBar.btnHangUp.PerformClick();
        }

        //private void m_oFollowUpBar_LoadFollowUps()
        //{
        //    if (m_MyFollowUpModule == null)
        //        this.InitializeMyFollowUpModule();

        //    m_MyFollowUpModule.LoadEvents();
        //}
        //private void m_oFollowUpBar_btnTop_OnClick()
        //{
        //    if (m_MyFollowUpModule.MoveFirst()) {
        //        CampaignBookingProperty.CampaignBoookingArguments _args = m_MyFollowUpModule.GetCampaignBookingArgs(false);
        //        m_oFollowUpBar.SetCampaignInfo(_args.CampaignInfo, _args.ToolTipInfo);
        //        m_oFollowUpBar.EventId = m_MyFollowUpModule.GetId();
        //        //if (m_CampaignBookingModule != null)
        //        //    m_CampaignBookingModule.LoadFollowUpData();
        //    }
        //}
        //private void m_oFollowUpBar_btnNext_OnClick()
        //{
        //    if (m_MyFollowUpModule.MoveNext()) {
        //        CampaignBookingProperty.CampaignBoookingArguments _args = m_MyFollowUpModule.GetCampaignBookingArgs(false);
        //        m_oFollowUpBar.SetCampaignInfo(_args.CampaignInfo, _args.ToolTipInfo);
        //        m_oFollowUpBar.EventId = m_MyFollowUpModule.GetId();
        //        //if (m_CampaignBookingModule != null)
        //        //    m_CampaignBookingModule.LoadFollowUpData();
        //    }
        //}
        //private void m_oFollowUpBar_btnPrevious_OnClick()
        //{
        //    if (m_MyFollowUpModule.MovePrevious()) {
        //        CampaignBookingProperty.CampaignBoookingArguments _args = m_MyFollowUpModule.GetCampaignBookingArgs(false);
        //        m_oFollowUpBar.SetCampaignInfo(_args.CampaignInfo, _args.ToolTipInfo);
        //        m_oFollowUpBar.EventId = m_MyFollowUpModule.GetId();
        //        //if (m_CampaignBookingModule != null)
        //        //    m_CampaignBookingModule.LoadFollowUpData();
        //    }
        //}
        //private void m_oFollowUpBar_btnLoad_OnClick(object sender, FollowUpBar.btnLoadOnClickArgs e)
        //private void m_oFollowUpBar_btnLoad_OnClick(object sender, CampaignBookingProperty.CampaignBoookingArguments e)
        //{
        //    if (e == null)
        //        return;

        //    //tcSalesConsultant.SelectedTabPage = tabCampaignList;
        //    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
        //        sub_campaign_account_lists _item = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
        //            i.account_id == e.oAppointment.AccountId &&
        //            i.final_list_id == e.oAppointment.FinalListId
        //        );
        //        if (_item != null)
        //            _efDbContext.Detach(_item);

        //        if (_item.locked && _item.locked_by != UserSession.CurrentUser.UserId) {
        //            user _user = _efDbContext.users.FirstOrDefault(i => i.id == _item.locked_by);
        //            if (_user != null)
        //                _efDbContext.Detach(_user);

        //            NotificationDialog.Error("Bright Sales", string.Format("You cannot work on this follow up.{0}This follow up is currently worked by{0}{1}", Environment.NewLine, _user.fullname));
        //            return;
        //        }
        //    }

        //    this.LoadSpecificData(e);
        //    //this.LoadSpecificData(e.CampaignBookingArgs);
            
        //    //m_oCallLogBar.EndClock();
        //    //m_oCallLogBar.Default();
        //    //m_oCallViewBar.PhoneCallEnded();
        //    //m_oCallViewBar.Default();
        //    //m_MyFollowUpModule.CloseCompany();
        //    //m_oFollowUpBar.Clear();
        //    //this.SetStateCallerBarGroup(false);
        //    //this.tcSalesConsultant.SelectedTabPage = tabMyFollowUp;
        //}
        //private void m_oFollowUpBar_btnSave_OnClick()
        //{
        //    if (m_MyFollowUpModule != null)
        //        //m_MyFollowUpModule.UpdateFocusedRow();
        //        m_MyFollowUpModule.LoadEvents();
        //}
        //private bool m_oFollowUpBar_CanWorkOnCompany()
        //{
        //    if (this.CampaignBooking_DialogEditor_OnEditMode()) {
        //        NotificationDialog.Warning("Bright Sales", "Dialog on edit mode.");
        //        return false;
        //    }
        //    else if (m_BrightSalesProperty.CommonProperty.OnCallMode)
        //    {
        //        NotificationDialog.Warning("Bright Sales", "Currently on call mode.");
        //        return false;
        //    }
        //    else if (!m_BrightSalesProperty.CommonProperty.CallLogSaved)
        //    {
        //        NotificationDialog.Warning("Bright Sales", "Please save your call log first.");
        //        return false;
        //    }

        //    return true;
        //}
        //private List<CTScSubCampaignContactList> m_oFollowUpBar_GetCampaignBookingContactList()
        //{
        //    if (m_CampaignBookingModule == null)
        //        return new List<CTScSubCampaignContactList>();

        //    return m_CampaignBookingModule.GetCampaignBookingContactList();
        //}
        //private CampaignBookingProperty.CampaignBoookingArguments m_oFollowUpBar_GetCampaignBookingArgs(bool pForWorkModePurpose)
        //{
        //    if (m_MyFollowUpModule == null)
        //        return new CampaignBookingProperty.CampaignBoookingArguments();

        //    m_MyFollowUpModule.SetFocusRow();
        //    return m_MyFollowUpModule.GetCampaignBookingArgs(pForWorkModePurpose);
        //}
        //private bool m_oFollowUpBar_HasBrowsableData()
        //{
        //    if (m_MyFollowUpModule == null) {
        //        WaitDialog.Show("Loading data ...");
        //        this.InitializeMyFollowUpModule();
        //        WaitDialog.Close(true);
        //    }

        //    if (!m_MyFollowUpModule.HasBrowsableData())
        //        return false;

        //    return true;
        //}
        //private bool m_oFollowUpBar_HasPendingCallAndLog()
        //{
        //    if (m_BrightSalesProperty.CommonProperty.OnCallMode || !m_BrightSalesProperty.CommonProperty.CallLogSaved)
        //        return true;

        //    return false;
        //}
        #endregion
        #endregion

        #region Keyboard Shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) 
        {
            /**
             * if focused control is the campaign combo list, disable enter
             * to avoid triggering the on key up enter of the campaign list module.
             */
            if (keyData == Keys.Enter && cboCampaignList.Focused) {
                if (this.CampaignListValueIsValid())
                    m_EventBus.Notify(new FrmSalesConsultantEvents.OnEnterCampaignListCombo());
            }

            /**
             * [@jeff 09.17.2012]: https://brightvision.jira.com/browse/PLATFORM-1938
             * shortcut keys for calling contacts.
             */
            if (tcSalesConsultant.SelectedTabPage == tabCampaignBooking && !this.CampaignBooking_DialogEditor_OnEditMode()) {
                if ((keyData == (Keys.Control | Keys.D1)) ||
                    (keyData == (Keys.Control | Keys.NumPad1))) { // call board
                    if (m_oCallViewBar.InitiateCallBoard())
                        return true;

                    NotificationDialog.Warning("Bright Sales", "Cannot call board.");
                }
                else if ((keyData == (Keys.Control | Keys.D2)) ||
                    (keyData == (Keys.Control | Keys.NumPad2))) { // call direct
                    if (m_oCallViewBar.InitiateCallDirect())
                        return true;

                    NotificationDialog.Warning("Bright Sales", "Cannot call direct.");
                }
                else if ((keyData == (Keys.Control | Keys.D3)) ||
                    (keyData == (Keys.Control | Keys.NumPad3))) { // call mobile
                    if (m_oCallViewBar.InitiateCallMobile())
                        return true;

                    NotificationDialog.Warning("Bright Sales", "Cannot call mobile.");
                }
                else if ((keyData == (Keys.Control | Keys.D4)) ||
                    (keyData == (Keys.Control | Keys.NumPad4))) { // call anonymous
                    if (m_oCallViewBar.InitiateCallAnonymous())
                        return true;

                    NotificationDialog.Warning("Bright Sales", "Cannot call anonymous.");
                }
                else if (keyData == (Keys.Alt | Keys.C)) { // campaign booking call status on focus
                    if (m_oCallLogRemarksBar.Visible) {
                        m_oCallLogRemarksBar.FocusCallStatus();
                        NotificationDialog.Information("Bright Sales", "Call status now focused.");
                        return true;
                    }
                }
            }

            /**
             * implement shorcut key for editing dialog.
             * will work only if on work mode and if tab is on cam apign booking tab.
             */
            if (keyData == (Keys.Control | Keys.E)) { // edit dialog
                if (tcSalesConsultant.SelectedTabPage == tabCampaignBooking) {
                    if (m_CampaignBookingModule != null)
                        m_CampaignBookingModule.EditCampaignDialog();
                    else
                        NotificationDialog.Warning("Bright Sales", "Campaign booking not workable. Please load a company to work on.");

                    return true;
                }
            }
            
            /**
             * implement shortcut keys for next and previous buttons.
             * will work only if on browse mode and if tab is on cam apign booking tab.
             */
            if (!this.CampaignBooking_DialogEditor_OnEditMode()) {
                //if (keyData == (Keys.Control | Keys.Right)) { // move next
                //https://brightvision.jira.com/browse/PLATFORM-2618
                if (keyData == (Keys.Alt | Keys.Right))
                { // move next
                    if (tcSalesConsultant.SelectedTabPage == tabCampaignBooking) {
                        if (m_CampaignBookingModule != null)
                            m_CampaignBookingModule.LoadNextCompany();
                        else
                            NotificationDialog.Warning("Bright Sales", "Campaign booking not workable. Please load a company to work on.");

                        return true;
                    }
                }
                //else if (keyData == (Keys.Control| Keys.Left)) { // move previous
                //https://brightvision.jira.com/browse/PLATFORM-2618
                else if (keyData == (Keys.Alt | Keys.Left))
                { // move previous
                    if (tcSalesConsultant.SelectedTabPage == tabCampaignBooking) {
                        if (m_CampaignBookingModule != null)
                            m_CampaignBookingModule.LoadPreviousCompany();
                        else
                            NotificationDialog.Warning("Bright Sales", "Campaign booking not workable. Please load a company to work on.");
                        return true;
                    }
                }
            }

            /**
             * [@jeff 05.17.2012]: https://brightvision.jira.com/browse/PLATFORM-1388
             * add control shortcut keys for modules.
             * implemented on the main form since all control key events go here first.
             */
            if (!this.CampaignBooking_DialogEditor_OnEditMode() && !m_BrightSalesProperty.CommonProperty.OnCallMode && m_BrightSalesProperty.CommonProperty.CallLogSaved)
            {
                if (keyData == (Keys.Control | Keys.F2)) { // campaign list
                    tcSalesConsultant.SelectedTabPage = tabCampaignList;
                    return true;
                }
                else if (keyData == (Keys.Control | Keys.F3)) { // campaign booking
                    tcSalesConsultant.SelectedTabPage = tabCampaignBooking;
                    return true;
                }
                else if (keyData == (Keys.Control | Keys.F4)) { // my follow ups
                    tcSalesConsultant.SelectedTabPage = tabMyFollowUp;
                    return true;
                }
                else if (keyData == (Keys.Control | Keys.F5)) { // reports
                    tcSalesConsultant.SelectedTabPage = tabReport;
                    return true;
                }
                else if (keyData == (Keys.Control | Keys.F6)) { // help website
                    tcSalesConsultant.SelectedTabPage = tabHelpWebsite;
                    return true;
                }
                else if (keyData == (Keys.Control | Keys.F7)) { // milltime
                    tcSalesConsultant.SelectedTabPage = tabMilltime;
                    return true;
                }
            }

            /**
             * keyboard shorcuts for campaign extra detail tab pages.
             */
            if (keyData == Keys.F1) // company/contact information
            {
                if (tcSalesConsultant.SelectedTabPage.Equals(tabCampaignList))
                    this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.CompanyInfortmation);
                else
                    this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.ContactInformation);

                return true;
            }
            else if (keyData == Keys.F2) // company/contact information
            {
                if (tcSalesConsultant.SelectedTabPage.Equals(tabCampaignList))
                    this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.ContactInformation);
                else
                    this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.CompanyInfortmation);

                return true;
            }
            else if (keyData == Keys.F3) // call log
            {
                this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.CallLog);
                return true;
            }
            else if (keyData == Keys.F4) // booking schedules
            {
                this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.BookingSchedule);
                return true;
            }
            else if (keyData == Keys.F5) // collected data
            {
                this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.CollectedData);
                return true;
            }
            else if (keyData == Keys.F6) // company website
            {
                this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.CompanyWebsite);
                return true;
            }
            else if (keyData == Keys.F7) // google search
            {
                this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.GoogleSearch);
                return true;
            }
            else if (keyData == Keys.F8) // bv standard question
            {
                this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.BvStandardQuestion);
                return true;
            }
            else if (keyData == Keys.F9) // map companies
            {
                this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.MapCompany);
                return true;
            }
            else if (keyData == Keys.F10) // map contacts
            {
                this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.MapContact);
                return true;
            }
            else if (keyData == Keys.F11) // bv sales script
            {
                this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.BvSalesScript);
                return true;
            }
            else if (keyData == Keys.F12) // my sales script
            {
                this.SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage.MySalesScript);
                return true;
            }

            /**
             * if not shortcut keys for modules, the go here.
             */
            if (FormUtility.ShortCutKeysHandled(keyData))
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Background Thread for Event Logging
        public void AddWorkToQueue(EventMessage eventMessage) {
            lock (work) {
                work.Add(eventMessage);
            }
        }
        private void work_AllWorkCompleted(object sender, EventArgs e) {
            if (this.InvokeRequired) {
                this.Invoke(new EventHandler(work_AllWorkCompleted), new object[] { sender, e });
            } else {
                stats = new int[6];
            }
        }
        private void work_ChangedWorkItemState(object sender, ChangedWorkItemStateEventArgs e) {
            lock (this) {
                stats[(int)e.PreviousState] -= 1;
                stats[(int)e.WorkItem.State] += 1;
            }

            if (!pausing || DateTime.Now > nextRefreshTime) {
                nextRefreshTime = DateTime.Now + refreshInterval;
            }
        }
        private void work_WorkerException(object sender, ResourceExceptionEventArgs e) {
            Application.OnThreadException(e.Exception);
        }
        private void reset_Click(object sender, System.EventArgs e) {
            lock (stats) {
                for (int i = 0; i < stats.Length; ++i)
                    stats[i] = 0;
            }
        }
        private void Pause() {
            work.Pause();
            pausing = true;
            resuming = false;
        }
        private void Resume() {
            work.Resume();
            pausing = false;
            resuming = true;
        }
        #endregion
    }
}