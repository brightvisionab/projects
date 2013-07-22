
#region Namespaces
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
using System.Data.SqlClient;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using SalesConsultant.Business;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using DevExpress.XtraEditors.Controls;
#endregion

namespace SalesConsultant.Modules
{
    public partial class EditImportFile : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Properties
        public ManageImportList objParentControl { get; set; }
        #endregion

        #region Private Members
        private CTImportList objImportFile = null;
        private bool m_DoneLoading = false;
        #endregion

        #region Constructor
        public EditImportFile(CTImportList ImportFile)
        {
            m_DoneLoading = false;
            InitializeComponent();
            objImportFile = ImportFile;
            this.PopulateCustomers();
            this.PopulateCounties();
            this.LoadImportFileData();
        }
        #endregion

        #region Object Events
        private void cboCustomer_EditValueChanged(object sender, EventArgs e)
        {
            if (m_DoneLoading)
            {
                this.Cursor = Cursors.WaitCursor;
                this.PopulateCampaigns();
                this.Cursor = Cursors.Default;
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            DialogResult objChoice = MessageBox.Show("Are you sure to make changes to this imported file?", "Import File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objChoice == DialogResult.No)
                return;

            if (txtImportListName.Text.Length < 1 || Convert.ToInt32(cboCustomer.EditValue) < 1 || Convert.ToInt32(cboCampaign.EditValue) < 1 || Convert.ToInt32(cboCountry.EditValue) < 1)
            {
                MessageBox.Show("Please supply all fields", "Import File", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            this.UpdateImportFile();
            objParentControl.PopulateImportListView();
            this.Cursor = Cursors.Default;
            this.ParentForm.Close();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update the import file
        /// </summary>
        private void UpdateImportFile()
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            
            /**
             * update imported file
             */
            imported_files Item = objDbModel.imported_files.FirstOrDefault(i => i.id == objImportFile.id);
            Item.list_name = txtImportListName.Text;
            Item.customer_id = (int) cboCustomer.EditValue;
            Item.campaign_id = (int) cboCampaign.EditValue;
            Item.country_id = (int) cboCountry.EditValue;
            Item.modified_on = DateTime.Now;
            Item.modified_by = UserSession.CurrentUser.UserId;

            /**
             * update related list
             */
            list _efeList = objDbModel.lists.FirstOrDefault(
                i => i.customer_id == objImportFile.customer_id
                && i.list_name == objImportFile.import_list_name);

            if (_efeList != null)
            {
                _efeList.list_name = txtImportListName.Text;
                _efeList.customer_id = Convert.ToInt32(cboCustomer.EditValue);
                _efeList.modified_on = DateTime.Now;
                _efeList.modified_by = UserSession.CurrentUser.UserId;
            }

            /**
             * update related merge list
             */
            merge_lists _efeMergeList = objDbModel.merge_lists.FirstOrDefault(
                i => i.campaign_id == objImportFile.campaign_id
                && i.customer_id == objImportFile.customer_id
                && i.list_name == objImportFile.import_list_name);

            if (_efeMergeList != null)
            {
                _efeMergeList.list_name = txtImportListName.Text;
                _efeMergeList.customer_id = Convert.ToInt32(cboCustomer.EditValue);
                _efeMergeList.campaign_id = Convert.ToInt32(cboCampaign.EditValue);
                _efeMergeList.modified_on = DateTime.Now;
                _efeMergeList.modified_by = UserSession.CurrentUser.UserId;
            }

            objDbModel.SaveChanges();
        }

        /// <summary>
        /// Load the import file data to the form
        /// </summary>
        private void LoadImportFileData()
        {
            txtImportListName.Text = objImportFile.import_list_name;
            cboCustomer.EditValue = Convert.ToInt32(objImportFile.customer_id);
            m_DoneLoading = true;
            this.PopulateCampaigns();
            cboCampaign.EditValue = objImportFile.campaign_id;

            if (objImportFile.country_id > 0)
                cboCountry.EditValue = objImportFile.country_id;
            else
                cboCountry.ItemIndex = 0;
        }

        /// <summary>
        /// Populates the customer combo box on the new import form
        /// </summary>
        private void PopulateCustomers()
        {
            cboCustomer.Properties.DataSource = null;
            cboCustomer.Properties.DataSource = ObjectCustomer.GetCustomers(ObjectCustomer.eViewType.ComboListView).Execute(MergeOption.AppendOnly);
            cboCustomer.Properties.DisplayMember = "customer_name";
            cboCustomer.Properties.ValueMember = "id";
            cboCustomer.Properties.Columns.Add(new LookUpColumnInfo("customer_name"));
            cboCustomer.ItemIndex = 0;
        }

        /// <summary>
        /// Populates the campaign combo box on the new import form
        /// </summary>
        private void PopulateCampaigns()
        {
            cboCampaign.Properties.DataSource = null;
            cboCampaign.Properties.Columns.Clear();
            if (Convert.ToInt32(cboCustomer.EditValue) < 1)
                return;

            cboCampaign.Properties.DataSource = ObjectCampaign.GetCustomerContactCampaigns((int)cboCustomer.EditValue).Execute(MergeOption.AppendOnly);
            cboCampaign.Properties.DisplayMember = "name";
            cboCampaign.Properties.ValueMember = "id";
            cboCampaign.Properties.Columns.Add(new LookUpColumnInfo("name"));
            cboCampaign.ItemIndex = 0;
        }

        /// <summary>
        /// Populates the countries
        /// </summary>
        private void PopulateCounties()
        {
            cboCountry.Properties.DataSource = null;
            cboCountry.Properties.DataSource = ObjectCountry.GetCountries().Execute(MergeOption.AppendOnly);
            cboCountry.Properties.DisplayMember = "name";
            cboCountry.Properties.ValueMember = "id";
            cboCountry.Properties.Columns.Add(new LookUpColumnInfo("name"));
            cboCountry.ItemIndex = 0;
        }
        #endregion
    }
}
