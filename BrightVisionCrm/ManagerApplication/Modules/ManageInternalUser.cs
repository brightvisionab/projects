
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Linq;
using System.Collections;
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

namespace ManagerApplication.Modules
{
    public partial class ManageInternalUser : DevExpress.XtraEditors.XtraUserControl
    {
        #region Contructors
        public ManageInternalUser()
        {
            this.Visible = false;
            InitializeComponent();
            this.lcControls.AllowCustomizationMenu = false;
            this.PopulateUserGrid();
            this.SetUserViewContextMenu();
            this.Visible = true;
        }
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private ObjectUser.UserInstance m_objUser = null;
        private ObjectUser.UserRoleInstance m_objUserRole = null;
        private ObjectUser.UserCustomerInstance m_objUserCustomer = null;
        private AddUser m_objAddUserForm = null;
        private PopupDialog m_objPopupDialog = null;
        private DialogResult m_objDialogResult;
        #endregion

        #region Object Events
        private void chkIsActive_CheckStateChanged(object sender, EventArgs e)
        {
            CheckEdit objCheckBox = sender as CheckEdit;
            if (objCheckBox.Checked) {
                this.SetUserStatus(true);
                MessageBox.Show(String.Format("User {0} successfully activated!", m_objUser.full_name), "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else {
                this.SetUserStatus(false);
                MessageBox.Show(String.Format("User {0} successfully de-activated!", m_objUser.full_name), "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void cbxShowActiveUser_CheckedChanged(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Filtering items.");
            this.UserViewFilter();
            WaitDialog.Close();
        }

        private void cmdSaveComment_Click(object sender, EventArgs e)
        {
            if (gridViewUsers.RowCount < 1)
                return;

            this.SetFocusedViewInstance();
            this.SaveComment();
        }        
        private void cmdAdd_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading form.");
            m_objAddUserForm = new AddUser(AddUser.SaveType.SaveTypeEdit, m_objUser, true) {
                IsNew = false, // since there is no adding of internal user anymore, only editing ...
                UserType = AddUser.eUserType.InternalUser,
                objInternalUserControl = this
            };
            m_objAddUserForm.InitializeModule();

            m_objPopupDialog = new PopupDialog() {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Manager Application - Users",
                ClientSize = new Size(m_objAddUserForm.Width + 2, m_objAddUserForm.Height + 2)
            };
            m_objPopupDialog.Controls.Add(m_objAddUserForm);

            WaitDialog.Close();
            m_objPopupDialog.ShowDialog(this.ParentForm);
            gridViewUsers.SelectRow("id", m_objAddUserForm.UserId);
            
        }
        private void btnEditUser_Click(object sender, EventArgs e)
        {
            if (gridViewUsers.RowCount < 1)
                return;

            WaitDialog.Show("Loading form.");
            this.SetFocusedViewInstance();
            m_objAddUserForm = new AddUser(AddUser.SaveType.SaveTypeEdit, m_objUser, true) {
                IsNew = false, // since there is no adding of internal user anymore, only editing ...
                UserType = AddUser.eUserType.InternalUser,
                objInternalUserControl = this
            };
            m_objAddUserForm.InitializeModule();

            m_objPopupDialog = new PopupDialog() {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Manager Application - Users",
                ClientSize = new Size(m_objAddUserForm.Width + 2, m_objAddUserForm.Height + 2)
            };
            m_objPopupDialog.Controls.Add(m_objAddUserForm);

            WaitDialog.Close();
            m_objPopupDialog.ShowDialog(this.ParentForm);          
            gridViewUsers.SelectRow("id", m_objAddUserForm.UserId);
        }

        private void gridViewUsers_DoubleClick(object sender, EventArgs e)
        {
            if (gridViewUsers.RowCount < 1)
                return;

            WaitDialog.Show("Loading form.");
            this.SetFocusedViewInstance();
            m_objAddUserForm = new AddUser(AddUser.SaveType.SaveTypeEdit, m_objUser, true) {
                IsNew = false, // since there is no adding of internal user anymore, only editing ...
                UserType = AddUser.eUserType.InternalUser,
                objInternalUserControl = this
            };
            m_objAddUserForm.InitializeModule();

            m_objPopupDialog = new PopupDialog() {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Manager Application - Users",
                ClientSize = new Size(m_objAddUserForm.Width + 2, m_objAddUserForm.Height + 2)
            };
            m_objPopupDialog.Controls.Add(m_objAddUserForm);

            WaitDialog.Close();
            m_objPopupDialog.ShowDialog(this.ParentForm);
        }        
        private void gridViewRoles_Click(object sender, EventArgs e)
        {
            if (gridViewUsers.RowCount < 1)
                return;

            this.SetFocusedViewInstance();
        }
        private void gridViewRoles_KeyUp(object sender, KeyEventArgs e)
        {
            if (gridViewUsers.RowCount < 1)
                return;

            this.SetFocusedViewInstance();
        }
        private void gridViewUsers_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            this.RenderComment();
            this.PopulateUserRolesView();
            this.PopulateUserCustomersView();
        }
        private void gridViewCustomers_ColumnFilterChanged(object sender, EventArgs e)
        {
            if (gridViewCustomers.FindFilterText.Length < 1) {
                WaitDialog.Show(ParentForm, "Reloading.");
                this.PopulateUserCustomersView();
                WaitDialog.Close();
            }
        }
        private void gridViewUsers_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void riChkEnableRole_CheckStateChanged(object sender, EventArgs e)
        {
            this.SetFocusedViewInstance();
            CheckEdit objCheckBox = sender as CheckEdit;
            if (objCheckBox.Checked)
                this.SetUserRoleStatus(true);
            else
                this.SetUserRoleStatus(false);
        }
        private void riChkCustomerEnable_CheckStateChanged(object sender, EventArgs e)
        {
            CheckEdit objCheckBox = sender as CheckEdit;
            this.SetFocusedViewInstance();
            this.SetUserCustomerStatus(objCheckBox.Checked);
            gridViewCustomers.RefreshData();
        }
        private void riChkIsInteralUser_CheckStateChanged(object sender, EventArgs e)
        {
            CheckEdit objCheckBox = sender as CheckEdit;
            if (objCheckBox.Checked) {
                this.SetUserType(true);
                MessageBox.Show(String.Format("User {0} updated to internal user!", m_objUser.full_name), "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else {
                this.SetUserType(false);
                MessageBox.Show(String.Format("User {0} updated to customer user!", m_objUser.full_name), "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void miPrintPreview_Click(object sender, EventArgs e)
        {
            DXGridUser.ShowPrintPreview();
        }

        private void btnDeleteSipAccount_Click(object sender, EventArgs e)
        {
            if (gridViewUsers.RowCount < 1)
                return;

            ObjectUser.UserInstance _item = gridViewUsers.GetFocusedRow() as ObjectUser.UserInstance;
            if (_item == null)
                return;

            user _efeUser = m_objBrightPlatformEntity.users.FirstOrDefault(i => i.id == _item.id);
            _efeUser.sip_id = null;
            m_objBrightPlatformEntity.users.ApplyCurrentValues(_efeUser);
            m_objBrightPlatformEntity.SaveChanges();
            gridViewUsers.SetRowCellValue(gridViewUsers.FocusedRowHandle, "sip_id", null);
        }
        private void rILookUpEditSIP_EditValueChanged(object sender, EventArgs e)
        {
            int newValue = (int)(sender as LookUpEdit).EditValue;
            int handle = gridViewUsers.FocusedRowHandle;
            int userid = (int)gridViewUsers.GetRowCellValue(handle, "id");
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var user = objDbModel.users.Where(b => b.id == userid).FirstOrDefault();
            if (user != null)
            {
                user.sip_id = newValue;
            }

            try
            {
                objDbModel.SaveChanges();
            }
            catch
            {
                return;
            }
        }

        private void tbxPassword_Leave(object sender, EventArgs e)
        {
            TextEdit _control = sender as TextEdit;
            if (gridViewUsers.RowCount < 1 || _control.Text.Length < 1)
                return;

            WaitDialog.Show("Saving data ...");
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                ObjectUser.UserInstance _item = gridViewUsers.GetFocusedRow() as ObjectUser.UserInstance;
                user _efoUser = _efDbContext.users.FirstOrDefault(i => i.id == _item.id);
                _efoUser.password = HashUtility.GetHashPassword(_control.Text);
                _efoUser.modified_by = UserSession.CurrentUser.UserId;
                _efoUser.modified_date = DateTime.Now;
                _efDbContext.users.ApplyCurrentValues(_efoUser);
                _efDbContext.SaveChanges();
                _efDbContext.Detach(_efoUser);
            }

            gridViewUsers.SetRowCellValue(gridViewUsers.FocusedRowHandle, "password", _control.Text);
            WaitDialog.Close();
        }
        private void tbxEmail_Leave(object sender, EventArgs e)
        {
            TextEdit _control = sender as TextEdit;
            if (gridViewUsers.RowCount < 1 || _control.Text.Length < 1)
                return;

            WaitDialog.Show("Saving data ...");
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                ObjectUser.UserInstance _item = gridViewUsers.GetFocusedRow() as ObjectUser.UserInstance;
                user _efoUser = _efDbContext.users.FirstOrDefault(i => i.id == _item.id);
                _efoUser.email = _control.Text;
                _efoUser.modified_by = UserSession.CurrentUser.UserId;
                _efoUser.modified_date = DateTime.Now;
                _efDbContext.users.ApplyCurrentValues(_efoUser);
                _efDbContext.SaveChanges();
                _efDbContext.Detach(_efoUser);
            }

            gridViewUsers.SetRowCellValue(gridViewUsers.FocusedRowHandle, "email", _control.Text);
            WaitDialog.Close();
        }
        #endregion

        #region Public Methods
        public void PopulateUserGrid()
        {
            try
            {
                //DXGridUser.DataSource = null;
                this.gridViewUsers.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gridViewUsers_FocusedRowChanged);
                this.gridViewCustomers.ColumnFilterChanged -= new System.EventHandler(this.gridViewCustomers_ColumnFilterChanged);
                //ObjectResult objectUserGetUsersExecute = ObjectUser.GetUsers(true).Execute(MergeOption.AppendOnly);
                List<ObjectUser.UserInstance> objectUserGetUsersExecute = ObjectUser.GetUsers(true);
                DXGridUser.DataSource = objectUserGetUsersExecute;
                //rILookUpEditSIP.DataSource = ObjectUser.GetSIPAccounts().Execute(MergeOption.AppendOnly);
                rILookUpEditSIP.DataSource = ObjectUser.GetSIPAccounts();
                UserViewFilter();
                this.gridViewUsers.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gridViewUsers_FocusedRowChanged);
                this.gridViewCustomers.ColumnFilterChanged += new System.EventHandler(this.gridViewCustomers_ColumnFilterChanged);
           
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Private Functions
        private void UserViewFilter()
        {
            gridViewUsers.Columns["active"].ClearFilter();
            if (cbxShowActiveUser.Checked)
                gridViewUsers.Columns["active"].FilterInfo = new ColumnFilterInfo("[active] = " + cbxShowActiveUser.Checked);
        }
        private void PopulateUserRolesView()
        {
            int RoleType = 0;
            if (m_objUser != null)
                RoleType = (m_objUser.internal_user ? 1 : 0);

            this.PopulateUserRolesView(RoleType);
        }
        private void PopulateUserRolesView(bool IsInternalUser)
        {
            this.PopulateUserRolesView(IsInternalUser ? 1 : 0);
        }
        private void PopulateUserRolesView(int RoleType)
        {
            try {
                DXGridRole.DataSource = null;
                if (m_objUser != null)
                    DXGridRole.DataSource = ObjectUser.GetUserRoles(RoleType, m_objUser.id).Execute(MergeOption.AppendOnly);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
        private void PopulateUserCustomersView()
        {
            try {
                DXGridCustomer.DataSource = null;
                if (m_objUser != null)
                    DXGridCustomer.DataSource = ObjectUser.GetUserCustomers(m_objUser.id).Execute(MergeOption.AppendOnly);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
        private void InitObjects()
        {
            m_objUser = null;
            m_objUserRole = null;
            m_objUserCustomer = null;
        }
        private void RenderComment()
        {
            txtComment.Text = string.Empty;
            this.SetFocusedViewInstance();
            if (m_objUser != null)
                txtComment.Text = ObjectUser.GetUserComment(m_objUser.id);
        }
        private void SetFocusedViewInstance()
        {
            this.InitObjects();
            m_objUser = gridViewUsers.GetFocusedRow() as ObjectUser.UserInstance;

            if (gridViewRoles.DataRowCount > 0)
                m_objUserRole = gridViewRoles.GetFocusedRow() as ObjectUser.UserRoleInstance;
            
            if (gridViewCustomers.DataRowCount > 0)
                m_objUserCustomer = gridViewCustomers.GetFocusedRow() as ObjectUser.UserCustomerInstance;
        }
        private void SetUserStatus(bool IsActivated)
        {
            ObjectUser.SaveUserStatus(m_objUser.id, IsActivated);
        }
        private void SetUserType(bool IsInternalUser)
        {
            ObjectUser.SaveUserType(m_objUser.id, IsInternalUser);
            this.PopulateUserRolesView(IsInternalUser);
        }
        private void SetUserRoleStatus(bool IsEnabled)
        {
            ObjectUser.SaveUserRole(m_objUser.id, m_objUserRole.role_id, IsEnabled);
            this.PopulateUserRolesView();
            MessageBox.Show("Successfully updated user role.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void SetUserCustomerStatus(bool IsEnabled)
        {
            ObjectUser.SaveUserCustomer(m_objUser.id, m_objUserCustomer.customer_id, IsEnabled);
            this.PopulateUserRolesView();
            MessageBox.Show("Successfully updated user customer.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void SaveComment()
        {
            ObjectUser.SaveUserComment(m_objUser.id, txtComment.Text.ToString());
            MessageBox.Show("Successfully updated.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void SetUserViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miPrintPreview = new MenuItem("Print Preview");
            miPrintPreview.Click += new EventHandler(miPrintPreview_Click);
            objClickMenu.MenuItems.Add(miPrintPreview);
            DXGridUser.ContextMenu = objClickMenu;
        }
        #endregion
    }
}
