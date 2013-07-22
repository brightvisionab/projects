namespace SalesConsultant.Forms
{
    partial class FrmSendEmailRecipients
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject2 = new DevExpress.Utils.SerializableAppearanceObject();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.tbxMailContent = new DevExpress.XtraEditors.MemoEdit();
            this.tbxMailSubject = new DevExpress.XtraEditors.TextEdit();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.btnSendEmail = new DevExpress.XtraEditors.SimpleButton();
            this.btnPreview = new DevExpress.XtraEditors.SimpleButton();
            this.cboReports = new DevExpress.XtraEditors.LookUpEdit();
            this.gcRecipients = new DevExpress.XtraGrid.GridControl();
            this.gvRecipients = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colFullname = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colRoleName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colEmail = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMobileNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMailTo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.colMailCC = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMailBCC = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemCheckEditBCC = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.colSMS = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemButtonEditSave = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.repositoryItemButtonEditCancel = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.repositoryItemTextEditField = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.cbxActive = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.tbxContactRemark = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            this.cbxHasContactRemark = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.cbxIsAbsent = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.repositoryItemRadioGroup1 = new DevExpress.XtraEditors.Repository.RepositoryItemRadioGroup();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.simpleLabelItem1 = new DevExpress.XtraLayout.SimpleLabelItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbxMailContent.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxMailSubject.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboReports.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcRecipients)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvRecipients)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEditBCC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEditSave)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEditCancel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEditField)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxActive)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxContactRemark)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxHasContactRemark)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxIsAbsent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemRadioGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleLabelItem1)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.tbxMailContent);
            this.layoutControl1.Controls.Add(this.tbxMailSubject);
            this.layoutControl1.Controls.Add(this.btnCancel);
            this.layoutControl1.Controls.Add(this.btnSendEmail);
            this.layoutControl1.Controls.Add(this.btnPreview);
            this.layoutControl1.Controls.Add(this.cboReports);
            this.layoutControl1.Controls.Add(this.gcRecipients);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(658, 189, 250, 350);
            this.layoutControl1.Padding = new System.Windows.Forms.Padding(5);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(731, 570);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // tbxMailContent
            // 
            this.tbxMailContent.Location = new System.Drawing.Point(12, 68);
            this.tbxMailContent.Name = "tbxMailContent";
            this.tbxMailContent.Size = new System.Drawing.Size(707, 195);
            this.tbxMailContent.StyleController = this.layoutControl1;
            this.tbxMailContent.TabIndex = 19;
            // 
            // tbxMailSubject
            // 
            this.tbxMailSubject.Location = new System.Drawing.Point(12, 28);
            this.tbxMailSubject.Name = "tbxMailSubject";
            this.tbxMailSubject.Size = new System.Drawing.Size(707, 20);
            this.tbxMailSubject.StyleController = this.layoutControl1;
            this.tbxMailSubject.TabIndex = 18;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::SalesConsultant.Properties.Resources.close_campaign_booking;
            this.btnCancel.Location = new System.Drawing.Point(657, 536);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(62, 22);
            this.btnCancel.StyleController = this.layoutControl1;
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            // 
            // btnSendEmail
            // 
            this.btnSendEmail.Image = global::SalesConsultant.Properties.Resources.save_ok;
            this.btnSendEmail.Location = new System.Drawing.Point(599, 536);
            this.btnSendEmail.Name = "btnSendEmail";
            this.btnSendEmail.Size = new System.Drawing.Size(54, 22);
            this.btnSendEmail.StyleController = this.layoutControl1;
            this.btnSendEmail.TabIndex = 16;
            this.btnSendEmail.Text = "Send";
            this.btnSendEmail.Click += new System.EventHandler(this.btnSendEmail_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Image = global::SalesConsultant.Properties.Resources.grid_find;
            this.btnPreview.Location = new System.Drawing.Point(339, 536);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(92, 22);
            this.btnPreview.StyleController = this.layoutControl1;
            this.btnPreview.TabIndex = 15;
            this.btnPreview.Text = "Show Report";
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // cboReports
            // 
            this.cboReports.Location = new System.Drawing.Point(12, 536);
            this.cboReports.Name = "cboReports";
            this.cboReports.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboReports.Properties.NullText = "";
            this.cboReports.Properties.ShowFooter = false;
            this.cboReports.Properties.ShowHeader = false;
            this.cboReports.Properties.ShowLines = false;
            this.cboReports.Size = new System.Drawing.Size(323, 20);
            this.cboReports.StyleController = this.layoutControl1;
            this.cboReports.TabIndex = 14;
            // 
            // gcRecipients
            // 
            this.gcRecipients.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gcRecipients.Location = new System.Drawing.Point(12, 283);
            this.gcRecipients.MainView = this.gvRecipients;
            this.gcRecipients.Name = "gcRecipients";
            this.gcRecipients.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemButtonEditSave,
            this.repositoryItemButtonEditCancel,
            this.repositoryItemTextEditField,
            this.cbxActive,
            this.tbxContactRemark,
            this.cbxHasContactRemark,
            this.cbxIsAbsent,
            this.repositoryItemCheckEdit1,
            this.repositoryItemCheckEditBCC,
            this.repositoryItemRadioGroup1});
            this.gcRecipients.Size = new System.Drawing.Size(707, 232);
            this.gcRecipients.TabIndex = 10;
            this.gcRecipients.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvRecipients});
            // 
            // gvRecipients
            // 
            this.gvRecipients.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colFullname,
            this.colRoleName,
            this.colEmail,
            this.colMobileNo,
            this.colMailTo,
            this.colMailCC,
            this.colMailBCC,
            this.colSMS});
            this.gvRecipients.GridControl = this.gcRecipients;
            this.gvRecipients.Name = "gvRecipients";
            this.gvRecipients.OptionsDetail.ShowDetailTabs = false;
            this.gvRecipients.OptionsDetail.SmartDetailExpand = false;
            this.gvRecipients.OptionsFind.FindMode = DevExpress.XtraEditors.FindMode.FindClick;
            this.gvRecipients.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gvRecipients.OptionsView.ColumnAutoWidth = false;
            this.gvRecipients.OptionsView.ShowGroupPanel = false;
            // 
            // colFullname
            // 
            this.colFullname.Caption = "Name";
            this.colFullname.FieldName = "fullname";
            this.colFullname.Name = "colFullname";
            this.colFullname.OptionsColumn.AllowEdit = false;
            this.colFullname.OptionsColumn.AllowFocus = false;
            this.colFullname.OptionsColumn.ReadOnly = true;
            this.colFullname.Visible = true;
            this.colFullname.VisibleIndex = 0;
            this.colFullname.Width = 60;
            // 
            // colRoleName
            // 
            this.colRoleName.Caption = "Role";
            this.colRoleName.FieldName = "role_name";
            this.colRoleName.Name = "colRoleName";
            this.colRoleName.OptionsColumn.AllowEdit = false;
            this.colRoleName.OptionsColumn.AllowFocus = false;
            this.colRoleName.OptionsColumn.ReadOnly = true;
            this.colRoleName.Visible = true;
            this.colRoleName.VisibleIndex = 1;
            this.colRoleName.Width = 100;
            // 
            // colEmail
            // 
            this.colEmail.Caption = "Email";
            this.colEmail.FieldName = "email";
            this.colEmail.Name = "colEmail";
            this.colEmail.OptionsColumn.AllowEdit = false;
            this.colEmail.OptionsColumn.AllowFocus = false;
            this.colEmail.OptionsColumn.ReadOnly = true;
            this.colEmail.Visible = true;
            this.colEmail.VisibleIndex = 2;
            this.colEmail.Width = 71;
            // 
            // colMobileNo
            // 
            this.colMobileNo.Caption = "Mobile";
            this.colMobileNo.FieldName = "mobile_no";
            this.colMobileNo.Name = "colMobileNo";
            this.colMobileNo.OptionsColumn.AllowEdit = false;
            this.colMobileNo.OptionsColumn.AllowFocus = false;
            this.colMobileNo.OptionsColumn.ReadOnly = true;
            this.colMobileNo.Visible = true;
            this.colMobileNo.VisibleIndex = 3;
            this.colMobileNo.Width = 59;
            // 
            // colMailTo
            // 
            this.colMailTo.Caption = "Mail To";
            this.colMailTo.ColumnEdit = this.repositoryItemCheckEdit1;
            this.colMailTo.FieldName = "mail_to";
            this.colMailTo.Name = "colMailTo";
            this.colMailTo.Visible = true;
            this.colMailTo.VisibleIndex = 4;
            // 
            // repositoryItemCheckEdit1
            // 
            this.repositoryItemCheckEdit1.AutoHeight = false;
            this.repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit1";
            // 
            // colMailCC
            // 
            this.colMailCC.Caption = "Mail CC";
            this.colMailCC.FieldName = "mail_cc";
            this.colMailCC.Name = "colMailCC";
            this.colMailCC.OptionsColumn.AllowEdit = false;
            this.colMailCC.OptionsColumn.AllowFocus = false;
            this.colMailCC.Visible = true;
            this.colMailCC.VisibleIndex = 5;
            // 
            // colMailBCC
            // 
            this.colMailBCC.Caption = "Mail BCC";
            this.colMailBCC.ColumnEdit = this.repositoryItemCheckEditBCC;
            this.colMailBCC.FieldName = "mail_bcc";
            this.colMailBCC.Name = "colMailBCC";
            this.colMailBCC.Visible = true;
            this.colMailBCC.VisibleIndex = 6;
            // 
            // repositoryItemCheckEditBCC
            // 
            this.repositoryItemCheckEditBCC.AutoHeight = false;
            this.repositoryItemCheckEditBCC.Name = "repositoryItemCheckEditBCC";
            this.repositoryItemCheckEditBCC.RadioGroupIndex = 1;
            // 
            // colSMS
            // 
            this.colSMS.Caption = "SMS";
            this.colSMS.FieldName = "sms";
            this.colSMS.Name = "colSMS";
            this.colSMS.Visible = true;
            this.colSMS.VisibleIndex = 7;
            // 
            // repositoryItemButtonEditSave
            // 
            this.repositoryItemButtonEditSave.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.repositoryItemButtonEditSave.Appearance.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.repositoryItemButtonEditSave.Appearance.Options.UseBorderColor = true;
            this.repositoryItemButtonEditSave.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.repositoryItemButtonEditSave.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "Save", 10, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject1, "", null, null, true)});
            this.repositoryItemButtonEditSave.Name = "repositoryItemButtonEditSave";
            this.repositoryItemButtonEditSave.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.HideTextEditor;
            // 
            // repositoryItemButtonEditCancel
            // 
            this.repositoryItemButtonEditCancel.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.repositoryItemButtonEditCancel.Appearance.Options.UseBorderColor = true;
            this.repositoryItemButtonEditCancel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.repositoryItemButtonEditCancel.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "Cancel", -1, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject2, "", null, null, true)});
            this.repositoryItemButtonEditCancel.Name = "repositoryItemButtonEditCancel";
            this.repositoryItemButtonEditCancel.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.HideTextEditor;
            // 
            // repositoryItemTextEditField
            // 
            this.repositoryItemTextEditField.AppearanceFocused.BackColor = System.Drawing.Color.White;
            this.repositoryItemTextEditField.AppearanceFocused.BackColor2 = System.Drawing.Color.White;
            this.repositoryItemTextEditField.AppearanceFocused.BorderColor = System.Drawing.Color.Silver;
            this.repositoryItemTextEditField.AppearanceFocused.Options.UseBackColor = true;
            this.repositoryItemTextEditField.AppearanceFocused.Options.UseBorderColor = true;
            this.repositoryItemTextEditField.AutoHeight = false;
            this.repositoryItemTextEditField.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.repositoryItemTextEditField.Name = "repositoryItemTextEditField";
            // 
            // cbxActive
            // 
            this.cbxActive.AutoHeight = false;
            this.cbxActive.Name = "cbxActive";
            // 
            // tbxContactRemark
            // 
            this.tbxContactRemark.Appearance.Options.UseTextOptions = true;
            this.tbxContactRemark.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.tbxContactRemark.AppearanceDisabled.Options.UseTextOptions = true;
            this.tbxContactRemark.AppearanceDisabled.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.tbxContactRemark.AppearanceFocused.BorderColor = System.Drawing.Color.Silver;
            this.tbxContactRemark.AppearanceFocused.Options.UseBackColor = true;
            this.tbxContactRemark.AppearanceFocused.Options.UseBorderColor = true;
            this.tbxContactRemark.AppearanceFocused.Options.UseTextOptions = true;
            this.tbxContactRemark.AppearanceFocused.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.tbxContactRemark.AppearanceReadOnly.Options.UseTextOptions = true;
            this.tbxContactRemark.AppearanceReadOnly.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.tbxContactRemark.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.tbxContactRemark.Name = "tbxContactRemark";
            // 
            // cbxHasContactRemark
            // 
            this.cbxHasContactRemark.AutoHeight = false;
            this.cbxHasContactRemark.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.UserDefined;
            this.cbxHasContactRemark.Name = "cbxHasContactRemark";
            this.cbxHasContactRemark.PictureChecked = global::SalesConsultant.Properties.Resources.contact_remarks;
            // 
            // cbxIsAbsent
            // 
            this.cbxIsAbsent.AutoHeight = false;
            this.cbxIsAbsent.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.UserDefined;
            this.cbxIsAbsent.Name = "cbxIsAbsent";
            this.cbxIsAbsent.PictureChecked = global::SalesConsultant.Properties.Resources.contact_absence;
            // 
            // repositoryItemRadioGroup1
            // 
            this.repositoryItemRadioGroup1.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem()});
            this.repositoryItemRadioGroup1.Name = "repositoryItemRadioGroup1";
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.emptySpaceItem1,
            this.layoutControlItem6,
            this.layoutControlItem7,
            this.simpleLabelItem1});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(731, 570);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.layoutControlItem1.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem1.Control = this.gcRecipients;
            this.layoutControlItem1.CustomizationFormText = "Mail Recipients";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 255);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(711, 252);
            this.layoutControlItem1.Text = "Mail Recipients";
            this.layoutControlItem1.TextLocation = DevExpress.Utils.Locations.Top;
            this.layoutControlItem1.TextSize = new System.Drawing.Size(172, 13);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.cboReports;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 524);
            this.layoutControlItem2.MaxSize = new System.Drawing.Size(327, 24);
            this.layoutControlItem2.MinSize = new System.Drawing.Size(327, 24);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(327, 26);
            this.layoutControlItem2.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem2.Text = "layoutControlItem2";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextToControlDistance = 0;
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.btnPreview;
            this.layoutControlItem3.CustomizationFormText = "layoutControlItem3";
            this.layoutControlItem3.Location = new System.Drawing.Point(327, 524);
            this.layoutControlItem3.MaxSize = new System.Drawing.Size(96, 26);
            this.layoutControlItem3.MinSize = new System.Drawing.Size(96, 26);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(96, 26);
            this.layoutControlItem3.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem3.Text = "layoutControlItem3";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextToControlDistance = 0;
            this.layoutControlItem3.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.btnSendEmail;
            this.layoutControlItem4.CustomizationFormText = "layoutControlItem4";
            this.layoutControlItem4.Location = new System.Drawing.Point(587, 524);
            this.layoutControlItem4.MaxSize = new System.Drawing.Size(58, 26);
            this.layoutControlItem4.MinSize = new System.Drawing.Size(58, 26);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(58, 26);
            this.layoutControlItem4.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem4.Text = "layoutControlItem4";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextToControlDistance = 0;
            this.layoutControlItem4.TextVisible = false;
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.btnCancel;
            this.layoutControlItem5.CustomizationFormText = "layoutControlItem5";
            this.layoutControlItem5.Location = new System.Drawing.Point(645, 524);
            this.layoutControlItem5.MaxSize = new System.Drawing.Size(66, 26);
            this.layoutControlItem5.MinSize = new System.Drawing.Size(66, 26);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(66, 26);
            this.layoutControlItem5.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem5.Text = "layoutControlItem5";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextToControlDistance = 0;
            this.layoutControlItem5.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem1.Location = new System.Drawing.Point(423, 524);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(164, 26);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.layoutControlItem6.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem6.Control = this.tbxMailSubject;
            this.layoutControlItem6.CustomizationFormText = "Subject";
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(711, 40);
            this.layoutControlItem6.Text = "Subject";
            this.layoutControlItem6.TextLocation = DevExpress.Utils.Locations.Top;
            this.layoutControlItem6.TextSize = new System.Drawing.Size(172, 13);
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.layoutControlItem7.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem7.Control = this.tbxMailContent;
            this.layoutControlItem7.CustomizationFormText = "Message";
            this.layoutControlItem7.Location = new System.Drawing.Point(0, 40);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Size = new System.Drawing.Size(711, 215);
            this.layoutControlItem7.Text = "Message";
            this.layoutControlItem7.TextLocation = DevExpress.Utils.Locations.Top;
            this.layoutControlItem7.TextSize = new System.Drawing.Size(172, 13);
            // 
            // simpleLabelItem1
            // 
            this.simpleLabelItem1.AllowHotTrack = false;
            this.simpleLabelItem1.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.simpleLabelItem1.AppearanceItemCaption.ForeColor = System.Drawing.Color.DarkGray;
            this.simpleLabelItem1.AppearanceItemCaption.Options.UseFont = true;
            this.simpleLabelItem1.AppearanceItemCaption.Options.UseForeColor = true;
            this.simpleLabelItem1.CustomizationFormText = "NOTE: Only one bcc can be choose.";
            this.simpleLabelItem1.Location = new System.Drawing.Point(0, 507);
            this.simpleLabelItem1.Name = "simpleLabelItem1";
            this.simpleLabelItem1.Size = new System.Drawing.Size(711, 17);
            this.simpleLabelItem1.Text = "NOTE: Only one bcc can be choose.";
            this.simpleLabelItem1.TextSize = new System.Drawing.Size(172, 13);
            // 
            // FrmSendEmailRecipients
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 570);
            this.Controls.Add(this.layoutControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSendEmailRecipients";
            this.Text = "Send New Email";
            this.Load += new System.EventHandler(this.FrmSendEmailRecipients_Load);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tbxMailContent.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxMailSubject.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboReports.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcRecipients)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvRecipients)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEditBCC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEditSave)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEditCancel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEditField)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxActive)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxContactRemark)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxHasContactRemark)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxIsAbsent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemRadioGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleLabelItem1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        public DevExpress.XtraGrid.GridControl gcRecipients;
        public DevExpress.XtraGrid.Views.Grid.GridView gvRecipients;
        private DevExpress.XtraGrid.Columns.GridColumn colFullname;
        private DevExpress.XtraGrid.Columns.GridColumn colRoleName;
        private DevExpress.XtraGrid.Columns.GridColumn colEmail;
        private DevExpress.XtraGrid.Columns.GridColumn colMobileNo;
        private DevExpress.XtraGrid.Columns.GridColumn colMailTo;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
        private DevExpress.XtraGrid.Columns.GridColumn colMailCC;
        private DevExpress.XtraGrid.Columns.GridColumn colMailBCC;
        private DevExpress.XtraGrid.Columns.GridColumn colSMS;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repositoryItemButtonEditSave;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repositoryItemButtonEditCancel;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEditField;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit cbxActive;
        private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit tbxContactRemark;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit cbxHasContactRemark;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit cbxIsAbsent;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraEditors.LookUpEdit cboReports;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraEditors.SimpleButton btnPreview;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraEditors.SimpleButton btnSendEmail;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraEditors.MemoEdit tbxMailContent;
        private DevExpress.XtraEditors.TextEdit tbxMailSubject;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEditBCC;
        private DevExpress.XtraEditors.Repository.RepositoryItemRadioGroup repositoryItemRadioGroup1;
        private DevExpress.XtraLayout.SimpleLabelItem simpleLabelItem1;
    }
}