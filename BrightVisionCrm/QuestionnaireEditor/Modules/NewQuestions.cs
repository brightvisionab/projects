using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
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

using QuestionnaireEditor.Business;

namespace QuestionnaireEditor.Modules {
    public partial class NewQuestions : DevExpress.XtraEditors.XtraUserControl {
        #region Member Variables
        private PopupDialog dialog = null;
        private AddQuestion ucAddQuestion = null;
        private AddTag ucAddTag = null;
        private BrightPlatformEntities BPContext = null;
        private Size oQuestionSize, oTagSize;
        private Dictionary<string, int> dictionarySelectedTags = null;
        private SaveMode eSaveMode = SaveMode.Unspecified;
        private bool IsSaved = false, IsQLSave = false;
        private string currentType = string.Empty;
        private EditorDialogAnswerOptions editorDiag = null;
        private bool IsLoaded = false;
        private question m_oQuestion = null;
        private IQueryable<questionlayout> m_oQuestionLayouts = null;
        #endregion

        #region Contructor
        public NewQuestions() {
            Initialize();
        }
        public NewQuestions(int questionid) {
            QuestionID = questionid;
            Initialize();
        }

        public int QuestionID {
            get;
            set;
        }

        #endregion

        #region Private Methods
        private void Initialize() {
            this.Visible = false;
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            ucAddQuestion = new AddQuestion();
            oQuestionSize = ucAddQuestion.Size;
            ucAddQuestion.Dock = DockStyle.Fill;
            ucAddQuestion.QuestionGrid = gridViewQuestion;
            ucAddTag = new AddTag();
            oTagSize = ucAddTag.Size;
            ucAddTag.Dock = DockStyle.Fill;
            ucAddTag.TagsGrid = gridControlTags;
            WaitDialog.SetWaitDialogCaption("Loading questions...");
            BindGridViewQuestion();
            WaitDialog.SetWaitDialogCaption("Loading language...");
            BindLanguage();
            WaitDialog.SetWaitDialogCaption("Loading tags...");
            BindGridViewTags();
            WaitDialog.SetWaitDialogCaption("Loading answer form...");
            BindGridAnswerForm();
            SetValidationRules();
            if (QuestionID > 0)
                LoadQuestion(QuestionID);
            WaitDialog.CloseWaitDialog();
            this.Visible = true;
        }
        private void LoadQuestion(int questionid) {
            IsQLSave = true; IsSaved = true;            
            DataTable dt = gridControlQuestions.DataSource as DataTable;
            m_oQuestion = BPContext.questions.Include("questions_text_language").FirstOrDefault(p => p.id == questionid);

            if (m_oQuestion.general_value != null && !string.IsNullOrEmpty(m_oQuestion.general_value.ToString()))
                comboBoxEditQuestionGeneralValue.SelectedText = m_oQuestion.general_value.ToString();
            var qtl = m_oQuestion.questions_text_language;

            DataRow dr = null;
            qtl.ForEach(delegate(questions_text_language dqtl) {
                dr = dt.NewRow();
                dr["language"] = dqtl.language_id;
                dr["question"] = dqtl.question_text;
                dr["description"] = dqtl.question_description;
                dr["helptext"] = dqtl.question_help_text;
                dt.Rows.Add(dr);
            });
            dt.AcceptChanges();

            dictionarySelectedTags = new Dictionary<string, int>();
            var qqt = m_oQuestion.questions_questiontags;
            questiontag qt = null;
            qqt.ForEach(delegate(questions_questiontags dqts) {
                qt = dqts.questiontag;
                dictionarySelectedTags.Add(qt.title, qt.id);
            });

            groupControlAnswerForm.Enabled = true;
            simpleButtonSaveQuestion.Enabled = true;

            dt = gridControlAnswerForm.DataSource as DataTable;
            m_oQuestionLayouts = BPContext.questionlayouts.Include("questionlayout_language").Where(p => p.questions_id == questionid);            
            CampaignQuestionnaire cQ = null;
            var cbeItems = comboBoxEditComponentType.Properties.Items;
            string componentType = string.Empty;
            questionlayout_language qlang = null;
            m_oQuestionLayouts.ForEach(delegate(questionlayout ql) {                
                cQ = CampaignQuestionnaire.InstanciateWith(ql.content_json);
                for (int x = 0; x < cbeItems.Count; ++x) {
                    if (cbeItems[x].ToString().ToLower() == cQ.Type.ToLower())
                        componentType = cbeItems[x].ToString();
                }
                dr = dt.NewRow();
                dr["title"] = ql.title;
                dr["component_type"] = componentType;                
                dr["general_value"] = ql.general_value;
                dr["properties"] = cQ;
                qlang = ql.questionlayout_language.FirstOrDefault();
                if (qlang != null) {
                    dr["language"] = qlang.language.name;
                    dr["language_id"] = qlang.language_id;
                } else {
                    dr["language"] = null;
                    dr["language_id"] = null;
                }
                dt.Rows.Add(dr);
            });
            dt.AcceptChanges();
        }

        #endregion

        #region Public Methods
        public void ForceCellValueChange() {
            propertyGridControl1_CellValueChanged(null, null);
        }

        //public ProgressBackgroundWork BackgroundWorker { get; set; }   

        #endregion

        #region DataBinding
        private void BindGridViewQuestion() {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("language"));
            dt.Columns.Add(new DataColumn("question"));
            dt.Columns.Add(new DataColumn("helptext"));
            dt.Columns.Add(new DataColumn("description"));
            gridControlQuestions.DataSource = dt;
        }

        private void BindGridAnswerForm() {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("title"));
            dt.Columns.Add(new DataColumn("language"));
            dt.Columns.Add(new DataColumn("language_id", System.Type.GetType("System.Int32")));
            dt.Columns.Add(new DataColumn("component_type"));
            dt.Columns.Add(new DataColumn("general_value"));
            dt.Columns.Add(new DataColumn("properties", typeof(CampaignQuestionnaire)));
            gridControlAnswerForm.DataSource = dt;
            eSaveMode = SaveMode.Add;
        }

        private void BindGridViewTags() {
            gridControlTags.ToolTipController = defaultToolTipController1.DefaultController;
            if (gridControlTags.DataSource == null) {
                var datasource = BPContext.questiontags.Execute(MergeOption.AppendOnly);
                gridControlTags.DataSource = datasource;
            }
        }

        private void BindLanguage() {
            var datasource = BPContext.languages.Execute(MergeOption.AppendOnly);
            this.repositoryItemLookUpEditLanguage.DataSource = datasource;
            this.repositoryItemLookUpEditLanguage.DisplayMember = "name";
            this.repositoryItemLookUpEditLanguage.ValueMember = "id";
            this.repositoryItemLookUpEditLanguage.Columns.Add(new LookUpColumnInfo("name", "Language"));
            this.repositoryItemLookUpEditLanguage.ValidateOnEnterKey = true;

            //this.repositoryItemLookUpEditAnswerFormLanguage.DataSource = datasource;
            //this.repositoryItemLookUpEditAnswerFormLanguage.DisplayMember = "name";
            //this.repositoryItemLookUpEditAnswerFormLanguage.ValueMember = "id";
            //this.repositoryItemLookUpEditAnswerFormLanguage.Columns.Add(new LookUpColumnInfo("name", "Language"));
            //this.repositoryItemLookUpEditAnswerFormLanguage.ValidateOnEnterKey = true;

            this.lookUpEditLanguage.Properties.DataSource = datasource;
            this.lookUpEditLanguage.Properties.DisplayMember = "name";
            this.lookUpEditLanguage.Properties.ValueMember = "id";
            this.lookUpEditLanguage.Properties.Columns.Add(new LookUpColumnInfo("name", "Language"));
            this.lookUpEditLanguage.Properties.ValidateOnEnterKey = true;
        }
        #endregion

        #region Question Methods
        private void gridViewQuestion_ValidateRow(object sender, ValidateRowEventArgs e) {
            ColumnView colView = sender as ColumnView;
            GridColumn column1 = colView.Columns[0];
            GridColumn column2 = colView.Columns[1];
            object language = colView.GetRowCellValue(e.RowHandle, column1);
            object questiontext = colView.GetRowCellDisplayText(e.RowHandle, column2);
            if (language == DBNull.Value) {
                e.Valid = false;
                colView.SetColumnError(column1, "Language is required.");
            }
            if (questiontext == DBNull.Value || string.IsNullOrEmpty(questiontext.ToString())) {
                e.Valid = false;
                colView.SetColumnError(column2, "Question is required.");
            }
        }

        private void gridViewQuestion_RowUpdated(object sender, RowObjectEventArgs e) {
            if (e != null && e.Row != null) {
                var view = e.Row as DataRowView;
                if (view.DataView != null && view.DataView.Count > 0) {
                    simpleButtonSaveQuestion.Enabled = true;
                } else {
                    groupControlAnswerForm.Enabled = false;
                    simpleButtonSaveQuestion.Enabled = true;
                }
            }
        }

        private void gridViewQuestion_InvalidRowException(object sender, InvalidRowExceptionEventArgs e) {
            //Suppress displaying the error message box
            e.ExceptionMode = ExceptionMode.NoAction;
        }

        private void simpleButtonAddQuestion_Click(object sender, EventArgs e) {
            dialog = new PopupDialog();
            dialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            dialog.MinimizeBox = false;
            dialog.MaximizeBox = false;
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.Text = "Add New Question";
            dialog.Controls.Add(ucAddQuestion);
            dialog.ClientSize = new Size(oQuestionSize.Width + 10, oQuestionSize.Height + 10);
            dialog.ShowDialog(this.ParentForm);
        }

        private void simpleButtonSaveQuestion_Click(object sender, EventArgs e) {
            //if (BackgroundWorker != null)
            //    BackgroundWorker.StartWork("Saving Question");
            Cursor currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            int rowcount = 0;
            
            DataTable dt = gridControlQuestions.DataSource as DataTable;
            rowcount = dt.Rows.Count;
            if (!IsSaved) {
                m_oQuestion = new question();
            } else {
                if (m_oQuestion == null) {
                    m_oQuestion = BPContext.questions.Include("questions_text_language").FirstOrDefault(p => p.id == QuestionID);
                }
            }

            m_oQuestion.modified_date = DateTime.Now;
            if (!string.IsNullOrEmpty(comboBoxEditQuestionGeneralValue.SelectedText))
                m_oQuestion.general_value = Convert.ToByte(comboBoxEditQuestionGeneralValue.SelectedText);

            if (IsSaved) {
                //m_oQuestion exisiting questions
                m_oQuestion.questions_text_language.ToList().ForEach(qt => BPContext.questions_text_language.DeleteObject(qt));
            }

            DataRowView rowView = null;
            questions_text_language qtextlang;
            for (int x = 0; x < rowcount; ++x) {
                rowView = (DataRowView)gridViewQuestion.GetRow(x);
                qtextlang = new questions_text_language();
                qtextlang.language_id = int.Parse(rowView.Row["language"].ToString());
                qtextlang.question_text = rowView.Row["question"].ToString();
                qtextlang.question_description = rowView.Row["description"].ToString();
                qtextlang.question_help_text = rowView.Row["helptext"].ToString();
                m_oQuestion.questions_text_language.Add(qtextlang);
            }
            if (IsSaved) {
                //delete existing tags in this question
                m_oQuestion.questions_questiontags.ToList().ForEach(qt => BPContext.questions_questiontags.DeleteObject(qt));
            }
            if (dictionarySelectedTags != null) {
                foreach (KeyValuePair<string, int> kvp in dictionarySelectedTags) {
                    m_oQuestion.questions_questiontags.Add(
                            new questions_questiontags() {
                                questiontags_id = kvp.Value
                            }
                        );

                }
            }
            if (!IsSaved) {
                BPContext.questions.AddObject(m_oQuestion);
                IsSaved = true;
            }
            BPContext.SaveChanges();
            if (QuestionID <= 0)
                QuestionID = m_oQuestion.id;
            
            groupControlAnswerForm.Enabled = true;
            simpleButtonSaveQuestion.Enabled = true;

            Cursor.Current = currentCursor;
            MessageBox.Show("Questions has been saved.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //if (BackgroundWorker != null) BackgroundWorker.StopWork();
        }
        #endregion

        #region Tag Methods
        private void gridViewTags_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e) {
            var questiontag = e.Row as questiontag;
            //add validation
            if (string.IsNullOrEmpty(questiontag.title)) return;
            BPContext.SaveChanges();
        }

        private void gridControlTags_EmbeddedNavigator_ButtonClick(object sender, NavigatorButtonClickEventArgs e) {
            if (e.Button.ButtonType == NavigatorButtonType.Remove) {
                if (MessageBox.Show("Do you want to delete the current row?", "Confirm deletion",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes) {
                    (gridControlTags.FocusedView as ColumnView).DeleteRow(gridViewTags.FocusedRowHandle);
                    BPContext.SaveChanges();
                }
                e.Handled = true;
            }
        }

        private void gridViewTags_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e) {
            ColumnView colView = sender as ColumnView;
            GridColumn column1 = colView.Columns[0];
            string tagname = (string)colView.GetRowCellValue(e.RowHandle, column1);
            if (string.IsNullOrEmpty(tagname)) {
                e.Valid = false;
                colView.SetColumnError(column1, "Tag name is required.");
            }
        }

        private void gridViewTags_InvalidRowException(object sender, InvalidRowExceptionEventArgs e) {
            //Suppress displaying the error message box
            e.ExceptionMode = ExceptionMode.NoAction;
        }
       
        private bool Cleared = false;
        private void gridViewTags_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e) {
            GridView view = gridViewTags;                        
            if (!IsLoaded) {
                if (!Cleared) {
                    if (view != null && view.SelectedRowsCount > 0) {
                        Cleared = true;
                        view.ClearSelection();
                    }
                }
                if (dictionarySelectedTags != null) {
                    KeyValuePair<string, int> kvTag;
                    questiontag qt;
                    for (int i = 0; i < view.RowCount; i++) {
                        qt = view.GetRow(i) as questiontag;
                        if (qt != null) {
                            kvTag = dictionarySelectedTags.FirstOrDefault(d => d.Value == qt.id);
                            if (kvTag.Key != null && kvTag.Value != 0) {
                                if (!view.IsRowSelected(i)) {
                                    view.SelectRow(i);
                                    view.FocusedRowHandle = i;
                                }
                            }
                        }
                    }
                    textEditSelectedTags.Text = string.Join(",", dictionarySelectedTags.Keys.ToArray());
                }
                IsLoaded = true;
                return;
            }

            if (view == null || view.SelectedRowsCount == 0) return;

            questiontag[] rows = new questiontag[view.SelectedRowsCount];
            for (int i = 0; i < view.SelectedRowsCount; i++) {
                rows[i] = view.GetRow(view.GetSelectedRows()[i]) as questiontag;
            }

            view.BeginSort();
            dictionarySelectedTags = new Dictionary<string, int>();
            try {
                foreach (questiontag row in rows) {
                    if (row != null) {
                        dictionarySelectedTags.Add(row.title, row.id);
                    }
                }
            } finally {
                view.EndSort();
            }
            textEditSelectedTags.Text = string.Join(",", dictionarySelectedTags.Keys.ToArray());
        }

        private void gridViewTags_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e) {
            if (e.FocusedRowHandle <= 0) {
                textEditSelectedTags.Text = string.Empty;
                gridViewTags.ClearSelection();
            }
        }

        private void simpleButtonAddNewTag_Click(object sender, EventArgs e) {
            dialog = new PopupDialog();
            dialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            dialog.MinimizeBox = false;
            dialog.MaximizeBox = false;
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.Text = "Add New Tag";
            dialog.Controls.Add(ucAddTag);
            dialog.ClientSize = new Size(oTagSize.Width + 10, oTagSize.Height + 10);
            dialog.ShowDialog(this.ParentForm);
        }

        #endregion

        #region Answer Form Methods
        private void SetValidationRules() {
            CustomIsBlankValidationRule isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The answerform name cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(textEditAnswerFormName, isBlankValidationRule);
            isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The language cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(lookUpEditLanguage, isBlankValidationRule);
            isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The component type cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(comboBoxEditComponentType, isBlankValidationRule);
        }

        private void BindPropertyGrid(string questionTypeConstants) {
            BindPropertyGrid(questionTypeConstants, null);
        }

        private void BindPropertyGrid(CampaignQuestionnaire Questionnaire) {
            BindPropertyGrid(null, Questionnaire);
        }

        private void BindPropertyGrid(string questionTypeConstants, CampaignQuestionnaire Questionnaire) {
            CampaignQuestionnaire oQuestionnaire = null;
            if (Questionnaire != null) {
                oQuestionnaire = Questionnaire;
            } else {
                oQuestionnaire = CampaignQuestionnaire.Instanciate(questionTypeConstants);
            }
            currentType = questionTypeConstants;
            this.layoutControlGroupPreview.BeginUpdate();
            //this.layoutControlGroupPreview.Clear();          
            DisposeGroupControls(this.layoutControlGroupPreview);
            if (questionTypeConstants == QuestionTypeConstants.Dropbox) {
                Dropbox oDropbox = new Dropbox(this.layoutControlPreview);
                oDropbox.DisableSelection = true;
                oDropbox.ToolTipController = defaultToolTipController1;
                oDropbox.Questionnaire = oQuestionnaire;
                oDropbox.BindControls();
                this.layoutControlGroupPreview.Add(oDropbox.ControlGroup);
            } else if (questionTypeConstants == QuestionTypeConstants.Textbox) {
                Textbox oTextbox = new Textbox(this.layoutControlPreview);
                oTextbox.DisableSelection = true;
                oTextbox.ToolTipController = defaultToolTipController1;
                oTextbox.Questionnaire = oQuestionnaire;
                oTextbox.BindControls();
                this.layoutControlGroupPreview.Add(oTextbox.ControlGroup);
            } else if (questionTypeConstants == QuestionTypeConstants.MultipleChoice) {
                Multiplechoice oMultipleChoice = new Multiplechoice(this.layoutControlPreview);
                oMultipleChoice.DisableSelection = true;
                oMultipleChoice.ToolTipController = defaultToolTipController1;
                oMultipleChoice.Questionnaire = oQuestionnaire;
                oMultipleChoice.BindControls();
                this.layoutControlGroupPreview.Add(oMultipleChoice.ControlGroup);
            } else if (questionTypeConstants == QuestionTypeConstants.Schedule) {
                Schedule oSchedule = new Schedule(this.layoutControlPreview);
                oSchedule.DisableSelection = true;
                oSchedule.ToolTipController = defaultToolTipController1;
                oSchedule.Questionnaire = oQuestionnaire;
                //TODO: Change this during the implementation of component user for account_id
                oSchedule.Questionnaire.Form.Settings.DataBindings.account_id = "1";
                oSchedule.BindControls();
                this.layoutControlGroupPreview.Add(oSchedule.ControlGroup);
            } 
            //else if (questionTypeConstants == QuestionTypeConstants.Seminar) {
            //    Seminar oSeminar = new Seminar(this.layoutControlPreview);
            //    oSeminar.DisableSelection = true;
            //    oSeminar.ToolTipController = defaultToolTipController1;
            //    oSeminar.Questionnaire = oQuestionnaire;
            //    //TODO: Change this during the implementation of component user for account_id
            //    oSeminar.Questionnaire.Form.Settings.DataBindings.account_id = "1";
            //    oSeminar.BindControls();
            //    this.layoutControlGroupPreview.Add(oSeminar.ControlGroup);
            //}
            DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            emptySpaceItem1.CustomizationFormText = "emptySpaceItemBottom";
            emptySpaceItem1.Name = "emptySpaceItemBottom";
            emptySpaceItem1.ShowInCustomizationForm = false;
            emptySpaceItem1.Size = new System.Drawing.Size(0, 0);
            emptySpaceItem1.Text = "emptySpaceItemBottom";
            emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);

            //add bottom spacer
            this.layoutControlGroupPreview.AddItem(emptySpaceItem1);
            this.layoutControlGroupPreview.EndUpdate();
            this.layoutControlPreview.BestFit();
            var answerOptionList = oQuestionnaire.Form.Settings.AnswerOptions;
            repositoryItemButtonEditAnswerOptions.Click += new EventHandler(repositoryItemButtonEditAnswerOptions_ButtonClick);
            propertyGridControl1.DefaultEditors.Add(typeof(List<AnswerOption>), repositoryItemButtonEditAnswerOptions);
            propertyGridControl1.SelectedObject = oQuestionnaire;
        }

        private void comboBoxEditComponentType_SelectedValueChanged(object sender, EventArgs e) {
            object selected = comboBoxEditComponentType.SelectedItem;
            if (selected != null) {
                if (!string.IsNullOrEmpty(selected.ToString())) {
                    BindPropertyGrid(selected.ToString().ToLower());
                }
            }
        }

        private void repositoryItemButtonEditAnswerOptions_ButtonClick(object sender, EventArgs e) {
            if (editorDiag != null) {
                if (editorDiag.DialogResult == DialogResult.OK || editorDiag.DialogResult == DialogResult.Cancel) {
                    propertyGridControl1.CloseEditor();
                    editorDiag.DialogResult = DialogResult.None;
                } else {
                    var oQuestionnaire = propertyGridControl1.SelectedObject as CampaignQuestionnaire;
                    editorDiag = new EditorDialogAnswerOptions(oQuestionnaire, propertyGridControl1);
                    editorDiag.Sender = this;
                    editorDiag.StartPosition = FormStartPosition.CenterParent;
                    editorDiag.ShowDialog();
                }

            } else {
                var oQuestionnaire = propertyGridControl1.SelectedObject as CampaignQuestionnaire;
                editorDiag = new EditorDialogAnswerOptions(oQuestionnaire, propertyGridControl1);
                editorDiag.Sender = this;
                editorDiag.StartPosition = FormStartPosition.CenterParent;
                editorDiag.ShowDialog();
            }
        }

        private void repositoryItemButtonEditAnswerOptions_CustomDisplayText(object sender, CustomDisplayTextEventArgs e) {
            e.DisplayText = "(AnswerOptionsCollection)";
        }

        private void repositoryItemImageComboBoxBackgroundColor_SelectedValueChanged(object sender, EventArgs e) {
            var option = propertyGridControl1.SelectedObject as CampaignQuestionnaire;
            option.Form.Settings.BackgroundColor = (sender as ComboBoxEdit).SelectedItem.ToString().ToLower();
            propertyGridControl1.SelectedObject = null;
            propertyGridControl1.SelectedObject = option;
            propertyGridControl1_CellValueChanged(null, null);
        }

        private void repositoryItemComboBoxCustomerOwnership_SelectedValueChanged(object sender, EventArgs e) {
            var selected = propertyGridControl1.SelectedObject as CampaignQuestionnaire;
            if (selected != null) {
                ComboBoxEdit boxedit = sender as ComboBoxEdit;
                if (boxedit != null && boxedit.EditValue != null) {
                    selected.Form.Settings.CustomerOwnership = bool.Parse(boxedit.EditValue.ToString().ToLower());
                    propertyGridControl1.SelectedObject = null;
                    propertyGridControl1.SelectedObject = selected;
                    propertyGridControl1_CellValueChanged(null, null);
                }
            }

        }

        private void repositoryItemComboBoxBVOwnership_SelectedValueChanged(object sender, EventArgs e) {
            var selected = propertyGridControl1.SelectedObject as CampaignQuestionnaire;
            if (selected != null) {
                ComboBoxEdit boxedit = sender as ComboBoxEdit;
                if (boxedit != null && boxedit.EditValue != null) {
                    selected.Form.Settings.BVOwnership = bool.Parse(boxedit.EditValue.ToString().ToLower());
                    propertyGridControl1.SelectedObject = null;
                    propertyGridControl1.SelectedObject = selected;
                    propertyGridControl1_CellValueChanged(null, null);
                }
            }
        }

        private void propertyGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e) {
            CampaignQuestionnaire oQuestionnaire = propertyGridControl1.SelectedObject as CampaignQuestionnaire;
            if (oQuestionnaire == null) return;
            propertyGridControl1.SelectedObject = null;
            propertyGridControl1.SelectedObject = oQuestionnaire;

            this.layoutControlGroupPreview.BeginUpdate();
            this.layoutControlGroupPreview.BeginInit();
            //this.layoutControlGroupPreview.Clear();            
            DisposeGroupControls(this.layoutControlGroupPreview);
            switch (oQuestionnaire.Type.ToLower()) {
                case QuestionTypeConstants.Dropbox:
                    Dropbox oDropbox = new Dropbox(this.layoutControlPreview);
                    oDropbox.DisableSelection = true;
                    oDropbox.ToolTipController = defaultToolTipController1;
                    oDropbox.Questionnaire = oQuestionnaire;
                    oDropbox.BindControls();
                    this.layoutControlGroupPreview.Add(oDropbox.ControlGroup);
                    break;
                case QuestionTypeConstants.MultipleChoice:
                    Multiplechoice oMultipleChoice = new Multiplechoice(this.layoutControlPreview);
                    oMultipleChoice.DisableSelection = true;
                    oMultipleChoice.Questionnaire = oQuestionnaire;
                    oMultipleChoice.BindControls();
                    this.layoutControlGroupPreview.Add(oMultipleChoice.ControlGroup);
                    break;
                case QuestionTypeConstants.Textbox:
                    Textbox oTextbox = new Textbox(this.layoutControlPreview);
                    oTextbox.DisableSelection = true;
                    oTextbox.Questionnaire = oQuestionnaire;
                    oTextbox.BindControls();
                    this.layoutControlGroupPreview.Add(oTextbox.ControlGroup);
                    break;
                case QuestionTypeConstants.Schedule:
                    Schedule oSchedule = new Schedule(this.layoutControlPreview);
                    oSchedule.DisableSelection = true;
                    oSchedule.Questionnaire = oQuestionnaire;
                    //TODO: Change this during the implementation of component user for account_id
                    oSchedule.Questionnaire.Form.Settings.DataBindings.account_id = "1";
                    oSchedule.BindControls();
                    this.layoutControlGroupPreview.Add(oSchedule.ControlGroup);
                    break;
                //case QuestionTypeConstants.Seminar:
                //    Seminar oSeminar = new Seminar(this.layoutControlPreview);
                //    oSeminar.DisableSelection = true;
                //    oSeminar.Questionnaire = oQuestionnaire;
                //    //TODO: Change this during the implementation of component user for account_id
                //    oSeminar.Questionnaire.Form.Settings.DataBindings.account_id = "1";
                //    oSeminar.BindControls();
                //    this.layoutControlGroupPreview.Add(oSeminar.ControlGroup);
                //    break;
            }
            DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            emptySpaceItem1.CustomizationFormText = "emptySpaceItemBottom";
            emptySpaceItem1.Name = "emptySpaceItemBottom";
            emptySpaceItem1.ShowInCustomizationForm = false;
            emptySpaceItem1.Size = new System.Drawing.Size(0, 0);
            emptySpaceItem1.Text = "emptySpaceItemBottom";
            emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);

            //add bottom spacer
            this.layoutControlGroupPreview.AddItem(emptySpaceItem1);
            this.layoutControlGroupPreview.EndInit();
            this.layoutControlGroupPreview.EndUpdate();
        }

        private void gridControlAnswerForm_EmbeddedNavigator_ButtonClick(object sender, NavigatorButtonClickEventArgs e) {
            if (e.Button.ButtonType == NavigatorButtonType.Remove) {
                if (MessageBox.Show("Do you want to delete the current row?", "Confirm Deletion",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes) {
                    (gridControlAnswerForm.FocusedView as ColumnView).DeleteRow(gridViewAnswerForm.FocusedRowHandle);
                    //BPContext.SaveChanges();
                    textEditAnswerFormName.Text = "";
                    comboBoxEditComponentType.SelectedIndex = -1;
                    lookUpEditLanguage.ItemIndex = -1;
                    comboBoxEditAnswerFormGeneralValue.SelectedIndex = -1;
                    propertyGridControl1.SelectedObject = null;
                    eSaveMode = SaveMode.Delete;
                    //layoutControlGroupPreview.Clear();
                    DisposeGroupControls(this.layoutControlGroupPreview);
                    layoutControlPreview.Refresh();
                    e.Handled = true;
                    textEditAnswerFormName.Focus();

                }
            } else if (e.Button.ButtonType == NavigatorButtonType.Append) {
                emptySpaceItemAnswerForm.Text = "Add Answer Form:";
                textEditAnswerFormName.Text = "";
                comboBoxEditComponentType.SelectedIndex = -1;
                lookUpEditLanguage.ItemIndex = -1;
                comboBoxEditAnswerFormGeneralValue.SelectedIndex = -1;
                propertyGridControl1.SelectedObject = null;
                //layoutControlGroupPreview.Clear();
                DisposeGroupControls(this.layoutControlGroupPreview);
                layoutControlPreview.Refresh();
                e.Handled = true;
                eSaveMode = SaveMode.Add;
                textEditAnswerFormName.Focus();
            } else if (e.Button.ButtonType == NavigatorButtonType.Edit) {
                var dataRowView = (gridControlAnswerForm.FocusedView as ColumnView).GetRow(gridViewAnswerForm.FocusedRowHandle) as DataRowView;
                if (dataRowView != null && dataRowView.DataView != null) {
                    DataTable dtTable = dataRowView.DataView.Table;
                    if (dtTable != null && dtTable.Rows.Count > 0) {
                        DataRow dataRow = dtTable.Rows[gridViewAnswerForm.FocusedRowHandle];
                        if (dataRow != null) {
                            emptySpaceItemAnswerForm.Text = "Edit Selected Answer Form:";
                            textEditAnswerFormName.EditValue = (string)dataRow["title"];
                            lookUpEditLanguage.EditValue = Convert.ToInt32(dataRow["language_id"]);
                            comboBoxEditComponentType.EditValue = (string)dataRow["component_type"];
                            if (dataRow["general_value"] != DBNull.Value)
                                comboBoxEditAnswerFormGeneralValue.EditValue = (string)dataRow["general_value"];
                            propertyGridControl1.SelectedObject = dataRow["properties"] as CampaignQuestionnaire;
                            ForceCellValueChange();
                            layoutControlPreview.Refresh();
                            e.Handled = true;
                            eSaveMode = SaveMode.Edit;
                            textEditAnswerFormName.Focus();
                        }
                    }
                }
            }
        }

        private void simpleButtonSaveAnswerForm_Click(object sender, EventArgs e) {
                        
            if (!dxValidationProvider1.Validate()) {
                return;                
            }

            Cursor currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            DataView dtView = gridViewAnswerForm.DataSource as DataView;
            CampaignQuestionnaire oQuestionnaire = propertyGridControl1.SelectedObject as CampaignQuestionnaire;            
            if (dtView != null && dtView.Table != null) {
                DataTable dtAF = dtView.Table as DataTable;
                DataRow dr;
                if (eSaveMode == SaveMode.Add) {
                    dr = dtAF.NewRow();
                } else {
                    dr = dtAF.Rows[gridViewAnswerForm.FocusedRowHandle];
                }
                dr["language"] = lookUpEditLanguage.GetColumnValue("name");
                dr["language_id"] = lookUpEditLanguage.EditValue;
                dr["title"] = textEditAnswerFormName.EditValue;
                dr["component_type"] = comboBoxEditComponentType.SelectedItem;
                dr["general_value"] = comboBoxEditAnswerFormGeneralValue.SelectedItem;
                dr["properties"] = oQuestionnaire;
                if (eSaveMode == SaveMode.Add)
                    dtAF.Rows.Add(dr);
                dtAF.AcceptChanges();
                gridControlAnswerForm.DataSource = dtAF;
            }

            int rowcount = 0;
            DataTable dt = gridControlAnswerForm.DataSource as DataTable;
            rowcount = dt.Rows.Count;            
            if(m_oQuestionLayouts == null)
                m_oQuestionLayouts = BPContext.questionlayouts.Include("questionlayout_language").Where(p => p.questions_id == QuestionID);
            if(m_oQuestion == null)
                m_oQuestion = BPContext.questions.FirstOrDefault(p => p.id == QuestionID);

            if (IsQLSave) {
                //delete existing questionlayout in this question
                m_oQuestionLayouts.ToList().ForEach(delegate(questionlayout qt) {
                    BPContext.questionlayout_language.DeleteObject(qt.questionlayout_language.FirstOrDefault());
                    BPContext.questionlayouts.DeleteObject(qt);
                });
            }

            DataRowView rowView = null;
            questionlayout qlayout;
            questionlayout_language qlang;
            object general = null;
            for (int x = 0; x < rowcount; ++x) {
                rowView = (DataRowView)gridViewAnswerForm.GetRow(x);
                qlayout = new questionlayout();
                qlayout.modified_date = DateTime.Now;
                qlayout.modified_by = 0;
                qlayout.created_by = 0;
                qlayout.default_setting = false;
                qlayout.title = rowView.Row["title"].ToString();
                qlayout.content_json = (rowView["properties"] as CampaignQuestionnaire).ToJSONString();
                general = rowView["general_value"];
                qlayout.general_value = general != null && !string.IsNullOrEmpty(general.ToString()) ? byte.Parse(general.ToString()) : byte.Parse("0");
                qlang = new questionlayout_language();
                qlang.language_id = int.Parse(rowView.Row["language_id"].ToString());
                qlayout.questionlayout_language.Add(qlang);
                m_oQuestion.questionlayouts.Add(qlayout);
            }
            BPContext.SaveChanges();
            IsQLSave = true;

            lookUpEditLanguage.ItemIndex = -1;
            textEditAnswerFormName.EditValue = "";
            comboBoxEditComponentType.SelectedIndex = -1;
            comboBoxEditAnswerFormGeneralValue.SelectedIndex = -1;
            propertyGridControl1.SelectedObject = null;
            //layoutControlGroupPreview.Clear();
            DisposeGroupControls(this.layoutControlGroupPreview);
            layoutControlPreview.Refresh();
            ForceCellValueChange();
            Cursor.Current = currentCursor;
            MessageBox.Show("Answer forms has been saved.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
        #endregion
       
    }
}

       