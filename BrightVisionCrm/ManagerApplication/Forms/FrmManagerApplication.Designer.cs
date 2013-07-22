namespace ManagerApplication.Forms
{
    partial class FrmManagerApplication
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
            this.components = new System.ComponentModel.Container();
            DevExpress.XtraSplashScreen.SplashScreenManager splashScreenManager1 = new DevExpress.XtraSplashScreen.SplashScreenManager(this, typeof(global::ManagerApplication.SplashScreen1), true, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmManagerApplication));
            this.navBarMenu = new DevExpress.XtraNavBar.NavBarControl();
            this.nbarGroup2 = new DevExpress.XtraNavBar.NavBarGroup();
            this.nbiUser = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItem4 = new DevExpress.XtraNavBar.NavBarItem();
            this.nbiCustomerCampaign = new DevExpress.XtraNavBar.NavBarItem();
            this.nbiSubCampaign = new DevExpress.XtraNavBar.NavBarItem();
            this.nbiCompanyContact = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarGroup3 = new DevExpress.XtraNavBar.NavBarGroup();
            this.nbiImporAndViewtList = new DevExpress.XtraNavBar.NavBarItem();
            this.nbiListCreation = new DevExpress.XtraNavBar.NavBarItem();
            this.nbiCallList = new DevExpress.XtraNavBar.NavBarItem();
            this.nbiFinalizedCallList = new DevExpress.XtraNavBar.NavBarItem();
            this.nbiMatchFilemakerMigrated = new DevExpress.XtraNavBar.NavBarItem();
            this.nBarGroup5 = new DevExpress.XtraNavBar.NavBarGroup();
            this.nbiGeoMapCompany = new DevExpress.XtraNavBar.NavBarItem();
            this.nbiGeoMapContact = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarGroup1 = new DevExpress.XtraNavBar.NavBarGroup();
            this.navBarItemManageQuestions = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarGroup2 = new DevExpress.XtraNavBar.NavBarGroup();
            this.navBarItemManageDialogs = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarGroup4 = new DevExpress.XtraNavBar.NavBarGroup();
            this.navPhoneSettings = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItem2 = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItem3 = new DevExpress.XtraNavBar.NavBarItem();
            this.nbiAutoTitleCorrection = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItem6 = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarGroup5 = new DevExpress.XtraNavBar.NavBarGroup();
            this.navBarItemViewConfiguration = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItemDisplayViews = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItem10 = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarGroup6 = new DevExpress.XtraNavBar.NavBarGroup();
            this.navBarItem1 = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItem9 = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItem8 = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItem11 = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItem12 = new DevExpress.XtraNavBar.NavBarItem();
            this.navBarItem7 = new DevExpress.XtraNavBar.NavBarItem();
            this.pnlContent = new DevExpress.XtraEditors.PanelControl();
            this.navBarItem5 = new DevExpress.XtraNavBar.NavBarItem();
            this.barStaticApplication = new DevExpress.XtraBars.BarStaticItem();
            this.barStaticBuildVersion = new DevExpress.XtraBars.BarStaticItem();
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.barButtonOption = new DevExpress.XtraBars.BarButtonItem();
            this.popupMenu1 = new DevExpress.XtraBars.PopupMenu(this.components);
            this.barButtonLogout = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonExit = new DevExpress.XtraBars.BarButtonItem();
            this.barStaticEnvironment = new DevExpress.XtraBars.BarStaticItem();
            ((System.ComponentModel.ISupportInitialize)(this.navBarMenu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlContent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu1)).BeginInit();
            this.SuspendLayout();
            // 
            // navBarMenu
            // 
            this.navBarMenu.ActiveGroup = this.nbarGroup2;
            this.navBarMenu.AllowSelectedLink = true;
            this.navBarMenu.Appearance.ItemPressed.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline);
            this.navBarMenu.Appearance.ItemPressed.Options.UseFont = true;
            this.navBarMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.navBarMenu.Groups.AddRange(new DevExpress.XtraNavBar.NavBarGroup[] {
            this.nbarGroup2,
            this.navBarGroup3,
            this.nBarGroup5,
            this.navBarGroup1,
            this.navBarGroup2,
            this.navBarGroup4,
            this.navBarGroup5,
            this.navBarGroup6});
            this.navBarMenu.Items.AddRange(new DevExpress.XtraNavBar.NavBarItem[] {
            this.nbiUser,
            this.nbiCustomerCampaign,
            this.nbiSubCampaign,
            this.nbiGeoMapCompany,
            this.nbiGeoMapContact,
            this.nbiCompanyContact,
            this.navBarItemManageQuestions,
            this.navBarItemManageDialogs,
            this.nbiImporAndViewtList,
            this.nbiListCreation,
            this.nbiCallList,
            this.nbiFinalizedCallList,
            this.navBarItem2,
            this.navBarItem3,
            this.navBarItem4,
            this.navBarItemViewConfiguration,
            this.navBarItemDisplayViews,
            this.nbiMatchFilemakerMigrated,
            this.navBarItem1,
            this.navBarItem8,
            this.navBarItem9,
            this.nbiAutoTitleCorrection,
            this.navBarItem10,
            this.navBarItem6,
            this.navBarItem7,
            this.navPhoneSettings,
            this.navBarItem11,
            this.navBarItem12});
            this.navBarMenu.Location = new System.Drawing.Point(0, 27);
            this.navBarMenu.Name = "navBarMenu";
            this.navBarMenu.OptionsNavPane.ExpandedWidth = 220;
            this.navBarMenu.Size = new System.Drawing.Size(220, 843);
            this.navBarMenu.TabIndex = 13;
            this.navBarMenu.Text = "nbarContextMenu";
            this.navBarMenu.View = new DevExpress.XtraNavBar.ViewInfo.StandardSkinExplorerBarViewInfoRegistrator("DevExpress Style");
            // 
            // nbarGroup2
            // 
            this.nbarGroup2.Caption = "List";
            this.nbarGroup2.Expanded = true;
            this.nbarGroup2.ItemLinks.AddRange(new DevExpress.XtraNavBar.NavBarItemLink[] {
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiUser),
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItem4),
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiCustomerCampaign),
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiSubCampaign),
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiCompanyContact)});
            this.nbarGroup2.Name = "nbarGroup2";
            // 
            // nbiUser
            // 
            this.nbiUser.Caption = "Internal Users";
            this.nbiUser.Name = "nbiUser";
            this.nbiUser.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbarItemUser_LinkClicked);
            // 
            // navBarItem4
            // 
            this.navBarItem4.Caption = "Customer Users";
            this.navBarItem4.Name = "navBarItem4";
            this.navBarItem4.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItem4_LinkClicked);
            // 
            // nbiCustomerCampaign
            // 
            this.nbiCustomerCampaign.Caption = "Customers and Campaigns";
            this.nbiCustomerCampaign.Name = "nbiCustomerCampaign";
            this.nbiCustomerCampaign.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbarItemCustomer_LinkClicked);
            // 
            // nbiSubCampaign
            // 
            this.nbiSubCampaign.Caption = "Sub - Campaigns";
            this.nbiSubCampaign.Name = "nbiSubCampaign";
            this.nbiSubCampaign.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbiSubCampaign_LinkClicked);
            // 
            // nbiCompanyContact
            // 
            this.nbiCompanyContact.Caption = "Companies and Contacts";
            this.nbiCompanyContact.Name = "nbiCompanyContact";
            this.nbiCompanyContact.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbiCompanyContact_LinkClicked);
            // 
            // navBarGroup3
            // 
            this.navBarGroup3.Caption = "List Handling";
            this.navBarGroup3.Expanded = true;
            this.navBarGroup3.ItemLinks.AddRange(new DevExpress.XtraNavBar.NavBarItemLink[] {
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiImporAndViewtList),
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiListCreation),
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiCallList),
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiFinalizedCallList),
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiMatchFilemakerMigrated)});
            this.navBarGroup3.Name = "navBarGroup3";
            // 
            // nbiImporAndViewtList
            // 
            this.nbiImporAndViewtList.Caption = "Import and View List";
            this.nbiImporAndViewtList.Name = "nbiImporAndViewtList";
            this.nbiImporAndViewtList.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbiImporAndViewtList_LinkClicked);
            // 
            // nbiListCreation
            // 
            this.nbiListCreation.Caption = "List Creation";
            this.nbiListCreation.Name = "nbiListCreation";
            this.nbiListCreation.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbiListCreation_LinkClicked);
            // 
            // nbiCallList
            // 
            this.nbiCallList.Caption = "Call List Creation";
            this.nbiCallList.Name = "nbiCallList";
            this.nbiCallList.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbiCallList_LinkClicked);
            // 
            // nbiFinalizedCallList
            // 
            this.nbiFinalizedCallList.Caption = "Final Call Lists";
            this.nbiFinalizedCallList.Name = "nbiFinalizedCallList";
            this.nbiFinalizedCallList.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbiFinalizedCallList_LinkClicked);
            // 
            // nbiMatchFilemakerMigrated
            // 
            this.nbiMatchFilemakerMigrated.Caption = "Match FileMakerMigrated";
            this.nbiMatchFilemakerMigrated.Name = "nbiMatchFilemakerMigrated";
            this.nbiMatchFilemakerMigrated.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbiMatchFilemakerMigrated_LinkClicked);
            // 
            // nBarGroup5
            // 
            this.nBarGroup5.Caption = "Geo Matching";
            this.nBarGroup5.Expanded = true;
            this.nBarGroup5.ItemLinks.AddRange(new DevExpress.XtraNavBar.NavBarItemLink[] {
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiGeoMapCompany),
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiGeoMapContact)});
            this.nBarGroup5.Name = "nBarGroup5";
            // 
            // nbiGeoMapCompany
            // 
            this.nbiGeoMapCompany.Caption = "Companies";
            this.nbiGeoMapCompany.Name = "nbiGeoMapCompany";
            this.nbiGeoMapCompany.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbiGeoMapCompany_LinkClicked);
            // 
            // nbiGeoMapContact
            // 
            this.nbiGeoMapContact.Caption = "Contacts";
            this.nbiGeoMapContact.Name = "nbiGeoMapContact";
            this.nbiGeoMapContact.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbiGeoMapContact_LinkClicked);
            // 
            // navBarGroup1
            // 
            this.navBarGroup1.Caption = "Questionnaire Editor";
            this.navBarGroup1.Expanded = true;
            this.navBarGroup1.ItemLinks.AddRange(new DevExpress.XtraNavBar.NavBarItemLink[] {
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItemManageQuestions)});
            this.navBarGroup1.Name = "navBarGroup1";
            // 
            // navBarItemManageQuestions
            // 
            this.navBarItemManageQuestions.Caption = "Manage Questions";
            this.navBarItemManageQuestions.Name = "navBarItemManageQuestions";
            this.navBarItemManageQuestions.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItemManageQuestions_LinkClicked);
            // 
            // navBarGroup2
            // 
            this.navBarGroup2.Caption = "Dialog Editor";
            this.navBarGroup2.Expanded = true;
            this.navBarGroup2.ItemLinks.AddRange(new DevExpress.XtraNavBar.NavBarItemLink[] {
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItemManageDialogs)});
            this.navBarGroup2.Name = "navBarGroup2";
            // 
            // navBarItemManageDialogs
            // 
            this.navBarItemManageDialogs.Caption = "Manage Dialogs";
            this.navBarItemManageDialogs.Name = "navBarItemManageDialogs";
            this.navBarItemManageDialogs.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItemManageDialogs_LinkClicked);
            // 
            // navBarGroup4
            // 
            this.navBarGroup4.Caption = "Settings";
            this.navBarGroup4.Expanded = true;
            this.navBarGroup4.ItemLinks.AddRange(new DevExpress.XtraNavBar.NavBarItemLink[] {
            new DevExpress.XtraNavBar.NavBarItemLink(this.navPhoneSettings),
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItem2),
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItem3),
            new DevExpress.XtraNavBar.NavBarItemLink(this.nbiAutoTitleCorrection),
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItem6)});
            this.navBarGroup4.Name = "navBarGroup4";
            // 
            // navPhoneSettings
            // 
            this.navPhoneSettings.Caption = "SIP Accounts";
            this.navPhoneSettings.Name = "navPhoneSettings";
            this.navPhoneSettings.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navPhoneSettings_LinkClicked);
            // 
            // navBarItem2
            // 
            this.navBarItem2.Caption = "Languages";
            this.navBarItem2.Name = "navBarItem2";
            this.navBarItem2.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItem2_LinkClicked);
            // 
            // navBarItem3
            // 
            this.navBarItem3.Caption = "Titles";
            this.navBarItem3.Name = "navBarItem3";
            this.navBarItem3.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItem3_LinkClicked);
            // 
            // nbiAutoTitleCorrection
            // 
            this.nbiAutoTitleCorrection.Caption = "Auto Title Correction";
            this.nbiAutoTitleCorrection.Name = "nbiAutoTitleCorrection";
            this.nbiAutoTitleCorrection.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.nbiAutoTitleCorrection_LinkClicked);
            // 
            // navBarItem6
            // 
            this.navBarItem6.Caption = "Report Templates";
            this.navBarItem6.Name = "navBarItem6";
            this.navBarItem6.Visible = false;
            this.navBarItem6.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItem6_LinkClicked);
            // 
            // navBarGroup5
            // 
            this.navBarGroup5.Caption = "Export";
            this.navBarGroup5.Expanded = true;
            this.navBarGroup5.ItemLinks.AddRange(new DevExpress.XtraNavBar.NavBarItemLink[] {
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItemViewConfiguration),
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItemDisplayViews),
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItem10)});
            this.navBarGroup5.Name = "navBarGroup5";
            // 
            // navBarItemViewConfiguration
            // 
            this.navBarItemViewConfiguration.Caption = "Report Configuration";
            this.navBarItemViewConfiguration.Name = "navBarItemViewConfiguration";
            this.navBarItemViewConfiguration.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItemViewConfiguration_LinkClicked);
            // 
            // navBarItemDisplayViews
            // 
            this.navBarItemDisplayViews.Caption = "Display Report";
            this.navBarItemDisplayViews.Name = "navBarItemDisplayViews";
            this.navBarItemDisplayViews.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItemDisplayViews_LinkClicked);
            // 
            // navBarItem10
            // 
            this.navBarItem10.Caption = "Follow-up & Emails";
            this.navBarItem10.Name = "navBarItem10";
            this.navBarItem10.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItem10_LinkClicked);
            // 
            // navBarGroup6
            // 
            this.navBarGroup6.Caption = "Master Data Tools";
            this.navBarGroup6.Expanded = true;
            this.navBarGroup6.ItemLinks.AddRange(new DevExpress.XtraNavBar.NavBarItemLink[] {
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItem1),
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItem9),
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItem8),
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItem11),
            new DevExpress.XtraNavBar.NavBarItemLink(this.navBarItem12)});
            this.navBarGroup6.Name = "navBarGroup6";
            // 
            // navBarItem1
            // 
            this.navBarItem1.Caption = "Master Data Import";
            this.navBarItem1.Name = "navBarItem1";
            this.navBarItem1.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItem1_LinkClicked);
            // 
            // navBarItem9
            // 
            this.navBarItem9.Caption = "Email Verification";
            this.navBarItem9.Name = "navBarItem9";
            this.navBarItem9.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItem9_LinkClicked);
            // 
            // navBarItem8
            // 
            this.navBarItem8.Caption = "View Event Logs";
            this.navBarItem8.Name = "navBarItem8";
            this.navBarItem8.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItem8_LinkClicked);
            // 
            // navBarItem11
            // 
            this.navBarItem11.Caption = "View Call Logs";
            this.navBarItem11.Name = "navBarItem11";
            this.navBarItem11.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItem11_LinkClicked);
            // 
            // navBarItem12
            // 
            this.navBarItem12.Caption = "View Message Logs";
            this.navBarItem12.Name = "navBarItem12";
            this.navBarItem12.LinkClicked += new DevExpress.XtraNavBar.NavBarLinkEventHandler(this.navBarItem12_LinkClicked);
            // 
            // navBarItem7
            // 
            this.navBarItem7.Caption = "navBarItem7";
            this.navBarItem7.Name = "navBarItem7";
            // 
            // pnlContent
            // 
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(220, 27);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(1216, 843);
            this.pnlContent.TabIndex = 14;
            // 
            // navBarItem5
            // 
            this.navBarItem5.Caption = "Final Call Lists";
            this.navBarItem5.Name = "navBarItem5";
            // 
            // barStaticApplication
            // 
            this.barStaticApplication.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Left;
            this.barStaticApplication.Id = 0;
            this.barStaticApplication.Name = "barStaticApplication";
            this.barStaticApplication.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // barStaticBuildVersion
            // 
            this.barStaticBuildVersion.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Left;
            this.barStaticBuildVersion.Id = 0;
            this.barStaticBuildVersion.Name = "barStaticBuildVersion";
            this.barStaticBuildVersion.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // ribbonControl1
            // 
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem,
            this.barStaticApplication,
            this.barStaticBuildVersion,
            this.barButtonOption,
            this.barStaticEnvironment,
            this.barButtonLogout,
            this.barButtonExit});
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.MaxItemId = 7;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.Office2010;
            this.ribbonControl1.ShowApplicationButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbonControl1.ShowCategoryInCaption = false;
            this.ribbonControl1.ShowExpandCollapseButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbonControl1.ShowPageHeadersMode = DevExpress.XtraBars.Ribbon.ShowPageHeadersMode.Hide;
            this.ribbonControl1.ShowToolbarCustomizeItem = false;
            this.ribbonControl1.Size = new System.Drawing.Size(1436, 27);
            this.ribbonControl1.Toolbar.ItemLinks.Add(this.barButtonOption);
            this.ribbonControl1.Toolbar.ShowCustomizeItem = false;
            // 
            // barButtonOption
            // 
            this.barButtonOption.ActAsDropDown = true;
            this.barButtonOption.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.DropDown;
            this.barButtonOption.DropDownControl = this.popupMenu1;
            this.barButtonOption.Glyph = ((System.Drawing.Image)(resources.GetObject("barButtonOption.Glyph")));
            this.barButtonOption.Id = 3;
            this.barButtonOption.Name = "barButtonOption";
            // 
            // popupMenu1
            // 
            this.popupMenu1.ItemLinks.Add(this.barButtonLogout);
            this.popupMenu1.ItemLinks.Add(this.barButtonExit);
            this.popupMenu1.ItemLinks.Add(this.barStaticApplication, true);
            this.popupMenu1.ItemLinks.Add(this.barStaticBuildVersion);
            this.popupMenu1.ItemLinks.Add(this.barStaticEnvironment);
            this.popupMenu1.Name = "popupMenu1";
            this.popupMenu1.Ribbon = this.ribbonControl1;
            // 
            // barButtonLogout
            // 
            this.barButtonLogout.Caption = "Logout";
            this.barButtonLogout.Glyph = global::ManagerApplication.Properties.Resources.logout;
            this.barButtonLogout.Id = 5;
            this.barButtonLogout.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L));
            this.barButtonLogout.Name = "barButtonLogout";
            this.barButtonLogout.ShortcutKeyDisplayString = "Ctrl+L";
            this.barButtonLogout.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonLogout_ItemClick);
            // 
            // barButtonExit
            // 
            this.barButtonExit.Caption = "Exit";
            this.barButtonExit.Glyph = global::ManagerApplication.Properties.Resources.close;
            this.barButtonExit.Id = 6;
            this.barButtonExit.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4));
            this.barButtonExit.Name = "barButtonExit";
            this.barButtonExit.ShortcutKeyDisplayString = "Alt+F4";
            this.barButtonExit.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonExit_ItemClick);
            // 
            // barStaticEnvironment
            // 
            this.barStaticEnvironment.Id = 4;
            this.barStaticEnvironment.Name = "barStaticEnvironment";
            this.barStaticEnvironment.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // FrmManagerApplication
            // 
            this.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1436, 870);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.navBarMenu);
            this.Controls.Add(this.ribbonControl1);
            this.Icon = global::ManagerApplication.Properties.Resources.bv_logo;
            this.Name = "FrmManagerApplication";
            this.Ribbon = this.ribbonControl1;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BrightManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmManagerApplication_FormClosing);
            this.Load += new System.EventHandler(this.FrmManagerApplication_Load);
            ((System.ComponentModel.ISupportInitialize)(this.navBarMenu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlContent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraNavBar.NavBarControl navBarMenu;
        private DevExpress.XtraNavBar.NavBarGroup nbarGroup2;
        private DevExpress.XtraNavBar.NavBarItem nbiUser;
        private DevExpress.XtraNavBar.NavBarItem nbiCustomerCampaign;
        private DevExpress.XtraEditors.PanelControl pnlContent;
        private DevExpress.XtraNavBar.NavBarItem nbiSubCampaign;
        private DevExpress.XtraNavBar.NavBarGroup nBarGroup5;
        private DevExpress.XtraNavBar.NavBarItem nbiGeoMapCompany;
        private DevExpress.XtraNavBar.NavBarItem nbiGeoMapContact;
        private DevExpress.XtraNavBar.NavBarItem nbiCompanyContact;
        private DevExpress.XtraNavBar.NavBarGroup navBarGroup1;
        private DevExpress.XtraNavBar.NavBarItem navBarItemManageQuestions;
        private DevExpress.XtraNavBar.NavBarGroup navBarGroup2;
        private DevExpress.XtraNavBar.NavBarItem navBarItemManageDialogs;
        private DevExpress.XtraNavBar.NavBarGroup navBarGroup3;
        private DevExpress.XtraNavBar.NavBarItem nbiImporAndViewtList;
        private DevExpress.XtraNavBar.NavBarItem nbiListCreation;
        private DevExpress.XtraNavBar.NavBarItem nbiCallList;
        private DevExpress.XtraNavBar.NavBarItem nbiFinalizedCallList;
        private DevExpress.XtraNavBar.NavBarGroup navBarGroup4;
        private DevExpress.XtraNavBar.NavBarItem navBarItem2;
        private DevExpress.XtraNavBar.NavBarItem navBarItem3;
        private DevExpress.XtraNavBar.NavBarItem navBarItem4;
        private DevExpress.XtraNavBar.NavBarGroup navBarGroup5;
        private DevExpress.XtraNavBar.NavBarItem navBarItemViewConfiguration;
        private DevExpress.XtraNavBar.NavBarItem navBarItemDisplayViews;
        private DevExpress.XtraNavBar.NavBarItem nbiMatchFilemakerMigrated;
        private DevExpress.XtraNavBar.NavBarItem navBarItem5;
        private DevExpress.XtraBars.BarStaticItem barStaticApplication;
        private DevExpress.XtraBars.BarStaticItem barStaticBuildVersion;
        private DevExpress.XtraNavBar.NavBarGroup navBarGroup6;
        private DevExpress.XtraNavBar.NavBarItem navBarItem1;
        private DevExpress.XtraNavBar.NavBarItem navBarItem8;
        private DevExpress.XtraNavBar.NavBarItem navBarItem9;
        private DevExpress.XtraNavBar.NavBarItem nbiAutoTitleCorrection;
        private DevExpress.XtraNavBar.NavBarItem navBarItem10;
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraBars.BarButtonItem barButtonOption;
        private DevExpress.XtraBars.PopupMenu popupMenu1;
        private DevExpress.XtraBars.BarStaticItem barStaticEnvironment;
        private DevExpress.XtraBars.BarButtonItem barButtonLogout;
        private DevExpress.XtraBars.BarButtonItem barButtonExit;
        private DevExpress.XtraNavBar.NavBarItem navBarItem6;
        private DevExpress.XtraNavBar.NavBarItem navBarItem7;
        private DevExpress.XtraNavBar.NavBarItem navPhoneSettings;
        private DevExpress.XtraNavBar.NavBarItem navBarItem11;
        private DevExpress.XtraNavBar.NavBarItem navBarItem12;
    }
}