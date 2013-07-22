
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Linq;

using DevExpress.XtraEditors;

using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Telephony.Business;
using BrightVision.Telephony.Utilities;
using BrightVision.Common.Utilities;

using Ozeki.Media;
using Ozeki.Media.MediaHandlers;
using Ozeki.Network.Nat;
using Ozeki.VoIP;
using Ozeki.VoIP.Media;
using Ozeki.VoIP.SDK;
using Ozeki.Common;
using SalesConsultant.Forms;
using DevExpress.Utils;

namespace SalesConsultant.Modules
{
    public partial class CallLogBar : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public CallLogBar()
        {
            InitializeComponent();
            this.Load += new EventHandler(CallLogBar_Load);
            
        }

        void CallLogBar_Load(object sender, EventArgs e)
        {
            m_SoftPhone = new FacadeSoftPhone(this.ParentForm);
            m_SoftPhone.CallState_Changed += new FacadeSoftPhone.CallStateChangedEventHandler(m_SoftPhone_CallState_Changed);
        }
        #endregion
        
        #region Public Events & Args
        public delegate void PhoneCallEndedEventHandler(object sender, PhoneCallEndedArgs e);
        public event PhoneCallEndedEventHandler PhoneCall_Ended;
        public class PhoneCallEndedArgs : EventArgs {
            public int? ContactId { get; set; }
        }

        public delegate void PhoneCallStartedEventHandler(object sender, EventArgs e);
        public event PhoneCallStartedEventHandler PhoneCall_Started;

        public delegate void CallBoardInitiatedEventHanlder(object sender, EventArgs e);
        public event CallBoardInitiatedEventHanlder CallBoard_Initiated;

        public delegate void CallDirectInitiatedEventHanlder(object sender, EventArgs e);
        public event CallDirectInitiatedEventHanlder CallDirect_Initiated;

        public delegate void CallMobileInitiatedEventHanlder(object sender, EventArgs e);
        public event CallMobileInitiatedEventHanlder CallMobile_Initiated;

        public delegate void CallAnonymousInitiatedEventHanlder(object sender, EventArgs e);
        public event CallAnonymousInitiatedEventHanlder CallAnonymous_Initiated;

        public delegate CTScSubCampaignContactList btnUseContactOnClickEventHandler(object sender, EventArgs e);
        public event btnUseContactOnClickEventHandler btnUseContact_OnClick;

        public class SaveCallLogArgs : EventArgs
        {
            public int Id { get; set; }
        }
        public delegate void btnSaveCallLogOnClickEventHandler(object sernder, SaveCallLogArgs e);
        public event btnSaveCallLogOnClickEventHandler btnSaveCallLog_OnClick;

        public delegate void EndCallInitiatedEventHandler(object sender, EventArgs e);
        public event EndCallInitiatedEventHandler EndCall_Initiated;
        #endregion

        #region Subscribed Events
        private void m_SoftPhone_CallState_Changed(object sender, FacadeSoftPhone.CallStateChangedArgs e)
        {
            if (e.PhoneCallState == CallState.InCall)
                this.Invoke(new MethodInvoker(delegate { this.StartCall(); }));

            else if (e.PhoneCallState == CallState.Completed) {
                m_AudioId = e.AudioId;
                this.Invoke(new MethodInvoker(delegate { this.EndCall(); }));
            }

            else if (e.PhoneCallState == CallState.Cancelled || e.PhoneCallState == CallState.Rejected)
                this.Invoke(new MethodInvoker(delegate { this.EndCall(); }));

            //else if (e.PhoneCallState == CallState.Error)
            //    this.Invoke(new MethodInvoker(delegate { this.EndCall(); }));
        }
        #endregion

        #region Public Properties
        public CallLogArgs CallLogBarParams = null;
        public class CallLogArgs {
            public int? ContactId { get; set; }
            public string ContactNo { get; set; }
            public eCallMethod CallMethod { get; set; }            
        }
        public enum eCallMethod {
            Call_Board,
            Call_Direct,
            Call_Mobile,
            Call_Anonymous,
            None
        }
        public PopupDialog phoneKeyboard;
        public CTScSubCampaignContactList ContactPerson { get; set; }
        public string CompanyBoardNo { get; set; }
        public int SubCampaignId { get; set; }
        public int AccountId { get; set; }
        public bool CallLogSaved { 
            get { return m_CallLogSaved; } 
        }
        #endregion

        #region Private Properties
        private BrightPlatformEntities m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);

        private TimeSpan m_StartTime = new TimeSpan();
        private TimeSpan m_EndTime = new TimeSpan();
        private TimeSpan m_TimeElapsed = new TimeSpan();
        
        private FacadeSoftPhone m_SoftPhone = null;

        private Guid m_AudioId = Guid.Empty;
        private bool m_OnCallMode = false;
        private bool m_CallLogSaved = true;
        private bool m_ShowKeyBoard = false;
        #endregion

        #region Public Methods
        public void Clear()
        {
            AccountId = 0;
            CallLogBarParams = null;
            ContactPerson = null;
            CompanyBoardNo = string.Empty;
            cboCallStatus.SelectedIndex = 0;
            this.Default();
        }
        public void AnonymousContactCallInitiated()
        {
            tbxContactTitle.Text = string.Empty;
            tbxContactFirstname.Text = "Call";
            tbxContactLastname.Text = "Board";
        }
        public void EndClock()
        {
            m_EndTime = DateTime.Now.TimeOfDay;
            m_TimeElapsed = m_EndTime.Subtract(m_StartTime);
            m_StartTime = new TimeSpan();
            m_EndTime = new TimeSpan();
            tmrCallTimer.Stop();
            m_OnCallMode = false;
            btnSaveCallLog.Enabled = true;
            btnHangUp.Enabled = false;
        }
        public void Default(bool pStartClockTick = false)
        {
            lciCallingFlag.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            lciTimer.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            tbxContactFirstname.Properties.ReadOnly = true;
            tbxContactLastname.Properties.ReadOnly = true;
            tbxContactTitle.Properties.ReadOnly = true;
            tbxContactFirstname.Text = string.Empty;
            tbxContactLastname.Text = string.Empty;
            tbxContactTitle.Text = string.Empty;
            tbxShortMessage.Text = string.Empty;
            cboCallStatus.SelectedIndex = 1;
            btnSaveCallLog.Enabled = true;
            btnHangUp.Enabled = false;

            if (ContactPerson != null) {
                tbxContactFirstname.Text = ContactPerson.first_name;
                tbxContactLastname.Text = ContactPerson.last_name;
                tbxContactTitle.Text = ContactPerson.title;
            }

            tmrCallTimer.Stop();
            this.ResetTimer(pStartClockTick);
        }
        public void InitiatePhoneCall()
        {
            lciCallingFlag.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            lciTimer.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            tbxContactFirstname.Properties.ReadOnly = true;
            tbxContactLastname.Properties.ReadOnly = true;
            tbxContactTitle.Properties.ReadOnly = true;
            tbxContactFirstname.Text = string.Empty;
            tbxContactLastname.Text = string.Empty;
            tbxContactTitle.Text = string.Empty;
            tbxShortMessage.Text = string.Empty;
            cboCallStatus.SelectedIndex = 1;
            btnSaveCallLog.Enabled = false;
            btnHangUp.Enabled = true;

            if (ContactPerson != null) {
                tbxContactFirstname.Text = ContactPerson.first_name;
                tbxContactLastname.Text = ContactPerson.last_name;
                tbxContactTitle.Text = ContactPerson.title;
            }

            tmrCallTimer.Stop();
            this.ResetTimer();
            m_ShowKeyBoard = false;

            /**
             * call mode:
             * 0 = built-in sip phone
             * 1 = external sip phone
             */
            int _CallMode = 1;
            int _UserId = UserSession.CurrentUser.UserId;
            m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            audio_settings _item = m_efDbContext.audio_settings.FirstOrDefault(i => i.user_id == _UserId);
            if (_item != null)
                _CallMode = _item.mode;

            if (_CallMode == 0) {
                m_SoftPhone.CreateCall(CallLogBarParams.ContactNo.ToSwedishPhoneNumber(), 30);
                m_ShowKeyBoard = true;
            }
            else if (_CallMode == 1) {
                System.Diagnostics.Process proc = new System.Diagnostics.Process {
                    EnableRaisingEvents = false
                };
                proc.StartInfo.FileName = string.Format("SIP:{0}", CallLogBarParams.ContactNo.ToSwedishPhoneNumber());
                proc.Start();
                proc.Close();
                this.StartCall();
            }

            m_CallLogSaved = false;
        }
        public void KillClockThread()
        {
            if (m_SoftPhone != null)
            {
                try
                {
                    m_SoftPhone.KillProcesses();
                }
                catch { }
            }
        }
        #endregion

        #region Private Methods
        private void ResetTimer(bool pStartClockTick = false)
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
        private void StartCall()
        {
            m_OnCallMode = true;
            if (PhoneCall_Started != null)
                PhoneCall_Started(this, new EventArgs());

            //this.Default(true); //commented, conflicts logic when making a call
            switch (CallLogBarParams.CallMethod) 
            {
                case eCallMethod.Call_Board:
                    if (CallBoard_Initiated != null) {
                        tbxContactTitle.Text = string.Empty;
                        tbxContactFirstname.Text = "Call";
                        tbxContactLastname.Text = "Board";
                        CallBoard_Initiated(this, new EventArgs());
                    }
                    break;

                case eCallMethod.Call_Direct:
                    if (CallDirect_Initiated != null) {
                        tbxContactTitle.Text = ContactPerson.title;
                        tbxContactFirstname.Text = ContactPerson.first_name;
                        tbxContactLastname.Text = ContactPerson.last_name;
                        CallDirect_Initiated(this, new EventArgs());
                    }
                    break;

                case eCallMethod.Call_Mobile:
                    if (CallMobile_Initiated != null) {
                        tbxContactTitle.Text = ContactPerson.title;
                        tbxContactFirstname.Text = ContactPerson.first_name;
                        tbxContactLastname.Text = ContactPerson.last_name;
                        CallMobile_Initiated(this, new EventArgs());
                    }
                    break;

                case eCallMethod.Call_Anonymous:
                    if (CallAnonymous_Initiated != null) {
                        tbxContactTitle.Text = string.Empty;
                        tbxContactFirstname.Text = "Call";
                        tbxContactLastname.Text = "Log";
                        CallAnonymous_Initiated(this, new EventArgs());
                    }
                    break;
            }

            if (m_ShowKeyBoard)
                ShowKeyboard();
        }

        private void ShowKeyboard()
        {
            PhoneKeyboard keyboard = new PhoneKeyboard();
            keyboard.KeyUp += new MouseUpPhoneKeyboardHandler(keyboard_KeyUp);
            keyboard.KeyDown += new MouseDownPhoneKeyboardHandler(keyboard_KeyDown);
            phoneKeyboard = new PopupDialog(keyboard);
            phoneKeyboard.MinimizeBox = false;
            phoneKeyboard.MaximizeBox = false;
            phoneKeyboard.StartPosition = FormStartPosition.Manual;
            Point locationOnForm = btnKeyPad.FindForm().PointToClient(btnKeyPad.Parent.PointToScreen(btnKeyPad.Location));
            Point loc = new Point(locationOnForm.X-200, locationOnForm.Y + 30);
            phoneKeyboard.Location = loc;
            phoneKeyboard.Owner = this.ParentForm;
            phoneKeyboard.Show();
        }
        private void EndCall()
        {
            if (lciCallingFlag.Visibility == DevExpress.XtraLayout.Utils.LayoutVisibility.Always) {
                lciCallingFlag.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                lciTimer.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            }

            this.EndClock();
            if (PhoneCall_Ended != null)
                PhoneCall_Ended(this, new PhoneCallEndedArgs() {
                    ContactId = CallLogBarParams.ContactId
                });

            if(phoneKeyboard != null)
                phoneKeyboard.Close();
        }
        #endregion

        #region Control Events
        private void keyboard_KeyDown(int dtmf)
        {
            m_SoftPhone.StartDTMF(dtmf);
        }
        private void keyboard_KeyUp(int dtmf)
        {
            m_SoftPhone.StopDTMF(dtmf);
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
        private void btnSaveCallLog_Click(object sender, EventArgs e)
        {
            if (tbxContactFirstname.Text.Length < 1 || tbxContactLastname.Text.Length < 1) {
                MessageBox.Show("Firstname / Lastname is required.", "Add Call Log", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            else if (tbxContactFirstname.Text.Equals("First name") || tbxContactLastname.Text.Equals("Last name")) {
                MessageBox.Show("Firstname / Lastname is required.", "Add Call Log", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            WaitDialog.Show("Saving data...");
            this.EndClock();
            if (EndCall_Initiated != null)
                EndCall_Initiated(this, new EventArgs());

            int? _ContactId = null;
            if (ContactPerson != null)
                if (ContactPerson.id > 0)
                    _ContactId = ContactPerson.id;

            string _ContactName = (string.Format("{0} {1}", tbxContactFirstname.Text, tbxContactLastname.Text)).Equals("Call Log") ? "Switchboard" : string.Format("{0} {1}", tbxContactFirstname.Text, tbxContactLastname.Text);
            event_followup_log newevent_followup_log = new event_followup_log() {
                            subcampaign_id = SubCampaignId,
                            account_id = AccountId,
                            contact_id = _ContactId,
                            title = tbxContactTitle.Text,
                            contact_no = CallLogBarParams.ContactNo,
                            short_message = tbxShortMessage.Text,
                            event_type = "Call Log",
                            event_status = cboCallStatus.Text,
                            date_of_transaction = DateTime.Now,
                            assigned_user = UserSession.CurrentUser.UserId,
                            done = true,
                            start_time = m_StartTime,
                            end_time = m_EndTime,
                            date_created = DateTime.Now,
                            created_by = UserSession.CurrentUser.UserId,
                            contact_name = _ContactName,
                            audio_id = m_AudioId
                        };
            m_efDbContext.event_followup_log.AddObject(newevent_followup_log);
            m_AudioId = Guid.Empty;
            m_CallLogSaved = true;
            int id = newevent_followup_log.id;
            m_efDbContext.SaveChanges();
            var args = new SaveCallLogArgs { Id = newevent_followup_log.id };
            if (btnSaveCallLog_OnClick != null)
                btnSaveCallLog_OnClick(sender, args);

            this.Default();
            WaitDialog.Close();
        }
        private void btnInList_Click(object sender, EventArgs e)
        {
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
            tbxContactFirstname.Properties.ReadOnly = false;
            tbxContactLastname.Properties.ReadOnly = false;
            tbxContactTitle.Properties.ReadOnly = false;
            tbxContactFirstname.Text = "";
            tbxContactLastname.Text = "";
            tbxContactTitle.Text = "";
            tbxContactTitle.Focus();
        }
        private void btnHangUp_Click(object sender, EventArgs e)
        {
            lciCallingFlag.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            lciTimer.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;

            /**
             * call mode:
             * 0 = built-in sip phone
             * 1 = external sip phone
             */
            int _UserId = UserSession.CurrentUser.UserId;
            int _CallMode = m_efDbContext.audio_settings.FirstOrDefault(i => i.user_id == _UserId).mode;
            if (_CallMode == 0)
                m_SoftPhone.StopCall();
            
            this.EndCall();
        }
        #endregion
    }
}