
#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using SalesConsultant.Business;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
#endregion

namespace SalesConsultant.Modules
{
    public partial class ManageCallList : DevExpress.XtraEditors.XtraUserControl
    {
        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = null;
        private string m_MessageBoxCaption = "Manager Application - Call Lists";
        private StringBuilder m_objMergeListIds = null;
        private StringBuilder m_objBlockedMergeListIds = null;
        #endregion

        #region Constructors
        public ManageCallList()
        {
            this.Visible = false;
            InitializeComponent();
            this.lcManageCallList.AllowCustomizationMenu = false;
            this.PopulateCustomerComboList();
            btnSave.Enabled = false;
            cmdPreview.Enabled = false;
            this.Visible = true;
        }
        #endregion

        #region Object Control Events
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (cboCustomer.Text.Length < 1)
            {
                MessageBox.Show("Please select a customer", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboCustomer.Focus();
                return;
            }
            else if (cboCampaign.Text.Length < 1)
            {
                MessageBox.Show("Please select a campaign", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboCampaign.Focus();
                return;
            }

            WaitDialog.Show(ParentForm, "Loading...");
            this.PopulateCallListView();
            this.PopulateSubCampaignComboList((int)cboCampaign.EditValue, (int)cboCustomer.EditValue);
           WaitDialog.Close();
        }
        
        private void cbxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            this.CheckAllItems();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cboSubCampaign.Text.Length < 1)
            {
                MessageBox.Show("Please select a sub campaign for this listing.", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                cboSubCampaign.Focus();
                return;
            }

            this.GetMergeListIds();
            if (m_objMergeListIds == null)
                return;

            if (m_objMergeListIds.Length < 1)
                return;

            string MessageContent = "If the selected sub-campaign has been previously created." + Environment.NewLine
                //+ "Those companies and contacts not found from the existing sub-campaign will be removed." + Environment.NewLine
                + "Only those non-existing companies and contacts from this lists will be added." + Environment.NewLine
                + "Existing companies and contacts will be left as is." + Environment.NewLine + Environment.NewLine
                + "Are you sure to save this list to the selected sub-campaign?";

            DialogResult objChoice = MessageBox.Show(MessageContent, "Call List", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objChoice == DialogResult.No)
                return;

            WaitDialog.Show(ParentForm, "Saving...");
            this.SaveSubCampaignAccountAndContacts();
            WaitDialog.Close();
            MessageBox.Show("Done saving to sub campaign account list.", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cboCustomer_EditValueChanged(object sender, EventArgs e)
        {
            this.ResetLabels();
            this.InitControls();
            this.PopulateCampaignComboList();
        }

        private void ResetLabels()
        {
            lblResult.Text = "Results:";
            lblCountActive.Text = "Results:";
        }

        private void cboCampaign_EditValueChanged(object sender, EventArgs e)
        {
            this.ResetLabels();
            this.InitControls();
            this.LoadCallListDefaults();
        }

        private void cmdPreview_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Loading...");
            this.LoadCallListAccountsAndContacts();
            WaitDialog.Close();
        }

        private void gvCallList_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvActiveAccountAndContact_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initialize the controls
        /// </summary>
        private void InitControls()
        {
            cboSubCampaign.Properties.DataSource = null;
            gcCallList.DataSource = null;
            gcActiveAccountAndContact.DataSource = null;
        }

        /// <summary>
        /// Load accounts and contacts data on view
        /// </summary>
        private void LoadCallListAccountsAndContacts()
        {
            this.GetMergeListIds();
            lblCountActive.Text = "Results: 0";
            gcActiveAccountAndContact.DataSource = null;
            DataTable objCallLists = ObjectCallList.GetSubCampaignAccountsAndContacts(m_objMergeListIds.ToString(), m_objBlockedMergeListIds.ToString());
            if (objCallLists.Rows.Count > 0) {
                gcActiveAccountAndContact.DataSource = objCallLists;
                gvActiveAccountAndContact.BestFitColumns();
                lblCountActive.Text = "Results: " + objCallLists.Rows.Count.ToString();
            }
        }

        /// <summary>
        /// Load defaults
        /// </summary>
        private void LoadCallListDefaults()
        {
            cboSubCampaign.Properties.Columns.Clear();
            cboSubCampaign.Properties.DataSource = null;
            gcCallList.DataSource = null;
            lblResult.Text = "Results:";
            cbxSelectAll.Checked = true;
            btnSave.Enabled = false;
            cmdPreview.Enabled = false;
            gcActiveAccountAndContact.DataSource = null;
        }

        /// <summary>
        /// Save sub campaign account list
        /// </summary>
        private void SaveSubCampaignAccountAndContacts()
        {
            int CustomerId = (int) cboCustomer.EditValue;
            int CampaignId = (int) cboCampaign.EditValue;
            int SubCampaignId = (int) cboSubCampaign.EditValue;
            int FinalCallListId = ObjectCallList.SaveFinalCallList(cboSubCampaign.Text, CustomerId, CampaignId, SubCampaignId);

            ObjectCallList.SaveSubCampaignAccountLists(m_objMergeListIds.ToString(), m_objBlockedMergeListIds.ToString(), FinalCallListId);
            ObjectCallList.SaveSubCampaignContactLists(m_objMergeListIds.ToString(), m_objBlockedMergeListIds.ToString(), FinalCallListId);
            ObjectCallList.UpdateMergeLists(m_objMergeListIds.ToString(), m_objBlockedMergeListIds.ToString());

            //this.LoadCallListDefaults();            
        }

        /// <summary>
        /// Get merge list ids
        /// </summary>
        private void GetMergeListIds()
        {
            m_objMergeListIds = new StringBuilder();
            m_objBlockedMergeListIds = new StringBuilder();

            CTCallList item = null;
            GridView objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcCallList.FocusedView;
            for (int i = 0; i < objGridView.RowCount; i++)
            {
                item = null;
                item = (CTCallList) objGridView.GetRow(i);
                if (!Convert.ToBoolean(item.use_item))
                    continue;

                if (!Convert.ToBoolean(item.block_listed))
                    m_objMergeListIds.AppendFormat("{0}", m_objMergeListIds.Length > 0 ? "," + item.id.ToString() : item.id.ToString());
                else
                    m_objBlockedMergeListIds.AppendFormat("{0}", m_objBlockedMergeListIds.Length > 0 ? "," + item.id.ToString() : item.id.ToString());
            }
        }

        /// <summary>
        /// Call list view control defaults
        /// </summary>
        private void LoadCallListViewDefaults()
        {
            lblResult.Text = "Results:";
            cbxSelectAll.Checked = true;
            gcCallList.DataSource = null;
            cboSubCampaign.Properties.DataSource = null;
        }

        /// <summary>
        /// Populate call list view
        /// </summary>
        private void PopulateCallListView()
        {
            try
            {
                List<CTCallList> objCallList = ObjectCallList.GetCallLists((int)cboCustomer.EditValue, (int)cboCampaign.EditValue);
                gcCallList.DataSource = null;
                gcCallList.DataSource = objCallList;
                lblResult.Text = "Results: " + objCallList.Count.ToString() + " records found";

                if (objCallList.Count > 0)
                {
                    btnSave.Enabled = true;
                    cmdPreview.Enabled = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Populate sub campaign combo box
        /// </summary>
        private void PopulateSubCampaignComboList(int CampaignId, int CustomerId)
        {
            // sub campaigns
            cboSubCampaign.Properties.Columns.Clear();
            cboSubCampaign.Properties.DataSource = null;
            cboSubCampaign.Properties.DataSource = ObjectSubCampaign.GetSubCampaigns(ObjectSubCampaign.eViewType.ComboListView, CampaignId, CustomerId).Execute(MergeOption.AppendOnly);
            cboSubCampaign.Properties.DisplayMember = "title";
            cboSubCampaign.Properties.ValueMember = "id";
            cboSubCampaign.Properties.Columns.Add(new LookUpColumnInfo("title"));
            cboSubCampaign.Properties.ShowHeader = false;
            cboSubCampaign.Properties.ShowFooter = false;
        }

        /// <summary>
        /// Populate campaign combo box
        /// </summary>
        private void PopulateCampaignComboList()
        {
            if (cboCustomer.Text.Length < 1)
            {
                MessageBox.Show("Please kindly select a customer", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                cboCustomer.Focus();
                return;
            }

            try
            {
                cboCampaign.Properties.Columns.Clear();
                cboCampaign.Properties.DataSource = null;
                cboCampaign.Properties.DataSource = ObjectCampaign.GetCampaigns(ObjectCampaign.eViewtype.ComboListViewByCustomerId, (int)cboCustomer.EditValue).Execute(MergeOption.AppendOnly);
                cboCampaign.Properties.DisplayMember = "name";
                cboCampaign.Properties.ValueMember = "id";
                cboCampaign.Properties.Columns.Add(new LookUpColumnInfo("name"));
                cboCampaign.Properties.ShowHeader = false;
                cboCampaign.Properties.ShowFooter = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Populate combo box listings
        /// </summary>
        private void PopulateCustomerComboList()
        {
            try
            {
                cboCustomer.Properties.DataSource = null;
                cboCustomer.Properties.DataSource = ObjectCustomer.GetCustomers(ObjectCustomer.eViewType.ComboListView).Execute(MergeOption.AppendOnly);
                cboCustomer.Properties.DisplayMember = "customer_name";
                cboCustomer.Properties.ValueMember = "id";
                cboCustomer.Properties.Columns.Add(new LookUpColumnInfo("customer_name"));
                cboCustomer.Properties.ShowHeader = false;
                cboCustomer.Properties.ShowFooter = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Check/uncheck all searched items
        /// </summary>
        private void CheckAllItems()
        {
            GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcCallList.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
                m_objGridView.SetRowCellValue(i, "use_item", cbxSelectAll.Checked);
        }
        #endregion

        private void gvActiveAccountAndContact_ColumnFilterChanged(object sender, EventArgs e)
        {
            lblCountActive.Text = string.Format("Results: {0}", gvActiveAccountAndContact.RowCount);
        }
    }
}
