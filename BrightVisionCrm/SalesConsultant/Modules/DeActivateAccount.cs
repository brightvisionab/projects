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

namespace SalesConsultant.Modules
{
    public partial class DeActivateAccount : DevExpress.XtraEditors.XtraUserControl
    {
        public DeActivateAccount()
        {
            InitializeComponent();
        }

        public delegate void AfterSaveEventHandler(object sender, EventArgs e);
        public event AfterSaveEventHandler AfterSave;

        public int AccountId { get; set; }
        public int FinalListId { get; set; }
        
        private void DeActivateSelectedCompany()
        {
            DialogResult objResult = MessageBox.Show("WARNING! this will inactivate the current company for all campaigns. Are you sure to perform this transaction?", "Delete Company", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.No) {
                this.ParentForm.DialogResult = DialogResult.Cancel;
                return;
            }

            WaitDialog.Show(ParentForm, "De-activating...");
            ObjectCompany.DeActivateCompany(AccountId, txtReason.Text);
            ObjectCompany.RemoveAccountFromSubCampaign(AccountId);//, FinalListId);
            WaitDialog.Close();

            if (AfterSave != null) {
                AfterSave(this, new EventArgs());
                this.ParentForm.Close();
            }

            //this.ParentForm.DialogResult = DialogResult.OK;
        }
        private void cmdSave_Click(object sender, EventArgs e)
        {
            if (txtReason.Text.Length < 1) {
                NotificationDialog.Information("Bright Sales", "Please add a reason for de-activating this particular company.");
                //cmdSave.DialogResult = DialogResult.None;
                this.ParentForm.DialogResult = DialogResult.None;
            }
            else {
                //cmdSave.DialogResult = DialogResult.OK;
                this.DeActivateSelectedCompany();
            }
        }
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.ParentForm.DialogResult = DialogResult.Cancel;
            //this.ParentForm.Close();
        }
    }
}
