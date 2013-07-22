using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraLayout;

using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;
using BrightVision.Model;
using BrightVision.Common.Business;
using QuestionnaireEditor.Business;

namespace QuestionnaireEditor.Forms {
    public partial class FrmEditor : DevExpress.XtraEditors.XtraForm {
        private Modules.ManageQuestions ucManageQuestions1;
        private Modules.ExportDesign ucExportDesign1;
        private Modules.ManageDialogs ucManageDialog1;
        public FrmEditor() {            
            InitializeComponent();            
            //ucNewQuestion1.BackgroundWorker = new Business.ProgressBackgroundWork(barEditItemProgress, barStaticItemStatus);
        }

        protected override void OnLoad(EventArgs e) {            
            WaitDialog.CreateWaitDialog("Loading components...");
            ucManageQuestions1 = new Modules.ManageQuestions();
            ucManageQuestions1.Dock = DockStyle.Fill;
            panelControl1.Controls.Add(ucManageQuestions1);
            base.OnLoad(e);
            WaitDialog.CloseWaitDialog();
        }

        private void navBarItemNewQuestion_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e) {
            WaitDialog.CreateWaitDialog("Loading components...");
            ucManageQuestions1 = new Modules.ManageQuestions();
            ucManageQuestions1.Dock = DockStyle.Fill;
            panelControl1.Controls.Clear();
            panelControl1.Controls.Add(ucManageQuestions1);
            WaitDialog.CloseWaitDialog();
        }

        private void navBarItemExportDesign_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e) {
            WaitDialog.CreateWaitDialog("Loading components...");
            ucExportDesign1 = new Modules.ExportDesign();
            ucExportDesign1.Dock = DockStyle.Fill;
            panelControl1.Controls.Clear();
            panelControl1.Controls.Add(ucExportDesign1);
            WaitDialog.CloseWaitDialog();
        }

        private void navBarItemMangeDialog_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e) {
            WaitDialog.CreateWaitDialog("Loading components...");
            ucManageDialog1 = new Modules.ManageDialogs();
            ucManageDialog1.Dock = DockStyle.Fill;
            panelControl1.Controls.Clear();
            panelControl1.Controls.Add(ucManageDialog1);
            WaitDialog.CloseWaitDialog();
        }

    }
}
