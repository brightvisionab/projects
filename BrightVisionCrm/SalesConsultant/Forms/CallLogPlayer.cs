
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Telephony.Utilities;
using System.Linq;
using Ozeki.Media.MediaHandlers;
using Ozeki.Media.Audio;
using Ozeki.Media;
using System.IO;
using SalesConsultant.Business;
using BrightVision.Telephony;

namespace SalesConsultant.Forms
{
    public partial class CallLogPlayer : DevExpress.XtraEditors.XtraForm
    {
        /**
         * just temporarily disabled axWindowsMediaPlayer (still available).
         * since it does not support setting of selected speaker device.
         */
        #region Variable
        private string m_MediaFile = string.Empty;
        private BrightSalesWaveStreamPlayback m_MediaPlayerWav = null;
        private BrightSalesMP3StreamPlayback m_MediaPlayerMp3 = null;
        private Speaker m_MediaSpeaker = null;
        private MediaConnector m_Connector = null;
        private eMediaType m_MediaType;
        private bool isPlaying = false;
        private bool isDragUp = false;
        private enum eMediaType {
            WavFile,
            Mp3File
        }
        #endregion

        #region Constructor
        public CallLogPlayer()
        {
            InitializeComponent();
        }
        public CallLogPlayer(string pAudioFileUrl)
        {
            InitializeComponent();
            int _volume = 50;
            BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            audio_settings _item = _efDbContext.audio_settings.FirstOrDefault(i => i.user_id == UserSession.CurrentUser.UserId);
            if (_item != null)
                if (_item.speaker_volume != null)
                    if (_item.speaker_volume > 0)
                        _volume = Convert.ToInt32(_item.speaker_volume * 10);

            if (Path.GetExtension(pAudioFileUrl) == ".wav")
                m_MediaType = eMediaType.WavFile;
            else if (Path.GetExtension(pAudioFileUrl) == ".mp3")
                m_MediaType = eMediaType.Mp3File;

          
            m_MediaFile = pAudioFileUrl;
            //axWindowsMediaPlayer.URL = pAudioFileUrl;
            //axWindowsMediaPlayer.Ctlcontrols.stop();
            //axWindowsMediaPlayer.settings.volume = _volume;
           
            trackBarControl.Properties.Minimum = 0;
            FileInfo info = new FileInfo(pAudioFileUrl);

            if (info.Length == 0) {
                MessageBox.Show("Audio file don't have any content.", "Can't play audio", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (IsMediaValidated())
            {
                if (m_MediaType == eMediaType.WavFile)
                    InitializeWavAudio();
                else
                    InitializeMp3Audio();

                if(!this.IsDisposed)
                    this.btnPlay_Click(null, null);
            }
        }
        #endregion

        #region Public
        public void PlayAudio()
        {
            //axWindowsMediaPlayer.Ctlcontrols.play();
            
        }

        public void Play(string fileUrl) {
            axWindowsMediaPlayer.URL = fileUrl;
            PlayAudio();
        }

        //public void Stop()
        //{
        //    try
        //    {
        //        axWindowsMediaPlayer.Ctlcontrols.stop();
        //        this.Hide();
        //    }
        //    catch { }
        //}
        #endregion

        #region Events Handlers
        private void btnPlay_Click(object sender, EventArgs e)
        {
            isPlaying = true;
            btnPlay.Enabled = false;
            btnPause.Enabled = true;
            btnStop.Enabled = true;
            btnPause.Focus();
            if (m_MediaType == eMediaType.WavFile) {
               m_MediaPlayerWav.StartStreaming();
            }
            else if (m_MediaType == eMediaType.Mp3File) {
                m_MediaPlayerMp3.StartStreaming();
            }
        }

        private void m_MediaPlayerWav_ChangedPosition(long position)
        {

            this.InvokeGUIThread(() =>
            {
                try
                {
                   // this.trackBarControl.EditValueChanged -= new System.EventHandler(this.trackBarControl_EditValueChanged);
                    if(trackBarControl.Value != (int)position)
                        trackBarControl.Value = (int)position;

                    if (position == 0 && !isDragUp && !IsStreaming())
                    {
                        btnPlay.Enabled = true;
                        btnPause.Enabled = false;
                        btnStop.Enabled = false;
                        btnPlay.Focus();
                    }
                    else if (position == MediaLength() && !isDragUp && IsStreaming()) {
                        StopMode();
                    }
                    else
                    {
                        isDragUp = false;
                    }

                    
                   // this.trackBarControl.EditValueChanged += new System.EventHandler(this.trackBarControl_EditValueChanged);
                }
                catch
                {

                }
            });
         
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = true;
            btnPause.Enabled = false;
            btnPlay.Focus();
            isPlaying = false;
            if (m_MediaType == eMediaType.WavFile) {
                m_MediaPlayerWav.PauseStreaming();
            }
            else if (m_MediaType == eMediaType.Mp3File) {
                m_MediaPlayerMp3.PauseStreaming();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isPlaying = false;
            btnPlay.Enabled = true;
            btnPause.Enabled = false;
            btnPause.Enabled = false;
            btnPlay.Focus();
            trackBarControl.Value = 0;

            if (m_MediaType == eMediaType.WavFile) {
                m_MediaPlayerWav.StopStreaming();
            }
            else if (m_MediaType == eMediaType.Mp3File) {
                m_MediaPlayerMp3.StopStreaming();
            }
           
        }

        private void CallLogPlayer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_MediaType == eMediaType.WavFile) {
                if (m_MediaPlayerWav != null)
                    m_MediaPlayerWav.StopStreaming();

                if (m_MediaSpeaker != null)
                    m_MediaSpeaker.Stop();

                m_Connector.Disconnect(m_MediaPlayerWav, m_MediaSpeaker);
                if (m_MediaPlayerWav != null)
                    m_MediaPlayerWav.Dispose();

                m_MediaPlayerWav = null;
            }
            else if (m_MediaType == eMediaType.Mp3File) {
                if (m_MediaPlayerMp3 != null)
                    m_MediaPlayerMp3.StopStreaming();

                if (m_MediaSpeaker != null)
                    m_MediaSpeaker.Stop();

                m_Connector.Disconnect(m_MediaPlayerMp3, m_MediaSpeaker);
                if (m_MediaPlayerMp3 != null)
                    m_MediaPlayerMp3.Dispose();

                m_MediaPlayerMp3 = null;
            }
        }

        private void btnSaveas_Click(object sender, EventArgs e)
        {
            FileManagerUtility.SaveToLocationAudio(m_MediaFile);  
        }

        private void trackBarControl_MouseUp(object sender, MouseEventArgs e)
        {
            EvalTrackBarEvent();
        }

        private void trackBarControl_DoubleClick(object sender, EventArgs e)
        {
            EvalTrackBarEvent(true);
        }

        private void trackBarControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_MediaType == eMediaType.WavFile)
            {
                if (m_MediaPlayerWav == null)
                    return;

                m_MediaPlayerWav.PauseStreaming();
            }
            else if (m_MediaType == eMediaType.Mp3File)
            {
                if (m_MediaPlayerMp3 == null)
                    return;
                m_MediaPlayerMp3.PauseStreaming();
              
            }
        }
        #endregion

        #region Private
        private void InvokeGUIThread(Action action)
        {
            if (!IsHandleCreated)
                CreateHandle();
            this.Invoke(action);
        }

        private void PlayMode()
        {
            isPlaying = true;
            btnPlay.Enabled = false;
            btnPause.Enabled = true;
            btnStop.Enabled = true;
            btnPlay.Focus();
        }

        private void StopMode() 
        {
            if (!isPlaying && trackBarControl.Value == 0) 
            {
                btnPlay.Enabled = true;
                btnPause.Enabled = false;
                btnStop.Enabled = false;
                btnPlay.Focus();
                isPlaying = false;
            }
            else if ( 
                !isPlaying &&
                trackBarControl.Value == MediaLength())
            {
                btnPlay.Enabled = true;
                btnPause.Enabled = false;
                btnStop.Enabled = false;
                btnPlay.Focus();
                isPlaying = false;
            }
        }

        private int MediaLength() {
            if (m_MediaType == eMediaType.WavFile)
                return (int)m_MediaPlayerWav.Length;
            else if(m_MediaType == eMediaType.Mp3File)
                return (int)m_MediaPlayerMp3.Length;
            return 0;
        }
        private bool IsStreaming()
        {
            if (m_MediaType == eMediaType.WavFile)
                return m_MediaPlayerWav.IsStreaming;
            else if (m_MediaType == eMediaType.Mp3File)
                return m_MediaPlayerMp3.IsStreaming;
            return false;
        }

        private void EvalTrackBarEvent(bool relToPosition=false)
        {
            if (m_MediaType == eMediaType.WavFile)
            {
                if (m_MediaPlayerWav == null)
                    return;
                if (m_MediaPlayerWav.Position != trackBarControl.Value || relToPosition)
                {
                    m_MediaPlayerWav.Position = trackBarControl.Value;
                    if (isPlaying)
                    {
                        isDragUp = true;
                        m_MediaPlayerWav.StartStreaming();
                        PlayMode();
                    }
                }
            }
            else if (m_MediaType == eMediaType.Mp3File)
            {
                if (m_MediaPlayerMp3 == null)
                    return;
                if (m_MediaPlayerMp3.Position != trackBarControl.Value || relToPosition)
                {
                    m_MediaPlayerMp3.Position = trackBarControl.Value;
                    if (isPlaying)
                    {
                        isDragUp = true;
                        m_MediaPlayerMp3.StartStreaming();
                        PlayMode();

                    }
                }
            }

            StopMode();
        }

        private void InitializeWavAudio()
        {
            try
            {
                m_MediaSpeaker = AudioSettingUtility.GetDefaultDeviceSpeaker();
                m_Connector = new MediaConnector();
                m_MediaPlayerWav = new BrightSalesWaveStreamPlayback(m_MediaFile);
                m_MediaSpeaker.Start();
                m_Connector.Connect(m_MediaPlayerWav, m_MediaSpeaker);
                trackBarControl.Properties.Minimum = 0;
                trackBarControl.Properties.Maximum = int.Parse(m_MediaPlayerWav.Length.ToString());
                m_MediaPlayerWav.ChangedPosition += new BrightSalesWaveStreamPlayback.ChangedPositionHandler(m_MediaPlayerWav_ChangedPosition);
            }
            catch {
                MessageBox.Show("Audio file don't have any content.", "Can't play audio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
        }

        private void InitializeMp3Audio()
        {
            try
            {
                m_Connector = new MediaConnector();
                m_MediaPlayerMp3 = new BrightSalesMP3StreamPlayback(m_MediaFile);
                m_MediaSpeaker.Start();
                m_Connector.Connect(m_MediaPlayerMp3, m_MediaSpeaker);
                trackBarControl.Properties.Minimum = 0;
                trackBarControl.Properties.Maximum = int.Parse(m_MediaPlayerMp3.Length.ToString());
                m_MediaPlayerMp3.ChangedPosition += new BrightSalesMP3StreamPlayback.ChangedPositionHandler(m_MediaPlayerWav_ChangedPosition);
            }
            catch{

            }
        }

        private bool IsMediaValidated()
        {

            if (m_MediaType == eMediaType.WavFile)
            {
                m_MediaSpeaker = AudioSettingUtility.GetDefaultDeviceSpeaker();
                if (m_MediaSpeaker == null)
                {
                    MessageBox.Show("No audio device found.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            else if (m_MediaType == eMediaType.Mp3File)
            {
                m_MediaSpeaker = AudioSettingUtility.GetDefaultDeviceSpeaker();
                if (m_MediaSpeaker == null)
                {
                    MessageBox.Show("No audio device found.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            return true;
        }
        
        #endregion
    }
}