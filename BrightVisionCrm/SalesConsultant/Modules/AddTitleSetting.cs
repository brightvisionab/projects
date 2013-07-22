using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Data.Objects;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;

using SalesConsultant.Business;
using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Events.Core;
using SalesConsultant.Facade;
using SalesConsultant.PublicProperties;
using SalesConsultant.Events;

namespace SalesConsultant.Modules
{
    public partial class AddTitleSetting : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Properties
        #endregion

        #region Private Members
        private List<CXTitle> m_GridData = null;
        private CXTitle m_GridRow = null;
        private SelectionProperty.eWriteMode m_WriteMode = SelectionProperty.eWriteMode.None;
        //private GridView m_objRefGridView = null;
        //private string m_MessageCaption = "Add Title";
        //private bool OnEditMode = false;
        //private BrightPlatformEntities BPContext = null;
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        #endregion

        #region Constructors
        public AddTitleSetting()
        {
            InitializeComponent();
        }
        public AddTitleSetting(SelectionProperty.eWriteMode WriteMode, List<CXTitle> pGridData, CXTitle pGridRow)
        {
            m_WriteMode = WriteMode;
            m_GridRow = pGridRow;
            m_GridData = pGridData;
            InitializeComponent();
            this.PopulateLanguageComboList();
            if (m_WriteMode == SelectionProperty.eWriteMode.Edit) {
                btnSave.Text = "Update";
                if (m_GridRow != null) {
                    txtTitle.Text = m_GridRow.name.ToString();
                    txtTitle.Tag = m_GridRow.id;
                    txtSSYK.Text = m_GridRow.ssyk.ToString();
                    cboLanguage.EditValue = m_GridRow.language_id;
                }
            }
        }
        #endregion

        #region Object Events

        private void btnSave_Click(object sender, EventArgs e) 
        {
            WaitDialog.Show("Saving data ...");
            if (ValidateEntries()) {
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    if (m_GridData != null) {
                        title _eftTitle = null;
                        int _ssyk = 0;
                        if (m_WriteMode == SelectionProperty.eWriteMode.Edit) {
                            m_WriteMode = SelectionProperty.eWriteMode.Edit;
                            //var row = GridData.Select(String.Format("id <> {0} AND name = '{1}'", GridRow.id, txtTitle.Text.Trim().Replace("'", "''")));
                            //if (row != null && row.Length > 0) {
                            string _Title = txtTitle.Text.Trim().Replace("'", "''");
                            int _Existing = m_GridData.Where(i => i.name == _Title && i.id != m_GridRow.id).Count();
                            if (_Existing > 0) {
                                BrightVision.Common.UI.NotificationDialog.Warning("Bright Sales", "Title name already exists. Please try another name.");
                                this.ParentForm.DialogResult = DialogResult.None;
                                return;
                            }
                            _eftTitle = _efDbContext.titles.FirstOrDefault(i => i.id == m_GridRow.id);
                            if (_eftTitle != null) {
                                _eftTitle.name = txtTitle.Text.Trim();
                                _eftTitle.language_id = Convert.ToInt32(cboLanguage.EditValue);
                                _eftTitle.ssyk = null;
                                if (int.TryParse(txtSSYK.Text.Trim(), out _ssyk))
                                    _eftTitle.ssyk = _ssyk;

                                _efDbContext.SaveChanges();
                                _efDbContext.Detach(_eftTitle);
                            }
                        }
                        else {
                            string _Title = txtTitle.Text.Trim().Replace("'", "''");
                            int _Existing = m_GridData.Where(i => i.name == _Title && i.id != m_GridRow.id).Count();
                            if (_Existing > 0) {
                                BrightVision.Common.UI.NotificationDialog.Warning("Bright Sales", "Title name already exists. Please try another name.");
                                this.ParentForm.DialogResult = DialogResult.None;
                                return;
                            }

                            m_WriteMode = SelectionProperty.eWriteMode.New;
                            _eftTitle = new title() {
                                date_created = DateTime.Now,
                                name = txtTitle.Text.Trim(),
                                language_id = Convert.ToInt32(cboLanguage.EditValue),
                                ssyk = null
                            };
                            if (int.TryParse(txtSSYK.Text.Trim(), out _ssyk))
                                _eftTitle.ssyk = _ssyk;

                            _efDbContext.titles.AddObject(_eftTitle);
                            _efDbContext.SaveChanges();
                            _efDbContext.Detach(_eftTitle);
                        }

                        m_EventBus.Notify(new AddTitleSettingEvents.OnSave() {
                            Title = _eftTitle,
                            WriteMode = m_WriteMode
                        });

                        //if (_eftTitle == null) {
                        //    _eftTitle = new title();
                        //    _eftTitle.date_created = DateTime.Now;
                        //}

                        //objTitle.name = txtTitle.Text.Trim();

                        //if (int.TryParse(txtSSYK.Text.Trim(), out ssyk))
                        //    objTitle.ssyk = ssyk;
                        //else
                        //    objTitle.ssyk = null;

                        //objTitle.language_id = (int)cboLanguage.EditValue;

                        //add new title
                        //if (!IsEditMode)
                        //{
                        //    BPContext.titles.AddObject(objTitle);
                        //}
                        //BPContext.SaveChanges();
                        //add new datarow to datasouce
                        //if (!IsEditMode)
                        //{
                        //    DataRow dr = dtSource.NewRow();
                        //    dr["id"] = objTitle.id;
                        //    dr["name"] = objTitle.name;
                        //    if (objTitle.ssyk == null)
                        //        dr["ssyk"] = DBNull.Value;
                        //    else
                        //        dr["ssyk"] = objTitle.ssyk;
                        //    dr["language_id"] = objTitle.language_id;
                        //    dr["date_created"] = objTitle.date_created;
                        //    dtSource.Rows.Add(dr);
                        //    dtSource.AcceptChanges();
                        //}
                        //pGridTitles.MoveLastVisible();
                    }
                }
            } 
            else {
                this.ParentForm.DialogResult = DialogResult.None;
            }
            WaitDialog.Close();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Populate language combo list
        /// </summary>
        private void PopulateLanguageComboList() {
            try {
                cboLanguage.Properties.DataSource = null;
                //cboLanguage.Properties.DataSource = BusinessLanguage.GetLanguages().Execute(MergeOption.AppendOnly);
                cboLanguage.Properties.DataSource = BusinessLanguage.GetLanguages();
                cboLanguage.Properties.DisplayMember = "description";
                cboLanguage.Properties.ValueMember = "id";
                cboLanguage.Properties.Columns.Add(new LookUpColumnInfo("description"));
                cboLanguage.EditValue = 1;
            }
            catch { 
            }
        }

        /// <summary>
        /// Validate entries
        /// </summary>
        private bool ValidateEntries() {
            if (txtTitle.Text.Length < 1) {
                MessageBox.Show("Please enter a title first.", "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtTitle.Focus();
                return false;
            }

            return true;
        }

        #endregion

    }
}
