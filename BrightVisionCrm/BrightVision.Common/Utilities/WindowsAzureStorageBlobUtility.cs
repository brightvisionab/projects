using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BrightVision.Common.Business;

namespace BrightVision.Common.Utilities
{
    public class WindowsAzureStorageBlobUtility
    {
        public void UploadFile(string path, int? customerId = 0)
        {
            string Environment = ConfigManager.AppSettings["BuildEnvironment"];
            string Env = "b";
            switch (Environment) {
                case "Production Environment": Env = "b"; break;
                case "Staging Environment": Env = "s"; break;
                case "Demo Environment": Env = "d"; break;
            }

            string AdditionParam = Env + "/" + customerId + "/" + DateTime.Now.ToString("yyMMdd") + "/";
            //string pworkingDir = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            this.RunWindowsAzureStorageBlob();

            System.Threading.Thread.Sleep(1000);
            System.Diagnostics.Process.Start("BrightSales_Audio_Uploader.exe",
                " \"" + AdditionParam + "\"" +
                " \"" + path + "\"");
        }
        public void UploadFile(string path)
        {
            this.RunWindowsAzureStorageBlob();

            System.Threading.Thread.Sleep(1000);
            System.Diagnostics.Process.Start("BrightSales_Audio_Uploader.exe", " \"" + path + "\"");
        }
        public void RunWindowsAzureStorageBlob()
        {
            System.Diagnostics.Process thisProc = System.Diagnostics.Process.GetCurrentProcess();
            if (IsProcessOpen("WindowsAzureStorageBlob") == false)
            {
                System.Diagnostics.Process.Start(
                    "BrightSales_Audio_Uploader.exe",
                    " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobAccountName"] + "\"" +
                    " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobAccountKey"] + "\"" +
                    " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobContainerName"] + "\"" +
                    " \"" + ConfigManager.AppSettings["WindowsAzureStorageBlobContainerNameOld"] + "\"" +
                    " \"" + UserSession.EntityConnection.ConnectionString.Replace("\"", "#@#") + "\"" +
                    " \"" + ConfigManager.AppSettings["BuildEnvironment"] + "\""
                );
                System.Threading.Thread.Sleep(1000);
                CheckIfWindowsAzureStorageBlobIsrunning();
            }
        }
        private void CheckIfWindowsAzureStorageBlobIsrunning()
        {
            System.Threading.Thread.Sleep(1000);
            if (IsProcessOpen("BrightSales_Audio_Uploader") == false)
            {
                CheckIfWindowsAzureStorageBlobIsrunning();
            }
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
        public bool CheckBlobFileExists(int followUpId, string audioId, bool IsNew = true)
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
                    //Common.UI.NotificationDialog.Error("Error", "Blob file does not exist.\nPlease consult system administrator.", followUpId);
                    return false;
                }
            }
            wab = null;

            return true;
        }

        public void ForceCloseBrightVisionAudioUploaderApp()
        {
            System.Diagnostics.Process.Start("BrightSales_Audio_Uploader.exe", "ForceClosed");
        }
    }
}
