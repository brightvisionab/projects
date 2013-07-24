
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;

using Ozeki.Media;
using Ozeki.Media.MediaHandlers;
using Ozeki.Network.Nat;
using Ozeki.VoIP;
using Ozeki.VoIP.Media;
using Ozeki.VoIP.SDK;
using Ozeki.Common;
using System.IO;
using System.Threading;
namespace BrightVision.Telephony.Business
{

    public class FacadeSoftPhone : Form
    {
        #region Constructors
        public FacadeSoftPhone()
        {
            this.InitializeComponent();
            //m_RingTimer = new Timer() {
            //    Enabled = true,
            //    Interval = 1000                
            //};
            //m_RingTimer.Tick += new EventHandler(m_RingTimer_Tick);
        }
        public FacadeSoftPhone(Form parentForm)
        {
            SaleForm = parentForm;
            this.InitializeComponent();
            //m_RingTimer = new Timer() {
            //    Enabled = true,
            //    Interval = 1000                
            //};
            //m_RingTimer.Tick += new EventHandler(m_RingTimer_Tick);
            workDelayRecording = new BackgroundWorker();
            workDelayRecording.DoWork += new DoWorkEventHandler(workDelayRecording_DoWork);
            workDelayRecording.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workDelayRecording_RunWorkerCompleted);
        }

        #endregion

        #region Public Events & Args
        public delegate void PhoneRegisterSuccessEventHandler();
        public event PhoneRegisterSuccessEventHandler PhoneRegisterSuccess;

        public delegate void CallStateChangedEventHandler(object sender, CallStateChangedArgs e);
        public event CallStateChangedEventHandler CallState_Changed;
        public class CallStateChangedArgs : EventArgs {
            public CallState PhoneCallState;
            public Guid AudioId;
        }
        BackgroundWorker workDelayRecording;
        #endregion

        #region Public Properties
        public bool RegisterSuccess {
            get { return m_PhoneRegisterSuccessful; }
        }
        #endregion

        #region Private Properties
        Form SaleForm;

        private string m_OzekiLicenseId = "OZSDK-BRI2CALL-120712-9D26513C";
        private string m_OzekiLicenseKey = "TUNDOjIsTVBMOjIsRzcyOTp0cnVlLE1TTEM6MixNRkM6MixVUDoyMDEzLjA3LjEyLFA6MjE5OS4wMS4wMXx4OVJpbjdaOTg5MTZRWGVHRm82STVqcVJkU0psNVl1WWVPcDA1Mm5RV1k0UUVub29HWlIyTDZoakMwcnk3bk9HdWJvK09xdHhFU0trWCsvZkJwVzNSZz09";

        Guid AudioId = Guid.Empty;

        private ISoftPhone softPhone;
        private IPhoneLine phoneLine;
        private PhoneLineState phoneLineInformation;
        private IPhoneCall call;

        private audio_settings m_UserAudioSetting = null;
        private Microphone m_UserMicrophone = AudioSettingUtility.GetDefaultDeviceMicrophone();// Microphone.GetDefaultDevice();
        private Speaker m_UserSpeaker = AudioSettingUtility.GetDefaultDeviceSpeaker(); // Speaker.GetDefaultDevice();

        private MediaConnector connector = new MediaConnector();
        private PhoneCallAudioSender mediaSender = new PhoneCallAudioSender();
        private PhoneCallAudioReceiver mediaReceiver = new PhoneCallAudioReceiver();
       
        WaveStreamRecorder recorder;
        WaveStreamRecorder recorderMic;
        WaveStreamRecorder recorderReceiver;

        AudioMixerMediaHandler mixer = new AudioMixerMediaHandler();
        AudioMixerMediaHandler mixerMic = null;
        AudioMixerMediaHandler mixerReceiver =null;  

        private bool inComingCall;
        private MediaUtility m_MediaUtility = new MediaUtility();
        private string m_PhoneNo = string.Empty;

        private System.Windows.Forms.Timer m_RingTimer;
        private TimeSpan m_TimeStart;
        private TimeSpan m_TimeEnd;
        private TimeSpan m_TimeElapsed;

        private bool m_RingTimeOut = false;
        private bool m_StartTimeOutCounter = false;
        private IContainer components;
        private System.Windows.Forms.Timer timer1;
        private int m_RingingTimeOut = 0;

        private bool m_PhoneRegisterSuccessful = false;
        #endregion

        #region Control Events
        private void m_RingTimer_Tick(object sender, EventArgs e)
        {
            if (m_StartTimeOutCounter) {
                m_TimeEnd = DateTime.Now.TimeOfDay;
                m_TimeElapsed = m_TimeEnd.Subtract(m_TimeStart);
                if (m_TimeElapsed.Seconds == m_RingingTimeOut)
                    this.call_CallStateChanged(null, new VoIPEventArgs<CallState>(CallState.Error));
            }
        }
        #endregion

        #region Public Methods
        public bool IsMicrophoneDeviceOk()
        {
            Microphone _device = Microphone.GetDefaultDevice();
            if (_device == null)
                return false;

            return true;
        }
        public static bool MicrophoneDeviceOk()
        {
            Microphone _device = Microphone.GetDefaultDevice();
            if (_device == null)
                return false;

            return true;
        }
        public static bool SpeakerDeviceOk()
        {
            Speaker _device = Speaker.GetDefaultDevice();
            if (_device == null)
                return false;

            return true;
        }
        public void UnRegisterPhone()
        {
            if (softPhone != null)
            {
                if(phoneLine != null)
                    softPhone.UnregisterPhoneLine(phoneLine);

                softPhone.Close();   
            }
            this.WireDownCallEvents();
            call = null;
        }
        public void RegisterPhone()
        {
            this.InitializeSoftPhone();
        }
        public void CreateCall(string pPhoneNo, int pPhoneRingTimeOut = 30)
        {
            if (pPhoneRingTimeOut > 0)
                m_RingingTimeOut = pPhoneRingTimeOut;

            m_RingTimeOut = false;
            m_PhoneNo = pPhoneNo;


            m_UserMicrophone.Start();
            connector.Connect(m_UserMicrophone, mediaSender);

            AudioId = Guid.NewGuid();
            CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
            m_UserMicrophone.Start();
            string fileNameTmp = String.Format(@"{0}\tmpwav\{1}_.wav", commonData.ApplicationFolderPath, AudioId);
            recorder = new WaveStreamRecorder(fileNameTmp);
            recorder.Stopped += new EventHandler<EventArgs>(recorder_Stopped);

            connector.Connect(m_UserMicrophone, mixer);
            connector.Connect(mediaReceiver, mixer);
            connector.Connect(mixer, recorder);
            m_UserSpeaker.Start();
            connector.Connect(mediaReceiver, m_UserSpeaker);
            
            call = softPhone.CreateCallObject(phoneLine, m_PhoneNo);
            mediaSender.AttachToCall(call);
            mediaReceiver.AttachToCall(call);

            this.WireUpCallEvents();
            //System.Threading.Thread.Sleep(1500);
            call.Start();
            //softPhone.RegisterPhoneLine(phoneLine);
            //this.InitializeSoftPhone();
        }
        public void StopCall()
        {
            m_RingTimeOut = false;
            if (call != null)
            {
                if (inComingCall && call.CallState == CallState.Ringing)
                    call.Reject();
                else
                    call.HangUp();

                inComingCall = false;
                //softPhone.UnregisterPhoneLine(phoneLine);
                softPhone.Close();
                this.WireDownCallEvents();
                call = null;
            }
            else
            {
                if (CallState_Changed != null)
                {
                    CallState_Changed(this, new CallStateChangedArgs()
                    {
                        PhoneCallState =  CallState.Cancelled 
                    });
                }
                m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.HangUp);

            }
            m_MediaUtility.Stop();
            if (softPhone != null) {
                //softPhone.UnregisterPhoneLine(phoneLine);
                softPhone.Close();
            }
        }
        public void KillProcesses()
        {
            if (m_RingTimer != null) {
                m_RingTimer.Stop();
                m_RingTimer.Tick -= new EventHandler(m_RingTimer_Tick);
                m_RingTimer = null;
            }

            this.Dispose();
        }

        public void StartDTMF(int dtmf)
        {
            call.StartDTMFSignal((DtmfNamedEvents)dtmf);
        }
        public void StopDTMF(int dtmf)
        {
            call.StopDTMFSignal((DtmfNamedEvents)dtmf);
        }

        public void MuteMic()
        {
            //m_UserMicrophone.Volume = 0;
            m_UserMicrophone.Muted = true;
        }
        public void UnMuteMic()
        {
            m_UserMicrophone.Muted = false;
            //m_UserMicrophone.Volume = (float)m_UserAudioSetting.mic_volume / 10;
        }
        #endregion

        #region Private Methods

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            m_RingTimer = new System.Windows.Forms.Timer(this.components)
            {
                Enabled = true,
                Interval = 1000
            };
            m_RingTimer.Tick += new EventHandler(m_RingTimer_Tick);
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "FacadeSoftPhone";
        }
        /// <summary>
        ///It initializes a softphone object with a SIP BPX, and it is for requisiting a SIP account that is nedded for a SIP PBX service. It registers this SIP
        ///account to the SIP PBX through an ’IphoneLine’ object which represents the telephoneline. 
        ///If the telephone registration is successful we have a call ready softphone. In this example the softphone can be reached by dialing the number 891.
        /// </summary>
        private void InitializeSoftPhone()
        {

            try
            {
                if (Ozeki.VoIP.SDK.Protection.LicenseManager.Instance.LicenseType != Ozeki.VoIP.SDK.Protection.LicenseType.Activated)
                    Ozeki.VoIP.SDK.Protection.LicenseManager.Instance.SetLicense(m_OzekiLicenseId, m_OzekiLicenseKey);

                using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    int? _SipAcctId = objDbModel.users.FirstOrDefault(i => i.id == UserSession.CurrentUser.UserId).sip_id;
                    if (!_SipAcctId.HasValue) {
                        //MessageBox.Show(
                        //    string.Format("Your account is not yet configured for calling.{0}Please contact your administrator.", Environment.NewLine),
                        //    "Bright Sales",
                        //    MessageBoxButtons.OK,
                        //    MessageBoxIcon.Information
                        //);
                        BrightVision.Common.UI.NotificationDialog.Error(
                            "Bright Sales",
                            string.Format("Your account is not yet configured for calling.{0}Please contact your administrator.", Environment.NewLine)
                        );
                        return;
                    }

                    sip_accounts sip = objDbModel.sip_accounts.FirstOrDefault(i => i.id == _SipAcctId);
                    if (sip != null)
                        objDbModel.Detach(sip);

                    if (m_UserAudioSetting == null)
                        m_UserAudioSetting = AudioSettingUtility.GetUserAudioSetting();

                    m_UserMicrophone = AudioSettingUtility.GetDefaultDeviceMicrophone();
                    m_UserSpeaker = AudioSettingUtility.GetDefaultDeviceSpeaker();
                    m_UserMicrophone.Volume = (float)m_UserAudioSetting.mic_volume / 10;
                    m_UserSpeaker.Volume = (float)m_UserAudioSetting.speaker_volume / 10;

                    try {
                        softPhone = SoftPhoneFactory.CreateSoftPhone(SoftPhoneFactory.GetLocalIP(), 5700, 5750, 5780);
                    }
                    catch {
                    }

                    this.DisableUnwantedCodec();
                    softPhone.IncomingCall -= new EventHandler<VoIPEventArgs<IPhoneCall>>(softPhone_IncomingCall);
                    softPhone.IncomingCall += new EventHandler<VoIPEventArgs<IPhoneCall>>(softPhone_IncomingCall);
                    SIPAccount acc = new SIPAccount(
                       true,
                       sip.display_name.Trim(),
                       sip.username.Trim(),
                       sip.username.Trim(),
                       sip.password,
                       sip.sip_url.Trim(),
                       5060,
                       ""
                    );
                    // var acc = new SIPAccount(true, sip.display_name, sip.username, sip.username, sip.password, sip.sip_url, 5060,"");
                    //  NatConfiguration newNatConfiguration = new NatConfiguration(NatTraversalMethod.Auto, new NatRemoteServer("stun.ozekiphone.com", "", ""));
                    phoneLine = softPhone.CreatePhoneLine(acc, Ozeki.Network.TransportType.Udp, SRTPMode.None);
                    phoneLine.PhoneLineStateChanged -= new EventHandler<VoIPEventArgs<PhoneLineState>>(phoneLine_PhoneLineInformation);
                    phoneLine.PhoneLineStateChanged += new EventHandler<VoIPEventArgs<PhoneLineState>>(phoneLine_PhoneLineInformation);
                    softPhone.RegisterPhoneLine(phoneLine);
                    objDbModel.Dispose();
                }
            }
            catch (Exception ex) {
            }

        }

        /// <summary>
        /// Occurs when an incoming call request has received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void softPhone_IncomingCall(object sender, VoIPEventArgs<IPhoneCall> e)
        {
            // incoming calls disabled. as per issue:
            // https://brightvision.jira.com/browse/PLATFORM-1976
            /** /
            InvokeGUIThread(() => {
                call = e.Item;
                WireUpCallEvents();
                inComingCall = true;
            });
            /**/
        }
        /// <summary>
        ///  It signs up to the necessary events of a call transact.
        /// </summary>
        private void WireUpCallEvents()
        {
            call.CallStateChanged += new EventHandler<VoIPEventArgs<CallState>>(call_CallStateChanged);
            //call.DtmfReceived += new EventHandler<VoIPEventArgs<OzTuple<VoIPMediaType, DtmfSignal>>>(call_DtmfReceived);
            call.CallErrorOccured += new EventHandler<VoIPEventArgs<CallError>>(call_CallErrorOccured);
            //call.DtmfStarted += new EventHandler<VoIPEventArgs<OzTuple<VoIPMediaType, int>>>(call_DtmfStarted);
            
            //this.SaleForm.KeyPress += new KeyPressEventHandler(SaleForm_KeyPress);
           // this.SaleForm.KeyUp += new KeyEventHandler(ParentForm_KeyUp);
        }

        /// <summary>
        /// The controls of the Windows Form applications can only be modified on the GUI thread. This method grants access to the GUI thread.
        /// </summary>
        /// <param name="action"></param>
        private void InvokeGUIThread(Action action)
        {
            if (!IsHandleCreated)
                CreateHandle();

            this.Invoke(action);
        }
        /// <summary>
        /// Occurs when the phone call state has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void call_CallStateChanged1(object sender, VoIPEventArgs<CallState> e)
        {
            this.InvokeGUIThread(() => {
                if (e.Item == CallState.Ringing) {
                    m_TimeStart = DateTime.Now.TimeOfDay;
                    m_StartTimeOutCounter = true;
                    m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.Ring);
                }

                else if (e.Item == CallState.InCall) {
                    try
                    {
                        m_StartTimeOutCounter = false;
                        mixer = new AudioMixerMediaHandler();
                        mixerMic = new AudioMixerMediaHandler();
                        mixerReceiver = new AudioMixerMediaHandler();  
                        AudioId = Guid.NewGuid();
                        CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
                        if (m_UserMicrophone != null)
                            m_UserMicrophone.Start();

                        if (m_UserSpeaker != null)
                            m_UserSpeaker.Start();

                        connector.Connect(m_UserMicrophone, mediaSender);
                        connector.Connect(mediaReceiver, m_UserSpeaker);
                        m_MediaUtility.Stop();
                        //m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.HangUp);

                        mediaSender.AttachToCall(call);
                        mediaReceiver.AttachToCall(call);


                        #region combine mic and receiver in record
                        string fileName = String.Format(@"{0}\\tmpwav\\{1}_.wav", commonData.ApplicationFolderPath, AudioId);
                        recorder = new WaveStreamRecorder(fileName);
                        recorder.Stopped += new EventHandler<EventArgs>(recorder_Stopped);
                        connector.Connect(m_UserMicrophone, mixer);
                        connector.Connect(mediaReceiver, mixer);
                        connector.Connect(mixer, recorder);
                        #endregion

                        #region record mic
                        fileName = String.Format(@"{0}\\tmpwav\\{1}_mic.wav", commonData.ApplicationFolderPath, AudioId);
                        recorderMic = new WaveStreamRecorder(fileName);
                        recorderMic.Stopped += new EventHandler<EventArgs>(recorder_Stopped1);
                        connector.Connect(m_UserMicrophone, mixerMic);
                        connector.Connect(mixerMic, recorderMic);
                        #endregion

                        #region record receiver
                        fileName = String.Format(@"{0}\\tmpwav\\{1}_receiver.wav", commonData.ApplicationFolderPath, AudioId);
                        recorderReceiver = new WaveStreamRecorder(fileName);
                        recorderReceiver.Stopped += new EventHandler<EventArgs>(recorder_Stopped2);
                        connector.Connect(mediaReceiver, mixerReceiver);
                        connector.Connect(mixerReceiver, recorderReceiver);
                        #endregion

                        recorder.StartStreaming();
                        //recorder.IsStreaming = true;
                        recorderReceiver.StartStreaming();
                        //recorderReceiver.IsStreaming = true;
                        recorderMic.StartStreaming();
                        //recorderMic.IsStreaming = true;
                    }
                    catch(Ozeki.Common.Exceptions.MediaException me) {
                        if (CallState_Changed != null)
                        {
                            CallState_Changed(this, new CallStateChangedArgs()
                            {
                                PhoneCallState = CallState.Rejected
                            });
                        }
                        if (call != null)
                            call.HangUp();

                        //softPhone.UnregisterPhoneLine(phoneLine);
                        softPhone.Close();
                        this.WireDownCallEvents();
                        call = null;
                        
                        m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.HangUp);
                        MessageBox.Show("Your mic or speaker is not working. Please change your mic or speaker in the Phone Settings.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                else if (e.Item == CallState.Completed) {
                    try
                    {
                        
                        if (m_UserMicrophone != null)
                            m_UserMicrophone.Stop();
                        if (m_UserSpeaker != null)
                            m_UserSpeaker.Stop();

                        connector.Disconnect(m_UserMicrophone, mediaSender);
                        connector.Disconnect(mediaReceiver, m_UserSpeaker);

                        if (recorder != null)
                        {
                            recorder.StopStreaming();
                            //recorder.IsStreaming = false;
                        }
                        if (recorderMic != null)
                        {
                            recorderMic.StopStreaming();
                            //recorderMic.IsStreaming = false;
                        }
                        if (recorderReceiver != null)
                        {
                            recorderReceiver.StopStreaming();
                            //recorderReceiver.IsStreaming = false;
                        }

                        connector.Disconnect(m_UserMicrophone, mixer);
                        connector.Disconnect(mediaReceiver, mixer);
                        connector.Disconnect(mixer, recorder);

                        connector.Disconnect(m_UserSpeaker, mixerMic);
                        connector.Disconnect(mixerMic, recorderMic);

                        connector.Disconnect(mixerMic, recorderMic);
                        connector.Disconnect(mixerReceiver, recorderReceiver);

                        mediaSender.Detach();
                        mediaReceiver.Detach();
                        if (m_RingTimeOut)
                            return;

                        //softPhone.UnregisterPhoneLine(phoneLine);
                        softPhone.Close();

                        this.WireDownCallEvents();
                        //call.HangUp();
                        call = null;
                        m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.HangUp);
                        recorder.Dispose();
                        mixer.Dispose();
                        recorder = null;
                        mixer = null;
                        recorderMic.Dispose();
                        mixerMic.Dispose();
                        recorderMic = null;
                        mixerMic = null;
                        mixerReceiver.Dispose();
                        recorderReceiver.Dispose();
                        mixerReceiver = null;
                        recorderReceiver = null;
                        WaitUntilTheRecordEndProcess();
                     
                    }
                    catch {
                        //softPhone.UnregisterPhoneLine(phoneLine);
                        softPhone.Close();
                        this.WireDownCallEvents();
                        call = null;
                    }
                }

                else if (e.Item == CallState.Cancelled) {
                    if (m_RingTimeOut)
                        return;

                    m_StartTimeOutCounter = false;
                    //softPhone.UnregisterPhoneLine(phoneLine);
                    softPhone.Close();
                    this.WireDownCallEvents();
                    call = null;
                    m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.HangUp);
                }

                else if (e.Item == CallState.Rejected) {
                    m_StartTimeOutCounter = false;
                    //softPhone.UnregisterPhoneLine(phoneLine);
                    softPhone.Close();
                    this.WireDownCallEvents();
                    call = null;
                    m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.HangUp);
                }

                else if (e.Item == CallState.Busy || e.Item == CallState.Error) {
                    m_RingTimeOut = true;
                    m_StartTimeOutCounter = false;
                    m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.Busy);
                    //softPhone.UnregisterPhoneLine(phoneLine);
                    softPhone.Close();
                    call.HangUp();
                    call = null;
                    //MessageBox.Show("Error encountered. Please check the format of the number you are calling.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //this.WireDownCallEvents();
                    //call = null;                
                }
              
                if (CallState_Changed != null)
                    CallState_Changed(this, new CallStateChangedArgs() {
                        PhoneCallState = e.Item,
                        AudioId = AudioId
                    });
            });
        }
        MemoryStream memStreamRecorder;
        MemoryStream memStreamReceiver;
        MemoryStream memStreamMic;
        CallState callstate;
        private void call_CallStateChanged(object sender, VoIPEventArgs<CallState> e)
        {
            callstate = e.Item;
          
                if (e.Item == CallState.Ringing)
                {
                    m_TimeStart = DateTime.Now.TimeOfDay;
                    m_StartTimeOutCounter = true;
                    m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.Ring);
                }

                else if (e.Item == CallState.InCall)
                {
                    try
                    {
                        m_StartTimeOutCounter = false;
                        m_MediaUtility.Stop();
                        recorder.StartStreaming();
                        //recorder.IsStreaming = true;
                    }
                    catch (Ozeki.Common.Exceptions.MediaException me)
                    {
                        
                        if (CallState_Changed != null)
                        {
                            AudioId = Guid.Empty;
                            CallState_Changed(this, new CallStateChangedArgs()
                            {
                                PhoneCallState = CallState.Rejected
                            });
                        }
                        if (call != null)
                            call.HangUp();

                        //softPhone.UnregisterPhoneLine(phoneLine);
                        softPhone.Close();
                        this.WireDownCallEvents();
                        call = null;

                        m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.HangUp);
                        MessageBox.Show("Your mic or speaker is not working. Please change your mic or speaker in the Phone Settings.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                else if (e.Item == CallState.Completed)
                {
                    try
                    {
                        recorder.StopStreaming();
                        //recorder.IsStreaming = false;

                        EndCall();
                        
                        if (m_RingTimeOut)
                            return;

                       
                        this.WireDownCallEvents();
                        call = null;
                        recorder.Dispose();
                        recorder = null;
                        CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
                        string fileNameTmp = String.Format(@"{0}\tmpwav\{1}_.wav", commonData.ApplicationFolderPath, AudioId);
                        string fileNameCache = String.Format(@"{0}\cachewav\{1}_.wav", commonData.ApplicationFolderPath, AudioId);
                        File.Copy(fileNameTmp, fileNameCache);
                       
                        m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.HangUp);
                        
                    }
                    catch
                    {
                        //softPhone.UnregisterPhoneLine(phoneLine);
                        softPhone.Close();
                        this.WireDownCallEvents();
                        call = null;
                    }
                }

                else if (e.Item == CallState.Cancelled)
                {
                    AudioId = Guid.Empty;
                    EndCall();
                    if (m_RingTimeOut)
                        return;

                    m_StartTimeOutCounter = false;
                    //softPhone.UnregisterPhoneLine(phoneLine);
                    
                    this.WireDownCallEvents();
                    call = null;
                    m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.HangUp);
                }

                else if (e.Item == CallState.Rejected)
                {
                    AudioId = Guid.Empty;
                    EndCall();
                    m_StartTimeOutCounter = false;
                    //softPhone.UnregisterPhoneLine(phoneLine);
                    
                    this.WireDownCallEvents();
                    call = null;
                    m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.HangUp);
                }

                else if (e.Item == CallState.Busy || e.Item == CallState.Error)
                {
                    AudioId = Guid.Empty;
                    EndCall();
                    m_RingTimeOut = true;
                    m_StartTimeOutCounter = false;
                    m_MediaUtility.Start(MediaUtility.ePhoneCallSoundType.Busy);
                    //softPhone.UnregisterPhoneLine(phoneLine);
                    softPhone.Close();
                    call.HangUp();
                    call = null;
                    //MessageBox.Show("Error encountered. Please check the format of the number you are calling.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //this.WireDownCallEvents();
                    //call = null;                
                }
               
            
                if (CallState_Changed != null)
                {
                    CallState_Changed(this, new CallStateChangedArgs()
                    {
                        PhoneCallState = e.Item,
                        AudioId = AudioId
                    });
                }
           
        }

        private void EndCall()
        {
            if (recorder != null)
            {
               
                connector.Disconnect(m_UserMicrophone, mixer);
                connector.Disconnect(mediaReceiver, mixer);
                connector.Disconnect(mixer, recorder);

                mediaSender.Detach();
                mediaReceiver.Detach();
            }
        }

        void workDelayRecording_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            #region combine mic and receiver in record

            if (callstate == CallState.InCall)
            {
                this.Invoke(new Action(delegate()
                {
                    memStreamRecorder = new MemoryStream();
                    recorder = new WaveStreamRecorder(memStreamRecorder);
                    memStreamRecorder = new MemoryStream();
                    recorder = new WaveStreamRecorder(memStreamRecorder);
                    recorder.Stopped += new EventHandler<EventArgs>(recorder_Stopped);
                    connector.Connect(m_UserMicrophone, mixer);
                    connector.Connect(mediaReceiver, mixer);
                    connector.Connect(mixer, recorder);
                    recorder.StartStreaming();
                    //recorder.IsStreaming = true;
                }));
            }

            #endregion

        }

        void workDelayRecording_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(5000);
            e.Cancel = true;
        }

        private void WriteToFile(MemoryStream ms, string filename) {
            FileStream file = new FileStream(filename, FileMode.Create, System.IO.FileAccess.Write);
            byte[] bytes = new byte[ms.Length];
            ms.Read(bytes, 0, (int)ms.Length);
            file.Write(bytes, 0, bytes.Length);
            file.Close();
            //ms.Close();
        }

        private void WaitUntilTheRecordEndProcess()
        {
            if (!(recorder == null)) {
                Thread.Sleep(500);
                WaitUntilTheRecordEndProcess();
            }
        }
        private void recorder_Stopped(object sender, EventArgs e)
        {
           
        }
        private void recorder_Stopped1(object sender, EventArgs e)
        {
      
        }
        private void recorder_Stopped2(object sender, EventArgs e)
        {
        
             
        }
        /// <summary>
        /// Displays DTMF signals
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void call_DtmfReceived(object sender, VoIPEventArgs<OzTuple<VoIPMediaType, DtmfSignal>> e)
        {
            InvokeGUIThread(() => {
                //labelCallStateInfo.Text = String.Format("DTMF signal received: {0} ", e.Item.Item2.Signal);
            });
        }


        /// <summary>
        /// There are certain situations when the call cannot be created, for example the dialed number is not available 
        /// or maybe there is no endpoint to the dialed PBX, or simply the telephone line is busy. 
        /// This event handling is for displaying these events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void call_CallErrorOccured(object sender, VoIPEventArgs<CallError> e)
        {
            InvokeGUIThread(() =>
            {
                //labelCallStateInfo.Text = e.Item.ToString();
            });
        }
        /// <summary>
        /// It signs down from the necessary events of a call transact.
        /// </summary>
        private void WireDownCallEvents()
        {
            if (call != null)
            {
                call.CallStateChanged -= (call_CallStateChanged);
                //call.DtmfReceived -= (call_DtmfReceived);
                call.CallErrorOccured -= (call_CallErrorOccured);
                //this.SaleForm.KeyPress -= new KeyPressEventHandler(SaleForm_KeyPress);
            }
        }
        /// <summary>
        /// Occurs when phone line state has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void phoneLine_PhoneLineInformation(object sender, VoIPEventArgs<PhoneLineState> e)
        {
            phoneLineInformation = e.Item;
            InvokeGUIThread(() => {
                if (e.Item == PhoneLineState.RegistrationSucceeded) {
                    m_PhoneRegisterSuccessful = true;
                    this.PhoneRegisterSuccess();
                }
                else if (e.Item == PhoneLineState.RegistrationRequested) {
                }
                else if (e.Item == PhoneLineState.RegistrationFailed) {
                    m_PhoneRegisterSuccessful = false;
                }
            });
        }

        public void mciConvertWavMP3(string fileName, bool waitFlag)
        {
            string pworkingDir = Path.GetDirectoryName(Application.ExecutablePath);
            string outfile = "-b 32 --resample 22.05 -m m \"" + pworkingDir + fileName + "\" \"" + pworkingDir + fileName.Replace(".wav", ".mp3") + "\"";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = "\"" + pworkingDir + "lame.exe" + "\"";
            psi.Arguments = outfile;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi);
            if (waitFlag)
            {
                p.WaitForExit();
            }
        }

        private void DisableUnwantedCodec()
        {
            var codecList = softPhone.Codecs;

            Ozeki.Media.Codec.CodecInfo codecPCMU8000 = codecList.FirstOrDefault(e => e.CodecName == "PCMU" && e.SampleRate == 8000);
            if (codecPCMU8000 != null)
                softPhone.DisableCodec(codecPCMU8000.Payload);

            Ozeki.Media.Codec.CodecInfo codecSILK12000 = codecList.FirstOrDefault(e => e.CodecName == "SILK" && e.SampleRate == 12000);
            if (codecSILK12000 != null)
                softPhone.DisableCodec(codecSILK12000.Payload);

            Ozeki.Media.Codec.CodecInfo codecSILK8000 = codecList.FirstOrDefault(e => e.CodecName == "SILK" && e.SampleRate == 8000);
            if (codecSILK8000 != null)
                softPhone.DisableCodec(codecSILK8000.Payload);
        }
        #endregion
    }
}
