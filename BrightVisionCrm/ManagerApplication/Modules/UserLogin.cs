using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ManagerApplication.Business;
using BrightVision.Common.Business;
using BrightVision.Model;
using DevExpress.XtraEditors.Controls;
using BrightVision.Common.Events.Core;
using ManagerApplication.Facade;
using ManagerApplication.Events;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules
{
    public partial class UserLogin : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Members
        public ManagerApplication.Forms.FrmManagerApplication m_objParentForm = null;
        #endregion

        #region Private Members
        private IEventAggregator m_EventBus = BrightManagerFacade.EventBus;
        private PopupDialog m_objContainerForm = null;
        #endregion

        #region Constructors
        public UserLogin(PopupDialog objPopupDialog)
        {
            InitializeComponent();
            m_objContainerForm = objPopupDialog;
        }
        #endregion

        #region Object Control Events
        private void cmdExit_Click(object sender, EventArgs e)
        {
            this.TerminateApplication();
        }
        private void cmdLogin_Click(object sender, EventArgs e)
        {
            this.ValidateUserLogin(false);
        }
        private void UserLogin_Load(object sender, EventArgs e)
        {
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            lblBuildVersion.Text = String.Format("<i>v.{0}</i>", curVersion.ToString());
            //tbxEmail.Text = string.Empty;
            txtPassword.Text = string.Empty;
            //this.LoadInternalUsers();

            /**
             * https://brightvision.jira.com/browse/PLATFORM-2392
             * read from text file the last user and load to textbox.
             */
            string _LastUser = FileManagerUtility.ReadUserLoginConf();
            if (!string.IsNullOrEmpty(_LastUser)) {
                tbxEmail.Text = _LastUser;
                txtPassword.Select();
            }

            //string _FilePath = Application.StartupPath + @"\lastuser.conf";
            //using (System.IO.StreamReader _srFile = new System.IO.StreamReader(_FilePath)) {
            //    string _LastUser = _srFile.ReadLine();
            //    if (!string.IsNullOrEmpty(_LastUser))
            //        tbxEmail.Text = _LastUser;
            //}
        }
        private void UserLogin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                cmdLogin.PerformClick();
        }
        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                cmdLogin.PerformClick();
        }
        private void cboUsername_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Return) {
                e.Handled = true;
                cmdLogin.PerformClick();
            }
        }
        private void cbeServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            int iServer = cbeServer.SelectedIndex;

            if (iServer == 0)
                UserSession.CurrentUser.ServerName = BrightVisionServers.Gothenburg;
            else if (iServer == 1)
                UserSession.CurrentUser.ServerName = BrightVisionServers.Hamachi;
            else if (iServer == 1)
                UserSession.CurrentUser.ServerName = BrightVisionServers.DemoEnv;
        }
        private void tbxEmail_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                cmdLogin.PerformClick();
        }
        private void cbeServer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                cmdLogin.PerformClick();
        }
        private void cmdLogin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                cmdLogin.PerformClick();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (tbxEmail.Text.Trim() == "") {
                MessageBox.Show("Please fill in the Email textbox first. ", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            BrightVision.Mandrill.ForgotPassword fp = new BrightVision.Mandrill.ForgotPassword(tbxEmail.Text);
            fp.ShowDialog();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Load usernames
        /// </summary>
        //private void LoadInternalUsers()
        //{
        //    try
        //    {
        //        cboUsername.Properties.DataSource = null;
        //        cboUsername.Properties.DisplayMember = "name";
        //        cboUsername.Properties.ValueMember = "id";
        //        cboUsername.Properties.Columns.Add(new LookUpColumnInfo("name"));
        //        cboUsername.Properties.DataSource = ObjectUser.GetManagerApplicationUsers().Execute(System.Data.Objects.MergeOption.AppendOnly);
        //    }
        //    catch (Exception ex){
        //        if (MessageBox.Show(ex.Message, "Network Connection Error",
        //                        System.Windows.Forms.MessageBoxButtons.RetryCancel, System.Windows.Forms.MessageBoxIcon.Error)
        //                        == System.Windows.Forms.DialogResult.Retry) {
        //                            LoadInternalUsers();
        //        } else {
        //            Application.Exit();
        //        }
        //    }
        //}

        /// <summary>
        /// Validate user login
        /// </summary>
        private void ValidateUserLogin(bool ShouldByPass)
        {
            //if (cboUsername.Text.Length < 1) {
            //    MessageBox.Show("Please select a user.", "User Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            if (ShouldByPass)
            {
                /** /
                UserSession objUser = UserSession.CurrentUser;
                objUser.UserId = (int)cboUsername.EditValue;
                objUser.UserFullName = cboUsername.Text;
                int iServer = cbeServer.SelectedIndex;
                if (iServer == 0)
                    objUser.ServerName = BrightVisionServers.Gothenburg;
                else if (iServer == 1)
                    objUser.ServerName = BrightVisionServers.Hamachi;

                bool valid = UserSession.IsEntityConnectionValid(objUser.ServerName);
                if (!valid) return;
                

                objUser.IsManagerAdmin = ObjectUser.IsManagerAdmin((int)cboUsername.EditValue);
                objUser.IsManagerUser = ObjectUser.IsManagerUser((int)cboUsername.EditValue);
                objUser.IsSalesUser = ObjectUser.IsSalesUser((int)cboUsername.EditValue);
                m_objParentForm.Text = "BrightManager     User: " + cboUsername.Text;
                m_objParentForm.DoneLoggedIn = true;
                m_objParentForm.SetFormControls(true);
                m_objContainerForm.IsLogin = false;
                m_objParentForm.WindowState = FormWindowState.Maximized;
                this.ParentForm.Close();
                /**/
            }
            else
            {
                if (tbxEmail.Text.Length < 1 || txtPassword.Text.Length < 1) {
                    MessageBox.Show("Invalid email / password.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                user _efoUser = null;
                if (ObjectUser.ValidateUserLogin(tbxEmail.Text, txtPassword.Text, ref _efoUser)) {
                    UserSession objUser = UserSession.CurrentUser;
                    objUser.UserId = _efoUser.id;
                    objUser.UserFullName = _efoUser.fullname;
                    int iServer = cbeServer.SelectedIndex;
                    bool _IsDemoEnv = false;

                    if (iServer == 0)
                        objUser.ServerName = BrightVisionServers.Gothenburg;
                    else if (iServer == 1)
                        objUser.ServerName = BrightVisionServers.Hamachi;
                    else if (iServer == 2) {
                        objUser.ServerName = BrightVisionServers.DemoEnv;
                        _IsDemoEnv = true;
                    }

                    bool valid = UserSession.IsEntityConnectionValid(objUser.ServerName);
                    if (!valid) return;
                    objUser.IsManagerAdmin = ObjectUser.IsManagerAdmin(_efoUser.id);
                    objUser.IsManagerUser = ObjectUser.IsManagerUser(_efoUser.id);
                    objUser.IsSalesUser = ObjectUser.IsSalesUser(_efoUser.id);
                    //m_objParentForm.Text = "BrightManager     User: " + _efoUser.fullname;
                    m_objParentForm.Text = FormUtility.GetConfigSetting("BuildEnvironment") + " - " + _efoUser.fullname;
                    m_objParentForm.WindowState = FormWindowState.Maximized;
                    m_objParentForm.DoneLoggedIn = true;
                    m_objParentForm.SetFormControls(true);
                    m_objContainerForm.IsLogin = false;

                    /**
                     * https://brightvision.jira.com/browse/PLATFORM-2392
                     * write to text file the logged in user.
                     */
                    FileManagerUtility.SaveUserLoginConf(tbxEmail.Text);

                    //string _FilePath = Application.StartupPath + @"\lastuser.conf";
                    //using (System.IO.StreamWriter _srFile = new System.IO.StreamWriter(_FilePath)) {
                    //    _srFile.Write(tbxEmail.Text);
                    //}

                    m_EventBus.Notify(new LoginSuccessEventNotifier() { IsDemoEnvironment = _IsDemoEnv });
                    this.ParentForm.Close();
                }
                else
                    MessageBox.Show("Invalid email / password", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);

                #region Commented
                /** /
                if (ObjectUser.ValidateUserLogin((int)cboUsername.EditValue, txtPassword.Text)) {
                    UserSession objUser = UserSession.CurrentUser;
                    objUser.UserId = (int)cboUsername.EditValue;
                    objUser.UserFullName = cboUsername.Text;
                    int iServer = cbeServer.SelectedIndex;
                    if (iServer == 0)
                        objUser.ServerName = BrightVisionServers.Gothenburg;
                    else if (iServer == 1)
                        objUser.ServerName = BrightVisionServers.Hamachi;
                    bool valid = UserSession.IsEntityConnectionValid(objUser.ServerName);
                    if (!valid) return;
                    objUser.IsManagerAdmin = ObjectUser.IsManagerAdmin((int)cboUsername.EditValue);
                    objUser.IsManagerUser = ObjectUser.IsManagerUser((int)cboUsername.EditValue);
                    objUser.IsSalesUser = ObjectUser.IsSalesUser((int)cboUsername.EditValue);
                    m_objParentForm.Text = "BrightManager     User: " + cboUsername.Text;
                    m_objParentForm.WindowState = FormWindowState.Maximized;
                    m_objParentForm.DoneLoggedIn = true;
                    m_objParentForm.SetFormControls(true);
                    m_objContainerForm.IsLogin = false;
                    this.ParentForm.Close();
                }
                else {
                    MessageBox.Show("Invalid username / password", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                /**/
                #endregion
            }
        }

        /// <summary>
        /// Terminate the application
        /// </summary>
        private void TerminateApplication()
        {
            DialogResult objResult = new DialogResult();
            objResult = MessageBox.Show("Are you sure to terminate the manager application?", "Manager Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.Yes)
            {
                m_objContainerForm.IsLogin = false;
                Application.Exit();
            }
        }
        #endregion  
    }
}
