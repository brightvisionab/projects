namespace ManagerApplication.Modules
{
    partial class AddSubCampaign
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
            this.lcAddSubCampaign = new DevExpress.XtraLayout.LayoutControl();
            this.cboStatus = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cboPriority = new DevExpress.XtraEditors.ComboBoxEdit();
            this.dpEndDate = new DevExpress.XtraEditors.DateEdit();
            this.dpStartDate = new DevExpress.XtraEditors.DateEdit();
            this.cboOwner = new DevExpress.XtraEditors.LookUpEdit();
            this.cboCampaign = new DevExpress.XtraEditors.LookUpEdit();
            this.cboCustomer = new DevExpress.XtraEditors.LookUpEdit();
            this.cmdCancel = new DevExpress.XtraEditors.SimpleButton();
            this.cmdSave = new DevExpress.XtraEditors.SimpleButton();
            this.txtDescription = new DevExpress.XtraEditors.MemoEdit();
            this.txtName = new DevExpress.XtraEditors.TextEdit();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem10 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem11 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem12 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem8 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.lcAddSubCampaign)).BeginInit();
            this.lcAddSubCampaign.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboStatus.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboPriority.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dpEndDate.Properties.VistaTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dpEndDate.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dpStartDate.Properties.VistaTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dpStartDate.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboOwner.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCampaign.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCustomer.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).BeginInit();
            this.SuspendLayout();
            // 
            // lcAddSubCampaign
            // 
            this.lcAddSubCampaign.Controls.Add(this.cboStatus);
            this.lcAddSubCampaign.Controls.Add(this.cboPriority);
            this.lcAddSubCampaign.Controls.Add(this.dpEndDate);
            this.lcAddSubCampaign.Controls.Add(this.dpStartDate);
            this.lcAddSubCampaign.Controls.Add(this.cboOwner);
            this.lcAddSubCampaign.Controls.Add(this.cboCampaign);
            this.lcAddSubCampaign.Controls.Add(this.cboCustomer);
            this.lcAddSubCampaign.Controls.Add(this.cmdCancel);
            this.lcAddSubCampaign.Controls.Add(this.cmdSave);
            this.lcAddSubCampaign.Controls.Add(this.txtDescription);
            this.lcAddSubCampaign.Controls.Add(this.txtName);
            this.lcAddSubCampaign.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lcAddSubCampaign.Location = new System.Drawing.Point(0, 0);
            this.lcAddSubCampaign.Name = "lcAddSubCampaign";
            this.lcAddSubCampaign.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(679, 151, 250, 350);
            this.lcAddSubCampaign.Root = this.layoutControlGroup1;
            this.lcAddSubCampaign.Size = new System.Drawing.Size(326, 336);
            this.lcAddSubCampaign.TabIndex = 0;
            this.lcAddSubCampaign.Text = "layoutControl1";
            this.lcAddSubCampaign.AllowCustomizationMenu = false;
            // 
            // cboStatus
            // 
            this.cboStatus.EditValue = "Active";
            this.cboStatus.Location = new System.Drawing.Point(73, 180);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboStatus.Properties.Items.AddRange(new object[] {
            "Confirmed",
            "In Development",
            "Active",
            "On Hold",
            "Archived",
            "Deleted"});
            this.cboStatus.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboStatus.Size = new System.Drawing.Size(241, 20);
            this.cboStatus.StyleController = this.lcAddSubCampaign;
            this.cboStatus.TabIndex = 30;
            // 
            // cboPriority
            // 
            this.cboPriority.EditValue = "3";
            this.cboPriority.Location = new System.Drawing.Point(73, 156);
            this.cboPriority.Name = "cboPriority";
            this.cboPriority.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboPriority.Properties.Items.AddRange(new object[] {
            "3",
            "2",
            "1",
            "0",
            "-1"});
            this.cboPriority.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboPriority.Size = new System.Drawing.Size(241, 20);
            this.cboPriority.StyleController = this.lcAddSubCampaign;
            this.cboPriority.TabIndex = 29;
            // 
            // dpEndDate
            // 
            this.dpEndDate.EditValue = null;
            this.dpEndDate.Location = new System.Drawing.Point(73, 132);
            this.dpEndDate.Name = "dpEndDate";
            this.dpEndDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dpEndDate.Properties.Mask.EditMask = "yyyy-MM-dd";
            this.dpEndDate.Properties.VistaTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.dpEndDate.Size = new System.Drawing.Size(241, 20);
            this.dpEndDate.StyleController = this.lcAddSubCampaign;
            this.dpEndDate.TabIndex = 28;
            this.dpEndDate.EditValueChanged += new System.EventHandler(this.dpEndDate_EditValueChanged);
            // 
            // dpStartDate
            // 
            this.dpStartDate.EditValue = null;
            this.dpStartDate.Location = new System.Drawing.Point(73, 108);
            this.dpStartDate.Name = "dpStartDate";
            this.dpStartDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dpStartDate.Properties.Mask.EditMask = "yyyy-MM-dd";
            this.dpStartDate.Properties.VistaTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.dpStartDate.Size = new System.Drawing.Size(241, 20);
            this.dpStartDate.StyleController = this.lcAddSubCampaign;
            this.dpStartDate.TabIndex = 27;
            this.dpStartDate.EditValueChanged += new System.EventHandler(this.dpStartDate_EditValueChanged);
            // 
            // cboOwner
            // 
            this.cboOwner.Location = new System.Drawing.Point(73, 84);
            this.cboOwner.Name = "cboOwner";
            this.cboOwner.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboOwner.Properties.NullText = "";
            this.cboOwner.Properties.ShowFooter = false;
            this.cboOwner.Properties.ShowHeader = false;
            this.cboOwner.Properties.ShowLines = false;
            this.cboOwner.Size = new System.Drawing.Size(241, 20);
            this.cboOwner.StyleController = this.lcAddSubCampaign;
            this.cboOwner.TabIndex = 26;
            // 
            // cboCampaign
            // 
            this.cboCampaign.Location = new System.Drawing.Point(73, 60);
            this.cboCampaign.Name = "cboCampaign";
            this.cboCampaign.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboCampaign.Properties.NullText = "";
            this.cboCampaign.Properties.ShowFooter = false;
            this.cboCampaign.Properties.ShowHeader = false;
            this.cboCampaign.Properties.ShowLines = false;
            this.cboCampaign.Size = new System.Drawing.Size(241, 20);
            this.cboCampaign.StyleController = this.lcAddSubCampaign;
            this.cboCampaign.TabIndex = 25;
            // 
            // cboCustomer
            // 
            this.cboCustomer.Location = new System.Drawing.Point(73, 36);
            this.cboCustomer.Name = "cboCustomer";
            this.cboCustomer.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboCustomer.Properties.NullText = "";
            this.cboCustomer.Properties.ShowFooter = false;
            this.cboCustomer.Properties.ShowHeader = false;
            this.cboCustomer.Properties.ShowLines = false;
            this.cboCustomer.Size = new System.Drawing.Size(241, 20);
            this.cboCustomer.StyleController = this.lcAddSubCampaign;
            this.cboCustomer.TabIndex = 24;
            this.cboCustomer.EditValueChanged += new System.EventHandler(this.cboCustomer_EditValueChanged);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Location = new System.Drawing.Point(165, 302);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(149, 22);
            this.cmdCancel.StyleController = this.lcAddSubCampaign;
            this.cmdCancel.TabIndex = 15;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdSave
            // 
            this.cmdSave.Location = new System.Drawing.Point(12, 302);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(149, 22);
            this.cmdSave.StyleController = this.lcAddSubCampaign;
            this.cmdSave.TabIndex = 14;
            this.cmdSave.Text = "Save";
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(73, 204);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(241, 94);
            this.txtDescription.StyleController = this.lcAddSubCampaign;
            this.txtDescription.TabIndex = 13;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(73, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(241, 20);
            this.txtName.StyleController = this.lcAddSubCampaign;
            this.txtName.TabIndex = 4;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem10,
            this.layoutControlItem11,
            this.layoutControlItem12,
            this.layoutControlItem5,
            this.layoutControlItem3,
            this.layoutControlItem2,
            this.layoutControlItem4,
            this.layoutControlItem6,
            this.layoutControlItem7,
            this.layoutControlItem8});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(326, 336);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.txtName;
            this.layoutControlItem1.CustomizationFormText = "Subcampaign Name:";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(306, 24);
            this.layoutControlItem1.Text = "Name:";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(57, 13);
            // 
            // layoutControlItem10
            // 
            this.layoutControlItem10.Control = this.txtDescription;
            this.layoutControlItem10.CustomizationFormText = "Description:";
            this.layoutControlItem10.Location = new System.Drawing.Point(0, 192);
            this.layoutControlItem10.Name = "layoutControlItem10";
            this.layoutControlItem10.Size = new System.Drawing.Size(306, 98);
            this.layoutControlItem10.Text = "Description:";
            this.layoutControlItem10.TextSize = new System.Drawing.Size(57, 13);
            // 
            // layoutControlItem11
            // 
            this.layoutControlItem11.Control = this.cmdSave;
            this.layoutControlItem11.CustomizationFormText = "layoutControlItem11";
            this.layoutControlItem11.Location = new System.Drawing.Point(0, 290);
            this.layoutControlItem11.Name = "layoutControlItem11";
            this.layoutControlItem11.Size = new System.Drawing.Size(153, 26);
            this.layoutControlItem11.Text = "layoutControlItem11";
            this.layoutControlItem11.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem11.TextToControlDistance = 0;
            this.layoutControlItem11.TextVisible = false;
            // 
            // layoutControlItem12
            // 
            this.layoutControlItem12.Control = this.cmdCancel;
            this.layoutControlItem12.CustomizationFormText = "layoutControlItem12";
            this.layoutControlItem12.Location = new System.Drawing.Point(153, 290);
            this.layoutControlItem12.Name = "layoutControlItem12";
            this.layoutControlItem12.Size = new System.Drawing.Size(153, 26);
            this.layoutControlItem12.Text = "layoutControlItem12";
            this.layoutControlItem12.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem12.TextToControlDistance = 0;
            this.layoutControlItem12.TextVisible = false;
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.cboCustomer;
            this.layoutControlItem5.CustomizationFormText = "Customer:";
            this.layoutControlItem5.Location = new System.Drawing.Point(0, 24);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(306, 24);
            this.layoutControlItem5.Text = "Customer:";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(57, 13);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.cboCampaign;
            this.layoutControlItem3.CustomizationFormText = "Campaign:";
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 48);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(306, 24);
            this.layoutControlItem3.Text = "Campaign:";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(57, 13);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.cboOwner;
            this.layoutControlItem2.CustomizationFormText = "Owner:";
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 72);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(306, 24);
            this.layoutControlItem2.Text = "Owner:";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(57, 13);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.dpStartDate;
            this.layoutControlItem4.CustomizationFormText = "Start Date:";
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 96);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(306, 24);
            this.layoutControlItem4.Text = "Start Date:";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(57, 13);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.dpEndDate;
            this.layoutControlItem6.CustomizationFormText = "End Date:";
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 120);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(306, 24);
            this.layoutControlItem6.Text = "End Date:";
            this.layoutControlItem6.TextSize = new System.Drawing.Size(57, 13);
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.Control = this.cboPriority;
            this.layoutControlItem7.CustomizationFormText = "Priority:";
            this.layoutControlItem7.Location = new System.Drawing.Point(0, 144);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Size = new System.Drawing.Size(306, 24);
            this.layoutControlItem7.Text = "Priority:";
            this.layoutControlItem7.TextSize = new System.Drawing.Size(57, 13);
            // 
            // layoutControlItem8
            // 
            this.layoutControlItem8.Control = this.cboStatus;
            this.layoutControlItem8.CustomizationFormText = "Status:";
            this.layoutControlItem8.Location = new System.Drawing.Point(0, 168);
            this.layoutControlItem8.Name = "layoutControlItem8";
            this.layoutControlItem8.Size = new System.Drawing.Size(306, 24);
            this.layoutControlItem8.Text = "Status:";
            this.layoutControlItem8.TextSize = new System.Drawing.Size(57, 13);
            // 
            // AddSubCampaign
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lcAddSubCampaign);
            this.Name = "AddSubCampaign";
            this.Size = new System.Drawing.Size(326, 336);
            this.Load += new System.EventHandler(this.AddSubCampaign1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.lcAddSubCampaign)).EndInit();
            this.lcAddSubCampaign.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboStatus.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboPriority.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dpEndDate.Properties.VistaTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dpEndDate.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dpStartDate.Properties.VistaTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dpStartDate.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboOwner.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCampaign.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCustomer.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl lcAddSubCampaign;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraEditors.MemoEdit txtDescription;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem10;
        private DevExpress.XtraEditors.SimpleButton cmdCancel;
        private DevExpress.XtraEditors.SimpleButton cmdSave;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem11;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem12;
        private DevExpress.XtraEditors.ComboBoxEdit cboStatus;
        private DevExpress.XtraEditors.ComboBoxEdit cboPriority;
        private DevExpress.XtraEditors.DateEdit dpEndDate;
        private DevExpress.XtraEditors.DateEdit dpStartDate;
        private DevExpress.XtraEditors.LookUpEdit cboOwner;
        private DevExpress.XtraEditors.LookUpEdit cboCampaign;
        private DevExpress.XtraEditors.LookUpEdit cboCustomer;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem8;
    }
}
