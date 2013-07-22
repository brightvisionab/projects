using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraSplashScreen;

namespace ManagerApplication {
    public partial class SplashScreen1 : SplashScreen {
        public SplashScreen1() {
            InitializeComponent();
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = "v." + curVersion.ToString();
        }

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg) {
            base.ProcessCommand(cmd, arg);
        }

        #endregion

        public enum SplashScreenCommand {
        }
    }
}