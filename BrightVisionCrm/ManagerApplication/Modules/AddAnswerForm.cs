using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules {
    public partial class AddAnswerForm : DevExpress.XtraEditors.XtraUserControl {

        #region Member Variables
        private BrightPlatformEntities BPContext = null;
        private int? question_id;
        private GridView gridviewSender;
        private SimpleButton createSchedule;
        #endregion

        #region Constructor
        public AddAnswerForm(int questionid, GridView sender, SimpleButton btnSchedule) {
            InitializeComponent();
            question_id = questionid;
            gridviewSender = sender;
            createSchedule = btnSchedule;
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        } 
        #endregion

        #region Private Methods
        private void AddAnswerForm_Load(object sender, EventArgs e) {
            var view = (gridviewSender.DataSource as DataView);
            List<string> answerformIds = null;
            if (view != null && view.Table != null) {
                DataRow[] drs = view.Table.Select("answer_form_id > 0");
                if (drs != null && drs.Length > 0) {
                    answerformIds = new List<string>();
                    foreach(DataRow dr in drs) {
                        answerformIds.Add(dr["answer_form_id"].ToString());
                    }                    
                }
            }
            string strAnsIds = null;

            if (answerformIds != null && answerformIds.Count > 0) {                
                strAnsIds = string.Join(",", answerformIds.ToArray());
            }

            var objSource = BPContext.FIGetAnswerForms(question_id,strAnsIds);
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
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["answer_form_id"], row.answerform_id);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["answer_form_language_id"], row.answerform_language_id);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["answer_form"], row.title.Trim());
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["content_json"], row.content_json);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["account_level"], row.account_level);
                gridview.UpdateCurrentRow();
                gridview.MakeRowVisible(gridview.FocusedRowHandle, true);
                gridview.ShowEditor();
                try {
                    //enable creation of schedule if there is schedule component added.
                    BrightVision.DQControl.Business.CampaignQuestionnaire ques =
                        BrightVision.DQControl.Business.CampaignQuestionnaire.InstanciateWith(row.content_json);
                    if (ques.Type.ToLower() == BrightVision.DQControl.Business.QuestionTypeConstants.Schedule) {
                        createSchedule.Enabled = true;
                    }
                } catch { }
            }
            ParentForm.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            ParentForm.Close();
        }

        private void gridViewAnswerForm_DoubleClick(object sender, EventArgs e) {
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                view.FocusedRowHandle = hitInfo.RowHandle;
                simpleButtonAssign_Click(null, null);
            }
        }
        private void gridViewAnswerForm_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion
        
    }
}
