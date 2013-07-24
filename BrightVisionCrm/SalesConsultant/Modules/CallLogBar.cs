
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
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;

using Ozeki.Media;
using Ozeki.Media.MediaHandlers;
using Ozeki.Network.Nat;
using Ozeki.VoIP;
using Ozeki.VoIP.Media;
using Ozeki.VoIP.SDK;
using Ozeki.Common;
using SalesConsultant.Forms;
using DevExpress.Utils;
using SalesConsultant.Business;
using BrightVision.Common.Events.Core;
using SalesConsultant.Events;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;

namespace SalesConsultant.Modules
{
    public partial class CallLogBar : DevExpress.XtraEditors.XtraUserControl
    {
        #region Instance
        /* https://brightvision.jira.com/browse/PLATFORM-2665
            * DAN: Added to update the m_SoftPhone to solve the scenario issue on ticket.
            * It happen because the variable m_SoftPhone.RegisterSuccess is false even thought the user has updated the audio settings.
        */
        private static CallLogBar instance = null;
        public static CallLogBar InstanceNoParam
        {
            get
            {
                return instance;
            }
        }
        #endregion

        #region Constructors
        public CallLogBar()
        {
            InitializeComponent();
            this.Load += new EventHandler(CallLogBar_Load);
            btnMicrophone.ToolTip = "Microphone On";
            this.RegisterEvents();
            instance = this;
        }
        #endregion

        #region Subscribed Events
        private void m_SoftPhone_CallState_Changed(object sender, FacadeSoftPhone.CallStateChangedArgs e)
        {
            if (e.PhoneCallState == CallState.InCall) {
                m_InCall = true;
                this.Invoke(new MethodInvoker(delegate { this.StartCall(); }));
            }

            else if (e.PhoneCallState == CallState.Completed) {
                m_InCall = false;
                m_AudioId = e.AudioId;
                this.Invoke(new MethodInvoker(delegate { this.EndCall(); }));
            }

            else if (e.PhoneCallState == CallState.Cancelled || e.PhoneCallState == CallState.Rejected) {
                m_InCall = false;
                this.Invoke(new MethodInvoker(delegate { this.EndCall(); }));
            }

            //else if (e.PhoneCallState == CallState.Error)
            //    this.Invoke(new MethodInvoker(delegate { this.EndCall(); }));
        }
        private void m_SoftPhone_PhoneRegisterSuccess()
        {
            m_EventBus.Notify(new PhoneRegisterSuccessEventNotifier() {
                NotificationMessage = "Phone register successful."
            });
        }
        #endregion

        #region Public Properties
        private BrightSalesProperty bsProperty = BrightSalesFacade.Property;
        public CallLogArgs CallLogBarParams = null;
     
        public PopupDialog phoneKeyboard;
        public CTScSubCampaignContactList ContactPerson { get; set; }
        public string CompanyBoardNo { get; set; }
        public int SubCampaignId { get; set; }
        public int AccountId { get; set; }
        public bool PhoneRegisterSuccess {
            get { return m_PhoneRegisterSuccess; }
        }
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private TimeSpan m_StartTime = new TimeSpan();
        private TimeSpan m_EndTime = new TimeSpan();
        private TimeSpan m_TimeElapsed = new TimeSpan();
        
        public FacadeSoftPhone m_SoftPhone = null;

        private Guid m_AudioId = Guid.Empty;
        private bool m_OnCallMode = false;
       
        private bool m_ShowKeyBoard = false;

        private bool m_MicrophoneOnMuteMode = false;
        private bool m_InCall = false;
        private PhoneCallEndEventNotifier m_PhoneCallEndedEvent = null;

        private bool m_PhoneRegisterSuccess = false;
        #endregion

        #region Public Methods
        public void HangUpCallIfExist()
        {
            if (lciCallingFlag.Visibility == DevExpress.XtraLayout.Utils.LayoutVisibility.Always)
                btnHangUp_Click(null, null);
        }
        public void UnRegisterPhone()
        {
            if (m_SoftPhone != null)
            {
                m_SoftPhone.CallState_Changed -= new FacadeSoftPhone.CallStateChangedEventHandler(m_SoftPhone_CallState_Changed);
                m_SoftPhone.PhoneRegisterSuccess -= new FacadeSoftPhone.PhoneRegisterSuccessEventHandler(m_SoftPhone_PhoneRegisterSuccess);
                m_SoftPhone.UnRegisterPhone();
                m_PhoneRegisterSuccess = false;
            }
        }
        public void RegisterPhone()
        {
            m_SoftPhone = new FacadeSoftPhone(this.ParentForm);
            m_SoftPhone.CallState_Changed += new FacadeSoftPhone.CallStateChangedEventHandler(m_SoftPhone_CallState_Changed);
            m_SoftPhone.PhoneRegisterSuccess += new FacadeSoftPhone.PhoneRegisterSuccessEventHandler(m_SoftPhone_PhoneRegisterSuccess);
            m_SoftPhone.RegisterPhone();
        }
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
            m_PhoneCallEndedEvent = new PhoneCallEndEventNotifier();
            m_EndTime = DateTime.Now.TimeOfDay;
            m_TimeElapsed = m_EndTime.Subtract(m_StartTime);
            m_PhoneCallEndedEvent.CallStart = m_StartTime;
            m_PhoneCallEndedEvent.CallEnd = m_EndTime;
            //m_StartTime = new TimeSpan();
            //m_EndTime = new TimeSpan();
            tmrCallTimer.Stop();
            m_OnCallMode = false;
            btnSaveCallLog.Enabled = true;
            //btnHangUp.Enabled = false;
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
            /**
             * call mode:
             * 0 = built-in sip phone
             * 1 = external sip phone
             */
            int _CallMode = 1;
            int _UserId = UserSession.CurrentUser.UserId;
            audio_settings _item = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                //m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
                _item = _efDbContext.audio_settings.FirstOrDefault(i => i.user_id == _UserId);
                _efDbContext.Detach(_item);
            }
            //if (_CallMode == 0 && !m_SoftPhone.RegisterSuccess) {
            //    MessageNotification(new MessageNotificationArgs() { 
            //        Message = "Phone register failed."
            //    });
                
            //}

            /*
             * https://brightvision.jira.com/browse/PLATFORM-3194
             */
            //lblCallingFlag.Text = string.Format("Calling {0}", CallLogBarParams.ContactNo.ToSwedishPhoneNumber());
            lblCallingFlag.Text = "";

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

            //layoutControlItem9
            btnHangUp.Enabled = true;
            
            if (ContactPerson != null) {
                tbxContactFirstname.Text = ContactPerson.first_name;
                tbxContactLastname.Text = ContactPerson.last_name;
                tbxContactTitle.Text = ContactPerson.title;
            }

            tmrCallTimer.Stop();
            this.ResetTimer();
            m_ShowKeyBoard = false;
            
            if (_item != null)
                _CallMode = _item.mode;

            if (_CallMode == 0) {
                if (!m_SoftPhone.RegisterSuccess)
                {
                    //MessageBox.Show("Please ask your administrator for your sip account. This may also be cause by the voip service provider not available.");
                    //MessageBox.Show(
                    //    string.Format("Your account is not yet configured for calling.{0}Please contact your administrator.", Environment.NewLine),
                    //    "Bright Sales",
                    //    MessageBoxButtons.OK,
                    //    MessageBoxIcon.Information
                    //);

                    NotificationDialog.Error("Bright Sales", string.Format("Your account is not yet configured for calling.{0}Please contact your administrator.", Environment.NewLine));

                    try
                    {
                        m_EventBus.Notify<CallLogBarVoidEvent>(CallLogBarVoidEvent.CallNotConnected);
                    }
                    catch
                    { }

                    btnHangUp.PerformClick();
                    return;
                }
                m_SoftPhone.CreateCall(CallLogBarParams.ContactNo.ToSwedishPhoneNumber(), 30);
                m_ShowKeyBoard = true;
            }
            else if (_CallMode == 1) {
                System.Diagnostics.Process proc = new System.Diagnostics.Process {
                    EnableRaisingEvents = false
                };
                try
                {
                    proc.StartInfo.FileName = string.Format("SIP:{0}", CallLogBarParams.ContactNo.ToSwedishPhoneNumber());
                    proc.Start();
                    proc.Close();
                    this.StartCall();
                }
                catch
                {
                    proc.Close();
                }
            }
            bsProperty.CommonProperty.CallLogSaved = false;
           
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
        public TimeSpan StartTime {
            get { return m_StartTime; }
        }
        public TimeSpan EndTime {
            get { return m_EndTime; }
        }
        public Guid AudioId {
            get { return m_AudioId; }
            set { m_AudioId = value; }
        }
        #endregion

        #region Private Methods
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<CallViewBarEvents.PhoneCallStart.CallLogBar>().Subscribe(PhoneCallStart);
        }
        private void PhoneCallStart(CallViewBarEvents.PhoneCallStart.CallLogBar e)
        {
            CallLogBarParams = new CallLogArgs()  { 
                ContactId = e.PhoneCallArgs.ContactId,
                ContactNo = e.PhoneCallArgs.ContactNo,
                CallMethod = e.PhoneCallArgs.CallMethod 
            };
            ContactPerson = e.PhoneCallArgs.ContactPerson;
            this.InitiatePhoneCall();
        }
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
            lciCallingFlag.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            lciTimer.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            m_OnCallMode = true;

            m_EventBus.Notify<CallLogBarVoidEvent>(CallLogBarVoidEvent.PhoneCallStarted);

            //if (PhoneCall_Started != null)
            //    PhoneCall_Started(this, new EventArgs());

            tmrCallTimer.Stop();
            this.ResetTimer(true);

            //this.Default(true); //commented, conflicts logic when making a call
            switch (CallLogBarParams.CallMethod)  
            {
                case SelectionProperty.CallMethod.Call_Board:
                    m_EventBus.Notify<CallLogBarVoidEvent>(CallLogBarVoidEvent.CallBoardInitiated);
                    tbxContactTitle.Text = string.Empty;
                    tbxContactFirstname.Text = "Call";
                    tbxContactLastname.Text = "Board";
                    break;

                case SelectionProperty.CallMethod.Call_Direct:
                    m_EventBus.Notify<CallLogBarVoidEvent>(CallLogBarVoidEvent.CallDirectInitiated);
                    tbxContactTitle.Text = ContactPerson.title;
                    tbxContactFirstname.Text = ContactPerson.first_name;
                    tbxContactLastname.Text = ContactPerson.last_name;
                    break;

                case SelectionProperty.CallMethod.Call_Mobile:
                    m_EventBus.Notify<CallLogBarVoidEvent>(CallLogBarVoidEvent.CallMobileInitiated);
                    tbxContactTitle.Text = ContactPerson.title;
                    tbxContactFirstname.Text = ContactPerson.first_name;
                    tbxContactLastname.Text = ContactPerson.last_name;
                    break;

                case SelectionProperty.CallMethod.Call_Anonymous:
                    m_EventBus.Notify<CallLogBarVoidEvent>(CallLogBarVoidEvent.CallAnonymousInitiated);
                    tbxContactTitle.Text = string.Empty;
                    tbxContactFirstname.Text = "Call";
                    tbxContactLastname.Text = "Log";
                    break;
            }

            //if (m_ShowKeyBoard)
            //    ShowKeyboard();
        }
        private void ShowKeyboard()
        {
            if (!m_InCall)
                return;

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
            m_PhoneCallEndedEvent.ContactId = CallLogBarParams.ContactId;
            m_EventBus.Notify(new PhoneCallEndEventNotifier() {
                ContactId = m_PhoneCallEndedEvent.ContactId,
                CallStart = m_PhoneCallEndedEvent.CallStart,
                CallEnd = m_PhoneCallEndedEvent.CallEnd
            });
            if(phoneKeyboard != null)
                phoneKeyboard.Close();
        }
        #endregion

        #region Control Events
        private void CallLogBar_Load(object sender, EventArgs e)
        {
            //BrightSalesFacade.EventBus.Notify<object>(new object());
            //m_SoftPhone = new FacadeSoftPhone(this.ParentForm);
            //m_SoftPhone.CallState_Changed += new FacadeSoftPhone.CallStateChangedEventHandler(m_SoftPhone_CallState_Changed);
            //m_SoftPhone.PhoneRegisterSuccess += new FacadeSoftPhone.PhoneRegisterSuccessEventHandler(m_SoftPhone_PhoneRegisterSuccess);
            //m_SoftPhone.RegisterPhone();
        }
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

            m_BrightSalesProperty.CommonProperty.OnCallMode = true;
        }
        private void btnSaveCallLog_Click(object sender, EventArgs e)
        {
            if (tbxContactFirstname.Text.Length < 1 || tbxContactLastname.Text.Length < 1) {
                NotificationDialog.Information("Bright Sales", "Firstname / Lastname is required.");
                return;
            }
            else if (tbxContactFirstname.Text.Equals("First name") || tbxContactLastname.Text.Equals("Last name")) {
                NotificationDialog.Information("Bright Sales", "Firstname / Lastname is required.");
                return;
            }

            WaitDialog.Show("Saving data...");
            this.EndClock();
            m_EventBus.Notify<CallLogBarVoidEvent>(CallLogBarVoidEvent.EndCallInitiated);

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
            BrightPlatformEntities m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            m_efDbContext.event_followup_log.AddObject(newevent_followup_log);
            m_AudioId = Guid.Empty;

            bsProperty.CommonProperty.CallLogSaved = false;
            
            int id = newevent_followup_log.id;
            m_efDbContext.SaveChanges();
            m_EventBus.Notify(new PhoneCallSaveEventNotifier() {
                EventLogId = newevent_followup_log.id
            });

            this.Default();
            WaitDialog.Close();
        }
        private void btnInList_Click(object sender, EventArgs e)
        {
            //ContactPerson = btnUseContact_OnClick(this, new EventArgs());
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

            /*
            using (BrightPlatformEntities m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                int _UserId = UserSession.CurrentUser.UserId;
                int _CallMode = m_efDbContext.audio_settings.FirstOrDefault(i => i.user_id == _UserId).mode;
                if (_CallMode == 0)
                    m_SoftPhone.StopCall();
            }
            */
            if (BrightSalesFacade.Property.CommonProperty.CallModeAudioSettings == 0)
                m_SoftPhone.StopCall();

            this.EndCall();
        }
        private void btnKeyPad_Click(object sender, EventArgs e)
        {
            if (m_ShowKeyBoard)
                this.ShowKeyboard();
        }
        private void btnMicrophone_Click(object sender, EventArgs e)
        {
            if (btnMicrophone.ToolTip == "Microphone On") {
                //AudioSettingUtility.MuteMicrophone();
                m_SoftPhone.MuteMic();
                btnMicrophone.ToolTip = "Microphone Off";
                btnMicrophone.Image = global::SalesConsultant.Properties.Resources.mic_mute;
            }
            else {
                //AudioSettingUtility.UnMuteMicrophone();
                m_SoftPhone.UnMuteMic();
                btnMicrophone.ToolTip = "Microphone On";
                btnMicrophone.Image = global::SalesConsultant.Properties.Resources.mic;
            }
        }
        #endregion
    }
}