
#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using BrightVision.Model;
using SalesConsultant.Business;
using BrightVision.Common.Business;
using DevExpress.XtraEditors.Controls;
using System.Collections;

#endregion

namespace SalesConsultant.Modules
{
    public partial class AddInternalUser : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Members
        public List<int> UsersNotIncluded { get; set; }
        #endregion

        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private string m_MessageBoxCaption = "Manager Application - Sub Campaign";
        private CTSubCampaignInternalUsers m_objUser = null;
        private int m_SubCampaignId = 0;
        #endregion

        #region Constructors
        public AddInternalUser()
        {
        }
        public AddInternalUser(int SubCampaignId)
        {
            InitializeComponent();
            //m_objUser = objUser;
            m_SubCampaignId = SubCampaignId;
        }
        #endregion

        #region Public Events
        public delegate void AfterSaveEventHandler(List<int> pSubCampaignUserId);
        public event AfterSaveEventHandler AfterSave;
        #endregion

        #region Control Events
        private void AddInternalUser_Load(object sender, EventArgs e)
        {
            this.PopulateInternalUser();
        }
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }
        private void cmdSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }
        #endregion

        #region Private Functions
        private void PopulateInternalUser()
        {
            cboUser.Properties.DataSource = null;
            //cboUser.Properties.Columns.Clear();
            cboUser.Properties.DataSource = GetInternalUsers();
            cboUser.Properties.ValueMember = "id";
            cboUser.Properties.DisplayMember = "name";
             
            //cboUser.Properties.Columns.Add(new LookUpColumnInfo("name"));
            //cboUser.ItemIndex = 0;
        }
        private ObjectResult GetInternalUsers()
        {
            if (UsersNotIncluded != null && UsersNotIncluded.Count > 0)
            {
                var UnFilteredUsers = ObjectUser.GetInternalUsers(UsersNotIncluded).Execute(MergeOption.AppendOnly);
                return UnFilteredUsers;
            }
            else {
                var UnFilteredUsers = ObjectUser.GetInternalUsers().Execute(MergeOption.AppendOnly);
                return UnFilteredUsers;
            }
            
        }
        private void Save()
        {
            List<int> selectedUsers = GetSelectedUsers();
            if (selectedUsers.Count == 0) {
                this.ParentForm.Close();
                return;
            }

            /**
             * save with initial role as sales consultant.
             * 3 - SalesConsultant
             */
            WaitDialog.Show(this.ParentForm, "Saving....");
            foreach (int userid in selectedUsers){
                if (!ObjectSubCampaign.ValidateSubCampaignUserExists(m_SubCampaignId, userid, true)) {
                    int _SubCampaignUserId = ObjectSubCampaign.SaveSubCampaignInternalUser(m_SubCampaignId, userid);
                    ObjectSubCampaign.SaveSubCampaignRole(_SubCampaignUserId, 3);
                }
            }           

            //m_objParentControl.PopulateSubCampaignInternalUserView(m_SubCampaignId);
            if (AfterSave != null)
                AfterSave(selectedUsers);

            WaitDialog.Close(true);
            this.ParentForm.Close();
            MessageBox.Show("Successfully updated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
           
        }
        public List<int> GetSelectedUsers() {
            List<int> useridList = new List<int>();
            string[] useridArrayString = cboUser.Properties.GetCheckedItems().ToString().Split(',');
            foreach (string userid in useridArrayString) {
                if (string.IsNullOrEmpty(userid))
                    continue;

                useridList.Add(int.Parse(userid));
            }
            return useridList;
        }
        #endregion
    }
}
