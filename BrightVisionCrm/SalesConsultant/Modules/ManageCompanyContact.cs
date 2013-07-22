
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
using System.Linq.Expressions;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
//using BrightVision.DQControl.UI;
using SalesConsultant.Business;
using SalesConsultant.Forms;

using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using DevExpress.Data.Filtering;
using BrightVision.DQControl.UI;
//using DevExpress.XtraVerticalGrid.Editors;
//using DevExpress.XtraEditors.Controls;
using BrightVision.Common.UI;
#endregion

namespace SalesConsultant.Modules
{
    public partial class ManageCompanyContact : UserControl
    {
        #region Enumerations
        /// <summary>
        /// Enum for view type
        /// </summary>
        private enum eViewType
        {
            Company,
            Contact
        }
        #endregion

        #region Public Properties

        #endregion

        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private GridView m_objGridView = null;
        private CTCompanyList m_objCompany = null;
        //private ObjectContact.ContactInstance m_objContact = null;
        private CTContact m_objContact = null;
        private PopupDialog m_objPopupDialog = null;
        private string m_MessageBoxCaption = "Manager Application - Companies and Contacts";
        //private List<ObjectContact.ContactInstance> m_objContactList = new List<ObjectContact.ContactInstance>();
        private List<CTContact> m_objContactList = new List<CTContact>();
        private List<CTCompanyList> m_objCompanyList = new List<CTCompanyList>();
        //private BrightVision.DQControl.UI.AddContact m_objContactForm = null;
        private AddContact m_objContactForm = null;
        private AddCompany m_objAddCompanyForm = null;
        private bool m_PerformSearch = true;
        private bool m_ResetAccountPaging = true;
        private bool m_ReloadPage = true;
        private int m_SelectedRowIndex = 0;
        private int m_SelectedPageNo = 1;
        private bool m_CompanyViewLoading = false;
        #endregion

        #region Constructors
        public ManageCompanyContact()
        {
            this.Visible = false;
            InitializeComponent();
            #region Add Title Dropdown
            RepositoryItemBVPopupContainerEdit repositoryItemComboBoxAuto = new RepositoryItemBVPopupContainerEdit();
            repositoryItemComboBoxAuto.BorderStyle = BorderStyles.NoBorder;
            repositoryItemComboBoxAuto.Appearance.BorderColor = Color.Transparent;
            repositoryItemComboBoxAuto.Validating += new CancelEventHandler(repositoryItemComboBoxAuto_Validating);
            gcContact.RepositoryItems.Add(repositoryItemComboBoxAuto);
            gvContact.Columns["title"].ColumnEdit = repositoryItemComboBoxAuto;
            #endregion
            this.lcManageCompanyContact.AllowCustomizationMenu = false;
            this.PopulateCompanyView(null, 1);
            this.SetCompanyViewContextMenu();
            this.SetContactViewContextMenu();
            this.Visible = true;
        }
        #endregion

        #region Object Control Events
        private void gvContact_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName.Equals("title"))
            {
                var val = gvContact.GetRowCellValue(e.RowHandle, "title");
                if (val != null && val.ToString() != "")
                {
                    var edit = e.Column.ColumnEdit as RepositoryItemBVPopupContainerEdit;
                    var data = edit.PopupControl.Tag as BVPopupContainerControlData;
                    bool hasMatch = data.MatchKeyword(val.ToString().Trim());
                    if (hasMatch)
                    {
                        e.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
                        e.Appearance.BackColor = Color.FromArgb(181, 245, 146);//green
                        e.Appearance.BackColor2 = Color.FromArgb(181, 245, 146);//green
                    }
                    else
                    {
                        e.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                        e.Appearance.BackColor = Color.FromArgb(244, 102, 102);//red
                        e.Appearance.BackColor2 = Color.FromArgb(244, 102, 102);//red
                    }
                }
                else
                {
                    e.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
                }
            }
        }
        private void repositoryItemComboBoxAuto_Validating(object sender, CancelEventArgs e)
        {
            var ctl = sender as DevExpress.XtraEditors.PopupContainerEdit;
            if (ctl != null)
            {
                if (ctl.Text.Trim() == string.Empty)
                {
                    ctl.Properties.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                    ctl.Properties.Appearance.BackColor = Color.FromArgb(244, 102, 102);
                }
                else
                {
                    var data = ctl.Properties.PopupControl.Tag as BVPopupContainerControlData;
                    if (data != null && data.HasMatch)
                    {
                        ctl.Properties.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
                        ctl.Properties.Appearance.BackColor = Color.FromArgb(181, 245, 146);
                    }
                    else
                    {
                        ctl.Properties.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                        ctl.Properties.Appearance.BackColor = Color.FromArgb(244, 102, 102);
                    }
                }
            }

            
        }
        private void gvCompany_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (m_CompanyViewLoading || e.FocusedRowHandle < 0)
                return;

            m_SelectedRowIndex = e.FocusedRowHandle;
            this.SetFocusedViewInstance(eViewType.Company);
            if (m_objCompany != null)
                this.PopulateContactView(m_objCompany.id);
            else
                gcContact.DataSource = null;
        }

        private void gvContact_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (!gvContact.IsNewItemRow(e.RowHandle))
            {
                this.SetFocusedViewInstance(eViewType.Contact);
                this.QueueUpdatedContacts();
            }
        }

        private void gvContact_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            this.SetFocusedViewInstance(eViewType.Contact);
        }

        private void cmdSaveChangesCompany_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Saving data...");
            if (m_objCompanyList.Count > 0) {
                this.SaveQueuedCompanies();
                WaitDialog.Close();
                MessageBox.Show("Successfully updated companies", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else {
                WaitDialog.Close();
                MessageBox.Show("No records to update", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cmdSaveChangesContact_Click(object sender, EventArgs e)
        {
            if (gvContact.RowCount < 1 || m_objContactList.Count < 1) {
                MessageBox.Show("No records to update", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            WaitDialog.Show("Saving data...");
            this.SaveQueuedContacts();
            WaitDialog.Close();
        }

        private void cmdAddContact_Click(object sender, EventArgs e)
        {
            this.DisplayContactForm(true);
        }

        private void cmdAddCompany_Click(object sender, EventArgs e)
        {
            this.DisplayCompanyForm();
        }

        private void gvCompany_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (!gvCompany.IsNewItemRow(e.RowHandle))
            {
                this.SetFocusedViewInstance(eViewType.Company);
                this.QueueUpdatedCompanies();
                gvContact.BestFitColumns();
            }
        }

        private void miCompanyPrintPreview_Click(object sender, EventArgs e)
        {
            gcCompany.ShowPrintPreview();
        }

        private void miContactPrintPreview_Click(object sender, EventArgs e)
        {
            gcContact.ShowPrintPreview();
        }
             
        private void gvCompany_ColumnFilterChanged(object sender, EventArgs e)
        {
            // filter to avoid executed twice
            if (m_PerformSearch)
            {
                WaitDialog.Show(ParentForm, "Loading...");
                this.PerformCustomSearch(1);
                m_SelectedRowIndex = gvCompany.FocusedRowHandle;
                this.SetFocusedViewInstance(eViewType.Company);
                if (m_objCompany != null)
                    this.PopulateContactView(m_objCompany.id);
               WaitDialog.Close();
            }
        }

        private void cboPagingAccount_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_ReloadPage)
            {
                WaitDialog.Show(ParentForm, "Loading...");
                m_SelectedPageNo = Convert.ToInt32(cboPagingAccount.Text);
                m_ResetAccountPaging = false;

                if (!string.IsNullOrEmpty(gvCompany.FindFilterText))
                {
                    m_PerformSearch = false;
                    this.PopulateCompanyView(gvCompany.FindFilterText, m_SelectedPageNo);
                }
                else
                    this.PerformCustomSearch(m_SelectedPageNo);

               WaitDialog.Close();
            }        
        }

        private void gvCompany_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.Equals("date_created") || e.Column.FieldName.Equals("date_modified"))
                e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyyy-MM-dd");
        }

        private void gvContact_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.Equals("date_created") || e.Column.FieldName.Equals("date_modified"))
                e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyyy-MM-dd");
        }

        private void gvCompany_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvContact_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Pupulates the company grid view contents
        /// </summary>
        public void PopulateCompanyView(string FilterCriteria, int PageNo, bool DefaultSelectNewCompany = false)
        {
            try
            {
                m_CompanyViewLoading = true;
                gcCompany.DataSource = null;
                gcContact.DataSource = null;
                gcCompany.DataSource = ObjectCompany.GetCompanies(DatabaseUtility.LargeDatasetFetchLimit, FilterCriteria, PageNo);

                if (DefaultSelectNewCompany)
                    this.SetDefaultSelectedRow(FilterCriteria);
                else
                    this.SetDefaultSelectedRow();

                if (m_ResetAccountPaging)
                    this.SetAccountPagination();

                m_ResetAccountPaging = true;
                m_PerformSearch = true;
                m_CompanyViewLoading = false;
               
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Pupulates the contacts grid view contents
        /// </summary>
        public void PopulateContactView(int AccountId)
        {
            try
            {
                gcContact.DataSource = null;
                gcContact.DataSource = ObjectContact.GetContacts(AccountId);
                //gcContact.DataSource = ObjectContact.GetContacts(AccountId).Execute(MergeOption.AppendOnly);
                gvContact.BestFitColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Set the default selected row
        /// </summary>
        private void SetDefaultSelectedRow(string DefaultSelectedCompanyName = null)
        {
            if (string.IsNullOrEmpty(DefaultSelectedCompanyName))
            {
                if (m_SelectedRowIndex < 1 || m_SelectedRowIndex > gvCompany.RowCount)
                    return;

                gvCompany.FocusedRowHandle = m_SelectedRowIndex;
            }
            else
            {
                if (gvCompany.RowCount < 1)
                    return;

                for (int i = 0; i < gvCompany.RowCount; i++) 
                {
                    if (gvCompany.GetRowCellValue(i, "company_name").ToString().Equals(DefaultSelectedCompanyName)) 
                    {
                        gvCompany.FocusedRowHandle = i;
                        return;
                    }
                }

                gvCompany.FocusedRowHandle = 0;
            }
        }

        /// <summary>
        /// Apply pagination
        /// </summary>
        private void SetAccountPagination()
        {
            CTCompanyCount objAccountPagination = ObjectCompany.GetCompanyPageCount(gvCompany.FindFilterText);
            cboPagingAccount.Properties.Items.Clear();
            if (objAccountPagination.page_count > 1)
            {
                cboPagingAccount.Text = "1";
                for (int i = 1; i <= objAccountPagination.page_count; i++)
                    cboPagingAccount.Properties.Items.Add(i.ToString());
            }

            m_SelectedPageNo = 1;
        }

        /// <summary>
        /// Gets the filter criteria and re populate the customer grid view
        /// </summary>
        private void PerformCustomSearch(int PageNo)
        {
            //CriteriaOperator objFilterCriteria = gvCompany.DataController.FilterCriteria;
            //SqlGenerator objSqlGenerator = new SqlGenerator(objFilterCriteria);
            //string sqlCommandFilter = objSqlGenerator.GetSQLFilter(SqlGenerator.eFilterCategory.AccountsView);
            
            m_ReloadPage = false;
            m_PerformSearch = false;
            this.PopulateCompanyView(gvCompany.FindFilterText, PageNo);
            m_PerformSearch = true;
            m_ReloadPage = true;
        }

        /// <summary>
        /// Initializes the popup dialog for company entry and loads the popup dialog
        /// </summary>
        private void DisplayCompanyForm()
        {
            m_objAddCompanyForm = new AddCompany();
            m_objAddCompanyForm.m_objParentControl = this;
            m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = "Manager Application - Add Company";
            m_objPopupDialog.Controls.Add(m_objAddCompanyForm);
            m_objPopupDialog.ClientSize = new Size(m_objAddCompanyForm.Width + 2, m_objAddCompanyForm.Height + 2);
            m_objPopupDialog.ShowDialog(this.ParentForm);
        }

        /// <summary>
        /// Save queued companies
        /// </summary>
        private void SaveQueuedCompanies()
        {
            if (m_objCompanyList.Count < 1)
                return;

            ObjectCompany.SaveCompanies(m_objCompanyList, BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Company_Contact);
            


            //https://brightvision.jira.com/browse/PLATFORM-2698
            //DAN: Added to update the date_modified column in the grid

            GridView view = gcCompany.FocusedView as GridView;

            foreach (CTCompanyList iCompany in m_objCompanyList)
            {
                for (int i = 0; i < gvCompany.RowCount; i++)
                {
                    CTCompanyList _item = gvCompany.GetRow(i) as CTCompanyList;

                    if (_item.id == iCompany.id)
                    {
                        _item.date_modified = DateTime.Now;
                        gvCompany.RefreshRow(i);
                        break;
                    }
                }
            }

            //this.PopulateCompanyView(null, m_SelectedPageNo);

            m_objCompanyList.Clear();
        }

        /// <summary>
        /// Save queued contacts
        /// </summary>
        private void SaveQueuedContacts()
        {
            if (m_objContactList.Count < 1)
                return;

            ObjectContact.SaveContacts(m_objContactList, m_objCompany.id, BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Company_Contact);

            //https://brightvision.jira.com/browse/PLATFORM-2698
            //DAN: Added to update the date_modified column in the grid

            GridView view = gcContact.FocusedView as GridView;

            foreach (CTContact iContact in m_objContactList)
            {
                for (int i = 0; i < gvContact.RowCount; i++)
                {
                    CTContact _item = gvContact.GetRow(i) as CTContact;

                    if (_item.id == iContact.id)
                    {
                        _item.date_modified = DateTime.Now;
                        gvContact.RefreshRow(i);
                        break;
                    }
                }
            }

            m_objContactList.Clear();
           

            //this.PopulateContactView(m_objCompany.id);
            MessageBox.Show("Successfully updated contacts", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Searches the list of existing objects, and then update, if not, add to list, for the purpose of batch update
        /// </summary>
        private void QueueUpdatedContacts()
        {
            try
            {
                if (!m_objContactList.Exists(i => i.id == (m_objContact == null ? 0 : m_objContact.id)))
                    m_objContactList.Add(m_objContact);
            }
            catch { }
        }

        /// <summary>
        /// Searches the list of existing objects, and then update, if not, add to list, for the purpose of batch update
        /// </summary>
        private void QueueUpdatedCompanies()
        {
            try
            {
                if (!m_objCompanyList.Exists(i => i.id == (m_objCompany == null ? 0 : m_objCompany.id)))
                    m_objCompanyList.Add(m_objCompany);                
            }
            catch { }
        }

        /// <summary>
        /// Initializes the popup dialog for contacts entry
        /// </summary>
        private void DisplayContactForm(bool IsNew)
        {
            if (gvCompany.RowCount > 0)
                this.SetFocusedViewInstance(eViewType.Company);

            if (m_objCompany == null)
                return;

            m_objContactForm = new AddContact(m_objCompany.id);            
            m_objContactForm.StartPosition = FormStartPosition.CenterScreen;
            if (m_objContactForm.ShowDialog(this.ParentForm) == DialogResult.OK) {
                this.PopulateContactView(m_objCompany.id);
                MessageBox.Show("Contact has been saved.", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Sets the focused view instance of the grid. Instantiates the objects that can be used for data manipulation.
        /// </summary>
        private void SetFocusedViewInstance(eViewType ViewType)
        {
            m_objGridView = null;
            switch (ViewType)
            {
                case eViewType.Company:
                    {
                        m_objCompany = null;
                        m_objGridView = gcCompany.FocusedView as GridView;
                        m_objCompany = m_objGridView.GetFocusedRow() as CTCompanyList;
                        break;
                    }

                case eViewType.Contact:
                    {
                        m_objContact = null;
                        m_objGridView = gcContact.FocusedView as GridView;
                        m_objContact = m_objGridView.GetFocusedRow() as CTContact;
                        break;
                    }
            }
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
        private void SetCompanyViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miCompanyPrintPreview = new MenuItem("Print Preview");
            miCompanyPrintPreview.Click += new EventHandler(miCompanyPrintPreview_Click);
            objClickMenu.MenuItems.Add(miCompanyPrintPreview);
            gcCompany.ContextMenu = objClickMenu;
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
        private void SetContactViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miContactPrintPreview = new MenuItem("Print Preview");
            miContactPrintPreview.Click += new EventHandler(miContactPrintPreview_Click);
            objClickMenu.MenuItems.Add(miContactPrintPreview);
            gcContact.ContextMenu = objClickMenu;
        }
        #endregion
    }
}
