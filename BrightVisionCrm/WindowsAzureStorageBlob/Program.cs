using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using System.Threading;

namespace WindowsAzureStorageBlob
{
    
    static class Program
    {
        static frmWindowsAzureStorageBlob MainForm;
        static Mutex mutex;

        [STAThread]
        static void Main(params string[] Arguments)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new frmWindowsAzureStorageBlob();
            if (Arguments.Length == 6)
            {
                MainForm.pWindowsAzureStorageAccountName = Arguments[0];
                MainForm.pWindowsAzureStorageAccountKey = Arguments[1];
                MainForm.pWindowsAzureStorageContainerName = Arguments[2];
                MainForm.pWindowsAzureStorageContainerNameOld = Arguments[3];
                MainForm.pUserSessionConnectionString = Arguments[4];

                string Env = Arguments[5];
                switch (Env)
                {                    
                    case "Staging Environment": Env = "s"; break;
                    case "Demo Environment": Env = "d"; break;
                    //case "Production Environment": Env = "b"; break;
                    default: Env = "b"; break;
                }
                MainForm.pBuildEnvironment = Env;
            }
            SingleInstanceApplication.Run(MainForm, NewInstanceHandler);
        }

        public static void NewInstanceHandler(object sender, StartupNextInstanceEventArgs e)
        {
            if (e.CommandLine.Count == 3)
            {
                MainForm.AddFile(e.CommandLine[1], e.CommandLine[2]);
            }
            else if (e.CommandLine.Count == 2)
            {
                if (e.CommandLine[1] == "ForceClosed")
                    MainForm.ForceClosedApp();
                else if (e.CommandLine[1] == "MoveFailedUploadAudios")
                    MainForm.MoveFailedUploadAudios();
                else
                    MainForm.AddFile(e.CommandLine[1]);
            }
            e.BringToForeground = false;
        }

        public class SingleInstanceApplication : WindowsFormsApplicationBase
        {
            private SingleInstanceApplication()
            {
                base.IsSingleInstance = true;
            }

            public static void Run(Form f, StartupNextInstanceEventHandler startupHandler)
            {
                SingleInstanceApplication app = new SingleInstanceApplication();
                app.MainForm = f;
                app.StartupNextInstance += startupHandler;
                app.Run(Environment.GetCommandLineArgs());
            }
        } 
    }
}
