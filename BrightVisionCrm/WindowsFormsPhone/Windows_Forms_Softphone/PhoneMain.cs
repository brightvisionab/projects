using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Ozeki.Media;
using Ozeki.Media.MediaHandlers;
using Ozeki.Network.Nat;
using Ozeki.VoIP;
using Ozeki.VoIP.Media;
using Ozeki.VoIP.SDK;
using Windows_Forms_Softphone.Windows;
using Ozeki.Common;

namespace Windows_Forms_Softphone
{
    public partial class PhoneMain : Form
    {
        ISoftPhone softPhone;
        IPhoneLine phoneLine;
        PhoneLineState phoneLineInformation;
        IPhoneCall call;
        Microphone microphone = Microphone.GetDefaultDevice();
        Speaker speaker =  Speaker.GetDefaultDevice();
        MediaConnector connector = new MediaConnector();
        PhoneCallAudioSender mediaSender = new PhoneCallAudioSender();
        PhoneCallAudioReceiver mediaReceiver = new PhoneCallAudioReceiver();
        bool inComingCall;

        public PhoneMain()
        {
            InitializeComponent();
            //some devices missing
            string message = String.Empty;

            if (microphone == null)
                message += "You have no microphone attached to your computer, please note that your partner will not hear your voice.\n";
            if (speaker == null)
                message += "You have no speaker attached to your computer, please note that you will not hear your partner.\n";

            if (message != String.Empty)
                MessageBox.Show(message, "Some devices missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
          
        }

        private void PhoneMain_Load(object sender, EventArgs e)
        {
            Tssl_Ozeki.Text = string.Format("© Copyright {0}, Ozeki Informatics Ltd.", DateTime.Now.Year);
            
            InitializeSoftPhone();
        }


        #region Ozeki VoIP-SIP SDK's events

        /// <summary>
        /// Occurs when phone line state has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void phoneLine_PhoneLineInformation(object sender, VoIPEventArgs<PhoneLineState> e)
        {
            phoneLineInformation = e.Item;
            InvokeGUIThread(() =>
            {
                labelIdentifier.Text = ((IPhoneLine) sender).SIPAccount.RegisterName;
                if (e.Item == PhoneLineState.RegistrationSucceeded)
                {
                    labelRegStatus.Text = "Online";
                    labelCallStateInfo.Text = "Registration succeeded";
                }
                else
                    labelCallStateInfo.Text = e.Item.ToString();
            });
        }

        /// <summary>
        /// Occurs when an incoming call request has received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void softPhone_IncomingCall(object sender, VoIPEventArgs<IPhoneCall> e)
        {
            InvokeGUIThread(()=>
                         {
                             labelCallStateInfo.Text = "Incoming call";
                             labelDialingNumber.Text = String.Format("from {0}", e.Item.DialInfo);
                             call = e.Item;
                             WireUpCallEvents();
                             inComingCall = true;
                         });
        }

        /// <summary>
        /// Occurs when the phone call state has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void call_CallStateChanged(object sender, VoIPEventArgs<CallState> e)
        {
            InvokeGUIThread(() => { labelCallStateInfo.Text = e.Item.ToString(); });

            switch (e.Item)
            {
                case CallState.InCall:
                    if (microphone != null)
                        microphone.Start();
                    connector.Connect(microphone, mediaSender);

                    if (speaker != null)
                        speaker.Start();
                    connector.Connect(mediaReceiver, speaker);
                    
                    mediaSender.AttachToCall(call);
                    mediaReceiver.AttachToCall(call);

                    break;
                case CallState.Completed:
                    if (microphone != null)
                        microphone.Stop();
                    connector.Disconnect(microphone, mediaSender);
                    if (speaker != null)
                        speaker.Stop();
                    connector.Disconnect(mediaReceiver, speaker);

                    mediaSender.Detach();
                    mediaReceiver.Detach();

                    WireDownCallEvents();
                    call = null;

                    InvokeGUIThread(() => { labelDialingNumber.Text = string.Empty; });
                    break;
                case CallState.Cancelled:
                    WireDownCallEvents();
                    call = null;
                    break;
            }
        }

        /// <summary>
        /// Displays DTMF signals
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void call_DtmfReceived(object sender, VoIPEventArgs<OzTuple<VoIPMediaType, DtmfSignal>> e)
        {
            InvokeGUIThread(() =>
                                {
                                    labelCallStateInfo.Text = String.Format("DTMF signal received: {0} ", e.Item.Item2.Signal);
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
            InvokeGUIThread(()=>
                         {
                             labelCallStateInfo.Text = e.Item.ToString();
                         });
        }

        #endregion

        #region Keypad events

        /// <summary>
        /// It starts a call with the dialed number or in case of an incoming call it accepts, picks up the call.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPickUp_Click(object sender, EventArgs e)
        {
            if (inComingCall)
            {
                inComingCall = false;
                call.Accept();
                return;
            }

            if (call != null)
                return;

            if (string.IsNullOrEmpty(labelDialingNumber.Text))
                return;

            if (phoneLineInformation != PhoneLineState.RegistrationSucceeded && phoneLineInformation != PhoneLineState.NoRegNeeded)
            {
                MessageBox.Show("Phone line state is not valid!");
                return;
            }

            call = softPhone.CreateCallObject(phoneLine, labelDialingNumber.Text);
            WireUpCallEvents();
            call.Start();
        }

        /// <summary>
        /// In case a call is in progress, it breaks the call.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonHangUp_Click(object sender, EventArgs e)
        {
            if (call != null)
            {   
                if (inComingCall && call.CallState==CallState.Ringing)
                    call.Reject();
                else
                    call.HangUp();
                inComingCall = false;
                call = null;
            }
            labelDialingNumber.Text = string.Empty;
        }

        /// <summary>
        /// It makes the dialing number.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonKeyPadButton_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                if (btn.Text=="0/+")
                {
                    labelDialingNumber.Text += "0";
                }
                else
                    labelDialingNumber.Text += btn.Text.Trim();
            }
        }

        /// <summary>
        ///  It sends a DTMF signal according to the RFC 2833 standard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonKeyPadButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (call != null && call.CallState == CallState.InCall)
            {
                var btn = sender as Button;
                if (btn != null)
                {
                    int id;

                    if (btn.Tag != null && int.TryParse(btn.Tag.ToString(), out id))
                    {
                        call.StartDTMFSignal(VoIPMediaType.Audio, (DtmfNamedEvents)id);
                    }
                }
            }
        }
        /// <summary>
        /// It stops sending the given DTMF signal. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonKeyPad_MouseUp(object sender, MouseEventArgs e)
        {
            if (call != null && call.CallState == CallState.InCall)
            {
                var btn = sender as Button;
                if (btn != null)
                {
                    int id;

                    if (btn.Tag != null && int.TryParse(btn.Tag.ToString(), out id))
                    {
                        call.StopDTMFSignal(VoIPMediaType.Audio, (DtmfNamedEvents)id);
                    }
                }
            }
        }

        #endregion

        #region Helper Functions
        /// <summary>
        ///It initializes a softphone object with a SIP BPX, and it is for requisiting a SIP account that is nedded for a SIP PBX service. It registers this SIP
        ///account to the SIP PBX through an ’IphoneLine’ object which represents the telephoneline. 
        ///If the telephone registration is successful we have a call ready softphone. In this example the softphone can be reached by dialing the number 891.
        /// </summary>
        private void InitializeSoftPhone()
        {
            try
            {
                softPhone = SoftPhoneFactory.CreateSoftPhone(SoftPhoneFactory.GetLocalIP(), 5700, 5750, 5700);
                softPhone.IncomingCall += new EventHandler<VoIPEventArgs<IPhoneCall>>(softPhone_IncomingCall);
                phoneLine = softPhone.CreatePhoneLine(new SIPAccount(true, "Testing 1", "46855117151", "46855117151", "d9RcDFrbDx8Ui", "sip1.cellip.com", 5060), new NatConfiguration(NatTraversalMethod.None));
                //phoneLine = softPhone.CreatePhoneLine(new SIPAccount(true, "oz875", "oz875", "oz875", "oz875", "192.168.112.100", 5060), new NatConfiguration(NatTraversalMethod.None));
                phoneLine.PhoneLineStateChanged += new EventHandler<VoIPEventArgs<PhoneLineState>>(phoneLine_PhoneLineInformation);

                softPhone.RegisterPhoneLine(phoneLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("You didn't give your local IP adress, so the program won't run properly.\n {0}", ex.Message), string.Empty, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
				
                var sb = new StringBuilder();
                sb.AppendLine("Some error happened.");
                sb.AppendLine();
                sb.AppendLine("Exception:");
                sb.AppendLine(ex.Message);
                sb.AppendLine();
                if(ex.InnerException != null)
                {
                    sb.AppendLine("Inner Exception:");
                    sb.AppendLine(ex.InnerException.Message);
                    sb.AppendLine();
                }
                sb.AppendLine("StackTrace:");
                sb.AppendLine(ex.StackTrace);

                MessageBox.Show(sb.ToString());				
            }
        }

			/// <summary>
        ///  It signs up to the necessary events of a call transact.
        /// </summary>
        private void WireUpCallEvents()
        {
            call.CallStateChanged += new EventHandler<VoIPEventArgs<CallState>>(call_CallStateChanged);
            call.DtmfReceived += new EventHandler<VoIPEventArgs<OzTuple<VoIPMediaType, DtmfSignal>>>(call_DtmfReceived);
            call.CallErrorOccured += new EventHandler<VoIPEventArgs<CallError>>(call_CallErrorOccured);
        }

        /// <summary>
        /// It signs down from the necessary events of a call transact.
        /// </summary>
        private void WireDownCallEvents()
        {
            if (call != null)
            {
                call.CallStateChanged -= (call_CallStateChanged);
                call.DtmfReceived -= (call_DtmfReceived);
                call.CallErrorOccured -= (call_CallErrorOccured);
            }
        }
        
        /// <summary>
        /// The controls of the Windows Form applications can only be modified on the GUI thread. This method grants access to the GUI thread.
        /// </summary>
        /// <param name="action"></param>
        private void InvokeGUIThread(Action action)
        {
            Invoke(action);
        }
        #endregion

        #region Menu events
        private void Tsmi_HelpPage_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://www.voip-sip-sdk.com/p_136-c-sharp-windows-forms-softphone-voip.html");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        private void Tsmi_About_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();

            aboutBox.ShowDialog(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        private void PhoneMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            softPhone.Close();
        }

    }
}
