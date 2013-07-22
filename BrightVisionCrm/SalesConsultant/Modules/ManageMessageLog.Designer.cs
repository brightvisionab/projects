namespace SalesConsultant.Modules
{
    partial class ManageMessageLog
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
            this.tbxRecordsToShow = new DevExpress.XtraEditors.TextEdit();
            this.cmdRefreshData = new DevExpress.XtraEditors.SimpleButton();
            this.gcMessageLog = new DevExpress.XtraGrid.GridControl();
            this.gvMessageLog = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.textEditSearch = new DevExpress.XtraEditors.TextEdit();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.simpleButtonFind = new DevExpress.XtraEditors.SimpleButton();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem3 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.simpleButtonClear = new DevExpress.XtraEditors.SimpleButton();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.cbxShowOnlyMyMessages = new DevExpress.XtraEditors.CheckEdit();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem5 = new DevExpress.XtraLayout.EmptySpaceItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbxRecordsToShow.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcMessageLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvMessageLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditSearch.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxShowOnlyMyMessages.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem5)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomizationMenu = false;
            this.layoutControl1.Controls.Add(this.cbxShowOnlyMyMessages);
            this.layoutControl1.Controls.Add(this.simpleButtonClear);
            this.layoutControl1.Controls.Add(this.simpleButtonFind);
            this.layoutControl1.Controls.Add(this.textEditSearch);
            this.layoutControl1.Controls.Add(this.tbxRecordsToShow);
            this.layoutControl1.Controls.Add(this.cmdRefreshData);
            this.layoutControl1.Controls.Add(this.gcMessageLog);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(412, 316, 250, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(868, 580);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // tbxRecordsToShow
            // 
            this.tbxRecordsToShow.EditValue = "100";
            this.tbxRecordsToShow.Location = new System.Drawing.Point(685, 7);
            this.tbxRecordsToShow.Name = "tbxRecordsToShow";
            this.tbxRecordsToShow.Properties.Appearance.BackColor = System.Drawing.SystemColors.Info;
            this.tbxRecordsToShow.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.tbxRecordsToShow.Properties.Appearance.ForeColor = System.Drawing.Color.Maroon;
            this.tbxRecordsToShow.Properties.Appearance.Options.UseBackColor = true;
            this.tbxRecordsToShow.Properties.Appearance.Options.UseFont = true;
            this.tbxRecordsToShow.Properties.Appearance.Options.UseForeColor = true;
            this.tbxRecordsToShow.Properties.Appearance.Options.UseTextOptions = true;
            this.tbxRecordsToShow.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.tbxRecordsToShow.Properties.Mask.EditMask = "f0";
            this.tbxRecordsToShow.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.tbxRecordsToShow.Size = new System.Drawing.Size(77, 20);
            this.tbxRecordsToShow.StyleController = this.layoutControl1;
            this.tbxRecordsToShow.TabIndex = 6;
            this.tbxRecordsToShow.Tag = "";
            // 
            // cmdRefreshData
            // 
            this.cmdRefreshData.Location = new System.Drawing.Point(786, 7);
            this.cmdRefreshData.Name = "cmdRefreshData";
            this.cmdRefreshData.Size = new System.Drawing.Size(75, 22);
            this.cmdRefreshData.StyleController = this.layoutControl1;
            this.cmdRefreshData.TabIndex = 5;
            this.cmdRefreshData.Text = "Refresh Data";
            this.cmdRefreshData.Click += new System.EventHandler(this.cmdRefreshData_Click);
            // 
            // gcMessageLog
            // 
            this.gcMessageLog.Location = new System.Drawing.Point(7, 33);
            this.gcMessageLog.MainView = this.gvMessageLog;
            this.gcMessageLog.Name = "gcMessageLog";
            this.gcMessageLog.Size = new System.Drawing.Size(854, 491);
            this.gcMessageLog.TabIndex = 4;
            this.gcMessageLog.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvMessageLog});
            // 
            // gvMessageLog
            // 
            this.gvMessageLog.GridControl = this.gcMessageLog;
            this.gvMessageLog.Name = "gvMessageLog";
            this.gvMessageLog.OptionsBehavior.Editable = false;
            this.gvMessageLog.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gvMessageLog.OptionsView.ColumnAutoWidth = false;
            this.gvMessageLog.OptionsView.ShowGroupPanel = false;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.emptySpaceItem1,
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.emptySpaceItem3,
            this.layoutControlItem6,
            this.layoutControlItem7,
            this.layoutControlItem3,
            this.emptySpaceItem2,
            this.layoutControlItem2,
            this.emptySpaceItem5});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlGroup1.Size = new System.Drawing.Size(868, 580);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcMessageLog;
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 26);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(858, 495);
            this.layoutControlItem1.Text = "layoutControlItem1";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextToControlDistance = 0;
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.cmdRefreshData;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(779, 0);
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
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 521);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(858, 49);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.tbxRecordsToShow;
            this.layoutControlItem3.CustomizationFormText = "Records to show:";
            this.layoutControlItem3.Location = new System.Drawing.Point(591, 0);
            this.layoutControlItem3.MaxSize = new System.Drawing.Size(168, 24);
            this.layoutControlItem3.MinSize = new System.Drawing.Size(168, 24);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(168, 26);
            this.layoutControlItem3.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem3.Text = "Records to show:";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(84, 13);
            // 
            // emptySpaceItem2
            // 
            this.emptySpaceItem2.AllowHotTrack = false;
            this.emptySpaceItem2.CustomizationFormText = "emptySpaceItem2";
            this.emptySpaceItem2.Location = new System.Drawing.Point(759, 0);
            this.emptySpaceItem2.MaxSize = new System.Drawing.Size(20, 0);
            this.emptySpaceItem2.MinSize = new System.Drawing.Size(20, 10);
            this.emptySpaceItem2.Name = "emptySpaceItem2";
            this.emptySpaceItem2.Size = new System.Drawing.Size(20, 26);
            this.emptySpaceItem2.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.emptySpaceItem2.Text = "emptySpaceItem2";
            this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
            // 
            // textEditSearch
            // 
            this.textEditSearch.Location = new System.Drawing.Point(7, 7);
            this.textEditSearch.Name = "textEditSearch";
            this.textEditSearch.Size = new System.Drawing.Size(231, 20);
            this.textEditSearch.StyleController = this.layoutControl1;
            this.textEditSearch.TabIndex = 7;
            this.textEditSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textEditSearch_KeyDown);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.textEditSearch;
            this.layoutControlItem4.CustomizationFormText = "layoutControlItem4";
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(235, 26);
            this.layoutControlItem4.Text = "layoutControlItem4";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextToControlDistance = 0;
            this.layoutControlItem4.TextVisible = false;
            // 
            // simpleButtonFind
            // 
            this.simpleButtonFind.Location = new System.Drawing.Point(242, 7);
            this.simpleButtonFind.Name = "simpleButtonFind";
            this.simpleButtonFind.Size = new System.Drawing.Size(51, 22);
            this.simpleButtonFind.StyleController = this.layoutControl1;
            this.simpleButtonFind.TabIndex = 8;
            this.simpleButtonFind.Text = "Find";
            this.simpleButtonFind.Click += new System.EventHandler(this.simpleButtonFind_Click);
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.simpleButtonFind;
            this.layoutControlItem5.CustomizationFormText = "layoutControlItem5";
            this.layoutControlItem5.Location = new System.Drawing.Point(235, 0);
            this.layoutControlItem5.MaxSize = new System.Drawing.Size(55, 26);
            this.layoutControlItem5.MinSize = new System.Drawing.Size(55, 26);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(55, 26);
            this.layoutControlItem5.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem5.Text = "layoutControlItem5";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextToControlDistance = 0;
            this.layoutControlItem5.TextVisible = false;
            // 
            // emptySpaceItem3
            // 
            this.emptySpaceItem3.AllowHotTrack = false;
            this.emptySpaceItem3.CustomizationFormText = "emptySpaceItem3";
            this.emptySpaceItem3.Location = new System.Drawing.Point(345, 0);
            this.emptySpaceItem3.MaxSize = new System.Drawing.Size(63, 26);
            this.emptySpaceItem3.MinSize = new System.Drawing.Size(63, 26);
            this.emptySpaceItem3.Name = "emptySpaceItem3";
            this.emptySpaceItem3.Size = new System.Drawing.Size(63, 26);
            this.emptySpaceItem3.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.emptySpaceItem3.Text = "emptySpaceItem3";
            this.emptySpaceItem3.TextSize = new System.Drawing.Size(0, 0);
            // 
            // simpleButtonClear
            // 
            this.simpleButtonClear.Location = new System.Drawing.Point(297, 7);
            this.simpleButtonClear.Name = "simpleButtonClear";
            this.simpleButtonClear.Size = new System.Drawing.Size(51, 22);
            this.simpleButtonClear.StyleController = this.layoutControl1;
            this.simpleButtonClear.TabIndex = 7;
            this.simpleButtonClear.Text = "Clear";
            this.simpleButtonClear.Click += new System.EventHandler(this.simpleButtonClear_Click);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.simpleButtonClear;
            this.layoutControlItem6.CustomizationFormText = "layoutControlItem6";
            this.layoutControlItem6.Location = new System.Drawing.Point(290, 0);
            this.layoutControlItem6.MaxSize = new System.Drawing.Size(55, 26);
            this.layoutControlItem6.MinSize = new System.Drawing.Size(55, 26);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(55, 26);
            this.layoutControlItem6.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem6.Text = "layoutControlItem6";
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextToControlDistance = 0;
            this.layoutControlItem6.TextVisible = false;
            // 
            // cbxShowOnlyMyMessages
            // 
            this.cbxShowOnlyMyMessages.Location = new System.Drawing.Point(415, 7);
            this.cbxShowOnlyMyMessages.Name = "cbxShowOnlyMyMessages";
            this.cbxShowOnlyMyMessages.Properties.Caption = "Show only my messages";
            this.cbxShowOnlyMyMessages.Size = new System.Drawing.Size(139, 19);
            this.cbxShowOnlyMyMessages.StyleController = this.layoutControl1;
            this.cbxShowOnlyMyMessages.TabIndex = 49;
            this.cbxShowOnlyMyMessages.EditValueChanged += new System.EventHandler(this.cbxShowOnlyMyMessages_EditValueChanged);
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.Control = this.cbxShowOnlyMyMessages;
            this.layoutControlItem7.CustomizationFormText = "layoutControlItem7";
            this.layoutControlItem7.Location = new System.Drawing.Point(408, 0);
            this.layoutControlItem7.MaxSize = new System.Drawing.Size(143, 26);
            this.layoutControlItem7.MinSize = new System.Drawing.Size(143, 26);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Size = new System.Drawing.Size(143, 26);
            this.layoutControlItem7.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem7.Text = "layoutControlItem7";
            this.layoutControlItem7.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem7.TextToControlDistance = 0;
            this.layoutControlItem7.TextVisible = false;
            // 
            // emptySpaceItem5
            // 
            this.emptySpaceItem5.AllowHotTrack = false;
            this.emptySpaceItem5.CustomizationFormText = "emptySpaceItem5";
            this.emptySpaceItem5.Location = new System.Drawing.Point(551, 0);
            this.emptySpaceItem5.Name = "emptySpaceItem5";
            this.emptySpaceItem5.Size = new System.Drawing.Size(40, 26);
            this.emptySpaceItem5.Text = "emptySpaceItem5";
            this.emptySpaceItem5.TextSize = new System.Drawing.Size(0, 0);
            // 
            // ManageMessageLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Name = "ManageMessageLog";
            this.Size = new System.Drawing.Size(868, 580);
            this.Load += new System.EventHandler(this.ManageMessageLog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tbxRecordsToShow.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcMessageLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvMessageLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditSearch.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxShowOnlyMyMessages.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem5)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraGrid.GridControl gcMessageLog;
        private DevExpress.XtraGrid.Views.Grid.GridView gvMessageLog;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraEditors.SimpleButton cmdRefreshData;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraEditors.TextEdit tbxRecordsToShow;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem2;
        private DevExpress.XtraEditors.TextEdit textEditSearch;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraEditors.SimpleButton simpleButtonFind;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem3;
        private DevExpress.XtraEditors.SimpleButton simpleButtonClear;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraEditors.CheckEdit cbxShowOnlyMyMessages;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem5;
    }
}
