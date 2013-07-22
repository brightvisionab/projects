using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using BrightVision.Model;
using ManagerApplication.Business;
using BrightVision.Common.Business;
using DevExpress.XtraEditors.Controls;
using System.Linq;

namespace ManagerApplication.Modules
{
    public partial class ReAssignFollowUp : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public ReAssignFollowUp(int pId)
        {
            InitializeComponent();
            m_EventFollowUpLogId = pId;
            this.GetUsers();
        }
        public ReAssignFollowUp()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Events & Args
        public delegate void AfterSaveEventHandler(AfterSaveArgs e);
        public event AfterSaveEventHandler AfterSave;
        public class AfterSaveArgs : EventArgs {
            public int UserId { get; set; }
            public string UserName { get; set; }
        }
        #endregion

        #region Subscribed Events
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private int m_EventFollowUpLogId = 0;
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void GetUsers()
        {
            event_followup_log _item = new event_followup_log();
            List<CTSubCampaignUser> _lstItems = new List<CTSubCampaignUser>();
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _item = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == m_EventFollowUpLogId);
                _lstItems = _efDbContext.FIGetSubCampaignUsers(_item.subcampaign_id).ToList();
            }

            cboSubCampaignUsers.Properties.DataSource = null;
            cboSubCampaignUsers.Properties.Columns.Clear();
            cboSubCampaignUsers.Properties.DataSource = _lstItems;
            cboSubCampaignUsers.Properties.ValueMember = "id";
            cboSubCampaignUsers.Properties.DisplayMember = "fullname";
            cboSubCampaignUsers.Properties.Columns.Add(new LookUpColumnInfo("fullname"));
            cboSubCampaignUsers.EditValue = _item.assigned_user;
        }
        #endregion

        #region Control Events
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            //DialogResult _dlg = MessageBox.Show("Are you sure to re-assign selected follow-up?", "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (_dlg == DialogResult.No)
            //    return;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                event_followup_log _item = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == m_EventFollowUpLogId);
                _item.assigned_user = Convert.ToInt32(cboSubCampaignUsers.EditValue);
                _efDbContext.event_followup_log.ApplyCurrentValues(_item);
                _efDbContext.SaveChanges();
            }

            if (AfterSave != null)
                AfterSave(new AfterSaveArgs() {
                    UserId = Convert.ToInt32(cboSubCampaignUsers.EditValue),
                    UserName = cboSubCampaignUsers.Text
                });

            this.ParentForm.Close();
        }
        #endregion
    }
}
