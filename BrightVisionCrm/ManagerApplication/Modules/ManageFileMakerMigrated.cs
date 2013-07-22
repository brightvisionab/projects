using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.Objects.SqlClient;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.Utils.Menu;

using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using ManagerApplication.Business;

namespace ManagerApplication.Modules {
    public partial class ManageFileMakerMigrated : DevExpress.XtraEditors.XtraUserControl {

        #region Member Variables
        long selectedCampaign_id = 0;
        int column_count = 0;
        BrightPlatformEntities BPContext = null;
        List<string> listPositions = null;
        List<object_question> listQuestion = null;
        DataTable dtFileMakerFields = null;
        #endregion

        #region Constructor
        public ManageFileMakerMigrated() {
            InitializeComponent();
            this.layoutControl1.AllowCustomizationMenu = false;
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            PopulateFileMakerCampaign();
            PopulateQuestions();
            riLookUpEditQuestionID.EditValueChanged += new EventHandler(riLookUpEditQuestionID_EditValueChanged);
            riLookUpEditQuestionID.KeyDown += new KeyEventHandler(riLookUpEditQuestionID_KeyDown);
        }
        #endregion

        #region Controllers
        private void ManageFileMakerMigrated_Load(object sender, EventArgs e) {
           
        }

        private void gvFileMakerFields_ShownEditor(object sender, EventArgs e) {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName == "column_order" && view.ActiveEditor is ComboBoxEdit) {
                ComboBoxEdit edit = (ComboBoxEdit)view.ActiveEditor;
                DataRow[] result = dtFileMakerFields.Select("LEN(column_order) > 0 AND column_order <> '<ignore>' ");
                List<string> usedPositions = new List<string>();

                foreach (DataRow dr in result) {
                    usedPositions.Add(dr["column_order"].ToString());
                }
                PopulateColumnPositionItems();
                listPositions = listPositions.Except(usedPositions).ToList();
                //add if has value
                if (!string.IsNullOrEmpty(edit.EditValue.ToString()))
                    listPositions.Add(edit.EditValue.ToString());

                listPositions.Sort();
                listPositions = listPositions.Distinct().ToList();
                edit.Properties.Items.Clear();
                edit.Properties.Items.AddRange(listPositions.ToArray());
            }
        }

        private void gvTableResult_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {            
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);            
        }

        private void gvFileMakerFields_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        
        private void riLookUpEditQuestionID_EditValueChanged(object sender, EventArgs e) {
            var edit = sender as LookUpEdit;
            var question = edit.GetSelectedDataRow() as object_question;
            if (question == null) return;
            if (question.id != "<ignore>") {
                GridColumn gc = gvFileMakerFields.Columns["question_lookup_desc"];
                gvFileMakerFields.SetFocusedRowCellValue(gc, question.text);
            } else {
                GridColumn gc = gvFileMakerFields.Columns["question_id"];
                gvFileMakerFields.SetFocusedRowCellValue(gc, "");
                gc = gvFileMakerFields.Columns["question_lookup_desc"];
                gvFileMakerFields.SetFocusedRowCellValue(gc, "");
            }
        }

        private void riLookUpEditQuestionID_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                gvFileMakerFields.HideEditor();
                e.Handled = true;
            }
        }

        private void lookUpEditCampaigns_EditValueChanged(object sender, EventArgs e) {
            var objSender = sender as LookUpEdit;
            if (objSender != null) {
                var eval = objSender.EditValue;
                if (eval != null && !string.IsNullOrEmpty(eval.ToString())) {
                    selectedCampaign_id = Convert.ToInt64(objSender.EditValue);
                    gcFileMakerFields.DataSource = null;
                    gcTableResult.DataSource = null;
                    gvTableResult.Columns.Clear();
                    dtFileMakerFields = null;
                }
            }
        }

        private void btnLoadColumns_Click(object sender, EventArgs e) {
            if (selectedCampaign_id > 0) {
                
                WaitDialog.Show(ParentForm, "Loading column fields...");
                PopulateFilemakerCampaignFields(selectedCampaign_id);
                WaitDialog.Close();
                return;
            }
            MessageBox.Show("Please select a FileMaker campaign to work with first before loading columns.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnLoadTableResult_Click(object sender, EventArgs e) {
            if ((selectedCampaign_id > 0 && dtFileMakerFields != null && dtFileMakerFields.Rows.Count > 0) || gvFileMakerFields.RowCount > 0) {
                
                WaitDialog.Show(ParentForm, "Loading data...");
                List<string> columnNames = new List<string>();
                DataTable dtTemp = dtFileMakerFields.Copy();
                foreach (DataRow dr in dtTemp.Rows) {
                    if (dr["column_order"].ToString() == "<ignore>")
                        dr.Delete();
                }
                dtTemp.AcceptChanges();
                dtTemp.Columns.Add("ForSort", typeof(int), "Convert(column_order,'System.Int32')");
                DataView dvTemp = new DataView(dtTemp);
                dvTemp.Sort = "ForSort";
                dtTemp = dvTemp.ToTable();
                foreach (DataRow dr in dtTemp.Rows) {
                    columnNames.Add("[" + dr["column_name"].ToString() + "]");
                }
                DataTable dtResults = DatabaseUtility.GetFileMakerProfilingResults(
                    selectedCampaign_id,
                    string.Join(",", columnNames.ToArray()));                
                if (dtResults != null) {
                    var rowcount = dtResults.Rows.Count;
                    if (rowcount > 0) {
                        foreach (DataRow dr in dtTemp.Rows) {
                            var colName = dr["column_name"].ToString();
                            var questionId = dr["question_id"].ToString();
                            foreach (DataRow ddr in dtResults.Rows) {
                                if (!string.IsNullOrEmpty(questionId) && !string.IsNullOrEmpty(ddr[colName].ToString()))
                                    ddr[colName] = ddr[colName].ToString() + "[↨]" + questionId;
                            }
                        }
                        dtResults.AcceptChanges();
                    }
                }

                if (gvTableResult.Columns.Count > 0)
                    gvTableResult.Columns.Clear();

                gcTableResult.DataSource = null;
                gcTableResult.DataSource = dtResults;

                gvTableResult.Columns["row"].Visible = false;

                if (gvTableResult.Columns.Count > 0) {
                    GridColumn gc = null;
                    for (int x = 0; x < gvTableResult.Columns.Count; ++x) {
                        gc = gvTableResult.Columns[x];
                        gc.OptionsColumn.AllowEdit = false;
                        gc.OptionsColumn.AllowFocus = false;
                    }
                    gvTableResult.OptionsView.ColumnAutoWidth = false;
                }
               WaitDialog.Close();
                return;
            }
            MessageBox.Show("Please load columns first before loading table result.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        } 
        #endregion

        #region Private Methods
        private void PopulateColumnPositionItems() {
            listPositions = new List<string>();
            listPositions.Add("<ignore>");
            for (int x = 1; x <= column_count; ++x) {
                listPositions.Add(x.ToString());
            }
        }

        private void PopulateFileMakerCampaign() {
            DataTable dt = DatabaseUtility.GetFileMakerCampaigns();
            lookUpEditCampaigns.Properties.DataSource = dt;
            lookUpEditCampaigns.Properties.DisplayMember = "path_name";
            lookUpEditCampaigns.Properties.ValueMember = "filemaker_id";
            lookUpEditCampaigns.Properties.Columns.Add(new LookUpColumnInfo("path_name", "Campaign name"));
            lookUpEditCampaigns.Properties.ShowHeader = false;
            lookUpEditCampaigns.Properties.ShowFooter = true;
            lookUpEditCampaigns.Properties.DropDownRows = 30;
        }

        private void PopulateFilemakerCampaignFields(long campaign_id) {
            dtFileMakerFields = DatabaseUtility.GetFileMakerCampaignFields(campaign_id);
            column_count = dtFileMakerFields.Rows.Count;
            int counter = 1;
            foreach (DataRow dr in dtFileMakerFields.Rows) {
                dr["column_order"] = counter.ToString();
                counter++;
            }
            dtFileMakerFields.AcceptChanges();
            gcFileMakerFields.DataSource = null;
            gcFileMakerFields.DataSource = dtFileMakerFields;
        }

        private void PopulateQuestions() {
            listQuestion = (from obj in BPContext.questions_text_language
                            select new object_question { iid = obj.id, text = obj.question_text }
                           ).ToList();

            if (listQuestion == null) listQuestion = new List<object_question>();
            listQuestion.Insert(0, new object_question { id = "<ignore>", text = "<ignore>" });

            riLookUpEditQuestionID.DataSource = listQuestion;
            riLookUpEditQuestionID.DisplayMember = "id";
            riLookUpEditQuestionID.ValueMember = "id";
            riLookUpEditQuestionID.Columns.Add(new LookUpColumnInfo("id", 7, "QuestionID"));
            riLookUpEditQuestionID.Columns.Add(new LookUpColumnInfo("text", "Question Text"));
            riLookUpEditQuestionID.PopupWidth = 300;
            riLookUpEditQuestionID.DropDownRows = 20;
        }
                
        #endregion

        #region Private Classes
        class object_customer {
            public int id { get; set; }
            public string name { get; set; }
        }

        class object_question {
            string _id;
            int _iid;
            public string id {
                get { return _id; }
                set { _id = value; }
            }
            public int iid {
                get { return _iid; }
                set {
                    _iid = value;
                    _id = _iid.ToString();
                }
            }
            public string text { get; set; }
        }

        class RowInfo {
            public RowInfo(GridView view, int rowHandle) {
                this.RowHandle = rowHandle;
                this.View = view;
            }
            public GridView View;
            public int RowHandle;
        }
        #endregion
    }
}
