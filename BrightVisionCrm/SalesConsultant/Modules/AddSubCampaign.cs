using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using DevExpress.XtraEditors;

using System.Data.Objects;
using System.Data.Objects.DataClasses;
using BrightVision.Model;
using SalesConsultant.Business;
using BrightVision.Common.Business;
using DevExpress.XtraEditors.Controls;

namespace SalesConsultant.Modules
{
    public partial class AddSubCampaign : XtraUserControl
    {
        #region Enumerations
        public enum SaveType {
            SaveTypeAdd,
            SaveTypeEdit
        }
        #endregion

        #region Public Members
        public ManageSubCampaign m_objParentControl = null;
        public int intSubCampaignId = 0;
        public int intParentCampaignId = 0;
        public int intCustomerId = 0;
        public int intOwnerId = 0;
        public int intDialogId = 0;
        public int intListId = 0;
        public bool isNew = true;
        #endregion

        #region Private Members
        private ObjectSubCampaign.SubCampaignInstance m_objSubCampaign = null;
        private SaveType m_eSaveType = SaveType.SaveTypeAdd;
        private string m_MessageBoxCaption = "Manager Application - Sub Campaigns";
        private bool m_DoneLoadingObjects = false;
        private bool m_DoneLoadingCustomer = false;
        #endregion

        #region Constructors
        public AddSubCampaign()
        {
            InitializeComponent();
        }
        public AddSubCampaign(SaveType eSaveType, ObjectSubCampaign.SubCampaignInstance objSubCampaign)
        {
            InitializeComponent();
            m_objSubCampaign = objSubCampaign;
            m_eSaveType = eSaveType;
        }
        #endregion

        #region Control Events
        private void AddSubCampaign1_Load(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Loading components...");
            m_DoneLoadingObjects = false;
            this.LoadComboData();

            if (m_eSaveType == SaveType.SaveTypeEdit)
            {
                this.InitControls();
                this.DisplaySubCampaignInformation();
            }
            else
                this.PopulateCampaigns((int)cboCustomer.EditValue);
            
            m_DoneLoadingObjects = true;
           WaitDialog.Close();
        }
        private void cmdSave_Click(object sender, EventArgs e)
        {
            this.SaveSubCampaign(isNew);
        }
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }
        private void cboCustomer_EditValueChanged(object sender, EventArgs e)
        {
            if (m_DoneLoadingCustomer)
                this.PopulateCampaigns((int)cboCustomer.EditValue);
        }
        private void dpStartDate_EditValueChanged(object sender, EventArgs e)
        {
            dpStartDate.EditValue = Convert.ToDateTime(dpStartDate.EditValue).ToString("yyy-MM-dd");
        }
        private void dpEndDate_EditValueChanged(object sender, EventArgs e)
        {
            dpEndDate.EditValue = Convert.ToDateTime(dpEndDate.EditValue).ToString("yyy-MM-dd");
        }
        #endregion

        #region Private Methods
        private bool ValidateEntries()
        {
            if (txtName.Text.Trim().Count() < 1)
            {
                MessageBox.Show("Please enter a sub-campaign name.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtName.Focus();
                return false;
            }
            else if (Convert.ToInt32(cboCustomer.EditValue) < 1)
            {
                MessageBox.Show("Please enter a customer.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cboCustomer.Focus();
                return false;
            }
            else if (Convert.ToInt32(cboCampaign.EditValue) < 1)
            {
                MessageBox.Show("Please enter a campaign.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cboCampaign.Focus();
                return false;
            }

            return true;
        }
        private void SaveSubCampaign(bool IsNew)
        {
            if (!this.ValidateEntries())
                return;

            WaitDialog.Show(ParentForm, "Saving data...");
            ObjectSubCampaign.SubCampaignInstance objParams = new ObjectSubCampaign.SubCampaignInstance();

            if (m_objSubCampaign != null)
                objParams.id = m_objSubCampaign.id;

            objParams.sub_campaign_name = txtName.Text;
            objParams.campaign_id = (int)cboCampaign.EditValue;
            objParams.customer_id = (int)cboCustomer.EditValue;
            objParams.owner_id = (int)cboOwner.EditValue;
            objParams.list_id = 0; //todo: implement this later if finalized -> objParams.list_id = (int) cboList.SelectedValue;
            objParams.start_date = Convert.ToDateTime(dpStartDate.EditValue);
            objParams.end_date = Convert.ToDateTime(dpEndDate.EditValue);
            objParams.sub_campaign_priority = Convert.ToInt32(cboPriority.Text);
            objParams.sub_campaign_status = cboStatus.Text;
            objParams.description = txtDescription.Text;
            m_objParentControl.SelectedSubCampaignId = ObjectSubCampaign.SaveSubCampaign(IsNew, objParams);
            m_objParentControl.PopulateSubCampaignView();

            WaitDialog.Close();
            MessageBox.Show("Successfully updated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.ParentForm.Close();
        }
        private void DisplaySubCampaignInformation()
        {
            subcampaign objSubCampaign;
            campaign objCampaign;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objSubCampaign = _efDbContext.subcampaigns.Where(i => i.id == m_objSubCampaign.id).SingleOrDefault();
                objCampaign = _efDbContext.campaigns.Where(i => i.id == objSubCampaign.campaign_id).SingleOrDefault();
                _efDbContext.Detach(objSubCampaign);
                _efDbContext.Detach(objCampaign);
            }
            txtName.Text = objSubCampaign.title;
            cboCustomer.EditValue = objCampaign.customer_id;
            this.PopulateCampaigns(objCampaign.customer_id);
            cboCampaign.EditValue = objSubCampaign.campaign_id;
            cboOwner.EditValue = objSubCampaign.campaign_manager_user_id;
            dpStartDate.EditValue = Convert.ToDateTime(objSubCampaign.start_date).ToString("yyyy-MM-dd");
            dpEndDate.EditValue = Convert.ToDateTime(objSubCampaign.end_date).ToString("yyyy-MM-dd");
            cboPriority.Text = objSubCampaign.priority.ToString();
            cboStatus.Text = objSubCampaign.status;
            txtDescription.Text = objSubCampaign.description;
        }
        private void InitControls()
        {
            txtName.Text = "";
            dpStartDate.EditValue = DateTime.Now;
            dpEndDate.EditValue = DateTime.Now;
            cboPriority.Text = "1";
            cboStatus.Text = "Active";
            txtDescription.Text = "";
        }
        private void PopulateOwners()
        {
            cboOwner.Properties.DataSource = null;
            cboOwner.Properties.DataSource = ObjectUser.GetOwnerUsers();
            cboOwner.Properties.ValueMember = "id";
            cboOwner.Properties.DisplayMember = "owner_name";
            cboOwner.Properties.Columns.Add(new LookUpColumnInfo("owner_name"));
            cboOwner.ItemIndex = 0;
        }
        private void PopulateCustomers()
        {
            m_DoneLoadingCustomer = false;
            cboCustomer.Properties.DataSource = null;
            cboCustomer.Properties.DataSource = ObjectCustomer.GetCustomers(ObjectCustomer.eViewType.SubCampaignView);
            cboCustomer.Properties.ValueMember = "id";
            cboCustomer.Properties.DisplayMember = "name";
            cboCustomer.Properties.Columns.Add(new LookUpColumnInfo("name"));
            cboCustomer.ItemIndex = 0;
            m_DoneLoadingCustomer = true;
        }
        private void PopulateCampaigns(int CustomerId)
        {
            if (m_DoneLoadingObjects)
                WaitDialog.Show(ParentForm, "Loading components...");

            cboCampaign.Properties.Columns.Clear();
            cboCampaign.Properties.DataSource = null;
            cboCampaign.Properties.DataSource = ObjectCampaign.GetCampaigns(ObjectCampaign.eViewtype.ComboListViewByCustomerId, CustomerId);
            cboCampaign.Properties.ValueMember = "id";
            cboCampaign.Properties.DisplayMember = "name";
            cboCampaign.Properties.Columns.Add(new LookUpColumnInfo("name"));
            cboCampaign.ItemIndex = 0;

            if (m_DoneLoadingObjects)
               WaitDialog.Close();
        }
        private void LoadComboData()
        {
            this.PopulateCustomers();
            this.PopulateOwners();
        }
        #endregion
    }
}
