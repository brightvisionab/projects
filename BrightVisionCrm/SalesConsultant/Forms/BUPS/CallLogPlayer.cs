
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

namespace SalesConsultant.Forms
{
    public partial class CallLogPlayer : DevExpress.XtraEditors.XtraForm
    {
        /**
         * just temporarily disabled axWindowsMediaPlayer (still available).
         * since it does not support setting of selected speaker device.
         */

        private string m_MediaFile = string.Empty;
        private WaveStreamPlayback m_MediaPlayerWav = null;
        private MP3StreamPlayback m_MediaPlayerMp3 = null;
        private Speaker m_MediaSpeaker = null;
        private MediaConnector m_Connector = null;
        private bool m_TrackerDragged = false;
        private eMediaType m_MediaType;

        private enum eMediaType {
            WavFile,
            Mp3File
        }
        
        public CallLogPlayer()
        {
            InitializeComponent();
        }
        public CallLogPlayer(string pAudioFileUrl)
        {
            int _volume = 50;
            /** /
            BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            audio_settings _item = _efDbContext.audio_settings.FirstOrDefault(i => i.user_id == UserSession.CurrentUser.UserId);
            if (_item != null)
                if (_item.speaker_volume != null)
                    if (_item.speaker_volume > 0)
                        _volume = Convert.ToInt32(_item.speaker_volume * 10);
            /**/
            
            if (Path.GetExtension(pAudioFileUrl) == ".wav")
                m_MediaType = eMediaType.WavFile;
            else if (Path.GetExtension(pAudioFileUrl) == ".mp3")
                m_MediaType = eMediaType.Mp3File;
            
            InitializeComponent();
            m_MediaFile = pAudioFileUrl;
            axWindowsMediaPlayer.URL = pAudioFileUrl;
            axWindowsMediaPlayer.Ctlcontrols.stop();
            axWindowsMediaPlayer.settings.volume = _volume;
            this.btnPlay_Click(null, null);
        }
        
        public void PlayAudio()
        {
            //axWindowsMediaPlayer.Ctlcontrols.play();
            
        }
        public void Play(string fileUrl) {
            axWindowsMediaPlayer.URL = fileUrl;
            PlayAudio();
        }
        public void Stop()
        {
            try
            {
                axWindowsMediaPlayer.Ctlcontrols.stop();
                this.Hide();
            }
            catch { }
        }

        private void InitializeSlider()
        {
            
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = false;
            btnPause.Enabled = true;
            btnPause.Focus();
            
            #region Wav File
            if (m_MediaType == eMediaType.WavFile) {
                if (m_MediaPlayerWav != null) {
                    if (!m_MediaPlayerWav.IsStreaming) {
                        if (m_TrackerDragged)
                            m_MediaPlayerWav.Stream.Position = Convert.ToInt64(barAudioTracker.EditValue);

                        m_MediaPlayerWav.StartStreaming();
                        AudioTick.Start();
                    }
                    
                    return;
                }

                m_MediaSpeaker = Speaker.GetDefaultDevice(); //AudioSettingUtility.GetDefaultDeviceSpeaker();
                if (m_MediaSpeaker == null) {
                    MessageBox.Show("No audio device found.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                m_Connector = new MediaConnector();
                m_MediaPlayerWav = new WaveStreamPlayback(m_MediaFile);
                barAudioTracker.Properties.Minimum = 0;
                barAudioTracker.Properties.Maximum = Convert.ToInt32(m_MediaPlayerWav.Stream.Length);

                m_MediaSpeaker.Start();
                m_Connector.Connect(m_MediaPlayerWav, m_MediaSpeaker);
                m_MediaPlayerWav.StartStreaming();
                m_MediaPlayerWav.IsStreaming = true;
                AudioTick.Start();
            }
            #endregion
            #region Mp3 File
            else if (m_MediaType == eMediaType.Mp3File) {
                if (m_MediaPlayerMp3 != null) {
                    if (!m_MediaPlayerMp3.IsStreaming) {
                        if (m_TrackerDragged)
                            m_MediaPlayerMp3.Stream.Position = Convert.ToInt64(barAudioTracker.EditValue);

                        m_MediaPlayerMp3.StartStreaming();
                        AudioTick.Start();
                    }
                    
                    return;
                }

                m_MediaSpeaker = Speaker.GetDefaultDevice(); // AudioSettingUtility.GetDefaultDeviceSpeaker();
                if (m_MediaSpeaker == null) {
                    MessageBox.Show("No audio device found.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                m_Connector = new MediaConnector();
                m_MediaPlayerMp3 = new MP3StreamPlayback(m_MediaFile);
                barAudioTracker.Properties.Minimum = 0;
                barAudioTracker.Properties.Maximum = Convert.ToInt32(m_MediaPlayerMp3.Stream.Length);

                m_MediaSpeaker.Start();
                m_Connector.Connect(m_MediaPlayerMp3, m_MediaSpeaker);
                m_MediaPlayerMp3.StartStreaming();
                m_MediaPlayerMp3.IsStreaming = true;
                AudioTick.Start();
            }
            #endregion
        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = true;
            btnPause.Enabled = false;
            btnPlay.Focus();
            AudioTick.Stop();

            if (m_MediaType == eMediaType.WavFile) {
                m_MediaPlayerWav.PauseStreaming();
                m_MediaPlayerWav.IsStreaming = false;
            }
            else if (m_MediaType == eMediaType.Mp3File) {
                m_MediaPlayerMp3.PauseStreaming();
                m_MediaPlayerMp3.IsStreaming = false;
            }
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = true;
            btnPause.Enabled = false;
            btnPlay.Focus();
            AudioTick.Stop();

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
        private void AudioTick_Tick(object sender, EventArgs e)
        {
            if (m_MediaType == eMediaType.WavFile && m_MediaPlayerWav != null)
                barAudioTracker.Value = Convert.ToInt32(m_MediaPlayerWav.Stream.Position);
            else if (m_MediaType == eMediaType.Mp3File && m_MediaPlayerMp3 != null)
                barAudioTracker.Value = Convert.ToInt32(m_MediaPlayerMp3.Stream.Position);

        }
    }
}