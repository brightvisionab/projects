using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using System.Configuration;
using BrightVision.Common.Utilities;
using ITHit.WebDAV.Client;
using System.Net;
using System.IO;
using BrightVision.Common.Business;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace BrightVision.Common.Utilities
{
    public static class FileManagerUtility
    {
        public static string GetMimeType(string pFileName)
        {
            string _mime = "application/octetstream";
            string _ext = System.IO.Path.GetExtension(pFileName).ToLower();
            Microsoft.Win32.RegistryKey _rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(_ext);
            if (_rk != null && _rk.GetValue("Content Type") != null)
                _mime = _rk.GetValue("Content Type").ToString();

            return _mime;
        }
        public static string GetServerUrl(Guid audioID)
        {
            var objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var followup = objDbModel.event_followup_log.Where(param => param.audio_id == audioID).FirstOrDefault();
            string salesAudioUri = ConfigManager.AppSettings["SalesAudioUri"];
               
            string audioUrl = String.Format("{0}/{1}/{2}/{3}/{4}_.mp3", salesAudioUri, followup.date_created.Value.Year, followup.date_created.Value.Month, followup.date_created.Value.Day, audioID);
               
            return audioUrl;
        }
        public static bool IsFileLock(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                if (stream != null)
                    stream.Close();
                return true;

            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }
        public static void SaveToFile(Stream pSrouce)
        {
            SaveFileDialog _dlgSave = new SaveFileDialog();
            _dlgSave.Filter = "wav files (*.wav)|*.wav|mp3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            _dlgSave.FilterIndex = 2;
            _dlgSave.RestoreDirectory = true;
            if (_dlgSave.ShowDialog() != DialogResult.OK) {
                pSrouce.Close();
                return;
            }

            if (File.Exists(_dlgSave.FileName)) {
                try {
                    File.Delete(_dlgSave.FileName);
                }
                catch {
                    //BrightVision.Common.UI.NotificationDialog.Information("Player", "Audio stream successfully saved to file.");
                    //MessageBox.Show("Cannot delete file. Copying audio fail.", "Copying audio fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            FileStream pDestination = new FileStream(_dlgSave.FileName, FileMode.Create, FileAccess.Write);
            int _Length = Convert.ToInt32(pSrouce.Length);
            Byte[] buffer = new Byte[_Length];
            int bytesRead = pSrouce.Read(buffer, 0, _Length);
            while (bytesRead > 0) {
                pDestination.Write(buffer, 0, bytesRead);
                bytesRead = pSrouce.Read(buffer, 0, _Length);
            }

            pSrouce.Close();
            pDestination.Close();
            BrightVision.Common.UI.NotificationDialog.Information("Player", "Audio stream successfully saved to file.");
        }
        public static void SaveToLocationAudio(string audioPath)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "wav files (*.wav)|*.wav|mp3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                if (File.Exists(saveFileDialog1.FileName)) {
                    try {
                        File.Delete(saveFileDialog1.FileName);
                    }
                    catch {
                        MessageBox.Show("Cannot delete file. Copying audio fail.", "Copying audio fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                if (!File.Exists(audioPath)) {
                    MessageBox.Show(string.Format("Audio file {0} not found.", audioPath), "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                File.Copy(audioPath, saveFileDialog1.FileName);
                BrightVision.Common.UI.NotificationDialog.Information("Player", "Audio stream successfully saved to file.");
                //MessageBox.Show("Save to Location successful.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public static string ReadUserLoginConf()
        {
            //string _FilePath = Application.StartupPath + @"\lastuser.conf";
            bool _JustCreated = false;
            string _Directory = string.Format(@"{0}\BrightVision", Environment.GetEnvironmentVariable("USERPROFILE"));
            if (!Directory.Exists(_Directory)) {
                Directory.CreateDirectory(_Directory);
                _JustCreated = true;
            }

            string _FilePath = string.Format(@"{0}\lastuser.conf", _Directory);
            if (!File.Exists(_FilePath)) {
                File.Create(_FilePath);
                _JustCreated = true;
            }

            if (_JustCreated)
                return string.Empty;

            string _LastUser = string.Empty;
            using (System.IO.StreamReader _srFile = new System.IO.StreamReader(_FilePath)) {
                _LastUser = _srFile.ReadLine();
            }

            return _LastUser;
        }
        public static void SaveUserLoginConf(string pLastUser)
        {
            string _Directory = string.Format(@"{0}\BrightVision", Environment.GetEnvironmentVariable("USERPROFILE"));
            if (!Directory.Exists(_Directory))
                Directory.CreateDirectory(_Directory);

            string _FilePath = string.Format(@"{0}\lastuser.conf", _Directory);
            if (!File.Exists(_FilePath))
                File.Create(_FilePath);

            using (System.IO.StreamWriter _srFile = new System.IO.StreamWriter(_FilePath)) {
                _srFile.Write(pLastUser);
            }
        }

        private static string XmlDecode(string value)
        {
            return value
              .Replace("&lt;", "<")
              .Replace("&gt;", ">")
              .Replace("&quot;", "\"")
              .Replace("&apos;", "'")
              .Replace("&amp;", "&");
        }
    }
}
