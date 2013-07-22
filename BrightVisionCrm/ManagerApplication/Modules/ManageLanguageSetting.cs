using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Data.Objects;
using ManagerApplication.Business;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules
{
    public partial class ManageLanguageSetting : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Properties
        public int LanguageId { get; set; }
        #endregion

        #region Private Members
        private List<BusinessLanguage.LanguageInstance> m_objLanguageList = new List<BusinessLanguage.LanguageInstance>();
        private string m_MessageCaption = "Language Settings";
        private int m_DefaultSelectedRow = 0;
        private bool m_IsLoadingView = false;
        #endregion

        #region Constructors
        public ManageLanguageSetting()
        {
            this.Visible = false;
            InitializeComponent();
            this.PopulateLanguageView();
            this.Visible = true;
        }
        #endregion

        #region Object Events
        private void gvLanguage_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (!m_IsLoadingView)
                m_DefaultSelectedRow = gvLanguage.FocusedRowHandle;
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            this.SaveUpdatedLanguages();
        }
        
        private void gvLanguage_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView objGridView = (GridView) sender;
            string ColFieldName = objGridView.FocusedColumn.FieldName;
            if (ValidationUtility.HasNumericEntries(e.Value.ToString()) && ColFieldName.Equals("code"))
            {
                e.ErrorText = "Not a valid language code. Must not contain numbers.";
                e.Valid = false;
            }
            else if (string.IsNullOrEmpty((e.Value.ToString())))
            {
                if (gvLanguage.FocusedColumn.FieldName.Equals("code"))
                    e.ErrorText = "Please specify a language code.";

                else if (gvLanguage.FocusedColumn.FieldName.Equals("description"))
                    e.ErrorText = "Please specify a language description.";

                e.Valid = false;
            }
            else if (ValidationUtility.HasSpecialChars(e.Value.ToString()))
            {
                e.ErrorText = "Not a valid language code. Must not contain special characters.";
                e.Valid = false;
            }
            else
            {
                this.QueueUpdatedLanguage();
                e.Valid = true;
            }
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            this.DisplayAddLanguageSettingForm();
        }

        private void gvLanguage_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion

        #region Public Methods
        public void SetDefaultSelectedRow()
        {
            if (LanguageId > 0) {
                BusinessLanguage.LanguageInstance _item = null;
                for (int i = 0; i < gvLanguage.RowCount; i++) {
                    _item = gvLanguage.GetRow(i) as BusinessLanguage.LanguageInstance;
                    if (_item.id == LanguageId) {
                        m_DefaultSelectedRow = i;
                        break;
                    }
                }
            }

            gvLanguage.FocusedRowHandle = m_DefaultSelectedRow;
        }

        /// <summary>
        /// Populate the languages grid view
        /// </summary>
        public void PopulateLanguageView()
        {
            try
            {
                m_IsLoadingView = true;
                gcLanguage.DataSource = null;
                gcLanguage.DataSource = Business.BusinessLanguage.GetLanguages().Execute(MergeOption.AppendOnly);
                m_IsLoadingView = false;
                this.SetDefaultSelectedRow();
                LanguageId = 0;
            }
            catch { }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Display add language setting form
        /// </summary>
        private void DisplayAddLanguageSettingForm()
        {
            AddLanguageSetting objForm = new AddLanguageSetting(this);
            PopupDialog objPopupDialog = new PopupDialog();
            objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            objPopupDialog.MinimizeBox = false;
            objPopupDialog.MaximizeBox = false;
            objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            objPopupDialog.Text = "Add Language Setting";
            objPopupDialog.Controls.Add(objForm);
            objPopupDialog.ClientSize = new Size(objForm.Width + 2, objForm.Height + 2);
            objPopupDialog.ShowDialog(this.ParentForm);
        }

        /// <summary>
        /// Queue updated languages
        /// </summary>
        private void QueueUpdatedLanguage()
        {
            GridView objGridView = (GridView) gcLanguage.FocusedView;
            BusinessLanguage.LanguageInstance objLanguage = (BusinessLanguage.LanguageInstance) objGridView.GetFocusedRow();
            if (m_objLanguageList.Find(item => item.id == objLanguage.id) == null)
                m_objLanguageList.Add(objLanguage);
        }

        /// <summary>
        /// Save updated languages
        /// </summary>
        private void SaveUpdatedLanguages()
        {
            if (m_objLanguageList.Count < 1)
            {
                MessageBox.Show("No records to update", m_MessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            WaitDialog.Show(ParentForm, "Saving...");
            BusinessLanguage.SaveUpdatedLanguages(m_objLanguageList);
            this.PopulateLanguageView();
            m_objLanguageList = null;
            m_objLanguageList = new List<BusinessLanguage.LanguageInstance>();
           WaitDialog.Close();
            MessageBox.Show("Successfully updated languages", m_MessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion      
    }
}
