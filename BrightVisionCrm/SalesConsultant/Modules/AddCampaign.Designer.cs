namespace SalesConsultant.Modules
{
    partial class AddCampaign
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
            this.lcAddCampaign = new DevExpress.XtraLayout.LayoutControl();
            this.cboStatus = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cboPriority = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cboWebAccess = new DevExpress.XtraEditors.LookUpEdit();
            this.cboOwner = new DevExpress.XtraEditors.LookUpEdit();
            this.cmdSave = new DevExpress.XtraEditors.SimpleButton();
            this.txtDescription = new DevExpress.XtraEditors.MemoEdit();
            this.cmdCancel = new DevExpress.XtraEditors.SimpleButton();
            this.txtName = new DevExpress.XtraEditors.TextEdit();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem8 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem9 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.lcAddCampaign)).BeginInit();
            this.lcAddCampaign.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboStatus.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboPriority.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboWebAccess.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboOwner.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            this.SuspendLayout();
            // 
            // lcAddCampaign
            // 
            this.lcAddCampaign.Controls.Add(this.cboStatus);
            this.lcAddCampaign.Controls.Add(this.cboPriority);
            this.lcAddCampaign.Controls.Add(this.cboWebAccess);
            this.lcAddCampaign.Controls.Add(this.cboOwner);
            this.lcAddCampaign.Controls.Add(this.cmdSave);
            this.lcAddCampaign.Controls.Add(this.txtDescription);
            this.lcAddCampaign.Controls.Add(this.cmdCancel);
            this.lcAddCampaign.Controls.Add(this.txtName);
            this.lcAddCampaign.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lcAddCampaign.Location = new System.Drawing.Point(0, 0);
            this.lcAddCampaign.Name = "lcAddCampaign";
            this.lcAddCampaign.Root = this.layoutControlGroup1;
            this.lcAddCampaign.Size = new System.Drawing.Size(333, 260);
            this.lcAddCampaign.TabIndex = 0;
            this.lcAddCampaign.Text = "layoutControl1";
            this.lcAddCampaign.AllowCustomizationMenu = false;
            // 
            // cboStatus
            // 
            this.cboStatus.EditValue = "On Hold";
            this.cboStatus.Location = new System.Drawing.Point(91, 103);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboStatus.Properties.Items.AddRange(new object[] {
            "On Hold",
            "Confirmed"});
            this.cboStatus.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboStatus.Size = new System.Drawing.Size(235, 20);
            this.cboStatus.StyleController = this.lcAddCampaign;
            this.cboStatus.TabIndex = 15;
            // 
            // cboPriority
            // 
            this.cboPriority.EditValue = "1";
            this.cboPriority.Location = new System.Drawing.Point(91, 79);
            this.cboPriority.Name = "cboPriority";
            this.cboPriority.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboPriority.Properties.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.cboPriority.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboPriority.Size = new System.Drawing.Size(235, 20);
            this.cboPriority.StyleController = this.lcAddCampaign;
            this.cboPriority.TabIndex = 14;
            // 
            // cboWebAccess
            // 
            this.cboWebAccess.Location = new System.Drawing.Point(91, 55);
            this.cboWebAccess.Name = "cboWebAccess";
            this.cboWebAccess.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboWebAccess.Properties.NullText = "";
            this.cboWebAccess.Properties.SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoComplete;
            this.cboWebAccess.Properties.ShowFooter = false;
            this.cboWebAccess.Properties.ShowHeader = false;
            this.cboWebAccess.Properties.ShowLines = false;
            this.cboWebAccess.Size = new System.Drawing.Size(235, 20);
            this.cboWebAccess.StyleController = this.lcAddCampaign;
            this.cboWebAccess.TabIndex = 13;
            // 
            // cboOwner
            // 
            this.cboOwner.Location = new System.Drawing.Point(91, 31);
            this.cboOwner.Name = "cboOwner";
            this.cboOwner.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboOwner.Properties.NullText = "";
            this.cboOwner.Properties.SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoComplete;
            this.cboOwner.Properties.ShowFooter = false;
            this.cboOwner.Properties.ShowHeader = false;
            this.cboOwner.Properties.ShowLines = false;
            this.cboOwner.Size = new System.Drawing.Size(235, 20);
            this.cboOwner.StyleController = this.lcAddCampaign;
            this.cboOwner.TabIndex = 12;
            // 
            // cmdSave
            // 
            this.cmdSave.Location = new System.Drawing.Point(7, 231);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(157, 22);
            this.cmdSave.StyleController = this.lcAddCampaign;
            this.cmdSave.TabIndex = 10;
            this.cmdSave.Text = "Save";
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(91, 127);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(235, 100);
            this.txtDescription.StyleController = this.lcAddCampaign;
            this.txtDescription.TabIndex = 9;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Location = new System.Drawing.Point(168, 231);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(158, 22);
            this.cmdCancel.StyleController = this.lcAddCampaign;
            this.cmdCancel.TabIndex = 11;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(91, 7);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(235, 20);
            this.txtName.StyleController = this.lcAddCampaign;
            this.txtName.TabIndex = 4;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem6,
            this.layoutControlItem8,
            this.layoutControlItem7,
            this.layoutControlItem9,
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.layoutControlItem4});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlGroup1.Size = new System.Drawing.Size(333, 260);
            this.layoutControlGroup1.Text = "layoutControlGroup1";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.txtName;
            this.layoutControlItem1.CustomizationFormText = "Campaign name:";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(323, 24);
            this.layoutControlItem1.Text = "Campaign name:";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(80, 13);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.txtDescription;
            this.layoutControlItem6.CustomizationFormText = "Description:";
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 120);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(323, 104);
            this.layoutControlItem6.Text = "Description:";
            this.layoutControlItem6.TextSize = new System.Drawing.Size(80, 13);
            // 
            // layoutControlItem8
            // 
            this.layoutControlItem8.Control = this.cmdCancel;
            this.layoutControlItem8.CustomizationFormText = "layoutControlItem8";
            this.layoutControlItem8.Location = new System.Drawing.Point(161, 224);
            this.layoutControlItem8.Name = "layoutControlItem8";
            this.layoutControlItem8.Size = new System.Drawing.Size(162, 26);
            this.layoutControlItem8.Text = "layoutControlItem8";
            this.layoutControlItem8.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem8.TextToControlDistance = 0;
            this.layoutControlItem8.TextVisible = false;
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.Control = this.cmdSave;
            this.layoutControlItem7.CustomizationFormText = "layoutControlItem7";
            this.layoutControlItem7.Location = new System.Drawing.Point(0, 224);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Size = new System.Drawing.Size(161, 26);
            this.layoutControlItem7.Text = "layoutControlItem7";
            this.layoutControlItem7.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem7.TextToControlDistance = 0;
            this.layoutControlItem7.TextVisible = false;
            // 
            // layoutControlItem9
            // 
            this.layoutControlItem9.Control = this.cboOwner;
            this.layoutControlItem9.CustomizationFormText = "Owner:";
            this.layoutControlItem9.Location = new System.Drawing.Point(0, 24);
            this.layoutControlItem9.Name = "layoutControlItem9";
            this.layoutControlItem9.Size = new System.Drawing.Size(323, 24);
            this.layoutControlItem9.Text = "Owner:";
            this.layoutControlItem9.TextSize = new System.Drawing.Size(80, 13);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.cboWebAccess;
            this.layoutControlItem2.CustomizationFormText = "Web Access:";
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 48);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(323, 24);
            this.layoutControlItem2.Text = "Web Access:";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(80, 13);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.cboPriority;
            this.layoutControlItem3.CustomizationFormText = "Priotity:";
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 72);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(323, 24);
            this.layoutControlItem3.Text = "Priority:";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(80, 13);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.cboStatus;
            this.layoutControlItem4.CustomizationFormText = "Status:";
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 96);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(323, 24);
            this.layoutControlItem4.Text = "Status:";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(80, 13);
            // 
            // AddCampaign
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lcAddCampaign);
            this.Name = "AddCampaign";
            this.Size = new System.Drawing.Size(333, 260);
            ((System.ComponentModel.ISupportInitialize)(this.lcAddCampaign)).EndInit();
            this.lcAddCampaign.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboStatus.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboPriority.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboWebAccess.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboOwner.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl lcAddCampaign;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraEditors.MemoEdit txtDescription;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraEditors.SimpleButton cmdSave;
        private DevExpress.XtraEditors.SimpleButton cmdCancel;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem8;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraEditors.LookUpEdit cboWebAccess;
        private DevExpress.XtraEditors.LookUpEdit cboOwner;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem9;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraEditors.ComboBoxEdit cboStatus;
        private DevExpress.XtraEditors.ComboBoxEdit cboPriority;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
    }
}
