namespace ManagerApplication.Modules
{
    partial class ManageCustomerCampaign
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
            this.lcCustomerCampaign = new DevExpress.XtraLayout.LayoutControl();
            this.cmdAddCampaign = new DevExpress.XtraEditors.SimpleButton();
            this.cmdAddCustomer = new DevExpress.XtraEditors.SimpleButton();
            this.gcCustomerCampaign = new DevExpress.XtraGrid.GridControl();
            this.gvCustomerCampaign = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gcolCampaignId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolCampaignName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolCampaignOwner = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolWebAccess = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolPriority = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolStatus = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolCampaignDescription = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolOwnerId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolWebAccessUserId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolCustomersId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcCustomer = new DevExpress.XtraGrid.GridControl();
            this.gvCustomer = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gcolCustomerId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolCustomerName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolOrgNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolReferenceNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolActive = new DevExpress.XtraGrid.Columns.GridColumn();
            this.riChkCustomerActive = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.gcolOwner = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolAddress = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcolDescription = new DevExpress.XtraGrid.Columns.GridColumn();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.lciCustomerView = new DevExpress.XtraLayout.LayoutControlItem();
            this.lciCampaignView = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            ((System.ComponentModel.ISupportInitialize)(this.lcCustomerCampaign)).BeginInit();
            this.lcCustomerCampaign.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcCustomerCampaign)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvCustomerCampaign)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.riChkCustomerActive)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciCustomerView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciCampaignView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
            this.SuspendLayout();
            // 
            // lcCustomerCampaign
            // 
            this.lcCustomerCampaign.Controls.Add(this.cmdAddCampaign);
            this.lcCustomerCampaign.Controls.Add(this.cmdAddCustomer);
            this.lcCustomerCampaign.Controls.Add(this.gcCustomerCampaign);
            this.lcCustomerCampaign.Controls.Add(this.gcCustomer);
            this.lcCustomerCampaign.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lcCustomerCampaign.Location = new System.Drawing.Point(0, 0);
            this.lcCustomerCampaign.Name = "lcCustomerCampaign";
            this.lcCustomerCampaign.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(310, 427, 250, 350);
            this.lcCustomerCampaign.Root = this.layoutControlGroup1;
            this.lcCustomerCampaign.Size = new System.Drawing.Size(1130, 638);
            this.lcCustomerCampaign.TabIndex = 25;
            this.lcCustomerCampaign.Text = "layoutControl1";
            this.lcCustomerCampaign.AllowCustomizationMenu = false;
            // 
            // cmdAddCampaign
            // 
            this.cmdAddCampaign.Enabled = false;
            this.cmdAddCampaign.Location = new System.Drawing.Point(962, 604);
            this.cmdAddCampaign.Name = "cmdAddCampaign";
            this.cmdAddCampaign.Size = new System.Drawing.Size(156, 22);
            this.cmdAddCampaign.StyleController = this.lcCustomerCampaign;
            this.cmdAddCampaign.TabIndex = 29;
            this.cmdAddCampaign.Text = "Add New Campaign";
            this.cmdAddCampaign.Visible = false;
            this.cmdAddCampaign.Click += new System.EventHandler(this.cmdAddCampaign_Click);
            // 
            // cmdAddCustomer
            // 
            this.cmdAddCustomer.Enabled = false;
            this.cmdAddCustomer.Location = new System.Drawing.Point(962, 342);
            this.cmdAddCustomer.Name = "cmdAddCustomer";
            this.cmdAddCustomer.Size = new System.Drawing.Size(156, 22);
            this.cmdAddCustomer.StyleController = this.lcCustomerCampaign;
            this.cmdAddCustomer.TabIndex = 28;
            this.cmdAddCustomer.Text = "Add New Customer";
            this.cmdAddCustomer.Visible = false;
            this.cmdAddCustomer.Click += new System.EventHandler(this.cmdAddCustomer_Click);
            // 
            // gcCustomerCampaign
            // 
            this.gcCustomerCampaign.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gcCustomerCampaign.Location = new System.Drawing.Point(12, 384);
            this.gcCustomerCampaign.MainView = this.gvCustomerCampaign;
            this.gcCustomerCampaign.Name = "gcCustomerCampaign";
            this.gcCustomerCampaign.Size = new System.Drawing.Size(1106, 216);
            this.gcCustomerCampaign.TabIndex = 27;
            this.gcCustomerCampaign.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvCustomerCampaign});
            // 
            // gvCustomerCampaign
            // 
            this.gvCustomerCampaign.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gcolCampaignId,
            this.gcolCampaignName,
            this.gcolCampaignOwner,
            this.gcolWebAccess,
            this.gcolPriority,
            this.gcolStatus,
            this.gcolCampaignDescription,
            this.gcolOwnerId,
            this.gcolWebAccessUserId,
            this.gcolCustomersId});
            this.gvCustomerCampaign.GridControl = this.gcCustomerCampaign;
            this.gvCustomerCampaign.Name = "gvCustomerCampaign";
            this.gvCustomerCampaign.OptionsView.ColumnAutoWidth = false;
            this.gvCustomerCampaign.OptionsView.ShowGroupExpandCollapseButtons = false;
            this.gvCustomerCampaign.OptionsView.ShowGroupPanel = false;
            this.gvCustomerCampaign.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gvCustomerCampaign_PopupMenuShowing);
            this.gvCustomerCampaign.DoubleClick += new System.EventHandler(this.gvCustomerCampaign_DoubleClick);
            // 
            // gcolCampaignId
            // 
            this.gcolCampaignId.Caption = "Id";
            this.gcolCampaignId.FieldName = "id";
            this.gcolCampaignId.Name = "gcolCampaignId";
            this.gcolCampaignId.OptionsColumn.AllowEdit = false;
            this.gcolCampaignId.Width = 30;
            // 
            // gcolCampaignName
            // 
            this.gcolCampaignName.Caption = "Campaign Name";
            this.gcolCampaignName.FieldName = "campaign_name";
            this.gcolCampaignName.Name = "gcolCampaignName";
            this.gcolCampaignName.OptionsColumn.AllowEdit = false;
            this.gcolCampaignName.Visible = true;
            this.gcolCampaignName.VisibleIndex = 0;
            this.gcolCampaignName.Width = 150;
            // 
            // gcolCampaignOwner
            // 
            this.gcolCampaignOwner.Caption = "Owner";
            this.gcolCampaignOwner.FieldName = "owner_name";
            this.gcolCampaignOwner.Name = "gcolCampaignOwner";
            this.gcolCampaignOwner.OptionsColumn.AllowEdit = false;
            this.gcolCampaignOwner.Visible = true;
            this.gcolCampaignOwner.VisibleIndex = 1;
            this.gcolCampaignOwner.Width = 130;
            // 
            // gcolWebAccess
            // 
            this.gcolWebAccess.Caption = "Web Access";
            this.gcolWebAccess.FieldName = "web_access_name";
            this.gcolWebAccess.Name = "gcolWebAccess";
            this.gcolWebAccess.OptionsColumn.AllowEdit = false;
            this.gcolWebAccess.Visible = true;
            this.gcolWebAccess.VisibleIndex = 2;
            this.gcolWebAccess.Width = 130;
            // 
            // gcolPriority
            // 
            this.gcolPriority.Caption = "Priority";
            this.gcolPriority.FieldName = "priority";
            this.gcolPriority.Name = "gcolPriority";
            this.gcolPriority.OptionsColumn.AllowEdit = false;
            this.gcolPriority.Visible = true;
            this.gcolPriority.VisibleIndex = 3;
            this.gcolPriority.Width = 100;
            // 
            // gcolStatus
            // 
            this.gcolStatus.Caption = "Status";
            this.gcolStatus.FieldName = "status";
            this.gcolStatus.Name = "gcolStatus";
            this.gcolStatus.OptionsColumn.AllowEdit = false;
            this.gcolStatus.Visible = true;
            this.gcolStatus.VisibleIndex = 4;
            this.gcolStatus.Width = 100;
            // 
            // gcolCampaignDescription
            // 
            this.gcolCampaignDescription.Caption = "Description";
            this.gcolCampaignDescription.FieldName = "description";
            this.gcolCampaignDescription.Name = "gcolCampaignDescription";
            this.gcolCampaignDescription.OptionsColumn.AllowEdit = false;
            this.gcolCampaignDescription.Visible = true;
            this.gcolCampaignDescription.VisibleIndex = 5;
            this.gcolCampaignDescription.Width = 330;
            // 
            // gcolOwnerId
            // 
            this.gcolOwnerId.FieldName = "owner_user_id";
            this.gcolOwnerId.Name = "gcolOwnerId";
            this.gcolOwnerId.Width = 20;
            // 
            // gcolWebAccessUserId
            // 
            this.gcolWebAccessUserId.FieldName = "web_access_user_id";
            this.gcolWebAccessUserId.Name = "gcolWebAccessUserId";
            this.gcolWebAccessUserId.Width = 20;
            // 
            // gcolCustomersId
            // 
            this.gcolCustomersId.FieldName = "customer_id";
            this.gcolCustomersId.Name = "gcolCustomersId";
            this.gcolCustomersId.Width = 20;
            // 
            // gcCustomer
            // 
            this.gcCustomer.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gcCustomer.Location = new System.Drawing.Point(12, 28);
            this.gcCustomer.MainView = this.gvCustomer;
            this.gcCustomer.Name = "gcCustomer";
            this.gcCustomer.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.riChkCustomerActive});
            this.gcCustomer.Size = new System.Drawing.Size(1106, 310);
            this.gcCustomer.TabIndex = 26;
            this.gcCustomer.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvCustomer});
            // 
            // gvCustomer
            // 
            this.gvCustomer.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gcolCustomerId,
            this.gcolCustomerName,
            this.gcolOrgNo,
            this.gcolReferenceNo,
            this.gcolActive,
            this.gcolOwner,
            this.gcolAddress,
            this.gcolDescription});
            this.gvCustomer.GridControl = this.gcCustomer;
            this.gvCustomer.Name = "gvCustomer";
            this.gvCustomer.OptionsFind.AlwaysVisible = true;
            this.gvCustomer.OptionsView.ColumnAutoWidth = false;
            this.gvCustomer.OptionsView.ShowGroupExpandCollapseButtons = false;
            this.gvCustomer.OptionsView.ShowGroupPanel = false;
            this.gvCustomer.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gvCustomer_PopupMenuShowing);
            this.gvCustomer.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvCustomer_FocusedRowChanged);
            this.gvCustomer.DoubleClick += new System.EventHandler(this.gvCustomer_DoubleClick);
            // 
            // gcolCustomerId
            // 
            this.gcolCustomerId.Caption = "Id";
            this.gcolCustomerId.FieldName = "id";
            this.gcolCustomerId.Name = "gcolCustomerId";
            this.gcolCustomerId.OptionsColumn.AllowEdit = false;
            this.gcolCustomerId.Width = 30;
            // 
            // gcolCustomerName
            // 
            this.gcolCustomerName.Caption = "Customer Name";
            this.gcolCustomerName.FieldName = "customer_name";
            this.gcolCustomerName.Name = "gcolCustomerName";
            this.gcolCustomerName.OptionsColumn.AllowEdit = false;
            this.gcolCustomerName.Visible = true;
            this.gcolCustomerName.VisibleIndex = 1;
            this.gcolCustomerName.Width = 150;
            // 
            // gcolOrgNo
            // 
            this.gcolOrgNo.Caption = "Org #";
            this.gcolOrgNo.FieldName = "org_no";
            this.gcolOrgNo.Name = "gcolOrgNo";
            this.gcolOrgNo.OptionsColumn.AllowEdit = false;
            this.gcolOrgNo.Visible = true;
            this.gcolOrgNo.VisibleIndex = 3;
            this.gcolOrgNo.Width = 100;
            // 
            // gcolReferenceNo
            // 
            this.gcolReferenceNo.Caption = "Reference #";
            this.gcolReferenceNo.FieldName = "reference_no";
            this.gcolReferenceNo.Name = "gcolReferenceNo";
            this.gcolReferenceNo.OptionsColumn.AllowEdit = false;
            this.gcolReferenceNo.Visible = true;
            this.gcolReferenceNo.VisibleIndex = 4;
            this.gcolReferenceNo.Width = 100;
            // 
            // gcolActive
            // 
            this.gcolActive.Caption = "Active";
            this.gcolActive.ColumnEdit = this.riChkCustomerActive;
            this.gcolActive.FieldName = "active";
            this.gcolActive.Name = "gcolActive";
            this.gcolActive.OptionsColumn.AllowEdit = false;
            this.gcolActive.Visible = true;
            this.gcolActive.VisibleIndex = 0;
            this.gcolActive.Width = 37;
            // 
            // riChkCustomerActive
            // 
            this.riChkCustomerActive.AutoHeight = false;
            this.riChkCustomerActive.DisplayValueChecked = "True";
            this.riChkCustomerActive.DisplayValueUnchecked = "False";
            this.riChkCustomerActive.Name = "riChkCustomerActive";
            this.riChkCustomerActive.CheckStateChanged += new System.EventHandler(this.riChkCustomerActive_CheckStateChanged);
            // 
            // gcolOwner
            // 
            this.gcolOwner.Caption = "Owner";
            this.gcolOwner.FieldName = "owner_name";
            this.gcolOwner.Name = "gcolOwner";
            this.gcolOwner.OptionsColumn.AllowEdit = false;
            this.gcolOwner.Visible = true;
            this.gcolOwner.VisibleIndex = 2;
            this.gcolOwner.Width = 150;
            // 
            // gcolAddress
            // 
            this.gcolAddress.Caption = "Address";
            this.gcolAddress.FieldName = "address";
            this.gcolAddress.Name = "gcolAddress";
            this.gcolAddress.OptionsColumn.AllowEdit = false;
            this.gcolAddress.Visible = true;
            this.gcolAddress.VisibleIndex = 5;
            this.gcolAddress.Width = 200;
            // 
            // gcolDescription
            // 
            this.gcolDescription.Caption = "Description";
            this.gcolDescription.FieldName = "description";
            this.gcolDescription.Name = "gcolDescription";
            this.gcolDescription.OptionsColumn.AllowEdit = false;
            this.gcolDescription.Visible = true;
            this.gcolDescription.VisibleIndex = 6;
            this.gcolDescription.Width = 200;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "Root";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.lciCustomerView,
            this.lciCampaignView,
            this.layoutControlItem1,
            this.emptySpaceItem1,
            this.layoutControlItem2,
            this.emptySpaceItem2});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(1130, 638);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // lciCustomerView
            // 
            this.lciCustomerView.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.lciCustomerView.AppearanceItemCaption.Options.UseFont = true;
            this.lciCustomerView.Control = this.gcCustomer;
            this.lciCustomerView.CustomizationFormText = "Customers:";
            this.lciCustomerView.Location = new System.Drawing.Point(0, 0);
            this.lciCustomerView.Name = "lciCustomerView";
            this.lciCustomerView.Size = new System.Drawing.Size(1110, 330);
            this.lciCustomerView.Text = "Customers";
            this.lciCustomerView.TextLocation = DevExpress.Utils.Locations.Top;
            this.lciCustomerView.TextSize = new System.Drawing.Size(62, 13);
            // 
            // lciCampaignView
            // 
            this.lciCampaignView.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.lciCampaignView.AppearanceItemCaption.Options.UseFont = true;
            this.lciCampaignView.Control = this.gcCustomerCampaign;
            this.lciCampaignView.CustomizationFormText = "Campaigns:";
            this.lciCampaignView.Location = new System.Drawing.Point(0, 356);
            this.lciCampaignView.Name = "lciCampaignView";
            this.lciCampaignView.Size = new System.Drawing.Size(1110, 236);
            this.lciCampaignView.Text = "Campaigns";
            this.lciCampaignView.TextLocation = DevExpress.Utils.Locations.Top;
            this.lciCampaignView.TextSize = new System.Drawing.Size(62, 13);
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.cmdAddCustomer;
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(950, 330);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(160, 26);
            this.layoutControlItem1.Text = "layoutControlItem1";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextToControlDistance = 0;
            this.layoutControlItem1.TextVisible = false;
            this.layoutControlItem1.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 330);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(950, 26);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.cmdAddCampaign;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(950, 592);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(160, 26);
            this.layoutControlItem2.Text = "layoutControlItem2";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextToControlDistance = 0;
            this.layoutControlItem2.TextVisible = false;
            this.layoutControlItem2.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            // 
            // emptySpaceItem2
            // 
            this.emptySpaceItem2.AllowHotTrack = false;
            this.emptySpaceItem2.CustomizationFormText = "emptySpaceItem2";
            this.emptySpaceItem2.Location = new System.Drawing.Point(0, 592);
            this.emptySpaceItem2.Name = "emptySpaceItem2";
            this.emptySpaceItem2.Size = new System.Drawing.Size(950, 26);
            this.emptySpaceItem2.Text = "emptySpaceItem2";
            this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
            // 
            // ManageCustomerCampaign
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lcCustomerCampaign);
            this.Name = "ManageCustomerCampaign";
            this.Size = new System.Drawing.Size(1130, 638);
            ((System.ComponentModel.ISupportInitialize)(this.lcCustomerCampaign)).EndInit();
            this.lcCustomerCampaign.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcCustomerCampaign)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvCustomerCampaign)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.riChkCustomerActive)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciCustomerView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciCampaignView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl lcCustomerCampaign;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraGrid.GridControl gcCustomer;
        private DevExpress.XtraGrid.Views.Grid.GridView gvCustomer;
        private DevExpress.XtraGrid.Columns.GridColumn gcolCustomerId;
        private DevExpress.XtraGrid.Columns.GridColumn gcolCustomerName;
        private DevExpress.XtraGrid.Columns.GridColumn gcolOrgNo;
        private DevExpress.XtraGrid.Columns.GridColumn gcolReferenceNo;
        private DevExpress.XtraGrid.Columns.GridColumn gcolActive;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit riChkCustomerActive;
        private DevExpress.XtraGrid.Columns.GridColumn gcolOwner;
        private DevExpress.XtraGrid.Columns.GridColumn gcolAddress;
        private DevExpress.XtraGrid.Columns.GridColumn gcolDescription;
        private DevExpress.XtraLayout.LayoutControlItem lciCustomerView;
        private DevExpress.XtraGrid.GridControl gcCustomerCampaign;
        private DevExpress.XtraGrid.Views.Grid.GridView gvCustomerCampaign;
        private DevExpress.XtraGrid.Columns.GridColumn gcolCampaignId;
        private DevExpress.XtraGrid.Columns.GridColumn gcolCampaignName;
        private DevExpress.XtraGrid.Columns.GridColumn gcolCampaignOwner;
        private DevExpress.XtraGrid.Columns.GridColumn gcolWebAccess;
        private DevExpress.XtraGrid.Columns.GridColumn gcolPriority;
        private DevExpress.XtraGrid.Columns.GridColumn gcolStatus;
        private DevExpress.XtraGrid.Columns.GridColumn gcolCampaignDescription;
        private DevExpress.XtraGrid.Columns.GridColumn gcolOwnerId;
        private DevExpress.XtraGrid.Columns.GridColumn gcolWebAccessUserId;
        private DevExpress.XtraGrid.Columns.GridColumn gcolCustomersId;
        private DevExpress.XtraLayout.LayoutControlItem lciCampaignView;
        private DevExpress.XtraEditors.SimpleButton cmdAddCustomer;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraEditors.SimpleButton cmdAddCampaign;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem2;
    }
}
