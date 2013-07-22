using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using ITHit.WebDAV.Client;
using System.Net;
using System.Configuration;
using BrightVision.Logging;
using System.Management;
using System.Diagnostics;




namespace BrightVision.FileManagement
{
    
    public class WebDavFileManager
    {
        #region Variables
        BackgroundWorker backWorkerUploader;
        BackgroundWorker backWorkerAudioConverter;
        BackgroundWorker backWorkerCheckWebDavConnection;
        bool isClose = false;
        bool isUploadDone = false;
        bool isAudioConverterDone = false;
        string webDavLicense;
        string salesAudioUri;
        string WebDavUserName;
        string WebDavPassword;
        bool skipAutoCheckConnection = false;
        bool connectionFail = false;
        //string webDavLicense1 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><License><Data><Product>IT Hit WebDAV Client .Net v1</Product><LicensedTo><![CDATA[Ulysses Maglana]]></LicensedTo><Quantity>1</Quantity><IssueDate><![CDATA[Tuesday, August 28, 2012]]></IssueDate><ExpirationDate><![CDATA[Friday, September 28, 2012]]></ExpirationDate><Type>Evaluation</Type></Data><Signature><![CDATA[BVL1CyxQRvr6XM13yJcqIgoDUV7mCizxbtQ6JyPQBvPeZo6z32ZWtaOFAmZ5uwp6uZjak8YHCwSn0HneIrcNnjHhC1RIpnm7h9tZk4HBSsL075NLIVlakGWxqIaVB99GbaDr2mWh6fpqKk9APZ65HKW9E7fHbmogYu7r03hQw6k=]]></Signature></License>";
        #endregion

        #region Constructor
        public WebDavFileManager()
        {

            salesAudioUri = ConfigManager.AppSettings["SalesAudioUri"];
            webDavLicense = XmlDecode(ConfigManager.AppSettings["WebDavLicense"]);
            string[] usernamePassword = ConfigManager.AppSettings["WebDavUsernamePassword"].Split('|');
           WebDavUserName = usernamePassword[0];
           WebDavPassword = usernamePassword[1];
           backWorkerUploader = new BackgroundWorker();
           backWorkerAudioConverter = new BackgroundWorker();
           backWorkerCheckWebDavConnection = new BackgroundWorker();
           backWorkerAudioConverter.WorkerSupportsCancellation = true;
           backWorkerUploader.WorkerSupportsCancellation = true;
           backWorkerCheckWebDavConnection.WorkerSupportsCancellation = true;
           //backWorkerUploader.DoWork += new DoWorkEventHandler(backWorkerUploader_DoWork);
           //backWorkerAudioConverter.DoWork += new DoWorkEventHandler(backWorkerAudioConverter_DoWork);
           //backWorkerCheckWebDavConnection.DoWork += new DoWorkEventHandler(backWorkerCheckWebDavConnection_DoWork);
           CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
           commonData.CreateFolder(commonData.ApplicationFolderPath + "\\tmpwav");
           commonData.CreateFolder(commonData.ApplicationFolderPath + "\\cachewav");
           commonData.CreateFolder(commonData.ApplicationFolderPath + "\\errorconversion");
        }
        public WebDavFileManager(bool IsAdmin) {
            salesAudioUri = ConfigManager.AppSettings["SalesAudioUri"];
            webDavLicense = XmlDecode(ConfigManager.AppSettings["WebDavLicense"]);
            string[] usernamePassword = ConfigManager.AppSettings["WebDavUsernamePassword"].Split('|');
            WebDavUserName = usernamePassword[0];
            WebDavPassword = usernamePassword[1];
            CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
            commonData.CreateFolder(commonData.ApplicationFolderPath + "\\tmpwav");
            commonData.CreateFolder(commonData.ApplicationFolderPath + "\\cachewav");
            commonData.CreateFolder(commonData.ApplicationFolderPath + "\\errorconversion");
        }

        public void Start() {
            //backWorkerCheckWebDavConnection.RunWorkerAsync();
            //backWorkerUploader.RunWorkerAsync();
            //backWorkerAudioConverter.RunWorkerAsync();
        }

        public string AudioToCacheFolder(string audioUrl)
        {
            var commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
            string audioPath = string.Empty;

            try
            {
                IResource resource = GetWebDavSession().OpenResource(audioUrl);
                resource.TimeOut = 36000000; // 10 hours
                using (Stream webStream = resource.GetReadStream())
                {
                    int bufSize = 1048576; // 1Mb
                    byte[] buffer = new byte[bufSize];
                    int bytesRead = 0;
                    audioPath = String.Format("{0}\\cachewav\\{1}", commonData.ApplicationFolderPath, resource.DisplayName);
                    using (FileStream fileStream = File.OpenWrite(audioPath))
                    {
                        while ((bytesRead = webStream.Read(buffer, 0, bufSize)) > 0)
                            fileStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception e)
            {
                
            }
            return audioPath;
        }

        public bool IsFileExist(string audioUrl, string audioId)
        {
            string salesAudioUri = audioUrl.Replace(audioId + "_.mp3", "");
            IFolder folder = GetWebDavSession().OpenFolder(new Uri(salesAudioUri));
            return folder.ItemExists(audioId + "_.mp3");
        }

        private void CheckConnection()
        {
            try
            {
                IFolder folder = GetWebDavSession().OpenFolder(new Uri(salesAudioUri));
                connectionFail = false;
            }
            catch {
                connectionFail = true;
                if (ConnectionFail != null)
                    ConnectionFail("Connection to the audio service is not available. Please contact the administrator. \nAuto-check connection every 3 minutes.");
            }
        }

        #endregion

        #region Event Methods

        void backWorkerCheckWebDavConnection_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(120000);
            for (; ; )
            {
                if (isClose)
                {
                    backWorkerCheckWebDavConnection.CancelAsync();
                    break;
                }
                if (skipAutoCheckConnection)
                {
                    //skipAutoCheckConnection = false;
                }
                else
                {
                    CheckConnection();
                }
                //3 min
                Thread.Sleep(180000);
            }
        }
        void backWorkerAudioConverter_DoWork(object sender, DoWorkEventArgs e)
        {
            for(;;){
                if (isClose)
                {
                    backWorkerAudioConverter.CancelAsync();
                    break;
                }
                CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
                var result = Directory.GetFiles(commonData.ApplicationFolderPath + "\\tmpwav");
                if (result.Count() > 0)
                {
                    string[] copyres = GetFileNames(result);
                    Logger log = new Logger(Logging.Enums.BrightVisionApplication.BrightSales);
                    log.SendInfo("array_of_wav_files", string.Join(", ", copyres));
                }

                Guid audioId = Guid.Empty;
               
                foreach (string path in result)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(path);
                        if (isClose)
                        {
                            backWorkerAudioConverter.CancelAsync();
                            break;
                        }

                        if (
                            File.Exists(path.Replace(".wav", ".mp3").Replace("\\tmpwav", "")) ||
                            IsFileLocked(fileInfo))
                        {
                            continue;
                        }
                        else
                        {
                            string filenameNoExt = Path.GetFileNameWithoutExtension(path);
                            string filename = filenameNoExt.Replace("_mic", "").Replace("_receiver", "").Replace("_", "");
                            audioId = Guid.Empty;
                            FileInfo info = new FileInfo(path);

                            if (Guid.TryParse(filename, out audioId) && info.Length > 0)
                            {
                                event_followup_log followup = null;
                                using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection))
                                {
                                    followup = objDbModel.event_followup_log.Where(param => param.audio_id == audioId).FirstOrDefault();
                                    
                                    if (followup == null)
                                    {
                                        if (DateTime.Now.Subtract(info.CreationTime).Days > 2) {
                                            File.Delete(path);
                                        }
                                        continue;
                                    }
                                    else if (
                                        filenameNoExt.Contains("_mic") &&
                                        followup.mic_uploaded.HasValue &&
                                        followup.mic_uploaded.Value)
                                    {
                                        File.Delete(path);
                                        objDbModel.Detach(followup);
                                        continue;
                                    }
                                    else if (
                                        filenameNoExt.Contains("_receiver") &&
                                        followup.mic_uploaded.HasValue &&
                                        followup.mic_uploaded.Value)
                                    {
                                        File.Delete(path);
                                        objDbModel.Detach(followup);
                                        continue;
                                    }
                                    else if (
                                        filenameNoExt.Contains("_") &&
                                        followup.main_uploaded.HasValue &&
                                        followup.main_uploaded.Value)
                                    {
                                        File.Delete(path);
                                        objDbModel.Detach(followup);
                                        continue;
                                    }

                                    subcampaign _eftSubCampaign = null;
                                    int _CustomerId = 0;
                                    if (followup != null) {
                                        int _CampaignId = (int)objDbModel.subcampaigns.FirstOrDefault(i => i.id == followup.subcampaign_id).campaign_id;
                                        _CustomerId = (int)objDbModel.campaigns.FirstOrDefault(i => i.id == _CampaignId).customer_id;
                                    }

                                    if (followup != null)
                                        objDbModel.Detach(followup);

                                    mciConvertWavMP3(path, true, _CustomerId);

                                    Thread.Sleep(3000);
                                }
                            }
                            else
                            {
                                File.Delete(path);
                                continue;
                            }
                        }

                        //This will convert from wav to mp3 move the mp3 to the main folder
                        //if (!File.Exists(path))
                        //    File.Copy(path, path.Replace("tmpwav", "cachewav"));
                    
                    }
                    catch (Exception ex)
                    {
                        Logger log = new Logger(Logging.Enums.BrightVisionApplication.BrightSales);
                        log.Error("error_convertaudio", "WebDavFileManager.backWorkerAudioConverter_DoWork_" + audioId, ex);
                    }
                    if (isClose)
                        break;
                }
                if (isClose)
                    break;
                Thread.Sleep(3000);
            }
            isAudioConverterDone = true;
        }

        private static string[] GetFileNames(string[] result)
        {
            string[] copyres = new string[result.Count()];
            result.CopyTo(copyres, 0);
            for (int cnt = 0; cnt < copyres.Count(); cnt++)
            {
                var fileInfo = new FileInfo(copyres[cnt]);
                copyres[cnt] = fileInfo.Name;
            }
           
            return copyres;
        }
        void backWorkerUploader_DoWork(object sender, DoWorkEventArgs e)
        {
            for (;;)
            {
                if (connectionFail)
                    Thread.Sleep(180000);
                
                if (isClose)
                    break;
                CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
                var result = Directory.GetFiles(commonData.ApplicationFolderPath);
                if (result.Count() > 0)
                {
                    string[] copyres = GetFileNames(result);
                    Logger log = new Logger(Logging.Enums.BrightVisionApplication.BrightSales);
                    log.SendInfo("array_of_mp3_files", string.Join(", ", result));
                }

                skipAutoCheckConnection = false;

                Guid audioId = Guid.Empty;
                
                    foreach (string path in result)
                    {
                        try
                        {
                            string filenameNoExt = Path.GetFileNameWithoutExtension(path);
                            FileInfo fileInfo = new FileInfo(path);
                            if (IsFileLocked(fileInfo))
                            {
                                Thread.Sleep(3000);
                                continue;
                            }

                            if (filenameNoExt.EndsWith("_") ||
                                filenameNoExt.EndsWith("_mic") ||
                                filenameNoExt.EndsWith("_receiver"))
                            {
                                string filename = filenameNoExt.Replace("_mic", "").Replace("_receiver", "").Replace("_", "");
                                audioId = Guid.Empty;

                                if (Guid.TryParse(filename, out audioId))
                                {
                                    BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
                                    var followup = objDbModel.event_followup_log.Where(param => param.audio_id == audioId).FirstOrDefault();

                                    if (followup != null)
                                    {
                                        skipAutoCheckConnection = true;



                                        SetDaysFolder(followup);

                                        var date = followup.date_created.Value;
                                        string year = date.Year.ToString();
                                        string month = date.Month.ToString();
                                        string days = date.Day.ToString();

                                        IFolder folder = GetWebDavSession().OpenFolder(salesAudioUri + "/" + year + "/" + month + "/" + days);
                                        FileInfo file = new FileInfo(path);

                                        if (folder.ItemExists(file.Name))
                                        {
                                            IResource todelete = folder.GetResource(file.Name);
                                            todelete.Delete();
                                        }

                                        IResource resource = folder.CreateResource(file.Name);
                                        resource.TimeOut = 36000000;
                                        using (Stream webStream = resource.GetWriteStream(file.Length))
                                        {
                                            int bufSize = 1048576; // 1Mb
                                            byte[] buffer = new byte[bufSize];
                                            int bytesRead = 0;

                                            using (FileStream fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                                            {
                                                while ((bytesRead = fileStream.Read(buffer, 0, bufSize)) > 0)
                                                {
                                                    webStream.Write(buffer, 0, bytesRead);
                                                    if (isClose)
                                                        break;
                                                }
                                            }
                                        }

                                        if (isClose)
                                            break;

                                        //Save to database when file is save to the server
                                        if (filenameNoExt.Contains("_mic"))
                                        {
                                            followup.mic_uploaded = true;
                                            objDbModel.SaveChanges();
                                        }
                                        else if (filenameNoExt.Contains("_receiver"))
                                        {
                                            followup.receiver_uploaded = true;
                                            objDbModel.SaveChanges();
                                        }
                                        else if (filenameNoExt.Contains("_"))
                                        {
                                            followup.main_uploaded = true;
                                            objDbModel.SaveChanges();
                                        }


                                        File.Delete(path);

                                        if (isClose)
                                            break;
                                    }
                                    //else {
                                    //    File.Delete(path);
                                    //}
                                    try
                                    {
                                        if (followup != null)
                                            objDbModel.Detach(followup);
                                    }
                                    catch { }

                                }
                                else
                                {
                                    File.Delete(path);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger log = new Logger(Logging.Enums.BrightVisionApplication.BrightSales);
                            log.Error("error_uploadaudio", "WebDavFileManager.backWorkerUploader_DoWork_" + audioId, ex);
                            skipAutoCheckConnection = false;
                        }
                    }
                    if (isClose)
                        break;
                skipAutoCheckConnection = false;
                Thread.Sleep(3000);
            }
            isUploadDone = true;
        }
        
        #endregion

        #region Private
        WebDavSession session;
        private WebDavSession GetWebDavSession()
        {
            if (session == null || connectionFail){
                session = new WebDavSession(webDavLicense);
                session.Credentials = new NetworkCredential(WebDavUserName, WebDavPassword);
            }
             
            return session;
        }
        private void SetDaysFolder(event_followup_log followup)
        {
            var date = followup.date_created.Value;
            string year = date.Year.ToString();
            string month = date.Month.ToString();
            string days = date.Day.ToString();
            IFolder folder = null;
            if (!GetWebDavSession().OpenFolder(salesAudioUri).ItemExists(year))
               GetWebDavSession().OpenFolder(salesAudioUri).CreateFolder(year);

            if (!GetWebDavSession().OpenFolder(salesAudioUri +"/"+ year).ItemExists(month))
                GetWebDavSession().OpenFolder(salesAudioUri + "/" + year).CreateFolder(month);

            if (!GetWebDavSession().OpenFolder(salesAudioUri + "/" + year + "/" + month + "/").ItemExists(days))
               GetWebDavSession().OpenFolder(salesAudioUri + "/" + year + "/" + month + "/").CreateFolder(days);
        
        }
        private void SetDaysFolder(event_followup_log followup, ref IFolder folder)
        {
            var date = followup.date_created.Value;
            string year = date.Year.ToString();
            string month = date.Month.ToString();
            string days = date.Day.ToString();

            if (folder.ItemExists(year))
                folder = folder.GetFolder(year);
            else
            {
                folder = folder.CreateFolder(year);
            }


            if (folder.ItemExists(month))
                folder = folder.GetFolder(month);
            else
                folder = folder.CreateFolder(month);

            if (folder.ItemExists(days))
                folder = folder.GetFolder(days);
            else
                folder = folder.CreateFolder(days);
        }
        private void mciConvertWavMP3(string fileName, bool waitFlag, int? customerId = 0)
        {
            string pworkingDir = Path.GetDirectoryName(Application.ExecutablePath);
            string mp3filename = fileName.Replace(".wav", ".mp3").Replace("\\tmpwav", "");
            if (!File.Exists(mp3filename))
            {
                string outfile = String.Format("-b 32 --resample 22.05 -m m \"{0}\" \"{1}\"", fileName, mp3filename);
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = String.Format("\"{0}\\lame.exe\"", pworkingDir);
                psi.Arguments = outfile;
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi);
                if (waitFlag)
                {
                    p.WaitForExit();
                }
                try
                {
                    if (File.Exists(mp3filename))
                    {
                        Logger log = new Logger(Logging.Enums.BrightVisionApplication.BrightSales);
                        log.SendInfo("conversion_successful", new FileInfo(fileName).Name);
                        //UploadFile(mp3filename, customerId);
                    }
                    else
                    {
                        Logger log = new Logger(Logging.Enums.BrightVisionApplication.BrightSales);
                        log.SendInfo("error_conversion", new FileInfo(fileName).Name);

                        var fileInfo = new FileInfo(fileName);
                        if (DateTime.Now.Subtract(fileInfo.CreationTime).Days > 5)
                        {
                            File.Move(fileName, fileName.Replace("tmpwav", "errorconversion"));
                            log.SendInfo("move_to_conversion_folder", new FileInfo(fileName).Name);
                        }
                    }

                }
                catch { }
               
            }
           
        }

        private void DeleteTempFiles() {
            try
            {
                CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
                //string tmpfolder = commonData.ApplicationFolderPath + "\\tmpwav";
                //var filesPath = Directory.GetFiles(tmpfolder);
                //foreach (string path in filesPath)
                //{
                //    File.Delete(path);
                //}

                string tmpfolder = commonData.ApplicationFolderPath + "\\cachewav";
                string[] filesPath = Directory.GetFiles(tmpfolder);
                foreach (string path in filesPath)
                {
                    if(!File.Exists(path.Replace("cachewav", "tmpwav")))
                        File.Delete(path);
                }
            }
            catch { 
            
            }
        }
        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private void UploadFile(string path, int? customerId = 0)
        {
            string Environment = ConfigManager.AppSettings["BuildEnvironment"];
            string Env = "b";
            switch (Environment)
            {
                case "Production Environment": Env = "b"; break;
                case "Staging Environment": Env = "s"; break;
                case "Demo Environment": Env = "d"; break;
            }

            string AdditionParam = Env + "/" + customerId + "/" + DateTime.Now.ToString("yyMMdd") + "/";

            string pworkingDir = Path.GetDirectoryName(Application.ExecutablePath);

            RunWindowsAzureStorageBlob(pworkingDir);

            //System.Diagnostics.Process.Start(String.Format("\"{0}\\WindowsAzureStorageBlob.exe\"", pworkingDir),
            //    " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobAccountName"] + "\"" +
            //    " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobAccountKey"] + "\"" +
            //    " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobContainerName"] + "\"" +
            //    " \"" + AdditionParam + "\"" +
            //    " \"" + path + "\"");

            System.Diagnostics.Process.Start("WindowsAzureStorageBlob.exe",
                " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobAccountName"] + "\"" +
                " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobAccountKey"] + "\"" +
                " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobContainerName"] + "\"" +
                " \"" + AdditionParam + "\"" +
                " \"" + path + "\"");
        }

        private void RunWindowsAzureStorageBlob(string pworkingDir)
        {
            //MessageBox.Show(String.Format("\"{0}\\WindowsAzureStorageBlob.exe\"", pworkingDir));
            Process thisProc = Process.GetCurrentProcess();
            if (IsProcessOpen("WindowsAzureStorageBlob") == false)
            {
                //System.Diagnostics.Process.Start(String.Format("\"{0}\\WindowsAzureStorageBlob.exe\"", pworkingDir));
                System.Diagnostics.Process.Start("WindowsAzureStorageBlob.exe", "\"" + UserSession.EntityConnection.ConnectionString.Replace("\"","#@#") + "\"");
                CheckIfWindowsAzureStorageBlobIsrunning();
            }
        }

        private void CheckIfWindowsAzureStorageBlobIsrunning()
        {
            System.Threading.Thread.Sleep(1000);
            if (IsProcessOpen("WindowsAzureStorageBlob") == false)
            {
                CheckIfWindowsAzureStorageBlobIsrunning();
            }
        }

        public bool IsProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }

        private string XmlDecode(string value)
        {
            return value
              .Replace("&lt;", "<")
              .Replace("&gt;", ">")
              .Replace("&quot;", "\"")
              .Replace("&apos;", "'")
              .Replace("&amp;", "&");
        }
        #endregion

        #region Public

        public void Stop()
        {
            isClose = true;

            if (!(isUploadDone && isAudioConverterDone))
            {
                Thread.Sleep(2000);
                Stop();
            }
            else
            {
                DeleteTempFiles();
            }
        }
        #endregion

        #region Events
        public delegate void ConnectionFailHandler(string message);
        public event ConnectionFailHandler ConnectionFail;
        #endregion


    }
}
