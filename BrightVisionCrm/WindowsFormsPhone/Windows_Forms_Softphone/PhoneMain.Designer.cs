namespace Windows_Forms_Softphone
{
    partial class PhoneMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PhoneMain));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.Tssl_Ozeki = new System.Windows.Forms.ToolStripStatusLabel();
            this.Tsmi_About = new System.Windows.Forms.ToolStripMenuItem();
            this.Tsmi_HelpPage = new System.Windows.Forms.ToolStripMenuItem();
            this.Tsmi_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.buttonPickUp = new System.Windows.Forms.Button();
            this.buttonHangUp = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelIdentifier = new System.Windows.Forms.Label();
            this.labelRegStatus = new System.Windows.Forms.Label();
            this.labelDialingNumber = new System.Windows.Forms.Label();
            this.labelCallStateInfo = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel2,
            this.Tssl_Ozeki});
            this.statusStrip1.Location = new System.Drawing.Point(0, 435);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(290, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.AutoSize = false;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(194, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // Tssl_Ozeki
            // 
            this.Tssl_Ozeki.Name = "Tssl_Ozeki";
            this.Tssl_Ozeki.Size = new System.Drawing.Size(81, 17);
            this.Tssl_Ozeki.Text = "Ozeki product";
            // 
            // Tsmi_About
            // 
            this.Tsmi_About.Name = "Tsmi_About";
            this.Tsmi_About.Size = new System.Drawing.Size(250, 24);
            this.Tsmi_About.Text = "About";
            this.Tsmi_About.Click += new System.EventHandler(this.Tsmi_About_Click);
            // 
            // Tsmi_HelpPage
            // 
            this.Tsmi_HelpPage.Name = "Tsmi_HelpPage";
            this.Tsmi_HelpPage.Size = new System.Drawing.Size(250, 24);
            this.Tsmi_HelpPage.Text = "Open online documentation";
            this.Tsmi_HelpPage.Click += new System.EventHandler(this.Tsmi_HelpPage_Click);
            // 
            // Tsmi_Help
            // 
            this.Tsmi_Help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Tsmi_HelpPage,
            this.Tsmi_About});
            this.Tsmi_Help.Name = "Tsmi_Help";
            this.Tsmi_Help.Size = new System.Drawing.Size(49, 23);
            this.Tsmi_Help.Text = "Help";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.Tsmi_Help});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(290, 27);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(41, 23);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(99, 24);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(23, 208);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(63, 35);
            this.button1.TabIndex = 4;
            this.button1.Tag = "1";
            this.button1.Text = "1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(92, 208);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(63, 35);
            this.button2.TabIndex = 5;
            this.button2.Tag = "2";
            this.button2.Text = "2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(161, 208);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(63, 35);
            this.button3.TabIndex = 6;
            this.button3.Tag = "3";
            this.button3.Text = "3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(23, 249);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(63, 35);
            this.button4.TabIndex = 9;
            this.button4.Tag = "4";
            this.button4.Text = "4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button4.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(92, 249);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(63, 35);
            this.button5.TabIndex = 8;
            this.button5.Tag = "5";
            this.button5.Text = "5";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button5.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(161, 249);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(63, 35);
            this.button6.TabIndex = 7;
            this.button6.Tag = "6";
            this.button6.Text = "6";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button6.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(23, 290);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(63, 35);
            this.button7.TabIndex = 12;
            this.button7.Tag = "7";
            this.button7.Text = "7";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button7.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button7.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(92, 290);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(63, 35);
            this.button8.TabIndex = 11;
            this.button8.Tag = "8";
            this.button8.Text = "8";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button8.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button8.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(161, 290);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(63, 35);
            this.button9.TabIndex = 10;
            this.button9.Tag = "9";
            this.button9.Text = "9";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button9.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button9.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(23, 331);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(63, 35);
            this.button10.TabIndex = 15;
            this.button10.Tag = "10";
            this.button10.Text = "*";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button10.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button10.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(92, 331);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(63, 35);
            this.button11.TabIndex = 14;
            this.button11.Tag = "0";
            this.button11.Text = "0/+";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button11.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button11.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(161, 331);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(63, 35);
            this.button12.TabIndex = 13;
            this.button12.Tag = "11";
            this.button12.Text = "#";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.buttonKeyPadButton_Click);
            this.button12.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPadButton_MouseDown);
            this.button12.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonKeyPad_MouseUp);
            // 
            // buttonPickUp
            // 
            this.buttonPickUp.Location = new System.Drawing.Point(23, 167);
            this.buttonPickUp.Name = "buttonPickUp";
            this.buttonPickUp.Size = new System.Drawing.Size(63, 35);
            this.buttonPickUp.TabIndex = 16;
            this.buttonPickUp.Text = "Pick Up";
            this.buttonPickUp.UseVisualStyleBackColor = true;
            this.buttonPickUp.Click += new System.EventHandler(this.buttonPickUp_Click);
            // 
            // buttonHangUp
            // 
            this.buttonHangUp.Location = new System.Drawing.Point(161, 167);
            this.buttonHangUp.Name = "buttonHangUp";
            this.buttonHangUp.Size = new System.Drawing.Size(63, 35);
            this.buttonHangUp.TabIndex = 17;
            this.buttonHangUp.Text = "Hang Up";
            this.buttonHangUp.UseVisualStyleBackColor = true;
            this.buttonHangUp.Click += new System.EventHandler(this.buttonHangUp_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.labelIdentifier);
            this.panel1.Controls.Add(this.labelRegStatus);
            this.panel1.Controls.Add(this.labelDialingNumber);
            this.panel1.Controls.Add(this.labelCallStateInfo);
            this.panel1.Location = new System.Drawing.Point(20, 17);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(203, 130);
            this.panel1.TabIndex = 18;
            // 
            // labelIdentifier
            // 
            this.labelIdentifier.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelIdentifier.Location = new System.Drawing.Point(121, 6);
            this.labelIdentifier.Name = "labelIdentifier";
            this.labelIdentifier.Size = new System.Drawing.Size(79, 18);
            this.labelIdentifier.TabIndex = 3;
            this.labelIdentifier.Text = "Identifier";
            this.labelIdentifier.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelRegStatus
            // 
            this.labelRegStatus.AutoSize = true;
            this.labelRegStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelRegStatus.Location = new System.Drawing.Point(3, 6);
            this.labelRegStatus.Name = "labelRegStatus";
            this.labelRegStatus.Size = new System.Drawing.Size(50, 18);
            this.labelRegStatus.TabIndex = 2;
            this.labelRegStatus.Text = "Offline";
            // 
            // labelDialingNumber
            // 
            this.labelDialingNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelDialingNumber.Location = new System.Drawing.Point(9, 80);
            this.labelDialingNumber.Name = "labelDialingNumber";
            this.labelDialingNumber.Size = new System.Drawing.Size(180, 22);
            this.labelDialingNumber.TabIndex = 1;
            this.labelDialingNumber.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // labelCallStateInfo
            // 
            this.labelCallStateInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelCallStateInfo.Location = new System.Drawing.Point(5, 44);
            this.labelCallStateInfo.Name = "labelCallStateInfo";
            this.labelCallStateInfo.Size = new System.Drawing.Size(195, 24);
            this.labelCallStateInfo.TabIndex = 0;
            this.labelCallStateInfo.Text = "No connection";
            this.labelCallStateInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.buttonHangUp);
            this.panel2.Controls.Add(this.buttonPickUp);
            this.panel2.Controls.Add(this.button10);
            this.panel2.Controls.Add(this.button11);
            this.panel2.Controls.Add(this.button12);
            this.panel2.Controls.Add(this.button7);
            this.panel2.Controls.Add(this.button8);
            this.panel2.Controls.Add(this.button9);
            this.panel2.Controls.Add(this.button4);
            this.panel2.Controls.Add(this.button5);
            this.panel2.Controls.Add(this.button6);
            this.panel2.Controls.Add(this.button3);
            this.panel2.Controls.Add(this.button2);
            this.panel2.Controls.Add(this.button1);
            this.panel2.Location = new System.Drawing.Point(21, 39);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(245, 385);
            this.panel2.TabIndex = 19;
            // 
            // PhoneMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(290, 457);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PhoneMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ozeki Windows Forms Softphone Sample";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PhoneMain_FormClosed);
            this.Load += new System.EventHandler(this.PhoneMain_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel Tssl_Ozeki;
        private System.Windows.Forms.ToolStripMenuItem Tsmi_About;
        private System.Windows.Forms.ToolStripMenuItem Tsmi_HelpPage;
        private System.Windows.Forms.ToolStripMenuItem Tsmi_Help;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button buttonPickUp;
        private System.Windows.Forms.Button buttonHangUp;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelIdentifier;
        private System.Windows.Forms.Label labelRegStatus;
        private System.Windows.Forms.Label labelDialingNumber;
        private System.Windows.Forms.Label labelCallStateInfo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}

