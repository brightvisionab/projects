
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;

using BrightVision.Model;
using BrightVision.Common.Business;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules
{
    public partial class ManageFollowUp : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public ManageFollowUp()
        {
            InitializeComponent();
            cboCampaign.Properties.DataSource = null;
            cboCampaign.Properties.DataSource = efDbModel.FIGetCampaignSelections().ToList();
            cboCampaign.Properties.DisplayMember = "selection";
            cboCampaign.Properties.ValueMember = "campaign_id";
            cboCampaign.Properties.Columns.Add(new LookUpColumnInfo("selection"));
        }
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        BrightPlatformEntities efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void ApplyFilter()
        {
            if (cbxFilterDone.Checked)
                gvFollowUp.Columns["done"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo("[done] = 'False'");
            else
                gvFollowUp.Columns["done"].ClearFilter();

            lblRows.Text = gvFollowUp.RowCount.ToString() + " Rows";
        }
        private void LoadData()
        {
            try
            {
                gcFollowUp.DataSource = null;
                List<CTCampaignFollowUp> _lstItems = efDbModel.FIGetCampaignFollowUps(Convert.ToInt32(cboCampaign.EditValue)).ToList();
                if (_lstItems.Count > 0)
                {
                    gcFollowUp.DataSource = _lstItems;
                    gvFollowUp.FocusedRowHandle = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
                    this.ApplyFilter();
                }
            }
            catch { }
        }
        #endregion

        #region Object Events
        private void cboCampaign_EditValueChanged(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading sub-campaigns...");
            this.LoadData();
            WaitDialog.Close();
        }
        private void gvFollowUp_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading sub-campaigns...");
            this.LoadData();
            WaitDialog.Close();
        }
        private void cbxDone_CheckedChanged(object sender, EventArgs e)
        {
            WaitDialog.Show("Updating data...");
            CheckEdit _cbxItem = sender as CheckEdit;
            CTCampaignFollowUp _item = gvFollowUp.GetRow(gvFollowUp.FocusedRowHandle) as CTCampaignFollowUp;
            event_followup_log _efeFollowUp = efDbModel.event_followup_log.FirstOrDefault(i => i.id == _item.id);
            _efeFollowUp.done = _cbxItem.Checked ? true : false;
            efDbModel.event_followup_log.ApplyCurrentValues(_efeFollowUp);
            efDbModel.SaveChanges();
            this.btnRefresh_Click(null, null);
            WaitDialog.Close();
        }
        private void cbxDone_EditValueChanging(object sender, ChangingEventArgs e)
        {
            DialogResult _dlg = MessageBox.Show("Are you sure to update this entry?", "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlg == DialogResult.No)
                e.Cancel = true;
        }
        private void cbxFilterDone_CheckedChanged(object sender, EventArgs e)
        {
            WaitDialog.Show("Filtering data...");
            this.ApplyFilter();
            WaitDialog.Close();
        }
        private void gvFollowUp_ColumnFilterChanged(object sender, EventArgs e)
        {
            lblRows.Text = gvFollowUp.RowCount.ToString() + " Rows";
            gvFollowUp.BestFitColumns();
        }
        private void gvFollowUp_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            lblRows.Text = gvFollowUp.RowCount.ToString() + " Rows";
            gvFollowUp.BestFitColumns();
        }
        private void gvFollowUp_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.Equals("date_of_transaction"))
            {
                e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyyy-MM-dd");
            }
            if (e.Column.FieldName.Equals("start_time") || e.Column.FieldName.Equals("end_time"))
            {
                var time = ((TimeSpan?)e.Value).Value.ToString();
                e.DisplayText = time.Substring(0, 5);
            }
        }
        private void btnReAssignFollowUp_Click(object sender, EventArgs e)
        {
            if (gvFollowUp.RowCount < 1)
                return;

            var _item = gvFollowUp.GetFocusedRow() as CTCampaignFollowUp;
            if (_item == null)
                return;

            ReAssignFollowUp _control = new ReAssignFollowUp(_item.id);
            _control.AfterSave += new ReAssignFollowUp.AfterSaveEventHandler(_control_AfterSave);
            PopupDialog _popup = new PopupDialog();
            _popup.FormBorderStyle = FormBorderStyle.FixedSingle;
            _popup.MinimizeBox = false;
            _popup.MaximizeBox = false;
            _popup.StartPosition = FormStartPosition.CenterScreen;
            _popup.Text = "Re-assign follow-ups";
            _popup.Controls.Add(_control);
            _popup.ClientSize = new Size(_control.Width + 2, _control.Height + 2);
            _popup.ShowDialog(this.ParentForm);
        }
        #endregion

        #region Subscribed Events
        private void _control_AfterSave(ReAssignFollowUp.AfterSaveArgs e)
        {
            gvFollowUp.SetRowCellValue(gvFollowUp.FocusedRowHandle, "assigned_user", e.UserName);
        }
        #endregion
    }
}
