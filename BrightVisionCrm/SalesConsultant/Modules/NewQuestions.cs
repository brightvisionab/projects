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
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using BrightVision.Model;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;

using SalesConsultant.Business;
using SalesConsultant.Forms;

namespace SalesConsultant.Modules
{
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
        private bool m_OnEditMode = false;
        #endregion

        #region Contructor
        public NewQuestions() {
            m_OnEditMode = false;
            Initialize();
        }
        public NewQuestions(int questionid) {
            QuestionID = questionid;
            m_OnEditMode = true;
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
            ucAddQuestion = new AddQuestion(QuestionID);
            oQuestionSize = ucAddQuestion.Size;
            ucAddQuestion.Dock = DockStyle.Fill;
            ucAddQuestion.QuestionGrid = gridViewQuestion;
            ucAddTag = new AddTag(BPContext);
            oTagSize = ucAddTag.Size;
            ucAddTag.Dock = DockStyle.Fill;
            ucAddTag.TagsGrid = gridViewTags;            
            BindGridViewQuestion();            
            BindLanguage();            
            BindGridViewTags();            
            BindGridAnswerForm();
            SetValidationRules();
            groupControlAnswerForm.Enabled = false;
            lcgAnswerForm.Enabled = false;

            if (QuestionID > 0)
                LoadQuestion(QuestionID);
            else {
                btnEditQuestion.Enabled = false;
                btnDeleteQuestion.Enabled = false;
            }

            repositoryItemButtonEditAnswerOptions.Click += new EventHandler(repositoryItemButtonEditAnswerOptions_ButtonClick);
            propertyGridControl1.DefaultEditors.Add(typeof(List<AnswerOption>), repositoryItemButtonEditAnswerOptions);

            this.Visible = true;
        }

        private void LoadQuestion(int questionid) {
            IsQLSave = true; IsSaved = true;

            #region Load Questions
            DataTable dt = gridControlQuestions.DataSource as DataTable;
            m_oQuestion = BPContext.questions.Include("questions_text_language").FirstOrDefault(p => p.id == questionid);
            if (m_oQuestion == null) {
                return;
            }
            btnEditQuestion.Enabled = true;
            btnDeleteQuestion.Enabled = true;

            if (m_oQuestion.general_value != null && !string.IsNullOrEmpty(m_oQuestion.general_value.ToString()))
                comboBoxEditQuestionGeneralValue.EditValue = m_oQuestion.general_value.ToString();
            var qtl = m_oQuestion.questions_text_language.Where(p => p.MGC == false);

            DataRow dr = null;
            qtl.ForEach(delegate(questions_text_language dqtl) {
                dr = dt.NewRow();
                dr["question_id"] = dqtl.questions_id;
                dr["question_text_language_id"] = dqtl.id;
                dr["language_id"] = dqtl.language_id;
                dr["question"] = dqtl.question_text;
                dr["description"] = dqtl.question_description;
                dr["helptext"] = dqtl.question_help_text;
                dt.Rows.Add(dr);
            });
            dt.AcceptChanges();

            dictionarySelectedTags = new Dictionary<string, int>();
            var qqt = m_oQuestion.questions_questiontags;
            questiontag qt = null;
            KeyValuePair<string, int> ItemToAdd = new KeyValuePair<string, int>(); //[@jeff 09.15.2011 #462]: added
            qqt.ForEach(delegate(questions_questiontags dqts) {
                qt = dqts.questiontag;
                //[@jeff 09.15.2011 #462]: check key val pair if exists in the dictionary, before insert
                ItemToAdd = new KeyValuePair<string, int>(qt.title, qt.id);
                if (!dictionarySelectedTags.ContainsKey(qt.title))
                    dictionarySelectedTags.Add(qt.title, qt.id);
            });

            simpleButtonSaveQuestion.Enabled = true;
            groupControlAnswerForm.Enabled = true; 
            #endregion

            #region Load Answer forms
            dt = gridControlAnswerForm.DataSource as DataTable;
            m_oQuestionLayouts = BPContext.questionlayouts.Include("questionlayout_language").Where(p => p.questions_id == questionid);
            if (m_oQuestionLayouts != null && m_oQuestionLayouts.Count() > 0) {
                btnEditAnswerform.Enabled = true;
                btnDeleteAnswerform.Enabled = true;
            } else {
                btnEditAnswerform.Enabled = false;
                btnDeleteAnswerform.Enabled = false;
            }
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
                dr["question_id"] = ql.questions_id;
                dr["question_layout_id"] = ql.id;
                dr["title"] = ql.title;
                dr["component_type"] = componentType;
                dr["general_value"] = ql.general_value;
                dr["account_level"] = ql.account_level;
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
            if (dt.Rows.Count <= 0) {
                btnEditAnswerform.Enabled = false;
                btnDeleteAnswerform.Enabled = false;
            }
            #endregion
        }

        #endregion

        #region Public Methods
        public void ForceCellValueChange() {
            propertyGridControl1_CellValueChanged(null, null);
        }

        //public ManageQuestions ParentControl { get; set; }
        //public ProgressBackgroundWork BackgroundWorker { get; set; }   

        #endregion

        #region DataBinding
        private void BindGridViewQuestion() {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("question_id"));
            dt.Columns.Add(new DataColumn("language_id"));
            dt.Columns.Add(new DataColumn("question_text_language_id"));            
            dt.Columns.Add(new DataColumn("question"));
            dt.Columns.Add(new DataColumn("helptext"));
            dt.Columns.Add(new DataColumn("description"));
            gridControlQuestions.DataSource = dt;
        }

        private void BindGridAnswerForm() {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("question_id"));
            dt.Columns.Add(new DataColumn("question_layout_id"));
            dt.Columns.Add(new DataColumn("title"));
            dt.Columns.Add(new DataColumn("language"));
            dt.Columns.Add(new DataColumn("language_id", System.Type.GetType("System.Int32")));
            dt.Columns.Add(new DataColumn("component_type"));
            dt.Columns.Add(new DataColumn("account_level"));
            dt.Columns.Add(new DataColumn("general_value"));
            dt.Columns.Add(new DataColumn("properties", typeof(CampaignQuestionnaire)));            
            gridControlAnswerForm.DataSource = dt;
            eSaveMode = SaveMode.Unspecified;
        }

        private void BindGridViewTags(int pQuestionTagId = 0) 
        {
            gridControlTags.DataSource = null;
            gridControlTags.ToolTipController = defaultToolTipController1.DefaultController;
            gridControlTags.DataSource = BPContext.questiontags.ToList();
            if (pQuestionTagId > 0 && gridViewTags.RowCount > 0) {
                for (int i = 0; i < gridViewTags.RowCount; i++) {
                    if (Convert.ToInt32(gridViewTags.GetRowCellValue(i, "id")) == pQuestionTagId) {
                        gridViewTags.ClearSelection();
                        gridViewTags.FocusedRowHandle = i;
                        gridViewTags.SelectRow(i);
                        break;
                    }
                }
            }
            //if (gridControlTags.DataSource == null) {
            //    var datasource = BPContext.questiontags.ToList();
            //    gridControlTags.DataSource = datasource;
            //}
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

        private bool CanDeleteSelectedTag()
        {
            GridView objRowView = (GridView) gridControlTags.FocusedView;
            questiontag Item = (questiontag) objRowView.GetFocusedRow();
            if (BPContext.questions_questiontags.FirstOrDefault(i => i.questiontags_id == Item.id) == null)
                return true;
            else
                return false;
        }
        #endregion

        #region Question Methods

        private void gridViewQuestion_DoubleClick(object sender, EventArgs e) {
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                view.FocusedRowHandle = hitInfo.RowHandle;
                btnEditQuestion_Click(null, null);
            }
        }

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

        private void gridViewQuestion_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e) {
            var obj = gridViewQuestion.GetFocusedRow();
            if (obj != null) {
                var campQuestion = propertyGridControl1.SelectedObject as CampaignQuestionnaire ;
                if (campQuestion != null) {
                    DataRowView drRowView = (DataRowView)obj;
                    campQuestion.Form.Settings.QuestionText = drRowView.Row["question"].ToString();
                    campQuestion.Form.Settings.QuestionHelp = drRowView.Row["helptext"].ToString();
                    ForceCellValueChange();
                }
            }
        }

        private void btnEditQuestion_Click(object sender, EventArgs e) {
            dialog = new PopupDialog();
            dialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            dialog.MinimizeBox = false;
            dialog.MaximizeBox = false;
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.Text = "Edit Question";
            ucAddQuestion = new AddQuestion(gridViewQuestion,QuestionID);
            ucAddQuestion.EditMode = true;
            ucAddQuestion.Dock = DockStyle.Fill;
            dialog.Controls.Add(ucAddQuestion);
            dialog.ClientSize = new Size(oQuestionSize.Width + 10, oQuestionSize.Height + 10);
            dialog.ShowDialog(this.ParentForm);
        }

        private List<int> dataRowIds = new List<int>();

        private void btnDeleteQuestion_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Are you sure you want to delete this question?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes) {
                //do delete
                var gridview = gridViewQuestion;
                var obj = gridview.GetFocusedRow();
                if (obj != null) {
                    DataRowView drRowView = (DataRowView) obj;
                    int qtlid;
                    if (int.TryParse(drRowView.Row["question_text_language_id"].ToString(), out qtlid)) {
                        dataRowIds.Add(qtlid);                        
                    }
                    gridview.DeleteRow(gridview.FocusedRowHandle);
                    if (gridview.RowCount <= 0) {
                        btnDeleteQuestion.Enabled = false;
                        btnEditQuestion.Enabled = false;
                    }
                }
            }
        }

        private void btnAddQuestion_Click(object sender, EventArgs e) {
            dialog = new PopupDialog();
            dialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            dialog.MinimizeBox = false;
            dialog.MaximizeBox = false;
            dialog.StartPosition = FormStartPosition.CenterScreen;

            dialog.Text = "Add New Question";
            ucAddQuestion = new AddQuestion(gridViewQuestion, QuestionID);
            ucAddQuestion.EditMode = false;
            ucAddQuestion.Dock = DockStyle.Fill;
            dialog.Controls.Add(ucAddQuestion);
            dialog.ClientSize = new Size(oQuestionSize.Width + 10, oQuestionSize.Height + 10);
            if (dialog.ShowDialog(this.ParentForm) == DialogResult.OK) {
                btnEditQuestion.Enabled = true;
                btnDeleteQuestion.Enabled = true;
            }
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
                m_oQuestion.created_by_user_id = UserSession.CurrentUser.UserId;                
            } else {
                if (m_oQuestion == null) {
                    m_oQuestion = BPContext.questions.Include("questions_text_language").FirstOrDefault(p => p.id == QuestionID);                    
                }
            }
            
            m_oQuestion.modified_by = UserSession.CurrentUser.UserId;
            m_oQuestion.modified_date = DateTime.Now;
            if (comboBoxEditQuestionGeneralValue.EditValue != null && !string.IsNullOrEmpty(comboBoxEditQuestionGeneralValue.EditValue.ToString()))
                m_oQuestion.general_value = Convert.ToByte(comboBoxEditQuestionGeneralValue.EditValue);
            
            DataRowView rowView = null;
            questions_text_language qtextlang;
            int question_id, question_text_language_id;
            bool exists = false;

            if (gridViewQuestion.RowCount > 0) {
                for (int x = 0; x < rowcount; ++x) {
                    rowView = (DataRowView)gridViewQuestion.GetRow(x);
                    if (rowView == null)
                        continue;

                    if (int.TryParse(rowView.Row["question_id"].ToString(), out question_id) &&
                       int.TryParse(rowView.Row["question_text_language_id"].ToString(), out question_text_language_id)) {
                        qtextlang = m_oQuestion.questions_text_language.FirstOrDefault(p => p.questions_id == question_id && p.id == question_text_language_id);
                        exists = true;
                    } else {
                        qtextlang = new questions_text_language();
                        exists = false;
                    }
                    qtextlang.question_text = rowView.Row["question"].ToString();
                    qtextlang.question_description = rowView.Row["description"].ToString();
                    qtextlang.question_help_text = rowView.Row["helptext"].ToString();
                    qtextlang.language_id = Convert.ToInt32(rowView.Row["language_id"]);
                    if (!exists) {
                        m_oQuestion.questions_text_language.Add(qtextlang);
                    }
                }
            } else {
                BPContext.FIDeleteQuestion(string.Join(",", dataRowIds.ToArray()));
                if (this.ParentForm != null) {
                    this.ParentForm.DialogResult = DialogResult.OK;
                }
                return;
            }
            //clean up non existent questions in the collection
            //m_oQuestion.questions_text_language.ToList().ForEach(delegate(questions_text_language qtl) {
            //    if (dataRowIds.Contains(qtl.id)) {
            //        BPContext.questions_text_language.DeleteObject(qtl);
            //    }
            //});
            BPContext.FIDeleteQuestion(string.Join(",", dataRowIds.ToArray()));
            dataRowIds.Clear();
            m_oQuestion.questions_questiontags.ToList().ForEach(delegate(questions_questiontags _qqt)
            {
                    BPContext.questions_questiontags.DeleteObject(_qqt);
            });
            //var listqqts = m_oQuestion.questions_questiontags.ToList().Clone();
            //List<int> listQQTIds = new List<int>();
            questions_questiontags mQQT = null;
            if (dictionarySelectedTags != null) {
                foreach (KeyValuePair<string, int> kvp in dictionarySelectedTags) {
                    //mQQT = listqqts.FirstOrDefault(p => p.questiontags_id == kvp.Value);
                    //if (mQQT == null) {
                        m_oQuestion.questions_questiontags.Add(new questions_questiontags() { questiontags_id = kvp.Value });                        
                    //}
                    //listQQTIds.Add(kvp.Value);
                }                
                //m_oQuestion.questions_questiontags.ToList().ForEach(delegate(questions_questiontags _qqt) {
                //    if (!listQQTIds.Contains(_qqt.questiontags_id))
                //        BPContext.questions_questiontags.DeleteObject(_qqt);
                //});
            }
            if (!IsSaved) {
                BPContext.questions.AddObject(m_oQuestion);
                IsSaved = true;
            }
            BPContext.SaveChanges();
            //ParentControl.QuestionId = m_oQuestion.id;
            dt.Rows.Clear();
            DataRow dr = null;
            m_oQuestion.questions_text_language.ForEach(delegate(questions_text_language dqtl) {
                dr = dt.NewRow();
                dr["question_id"] = dqtl.questions_id;
                dr["question_text_language_id"] = dqtl.id;
                dr["language_id"] = dqtl.language_id;
                dr["question"] = dqtl.question_text;
                dr["description"] = dqtl.question_description;
                dr["helptext"] = dqtl.question_help_text;
                dt.Rows.Add(dr);
            });
            dt.AcceptChanges();

            if (QuestionID <= 0)
                QuestionID = m_oQuestion.id;

            groupControlAnswerForm.Enabled = true;
            simpleButtonSaveQuestion.Enabled = true;
            if (gridViewAnswerForm.RowCount > 0) {
                if(eSaveMode != SaveMode.Edit)
                    btnEditAnswerform.Enabled = true;
                btnDeleteAnswerform.Enabled = true;
            } else {
                btnEditAnswerform.Enabled = false;
                btnDeleteAnswerform.Enabled = false;
            }
            Cursor.Current = currentCursor;
            //if (sender != null && sender.ToString() != "skip")
            //    MessageBox.Show("Questions has been saved.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //if (BackgroundWorker != null) BackgroundWorker.StopWork();
        }
        #endregion

        #region Tag Methods
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
                if (view.RowCount <= 0) {
                    btnEditTag.Enabled = false;
                    btnDeleteTag.Enabled = false;
                } else {
                    btnEditTag.Enabled = true;
                    btnDeleteTag.Enabled = true;
                }
                return;
            }

            if (view == null || view.SelectedRowsCount == 0) return;

            if (view.SelectedRowsCount > 1) btnEditTag.Enabled = false;
            else btnEditTag.Enabled = true;

            questiontag[] rows = new questiontag[view.SelectedRowsCount];
            for (int i = 0; i < view.SelectedRowsCount; i++) {
                rows[i] = view.GetRow(view.GetSelectedRows()[i]) as questiontag;
            }

            view.BeginSort();
            dictionarySelectedTags = new Dictionary<string, int>();
            try
            {
                //[@jeff 09.27.2011]: http://brightvision.jira.com/browse/PLATFORM-541
                KeyValuePair<string, int> iTag;
                foreach (questiontag row in rows)
                    if (row != null)
                    {
                        iTag = new KeyValuePair<string, int>(row.title, row.id);
                        if (!dictionarySelectedTags.Contains(iTag) && !dictionarySelectedTags.Keys.Contains(row.title))
                            dictionarySelectedTags.Add(row.title, row.id);
                    }
            }
            finally
            {
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
        private void gridViewTags_DoubleClick(object sender, EventArgs e)
        {
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow)
            {
                view.FocusedRowHandle = hitInfo.RowHandle;
                btnEditTag_Click(null, null);
            }
        }

        private void btnAddTag_Click(object sender, EventArgs e) {
            dialog = new PopupDialog();
            dialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            dialog.MinimizeBox = false;
            dialog.MaximizeBox = false;
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.Text = "Add New Tag";
            ucAddTag = new AddTag(gridViewTags, BPContext);
            ucAddTag.AfterSave += new AddTag.AfterSaveEventHandler(ucAddTag_AfterSave);
            ucAddTag.EditMode = false;
            dialog.Controls.Add(ucAddTag);
            dialog.ClientSize = new Size(oTagSize.Width + 10, oTagSize.Height + 10);
            //dialog.FormClosing += new FormClosingEventHandler(AddTag_FormClosing);
            dialog.ShowDialog(this.ParentForm);
        }
        private void btnEditTag_Click(object sender, EventArgs e) {
            if (gridViewTags.SelectedRowsCount <= 0) {
                MessageBox.Show("Please select one or more tags first.", "Edit Tag", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            dialog = new PopupDialog();
            dialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            dialog.MinimizeBox = false;
            dialog.MaximizeBox = false;
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.Text = "Edit Tag";
            ucAddTag = new AddTag(gridViewTags,BPContext);
            ucAddTag.AfterSave += new AddTag.AfterSaveEventHandler(ucAddTag_AfterSave);
            ucAddTag.EditMode = true;
            dialog.Controls.Add(ucAddTag);
            dialog.ClientSize = new Size(oTagSize.Width + 10, oTagSize.Height + 10);
            dialog.ShowDialog(this.ParentForm);
            //todo disable edit if multiple selection
        }
        private void ucAddTag_AfterSave(object sender, AddTag.AddTagArgs e)
        {
            btnEditTag.Enabled = true;
            btnDeleteTag.Enabled = true;
            this.BindGridViewTags(e.QuestionTagId);
            ucAddTag.AfterSave -= new AddTag.AfterSaveEventHandler(ucAddTag_AfterSave);
        }
        private void btnDeleteTag_Click(object sender, EventArgs e) 
        {
            if (gridViewTags.SelectedRowsCount <= 0) {
                MessageBox.Show("Please select one or more tags first.", "Delete Tag", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //[@jeff 09.15.2011 #461,490]: check if being used from questions_questiontags FK
            if (!this.CanDeleteSelectedTag())
            {
                MessageBox.Show("This record cannot be deleted. It is in use by other data.", "Delete Tag", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete the selected tag(s)?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes) return;
            
            int[] rows = gridViewTags.GetSelectedRows();
            if (rows != null && rows.Length > 0)
            {
                //[@jeff 09.19.2011]: remove from dictionarySelectedTags
                foreach (int i in rows)
                {
                    if (i > 0)
                    {
                        questiontag ItemToRemove = (questiontag)(gridControlTags.FocusedView as ColumnView).GetRow(i);
                        if (ItemToRemove != null)
                            dictionarySelectedTags.Remove(ItemToRemove.title);
                    }
                }

                //(gridControlTags.FocusedView as ColumnView).DeleteSelectedRows();
                try
                {
                    bool _HasToUpdate = false;
                    int[] rowIndexes = (gridControlTags.FocusedView as ColumnView).GetSelectedRows();
                    foreach (int index in rowIndexes)
                    {
                        if (index > 0)
                        {
                            var tag = gridControlTags.FocusedView.GetRow(index);
                            BPContext.questiontags.DeleteObject((questiontag)tag);
                            _HasToUpdate = true;
                        }
                    }
                    if (_HasToUpdate)
                    {
                        BPContext.SaveChanges();
                        var datasource = BPContext.questiontags.ToList();
                        gridControlTags.DataSource = datasource;
                    }
                }
                catch { }
                if (gridViewTags.RowCount <= 0)
                {
                    btnDeleteTag.Enabled = false;
                    btnEditTag.Enabled = false;
                }
            }
        }

        //private void AddTag_FormClosing(object sender, EventArgs e) {
        //    var objSender = sender as PopupDialog;
        //    if (objSender != null && objSender.DialogResult == DialogResult.OK) {
        //        btnEditTag.Enabled = true;
        //        btnDeleteTag.Enabled = true;
        //    }
        //}
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

        private void RemoveControlError() {
            Control[] oControls = new Control[] { 
                textEditAnswerFormName, lookUpEditLanguage, comboBoxEditComponentType };
            oControls.ForEach(p => dxValidationProvider1.RemoveControlError(p));   
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
                if(oQuestionnaire.Form.Settings.Label == "[Sample label]")
                    oQuestionnaire.Form.Settings.Label = "";
            } else {
                oQuestionnaire = CampaignQuestionnaire.Instanciate(questionTypeConstants);
                oQuestionnaire.Form.Settings.Label = "";
            }
            int langId = 0;
            var obj = gridViewQuestion.GetFocusedRow();
            if (obj != null) {
                DataRowView drRowView = (DataRowView)obj;
                oQuestionnaire.Form.Settings.QuestionText = drRowView.Row["question"].ToString();
                oQuestionnaire.Form.Settings.QuestionHelp = drRowView.Row["helptext"].ToString();
                var langitem = lookUpEditLanguage.EditValue;
                if (langitem != null) {
                    langId = int.Parse(langitem.ToString());                    
                    var lang = BPContext.languages.FirstOrDefault(x => x.id == langId);
                    if (lang != null)
                        oQuestionnaire.Form.Settings.DataBindings.language_code = lang.code;
                }               
            }
            var binding = oQuestionnaire.Form.Settings.DataBindings;
            if (string.IsNullOrEmpty(binding.language_code))
                binding.language_code = "SE";

            binding.account_level = (bool)rgLevel.EditValue;

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
                //oSchedule.Questionnaire.Form.Settings.DataBindings.account_id = "1";
                oSchedule.BindControls();
                this.layoutControlGroupPreview.Add(oSchedule.ControlGroup);
            }
            else if (questionTypeConstants == QuestionTypeConstants.SmartText) {
                SmartText oSmartText = new SmartText(this.layoutControlPreview);
                oSmartText.DisableSelection = true;
                oSmartText.ToolTipController = defaultToolTipController1;
                oSmartText.Questionnaire = oQuestionnaire;
                oSmartText.BindPropertyGrid();
                oSmartText.BindSampleDataTable();
                this.layoutControlGroupPreview.Add(oSmartText.ControlGroup);
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
        private void lookUpEditLanguage_EditValueChanged(object sender, EventArgs e) {
            object selected = comboBoxEditComponentType.SelectedItem;
            if (selected != null) {
                if (!string.IsNullOrEmpty(selected.ToString())) {
                    BindPropertyGrid(selected.ToString().ToLower());
                }
            }
        }

        private void rgLevel_SelectedIndexChanged(object sender, EventArgs e) {
            var edit = sender as RadioGroup;
            bool isAccountLevel = (bool)edit.EditValue;
            bool oldValue = (bool)edit.OldEditValue;
            string comType = (string)comboBoxEditComponentType.EditValue;
            if (isAccountLevel) {
                if (!oldValue && (comType != null && comType.ToString() == "Schedule")) {
                    if (MessageBox.Show("Schedule component is not allowed to be used in Account Level. " +
                    "Would you like to discard the changes on current edited component and use account level option anyway?",
                    "System Info", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) ==
                        DialogResult.OK) {
                        comboBoxEditComponentType.SelectedIndex = -1;
                        comboBoxEditComponentType.Properties.Items.Clear();
                        comboBoxEditComponentType.Properties.Items.AddRange(new string[] { "Dropbox", "Textbox", "MultipleChoice","SmartText" });
                        propertyGridControl1.SelectedObject = null;
                        DisposeGroupControls(this.layoutControlGroupPreview);
                        layoutControlPreview.Refresh();
                    } else {
                        edit.EditValue = false;
                        edit.SelectedIndex = 0;
                    }
                } else {
                    comboBoxEditComponentType.Properties.Items.RemoveAt(3);//remove schedule component
                }
            } else {
                comboBoxEditComponentType.Properties.Items.Clear();
                comboBoxEditComponentType.Properties.Items.AddRange(new string[] { "Dropbox", "Textbox", "MultipleChoice", "Schedule", "SmartText" });
            }

            object selected = comboBoxEditComponentType.SelectedItem;
            if (selected != null) {
                if (!string.IsNullOrEmpty(selected.ToString())) {
                    BindPropertyGrid(selected.ToString().ToLower());
                }
            }
        }

        private void repositoryItemButtonEditAnswerOptions_ButtonClick(object sender, EventArgs e) {            
            var oQuestionnaire = propertyGridControl1.SelectedObject as CampaignQuestionnaire;
            editorDiag = new EditorDialogAnswerOptions(oQuestionnaire, propertyGridControl1);
            editorDiag.Sender = this;
            editorDiag.StartPosition = FormStartPosition.CenterParent;
            if (editorDiag.ShowDialog() == DialogResult.OK) {
                propertyGridControl1.CloseEditor();
            } else {
                propertyGridControl1.CloseEditor();
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
                case QuestionTypeConstants.SmartText:
                    SmartText oSmartText = new SmartText(this.layoutControlPreview);
                    oSmartText.DisableSelection = true;
                    oSmartText.ToolTipController = defaultToolTipController1;
                    oSmartText.Questionnaire = oQuestionnaire;
                    oSmartText.BindPropertyGrid();
                    oSmartText.BindSampleDataTable();
                    this.layoutControlGroupPreview.Add(oSmartText.ControlGroup);
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

        private void simpleButtonSaveAnswerForm_Click(object sender, EventArgs e) {

            if (!dxValidationProvider1.Validate()) {
                return;
            }

            Cursor currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            DataView dtView = gridViewAnswerForm.DataSource as DataView;
            CampaignQuestionnaire oQuestionnaire = propertyGridControl1.SelectedObject as CampaignQuestionnaire;
            oQuestionnaire.Form.Settings.QuestionText = "";
            oQuestionnaire.Form.Settings.QuestionHelp = "";
            oQuestionnaire.Form.Settings.DataBindings.account_level = (bool)rgLevel.EditValue;

            if (dtView != null && dtView.Table != null) {                
                m_oQuestion = BPContext.questions.FirstOrDefault(p => p.id == QuestionID);

                DataTable dtAF = dtView.Table;
                DataRow dr;
                questionlayout qlayout;
                questionlayout_language qlang;
                object general = null;          

                if (eSaveMode == SaveMode.Add) {
                    qlayout = new questionlayout();
                    qlang = new questionlayout_language();
                    qlayout.created_by = UserSession.CurrentUser.UserId;
                    
                    dr = dtAF.NewRow();                 
                } else {
                    dr = dtAF.Rows[gridViewAnswerForm.FocusedRowHandle];
                    if (dr == null) return;

                    int qlid = Convert.ToInt32(dr["question_layout_id"]);
                    qlayout = BPContext.questionlayouts
                                .Include("questionlayout_language")
                                .FirstOrDefault(p => p.id == qlid);                    
                    qlang = qlayout.questionlayout_language.FirstOrDefault();
                    qlayout.modified_by = UserSession.CurrentUser.UserId;                    
                }
                qlayout.modified_date = DateTime.Now;
                qlayout.default_setting = false;
                qlayout.title = textEditAnswerFormName.EditValue.ToString();
                qlayout.content_json = oQuestionnaire.ToJSONString();
                qlayout.account_level = (bool)rgLevel.EditValue;
                general = comboBoxEditAnswerFormGeneralValue.SelectedItem;
                qlayout.general_value = general != null && !string.IsNullOrEmpty(general.ToString()) ? byte.Parse(general.ToString()) : byte.Parse("0");
                qlang.language_id = int.Parse(lookUpEditLanguage.EditValue.ToString());

                if (eSaveMode == SaveMode.Add) {
                    qlayout.questionlayout_language.Add(qlang);
                    m_oQuestion.questionlayouts.Add(qlayout);
                }
                BPContext.SaveChanges();
                IsQLSave = true;

                dr["question_id"] = QuestionID;   
                dr["question_layout_id"] = qlayout.id;
                dr["language"] = lookUpEditLanguage.GetColumnValue("name");
                dr["language_id"] = lookUpEditLanguage.EditValue;
                dr["title"] = textEditAnswerFormName.EditValue;
                dr["component_type"] = comboBoxEditComponentType.SelectedItem;
                dr["general_value"] = comboBoxEditAnswerFormGeneralValue.SelectedItem;
                dr["account_level"] = (bool)rgLevel.EditValue;
                dr["properties"] = oQuestionnaire;
                if (eSaveMode == SaveMode.Add)
                    dtAF.Rows.Add(dr);
                dtAF.AcceptChanges();
                gridControlAnswerForm.DataSource = dtAF;
            }
            lcgAnswerForm.Enabled = false;            
            btnEditAnswerform.Enabled = true;
            btnDeleteAnswerform.Enabled = true;
            btnAddAnswerform.Enabled = true;
            eSaveMode = SaveMode.Unspecified;

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
            if (sender != null && sender.ToString() != "skip")
                MessageBox.Show("Answer forms has been saved.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //if (this.ParentForm != null) {
            //    this.ParentForm.DialogResult = DialogResult.OK;
            //}
        }

        private void simpleButtonDoneQuestion_Click(object sender, EventArgs e) {
            simpleButtonSaveQuestion_Click("skip", null);
            if (AfterSave != null)
                AfterSave(this, new EditQuestionArgs() { OnEditMode = m_OnEditMode, QuestionId = m_oQuestion.id });
            this.ParentForm.Close();
            //if (this.ParentForm != null) {
            //    this.ParentForm.DialogResult = DialogResult.OK;
            //}
        }

        private void simpleButtonDoneAnswerform_Click(object sender, EventArgs e) {
            simpleButtonSaveAnswerForm_Click("skip", null);
            if (this.ParentForm != null) {
                this.ParentForm.DialogResult = DialogResult.OK;
            }
        }

        private void btnAddAnswerform_Click(object sender, EventArgs e) {
            if (eSaveMode == SaveMode.Edit) {
                if (MessageBox.Show("Would you like to discard the current edit answer form operation?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    != DialogResult.Yes) return;
            }
            RemoveControlError();
            btnAddAnswerform.Enabled = false;
            if (gridViewAnswerForm.RowCount > 0) 
                btnEditAnswerform.Enabled = true;            
            lcgAnswerForm.Enabled = true;
            lcgAnswerForm.Text = "Add Answer Form";
            textEditAnswerFormName.Text = "";
            comboBoxEditComponentType.SelectedIndex = -1;
            lookUpEditLanguage.ItemIndex = -1;
            comboBoxEditAnswerFormGeneralValue.SelectedIndex = -1;
            propertyGridControl1.SelectedObject = null;            
            DisposeGroupControls(this.layoutControlGroupPreview);
            layoutControlPreview.Refresh();            
            eSaveMode = SaveMode.Add;
            textEditAnswerFormName.Focus();
        }

        private void btnEditAnswerform_Click(object sender, EventArgs e) {
            if (eSaveMode == SaveMode.Add) {
                if (MessageBox.Show("Would you like to discard the current add answer form operation?", "Confirm", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    != DialogResult.Yes) return;
            }
            RemoveControlError();
            btnAddAnswerform.Enabled = true;
            btnEditAnswerform.Enabled = false;            
            lcgAnswerForm.Enabled = true;
            var dataRowView = (gridControlAnswerForm.FocusedView as ColumnView).GetRow(gridViewAnswerForm.FocusedRowHandle) as DataRowView;
            if (dataRowView != null && dataRowView.DataView != null) {
                DataTable dtTable = dataRowView.DataView.Table;
                if (dtTable != null && dtTable.Rows.Count > 0) {
                    DataRow dataRow = dtTable.Rows[gridViewAnswerForm.FocusedRowHandle];
                    if (dataRow != null) {
                        lcgAnswerForm.Text = "Edit Selected Answer Form:";
                        textEditAnswerFormName.EditValue = (string)dataRow["title"];
                        lookUpEditLanguage.EditValue = Convert.ToInt32(dataRow["language_id"]);
                        comboBoxEditComponentType.EditValue = (string)dataRow["component_type"];
                        if (dataRow["general_value"] != DBNull.Value)
                            comboBoxEditAnswerFormGeneralValue.EditValue = (string)dataRow["general_value"];
                        rgLevel.EditValue = bool.Parse(dataRow["account_level"].ToString());
                        var campQuestion = dataRow["properties"] as CampaignQuestionnaire;
                        var obj = gridViewQuestion.GetFocusedRow();
                        if (obj != null) {
                            DataRowView drRowView = (DataRowView)obj;
                            campQuestion.Form.Settings.QuestionText = drRowView.Row["question"].ToString();
                            campQuestion.Form.Settings.QuestionHelp = drRowView.Row["helptext"].ToString();
                        }
                        propertyGridControl1.SelectedObject = campQuestion;
                        ForceCellValueChange();
                        layoutControlPreview.Refresh();
                        eSaveMode = SaveMode.Edit;
                        textEditAnswerFormName.Focus(); 
                    }
                }
            }
        }

        private void btnDeleteAnswerform_Click(object sender, EventArgs e) {

            //[@jeff 09.15.2011 #463]: bypass if no records found
            if (gridViewAnswerForm.RowCount < 1) {                
                return;
            }
            if (MessageBox.Show("Are you sure you want to delete the selected row?", "Confirm",
                   MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            RemoveControlError();
            lcgAnswerForm.Enabled = false;            
            DataView dtView = gridViewAnswerForm.DataSource as DataView;            
            DataTable dtAF = dtView.Table;
            DataRow dr = dtAF.Rows[gridViewAnswerForm.FocusedRowHandle];
            if (dr == null) return;
            int qlid = Convert.ToInt32(dr["question_layout_id"]);
            questionlayout qlayout = BPContext.questionlayouts
                                .Include("questionlayout_language")
                                .FirstOrDefault(p => p.id == qlid);
            questionlayout_language qlang = qlayout.questionlayout_language.FirstOrDefault();

            if (qlayout != null) {
                if (qlang != null) {
                    BPContext.questionlayout_language.DeleteObject(qlang);
                }
                BPContext.questionlayouts.DeleteObject(qlayout);
                BPContext.SaveChanges();                
                dr.Delete();
                dr.AcceptChanges();
                dtAF.AcceptChanges();                
            }
            //gridViewAnswerForm.DeleteRow(gridViewAnswerForm.FocusedRowHandle);            
            if (gridViewAnswerForm.RowCount <= 0) {
                btnDeleteAnswerform.Enabled = false;
                btnEditAnswerform.Enabled = false;
            } else {                
                btnEditAnswerform.Enabled = true;
                btnDeleteAnswerform.Enabled = true;
            }
            btnAddAnswerform.Enabled = true;
            textEditAnswerFormName.Text = "";
            comboBoxEditComponentType.SelectedIndex = -1;
            lookUpEditLanguage.ItemIndex = -1;
            comboBoxEditAnswerFormGeneralValue.SelectedIndex = -1;
            propertyGridControl1.SelectedObject = null;
            eSaveMode = SaveMode.Delete;
            DisposeGroupControls(this.layoutControlGroupPreview);
            layoutControlPreview.Refresh();
        }

        private void btnCancelEdit_Click(object sender, EventArgs e) {
            lcgAnswerForm.Enabled = false;
            lcgAnswerForm.Text = "Add/Edit Answer Form";
            textEditAnswerFormName.Text = "";
            comboBoxEditComponentType.SelectedIndex = -1;
            lookUpEditLanguage.ItemIndex = -1;
            comboBoxEditAnswerFormGeneralValue.SelectedIndex = -1;
            propertyGridControl1.SelectedObject = null;
            eSaveMode = SaveMode.Unspecified;
            DisposeGroupControls(this.layoutControlGroupPreview);
            layoutControlPreview.Refresh();
            if (gridViewAnswerForm.RowCount <= 0) {
                btnDeleteAnswerform.Enabled = false;
                btnEditAnswerform.Enabled = false;
            } else {               
                btnEditAnswerform.Enabled = true;
                btnDeleteAnswerform.Enabled = true;
            }
            btnAddAnswerform.Enabled = true;
            RemoveControlError();
        }

        private void gridViewAnswerForm_DoubleClick(object sender, EventArgs e) {
            if (eSaveMode == SaveMode.Edit) return;
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                view.FocusedRowHandle = hitInfo.RowHandle;
                btnEditAnswerform_Click(null, null);
            }
        }

        private void gridViewAnswerForm_MouseDown(object sender, MouseEventArgs e) {
            if(eSaveMode == SaveMode.Edit || eSaveMode == SaveMode.Add)
                (e as DevExpress.Utils.DXMouseEventArgs).Handled = true;
        }

        private void gridViewAnswerForm_MouseUp(object sender, MouseEventArgs e) {
            if (eSaveMode == SaveMode.Edit || eSaveMode == SaveMode.Add)
                (e as DevExpress.Utils.DXMouseEventArgs).Handled = true;
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

        #region Grid ContextMenu
        private void gridViewQuestion_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gridViewTags_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gridViewAnswerForm_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion

        #region Public Events & Arguments
        public delegate void AfterSaveEventHandler(object sender, EditQuestionArgs e);
        public event AfterSaveEventHandler AfterSave;
        public class EditQuestionArgs: EventArgs
        {
            public bool OnEditMode { get; set; } 
            public int QuestionId { get; set; }
        }
        #endregion
    }
}
