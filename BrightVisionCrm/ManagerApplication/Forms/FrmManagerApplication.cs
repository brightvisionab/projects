using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ManagerApplication.Modules;
using ManagerApplication.Business;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Threading;
using BrightVision.EventLog;
using BrightVision.Reporting.UI;
using BrightVision.Common.Events.Core;
using ManagerApplication.Facade;
using ManagerApplication.Events;


namespace ManagerApplication.Forms
{
    public partial class FrmManagerApplication : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        #region Public Members
        public bool DoneLoggedIn = false;
        #endregion

        #region Private Members
        private IEventAggregator m_EventBus = BrightManagerFacade.EventBus;
        private ManageInternalUser m_objFrmManageUser = null;
        private ManageCustomerCampaign m_objFrmCustomerCampaign = null;
        private ManageSubCampaign m_objFrmSubCampaign = null;
        private ManageCompanyGeoData m_objFrmGeoMatchCompany = null;
        private ManageContactGeoData m_objFrmGeoMatchContact = null;
        private ManageCompanyContact m_objFrmCompanyContact = null;
        private ManageImportList m_objFrmManageImportList = null;
        private ManageListCreation m_objFrmManageListCreation = null;
        private ManageQuestions ucManageQuestions1;
        private ManageDialogs ucManageDialog1;
        private UserLogin m_objFrmUserLogin = null;
        private PopupDialog m_objPopupDialog = null;
        private ManageCallList m_objFrmManageCallList = null;
        private ManageFinalCallList m_objFrmManageFinalCallList = null;
        private ViewConfiguration m_objViewConfiguration = null;
        private ViewDisplay m_objViewDisplay = null;
        private ManageFileMakerMigrated m_objFileMakerMigrated = null;
        private AutoTitleCorrection m_objAutoTitleCorrection = null;
        private ToolStripMenuItem m_tsmiLogOut = null;
        private ToolStripMenuItem m_tsmiExit = null;
        private ManageSIPAccount m_objFrmManageSIPAccount = null;

        private int m_ViewConfigId = 0;

        #region Background Thread for Event Logging

        private int[] stats;
        private TimeSpan refreshInterval;
        private DateTime nextRefreshTime;
        private WorkQueue work;
        private int minThreads, maxThreads, concurrentLimit;
        private bool pausing = false, resuming = false;
        private int newWork = 10;//sample of work load

        #endregion
        #endregion

        #region Contructors
        public FrmManagerApplication()
        {
            InitializeComponent();
            this.KeyPreview = true;
            ContextMenuStrip _cmsMenu = new ContextMenuStrip();
            m_tsmiLogOut = new ToolStripMenuItem("Log Out");
            m_tsmiLogOut.Click += new EventHandler(m_tsmiLogOut_Click);
            m_tsmiLogOut.Image = Properties.Resources.logout;
            m_tsmiExit = new ToolStripMenuItem("Exit");
            m_tsmiExit.Click += new EventHandler(m_tsmiExit_Click);
            m_tsmiExit.Image = Properties.Resources.close;
            _cmsMenu.Items.Add(m_tsmiLogOut);
            _cmsMenu.Items.Add(m_tsmiExit);
            this.ContextMenuStrip = _cmsMenu;
            this.ContextMenuStrip.BringToFront();

            #region Background Thread for Event Logging
            nextRefreshTime = DateTime.Now;
            refreshInterval = TimeSpan.FromSeconds(0.20);

            stats = new int[6];

            work = new WorkQueue();
            work.ConcurrentLimit = 100;
            work.AllWorkCompleted += new EventHandler(work_AllWorkCompleted);
            work.WorkerException += new ResourceExceptionEventHandler(work_WorkerException);
            work.ChangedWorkItemState += new ChangedWorkItemStateEventHandler(work_ChangedWorkItemState);
           
            minThreads = ((WorkThreadPool)work.WorkerPool).MinThreads;
            maxThreads = ((WorkThreadPool)work.WorkerPool).MaxThreads;
            concurrentLimit = work.ConcurrentLimit;
            #endregion

            m_EventBus.GetEvent<LoginSuccessEventNotifier>().Subscribe(LoginSuccess);
        }
        #endregion

        #region Form Control Events
        private void navBarItem11_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show("Loading components...");
            ViewCallLogs objForm = new ViewCallLogs();
            objForm.Visible = false;
            objForm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(objForm);
            //objForm.GetCallLogs();
            objForm.Visible = true;
            WaitDialog.Close();
        }

        private void navBarItem12_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            ManageMessageLog objForm = new ManageMessageLog();
            objForm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(objForm);
            WaitDialog.Close();
        }

        private void navBarItem6_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show("Loading components...");
            //ManageReportTemplate objForm = new ManageReportTemplate();
            EditorReportTemplate objForm = new EditorReportTemplate();
            objForm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(objForm);
            WaitDialog.Close();
        }

        private void navBarItem10_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show("Loading components...");
            ManageFollowUp objForm = new ManageFollowUp();
            objForm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(objForm);
            WaitDialog.Close();
        }

        private void navBarItem9_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            
            //BVReportTemplate _item = new BVReportTemplate();
            //_item.ShowDesigner();
            
            //WaitDialog.Show("Loading components...");
            //ManageEmailVerification objForm = new ManageEmailVerification();
            //objForm.Dock = DockStyle.Fill;
            //pnlContent.Controls.Clear();
            //pnlContent.Controls.Add((UserControl)_item);
            //WaitDialog.Close();
        }

        private void navBarItem8_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            ManageEventLog objForm = new ManageEventLog();
            objForm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(objForm);
           WaitDialog.Close();
        }

        void m_tsmiExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void m_tsmiLogOut_Click(object sender, EventArgs e)
        {
            if (!this.LogoutApplication())
                return;

            this.Hide();
            pnlContent.Controls.Clear();
            this.SetFormControls(false);
            this.LoadUserLoginForm();
        }

        private void barButtonLogout_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            if (!this.LogoutApplication())
                return;

            this.Hide();
            pnlContent.Controls.Clear();
            this.SetFormControls(false);
            this.LoadUserLoginForm();
        }

        private void barButtonExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            this.Close();
        }

        private void navBarItem1_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show("Loading components...");
            MasterDataImport objForm = new MasterDataImport();
            objForm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(objForm);
            WaitDialog.Close();
        }

        private void navBarItem4_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            ManageCustomerUser objForm = new ManageCustomerUser();
            objForm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(objForm);
           WaitDialog.Close();
        }

        private void navBarItem3_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            ManageTitleSetting objManageTitle = new ManageTitleSetting();
            objManageTitle.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(objManageTitle);
           WaitDialog.Close();
        }

        private void navBarItem2_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            ManageLanguageSetting objManageLanguage = new ManageLanguageSetting();
            objManageLanguage.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(objManageLanguage);
           WaitDialog.Close();
        }

        private void nbiFinalizedCallList_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            m_objFrmManageFinalCallList = null;
            m_objFrmManageFinalCallList = new ManageFinalCallList();
            m_objFrmManageFinalCallList.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objFrmManageFinalCallList);
           WaitDialog.Close();
        }

        private void nbiCallList_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            m_objFrmManageCallList = null;
            m_objFrmManageCallList = new ManageCallList();
            m_objFrmManageCallList.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objFrmManageCallList);
           WaitDialog.Close();
        }

        private void nbiImporAndViewtList_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            m_objFrmManageImportList = null;
            m_objFrmManageImportList = new ManageImportList();
            m_objFrmManageImportList.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objFrmManageImportList);
           WaitDialog.Close();
        }

        private void nbarItemUser_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            if (!UserSession.CurrentUser.IsManagerAdmin)
            {
                MessageBox.Show("You are not allowed to view this module." + Environment.NewLine + " Please contact your administrator.", "Users", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            WaitDialog.Show(this, "Loading components...");
            m_objFrmManageUser = null;
            m_objFrmManageUser = new ManageInternalUser();
            m_objFrmManageUser.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objFrmManageUser);
           WaitDialog.Close();
        }

        private void nbarItemCustomer_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            if (UserSession.CurrentUser.IsManagerAdmin || UserSession.CurrentUser.IsManagerUser)
            {
                WaitDialog.Show(this, "Loading components...");
                m_objFrmCustomerCampaign = null;
                m_objFrmCustomerCampaign = new ManageCustomerCampaign();
                m_objFrmCustomerCampaign.Dock = DockStyle.Fill;
                pnlContent.Controls.Clear();
                pnlContent.Controls.Add(m_objFrmCustomerCampaign);
               WaitDialog.Close();
            }
            else
                MessageBox.Show("You are not allowed to view this module." + Environment.NewLine + "Please contact your administrator", "Manager Application", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void nbiSubCampaign_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            if (UserSession.CurrentUser.IsManagerAdmin || UserSession.CurrentUser.IsManagerUser)
            {
                WaitDialog.Show(this, "Loading components...");
                m_objFrmSubCampaign = null;
                m_objFrmSubCampaign = new ManageSubCampaign();
                m_objFrmSubCampaign.Dock = DockStyle.Fill;
                pnlContent.Controls.Clear();
                pnlContent.Controls.Add(m_objFrmSubCampaign);
               WaitDialog.Close();
            }
            else
                MessageBox.Show("You are not allowed to view this module." + Environment.NewLine + "Please contact your administrator", "Manager Application", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void nbiGeoMapCompany_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            m_objFrmGeoMatchCompany = null;
            m_objFrmGeoMatchCompany = new ManageCompanyGeoData();
            m_objFrmGeoMatchCompany.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();            
            pnlContent.Controls.Add(m_objFrmGeoMatchCompany);
           WaitDialog.Close();
        }

        private void nbiCompanyContact_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            m_objFrmCompanyContact = null;
            m_objFrmCompanyContact = new ManageCompanyContact();
            m_objFrmCompanyContact.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objFrmCompanyContact);            
           WaitDialog.Close();
        }

        private void navBarItemManageDialogs_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e) 
        {
            WaitDialog.Show(this, "Loading components...");
            ucManageDialog1 = new Modules.ManageDialogs();
            ucManageDialog1.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(ucManageDialog1);
           WaitDialog.Close();
        }

        private void navBarItemManageQuestions_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e) 
        {
            WaitDialog.Show(this, "Loading components...");
            ucManageQuestions1 = new Modules.ManageQuestions();
            ucManageQuestions1.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(ucManageQuestions1);
           WaitDialog.Close();
        }

        private void nbiGeoMapContact_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e) 
        {
            WaitDialog.Show(this, "Loading components...");
            m_objFrmGeoMatchContact = null;
            m_objFrmGeoMatchContact = new ManageContactGeoData();
            m_objFrmGeoMatchContact.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objFrmGeoMatchContact);
           WaitDialog.Close();
        }

        private void FrmManagerApplication_Load(object sender, EventArgs e)
        {
            this.AddWorkToQueue(new EventMessage { IsLoadTitle = true });
            System.Threading.Thread.Sleep(4000);
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            barStaticApplication.Caption = "BrightManager";
            barStaticBuildVersion.Caption = "Version: " + curVersion.ToString();
            barStaticEnvironment.Caption = FormUtility.GetConfigSetting("BuildEnvironment");
            this.Visible = false;
            this.SetFormControls(false);
            this.LoadUserLoginForm();
        }

        private void FrmManagerApplication_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DoneLoggedIn)
            {
                if (this.TerminateApplication())
                {
                    DoneLoggedIn = false;
                    Application.Exit();
                }
                else
                    e.Cancel = true;
            }
        }

        private void nbiListCreation_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components ...");
            m_objFrmManageListCreation = null;
            m_objFrmManageListCreation = new ManageListCreation();
            m_objFrmManageListCreation.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objFrmManageListCreation);
           WaitDialog.Close();
        }

        private void navBarItemViewConfiguration_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e) 
        {
            WaitDialog.Show(this, "Loading components ...");
            if (m_objViewConfiguration == null) {
                m_objViewConfiguration = new ViewConfiguration() {
                    Dock = DockStyle.Fill
                };
                // event handler trigger when pressing preview button on report conficuration
                m_objViewConfiguration.btnPreviewViewOnClick += new ViewConfiguration.btnPreviewViewOnClickEventHandler(m_objViewConfiguration_btnPreviewViewOnClick);
            }
            else
                m_objViewConfiguration.ReloadSubCampaignList();

            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objViewConfiguration);
            WaitDialog.Close();
        }

        private void m_objViewConfiguration_btnPreviewViewOnClick(int pViewConfigId)
        {
            navBarGroup5.SelectedLinkIndex = 1;
            m_ViewConfigId = pViewConfigId;
            this.navBarItemDisplayViews_LinkClicked(null, null);
        }

        private void navBarItemDisplayViews_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e) 
        {
            if (!UserSession.CurrentUser.IsManagerAdmin && !UserSession.CurrentUser.IsManagerUser) {
                MessageBox.Show("You do not have access to view this module. Please contact administrator", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            WaitDialog.Show(this, "Loading components ...");
            if (m_ViewConfigId > 0) {
                m_objViewDisplay = new ViewDisplay(m_ViewConfigId) {
                    Dock = DockStyle.Fill
                };
                //m_objViewDisplay.SetCampaignListEventChangeTrigger(false);
                //m_objViewDisplay.ReloadCampaignList();
                //m_objViewDisplay.SetCampaignListEventChangeTrigger(true);
                m_objViewDisplay.AutoLoadReport();
            }                 
            else if (m_objViewDisplay == null) {
                m_objViewDisplay = new ViewDisplay() {
                    Dock = DockStyle.Fill
                };                
            }
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objViewDisplay);
            WaitDialog.Close();
        }
     
        private void nbiMatchFilemakerMigrated_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e) 
        {
            if (!UserSession.CurrentUser.IsManagerAdmin && !UserSession.CurrentUser.IsManagerUser) {
                MessageBox.Show("You do not have access to view this module. Please contact administrator", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            WaitDialog.Show(this, "Loading components...");
            m_objFileMakerMigrated = new ManageFileMakerMigrated();
            m_objFileMakerMigrated.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objFileMakerMigrated);
           WaitDialog.Close();
        }

        private void nbiAutoTitleCorrection_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e) {
            WaitDialog.Show(this, "Loading components...");
            m_objAutoTitleCorrection = new AutoTitleCorrection();
            m_objAutoTitleCorrection.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objAutoTitleCorrection);
           WaitDialog.Close();
        }

        private void navPhoneSettings_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            WaitDialog.Show(this, "Loading components...");
            m_objFrmManageSIPAccount = new ManageSIPAccount();
            m_objFrmManageSIPAccount.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(m_objFrmManageSIPAccount);
            WaitDialog.Close();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set visible true/false the form controls
        /// </summary>
        public void SetFormControls(bool IsVisible)
        {
            navBarMenu.Visible = IsVisible;
            pnlContent.Visible = IsVisible;
            if (IsVisible) {
                pnlContent.Controls.Clear();
                m_objViewConfiguration = null;
                m_objViewDisplay = null;
                this.Show();
            }
            else
                this.Hide();
        }
        #endregion

        #region Private Methods
        private void LoginSuccess(LoginSuccessEventNotifier e)
        {
            if (e.IsDemoEnvironment)
                this.Text = string.Format("{0}  [ USING DEMO ENVIRONMENT ]", this.Text);
        }
        private bool LogoutApplication()
        {
            DialogResult objResult = new DialogResult();
            objResult = MessageBox.Show("Are you sure you want to logout?", "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.Yes)
                return true;
            else
                return false;
        }
        private bool TerminateApplication()
        {
            DialogResult objResult = new DialogResult();
            objResult = MessageBox.Show("Are you sure to terminate the manager application?", "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.Yes)
                return true;
            else
                return false;
        }
        private void LoadUserLoginForm()
        {
            m_objPopupDialog = new PopupDialog();
            
            m_objFrmUserLogin = new UserLogin(m_objPopupDialog);
            m_objFrmUserLogin.m_objParentForm = this;
            m_objPopupDialog.IsLogin = true;
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;            
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = "Login to Manager Application";
            m_objPopupDialog.Controls.Add(m_objFrmUserLogin);
            m_objPopupDialog.ClientSize = new Size(m_objFrmUserLogin.Width, m_objFrmUserLogin.Height);
            m_objPopupDialog.FormClosed += new FormClosedEventHandler(m_objPopupDialog_FormClosed);
            m_objPopupDialog.ShowDialog(this.ParentForm);
        }
        #endregion

        #region Custom Events
        private void m_objPopupDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Visible = true;
        }
        #endregion    
         
        #region Keyboard Shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (FormUtility.ShortCutKeysHandled(keyData))
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region BackgroundThread for Event Logging
        /// <summary>
        /// Add 
        /// </summary>
        public void AddWorkToQueue(EventMessage eventMessage) {
            lock (work) {
                work.Add(eventMessage);
            }
        }

        /// <summary>
        ///   All work is completed.  Update the GUI.
        /// </summary>
        private void work_AllWorkCompleted(object sender, EventArgs e) {
            if (this.InvokeRequired) {
                this.Invoke(new EventHandler(work_AllWorkCompleted), new object[] { sender, e });
            } else {
                stats = new int[6];
            }
        }

        /// <summary>
        ///   Record where the state of the work item.
        /// </summary>
        private void work_ChangedWorkItemState(object sender, ChangedWorkItemStateEventArgs e) {
            lock (this) {
                stats[(int)e.PreviousState] -= 1;
                stats[(int)e.WorkItem.State] += 1;
            }

            if (!pausing || DateTime.Now > nextRefreshTime) {
                nextRefreshTime = DateTime.Now + refreshInterval;
            }
        }
        private void work_WorkerException(object sender, ResourceExceptionEventArgs e) {
            Application.OnThreadException(e.Exception);
        }

        /// <summary>
        ///   Clear the work item state counts.
        /// </summary>
        private void reset_Click(object sender, System.EventArgs e) {
            lock (stats) {
                for (int i = 0; i < stats.Length; ++i)
                    stats[i] = 0;
            }
        }

        /// <summary>
        ///   Pause the work queue.
        /// </summary>
        private void Pause() {
            work.Pause();
            pausing = true;
            resuming = false;
        }

        /// <summary>
        ///   Resume the work queue.
        /// </summary>
        private void Resume() {
            work.Resume();
            pausing = false;
            resuming = true;
        }
        #endregion        

    }
}
