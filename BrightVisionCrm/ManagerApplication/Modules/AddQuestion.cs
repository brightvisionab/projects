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
using ManagerApplication.Business;

namespace ManagerApplication.Modules {
    public partial class AddQuestion : DevExpress.XtraEditors.XtraUserControl {

        #region Member Variables
        private BrightPlatformEntities BPContext = null;
        
        #endregion

        #region Constructor
        public AddQuestion(int question_id) {
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            QuestionID = question_id;
        }
        public AddQuestion(GridView oQuestionGrid, int question_id) {
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            QuestionID = question_id;
            QuestionGrid = oQuestionGrid;
        }
        #endregion

        #region Methods
        protected override void OnLoad(EventArgs e) {
            BindLanguage();
            if (EditMode) {
                if (QuestionGrid != null) {
                    var gridview = QuestionGrid;
                    var obj = gridview.GetFocusedRow();
                    if (obj != null) {
                        DataRowView drRowView = (DataRowView)obj;
                        textEditQuestion.Text = drRowView.Row["question"].ToString();
                        textEditDescription.Text = drRowView.Row["description"].ToString();
                        textEditHelp.Text = drRowView.Row["helptext"].ToString();
                        lookUpEditLanguage.EditValue =Convert.ToInt32(drRowView.Row["language_id"]);
                    }
                }
            }
            SetValidationRules();
            base.OnLoad(e);
        }
        private bool m_editMode;
        public bool EditMode {
            get { return m_editMode; }
            set {
                m_editMode = value;
                if(value)
                    simpleButtonAdd.Text = "Update";                
            }
        }
        public GridView QuestionGrid { get; set; }
        public int QuestionID { get; set; }

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
            if (!dxValidationProvider1.Validate()) {
                this.ParentForm.DialogResult = DialogResult.None;
                return;
            }
            if (QuestionGrid != null) {
                var gridview = QuestionGrid;
                if (simpleButtonAdd.Text != "Update") {
                    gridview.AddNewRow();    
                }                

                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns[0], lookUpEditLanguage.EditValue);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns[1], textEditQuestion.Text);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns[2], textEditDescription.Text);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns[3], textEditHelp.Text);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns[4], QuestionID);
                gridview.CloseEditor();
                gridview.UpdateCurrentRow();
                gridview.MakeRowVisible(gridview.FocusedRowHandle, true);
            }
            this.textEditQuestion.Text = string.Empty;
            this.textEditHelp.Text = string.Empty;
            this.textEditDescription.Text = string.Empty;
            this.lookUpEditLanguage.ItemIndex = 0;
            ParentForm.DialogResult = DialogResult.OK;
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
        #endregion
    }
}
