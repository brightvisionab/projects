using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ManagerApplication.Forms;

namespace ManagerApplication {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new DemoWindow());
            try
            {
                //check for updates
                FrmAppUpdater updater = new FrmAppUpdater();
                if (updater.HasNewUpdate)
                {
                    updater.ShowDialog();
                }
                else
                {
                    Application.Run(new FrmManagerApplication());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source + (ex.StackTrace != null ? ex.StackTrace : ""), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }
}
