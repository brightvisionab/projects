namespace ManagerApplication.Modules
{
    partial class ManageEventLog
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
            this.gcEventLog = new DevExpress.XtraGrid.GridControl();
            this.gvEventLog = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbxRecordsToShow.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcEventLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvEventLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomizationMenu = false;
            this.layoutControl1.Controls.Add(this.tbxRecordsToShow);
            this.layoutControl1.Controls.Add(this.cmdRefreshData);
            this.layoutControl1.Controls.Add(this.gcEventLog);
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
            this.tbxRecordsToShow.Location = new System.Drawing.Point(686, 551);
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
            this.tbxRecordsToShow.Size = new System.Drawing.Size(76, 20);
            this.tbxRecordsToShow.StyleController = this.layoutControl1;
            this.tbxRecordsToShow.TabIndex = 6;
            this.tbxRecordsToShow.Tag = "";
            // 
            // cmdRefreshData
            // 
            this.cmdRefreshData.Location = new System.Drawing.Point(786, 551);
            this.cmdRefreshData.Name = "cmdRefreshData";
            this.cmdRefreshData.Size = new System.Drawing.Size(75, 22);
            this.cmdRefreshData.StyleController = this.layoutControl1;
            this.cmdRefreshData.TabIndex = 5;
            this.cmdRefreshData.Text = "Refresh Data";
            this.cmdRefreshData.Click += new System.EventHandler(this.cmdRefreshData_Click);
            // 
            // gcEventLog
            // 
            this.gcEventLog.Location = new System.Drawing.Point(7, 7);
            this.gcEventLog.MainView = this.gvEventLog;
            this.gcEventLog.Name = "gcEventLog";
            this.gcEventLog.Size = new System.Drawing.Size(854, 540);
            this.gcEventLog.TabIndex = 4;
            this.gcEventLog.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvEventLog});
            // 
            // gvEventLog
            // 
            this.gvEventLog.GridControl = this.gcEventLog;
            this.gvEventLog.Name = "gvEventLog";
            this.gvEventLog.OptionsBehavior.Editable = false;
            this.gvEventLog.OptionsFind.AlwaysVisible = true;
            this.gvEventLog.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gvEventLog.OptionsView.ColumnAutoWidth = false;
            this.gvEventLog.OptionsView.ShowGroupPanel = false;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.emptySpaceItem1,
            this.layoutControlItem3,
            this.emptySpaceItem2});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlGroup1.Size = new System.Drawing.Size(868, 580);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcEventLog;
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(858, 544);
            this.layoutControlItem1.Text = "layoutControlItem1";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextToControlDistance = 0;
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.cmdRefreshData;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(779, 544);
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
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 544);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(591, 26);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.tbxRecordsToShow;
            this.layoutControlItem3.CustomizationFormText = "Records to show:";
            this.layoutControlItem3.Location = new System.Drawing.Point(591, 544);
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
            this.emptySpaceItem2.Location = new System.Drawing.Point(759, 544);
            this.emptySpaceItem2.MaxSize = new System.Drawing.Size(20, 0);
            this.emptySpaceItem2.MinSize = new System.Drawing.Size(20, 10);
            this.emptySpaceItem2.Name = "emptySpaceItem2";
            this.emptySpaceItem2.Size = new System.Drawing.Size(20, 26);
            this.emptySpaceItem2.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.emptySpaceItem2.Text = "emptySpaceItem2";
            this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
            // 
            // ManageEventLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Name = "ManageEventLog";
            this.Size = new System.Drawing.Size(868, 580);
            this.Load += new System.EventHandler(this.ManageEventLog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tbxRecordsToShow.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcEventLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvEventLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraGrid.GridControl gcEventLog;
        private DevExpress.XtraGrid.Views.Grid.GridView gvEventLog;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraEditors.SimpleButton cmdRefreshData;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraEditors.TextEdit tbxRecordsToShow;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem2;
    }
}
