using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.DXErrorProvider;

using BrightVision.Model;
using BrightVision.Common.Business;
using QuestionnaireEditor.Business;

namespace QuestionnaireEditor.Modules {
    public partial class AddTag : DevExpress.XtraEditors.XtraUserControl {
        private BrightPlatformEntities BPContext = null;

        public AddTag() {
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        }
        public GridControl TagsGrid { get; set; }
        private void simpleButtonCancel_Click(object sender, EventArgs e) {
            ParentForm.Close();
        }

        private void simpleButtonAdd_Click(object sender, EventArgs e) {
            if (!dxValidationProvider1.Validate()) return;

            BPContext.questiontags.AddObject(new questiontag() {
                title = textEditTagname.Text,
                description = textEditDescription.Text
            });
            BPContext.SaveChanges();
            var datasource = BPContext.questiontags.Execute(System.Data.Objects.MergeOption.AppendOnly);
            TagsGrid.DataSource = datasource;
            this.textEditTagname.Text = string.Empty;
            this.textEditDescription.Text = string.Empty;
            ParentForm.Close();
        }
        private void SetValidationRules() {
            CustomIsBlankValidationRule isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The tag name field cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(textEditTagname, isBlankValidationRule);
        }

        private void textEditTagname_Validating(object sender, CancelEventArgs e) {
            if (!dxValidationProvider1.Validate()) e.Cancel = true;
        }
    }
}
