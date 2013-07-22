
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SalesConsultant.Business;
using System.Data.Objects;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using BrightVision.Common.Business;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using SalesConsultant.Forms;
using BrightVision.Common.Events.Core;
using SalesConsultant.Facade;
using SalesConsultant.Events;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.Modules 
{
    public partial class ManageTitleSetting : XtraUserControl 
    {
        #region Constructors
        public ManageTitleSetting() 
        {
            this.Visible = false;
            InitializeComponent();
            this.RegisterEvents();
            this.lcManageTitleSetting.AllowCustomizationMenu = false;
            this.GetLanguages();
            this.GetTitles();
            this.GetUnstructuredTitles();
            this.Visible = true;
        }
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private List<CXTitle> m_lstTitles = new List<CXTitle>();
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<AddTitleSettingEvents.OnSave>().Subscribe(OnSave);
        }
        private void OnSave(AddTitleSettingEvents.OnSave e)
        {
            gcTitle.BeginUpdate();
            if (e.WriteMode == SelectionProperty.eWriteMode.New) {
                m_lstTitles.Add(new CXTitle() {
                    id = e.Title.id,
                    language_id = e.Title.language_id,
                    name = e.Title.name,
                    ssyk = e.Title.ssyk,
                    occurences = e.Title.ssyk,
                    date_created = e.Title.date_created
                });
                this.UpdateTitles();
            }
            else {
                gvTitle.SetRowCellValue(gvTitle.FocusedRowHandle, "id", e.Title.id);
                gvTitle.SetRowCellValue(gvTitle.FocusedRowHandle, "language_id", e.Title.language_id);
                gvTitle.SetRowCellValue(gvTitle.FocusedRowHandle, "name", e.Title.name);
                gvTitle.SetRowCellValue(gvTitle.FocusedRowHandle, "ssyk", e.Title.ssyk);
                gvTitle.SetRowCellValue(gvTitle.FocusedRowHandle, "occurences", e.Title.occurences);
                gvTitle.SetRowCellValue(gvTitle.FocusedRowHandle, "date_created", e.Title.date_created);
            }
            gcTitle.EndUpdate();
        }

        private void GetTitles() 
        {
            try {
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    m_lstTitles = _efDbContext.FIGetTitles().ToList();
                }
                this.UpdateTitles();
            } 
            catch {
                return;
            }
        }
        private void UpdateTitles()
        {
            gcTitle.BeginUpdate();
            gcTitle.DataSource = null;
            gcTitle.DataSource = m_lstTitles;
            gvTitle.BestFitColumns();
            gcTitle.EndUpdate();
        }
        private void GetUnstructuredTitles()
        {
            //var unstructuredTitleList =
            //        (from x in BPContext.contacts
            //         where x.title_id == null
            //         select new { title = x.title })
            //         .OrderByDescending(p => p.title)
            //         .Distinct().ToList();
            //gcUnstructuredTitles.DataSource = unstructuredTitleList;

            //gcUnstructuredTitles.DataSource = null;
            //gcUnstructuredTitles.DataSource = BPContext.FIGetUnstructuredTitles().ToList();

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                gcUnstructuredTitles.BeginUpdate();
                gcUnstructuredTitles.DataSource = null;
                gcUnstructuredTitles.DataSource = _efDbContext.FIGetUnstructuredTitles().ToList();
                gvUnstructuredTitles.BestFitColumns();
                gcUnstructuredTitles.EndUpdate();
            }
        }
        private void DisplayAddTitleSettingForm(bool pOnEdit) 
        {
            SelectionProperty.eWriteMode _mode = SelectionProperty.eWriteMode.New;
            if (pOnEdit)
                _mode = SelectionProperty.eWriteMode.Edit;

            AddTitleSetting _control = new AddTitleSetting(_mode, m_lstTitles, gvTitle.GetFocusedRow() as CXTitle);
            PopupDialog objPopupDialog = new PopupDialog() { 
                FormBorderStyle = FormBorderStyle.FixedSingle, 
                MinimizeBox = false, 
                MaximizeBox = false, 
                StartPosition = FormStartPosition.CenterScreen,
                Text = !pOnEdit ? "Add Title" : "Update Title" 
            };
            objPopupDialog.Controls.Add(_control);
            objPopupDialog.ClientSize = new Size(_control.Width + 2, _control.Height + 2);
            objPopupDialog.ShowDialog(this.ParentForm);
        }
        private void GetLanguages() 
        {
            try {
                cboLanguage.BeginUpdate();
                cboLanguage.DataSource = null;
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    cboLanguage.DataSource = _efDbContext.FXGetLanguages().ToList();
                }
                cboLanguage.DisplayMember = "code";
                cboLanguage.ValueMember = "id";
                cboLanguage.Columns.Add(new LookUpColumnInfo("code"));
                cboLanguage.Columns.Add(new LookUpColumnInfo("name"));
                cboLanguage.EndUpdate();
            } 
            catch { 
            }
        }
        #endregion

        #region Control Events
        private void txtTitleID_EditValueChanged(object sender, EventArgs e)
        {
            TextEdit _control = sender as TextEdit;
            string _value = _control.EditValue.ToString();
            if (string.IsNullOrEmpty(_value)) 
                return;

            gvTitle.FocusedRowHandle = 0;
            txtLookupTitleName.Text = string.Empty;
            int _id = 0;

            if (int.TryParse(_value, out _id)) {
                CXTitle _cxTitle = m_lstTitles.FirstOrDefault(i => i.id == _id);
                if (_cxTitle != null) {
                    txtLookupTitleName.Text = _cxTitle.name;
                    gvTitle.FocusedRowHandle = gvTitle.GetRowHandle(m_lstTitles.IndexOf(_cxTitle));
                }

                //var source = gcTitle.DataSource as DataTable;
                //var rows = source.Select("id=" + _value);
                //if (rows != null && rows.Length > 0)
                //{
                //    txtLookupTitleName.Text = rows[0]["name"].ToString();
                //    gvTitle.FocusedRowHandle = gvTitle.GetRowHandle(source.Rows.IndexOf(rows[0]));
                //}
                //else
                //{
                //    gvTitle.FocusedRowHandle = 0;
                //    txtLookupTitleName.Text = "";
                //}
            }
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            this.DisplayAddTitleSettingForm(false);
        }
        private void btnEditTitle_Click(object sender, EventArgs e)
        {
            if (gvTitle.RowCount < 1) 
                return;

            this.DisplayAddTitleSettingForm(true);
        }
        private void btnDeleteTitle_Click(object sender, EventArgs e)
        {
            if (gvTitle.RowCount < 1) 
                return;

            DialogResult _dlg = MessageBox.Show("Are you sure you want to delete the selected title?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlg == DialogResult.No)
                return;

            try {
                CXTitle _cxTitle = gvTitle.GetFocusedRow() as CXTitle;
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    title _eftTitle = _efDbContext.titles.FirstOrDefault(i => i.id == _cxTitle.id);
                    if (_eftTitle != null) {
                        _efDbContext.titles.DeleteObject(_eftTitle);
                        _efDbContext.SaveChanges();
                    }
                }

                m_lstTitles.Remove(_cxTitle);
                gvTitle.DeleteRow(gvTitle.FocusedRowHandle);
            }
            catch (Exception ex) {
                if (ex.InnerException != null && ex.InnerException.Message != null)
                    if (ex.InnerException.Message.Contains("conflicted"))
                        BrightVision.Common.UI.NotificationDialog.Warning("Bright Sales", "Could not delete the selected title. One or more contacts currently using this title.");
            }

            //var dr = gvTitle.GetDataRow(gvTitle.FocusedRowHandle);
            //if (dr != null)
            //{
            //    int id = Convert.ToInt32(dr["id"]);
            //    var titleobj = BPContext.titles.FirstOrDefault(x => x.id == id);
            //    if (titleobj != null)
            //    {
            //        try
            //        {
            //            BPContext.titles.DeleteObject(titleobj);
            //            BPContext.SaveChanges();
            //        }
            //        catch (Exception ex)
            //        {
            //            if (ex.InnerException != null && ex.InnerException.Message != null)
            //            {
            //                if (ex.InnerException.Message.Contains("conflicted"))
            //                {
            //                    MessageBox.Show("Could not delete the selected title. One or more contacts currently using this title.",
            //                        "System Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //    dr.Delete();
            //    dr.AcceptChanges();
            //    dr.Table.AcceptChanges();
            //}
        }
        private void btnCalcOccurences_Click(object sender, EventArgs e)
        {
            if (gvTitle.RowCount < 1) 
                return;

            DialogResult _dlg = MessageBox.Show("This will delete all existing occurrences and calculate new one.", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (_dlg == DialogResult.Cancel)
                return;

            try {
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    _efDbContext.FIUpdateTitleOccurences();
                }
                UserSession.CurrentUser.TitleList = null;
                UserSession.CurrentUser.TitleList = DatabaseUtility.ExecuteStoredProcedure("bvGetTitles_sp", null);
                this.GetTitles();
            }
            catch { 
            }

            //try
            //{
            //    if (MessageBox.Show("This will delete all existing occurrences and calculate new one.", "Confirmation",
            //        MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
            //    BPContext.FIUpdateTitleOccurences();
            //    UserSession.CurrentUser.TitleList = null;
            //    UserSession.CurrentUser.TitleList = BrightVision.Common.Utilities.DatabaseUtility.ExecuteStoredProcedure("bvGetTitles_sp", null);
            //    gcTitle.DataSource = null;
            //    gcTitle.DataSource = UserSession.CurrentUser.TitleList;
            //}
            //catch { }
        }
        private void btnReplaceTitle_Click(object sender, EventArgs e)
        {
            try {
                if (string.IsNullOrEmpty(txtTitleID.Text) || string.IsNullOrEmpty(txtLookupTitleName.Text) || txtTitleID.Text == "0")  {
                    BrightVision.Common.UI.NotificationDialog.Warning("Bright Sales", "Please enter a valid title Id number.");
                    return;
                }

                CTUnstructuredTitle _item = gvUnstructuredTitles.GetFocusedRow() as CTUnstructuredTitle;
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    int _TitleId = Convert.ToInt32(txtTitleID.Text);
                    _efDbContext.FIUpdateContactTitleId(
                        _TitleId,
                        txtUnstructuredTitle.Text,
                        UserSession.CurrentUser.ComputerName,
                        BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Title_Setting
                    );
                }
                
                gvUnstructuredTitles.DeleteSelectedRows();
                BrightVision.Common.UI.NotificationDialog.Information("Bright Sales", string.Format("{0} occurence(s) replaced.", _item.occurences));
                txtUnstructuredTitle.Text = gvUnstructuredTitles.GetRowCellDisplayText(gvUnstructuredTitles.FocusedRowHandle, "title");
                txtTitleID.EditValue = 0;
                txtLookupTitleName.Text = string.Empty;
            }
            catch { 
            }

            //try
            //{
            //    bool isValid = true;
            //    if (txtTitleID.Text == string.Empty || txtTitleID.Text == "0") isValid = false;
            //    if (txtLookupTitleName.Text == string.Empty) isValid = false;

            //    if (!isValid)
            //    {
            //        MessageBox.Show("Please enter a valid Title ID number first", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }

            //    CTUnstructuredTitle _item = gvUnstructuredTitles.GetFocusedRow() as CTUnstructuredTitle;
            //    int titleID = int.Parse(txtTitleID.Text);
            //    //var res = BPContext.FIUpdateContactTitleID(
            //    //    titleID, 
            //    //    txtUnstructuredTitle.Text,
            //    //    UserSession.CurrentUser.ComputerName,
            //    //    BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Title_Setting
            //    //).FirstOrDefault();
            //    BPContext.FIUpdateContactTitleId(
            //        titleID,
            //        txtUnstructuredTitle.Text,
            //        UserSession.CurrentUser.ComputerName,
            //        BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Title_Setting
            //    );

            //    gvUnstructuredTitles.DeleteSelectedRows();
            //    string sCaption = "Completed";
            //    string sMessage = " occurence(s) replaced.";
            //    //sMessage = res.ToString() + sMessage;                
            //    sMessage = _item.occurences.ToString() + sMessage;
            //    MessageBox.Show(sMessage, sCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    txtUnstructuredTitle.Text = gvUnstructuredTitles.GetRowCellDisplayText(gvUnstructuredTitles.FocusedRowHandle, "title");
            //    txtTitleID.EditValue = 0;
            //    txtLookupTitleName.Text = "";
            //}
            //catch { }
        }
        private void gvTitle_DoubleClick(object sender, EventArgs e)
        {
            GridView _gv = sender as GridView;
            GridHitInfo hitInfo = _gv.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow)
                this.DisplayAddTitleSettingForm(true);
        }
        private void gvTitle_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView _gv = sender as GridView;
            GridUtility.CreateGridContextMenu(_gv, e);
        }
        private void gvTitle_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Value == null)
                return;

            if (e.Column.FieldName == "date_created") {
                DateTime _dtDate = DateTime.MinValue;
                if (DateTime.TryParse(e.Value.ToString(), out _dtDate))
                    e.DisplayText = _dtDate.ToString("yyyy-MM-dd");
            }
        }
        private void gvUnstructuredTitles_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            GridView _gv = sender as GridView;
            txtUnstructuredTitle.Text = _gv.GetRowCellDisplayText(e.FocusedRowHandle, "title");
        }
        private void gvUnstructuredTitles_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView _gv = sender as GridView;
            GridUtility.CreateGridContextMenu(_gv, e);
        }
        #endregion
    }
}
