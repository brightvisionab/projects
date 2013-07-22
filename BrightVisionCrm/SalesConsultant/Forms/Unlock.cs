
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;

using BrightVision.Common.Business;
using BrightVision.Model;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace SalesConsultant.Forms
{
    public partial class Unlock : DevExpress.XtraEditors.XtraForm
    {
        #region Constructors
        public Unlock()
        {            
            InitializeComponent();
            this.InitiateLockScreen();
        }
        #endregion

        #region Public Events & Args
        public delegate void InvalidLoginEventHandler();
        public event InvalidLoginEventHandler InvalidLogin;
        #endregion

        #region Private Properties
        private int m_UserId { get; set; }
        #endregion

        #region Private Methods
        private void InitiateLockScreen()
        {
            user _efoUser = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                m_UserId = UserSession.CurrentUser.UserId;
                _efoUser = _efDbContext.users.FirstOrDefault(i => i.id == UserSession.CurrentUser.UserId);
                _efDbContext.Detach(_efoUser);
            }
            tbxUser.Text = _efoUser.fullname;
            tbxPassword.Focus();
        }
        #endregion

        #region Control Events
        private void btnUnlock_Click(object sender, EventArgs e)
        {
            if (tbxPassword.Text.Length < 1) {
                this.InvalidLogin();
                return;
            }

            user _efoUser = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                string _Password = BrightVision.Common.Utilities.HashUtility.GetHashPassword(tbxPassword.Text);
                _efoUser = _efDbContext.users.FirstOrDefault(i => i.id == m_UserId && i.password == _Password);
                if (_efoUser != null) {
                    _efDbContext.Detach(_efoUser);
                    this.Close();
                }
                else {
                    tbxPassword.Text = string.Empty;
                    this.InvalidLogin();
                }
            }
        }
        private void tbxPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnUnlock.PerformClick();
        }
        #endregion
    }
}