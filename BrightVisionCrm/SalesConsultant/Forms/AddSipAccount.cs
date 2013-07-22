
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Model;
using BrightVision.Common.Business;
using System.Linq;

namespace SalesConsultant.Modules
{

    public partial class AddSipAccount : XtraUserControl
    {
        #region Constructors
        public AddSipAccount()
        {
            InitializeComponent();

            //Need to add IsModified to make DoValidate method work
            textEditSIPUrl.IsModified = true;
            textEditDisplayName.IsModified = true;
            textEditUsername.IsModified = true;
            textEditPassword.IsModified = true;
            textEditOperator.IsModified = true;
        }
        public AddSipAccount(int pSipId) : this()
        {
            m_SipId = pSipId;
        }
        #endregion

        #region Public Properties
        public int SipId { get; set; }
        #endregion

        #region Private Properties
        private int m_SipId = 0;
        #endregion

        #region Control Events
        private void simpleButtonSave_Click(object sender, EventArgs e)
        {
            if (!(textEditSIPUrl.DoValidate()
                && textEditDisplayName.DoValidate()
                && textEditUsername.DoValidate()
                && textEditPassword.DoValidate()
                && textEditOperator.DoValidate()
                ))
                return;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {                
                WaitDialog.Show(this.ParentForm, "Saving data ...");
                sip_accounts _eftSipAccount = null;
                if (m_SipId < 1) {
                    _eftSipAccount = new sip_accounts() {
                        sip_url = textEditSIPUrl.EditValue.ToString(),
                        display_name = textEditDisplayName.EditValue.ToString(),
                        username = textEditUsername.EditValue.ToString(),
                        password = textEditPassword.EditValue.ToString(),
                        @operator = textEditOperator.EditValue.ToString(),
                        comment = memoEditComment.EditValue == null ? "" : memoEditComment.EditValue.ToString()
                    };
                    _efDbContext.sip_accounts.AddObject(_eftSipAccount);
                }
                else {
                    _eftSipAccount = _efDbContext.sip_accounts.Where(pr => pr.id == m_SipId).FirstOrDefault();
                    _eftSipAccount.sip_url = textEditSIPUrl.EditValue.ToString();
                    _eftSipAccount.display_name = textEditDisplayName.EditValue.ToString();
                    _eftSipAccount.username = textEditUsername.EditValue.ToString();
                    _eftSipAccount.password = textEditPassword.EditValue.ToString();
                    _eftSipAccount.@operator = textEditOperator.EditValue.ToString();
                    _eftSipAccount.comment = memoEditComment.EditValue == null ? "" : memoEditComment.EditValue.ToString();
                }
                _efDbContext.SaveChanges();
                _efDbContext.Detach(_eftSipAccount);
                SipId = _eftSipAccount.id;

                WaitDialog.Close();
                this.ParentForm.Close();
                
                //var sipAccount = _efDbContext.sip_accounts.FirstOrDefault(pr => pr.display_name == _eftSipAccount.display_name);
                //SipId = sipAccount.id;
            }
        }
        private void AddSipAccount_Load(object sender, EventArgs e)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                if (m_SipId != 0) {
                    sip_accounts _eftSipAccount = _efDbContext.sip_accounts.FirstOrDefault(i => i.id == m_SipId);
                    _efDbContext.Detach(_eftSipAccount);

                    textEditSIPUrl.EditValue = _eftSipAccount.sip_url;
                    textEditDisplayName.EditValue = _eftSipAccount.display_name;
                    textEditUsername.EditValue = _eftSipAccount.username;
                    textEditPassword.EditValue = _eftSipAccount.password;
                    textEditOperator.EditValue = _eftSipAccount.@operator;
                    memoEditComment.EditValue = _eftSipAccount.comment;
                }
            }
        }
        private void textEditSipUrl_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textEditSIPUrl.Text)) 
                e.Cancel = true;
        }
        private void textEditDisplayName_Validating(object sender, CancelEventArgs e)
        {
            string _DisplayName = textEditDisplayName.Text;
            if (string.IsNullOrWhiteSpace(textEditDisplayName.Text)) {
                e.Cancel = true;
                return;
            }

            int _Count = 0;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _Count = _efDbContext.sip_accounts.Where(i => i.display_name == _DisplayName && i.id != m_SipId).Count();
            }

            if (_Count > 0) {
                e.Cancel = true;
                BrightVision.Common.UI.NotificationDialog.Warning("Bright Sales", "The display name is already used.");
            }
        }
        private void textEditUsername_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textEditUsername.Text))
                e.Cancel = true;
        }
        private void textEditPassword_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textEditPassword.Text))
                e.Cancel = true;
        }
        private void textEditOperator_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textEditOperator.Text))
                e.Cancel = true;
        }
        #endregion
    }
}
