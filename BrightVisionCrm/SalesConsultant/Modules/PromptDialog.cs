using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraVerticalGrid.Rows;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using SalesConsultant.Business;
using BrightVision.Model;

using System.Data.Objects;
using System.Data.Objects.DataClasses;
using BrightVision.Common.Business;

namespace SalesConsultant.Modules
{
    public partial class PromptDialog : DevExpress.XtraEditors.XtraUserControl {
        #region Variables
        private BrightPlatformEntities BPContext = null;
        private dialog m_oDialog = null; 
        #endregion

        public ManageDialogs ParentController { get; set; }

        #region Constructor
        public PromptDialog(dialog objDialog) {
            m_oDialog = objDialog;
            this.Visible = false;
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            WaitDialog.Show(ParentForm, "Loading subcampaigns...");
            SetValidationRules();
            BindLookupEdit();
            this.Visible = true;
            textEditDialogname.Text = m_oDialog.name;
            WaitDialog.Close();
        } 
        #endregion

        #region Methods
        private void BindLookupEdit() {
            var datasource = BPContext.subcampaigns.Execute(MergeOption.AppendOnly).ToList();
            this.lookUpEditSubcampaigns.Properties.DataSource = datasource;
            this.lookUpEditSubcampaigns.Properties.DisplayMember = "title";
            this.lookUpEditSubcampaigns.Properties.ValueMember = "id";
            this.lookUpEditSubcampaigns.Properties.Columns.Add(new LookUpColumnInfo("title", "Sub campaign name"));
            this.lookUpEditSubcampaigns.Properties.ShowHeader = false;
            this.lookUpEditSubcampaigns.Properties.ValidateOnEnterKey = true;
        }

        private void SetValidationRules() {
            CustomIsBlankValidationRule isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The dialog name cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(textEditDialogname, isBlankValidationRule);
            isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The subcampaign cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(lookUpEditSubcampaigns, isBlankValidationRule);
            isBlankValidationRule = new CustomIsBlankValidationRule();
        }

        private void simpleButtonSave_Click(object sender, EventArgs e) {
            if (m_oDialog == null) return;
            if (!dxValidationProvider1.Validate()) return;
            if (ObjectDialog.Exists(textEditDialogname.Text, (int)lookUpEditSubcampaigns.EditValue))
            {
                MessageBox.Show("Dialog already exist for sub-campaign " + lookUpEditSubcampaigns.Text.ToUpper(), "Dialogs", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            Cursor currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            string dialog_text_json = string.Empty;

            dialog oDialog = new dialog();
            oDialog.is_active = true;
            oDialog.name = textEditDialogname.Text;
            oDialog.subcampaign_id = Convert.ToInt32(lookUpEditSubcampaigns.EditValue);
            oDialog.created_date = DateTime.Now;
            oDialog.dialog_text_json = m_oDialog.dialog_text_json;
            BPContext.dialogs.AddObject(oDialog);
            BPContext.SaveChanges();
            ParentController.DefaultSelectedDialogId = oDialog.id;
            Cursor.Current = currentCursor;
            if (ParentForm != null) {
                ParentForm.DialogResult = DialogResult.OK;
                if (ParentForm.Owner != null)
                    ParentForm.Owner.DialogResult = DialogResult.OK;
            }
        }

        private void simpleButtonCancel_Click(object sender, EventArgs e) {
            if (ParentForm != null)
                ParentForm.DialogResult = DialogResult.Cancel;
        } 
        #endregion

    }
}
