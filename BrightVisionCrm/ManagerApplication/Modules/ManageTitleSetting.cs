using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ManagerApplication.Business;
using System.Data.Objects;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using BrightVision.Common.Business;
using BrightVision.Model;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules 
{
    public partial class ManageTitleSetting : DevExpress.XtraEditors.XtraUserControl 
    {
        #region Private Members
        private BrightPlatformEntities BPContext = null;
        #endregion

        #region Constructors
        public ManageTitleSetting() {
            this.Visible = false;
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            this.lcManageTitleSetting.AllowCustomizationMenu = false;
            this.PopulateLanguageComboList();
            this.PopulateTitleView();
            PopulateUnstructuredTitles();
            this.Visible = true;
        }
        #endregion

        #region Object Events

        private void txtTitleID_EditValueChanged(object sender, EventArgs e) {
            var edit = sender as TextEdit;
            var val = edit.EditValue.ToString();
            if (string.IsNullOrEmpty(val)) return;
            int id = 0;
            if (int.TryParse(val, out id)) {
                var source = gcTitle.DataSource as DataTable;
                var rows = source.Select("id=" + val);
                if (rows != null && rows.Length > 0) {
                    txtLookupTitleName.Text = rows[0]["name"].ToString();
                    gvTitle.FocusedRowHandle = gvTitle.GetRowHandle(source.Rows.IndexOf(rows[0]));
                } else {
                    gvTitle.FocusedRowHandle = 0;
                    txtLookupTitleName.Text = "";
                }
            }            
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            DisplayAddTitleSettingForm(false);
        }

        private void btnEditTitle_Click(object sender, EventArgs e) {
            if (gvTitle.RowCount <= 0) return;
            DisplayAddTitleSettingForm(true);
        }

        private void btnDeleteTitle_Click(object sender, EventArgs e) {
            if (gvTitle.RowCount <= 0) return;

            if(MessageBox.Show("Are you sure you want to delete the selected title?",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.No) return;
            var dr = gvTitle.GetDataRow(gvTitle.FocusedRowHandle);            
            if(dr != null) {
                int id = Convert.ToInt32(dr["id"]);
                var titleobj = BPContext.titles.FirstOrDefault(x => x.id == id);
                if (titleobj != null) {
                    try {
                        BPContext.titles.DeleteObject(titleobj);
                        BPContext.SaveChanges();
                    } catch (Exception ex) {
                        if(ex.InnerException != null && ex.InnerException.Message != null) {
                            if (ex.InnerException.Message.Contains("conflicted")) {
                                MessageBox.Show("Could not delete the selected title. One or more contacts currently using this title.",
                                    "System Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                return;
                            }                            
                        }
                    }
                }
                dr.Delete();
                dr.AcceptChanges();
                dr.Table.AcceptChanges();
            }
        }

        private void btnCalcOccurences_Click(object sender, EventArgs e) {
            if (gvTitle.RowCount <= 0) return;
            try {
                if (MessageBox.Show("This will delete all existing occurrences and calculate new one.", "Confirmation",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
                BPContext.FIUpdateTitleOccurences();
                UserSession.CurrentUser.TitleList = null;
                UserSession.CurrentUser.TitleList = BrightVision.Common.Utilities.DatabaseUtility.ExecuteStoredProcedure("bvGetTitles_sp", null);
                gcTitle.DataSource = null;
                gcTitle.DataSource = UserSession.CurrentUser.TitleList;
            } catch { }
        }

        private void btnReplaceTitle_Click(object sender, EventArgs e) {
            try {
                bool isValid = true;
                if (txtTitleID.Text == string.Empty || txtTitleID.Text == "0") isValid = false;
                if (txtLookupTitleName.Text == string.Empty) isValid = false;

                if (!isValid) {
                    MessageBox.Show("Please enter a valid Title ID number first", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CTUnstructuredTitle _item = gvUnstructuredTitles.GetFocusedRow() as CTUnstructuredTitle;
                int titleID = int.Parse(txtTitleID.Text);
                //var res = BPContext.FIUpdateContactTitleID(
                //    titleID, 
                //    txtUnstructuredTitle.Text,
                //    UserSession.CurrentUser.ComputerName,
                //    BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Title_Setting
                //).FirstOrDefault();
                BPContext.FIUpdateContactTitleId(
                    titleID,
                    txtUnstructuredTitle.Text,
                    UserSession.CurrentUser.ComputerName,
                    BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Title_Setting
                );

                gvUnstructuredTitles.DeleteSelectedRows();                
                string sCaption = "Completed";
                string sMessage = " occurence(s) replaced.";
                //sMessage = res.ToString() + sMessage;                
                sMessage = _item.occurences.ToString() + sMessage;
                MessageBox.Show(sMessage, sCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtUnstructuredTitle.Text = gvUnstructuredTitles.GetRowCellDisplayText(gvUnstructuredTitles.FocusedRowHandle, "title");
                txtTitleID.EditValue = 0;
                txtLookupTitleName.Text = "";
            } catch { }
        }

        private void gvTitle_DoubleClick(object sender, EventArgs e) {
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                DisplayAddTitleSettingForm(true);
            }
        }

        private void gvTitle_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvTitle_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e) 
        {
            if (e.Value == null)
                return;

            if (e.Column.FieldName == "date_created") {
                DateTime dt = DateTime.MinValue;
                if(DateTime.TryParse(e.Value.ToString(), out dt))
                    e.DisplayText = dt.ToString("yyyy-MM-dd");
            }
        }

        private void gvUnstructuredTitles_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e) 
        {
            var view = sender as GridView;
            txtUnstructuredTitle.Text = view.GetRowCellDisplayText(e.FocusedRowHandle, "title");
            
        }
        private void gvUnstructuredTitles_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            var view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        #endregion

        #region Public Methods
        public void PopulateTitleView() 
        {
            try {
                var titleList = (DataTable)UserSession.CurrentUser.TitleList;
                if (titleList == null)
                    titleList = BrightVision.Common.Utilities.DatabaseUtility.ExecuteStoredProcedure("bvGetTitles_sp", null);
                gcTitle.DataSource = null;
                gcTitle.DataSource = titleList;

            } catch {
                return;
            }
        }
        #endregion

        #region Private Methods
        private void PopulateUnstructuredTitles()
        {
            //var unstructuredTitleList =
            //        (from x in BPContext.contacts
            //         where x.title_id == null
            //         select new { title = x.title })
            //         .OrderByDescending(p => p.title)
            //         .Distinct().ToList();
            gcUnstructuredTitles.DataSource = null;
            //gcUnstructuredTitles.DataSource = unstructuredTitleList;
            gcUnstructuredTitles.DataSource = BPContext.FIGetUnstructuredTitles().ToList();
        }
        private void DisplayAddTitleSettingForm(bool isEditMode) 
        {
            AddTitleSetting objForm = new AddTitleSetting(gvTitle, isEditMode);
            PopupDialog objPopupDialog = new PopupDialog() { 
                FormBorderStyle = FormBorderStyle.FixedSingle, 
                MinimizeBox = false, 
                MaximizeBox = false, 
                StartPosition = FormStartPosition.CenterScreen, 
                Text = !isEditMode ? "Add Title" : "Update Title" 
            };
            objPopupDialog.Controls.Add(objForm);
            objPopupDialog.ClientSize = new Size(objForm.Width + 2, objForm.Height + 2);
            objPopupDialog.ShowDialog(this.ParentForm);
        }
        private void PopulateLanguageComboList() {
            try {
                cboLanguage.DataSource = null;
                cboLanguage.DataSource = BusinessLanguage.GetLanguages().Execute(MergeOption.AppendOnly);
                cboLanguage.DisplayMember = "code";
                cboLanguage.ValueMember = "id";
                cboLanguage.Columns.Add(new LookUpColumnInfo("code"));
                cboLanguage.Columns.Add(new LookUpColumnInfo("description"));
            } catch { }
        }
        #endregion
    }

}
