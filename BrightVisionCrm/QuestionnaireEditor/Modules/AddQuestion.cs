using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
    public partial class AddQuestion : DevExpress.XtraEditors.XtraUserControl {
        private BrightPlatformEntities BPContext = null;
        public AddQuestion() {
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        }
        protected override void OnLoad(EventArgs e) {
            BindLanguage();
            SetValidationRules();
            base.OnLoad(e);
        }
        public GridView QuestionGrid { get; set; }

        private void BindLanguage() {
            if (this.lookUpEditLanguage.Properties.DataSource != null) return;
            this.textEditQuestion.Text = string.Empty;
            this.textEditHelp.Text = string.Empty;
            this.textEditDescription.Text = string.Empty;
            var datasource = BPContext.languages.Execute(System.Data.Objects.MergeOption.AppendOnly);
            this.lookUpEditLanguage.Properties.DataSource = datasource;
            this.lookUpEditLanguage.Properties.DisplayMember = "name";
            this.lookUpEditLanguage.Properties.ValueMember = "id";
            this.lookUpEditLanguage.Properties.Columns.Add(new LookUpColumnInfo("name", "Language"));
            this.lookUpEditLanguage.ItemIndex = -1;
        }

        private void simpleButtonCancel_Click(object sender, EventArgs e) {
            
            ParentForm.Close();
        }

        private void simpleButtonAdd_Click(object sender, EventArgs e) {            
            if (!dxValidationProvider1.Validate()) return;
           
            if (QuestionGrid != null) {
                var gridview = QuestionGrid;
                gridview.AddNewRow();
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns[0], lookUpEditLanguage.EditValue);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns[1], textEditQuestion.Text);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns[2], textEditDescription.Text);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns[3], textEditHelp.Text);
                gridview.CloseEditor();
                gridview.UpdateCurrentRow();                
                gridview.MakeRowVisible(gridview.FocusedRowHandle, true);                
            }
            this.textEditQuestion.Text = string.Empty;
            this.textEditHelp.Text = string.Empty;
            this.textEditDescription.Text = string.Empty;
            this.lookUpEditLanguage.ItemIndex = 0;
            ParentForm.Close();
        }

        private void SetValidationRules() {
            CustomIsBlankValidationRule isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The question field cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(textEditQuestion, isBlankValidationRule);
            isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The language cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(lookUpEditLanguage, isBlankValidationRule);
        }
    }
}
