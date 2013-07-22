using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;

namespace BrightVision.AppUpdater
{
    public partial class FormAppUpdater : Form
    {
        private delegate void SetTextCallBack(string text);

        public FormAppUpdater()
        {
            InitializeComponent();
        }

        private void FormAppUpdater_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            Thread t = new Thread(StartMovingFiles);
            t.Start();
        }

        private void StartMovingFiles()
        {
            
            string tempPath = System.IO.Path.GetTempPath() + @"\Brightvision";
            string tempBackupFolder = System.IO.Path.GetTempPath() + @"\Brightvision\Brightvision";

            if (Directory.Exists(tempPath))
            {
                if (Directory.Exists(tempBackupFolder)) Directory.Delete(tempBackupFolder, true);
                Directory.CreateDirectory(tempBackupFolder + @"\Debug");


                //copy all files to temp folder for backup incase an error encountered
                //so that it can still back the old files
                lblDescription.Text = "Making backup for currently installed application";
                Application.DoEvents();
                Thread.Sleep(3000);
                foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
                    File.Copy(file, Path.Combine(tempBackupFolder + @"\Debug", Path.GetFileName(file)), true);

                
                lblDescription.Text = "Starting to update application";
                Application.DoEvents();
                Thread.Sleep(3000);
                //update the file from the temp folder that has the updated files
                foreach (var file in Directory.GetFiles(tempPath + @"\Debug"))
                {
                    lblDescription.Text = "Updating " + Path.GetFileName(file);
                    Application.DoEvents();
                    try
                    {
                        File.Copy(file, Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(file)), true);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                lblDescription.Text = "Done updating";

                if (MessageBox.Show("Done updating. Restart the BS Application", "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("SalesConsultant.exe");
                }

                Application.Exit();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(StartMovingFiles);
            t.Start();
        }


    }
}
