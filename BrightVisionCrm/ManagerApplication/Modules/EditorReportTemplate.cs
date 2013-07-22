
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Data;
using ManagerApplication.Business;
using BrightVision.Common.Business;
using BrightVision.Model;
using BrightVision.Reporting.UI;
using BrightVision.Reporting.Template;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules
{
    public partial class EditorReportTemplate : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public EditorReportTemplate()
        {
            InitializeComponent();
            this.LoadAdditionalDataReportTemplates();
        }
        #endregion

        #region Subscribed Events
        private void _frm_AfterSave(object sender, AddReportTemplate.AddReportTemplateArgs e)
        {
            this.LoadAdditionalDataReportTemplates();
            for (int i = 0; i < gvReportTemplate.RowCount; i++)
            {
                CTAdditionalDataReportTemplate _item = gvReportTemplate.GetRow(i) as CTAdditionalDataReportTemplate;
                if (_item.id == e.ReportTemplateId)
                {
                    gvReportTemplate.FocusedRowHandle = i;
                    break;
                }
            }
        }
        private void _oReport_AfterSave(object sender, ReportUserDesigner.AfterSaveArgs e)
        {
            gvReportTemplate.SetRowCellValue(gvReportTemplate.FocusedRowHandle, "layout_config", e.efAdditionalDataReportTemplate.layout_config);
            m_oReport.AfterSave -= new ReportUserDesigner.AfterSaveEventHandler(_oReport_AfterSave);
        }
        #endregion

        #region Public Properties
        public TemplateProperty GetSamplePropertyTemplate()
        {
            var template = new TemplateProperty();
            template.IsFooterVisible = true;
            template.IsPageNumberVisible = true;
            template.IsEmptyDynamicValueVisible = false;
            template.DynamicProperty = new List<TemplateDynamicData>();
            template.DynamicProperty.Add(
                new TemplateDynamicData
                {
                    Type = TemplateDynamicType.TextField,
                }
            );
            template.DynamicProperty.Add(
                new TemplateDynamicData
                {
                    Type = TemplateDynamicType.PageBreak,

                }
            );
            template.DynamicProperty.Add(
                new TemplateDynamicData
                {
                    Type = TemplateDynamicType.Statistics,
                    Statistics = new StatisticsTemplate { }
                }
            );
            return template;
        }
        #endregion

        #region Private Properties
        private BrightPlatformEntities m_efDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 };
        private ReportUserDesigner m_oReport = null;
        private TemplateProperty templateData = new TemplateProperty();
        #endregion

        #region Public Methods
        public void ChangeActionButtonEnable(bool enable)
        {
            btnAddLayoutSetting.Enabled = enable;
            btnSaveLayoutSetting.Enabled = enable;
            simpleButtonReOrder.Enabled = enable;
            cboLayoutElement.Enabled = enable;
        }
        #endregion

        #region Private Methods
        private void LoadAdditionalDataReportTemplates()
        {
            try {
                gcReportTemplate.DataSource = null;
                gcReportTemplate.DataSource = ExportView.GetAdditionalDataReportTemplates();
            }
            catch { }
        }
        private void LoadLayoutSettings()
        {
            if (gvReportTemplate.RowCount < 1)
                return;

            templateData.DynamicProperty = null;
            CTAdditionalDataReportTemplate _item = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            if (string.IsNullOrEmpty(_item.data_config)) {
                reportTemplatePropertyGrid1.CreateDefaultLayoutSetting();
                return;
            }

            /**
             * auto load selected report template item data config.
             */
            templateData = SerializeUtility.DeserializeFromXml<TemplateProperty>(_item.data_config);
            reportTemplatePropertyGrid1.CreateReportLayoutSettings(templateData);
        }
        #endregion

        #region Object Events
        private void btnNewTemplate_Click(object sender, EventArgs e)
        {
            AddReportTemplate _frm = new AddReportTemplate();
            PopupDialog _dlg = new PopupDialog();
            _frm.AfterSave += new AddReportTemplate.AfterSaveEventHandler(_frm_AfterSave);
            _dlg.FormBorderStyle = FormBorderStyle.FixedSingle;
            _dlg.MinimizeBox = false;
            _dlg.MaximizeBox = false;
            _dlg.StartPosition = FormStartPosition.CenterScreen;
            _dlg.Text = "Add Report Template";
            _dlg.Controls.Add(_frm);
            _dlg.ClientSize = new Size(_frm.Width + 2, _frm.Height + 2);
            _dlg.ShowDialog(this.ParentForm);
        }
        private void cbxDefaultTemplate_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            CheckEdit _cbx = sender as CheckEdit;
            if (!_cbx.Checked && m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.is_default == true) != null) {
                MessageBox.Show("A default report template has already been set.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
        }
        private void cbxDefaultTemplate_EditValueChanged(object sender, EventArgs e)
        {
            WaitDialog.Show("Updating data.");
            CheckEdit _cbx = sender as CheckEdit;
            CTAdditionalDataReportTemplate _ReportTemplate = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            if (_ReportTemplate != null) {
                additional_data_report_templates _efeReportTemplate = m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplate.id);
                _efeReportTemplate.is_default = _cbx.Checked;
                _efeReportTemplate.modified_on = DateTime.Now;
                _efeReportTemplate.modified_by = UserSession.CurrentUser.UserId;
                m_efDbModel.additional_data_report_templates.ApplyCurrentValues(_efeReportTemplate);
                m_efDbModel.SaveChanges();
            }
            WaitDialog.Close();
        }
        private void btnDeleteTemplate_Click(object sender, EventArgs e)
        {
            /**
             * TODO: 
             * please kindly confirm to dave/johan on bahavoir on deleting templates.
             * what will happen if we delete a template with reports already using it as a template?
             * 
             * TEMPORARY SOLUTION:
             * will not allow to delete templates when its being referenced by reports.
             */

            if (gvReportTemplate.RowCount < 1)
                return;

            /**
             * check if there are existing reports being referenced to the selected report template.
             */
            DialogResult _dlg;
            WaitDialog.Show("Deleting data.");
            CTAdditionalDataReportTemplate _ReportTemplate = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            if (m_efDbModel.export_view_report_templates.FirstOrDefault(i => i.additional_data_report_template_id == _ReportTemplate.id) != null)
            {
                MessageBox.Show("Delete not allowed. Some reports are already using this template.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (_ReportTemplate.is_default)
            {
                string _msg = string.Format(
                    "You are deleting a default report template. You will need to set a default template after deleting for the reports to work well.{0}Are you sure to do this?",
                    Environment.NewLine
                 );
                _dlg = MessageBox.Show(_msg, "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            else
            {
                _dlg = MessageBox.Show("Are you sure to delete this template?", "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (_dlg == DialogResult.No)
                return;


            m_efDbModel.additional_data_report_templates.DeleteObject(
                m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplate.id)
            );
            m_efDbModel.SaveChanges();

            gvReportTemplate.DeleteRow(gvReportTemplate.FocusedRowHandle);
            gvReportTemplate.FocusedRowHandle = 0;
            gvReportTemplate.SelectRow(0);
            WaitDialog.Close();
        }
        private void btnEditReportDesigner_Click(object sender, EventArgs e)
        {
            if (gvReportTemplate.RowCount < 1)
                return;

            WaitDialog.Show("Loading report designer.");
            CTAdditionalDataReportTemplate _item = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            if (string.IsNullOrEmpty(_item.layout_config)) {
                m_oReport = new ReportUserDesigner(true);
                m_oReport.AfterSave += new ReportUserDesigner.AfterSaveEventHandler(_oReport_AfterSave);
                m_oReport.ModuleType = ReportUserDesigner.eModuleType.ReportTemplate;
                m_oReport.ReportTemplateId = _item.id;
                m_oReport.Show();
            }
            else {
                m_oReport = new ReportUserDesigner(_item.layout_config);
                m_oReport.AfterSave += new ReportUserDesigner.AfterSaveEventHandler(_oReport_AfterSave);
                m_oReport.ModuleType = ReportUserDesigner.eModuleType.ReportTemplate;
                m_oReport.ReportTemplateId = _item.id;
                m_oReport.ShowReportDesigner();
            }
            WaitDialog.Close();
        }
        private void btnAddLayoutSetting_Click(object sender, EventArgs e)
        {
            TemplateDynamicData templateDynamic = null;
            if (templateData.DynamicProperty == null)
            {
                templateData.DynamicProperty = new List<TemplateDynamicData>();
            }

            switch (cboLayoutElement.EditValue.ToString())
            {
                case "Page Break":
                    templateDynamic = new TemplateDynamicData() { Type = TemplateDynamicType.PageBreak, IsVisible = true };
                    reportTemplatePropertyGrid1.AddPageBreak(templateDynamic);
                    break;
                case "Text Field":
                    templateDynamic = new TemplateDynamicData
                    {
                        Type = TemplateDynamicType.TextField,
                        IsVisible = true,
                        Text = new TextField { Size = TextFieldFontSize.Normal, Style = TextFieldFontStyle.None }
                    };
                    reportTemplatePropertyGrid1.AddTextField(templateDynamic);
                    break;
                case "Statistical Component":
                    templateDynamic = new TemplateDynamicData
                    {
                        Type = TemplateDynamicType.Statistics,
                        IsVisible = true,
                        Statistics = new StatisticsTemplate
                        {
                            IsNullCategoryVisible = true,
                            IsTotalCountVisible = true,
                            GraphSize = GraphSize.Normal,
                            PercentageValueSortBy = PercentageValueSortingOption.PercentageAscending
                        }
                    };
                    reportTemplatePropertyGrid1.AddStatistics(templateDynamic);
                    break;
            }
        }
        private void simpleButtonReOrder_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Sorting items.");
            this.LoadLayoutSettings();
            WaitDialog.Close();
        }
        private void simpleButtonClear_Click(object sender, EventArgs e)
        {
            templateData = new TemplateProperty
            {
                DynamicProperty = new List<TemplateDynamicData>(),
                IsFooterVisible = true,
                IsPageNumberVisible = true,
                IsEmptyDynamicValueVisible = false
            };
            reportTemplatePropertyGrid1.CreateDefaultLayoutSetting(templateData);
        }

        /*
                private void repositoryItemComboBoxBoolean_EditValueChanged(object sender, EventArgs e)
                {
                    var row = vGridControlProperty.FocusedRow;
                    bool value = (sender as DevExpress.XtraEditors.ComboBoxEdit).EditValue.ToString() == "True" ? true : false;
                    var templateDataTemp = templateData;
                    if (row.Tag == null)
                    {
                        string fieldName = row.Properties.FieldName;
                        ReportLayoutSettingsUtility.SetValueTemplateProperty(value, fieldName, ref templateDataTemp);
                    }
                    if (row.Tag is TemplateDynamicData)
                    {
                        var templdateDynamicData = row.Tag as TemplateDynamicData;
                        if (templdateDynamicData.Type == TemplateDynamicType.TextField || templdateDynamicData.Type == TemplateDynamicType.PageBreak)
                        {
                            string fieldName = row.Properties.FieldName;
                            ReportLayoutSettingsUtility.SetValueTemplateDynamicData(value, fieldName, ref templdateDynamicData);
                        }
                        else if (templdateDynamicData.Type == TemplateDynamicType.Statistics)
                        {
                            string fieldName = row.Properties.FieldName;
                            if (fieldName == "IsVisible")
                            {
                                ReportLayoutSettingsUtility.SetValueTemplateDynamicData(value, fieldName, ref templdateDynamicData);
                            }
                            else
                            {
                                var stats = templdateDynamicData.Statistics;
                                ReportLayoutSettingsUtility.SetValueStatisticsProperty(value, fieldName, ref stats);
                            }
                        }
                    }
                }
                private void repositoryItemTextEdit_EditValueChanged(object sender, EventArgs e)
                {
                    var row = vGridControlProperty.FocusedRow;
                    string value = (sender as DevExpress.XtraEditors.TextEdit).EditValue.ToString();
                    if (row.Tag is TemplateDynamicData)
                    {
                        var templdateDynamicData = row.Tag as TemplateDynamicData;
                        if (templdateDynamicData.Type == TemplateDynamicType.TextField)
                        {
                            string fieldName = row.Properties.FieldName;
                            ReportLayoutSettingsUtility.SetValueTemplateDynamicData(value, fieldName, ref templdateDynamicData);
                        }
                        else if (templdateDynamicData.Type == TemplateDynamicType.Statistics)
                        {
                            string fieldName = row.Properties.FieldName;
                            var stats = templdateDynamicData.Statistics;
                            ReportLayoutSettingsUtility.SetValueStatisticsProperty(value, fieldName, ref stats);
                        }
                    }
                }
                private void repositoryItemTextEditNumberOnly_EditValueChanged(object sender, EventArgs e)
                {
                    var row = vGridControlProperty.FocusedRow;
                    string value = "0";
                    object textboxObj = (sender as DevExpress.XtraEditors.TextEdit).EditValue;
                    if (textboxObj != null)
                        value = textboxObj.ToString();

                    if (row.Tag is TemplateDynamicData)
                    {
                        var templdateDynamicData = row.Tag as TemplateDynamicData;
                        string fieldName = row.Properties.FieldName;
                        ReportLayoutSettingsUtility.SetValueTemplateDynamicData(value, fieldName, ref templdateDynamicData);
                    }
                }
        */

        private void btnSaveLayoutSetting_Click(object sender, EventArgs e)
        {
            /**
             * save xml layout to the selected report template.
             */
            WaitDialog.Show("Saving data.");
            string _xml = SerializeUtility.Serialize(templateData);
            long _ReportTemplateId = (gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate).id;
            additional_data_report_templates _efeReportTemplate = m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplateId);
            _efeReportTemplate.data_config = _xml;
            _efeReportTemplate.modified_by = UserSession.CurrentUser.UserId;
            _efeReportTemplate.modified_on = DateTime.Now;
            m_efDbModel.additional_data_report_templates.ApplyCurrentValues(_efeReportTemplate);
            m_efDbModel.SaveChanges();
            gvReportTemplate.SetRowCellValue(gvReportTemplate.FocusedRowHandle, "data_config", _xml);
            WaitDialog.Close();
            MessageBox.Show("Report layout settings saved.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void gvReportTemplate_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            WaitDialog.Show("Loading data.");
            this.LoadLayoutSettings();
            WaitDialog.Close();
        }
        #endregion
    }
}
