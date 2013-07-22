namespace SalesConsultant.Modules
{
    partial class CollectedData
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
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.cbxCustomerOwned = new DevExpress.XtraEditors.CheckEdit();
            this.btnReload = new DevExpress.XtraEditors.SimpleButton();
            this.cbxBrightvisionOwned = new DevExpress.XtraEditors.CheckEdit();
            this.cmdShowSelectedData = new DevExpress.XtraEditors.SimpleButton();
            this.cmdShowAllData = new DevExpress.XtraEditors.SimpleButton();
            this.gcCollectedData = new DevExpress.XtraGrid.GridControl();
            this.gvCollectedData = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbxCustomerOwned.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxBrightvisionOwned.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcCollectedData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvCollectedData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomizationMenu = false;
            this.layoutControl1.Controls.Add(this.cbxCustomerOwned);
            this.layoutControl1.Controls.Add(this.btnReload);
            this.layoutControl1.Controls.Add(this.cbxBrightvisionOwned);
            this.layoutControl1.Controls.Add(this.cmdShowSelectedData);
            this.layoutControl1.Controls.Add(this.cmdShowAllData);
            this.layoutControl1.Controls.Add(this.gcCollectedData);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(517, 181, 250, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(573, 392);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // cbxCustomerOwned
            // 
            this.cbxCustomerOwned.EditValue = true;
            this.cbxCustomerOwned.Enabled = false;
            this.cbxCustomerOwned.Location = new System.Drawing.Point(392, 2);
            this.cbxCustomerOwned.Name = "cbxCustomerOwned";
            this.cbxCustomerOwned.Properties.Caption = "Customer Data";
            this.cbxCustomerOwned.Properties.ReadOnly = true;
            this.cbxCustomerOwned.Size = new System.Drawing.Size(95, 19);
            this.cbxCustomerOwned.StyleController = this.layoutControl1;
            this.cbxCustomerOwned.TabIndex = 47;
            this.cbxCustomerOwned.CheckedChanged += new System.EventHandler(this.cbxCustomerOwned_CheckedChanged);
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(505, 368);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(66, 22);
            this.btnReload.StyleController = this.layoutControl1;
            this.btnReload.TabIndex = 46;
            this.btnReload.Text = "Reload";
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // cbxBrightvisionOwned
            // 
            this.cbxBrightvisionOwned.EditValue = true;
            this.cbxBrightvisionOwned.Enabled = false;
            this.cbxBrightvisionOwned.Location = new System.Drawing.Point(491, 2);
            this.cbxBrightvisionOwned.Name = "cbxBrightvisionOwned";
            this.cbxBrightvisionOwned.Properties.Caption = "Public Data";
            this.cbxBrightvisionOwned.Properties.ReadOnly = true;
            this.cbxBrightvisionOwned.Size = new System.Drawing.Size(80, 19);
            this.cbxBrightvisionOwned.StyleController = this.layoutControl1;
            this.cbxBrightvisionOwned.TabIndex = 45;
            this.cbxBrightvisionOwned.CheckedChanged += new System.EventHandler(this.cbxBrightvisionOwned_CheckedChanged);
            // 
            // cmdShowSelectedData
            // 
            this.cmdShowSelectedData.Enabled = false;
            this.cmdShowSelectedData.Location = new System.Drawing.Point(81, 2);
            this.cmdShowSelectedData.Name = "cmdShowSelectedData";
            this.cmdShowSelectedData.Size = new System.Drawing.Size(164, 22);
            this.cmdShowSelectedData.StyleController = this.layoutControl1;
            this.cmdShowSelectedData.TabIndex = 44;
            this.cmdShowSelectedData.Text = "Show data from seleted contact";
            this.cmdShowSelectedData.Click += new System.EventHandler(this.cmdShowSelectedData_Click);
            // 
            // cmdShowAllData
            // 
            this.cmdShowAllData.Enabled = false;
            this.cmdShowAllData.Location = new System.Drawing.Point(2, 2);
            this.cmdShowAllData.Name = "cmdShowAllData";
            this.cmdShowAllData.Size = new System.Drawing.Size(75, 22);
            this.cmdShowAllData.StyleController = this.layoutControl1;
            this.cmdShowAllData.TabIndex = 43;
            this.cmdShowAllData.Text = "Show all data";
            this.cmdShowAllData.Click += new System.EventHandler(this.cmdShowAllData_Click);
            // 
            // gcCollectedData
            // 
            this.gcCollectedData.Location = new System.Drawing.Point(2, 28);
            this.gcCollectedData.MainView = this.gvCollectedData;
            this.gcCollectedData.Name = "gcCollectedData";
            this.gcCollectedData.Size = new System.Drawing.Size(569, 336);
            this.gcCollectedData.TabIndex = 42;
            this.gcCollectedData.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvCollectedData});
            // 
            // gvCollectedData
            // 
            this.gvCollectedData.GridControl = this.gcCollectedData;
            this.gvCollectedData.Name = "gvCollectedData";
            this.gvCollectedData.OptionsBehavior.Editable = false;
            this.gvCollectedData.OptionsFind.FindMode = DevExpress.XtraEditors.FindMode.Always;
            this.gvCollectedData.OptionsView.AllowCellMerge = true;
            this.gvCollectedData.OptionsView.ColumnAutoWidth = false;
            this.gvCollectedData.CellMerge += new DevExpress.XtraGrid.Views.Grid.CellMergeEventHandler(this.gvCollectedData_CellMerge);
            this.gvCollectedData.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gvCollectedData_PopupMenuShowing);
            this.gvCollectedData.CalcRowHeight += new DevExpress.XtraGrid.Views.Grid.RowHeightEventHandler(this.gvCollectedData_CalcRowHeight);
            this.gvCollectedData.EndGrouping += new System.EventHandler(this.gvCollectedData_EndGrouping);
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
            this.emptySpaceItem1,
            this.layoutControlItem4,
            this.emptySpaceItem2,
            this.layoutControlItem5,
            this.layoutControlItem6});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlGroup1.Size = new System.Drawing.Size(573, 392);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcCollectedData;
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 26);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(573, 340);
            this.layoutControlItem1.Text = "layoutControlItem1";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextToControlDistance = 0;
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.cmdShowAllData;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem2.MaxSize = new System.Drawing.Size(79, 26);
            this.layoutControlItem2.MinSize = new System.Drawing.Size(79, 26);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(79, 26);
            this.layoutControlItem2.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem2.Text = "layoutControlItem2";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextToControlDistance = 0;
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.cmdShowSelectedData;
            this.layoutControlItem3.CustomizationFormText = "layoutControlItem3";
            this.layoutControlItem3.Location = new System.Drawing.Point(79, 0);
            this.layoutControlItem3.MaxSize = new System.Drawing.Size(168, 26);
            this.layoutControlItem3.MinSize = new System.Drawing.Size(168, 26);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(168, 26);
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
            this.emptySpaceItem1.Location = new System.Drawing.Point(247, 0);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(143, 26);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.cbxBrightvisionOwned;
            this.layoutControlItem4.CustomizationFormText = "layoutControlItem4";
            this.layoutControlItem4.Location = new System.Drawing.Point(489, 0);
            this.layoutControlItem4.MaxSize = new System.Drawing.Size(84, 23);
            this.layoutControlItem4.MinSize = new System.Drawing.Size(84, 23);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(84, 26);
            this.layoutControlItem4.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem4.Text = "layoutControlItem4";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextToControlDistance = 0;
            this.layoutControlItem4.TextVisible = false;
            this.layoutControlItem4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            // 
            // emptySpaceItem2
            // 
            this.emptySpaceItem2.AllowHotTrack = false;
            this.emptySpaceItem2.CustomizationFormText = "emptySpaceItem2";
            this.emptySpaceItem2.Location = new System.Drawing.Point(0, 366);
            this.emptySpaceItem2.Name = "emptySpaceItem2";
            this.emptySpaceItem2.Size = new System.Drawing.Size(503, 26);
            this.emptySpaceItem2.Text = "emptySpaceItem2";
            this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.btnReload;
            this.layoutControlItem5.CustomizationFormText = "layoutControlItem5";
            this.layoutControlItem5.Location = new System.Drawing.Point(503, 366);
            this.layoutControlItem5.MaxSize = new System.Drawing.Size(70, 26);
            this.layoutControlItem5.MinSize = new System.Drawing.Size(70, 26);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(70, 26);
            this.layoutControlItem5.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem5.Text = "layoutControlItem5";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextToControlDistance = 0;
            this.layoutControlItem5.TextVisible = false;
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.cbxCustomerOwned;
            this.layoutControlItem6.CustomizationFormText = "layoutControlItem6";
            this.layoutControlItem6.Location = new System.Drawing.Point(390, 0);
            this.layoutControlItem6.MaxSize = new System.Drawing.Size(99, 23);
            this.layoutControlItem6.MinSize = new System.Drawing.Size(99, 23);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(99, 26);
            this.layoutControlItem6.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem6.Text = "layoutControlItem6";
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextToControlDistance = 0;
            this.layoutControlItem6.TextVisible = false;
            this.layoutControlItem6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            // 
            // CollectedData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Name = "CollectedData";
            this.Size = new System.Drawing.Size(573, 392);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cbxCustomerOwned.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxBrightvisionOwned.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcCollectedData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvCollectedData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraGrid.GridControl gcCollectedData;
        private DevExpress.XtraGrid.Views.Grid.GridView gvCollectedData;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraEditors.SimpleButton cmdShowSelectedData;
        private DevExpress.XtraEditors.SimpleButton cmdShowAllData;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        public DevExpress.XtraEditors.CheckEdit cbxBrightvisionOwned;
        private DevExpress.XtraEditors.SimpleButton btnReload;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraEditors.CheckEdit cbxCustomerOwned;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
    }
}
