
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
using BrightVision.Common.Utilities;
using System.Linq;
using Ozeki.Media.MediaHandlers;
using Ozeki.Media.Audio;
using Ozeki.Media;
using System.IO;
using BrightVision.Common.Utilities;
using System.Net;

namespace BrightVision.Common.UI
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
        private BrightSalesMP3StreamPlayback m_MediaPlayerStream = null;
        private Speaker m_MediaSpeaker = null;
        private MediaConnector m_Connector = null;
        private eMediaType m_MediaType;
        private bool isPlaying = false;
        private bool isDragUp = false;
        private MemoryStream m_FileStream = null;
        private bool m_CanBePlayed = false;
        private enum eMediaType {
            WavFile,
            Mp3File,
            Stream
        }
        #endregion

        #region Public Properties
        public bool CanBePlayed {
            get { return m_CanBePlayed; }
        }
        #endregion

        #region Constructors
        public CallLogPlayer()
        {
            InitializeComponent();
        }
        public CallLogPlayer(string pAudioFileUrl, bool pFromStream = false)
        {
            InitializeComponent();
            int _volume = 50;
            audio_settings _item = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _item = _efDbContext.audio_settings.FirstOrDefault(i => i.user_id == UserSession.CurrentUser.UserId);
                if (_item != null)
                    _efDbContext.Detach(_item);
            }

            if (_item != null && _item.speaker_volume > 0)
                _volume = Convert.ToInt32(_item.speaker_volume * 10);

            if (pFromStream)
                m_MediaType = eMediaType.Stream;
            else if (Path.GetExtension(pAudioFileUrl) == ".wav")
                m_MediaType = eMediaType.WavFile;
            else if (Path.GetExtension(pAudioFileUrl) == ".mp3")
                m_MediaType = eMediaType.Mp3File;

            m_MediaFile = pAudioFileUrl;
            trackBarControl.Properties.Minimum = 0;
            if (!pFromStream) {
                FileInfo info = new FileInfo(pAudioFileUrl);
                if (info.Length == 0) {
                    NotificationDialog.Information("Player", "Audio file don't have any content.");
                    m_CanBePlayed = false;
                    return;
                }
            }

            if (pFromStream) {
                //checking for  m_MediaFile
                
            }

            if (!this.IsMediaValidated()) {
                m_CanBePlayed = false;
                return;
            }

            if (m_MediaType == eMediaType.Mp3File)
                this.InitializeMp3Audio();
            else if (m_MediaType == eMediaType.WavFile)
                this.InitializeWavAudio();
            else if (m_MediaType == eMediaType.Stream)
                if (!this.InitializeStreamAudio())
                    return;

            m_CanBePlayed = true;
            if (!this.IsDisposed)
                this.btnPlay_Click(null, null);
        }
        public CallLogPlayer(int followUpId, string pAudioId, string pAudioFileUrl, bool pFromStream = false, bool pIsNew = true)
        {
            if (!CheckBlobFileExists(followUpId, pAudioId, pIsNew))
            {
                m_CanBePlayed = false;
                return;
            }

            InitializeComponent();
            int _volume = 50;
            audio_settings _item = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                _item = _efDbContext.audio_settings.FirstOrDefault(i => i.user_id == UserSession.CurrentUser.UserId);
                if (_item != null)
                    _efDbContext.Detach(_item);
            }

            if (_item != null && _item.speaker_volume > 0)
                _volume = Convert.ToInt32(_item.speaker_volume * 10);

            if (pFromStream)
                m_MediaType = eMediaType.Stream;
            else if (Path.GetExtension(pAudioFileUrl) == ".wav")
                m_MediaType = eMediaType.WavFile;
            else if (Path.GetExtension(pAudioFileUrl) == ".mp3")
                m_MediaType = eMediaType.Mp3File;

            m_MediaFile = pAudioFileUrl;
            trackBarControl.Properties.Minimum = 0;
            if (!pFromStream)
            {
                FileInfo info = new FileInfo(pAudioFileUrl);
                if (info.Length == 0)
                {
                    NotificationDialog.Information("Player", "Audio file don't have any content.");
                    m_CanBePlayed = false;
                    return;
                }
            }

            if (pFromStream)
            {
                //checking for  m_MediaFile

            }

            if (!this.IsMediaValidated())
            {
                m_CanBePlayed = false;
                return;
            }

            if (m_MediaType == eMediaType.Mp3File)
                this.InitializeMp3Audio();
            else if (m_MediaType == eMediaType.WavFile)
                this.InitializeWavAudio();
            else if (m_MediaType == eMediaType.Stream)
                if (!this.InitializeStreamAudio())
                    return;

            m_CanBePlayed = true;
            if (!this.IsDisposed)
                this.btnPlay_Click(null, null);
        }
        #endregion

        #region Public Methods
        #endregion

        #region Control Events
        private void btnPlay_Click(object sender, EventArgs e)
        {
            isPlaying = true;
            btnPlay.Enabled = false;
            btnPause.Enabled = true;
            btnStop.Enabled = true;
            btnPause.Focus();

            if (m_MediaType == eMediaType.WavFile) {
               if( m_MediaPlayerWav.Position != trackBarControl.Value)
                   m_MediaPlayerWav.Position = trackBarControl.Value;
               m_MediaPlayerWav.StartStreaming();
            }
            else if (m_MediaType == eMediaType.Mp3File) {
                if (m_MediaPlayerMp3.Position != trackBarControl.Value)
                    m_MediaPlayerMp3.Position = trackBarControl.Value;
                m_MediaPlayerMp3.StartStreaming();
            }
            else if (m_MediaType == eMediaType.Stream) {
                if (m_MediaPlayerStream.Position != trackBarControl.Value)
                    m_MediaPlayerStream.Position = trackBarControl.Value;
                m_MediaPlayerStream.StartStreaming();
            }
        }
        private void m_MediaPlayerWav_ChangedPosition(long position)
        {
            this.InvokeGUIThread(() => {
                try {
                   if(trackBarControl.Value != (int)position)
                        trackBarControl.Value = (int)position;

                    if (position == 0 && !isDragUp && !IsStreaming()) {
                        btnPlay.Enabled = true;
                        btnPause.Enabled = false;
                        btnStop.Enabled = false;
                        btnPlay.Focus();
                    }
                    else if (position == MediaLength() && !isDragUp && IsStreaming())
                        StopMode();
                    else
                        isDragUp = false;
                }
                catch {
                }
            });
        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = true;
            btnPause.Enabled = false;
            btnPlay.Focus();
            isPlaying = false;

            if (m_MediaType == eMediaType.WavFile)
                m_MediaPlayerWav.PauseStreaming();
            else if (m_MediaType == eMediaType.Mp3File)
                m_MediaPlayerMp3.PauseStreaming();
            else if (m_MediaType == eMediaType.Stream)
                m_MediaPlayerStream.PauseStreaming();
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (trackBarControl.Value == 0)
                return;

            isPlaying = false;
            btnPlay.Enabled = true;
            btnPause.Enabled = false;
            btnPause.Enabled = false;
            btnPlay.Focus();
            trackBarControl.Value = 0;

            if (m_MediaType == eMediaType.WavFile)
                m_MediaPlayerWav.StopStreaming();
            else if (m_MediaType == eMediaType.Mp3File)
                m_MediaPlayerMp3.StopStreaming();
            else if (m_MediaType == eMediaType.Stream)
                m_MediaPlayerStream.StopStreaming();
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
            else if (m_MediaType == eMediaType.Stream) {
                if (m_MediaPlayerStream != null)
                    m_MediaPlayerStream.StopStreaming();

                if (m_MediaSpeaker != null)
                    m_MediaSpeaker.Stop();

                m_Connector.Disconnect(m_MediaPlayerStream, m_MediaSpeaker);
                if (m_MediaPlayerStream != null)
                    m_MediaPlayerStream.Dispose();

                m_MediaPlayerStream = null;
            }
        }
        private void btnSaveas_Click(object sender, EventArgs e)
        {
            /*this.InvokeGUIThread(() => {
                if (m_MediaType == eMediaType.Stream) {
                    MemoryStream _CallData = new MemoryStream();
                    m_FileStream.CopyTo(_CallData);
                    FileManagerUtility.SaveToFile(_CallData);
                }
                else
                    FileManagerUtility.SaveToLocationAudio(m_MediaFile);

                m_FileStream.Position = 0;
                btnStop.PerformClick();
            });*/

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "MP3 file|*.mp3";
                dialog.Title = "Save as";
                dialog.FileName = "*.mp3";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    WaitDialog.Show("Saving audio files....");

                    BrightVision.Windows.Azure.Storage.Blob.WindowsAzureStorageBlob wab = new BrightVision.Windows.Azure.Storage.Blob.WindowsAzureStorageBlob();

                    string WindowsAzureStorageBlobAccountName = ConfigManager.AppSettings["WindowsAzureStorageBlobAccountName"].ToString();
                    string WindowsAzureStorageBlobAccountKey = ConfigManager.AppSettings["WindowsAzureStorageBlobAccountKey"].ToString();
                    string WindowsAzureStorageBlobContainerName = ConfigManager.AppSettings["WindowsAzureStorageBlobContainerName"].ToString();
                    string WindowsAzureStorageBlobContainerNameOld = ConfigManager.AppSettings["WindowsAzureStorageBlobContainerNameOld"].ToString();
                    string strAudioId = "";

                    if (m_MediaFile.IndexOf("/" + WindowsAzureStorageBlobContainerNameOld + "/") > 0)
                    {
                        strAudioId = m_MediaFile.Replace(ConfigManager.AppSettings["AzureBlobStorageOldAudioUrl"].ToString() + "/", "");
                        WindowsAzureStorageBlobContainerName = WindowsAzureStorageBlobContainerNameOld;
                    }
                    else
                    {
                        strAudioId = m_MediaFile.Replace(ConfigManager.AppSettings["AzureBlobStorageNewAudioUrl"].ToString() + "/", "");
                    }

                    if (wab.InitialzeWindowsAzureStorage(
                            WindowsAzureStorageBlobAccountName,
                            WindowsAzureStorageBlobAccountKey,
                            WindowsAzureStorageBlobContainerName
                        )
                    )
                    {
                        string msg = "";
                        if (wab.ProcessDownload(strAudioId, dialog.FileName, ref msg))
                        {
                            NotificationDialog.Information("Success", "Successfully save audio file.");

                            string argument = @"/select, " + dialog.FileName;
                            System.Diagnostics.Process.Start("explorer.exe", argument);
                        }
                        else
                        {
                            if (wab.IsBlobFileExist(strAudioId))
                            {
                                NotificationDialog.Error("Error", "Blob file does not exist.\nPlease consult system administrator.");
                            }
                            else
                            {
                                NotificationDialog.Error("Error", "Unable save audio file.\nERROR: " + msg + "\nPlease consult system administrator.");
                            }
                        }

                    }
                    wab = null;

                    WaitDialog.Close();
                }
            }
            
        }
        private void trackBarControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (isPlaying)
                btnPlay.PerformClick();
        }
        private void trackBarControl_DoubleClick(object sender, EventArgs e)
        {
            this.EvalTrackBarEvent(true);
        }
        private void trackBarControl_MouseDown(object sender, MouseEventArgs e)
        {
            bool tempIsPlaying = isPlaying;
            btnPause.PerformClick();
            isPlaying = tempIsPlaying;
        }
        #endregion

        #region Private Methods
        private void InvokeGUIThread(Action action)
        {
            try {
                if (!IsHandleCreated)
                    CreateHandle();

                this.Invoke(action);
            }
            catch {
            }
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
            if (!isPlaying && trackBarControl.Value == 0) {
                btnPlay.Enabled = true;
                btnPause.Enabled = false;
                btnStop.Enabled = false;
                btnPlay.Focus();
                isPlaying = false;
            }
            else if (!isPlaying && trackBarControl.Value == MediaLength()) {
                btnPlay.Enabled = true;
                btnPause.Enabled = false;
                btnStop.Enabled = false;
                btnPlay.Focus();
                isPlaying = false;
            }
        }
        private int MediaLength() 
        {
            if (m_MediaType == eMediaType.WavFile)
                return (int)m_MediaPlayerWav.Length;

            else if(m_MediaType == eMediaType.Mp3File)
                return (int)m_MediaPlayerMp3.Length;

            else if (m_MediaType == eMediaType.Stream)
                return (int)m_MediaPlayerStream.Length;

            return 0;
        }
        private bool IsStreaming()
        {
            if (m_MediaType == eMediaType.WavFile)
                return m_MediaPlayerWav.IsStreaming;

            else if (m_MediaType == eMediaType.Mp3File)
                return m_MediaPlayerMp3.IsStreaming;

            else if (m_MediaType == eMediaType.Stream)
                return m_MediaPlayerStream.IsStreaming;

            return false;
        }
        private void EvalTrackBarEvent(bool relToPosition=false)
        {
            if (m_MediaType == eMediaType.WavFile) {
                if (m_MediaPlayerWav == null)
                    return;

                if (m_MediaPlayerWav.Position != trackBarControl.Value || relToPosition) {
                    m_MediaPlayerWav.Position = trackBarControl.Value;
                    if (isPlaying) {
                        isDragUp = true;
                        m_MediaPlayerWav.StartStreaming();
                        PlayMode();
                    }
                }
            }
            else if (m_MediaType == eMediaType.Mp3File) {
                if (m_MediaPlayerMp3 == null)
                    return;

                if (m_MediaPlayerMp3.Position != trackBarControl.Value || relToPosition) {
                    m_MediaPlayerMp3.Position = trackBarControl.Value;
                    if (isPlaying) {
                        isDragUp = true;
                        m_MediaPlayerMp3.StartStreaming();
                        PlayMode();
                    }
                }
            }
            else if (m_MediaType == eMediaType.Stream) {
                if (m_MediaPlayerStream == null)
                    return;

                if (m_MediaPlayerStream.Position != trackBarControl.Value || relToPosition) {
                    m_MediaPlayerStream.Position = trackBarControl.Value;
                    if (isPlaying) {
                        isDragUp = true;
                        m_MediaPlayerStream.StartStreaming();
                        PlayMode();

                    }
                }
            }

            this.StopMode();
        }
        private void InitializeWavAudio()
        {
            try {
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
        private bool InitializeStreamAudio()
        {
            try {
                m_FileStream = new MemoryStream();
                using (Stream _stream = WebRequest.Create(m_MediaFile).GetResponse().GetResponseStream()) {
                    _stream.CopyTo(m_FileStream);
                    m_FileStream.Position = 0;
                }

                m_Connector = new MediaConnector();
                m_MediaPlayerStream = new BrightSalesMP3StreamPlayback(m_FileStream);
                m_MediaSpeaker.Start();
                m_Connector.Connect(m_MediaPlayerStream, m_MediaSpeaker);
                trackBarControl.Properties.Minimum = 0;
                trackBarControl.Properties.Maximum = int.Parse(m_MediaPlayerStream.Length.ToString());
                m_MediaPlayerStream.ChangedPosition += new BrightSalesMP3StreamPlayback.ChangedPositionHandler(m_MediaPlayerWav_ChangedPosition);
                return true;
            }
            catch {
                NotificationDialog.Error("Player", "Audio stream file failed to load.");
                return false;
            }
        }
        private void InitializeMp3Audio()
        {
            try {
                m_Connector = new MediaConnector();
                m_MediaPlayerMp3 = new BrightSalesMP3StreamPlayback(m_MediaFile);
                m_MediaSpeaker.Start();
                m_Connector.Connect(m_MediaPlayerMp3, m_MediaSpeaker);
                trackBarControl.Properties.Minimum = 0;
                trackBarControl.Properties.Maximum = int.Parse(m_MediaPlayerMp3.Length.ToString());
                m_MediaPlayerMp3.ChangedPosition += new BrightSalesMP3StreamPlayback.ChangedPositionHandler(m_MediaPlayerWav_ChangedPosition);
            }
            catch {

            }
        }
        private bool IsMediaValidated()
        {
            //if (m_MediaType == eMediaType.WavFile) {
            //    m_MediaSpeaker = AudioSettingUtility.GetDefaultDeviceSpeaker();
            //    if (m_MediaSpeaker == null) {
            //        MessageBox.Show("No audio device found.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return false;
            //    }
            //}

            //else if (m_MediaType == eMediaType.Mp3File) {
            //    m_MediaSpeaker = AudioSettingUtility.GetDefaultDeviceSpeaker();
            //    if (m_MediaSpeaker == null) {
            //        MessageBox.Show("No audio device found.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return false;
            //    }
            //}

            m_MediaSpeaker = AudioSettingUtility.GetDefaultDeviceSpeaker();
            if (m_MediaSpeaker == null) {
                NotificationDialog.Warning("Player", "No audio device found.");
                return false;
            }

            return true;
        }
        private bool CheckBlobFileExists(int followUpId, string audioId, bool IsNew = true)
        {
            BrightVision.Windows.Azure.Storage.Blob.WindowsAzureStorageBlob wab = new BrightVision.Windows.Azure.Storage.Blob.WindowsAzureStorageBlob();

            string WindowsAzureStorageBlobAccountName = ConfigManager.AppSettings["WindowsAzureStorageBlobAccountName"].ToString();
            string WindowsAzureStorageBlobAccountKey = ConfigManager.AppSettings["WindowsAzureStorageBlobAccountKey"].ToString();
            string WindowsAzureStorageBlobContainerName = ConfigManager.AppSettings["WindowsAzureStorageBlobContainerName"].ToString();
            string strAudioId = "";

            if (!IsNew)
            {
                strAudioId = audioId + "_.mp3";

                WindowsAzureStorageBlobContainerName = ConfigManager.AppSettings["WindowsAzureStorageBlobContainerNameOld"].ToString();
            }
            else
            {
                strAudioId = audioId + ".mp3";
            }


            if (wab.InitialzeWindowsAzureStorage(
                    WindowsAzureStorageBlobAccountName,
                    WindowsAzureStorageBlobAccountKey,
                    WindowsAzureStorageBlobContainerName
                )
            )
            {
                if (!wab.IsBlobFileExist(strAudioId))
                {
                    NotificationDialog.Error("Error", "Blob file does not exist.\nPlease consult system administrator.", followUpId);
                    return false;
                }
            }
            wab = null;

            return true;
        }
        #endregion
    }
}