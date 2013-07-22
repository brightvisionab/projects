using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Linq;

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
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using BrightVision.Model;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ManagerApplication.Business;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules {
    public partial class TemplateDialog : DevExpress.XtraEditors.XtraUserControl {

        #region Variables
        private BrightPlatformEntities BPContext = null;
        private PopupDialog popupDialog = null;
        private PromptDialog promptDialog1 = null;    
        #endregion   

        public ManageDialogs ParentController { get; set; }

        #region Constructor
        public TemplateDialog() {
            this.Visible = false;
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);            
            BindLookupEdit();
            this.Visible = true;
        } 
        #endregion

        #region Methods
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
            BindGridDialogs(Convert.ToInt32(this.lookUpEditSubcampaigns.EditValue));
        }

        private void BindGridDialogs(int subcampaignid) {
            int? id = null;
            if (subcampaignid > 0)
                id = subcampaignid;
            //var dialogQuery = BPContext.FIGetDialogs();
            gridControlDialog.DataSource = BPContext.FIGetDialogs().ToList();
            gridViewDialog.BestFitColumns();
        }

        private void simpleButtonCloneDialog_Click(object sender, EventArgs e) {
            GridView view = gridViewDialog;
            if (view == null || view.SelectedRowsCount <= 0) {
                MessageBox.Show("Please select a dialog to clone first.","Clone Dialog",  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            WaitDialog.Show(ParentForm, "Loading components...");
            popupDialog = new PopupDialog();
            popupDialog.MaximizeBox = false;
            popupDialog.MinimizeBox = false;
            popupDialog.StartPosition = FormStartPosition.CenterScreen;
            popupDialog.Text = "Enter New Dialog Details";
            
            var ctscd = view.GetRow(view.GetSelectedRows()[0]) as CTSubCampaignDialogs;
            dialog objDiag = BPContext.dialogs.FirstOrDefault(p => p.id == ctscd.DialogID);
            promptDialog1 = new PromptDialog(objDiag);
            promptDialog1.ParentController = ParentController;
            popupDialog.ClientSize = new Size(promptDialog1.Width + 10, promptDialog1.Height + 10);
            promptDialog1.Dock = DockStyle.Fill;
            popupDialog.Controls.Add(promptDialog1);
            WaitDialog.Close();
            popupDialog.ShowDialog(this.ParentForm);
           
        }

        private void simpleButtonCancel_Click(object sender, EventArgs e) {
            if (ParentForm != null) {
                ParentForm.Close();
            }
        }

        private void gridViewDialog_DoubleClick(object sender, EventArgs e) {
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                view.FocusedRowHandle = hitInfo.RowHandle;
                simpleButtonCloneDialog_Click(null, null);
            }
        }

        private void gridViewDialog_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion
    }
}
