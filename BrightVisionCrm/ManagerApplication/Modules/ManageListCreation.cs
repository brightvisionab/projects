
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
using ManagerApplication.Business;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
#endregion

namespace ManagerApplication.Modules
{
    public partial class ManageListCreation : DevExpress.XtraEditors.XtraUserControl
    {
        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = null;
        private string m_MessageBoxCaption = "Manager Application - List Creation";
        private GridCheckMarksSelection gridSelection = null;
        #endregion

        #region Constructors
        public ManageListCreation()
        {
            this.Visible = false;
            InitializeComponent();
            this.lcListCreationMergeList.AllowCustomizationMenu = false;
            this.lcListCreationSearch.AllowCustomizationMenu = false;
          
            this.InitializeUserControlContents();
            this.SetPrintExportContextMenuSearchList();
            this.SetPrintExportContextMenuMergeList();
            
            this.Visible = true;
            gridSelection = new GridCheckMarksSelection(gvMergeList);
           
        }
        #endregion

        #region Object Control Events

        private void gcMergeList_DataSourceChanged(object sender, EventArgs e)
        {
            gridSelection.ClearSelection();
        }

        private void txtLimitRow_Validating(object sender, CancelEventArgs e)
        {
            if (!ValidationUtility.IsCurrency(txtLimitRow.Text))
                txtLimitRow.Text = "100";         
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Loading...");
            this.BuildQuery();
           WaitDialog.Close();
           
        }

        private void txtEmployeesMin_Validating(object sender, CancelEventArgs e)
        {
            if (txtEmployeesMin.Text.Length > 0 && !ValidationUtility.IsCurrency(txtEmployeesMin.Text))
            {
                MessageBox.Show("Invalid numeric value", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmployeesMin.Text = "0";
            }

            if (Convert.ToInt32(txtEmployeesMin.Text) > 0 && Convert.ToInt32(txtEmployeesMax.Text) > 0)
                if (Convert.ToInt32(txtEmployeesMin.Text) > Convert.ToInt32(txtEmployeesMax.Text))
                {
                    MessageBox.Show("Min should be less or equal the Max", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtEmployeesMin.Text = "0";
                }
        }

        private void txtEmployeesMax_Validating(object sender, CancelEventArgs e)
        {
            if (txtEmployeesMax.Text.Length > 0 && !ValidationUtility.IsCurrency(txtEmployeesMax.Text))
            {
                MessageBox.Show("Invalid numeric value", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmployeesMax.Text = "0";
            }

            if (Convert.ToInt32(txtEmployeesMin.Text) > 0 && Convert.ToInt32(txtEmployeesMax.Text) > 0)
                if (Convert.ToInt32(txtEmployeesMin.Text) > Convert.ToInt32(txtEmployeesMax.Text))
                {
                    MessageBox.Show("Min should be less or equal the Max", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtEmployeesMin.Text = "0";
                }
        }

        private void txtRevenueMin_Validating(object sender, CancelEventArgs e)
        {
            if (txtRevenueMin.Text.Length > 0 && !ValidationUtility.IsCurrency(txtRevenueMin.Text))
            {
                MessageBox.Show("Invalid numeric value", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtRevenueMin.Text = "0";
            }

            if (Convert.ToInt32(txtRevenueMin.Text) > 0 && Convert.ToInt32(txtRevenueMax.Text) > 0)
                if (Convert.ToInt32(txtRevenueMin.Text) > Convert.ToInt32(txtRevenueMax.Text))
                {
                    MessageBox.Show("Min should be less or equal the Max", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtRevenueMin.Text = "0";
                }
        }

        private void txtRevenueMax_Validating(object sender, CancelEventArgs e)
        {
            if (txtRevenueMax.Text.Length > 0 && !ValidationUtility.IsCurrency(txtRevenueMax.Text))
            {
                MessageBox.Show("Invalid numeric value", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtRevenueMax.Text = "0";
            }

            if (Convert.ToInt32(txtRevenueMin.Text) > 0 && Convert.ToInt32(txtRevenueMax.Text) > 0)
                if (Convert.ToInt32(txtRevenueMin.Text) > Convert.ToInt32(txtRevenueMax.Text))
                {
                    MessageBox.Show("Min should be less or equal the Max", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtRevenueMin.Text = "0";
                }
        }

        private void txtResultMin_Validating(object sender, CancelEventArgs e)
        {
            if (txtResultMin.Text.Length > 0 && !ValidationUtility.IsCurrency(txtResultMin.Text))
            {
                MessageBox.Show("Invalid numeric value", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtResultMin.Text = "0";
            }

            if (Convert.ToInt32(txtResultMin.Text) > 0 && Convert.ToInt32(txtResultMax.Text) > 0)
                if (Convert.ToInt32(txtResultMin.Text) > Convert.ToInt32(txtResultMax.Text))
                {
                    MessageBox.Show("Min should be less or equal the Max", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtResultMin.Text = "0";
                }
        }

        private void txtResultMax_Validating(object sender, CancelEventArgs e)
        {
            if (txtResultMax.Text.Length > 0 && !ValidationUtility.IsCurrency(txtResultMax.Text))
            {
                MessageBox.Show("Invalid numeric value", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtResultMax.Text = "0";
            }

            if (Convert.ToInt32(txtResultMin.Text) > 0 && Convert.ToInt32(txtResultMax.Text) > 0)
                if (Convert.ToInt32(txtResultMin.Text) > Convert.ToInt32(txtResultMax.Text))
                {
                    MessageBox.Show("Min should be less or equal the Max", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtResultMin.Text = "0";
                }
        }

        private void cbxSelectAll_CheckStateChanged(object sender, EventArgs e)
        {
            this.CheckAllItems();
        }

        private void txtListName_Validated(object sender, EventArgs e)
        {
            if (txtListName.Text.Length < 1)
                txtListName.Text = "[ enter list name here ]";
        }

        private void txtListName_Click(object sender, EventArgs e)
        {
            txtListName.Select(0, txtListName.Text.Length);
        }

        private void btnExportToBin_Click(object sender, EventArgs e)
        {
            int _CustId = Convert.ToInt32(cboCustomer.EditValue);
            BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            if (_efDbModel.lists.FirstOrDefault(i => i.list_name == txtListName.Text && i.customer_id == _CustId) != null) {
                MessageBox.Show("List name already exist.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            
            if (cboCustomer.EditValue == null) {
                MessageBox.Show("No selected customer", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                cboCustomer.Focus();
                return;
            }
            else if (txtListName.Text.Length < 1 || txtListName.Text.Equals("[ enter list name here ]")) {
                MessageBox.Show("No list name specified", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                txtListName.Focus();
                return;
            }

            WaitDialog.Show(ParentForm, "Exporting to bin...");
            m_objBrightPlatformEntity = null;
            m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);

            DataRowView objRowView = null;
            StringBuilder colAccountIds = new StringBuilder();

            try {
                GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcListCreationSearch.FocusedView;
                for (int i = 0; i < m_objGridView.RowCount; i++) {
                    objRowView = null;
                    objRowView = (DataRowView)m_objGridView.GetRow(i);
                    if (cbxExportCheckedItemsOnly.Checked && !Convert.ToBoolean(objRowView.Row[1]))
                        continue;

                    if (colAccountIds.Length > 0)
                        colAccountIds.Append(",");

                    colAccountIds.AppendFormat("{0}", (int)objRowView.Row[0]);
                }

                // save if has account ids selected
                if (colAccountIds.Length > 0) {
                    int ListId = ObjectListCreation.SaveList((int)cboCustomer.EditValue, txtListName.Text);
                    ObjectListCreation.SaveListAccounts(ListId, colAccountIds.ToString());
                }

                //DatabaseUtility.ExecuteBulkProcessing("vw_list_accounts", objBinListDataTable, m_objConnection, m_objTransaction);
                //m_objTransaction.Commit();
                this.IntializeSearchListDefaults();
                WaitDialog.Close();
                MessageBox.Show("Successfully exported data to database", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) {
                //ObjectListCreation.RemoveList(objList);
                //m_objTransaction.Rollback();
                this.IntializeSearchListDefaults();
                WaitDialog.Close();
                MessageBox.Show(string.Format("Transaction rolled back due to the ff:{0}{1}", Environment.NewLine, ex.Message), m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            
        }

        private void cbxMergeListSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            ToggleSelectAll(cbxMergeListSelectAll.Checked);
        }

        private void ToggleSelectAll(bool toggle)
        {
            GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcMergeList.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                if (cbxIncludeExported.Checked && !toggle)
                { 
                    var objRow = (DataRowView)m_objGridView.GetRow(i);
                    if (!Convert.ToBoolean(objRow.Row["exported"])) {
                        gridSelection.SelectRow(i, toggle);
                    }
                }else
                    gridSelection.SelectRow(i, toggle);
            }
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            if (gcMergeList.IsPrintingAvailable)
                gcMergeList.ShowPrintPreview();
        }

        private void cbxShowCheckedItemsOnly_CheckedChanged(object sender, EventArgs e)
        {
            this.SetMergeListRowVisibility();
        }

        private void txtFinalListName_Validated(object sender, EventArgs e)
        {
            if (gvMergeList.RowCount < 1)
                return;

            if (txtFinalListName.Text.Length < 1)
                txtFinalListName.Text = "[ enter list name here ]";
        }

        private void txtFinalListName_Click(object sender, EventArgs e)
        {
            txtFinalListName.Select(0, txtFinalListName.Text.Length);
        }

        private void btnExportAsList_Click(object sender, EventArgs e)
        {            
            //WaitDialog.Show(ParentForm, "Saving merged list...");
            this.ExportAsList();
            //WaitDialog.Close();
        }

        private void btnRemoveExportedMergeList_Click(object sender, EventArgs e)
        {
            if (!HasExportSelection())
            {
                MessageBox.Show("No selected records to delete.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            //    return;

            DialogResult objResult = MessageBox.Show("Are you sure to perform this operation?", m_MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.Yes)
            {
                WaitDialog.Show(ParentForm, "Removing items...");
                this.RemoveSelectedMergeListItems(true);
                WaitDialog.Close();
            }
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (gvMergeList.RowCount < 1 || gridSelection.SelectedCount == 0)
            {
                MessageBox.Show("No selected records to delete.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult objResult = MessageBox.Show("Are you sure to perform this operation?", m_MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.Yes)
            {
                WaitDialog.Show(ParentForm, "Removing items...");
                this.RemoveSelectedMergeListItems(false);
                gridSelection.ClearSelection();
               WaitDialog.Close();
            }
        }

        private void btnLoadList_Click(object sender, EventArgs e)
        {
            if (cboMergeListCustomer.Text.Length < 1)
                return;

            WaitDialog.Show(ParentForm, "Loading merged lists...");
            this.PopulateMergeListView(Convert.ToInt32(cboMergeListCustomer.EditValue));
            gridSelection.ClearSelection();
            ToggleSelectAll(cbxMergeListSelectAll.Checked);
            ToggleIncludeExport(cbxIncludeExported.Checked);
            WaitDialog.Close();
        }

        private void btnClearTable_Click(object sender, EventArgs e)
        {
            this.IntializeSearchListDefaults();
        }

        private void cbxSelectAllActivityCode_CheckedChanged(object sender, EventArgs e)
        {
            this.CheckAllActivityCodes();
        }

        private void cbxSelectAllContactTitle_CheckedChanged(object sender, EventArgs e)
        {
            this.CheckAllContactTitles();
        }

        private void cboCustomer_EditValueChanged(object sender, EventArgs e)
        {
            cboMergeListCustomer.EditValue = cboCustomer.EditValue;
        }

        private void cboMergeListCustomer_EditValueChanged(object sender, EventArgs e)
        {
            if (cboMergeListCustomer.EditValue == null)
                return;

            cboCustomer.EditValue = cboMergeListCustomer.EditValue;
            this.PopulateCampaignComboList();
            this.PopulateCustomerExportList();
        }

        private void objPrintPreviewSearchList_Click(object sender, EventArgs e)
        {
            if (gcListCreationSearch.IsPrintingAvailable)
                gcListCreationSearch.ShowPrintPreview();
        }

        private void objPrintPreviewMergeList_Click(object sender, EventArgs e)
        {
            if (gcMergeList.IsPrintingAvailable)
                gcMergeList.ShowPrintPreview();
        }

        private void cbxIncludeExported_CheckedChanged(object sender, EventArgs e)
        {
            ToggleIncludeExport(cbxIncludeExported.Checked);
        }

        private void ToggleIncludeExport(bool toggle)
        {
            DataRowView objRow;
            GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcMergeList.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                objRow = null;
                objRow = (DataRowView)m_objGridView.GetRow(i);
                if (Convert.ToBoolean(objRow.Row["exported"]))
                    gridSelection.SelectRow(i, toggle);
            }
         }

        private void cbxHideExported_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterMergeList();
        }

        private void cmdLoadByList_Click(object sender, EventArgs e)
        {
            if (cboMergeListCustomer.Text.Length < 1 || cboListName.Text.Length < 1)
                return;

            WaitDialog.Show(ParentForm, "Loading merged lists...");
            this.PopulateMergeListViewByListId((int)cboMergeListCustomer.EditValue, (int)cboListName.EditValue);
            gridSelection.ClearSelection();
            ToggleSelectAll(cbxMergeListSelectAll.Checked);
            ToggleIncludeExport(cbxIncludeExported.Checked);
            
            WaitDialog.Close();
        }

        private void gvListCreationSearch_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvActivityCode_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvContact_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Filter merge list items
        /// </summary>
        private void FilterMergeList()
        {
            gvMergeList.ClearColumnsFilter();
            if (cbxHideExported.Checked)
                gvMergeList.Columns["exported"].FilterInfo = new ColumnFilterInfo("[exported] = 0");
        }

        /// <summary>
        /// Remove the selected merge list items
        /// </summary>
        private void RemoveSelectedMergeListItems(bool RemoveOnlyExportedItem)
        {
            StringBuilder TableIds = new StringBuilder();
            DataRowView objRowView = null;

            GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcMergeList.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                objRowView = null;
                objRowView = (DataRowView) m_objGridView.GetRow(i);

                if (!gridSelection.IsRowSelected(i))
                    continue;
                else
                    if (RemoveOnlyExportedItem && !Convert.ToBoolean(objRowView.Row[2]))
                        continue;

                if (TableIds.Length > 0)
                    TableIds.Append(", ");
                
                TableIds.Append(objRowView.Row[0].ToString());
            }

            if (TableIds.Length > 0)
            {
                ObjectListCreation.RemoveExportedMergeList(TableIds.ToString());
                this.LoadMergeListViewDefaults();
                MessageBox.Show("Merge list successfully updated", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No selected records to delete", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Saves the selected merge list items in one final list (by updating the list_accounts table)
        /// </summary>
        private void ExportAsList()
        {
            if (txtFinalListName.Text.Length < 1 || txtFinalListName.Text.Equals("[ enter list name here ]")) {
                MessageBox.Show("Please enter a list name", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtFinalListName.Focus();
                txtFinalListName.SelectAll();
                return;
            }

            if (cboCampaign.Text.Length < 1) {
                MessageBox.Show("Please select a campaign", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboCampaign.Focus();
                return;
            }

            WaitDialog.Show(ParentForm, "Saving merged list...");
            DataRowView objRowView = null;
            StringBuilder TableIds = new StringBuilder();
            
            m_objBrightPlatformEntity = null;
            m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);

            GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcMergeList.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++) {
                objRowView = null;
                objRowView = (DataRowView) m_objGridView.GetRow(i);

                if (cbxMergeListExportCheckedItemsOnly.Checked && !Convert.ToBoolean(objRowView.Row[1]))
                    continue;

                if (TableIds.Length > 0)
                    TableIds.Append(", ");

                TableIds.Append(objRowView.Row[0].ToString());
            }

            if (TableIds.Length > 0) {
                int MergeListId = ObjectListCreation.SaveMergeList((int)cboMergeListCustomer.EditValue, txtFinalListName.Text, (int)cboCampaign.EditValue);
                ObjectListCreation.SaveExportedListAccounts(TableIds.ToString());
                ObjectListCreation.CreateMergeListItems(MergeListId, TableIds.ToString());
                this.LoadMergeListViewDefaults();
                WaitDialog.Close();
                MessageBox.Show("Merge list successfully saved", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                WaitDialog.Close();
        }

        /// <summary>
        /// Load merge list view defaults
        /// </summary>
        private void LoadMergeListViewDefaults()
        {
            gcMergeList.DataSource = null;
            lblMergeListResult.Text = "Results: 0 records found";
            //this.PopulateMergeListView((int)cboMergeListCustomer.EditValue);
            txtFinalListName.Text = "[ enter list name here ]";
            cbxMergeListSelectAll.Checked = false;
            cbxMergeListExportCheckedItemsOnly.Checked = false;
        }

        /// <summary>
        /// Clear the list search record items view
        /// </summary>
        private void InitSearchListView()
        {
            gcListCreationSearch.DataSource = null;
        }

        /// <summary>
        /// Initialize search list controls
        /// </summary>
        private void IntializeSearchListDefaults()
        {
            gcListCreationSearch.DataSource = null;
            cbxSelectAll.Checked = false;
            cbxExportCheckedItemsOnly.Checked = false;
            txtListName.Text = "[ enter list name here ]";
            lblSearchResult.Text = "Search Results:";
        }

        /// <summary>
        /// Check/uncheck all searched items
        /// </summary>
        private void CheckAllMergeListItems()
        {
            DataRowView objRow = null;
            GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcMergeList.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                if (cbxMergeListSelectAll.Checked)
                {

                    m_objGridView.SetRowCellValue(i, "use_item", false);

                    objRow = null;
                    objRow = (DataRowView) m_objGridView.GetRow(i);
                    if (!cbxIncludeExported.Checked && Convert.ToBoolean(objRow.Row["exported"]))
                        continue;
                }
                //gridSelection.SelectRow(i, cbxMergeListSelectAll.Checked);
                m_objGridView.SetRowCellValue(i, "use_item", cbxMergeListSelectAll.Checked);
            }
        }

        /// <summary>
        /// Check/uncheck all contact titles items
        /// </summary>
        private void CheckAllContactTitles()
        {
            GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcContact.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
                m_objGridView.SetRowCellValue(i, "use_item", cbxSelectAllContactTitle.Checked);
        }

        /// <summary>
        /// Check/uncheck all activity code items
        /// </summary>
        private void CheckAllActivityCodes()
        {
            GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcActivityCode.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
                m_objGridView.SetRowCellValue(i, "use_item", cbxSelectAllActivityCode.Checked);
        }

        /// <summary>
        /// Check/uncheck all searched items
        /// </summary>
        private void CheckAllItems()
        {
            GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcListCreationSearch.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
                m_objGridView.SetRowCellValue(i, "use_record", cbxSelectAll.Checked);
        }

        /// <summary>
        /// Populate search list
        /// </summary>
        private void PopulateSearchListView(string sqlQuery)
        {
            DataTable objRecordTable = DatabaseUtility.ExecuteSqlQuery(sqlQuery);
            gcListCreationSearch.DataSource = null;
            gcListCreationSearch.DataSource = objRecordTable;
            lblSearchResult.Text = "Search Results: " + (gvListCreationSearch.RowCount > 0? gvListCreationSearch.RowCount: 0).ToString() + " records found";
        }

        /// <summary>
        /// Initialize numeric fields
        /// </summary>
        private void InitializeNumericFields()
        {
            txtEmployeesMin.Text = "0";
            txtEmployeesMax.Text = "0";
            txtRevenueMin.Text = "0";
            txtRevenueMax.Text = "0";
            txtResultMin.Text = "0";
            txtResultMax.Text = "0";
        }

        /// <summary>
        /// Build the sql query
        /// </summary>
        private void BuildQuery()
        {
            SqlBuilder objSqlBuilder = new SqlBuilder();
            objSqlBuilder.Clear();

            if (txtCompany.Text.Length > 0)
                objSqlBuilder.CreateWhere("a.company_name", txtCompany.Text, SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.String);

            if (txtCity.Text.Length > 0)
                objSqlBuilder.CreateWhere("a.city", txtCity.Text, SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.String);

            if (txtStreetAddress.Text.Length > 0)
                objSqlBuilder.CreateWhere("a.street_address", txtStreetAddress.Text, SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.String);

            if (txtBoxAddress.Text.Length > 0)
                objSqlBuilder.CreateWhere("a.box_address", txtBoxAddress.Text, SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.String);

            if (txtTelephone.Text.Length > 0)
                objSqlBuilder.CreateWhere("a.telephone", txtTelephone.Text, SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.String);

            if (txtCountry.Text.Length > 0)
                objSqlBuilder.CreateWhere("a.country", txtCountry.Text, SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.String);

            if (cboCategory.Text.Length > 0)
                objSqlBuilder.CreateWhere("a.category", cboCategory.Text, SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.String);

            if (Convert.ToInt32(txtEmployeesMin.Text) > 0 || Convert.ToInt32(txtEmployeesMax.Text) > 0)
                objSqlBuilder.CreateWhere("a.employees_total", txtEmployeesMin.Text, txtEmployeesMax.Text, SqlBuilder.eSqlConditionType.And);

            if (Convert.ToInt32(txtRevenueMin.Text) > 0 || Convert.ToInt32(txtRevenueMax.Text) > 0)
                objSqlBuilder.CreateWhere("a.turnover", txtRevenueMin.Text, txtRevenueMax.Text, SqlBuilder.eSqlConditionType.And);

            if (Convert.ToInt32(txtResultMin.Text) > 0 || Convert.ToInt32(txtResultMax.Text) > 0)
                objSqlBuilder.CreateWhere("a.result", txtResultMin.Text, txtResultMax.Text, SqlBuilder.eSqlConditionType.And);

            if (cbxShowOnlyValidatedCompanies.Checked)
                objSqlBuilder.CreateWhere("a.validated", "1", SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.Int);

            GridView m_objGridView = null;

            // activity codes
            StringBuilder sqlActivityCodes = new StringBuilder();
            DataRowView objRow = null;
            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcActivityCode.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                objRow = null;
                objRow = (DataRowView) m_objGridView.GetRow(i);

                if (!Convert.ToBoolean(objRow.Row["use_item"]))
                    continue;

                if (sqlActivityCodes.Length > 0)
                    sqlActivityCodes.Append(",");

                sqlActivityCodes.AppendFormat("'{0}'", objRow.Row["code"].ToString().Trim());
            }

            if (sqlActivityCodes.Length > 0)
                objSqlBuilder.CreateWhere("a.activity_code", sqlActivityCodes.ToString(), SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.In);

            // contact information
            StringBuilder sqlCICondition;
            StringBuilder sqlContactInformation = new StringBuilder();
            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcContact.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                objRow = null;
                objRow = (DataRowView) m_objGridView.GetRow(i);

                if (!Convert.ToBoolean(objRow.Row["use_item"]))
                    continue;

                sqlCICondition = new StringBuilder();
                sqlCICondition.AppendFormat("c.title LIKE '%{0}%'", objRow.Row["title"].ToString().Trim());

                if (Convert.ToBoolean(objRow.Row["with_email"]))
                    sqlCICondition.Append(" AND c.email IS NOT NULL AND c.email <> ''");

                if (Convert.ToBoolean(objRow.Row["with_direct_phone"]))
                    sqlCICondition.Append(" AND c.direct_phone IS NOT NULL AND c.direct_phone <> ''");

                if (Convert.ToBoolean(objRow.Row["with_mobile"]))
                    sqlCICondition.Append(" AND c.mobile IS NOT NULL AND c.mobile <> ''");

                if (sqlContactInformation.Length > 0)
                    sqlContactInformation.Append(" OR ");

                sqlContactInformation.Append("(" + sqlCICondition.ToString() + ")");
            }

            if (sqlContactInformation.Length > 0)
            {
                objSqlBuilder.CreateWhere("(" + sqlContactInformation.ToString() + ")", SqlBuilder.eSqlConditionType.And);
                objSqlBuilder.CreateJoin("account_contacts", "ac", "a.id = ac.account_id", SqlBuilder.eSqlJoinType.InnerJoin);
                objSqlBuilder.CreateJoin("contacts", "c", "ac.contact_id = c.id", SqlBuilder.eSqlJoinType.InnerJoin);
            }

            // create a sub query to get ids only
            objSqlBuilder.CreateFrom("accounts", "a");
            objSqlBuilder.CreateSelect("TOP " + txtLimitRow.Text + " a.id", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateGroupBy("a.id");
            string sqlAccountIds = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.Select);

            objSqlBuilder.Clear();
            objSqlBuilder.CreateSelect("a.id", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("CAST(1 AS bit)", "use_record", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.company_name", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.org_no", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.box_address", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.street_address", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.zipcode", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.city", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.country", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.telephone", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.telefax", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.www", "site", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.year_established", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.parent_company", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.activity_code", null, SqlBuilder.eSqlSelectAggregateType.None);
            //objSqlBuilder.CreateSelect("CAST(a.activity_code_description AS nvarchar(max))", "activity_code_description", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.activity_code_2", null, SqlBuilder.eSqlSelectAggregateType.None);
            //objSqlBuilder.CreateSelect("CAST(a.activity_code2_description AS nvarchar(max))", "activity_code_2_description", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.currency", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.fiscal", null, SqlBuilder.eSqlSelectAggregateType.None);
            //objSqlBuilder.CreateSelect("ISNULL(a.consolidated_figures, 0)", "consolidated_figure", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("ISNULL(a.turnover, 0)", "turnover", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("ISNULL(a.export, 0)", "export", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("ISNULL(a.result, 0)", "result", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("ISNULL(a.sales_abroad, 0)", "sales_abroad", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("ISNULL(a.employees_abroad, 0)", "employees_abroad", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("ISNULL(a.employees_total, 0)", "employees_total", SqlBuilder.eSqlSelectAggregateType.None);
            //objSqlBuilder.CreateSelect("ISNULL(a.net_interest_incom, 0)", "net_interest_income", SqlBuilder.eSqlSelectAggregateType.None);
           // objSqlBuilder.CreateSelect("ISNULL(a.gross_premiums, 0)", "gross_premium", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.category", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.county", null, SqlBuilder.eSqlSelectAggregateType.None);
           // objSqlBuilder.CreateSelect("a.county_code", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.bv_source", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("a.regions", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateFrom("accounts", "a");
            objSqlBuilder.CreateDerivedTableJoin(sqlAccountIds, "keys", "a.id = keys.id", SqlBuilder.eSqlJoinType.InnerJoin);
            objSqlBuilder.CreateOrderBy("a.company_name");

            string sqlQuery = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.Select);
            this.PopulateSearchListView(sqlQuery);
        }

        /// <summary>
        /// Get account categories
        /// </summary>
        private void PopulateCategoryCombo()
        {
            cboCategory.Properties.Columns.Clear();
            cboCategory.Properties.DataSource = null;
            cboCategory.Properties.DataSource = ObjectCompany.GetCompanyCategories().Execute(MergeOption.AppendOnly);
            cboCategory.Properties.DisplayMember = "category";
            cboCategory.Properties.Columns.Add(new LookUpColumnInfo("category"));
            cboCategory.Properties.ShowHeader = false;
            cboCategory.Properties.ShowFooter = false;
        }

        /// <summary>
        /// Populate customer list
        /// </summary>
        private void PopulateCustomerComboLists()
        {
            ObjectResult objCustomers = ObjectCustomer.GetCustomers(ObjectCustomer.eViewType.ComboListView).Execute(MergeOption.AppendOnly);

            cboCustomer.Properties.DataSource = objCustomers;
            cboCustomer.Properties.DisplayMember = "customer_name";
            cboCustomer.Properties.ValueMember = "id";
            cboCustomer.Properties.Columns.Add(new LookUpColumnInfo("customer_name"));
            cboCustomer.Properties.ShowHeader = false;
            cboCustomer.Properties.ShowFooter = false;

            cboMergeListCustomer.Properties.DataSource = objCustomers;
            cboMergeListCustomer.Properties.DisplayMember = "customer_name";
            cboMergeListCustomer.Properties.ValueMember = "id";
            cboMergeListCustomer.Properties.Columns.Add(new LookUpColumnInfo("customer_name"));
            cboMergeListCustomer.Properties.ShowHeader = false;
            cboMergeListCustomer.Properties.ShowFooter = false;
        }

        /// <summary>
        /// Populate customer exported list
        /// </summary>
        private void PopulateCustomerExportList()
        {
            WaitDialog.Show(ParentForm, "Loading...");
            cboListName.Properties.DataSource = null;
            cboListName.Properties.DataSource = ObjectCallList.GetCustomerCallList((int)cboMergeListCustomer.EditValue);
            cboListName.Properties.DisplayMember = "list_name";
            cboListName.Properties.ValueMember = "id";
            cboListName.Properties.Columns.Clear();
            cboListName.Properties.Columns.Add(new LookUpColumnInfo("list_name"));
            cboListName.ItemIndex = 0;
           WaitDialog.Close();
        }

        /// <summary>
        /// Populate campaign list
        /// </summary>
        private void PopulateCampaignComboList()
        {
            cboCampaign.Properties.Columns.Clear();
            cboCampaign.Properties.DataSource = null;
            cboCampaign.Properties.DataSource = ObjectCampaign.GetCampaigns(ObjectCampaign.eViewtype.ComboListViewByCustomerId, (int)cboMergeListCustomer.EditValue).Execute(MergeOption.AppendOnly);
            cboCampaign.Properties.DisplayMember = "name";
            cboCampaign.Properties.ValueMember = "id";
            cboCampaign.Properties.Columns.Add(new LookUpColumnInfo("name"));
            cboCampaign.Properties.ShowHeader = false;
            cboCampaign.Properties.ShowFooter = false;
        }

        /// <summary>
        /// Initialize form contents
        /// </summary>
        private void InitializeUserControlContents()
        {
            this.PopulateCustomerComboLists();
            //this.PopulateCampaignComboList();
            this.PopulateCategoryCombo();
            this.PopulateActivityCodeView();
            this.PopulateContactInformationView();
            this.InitializeNumericFields();
        }

        /// <summary>
        /// Populate activity code view
        /// </summary>
        private void PopulateActivityCodeView()
        {
            try
            {
                gcActivityCode.DataSource = null;
                gcActivityCode.DataSource = ObjectCompany.GetActivityCodes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Populate contact information view
        /// </summary>
        private void PopulateContactInformationView()
        {
            try
            {
                gcContact.DataSource = null;
                gcContact.DataSource = ObjectContact.GetContactDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Populate merge list view
        /// </summary>
        private void PopulateMergeListViewByListId(int CustomerId, int ListId)
        {
            try
            {
                gcMergeList.DataSource = null;
                gcMergeList.DataSource = ObjectListCreation.GetMergeListsByListId(CustomerId, ListId);
                lblMergeListResult.Text = "Results: " + gvMergeList.RowCount.ToString() + " records found";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Populate merge list view
        /// </summary>
        private void PopulateMergeListView(int CustomerId)
        {
            try
            {
                gcMergeList.DataSource = null;
                gcMergeList.DataSource = ObjectListCreation.GetMergeLists(CustomerId);
                lblMergeListResult.Text = "Results: " + gvMergeList.RowCount.ToString() + " records found";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        /// <summary>
        /// Hide / unhide merge list rows
        /// </summary>
        private void SetMergeListRowVisibility()
        {
            //todo: follow up on this functionality: http://community.devexpress.com/forums/p/101874/346840.aspx#346840

            //GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcMergeList.FocusedView;
            //if (cbxShowCheckedItemsOnly.Checked)
            //{
            //    // hide all unchecked rows
            //    DataRowView objRowView = null;
            //    for (int i = 0; i < m_objGridView.RowCount; i++)
            //    {
            //        objRowView = null;
            //        objRowView = (DataRowView) m_objGridView.GetRow(i);

            //        if (!Convert.ToBoolean(objRowView.Row[0]))
            //            //gcMergeList
            //            m_objGridView.Columns[
            //        //    m_objGridView.MakeRowVisible(i, false);
            //        //m_objGridView.
            //    }
            //    m_objGridView.RefreshData();
            //}
            //else
            //    gcMergeList.Refresh();
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
        private void SetPrintExportContextMenuSearchList()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem objPrintPreview = new MenuItem("Print Preview");
            objPrintPreview.Click += new EventHandler(objPrintPreviewSearchList_Click);
            objClickMenu.MenuItems.Add(objPrintPreview);
            gcListCreationSearch.ContextMenu = objClickMenu;
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
        private void SetPrintExportContextMenuMergeList()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem objPrintPreview = new MenuItem("Print Preview");
            objPrintPreview.Click += new EventHandler(objPrintPreviewMergeList_Click);
            objClickMenu.MenuItems.Add(objPrintPreview);
            gcMergeList.ContextMenu = objClickMenu;
        }

        private bool HasExportSelection() 
        {
            GridView m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcMergeList.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                var objRow = (DataRowView)m_objGridView.GetRow(i);
                var isRowSelected = gridSelection.IsRowSelected(i);
                if (Convert.ToBoolean(objRow.Row["exported"]) && isRowSelected)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

    }
}
