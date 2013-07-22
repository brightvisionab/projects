
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
#endregion

namespace SalesConsultant.Modules
{
    public partial class AddCustomerUser : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Members
        public ManageSubCampaign m_objParentControl = null;
        #endregion

        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private string m_MessageBoxCaption = "Manager Application - Sub Campaign";
        private CTSubCampaignCustomerUsers m_objUser = null;
        private int m_SubCampaignId = 0;
        private int m_CustomerId = 0;
        #endregion

        #region Constructors
        public AddCustomerUser(CTSubCampaignCustomerUsers objUser, int SubCampaignId, int CustomerId)
        {            
            InitializeComponent();
            m_objUser = objUser;
            m_SubCampaignId = SubCampaignId;
            m_CustomerId = CustomerId;
        }
        #endregion

        #region Object Control Events
        private void AddCustomerUser_Load(object sender, EventArgs e)
        {
            this.PopulateCustomerUser();
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Populates the customer users combo box on the sub campaign customer user form
        /// </summary>
        private void PopulateCustomerUser()
        {
            cboUser.Properties.DataSource = null;
            cboUser.Properties.Columns.Clear();
            cboUser.Properties.DataSource = ObjectUser.GetCustomerUsers(m_CustomerId).Execute(MergeOption.AppendOnly);
            cboUser.Properties.ValueMember = "id";
            cboUser.Properties.DisplayMember = "name";
            cboUser.Properties.Columns.Add(new LookUpColumnInfo("name"));
            cboUser.ItemIndex = 0;
        }

        /// <summary>
        /// Saves the subcampaign customer user
        /// </summary>
        private void Save()
        {
            if (string.IsNullOrEmpty(cboUser.Text) || Convert.ToInt32(cboUser.EditValue) < 1)
                return;

            if (ObjectSubCampaign.ValidateSubCampaignUserExists(m_SubCampaignId, (int)cboUser.EditValue, false))
            {
                MessageBox.Show("Sub-campaign customer user already exists!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            int _id = ObjectSubCampaign.SaveSubCampaignCustomerUser(m_SubCampaignId, (int)cboUser.EditValue, txtDescription.Text);
            //m_objParentControl.PopulateSubCampaignCustomerUserView(m_SubCampaignId);
            if (AfterSave != null)
                AfterSave(_id);
            //MessageBox.Show("Successfully updated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.ParentForm.Close();
        }
        #endregion

        #region Public Events
        public delegate void AfterSaveEventHandler(int pSubCampaignUserId);
        public event AfterSaveEventHandler AfterSave;
        #endregion
    }
}
