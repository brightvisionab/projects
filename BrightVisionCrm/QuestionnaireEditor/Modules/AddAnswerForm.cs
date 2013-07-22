using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;

using BrightVision.Model;
using BrightVision.Common.Business;

namespace QuestionnaireEditor.Modules {
    public partial class AddAnswerForm : DevExpress.XtraEditors.XtraUserControl {
        private BrightPlatformEntities BPContext = null;
        private int? question_id;
        private GridView gridviewSender;
        public AddAnswerForm(int questionid, GridView sender) {
            InitializeComponent();
            question_id = questionid;
            gridviewSender = sender;
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        }

        private void AddAnswerForm_Load(object sender, EventArgs e) {
            var objSource = BPContext.FIGetAnswerForms(question_id,null);
            if (gridControlAnswerForm.DataSource == null) {
                gridControlAnswerForm.DataSource = objSource;
                if (gridViewAnswerForm.RowCount > 0) 
                    simpleButtonAssign.Enabled = true;
                else 
                    simpleButtonAssign.Enabled = false;
            }            
        }

        private void simpleButtonAssign_Click(object sender, EventArgs e) {
            GridView view = gridViewAnswerForm;
            view.GridControl.Focus();
            int index = view.FocusedRowHandle;
            if (index < 0) return;

            var row = view.GetRow(index) as CTAnswerForm;
            if (row != null) {               
                GridView gridview = gridviewSender;
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["answer_form_language_id"], row.answerform_language_id);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["answer_form_id"], row.answerform_id);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["answer_form"], row.title.Trim());
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["content_json"], row.content_json);
                gridview.UpdateCurrentRow();
                gridview.MakeRowVisible(gridview.FocusedRowHandle, true);
                gridview.ShowEditor();
            }
            ParentForm.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            ParentForm.Close();
        }
    }
}
