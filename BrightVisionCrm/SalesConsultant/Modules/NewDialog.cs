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
using BrightVision.Common.Utilities;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SalesConsultant.Business;
using SalesConsultant.Forms;
using BrightVision.Common.Business;

namespace SalesConsultant.Modules
{
    public partial class NewDialog : DevExpress.XtraEditors.XtraUserControl {

        #region Properties
        public ManageDialogs ParentController { get; set; }
        #endregion

        #region Member Variables
        private BrightPlatformEntities BPContext = null;
        private Dictionary<int, BindingSource> ChildList = null;
        private const string OrderFieldName = "position";
        private PopupDialog popupDialog = null;
        private bool isSaved = false;
        private dialog m_oDialog = null;
        private bool m_IsEdit = false;
        #endregion

        #region Constructor
        public NewDialog() {
            Initialize();
            m_IsEdit = false;
        }
        public NewDialog(CTSubCampaignDialogs subcampaignDialog) {
            SubCampaignDialog = subcampaignDialog;
            Initialize();
            m_IsEdit = true;
        }
        public CTSubCampaignDialogs SubCampaignDialog { get; set; }
        #endregion

        #region Private Methods
        private void Initialize() {
            this.Visible = false;
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);            
            PopulateCustomerComboList();                        
            BindGridQuestions();
            BindGridDialog();
            SetValidationRules();
            ChildList = new Dictionary<int, BindingSource>();
            if (SubCampaignDialog != null) {
                PopulateCampaignComboList(SubCampaignDialog.CustomerID);
                PopulateSubCampaignComboList(SubCampaignDialog.CustomerID, SubCampaignDialog.CampaignID);
                lookUpEditCampaign.EditValue = SubCampaignDialog.CampaignID;
                lookUpEditSubCampaign.EditValue = SubCampaignDialog.SubCampaignID;
                LoadDialog(SubCampaignDialog);
            }            
            this.Visible = true;
           WaitDialog.Close();
        }

        private void LoadDialog(CTSubCampaignDialogs subcampaignDialog) {
            isSaved = true;
            textEditDialogname.Text = subcampaignDialog.DialogName;
            lookUpEditCustomer.EditValue = subcampaignDialog.CustomerID;
            lookUpEditCampaign.EditValue = subcampaignDialog.CampaignID;
            lookUpEditSubCampaign.EditValue = subcampaignDialog.SubCampaignID;
            lookUpEditCustomer.Properties.ReadOnly = true;
            lookUpEditCampaign.Properties.ReadOnly = true;
            lookUpEditSubCampaign.Properties.ReadOnly = true;
            m_oDialog = BPContext.dialogs.FirstOrDefault(p => p.id == subcampaignDialog.DialogID);
            var CQList = new List<CampaignQuestionnaire>();
            var QLIDs = new List<int>();
            int qlid;
            if (!string.IsNullOrEmpty(m_oDialog.dialog_text_json)) {
                var jaDiag = JArray.Parse(m_oDialog.dialog_text_json);
                CampaignQuestionnaire CQ = null;                
                jaDiag.ForEach(delegate(JToken jt) {
                    /**
                     * https://brightvision.jira.com/browse/PLATFORM-3057
                     * fixed json text for unwanted characters
                     */
                    string _json = ValidationUtility.StripJsonInvalidChars(JsonConvert.ToString(jt.ToString(Formatting.None)).Unescape());
                    //CQ = CampaignQuestionnaire.InstanciateWith(jt.ToString(Formatting.None).Unescape());
                    CQ = CampaignQuestionnaire.InstanciateWith(_json);
                    if (CQ != null) {
                        if (CQ.Type.ToLower() == QuestionTypeConstants.Schedule)
                            btnCreateSchedule.Enabled = true;
                        if (int.TryParse(CQ.Form.Settings.DataBindings.questionlayout_id, out qlid))
                            QLIDs.Add(qlid);
                        CQList.Add(CQ);
                    }
                });
            }
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
            questionlayout_language questionLayoutLanguage = null;
            CTQuestionTags questiontags = null;
            DataRow newDr = null;
            var datasource = gridControlQuestions.DataSource as List<CTQuestionTags>;
            CQList.ForEach(delegate(CampaignQuestionnaire dcq) {
                if (int.TryParse(dcq.Form.Settings.DataBindings.question_id, out qid) &&
                    int.TryParse(dcq.Form.Settings.DataBindings.questions_text_language_id, out qtlid) &&
                    int.TryParse(dcq.Form.Settings.DataBindings.questionlayout_id, out qlid)) {
                    questionLayout = answerform.FirstOrDefault(p => p.questions_id == qid && p.id == qlid);
                    if (questionLayout != null) {
                        questionLayoutLanguage = questionLayout.questionlayout_language.FirstOrDefault(p => p.questionlayout_id == questionLayout.id);
                        if(questionLayoutLanguage != null)
                            questiontags = datasource.FirstOrDefault(x => x.question_id == qid && x.question_text_language_id == qtlid);
                    }
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
                    newDr["language_code"] = questiontags.language_code;

                    if (questionLayout != null) {
                        newDr["answer_form"] = questionLayout.title;
                        newDr["answer_form_id"] = questionLayout.id;
                        newDr["answer_form_language_id"] = questionLayoutLanguage.id;
                        newDr["content_json"] = questionLayout.content_json;
                        newDr["account_level"] = questionLayout.account_level;
                    }
                    dtDialog.Rows.Add(newDr);
                    dtDialog.AcceptChanges();
                    simpleButtonAssignAnswerForm.Enabled = true;
                }
            });           
            //show dialog preview
            simpleButtonShowDialog_Click(null, null);
        }

        //private void BindSourceLookup() {
        //    var datasource = BPContext.subcampaigns.Execute(MergeOption.AppendOnly).ToList();
        //    this.lookUpEditCustomer.Properties.DataSource = datasource;
        //    this.lookUpEditCustomer.Properties.DisplayMember = "title";
        //    this.lookUpEditCustomer.Properties.ValueMember = "id";
        //    this.lookUpEditCustomer.Properties.Columns.Add(new LookUpColumnInfo("title", "Sub campaign name"));
        //    this.lookUpEditCustomer.Properties.ShowHeader = false;
        //    this.lookUpEditCustomer.Properties.ValidateOnEnterKey = true;
        //}

        private void BindGridQuestions() {

            var objSource = BPContext.FIQuestionTags(null);
            if (gridControlQuestions.DataSource == null) {
                gridControlQuestions.DataSource = objSource.ToList();
                gridViewQuestion.Columns["question_id"].Group();
                gridViewQuestion.ExpandAllGroups();
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
            dtDialog.Columns.Add("language_code", typeof(string));
            dtDialog.Columns.Add("answer_form", typeof(string));
            dtDialog.Columns.Add("answer_form_id", typeof(int));
            dtDialog.Columns.Add("answer_form_language_id", typeof(int));
            dtDialog.Columns.Add("content_json", typeof(string));
            dtDialog.Columns.Add("account_level", typeof(bool));
            dtDialog.AcceptChanges();

            gridControlDialog.DataSource = dtDialog;
            gridViewDialog.Columns[OrderFieldName].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            gridViewDialog.OptionsCustomization.AllowSort = false;
        }

        private void gridViewQuestion_DoubleClick(object sender, EventArgs e) {
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                view.FocusedRowHandle = hitInfo.RowHandle;
                simpleButtonAddToDialog_Click(null, null);
            }
        }

        private void gridViewDialog_DoubleClick(object sender, EventArgs e) {
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                view.FocusedRowHandle = hitInfo.RowHandle;
                simpleButtonAssignAnswerForm_Click(null, null);
            }
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

        private void simpleButtonMoveDown_Click(object sender, EventArgs e) 
        {
            if (gridViewDialog.RowCount < 1)
                return;

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
                if (dtDialog != null && dtDialog.Rows.Count > 0) {                    
                    rowcount = Convert.ToInt32(dtDialog.Compute("max(position)", string.Empty));
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
                var ucAddAnswerform = new AddAnswerForm(questionid, gridViewDialog, btnCreateSchedule);
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
            string json = "";
            try {
                foreach (DataRow row in rows) {
                    try {
                        json = row["content_json"].ToString();
                        CampaignQuestionnaire ques = CampaignQuestionnaire.InstanciateWith(json);
                        if (ques.Type.ToLower() == QuestionTypeConstants.Schedule)
                            btnCreateSchedule.Enabled = false;
                    }catch {
                    }
                    row.Delete();
                    if (row.Table != null)
                        row.Table.AcceptChanges();
                }
            } finally {
                view.EndSort();
            }
           
            //reset index
            int counter = 1;
            var dtDialog = (view.DataSource as DataView).Table;            
            foreach (DataRow dr in dtDialog.Rows) {
                dr["position"] = counter;
                counter++;
            }
            dtDialog.AcceptChanges();
        }

        private void simpleButtonShowDialog_Click(object sender, EventArgs e) {
            ShowDialog();
        }

		private void ccbAccountLevel_EditValueChanged(object sender, EventArgs e) {
            var edit = sender as CheckedComboBoxEdit;
            var editVal = (string)edit.EditValue;
            if (!string.IsNullOrEmpty(editVal)) {
                string[] filters = editVal.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                ShowDialog(filters);
            }
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

        private void simpleButtonSaveDialog_Click(object sender, EventArgs e) 
        {            
            if (!dxValidationProvider1.Validate()) 
                return;

            string _msg = "Dialog already exist for sub-campaign " + lookUpEditSubCampaign.Text.ToUpper() + "." + Environment.NewLine
                        + "Only one(1) dialog per sub-campaign is allowed.";

            if (SubCampaignDialog == null) 
            {
                if (!ObjectDialog.CanAddDialog((int)lookUpEditSubCampaign.EditValue))
                {
                    MessageBox.Show(_msg, "Dialogs", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                else if (ObjectDialog.Exists(textEditDialogname.Text, (int)lookUpEditSubCampaign.EditValue) &&
                        (!m_IsEdit || (m_IsEdit && !SubCampaignDialog.DialogName.Equals(textEditDialogname.Text))))
                {
                    MessageBox.Show(_msg, "Dialogs", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
            }
            else if (SubCampaignDialog !=null && ObjectDialog.Exists(textEditDialogname.Text, (int)lookUpEditSubCampaign.EditValue, SubCampaignDialog.DialogID))
            {
                if (!m_IsEdit || (m_IsEdit && !SubCampaignDialog.DialogName.Equals(textEditDialogname.Text)))
                {
                    MessageBox.Show(_msg, "Dialogs", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
            }

            /**
             * applied so that when pressing save and then pressing done for new entries,
             * it should validate as edit mode when pressing done.
             */
            if (!m_IsEdit)
                m_IsEdit = true;

            var dtDialog = gridControlDialog.DataSource as DataTable;
            if (dtDialog != null && dtDialog.Rows.Count > 0) dtDialog.AcceptChanges();
            Cursor currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            string dialog_text_json = string.Empty;
            GridView view = gridViewDialog;
            if (view != null && view.RowCount > 0) {

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
                    if (row["language_code"] != null)
                        oQuestionnaire.Form.Settings.DataBindings.language_code = row["language_code"].ToString();

                    if (row["answer_form_id"] != null)
                        oQuestionnaire.Form.Settings.DataBindings.questionlayout_id = row["answer_form_id"].ToString();
                    if (row["answer_form_language_id"] != null)
                        oQuestionnaire.Form.Settings.DataBindings.questionlayout_language_id = row["answer_form_language_id"].ToString();
                    joDiag = JObject.Parse(oQuestionnaire.ToJSONString());
                    jaDiag.Add(joDiag);
                }
                dialog_text_json = jaDiag.ToString(Formatting.None);
            }
            if (!isSaved) {
                m_oDialog = new dialog();
                m_oDialog.created_date = DateTime.Now;
            } else {
                if (m_oDialog == null) {
                    if (SubCampaignDialog != null && SubCampaignDialog.DialogID > 0) {
                        m_oDialog = BPContext.dialogs.FirstOrDefault(q => q.id == SubCampaignDialog.DialogID);
                    }
                }
            }

            m_oDialog.subcampaign_id = Convert.ToInt32(lookUpEditSubCampaign.EditValue);
            m_oDialog.name = textEditDialogname.Text.Trim();
            m_oDialog.dialog_text_json = dialog_text_json;
            m_oDialog.created_by = UserSession.CurrentUser.UserId;
            m_oDialog.is_active = true;

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

            /**
             * lets set this so we can determine on the parent controller
             * which dialog is to be default selected
             */
            ParentController.DefaultSelectedDialogId = m_oDialog.id;

            Cursor.Current = currentCursor;
            if(sender != null && sender.ToString() != "skip")
                MessageBox.Show("Dialog has been saved.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //if (this.ParentForm != null) {
            //    this.ParentForm.DialogResult = DialogResult.OK;
            //}
        }

        private void simpleButtonDone_Click(object sender, EventArgs e) {
            if (!dxValidationProvider1.Validate()) return;
            simpleButtonSaveDialog.PerformClick();
            if (this.ParentForm != null) {
                this.ParentForm.DialogResult = DialogResult.OK;
            }
        }

        private void SetValidationRules() {
            CustomIsBlankValidationRule isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The dialog name cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(textEditDialogname, isBlankValidationRule);
            isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The customer cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(lookUpEditCustomer, isBlankValidationRule);
            isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The campaign cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(lookUpEditCampaign, isBlankValidationRule);
            isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The subcampaign cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(lookUpEditSubCampaign, isBlankValidationRule);
        }
        
        private void lookUpEditCustomer_EditValueChanged(object sender, EventArgs e) {
            if (lookUpEditCustomer.Text.Length > 0) {
                
                WaitDialog.Show(ParentForm, "Loading customer campaigns...");
                this.PopulateCampaignComboList((int)lookUpEditCustomer.EditValue);
                WaitDialog.Close();
            }
        }

        private void lookUpEditCampaign_EditValueChanged(object sender, EventArgs e) {
            if (lookUpEditCampaign.Text.Length > 0) {
                
                WaitDialog.Show(ParentForm, "Loading subcampaigns...");
                this.PopulateSubCampaignComboList((int)lookUpEditCustomer.EditValue, (int)lookUpEditCampaign.EditValue);
                WaitDialog.Close();

            }
        }


        private void ShowDialog(string[] sFilter = null) {
            int rowcount = 0;
            bool hasRows = gridViewDialog.RowCount > 0;

            bool isContactLevel = true;
            bool isAccountLevel = true;

            if (sFilter != null) {
                if (sFilter.Length == 1) {                    
                    isContactLevel = sFilter[0].Trim() == "contact";                    
                    isAccountLevel = sFilter[0].Trim() == "account";                    
                } else if (sFilter.Length > 1) {
                    isContactLevel = sFilter[0].Trim() == "contact";
                    isAccountLevel = sFilter[1].Trim() == "account";
                }                
            }

            DataTable dtDialog = null;

            if (!hasRows) {
                dtDialog = gridControlDialog.DataSource as DataTable;
                if (dtDialog == null || dtDialog.Rows.Count <= 0) {
                    DisposeGroupControls(layoutControlGroupQuestionnaire);
                    return;
                }
                rowcount = dtDialog.Rows.Count;
            } else {
                rowcount = gridViewDialog.RowCount;
            }

            DataRow row;
            CampaignQuestionnaire oQuestionnaire = null;

            layoutControlGroupQuestionnaire.BeginUpdate();
            layoutControlGroupQuestionnaire.BeginInit();
            DisposeGroupControls(layoutControlGroupQuestionnaire);
            bool hasActiveRows = false;
            for (int i = 0; i < rowcount; ++i) {                
                if (hasRows)
                    row = gridViewDialog.GetDataRow(i);
                else
                    row = dtDialog.Rows[i];
                if (row.RowState == DataRowState.Deleted) continue;
                hasActiveRows = true;
                if (row["content_json"] == null || string.IsNullOrEmpty(row["content_json"].ToString())) continue;
                oQuestionnaire = CampaignQuestionnaire.InstanciateWith(row["content_json"].ToString());
                if (oQuestionnaire == null) continue;

                if (isAccountLevel && !isContactLevel) {
                    if (!oQuestionnaire.Form.Settings.DataBindings.account_level) continue;
                } else if (isContactLevel && !isAccountLevel) {
                    if (oQuestionnaire.Form.Settings.DataBindings.account_level) continue;
                } else if (!isAccountLevel && !isContactLevel) continue;

                if (row["question_text"] != null)
                    oQuestionnaire.Form.Settings.QuestionText = row["question_text"].ToString();
                if (row["question_help_text"] != null)
                    oQuestionnaire.Form.Settings.QuestionHelp = row["question_help_text"].ToString();
                if (row["language_code"] != null)
                    oQuestionnaire.Form.Settings.DataBindings.language_code = row["language_code"].ToString();

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
                    case QuestionTypeConstants.SmartText:
                        SmartText oSmartText = new SmartText(this.layoutControlQuestionnaire);
                        oSmartText.DisableSelection = true;
                        oSmartText.ToolTipController = defaultToolTipController1;
                        oSmartText.Questionnaire = oQuestionnaire;
                        oSmartText.BindControls();
                        oSmartText.BindPropertyGrid();
                        oSmartText.BindSampleDataTable();
                        this.layoutControlGroupQuestionnaire.Add(oSmartText.ControlGroup);
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

               // break;
            }
            if (!hasActiveRows) DisposeGroupControls(layoutControlGroupQuestionnaire);
            DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            emptySpaceItem1.CustomizationFormText = "emptySpaceItemBottom";
            emptySpaceItem1.Name = "emptySpaceItemBottom";
            emptySpaceItem1.ShowInCustomizationForm = false;
            emptySpaceItem1.Size = new System.Drawing.Size(0, 0);
            emptySpaceItem1.MaxSize = new System.Drawing.Size(0, 0);
            emptySpaceItem1.Text = "emptySpaceItemBottom";
            emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);

            //add bottom spacer
            this.layoutControlGroupQuestionnaire.AddItem(emptySpaceItem1);
            this.layoutControlGroupQuestionnaire.EndInit();
            this.layoutControlGroupQuestionnaire.EndUpdate();
            this.layoutControlQuestionnaire.BestFit();
        }

        /// <summary>
        /// Load the customers combo lists
        /// </summary>
        private void PopulateCustomerComboList() {
            try {
                
                lookUpEditCustomer.Properties.DataSource = null;
                lookUpEditCampaign.Properties.DataSource = null;
                lookUpEditSubCampaign.Properties.DataSource = null;

                lookUpEditCustomer.Properties.Columns.Clear();
                lookUpEditCampaign.Properties.Columns.Clear();
                lookUpEditSubCampaign.Properties.Columns.Clear();

                lookUpEditCustomer.EditValue = 0;
                lookUpEditCampaign.EditValue = 0;
                lookUpEditSubCampaign.EditValue = 0;

                lookUpEditCustomer.Properties.DataSource = ObjectCustomer.GetCustomers(UserSession.CurrentUser.UserId).Execute(MergeOption.AppendOnly);
                lookUpEditCustomer.Properties.DisplayMember = "customer_name";
                lookUpEditCustomer.Properties.ValueMember = "id";
                lookUpEditCustomer.Properties.Columns.Add(new LookUpColumnInfo("customer_name"));
            } catch { }
        }

        /// <summary>
        /// Populate campaign combo list
        /// </summary>
        private void PopulateCampaignComboList(int CustomerId) {
            try {
                lookUpEditCampaign.Properties.DataSource = null;
                lookUpEditSubCampaign.Properties.DataSource = null;

                lookUpEditCampaign.Properties.Columns.Clear();
                lookUpEditSubCampaign.Properties.Columns.Clear();

                lookUpEditCampaign.EditValue = 0;
                lookUpEditSubCampaign.EditValue = 0;

                lookUpEditCampaign.Properties.DataSource = ObjectCampaign.GetCampaigns(CustomerId, UserSession.CurrentUser.UserId).Execute(MergeOption.AppendOnly);
                lookUpEditCampaign.Properties.DisplayMember = "name";
                lookUpEditCampaign.Properties.ValueMember = "id";
                lookUpEditCampaign.Properties.Columns.Add(new LookUpColumnInfo("name"));
            } catch { }
        }

        /// <summary>
        /// Populate sub campaign combo list
        /// </summary>
        private void PopulateSubCampaignComboList(int CustomerId, int CampaignId) {
            try {

                lookUpEditSubCampaign.Properties.DataSource = null;
                lookUpEditSubCampaign.Properties.Columns.Clear();
                lookUpEditSubCampaign.EditValue = 0;
                //lookUpEditSubCampaign.Properties.DataSource = ObjectSubCampaign.GetSubCampaigns(CampaignId, CustomerId, UserSession.CurrentUser.UserId).Execute(MergeOption.AppendOnly);
                var listSubcampaign = ObjectSubCampaign.GetSubCampaigns(CampaignId, CustomerId).Execute(MergeOption.AppendOnly);                
                lookUpEditSubCampaign.Properties.DataSource = ObjectSubCampaign.GetSubCampaigns(CampaignId, CustomerId).Execute(MergeOption.AppendOnly);
                lookUpEditSubCampaign.Properties.DisplayMember = "title";
                lookUpEditSubCampaign.Properties.ValueMember = "id";
                lookUpEditSubCampaign.Properties.Columns.Add(new LookUpColumnInfo("title"));
            } catch { }
        }

        private void btnCreateSchedule_Click(object sender, EventArgs e) {

            if (Convert.ToInt32(lookUpEditSubCampaign.EditValue) < 1)
            {
                MessageBox.Show("Please enter/select sub-campaign/subrequired fields first.", "Dialogs", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            var val = lookUpEditSubCampaign.EditValue;
            if (val != null) {
                int editval = (int)val;
                if (editval > 0) {
                    SalesConsultant.Forms.FrmSchedulingPopup
                        frmSchedulingPopup = new SalesConsultant.Forms.FrmSchedulingPopup();

                    if (SubCampaignDialog != null) {
                        frmSchedulingPopup.SetBreadCrumb(string.Format(
                            "{0}{1}{2}{3}",
                            string.IsNullOrEmpty(SubCampaignDialog.Customer) ? "" : SubCampaignDialog.Customer + " > ",
                            string.IsNullOrEmpty(SubCampaignDialog.Campaign) ? "" : SubCampaignDialog.Campaign + " > ",
                            string.IsNullOrEmpty(SubCampaignDialog.SubCampaignTitle) ? "" : SubCampaignDialog.SubCampaignTitle + " > ",
                            string.IsNullOrEmpty(SubCampaignDialog.DialogName) ? "" : SubCampaignDialog.DialogName));
                    } else {
                        var customer = lookUpEditCustomer.Text;
                        var campaign = lookUpEditCampaign.Text;
                        var subcampaign = lookUpEditSubCampaign.Text;
                        var dialog = textEditDialogname.Text;
                        frmSchedulingPopup.SetBreadCrumb(string.Format(
                            "{0}{1}{2}{3}",
                            string.IsNullOrEmpty(customer) ? "" : customer + " > ",
                            string.IsNullOrEmpty(campaign) ? "" : campaign + " > ",
                            string.IsNullOrEmpty(subcampaign) ? "" : subcampaign,
                            string.IsNullOrEmpty(dialog) ? "" : " > " + dialog));
                    }

                    frmSchedulingPopup.SubCampaignID = editval;
                    frmSchedulingPopup.ShowDialog();
                }
            }
        }

        private void gridViewQuestion_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gridViewDialog_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gridViewQuestion_EndGrouping(object sender, EventArgs e) {
            var view = sender as GridView;
            if (view != null) {
                view.ExpandAllGroups();
            }
        }
        #endregion

    }
}
