using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using BrightSales_Audio_Uploader.Properties;
using System.Drawing;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading;
using System.Diagnostics;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;

namespace WindowsAzureStorageBlob
{
    public partial class frmWindowsAzureStorageBlob : Form
    {
        #region Varaibles
        private NotifyIcon ni;
        private CloudBlobContainer pCloudContainer;
        private CloudBlobContainer pCloudContainerOld;
        public string pWindowsAzureStorageAccountName = "lii";
        public string pWindowsAzureStorageAccountKey = "GUmKQvgTqn2t3CxJb7M4b4hYxocBJ5C9GbDec+Rm4JIxuUX1qKqVf2WzI/5w5UAMPVSMUs3DGY9FFFGl1KK5ZA==";
        public string pWindowsAzureStorageContainerName = "iii";
        public string pWindowsAzureStorageContainerNameOld = "old";
        public string pBuildEnvironment = "d";
        public string pUserSessionConnectionString = @"metadata=res://*/BrightPlatform.csdl|res://*/BrightPlatform.ssdl|res://*/BrightPlatform.msl;provider=System.Data.SqlClient;provider connection string=#@#data source=192.168.1.245,1010\sql2008r2;initial catalog=BrightPlatform_NextVersion;persist security info=True;user id=michael;password=Bright1234;multipleactiveresultsets=True;Connection Timeout=60;Asynchronous Processing=true;App=EntityFramework#@#";

        private static string pLastUploadedAudio = "";
        public bool pDoneProcess = true;
        private static string[] p_staticFiles = new string[] { };
        private static Dictionary<string, DictionaryValues> p_DictionaryFiles = new Dictionary<string, DictionaryValues>();
        private static Dictionary<string, bool> p_DictionaryUploadedFiles = new Dictionary<string, bool>();
        private Thread pThreadProcessAudioConvert;
        private Thread pThreadProcessUpload;
        private Thread pThreadReProcessUpload;
        private static System.Windows.Forms.Timer pTimer;

        public string[] pFiles
        {
            set { p_staticFiles = value; }
            get { return p_staticFiles; }
        }

        private class DictionaryValues
        {
            private string _AdditionalParam = "";
            private bool _IsNew = true;

            public string AdditionalParam
            {
                set { _AdditionalParam = value; }
                get { return _AdditionalParam; }
            }
            public bool IsNew
            {
                set { _IsNew = value; }
                get { return _IsNew; }
            }
        }
        #endregion

        #region Constructor
        public frmWindowsAzureStorageBlob()
        {
            InitializeComponent();
            InitialzeWindowsAzureStorage();
            ni = new NotifyIcon();
            ni.ContextMenuStrip = new ContextMenuStrip();
        }
        #endregion

        #region Initialize
        void InitialzeWindowsAzureStorage()
        {
            try
            {
                string connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", pWindowsAzureStorageAccountName, pWindowsAzureStorageAccountKey);

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();


                //Retrieve the iii which is the new container
                // Retrieve a reference to a container. 
                pCloudContainer = blobClient.GetContainerReference(pWindowsAzureStorageContainerName);
                // Create the container if it doesn't already exist.
                pCloudContainer.CreateIfNotExists();
                pCloudContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });



                //Retrieve the old which is the old container
                // Retrieve a reference to a container. 
                pCloudContainerOld = blobClient.GetContainerReference(pWindowsAzureStorageContainerNameOld);
                // Create the container if it doesn't already exist.
                pCloudContainerOld.CreateIfNotExists();
                pCloudContainerOld.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
            catch
            {
                NotificationDialog.Error("BrightSales Audio Uploader", "Unable to connect to Windows Azure.\nPlease contact your system administrator.");
            }
        }


        /// <summary>
        /// Initialize to displays the icon in the system tray.
        /// </summary>
        public void InitializeIcon()
        {
            // Put the icon in the system tray and allow it react to mouse clicks.			
            ni.MouseClick += new MouseEventHandler(ni_MouseClick);
            ni.Icon = Resources.bv_logo;
            ni.Text = "BrightSales Audio Uploader";
            ni.Visible = true;
        }
        #endregion

        #region Events
        private void frmWindowsAzureStorageBlob_Load(object sender, EventArgs e)
        {
            InitializeIcon();
            this.Hide();

            if (p_DictionaryFiles.Count > 0)
                ni.ShowBalloonTip(1000, "BrightSales Audio Uploader", "1 file currently on que for uploading to Windows Azure", ToolTipIcon.Info);

            //DeleteBlobFile();

            /*
             * Check on files for those who were failed for processing/uploading to windows azure.
             */

            //pThreadReProcessUpload = new Thread(() => MoveFailedUploadAudios());
            //pThreadReProcessUpload.Start();

            if (pTimer == null)
            {
                pTimer = new System.Windows.Forms.Timer();
                pTimer.Interval = 3000;
                pTimer.Enabled = true;
                pTimer.Tick += pTimer_Tick;
            }
                
        }

        void pTimer_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.Process thisProc = System.Diagnostics.Process.GetCurrentProcess();
            if (!IsProcessOpen("SalesConsultant"))
            {
                pTimer.Enabled = false;
                ForceClosedApp();
            }
        }

        void ni_MouseClick(object sender, MouseEventArgs e)
        {
            // Handle mouse button clicks.
            if (e.Button == MouseButtons.Left)
            {
                Point pt = this.PointToScreen(e.Location);
                ContextMenuStrip contextMenuStrip1 = new ContextMenuStrip();


                ToolStripMenuItem Item = new ToolStripMenuItem();
                Item.Name = "BrightSales Audio Uploader";
                Item.Text = "BrightSales Audio Uploader";
                Item.ImageAlign = System.Drawing.ContentAlignment.TopRight;
                contextMenuStrip1.Items.Add(Item);

                contextMenuStrip1.Items.Add("-");


                Item = new ToolStripMenuItem();
                Item.Name = "Exit";
                Item.Text = "Exit";
                Item.Image = Resources.Exit;
                Item.Click += Item_Click;


                contextMenuStrip1.Items.Add(Item);

                contextMenuStrip1.Show(Cursor.Position);

                //int x = 0;
                //if (1 == x)
                //{
                //    MoveFailedUploadAudios();
                //    //CheckForFilesNotYetUploadedFromTmpWavFolder();
                //    //DeleteBlobFile();
                //}
            }
        }

        void Item_Click(object sender, EventArgs e)
        {
            if (!pDoneProcess)
            {
                //NotificationDialog.Information("BrightSales Audio Uploader", "Unable to exit as currently uploading a file.\nPlease wait for it to finish.");
                if (MessageBox.Show("There were file(s) currently being process/upload.\nWould you like to force closed it?", "BrightSales Audio Uploaded", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                {
                    ni.Dispose();
                    Application.Exit();
                }
            }
            else
            {
                ni.Dispose();
                Application.Exit();
            }
        }


        #endregion

        #region METHODS
        public void ForceClosedApp()
        {
            try
            {
                string lastKey = p_DictionaryFiles.Keys.Last();
                if (lastKey == pLastUploadedAudio && pDoneProcess)
                {
                    ni.Dispose();
                    Application.Exit();
                }
            }
            catch
            {
                ni.Dispose();
                Application.Exit();
            }
        }
        public void AddFile(string AdditionalParam, string fileName)
        {
            CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
            string path = commonData.ApplicationFolderPath + "\\tmpwav\\";

            fileName = path + fileName + "_.wav";


            if (!System.IO.File.Exists(fileName))
                return;

            /*
            string filename = Path.GetFileName(fileName);
            string filenameNoExt = Path.GetFileNameWithoutExtension(fileName);

            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName);

            ToolStripMenuItem Item = new ToolStripMenuItem();
            Item.Name = filenameNoExt;
            Item.Text = filename;
            Item.Image = icon.ToBitmap();

            ni.ContextMenuStrip.Items.Add(Item);

            if (pDoneProcess)
            {
                pFiles = new string[] { };
                p_DictionaryFiles.Clear();
            }

            p_DictionaryFiles.Add(filenameNoExt, AdditionalParam);
            */
            AddContextMenu(AdditionalParam, fileName);

            if (p_DictionaryFiles.Count > 0)
                ni.ShowBalloonTip(1000, "BrightSales Audio Uploader", "Added file on que for uploading to Windows Azure", ToolTipIcon.Info);

            StartThreadProcessAudioConvert(fileName);
        }
        public void AddFile(string file)
        {
            if (file.IndexOf(".mp3") > 0)
            {
                CheckFileBeforeProcess(file);
            }
            else
            {
                CheckFileBeforeProcess(file, true);
            }
        }
        private void AddContextMenu(string AdditionalParam, string file, bool isNew = true)
        {
            string filename = Path.GetFileName(file);
            string filenameNoExt = Path.GetFileNameWithoutExtension(file);

            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(file);

            ToolStripMenuItem Item = new ToolStripMenuItem();
            Item.Name = filenameNoExt;
            Item.Text = filename;
            Item.Image = icon.ToBitmap();

            ni.ContextMenuStrip.Items.Add(Item);

            if (pDoneProcess)
            {
                pFiles = new string[] { };
                //p_DictionaryFiles.Clear();
            }

            //p_DictionaryFiles.Add(filenameNoExt, AdditionalParam);
            p_DictionaryFiles[filenameNoExt] = new DictionaryValues();
            p_DictionaryFiles[filenameNoExt].AdditionalParam = AdditionalParam;
            p_DictionaryFiles[filenameNoExt].IsNew = isNew;
        }

        public void StartThreadProcessAudioConvert(string file)
        {
            pThreadProcessAudioConvert = new Thread(() => AudioConverter(file));
            pThreadProcessAudioConvert.Start();
        }

        public void StartThreadProcessUpload()
        {
            pThreadProcessUpload = new Thread(new ThreadStart(ProcessUpload));
            pThreadProcessUpload.Start();
        }

        private void ProcessUpload()
        {
            pTimer.Enabled = false;
            pDoneProcess = false;
            string filename = "";
            string filenameNoExt = "";
            bool IsAlreadyUploaded = false;

            int i = 0;
            foreach (string fileName in pFiles)
            {
                Application.DoEvents();
                //filename = filename = fileName.Replace(pPath, "").Replace(@"\", "");

                filename = Path.GetFileName(fileName);
                filenameNoExt = Path.GetFileNameWithoutExtension(fileName);
                pLastUploadedAudio = filenameNoExt;
                //Check if file was already uploaded so that it will just bypass
                if (p_DictionaryUploadedFiles.TryGetValue(filenameNoExt, out IsAlreadyUploaded)) continue;

                ni.ContextMenuStrip.Items[filenameNoExt].Image = Resources.Up;
                try
                {
                    string addParam = "";
                    bool IsNew = true;
                    //p_DictionaryFiles.TryGetValue(filenameNoExt, out addParam);
                    DictionaryValues dicValues;
                    try
                    {
                        addParam = p_DictionaryFiles[filenameNoExt].AdditionalParam;
                        IsNew = p_DictionaryFiles[filenameNoExt].IsNew;
                    }
                    catch { }


                    // Retrieve reference to a blob named "myblob".
                    CloudBlockBlob blockBlob = pCloudContainer.GetBlockBlobReference(addParam + filenameNoExt.Replace("-", "").Replace("_", "").ToUpper() + ".mp3");

                    if (!IsNew)
                    {
                        blockBlob = pCloudContainerOld.GetBlockBlobReference(filenameNoExt + ".mp3");
                    }

                    // Create or overwrite the "myblob" blob with contents from a local file.
                    using (var fileStream = System.IO.File.OpenRead(fileName))
                    {
                        blockBlob.Properties.ContentType = "audio/mpeg";
                        blockBlob.UploadFromStream(fileStream);
                    }

                    //Add files to dictionary that were successfully uploaded.
                    p_DictionaryUploadedFiles.Add(filenameNoExt, true);

                    ni.ContextMenuStrip.Items[filenameNoExt].Image = Resources.Check.ToBitmap();
                    SaveUploadedfile(fileName, addParam);

                    ni.ShowBalloonTip(1000, "BrightSales Audio Uploader", " file were successfully uploaded to Windows Azure", ToolTipIcon.Info);
                }
                catch (Exception ex)
                {
                    ni.ContextMenuStrip.Items[filenameNoExt].Image = Resources.Exit;
                    NotificationDialog.Error("BrightSales Audio Uploader", "Unable to upload file to azure.\nPlease contact your system administrator.");
                    ni.ShowBalloonTip(1000, "BrightSales Audio Uploader", " There were problem when trying to upload the file to windows azure.\nPlease contact your system administrator.", ToolTipIcon.Info);
                }

                Application.DoEvents();
            }

            pDoneProcess = true;


            System.Threading.Thread.Sleep(10000); //Let Ballon Tip show before disposing the app.

            string lastKey = p_DictionaryFiles.Keys.Last();
            if (lastKey == filenameNoExt)
            {
                pTimer.Enabled = false;
                ni.Dispose();
                Application.Exit();
            }
            else
            {
                StartThreadProcessUpload();
            }
        }

        private void SaveUploadedfile(string fileName, string additionalParam)
        {
            try
            {
                pUserSessionConnectionString = pUserSessionConnectionString.Replace("#@#", "\"");

                string _filename = Path.GetFileName(fileName);
                string _filenameNoExt = Path.GetFileNameWithoutExtension(fileName);

                _filenameNoExt = _filenameNoExt.Replace("_mic", "").Replace("_receiver", "").Replace("_", "");
                Guid audioId = Guid.Empty;

                if (Guid.TryParse(_filenameNoExt, out audioId))
                {
                    string _audiId = audioId.ToString().Replace("-", "").Replace("_", "");
                    using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(pUserSessionConnectionString))
                    {
                        var followup = objDbModel.event_followup_log.Where(param => param.audio_id == audioId).FirstOrDefault();

                        if (followup == null)
                        {
                            //Check if save on azure_blob_audio_id column.
                            followup = objDbModel.event_followup_log.Where(param => param.azure_blob_audio_id.Contains(_audiId)).FirstOrDefault();
                        }

                        if (followup != null && followup.is_azure_blob != null)
                        {
                            followup.main_uploaded = true;
                            followup.azure_blob_audio_id = additionalParam + _audiId.ToUpper();
                            followup.audio_id = null;
                            objDbModel.SaveChanges();
                        }
                        else 
                        {
                            followup.main_uploaded = true;
                            objDbModel.SaveChanges();
                        }

                        /*
                         * https://brightvision.jira.com/browse/PLATFORM-2947
                         */
                        File.Delete(fileName);
                    }
                }
            }
            catch
            {
                NotificationDialog.Error("BrightSales Audio Uploader", "Can't save audio to server.\nPlease contact your system administrator.");
            }
        }

        private void AudioConverter(string path)
        {
            string filenameNoExt = Path.GetFileNameWithoutExtension(path);
            string filename = filenameNoExt.Replace("_mic", "").Replace("_receiver", "").Replace("_", "");
            Guid audioId = Guid.Empty;
            FileInfo info = new FileInfo(path);

            if (Guid.TryParse(filename, out audioId) && info.Length > 0)
            {
                pUserSessionConnectionString = pUserSessionConnectionString.Replace("#@#", "\"");

                event_followup_log followup = null;
                using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(pUserSessionConnectionString))
                {
                    followup = objDbModel.event_followup_log.Where(param => param.audio_id == audioId).FirstOrDefault();                    

                    subcampaign _eftSubCampaign = null;
                    int _CustomerId = 0;
                    if (followup != null)
                    {
                        int _CampaignId = (int)objDbModel.subcampaigns.FirstOrDefault(i => i.id == followup.subcampaign_id).campaign_id;
                        _CustomerId = (int)objDbModel.campaigns.FirstOrDefault(i => i.id == _CampaignId).customer_id;
                    }

                    if (followup != null)
                        objDbModel.Detach(followup);

                    mciConvertWavMP3(path, true, _CustomerId);

                    //Thread.Sleep(3000);
                }
            }
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
                        /*
                         * https://brightvision.jira.com/browse/PLATFORM-2947
                         */
                        //Remove audio from tmpwav folder
                        File.Delete(fileName);
                        //Remove audio from cachewav folder
                        File.Delete(fileName.Replace(@"\tmpwav\", @"\cachewav\"));

                        //Logger log = new Logger(BrightVision.Logging.Enums.BrightVisionApplication.BrightSales);
                        //log.SendInfo("conversion_successful", new FileInfo(fileName).Name);
                        //UploadFile(mp3filename, customerId);
                        string filename = Path.GetFileNameWithoutExtension(mp3filename);
                        ni.ContextMenuStrip.Items[filename].Text = filename + "_.mp3";

                        string[] Files = pFiles;
                        Array.Resize(ref Files, Files.Length + 1);
                        Files[Files.Length - 1] = mp3filename;
                        pFiles = Files;

                        //p_DictionaryFiles.Add(fileName, AdditionalParam);
                        if (pDoneProcess) StartThreadProcessUpload();
                    }
                    else
                    {
                        //Logger log = new Logger(BrightVision.Logging.Enums.BrightVisionApplication.BrightSales);
                        //log.SendInfo("error_conversion", new FileInfo(fileName).Name);

                        var fileInfo = new FileInfo(fileName);
                        if (DateTime.Now.Subtract(fileInfo.CreationTime).Days > 5)
                        {
                            File.Move(fileName, fileName.Replace("tmpwav", "errorconversion"));
                            //log.SendInfo("move_to_conversion_folder", new FileInfo(fileName).Name);
                        }
                    }

                }
                catch { }

            }

            pThreadProcessAudioConvert.Abort();

        }

        public bool IsProcessOpen(string name)
        {
            foreach (System.Diagnostics.Process clsProcess in System.Diagnostics.Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }

        public void MoveFailedUploadAudios()
        {
            CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
            string FailedUploadFolder = commonData.ApplicationFolderPath + @"\FailedUpload";

            //Create folder if not exist
            bool isExists = System.IO.Directory.Exists(FailedUploadFolder);
            if (!isExists)
            {
                System.IO.Directory.CreateDirectory(FailedUploadFolder);
                System.Threading.Thread.Sleep(2000);
            }

            string filename = null;
            //Move mp3 audios from ex: C:\ProgramData\BrightVision\BrightSales to C:\ProgramData\BrightVision\FailedUpload
            string[] files = Directory.GetFiles(commonData.ApplicationFolderPath, "*.mp3");
            for (int i = 0; i < files.Length; i++)
            {
                filename = Path.GetFileName(files[i]);

                if (File.Exists(FailedUploadFolder + @"\" + filename))
                    File.Delete(files[i]);
                else
                    File.Move(files[i], FailedUploadFolder + @"\" + filename);
            }

            files = new string[] { };
            //Move wav audios from ex: C:\ProgramData\BrightVision\BrightSales\tmpwav to C:\ProgramData\BrightVision\FailedUpload
            files = Directory.GetFiles(commonData.ApplicationFolderPath + "\\tmpwav\\", "*.wav");
            for (int i = 0; i < files.Length; i++)
            {
                filename = Path.GetFileName(files[i]);

                if (File.Exists(FailedUploadFolder + @"\" + filename))
                    File.Delete(files[i]);
                else
                    File.Move(files[i], FailedUploadFolder + @"\" + filename);
            }

            files = new string[] { };
            //Move wav audios from ex: C:\ProgramData\BrightVision\BrightSales\cachewav to C:\ProgramData\BrightVision\FailedUpload
            files = Directory.GetFiles(commonData.ApplicationFolderPath + "\\cachewav\\", "*.wav");
            for (int i = 0; i < files.Length; i++)
            {
                filename = Path.GetFileName(files[i]);

                if (File.Exists(FailedUploadFolder + @"\" + filename))
                    File.Delete(files[i]);
                else
                    File.Move(files[i], FailedUploadFolder + @"\" + filename);
            }
        }
        #endregion

        #region Methods for reprocess/reuploading for failed things happen during uploading to windows azure/saving to database
        private void CheckForFilesNotYetUploaded()
        {
            ni.ShowBalloonTip(1000, "BrightSales Audio Uploader", "Checking on cached audio folder to reprocess failed audio(s).", ToolTipIcon.Info);

            /*
             * Process first all the converted audio to mp3 so that after it is being done uploaded/process,
             * we can delete its tmpwav and cache file so that when getting all the tmpwav audio, 
             * we already lessen the audio file to process.
             */
            CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
            string[] files = Directory.GetFiles(commonData.ApplicationFolderPath, "*.mp3");
            for (int i = 0; i < files.Length; i++)
            {
                CheckFileBeforeProcess(files[i]);
            }

            /*
             * Get all file from tmpwav to be process fro uploading to windows azure if not yet/has failure during its uploading.
             */
            CheckForFilesNotYetUploadedFromTmpWavFolder();
        }

        private void CheckForFilesNotYetUploadedFromTmpWavFolder()
        {
            CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
            string[] files = Directory.GetFiles(commonData.ApplicationFolderPath + "\\tmpwav\\", "*.wav");
            for (int i = 0; i < files.Length; i++)
            {
                CheckFileBeforeProcess(files[i], true);
            }
        }

        private void CheckFileBeforeProcess(string file, bool fromTempWavFolder = false)
        {
            try
            {
                pUserSessionConnectionString = pUserSessionConnectionString.Replace("#@#", "\"");

                string _filename = Path.GetFileName(file);
                string _filenameNoExt = Path.GetFileNameWithoutExtension(file);


                //DAN: Do not include file if already exist/added
                try
                {
                    DictionaryValues dv = p_DictionaryFiles[_filenameNoExt];
                    return;
                }catch
                {

                }


                _filenameNoExt = _filenameNoExt.Replace("_mic", "").Replace("_receiver", "").Replace("_", "");
                Guid audioId = Guid.Empty;

                if (Guid.TryParse(_filenameNoExt, out audioId))
                {
                    string _azure_blob_audio_id = audioId.ToString().Replace("-", "").Replace("_", "");
                    using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(pUserSessionConnectionString))
                    {
                        var followup = objDbModel.event_followup_log.Where(param => param.audio_id == audioId).FirstOrDefault();

                        if (followup == null)
                        {
                            //Check if save on azure_blob_audio_id column.
                            followup = objDbModel.event_followup_log.Where(param => param.azure_blob_audio_id.Contains(_azure_blob_audio_id)).FirstOrDefault();
                        }

                        if (followup == null) //This would mean that this audio file perhaps does not belong to BS
                        {
                            DeleteAudioFile(file, _filenameNoExt);
                            return;
                        }


                        int _CustomerId = 0;
                        int _CampaignId = (int)objDbModel.subcampaigns.FirstOrDefault(i => i.id == followup.subcampaign_id).campaign_id;
                        _CustomerId = (int)objDbModel.campaigns.FirstOrDefault(i => i.id == _CampaignId).customer_id;

                        bool isNew = (followup.is_azure_blob != null) ? true : false;
                        string AdditionParam = pBuildEnvironment + "/" + _CustomerId + "/" + DateTime.Parse(followup.date_created.ToString()).ToString("yyMMdd") + "/";
                        if (!CheckFileIfExistOnWindowsAzure(AdditionParam, _azure_blob_audio_id, isNew))
                        {                            
                            if (fromTempWavFolder) //Meaning file is located on tmpwav folder and was not yet converted to mp3 for process to upload to windows azure
                            {
                                //AddFile(AdditionParam, _filenameNoExt);
                                AddContextMenu(AdditionParam, file, isNew);
                                mciConvertWavMP3(file, true, _CustomerId);
                            }
                            else //Meaning file are already converted to mp3 but was not uploaded to windows azure perhaps due to error
                            {
                                AddContextMenu(AdditionParam, file, isNew);

                                _filenameNoExt += "_";
                                ni.ContextMenuStrip.Items[_filenameNoExt].Text = _filenameNoExt + ".mp3";

                                string[] Files = pFiles;
                                Array.Resize(ref Files, Files.Length + 1);
                                Files[Files.Length - 1] = file;
                                pFiles = Files;

                                if (pDoneProcess) StartThreadProcessUpload();
                                pDoneProcess = false;


                                /*
                                 * https://brightvision.jira.com/browse/PLATFORM-2947
                                 * Remove audio file except converted audio which is now mp3 so that it can be process for uploading
                                 */
                                //Remove audio from tmpwav folder
                                File.Delete(file.Replace(_filenameNoExt, @"tmpwav\" + _filenameNoExt));
                                //Remove audio from cachewav folder
                                File.Delete(file.Replace(_filenameNoExt, @"\cachewav\" + _filenameNoExt));
                            }
                        }
                        else //This would mean that file was successfully uploaded to windows azure
                        {
                            //This would mean that the mp3 audio file was not deleted after all process are successful.
                            if (followup.is_azure_blob != null && followup.azure_blob_audio_id != null)
                            {
                                DeleteAudioFile(file, _filenameNoExt);
                                return;
                            }

                            //This would mean that audio file was not successfully updated the database after all process are successful.
                            SaveUploadedfile(file, AdditionParam);
                        }
                    }
                }
            }
            catch
            {
                //NotificationDialog.Error("BrightSales Audio Uploader", "Can't save audio to server.\nPlease contact your system administrator.");
            }
        }

        private bool CheckFileIfExistOnWindowsAzure(string AdditionParam, string AudioId, bool IsNew = true)
        {
            // Retrieve reference to a blob.
            //FYI: Case Sensitive
            if (IsNew)
            {
                CloudBlockBlob blockBlob = pCloudContainer.GetBlockBlobReference(AdditionParam + AudioId.ToUpper() + ".mp3");

                if (blockBlob.Exists())
                    return true;
            }
            else
            {
                CloudBlockBlob blockBlob = pCloudContainerOld.GetBlockBlobReference(AudioId + "_.mp3");
                if (blockBlob.Exists())
                    return true;
            }

            return false;
        }

        private void DeleteAudioFile(string file, string _filenameNoExt)
        {
            //Remove audio from tmpwav folder
            if (file.IndexOf(".mp3") > 0)
            {
                File.Delete(file.Replace(_filenameNoExt, @"tmpwav\" + _filenameNoExt));
                //Remove audio from cachewav folder
                File.Delete(file.Replace(_filenameNoExt, @"\cachewav\" + _filenameNoExt));
                //Remove mp3 audio
                File.Delete(file);
            }
            else if (file.IndexOf(@"\tmpwav\") > 0)
            {
                //Remove audio from tmpwav folder
                File.Delete(file);
                //Remove audio from cachewav folder
                File.Delete(file.Replace(@"\tmpwav\", @"\cachewav\"));
                //Remove mp3 audio
                File.Delete(file.Replace(@"\tmpwav", ""));
            }
        }
        #endregion

        #region Sample code for deleting blob file on windows azure
        /*
         * Sample used
         */
        private void DeleteBlobFile()
        {
            string[] blobFile = new string[] { 
                "s/382/130524/76010D4D888F499B8E502652BF83A48B.mp3", 
                "s/382/130524/17F84E61CE37471899F653A169CB1665.mp3", 
                "s/382/130524/59FF55B3C16542CC8878D67614F9676C.mp3", 
                "s/382/130524/11FE4187F19346A99AE01F3D654E8A20.mp3", 
                "s/382/130524/2E57B84D219D479CA0E79D618D6465D6.mp3", 
                "s/382/130524/403F9B74003B4A1899338B04C956901C.mp3", 
                "s/382/130524/E92616B4913D488789512EB5D033AA29.mp3", 
                "s/382/130524/547F189C52C9489ABD42019DC4BCAE0C.mp3", 
                "s/382/130524/AFE7E08C232B4E789125DA6EE0552AA0.mp3", 
                "s/382/130524/3E12415419FD4AA6BCA6CBFAFA22C484.mp3" };

            for (int i = 0; i < blobFile.Length; i++)
            {
                //FYI: case sensitive
                CloudBlockBlob blockBlob = pCloudContainer.GetBlockBlobReference(blobFile[i]);

                blockBlob.DeleteIfExists();
            }
            MessageBox.Show("Done");
        }
        #endregion
    }
}
