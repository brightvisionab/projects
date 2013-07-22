
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Data;
using DevExpress.XtraReports.UI;
using ManagerApplication.Business;
using BrightVision.Common.Business;
using BrightVision.Model;
using BrightVision.Reporting.UI;
using BrightVision.Reporting.Template;
using BrightVision.Common.Utilities;
using BrightVision.Reporting;
using System.IO;
using DevExpress.Data.Linq;

namespace ManagerApplication.Modules
{
    public partial class EditorDisplayViewReportTemplate : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public EditorDisplayViewReportTemplate()
        {
            WaitDialog.Show("Loading view.");
            InitializeComponent();
        }
        #endregion

        #region Public Events & Arguments
        #endregion

        #region Subscribed Events
        private void _oReport_AfterSave(object sender, ReportUserDesigner.AfterSaveArgs e)
        {
            gvReportTemplate.SetRowCellValue(gvReportTemplate.FocusedRowHandle, "layout_config", e.efAdditionalDataReportTemplate.layout_config);
            m_oReport.AfterSave -= new ReportUserDesigner.AfterSaveEventHandler(_oReport_AfterSave);
        }
        #endregion

        #region Public Properties
        public string CampaignInfo { get; set; }
        public string ViewInfo { get; set; }
        public int ViewConfigId { get; set; }
        public int SubCampaignId { get; set; }
        public long AdditionalDataReportTemplatesId { get; set; }
        public DataTable StaticDatasource { get; set; }
        public ReportDataSet ReportDatasetTemp { get; set; }
        public TemplateProperty GetSamplePropertyTemplate()
        {
            var template = new TemplateProperty();
            template.IsFooterVisible = true;
            template.IsPageNumberVisible = true;
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
        public List<string> ColumnNameList { get; set; }
        #endregion

        #region Private Properties
        private BrightPlatformEntities m_efDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 };
        private ReportUserDesigner m_oReport = null;
        private TemplateProperty templateData = new TemplateProperty();
        private export_view_report_templates m_efeExportViewTemplate = null;
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        private void SetInitialSelectedRow(export_view_report_templates template)
        {
            if (template.additional_data_report_template_id == null)
                return;
            for (int i = 0; i < gvReportTemplate.DataRowCount; i++)
            {
                long tmpid = long.Parse(gvReportTemplate.GetRowCellValue(i, "id").ToString());
                if (tmpid == template.additional_data_report_template_id.Value)
                {
                    gvReportTemplate.FocusedRowHandle = i;
                    //gvReportTemplate.SelectRow(i);
                    //gvReportTemplate.SetFocusedRowCellValue("id", tmpid);
                    AdditionalDataReportTemplatesId = tmpid;
                    return;
                }
            }
        }
        private void LoadAdditionalDataReportTemplates()
        {
            try
            {
                gcReportTemplate.DataSource = null;
                gcReportTemplate.DataSource = ExportView.GetAdditionalDataReportTemplates();
            }
            catch { }
        }
        private void LoadLayoutSettings()
        {
            if (gvReportTemplate.RowCount < 1)
                return;

            //vGridControlProperty.Rows.Clear();
            templateData.DynamicProperty = null;
            CTAdditionalDataReportTemplate _item = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            if (string.IsNullOrEmpty(_item.data_config))
            {
                templateData = new TemplateProperty
                {
                    DynamicProperty = new List<TemplateDynamicData>(),
                    IsFooterVisible = true,
                    IsPageNumberVisible = true,
                    IsEmptyDynamicValueVisible = false
                };

                reportTemplatePropertyGrid.CreateDefaultLayoutSetting(templateData);
                return;
            }
            reportTemplatePropertyGrid.ColumnList = ColumnNameList;
            /**
             * auto load selected report template item data config.
             */
            templateData = SerializeUtility.DeserializeFromXml<TemplateProperty>(_item.data_config);
            reportTemplatePropertyGrid.CreateReportLayoutSettings(templateData);
        }
        public string ToString(XtraReportDefaultTemplate template)
        {
            var ms = new MemoryStream();
            //xreport.SaveLayoutToXml(ms);
            template.SaveLayout(ms);
            ms.Position = 0;

            var sr = new StreamReader(ms, Encoding.Default);
            string _reportTemplate = sr.ReadToEnd();
            return _reportTemplate;
        }
        #endregion

        #region Object Events
        private void simpleButtonEditReportDesigner_Click(object sender, EventArgs e)
        {
            if (gvReportTemplate.RowCount < 1)
                return;

            WaitDialog.Show(ParentForm, "Loading report designer.");
            CTAdditionalDataReportTemplate _item = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            if (string.IsNullOrEmpty(_item.layout_config))
            {
                m_oReport = new ReportUserDesigner(true);
                m_oReport.AfterSave += new ReportUserDesigner.AfterSaveEventHandler(_oReport_AfterSave);
                m_oReport.ModuleType = ReportUserDesigner.eModuleType.ReportTemplate;
                m_oReport.ReportTemplateId = _item.id;
                WaitDialog.Close();
                m_oReport.Show();
            }
            else
            {
                m_oReport = new ReportUserDesigner(_item.layout_config);
                m_oReport.AfterSave += new ReportUserDesigner.AfterSaveEventHandler(_oReport_AfterSave);
                m_oReport.ModuleType = ReportUserDesigner.eModuleType.ReportTemplate;
                m_oReport.ReportTemplateId = _item.id;
                WaitDialog.Close();
                m_oReport.ShowReportDesigner();
            }
        }
        private void simpleButtonAddLayoutSetting_Click(object sender, EventArgs e)
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
                    reportTemplatePropertyGrid.AddPageBreak(templateDynamic);
                    break;
                case "Text Field":
                    templateDynamic = new TemplateDynamicData
                    {
                        Type = TemplateDynamicType.TextField,
                        IsVisible = true,
                        Text = new TextField { Size = TextFieldFontSize.Normal, Style = TextFieldFontStyle.None }
                    };
                    reportTemplatePropertyGrid.AddTextField(templateDynamic);
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
                    reportTemplatePropertyGrid.AddStatistics(templateDynamic);
                    break;
            }
        }
        private void simpleButtonReOrder_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Sorting items.");
            this.LoadLayoutSettings();
            WaitDialog.Close();
        }
        private void btnSaveToView_Click(object sender, EventArgs e)
        {
            /**
             * ready to use vars
             * ViewConfigId
             * SubCampaignId
             * m_efeExportViewTemplate -> object handle for export_view_report_templates
             */
            WaitDialog.Show(ParentForm, "Loading components...");
            #region save changes to additional_data_report_templates
            templateData.StatisticsDataSource = null;
            string _xml = SerializeUtility.Serialize(reportTemplatePropertyGrid.TemplateP);
            CTAdditionalDataReportTemplate _ReportTemplate = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            additional_data_report_templates _efeReportTemplate = m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplate.id);
            if (string.IsNullOrEmpty(_efeReportTemplate.layout_config))
                _efeReportTemplate.layout_config = BrightVision.Reporting.Business.FacadeReportTemplate.GetDefaultReportLayout(); //SerializeUtility.Serialize(new XtraReportDefaultTemplate());

            _efeReportTemplate.data_config = _xml;
            _efeReportTemplate.modified_by = UserSession.CurrentUser.UserId;
            _efeReportTemplate.modified_on = DateTime.Now;
            m_efDbModel.additional_data_report_templates.ApplyCurrentValues(_efeReportTemplate);
            m_efDbModel.SaveChanges();
            gvReportTemplate.SetRowCellValue(gvReportTemplate.FocusedRowHandle, "data_config", _xml);
            #endregion

            #region apply changes to export_view_template
            var exportViewTemplate = m_efDbModel.export_view_report_templates.FirstOrDefault(x => x.view_config_id == ViewConfigId);
            exportViewTemplate.modified_by = UserSession.CurrentUser.UserId;
            exportViewTemplate.modified_on = DateTime.Now;
            exportViewTemplate.additional_data_report_template_id = _efeReportTemplate.id;
            exportViewTemplate.layout_config = _efeReportTemplate.layout_config;
            m_efDbModel.export_view_report_templates.ApplyCurrentValues(exportViewTemplate);
            m_efDbModel.SaveChanges();
            #endregion

            #region set the AdditionalDataReportTemplatesId to the selected row  and fire the gvReportTemplate_RowStyle event by calling the refresh method
            gcReportTemplate.Refresh();
            SetInitialSelectedRow(exportViewTemplate);
            //have to this to redraw the gcReportTemplate
            gcReportTemplate.Focus();
            #endregion
            WaitDialog.Close();
            MessageBox.Show("Report layout settings saved.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void btnShowReport_Click(object sender, EventArgs e)
        {
            /**
             * ready to use vars
             * ViewConfigId
             * SubCampaignId
             * m_efeExportViewTemplate -> object handle for export_view_report_templates
             */

            WaitDialog.Show(ParentForm, "Loading report...");
            if (StaticDatasource.Rows.Count == 0)
            {
                WaitDialog.Close();
                return;
            }

            // current report template selected row
            XtraReportDefaultTemplate _report;
            var _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 };
            var _ReportTemplate = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            var _efeReportTemplate = m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplate.id);
            templateData.StatisticsDataSource = this.StaticDatasource;
            string template = _efeReportTemplate.layout_config;
            if (template == null)
                template = ToString(new XtraReportDefaultTemplate());

            _report = new XtraReportDefaultTemplate(template, templateData);
            _report.DataSource = this.ReportDatasetTemp;
            WaitDialog.Close();
            _report.ShowPreviewDialog();
            
        }
        private void simpleButtonClear_Click(object sender, EventArgs e)
        {
            templateData = new TemplateProperty
            {
                DynamicProperty = new List<TemplateDynamicData>(),
                IsFooterVisible = true,
                IsPageNumberVisible = true
            };
            reportTemplatePropertyGrid.CreateDefaultLayoutSetting(templateData);
        }
        private void cbxDefaultTemplate_EditValueChanged(object sender, EventArgs e)
        {
            WaitDialog.Show("Updating data.");
            CheckEdit _cbx = sender as CheckEdit;
            CTAdditionalDataReportTemplate _ReportTemplate = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            additional_data_report_templates _efeReportTemplate = m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplate.id);
            _efeReportTemplate.is_default = _cbx.Checked;
            _efeReportTemplate.modified_on = DateTime.Now;
            _efeReportTemplate.modified_by = UserSession.CurrentUser.UserId;
            m_efDbModel.additional_data_report_templates.ApplyCurrentValues(_efeReportTemplate);
            m_efDbModel.SaveChanges();
            WaitDialog.Close();
        }
        private void cbxDefaultTemplate_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            CheckEdit _cbx = sender as CheckEdit;
            if (!_cbx.Checked && m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.is_default == true) != null)
            {
                MessageBox.Show("A default report template has already been set.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
        }
        private void gvReportTemplate_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            WaitDialog.Show("Loading data.");
            this.LoadLayoutSettings();
            WaitDialog.Close();
        }
        private void gvReportTemplate_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                long Id = long.Parse(gvReportTemplate.GetRowCellValue(e.RowHandle, "id").ToString());
                if (AdditionalDataReportTemplatesId == Id)
                {
                    e.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
                }
            }
        }
        private void EditorDisplayViewReportTemplate_Load(object sender, EventArgs e)
        {
            try
            {
                templateData = new TemplateProperty
                {
                    DynamicProperty = new List<TemplateDynamicData>(),
                    IsFooterVisible = true,
                    IsPageNumberVisible = true
                };
                reportTemplatePropertyGrid.CreateDefaultLayoutSetting(templateData);
                this.LoadAdditionalDataReportTemplates();
                lblCampaignInformation.Text = string.Format(
                    "Current Campaign: {0}{2}Current View: {1}{3}",
                    CampaignInfo,
                    ViewInfo,
                    Environment.NewLine,
                    Environment.NewLine
                );

                /**
                 * create table entry for this view if not yet existing.
                 */
                m_efeExportViewTemplate = m_efDbModel.export_view_report_templates.FirstOrDefault(i =>
                    i.sub_campaign_id == SubCampaignId &&
                    i.view_config_id == ViewConfigId
                );

                if (m_efeExportViewTemplate == null)
                {
                    string _LayoutConfig = null;
                    additional_data_report_templates _efeReportTemplate = m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.is_default == true);
                    if (_efeReportTemplate != null)
                        _LayoutConfig = _efeReportTemplate.layout_config;

                    m_efeExportViewTemplate = new export_view_report_templates()
                    {
                        sub_campaign_id = SubCampaignId,
                        view_config_id = ViewConfigId,
                        layout_config = _LayoutConfig,
                        created_on = DateTime.Now,
                        created_by = UserSession.CurrentUser.UserId,
                        modified_on = DateTime.Now,
                        modified_by = UserSession.CurrentUser.UserId
                    };
                    if (_efeReportTemplate != null)
                    {
                        m_efeExportViewTemplate.additional_data_report_template_id = _efeReportTemplate.id;
                        m_efDbModel.export_view_report_templates.AddObject(m_efeExportViewTemplate);
                        m_efDbModel.SaveChanges();
                    }
                }
                else if (m_efeExportViewTemplate.additional_data_report_template_id == null || m_efeExportViewTemplate.layout_config == null)
                {
                    string _LayoutConfig = null;
                    additional_data_report_templates _efeReportTemplate = m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.is_default == true);
                    if (_efeReportTemplate != null)
                        _LayoutConfig = _efeReportTemplate.layout_config;

                    m_efeExportViewTemplate = new export_view_report_templates()
                    {
                        sub_campaign_id = SubCampaignId,
                        view_config_id = ViewConfigId,
                        layout_config = _LayoutConfig,
                        additional_data_report_template_id = _efeReportTemplate.id,
                        created_on = DateTime.Now,
                        created_by = UserSession.CurrentUser.UserId,
                        modified_on = DateTime.Now,
                        modified_by = UserSession.CurrentUser.UserId
                    };
                    m_efDbModel.export_view_report_templates.AddObject(m_efeExportViewTemplate);
                    m_efDbModel.SaveChanges();
                }

                SetInitialSelectedRow(m_efeExportViewTemplate);
                WaitDialog.Close();
            }
            catch (Exception ee)
            {

            }
        }
        #endregion
    }
}
