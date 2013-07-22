namespace SalesConsultant.Modules
{
    partial class EditImportFile
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
            this.lcNewImport = new DevExpress.XtraLayout.LayoutControl();
            this.cboCountry = new DevExpress.XtraEditors.LookUpEdit();
            this.cmdUpdate = new DevExpress.XtraEditors.SimpleButton();
            this.cboCustomer = new DevExpress.XtraEditors.LookUpEdit();
            this.cboCampaign = new DevExpress.XtraEditors.LookUpEdit();
            this.txtImportListName = new DevExpress.XtraEditors.TextEdit();
            this.cmdCancel = new DevExpress.XtraEditors.SimpleButton();
            this.lcgNewImport = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem11 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem12 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.lcNewImport)).BeginInit();
            this.lcNewImport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboCountry.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCustomer.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCampaign.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtImportListName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lcgNewImport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            this.SuspendLayout();
            // 
            // lcNewImport
            // 
            this.lcNewImport.Controls.Add(this.cboCountry);
            this.lcNewImport.Controls.Add(this.cmdUpdate);
            this.lcNewImport.Controls.Add(this.cboCustomer);
            this.lcNewImport.Controls.Add(this.cboCampaign);
            this.lcNewImport.Controls.Add(this.txtImportListName);
            this.lcNewImport.Controls.Add(this.cmdCancel);
            this.lcNewImport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lcNewImport.Location = new System.Drawing.Point(0, 0);
            this.lcNewImport.Name = "lcNewImport";
            this.lcNewImport.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(787, 172, 250, 350);
            this.lcNewImport.Root = this.lcgNewImport;
            this.lcNewImport.Size = new System.Drawing.Size(325, 152);
            this.lcNewImport.TabIndex = 1;
            this.lcNewImport.Text = "layoutControl1";
            this.lcNewImport.AllowCustomizationMenu = false;
            // 
            // cboCountry
            // 
            this.cboCountry.Location = new System.Drawing.Point(67, 84);
            this.cboCountry.Name = "cboCountry";
            this.cboCountry.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboCountry.Properties.NullText = "";
            this.cboCountry.Properties.ShowFooter = false;
            this.cboCountry.Properties.ShowHeader = false;
            this.cboCountry.Properties.ShowLines = false;
            this.cboCountry.Size = new System.Drawing.Size(246, 20);
            this.cboCountry.StyleController = this.lcNewImport;
            this.cboCountry.TabIndex = 18;
            // 
            // cmdUpdate
            // 
            this.cmdUpdate.Location = new System.Drawing.Point(248, 118);
            this.cmdUpdate.Name = "cmdUpdate";
            this.cmdUpdate.Size = new System.Drawing.Size(65, 22);
            this.cmdUpdate.StyleController = this.lcNewImport;
            this.cmdUpdate.TabIndex = 16;
            this.cmdUpdate.Text = "Update";
            this.cmdUpdate.Click += new System.EventHandler(this.cmdUpdate_Click);
            // 
            // cboCustomer
            // 
            this.cboCustomer.Location = new System.Drawing.Point(67, 36);
            this.cboCustomer.Name = "cboCustomer";
            this.cboCustomer.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboCustomer.Properties.NullText = "";
            this.cboCustomer.Properties.ShowFooter = false;
            this.cboCustomer.Properties.ShowHeader = false;
            this.cboCustomer.Properties.ShowLines = false;
            this.cboCustomer.Size = new System.Drawing.Size(246, 20);
            this.cboCustomer.StyleController = this.lcNewImport;
            this.cboCustomer.TabIndex = 15;
            this.cboCustomer.EditValueChanged += new System.EventHandler(this.cboCustomer_EditValueChanged);
            // 
            // cboCampaign
            // 
            this.cboCampaign.Location = new System.Drawing.Point(67, 60);
            this.cboCampaign.Name = "cboCampaign";
            this.cboCampaign.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboCampaign.Properties.NullText = "";
            this.cboCampaign.Properties.ShowFooter = false;
            this.cboCampaign.Properties.ShowHeader = false;
            this.cboCampaign.Properties.ShowLines = false;
            this.cboCampaign.Size = new System.Drawing.Size(246, 20);
            this.cboCampaign.StyleController = this.lcNewImport;
            this.cboCampaign.TabIndex = 14;
            // 
            // txtImportListName
            // 
            this.txtImportListName.Location = new System.Drawing.Point(67, 12);
            this.txtImportListName.Name = "txtImportListName";
            this.txtImportListName.Size = new System.Drawing.Size(246, 20);
            this.txtImportListName.StyleController = this.lcNewImport;
            this.txtImportListName.TabIndex = 4;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Location = new System.Drawing.Point(180, 118);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(64, 22);
            this.cmdCancel.StyleController = this.lcNewImport;
            this.cmdCancel.TabIndex = 17;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lcgNewImport
            // 
            this.lcgNewImport.CustomizationFormText = "layoutControlGroup1";
            this.lcgNewImport.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.lcgNewImport.GroupBordersVisible = false;
            this.lcgNewImport.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem11,
            this.layoutControlItem12,
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.emptySpaceItem1,
            this.emptySpaceItem2,
            this.layoutControlItem4});
            this.lcgNewImport.Location = new System.Drawing.Point(0, 0);
            this.lcgNewImport.Name = "Root";
            this.lcgNewImport.Size = new System.Drawing.Size(325, 152);
            this.lcgNewImport.Text = "Root";
            this.lcgNewImport.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.txtImportListName;
            this.layoutControlItem1.CustomizationFormText = "Import list name:";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(305, 24);
            this.layoutControlItem1.Text = "List Name:";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(51, 13);
            // 
            // layoutControlItem11
            // 
            this.layoutControlItem11.Control = this.cboCampaign;
            this.layoutControlItem11.CustomizationFormText = "Campaign:";
            this.layoutControlItem11.Location = new System.Drawing.Point(0, 48);
            this.layoutControlItem11.Name = "layoutControlItem11";
            this.layoutControlItem11.Size = new System.Drawing.Size(305, 24);
            this.layoutControlItem11.Text = "Campaign:";
            this.layoutControlItem11.TextSize = new System.Drawing.Size(51, 13);
            // 
            // layoutControlItem12
            // 
            this.layoutControlItem12.Control = this.cboCustomer;
            this.layoutControlItem12.CustomizationFormText = "Customer:";
            this.layoutControlItem12.Location = new System.Drawing.Point(0, 24);
            this.layoutControlItem12.Name = "layoutControlItem12";
            this.layoutControlItem12.Size = new System.Drawing.Size(305, 24);
            this.layoutControlItem12.Text = "Customer:";
            this.layoutControlItem12.TextSize = new System.Drawing.Size(51, 13);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.cmdUpdate;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(236, 106);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(69, 26);
            this.layoutControlItem2.Text = "layoutControlItem2";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextToControlDistance = 0;
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.cmdCancel;
            this.layoutControlItem3.CustomizationFormText = "layoutControlItem3";
            this.layoutControlItem3.Location = new System.Drawing.Point(168, 106);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(68, 26);
            this.layoutControlItem3.Text = "layoutControlItem3";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextToControlDistance = 0;
            this.layoutControlItem3.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 96);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(305, 10);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem2
            // 
            this.emptySpaceItem2.AllowHotTrack = false;
            this.emptySpaceItem2.CustomizationFormText = "emptySpaceItem2";
            this.emptySpaceItem2.Location = new System.Drawing.Point(0, 106);
            this.emptySpaceItem2.Name = "emptySpaceItem2";
            this.emptySpaceItem2.Size = new System.Drawing.Size(168, 26);
            this.emptySpaceItem2.Text = "emptySpaceItem2";
            this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.cboCountry;
            this.layoutControlItem4.CustomizationFormText = "Country:";
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 72);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(305, 24);
            this.layoutControlItem4.Text = "Country:";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(51, 13);
            // 
            // EditImportFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lcNewImport);
            this.Name = "EditImportFile";
            this.Size = new System.Drawing.Size(325, 152);
            ((System.ComponentModel.ISupportInitialize)(this.lcNewImport)).EndInit();
            this.lcNewImport.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboCountry.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCustomer.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCampaign.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtImportListName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lcgNewImport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl lcNewImport;
        private DevExpress.XtraEditors.SimpleButton cmdCancel;
        private DevExpress.XtraEditors.SimpleButton cmdUpdate;
        private DevExpress.XtraEditors.LookUpEdit cboCustomer;
        private DevExpress.XtraEditors.LookUpEdit cboCampaign;
        private DevExpress.XtraEditors.TextEdit txtImportListName;
        public DevExpress.XtraLayout.LayoutControlGroup lcgNewImport;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem11;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem12;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem2;
        private DevExpress.XtraEditors.LookUpEdit cboCountry;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
    }
}
