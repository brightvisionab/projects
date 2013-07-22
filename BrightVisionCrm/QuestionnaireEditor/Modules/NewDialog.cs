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
using BrightVision.Common.Utilities;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using QuestionnaireEditor.Business;

namespace QuestionnaireEditor.Modules {
    public partial class NewDialog : DevExpress.XtraEditors.XtraUserControl {

        #region Member Variables
        private BrightPlatformEntities BPContext = null;
        private Dictionary<int, BindingSource> ChildList = null;
        private const string OrderFieldName = "position";
        private PopupDialog popupDialog = null;
        private bool isSaved = false;
        private dialog m_oDialog = null;
        #endregion
        
        #region Constructor
        public NewDialog() {
            Initialize();
        }
        public NewDialog(CTSubCampaignDialogs subcampaignDialog) {
            SubCampaignDialog = subcampaignDialog;
            Initialize();
        }
        public CTSubCampaignDialogs SubCampaignDialog { get; set; }
        #endregion
        
        #region Private Methods
        private void Initialize() {
            this.Visible = false;
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            WaitDialog.SetWaitDialogCaption("Loading subcampaigns...");
            BindSourceLookup();
            WaitDialog.SetWaitDialogCaption("Loading questions...");
            BindGridQuestions();
            BindGridDialog();
            SetValidationRules();
            ChildList = new Dictionary<int, BindingSource>();
            if (SubCampaignDialog != null)
                LoadDialog(SubCampaignDialog);
            this.Visible = true;
            WaitDialog.CloseWaitDialog();
        }

        private void LoadDialog(CTSubCampaignDialogs subcampaignDialog) {
            isSaved = true;
            textEditDialogname.Text = subcampaignDialog.DialogName;
            lookUpEditSource.EditValue = subcampaignDialog.SubCampaignID;

            m_oDialog = BPContext.dialogs.FirstOrDefault(p => p.id == subcampaignDialog.DialogID);
            var CQList = new List<CampaignQuestionnaire>();
            var QLIDs = new List<int>();
            var jaDiag = JArray.Parse(m_oDialog.dialog_text_json);
            CampaignQuestionnaire CQ = null;
            int qlid;
            jaDiag.ForEach(delegate(JToken jt) {
                CQ = CampaignQuestionnaire.InstanciateWith(jt.ToString(Formatting.None).Unescape());
                if(CQ != null) {
                    if(int.TryParse(CQ.Form.Settings.DataBindings.questionlayout_id, out qlid))
                        QLIDs.Add(qlid);
                    CQList.Add(CQ);
                }
            });
            if (CQList.Count <= 0) {
                simpleButtonAssignAnswerForm.Enabled = false;
                return;
            }

            DataTable dtDialog = gridControlDialog.DataSource as DataTable;
            int qid, qtlid;
            var answerform = (from bp in BPContext.questionlayouts.Include("questionlayout_language")
                             where QLIDs.Contains(bp.id)
                             select bp).ToList();
            questionlayout questionLayout = null;
            CTQuestionTags questiontags = null;
            DataRow newDr = null;
            CQList.ForEach(delegate(CampaignQuestionnaire dcq) {

                var datasource = gridControlQuestions.DataSource as List<CTQuestionTags>;                
                if(int.TryParse(dcq.Form.Settings.DataBindings.question_id, out qid) && 
                    int.TryParse(dcq.Form.Settings.DataBindings.questions_text_language_id, out qtlid) &&
                    int.TryParse(dcq.Form.Settings.DataBindings.questionlayout_id, out qlid)) {
                        questionLayout = answerform.FirstOrDefault(p => p.questions_id == qid && p.id == qlid);
                        questiontags = datasource.FirstOrDefault(x => x.question_id == qid && x.question_text_language_id == qtlid);
                }

                if (questiontags != null) {
                    int rowcount = 0;                    
                    if (dtDialog != null) {
                        //DataRow[] drs = dtDialog.Select("question_text='" + row.question_text + "'");
                        //if (drs.Length > 0) {
                        //    MessageBox.Show("The selected question has already been added to the dialog list.", "Information");
                        //    return;
                        //} else {
                        DataRow[] last = dtDialog.Select("position = Max(position)");
                        if (last.Length > 0) {
                            rowcount = Convert.ToInt32(last[0].ItemArray[0]);
                        }
                        //}
                    }
                    newDr = dtDialog.NewRow();
                    newDr["position"] = rowcount + 1;
                    newDr["question_text"] = questiontags.question_text;
                    newDr["question_id"] = questiontags.question_id;
                    newDr["question_text_language_id"] = questiontags.question_text_language_id;
                    newDr["question_help_text"] = questiontags.question_help_text;
                    newDr["question_description"] = questiontags.question_description;

                    if (questionLayout != null) {
                        newDr["answer_form"] = questionLayout.title;
                        newDr["answer_form_id"] = questionLayout.id;
                        newDr["answer_form_language_id"] = questionLayout.questionlayout_language.FirstOrDefault().id;
                        newDr["content_json"] = questionLayout.content_json;
                    }
                    dtDialog.Rows.Add(newDr);
                    dtDialog.AcceptChanges();
                    simpleButtonAssignAnswerForm.Enabled = true;
                }
            });
        }

        private void BindSourceLookup() {
            var datasource = BPContext.subcampaigns.Execute(MergeOption.AppendOnly).ToList();
            this.lookUpEditSource.Properties.DataSource = datasource;
            this.lookUpEditSource.Properties.DisplayMember = "title";
            this.lookUpEditSource.Properties.ValueMember = "id";
            this.lookUpEditSource.Properties.Columns.Add(new LookUpColumnInfo("title", "Sub campaign name"));
            this.lookUpEditSource.Properties.ShowHeader = false;
            this.lookUpEditSource.Properties.ValidateOnEnterKey = true;
        }

        private void BindGridQuestions() {

            var objSource = BPContext.FIQuestionTags(null);
            if (gridControlQuestions.DataSource == null) {
                gridControlQuestions.DataSource = objSource.ToList();
            }
        }

        private void BindGridDialog() {
            DataTable dtDialog = new DataTable();
            //add columns
            dtDialog.Columns.Add("position", typeof(int));
            dtDialog.Columns.Add("question_text", typeof(string));
            dtDialog.Columns.Add("question_help_text", typeof(string));
            dtDialog.Columns.Add("question_description", typeof(string));
            dtDialog.Columns.Add("question_id", typeof(int));
            dtDialog.Columns.Add("question_text_language_id", typeof(int));
            dtDialog.Columns.Add("answer_form", typeof(string));
            dtDialog.Columns.Add("answer_form_id", typeof(int));
            dtDialog.Columns.Add("answer_form_language_id", typeof(int));            
            dtDialog.Columns.Add("content_json", typeof(string));
            dtDialog.AcceptChanges();

            gridControlDialog.DataSource = dtDialog;
            gridViewDialog.Columns[OrderFieldName].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            gridViewDialog.OptionsCustomization.AllowSort = false;
        }

        //private void gridViewQuestion_MasterRowEmpty(object sender, MasterRowEmptyEventArgs e) {
        //    BindingSource source = null;
        //    try {
        //        source = ChildList[e.RowHandle];
        //    } catch { }

        //    if (source == null) {
        //        var obj = (CTQuestionLanguage)gridViewQuestion.GetRow(e.RowHandle);
        //        var tags = BPContext.FIQuestionTags(obj.question_id);
        //        source = new BindingSource(tags, "");
        //        ChildList.Add(e.RowHandle, source);
        //    }
        //    e.IsEmpty = source != null ? source.Count == 0 : true;
        //}

        //private void gridViewQuestion_MasterRowGetRelationCount(object sender, MasterRowGetRelationCountEventArgs e) {
        //    e.RelationCount = 1;
        //}

        //private void gridViewQuestion_MasterRowGetRelationName(object sender, MasterRowGetRelationNameEventArgs e) {
        //    e.RelationName = "Tags";
        //}

        //private void gridViewQuestion_MasterRowGetChildList(object sender, MasterRowGetChildListEventArgs e) {
        //    BindingSource source = null;
        //    try {
        //        source = ChildList[e.RowHandle];
        //    } catch { }

        //    if (source == null) {
        //        var obj = (CTQuestionLanguage)gridViewQuestion.GetRow(e.RowHandle);
        //        var tags = BPContext.FIQuestionTags(obj.question_id);
        //        source = new BindingSource(tags, "");
        //        ChildList.Add(e.RowHandle, source);
        //    }
        //    e.ChildList = source;
        //}

        private void simpleButtonMoveUp_Click(object sender, EventArgs e) {
            GridView view = gridViewDialog;
            view.GridControl.Focus();
            int index = view.FocusedRowHandle;
            if (index <= 0) return;

            DataRow row1 = view.GetDataRow(index);
            DataRow row2 = view.GetDataRow(index - 1);
            object val1 = row1[OrderFieldName];
            object val2 = row2[OrderFieldName];
            row1[OrderFieldName] = val2;
            row2[OrderFieldName] = val1;

            view.FocusedRowHandle = index - 1;
        }

        private void simpleButtonMoveDown_Click(object sender, EventArgs e) {
            GridView view = gridViewDialog;
            view.GridControl.Focus();
            int index = view.FocusedRowHandle;
            if (index >= view.DataRowCount - 1) return;

            DataRow row1 = view.GetDataRow(index);
            DataRow row2 = view.GetDataRow(index + 1);
            object val1 = row1[OrderFieldName];
            object val2 = row2[OrderFieldName];
            row1[OrderFieldName] = val2;
            row2[OrderFieldName] = val1;

            view.FocusedRowHandle = index + 1;
        }

        private void simpleButtonAddToDialog_Click(object sender, EventArgs e) {
            GridView view = gridViewQuestion;
            view.GridControl.Focus();
            int index = view.FocusedRowHandle;
            if (index < 0) {
                simpleButtonAssignAnswerForm.Enabled = false;
                return;
            }

            var row = view.GetRow(index) as CTQuestionTags;
            if (row != null) {
                int rowcount = 0;

                DataTable dtDialog = gridControlDialog.DataSource as DataTable;
                if (dtDialog != null) {
                    //DataRow[] drs = dtDialog.Select("question_text='" + row.question_text + "'");
                    //if (drs.Length > 0) {
                    //    MessageBox.Show("The selected question has already been added to the dialog list.", "Information");
                    //    return;
                    //} else {
                    DataRow[] last = dtDialog.Select("position = Max(position)");
                    if (last.Length > 0) {
                        rowcount = Convert.ToInt32(last[0].ItemArray[0]);
                    }
                    //}
                }

                GridView gridview = gridViewDialog;

                gridview.AddNewRow();
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["position"], rowcount + 1);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["question_text"], row.question_text);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["question_id"], row.question_id);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["question_text_language_id"], row.question_text_language_id);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["question_help_text"], row.question_help_text);
                gridview.SetRowCellValue(gridview.FocusedRowHandle, gridview.Columns["question_description"], row.question_description);

                gridview.UpdateCurrentRow();
                gridview.MakeRowVisible(gridview.FocusedRowHandle, true);
                gridview.CloseEditor();
                simpleButtonAssignAnswerForm.Enabled = true;
            }
        }

        private void simpleButtonAssignAnswerForm_Click(object sender, EventArgs e) {
            Cursor currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            GridView view = gridViewDialog;
            view.GridControl.Focus();
            int index = view.FocusedRowHandle;
            if (index < 0) return;

            var row = view.GetDataRow(index);
            if (row != null) {
                popupDialog = new PopupDialog();
                popupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
                popupDialog.MinimizeBox = false;
                popupDialog.MaximizeBox = false;
                popupDialog.StartPosition = FormStartPosition.CenterScreen;

                int questionid = Convert.ToInt32(row["question_id"]);
                var ucAddAnswerform = new AddAnswerForm(questionid, gridViewDialog);
                Size oSize = ucAddAnswerform.Size;
                ucAddAnswerform.Dock = DockStyle.Fill;

                popupDialog.Text = "Select Answer Form";
                popupDialog.Controls.Add(ucAddAnswerform);
                popupDialog.ClientSize = new Size(oSize.Width + 10, oSize.Height + 10);
                popupDialog.ShowDialog(this.ParentForm);
            }
            Cursor.Current = currentCursor;
        }

        private void simpleButtonRemove_Click(object sender, EventArgs e) {
            GridView view = gridViewDialog;
            if (view == null || view.SelectedRowsCount == 0) return;

            DataRow[] rows = new DataRow[view.SelectedRowsCount];
            for (int i = 0; i < view.SelectedRowsCount; i++)
                rows[i] = view.GetDataRow(view.GetSelectedRows()[i]);

            view.BeginSort();
            try {
                foreach (DataRow row in rows)
                    row.Delete();
            } finally {
                view.EndSort();
            }

        }

        private void simpleButtonShowDialog_Click(object sender, EventArgs e) {

            GridView view = gridViewDialog;
            if (view == null || view.RowCount == 0) return;

            DataRow row;
            CampaignQuestionnaire oQuestionnaire = null;

            layoutControlGroupQuestionnaire.BeginUpdate();
            layoutControlGroupQuestionnaire.BeginInit();
            DisposeGroupControls(layoutControlGroupQuestionnaire);

            for (int i = 0; i < view.RowCount; i++) {
                row = view.GetDataRow(i);
                if (row["content_json"] == null || string.IsNullOrEmpty(row["content_json"].ToString())) continue;
                oQuestionnaire = CampaignQuestionnaire.InstanciateWith(row["content_json"].ToString());
                if (oQuestionnaire == null) continue;
                if (row["question_text"] != null)
                    oQuestionnaire.Form.Settings.QuestionText = row["question_text"].ToString();
                if (row["question_help_text"] != null)
                    oQuestionnaire.Form.Settings.QuestionHelp = row["question_help_text"].ToString();
                switch (oQuestionnaire.Type.ToLower()) {
                    case QuestionTypeConstants.Dropbox:
                        Dropbox oDropbox = new Dropbox(this.layoutControlQuestionnaire);
                        oDropbox.DisableSelection = true;
                        oDropbox.ToolTipController = defaultToolTipController1;
                        oDropbox.Questionnaire = oQuestionnaire;
                        oDropbox.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(oDropbox.ControlGroup);
                        break;
                    case QuestionTypeConstants.MultipleChoice:
                        Multiplechoice oMultipleChoice = new Multiplechoice(this.layoutControlQuestionnaire);
                        oMultipleChoice.DisableSelection = true;
                        oMultipleChoice.ToolTipController = defaultToolTipController1;
                        oMultipleChoice.Questionnaire = oQuestionnaire;
                        oMultipleChoice.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(oMultipleChoice.ControlGroup);
                        break;
                    case QuestionTypeConstants.Textbox:
                        Textbox oTextbox = new Textbox(this.layoutControlQuestionnaire);
                        oTextbox.DisableSelection = true;
                        oTextbox.ToolTipController = defaultToolTipController1;
                        oTextbox.Questionnaire = oQuestionnaire;
                        oTextbox.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(oTextbox.ControlGroup);
                        break;
                    case QuestionTypeConstants.Schedule:
                        Schedule oSchedule = new Schedule(this.layoutControlQuestionnaire);
                        oSchedule.DisableSelection = true;
                        oSchedule.ToolTipController = defaultToolTipController1;
                        oSchedule.Questionnaire = oQuestionnaire;
                        oSchedule.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(oSchedule.ControlGroup);
                        break;
                    //case QuestionTypeConstants.Seminar:
                    //    Seminar oSeminar = new Seminar(this.layoutControlQuestionnaire);
                    //    oSeminar.DisableSelection = true;
                    //    oSeminar.ToolTipController = defaultToolTipController1;
                    //    oSeminar.Questionnaire = oQuestionnaire;
                    //    oSeminar.BindControls();
                    //    this.layoutControlGroupQuestionnaire.Add(oSeminar.ControlGroup);
                    //    break;
                }
            }
            DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            emptySpaceItem1.CustomizationFormText = "emptySpaceItemBottom";
            emptySpaceItem1.Name = "emptySpaceItemBottom";
            emptySpaceItem1.ShowInCustomizationForm = false;
            emptySpaceItem1.Size = new System.Drawing.Size(0, 0);
            emptySpaceItem1.Text = "emptySpaceItemBottom";
            emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);

            //add bottom spacer
            this.layoutControlGroupQuestionnaire.AddItem(emptySpaceItem1);
            this.layoutControlGroupQuestionnaire.EndInit();
            this.layoutControlGroupQuestionnaire.EndUpdate();

        }

        private void DisposeGroupControls(BaseLayoutItem Item) {
            if (Item == null) return;
            var FlatList = new DevExpress.XtraLayout.Helpers.FlatItemsList();
            List<BaseLayoutItem> Items = FlatList.GetItemsList(Item);
            ILayoutControl Layout = Item.Owner;
            if (Layout != null) {
                Layout.BeginUpdate();
                BaseLayoutItem li;
                for (int i = Items.Count - 1; i >= 0; --i) {
                    li = Items[i];
                    if (!li.Equals(Item) && li.Name != Item.Name) {
                        if (li is LayoutControlItem) {
                            Control TempControl = (li as LayoutControlItem).Control;
                            if (TempControl != null) {
                                TempControl.Dispose();
                            }
                            li.Dispose();
                            Items.RemoveAt(i);
                        } else {
                            li.Dispose();
                            Items.RemoveAt(i);
                        }
                    }
                }
                Layout.EndUpdate();
            }
        }

        private void simpleButtonClear_Click(object sender, EventArgs e) {
            DisposeGroupControls(layoutControlGroupQuestionnaire);
        }

        private void simpleButtonSaveDialog_Click(object sender, EventArgs e) {
            if (!dxValidationProvider1.Validate()) return;

            Cursor currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            GridView view = gridViewDialog;
            if (view == null || view.RowCount == 0) return;

            DataRow row;
            CampaignQuestionnaire oQuestionnaire = null;
            JArray jaDiag = new JArray();
            JObject joDiag = null;
            for (int i = 0; i < view.RowCount; i++) {
                row = view.GetDataRow(i);
                if (row["content_json"] == null || string.IsNullOrEmpty(row["content_json"].ToString())) continue;
                oQuestionnaire = CampaignQuestionnaire.InstanciateWith(row["content_json"].ToString());
                if (oQuestionnaire == null) continue;
                if (row["question_text"] != null)
                    oQuestionnaire.Form.Settings.QuestionText = row["question_text"].ToString();
                if (row["question_help_text"] != null)
                    oQuestionnaire.Form.Settings.QuestionHelp = row["question_help_text"].ToString();
                if (row["question_id"] != null)
                    oQuestionnaire.Form.Settings.DataBindings.question_id = row["question_id"].ToString();
                if (row["question_text_language_id"] != null)
                    oQuestionnaire.Form.Settings.DataBindings.questions_text_language_id = row["question_text_language_id"].ToString();
                if (row["answer_form_id"] != null)
                    oQuestionnaire.Form.Settings.DataBindings.questionlayout_id = row["answer_form_id"].ToString();
                if (row["answer_form_language_id"] != null)
                    oQuestionnaire.Form.Settings.DataBindings.questionlayout_id = row["answer_form_language_id"].ToString();
                joDiag = JObject.Parse(oQuestionnaire.ToJSONString());
                jaDiag.Add(joDiag);
            }
            string dialog_text_json = jaDiag.ToString(Formatting.None);
            
            if (!isSaved) {
                m_oDialog = new dialog();                
            } else {
                if (m_oDialog == null) {
                    if (SubCampaignDialog != null && SubCampaignDialog.DialogID > 0) {
                        m_oDialog = BPContext.dialogs.FirstOrDefault(q => q.id == SubCampaignDialog.DialogID);
                    }
                }
            }

            m_oDialog.subcampaign_id = Convert.ToInt32(lookUpEditSource.EditValue);
            m_oDialog.name = textEditDialogname.Text.Trim();
            m_oDialog.dialog_text_json = dialog_text_json;
            m_oDialog.created_date = DateTime.Now;

            if (!isSaved) {
                BPContext.dialogs.AddObject(m_oDialog);
                isSaved = true;
            }

            BPContext.SaveChanges();

            if (SubCampaignDialog == null) {
                SubCampaignDialog = new CTSubCampaignDialogs() {
                    DialogID = m_oDialog.id,
                    DateCreated = m_oDialog.created_date,
                    DialogName = m_oDialog.name,
                    SubCampaignID = m_oDialog.subcampaign_id,
                    SubCampaignTitle = m_oDialog.subcampaign.title
                };
            }
            Cursor.Current = currentCursor;
            MessageBox.Show("Dialog has been saved.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SetValidationRules() {
            CustomIsBlankValidationRule isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The answerform name cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(textEditDialogname, isBlankValidationRule);
            isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The language cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(lookUpEditSource, isBlankValidationRule);
        } 
        #endregion

    }
}
