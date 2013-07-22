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

using ManagerApplication.Business;
using BrightVision.Model;
using BrightVision.Common.Business;

namespace ManagerApplication.Modules {
    public partial class AddTitleSetting : DevExpress.XtraEditors.XtraUserControl {
        #region Private Members
        private GridView m_objRefGridView = null;
        private string m_MessageCaption = "Add Title";
        private bool IsEditMode = false;
        private BrightPlatformEntities BPContext = null;
        #endregion

        #region Constructors
        public AddTitleSetting(GridView gridView, bool isEditMode) {
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            this.PopulateLanguageComboList();
            m_objRefGridView = gridView;
            IsEditMode = isEditMode;
            if (isEditMode) {
                m_MessageCaption = "Update Title";
                btnSave.Text = "Update";
                DataRow row = gridView.GetDataRow(gridView.FocusedRowHandle);
                if (row != null) {
                    txtTitle.Text = row["name"].ToString();
                    txtTitle.Tag = row["id"];
                    txtSSYK.Text = row["ssyk"].ToString();
                    cboLanguage.EditValue = row["language_id"];
                }
            }
        }
        #endregion

        #region Object Events

        private void btnSave_Click(object sender, EventArgs e) {
            if (ValidateEntries()) {
                DataTable dtSource = (DataTable)m_objRefGridView.GridControl.DataSource;
                if (dtSource != null) {
                    title objTitle = null;
                    int ssyk = 0;
                    if (IsEditMode) {
                        var titleid = (int)txtTitle.Tag;
                        var row = dtSource.Select("id <> " + titleid.ToString() + " AND name = '" + txtTitle.Text.Trim().Replace("'", "''") + "'");
                        if (row != null && row.Length > 0) {
                            MessageBox.Show("Title name already exists. Please try another name.");
                            this.ParentForm.DialogResult = DialogResult.None;
                            return;
                        } else {
                            DataRow dr = m_objRefGridView.GetFocusedDataRow();
                            if (dr != null) {
                                dr.BeginEdit();
                                dr["name"] = txtTitle.Text.Trim();
                                if (int.TryParse(txtSSYK.Text.Trim(), out ssyk))
                                    dr["ssyk"] = ssyk;
                                else
                                    dr["ssyk"] = DBNull.Value;
                                dr["language_id"] = (int)cboLanguage.EditValue;
                                dr.EndEdit();
                                dr.AcceptChanges();
                                dr.Table.AcceptChanges();
                            }
                            objTitle = BPContext.titles.FirstOrDefault(x => x.id == titleid);
                        }
                    }
                    //add new title
                    if (objTitle == null) {
                        objTitle = new title();
                        objTitle.date_created = DateTime.Now;
                    }
                    objTitle.name = txtTitle.Text.Trim();

                    if (int.TryParse(txtSSYK.Text.Trim(), out ssyk))
                        objTitle.ssyk = ssyk;
                    else
                        objTitle.ssyk = null;
                    objTitle.language_id = (int)cboLanguage.EditValue;

                    //add new title
                    if (!IsEditMode) {
                        BPContext.titles.AddObject(objTitle);
                    }
                    BPContext.SaveChanges();
                    //add new datarow to datasouce
                    if (!IsEditMode) {
                        DataRow dr = dtSource.NewRow();
                        dr["id"] = objTitle.id;
                        dr["name"] = objTitle.name;
                        if (objTitle.ssyk == null)
                            dr["ssyk"] = DBNull.Value;
                        else
                            dr["ssyk"] = objTitle.ssyk;
                        dr["language_id"] = objTitle.language_id;
                        dr["date_created"] = objTitle.date_created;
                        dtSource.Rows.Add(dr);
                        dtSource.AcceptChanges();
                    }
                    m_objRefGridView.MoveLastVisible();
                }
            } else {
                this.ParentForm.DialogResult = DialogResult.None;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Populate language combo list
        /// </summary>
        private void PopulateLanguageComboList() {
            try {
                cboLanguage.Properties.DataSource = null;
                cboLanguage.Properties.DataSource = BusinessLanguage.GetLanguages().Execute(MergeOption.AppendOnly);
                cboLanguage.Properties.DisplayMember = "description";
                cboLanguage.Properties.ValueMember = "id";
                cboLanguage.Properties.Columns.Add(new LookUpColumnInfo("description"));
                cboLanguage.EditValue = 1;
            } catch { }
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
