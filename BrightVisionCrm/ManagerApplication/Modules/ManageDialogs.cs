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
    public partial class ManageDialogs : DevExpress.XtraEditors.XtraUserControl
    {
        #region Properties
        public int DefaultSelectedDialogId = 0;
        private GridCheckMarksSelection gridSelection = null;
        #endregion

        #region Member Variables
        private BrightPlatformEntities BPContext = null;
        private NewDialog ucNewDialog1;
        private TemplateDialog ucTemplateDialog1;
        private PopupDialog popupDialog = null;
        private PopupDialog popupDialogTemplate = null;
        //private int lastSelectedIndex = 0;
        private int m_DefaultSelectedRow = 0;
        private bool m_FetchingData = false;
        #endregion

        #region Constructor
        public ManageDialogs() {
            this.Visible = false;
            InitializeComponent();
            gridSelection = new GridCheckMarksSelection(gridViewDialog);
            gridViewDialog.Columns["Customer"].VisibleIndex = 1;
            this.layoutControl1.AllowCustomizationMenu = false;
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);            
            BindGridDialogs();
            this.SetDialogViewContextMenu();
            this.Visible = true;
        } 
        #endregion
        
        #region Private Methods
        private void SetDefaultSelectedRow()
        {
            if (DefaultSelectedDialogId < 1)
            {
                if (gridViewDialog.RowCount > 0)
                    gridViewDialog.FocusedRowHandle = m_DefaultSelectedRow;
                else
                    m_DefaultSelectedRow = 0;
            }
            else
            {
                for (int i = 0; i < gridViewDialog.RowCount; i++)
                {
                    if (Convert.ToInt32(gridViewDialog.GetRowCellValue(i, "DialogID")) == DefaultSelectedDialogId)
                    {
                        gridViewDialog.FocusedRowHandle = i;
                        return;
                    }
                }

                DefaultSelectedDialogId = 0;
                gridViewDialog.FocusedRowHandle = 0;
            }
        }

        private void SetDialogViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miDialogPrintPreview = new MenuItem("Print Preview");
            miDialogPrintPreview.Click += new EventHandler(miDialogPrintPreview_Click);
            objClickMenu.MenuItems.Add(miDialogPrintPreview);
            gridControlDialog.ContextMenu = objClickMenu;
        }

        private void BindGridDialogs()
        {
            m_FetchingData = true;
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            BPContext.CommandTimeout = 0;
            var dialogQuery = BPContext.FIGetDialogs();
            gridControlDialog.DataSource = dialogQuery;
            m_FetchingData = false;
            gridSelection.ClearSelection();
            this.SetDefaultSelectedRow();
        }
        #endregion

        #region Object Events
        private void simpleButtonAdd_Click(object sender, EventArgs e) {
            WaitDialog.Show(ParentForm, "Loading components...");
            popupDialog = new PopupDialog();
            Padding pads = popupDialog.Padding;
            pads.Left = 5;            
            popupDialog.StartPosition = FormStartPosition.CenterScreen;
            popupDialog.Text = "Add Dialog";
            ucNewDialog1 = new NewDialog();
            ucNewDialog1.ParentController = this;
            ucNewDialog1.Dock = DockStyle.Fill;
            popupDialog.Controls.Add(ucNewDialog1);
            popupDialog.ClientSize = new Size(1460, 930);
            popupDialog.FormClosing += new FormClosingEventHandler(popupDialog_FormClosing);
            WaitDialog.Close();
            popupDialog.ShowDialog(this.ParentForm);           
        }

        private void simpleButtonEdit_Click(object sender, EventArgs e) 
        {
            if (gridViewDialog.RowCount < 1)
                return;

            WaitDialog.Show(ParentForm, "Loading components...");
            popupDialog = new PopupDialog();
            Padding pads = popupDialog.Padding;
            pads.Left = 5;
            popupDialog.StartPosition = FormStartPosition.CenterScreen;
            popupDialog.Text = "Edit Dialog";

            GridView view = gridViewDialog;
            if (view != null || view.SelectedRowsCount > 0) {
                var ctscd = view.GetRow(view.GetSelectedRows()[0]) as CTSubCampaignDialogs;                
                ucNewDialog1 = new NewDialog(ctscd);
                ucNewDialog1.ParentController = this;
            }

            ucNewDialog1.Dock = DockStyle.Fill;
            popupDialog.Controls.Add(ucNewDialog1);
            popupDialog.ClientSize = new Size(1460, 930);
            popupDialog.FormClosing += new FormClosingEventHandler(popupDialog_FormClosing);
            WaitDialog.Close();
            popupDialog.ShowDialog(this.ParentForm);           
        }

        private void popupDialog_FormClosing(object sender, FormClosingEventArgs e) {
            BindGridDialogs();
        }
        
        private void gridViewDialog_DoubleClick(object sender, EventArgs e) {
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                view.FocusedRowHandle = hitInfo.RowHandle;
                simpleButtonEdit_Click(null, null);
            }
        }
        private void gridViewDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            if (gridViewDialog.RowCount > 0)
                simpleButtonEdit_Click(null, null);
        }

        private void gridViewDialog_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e) {
            //if (e.FocusedRowHandle == 0)
            //    lastSelectedIndex = e.PrevFocusedRowHandle;
            //else
            //    lastSelectedIndex = e.FocusedRowHandle;

            if (!m_FetchingData)
            {
                m_DefaultSelectedRow = 0;
                if (gridViewDialog.RowCount > 0)
                    m_DefaultSelectedRow = e.FocusedRowHandle;
            }
        }

        private void simpleButtonDelete_Click(object sender, EventArgs e) {
            int selCount = gridSelection.SelectedCount;
            if (selCount < 1) return;

            if (MessageBox.Show("Are you sure you want to delete the selected dialog(s)?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            
            var rows = gridSelection.GetSelectedRows;
            CTSubCampaignDialogs row = null;
            List<string> ids = new List<string>();
            for (int x = 0; x < selCount; ++x) {
                row = rows[x] as CTSubCampaignDialogs;
                ids.Add(row.DialogID.ToString());
            }
            BPContext.FIUpdateDialog(string.Join(",", ids.ToArray()), false);
            WaitDialog.Show(ParentForm, "Reloading dialogs...");
            gridControlDialog.DataSource = null;
            BindGridDialogs();
            gridSelection.ClearSelection();
            WaitDialog.Close();
        }

        private void simpleButtonCloneDialog_Click(object sender, EventArgs e) {
            WaitDialog.Show(ParentForm, "Loading components...");
            popupDialogTemplate = new PopupDialog();
            popupDialogTemplate.MaximizeBox = false;
            popupDialogTemplate.MinimizeBox = false;
            popupDialogTemplate.StartPosition = FormStartPosition.CenterScreen;
            popupDialogTemplate.Text = "Clone Dialog From All Dialog Templates";
            ucTemplateDialog1 = new TemplateDialog();
            ucTemplateDialog1.Dock = DockStyle.Fill;
            ucTemplateDialog1.ParentController = this;
            popupDialogTemplate.Controls.Add(ucTemplateDialog1);
            popupDialogTemplate.ClientSize = new Size(ucTemplateDialog1.Width + 10, ucTemplateDialog1.Height + 10);
            popupDialogTemplate.FormClosed += new FormClosedEventHandler(popupDialogTemplate_FormClosed);
            WaitDialog.Close();
            popupDialogTemplate.ShowDialog(this.ParentForm);            
        }

        private void popupDialogTemplate_FormClosed(object sender, FormClosedEventArgs e) {
            var pf = sender as Form;
            if (pf.DialogResult == DialogResult.OK)
                BindGridDialogs();
        }

        private void gridViewDialog_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Delete) {
                simpleButtonDelete_Click(null, null);
            }
        }

        private void miDialogPrintPreview_Click(object sender, EventArgs e)
        {
            gridControlDialog.ShowPrintPreview();
        }

        private void gridViewDialog_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion
    }
}
