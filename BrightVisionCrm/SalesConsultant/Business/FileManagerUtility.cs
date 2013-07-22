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

namespace SalesConsultant.Business
{
    public static class FileManagerUtility
    {

        public static string DownloadedAudioFile(Guid audioID)
        {
            string audioPath = string.Empty;
            try
            {
                var objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
                var followup = objDbModel.event_followup_log.Where(param => param.audio_id == audioID).FirstOrDefault();
                string salesAudioUri = ConfigurationManager.AppSettings["SalesAudioUri"];
                string webDavLicense = XmlDecode(ConfigurationManager.AppSettings["WebDavLicense"]);
                string[] usernamePassword = ConfigurationManager.AppSettings["WebDavUsernamePassword"].Split('|');
                string WebDavUserName = usernamePassword[0];
                string WebDavPassword = usernamePassword[1];
                string audioUrl = String.Format("{0}/{1}/{2}/{3}/{4}_.mp3", salesAudioUri, followup.date_created.Value.Year, followup.date_created.Value.Month, followup.date_created.Value.Day, audioID);
                var commonData = new CommonApplicationData("BrightVision", "BrightSales", true);

                return BrightSalesFacade.FileUpload.AudioToCacheFolder(audioUrl);
                //var session = new WebDavSession(webDavLicense);
                //session.Credentials = new NetworkCredential(WebDavUserName, WebDavPassword);
                //IFolder folder = session.OpenFolder(new Uri(salesAudioUri));
                //IResource resource = session.OpenResource(audioUrl);
                //resource.TimeOut = 36000000; // 10 hours

                //using (Stream webStream = resource.GetReadStream())
                //{
                //    int bufSize = 1048576; // 1Mb
                //    byte[] buffer = new byte[bufSize];
                //    int bytesRead = 0;
                //    audioPath = String.Format("{0}\\cachewav\\{1}", commonData.ApplicationFolderPath, resource.DisplayName);
                //    using (FileStream fileStream = File.OpenWrite(audioPath))
                //    {
                //        while ((bytesRead = webStream.Read(buffer, 0, bufSize)) > 0)
                //            fileStream.Write(buffer, 0, bytesRead);
                //    }
                //}
            }
            catch(Exception e) {
             //   MessageBox.Show("There is a problem connecting to the file server. please contact administrator. Error" +e.Message, "Cannot download audio file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return audioPath;
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
        private static string XmlDecode(string value)
        {
            return value
              .Replace("&lt;", "<")
              .Replace("&gt;", ">")
              .Replace("&quot;", "\"")
              .Replace("&apos;", "'")
              .Replace("&amp;", "&");
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
                MessageBox.Show("Save to Location successful.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
