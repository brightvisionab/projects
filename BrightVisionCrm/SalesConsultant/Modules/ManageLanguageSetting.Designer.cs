namespace SalesConsultant.Modules
{
    partial class ManageLanguageSetting
    {

        #region Component Designer generated code


        #endregion

        private DevExpress.XtraLayout.LayoutControl lcManageLanguage;
        private DevExpress.XtraEditors.SimpleButton cmdSave;
        private DevExpress.XtraGrid.GridControl gcLanguage;
        private DevExpress.XtraGrid.Views.Grid.GridView gvLanguage;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraEditors.SimpleButton cmdAdd;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;

        private void InitializeComponent()
        {
            this.lcManageLanguage = new DevExpress.XtraLayout.LayoutControl();
            this.cmdSave = new DevExpress.XtraEditors.SimpleButton();
            this.gcLanguage = new DevExpress.XtraGrid.GridControl();
            this.gvLanguage = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.txtLanguageName = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.cmdAdd = new DevExpress.XtraEditors.SimpleButton();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            ((System.ComponentModel.ISupportInitialize)(this.lcManageLanguage)).BeginInit();
            this.lcManageLanguage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcLanguage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvLanguage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLanguageName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            this.SuspendLayout();
            // 
            // lcManageLanguage
            // 
            this.lcManageLanguage.AllowCustomizationMenu = false;
            this.lcManageLanguage.Controls.Add(this.cmdSave);
            this.lcManageLanguage.Controls.Add(this.gcLanguage);
            this.lcManageLanguage.Controls.Add(this.cmdAdd);
            this.lcManageLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lcManageLanguage.Location = new System.Drawing.Point(0, 0);
            this.lcManageLanguage.Name = "lcManageLanguage";
            this.lcManageLanguage.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(549, 340, 250, 350);
            this.lcManageLanguage.Root = this.layoutControlGroup1;
            this.lcManageLanguage.Size = new System.Drawing.Size(757, 551);
            this.lcManageLanguage.TabIndex = 27;
            this.lcManageLanguage.Text = "layoutControl1";
            this.lcManageLanguage.AllowCustomizationMenu = false;
            // 
            // cmdSave
            // 
            this.cmdSave.Location = new System.Drawing.Point(69, 12);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(80, 22);
            this.cmdSave.StyleController = this.lcManageLanguage;
            this.cmdSave.TabIndex = 6;
            this.cmdSave.Text = "Save Changes";
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // gcLanguage
            // 
            this.gcLanguage.Location = new System.Drawing.Point(12, 38);
            this.gcLanguage.MainView = this.gvLanguage;
            this.gcLanguage.Name = "gcLanguage";
            this.gcLanguage.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.txtLanguageName});
            this.gcLanguage.Size = new System.Drawing.Size(733, 501);
            this.gcLanguage.TabIndex = 5;
            this.gcLanguage.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvLanguage});
            // 
            // gvLanguage
            // 
            this.gvLanguage.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn1,
            this.gridColumn2,
            this.gridColumn3});
            this.gvLanguage.GridControl = this.gcLanguage;
            this.gvLanguage.Name = "gvLanguage";
            this.gvLanguage.OptionsFind.AlwaysVisible = true;
            this.gvLanguage.OptionsView.ColumnAutoWidth = false;
            this.gvLanguage.OptionsView.ShowGroupPanel = false;
            this.gvLanguage.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gvLanguage_PopupMenuShowing);
            this.gvLanguage.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvLanguage_FocusedRowChanged);
            this.gvLanguage.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gvLanguage_ValidatingEditor);
            // 
            // gridColumn1
            // 
            this.gridColumn1.Caption = "Id";
            this.gridColumn1.FieldName = "id";
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.OptionsColumn.AllowEdit = false;
            this.gridColumn1.Width = 40;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "Language Code";
            this.gridColumn2.ColumnEdit = this.txtLanguageName;
            this.gridColumn2.FieldName = "code";
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 0;
            this.gridColumn2.Width = 100;
            // 
            // txtLanguageName
            // 
            this.txtLanguageName.AutoHeight = false;
            this.txtLanguageName.MaxLength = 10;
            this.txtLanguageName.Name = "txtLanguageName";
            // 
            // gridColumn3
            // 
            this.gridColumn3.Caption = "Description";
            this.gridColumn3.FieldName = "description";
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.Visible = true;
            this.gridColumn3.VisibleIndex = 1;
            this.gridColumn3.Width = 150;
            // 
            // cmdAdd
            // 
            this.cmdAdd.Location = new System.Drawing.Point(12, 12);
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(53, 22);
            this.cmdAdd.StyleController = this.lcManageLanguage;
            this.cmdAdd.TabIndex = 4;
            this.cmdAdd.Text = "Add";
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "Root";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.emptySpaceItem1});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(757, 551);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.cmdAdd;
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.MaxSize = new System.Drawing.Size(57, 26);
            this.layoutControlItem1.MinSize = new System.Drawing.Size(57, 26);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(57, 26);
            this.layoutControlItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem1.Text = "layoutControlItem1";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextToControlDistance = 0;
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.gcLanguage;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 26);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(737, 505);
            this.layoutControlItem2.Text = "layoutControlItem2";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextToControlDistance = 0;
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.cmdSave;
            this.layoutControlItem3.CustomizationFormText = "layoutControlItem3";
            this.layoutControlItem3.Location = new System.Drawing.Point(57, 0);
            this.layoutControlItem3.MaxSize = new System.Drawing.Size(84, 26);
            this.layoutControlItem3.MinSize = new System.Drawing.Size(84, 26);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(84, 26);
            this.layoutControlItem3.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem3.Text = "layoutControlItem3";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextToControlDistance = 0;
            this.layoutControlItem3.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem1.Location = new System.Drawing.Point(141, 0);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(596, 26);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // ManageLanguageSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lcManageLanguage);
            this.Name = "ManageLanguageSetting";
            this.Size = new System.Drawing.Size(757, 551);
            ((System.ComponentModel.ISupportInitialize)(this.lcManageLanguage)).EndInit();
            this.lcManageLanguage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcLanguage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvLanguage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLanguageName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            this.ResumeLayout(false);

        }

        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit txtLanguageName;
    }
}
