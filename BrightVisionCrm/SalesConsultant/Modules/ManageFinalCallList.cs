
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
using SalesConsultant.Forms;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
#endregion

namespace SalesConsultant.Modules
{
    public partial class ManageFinalCallList : DevExpress.XtraEditors.XtraUserControl
    {       
        #region Private Members
        private List<int> m_ModifiedRowHandleContacts = null;
        private List<CTFinalCallListContact> m_ModifiedContacts = null;
        private CTFinalCallListContact m_SelectedContact = null;
        private List<int> m_ModifiedRowHandleAccounts = null;
        private List<CTFinalCallListAccount> m_ModifiedAccounts = null;
        private CTFinalCallListAccount m_SelectedAccount = null;
        private bool m_DoneLoadingCustomer = true;
        private bool m_DoneLoadingCampaign = true;
        private bool m_DoneLoadingSubCampaign = true;
        private List<CTFinalCallListAccount> m_lstAccounts = null;
        private List<CTFinalCallListContact> m_lstContacts = null;
        #endregion

        #region Constructors
        public ManageFinalCallList()
        {
            this.Visible = false;
            InitializeComponent();
            this.lcManageFinalCallList.AllowCustomizationMenu = false;
            this.PopulateCustomerComboList();
            this.Visible = true;
        }
        #endregion

        #region Object Control Events
        private void cmdApplyCustomPrioContact_Click(object sender, EventArgs e)
        {
            if (txtCustomPrioContact.Text.Length < 1)
            {
                MessageBox.Show("Please enter a custom prio text.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCustomPrioContact.Focus();
                return;
            }

            WaitDialog.Show(ParentForm, "Updating account list...");
            this.UpdateContactPriorities(txtCustomPrioContact.Text);
            WaitDialog.Close();
        }

        private void cmdCompanySelectMarked_Click(object sender, EventArgs e)
        {
            int[] _SelectedRows = gvCompany.GetSelectedRows();
            foreach (int _RowHandle in _SelectedRows)
                gvCompany.SetRowCellValue(_RowHandle, "selected", true);
        }

        private void cmdCompanyDeSelectMarked_Click(object sender, EventArgs e)
        {
            int[] _SelectedRows = gvCompany.GetSelectedRows();
            foreach (int _RowHandle in _SelectedRows)
                gvCompany.SetRowCellValue(_RowHandle, "selected", false);
        }

        private void cmdContactSelectMarked_Click(object sender, EventArgs e)
        {
            int[] _SelectedRows = gvContact.GetSelectedRows();
            foreach (int _RowHandle in _SelectedRows)
                gvContact.SetRowCellValue(_RowHandle, "selected", true);
        }

        private void cmdContactDeSelectMarked_Click(object sender, EventArgs e)
        {
            int[] _SelectedRows = gvContact.GetSelectedRows();
            foreach (int _RowHandle in _SelectedRows)
                gvContact.SetRowCellValue(_RowHandle, "selected", false);
        }

        private void cmdAddCompany_Click(object sender, EventArgs e)
        {
            
           WaitDialog.Show(ParentForm, "Loading...");
            AddSubCampaignAccount _ucSubCampaignAccount = new AddSubCampaignAccount();
            PopupDialog _dlgPopup = new PopupDialog();
            //_ucSubCampaignAccount.ParentController = this;
            _dlgPopup.FormBorderStyle = FormBorderStyle.FixedSingle;
            _dlgPopup.MinimizeBox = false;
            _dlgPopup.MaximizeBox = false;
            _dlgPopup.StartPosition = FormStartPosition.CenterScreen;
            _dlgPopup.Text = "Add Companies";
            _dlgPopup.Controls.Add(_ucSubCampaignAccount);
            _dlgPopup.ClientSize = new Size(_ucSubCampaignAccount.Width + 2, _ucSubCampaignAccount.Height + 2);
            WaitDialog.Close();
            _dlgPopup.ShowDialog(this.ParentForm);
        }

        private void cmdApplyCustomPrio_Click(object sender, EventArgs e)
        {
            if (txtCustomPrio.Text.Length < 1)
            {
                MessageBox.Show("Please enter a custom prio text.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCustomPrio.Focus();
                return;
            }

            
            WaitDialog.Show(ParentForm, "Updating account list...");
            this.UpdateAccountPriorities(txtCustomPrio.Text);
            WaitDialog.Close();
        }

        private void cmdApplyCustomAssignedTo_Click(object sender, EventArgs e)
        {
            if (txtCustomAssignedTo.Text.Length < 1)
            {
                MessageBox.Show("Please enter a custom assigned to text.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCustomAssignedTo.Focus();
                return;
            }

            if (gvCompany.RowCount < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Updating account list...");
            CTFinalCallListAccount item = new CTFinalCallListAccount();
            for (int i = 0; i < gvCompany.RowCount; i++)
            {
                item = gvCompany.GetRow(i) as CTFinalCallListAccount;
                if (Convert.ToBoolean(item.selected))
                    gvCompany.SetRowCellValue(i, "assigned_to", txtCustomAssignedTo.Text);
            }

            gvCompany.BestFitColumns();
            WaitDialog.Close();
        }

        private void gvCompany_ColumnFilterChanged(object sender, EventArgs e)
        {
            lblCompanyRecordStat.Text = "     Records: " + gvCompany.RowCount.ToString();
        }

        private void gvContact_ColumnFilterChanged(object sender, EventArgs e)
        {
            lblContactRecordStat.Text = "     Records: " + gvContact.RowCount.ToString();
        }

        private void gvCompany_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvContact_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void cmdContactSaveChanges_Click(object sender, EventArgs e)
        {
            if (m_ModifiedContacts == null)
                return;

            if (m_ModifiedContacts.Count < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Saving contact list...");
            this.SaveQueuedContacts();
            WaitDialog.Close();
        }

        private void cmdContactPriorityTop_Click(object sender, EventArgs e)
        {
            this.UpdateContactPriorities("Top");
        }

        private void cmdContactPriorityHigh_Click(object sender, EventArgs e)
        {
            this.UpdateContactPriorities("High");
        }

        private void cmdContactPriorityMedium_Click(object sender, EventArgs e)
        {
            this.UpdateContactPriorities("Medium");
        }

        private void cmdContactPriorityLow_Click(object sender, EventArgs e)
        {
            this.UpdateContactPriorities("Low");
        }

        private void cmdContactSelectAll_Click(object sender, EventArgs e)
        {
            if (gvContact.RowCount < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Updating contact list...");
            CTFinalCallListContact item = new CTFinalCallListContact();
            for (int i = 0; i < gvContact.RowCount; i++)
            {
                item = gvContact.GetRow(i) as CTFinalCallListContact;
                if (item.id > 0)
                    gvContact.SetRowCellValue(i, "selected", true);
            }

            WaitDialog.Close();
        }

        private void cmdContactClearSelection_Click(object sender, EventArgs e)
        {
            if (gvContact.RowCount < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Updating contact list...");
            for (int i = 0; i < gvContact.RowCount; i++)
                gvContact.SetRowCellValue(i, "selected", false);

            WaitDialog.Close();
        }

        private void cmdCompanyReleaseLockForSelected_Click(object sender, EventArgs e)
        {
            if (gvCompany.RowCount < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Queueing items...");
            List<int> ItemsToUnlock = new List<int>();
            CTFinalCallListAccount item = new CTFinalCallListAccount();
            for (int i = 0; i < gvCompany.RowCount; i++)
            {
                item = gvCompany.GetRow(i) as CTFinalCallListAccount;
                if (Convert.ToBoolean(item.selected) && !string.IsNullOrEmpty(item.locked_by))
                {
                    ItemsToUnlock.Add(item.id);
                    gvCompany.SetRowCellValue(i, "locked_by", "");
                }
            }
            
            if (ItemsToUnlock.Count > 0)
                ObjectCallList.ReleaseAccountLocks(ItemsToUnlock);
            
            WaitDialog.Close();
        }

        private void cmcCompanyAssignSalesUser_Click(object sender, EventArgs e)
        {
            if (gvCompany.RowCount < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Updating account list...");
            CTFinalCallListAccount item = new CTFinalCallListAccount();
            for (int i = 0; i < gvCompany.RowCount; i++)
            {
                item = gvCompany.GetRow(i) as CTFinalCallListAccount;
                if (Convert.ToBoolean(item.selected))
                    gvCompany.SetRowCellValue(i, "assigned_to", cboSalesUser.Text);
            }

            gvCompany.BestFitColumns();
            WaitDialog.Close();
        }

        private void cmdCompanyPriorityTop_Click(object sender, EventArgs e)
        {
            
            WaitDialog.Show(ParentForm, "Updating account list...");
            this.UpdateAccountPriorities("Top");
            WaitDialog.Close();
        }

        private void cmdCompanyPriorityHigh_Click(object sender, EventArgs e)
        {
            
            WaitDialog.Show(ParentForm, "Updating account list...");
            this.UpdateAccountPriorities("High");
            WaitDialog.Close();
        }

        private void cmdCompanyPriorityMedium_Click(object sender, EventArgs e)
        {
            
            WaitDialog.Show(ParentForm, "Updating account list...");
            this.UpdateAccountPriorities("Medium");
            WaitDialog.Close();
        }

        private void cmdCompanyPriorityLow_Click(object sender, EventArgs e)
        {
            
            WaitDialog.Show(ParentForm, "Updating account list...");
            this.UpdateAccountPriorities("Low");
            WaitDialog.Close();
        }

        private void cmdCompanyDeActivate_Click(object sender, EventArgs e)
        {
            if (gvCompany.RowCount < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Updating account list...");
            CTFinalCallListAccount item = new CTFinalCallListAccount();
            for (int i = 0; i < gvCompany.RowCount; i++)
            {
                item = gvCompany.GetRow(i) as CTFinalCallListAccount;
                if (Convert.ToBoolean(item.selected))
                    gvCompany.SetRowCellValue(i, "active", "No");
            }
            WaitDialog.Close();
        }

        private void cmdCompanyActivate_Click(object sender, EventArgs e)
        {
            if (gvCompany.RowCount < 1)
                return;
            
            WaitDialog.Show(ParentForm, "Updating account list...");
            CTFinalCallListAccount item = new CTFinalCallListAccount();
            for (int i = 0; i < gvCompany.RowCount; i++) {
                item = gvCompany.GetRow(i) as CTFinalCallListAccount;
                if (Convert.ToBoolean(item.selected))
                    gvCompany.SetRowCellValue(i, "active", "Yes");
            }
            WaitDialog.Close();
        }

        private void cmdCompanyClearSelection_Click(object sender, EventArgs e)
        {
            if (gvCompany.RowCount < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Updating account list...");
            for (int i = 0; i < gvCompany.RowCount; i++)
                gvCompany.SetRowCellValue(i, "selected", false);

            WaitDialog.Close();
        }

        private void cmdCompanySelectAll_Click(object sender, EventArgs e)
        {
            if (gvCompany.RowCount < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Updating account list...");
            for (int i = 0; i < gvCompany.RowCount; i++)
                gvCompany.SetRowCellValue(i, "selected", true);

            WaitDialog.Close();
        }

        private void cmdCompanySaveChanges_Click(object sender, EventArgs e)
        {
            if (m_ModifiedAccounts == null)
                return;

            if (m_ModifiedAccounts.Count < 1)
                return;
            
            WaitDialog.Show(ParentForm, "Saving account list...");
            this.SaveQueuedAccounts();
            WaitDialog.Close();
        }

        private void gvContact_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            this.GetFocusedContact();
        }

        private void gvContact_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName.Equals("selected"))
                return;

            this.QueueUpdatedCallListContacts(e.RowHandle);
        }

        private void gvCompany_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            this.GetFocusedAccount();
        }

        private void gvCompany_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName.Equals("selected") || e.Column.FieldName.Equals("modified"))
                return;

            this.QueueUpdatedCallListAccounts(e.RowHandle);
        }

        private void cboSubCampaign_EditValueChanged(object sender, EventArgs e)
        {
            if (cboSubCampaign.EditValue == null || !m_DoneLoadingCampaign)
                return;

            if (Convert.ToInt32(cboSubCampaign.EditValue) < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Loading components...");
            ObjectSubCampaign.SubCampaignModificationInfo item = ObjectSubCampaign.GetSubCampaignModificationInfo((int)cboSubCampaign.EditValue);
            lblSubCampaignDetail.Text = String.Format("Creation: {0}     Modification: {1}", item.creation_info, item.modification_info);
            this.PopulateCompanies();
            this.PopulateContacts();
            WaitDialog.Close();
        }

        private void cboCampaign_EditValueChanged(object sender, EventArgs e)
        {
            if (cboCampaign.EditValue == null || !m_DoneLoadingCustomer)
                return;

            
            WaitDialog.Show(ParentForm, "Loading sub campaigns...");
            m_DoneLoadingCampaign = false;
            this.PopulateSubCampaignComboList();
            m_DoneLoadingCampaign = true;
            WaitDialog.Close();
        }

        private void cboCustomer_EditValueChanged(object sender, EventArgs e)
        {
            if (cboCustomer.EditValue == null)
                return;

            
            WaitDialog.Show(ParentForm, "Loading campaigns...");
            this.PopulateCampaignComboList();
            WaitDialog.Close();
        }

        private void chkContactSelected_EditValueChanging(object sender, ChangingEventArgs e)
        {
            if (m_SelectedContact.id < 1)
                e.Cancel = true;
        }
        #endregion

        #region Public Methods
        public void AddSubCampaignCompaniesAndContacts(List<int> lstAccountIds)
        {
            /**
             * process accounts and contacts to be added to sub-campaign
             */
            BrightPlatformEntities _efDbmodel = new BrightPlatformEntities(UserSession.EntityConnection);
            List<int> _ToAddAcctIds = new List<int>();
            List<int> _ToAddContactIds = new List<int>();

            foreach (int _AcctId in lstAccountIds)
            {
                if (m_lstAccounts.Find(i => i.account_id == _AcctId) == null)
                    _ToAddAcctIds.Add(_AcctId);

                /**
                 * validate and get to be added contacts
                 */
                List<int> _lstContactIds =
                (
                    from _efAcctContact in _efDbmodel.account_contacts
                    where _efAcctContact.account_id == _AcctId
                    select _efAcctContact.contact_id

                ).ToList();

                foreach (int _ContactId in _lstContactIds)
                {
                    if (m_lstContacts.Find(i => i.account_id == _AcctId && i.contact_id == _ContactId) == null)
                        if (_efDbmodel.contacts.FirstOrDefault(i => i.id == _ContactId && i.active) != null)
                            _ToAddContactIds.Add(_ContactId);
                }
            }

            /**
             * save accounts and contacts to sub_campaign
             */
            ObjectCallList.AddCompaniesToSubCampaign(_ToAddAcctIds, (int)cboSubCampaign.EditValue, "Manually Added");
            ObjectCallList.AddContactsToSubCampaign(_ToAddContactIds, (int)cboSubCampaign.EditValue);

            ObjectSubCampaign.SubCampaignModificationInfo _item = ObjectSubCampaign.GetSubCampaignModificationInfo((int)cboSubCampaign.EditValue);
            lblSubCampaignDetail.Text = "Creation: " + _item.creation_info + "     Modification: " + _item.modification_info;
            this.PopulateCompanies();
            this.PopulateContacts();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Set the account list modified flags
        /// </summary>
        private void SetAccountModifiedFlag()
        {
            if (m_ModifiedRowHandleAccounts.Count < 1)
                return;

            foreach (int row_handle in m_ModifiedRowHandleAccounts)
                gvCompany.SetRowCellValue(row_handle, "modified", "Yes");
        }

        /// <summary>
        /// Update the contact list priorities
        /// </summary>
        /// <param name="Priority"></param>
        private void UpdateContactPriorities(string Priority)
        {
            if (gvContact.RowCount < 1)
                return;

            /**
             * we need to get all the lists first and queue this items.
             */
            #region Code Logic
            List<CTFinalCallListContact> _lstContacts = new List<CTFinalCallListContact>();
            for (int i = 0; i < gvContact.RowCount; i++)
            {
                CTFinalCallListContact _item = gvContact.GetRow(i) as CTFinalCallListContact;
                if (Convert.ToBoolean(_item.selected))
                {
                    _item.priority = Priority;
                    _lstContacts.Add(_item);
                }
            }
            this.QueueUpdatedCallListContacts(_lstContacts);
            #endregion

            CTFinalCallListContact item = new CTFinalCallListContact();
            for (int i = 0; i < gvContact.RowCount; i++)
            {
                item = gvContact.GetRow(i) as CTFinalCallListContact;
                if (Convert.ToBoolean(item.selected) && item.id > 0)
                    gvContact.SetRowCellValue(i, "priority", Priority);
            }
        }

        /// <summary>
        /// Update the account list priorities
        /// </summary>
        /// <param name="Priority"></param>
        private void UpdateAccountPriorities(string Priority)
        {
            if (gvCompany.RowCount < 1)
                return;

            /**
             * we need to get all the lists first and queue this items.
             */
            #region Code Logic
            List<CTFinalCallListAccount> _lstAccounts = new List<CTFinalCallListAccount>();
            for (int i = 0; i < gvCompany.RowCount; i++)
            {
                CTFinalCallListAccount _item = gvCompany.GetRow(i) as CTFinalCallListAccount;
                if (Convert.ToBoolean(_item.selected))
                {
                    _item.priority = Priority;
                    _lstAccounts.Add(_item);
                }
            }
            this.QueueUpdatedCallListAccounts(_lstAccounts);
            #endregion

            CTFinalCallListAccount item = new CTFinalCallListAccount();
            for (int i = 0; i < gvCompany.RowCount; i++)
            {
                item = gvCompany.GetRow(i) as CTFinalCallListAccount;
                if (Convert.ToBoolean(item.selected))
                    gvCompany.SetRowCellValue(i, "priority", Priority);
            }
        }

        /// <summary>
        /// Load the companies of a sub campaign
        /// </summary>
        private void PopulateCompanies()
        {
            try
            {
                m_lstAccounts = null;
                gcCompany.DataSource = null;
                m_lstAccounts = ObjectCallList.GetFinalCallListAccounts((int)cboSubCampaign.EditValue);
                gcCompany.DataSource = m_lstAccounts;
                gvCompany.Columns["org_no"].Visible = false;
                gvCompany.Columns["box_address"].Visible = false;
                gvCompany.Columns["street_address"].Visible = false;
                gvCompany.Columns["zipcode"].Visible = false;
                gvCompany.Columns["country"].Visible = false;
                gvCompany.Columns["county"].Visible = false;
                gvCompany.Columns["municipality"].Visible = false;
                gvCompany.Columns["city"].Visible = false;
                gvCompany.Columns["telephone"].Visible = false;
                gvCompany.Columns["telefax"].Visible = false;
                gvCompany.Columns["www"].Visible = false;
                gvCompany.Columns["parent_company"].Visible = false;
                gvCompany.Columns["year_established"].Visible = false;
                gvCompany.Columns["activity_code"].Visible = false;
                gvCompany.Columns["activity_code_2"].Visible = false;
                gvCompany.Columns["currency"].Visible = false;
                gvCompany.Columns["fiscal"].Visible = false;
                gvCompany.Columns["turnover"].Visible = false;
                gvCompany.Columns["export"].Visible = false;
                gvCompany.Columns["result"].Visible = false;
                gvCompany.Columns["sales_abroad"].Visible = false;
                gvCompany.Columns["employees_total"].Visible = false;
                gvCompany.Columns["employees_abroad"].Visible = false;
                gvCompany.Columns["fiscal_2"].Visible = false;
                gvCompany.Columns["turnover_2"].Visible = false;
                gvCompany.Columns["export_2"].Visible = false;
                gvCompany.Columns["result_2"].Visible = false;
                gvCompany.Columns["sales_abroad_2"].Visible = false;
                gvCompany.Columns["employees_total_2"].Visible = false;
                gvCompany.Columns["employees_abroad_2"].Visible = false;
                gvCompany.Columns["fiscal_3"].Visible = false;
                gvCompany.Columns["turnover_3"].Visible = false;
                gvCompany.Columns["export_3"].Visible = false;
                gvCompany.Columns["result_3"].Visible = false;
                gvCompany.Columns["sales_abroad_3"].Visible = false;
                gvCompany.Columns["employees_total_3"].Visible = false;
                gvCompany.Columns["employees_abroad_3"].Visible = false;
                gvCompany.Columns["category"].Visible = false;
                gvCompany.Columns["bv_source"].Visible = false;
                gvCompany.Columns["regions"].Visible = false;
                gvCompany.BestFitColumns();
                gvCompany.Focus();
                lblCompanyRecordStat.Text = "     Records: " + m_lstAccounts.Count.ToString();

                if (m_lstAccounts.Count > 0)
                {
                    this.PopulateSubCampaignUserComboList();
                    lcFinalListAccount.Enabled = true;
                }
                else
                {
                    cboSalesUser.Properties.Columns.Clear();
                    cboSalesUser.Properties.DataSource = null;
                    lcFinalListAccount.Enabled = false;
                }
            }
            catch { }
        }

        /// <summary>
        /// Load the contacts of a sub campaign
        /// </summary>
        private void PopulateContacts()
        {
            try
            {
                m_lstContacts = null;
                gcContact.DataSource = null;
                m_lstContacts = ObjectCallList.GetFinalCallListContacts((int)cboSubCampaign.EditValue);
                gcContact.DataSource = m_lstContacts;
                gvContact.Columns["middle_name"].Visible = false;
                gvContact.Columns["direct_phone"].Visible = false;
                gvContact.Columns["mobile"].Visible = false;
                gvContact.Columns["email"].Visible = false;
                gvContact.Columns["address_1"].Visible = false;
                gvContact.Columns["address_2"].Visible = false;
                gvContact.Columns["zipcode"].Visible = false;
                gvContact.Columns["country"].Visible = false;
                gvContact.Columns["role"].Visible = false;
                gvContact.Columns["profile_title"].Visible = false;
                gvContact.Columns["email_verified"].Visible = false;
                gvContact.BestFitColumns();
                gvContact.Focus();
                lblContactRecordStat.Text = string.Format("     Records: {0}", m_lstContacts.Count);

                if (m_lstContacts.Count > 0)
                    lcFinalListContact.Enabled = true;
                else
                    lcFinalListContact.Enabled = false;
            }
            catch {
                return;
            }
        }

        /// <summary>
        /// Save updated call list contacts
        /// </summary>
        private void SaveQueuedContacts()
        {
            ObjectCallList.SaveFinalCallListContacts(m_ModifiedContacts, (int)cboSubCampaign.EditValue);
            ObjectSubCampaign.SubCampaignModificationInfo item = ObjectSubCampaign.GetSubCampaignModificationInfo((int)cboSubCampaign.EditValue);
            lblSubCampaignDetail.Text = "Creation: " + item.creation_info + "     Modification: " + item.modification_info;
            m_ModifiedRowHandleContacts = null;
            MessageBox.Show("Successfully updated final call list contacts.", "Final Call List Contacts", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Save updated call list accounts
        /// </summary>
        private void SaveQueuedAccounts()
        {
            ObjectCallList.SaveFinalCallListAccounts(m_ModifiedAccounts, (int)cboSubCampaign.EditValue);
            this.SetAccountModifiedFlag();
            ObjectSubCampaign.SubCampaignModificationInfo item = ObjectSubCampaign.GetSubCampaignModificationInfo((int)cboSubCampaign.EditValue);
            lblSubCampaignDetail.Text = String.Format("Creation: {0}     Modification: {1}", item.creation_info, item.modification_info);
            m_ModifiedAccounts = null;
            m_ModifiedRowHandleAccounts = null;
            MessageBox.Show("Successfully updated final call list accounts.", "Final Call List Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Searches the list of existing objects, and then update, if not, add to list, for the purpose of batch update
        /// </summary>
        private void QueueUpdatedCallListContacts(int SelectedRowHandle)
        {
            if (m_SelectedContact == null)
                return;

            if (m_ModifiedContacts == null)
                m_ModifiedContacts = new List<CTFinalCallListContact>();

            CTFinalCallListContact item = gvContact.GetRow(SelectedRowHandle) as CTFinalCallListContact;
            if (!m_ModifiedContacts.Exists(i => i.id == item.id))
                m_ModifiedContacts.Add(item);

            if (m_ModifiedRowHandleContacts == null)
                m_ModifiedRowHandleContacts = new List<int>();

            if (!m_ModifiedRowHandleContacts.Exists(i => i == SelectedRowHandle))
                m_ModifiedRowHandleContacts.Add(SelectedRowHandle);
        }
        private void QueueUpdatedCallListContacts(List<CTFinalCallListContact> lstContacts)
        {
            if (lstContacts.Count < 1)
                return;

            if (m_ModifiedContacts == null)
                m_ModifiedContacts = new List<CTFinalCallListContact>();

            foreach (CTFinalCallListContact item in lstContacts)
                if (!m_ModifiedContacts.Exists(i => i.id == item.id))
                    m_ModifiedContacts.Add(item);
        }

        /// <summary>
        /// Searches the list of existing objects, and then update, if not, add to list, for the purpose of batch update
        /// </summary>
        private void QueueUpdatedCallListAccounts(int SelectedRowHandle)
        {
            if (m_SelectedAccount == null)
                return;

            if (m_ModifiedAccounts == null)
                m_ModifiedAccounts = new List<CTFinalCallListAccount>();

            CTFinalCallListAccount item = gvCompany.GetRow(SelectedRowHandle) as CTFinalCallListAccount;
            if (item != null)
            {
                if (!m_ModifiedAccounts.Exists(i => i.id == item.id))
                    m_ModifiedAccounts.Add(item);
            }
            
            if (m_ModifiedRowHandleAccounts == null)
                m_ModifiedRowHandleAccounts = new List<int>();

            if (!m_ModifiedRowHandleAccounts.Exists(i => i == SelectedRowHandle))
                m_ModifiedRowHandleAccounts.Add(SelectedRowHandle);
        }
        private void QueueUpdatedCallListAccounts(List<CTFinalCallListAccount> lstAccounts)
        {
            if (lstAccounts.Count < 1)
                return;

            if (m_ModifiedAccounts == null)
                m_ModifiedAccounts = new List<CTFinalCallListAccount>();

            foreach (CTFinalCallListAccount item in lstAccounts)
                if (!m_ModifiedAccounts.Exists(i => i.id == item.id))
                    m_ModifiedAccounts.Add(item);
        }

        /// <summary>
        /// Sets the focused view instance of the grid
        /// </summary>
        private void GetFocusedAccount()
        {
            m_SelectedAccount = gvCompany.GetFocusedRow() as CTFinalCallListAccount;
        }

        /// <summary>
        /// Sets the focused view instance of the grid
        /// </summary>
        private void GetFocusedContact()
        {
            m_SelectedContact = gvContact.GetFocusedRow() as CTFinalCallListContact;
        }

        /// <summary>
        /// Populate customer combo box
        /// </summary>
        private void PopulateCustomerComboList()
        {
            try
            {
                m_DoneLoadingCustomer = false;
                cboCampaign.Properties.Columns.Clear();
                cboCampaign.Properties.DataSource = null;
                cboSubCampaign.Properties.Columns.Clear();
                cboSubCampaign.Properties.DataSource = null;
                gcCompany.DataSource = null;
                gcContact.DataSource = null;
                lcFinalListAccount.Enabled = false;
                lcFinalListContact.Enabled = false;
                cboCustomer.Properties.DataSource = null;
                cboCustomer.Properties.DataSource = ObjectCustomer.GetCustomers(ObjectCustomer.eViewType.ComboListView).Execute(MergeOption.AppendOnly);
                cboCustomer.Properties.DisplayMember = "customer_name";
                cboCustomer.Properties.ValueMember = "id";
                cboCustomer.Properties.Columns.Add(new LookUpColumnInfo("customer_name"));
                cboCustomer.Properties.ShowHeader = false;
                cboCustomer.Properties.ShowFooter = false;
                cboCustomer.EditValue = 0;
                m_DoneLoadingCustomer = true;
            }
            catch { }
        }

        /// <summary>
        /// Populate campaign combo box
        /// </summary>
        private void PopulateCampaignComboList()
        {
            if (cboCustomer.EditValue == null)
                return;

            try
            {
                cboSubCampaign.Properties.Columns.Clear();
                cboSubCampaign.Properties.DataSource = null;
                gcCompany.DataSource = null;
                gcContact.DataSource = null;
                lcFinalListAccount.Enabled = false;
                lcFinalListContact.Enabled = false;
                cboCampaign.Properties.Columns.Clear();
                cboCampaign.Properties.DataSource = null;
                cboCampaign.Properties.DataSource = ObjectCampaign.GetCampaigns(ObjectCampaign.eViewtype.ComboListViewByCustomerId, (int)cboCustomer.EditValue).Execute(MergeOption.AppendOnly);
                cboCampaign.Properties.DisplayMember = "name";
                cboCampaign.Properties.ValueMember = "id";
                cboCampaign.Properties.Columns.Add(new LookUpColumnInfo("name"));
                cboCampaign.Properties.ShowHeader = false;
                cboCampaign.Properties.ShowFooter = false;
                cboCampaign.EditValue = 0;
            }
            catch { }
        }

        /// <summary>
        /// Populate sub campaign combo box
        /// </summary>
        private void PopulateSubCampaignComboList()
        {
            if (cboCampaign.EditValue == null)
                return;

            try
            {
                gcCompany.DataSource = null;
                gcContact.DataSource = null;
                lcFinalListAccount.Enabled = false;
                lcFinalListContact.Enabled = false;
                cboSubCampaign.Properties.Columns.Clear();
                cboSubCampaign.Properties.DataSource = null;
                cboSubCampaign.Properties.DataSource = ObjectSubCampaign.GetFinalCallListSubCampaigns((int)cboCampaign.EditValue, (int)cboCustomer.EditValue).Execute(MergeOption.AppendOnly);
                cboSubCampaign.Properties.DisplayMember = "title";
                cboSubCampaign.Properties.ValueMember = "id";
                cboSubCampaign.Properties.Columns.Add(new LookUpColumnInfo("title"));
                cboSubCampaign.EditValue = 0;
                m_DoneLoadingSubCampaign = true;
            }
            catch { }
        }

        /// <summary>
        /// Populate sub campaign users combo box
        /// </summary>
        private void PopulateSubCampaignUserComboList()
        {
            if (cboSubCampaign.EditValue == null)
                return;

            try
            {
                m_DoneLoadingSubCampaign = false;
                cboSalesUser.Properties.Columns.Clear();
                cboSalesUser.Properties.DataSource = null;
                cboSalesUser.Properties.DataSource = ObjectUser.GetSubCampaignSalesUsers((int)cboSubCampaign.EditValue).Execute(MergeOption.AppendOnly);
                cboSalesUser.Properties.DisplayMember = "name";
                cboSalesUser.Properties.ValueMember = "id";
                cboSalesUser.Properties.Columns.Add(new LookUpColumnInfo("name"));
                cboSalesUser.EditValue = 0;
                m_DoneLoadingSubCampaign = true;
            }
            catch { }
        }
        #endregion
    }
}
