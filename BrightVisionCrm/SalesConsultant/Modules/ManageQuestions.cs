using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraVerticalGrid.Rows;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using BrightVision.Model;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SalesConsultant.Business;
using SalesConsultant.Forms;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;

namespace SalesConsultant.Modules
{
    public partial class ManageQuestions : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Properties
        public int QuestionId = 0;
        #endregion

        #region Variables
        private BrightPlatformEntities BPContext = null;
        private NewQuestions ucNewQuestion1;
        private PopupDialog popupDialog = null;
        private GridCheckMarksSelection gridSelection = null;
        private int m_SelectedRowHandle = 0;
        #endregion

        #region Constructor
        public ManageQuestions() {            
            this.Visible = false;
            InitializeComponent();
            gridSelection = new GridCheckMarksSelection(gridViewQuestion);
            this.layoutControl1.AllowCustomizationMenu = false;
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            BindGridQuestions();
            this.SetQuestionViewContextMenu();
            this.Visible = true;
        } 
        #endregion

        #region Methods

        private void BindGridQuestions() {            
            if (gridControlQuestions.DataSource == null) {
                var objSource = BPContext.FIQuestionTags(null).ToList();
                gridControlQuestions.DataSource = objSource;
                
                gridViewQuestion.Columns["question_id"].Group();
                gridViewQuestion.ExpandAllGroups();
                gridSelection.ClearSelection();
            }
        }

        private void simpleButtonAdd_Click(object sender, EventArgs e) {
            WaitDialog.Show(ParentForm, "Loading components...");
            popupDialog = new PopupDialog();
            //Padding pads = popupDialog.Padding;
            //pads.Left = 5;
            //popupDialog.WindowState = FormWindowState.Maximized;
            popupDialog.Padding = new System.Windows.Forms.Padding(0);
            popupDialog.StartPosition = FormStartPosition.CenterScreen;
            popupDialog.Text = "Add Question";
            ucNewQuestion1 = new Modules.NewQuestions();
            ucNewQuestion1.AfterSave += new NewQuestions.AfterSaveEventHandler(ucNewQuestion1_AfterSave);
            //ucNewQuestion1.ParentControl = this;
            ucNewQuestion1.Dock = DockStyle.Fill;
            popupDialog.Controls.Add(ucNewQuestion1);
            popupDialog.ClientSize = new Size(1660, 980);
            //popupDialog.ClientSize = new Size(ucNewQuestion1.Width + 10, ucNewQuestion1.Height + 10);
            //popupDialog.FormClosing += new FormClosingEventHandler(popupDialog_FormClosing);
            WaitDialog.Close();
            popupDialog.ShowDialog(this.ParentForm);
        }

        private void simpleButtonEdit_Click(object sender, EventArgs e) 
        {
            int selCount = gridSelection.SelectedCount;
            if (gridViewQuestion.RowCount < 1 || selCount == 0)
                return;

            WaitDialog.Show(ParentForm, "Loading components...");
            m_SelectedRowHandle = gridViewQuestion.FocusedRowHandle;
            popupDialog = new PopupDialog();
            popupDialog.Padding = new System.Windows.Forms.Padding(0);
            //Padding pads = popupDialog.Padding;
            //pads.Left = 5;
            popupDialog.ClientSize = new Size(1660, 980);
            //popupDialog.WindowState = FormWindowState.Maximized;
            popupDialog.StartPosition = FormStartPosition.CenterScreen;
            popupDialog.Text = "Edit Question";

            GridView view = gridViewQuestion;
            if (view != null || view.SelectedRowsCount > 0) {
                var ctqt = view.GetRow(view.GetSelectedRows()[0]) as CTQuestionTags;
                ucNewQuestion1 = new Modules.NewQuestions(ctqt.question_id);
                ucNewQuestion1.AfterSave += new NewQuestions.AfterSaveEventHandler(ucNewQuestion1_AfterSave);
            }

            //ucNewQuestion1.ParentControl = this;
            ucNewQuestion1.Dock = DockStyle.Fill;
            popupDialog.Controls.Add(ucNewQuestion1);
            //popupDialog.ClientSize = new Size(ucNewQuestion1.Width + 10, ucNewQuestion1.Height + 10);
            popupDialog.FormClosing += new FormClosingEventHandler(popupDialog_FormClosing);
            WaitDialog.Close();
            popupDialog.ShowDialog(this.ParentForm);
            gridSelection.ClearSelection();
        }

        private void simpleButtonDelete_Click(object sender, EventArgs e) {
            int selCount = gridSelection.SelectedCount;
           
            if (selCount < 1) return;

            if (MessageBox.Show("Are you sure you want to delete the selected question(s)?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.No) return;
            
            var rows = gridSelection.GetSelectedRows;
            CTQuestionTags row = null;
            List<string> ids = new List<string>();
            for (int x = 0; x < selCount; ++x) {
                row = rows[x] as CTQuestionTags;
                ids.Add(row.question_text_language_id.ToString());
            }

            BPContext.FIDeleteQuestion(string.Join(",", ids.ToArray()));
            WaitDialog.Show(ParentForm, "Reloading dialogs...");
            gridControlQuestions.DataSource = null;
            BindGridQuestions();
            WaitDialog.Close();
        }

        private void popupDialog_FormClosing(object sender, FormClosingEventArgs e) 
        {
            //if (popupDialog != null && popupDialog.DialogResult == DialogResult.OK) {
            WaitDialog.Show(ParentForm, "Reloading questions...");
            var objSource = BPContext.FIQuestionTags(null);
            gridControlQuestions.DataSource = objSource;
            gridViewQuestion.FocusedRowHandle = m_SelectedRowHandle;
            WaitDialog.Close();

            /** /
            int selectedIndex = 0;
            if (QuestionId > 0) {
                CTQuestionTags _item = null;
                for (int i = 0; i < gridViewQuestion.RowCount; i++) {
                    var row = gridViewQuestion.GetRow(i);
                    if(row == null) continue;
                    _item =  row as CTQuestionTags;
                    if (QuestionId == _item.question_id) {
                        selectedIndex = i;
                        break;
                    }
                }
            }
            QuestionId = 0;
            gridViewQuestion.FocusedRowHandle = selectedIndex; 
            /**/
            //else {
            //    if (popupDialog.Text == "Edit Question")
            //        selectedIndex = lastSelectedIndex;
            //    else
            //        selectedIndex = lastSelectedIndex + 1;
            //}

            
            //}
        }

        private void gridViewQuestion_DoubleClick(object sender, EventArgs e) {
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                view.FocusedRowHandle = hitInfo.RowHandle;
                simpleButtonEdit_Click(null, null);
            }
        }

        private void gridViewQuestion_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private int lastSelectedIndex = 0;
        
        private void gridViewQuestion_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e) {
            if (e.FocusedRowHandle == 0)
                lastSelectedIndex = e.PrevFocusedRowHandle;
            else
                lastSelectedIndex = e.FocusedRowHandle;
        }

        private void gridViewQuestion_EndGrouping(object sender, EventArgs e) {
             var view = sender as GridView;
             if (view != null) {
                 view.ExpandAllGroups();
             }
        }
        
        private void miQuestionPrintPreview_Click(object sender, EventArgs e)
        {
            gridControlQuestions.ShowPrintPreview();
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
        private void SetQuestionViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miQuestionPrintPreview = new MenuItem("Print Preview");
            miQuestionPrintPreview.Click += new EventHandler(miQuestionPrintPreview_Click);
            objClickMenu.MenuItems.Add(miQuestionPrintPreview);
            gridControlQuestions.ContextMenu = objClickMenu;
        }
        #endregion

        #region Event Subscriptions
        private void ucNewQuestion1_AfterSave(object sender, NewQuestions.EditQuestionArgs e)
        {
            WaitDialog.Show(ParentForm, "Reloading questions...");
            QuestionId = e.QuestionId;
            var objSource = BPContext.FIQuestionTags(null);
            gridControlQuestions.DataSource = objSource;
            if (e.OnEditMode)
                gridViewQuestion.FocusedRowHandle = m_SelectedRowHandle;
            else {
                for (int i = 0; i < gridViewQuestion.DataRowCount; i++) {
                    CTQuestionTags _item = gridViewQuestion.GetRow(i) as CTQuestionTags;
                    if (_item.question_id == e.QuestionId) {
                        gridViewQuestion.FocusedRowHandle = i;
                        break;
                    }
                }
            }
            ucNewQuestion1.AfterSave -= new NewQuestions.AfterSaveEventHandler(ucNewQuestion1_AfterSave);
            WaitDialog.Close();
        }
        #endregion
    }
}
