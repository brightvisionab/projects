
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Globalization;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraVerticalGrid.Rows;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.Data.Filtering;
using DevExpress.XtraRichEdit;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BrightVision.Common.Modules;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using SalesConsultant.Modules;
using SalesConsultant.Business;
using SalesConsultant.Forms;

namespace SalesConsultant.Modules {
    public partial class CallerView : DevExpress.XtraEditors.XtraUserControl {

        #region Member Variables
        BrightPlatformEntities BPContext = null;
        TimeSpan m_tsCallStartTime;
        TimeSpan m_tsCallEndTime;
        DateTime dtStart = DateTime.MinValue;
        DateTime dtEnd = DateTime.MinValue;
        string m_sCalledPhoneNo = "";
        string m_sCompanyRemarks= "";
        private bool m_OnCallMode = false;
        private int m_OnCallContactId = 0;
        #endregion

        #region Constructor
        public CallerView() 
        {
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        } 
        #endregion

        #region Properties
        //public DialogEditor DialogEditorModule { get; set; }
        public CTScSubCampaignContactList SelectedContact { get; set; }
        public string CompanyBoardNumber { get; set; }
        public int FinalListId { get; set; }
        public int AccountId { get; set; }
        public int SubCampaignId { get; set; }
        public string CompanyRemarks {
            get {
                return m_sCompanyRemarks;
            }
            set {
                m_sCompanyRemarks = value;
                txtCompanyRemark.Text = value;
            }
        }
        //public string CalledPhoneNumber {
        //    get { return m_sCalledPhoneNo; }
        //}
        #endregion

        #region Controller
        private void txtCompanyRemark_TextChanged(object sender, EventArgs e) {
            var edit = sender as MemoEdit;
            m_sCompanyRemarks = edit.Text;
        }

        private void timerCallLog_Tick(object sender, EventArgs e) {
            dtEnd = DateTime.Now;
            TimeSpan timeElapse = dtEnd.Subtract(dtStart);
            esiLogLength.Text = string.Format("{0}:{1}", timeElapse.Minutes, timeElapse.Seconds);
            esiLogCallEnd.Text = dtEnd.ToString("H:mm");
        }

        private void btnFollowupAdd_Click(object sender, EventArgs e) 
        {
            if (SelectedContact == null || SelectedContact.id < 1) {
                NotificationDialog.Information("Bright Sales", "No selected contact for follow up.");
                return;
            } else if (cboFollowUpDate.Text.Length < 1) {
                NotificationDialog.Information("Bright Sales", "Please select a date.");
                return;
            }

            string _ContactName = (txtLogFirstName.Text + " " + txtLogLastname.Text).Equals("Call Log") ? "None" : txtLogFirstName.Text + " " + txtLogLastname.Text;
            event_followup_log objFollowUpData = new event_followup_log() {
                subcampaign_id = SubCampaignId,
                account_id = AccountId,
                contact_id = SelectedContact != null ? SelectedContact.id : 0,
                contact_no = m_sCalledPhoneNo,
                title = txtLogTitle.Text,
                event_type = cbFollowUpStatus.Text.Equals("Follow-Up Call") ? "Make Call" : "Send Mail",
                event_status = cbFollowUpStatus.Text,
                short_message = txtFollowUpShortMessage.Text,
                date_of_transaction = (DateTime)cboFollowUpDate.EditValue,
                start_time = TimeSpan.Parse(cboFollowUpTimeStart.EditValue.ToString()),
                end_time = TimeSpan.Parse(cboFollowUpTimeStart.EditValue.ToString()), // set as the same as end time will not apply for follow ups
                assigned_user = (int)cboFollowUpAssignedTo.EditValue,
                date_created = DateTime.Now,
                created_by = UserSession.CurrentUser.UserId,
                contact_name = SelectedContact.first_name + " " + SelectedContact.last_name
            };
            bool _SaveExistingFollowUpCallsAsDone = false;
            if (EventFollowUp.FollowUpCallExists(objFollowUpData))
            {
                DialogResult _dlg = MessageBox.Show(
                    "Follow-up call already exist for the current contact." + Environment.NewLine + Environment.NewLine + "Would you like to mark existing follow-up calls for this contact as done?",
                    "Bright Sales",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                WaitDialog.Show(ParentForm, "Saving event follow-up...");
                if (_dlg == DialogResult.Yes)
                    _SaveExistingFollowUpCallsAsDone = true;
            }
            else
                WaitDialog.Show(ParentForm, "Saving event follow-up...");
            
            EventFollowUp.Save(objFollowUpData, _SaveExistingFollowUpCallsAsDone);

            if (onFollowUpLogSaving != null)
                onFollowUpLogSaving(sender, e);

            this.ResetCallViewFields();
            WaitDialog.Close();
        }

        private void btnAddLog_Click(object sender, EventArgs e) 
        {
            if (txtLogFirstName.Text.Length < 1 || txtLogLastname.Text.Length < 1)  {
                NotificationDialog.Information("Bright Sales", "Firstname / Lastname is required.");
                return;
            } 
            else if (txtLogFirstName.Text.Equals("First name") || txtLogLastname.Text.Equals("Last name"))  {
                NotificationDialog.Information("Bright Sales", "Firstname / Lastname is required.");
                return;
            }
            
            WaitDialog.Show(ParentForm, "Saving event call log...");
            this.HangUpCall();

            string _ContactName = (string.Format("{0} {1}", txtLogFirstName.Text, txtLogLastname.Text)).Equals("Call Log") ? "Switchboard" : string.Format("{0} {1}", txtLogFirstName.Text, txtLogLastname.Text);
            BPContext.event_followup_log.AddObject(new event_followup_log() {
                subcampaign_id = SubCampaignId,
                account_id = AccountId,
                contact_id = SelectedContact != null ? SelectedContact.id : 0,
                title = txtLogTitle.Text,
                contact_no = m_sCalledPhoneNo,
                short_message = txtLogShortMessage.Text,
                event_type = "Call Log",
                event_status = cbLogCallStatus.Text,
                date_of_transaction = DateTime.Now,
                assigned_user = UserSession.CurrentUser.UserId,
                done = true,
                start_time = m_tsCallStartTime,
                end_time = m_tsCallEndTime,
                date_created = DateTime.Now,
                created_by = UserSession.CurrentUser.UserId,
                contact_name = _ContactName
            });

            BPContext.SaveChanges();
            
            if (OnCallLogAdding != null)
                OnCallLogAdding(sender, e);

            this.ResetCallViewFields();
            WaitDialog.Close();
        }

        private void btnMute_Click(object sender, EventArgs e) {            
            btnMute.Enabled = true;
        }

        private void btnHangUp_Click(object sender, EventArgs e) {
            this.HangUpCall();            
        }

        private void btnCallBoard_Click(object sender, EventArgs e) 
        {
            try
            {
                ResetTimers();
                EnableControls(false);
                txtLogFirstName.Text = "Call";
                txtLogLastname.Text = "Log";
                btnHangUp.Enabled = true;
                m_sCalledPhoneNo = CompanyBoardNumber;
                m_OnCallMode = true;
                InitiateCall(CompanyBoardNumber);
            }
            catch 
            {
                this.HangUpCall();
            }
            finally
            {
                LogAnEvent(BrightVision.Common.Classes.EventLog.EventTypes.PRESS_CALL_SWITCH);
            }
        }

        private void btnCallDirect_Click(object sender, EventArgs e) 
        {
            if (SelectedContact == null) 
                return;

            try
            {
                ResetTimers();
                EnableControls(false);
                btnHangUp.Enabled = true;
                m_sCalledPhoneNo = SelectedContact.direct_phone;
                m_OnCallMode = true;
                InitiateCall(SelectedContact.direct_phone);
            }
            catch
            {
                this.HangUpCall();
            }
            finally
            {
                LogAnEvent(BrightVision.Common.Classes.EventLog.EventTypes.PRESS_CALL_DIRECT);
            }
        }

        private void btnCallMobile_Click(object sender, EventArgs e) {
            if (SelectedContact == null) 
                return;

            try
            {
                ResetTimers();
                EnableControls(false);
                btnHangUp.Enabled = true;
                m_sCalledPhoneNo = SelectedContact.mobile;
                m_OnCallMode = true;
                InitiateCall(SelectedContact.mobile);
            }
            catch 
            {
                this.HangUpCall();
            }
            finally
            {
                LogAnEvent(BrightVision.Common.Classes.EventLog.EventTypes.PRESS_CALL_MOBILE);
            }
        }

        private void btnCallNumber_Click(object sender, EventArgs e) 
        {
            if (txtEnterNumber.Text.Equals("Enter Number")) {
                NotificationDialog.Information("Bright Sales", "Please enter a number to call.");
                return;
            }

            try
            {
                ResetTimers();
                EnableControls(false);
                btnHangUp.Enabled = true;
                txtLogFirstName.Text = "Call";
                txtLogLastname.Text = "Log";
                m_sCalledPhoneNo = txtEnterNumber.Text.Trim();
                m_OnCallMode = true;
                InitiateCall(txtEnterNumber.Text.Trim());
            }
            catch
            {
                this.HangUpCall();
            }
        }

        private void btnChange_Click(object sender, EventArgs e) 
        {
            txtLogFirstName.Properties.ReadOnly = false;
            txtLogLastname.Properties.ReadOnly = false;
            txtLogFirstName.Text = "";
            txtLogLastname.Text = "";
            txtLogTitle.Text = "";
        }

        private void btnUseContact_Click(object sender, EventArgs e) 
        {
            if (SelectedContact == null) 
                return;

            txtLogFirstName.Text = SelectedContact.first_name;
            txtLogLastname.Text = SelectedContact.last_name;
        }
       
        private void txtEnterNumber_Enter(object sender, EventArgs e) {
            //if (txtEnterNumber.Text.Trim().ToLower() == "enter number")
            //    txtEnterNumber.Text = "";
            //else
            //    txtEnterNumber.SelectAll();
        }

        private void txtEnterNumber_Leave(object sender, EventArgs e) {
            //if (txtEnterNumber.Text.Trim() == "")
            //    txtEnterNumber.Text = "Enter Number";
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// hang up the current call
        /// </summary>
        private void HangUpCall() {
            timerCallLog.Stop();
            EnableControls(true);
            //DialogEditorModule.DialogSaveMode = DialogEditor.SaveMode.Unspecified;
            //DialogEditorModule.DialogSaveMode = DialogEditor.SaveMode.Unspecified;
            SetAvailableContactNumbers();
            m_tsCallStartTime = Convert.ToDateTime(esiLogCallStart.Text).TimeOfDay;
            m_tsCallEndTime = Convert.ToDateTime(esiLogCallEnd.Text).TimeOfDay;
            ResetTimers(false);

            if (m_OnCallMode && OnCallHangUp != null)
                OnCallHangUp(this, new CallerViewArgs() { ContactId = m_OnCallContactId });
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void ResetTimers(bool StartClockTick = true) {
            dtStart = DateTime.Now;
            dtEnd = DateTime.Now;

            esiLogCallStart.Text = dtStart.ToString("H:mm");
            esiLogCallEnd.Text = dtEnd.ToString("H:mm");

            if (StartClockTick)
                timerCallLog.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        private void InitiateCall(string number) 
        {
            if (SelectedContact == null)
                m_OnCallContactId = 0;
            else
                m_OnCallContactId = SelectedContact.id;

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = "SIP:" + number.ToSwedishPhoneNumber();
            proc.Start();
            proc.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled"></param>
        private void EnableControls(bool enabled) {
            txtEnterNumber.Enabled = enabled;
            btnCallBoard.Enabled = enabled;
            btnCallNumber.Enabled = enabled;
            btnCallDirect.Enabled = enabled;
            btnCallMobile.Enabled = enabled;
            btnHangUp.Enabled = false;
            //if (enabled) {
            //    DialogEditorModule.DialogSaveMode = DialogEditor.SaveMode.Edit;
            //}
        }

        private void LogAnEvent(BrightVision.Common.Classes.EventLog.EventTypes eType, params string[] values)
        {
            /** /
            EventQueue.Instance.AddWorkToQueue(new BrightVision.EventLog.EventMessage() {
                EventID = (int)eType,
                AccountID = AccountId,
                //ContactID = m_oContactView.SelectedContact.id,
                ContactID = SelectedContact != null ? SelectedContact.id : 0,
                SubCampaignID = SubCampaignId,
                UserID = UserSession.CurrentUser.UserId,
                ComputerName = UserSession.CurrentUser.ComputerName,
                LocalDateTime = DateTime.Now,
                Param1 = values.Length >= 1 ? values[0] : null,
                Param2 = values.Length >= 2 ? values[1] : null,
                Param3 = values.Length >= 3 ? values[2] : null,
                Param4 = values.Length >= 4 ? values[3] : null,
                Param5 = values.Length >= 5 ? values[4] : null,
                Param6 = values.Length >= 6 ? values[5] : null
            });
            /**/

            /**
             * update directly to avoid delayed grid data updated information.
             */
            //int? _contactId = null;
            //if (SelectedContact != null)
            //    if (SelectedContact.id > 0)
            //        _contactId = SelectedContact.id;

            //if (BPContext == null)
            //    BPContext = new BrightPlatformEntities(UserSession.EntityConnection);

            //BPContext.event_log.AddObject(
            //    new event_log() {
            //        event_id = (int)eType,
            //        user_id = UserSession.CurrentUser.UserId,
            //        subcampaign_id = SubCampaignId,
            //        account_id = AccountId,
            //        contact_id = _contactId,
            //        local_datetime = DateTime.Now,
            //        computer_name = UserSession.CurrentUser.ComputerName,
            //        param1 = null,
            //        param2 = null,
            //        param3 = null,
            //        param4 = null,
            //        param5 = null,
            //        param6 = null
            //    }
            //);

            //BPContext.SaveChanges();

            //if (CallAttemptMade != null)
            //    CallAttemptMade(this, new EventArgs());
        }
        #endregion

        #region public Methods
        public bool OnCallMode()
        {
            return m_OnCallMode;
        }

        public void SetCompanyRemarks(string pRemarks)
        {
            txtCompanyRemark.Text = pRemarks;
        }
        public void SetState()
        {
            //if (pMode == ManageCampaignBooking.eCampaignBookingMode.BrowseMode)
            //{
            //    this.HangUpCall();
            //    esiLogCallStart.Text = "00:00";
            //    esiLogCallEnd.Text = "00:00";
            //    esiLogLength.Text = "00:00";
            //    this.Enabled = false;
            //}
            //else
            //{
            //    this.Enabled = true;
            //    this.SetAvailableContactNumbers();
            //}
        }
        public void SetCallDirectButtonState(bool pState)
        {
            btnCallDirect.Enabled = pState;
        }
        public void SetCallMobileButtonState(bool pState)
        {
            btnCallMobile.Enabled = pState;
        }
        public void ResetCallViewFields() {
            txtLogFirstName.Properties.ReadOnly = true;
            txtLogLastname.Properties.ReadOnly = true;
            txtLogFirstName.Text = "";
            txtLogLastname.Text = "";
            txtLogTitle.Text = "";
            lblContact.Text = "";
            txtFollowUpShortMessage.Text = "";
            txtLogShortMessage.Text = "";
            cbLogCallStatus.SelectedIndex = 1;
            cbFollowUpStatus.Text = "Follow-Up Call";
            cboFollowUpAssignedTo.EditValue = UserSession.CurrentUser.UserId;
            cboFollowUpDate.EditValue = DateTime.Now;
            if (SelectedContact != null) {
                txtLogFirstName.Text = SelectedContact.first_name;
                txtLogLastname.Text = SelectedContact.last_name;
                txtLogTitle.Text = SelectedContact.title;
                lblContact.Text = String.Format("Contact: {0} {1}", SelectedContact.first_name, SelectedContact.last_name);
            }
            timerCallLog.Stop();
            this.ResetTimers(false);
        }
        
        //CheckContactNumbers is obsolete please use method below
        public void SetAvailableContactNumbers() {
            btnCallNumber.Enabled = true;
            btnCallBoard.Enabled = false;
            btnCallDirect.Enabled = false;
            btnCallMobile.Enabled = false;
            btnHangUp.Enabled = false;
            m_OnCallMode = false;

            if (!string.IsNullOrEmpty(CompanyBoardNumber))
                btnCallBoard.Enabled = true;

            if (SelectedContact == null) return;

            if (!string.IsNullOrEmpty(SelectedContact.direct_phone))
                btnCallDirect.Enabled = true;

            if (!string.IsNullOrEmpty(SelectedContact.mobile))
                btnCallMobile.Enabled = true;
        }

        //public void PopulateFollowUpAssignedToComboList() {
        //    try {
        //        cboFollowUpAssignedTo.Properties.Columns.Clear();
        //        cboFollowUpAssignedTo.Properties.DataSource = null;
        //        cboFollowUpAssignedTo.Properties.DataSource = SalesScript.GetUsers(FinalListId).Execute(MergeOption.AppendOnly);
        //        cboFollowUpAssignedTo.Properties.DisplayMember = "name";
        //        cboFollowUpAssignedTo.Properties.ValueMember = "id";
        //        cboFollowUpAssignedTo.Properties.Columns.Add(new LookUpColumnInfo("name"));
        //        cboFollowUpAssignedTo.EditValue = UserSession.CurrentUser.UserId;
        //    } catch { }
        //}
        #endregion

        #region Events & Delegates
        public delegate void CallLogAddingEventHandler(object sender, EventArgs e);
        public delegate void FollowUpLogSavingEventHandler(object sender, EventArgs e);
        public delegate void OnCallHangUpEventHandler(object sender, CallerViewArgs e);
        public delegate void CallAttemptMadeEventHandler(object sender, EventArgs e);
        public event CallLogAddingEventHandler OnCallLogAdding;
        public event FollowUpLogSavingEventHandler onFollowUpLogSaving;
        public event OnCallHangUpEventHandler OnCallHangUp;
        public event CallAttemptMadeEventHandler CallAttemptMade;
        #endregion

        #region Event Arguments
        public class CallerViewArgs : EventArgs
        {
            public int ContactId;
        }
        #endregion
    }
}
