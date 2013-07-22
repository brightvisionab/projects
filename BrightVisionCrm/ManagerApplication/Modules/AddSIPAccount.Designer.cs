namespace ManagerApplication.Modules
{
    partial class AddSIPAccount
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
            this.memoEditComment = new DevExpress.XtraEditors.MemoEdit();
            this.simpleButtonSave = new DevExpress.XtraEditors.SimpleButton();
            this.textEditOperator = new DevExpress.XtraEditors.TextEdit();
            this.textEditPassword = new DevExpress.XtraEditors.TextEdit();
            this.textEditUsername = new DevExpress.XtraEditors.TextEdit();
            this.textEditDisplayName = new DevExpress.XtraEditors.TextEdit();
            this.textEditSIPUrl = new DevExpress.XtraEditors.TextEdit();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.memoEditComment.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditOperator.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditPassword.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditUsername.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditDisplayName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditSIPUrl.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.memoEditComment);
            this.layoutControl1.Controls.Add(this.simpleButtonSave);
            this.layoutControl1.Controls.Add(this.textEditOperator);
            this.layoutControl1.Controls.Add(this.textEditPassword);
            this.layoutControl1.Controls.Add(this.textEditUsername);
            this.layoutControl1.Controls.Add(this.textEditDisplayName);
            this.layoutControl1.Controls.Add(this.textEditSIPUrl);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(1805, 608, 250, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(321, 246);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // memoEditComment
            // 
            this.memoEditComment.Location = new System.Drawing.Point(80, 132);
            this.memoEditComment.Name = "memoEditComment";
            this.memoEditComment.Size = new System.Drawing.Size(229, 76);
            this.memoEditComment.StyleController = this.layoutControl1;
            this.memoEditComment.TabIndex = 11;
            // 
            // simpleButtonSave
            // 
            this.simpleButtonSave.Location = new System.Drawing.Point(241, 212);
            this.simpleButtonSave.Name = "simpleButtonSave";
            this.simpleButtonSave.Size = new System.Drawing.Size(68, 22);
            this.simpleButtonSave.StyleController = this.layoutControl1;
            this.simpleButtonSave.TabIndex = 10;
            this.simpleButtonSave.Text = "Save";
            this.simpleButtonSave.Click += new System.EventHandler(this.simpleButtonSave_Click);
            // 
            // textEditOperator
            // 
            this.textEditOperator.EditValue = "";
            this.textEditOperator.Location = new System.Drawing.Point(80, 108);
            this.textEditOperator.Name = "textEditOperator";
            this.textEditOperator.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.False;
            this.textEditOperator.Properties.ValidateOnEnterKey = true;
            this.textEditOperator.Size = new System.Drawing.Size(229, 20);
            this.textEditOperator.StyleController = this.layoutControl1;
            this.textEditOperator.TabIndex = 8;
            this.textEditOperator.Validating += new System.ComponentModel.CancelEventHandler(this.textEditOperator_Validating);
            // 
            // textEditPassword
            // 
            this.textEditPassword.EditValue = "";
            this.textEditPassword.Location = new System.Drawing.Point(80, 84);
            this.textEditPassword.Name = "textEditPassword";
            this.textEditPassword.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.False;
            this.textEditPassword.Properties.ValidateOnEnterKey = true;
            this.textEditPassword.Size = new System.Drawing.Size(229, 20);
            this.textEditPassword.StyleController = this.layoutControl1;
            this.textEditPassword.TabIndex = 7;
            this.textEditPassword.Validating += new System.ComponentModel.CancelEventHandler(this.textEditPassword_Validating);
            // 
            // textEditUsername
            // 
            this.textEditUsername.EditValue = "";
            this.textEditUsername.Location = new System.Drawing.Point(80, 60);
            this.textEditUsername.Name = "textEditUsername";
            this.textEditUsername.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.False;
            this.textEditUsername.Properties.ValidateOnEnterKey = true;
            this.textEditUsername.Size = new System.Drawing.Size(229, 20);
            this.textEditUsername.StyleController = this.layoutControl1;
            this.textEditUsername.TabIndex = 6;
            this.textEditUsername.Validating += new System.ComponentModel.CancelEventHandler(this.textEditUsername_Validating);
            // 
            // textEditDisplayName
            // 
            this.textEditDisplayName.EditValue = "";
            this.textEditDisplayName.Location = new System.Drawing.Point(80, 36);
            this.textEditDisplayName.Name = "textEditDisplayName";
            this.textEditDisplayName.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.False;
            this.textEditDisplayName.Properties.ValidateOnEnterKey = true;
            this.textEditDisplayName.Size = new System.Drawing.Size(229, 20);
            this.textEditDisplayName.StyleController = this.layoutControl1;
            this.textEditDisplayName.TabIndex = 5;
            this.textEditDisplayName.Validating += new System.ComponentModel.CancelEventHandler(this.textEditDisplayName_Validating);
            // 
            // textEditSIPUrl
            // 
            this.textEditSIPUrl.EditValue = "";
            this.textEditSIPUrl.Location = new System.Drawing.Point(80, 12);
            this.textEditSIPUrl.Name = "textEditSIPUrl";
            this.textEditSIPUrl.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.False;
            this.textEditSIPUrl.Properties.ValidateOnEnterKey = true;
            this.textEditSIPUrl.Size = new System.Drawing.Size(229, 20);
            this.textEditSIPUrl.StyleController = this.layoutControl1;
            this.textEditSIPUrl.TabIndex = 4;
            this.textEditSIPUrl.Validating += new System.ComponentModel.CancelEventHandler(this.textEditSIPUrl_Validating);
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
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.layoutControlItem7,
            this.emptySpaceItem1,
            this.layoutControlItem6});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(321, 246);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.textEditSIPUrl;
            this.layoutControlItem1.CustomizationFormText = "SIP Url";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(301, 24);
            this.layoutControlItem1.Text = "SIP Url";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(64, 13);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.textEditDisplayName;
            this.layoutControlItem2.CustomizationFormText = "Display Name";
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 24);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(301, 24);
            this.layoutControlItem2.Text = "Display Name";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(64, 13);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.textEditUsername;
            this.layoutControlItem3.CustomizationFormText = "Username";
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 48);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(301, 24);
            this.layoutControlItem3.Text = "Username";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(64, 13);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.textEditPassword;
            this.layoutControlItem4.CustomizationFormText = "Password";
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 72);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(301, 24);
            this.layoutControlItem4.Text = "Password";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(64, 13);
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.textEditOperator;
            this.layoutControlItem5.CustomizationFormText = "Operator";
            this.layoutControlItem5.Location = new System.Drawing.Point(0, 96);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(301, 24);
            this.layoutControlItem5.Text = "Operator";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(64, 13);
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.Control = this.simpleButtonSave;
            this.layoutControlItem7.CustomizationFormText = "layoutControlItem7";
            this.layoutControlItem7.Location = new System.Drawing.Point(229, 200);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Size = new System.Drawing.Size(72, 26);
            this.layoutControlItem7.Text = "layoutControlItem7";
            this.layoutControlItem7.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem7.TextToControlDistance = 0;
            this.layoutControlItem7.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 200);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(229, 26);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.memoEditComment;
            this.layoutControlItem6.CustomizationFormText = "Comment";
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 120);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(301, 80);
            this.layoutControlItem6.Text = "Comment";
            this.layoutControlItem6.TextSize = new System.Drawing.Size(64, 13);
            // 
            // AddSIPAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Name = "AddSIPAccount";
            this.Size = new System.Drawing.Size(321, 246);
            this.Load += new System.EventHandler(this.AddSIPAccount_Load);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.memoEditComment.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditOperator.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditPassword.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditUsername.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditDisplayName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditSIPUrl.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraEditors.TextEdit textEditUsername;
        private DevExpress.XtraEditors.TextEdit textEditDisplayName;
        private DevExpress.XtraEditors.TextEdit textEditSIPUrl;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraEditors.TextEdit textEditOperator;
        private DevExpress.XtraEditors.TextEdit textEditPassword;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraEditors.SimpleButton simpleButtonSave;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraEditors.MemoEdit memoEditComment;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
    }
}
