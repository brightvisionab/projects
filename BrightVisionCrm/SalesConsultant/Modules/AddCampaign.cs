
#region Namespace
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

using System.Data.Objects;
using System.Data.Objects.DataClasses;
using BrightVision.Model;
using BrightVision.Common.Business;
using SalesConsultant.Business;
using DevExpress.XtraEditors.Controls;
#endregion

namespace SalesConsultant.Modules
{
    public partial class AddCampaign : DevExpress.XtraEditors.XtraUserControl
    {
        #region Enumerations
        /// <summary>
        /// Enum for save type
        /// </summary>
        public enum SaveType
        {
            SaveTypeAdd,
            SaveTypeEdit
        }
        #endregion

        #region Public Properties
        public ManageCustomerCampaign m_objParentControl { get; set; }
        public int CampaignId { get; set; }
        public int CustomerId { get; set; }
        public bool IsNew { get; set; }
        #endregion

        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private ObjectCampaign.CampaignInstance m_objCampaign = null;
        private SaveType m_eSaveType = SaveType.SaveTypeAdd;
        private string m_MessageBoxCaption = "Manager Application - Customers & Campaigns";
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public AddCampaign()
        {
            InitializeComponent();
            this.InitializeModule();
        }

        /// <summary>
        /// Constructor to initialize save type and the campaign data object to edit
        /// </summary>
        public AddCampaign(SaveType eSaveType, ObjectCampaign.CampaignInstance objCampaign)
        {
            InitializeComponent();
            m_objCampaign = objCampaign;
            m_eSaveType = eSaveType;
            this.InitializeModule();
        }
        #endregion

        #region Object Events
        private void cmdSave_Click(object sender, EventArgs e)
        {
            this.SaveCampaign(IsNew);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Initializes this module
        /// </summary>
        private void InitializeModule()
        {
            this.LoadComboData();
            if (m_eSaveType == SaveType.SaveTypeEdit)
            {
                this.InitControls();
                this.DisplayCampaignInformation();
            }
        }

        /// <summary>
        /// Populates the web access combo box on the campaign form
        /// </summary>
        private void PopulateWebAccess()
        {
            cboWebAccess.Properties.DataSource = null;
            cboWebAccess.Properties.DataSource = ObjectUser.GetWebAccessUsers(CustomerId).Execute(MergeOption.AppendOnly);
            cboWebAccess.Properties.DisplayMember = "web_access_name";
            cboWebAccess.Properties.ValueMember = "id";
            cboWebAccess.Properties.Columns.Add(new LookUpColumnInfo("web_access_name"));
        }

        /// <summary>
        /// Populates the owner combo box on the campaign form
        /// </summary>
        private void PopulateOwners()
        {
            cboOwner.Properties.DataSource = null;
            cboOwner.Properties.DataSource = ObjectUser.GetOwnerUsers().Execute(MergeOption.AppendOnly);
            cboOwner.Properties.DisplayMember = "owner_name";
            cboOwner.Properties.ValueMember = "id";
            cboOwner.Properties.Columns.Add(new LookUpColumnInfo("owner_name"));
        }

        /// <summary>
        /// Simple method just to display the error message as per string parameter specified
        /// </summary>
        private void DisplayValidationError(string strFieldName)
        {
            MessageBox.Show("Invalid " + strFieldName, m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Provides the validation logic when adding/editing a record
        /// </summary>
        private bool ValidateEntries()
        {
            if (txtName.Text.Trim().Count() < 1)
            {
                this.DisplayValidationError("campaign name");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Logic to save campaign record
        /// </summary>
        private void SaveCampaign(bool IsNew)
        {
            if (!this.ValidateEntries())
                return;

            ObjectCampaign.CampaignInstance objParams = new ObjectCampaign.CampaignInstance();
            objParams.id = CampaignId;
            objParams.campaign_name = txtName.Text;
            objParams.customer_id = CustomerId;
            objParams.description = txtDescription.Text;
            objParams.status = cboStatus.Text;
            objParams.priority = Convert.ToByte(cboPriority.Text);
            objParams.owner_user_id = (int) cboOwner.EditValue;
            objParams.web_access_user_id = (int)cboWebAccess.EditValue;
            ObjectCampaign.SaveCampaign(IsNew, objParams);
            m_objParentControl.PopulateCampaignView(CustomerId);

            MessageBox.Show("Successfully updated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.ParentForm.Close();
        }

        /// <summary>
        /// Displays the campaign information in the campaign form when editing the current selected record
        /// </summary>
        private void DisplayCampaignInformation()
        {
            CampaignId = 0;
            var objCampaign = m_objBrightPlatformEntity.campaigns.Where(objField => objField.id == m_objCampaign.id).SingleOrDefault();
            txtName.Text = m_objCampaign.campaign_name;
            cboOwner.EditValue = m_objCampaign.owner_user_id; //this.GetSelectedIndexForOwner(m_objCampaign.owner_name);  //GetKeyValueByDisplayText (m_objCampaign.owner_name); //this.GetSelectedIndexForOwner(m_objCampaign.owner_name);
            cboWebAccess.EditValue = m_objCampaign.web_access_user_id; //this.GetSelectedIndexForWebAccess(m_objCampaign.web_access_name);
            cboPriority.Text = m_objCampaign.priority.ToString();
            cboStatus.Text = m_objCampaign.status;
            txtDescription.Text = m_objCampaign.description;
            CampaignId = m_objCampaign.id;
            CustomerId = m_objCampaign.customer_id;
        }

        /// <summary>
        /// Initializes the campaign form object controls
        /// </summary>
        private void InitControls()
        {
            txtName.Text = "";
            cboPriority.Text = "1";
            cboStatus.Text = "On Hold";
            txtDescription.Text = "";
        }

        /// <summary>
        /// Populates the owner and web access combo box on the user form
        /// </summary>
        private void LoadComboData()
        {
            this.PopulateOwners();
            this.PopulateWebAccess();
        }
        #endregion
    }
}
