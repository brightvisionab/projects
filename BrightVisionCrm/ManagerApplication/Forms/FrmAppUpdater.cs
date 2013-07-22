using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
using BrightVision.Common.Business;
using BrightVision.Model;
namespace ManagerApplication.Forms {
    public partial class FrmAppUpdater : Form {

        public FrmAppUpdater() {
            InitializeComponent();
            CheckUpdates();
        }

        public bool HasNewUpdate { get; set; }

        private void CheckUpdates() {
            BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            Version newVersion = null;
            //string url = "";
            var appversion = BPContext.app_version.OrderByDescending(x => x.id).FirstOrDefault();
            if (appversion == null) return;

            string xmlString = appversion.config;
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmlString);
            var node = doc.SelectSingleNode("//manager_app");
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
    }
}
