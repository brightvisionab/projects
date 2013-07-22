
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using SalesConsultant.Forms;
using DevExpress.XtraEditors.Controls;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using SalesConsultant.Events;
using BrightVision.Common.Events.Core;
using BrightVision.Common.Utilities;

namespace SalesConsultant.Modules
{
    public partial class UserLogin : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Event Handlers
        public delegate void AfterLoginEventHandler();
        public event AfterLoginEventHandler AfterLogin;
        #endregion

        #region Public Members
        #endregion

        #region Private Members
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        private PopupDialog m_objContainerForm = null;
        #endregion

        #region Constructors
        public UserLogin(PopupDialog objPopupDialog)
        {
            InitializeComponent();
            m_objContainerForm = objPopupDialog;

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
        #endregion

        #region Object Control Events
        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult objResult = new DialogResult();
            objResult = MessageBox.Show("Are you sure to terminate the sales consultant application?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.Yes) {
                m_objContainerForm.DialogResult = DialogResult.Cancel;
                m_objContainerForm.IsLogin = false;
                Application.Exit();
            }
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            this.ValidateUserLogin(false);
        }
        private void UserLogin_Load(object sender, EventArgs e)
        {            
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            lblBuildVersion.Text = String.Format("<i>v.{0}</i>", curVersion.ToString());
            //cboUsername.Properties.DataSource = null;
            //cboUsername.Properties.Columns.Clear();
            //cboUsername.Properties.DataSource = ObjectUser.GetSalesConsultantApplicationUsers().Execute(System.Data.Objects.MergeOption.AppendOnly);
            //cboUsername.Properties.DisplayMember = "name";
            //cboUsername.Properties.ValueMember = "id";
            //cboUsername.Properties.Columns.Add(new LookUpColumnInfo("name"));
        }
        private void UserLogin_KeyUp(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Enter)
            //    this.ValidateUserLogin(false);
        }
        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin.PerformClick();
        }
        private void cboUsername_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Return) {
                e.Handled = true;
                btnLogin.PerformClick();
            }
        }
        private void tbxEmail_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin.PerformClick();
        }
        private void cbeServer_SelectedValueChanged(object sender, EventArgs e)
        {
            int iServer = cbeServer.SelectedIndex;

            if (iServer == 0)
                UserSession.CurrentUser.ServerName = BrightVisionServers.Gothenburg;
            else if (iServer == 1)
                UserSession.CurrentUser.ServerName = BrightVisionServers.Hamachi;
            else if (iServer == 1)
                UserSession.CurrentUser.ServerName = BrightVisionServers.DemoEnv;
        }
        private void cbeServer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin.PerformClick();
        }
        private void btnLogin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin.PerformClick();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (tbxEmail.Text.Trim() == "")
            {
                MessageBox.Show("Please fill in the Email textbox first. ", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            BrightVision.Mandrill.ForgotPassword fp = new BrightVision.Mandrill.ForgotPassword(tbxEmail.Text);
            fp.ShowDialog();
        }
        #endregion

        #region Private Methods
        private void ValidateUserLogin(bool ShouldByPass)
        {
            //if (cboUsername.Text.Length < 1)
            //{
            //    MessageBox.Show("Please select a user.", "User Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            if (ShouldByPass) {
                /** /
                m_objContainerForm.IsLogin = false;                
                UserSession objUser = UserSession.CurrentUser;
                //if (cboUsername.EditValue == null) {
                //    MessageBox.Show("Please supply your username / password.", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
                this.ParentForm.Hide();
                objUser.UserId = (int)cboUsername.EditValue;
                objUser.UserFullName = cboUsername.Text;
                int iServer = cbeServer.SelectedIndex;
                if (iServer == 0)
                    objUser.ServerName = BrightVisionServers.Gothenburg;
                else if (iServer == 1)
                    objUser.ServerName = BrightVisionServers.Hamachi;
                bool valid = UserSession.IsEntityConnectionValid(objUser.ServerName);
                if (!valid) return;
                //objUser.IsCampaignOwner = ObjectUser.IsCampaignOwner((int)cboUsername.EditValue);
                //objUser.IsSubCampaignManager = ObjectUser.IsSubCampaignManager((int)cboUsername.EditValue);
                //objUser.IsSubCampaignSales = ObjectUser.IsSubCampaignSales((int)cboUsername.EditValue);
                if (AfterLogin != null)
                    AfterLogin();

                //m_objParentForm.Text = "BrightSales     User: " + cboUsername.Text;
                //m_objParentForm.WindowState = FormWindowState.Maximized;
                //m_objParentForm.DoneLoggedIn = true;
                //m_objParentForm.SetFormControls(true);
                this.ParentForm.Close();
                /**/
            }
            else {
                if (tbxEmail.Text.Length < 1 || txtPassword.Text.Length < 1) {
                    NotificationDialog.Error("Bright Sales", "Invalid email / password.");
                    return;
                }

                BrightVision.Model.user _efoUser = null;
                if (ObjectUser.ValidateUserLogin(tbxEmail.Text, txtPassword.Text,ref _efoUser)) {                    
                    m_objContainerForm.IsLogin = false;
                    UserSession objUser = UserSession.CurrentUser;
                    objUser.IsManagerAdmin = ObjectUser.IsManagerAdmin(_efoUser.id);
                    objUser.IsManagerUser = ObjectUser.IsManagerUser(_efoUser.id);
                    objUser.IsSalesUser = ObjectUser.IsSalesUser(_efoUser.id);
                    objUser.UserId = _efoUser.id;
                    objUser.UserFullName = _efoUser.fullname;
                    objUser.UserEmail = _efoUser.email;
                    int iServer = cbeServer.SelectedIndex;
                    
                    if (iServer == 0)
                        objUser.ServerName = BrightVisionServers.Gothenburg;
                    else if (iServer == 1)
                        objUser.ServerName = BrightVisionServers.Hamachi;
                    else if (iServer == 2)
                        objUser.ServerName = BrightVisionServers.DemoEnv;

                    bool valid = UserSession.IsEntityConnectionValid(objUser.ServerName);
                    if (!valid) 
                        return;

                    /**
                     * https://brightvision.jira.com/browse/PLATFORM-2392
                     * write to text file the logged in user.
                     */
                    FileManagerUtility.SaveUserLoginConf(tbxEmail.Text);


                    /**
                     * https://brightvision.jira.com/browse/PLATFORM-2803
                     * Update mandrill outgoing email address to currently login user
                     */
                    //BrightVision.Mandrill.MandrillEx mandrillEx = new BrightVision.Mandrill.MandrillEx();
                    //mandrillEx.From = tbxEmail.Text;
                    //mandrillEx.FromName = UserSession.CurrentUser.UserFullName;
                    //mandrillEx = null;

                    //string _FilePath = Application.StartupPath + @"\lastuser.conf";
                    //using (System.IO.StreamWriter _srFile = new System.IO.StreamWriter(_FilePath)) {
                    //    _srFile.Write(tbxEmail.Text);
                    //}

                    this.ParentForm.Hide();
                    if (AfterLogin != null)
                        AfterLogin();

                    if (iServer == 2)
                        m_EventBus.Notify(new LoginEvents.OnSuccess() { 
                            WorkingEnvironment = SelectionProperty.WorkingEnvironment.Demo 
                        });

                    this.ParentForm.Close();
                }
                else
                    NotificationDialog.Error("Bright Sales", "Invalid username / password.");

                /** /
                //if (cboUsername.Text.Length < 1 || txtPassword.Text.Length < 1)
                //{
                //    MessageBox.Show("Please supply your username / password.", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}

                //if (ObjectUser.ValidateUserLogin((int)cboUsername.EditValue, txtPassword.Text))
                //{                    
                    m_objContainerForm.IsLogin = false;
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
                    //objUser.IsCampaignOwner = ObjectUser.IsCampaignOwner((int)cboUsername.EditValue);
                    //objUser.IsSubCampaignManager = ObjectUser.IsSubCampaignManager((int)cboUsername.EditValue);
                    //objUser.IsSubCampaignSales = ObjectUser.IsSubCampaignSales((int)cboUsername.EditValue);
                    this.ParentForm.Hide();
                    if (AfterLogin != null)
                        AfterLogin();

                    this.ParentForm.Close();
                //}
                //else
                //{
                //    MessageBox.Show("Invalid username / password.", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
                /**/
            }
        }      
        #endregion        
    }
}
