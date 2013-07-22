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
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using ManagerApplication.Business;

namespace ManagerApplication.Modules
{
    public partial class AddCustomer : DevExpress.XtraEditors.XtraUserControl
    {
        #region Enums
        /// <summary>
        /// Enum for save type (e.g. add new, edit)
        /// </summary>
        public enum SaveType
        {
            SaveTypeAdd,
            SaveTypeEdit
        }
        #endregion

        #region Public Members
        public ManageCustomerCampaign m_objParentControl = null;
        public int intCustomerId = 0;
        public bool isNew = true;
        #endregion

        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private ObjectCustomer.CustomerInstance m_objCustomer = null;
        private SaveType m_eSaveType = SaveType.SaveTypeAdd;
        private string m_MessageBoxCaption = "Manager Application - Customers & Campaigns";
        #endregion

        #region Constructors
        public AddCustomer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor to initialize save type and the customer data object to edit
        /// </summary>
        public AddCustomer(SaveType eSaveType, ObjectCustomer.CustomerInstance objCustomer)
        {
            InitializeComponent();
            m_objCustomer = objCustomer;
            m_eSaveType = eSaveType;
        }
        #endregion

        #region Object Control Events
        private void AddCustomer_Load(object sender, EventArgs e)
        {
            if (m_eSaveType == SaveType.SaveTypeEdit)
            {
                this.InitControls();
                this.DisplayCustomerInformation();
            }
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            this.SaveCustomer(isNew);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }
        #endregion

        #region Private Functions
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
                this.DisplayValidationError("customer name");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Logic to save customer record
        /// </summary>
        private void SaveCustomer(bool IsNew)
        {
            if (!this.ValidateEntries())
                return;

            ObjectCustomer.CustomerInstance objParams = new ObjectCustomer.CustomerInstance();
            
            objParams.id = intCustomerId;
            objParams.customer_name = txtName.Text;
            objParams.org_no = txtOrgNo.Text;
            objParams.reference_no = txtReferenceNo.Text;
            objParams.active = chkActive.Checked;
            objParams.owner_name = txtOwner.Text;
            objParams.address = txtAddress.Text;
            objParams.description = txtDescription.Text;

            ObjectCustomer.SaveCustomer(IsNew, objParams);
            m_objParentControl.PopulateCustomerView();

            MessageBox.Show("Successfully updated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.ParentForm.Close();
        }

        /// <summary>
        /// Displays the customer information in the customer form when editing the current selected record
        /// </summary>
        private void DisplayCustomerInformation()
        {
            var objCustomer = m_objBrightPlatformEntity.customers.Where(objField => objField.id == m_objCustomer.id).SingleOrDefault();
            intCustomerId = 0;

            txtName.Text = m_objCustomer.customer_name;
            txtOrgNo.Text = m_objCustomer.org_no;
            txtReferenceNo.Text = m_objCustomer.reference_no;
            chkActive.Checked = m_objCustomer.active;
            txtOwner.Text = m_objCustomer.owner_name;
            txtAddress.Text = m_objCustomer.address;
            txtDescription.Text = m_objCustomer.description;
            intCustomerId = objCustomer.id;
        }

        /// <summary>
        /// Initializes the customer form object members
        /// </summary>
        private void InitControls()
        {
            txtName.Text = "";
            txtOrgNo.Text = "";
            txtReferenceNo.Text = "";
            chkActive.Checked = false;
            txtOwner.Text = "";
            txtAddress.Text = "";
            txtDescription.Text = "";
        }
        #endregion
    }
}
