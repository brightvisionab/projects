
#region Namespace
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Linq;

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
using SalesConsultant.Forms;
#endregion

namespace SalesConsultant.Modules
{
    public partial class ManageCustomerUser : DevExpress.XtraEditors.XtraUserControl
    {
        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private GridView m_objGridView = null;
        private ObjectUser.UserInstance m_objUser = null;
        private ObjectUser.UserRoleInstance m_objUserRole = null;
        private ObjectUser.UserCustomerInstance m_objUserCustomer = null;
        private AddUser m_objAddUserForm = null;
        private DialogResult m_objDialogResult;
        private string m_MessageBoxCaption = "Manager Application - Users";
        #endregion

        #region Contructors
        public ManageCustomerUser()
        {
            this.Visible = false;
            InitializeComponent();
            this.lcControls.AllowCustomizationMenu = false;
            this.PopulateUserGrid(null);
            this.SetUserViewContextMenu();
            this.Visible = true;
        }
        #endregion

        #region Object Control Events
        private void cbxShowActiveUser_CheckedChanged(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Applying filter...");
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

        private void chkIsActive_CheckStateChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckEdit objCheckBox = sender as DevExpress.XtraEditors.CheckEdit;
            if (objCheckBox.Checked)
            {
                this.SetUserStatus(true);
                MessageBox.Show("User " + m_objUser.full_name + " successfully activated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                this.SetUserStatus(false);
                MessageBox.Show("User " + m_objUser.full_name + " successfully de-activated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            this.DisplayUserForm(true);
        }

        private void gridViewUsers_DoubleClick(object sender, EventArgs e)
        {
            if (gridViewUsers.RowCount < 1)
                return;

            this.SetFocusedViewInstance();
            this.DisplayUserForm(false);
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

        private void riChkEnableRole_CheckStateChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckEdit objCheckBox = sender as DevExpress.XtraEditors.CheckEdit;
            this.SetFocusedViewInstance();

            if (objCheckBox.Checked)
                this.SetUserRoleStatus(true);
            else
                this.SetUserRoleStatus(false);
        }

        private void riChkCustomerEnable_CheckStateChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckEdit objCheckBox = sender as DevExpress.XtraEditors.CheckEdit;
            this.SetFocusedViewInstance();
            this.SetUserCustomerStatus(objCheckBox.Checked);
        }

        private void gridViewUsers_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            this.RenderComment();
            this.PopulateUserRolesView();
        }

        private void riChkIsInteralUser_CheckStateChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckEdit objCheckBox = sender as DevExpress.XtraEditors.CheckEdit;
            if (objCheckBox.Checked)
            {
                this.SetUserType(true);
                MessageBox.Show("User " + m_objUser.full_name + " updated to internal user!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                this.SetUserType(false);
                MessageBox.Show("User " + m_objUser.full_name + " updated to customer user!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void miPrintPreview_Click(object sender, EventArgs e)
        {
            DXGridUser.ShowPrintPreview();
        }

        private void gridViewUsers_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Pupulates the user grid view contents
        /// </summary>
        public void PopulateUserGrid(string DefaultSelectedUser)
        {
            try
            {
                this.gridViewUsers.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gridViewUsers_FocusedRowChanged);
                DXGridUser.DataSource = null;
                //DXGridUser.DataSource = ObjectUser.GetUsers(false).Execute(MergeOption.AppendOnly);
                DXGridUser.DataSource = ObjectUser.GetUsers(false);
                this.UserViewFilter();
                this.SetDefaultSelectedUser(DefaultSelectedUser);
                this.gridViewUsers.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gridViewUsers_FocusedRowChanged);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Sets the default selected user
        /// </summary>
        private void SetDefaultSelectedUser(string DefaultSelectedUser)
        {
            if (string.IsNullOrEmpty(DefaultSelectedUser))
                return;

            int RowNo = 0;
            ObjectUser.UserInstance objUser = null;
            GridView objGridView = (GridView) DXGridUser.FocusedView;
            for (int i = 0; i < objGridView.RowCount; i++)
            {
                objUser = (ObjectUser.UserInstance) objGridView.GetRow(i);
                if (objUser.full_name.Equals(DefaultSelectedUser)) 
                    RowNo = i;
            }

            gridViewUsers.FocusedRowHandle = RowNo;
            //gridViewUsers.UnselectRow(0);
            //gridViewUsers.SelectRow(RowNo);
        }

        /// <summary>
        /// Filters the user view for active/inactive users
        /// </summary>
        private void UserViewFilter()
        {
            gridViewUsers.Columns["active"].ClearFilter();
            if (cbxShowActiveUser.Checked)
                gridViewUsers.Columns["active"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo("[active] = " + cbxShowActiveUser.Checked);
        }

        /// <summary>
        /// Function to populate user roles grid view (Overload Method 1)
        /// </summary>
        private void PopulateUserRolesView()
        {
            int RoleType = 0;
            if (m_objUser != null)
                RoleType = (m_objUser.internal_user ? 1 : 0);

            this.PopulateUserRolesView(RoleType);
        }

        /// <summary>
        /// Function to populate user roles grid view (Overload Method 2)
        /// </summary>
        private void PopulateUserRolesView(bool IsInternalUser)
        {
            this.PopulateUserRolesView(IsInternalUser ? 1 : 0);
        }

        /// <summary>
        /// Function to populate user roles grid view (Main/Pivot Method)
        /// </summary>
        private void PopulateUserRolesView(int RoleType)
        {
            try
            {
                DXGridRole.DataSource = null;
                if (m_objUser != null)
                    DXGridRole.DataSource = ObjectUser.GetUserRoles(RoleType, m_objUser.id).Execute(MergeOption.AppendOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Initializes the popup dialog for user entry
        /// </summary>
        private void DisplayUserForm(bool IsNew)
        {
            WaitDialog.Show(ParentForm, "Loading components...");
            if (IsNew)
            {
                m_objAddUserForm = new AddUser();
                m_objAddUserForm.IsNew = true;
            }
            else
            {
                m_objAddUserForm = new AddUser(AddUser.SaveType.SaveTypeEdit, m_objUser);
                m_objAddUserForm.IsNew = false;
            }

            m_objAddUserForm.UserType = AddUser.eUserType.CustomerUser;
            m_objAddUserForm.InitializeModule();
            m_objAddUserForm.objCustomerUserControl = this;
            PopupDialog m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = "Manager Application - Users";
            m_objPopupDialog.Controls.Add(m_objAddUserForm);
            m_objPopupDialog.ClientSize = new Size(m_objAddUserForm.Width + 2, m_objAddUserForm.Height + 2);
           WaitDialog.Close();
            m_objPopupDialog.ShowDialog(this.ParentForm);
        }

        /// <summary>
        /// Initializes the grid view object and user instance, for use in data manipulation
        /// </summary>
        private void InitObjects()
        {
            m_objGridView = null;
            m_objUser = null;
            m_objUserRole = null;
            m_objUserCustomer = null;
        }

        /// <summary>
        /// Displays the user comment in the comment textbox
        /// </summary>
        private void RenderComment()
        {
            this.SetFocusedViewInstance();
            txtComment.Text = "";
            if (m_objUser != null)
                txtComment.Text = ObjectUser.GetUserComment(m_objUser.id);
        }

        /// <summary>
        /// Sets the focused view instance of the grid. Instantiates the objects that can be used for data manipulation.
        /// Where m_objUser, is the current selected object in the grid view
        /// </summary>
        private void SetFocusedViewInstance()
        {
            this.InitObjects();

            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)DXGridUser.FocusedView;
            m_objUser = (ObjectUser.UserInstance)m_objGridView.GetFocusedRow();

            if (gridViewRoles.DataRowCount > 0)
            {
                m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)DXGridRole.FocusedView;
                m_objUserRole = (ObjectUser.UserRoleInstance)m_objGridView.GetFocusedRow();
            }
        }

        /// <summary>
        /// Sets the user's status (e.g. active, inactive)
        /// </summary>
        private void SetUserStatus(bool IsActivated)
        {
            ObjectUser.SaveUserStatus(m_objUser.id, IsActivated);
        }

        /// <summary>
        /// Sets the user's user type (e.g. internal user, customer user)
        /// </summary>
        private void SetUserType(bool IsInternalUser)
        {
            ObjectUser.SaveUserType(m_objUser.id, IsInternalUser);
            this.PopulateUserRolesView(IsInternalUser);
        }

        /// <summary>
        /// Sets the user role's status (e.g. active, inactive)
        /// </summary>
        private void SetUserRoleStatus(bool IsEnabled)
        {
            ObjectUser.SaveUserRole(m_objUser.id, m_objUserRole.role_id, IsEnabled);
            this.PopulateUserRolesView();
            MessageBox.Show("Successfully updated user's roles!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Sets the user customer's status (e.g. active, inactive)
        /// </summary>
        private void SetUserCustomerStatus(bool IsEnabled)
        {
            ObjectUser.SaveUserCustomer(m_objUser.id, m_objUserCustomer.customer_id, IsEnabled);
            this.PopulateUserRolesView();
            MessageBox.Show("Successfully updated user's customers!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Logic to save user comments
        /// </summary>
        private void SaveComment()
        {
            ObjectUser.SaveUserComment(m_objUser.id, txtComment.Text.ToString());
            MessageBox.Show("Successfully updated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Logic that deletes the current selected user
        /// </summary>
        private void DeleteUser()
        {
            m_objDialogResult = MessageBox.Show("Are you sure to delete this user?", "Manager Application - Users", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (m_objDialogResult == DialogResult.Yes)
            {
                this.SetFocusedViewInstance();
                ObjectUser.DeleteUser(m_objUser.id);
                this.PopulateUserGrid(null);
            }
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
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
