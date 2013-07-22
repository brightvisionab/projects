namespace SalesConsultant.Modules
{
    partial class CallViewBar
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
            this.tbxCallUsageTime = new DevExpress.XtraEditors.TextEdit();
            this.tbxNoToCall = new DevExpress.XtraEditors.TextEdit();
            this.btnCallMobile = new DevExpress.XtraEditors.SimpleButton();
            this.btnCallDirect = new DevExpress.XtraEditors.SimpleButton();
            this.btnCallBoard = new DevExpress.XtraEditors.SimpleButton();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.lciTimer = new DevExpress.XtraLayout.LayoutControlItem();
            this.tmrCallTimer = new System.Windows.Forms.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbxCallUsageTime.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxNoToCall.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomizationMenu = false;
            this.layoutControl1.Controls.Add(this.tbxCallUsageTime);
            this.layoutControl1.Controls.Add(this.tbxNoToCall);
            this.layoutControl1.Controls.Add(this.btnCallMobile);
            this.layoutControl1.Controls.Add(this.btnCallDirect);
            this.layoutControl1.Controls.Add(this.btnCallBoard);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(21, 139, 250, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(285, 26);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // tbxCallUsageTime
            // 
            this.tbxCallUsageTime.EditValue = "00 : 00";
            this.tbxCallUsageTime.Location = new System.Drawing.Point(240, 2);
            this.tbxCallUsageTime.Name = "tbxCallUsageTime";
            this.tbxCallUsageTime.Properties.Appearance.BackColor = System.Drawing.SystemColors.Control;
            this.tbxCallUsageTime.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
            this.tbxCallUsageTime.Properties.Appearance.Options.UseBackColor = true;
            this.tbxCallUsageTime.Properties.Appearance.Options.UseForeColor = true;
            this.tbxCallUsageTime.Properties.Appearance.Options.UseTextOptions = true;
            this.tbxCallUsageTime.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.tbxCallUsageTime.Properties.ReadOnly = true;
            this.tbxCallUsageTime.Size = new System.Drawing.Size(43, 20);
            this.tbxCallUsageTime.StyleController = this.layoutControl1;
            this.tbxCallUsageTime.TabIndex = 10;
            this.tbxCallUsageTime.ToolTip = "Timer";
            // 
            // tbxNoToCall
            // 
            this.tbxNoToCall.EditValue = "";
            this.tbxNoToCall.Location = new System.Drawing.Point(98, 2);
            this.tbxNoToCall.Name = "tbxNoToCall";
            this.tbxNoToCall.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.tbxNoToCall.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.tbxNoToCall.Properties.Appearance.ForeColor = System.Drawing.Color.Maroon;
            this.tbxNoToCall.Properties.Appearance.Options.UseBackColor = true;
            this.tbxNoToCall.Properties.Appearance.Options.UseFont = true;
            this.tbxNoToCall.Properties.Appearance.Options.UseForeColor = true;
            this.tbxNoToCall.Properties.Appearance.Options.UseTextOptions = true;
            this.tbxNoToCall.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.tbxNoToCall.Size = new System.Drawing.Size(140, 20);
            this.tbxNoToCall.StyleController = this.layoutControl1;
            this.tbxNoToCall.TabIndex = 9;
            this.tbxNoToCall.ToolTip = "Enter no to call";
            this.tbxNoToCall.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxNoToCall_KeyUp);
            // 
            // btnCallMobile
            // 
            this.btnCallMobile.Image = global::SalesConsultant.Properties.Resources.call_mobile;
            this.btnCallMobile.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.btnCallMobile.Location = new System.Drawing.Point(66, 2);
            this.btnCallMobile.Name = "btnCallMobile";
            this.btnCallMobile.Size = new System.Drawing.Size(28, 22);
            this.btnCallMobile.StyleController = this.layoutControl1;
            this.btnCallMobile.TabIndex = 8;
            this.btnCallMobile.ToolTip = "Mobile Phone";
            this.btnCallMobile.Click += new System.EventHandler(this.btnCallMobile_Click);
            // 
            // btnCallDirect
            // 
            this.btnCallDirect.Image = global::SalesConsultant.Properties.Resources.call_direct;
            this.btnCallDirect.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.btnCallDirect.Location = new System.Drawing.Point(34, 2);
            this.btnCallDirect.Name = "btnCallDirect";
            this.btnCallDirect.Size = new System.Drawing.Size(28, 22);
            this.btnCallDirect.StyleController = this.layoutControl1;
            this.btnCallDirect.TabIndex = 7;
            this.btnCallDirect.Text = " ";
            this.btnCallDirect.ToolTip = "Call Direct";
            this.btnCallDirect.Click += new System.EventHandler(this.btnCallDirect_Click);
            // 
            // btnCallBoard
            // 
            this.btnCallBoard.Image = global::SalesConsultant.Properties.Resources.call_board_2;
            this.btnCallBoard.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.btnCallBoard.Location = new System.Drawing.Point(2, 2);
            this.btnCallBoard.Name = "btnCallBoard";
            this.btnCallBoard.Size = new System.Drawing.Size(28, 22);
            this.btnCallBoard.StyleController = this.layoutControl1;
            this.btnCallBoard.TabIndex = 6;
            this.btnCallBoard.ToolTip = "Call Board";
            this.btnCallBoard.Click += new System.EventHandler(this.btnCallBoard_Click);
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem3,
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.layoutControlItem6,
            this.lciTimer});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlGroup1.Size = new System.Drawing.Size(285, 26);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.btnCallBoard;
            this.layoutControlItem3.CustomizationFormText = "layoutControlItem3";
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem3.MaxSize = new System.Drawing.Size(32, 26);
            this.layoutControlItem3.MinSize = new System.Drawing.Size(32, 26);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(32, 26);
            this.layoutControlItem3.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem3.Text = "layoutControlItem3";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextToControlDistance = 0;
            this.layoutControlItem3.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.btnCallDirect;
            this.layoutControlItem4.CustomizationFormText = "layoutControlItem4";
            this.layoutControlItem4.Location = new System.Drawing.Point(32, 0);
            this.layoutControlItem4.MaxSize = new System.Drawing.Size(32, 26);
            this.layoutControlItem4.MinSize = new System.Drawing.Size(32, 26);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(32, 26);
            this.layoutControlItem4.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem4.Text = "layoutControlItem4";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextToControlDistance = 0;
            this.layoutControlItem4.TextVisible = false;
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.btnCallMobile;
            this.layoutControlItem5.CustomizationFormText = "layoutControlItem5";
            this.layoutControlItem5.Location = new System.Drawing.Point(64, 0);
            this.layoutControlItem5.MaxSize = new System.Drawing.Size(32, 26);
            this.layoutControlItem5.MinSize = new System.Drawing.Size(32, 26);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(32, 26);
            this.layoutControlItem5.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem5.Text = "layoutControlItem5";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextToControlDistance = 0;
            this.layoutControlItem5.TextVisible = false;
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.tbxNoToCall;
            this.layoutControlItem6.CustomizationFormText = "layoutControlItem6";
            this.layoutControlItem6.Location = new System.Drawing.Point(96, 0);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(144, 26);
            this.layoutControlItem6.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.SupportHorzAlignment;
            this.layoutControlItem6.Text = "layoutControlItem6";
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextToControlDistance = 0;
            this.layoutControlItem6.TextVisible = false;
            // 
            // lciTimer
            // 
            this.lciTimer.Control = this.tbxCallUsageTime;
            this.lciTimer.CustomizationFormText = "layoutControlItem7";
            this.lciTimer.Location = new System.Drawing.Point(240, 0);
            this.lciTimer.MaxSize = new System.Drawing.Size(45, 24);
            this.lciTimer.MinSize = new System.Drawing.Size(10, 24);
            this.lciTimer.Name = "lciTimer";
            this.lciTimer.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 2, 0);
            this.lciTimer.Size = new System.Drawing.Size(45, 26);
            this.lciTimer.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.lciTimer.Spacing = new DevExpress.XtraLayout.Utils.Padding(0, 2, 0, 0);
            this.lciTimer.Text = "lciTimer";
            this.lciTimer.TextSize = new System.Drawing.Size(0, 0);
            this.lciTimer.TextToControlDistance = 0;
            this.lciTimer.TextVisible = false;
            this.lciTimer.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            // 
            // tmrCallTimer
            // 
            this.tmrCallTimer.Interval = 1000;
            this.tmrCallTimer.Tick += new System.EventHandler(this.tmrCallTimer_Tick);
            // 
            // CallViewBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.layoutControl1);
            this.Name = "CallViewBar";
            this.Size = new System.Drawing.Size(285, 26);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tbxCallUsageTime.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbxNoToCall.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciTimer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraEditors.TextEdit tbxNoToCall;
        private DevExpress.XtraEditors.SimpleButton btnCallMobile;
        private DevExpress.XtraEditors.SimpleButton btnCallDirect;
        private DevExpress.XtraEditors.SimpleButton btnCallBoard;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private System.Windows.Forms.Timer tmrCallTimer;
        private DevExpress.XtraEditors.TextEdit tbxCallUsageTime;
        private DevExpress.XtraLayout.LayoutControlItem lciTimer;
    }
}
