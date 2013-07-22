
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;

using DevExpress.XtraEditors;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using BrightVision.Common.Classes;

using SalesConsultant.Business;
using BrightVision.Common.Events.Core;
using SalesConsultant.Events;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;

namespace SalesConsultant.Modules
{
    public partial class CallViewBar : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public CallViewBar()
        {
            InitializeComponent();            
        }
        #endregion

        #region Public Events & Arguments
        #endregion

        #region Public Properties
        public CTScSubCampaignContactList ContactPerson { get; set; }
        public string CompanyBoardNo { get; set; }
        public int SubCampaignId { get;set; }
        public int AccountId { get; set; }
        public int FinalListId { get; set; }

        public enum eCallMethod {
            Call_Board,
            Call_Direct,
            Call_Mobile,
            Call_Anonymous,
            None
        }

        public TimeSpan CallAttemptTime {
            get {
                return m_CallAttemptTime;
            }
        }

        //public bool SetStateCallDirect {
        //    set { btnCallDirect.Enabled = value; }
        //}
        //public bool SetStateCallMoBile {
        //    set { btnCallMobile.Enabled = value; }
        //}
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        private TimeSpan m_StartTime = new TimeSpan();
        private TimeSpan m_EndTime = new TimeSpan();
        private TimeSpan m_TimeElapsed = new TimeSpan();
        private TimeSpan m_CallAttemptTime = new TimeSpan();

        private string m_CalledPhoneNo = string.Empty;
        
        private int? m_OnCallContactId = null;

        private SelectionProperty.CallMethod m_CallMethod = SelectionProperty.CallMethod.None;
        private audio_settings m_UserAudioSetting = null;
        private Guid m_AudioId = Guid.Empty;
        #endregion

        #region Public Methods
		public void Clear()
        {
            AccountId = 0;
            ContactPerson = null;
            CompanyBoardNo = string.Empty;            
            this.Default(false);
        }
        public void SetContactNumbers()
        {
            m_BrightSalesProperty.CommonProperty.OnCallMode = false;
            btnCallBoard.Enabled = false;
            btnCallDirect.Enabled = false;
            btnCallMobile.Enabled = false;
            btnCallBoard.ToolTip = "";
            btnCallDirect.ToolTip = "";
            btnCallMobile.ToolTip = "";

            if (!string.IsNullOrEmpty(CompanyBoardNo))
            {
                btnCallBoard.Enabled = true;
                btnCallBoard.ToolTip = "Call Board: " + CompanyBoardNo;
            }

            if (ContactPerson == null) 
                return;

            if (!string.IsNullOrEmpty(ContactPerson.direct_phone))
            {
                btnCallDirect.Enabled = true;
                btnCallDirect.ToolTip = "Call Direct: " + ContactPerson.direct_phone;
            }

            if (!string.IsNullOrEmpty(ContactPerson.mobile))
            {
                btnCallMobile.Enabled = true;
                btnCallMobile.ToolTip = "Call Mobile: " + ContactPerson.mobile;
            }
        }
        public void SetState()
        {
            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse) {
                this.PhoneCallEnded();
                this.Enabled = false;
            }
            else {
                this.SetContactNumbers();
                this.Enabled = true;
            }
        }
      
        public void Default(bool pStartClockTick = false, bool pRetainCalledNo = false)
        {
            this.ResetTimers(pStartClockTick);
            if (!pStartClockTick)
                if (!pRetainCalledNo)
                    tbxNoToCall.Text = string.Empty;
        }
        public void PhoneCallStarted()
        {
            this.Default(true);
            m_OnCallContactId = null;
            m_BrightSalesProperty.CommonProperty.OnCallMode = true;
            if (ContactPerson != null)
                m_OnCallContactId = ContactPerson.id;
        }
        public void PhoneCallEnded()
        {
            m_EndTime = DateTime.Now.TimeOfDay;
            m_TimeElapsed = m_EndTime.Subtract(m_StartTime);
            m_StartTime = new TimeSpan();
            m_EndTime = new TimeSpan();
            m_BrightSalesProperty.CommonProperty.OnCallMode = false;
            m_CalledPhoneNo = null;

            tmrCallTimer.Stop();
            this.ChangeButtonState(true);
            this.SetContactNumbers();
        }

        public bool InitiateCallDirect()
        {
            if (btnCallDirect.Enabled) {
                btnCallDirect_Click(null, null);
                return true;
            }

            return false;
        }
        public bool InitiateCallBoard()
        {
            if (btnCallBoard.Enabled) {
                btnCallBoard_Click(null, null);
                return true;
            }

            return false;
        }
        public bool InitiateCallMobile()
        {
            if (btnCallMobile.Enabled) {
                btnCallMobile_Click(null, null);
                return true;
            }

            return false;
        }
        public bool InitiateCallAnonymous()
        {
            //if (tbxNoToCall.Enabled)
            //    return true;

            tbxNoToCall.Focus();
            return true;
        }
        #endregion

        #region Private Methods
        private void InitiatePhoneCall()
        {
            this.Default();
            m_CallAttemptTime = DateTime.Now.TimeOfDay;

            /**
             * https://brightvision.jira.com/browse/PLATFORM-2758
             */    
            //m_CalledPhoneNo = m_CalledPhoneNo.ToSwedishPhoneNumber();

            m_EventBus.Notify(new CallViewBarEvents.PhoneCallStart.FrmSalesConsultant() {
                PhoneCallArgs = new CallViewBarEvents.PhoneCallArguments() {
                    ContactPerson = ContactPerson,
                    ContactId = m_OnCallContactId,
                    ContactNo = m_CalledPhoneNo,
                    CallMethod = m_CallMethod
                }
            });
            m_EventBus.Notify(new CallViewBarEvents.PhoneCallStart.CallLogRemarks() {
                PhoneCallArgs = new CallViewBarEvents.PhoneCallArguments() {
                    ContactPerson = ContactPerson,
                    ContactId = m_OnCallContactId,
                    ContactNo = m_CalledPhoneNo,
                    CallMethod = m_CallMethod
                }
            });
            m_EventBus.Notify(new CallViewBarEvents.PhoneCallStart.CallLogBar() {
                PhoneCallArgs = new CallViewBarEvents.PhoneCallArguments() {
                    ContactPerson = ContactPerson,
                    ContactId = m_OnCallContactId,
                    ContactNo = m_CalledPhoneNo,
                    CallMethod = m_CallMethod
                }
            });
        }
        private void ChangeButtonState(bool pState)
        {
            tbxNoToCall.Enabled = pState;
            btnCallBoard.Enabled = pState;
            btnCallDirect.Enabled = pState;
            btnCallMobile.Enabled = pState;
        }
        private bool UserCanDoCall()
        {
            //if (!ObjectUser.CanDoCall()) {
            //    MessageBox.Show(
            //        string.Format("Your account is not yet configured for calling.{0}Please contact your administrator.", Environment.NewLine),
            //        "Bright Sales",
            //        MessageBoxButtons.OK,
            //        MessageBoxIcon.Information
            //    );
            //    return false;
            //} 

            if (m_BrightSalesProperty.CommonProperty.CurrentTab != SelectionProperty.CurrentTab.CampaignBooking)
                return false;

            if (!m_BrightSalesProperty.CommonProperty.CallLogSaved)
            {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return false;
            }
            
            m_UserAudioSetting = AudioSettingUtility.GetUserAudioSetting();
            /*
             * https://brightvision.jira.com/browse/PLATFORM-2375
             * Will only going to check if Phone setting is set to internal. 
            */
            if (m_UserAudioSetting == null)
            {
                audio_settings _item = (new BrightPlatformEntities(UserSession.EntityConnection)).audio_settings.FirstOrDefault(i => i.user_id == UserSession.CurrentUser.UserId);

                if ((_item != null && _item.mode == 0) || _item == null)
                {
                    NotificationDialog.Information("Bright Sales", "Your audio settings is not yet configured for calling.");
                    return false;
                }
            }

            return true;
        }
        private void SaveEventLog(EventLog.EventTypes pType)
        {
            BrightPlatformEntities m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            int? _ContactId = null;
            int? _SubCampaignId = null;
            int? _AccountId = null;

            if (ContactPerson != null) {
                if (ContactPerson.id > 0)
                    _ContactId = ContactPerson.id;
            }

            if (SubCampaignId > 0)
                _SubCampaignId = SubCampaignId;

            if (AccountId > 0)
                _AccountId = AccountId;

            m_efDbContext.event_log.AddObject(
                new event_log() {
                    event_id = (int)pType,
                    user_id = UserSession.CurrentUser.UserId,
                    subcampaign_id = _SubCampaignId,
                    account_id = _AccountId,
                    contact_id = _ContactId,
                    local_datetime = DateTime.Now,
                    computer_name = UserSession.CurrentUser.ComputerName,
                    param1 = null,
                    param2 = null,
                    param3 = null,
                    param4 = null,
                    param5 = null,
                    param6 = null
                }
            );

            m_efDbContext.SaveChanges();
            m_EventBus.Notify(new CallViewBarEvents.PhoneCallAttempt());

            //if (CallAttemptMade != null)
            //    CallAttemptMade(this, new EventArgs());
        }
        private void ResetTimers(bool pStartClockTick = true)
        {
            tbxCallUsageTime.Text = "00 : 00";
            m_StartTime = new TimeSpan();
            m_EndTime = new TimeSpan();
            m_TimeElapsed = new TimeSpan();

            if (pStartClockTick) {
                tmrCallTimer.Start();
                m_StartTime = DateTime.Now.TimeOfDay;
            }
        }
        #endregion

        #region Control Events
        private void btnCallBoard_Click(object sender, EventArgs e)
        {
            if (!this.UserCanDoCall())
                return;

            if (ValidationUtility.HasLetters(CompanyBoardNo)) {
                NotificationDialog.Error("Bright Sales", "No to call must not have letters on it.");
                return;
            }
            
            try {
                tbxNoToCall.Enabled = false;
                btnCallDirect.Enabled = false;
                btnCallMobile.Enabled = false;
                btnCallBoard.Enabled = false;
                btnCallBoard.Enabled = false; // workaround for non-disabling button
                //this.ChangeButtonState(false);
                m_CalledPhoneNo = CompanyBoardNo;
                //m_OnCallMode = true;
                m_CallMethod = SelectionProperty.CallMethod.Call_Board;
                this.InitiatePhoneCall();

                //this.Default(true);
                //if (tbxCallUsageTime_OnStartCall != null)
                //    tbxCallUsageTime_OnStartCall(this, new EventArgs());

                //this.StartCall(CompanyBoardNo);
                //if (btnCallBoard_OnClick != null)
                //    btnCallBoard_OnClick(this, new EventArgs());
            }
            catch {
                this.PhoneCallEnded();
            }
            finally {
                this.SaveEventLog(EventLog.EventTypes.PRESS_CALL_SWITCH);
            }
        }
        private void btnCallDirect_Click(object sender, EventArgs e)
        {
            if (!this.UserCanDoCall())
                return;

            if (ContactPerson == null)
                return;

            if (ValidationUtility.HasLetters(ContactPerson.direct_phone)) {
                NotificationDialog.Error("Bright Sales", "No to call must not have letters on it.");
                return;
            }
            try {
                tbxNoToCall.Enabled = false;
                btnCallBoard.Enabled = false;
                btnCallMobile.Enabled = false;
                btnCallDirect.Enabled = false;
                btnCallDirect.Enabled = false;
                //this.ChangeButtonState(false);
                m_CalledPhoneNo = ContactPerson.direct_phone;
                //m_OnCallMode = true;
                m_CallMethod = SelectionProperty.CallMethod.Call_Direct;
                this.InitiatePhoneCall();

                //this.Default(true);
                //if (tbxCallUsageTime_OnStartCall != null)
                //    tbxCallUsageTime_OnStartCall(this, new EventArgs());

                //if (btnCallDirect_OnClick != null)
                //    btnCallDirect_OnClick(this, new EventArgs());

                //this.StartCall(ContactPerson.direct_phone);
            }
            catch {
                this.PhoneCallEnded();
            }
            finally {
                this.SaveEventLog(BrightVision.Common.Classes.EventLog.EventTypes.PRESS_CALL_DIRECT);
            }
        }
        private void btnCallMobile_Click(object sender, EventArgs e)
        {
            if (!this.UserCanDoCall())
                return;

            if (ContactPerson == null)
                return;

            if (ValidationUtility.HasLetters(ContactPerson.mobile)) {
                NotificationDialog.Error("Bright Sales", "No to call must not have letters on it.");
                return;
            }
            try {
                tbxNoToCall.Enabled = false;
                btnCallBoard.Enabled = false;
                btnCallDirect.Enabled = false;
                btnCallMobile.Enabled = false;
                btnCallMobile.Enabled = false;
                //this.ChangeButtonState(false);
                m_CalledPhoneNo = ContactPerson.mobile;
                //m_OnCallMode = true;
                m_CallMethod = SelectionProperty.CallMethod.Call_Mobile;
                this.InitiatePhoneCall();

                //this.Default(true);
                //if (tbxCallUsageTime_OnStartCall != null)
                //    tbxCallUsageTime_OnStartCall(this, new EventArgs());

                //if (btnCallMobile_OnClick != null)
                //    btnCallMobile_OnClick(this, new EventArgs());

                //this.StartCall(ContactPerson.mobile);
            }
            catch {
                this.PhoneCallEnded();
            }
            finally {
                this.SaveEventLog(BrightVision.Common.Classes.EventLog.EventTypes.PRESS_CALL_MOBILE);
            }
        }
        private void tbxNoToCall_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            if (!this.UserCanDoCall())
                return;

            if (tbxNoToCall.Text.Equals("Enter Number") || tbxNoToCall.Text.Length < 1) {
                NotificationDialog.Information("Bright Sales", "Please enter a number to call.");
                return;
            }

            if (ValidationUtility.HasLetters(tbxNoToCall.Text.Trim())) {
                NotificationDialog.Error("Bright Sales", "No to call must not have letters on it.");
                return;
            }

            try {
                btnCallBoard.Enabled = false;
                btnCallDirect.Enabled = false;
                btnCallMobile.Enabled = false;
                tbxNoToCall.Enabled = false;
                tbxNoToCall.Enabled = false;
                //this.ChangeButtonState(false);
                m_CalledPhoneNo = tbxNoToCall.Text.Trim();
                //m_OnCallMode = true;
                m_CallMethod = SelectionProperty.CallMethod.Call_Anonymous;
                this.InitiatePhoneCall();
                
                //this.Default(true);
                //if (tbxCallUsageTime_OnStartCall != null)
                //    tbxCallUsageTime_OnStartCall(this, new EventArgs());

                //if (tbxNoToCall_OnKeyUp != null)
                //    tbxNoToCall_OnKeyUp(this, new EventArgs());

                //this.StartCall(tbxNoToCall.Text.Trim());
            }
            catch {
                this.PhoneCallEnded();
            }
            finally {
                this.SaveEventLog(BrightVision.Common.Classes.EventLog.EventTypes.PRESS_CALL_ANONYMOUS);
            }
        }
        private void tmrCallTimer_Tick(object sender, EventArgs e)
        {
            m_EndTime = DateTime.Now.TimeOfDay;
            m_TimeElapsed = m_EndTime.Subtract(m_StartTime);
            tbxCallUsageTime.Text = string.Format("{0} : {1}",
                m_TimeElapsed.Minutes.ToString().PadLeft(2, '0'),
                m_TimeElapsed.Seconds.ToString().PadLeft(2, '0')
            );
        }
        #endregion
    }
}
