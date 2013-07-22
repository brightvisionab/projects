
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Model;
using BrightVision.Common.Business;
using System.Linq;

namespace SalesConsultant.Modules
{
    public partial class AddReportTemplate : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public AddReportTemplate()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Events & Arguments
        public delegate void AfterSaveEventHandler(object sender, AddReportTemplateArgs e);
        public event AfterSaveEventHandler AfterSave;
        public class AddReportTemplateArgs : EventArgs {
            public long ReportTemplateId { get; set; }
        }
        #endregion

        #region Object Events
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (tbxReportTemplateName.Text.Length < 1)
                return;

            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                if (_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.template_name.Equals(tbxReportTemplateName.Text)) != null) {
                    MessageBox.Show("Report template name already exists.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var defaultTemplate = _efDbModel.additional_data_report_templates.FirstOrDefault(i => i.is_default);
                if (defaultTemplate == null) {
                    defaultTemplate = new additional_data_report_templates();
                    defaultTemplate.layout_config = string.Empty;
                    defaultTemplate.data_config = string.Empty;
                }
                WaitDialog.Show("Saving new data.");
                additional_data_report_templates _item = new additional_data_report_templates() {
                    template_name = tbxReportTemplateName.Text,
                    created_on = DateTime.Now,
                    created_by = UserSession.CurrentUser.UserId,
                    layout_config = defaultTemplate.layout_config
                };
                _efDbModel.additional_data_report_templates.AddObject(_item);
                _efDbModel.SaveChanges();

                if (AfterSave != null)
                    AfterSave(this, new AddReportTemplateArgs() { ReportTemplateId = _item.id });

                if (_item != null)
                    _efDbModel.Detach(_item);
            }

            WaitDialog.Close();
            this.ParentForm.Close();
        }
        private void tbxReportTemplateName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnSave.PerformClick();
        }
        #endregion
    }
}
