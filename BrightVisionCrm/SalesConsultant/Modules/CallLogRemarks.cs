
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Linq;

using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Telephony.Business;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
using SalesConsultant.Forms;
using System.Reflection;
using SalesConsultant.Events;
using BrightVision.Common.Events.Core;
using SalesConsultant.Business;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;

namespace SalesConsultant.Modules
{
    public partial class CallLogRemarks : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public CallLogRemarks()
        {
            InitializeComponent();
            this.SetSelectedCallStatus(eCallStatus.NoAnswer);
            this.RegisterEvents();
        }
        #endregion

        #region Public Events & Args
        public delegate bool UserOnCallEventHandler();
        public event UserOnCallEventHandler UserOnCall;

        public delegate void UserOnCallForceStopEventHandler();
        public event UserOnCallForceStopEventHandler UserOnCallForceStop;

        public delegate CTScSubCampaignContactList GetContactPersonEventHandler();
        public event GetContactPersonEventHandler GetContactPerson;

        public delegate void btnSaveCallLogOnClickEventHandler(object sernder, SaveCallLogArgs e);
        public event btnSaveCallLogOnClickEventHandler btnSaveCallLog_OnClick;
        public class SaveCallLogArgs : EventArgs {
            public int Id { get; set; }
            public string CallMethod { get; set; }
            public event_followup_log m_FollowupLog { get; set; }
        }

        public delegate void EndCallInitiatedEventHandler(object sender, EventArgs e);
        public event EndCallInitiatedEventHandler EndCall_Initiated;
        #endregion

        #region Public Properties
        public CallLogArgs CallLogBarParams { get; set; }
        public CTScSubCampaignContactList ContactPerson { get; set; }
        public int SubCampaignId { get; set; }
        public int AccountId { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid AudioId { get; set; }

        public SelectionProperty.CallMethod CallMethod
        {
            get { return m_CallMethod; }
            set { m_CallMethod = value; }
        }
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private SelectionProperty.CallMethod m_CallMethod = SelectionProperty.CallMethod.None;
        private enum eCallStatus 
        {
            [Description("None")]
            None,
            [Description("Successfull")]
            Completed,
            [Description("No Answer")]
            NoAnswer,
            [Description("Busy Signal")]
            BusySignal,
            [Description("Call Diverted To")]
            CallForwarding,
            [Description("Don't Have Time")]
            DontHavetime
        }
        private string m_SelectedCallStatus = string.Empty;
        #endregion

        #region Public Methods
        public void LoadContactPerson()
        {
            this.SetSelectedCallStatus(eCallStatus.NoAnswer);
            //cboCallStatus.SelectedIndex = 1; // Default: 'No Answer'
            tbxShortMessage.Text = string.Empty;
            if (m_CallMethod == SelectionProperty.CallMethod.Call_Board) {
                tbxContactTitle.Text = string.Empty;
                tbxContactFirstname.Text = "Switch";
                tbxContactLastname.Text = "Board";
                tbxContactFirstname.Properties.ReadOnly = true;
                tbxContactLastname.Properties.ReadOnly = true;
                tbxContactTitle.Properties.ReadOnly = true;
                return;
            }

            if (ContactPerson != null) {
                tbxContactFirstname.Text = ContactPerson.first_name;
                tbxContactLastname.Text = ContactPerson.last_name;
                tbxContactTitle.Text = ContactPerson.title;
                tbxContactFirstname.Properties.ReadOnly = true;
                tbxContactLastname.Properties.ReadOnly = true;
                tbxContactTitle.Properties.ReadOnly = true;
            }
            else {
                tbxContactFirstname.Text = "Call";
                tbxContactLastname.Text = "Log";
                tbxContactFirstname.Properties.ReadOnly = false;
                tbxContactLastname.Properties.ReadOnly = false;
                tbxContactTitle.Properties.ReadOnly = false;
            }
        }
        public void FocusCallStatus()
        {
            this.SetSelectedCallStatus(eCallStatus.NoAnswer);
            //cboCallStatus.Focus();
        }
        #endregion

        #region Private Methods
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<CallViewBarEvents.PhoneCallStart.CallLogRemarks>().Subscribe(PhoneCallStart);
        }
        private void PhoneCallStart(CallViewBarEvents.PhoneCallStart.CallLogRemarks e)
        {
            CallMethod = e.PhoneCallArgs.CallMethod;
            ContactPerson = e.PhoneCallArgs.ContactPerson;
            this.LoadContactPerson();
        }
        private event_followup_log SaveCallLog(bool pIsSaved)
        {
            if (EndCall_Initiated != null)
                EndCall_Initiated(this, new EventArgs());

            if (StartTime.ToString().Equals("00:00:00"))
                EndTime = new TimeSpan();

            int? _ContactId = null;
            if (ContactPerson != null)
                if (ContactPerson.id > 0)
                    _ContactId = ContactPerson.id;

            string _CallMethod = string.Empty;
            if (m_CallMethod == SelectionProperty.CallMethod.Call_Board)
                _CallMethod = "Call Board";
            else if (m_CallMethod == SelectionProperty.CallMethod.Call_Direct)
                _CallMethod = "Call Direct";
            else if (m_CallMethod == SelectionProperty.CallMethod.Call_Mobile)
                _CallMethod = "Call Mobile";
            else if (m_CallMethod == SelectionProperty.CallMethod.Call_Anonymous)
                _CallMethod = "Call Anonymous";

            //if (AudioId.ToString().Equals("00000000-0000-0000-0000-000000000000")) {
            //    StartTime = 
            //}

            string _ContactName = (string.Format("{0} {1}", tbxContactFirstname.Text, tbxContactLastname.Text)).Equals("Call Log") ? "Switchboard" : string.Format("{0} {1}", tbxContactFirstname.Text, tbxContactLastname.Text);
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                event_followup_log _efeCallLog = new event_followup_log() {
                    subcampaign_id = SubCampaignId,
                    account_id = AccountId,
                    contact_id = _ContactId,
                    title = tbxContactTitle.Text,
                    contact_no = CallLogBarParams == null ? "" : CallLogBarParams.ContactNo,
                    short_message = tbxShortMessage.Text,
                    event_type = "Call Log",
                    event_status = m_SelectedCallStatus, //cboCallStatus.Text,
                    date_of_transaction = DateTime.Now,
                    assigned_user = UserSession.CurrentUser.UserId,
                    done = true,
                    start_time = StartTime,
                    end_time = EndTime,
                    date_created = DateTime.Now,
                    created_by = UserSession.CurrentUser.UserId,
                    contact_name = _ContactName,
                    audio_id = AudioId,
                    is_saved = pIsSaved,
                    call_method = _CallMethod,
                    is_azure_blob = true

                };
                _efDbContext.event_followup_log.AddObject(_efeCallLog);
                _efDbContext.SaveChanges();
                return _efeCallLog;
            }
        }
        private string GetEnumDescription(eCallStatus value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
        private void SetSelectedCallStatus(eCallStatus pCallStatus = eCallStatus.None)
        {
            m_SelectedCallStatus = this.GetEnumDescription(pCallStatus);
            if (pCallStatus == eCallStatus.Completed) {
                btnCompleted.LookAndFeel.UseDefaultLookAndFeel = false;
                btnNoAnswer.LookAndFeel.UseDefaultLookAndFeel = true;
                btnBusySignal.LookAndFeel.UseDefaultLookAndFeel = true;
                btnCallForwarding.LookAndFeel.UseDefaultLookAndFeel = true;
                btnDontHaveTime.LookAndFeel.UseDefaultLookAndFeel = true;
                btnCompleted.LookAndFeel.SkinName = "Office 2010 Black";
            }
            else if (pCallStatus == eCallStatus.NoAnswer) {
                btnCompleted.LookAndFeel.UseDefaultLookAndFeel = true;
                btnNoAnswer.LookAndFeel.UseDefaultLookAndFeel = false;
                btnBusySignal.LookAndFeel.UseDefaultLookAndFeel = true;
                btnCallForwarding.LookAndFeel.UseDefaultLookAndFeel = true;
                btnDontHaveTime.LookAndFeel.UseDefaultLookAndFeel = true;
                btnNoAnswer.LookAndFeel.SkinName = "Office 2010 Black";
            }
            else if (pCallStatus == eCallStatus.BusySignal) {
                btnCompleted.LookAndFeel.UseDefaultLookAndFeel = true;
                btnNoAnswer.LookAndFeel.UseDefaultLookAndFeel = true;
                btnBusySignal.LookAndFeel.UseDefaultLookAndFeel = false;
                btnCallForwarding.LookAndFeel.UseDefaultLookAndFeel = true;
                btnDontHaveTime.LookAndFeel.UseDefaultLookAndFeel = true;
                btnBusySignal.LookAndFeel.SkinName = "Office 2010 Black";
            }
            else if (pCallStatus == eCallStatus.CallForwarding) {
                btnCompleted.LookAndFeel.UseDefaultLookAndFeel = true;
                btnNoAnswer.LookAndFeel.UseDefaultLookAndFeel = true;
                btnBusySignal.LookAndFeel.UseDefaultLookAndFeel = true;
                btnCallForwarding.LookAndFeel.UseDefaultLookAndFeel = false;
                btnDontHaveTime.LookAndFeel.UseDefaultLookAndFeel = true;
                btnCallForwarding.LookAndFeel.SkinName = "Office 2010 Black";
            }
            else if (pCallStatus == eCallStatus.DontHavetime) {
                btnCompleted.LookAndFeel.UseDefaultLookAndFeel = true;
                btnNoAnswer.LookAndFeel.UseDefaultLookAndFeel = true;
                btnBusySignal.LookAndFeel.UseDefaultLookAndFeel = true;
                btnCallForwarding.LookAndFeel.UseDefaultLookAndFeel = true;
                btnDontHaveTime.LookAndFeel.UseDefaultLookAndFeel = false;
                btnDontHaveTime.LookAndFeel.SkinName = "Office 2010 Black";
            }
        }

        #region Windows Azure Storage Blob 
        private void TriggerWindowsAzureStorageBlobApplication()
        {
            int _CustomerId = 0;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                int _CampaignId = (int)_efDbContext.subcampaigns.FirstOrDefault(i => i.id == SubCampaignId).campaign_id;
                _CustomerId = (int)_efDbContext.campaigns.FirstOrDefault(i => i.id == _CampaignId).customer_id;
            }

            //this.UploadFile(AudioId.ToString(), _CustomerId);
            (new Utils.WindowsAzureStorageBlobUtility()).UploadFile(AudioId.ToString(), _CustomerId);
        }
        /*
        private void UploadFile(string path, int? customerId = 0)
        {
            string Environment = ConfigManager.AppSettings["BuildEnvironment"];
            string Env = "b";
            switch (Environment) {
                case "Production Environment": Env = "b"; break;
                case "Staging Environment": Env = "s"; break;
                case "Demo Environment": Env = "d"; break;
            }

            string AdditionParam = Env + "/" + customerId + "/" + DateTime.Now.ToString("yyMMdd") + "/";
            string pworkingDir = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            this.RunWindowsAzureStorageBlob(pworkingDir);

            System.Threading.Thread.Sleep(1000);
            System.Diagnostics.Process.Start("WindowsAzureStorageBlob.exe",
                " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobAccountName"] + "\"" +
                " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobAccountKey"] + "\"" +
                " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobContainerName"] + "\"" +
                " \"" + AdditionParam + "\"" +
                " \"" + path + "\"");
        }
        private void RunWindowsAzureStorageBlob(string pworkingDir)
        {
            System.Diagnostics.Process thisProc = System.Diagnostics.Process.GetCurrentProcess();
            if (IsProcessOpen("WindowsAzureStorageBlob") == false) {
                System.Diagnostics.Process.Start("WindowsAzureStorageBlob.exe", "\"" + UserSession.EntityConnection.ConnectionString.Replace("\"", "#@#") + "\"");
                System.Threading.Thread.Sleep(1000);
                CheckIfWindowsAzureStorageBlobIsrunning();
            }
        }
        private void CheckIfWindowsAzureStorageBlobIsrunning()
        {
            System.Threading.Thread.Sleep(1000);
            if (IsProcessOpen("WindowsAzureStorageBlob") == false)
            {
                CheckIfWindowsAzureStorageBlobIsrunning();
            }
        }
        public bool IsProcessOpen(string name)
        {
            foreach (System.Diagnostics.Process clsProcess in System.Diagnostics.Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }
        */
        #endregion
        #endregion

        #region Controls
        private void btnSaveCallLog_Click(object sender, EventArgs e)
        {
            /**
             * https://brightvision.jira.com/browse/PLATFORM-2789
             * DAN.4.19.2013: Commented as jeff advice that even though currently on call, will need to auto stop the calling and proceed to saving.
             */
            if (UserOnCall())
                UserOnCallForceStop();

            #region Old Code
            //if (tbxContactFirstname.Text.Length < 1 || tbxContactLastname.Text.Length < 1) {
            //    MessageBox.Show("Firstname / Lastname is required.", "Add Call Log", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            //    return;
            //}
            //else if (tbxContactFirstname.Text.Equals("First name") || tbxContactLastname.Text.Equals("Last name")) {
            //    MessageBox.Show("Firstname / Lastname is required.", "Add Call Log", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            //    return;
            //}

            //WaitDialog.Show("Saving ...");
            //if (EndCall_Initiated != null)
            //    EndCall_Initiated(this, new EventArgs());

            //if (StartTime.ToString().Equals("00:00:00"))
            //    EndTime = new TimeSpan();

            //int? _ContactId = null;
            //if (ContactPerson != null)
            //    if (ContactPerson.id > 0)
            //        _ContactId = ContactPerson.id;

            //string _ContactName = (string.Format("{0} {1}", tbxContactFirstname.Text, tbxContactLastname.Text)).Equals("Call Log") ? "Switchboard" : string.Format("{0} {1}", tbxContactFirstname.Text, tbxContactLastname.Text);
            //event_followup_log _efeCallLog = new event_followup_log() {
            //    subcampaign_id = SubCampaignId,
            //    account_id = AccountId,
            //    contact_id = _ContactId,
            //    title = tbxContactTitle.Text,
            //    contact_no = CallLogBarParams == null? "": CallLogBarParams.ContactNo,
            //    short_message = tbxShortMessage.Text,
            //    event_type = "Call Log",
            //    event_status = cboCallStatus.Text,
            //    date_of_transaction = DateTime.Now,
            //    assigned_user = UserSession.CurrentUser.UserId,
            //    done = true,
            //    start_time = StartTime,
            //    end_time = EndTime,
            //    date_created = DateTime.Now,
            //    created_by = UserSession.CurrentUser.UserId,
            //    contact_name = _ContactName,
            //    audio_id = AudioId
            //};

            //BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            //_efDbContext.event_followup_log.AddObject(_efeCallLog);
            //_efDbContext.SaveChanges();
            #endregion

            WaitDialog.Show("Saving ...");
            event_followup_log  _efeCallLog = this.SaveCallLog(true);
            this.TriggerWindowsAzureStorageBlobApplication();
            var args = new SaveCallLogArgs { Id = _efeCallLog.id, CallMethod = _efeCallLog.call_method };
            if (btnSaveCallLog_OnClick != null)
                btnSaveCallLog_OnClick(sender, args);
            WaitDialog.Close();
        }
        private void btnInList_Click(object sender, EventArgs e)
        {
            //if (m_CallMethod == FrmSalesConsultant.eCallMethod.Call_Board)
            //    return;

            ContactPerson = GetContactPerson();
            if (ContactPerson == null) {
                tbxContactFirstname.Text = "Call";
                tbxContactLastname.Text = "Log";
                return;
            }

            tbxContactFirstname.Text = ContactPerson.first_name;
            tbxContactLastname.Text = ContactPerson.last_name;
            tbxContactTitle.Text = ContactPerson.title;
        }
        private void btnNotInList_Click(object sender, EventArgs e)
        {
            //if (m_CallMethod == FrmSalesConsultant.eCallMethod.Call_Board)
            //    return;

            tbxContactFirstname.Properties.ReadOnly = false;
            tbxContactLastname.Properties.ReadOnly = false;
            tbxContactTitle.Properties.ReadOnly = false;
            tbxContactFirstname.Text = "";
            tbxContactLastname.Text = "";
            tbxContactTitle.Text = "";
            tbxContactTitle.Focus();
        }
        private void tbxShortMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnSaveCallLog_Click(null, null);
        }
        private void btnDiscard_Click(object sender, EventArgs e)
        {
            if (UserOnCall()) {
                NotificationDialog.Information("Bright Sales", "Phone call is in progress.");
                return;
            }

            WaitDialog.Show("Saving ...");
            event_followup_log _efeCallLog = this.SaveCallLog(false);
            TriggerWindowsAzureStorageBlobApplication();
            var args = new SaveCallLogArgs { Id = _efeCallLog.id };
            if (btnSaveCallLog_OnClick != null)
                btnSaveCallLog_OnClick(sender, args);
            WaitDialog.Close();
        }

        /**
         * call status button group.
         */
        private void btnCompleted_Click(object sender, EventArgs e)
        {
            this.SetSelectedCallStatus(eCallStatus.Completed);
        }
        private void btnNoAnswer_Click(object sender, EventArgs e)
        {
            this.SetSelectedCallStatus(eCallStatus.NoAnswer);
        }
        private void btnBusySignal_Click(object sender, EventArgs e)
        {
            this.SetSelectedCallStatus(eCallStatus.BusySignal);
        }
        private void btnCallForwarding_Click(object sender, EventArgs e)
        {
            this.SetSelectedCallStatus(eCallStatus.CallForwarding);
        }
        private void btnDontHaveTime_Click(object sender, EventArgs e)
        {
            this.SetSelectedCallStatus(eCallStatus.DontHavetime);
        }
        #endregion
    }
}
