using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QuestionnaireEditor {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.UserSkins.OfficeSkins.Register();
            DevExpress.Skins.SkinManager.EnableFormSkins();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.FrmMain());
            //Application.Run(new Forms.TestForm());
            //Application.Run(new Forms.FrmEditor());
        }
    }
}
