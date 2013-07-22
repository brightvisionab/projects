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

using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using QuestionnaireEditor.Business;

namespace QuestionnaireEditor.Modules {
    public partial class ManageQuestions : DevExpress.XtraEditors.XtraUserControl {
        private BrightPlatformEntities BPContext = null;
        private NewQuestions ucNewQuestion1;
        private PopupDialog popupDialog = null;

        public ManageQuestions() {
            this.Visible = false;
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            WaitDialog.SetWaitDialogCaption("Loading questions...");
            BindGridQuestions();     
            this.Visible = true;
            WaitDialog.CloseWaitDialog();
        }
        private void BindGridQuestions() {
            var objSource = BPContext.FIQuestionTags(null);
            if (gridControlQuestions.DataSource == null) {
                gridControlQuestions.DataSource = objSource;
            }
        }

        private void simpleButtonAdd_Click(object sender, EventArgs e) {
            WaitDialog.CreateWaitDialog("Loading components...");
            popupDialog = new PopupDialog();
            Padding pads = popupDialog.Padding;
            pads.Left = 5;
            popupDialog.WindowState = FormWindowState.Maximized;
            popupDialog.StartPosition = FormStartPosition.CenterScreen;
            popupDialog.Text = "Add Question";
            ucNewQuestion1 = new Modules.NewQuestions();
            ucNewQuestion1.Dock = DockStyle.Fill;
            ucNewQuestion1.lblHeader.Text = "Add Question";            
            popupDialog.Controls.Add(ucNewQuestion1);            
            popupDialog.ClientSize = new Size(ucNewQuestion1.Width + 10, ucNewQuestion1.Height + 10);
            popupDialog.ShowDialog(this.ParentForm);
            WaitDialog.CloseWaitDialog();
        }

        private void simpleButtonEdit_Click(object sender, EventArgs e) {
            WaitDialog.CreateWaitDialog("Loading components...");
            popupDialog = new PopupDialog();
            Padding pads = popupDialog.Padding;
            pads.Left = 5;
            popupDialog.WindowState = FormWindowState.Maximized;
            popupDialog.StartPosition = FormStartPosition.CenterScreen;
            popupDialog.Text = "Edit Question";
            
            GridView view = gridViewQuestion;
            if (view != null || view.SelectedRowsCount > 0) {      
                var ctqt = view.GetRow(view.GetSelectedRows()[0]) as CTQuestionTags;
                ucNewQuestion1 = new Modules.NewQuestions(ctqt.question_id);
            }
            
            ucNewQuestion1.Dock = DockStyle.Fill;
            ucNewQuestion1.lblHeader.Text = "Edit Question";
            popupDialog.Controls.Add(ucNewQuestion1);
            popupDialog.ClientSize = new Size(ucNewQuestion1.Width + 10, ucNewQuestion1.Height + 10);
            popupDialog.ShowDialog(this.ParentForm);
            popupDialog.FormClosing += new FormClosingEventHandler(popupDialog_FormClosing);
            WaitDialog.CloseWaitDialog();
        }

        void popupDialog_FormClosing(object sender, FormClosingEventArgs e) {
            WaitDialog.CreateWaitDialog("Reloading questions...");
            var objSource = BPContext.FIQuestionTags(null);
            gridControlQuestions.DataSource = objSource;
            WaitDialog.CloseWaitDialog();
        }
    }
}
