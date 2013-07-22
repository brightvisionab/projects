using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightVision.Model;
using BrightVision.Common.Business;
using System.Data.Objects;
using ManagerApplication.Business;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules
{
    public partial class ManageSIPAccount : UserControl
    {
        BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        public ManageSIPAccount()
        {
            this.Visible = false;
            InitializeComponent();
            LoadSIPAccount();
            this.Visible = true;
        }

        #region Events
        private void LoadSIPAccount()
        {
            objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            gcSIPAccount.DataSource = null;
            var sipaccount = objDbModel.sip_accounts.AsQueryable();
            gcSIPAccount.DataSource = sipaccount;
        }
        private void simpleButtonDelete_Click(object sender, EventArgs e)
        {
            if (gvSIPAccount.RowCount < 1)
                return;

            if (gvSIPAccount.FocusedRowHandle < 0)
                return;
            /**
             * validation to check if there are users related to this account.
             */
            var _item = gvSIPAccount.GetFocusedRow() as sip_accounts;
            int countUser = objDbModel.users.Where(i => i.sip_id == _item.id).Count();
            if (countUser > 0)
            {
                MessageBox.Show(
                    string.Format("There are {0} users assigned to this SIP account.",
                        countUser.ToString()
                    ),
                    "Bright Sales",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }
            
            
            var dialog = MessageBox.Show("Are you sure you want to delete the selected SIP account?", "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog != DialogResult.Yes)
                return;
            WaitDialog.Show("Loading Component...");
            gvSIPAccount.DeleteRow(gvSIPAccount.FocusedRowHandle);
            objDbModel.SaveChanges();
            LoadSIPAccount();
            WaitDialog.Close();
            MessageBox.Show("SIP account deleted.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        private void simpleButtonAdd_Click(object sender, EventArgs e)
        {
            var newAddSIPAccount = new AddSIPAccount();
            var dialog = new PopupDialog(newAddSIPAccount, "Add SIP Account");
            dialog.ShowDialog();
            if (newAddSIPAccount.SIPId > 0) {
                WaitDialog.Show("Loading Component...");
                LoadSIPAccount();
                WaitDialog.Close();
                MessageBox.Show("Successfully Saved!", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                gvSIPAccount.SelectRow("id", newAddSIPAccount.SIPId);
                WaitDialog.Close();
            }
        }
        private void simpleButtonEdit_Click(object sender, EventArgs e)
        {
            if (gvSIPAccount.RowCount < 1)
                return;

            var sipaccount = gvSIPAccount.GetFocusedRow() as sip_accounts;
            var EditSIPAccount = new AddSIPAccount(sipaccount.id);
            var dialog = new PopupDialog(EditSIPAccount, "Edit SIP EditSIPAccount");
            dialog.ShowDialog();
            if (sipaccount.id > 0) {
                WaitDialog.Show("Loading Component...");
                LoadSIPAccount();
                gvSIPAccount.SelectRow("id", sipaccount.id);
                WaitDialog.Close();
            }
        }
    }
}
