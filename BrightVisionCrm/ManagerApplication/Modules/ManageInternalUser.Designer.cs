using DevExpress.XtraEditors.Controls;
using BrightVision.Common.Business;
using System.Data.Objects;
namespace ManagerApplication.Modules
{
    partial class ManageInternalUser
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lcControls = new DevExpress.XtraLayout.LayoutControl();
            this.btnDeleteSipAccount = new DevExpress.XtraEditors.SimpleButton();
            this.btnEditUser = new DevExpress.XtraEditors.SimpleButton();
            this.txtComment = new DevExpress.XtraEditors.MemoEdit();
            this.DXGridCustomer = new DevExpress.XtraGrid.GridControl();
            this.gridViewCustomers = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colUserCustomerId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCustomerID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCustomerName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCustomerEnable = new DevExpress.XtraGrid.Columns.GridColumn();
            this.riChkCustomerEnable = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.DXGridRole = new DevExpress.XtraGrid.GridControl();
            this.gridViewRoles = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colUserRoleId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colRoleId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colRoleName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colRoleEnable = new DevExpress.XtraGrid.Columns.GridColumn();
            this.riChkEnableRole = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.cmdAdd = new DevExpress.XtraEditors.SimpleButton();
            this.cmdSaveComment = new DevExpress.XtraEditors.SimpleButton();
            this.cbxShowActiveUser = new DevExpress.XtraEditors.CheckEdit();
            this.DXGridUser = new DevExpress.XtraGrid.GridControl();
            this.gridViewUsers = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colUserId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colFirstname = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colActive = new DevExpress.XtraGrid.Columns.GridColumn();
            this.chkIsActive = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.gcolInternalUser = new DevExpress.XtraGrid.Columns.GridColumn();
            this.riChkIsInteralUser = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.colTitle = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colManager = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSite = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colPhone = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMobile = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSIP = new DevExpress.XtraGrid.Columns.GridColumn();
            this.rILookUpEditSIP = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
            this.colEmail = new DevExpress.XtraGrid.Columns.GridColumn();
            this.tbxEmail = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.colPassword = new DevExpress.XtraGrid.Columns.GridColumn();
            this.tbxPassword = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.gcolReportsToId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.lcgUserView = new DevExpress.XtraLayout.LayoutControlGroup();
            this.lciUserView = new DevExpress.XtraLayout.LayoutControlItem();
            this.lciCmdAdd = new DevExpress.XtraLayout.LayoutControlItem();
            this.lciUserRoleView = new DevExpress.XtraLayout.LayoutControlItem();
            this.lciUserCustomerView = new DevExpress.XtraLayout.LayoutControlItem();
            this.lciUserComment = new DevExpress.XtraLayout.LayoutControlItem();
            this.lciCmdSaveComment = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.lcControls)).BeginInit();
            this.lcControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtComment.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DXGridCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewCustomers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.riChkCustomerEnable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DXGridRole)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewRoles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.riChkEnableRole)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxShowActiveUser.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DXGridUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewUsers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIsActive)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.riChkIsInteralUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rILookUpEditSIP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxEmail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lcgUserView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciUserView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciCmdAdd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciUserRoleView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciUserCustomerView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciUserComment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciCmdSaveComment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            this.SuspendLayout();
            // 
            // lcControls
            // 
            this.lcControls.AllowCustomizationMenu = false;
            this.lcControls.Controls.Add(this.btnDeleteSipAccount);
            this.lcControls.Controls.Add(this.btnEditUser);
            this.lcControls.Controls.Add(this.txtComment);
            this.lcControls.Controls.Add(this.DXGridCustomer);
            this.lcControls.Controls.Add(this.DXGridRole);
            this.lcControls.Controls.Add(this.cmdAdd);
            this.lcControls.Controls.Add(this.cmdSaveComment);
            this.lcControls.Controls.Add(this.cbxShowActiveUser);
            this.lcControls.Controls.Add(this.DXGridUser);
            this.lcControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lcControls.Location = new System.Drawing.Point(0, 0);
            this.lcControls.Name = "lcControls";
            this.lcControls.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(184, 301, 250, 350);
            this.lcControls.Root = this.lcgUserView;
            this.lcControls.Size = new System.Drawing.Size(1256, 621);
            this.lcControls.TabIndex = 34;
            this.lcControls.Text = "layoutControl1";
            // 
            // btnDeleteSipAccount
            // 
            this.btnDeleteSipAccount.Location = new System.Drawing.Point(684, 592);
            this.btnDeleteSipAccount.Name = "btnDeleteSipAccount";
            this.btnDeleteSipAccount.Size = new System.Drawing.Size(101, 22);
            this.btnDeleteSipAccount.StyleController = this.lcControls;
            this.btnDeleteSipAccount.TabIndex = 41;
            this.btnDeleteSipAccount.Text = "Delete Sip Account";
            this.btnDeleteSipAccount.Click += new System.EventHandler(this.btnDeleteSipAccount_Click);
            // 
            // btnEditUser
            // 
            this.btnEditUser.Location = new System.Drawing.Point(872, 592);
            this.btnEditUser.Name = "btnEditUser";
            this.btnEditUser.Size = new System.Drawing.Size(78, 22);
            this.btnEditUser.StyleController = this.lcControls;
            this.btnEditUser.TabIndex = 40;
            this.btnEditUser.Text = "Edit User";
            this.btnEditUser.Click += new System.EventHandler(this.btnEditUser_Click);
            // 
            // txtComment
            // 
            this.txtComment.Location = new System.Drawing.Point(954, 508);
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(295, 80);
            this.txtComment.StyleController = this.lcControls;
            this.txtComment.TabIndex = 37;
            // 
            // DXGridCustomer
            // 
            this.DXGridCustomer.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DXGridCustomer.Location = new System.Drawing.Point(954, 166);
            this.DXGridCustomer.MainView = this.gridViewCustomers;
            this.DXGridCustomer.Name = "DXGridCustomer";
            this.DXGridCustomer.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.riChkCustomerEnable});
            this.DXGridCustomer.Size = new System.Drawing.Size(295, 322);
            this.DXGridCustomer.TabIndex = 35;
            this.DXGridCustomer.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewCustomers});
            // 
            // gridViewCustomers
            // 
            this.gridViewCustomers.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colUserCustomerId,
            this.colCustomerID,
            this.colCustomerName,
            this.colCustomerEnable});
            this.gridViewCustomers.GridControl = this.DXGridCustomer;
            this.gridViewCustomers.Name = "gridViewCustomers";
            this.gridViewCustomers.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            this.gridViewCustomers.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            this.gridViewCustomers.OptionsView.ColumnAutoWidth = false;
            this.gridViewCustomers.OptionsView.ShowGroupExpandCollapseButtons = false;
            this.gridViewCustomers.OptionsView.ShowGroupPanel = false;
            this.gridViewCustomers.ColumnFilterChanged += new System.EventHandler(this.gridViewCustomers_ColumnFilterChanged);
            // 
            // colUserCustomerId
            // 
            this.colUserCustomerId.Caption = "colUserCustomerId";
            this.colUserCustomerId.FieldName = "user_customer_id";
            this.colUserCustomerId.Name = "colUserCustomerId";
            // 
            // colCustomerID
            // 
            this.colCustomerID.Caption = "ID";
            this.colCustomerID.FieldName = "customer_id";
            this.colCustomerID.Name = "colCustomerID";
            this.colCustomerID.OptionsColumn.AllowEdit = false;
            this.colCustomerID.Width = 30;
            // 
            // colCustomerName
            // 
            this.colCustomerName.Caption = "Customer name";
            this.colCustomerName.FieldName = "customer_name";
            this.colCustomerName.Name = "colCustomerName";
            this.colCustomerName.OptionsColumn.AllowEdit = false;
            this.colCustomerName.Visible = true;
            this.colCustomerName.VisibleIndex = 0;
            this.colCustomerName.Width = 200;
            // 
            // colCustomerEnable
            // 
            this.colCustomerEnable.Caption = "Enable";
            this.colCustomerEnable.ColumnEdit = this.riChkCustomerEnable;
            this.colCustomerEnable.FieldName = "customer_enabled";
            this.colCustomerEnable.Name = "colCustomerEnable";
            this.colCustomerEnable.Visible = true;
            this.colCustomerEnable.VisibleIndex = 1;
            this.colCustomerEnable.Width = 50;
            // 
            // riChkCustomerEnable
            // 
            this.riChkCustomerEnable.AutoHeight = false;
            this.riChkCustomerEnable.DisplayValueChecked = "True";
            this.riChkCustomerEnable.DisplayValueUnchecked = "False";
            this.riChkCustomerEnable.Name = "riChkCustomerEnable";
            this.riChkCustomerEnable.CheckStateChanged += new System.EventHandler(this.riChkCustomerEnable_CheckStateChanged);
            // 
            // DXGridRole
            // 
            this.DXGridRole.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DXGridRole.Location = new System.Drawing.Point(954, 23);
            this.DXGridRole.MainView = this.gridViewRoles;
            this.DXGridRole.Name = "DXGridRole";
            this.DXGridRole.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.riChkEnableRole});
            this.DXGridRole.Size = new System.Drawing.Size(295, 123);
            this.DXGridRole.TabIndex = 35;
            this.DXGridRole.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewRoles});
            // 
            // gridViewRoles
            // 
            this.gridViewRoles.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colUserRoleId,
            this.colRoleId,
            this.colRoleName,
            this.colRoleEnable});
            this.gridViewRoles.GridControl = this.DXGridRole;
            this.gridViewRoles.Name = "gridViewRoles";
            this.gridViewRoles.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            this.gridViewRoles.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            this.gridViewRoles.OptionsView.ColumnAutoWidth = false;
            this.gridViewRoles.OptionsView.ShowGroupExpandCollapseButtons = false;
            this.gridViewRoles.OptionsView.ShowGroupPanel = false;
            this.gridViewRoles.KeyUp += new System.Windows.Forms.KeyEventHandler(this.gridViewRoles_KeyUp);
            this.gridViewRoles.Click += new System.EventHandler(this.gridViewRoles_Click);
            // 
            // colUserRoleId
            // 
            this.colUserRoleId.Caption = "colUserRoleId";
            this.colUserRoleId.FieldName = "user_role_id";
            this.colUserRoleId.Name = "colUserRoleId";
            this.colUserRoleId.OptionsColumn.FixedWidth = true;
            // 
            // colRoleId
            // 
            this.colRoleId.Caption = "ID";
            this.colRoleId.FieldName = "role_id";
            this.colRoleId.Name = "colRoleId";
            this.colRoleId.OptionsColumn.AllowEdit = false;
            this.colRoleId.Width = 30;
            // 
            // colRoleName
            // 
            this.colRoleName.Caption = "Role name";
            this.colRoleName.FieldName = "role_name";
            this.colRoleName.Name = "colRoleName";
            this.colRoleName.OptionsColumn.AllowEdit = false;
            this.colRoleName.Visible = true;
            this.colRoleName.VisibleIndex = 0;
            this.colRoleName.Width = 200;
            // 
            // colRoleEnable
            // 
            this.colRoleEnable.Caption = "Enable";
            this.colRoleEnable.ColumnEdit = this.riChkEnableRole;
            this.colRoleEnable.FieldName = "role_enabled";
            this.colRoleEnable.Name = "colRoleEnable";
            this.colRoleEnable.Visible = true;
            this.colRoleEnable.VisibleIndex = 1;
            this.colRoleEnable.Width = 50;
            // 
            // riChkEnableRole
            // 
            this.riChkEnableRole.AutoHeight = false;
            this.riChkEnableRole.DisplayValueChecked = "True";
            this.riChkEnableRole.DisplayValueUnchecked = "False";
            this.riChkEnableRole.Name = "riChkEnableRole";
            this.riChkEnableRole.CheckStateChanged += new System.EventHandler(this.riChkEnableRole_CheckStateChanged);
            // 
            // cmdAdd
            // 
            this.cmdAdd.Location = new System.Drawing.Point(789, 592);
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(79, 22);
            this.cmdAdd.StyleController = this.lcControls;
            this.cmdAdd.TabIndex = 36;
            this.cmdAdd.Text = "Add New User";
            this.cmdAdd.Visible = false;
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdSaveComment
            // 
            this.cmdSaveComment.Location = new System.Drawing.Point(1166, 592);
            this.cmdSaveComment.Name = "cmdSaveComment";
            this.cmdSaveComment.Size = new System.Drawing.Size(83, 22);
            this.cmdSaveComment.StyleController = this.lcControls;
            this.cmdSaveComment.TabIndex = 38;
            this.cmdSaveComment.Text = "Save Comment";
            this.cmdSaveComment.Click += new System.EventHandler(this.cmdSaveComment_Click);
            // 
            // cbxShowActiveUser
            // 
            this.cbxShowActiveUser.EditValue = true;
            this.cbxShowActiveUser.Location = new System.Drawing.Point(7, 592);
            this.cbxShowActiveUser.Name = "cbxShowActiveUser";
            this.cbxShowActiveUser.Properties.Caption = "Show active users only";
            this.cbxShowActiveUser.Size = new System.Drawing.Size(134, 19);
            this.cbxShowActiveUser.StyleController = this.lcControls;
            this.cbxShowActiveUser.TabIndex = 39;
            this.cbxShowActiveUser.CheckedChanged += new System.EventHandler(this.cbxShowActiveUser_CheckedChanged);
            // 
            // DXGridUser
            // 
            this.DXGridUser.EmbeddedNavigator.Buttons.CancelEdit.Enabled = false;
            this.DXGridUser.EmbeddedNavigator.Buttons.CancelEdit.Visible = false;
            this.DXGridUser.EmbeddedNavigator.Buttons.Edit.Enabled = false;
            this.DXGridUser.EmbeddedNavigator.Buttons.Edit.Visible = false;
            this.DXGridUser.EmbeddedNavigator.Buttons.EndEdit.Enabled = false;
            this.DXGridUser.EmbeddedNavigator.Buttons.EndEdit.Visible = false;
            this.DXGridUser.EmbeddedNavigator.Buttons.NextPage.Enabled = false;
            this.DXGridUser.EmbeddedNavigator.Buttons.NextPage.Visible = false;
            this.DXGridUser.EmbeddedNavigator.Buttons.PrevPage.Enabled = false;
            this.DXGridUser.EmbeddedNavigator.Buttons.PrevPage.Visible = false;
            this.DXGridUser.EmbeddedNavigator.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Office2003;
            this.DXGridUser.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DXGridUser.Location = new System.Drawing.Point(7, 7);
            this.DXGridUser.MainView = this.gridViewUsers;
            this.DXGridUser.Name = "DXGridUser";
            this.DXGridUser.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.chkIsActive,
            this.riChkIsInteralUser,
            this.rILookUpEditSIP,
            this.tbxPassword,
            this.tbxEmail});
            this.DXGridUser.Size = new System.Drawing.Size(943, 581);
            this.DXGridUser.TabIndex = 35;
            this.DXGridUser.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewUsers});
            // 
            // gridViewUsers
            // 
            this.gridViewUsers.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colUserId,
            this.colFirstname,
            this.colActive,
            this.gcolInternalUser,
            this.colTitle,
            this.colManager,
            this.colSite,
            this.colPhone,
            this.colMobile,
            this.colSIP,
            this.colEmail,
            this.colPassword,
            this.gcolReportsToId});
            this.gridViewUsers.GridControl = this.DXGridUser;
            this.gridViewUsers.IndicatorWidth = 10;
            this.gridViewUsers.Name = "gridViewUsers";
            this.gridViewUsers.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            this.gridViewUsers.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            this.gridViewUsers.OptionsFind.AlwaysVisible = true;
            this.gridViewUsers.OptionsView.ColumnAutoWidth = false;
            this.gridViewUsers.OptionsView.ShowGroupPanel = false;
            this.gridViewUsers.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridViewUsers_PopupMenuShowing);
            this.gridViewUsers.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gridViewUsers_FocusedRowChanged);
            this.gridViewUsers.DoubleClick += new System.EventHandler(this.gridViewUsers_DoubleClick);
            // 
            // colUserId
            // 
            this.colUserId.Caption = "Id";
            this.colUserId.FieldName = "id";
            this.colUserId.Name = "colUserId";
            this.colUserId.OptionsColumn.AllowEdit = false;
            this.colUserId.OptionsColumn.FixedWidth = true;
            this.colUserId.Width = 30;
            // 
            // colFirstname
            // 
            this.colFirstname.Caption = "Fullname";
            this.colFirstname.FieldName = "full_name";
            this.colFirstname.Name = "colFirstname";
            this.colFirstname.OptionsColumn.AllowEdit = false;
            this.colFirstname.OptionsColumn.FixedWidth = true;
            this.colFirstname.Visible = true;
            this.colFirstname.VisibleIndex = 1;
            this.colFirstname.Width = 150;
            // 
            // colActive
            // 
            this.colActive.Caption = "Active";
            this.colActive.ColumnEdit = this.chkIsActive;
            this.colActive.FieldName = "active";
            this.colActive.Name = "colActive";
            this.colActive.OptionsColumn.FixedWidth = true;
            this.colActive.Visible = true;
            this.colActive.VisibleIndex = 0;
            this.colActive.Width = 37;
            // 
            // chkIsActive
            // 
            this.chkIsActive.AutoHeight = false;
            this.chkIsActive.DisplayValueChecked = "True";
            this.chkIsActive.DisplayValueUnchecked = "False";
            this.chkIsActive.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.chkIsActive.FullFocusRect = true;
            this.chkIsActive.Name = "chkIsActive";
            this.chkIsActive.CheckStateChanged += new System.EventHandler(this.chkIsActive_CheckStateChanged);
            // 
            // gcolInternalUser
            // 
            this.gcolInternalUser.Caption = "Internal";
            this.gcolInternalUser.ColumnEdit = this.riChkIsInteralUser;
            this.gcolInternalUser.FieldName = "internal_user";
            this.gcolInternalUser.Name = "gcolInternalUser";
            this.gcolInternalUser.Width = 45;
            // 
            // riChkIsInteralUser
            // 
            this.riChkIsInteralUser.AutoHeight = false;
            this.riChkIsInteralUser.DisplayValueChecked = "True";
            this.riChkIsInteralUser.DisplayValueUnchecked = "False";
            this.riChkIsInteralUser.Name = "riChkIsInteralUser";
            this.riChkIsInteralUser.CheckStateChanged += new System.EventHandler(this.riChkIsInteralUser_CheckStateChanged);
            // 
            // colTitle
            // 
            this.colTitle.Caption = "Title";
            this.colTitle.FieldName = "title";
            this.colTitle.Name = "colTitle";
            this.colTitle.OptionsColumn.AllowEdit = false;
            this.colTitle.OptionsColumn.FixedWidth = true;
            this.colTitle.Visible = true;
            this.colTitle.VisibleIndex = 2;
            this.colTitle.Width = 100;
            // 
            // colManager
            // 
            this.colManager.Caption = "Manager";
            this.colManager.FieldName = "manager";
            this.colManager.Name = "colManager";
            this.colManager.OptionsColumn.AllowEdit = false;
            this.colManager.OptionsColumn.FixedWidth = true;
            this.colManager.Visible = true;
            this.colManager.VisibleIndex = 3;
            this.colManager.Width = 120;
            // 
            // colSite
            // 
            this.colSite.Caption = "Site";
            this.colSite.FieldName = "site";
            this.colSite.Name = "colSite";
            this.colSite.OptionsColumn.AllowEdit = false;
            this.colSite.OptionsColumn.FixedWidth = true;
            this.colSite.Visible = true;
            this.colSite.VisibleIndex = 4;
            this.colSite.Width = 100;
            // 
            // colPhone
            // 
            this.colPhone.Caption = "Phone";
            this.colPhone.FieldName = "phone";
            this.colPhone.Name = "colPhone";
            this.colPhone.OptionsColumn.AllowEdit = false;
            this.colPhone.OptionsColumn.FixedWidth = true;
            this.colPhone.Visible = true;
            this.colPhone.VisibleIndex = 5;
            this.colPhone.Width = 100;
            // 
            // colMobile
            // 
            this.colMobile.Caption = "Mobile";
            this.colMobile.FieldName = "mobile";
            this.colMobile.Name = "colMobile";
            this.colMobile.OptionsColumn.AllowEdit = false;
            this.colMobile.OptionsColumn.FixedWidth = true;
            this.colMobile.Visible = true;
            this.colMobile.VisibleIndex = 6;
            this.colMobile.Width = 100;
            // 
            // colSIP
            // 
            this.colSIP.Caption = "SIP Account";
            this.colSIP.ColumnEdit = this.rILookUpEditSIP;
            this.colSIP.FieldName = "sip_id";
            this.colSIP.MinWidth = 35;
            this.colSIP.Name = "colSIP";
            this.colSIP.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
            this.colSIP.Visible = true;
            this.colSIP.VisibleIndex = 7;
            this.colSIP.Width = 140;
            // 
            // rILookUpEditSIP
            // 
            this.rILookUpEditSIP.AutoHeight = false;
            this.rILookUpEditSIP.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.rILookUpEditSIP.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("display_name", "display_name")});
            this.rILookUpEditSIP.DisplayMember = "display_name";
            this.rILookUpEditSIP.Name = "rILookUpEditSIP";
            this.rILookUpEditSIP.NullText = "";
            this.rILookUpEditSIP.ShowHeader = false;
            this.rILookUpEditSIP.ThrowExceptionOnInvalidLookUpEditValueType = true;
            this.rILookUpEditSIP.ValueMember = "id";
            this.rILookUpEditSIP.EditValueChanged += new System.EventHandler(this.rILookUpEditSIP_EditValueChanged);
            // 
            // colEmail
            // 
            this.colEmail.Caption = "Email";
            this.colEmail.ColumnEdit = this.tbxEmail;
            this.colEmail.FieldName = "email";
            this.colEmail.Name = "colEmail";
            this.colEmail.OptionsColumn.FixedWidth = true;
            this.colEmail.Visible = true;
            this.colEmail.VisibleIndex = 8;
            this.colEmail.Width = 200;
            // 
            // tbxEmail
            // 
            this.tbxEmail.AutoHeight = false;
            this.tbxEmail.Name = "tbxEmail";
            //this.tbxEmail.EditValueChanged += new System.EventHandler(this.tbxEmail_EditValueChanged);
            this.tbxEmail.Leave += new System.EventHandler(this.tbxEmail_Leave);
            // 
            // colPassword
            // 
            this.colPassword.Caption = "Password";
            this.colPassword.ColumnEdit = this.tbxPassword;
            this.colPassword.FieldName = "password";
            this.colPassword.Name = "colPassword";
            this.colPassword.Visible = true;
            this.colPassword.VisibleIndex = 9;
            this.colPassword.Width = 300;
            // 
            // tbxPassword
            // 
            this.tbxPassword.AutoHeight = false;
            this.tbxPassword.Name = "tbxPassword";
            this.tbxPassword.PasswordChar = '*';
            //this.tbxPassword.EditValueChanged += new System.EventHandler(this.tbxPassword_EditValueChanged);
            this.tbxPassword.Leave += new System.EventHandler(this.tbxPassword_Leave);
            // 
            // gcolReportsToId
            // 
            this.gcolReportsToId.Caption = "Reports to Id";
            this.gcolReportsToId.FieldName = "reports_to";
            this.gcolReportsToId.Name = "gcolReportsToId";
            // 
            // lcgUserView
            // 
            this.lcgUserView.CustomizationFormText = "lcgUserView";
            this.lcgUserView.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.lcgUserView.GroupBordersVisible = false;
            this.lcgUserView.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.lciUserView,
            this.lciCmdAdd,
            this.lciUserRoleView,
            this.lciUserCustomerView,
            this.lciUserComment,
            this.lciCmdSaveComment,
            this.emptySpaceItem2,
            this.layoutControlItem1,
            this.emptySpaceItem1,
            this.layoutControlItem2,
            this.layoutControlItem3});
            this.lcgUserView.Location = new System.Drawing.Point(0, 0);
            this.lcgUserView.Name = "Root";
            this.lcgUserView.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.lcgUserView.Size = new System.Drawing.Size(1256, 621);
            this.lcgUserView.Text = "Root";
            this.lcgUserView.TextVisible = false;
            // 
            // lciUserView
            // 
            this.lciUserView.Control = this.DXGridUser;
            this.lciUserView.CustomizationFormText = "Users:";
            this.lciUserView.Location = new System.Drawing.Point(0, 0);
            this.lciUserView.Name = "lciUserView";
            this.lciUserView.Size = new System.Drawing.Size(947, 585);
            this.lciUserView.Text = "Users:";
            this.lciUserView.TextLocation = DevExpress.Utils.Locations.Top;
            this.lciUserView.TextSize = new System.Drawing.Size(0, 0);
            this.lciUserView.TextToControlDistance = 0;
            this.lciUserView.TextVisible = false;
            // 
            // lciCmdAdd
            // 
            this.lciCmdAdd.Control = this.cmdAdd;
            this.lciCmdAdd.CustomizationFormText = "layoutControlItem1";
            this.lciCmdAdd.Location = new System.Drawing.Point(782, 585);
            this.lciCmdAdd.MaxSize = new System.Drawing.Size(83, 26);
            this.lciCmdAdd.MinSize = new System.Drawing.Size(83, 26);
            this.lciCmdAdd.Name = "lciCmdAdd";
            this.lciCmdAdd.Size = new System.Drawing.Size(83, 26);
            this.lciCmdAdd.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.lciCmdAdd.Text = "lciCmdAdd";
            this.lciCmdAdd.TextSize = new System.Drawing.Size(0, 0);
            this.lciCmdAdd.TextToControlDistance = 0;
            this.lciCmdAdd.TextVisible = false;
            this.lciCmdAdd.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            // 
            // lciUserRoleView
            // 
            this.lciUserRoleView.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.lciUserRoleView.AppearanceItemCaption.Options.UseFont = true;
            this.lciUserRoleView.Control = this.DXGridRole;
            this.lciUserRoleView.CustomizationFormText = "lciRoleView";
            this.lciUserRoleView.Location = new System.Drawing.Point(947, 0);
            this.lciUserRoleView.Name = "lciUserRoleView";
            this.lciUserRoleView.Size = new System.Drawing.Size(299, 143);
            this.lciUserRoleView.Text = "Roles";
            this.lciUserRoleView.TextLocation = DevExpress.Utils.Locations.Top;
            this.lciUserRoleView.TextSize = new System.Drawing.Size(80, 13);
            // 
            // lciUserCustomerView
            // 
            this.lciUserCustomerView.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.lciUserCustomerView.AppearanceItemCaption.Options.UseFont = true;
            this.lciUserCustomerView.Control = this.DXGridCustomer;
            this.lciUserCustomerView.CustomizationFormText = "Customers for user:";
            this.lciUserCustomerView.Location = new System.Drawing.Point(947, 143);
            this.lciUserCustomerView.Name = "lciUserCustomerView";
            this.lciUserCustomerView.Size = new System.Drawing.Size(299, 342);
            this.lciUserCustomerView.Text = "Customers";
            this.lciUserCustomerView.TextLocation = DevExpress.Utils.Locations.Top;
            this.lciUserCustomerView.TextSize = new System.Drawing.Size(80, 13);
            // 
            // lciUserComment
            // 
            this.lciUserComment.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.lciUserComment.AppearanceItemCaption.Options.UseFont = true;
            this.lciUserComment.Control = this.txtComment;
            this.lciUserComment.CustomizationFormText = "Comment";
            this.lciUserComment.Location = new System.Drawing.Point(947, 485);
            this.lciUserComment.Name = "lciUserComment";
            this.lciUserComment.Size = new System.Drawing.Size(299, 100);
            this.lciUserComment.Text = "User Remarks";
            this.lciUserComment.TextLocation = DevExpress.Utils.Locations.Top;
            this.lciUserComment.TextSize = new System.Drawing.Size(80, 13);
            // 
            // lciCmdSaveComment
            // 
            this.lciCmdSaveComment.Control = this.cmdSaveComment;
            this.lciCmdSaveComment.CustomizationFormText = "layoutControlItem2";
            this.lciCmdSaveComment.Location = new System.Drawing.Point(1159, 585);
            this.lciCmdSaveComment.MaxSize = new System.Drawing.Size(87, 26);
            this.lciCmdSaveComment.MinSize = new System.Drawing.Size(87, 26);
            this.lciCmdSaveComment.Name = "lciCmdSaveComment";
            this.lciCmdSaveComment.Size = new System.Drawing.Size(87, 26);
            this.lciCmdSaveComment.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.lciCmdSaveComment.Text = "lciCmdSaveComment";
            this.lciCmdSaveComment.TextSize = new System.Drawing.Size(0, 0);
            this.lciCmdSaveComment.TextToControlDistance = 0;
            this.lciCmdSaveComment.TextVisible = false;
            // 
            // emptySpaceItem2
            // 
            this.emptySpaceItem2.AllowHotTrack = false;
            this.emptySpaceItem2.CustomizationFormText = "emptySpaceItem2";
            this.emptySpaceItem2.Location = new System.Drawing.Point(947, 585);
            this.emptySpaceItem2.Name = "emptySpaceItem2";
            this.emptySpaceItem2.Size = new System.Drawing.Size(212, 26);
            this.emptySpaceItem2.Text = "emptySpaceItem2";
            this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.cbxShowActiveUser;
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 585);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(138, 26);
            this.layoutControlItem1.Text = "layoutControlItem1";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextToControlDistance = 0;
            this.layoutControlItem1.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem1.Location = new System.Drawing.Point(138, 585);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(539, 26);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.btnEditUser;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(865, 585);
            this.layoutControlItem2.MaxSize = new System.Drawing.Size(82, 26);
            this.layoutControlItem2.MinSize = new System.Drawing.Size(82, 26);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(82, 26);
            this.layoutControlItem2.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem2.Text = "layoutControlItem2";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextToControlDistance = 0;
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.btnDeleteSipAccount;
            this.layoutControlItem3.CustomizationFormText = "layoutControlItem3";
            this.layoutControlItem3.Location = new System.Drawing.Point(677, 585);
            this.layoutControlItem3.MaxSize = new System.Drawing.Size(105, 26);
            this.layoutControlItem3.MinSize = new System.Drawing.Size(105, 26);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(105, 26);
            this.layoutControlItem3.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem3.Text = "layoutControlItem3";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextToControlDistance = 0;
            this.layoutControlItem3.TextVisible = false;
            // 
            // ManageInternalUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lcControls);
            this.Name = "ManageInternalUser";
            this.Size = new System.Drawing.Size(1256, 621);
            ((System.ComponentModel.ISupportInitialize)(this.lcControls)).EndInit();
            this.lcControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtComment.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DXGridCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewCustomers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.riChkCustomerEnable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DXGridRole)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewRoles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.riChkEnableRole)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxShowActiveUser.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DXGridUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewUsers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIsActive)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.riChkIsInteralUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rILookUpEditSIP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxEmail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lcgUserView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciUserView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciCmdAdd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciUserRoleView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciUserCustomerView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciUserComment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciCmdSaveComment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl lcControls;
        private DevExpress.XtraLayout.LayoutControlGroup lcgUserView;
        private DevExpress.XtraGrid.GridControl DXGridUser;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewUsers;
        private DevExpress.XtraGrid.Columns.GridColumn colUserId;
        private DevExpress.XtraGrid.Columns.GridColumn colFirstname;
        private DevExpress.XtraGrid.Columns.GridColumn colActive;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit chkIsActive;
        private DevExpress.XtraGrid.Columns.GridColumn gcolInternalUser;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit riChkIsInteralUser;
        private DevExpress.XtraGrid.Columns.GridColumn colTitle;
        private DevExpress.XtraGrid.Columns.GridColumn colManager;
        private DevExpress.XtraGrid.Columns.GridColumn colSite;
        private DevExpress.XtraGrid.Columns.GridColumn colPhone;
        private DevExpress.XtraGrid.Columns.GridColumn colMobile;
        private DevExpress.XtraGrid.Columns.GridColumn colEmail;
        private DevExpress.XtraGrid.Columns.GridColumn colPassword;
        private DevExpress.XtraGrid.Columns.GridColumn gcolReportsToId;
        private DevExpress.XtraLayout.LayoutControlItem lciUserView;
        private DevExpress.XtraEditors.SimpleButton cmdAdd;
        private DevExpress.XtraLayout.LayoutControlItem lciCmdAdd;
        private DevExpress.XtraGrid.GridControl DXGridRole;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewRoles;
        private DevExpress.XtraGrid.Columns.GridColumn colUserRoleId;
        private DevExpress.XtraGrid.Columns.GridColumn colRoleId;
        private DevExpress.XtraGrid.Columns.GridColumn colRoleName;
        private DevExpress.XtraGrid.Columns.GridColumn colRoleEnable;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit riChkEnableRole;
        private DevExpress.XtraLayout.LayoutControlItem lciUserRoleView;
        private DevExpress.XtraGrid.GridControl DXGridCustomer;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewCustomers;
        private DevExpress.XtraGrid.Columns.GridColumn colUserCustomerId;
        private DevExpress.XtraGrid.Columns.GridColumn colCustomerID;
        private DevExpress.XtraGrid.Columns.GridColumn colCustomerName;
        private DevExpress.XtraGrid.Columns.GridColumn colCustomerEnable;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit riChkCustomerEnable;
        private DevExpress.XtraLayout.LayoutControlItem lciUserCustomerView;
        private DevExpress.XtraEditors.MemoEdit txtComment;
        private DevExpress.XtraLayout.LayoutControlItem lciUserComment;
        private DevExpress.XtraEditors.SimpleButton cmdSaveComment;
        private DevExpress.XtraLayout.LayoutControlItem lciCmdSaveComment;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem2;
        private DevExpress.XtraEditors.CheckEdit cbxShowActiveUser;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraEditors.SimpleButton btnEditUser;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraGrid.Columns.GridColumn colSIP;
        private DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit rILookUpEditSIP;
        private DevExpress.XtraEditors.SimpleButton btnDeleteSipAccount;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit tbxEmail;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit tbxPassword;

    }
}
