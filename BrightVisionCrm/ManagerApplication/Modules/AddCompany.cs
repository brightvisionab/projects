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
using BrightVision.Model;
using BrightVision.Common.Utilities;
using ManagerApplication.Business;
using BrightVision.Common.Business;

namespace ManagerApplication.Modules
{
    public partial class AddCompany : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Members
        public ManageCompanyContact m_objParentControl = null;
        #endregion

        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private string m_MessageBoxCaption = "Manager Application - Companies and Contacts";
        #endregion

        #region Constructors
        public AddCompany()
        {
            InitializeComponent();
        }
        #endregion

        #region Object Control Events

        private void cmdSave_Click(object sender, EventArgs e)
        {
            if (txtName.Text.Trim() == string.Empty) {
                MessageBox.Show("Company name is required.", "Add Company", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.ParentForm.DialogResult = DialogResult.None;
                return;
            }

            
            WaitDialog.Show(ParentForm, "Saving new account...");
            this.SaveCompany();
            WaitDialog.Close();
            MessageBox.Show("Company has been saved.", "System Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Logic to save customer record
        /// </summary>
        private void SaveCompany() 
        {
            var BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            BPContext.CommandTimeout = 0;
            var objAccount = new account {
                company_name = !string.IsNullOrEmpty(txtName.Text.Trim()) ? txtName.Text.Trim() : null,
                org_no = !string.IsNullOrEmpty(txtOrgNo.Text.Trim()) ? txtOrgNo.Text.Trim() : null,
                box_address = !string.IsNullOrEmpty(txtBoxAddress.Text.Trim()) ? txtBoxAddress.Text.Trim() : null,
                street_address = !string.IsNullOrEmpty(txtStreetAddress.Text.Trim()) ? txtStreetAddress.Text.Trim() : null,
                zipcode = !string.IsNullOrEmpty(txtZipcode.Text.Trim()) ? txtZipcode.Text.Trim() : null,
                country = !string.IsNullOrEmpty(txtCountry.Text.Trim()) ? txtCountry.Text.Trim() : null,
                county = !string.IsNullOrEmpty(txtCounty.Text.Trim()) ? txtCounty.Text.Trim() : null,
                municipality = !string.IsNullOrEmpty(txtMunicipality.Text.Trim()) ? txtMunicipality.Text.Trim() : null,
                city = !string.IsNullOrEmpty(txtCity.Text.Trim()) ? txtCity.Text.Trim() : null,
                telephone = !string.IsNullOrEmpty(txtTelephone.Text.Trim()) ? txtTelephone.Text.Trim() : null,
                telefax = !string.IsNullOrEmpty(txtTelefax.Text.Trim()) ? txtTelefax.Text.Trim() : null,
                www = !string.IsNullOrEmpty(txtSite.Text.Trim()) ? txtSite.Text.Trim() : null,
                parent_company = !string.IsNullOrEmpty(txtParentCompany.Text.Trim()) ? txtParentCompany.Text.Trim() : null,
                year_established = !string.IsNullOrEmpty(txtYearEstablished.Text.Trim()) ? txtYearEstablished.Text.Trim() : null,
                activity_code = !string.IsNullOrEmpty(txtActivityCode.Text.Trim()) ? txtActivityCode.Text.Trim() : null,
                activity_code_2 = !string.IsNullOrEmpty(txtActivityCode2.Text.Trim()) ? txtActivityCode2.Text.Trim() : null,
                currency = !string.IsNullOrEmpty(txtCurrency.Text.Trim()) ? txtCurrency.Text.Trim() : null,
                fiscal = !string.IsNullOrEmpty(txtFiscal1.Text.Trim()) ? txtFiscal1.Text.Trim() : null,
                turnover = !string.IsNullOrEmpty(txtTurnover1.Text.Trim()) ? decimal.Parse(txtTurnover1.Text.Trim()) : (decimal?) null,
                export = !string.IsNullOrEmpty(txtExport1.Text.Trim()) ? decimal.Parse(txtExport1.Text.Trim()) : (decimal?) null,
                result = !string.IsNullOrEmpty(txtResult1.Text.Trim()) ? decimal.Parse(txtResult1.Text.Trim()) : (decimal?) null,
                sales_abroad = !string.IsNullOrEmpty(txtSalesAbroad1.Text.Trim()) ? decimal.Parse(txtSalesAbroad1.Text.Trim()) : (decimal?)null,
                employees_total = !string.IsNullOrEmpty(txtEmpTotal1.Text.Trim()) ? int.Parse(txtEmpTotal1.Text.Trim()) : (int?)null,
                employees_abroad = !string.IsNullOrEmpty(txtEmpAbroad1.Text.Trim()) ? int.Parse(txtEmpAbroad1.Text.Trim()) : (int?)null,
                fiscal_2 = !string.IsNullOrEmpty(txtFiscal2.Text.Trim()) ? txtFiscal2.Text.Trim() : null,
                turnover_2 = !string.IsNullOrEmpty(txtTurnover2.Text.Trim()) ? decimal.Parse(txtTurnover2.Text.Trim()) : (decimal?)null,
                export_2 = !string.IsNullOrEmpty(txtExport2.Text.Trim()) ? decimal.Parse(txtExport2.Text.Trim()) : (decimal?)null,
                result_2 = !string.IsNullOrEmpty(txtResult2.Text.Trim()) ? decimal.Parse(txtResult2.Text.Trim()) : (decimal?)null,
                sales_abroad_2 = !string.IsNullOrEmpty(txtSalesAbroad2.Text.Trim()) ? decimal.Parse(txtSalesAbroad2.Text.Trim()) : (decimal?)null,
                employees_total_2 = !string.IsNullOrEmpty(txtEmpTotal2.Text.Trim()) ? int.Parse(txtEmpTotal2.Text.Trim()) : (int?)null,
                employees_abroad_2 = !string.IsNullOrEmpty(txtEmpAbroad2.Text.Trim()) ? int.Parse(txtEmpAbroad2.Text.Trim()) : (int?)null,
                fiscal_3 = !string.IsNullOrEmpty(txtFiscal3.Text.Trim()) ? txtFiscal3.Text.Trim() : null,
                turnover_3 = !string.IsNullOrEmpty(txtTurnover3.Text.Trim()) ? decimal.Parse(txtTurnover3.Text.Trim()) : (decimal?)null,
                export_3 = !string.IsNullOrEmpty(txtExport3.Text.Trim()) ? decimal.Parse(txtExport3.Text.Trim()) : (decimal?)null,
                result_3 = !string.IsNullOrEmpty(txtResult3.Text.Trim()) ? decimal.Parse(txtResult3.Text.Trim()) : (decimal?)null,
                sales_abroad_3 = !string.IsNullOrEmpty(txtSalesAbroad3.Text.Trim()) ? decimal.Parse(txtSalesAbroad3.Text.Trim()) : (decimal?)null,
                employees_total_3 = !string.IsNullOrEmpty(txtEmpTotal3.Text.Trim()) ? int.Parse(txtEmpTotal3.Text.Trim()) : (int?)null,
                employees_abroad_3 = !string.IsNullOrEmpty(txtEmpAbroad3.Text.Trim()) ? int.Parse(txtEmpAbroad3.Text.Trim()) : (int?)null,
                created_date = DateTime.Now,
                created_by = UserSession.CurrentUser.UserId,
                modified_date = DateTime.Now,
                active = true,
                last_modified_machine = UserSession.CurrentUser.ComputerName,
                last_modified_source = BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Add_Company
            };

            BPContext.accounts.AddObject(objAccount);
            BPContext.SaveChanges();
            //m_objParentControl.PopulateCompanyView(null, 1);
            m_objParentControl.PopulateCompanyView(objAccount.company_name, 1, true);            
            this.ParentForm.Close();
        }

        #endregion
    }
}
