
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;

using DevExpress.XtraEditors;

using BrightVision.Common.Business;
using BrightVision.Model;

namespace SalesConsultant.Forms
{
    public partial class ChangePassword : DevExpress.XtraEditors.XtraForm
    {
        #region Constructors
        public ChangePassword(bool pForceUserToUpdatePassword)
        {
            InitializeComponent();
            this.InitiateChangePasswordScreen();

            if (pForceUserToUpdatePassword)
                btnCancel.Enabled = false;
        }
        public ChangePassword()
        {
            InitializeComponent();
            this.InitiateChangePasswordScreen();
        }
        #endregion

        #region Public Events & Args
        public delegate void InvalidOldPasswordEventHandler();
        public event InvalidOldPasswordEventHandler InvalidOldPassword;

        public delegate void PasswordsDoesNotMatchEventHandler();
        public event PasswordsDoesNotMatchEventHandler PasswordsDoesNotMatch;

        public delegate void PasswordsMustBeDifferentEventHandler();
        public event PasswordsMustBeDifferentEventHandler PasswordsMustBeDifferent;

        public delegate void AfterSaveEventHandler();
        public event AfterSaveEventHandler AfterSave;
        #endregion

        #region Private Properties
        private int m_UserId { get; set; }
        #endregion

        #region Private Methods
        private void InitiateChangePasswordScreen()
        {
            user _efoUser = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                m_UserId = UserSession.CurrentUser.UserId;
                _efoUser = _efDbContext.users.FirstOrDefault(i => i.id == UserSession.CurrentUser.UserId);
                _efDbContext.Detach(_efoUser);
            }
            tbxUser.Text = _efoUser.fullname;
            tbxOldPassword.Focus();
        }
        private bool IsCorrectOldPassword()
        {
            if (tbxOldPassword.Text.Length < 1)
                return false;

            user _efoUser = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efoUser = _efDbContext.users.FirstOrDefault(i => i.id == m_UserId);
                _efDbContext.Detach(_efoUser);
            }

            if (_efoUser.password.ToLower() != BrightVision.Common.Utilities.HashUtility.GetHashPassword(tbxOldPassword.Text).ToLower())
                return false;

            return true;
        }
        private bool IsCorrectNewPassword()
        {
            if (tbxNewPassword.Text.Length < 6 || tbxConfirmNewPassword.Text.Length < 6)
                return false;

            if (tbxNewPassword.Text != tbxConfirmNewPassword.Text)
                return false;

            return true;
        }
        private void LogEvent()
        {
            string Source = "BrightSales";
            if (Application.ProductName == "ManagerApplication") Source = "BrightManager";

            BrightPlatformEntities m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);

            m_efDbContext.event_log.AddObject(
               new event_log()
               {
                   event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.RESET_PASSWORD,
                   user_id = UserSession.CurrentUser.UserId,
                   subcampaign_id = null,
                   account_id = null,
                   contact_id = null,
                   local_datetime = DateTime.Now,
                   computer_name = UserSession.CurrentUser.ComputerName,
                   param1 = tbxOldPassword.Text,
                   param2 = UserSession.CurrentUser.ComputerIP,
                   param3 = UserSession.CurrentUser.UserId.ToString(),
                   param4 = Source,
                   param5 = tbxNewPassword.Text,
                   param6 = null
               }
           );

            m_efDbContext.SaveChanges();
        }
        #endregion

        #region Control Events
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!this.IsCorrectOldPassword()) {
                this.InvalidOldPassword();
                return;
            }

            if (!this.IsCorrectNewPassword()) {
                this.PasswordsDoesNotMatch();
                return;
            }

            if (tbxOldPassword.Text == tbxNewPassword.Text) {
                this.PasswordsMustBeDifferent();
                return;
            }

            WaitDialog.Show("Saving password.");
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                user _eftUser = _efDbContext.users.FirstOrDefault(i => i.id == m_UserId);
                _eftUser.password = BrightVision.Common.Utilities.HashUtility.GetHashPassword(tbxNewPassword.Text);
                _eftUser.modified_by = UserSession.CurrentUser.UserId;
                _eftUser.modified_date = DateTime.Now;
                
                _efDbContext.change_password_logs.AddObject(new change_password_logs() {
                    user_id = m_UserId,
                    new_password = tbxNewPassword.Text,
                    old_password = tbxOldPassword.Text,
                    updated_by = UserSession.CurrentUser.UserId,
                    updated_on = DateTime.Now,
                    machine_name = UserSession.CurrentUser.ComputerName,
                    machine_ip = UserSession.CurrentUser.ComputerIP
                });

                _efDbContext.SaveChanges();
                _efDbContext.Detach(_eftUser);
            }
            LogEvent();
            WaitDialog.Close();

            this.AfterSave();
            this.Close();
        }
        private void tbxOldPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnSave.PerformClick();
        }
        private void tbxNewPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnSave.PerformClick();
        }
        private void tbxConfirmNewPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnSave.PerformClick();
        }
        #endregion
    }
}