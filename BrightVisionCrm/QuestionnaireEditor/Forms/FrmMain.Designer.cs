namespace QuestionnaireEditor.Forms {
    partial class FrmMain {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.layoutControlQuestionnaire = new DevExpress.XtraLayout.LayoutControl();
            this.layoutControlGroupQuestionnaire = new DevExpress.XtraLayout.LayoutControlGroup();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.simpleButton4 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton3 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.memoEditJSON = new DevExpress.XtraEditors.MemoEdit();
            this.simpleButtonPreview = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButtonEdit = new DevExpress.XtraEditors.SimpleButton();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.defaultToolTipController1 = new DevExpress.Utils.DefaultToolTipController(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlQuestionnaire)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroupQuestionnaire)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.memoEditJSON.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // layoutControlQuestionnaire
            // 
            this.layoutControlQuestionnaire.AllowCustomizationMenu = false;
            this.layoutControlQuestionnaire.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControlQuestionnaire.Location = new System.Drawing.Point(2, 50);
            this.layoutControlQuestionnaire.LookAndFeel.SkinName = "Black";
            this.layoutControlQuestionnaire.Margin = new System.Windows.Forms.Padding(0);
            this.layoutControlQuestionnaire.Name = "layoutControlQuestionnaire";
            this.layoutControlQuestionnaire.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(1013, 138, 250, 350);
            this.layoutControlQuestionnaire.OptionsSerialization.RestoreLayoutItemCustomizationFormText = false;
            this.layoutControlQuestionnaire.OptionsSerialization.RestoreLayoutItemText = false;
            this.layoutControlQuestionnaire.Root = this.layoutControlGroupQuestionnaire;
            this.layoutControlQuestionnaire.Size = new System.Drawing.Size(417, 466);
            this.layoutControlQuestionnaire.TabIndex = 1;
            this.layoutControlQuestionnaire.Text = "layoutControl1";
            // 
            // layoutControlGroupQuestionnaire
            // 
            this.layoutControlGroupQuestionnaire.CustomizationFormText = "Root";
            this.layoutControlGroupQuestionnaire.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroupQuestionnaire.GroupBordersVisible = false;
            this.layoutControlGroupQuestionnaire.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroupQuestionnaire.Name = "Root";
            this.layoutControlGroupQuestionnaire.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlGroupQuestionnaire.ShowInCustomizationForm = false;
            this.layoutControlGroupQuestionnaire.Size = new System.Drawing.Size(417, 466);
            this.layoutControlGroupQuestionnaire.Spacing = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlGroupQuestionnaire.Text = "Root";
            this.layoutControlGroupQuestionnaire.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItemBottom";
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 0);
            this.emptySpaceItem1.Name = "emptySpaceItemBottom";
            this.emptySpaceItem1.ShowInCustomizationForm = false;
            this.emptySpaceItem1.Size = new System.Drawing.Size(0, 0);
            this.emptySpaceItem1.Text = "emptySpaceItemBottom";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // groupControl2
            // 
            this.defaultToolTipController1.SetAllowHtmlText(this.groupControl2, DevExpress.Utils.DefaultBoolean.Default);
            this.groupControl2.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.groupControl2.AppearanceCaption.Options.UseFont = true;
            this.groupControl2.Controls.Add(this.layoutControlQuestionnaire);
            this.groupControl2.Controls.Add(this.panelControl1);
            this.groupControl2.Location = new System.Drawing.Point(415, 12);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(421, 518);
            this.groupControl2.TabIndex = 2;
            this.groupControl2.Text = "Dynamic Campaign Questionnaire";
            // 
            // panelControl1
            // 
            this.defaultToolTipController1.SetAllowHtmlText(this.panelControl1, DevExpress.Utils.DefaultBoolean.Default);
            this.panelControl1.Controls.Add(this.simpleButton4);
            this.panelControl1.Controls.Add(this.simpleButton3);
            this.panelControl1.Controls.Add(this.simpleButton2);
            this.panelControl1.Controls.Add(this.simpleButton1);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(2, 22);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(417, 28);
            this.panelControl1.TabIndex = 2;
            // 
            // simpleButton4
            // 
            this.simpleButton4.Location = new System.Drawing.Point(304, 2);
            this.simpleButton4.Name = "simpleButton4";
            this.simpleButton4.Size = new System.Drawing.Size(108, 23);
            this.simpleButton4.TabIndex = 0;
            this.simpleButton4.Text = "Save as InProgress";
            // 
            // simpleButton3
            // 
            this.simpleButton3.Location = new System.Drawing.Point(192, 2);
            this.simpleButton3.Name = "simpleButton3";
            this.simpleButton3.Size = new System.Drawing.Size(106, 23);
            this.simpleButton3.TabIndex = 0;
            this.simpleButton3.Text = "Don\'t save and close";
            // 
            // simpleButton2
            // 
            this.simpleButton2.Location = new System.Drawing.Point(84, 2);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(102, 23);
            this.simpleButton2.TabIndex = 0;
            this.simpleButton2.Text = "Show missing fields";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(3, 2);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 23);
            this.simpleButton1.TabIndex = 0;
            this.simpleButton1.Text = "Edit Dialog";
            // 
            // memoEditJSON
            // 
            this.memoEditJSON.Dock = System.Windows.Forms.DockStyle.Fill;
            this.memoEditJSON.EditValue = "";
            this.memoEditJSON.Location = new System.Drawing.Point(2, 22);
            this.memoEditJSON.Name = "memoEditJSON";
            this.memoEditJSON.Properties.Appearance.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.memoEditJSON.Properties.Appearance.Options.UseFont = true;
            this.memoEditJSON.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.memoEditJSON.Properties.LinesCount = 10;
            this.memoEditJSON.Properties.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.memoEditJSON.Properties.WordWrap = false;
            this.memoEditJSON.Size = new System.Drawing.Size(299, 494);
            this.memoEditJSON.TabIndex = 3;
            // 
            // simpleButtonPreview
            // 
            this.simpleButtonPreview.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.simpleButtonPreview.Appearance.Options.UseFont = true;
            this.simpleButtonPreview.Location = new System.Drawing.Point(321, 217);
            this.simpleButtonPreview.Name = "simpleButtonPreview";
            this.simpleButtonPreview.Size = new System.Drawing.Size(88, 23);
            this.simpleButtonPreview.TabIndex = 4;
            this.simpleButtonPreview.Text = "Update >>";
            this.simpleButtonPreview.Click += new System.EventHandler(this.simpleButtonPreview_Click);
            // 
            // simpleButtonEdit
            // 
            this.simpleButtonEdit.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.simpleButtonEdit.Appearance.Options.UseFont = true;
            this.simpleButtonEdit.Location = new System.Drawing.Point(321, 246);
            this.simpleButtonEdit.Name = "simpleButtonEdit";
            this.simpleButtonEdit.Size = new System.Drawing.Size(88, 23);
            this.simpleButtonEdit.TabIndex = 4;
            this.simpleButtonEdit.Text = "<< Edit Script";
            this.simpleButtonEdit.Click += new System.EventHandler(this.simpleButtonEdit_Click);
            // 
            // groupControl1
            // 
            this.defaultToolTipController1.SetAllowHtmlText(this.groupControl1, DevExpress.Utils.DefaultBoolean.Default);
            this.groupControl1.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.groupControl1.AppearanceCaption.Options.UseFont = true;
            this.groupControl1.Controls.Add(this.memoEditJSON);
            this.groupControl1.Location = new System.Drawing.Point(12, 12);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(303, 518);
            this.groupControl1.TabIndex = 5;
            this.groupControl1.Text = "Script";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(417, 537);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // defaultToolTipController1
            // 
            // 
            // 
            // 
            this.defaultToolTipController1.DefaultController.Appearance.BackColor = System.Drawing.Color.WhiteSmoke;
            this.defaultToolTipController1.DefaultController.Appearance.BackColor2 = System.Drawing.Color.Gainsboro;
            this.defaultToolTipController1.DefaultController.Appearance.BorderColor = System.Drawing.Color.Gray;
            this.defaultToolTipController1.DefaultController.Appearance.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal;
            this.defaultToolTipController1.DefaultController.Appearance.Options.UseBackColor = true;
            this.defaultToolTipController1.DefaultController.Appearance.Options.UseBorderColor = true;
            this.defaultToolTipController1.DefaultController.Rounded = true;
            this.defaultToolTipController1.DefaultController.ToolTipType = DevExpress.Utils.ToolTipType.SuperTip;
            // 
            // FrmMain
            // 
            this.defaultToolTipController1.SetAllowHtmlText(this, DevExpress.Utils.DefaultBoolean.Default);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 567);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupControl1);
            this.Controls.Add(this.simpleButtonEdit);
            this.Controls.Add(this.simpleButtonPreview);
            this.Controls.Add(this.groupControl2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.LookAndFeel.SkinName = "Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "FrmMain";
            this.Text = "Brightvision Questionnaire Editor";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlQuestionnaire)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroupQuestionnaire)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.memoEditJSON.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControlQuestionnaire;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroupQuestionnaire;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.MemoEdit memoEditJSON;
        private DevExpress.XtraEditors.SimpleButton simpleButtonPreview;
        private DevExpress.XtraEditors.SimpleButton simpleButtonEdit;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.Utils.DefaultToolTipController defaultToolTipController1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SimpleButton simpleButton4;
        private DevExpress.XtraEditors.SimpleButton simpleButton3;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;

    }
}