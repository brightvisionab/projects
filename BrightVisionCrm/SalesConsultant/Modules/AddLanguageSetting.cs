
#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SalesConsultant.Business;
using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
#endregion

namespace SalesConsultant.Modules
{
    public partial class AddLanguageSetting : DevExpress.XtraEditors.XtraUserControl
    {
        #region Private Members
        private ManageLanguageSetting m_objParentControl = null;
        private string m_MessageCaption = "Add Language Setting";
        #endregion

        #region Constructors
        public AddLanguageSetting(ManageLanguageSetting ParentControl)
        {
            InitializeComponent();
            m_objParentControl = ParentControl;
        }
        #endregion

        #region Object Events
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.SaveLanguage();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Validate entries
        /// </summary>
        private bool ValidateEntries()
        {
            if (txtLangaugeCode.Text.Length < 1)
            {
                MessageBox.Show("Please specify a language code", m_MessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLangaugeCode.Focus();
                return false;
            }
            else if (txtLanguageDescription.Text.Length < 1)
            {
                MessageBox.Show("Please specify a language description", m_MessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLanguageDescription.Focus();
                return false;
            }
            else if (ValidationUtility.HasNumericEntries(txtLangaugeCode.Text))
            {
                MessageBox.Show("Not a valid language code", m_MessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLangaugeCode.Focus();
                return false;
            }
            else if (ValidationUtility.HasSpecialChars(txtLangaugeCode.Text))
            {
                MessageBox.Show("Not a valid language code", m_MessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLangaugeCode.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save language
        /// </summary>
        private void SaveLanguage()
        {
            if (!ValidateEntries())
                return;

            language objLanguage = new language()
            {
                code = txtLangaugeCode.Text,
                name = txtLanguageDescription.Text
            };

            WaitDialog.Show(ParentForm, "Saving...");
            m_objParentControl.LanguageId = BusinessLanguage.SaveLanguage(objLanguage);
            m_objParentControl.PopulateLanguageView();
           WaitDialog.Close();
            MessageBox.Show("Successfully saved language setting", m_MessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.ParentForm.Close();
        }
        #endregion
    }
}
