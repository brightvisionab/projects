using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraVerticalGrid.Rows;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraLayout;

using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using QuestionnaireEditor.Business;

namespace QuestionnaireEditor.Modules {
    public partial class ManageDialogs : DevExpress.XtraEditors.XtraUserControl {
        private BrightPlatformEntities BPContext = null;
        private NewDialog ucNewDialog1;
        private PopupDialog popupDialog = null;

        public ManageDialogs() {
            this.Visible = false;
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            WaitDialog.SetWaitDialogCaption("Loading subcampaigns...");
            BindLookupEdit();     
            this.Visible = true;
            WaitDialog.CloseWaitDialog();
        }
        private void BindLookupEdit() {
            var datasource = BPContext.subcampaigns.Execute(MergeOption.AppendOnly).ToList();
            datasource.Insert(0, new subcampaign() { id = 0, title = "All" });
            this.lookUpEditSubcampaigns.Properties.DataSource = datasource;
            this.lookUpEditSubcampaigns.Properties.DisplayMember = "title";
            this.lookUpEditSubcampaigns.Properties.ValueMember = "id";
            this.lookUpEditSubcampaigns.EditValue = 0;
            this.lookUpEditSubcampaigns.Properties.Columns.Add(new LookUpColumnInfo("title", "Sub campaign name"));
            this.lookUpEditSubcampaigns.Properties.ShowHeader = false;
            this.lookUpEditSubcampaigns.Properties.ValidateOnEnterKey = true;
            WaitDialog.SetWaitDialogCaption("Loading dialogs...");
            BindGridDialogs(Convert.ToInt32(this.lookUpEditSubcampaigns.EditValue));
        }
        private void BindGridDialogs(int subcampaignid) {
            int? id = null;
            if(subcampaignid > 0) {                
                id = subcampaignid;         
            }
            var dialogQuery = BPContext.FIGetDialogs();
            gridControlDialog.DataSource = dialogQuery;     
        }

        private void simpleButtonAdd_Click(object sender, EventArgs e) {
            WaitDialog.CreateWaitDialog("Loading components...");
            popupDialog = new PopupDialog();
            Padding pads = popupDialog.Padding;
            pads.Left = 5;
            popupDialog.WindowState = FormWindowState.Maximized;
            popupDialog.StartPosition = FormStartPosition.CenterScreen;
            popupDialog.Text = "Add Dialog";
            ucNewDialog1 = new NewDialog();
            ucNewDialog1.Dock = DockStyle.Fill;
            popupDialog.Controls.Add(ucNewDialog1);
            popupDialog.ClientSize = new Size(ucNewDialog1.Width + 10, ucNewDialog1.Height + 10);
            popupDialog.ShowDialog(this.ParentForm);
            WaitDialog.CloseWaitDialog();
        }

        private void simpleButtonEdit_Click(object sender, EventArgs e) {
            WaitDialog.CreateWaitDialog("Loading components...");
            popupDialog = new PopupDialog();
            Padding pads = popupDialog.Padding;
            pads.Left = 5;
            popupDialog.WindowState = FormWindowState.Maximized;
            popupDialog.StartPosition = FormStartPosition.CenterScreen;
            popupDialog.Text = "Edit Question";
            
            GridView view = gridViewDialog;
            if (view != null || view.SelectedRowsCount > 0) {      
                var ctscd = view.GetRow(view.GetSelectedRows()[0]) as CTSubCampaignDialogs;
                ucNewDialog1 = new NewDialog(ctscd);
            }

            ucNewDialog1.Dock = DockStyle.Fill;
            popupDialog.Controls.Add(ucNewDialog1);
            popupDialog.ClientSize = new Size(ucNewDialog1.Width + 10, ucNewDialog1.Height + 10);
            popupDialog.ShowDialog(this.ParentForm);
            popupDialog.FormClosing += new FormClosingEventHandler(popupDialog_FormClosing);
            WaitDialog.CloseWaitDialog();
        }

        void popupDialog_FormClosing(object sender, FormClosingEventArgs e) {
            WaitDialog.CreateWaitDialog("Reloading questions...");
            //var objSource = BPContext.FIQuestionTags(null);
            //gridControlDialog.DataSource = objSource;
            WaitDialog.CloseWaitDialog();
        }

        private void lookUpEditSubcampaigns_EditValueChanged(object sender, EventArgs e) {
            BindGridDialogs(Convert.ToInt32(lookUpEditSubcampaigns.EditValue));
        }
    }
}
