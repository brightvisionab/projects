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
namespace ManagerApplication.Modules
{

    public partial class AddSIPAccount : DevExpress.XtraEditors.XtraUserControl
    {
        #region Delegate
        public class SaveSIPArg { public int SIPId { get; set; } }
        public delegate void SavedSIPHandler(SaveSIPArg arg);
        #endregion

        #region Variable
        int sipId=0;
        public event SavedSIPHandler Saved;
        #endregion

        #region Properties
        public int SIPId { get; set; }
        #endregion

        #region Constructor
        public AddSIPAccount()
        {
            InitializeComponent();

            //Need to add IsModified to make DoValidate method work
            textEditSIPUrl.IsModified = true;
            textEditDisplayName.IsModified = true;
            textEditUsername.IsModified = true;
            textEditPassword.IsModified = true;
            textEditOperator.IsModified = true;
        }

        public AddSIPAccount(int sipid):this() {
            sipId = sipid;
            
        }
        #endregion

        #region Events
        private void simpleButtonSave_Click(object sender, EventArgs e)
        {
            if (!(textEditSIPUrl.DoValidate()
                && textEditDisplayName.DoValidate()
                && textEditUsername.DoValidate()
                && textEditPassword.DoValidate()
                && textEditOperator.DoValidate()
                ))
                return;

            BrightPlatformEntities objModel = new BrightPlatformEntities(UserSession.EntityConnection);
            WaitDialog.Show(this.ParentForm, "Saving SIP Account...");
            sip_accounts sipaccount = null;
            if (sipId == 0)
            {
                sipaccount = objModel.sip_accounts.CreateObject();
                sipaccount.sip_url = textEditSIPUrl.EditValue.ToString();
                sipaccount.display_name = textEditDisplayName.EditValue.ToString();
                sipaccount.username = textEditUsername.EditValue.ToString();
                sipaccount.password = textEditPassword.EditValue.ToString();
                sipaccount.@operator = textEditOperator.EditValue.ToString();
                sipaccount.comment = memoEditComment.EditValue == null ? "" : memoEditComment.EditValue.ToString();
                objModel.sip_accounts.AddObject(sipaccount);
            }
            else
            {
                sipaccount = objModel.sip_accounts.Where(pr => pr.id == sipId).FirstOrDefault();
                sipaccount.sip_url = textEditSIPUrl.EditValue.ToString();
                sipaccount.display_name = textEditDisplayName.EditValue.ToString();
                sipaccount.username = textEditUsername.EditValue.ToString();
                sipaccount.password = textEditPassword.EditValue.ToString();
                sipaccount.@operator = textEditOperator.EditValue.ToString();
                sipaccount.comment = memoEditComment.EditValue == null ? "" : memoEditComment.EditValue.ToString();
            }
            objModel.SaveChanges();
            WaitDialog.Close();
            this.ParentForm.Close();
           

            var sipAccount = objModel.sip_accounts.Where(pr => pr.display_name == sipaccount.display_name).FirstOrDefault();
            SIPId = sipAccount.id;
        }

        private void AddSIPAccount_Load(object sender, EventArgs e){
            BrightPlatformEntities objModel = new BrightPlatformEntities(UserSession.EntityConnection);
            if (sipId != 0)
            {
                var sipaccount = objModel.sip_accounts.Where(pr => pr.id == sipId).FirstOrDefault();
                textEditSIPUrl.EditValue = sipaccount.sip_url;
                textEditDisplayName.EditValue = sipaccount.display_name;
                textEditUsername.EditValue = sipaccount.username;
                textEditPassword.EditValue = sipaccount.password;
                textEditOperator.EditValue = sipaccount.@operator;
                memoEditComment.EditValue = sipaccount.comment;
            }
           
        }

        private void textEditSIPUrl_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textEditSIPUrl.Text)) 
            {
                e.Cancel = true;
            }
        }

        private void textEditDisplayName_Validating(object sender, CancelEventArgs e)
        {
            string displayName = textEditDisplayName.Text;
            if (string.IsNullOrWhiteSpace(textEditDisplayName.Text))
            {
                e.Cancel = true;
                return;
            }

            #region Check if display name is already use
            var objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var sipAccountResult = objDbModel.sip_accounts.Where(s => s.display_name == displayName && s.id != sipId).Count();
            if (sipAccountResult > 0)
            {
                e.Cancel = true;
                MessageBox.Show("The display name is already used.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void textEditUsername_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textEditUsername.Text))
            {
                e.Cancel = true;
            }
        }

        private void textEditPassword_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textEditPassword.Text))
            {
                e.Cancel = true;
            }
        }

        private void textEditOperator_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textEditOperator.Text))
            {
                e.Cancel = true;
            }
        }
        #endregion
    }
}
