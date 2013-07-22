
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
    public partial class AddEmailVerifyContact : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public AddEmailVerifyContact()
        {
            InitializeComponent();
            cboSubCampaign.Properties.DataSource = null;
            cboSubCampaign.Properties.DataSource = efDbModel.FIGetSubCampaignSelections().ToList();
            cboSubCampaign.Properties.DisplayMember = "selection";
            cboSubCampaign.Properties.ValueMember = "id";
            cboSubCampaign.Properties.Columns.Add(new LookUpColumnInfo("selection"));
            cboSubCampaign.ItemIndex = 0;
        }
        #endregion

        #region Public Event Handlers
        public delegate void btnAddToQueueOnClickEventHandler(object sender, ContactEmailArgs e);
        public event btnAddToQueueOnClickEventHandler btnAddToQueue_OnClick;
        #endregion

        #region Public Event Arguments
        public class ContactEmailArgs : EventArgs
        {
            public List<CTEmailVerifyContact> lstEmailVerifyContacts;
        }
        #endregion

        #region Subscribed Events
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        BrightPlatformEntities efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        #region Object Events
        private void btnLoadContactsByKeyword_Click(object sender, EventArgs e)
        {
            if (tbxContactSearch.Text.Length < 1)
                return;

            WaitDialog.Show("Loading contacts...");
            try
            {
                gcContact.DataSource = null;
                gcContact.DataSource = efDbModel.FIGetContactSelectionForEmailVerification(null, tbxContactSearch.Text).ToList();
                gvContact.BestFitColumns();
                lblRows.Text = gvContact.RowCount.ToString() +  " Rows";
            }
            catch { }
            WaitDialog.Close();
        }
        private void btnLoadContactsBySubCampaign_Click(object sender, EventArgs e)
        {
            if (cboSubCampaign.EditValue == null)
                return;

            WaitDialog.Show("Loading contacts...");
            try
            {
                gcContact.DataSource = null;
                gcContact.DataSource = efDbModel.FIGetContactSelectionForEmailVerification(Convert.ToInt32(cboSubCampaign.EditValue), null).ToList();
                gvContact.BestFitColumns();
                lblRows.Text = gvContact.RowCount.ToString() + " Rows";
            }
            catch { }
            WaitDialog.Close();
        }
        private void gvContact_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }
        private void btnAddToQueue_Click(object sender, EventArgs e)
        {
            if (gvContact.RowCount < 1)
                return;

            /**
             * get the list of contacts to be added to verification queue
             */
            WaitDialog.Show("Queueing contacts...");
            List<CTEmailVerifyContact> _lstEmailVerifyContacts = new List<CTEmailVerifyContact>();
            for (int i = 0; i < gvContact.RowCount; i++)
                if (Convert.ToBoolean(gvContact.GetRowCellValue(i, "selected")) && gvContact.GetRowCellValue(i, "email").ToString().Length > 0)
                    _lstEmailVerifyContacts.Add(gvContact.GetRow(i) as CTEmailVerifyContact);

            if (_lstEmailVerifyContacts.Count < 1)
            {
                MessageBox.Show("No selected items.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (btnAddToQueue_OnClick != null)
            {
                ContactEmailArgs _args = new ContactEmailArgs();
                _args.lstEmailVerifyContacts = _lstEmailVerifyContacts;
                btnAddToQueue_OnClick(this, _args);
            }
            WaitDialog.Close();
            MessageBox.Show("Items added. Please close this form if you have no more contacts to queue.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void cbxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (gvContact.RowCount < 1)
                return;

            WaitDialog.Show("Updating list...");
            for (int i = 0; i < gvContact.RowCount; i++)
                gvContact.SetRowCellValue(i, "selected", cbxSelectAll.Checked);
            WaitDialog.Close();
        }
        #endregion
    }
}
