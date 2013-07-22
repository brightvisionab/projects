using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
using BrightVision.Common.Business;
using BrightVision.Model;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Net;
using System.Drawing;
using System.Reflection;


namespace SalesConsultant.Forms {
    public partial class FrmAppUpdater : Form {

        public FrmAppUpdater() {
            InitializeComponent();
            CheckUpdates();
        }

        public bool HasNewUpdate { get; set; }
        private delegate void SetTextDownloadDetailsCallback(string text);
        private delegate void SetTextSizeDetailsCallback(string text);
        private delegate void SetTextListDetailsCallback(string text);
        private delegate void EditTextListDetailsCallback(string text);
        private Version newVersion = null;
        private BackgroundWorker backWorker = new BackgroundWorker();
        public bool DoneDownloadingUpdates = false;


        private void CheckUpdates() {
            BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            
            //string url = "";
            var appversion = BPContext.app_version.OrderByDescending(x => x.id).FirstOrDefault();
            if (appversion == null) return;

            string xmlString = appversion.config;
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmlString);
            var node = doc.SelectSingleNode("//sales_app");
            newVersion = new Version(node.Attributes["version"].Value);            
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (curVersion.CompareTo(newVersion) < 0) {
                lblCurrentVersion.Text = curVersion.ToString();
                lblLatestVersion.Text = newVersion.ToString();
                HasNewUpdate = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void btnProceed_Click(object sender, EventArgs e)
        {
            lblSizeDetails.Text = "";
            lblDownloadDetails.Text = "Initializing...";
            listDetails.Items.Add("Initializing...");
            pnlMain.Visible = false;
            pnlLoader.Location = pnlMain.Location;
            pnlLoader.Visible = true;
            btnDetails.Visible = true;
            btnDetails.Enabled = true;

            btnProceed.Visible = false;
            btnProceed.Enabled = false;

            Application.DoEvents();

            backWorker.WorkerSupportsCancellation = true;
            backWorker.DoWork += new DoWorkEventHandler(StartDownloadUpdates);
            backWorker.RunWorkerAsync();
        }

        private void StartDownloadUpdates(object sender, DoWorkEventArgs e)
        {
            try
            {
                lblDownloadDetails.Text = "Connecting to server to get updates...";
                listDetails.Items.Add("Connecting to server to get updates...");
                Application.DoEvents();

                string tempPath = System.IO.Path.GetTempPath() + @"\Brightvision";

                WebClient webClient = new WebClient();
                //Stream data = webClient.OpenRead("http://lii.blob.core.windows.net/updates/Chart.zip");
                // This stream cannot be opened with the ZipFile class because CanSeek is false.
                //UnZipFiles(zipPath, tempPath + @"\Brightvision", null, false);

                //string file = @"Brightvision";
                string file = @"BrightVision_" + newVersion.ToString();
                Stream data = webClient.OpenRead(string.Format("http://lii.blob.core.windows.net/updates/{0}.zip", file));

                lblDownloadDetails.Text = "Connected to server...";
                listDetails.Items.Add("Connected to server...");
                Application.DoEvents();

                UnzipFromStream(data, tempPath, null, false);
                StartUpdatingApps();
            }
            catch(Exception ex)
            {
                //(new BrightVision.Common.Utilities.RaygunClientLogger()).Send(ex);
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Application.Exit();
        }

        public void UnzipFromStream(Stream zipStream, string outputFolder, string password, bool deleteZipFile)
        {
            try
            {
                if(Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);

                ZipInputStream s = new ZipInputStream(zipStream);
                if (password != null && password != String.Empty)
                    s.Password = password;
                ZipEntry theEntry;
                string tmpEntry = String.Empty;

                lblDownloadDetails.Text = "Preparing to download files...";
                listDetails.Items.Add("Preparing to download files...");
                Application.DoEvents();

                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = outputFolder;
                    string fileName = Path.GetFileName(theEntry.Name);
                    // create directory 
                    if (directoryName != "")
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    if (fileName != String.Empty)
                    {
                        if (theEntry.Name.IndexOf(".ini") < 0)
                        {
                            this.SetTextSizeDetails("0/" + s.Length.ToString());

                            this.SetTextDownloadDetails("Downloading " + theEntry.Name.Substring(theEntry.Name.LastIndexOf('/') + 1, theEntry.Name.Length - theEntry.Name.LastIndexOf('/') - 1));
                            SetTextListDetails("Downloading " + theEntry.Name);
                            Application.DoEvents();

                            string fullPath = directoryName + "\\" + theEntry.Name;
                            fullPath = fullPath.Replace("\\ ", "\\");
                            string fullDirPath = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                            FileStream streamWriter = File.Create(fullPath);
                            int sizetotal = 0;
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {                                    
                                    streamWriter.Write(data, 0, size);
                                    sizetotal += size;
                                    this.SetTextSizeDetails(sizetotal.ToString() + "/" + s.Length.ToString());
                                    Application.DoEvents();
                                }
                                else
                                {
                                    break;
                                }
                            }
                            streamWriter.Close();
                            EditTextListDetails("Downloading " + theEntry.Name + "...done");
                            Application.DoEvents();
                        }
                    }
                }
                s.Close();      

                DoneDownloadingUpdates = true;
                this.SetTextDownloadDetails("Download completed");
                this.SetTextSizeDetails("");
                SetTextListDetails("Download completed");
            }
            catch (Exception e)
            {
                //(new BrightVision.Common.Utilities.RaygunClientLogger()).Send(e);
                MessageBox.Show(e.Message, e.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            backWorker.Dispose();
        }
        
        private static void UnZipFiles(string zipPathAndFile, string outputFolder, string password, bool deleteZipFile)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
            if (password != null && password != String.Empty)
                s.Password = password;
            ZipEntry theEntry;
            string tmpEntry = String.Empty;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = outputFolder;
                string fileName = Path.GetFileName(theEntry.Name);
                // create directory 
                if (directoryName != "")
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (fileName != String.Empty)
                {
                    if (theEntry.Name.IndexOf(".ini") < 0)
                    {
                        string fullPath = directoryName + "\\" + theEntry.Name;
                        fullPath = fullPath.Replace("\\ ", "\\");
                        string fullDirPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                        FileStream streamWriter = File.Create(fullPath);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        streamWriter.Close();
                    }
                }
            }
            s.Close();
            if (deleteZipFile)
                File.Delete(zipPathAndFile);
        }

        private void SetTextDownloadDetails(string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.lblDownloadDetails.InvokeRequired)
            {
                SetTextDownloadDetailsCallback d = new SetTextDownloadDetailsCallback(SetTextDownloadDetails);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lblDownloadDetails.Text = text;
                               
                string str = this.lblDownloadDetails.Text;
                int iCountWords = str.Length;
                if (iCountWords > 45)
                {
                    this.lblDownloadDetails.Text = str.Substring(0, 45) + "...";
                }
               
            }
        }

        private void SetTextSizeDetails(string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.lblSizeDetails.InvokeRequired)
            {
                SetTextSizeDetailsCallback d = new SetTextSizeDetailsCallback(SetTextSizeDetails);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lblSizeDetails.Location = new Point((lblDownloadDetails.Location.X + lblDownloadDetails.Size.Width), lblDownloadDetails.Location.Y);
                this.lblSizeDetails.Text = text;

                //int curX = this.lblSizeDetails.Location.X;
                //int lblWidth = this.lblSizeDetails.Width;
                //int iTotalWidth = curX + lblWidth;
                //int formWidth = this.Width;
                //this.lblSizeDetails.Location = new Point(curX - (56 - (formWidth - iTotalWidth)), this.lblSizeDetails.Location.Y);
            }
        }

        private void SetTextListDetails(string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.listDetails.InvokeRequired)
            {
                SetTextListDetailsCallback d = new SetTextListDetailsCallback(SetTextListDetails);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.listDetails.Items.Add(text + "...downloading");
                this.listDetails.TopIndex = this.listDetails.Items.Count-1;
            }
        }
        private void EditTextListDetails(string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.listDetails.InvokeRequired)
            {
                EditTextListDetailsCallback d = new EditTextListDetailsCallback(EditTextListDetails);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.listDetails.Items[this.listDetails.Items.Count-1] = text;
            }
        }
        
        private void btnDetails_Click(object sender, EventArgs e)
        {
            if (btnDetails.Text == "View Details")
            {
                btnDetails.Text = "Hide Details";
                this.Height = 548;
            }
            else if (btnDetails.Text == "Hide Details")
            {
                btnDetails.Text = "View Details";
                this.Height = 200;

            }
        }

        static void StartUpdatingApps()
        {
            System.Diagnostics.Process.Start("BrightVision.LoadAppUpdater.Memory.exe");
        }
    }
}
