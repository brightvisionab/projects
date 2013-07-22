namespace SalesConsultant.Modules
{
    partial class ReleaseSendMail
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
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject2 = new DevExpress.Utils.SerializableAppearanceObject();
            this.richTextBoxMessage = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSubject = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.repositoryItemButtonEditSave = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.repositoryItemButtonEditCancel = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.repositoryItemTextEditField = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.cbxActive = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.tbxContactRemark = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            this.cbxHasContactRemark = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.cbxIsAbsent = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.gvRecipients = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colFullname = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colRoleName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colEmail = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMobileNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMailTo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMailCC = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMailBCC = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSMS = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcRecipients = new DevExpress.XtraGrid.GridControl();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEditSave)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEditCancel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEditField)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxActive)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxContactRemark)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxHasContactRemark)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxIsAbsent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvRecipients)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcRecipients)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBoxMessage
            // 
            this.richTextBoxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxMessage.Location = new System.Drawing.Point(4, 43);
            this.richTextBoxMessage.Name = "richTextBoxMessage";
            this.richTextBoxMessage.Size = new System.Drawing.Size(442, 139);
            this.richTextBoxMessage.TabIndex = 4;
            this.richTextBoxMessage.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Message :";
            // 
            // txtSubject
            // 
            this.txtSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSubject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSubject.Location = new System.Drawing.Point(64, 3);
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(382, 21);
            this.txtSubject.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(1, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Subject :";
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
            // repositoryItemCheckEdit1
            // 
            this.repositoryItemCheckEdit1.AutoHeight = false;
            this.repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit1";
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
            this.colRoleName.Width = 53;
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
            this.colMailBCC.FieldName = "mail_bcc";
            this.colMailBCC.Name = "colMailBCC";
            this.colMailBCC.OptionsColumn.AllowEdit = false;
            this.colMailBCC.OptionsColumn.AllowFocus = false;
            this.colMailBCC.Visible = true;
            this.colMailBCC.VisibleIndex = 6;
            // 
            // colSMS
            // 
            this.colSMS.Caption = "SMS";
            this.colSMS.FieldName = "sms";
            this.colSMS.Name = "colSMS";
            this.colSMS.OptionsColumn.AllowEdit = false;
            this.colSMS.OptionsColumn.AllowFocus = false;
            this.colSMS.Visible = true;
            this.colSMS.VisibleIndex = 7;
            // 
            // gcRecipients
            // 
            this.gcRecipients.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gcRecipients.Location = new System.Drawing.Point(4, 188);
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
            this.repositoryItemCheckEdit1});
            this.gcRecipients.Size = new System.Drawing.Size(442, 200);
            this.gcRecipients.TabIndex = 9;
            this.gcRecipients.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvRecipients});
            // 
            // ReleaseSendMail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gcRecipients);
            this.Controls.Add(this.richTextBoxMessage);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtSubject);
            this.Controls.Add(this.label2);
            this.Name = "ReleaseSendMail";
            this.Size = new System.Drawing.Size(450, 392);
            this.Load += new System.EventHandler(this.ReleaseSendMail_Load);
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEditSave)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEditCancel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEditField)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxActive)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxContactRemark)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxHasContactRemark)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbxIsAbsent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvRecipients)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcRecipients)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxMessage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSubject;
        private System.Windows.Forms.Label label2;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repositoryItemButtonEditSave;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repositoryItemButtonEditCancel;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEditField;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit cbxActive;
        private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit tbxContactRemark;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit cbxHasContactRemark;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit cbxIsAbsent;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
        public DevExpress.XtraGrid.GridControl gcRecipients;
        public DevExpress.XtraGrid.Views.Grid.GridView gvRecipients;
        private DevExpress.XtraGrid.Columns.GridColumn colFullname;
        private DevExpress.XtraGrid.Columns.GridColumn colRoleName;
        private DevExpress.XtraGrid.Columns.GridColumn colEmail;
        private DevExpress.XtraGrid.Columns.GridColumn colMobileNo;
        private DevExpress.XtraGrid.Columns.GridColumn colMailTo;
        private DevExpress.XtraGrid.Columns.GridColumn colMailCC;
        private DevExpress.XtraGrid.Columns.GridColumn colMailBCC;
        private DevExpress.XtraGrid.Columns.GridColumn colSMS;
    }
}
