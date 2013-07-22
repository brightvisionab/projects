using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraVerticalGrid.Rows;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using BrightVision.Model;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Reporting.UI;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ManagerApplication.Business;
using BrightVision.Reporting.Template;

namespace ManagerApplication.Modules 
{
    public partial class ViewConfiguration : XtraUserControl 
    {
        #region Member Variables
        private BrightPlatformEntities m_efDbContext = null;
        private DataTable dtAvailableColumns = null;
        private DataTable dtColumnsInView = null;
        private view_configuration m_efoViewConfig = null;
        private List<DialogField> m_oDialogFields = null;

        private const string OrderFieldName = "position";
        private string m_ViewName = string.Empty;
        private int m_iSubcampaignID = 0;

        private bool IsNewConfig = false;
        private bool IsSaved = true;

        private bool m_IsNew = true;
        private bool m_HasSelection = false;

        private bool m_LoadedConfig = false;
        private bool m_LoadedLayout = false;
        private bool m_LoadedParameters = false;
        private bool m_GoDefault = false;

        private ReportConfig m_ConfigGrid = null;
        private ReportConfig m_ConfigLayout = null;
        private ReportConfig m_ConfigParameter = null;

        private ReportUserDesigner m_oReport = null;
        private TemplateProperty templateData = new TemplateProperty();

        private class ReportConfig {
            public bool Complete { get; set; }
            public bool OnEditMode { get; set; }
        }
        #endregion

        #region Constructor
        public ViewConfiguration() 
        {
            this.Visible = false;
            InitializeComponent();
            this.layoutControl1.AllowCustomizationMenu = false;
            m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            WaitDialog.Show(ParentForm, "Loading subcampaigns...");                        
            BindSubcampaignLookup();
            SetValidationRules();
            this.Visible = true;
            btnMerge.Enabled = false;
            btnPreviewView.Enabled = false;
            m_oDialogFields = new List<DialogField>();
            WaitDialog.Close();
        } 
        #endregion

        #region Public Events & Args
        public delegate void btnPreviewViewOnClickEventHandler(int pViewConfigId);
        public event btnPreviewViewOnClickEventHandler btnPreviewViewOnClick;
        #endregion

        #region Subscribed Events
        private void m_oReport_AfterSave(object sender, ReportUserDesigner.AfterSaveArgs e)
        {
            if (m_oReport.ModuleType == ReportUserDesigner.eModuleType.ReportTemplate) {
                gvReportTemplate.SetRowCellValue(gvReportTemplate.FocusedRowHandle, "layout_config", e.efAdditionalDataReportTemplate.layout_config);
                if (!string.IsNullOrEmpty(e.efAdditionalDataReportTemplate.layout_config))
                    gvReportTemplate.SetRowCellValue(gvReportTemplate.FocusedRowHandle, "has_layout", true);
            }

            else if (m_oReport.ModuleType == ReportUserDesigner.eModuleType.ViewConfiguration) {
                if (!string.IsNullOrEmpty(e.efViewConfiguration.report_layout_config)) {
                    m_ConfigLayout.Complete = true;
                    tbpgeLayoutConfig.Image = ManagerApplication.Properties.Resources.save_ok;
                    btnEditReportTemplate.Image = ManagerApplication.Properties.Resources.view_config_completed;
                    m_efoViewConfig.report_layout_config = e.efViewConfiguration.report_layout_config;
                    m_efDbContext.view_configuration.ApplyCurrentValues(m_efoViewConfig);
                    m_efDbContext.SaveChanges();
                }
            }

            m_oReport.AfterSave -= new ReportUserDesigner.AfterSaveEventHandler(m_oReport_AfterSave);
        }
        private void _frm_AfterSave(object sender, AddReportTemplate.AddReportTemplateArgs e)
        {
            this.LoadLayoutConfigTemplates();
            for (int i = 0; i < gvReportTemplate.RowCount; i++) {
                CTAdditionalDataReportTemplate _item = gvReportTemplate.GetRow(i) as CTAdditionalDataReportTemplate;
                if (_item.id == e.ReportTemplateId) {
                    gvReportTemplate.FocusedRowHandle = i;
                    break;
                }
            }
        }
        private void _control_AfterSave(CreateBlankView.AfterSaveArgs e)
        {
            m_ViewName = e.ViewName;
            m_IsNew = e.IsNew;

            if (m_IsNew) {
                WaitDialog.Show("Saving configuration...");
                if (this.SaveConfiguration(true))
                    m_LoadedConfig = true; // so that the next save, the popup will not show again.

                #region Commented Code
                //m_ConfigGrid.Complete = true;
                //tbpgeGridConfig.Image = ManagerApplication.Properties.Resources.save_ok;

                //WaitDialog.Show(ParentForm, "Loading Data Views...");
                //lookUpEditViewList.EditValue = null;
                //lookUpEditViewList.Properties.ReadOnly = true;
                //lookUpEditSubcampaign.Properties.ReadOnly = true;

                ////lcgHeaderControls.Enabled = true;
                //lcgLeft.Enabled = true;
                //lcgRight.Enabled = true;
                //txtViewName.Text = "";

                //BindGridAvailableColumns();
                //BindGridColumnsInView();
                ////var objSen = sender as SimpleButton;
                ////if (objSen != null) objSen.Enabled = false;
                //btnCreateBlankView.Enabled = false;
                //btnLoadView.Enabled = false;
                //btnDeleteView.Enabled = false;
                ////btnDuplicateView.Enabled = false;
                //btnShowXmlViewConfig.Enabled = true;
                //IsNewConfig = true;
                //viewConfig = null;
                #endregion
            }
            else {
                WaitDialog.Show("Renaming View ...");
                m_efoViewConfig.name = m_ViewName;
                m_efoViewConfig.modified_by = UserSession.CurrentUser.UserId;
                m_efoViewConfig.date_modified = DateTime.Now;
                try {
                    m_efDbContext.SaveChanges();
                    int configID = m_efoViewConfig.id;
                    this.BindViewList();
                    lookUpEditViewList.EditValue = configID;
                    //btnDeleteView.Enabled = true;
                    //btnShowXmlViewConfig.Enabled = true;
                    m_ConfigGrid.Complete = true;
                    tbpgeGridConfig.Image = ManagerApplication.Properties.Resources.save_ok;
                    IsNewConfig = false;
                    IsSaved = true;

                    /**
                     * reload view list combo box
                     */
                    this.lookUpEditViewList.EditValueChanged -= new System.EventHandler(this.lookUpEditViewList_EditValueChanged);
                    int _EditValue = lookUpEditViewList.EditValue == null ? 0 : (int)lookUpEditViewList.EditValue;
                    this.BindViewList(_EditValue);
                    this.lookUpEditViewList.EditValueChanged += new System.EventHandler(this.lookUpEditViewList_EditValueChanged);

                    btnLoadView.Enabled = true;
                    btnDeleteView.Enabled = true;
                    btnRenameView.Enabled = true;
                    btnShowXmlViewConfig.Enabled = true;
                }
                catch {
                }
            }            
            WaitDialog.Close();
        }
        private void _frmParamsTab_AfterSave(object sender, AddReportTemplate.AddReportTemplateArgs e)
        {
            this.LoadParameterConfigTemplates();
            for (int i = 0; i < gvReportTemplateParameterTab.RowCount; i++) {
                CTAdditionalDataReportTemplate _item = gvReportTemplateParameterTab.GetRow(i) as CTAdditionalDataReportTemplate;
                if (_item.id == e.ReportTemplateId) {
                    gvReportTemplateParameterTab.FocusedRowHandle = i;
                    break;
                }
            }
        }
        #endregion

        #region Public Members
        public List<DialogField> DialogFields { get { return m_oDialogFields; } }
        #endregion

        #region Private Methods
        private void SetStatisticsAvailableColumns()
        {
            /**
             * get available columns for the stat page.
             */
            List<ReportTemplatePropertyGrid.StatColumn> _Columns = new List<ReportTemplatePropertyGrid.StatColumn>();
            for (int i = 0; i < gvColumnView.RowCount; i++) {
                _Columns.Add(new ReportTemplatePropertyGrid.StatColumn() {
                    PositionIndex = Convert.ToInt32(gvColumnView.GetRowCellValue(i, "position")),
                    DisplayName = gvColumnView.GetRowCellValue(i, "display_name").ToString(),
                    LabelName = gvColumnView.GetRowCellValue(i, "label_name").ToString()
                });
            }
            if (_Columns.Count > 0)
                vgridReportParameter.SetAvaiableColumns(_Columns);
        }
        private void SetPreviewReport()
        {
            btnPreviewView.Enabled = false;
            if (m_ConfigGrid.Complete && m_ConfigLayout.Complete && m_ConfigParameter.Complete)
                btnPreviewView.Enabled = true;
        }
        private void LoadParameterSettings(bool pLoadFromTemplate = true)
        {
            if (gvReportTemplateParameterTab.RowCount < 1)
                return;

            string _DataConfig = string.Empty;
            templateData.DynamicProperty = null;

            if (pLoadFromTemplate) {
                CTAdditionalDataReportTemplate _item = gvReportTemplateParameterTab.GetFocusedRow() as CTAdditionalDataReportTemplate;
                _DataConfig = GetDataConfig(_item.id);
            }
            else {
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    view_configuration _eftViewConfig = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_efoViewConfig.id);
                    _DataConfig = _eftViewConfig.report_data_config;
                    _efDbContext.Detach(_eftViewConfig);
                }
            }

            if (string.IsNullOrEmpty(_DataConfig)) {
                vgridReportParameter.CreateDefaultLayoutSetting();
                return;
            }

            /**
             * auto load selected report template item data config.
             */
            templateData = SerializeUtility.DeserializeFromXml<TemplateProperty>(_DataConfig);
            vgridReportParameter.CreateReportLayoutSettings(templateData);
            this.SetStatisticsAvailableColumns();
        }

        private string GetDataConfig(long p)
        {
            string configData= string.Empty;
            using (BrightPlatformEntities entities = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var res = entities.additional_data_report_templates.FirstOrDefault(e => e.id == p);
                if(res !=null){
                    configData = res.data_config;
                    entities.Detach(res);
                }
                

            }
            return configData;

        }
        private void LoadParameterConfigTemplates()
        {
            try {
                gcReportTemplateParameterTab.DataSource = null;
                gcReportTemplateParameterTab.DataSource = ExportView.GetAdditionalDataReportTemplates();
            }
            catch { }
        }
        private void LoadLayoutConfigTemplates()
        {
            try {
                gcReportTemplate.DataSource = null;
                gcReportTemplate.DataSource = ExportView.GetAdditionalDataReportTemplates();
                btnLoadLayoutTemplateToReport.Enabled = true;
            }
            catch { }
        }
        private void BindViewList(int pDefaultEditValue = 0) 
        {
            //viewConfig = null;
            var editval = lookUpEditSubcampaign.EditValue;
            if (editval != null) {                
                m_iSubcampaignID = (int)lookUpEditSubcampaign.EditValue;
                
                //lcgHeaderControls.Enabled = true;
                
                var listView = m_efDbContext.view_configuration
                    .Where(x => x.subcampaign_id == m_iSubcampaignID && x.MGC == false)
                    .Select(y=> new { id = y.id, name= y.name})
                    .ToList();

                lookUpEditViewList.EditValue = null;
                lookUpEditViewList.ItemIndex = -1;
                lookUpEditViewList.Properties.Columns.Clear();
                lookUpEditViewList.Properties.DataSource = listView;
                lookUpEditViewList.EditValue = null;
                lookUpEditViewList.ItemIndex = -1;
                lookUpEditViewList.Properties.DisplayMember = "name";
                lookUpEditViewList.Properties.ValueMember = "id";
                lookUpEditViewList.Properties.Columns.Add(new LookUpColumnInfo("name"));
                lookUpEditViewList.Properties.ShowHeader = false;
                lookUpEditViewList.Properties.ReadOnly = false;
                
                btnLoadView.Enabled = false;
                btnRenameView.Enabled = false;
                btnDeleteView.Enabled = false;
                //btnPreviewView.Enabled = false;
                
                if (listView == null && listView.Count < 1) {
                    gcAvailableColumns.DataSource = null;
                    gcColumnView.DataSource = null;
                    lcgLeft.Enabled = false;
                    lcgRight.Enabled = false;
                    tcViewConfig.Enabled = false;
                    m_ViewName = string.Empty;
                }

                btnShowXmlViewConfig.Enabled = false;
                btnCreateBlankView.Enabled = true;
                btnCancel.Enabled = true;
                //btnLoadLayoutTemplateToReport.Enabled = false;
                //btnLoadView.Enabled = false;
                //btnDeleteView.Enabled = false;

                if (pDefaultEditValue > 0)
                    lookUpEditViewList.EditValue = pDefaultEditValue; 
            }
        }
        private void PopulateDataTableAvailableColumns() {
            //create columns
            dtAvailableColumns = new DataTable("available_columns");
            DataColumn dtIndex = new DataColumn("id", typeof(int));
            dtIndex.AutoIncrement = true;
            dtIndex.AllowDBNull = false;
            dtIndex.AutoIncrementSeed = 1;
            dtIndex.AutoIncrementStep = 1;
            dtAvailableColumns.Columns.Add(dtIndex);
            dtAvailableColumns.Columns.Add("dialog_id", typeof(int));
            dtAvailableColumns.Columns.Add("questionlayout_id", typeof(int)); //need this to identify the answer to a specific component questionnaire
            //dtAvailableColumns.Columns.Add("questiontextlanguage_id", typeof(int)); //need this to identify the real question based on question languageid
            dtAvailableColumns.Columns.Add("source", typeof(string));
            dtAvailableColumns.Columns.Add("component_type", (typeof(string)));
            dtAvailableColumns.Columns.Add("field_name", (typeof(string))); //constant name to identify the condition to search for its value.
            dtAvailableColumns.Columns.Add("external_value", typeof(string)); //this will hold the values that are defined in the dialog json not in database.
            dtAvailableColumns.Columns.Add("field_index", typeof(string));
            dtAvailableColumns.Columns.Add("label_name", typeof(string));
            dtAvailableColumns.Columns.Add("field_type", typeof(string));            
            dtAvailableColumns.Columns.Add("default", typeof(string));
        }
        private void PopulateDataTableColumnsInView() {            
            //create columns
            dtColumnsInView = new DataTable("item");            
            DataColumn dtCol = new DataColumn();
            dtCol.ColumnName = "position";
            dtCol.ColumnMapping = MappingType.Attribute;
            dtCol.DataType = typeof(int);            
            dtColumnsInView.Columns.Add(dtCol);
            dtCol = new DataColumn();
            dtCol.ColumnName = "id";
            dtCol.ColumnMapping = MappingType.Attribute;
            dtCol.DataType = typeof(int);
            dtColumnsInView.Columns.Add(dtCol);
            dtColumnsInView.Columns.Add("merge_data", typeof(string));//store merge data row information in this column.
            dtColumnsInView.Columns.Add("dialog_id", typeof(string));
            dtColumnsInView.Columns.Add("questionlayout_id", typeof(string)); //need this to identify the answer to a specific component questionnaire
            //dtColumnsInView.Columns.Add("questiontextlanguage_id", typeof(string)); //need this to identify the real question based on question languageid
            dtColumnsInView.Columns.Add("source", typeof(string));
            dtColumnsInView.Columns.Add("component_type", typeof(string));
            dtColumnsInView.Columns.Add("field_name", typeof(string)); //constant name to identify the condition to search for its value.
            dtColumnsInView.Columns.Add("external_value", typeof(string)); //this will hold the values that are defined in the dialog json not in database.
            dtColumnsInView.Columns.Add("field_index", typeof(string));
            dtColumnsInView.Columns.Add("label_name", typeof(string));
            dtColumnsInView.Columns.Add("display_name", typeof(string));
            dtColumnsInView.Columns.Add("column_width", typeof(string));
            dtColumnsInView.Columns.Add("sort1", typeof(string));
            dtColumnsInView.Columns.Add("sort2", typeof(string));
        }
        private void BindGridAvailableColumns() { 
            if(m_iSubcampaignID <= 0 ) return;

            PopulateDataTableAvailableColumns();
            var diag = m_efDbContext.dialogs.Where(x => x.is_active == true).FirstOrDefault(x => x.subcampaign_id == m_iSubcampaignID);
            if (diag != null) {
                if (diag.dialog_text_json.Length > 0) {
                    var jaDiag = JArray.Parse(diag.dialog_text_json);
                    CampaignQuestionnaire CQ = null;
                    IList<AnswerOption> listAO = null;
                    List<MultipleChoiceValue> listMCV = null;
                    List<OtherChoice> listOtherChoice = null;
                    Settings CQSettings = null;
                    string scheduleType = string.Empty;
                    int dialog_id = diag.id,  questionlayout_id = 0;//, questiontextlanguage_id = 0;                
                    int counter = 0;
                    bool accountLevel = false;
                    string sourceLabel = string.Empty;
                    #region Dialog Fields
                    jaDiag.ForEach(delegate(JToken jt) {
                        /**
                         * [@jeff 07.04.2012]: https://brightvision.jira.com/browse/PLATFORM-1589
                         * added validation to strip invalid chars from json data.
                         */
                        string _jsonData = ValidationUtility.StripJsonInvalidChars(JsonConvert.ToString(jt.ToString(Newtonsoft.Json.Formatting.None)).Unescape());
                        CQ = CampaignQuestionnaire.InstanciateWith(_jsonData);
                        if (CQ != null) {
                            CQSettings = CQ.Form.Settings;
                            int.TryParse(CQSettings.DataBindings.questionlayout_id, out questionlayout_id);
                            //int.TryParse(CQSettings.DataBindings.questions_text_language_id, out questiontextlanguage_id);
                            listAO = CQSettings.AnswerOptions;
                            if (listAO != null && listAO.Count >= 0) {
                                //answeroption fields                 
                                accountLevel = CQSettings.DataBindings.account_level;
                                sourceLabel = accountLevel ? "Dialog Account Level" : "Dialog Contact Level";

                                if (CQ.Type.ToLower() == QuestionTypeConstants.Dropbox) {
                                    AddDataRowDialogFields(dialog_id, questionlayout_id, accountLevel, "Dropbox", 
                                        CQSettings.Label, CQSettings.QuestionText, CQSettings.QuestionHelp, CQSettings.BVOwnership.ToString(),
                                        CQSettings.CustomerOwnership.ToString(),CQSettings.PlotDoneStatus, CQSettings.Priority);
                                    counter = 0;
                                    foreach (AnswerOption opt in listAO) {
                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, "Dropbox", "DropboxValue", counter.ToString(), "",
                                            opt.TextPrefix, "Dropdown", "DropboxValue");
                                        counter++;
                                    }
                                } else if (CQ.Type.ToLower() == QuestionTypeConstants.Textbox) {
                                    AddDataRowDialogFields(dialog_id, questionlayout_id, accountLevel, "Textbox",
                                        CQSettings.Label, CQSettings.QuestionText, CQSettings.QuestionHelp, CQSettings.BVOwnership.ToString(),
                                        CQSettings.CustomerOwnership.ToString(), CQSettings.PlotDoneStatus, CQSettings.Priority);                                
                                    foreach (AnswerOption opt in listAO) {
                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, "Textbox", "InputValue", "", "", 
                                            (!string.IsNullOrEmpty(opt.TextPrefix) ? opt.TextPrefix : "InputValue"), "Text", "InputValue");
                                    }
                                } else if (CQ.Type.ToLower() == QuestionTypeConstants.MultipleChoice) {
                                    AddDataRowDialogFields(dialog_id, questionlayout_id, accountLevel, "MultipleChoice",
                                        CQSettings.Label, CQSettings.QuestionText, CQSettings.QuestionHelp, CQSettings.BVOwnership.ToString(),
                                        CQSettings.CustomerOwnership.ToString(), CQSettings.PlotDoneStatus, CQSettings.Priority);
                                    listMCV = listAO[0].MultipleChoiceValues;
                                    if (listMCV != null) {
                                        counter=0;
                                        foreach (MultipleChoiceValue mcv in listMCV) {
                                            AddDataRow(dialog_id, questionlayout_id, sourceLabel, "MultipleChoice", "MultipleChoiceValue", counter.ToString(), "", 
                                                mcv.TextPrefix, "Multiple Choice", "MultipleChoiceValue");
                                            counter++;
                                        }
                                    
                                    }
                                    listOtherChoice = listAO[0].OtherChoices;
                                    if (listOtherChoice != null) {
                                        counter = 0;
                                        foreach (OtherChoice oChoice in listOtherChoice) {
                                            AddDataRow(dialog_id, questionlayout_id, sourceLabel, "MultipleChoice", "MultipleChoiceOtherChoiceValue", counter.ToString(), "",
                                                oChoice.TextPrefix, "Text", "InputValue");
                                            counter++;
                                        }
                                    
                                    }
                                } else if (CQ.Type.ToLower() == QuestionTypeConstants.Schedule) {
                                    if (listAO[0].ScheduleType != null) {
                                        if (listAO[0].ScheduleType.ScheduleTypeSelectedValue == "Meeting") {
                                            scheduleType = "Meeting Schedule";
                                        } else if (listAO[0].ScheduleType.ScheduleTypeSelectedValue == "Seminar") {
                                            scheduleType = "Seminar Schedule";
                                        } else if (listAO[0].ScheduleType.ScheduleTypeSelectedValue == "Webinar") {
                                            scheduleType = "Webinar Schedule";
                                        }
                                        AddDataRowDialogFields(dialog_id, questionlayout_id, accountLevel, scheduleType,
                                            CQSettings.Label, CQSettings.QuestionText, CQSettings.QuestionHelp, CQSettings.BVOwnership.ToString(),
                                            CQSettings.CustomerOwnership.ToString(), CQSettings.PlotDoneStatus, CQSettings.Priority);
                                        if (listAO[0].ScheduleType != null)
                                            AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "ScheduleType","", "",
                                                listAO[0].ScheduleType.TextPrefix, "Text", "Schedule Type");
                                        else
                                            AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "ScheduleType", "", "",
                                                "Schedule Type", "Text", "Schedule Type");

                                        if (listAO[0].ScheduleSalesPerson != null)
                                            AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "ResourceName", "", "",
                                                listAO[0].ScheduleSalesPerson.TextPrefix, "Text", "Sales Person");
                                        else
                                            AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "ResourceName", "", "",
                                                "Sales Person", "Text", "Sales Person");

                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "ResourceID", "", "",
                                            "ResourceID", "Reference", "resource_id");
                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "ScheduleID", "", "",
                                            "ScheduleID", "Reference", "schedule_id");

                                        //schedule details
                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "Subject", "", "",
                                            "Subject", "Text", "Subject");
                                        if (listAO[0].ScheduleType != null) {
                                            if (listAO[0].ScheduleType.ScheduleTypeSelectedValue == "Meeting" ||
                                                listAO[0].ScheduleType.ScheduleTypeSelectedValue == "Seminar") {
                                                    AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "Location", "", "",
                                                    "Location", "Text", "Location");
                                            }
                                        }
                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "StartTime", "", "", "Start time", "DateTime", "Start time");
                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "EndTime", "", "", "End time", "DateTime", "End time");
                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "Description", "", "", "Description", "Text", "Description");
                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "AllDayEvent", "", "", "All day event", "Boolean", "All day event");
                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "Attendies", "", "", listAO[0].AttendiesLabel, "Multiselect", "Attendies");
                                        listOtherChoice = listAO[0].OtherChoices;
                                        if (listOtherChoice != null) {
                                            counter = 0;
                                            foreach (OtherChoice oChoice in listOtherChoice) {
                                                AddDataRow(dialog_id, questionlayout_id, sourceLabel, scheduleType, "ScheduleOtherChoiceValue", counter.ToString(), "", oChoice.TextPrefix, "Text", "InputValue");
                                                counter++;
                                            }                                        
                                        }
                                    }
                                }
                                else if (CQ.Type.ToLower() == QuestionTypeConstants.SmartText){
                                    AddDataRowDialogFields(dialog_id, questionlayout_id, accountLevel, "SmartText",
                                        CQSettings.Label, CQSettings.QuestionText, CQSettings.QuestionHelp, CQSettings.BVOwnership.ToString(),
                                        CQSettings.CustomerOwnership.ToString(), CQSettings.PlotDoneStatus, CQSettings.Priority);
                                    foreach (AnswerOption opt in listAO)
                                    {
                                        AddDataRow(dialog_id, questionlayout_id, sourceLabel, "SmartText", "SmartTextValues", "", "",
                                            (!string.IsNullOrEmpty(opt.TextPrefix) ? opt.TextPrefix : "Values"), "Text", "SmartTextValues");
                                    }
                                }
                            }
                        }
                    });
                    #endregion
                }
            }

            #region Account Fields
            AddDataRow(0, 0, "Account", "", "CompanyName", "", "", "Company Name", "Text", "");
            AddDataRow(0, 0, "Account", "", "OrgNo", "", "", "Organization No", "Text", "");
            AddDataRow(0, 0, "Account", "", "YearEstablished", "", "", "Year Established", "Text", "");
            AddDataRow(0, 0, "Account", "", "ParentCompany", "", "", "Parent Company", "Text", "");
            AddDataRow(0, 0, "Account", "", "Website", "", "", "Website", "Text", "");
            AddDataRow(0, 0, "Account", "", "Telephone", "", "", "Telephone", "Text", "");
            AddDataRow(0, 0, "Account", "", "Telefax", "", "", "Telefax", "Text", "");
            AddDataRow(0, 0, "Account", "", "Box", "", "", "Box", "Text", "");
            AddDataRow(0, 0, "Account", "", "Street", "", "", "Street", "Text", "");
            AddDataRow(0, 0, "Account", "", "ZipCode", "", "", "Zip Code", "Text", "");
            AddDataRow(0, 0, "Account", "", "City", "", "", "City", "Text", "");
            AddDataRow(0, 0, "Account", "", "Country", "", "", "Country", "Text", "");
            AddDataRow(0, 0, "Account", "", "County", "", "", "County", "Text", "");
            AddDataRow(0, 0, "Account", "", "Municipality", "", "", "Municipality", "Text", "");
            //AddDataRow(0, 0, "Account", "", "CountyCode", "", "", "County Code", "Text", "");
            AddDataRow(0, 0, "Account", "", "Region", "", "", "Region", "Text", "");
            AddDataRow(0, 0, "Account", "", "ActivityCode", "", "", "Activity Code", "Text", "");
            //AddDataRow(0, 0, "Account", "", "ActivityCodeDescription", "", "", "Activity Code Description", "Text", "");
            AddDataRow(0, 0, "Account", "", "ActivityCode2", "", "", "Activity Code2", "Text", "");
            //AddDataRow(0, 0, "Account", "", "ActivityCodeDescription2", "", "", "Activity Code Description 2", "Text", "");
            AddDataRow(0, 0, "Account", "", "Currency", "", "", "Currency", "Text", "");
            AddDataRow(0, 0, "Account", "", "FiscalYear1", "", "", "Fiscal Year 1", "Text", "");
            //AddDataRow(0, 0, "Account", "", "ConsolidatedFigure", "", "", "Consolidated Figure", "Text", "");
            //AddDataRow(0, 0, "Account", "", "Revenue", "", "", "Revenue", "Text", "");
            AddDataRow(0, 0, "Account", "", "Turnover1", "", "", "Turnover 1", "Text", "");
            AddDataRow(0, 0, "Account", "", "Export1", "", "", "Export 1", "Text", "");
            AddDataRow(0, 0, "Account", "", "Result1", "", "", "Result 1", "Text", "");
            AddDataRow(0, 0, "Account", "", "SalesAbroad1", "", "", "Sales Abroad", "Text", "");
            AddDataRow(0, 0, "Account", "", "EmployeesAboad1", "", "", "Employees Aboad 1", "Text", "");
            AddDataRow(0, 0, "Account", "", "EmployeesTotal1", "", "", "Employees Total 1", "Text", "");
            //AddDataRow(0, 0, "Account", "", "NetInterestIncome", "", "", "Net Interest Income", "Text", "");
            //AddDataRow(0, 0, "Account", "", "GrossPremium", "", "", "Gross Premium", "Text", "");
            AddDataRow(0, 0, "Account", "", "FiscalYear2", "", "", "Fiscal Year 2", "Text", "");
            AddDataRow(0, 0, "Account", "", "Turnover2", "", "", "Turnover 2", "Text", "");
            AddDataRow(0, 0, "Account", "", "Export2", "", "", "Export 2", "Text", "");
            AddDataRow(0, 0, "Account", "", "Result2", "", "", "Result 2", "Text", "");
            AddDataRow(0, 0, "Account", "", "SalesAbroad2", "", "", "Sales Abroad 2", "Text", "");
            AddDataRow(0, 0, "Account", "", "EmployeesAboad2", "", "", "Employees Aboad 2", "Text", "");
            AddDataRow(0, 0, "Account", "", "EmployeesTotal2", "", "", "Employees Total 2", "Text", "");
            AddDataRow(0, 0, "Account", "", "FiscalYear3", "", "", "Fiscal Year 3", "Text", "");
            AddDataRow(0, 0, "Account", "", "Turnover3", "", "", "Turnover 3", "Text", "");
            AddDataRow(0, 0, "Account", "", "Export3", "", "", "Export 3", "Text", "");
            AddDataRow(0, 0, "Account", "", "Result3", "", "", "Result 3", "Text", "");
            AddDataRow(0, 0, "Account", "", "SalesAbroad3", "", "", "Sales Abroad 3", "Text", "");
            AddDataRow(0, 0, "Account", "", "EmployeesAboad3", "", "", "Employees Aboad 3", "Text", "");
            AddDataRow(0, 0, "Account", "", "EmployeesTotal3", "", "", "Employees Total 3", "Text", "");
            AddDataRow(0, 0, "Account", "", "Category", "", "", "Category", "Text", "");
            AddDataRow(0, 0, "Account", "", "BVSource", "", "", "BV Source", "Text", "");
            //AddDataRow(0, 0, "Account", "", "CompanyStatus", "", "", "Company Status", "Text", "");
            AddDataRow(0, 0, "Account", "", "Priority", "", "", "Priority", "Text", "");
            //AddDataRow(0, 0, "Account", "", "LastContact", "", "", "Last Contact", "Text", ""); //Company last changed
            AddDataRow(0, 0, "Account", "", "AssignedTo", "", "", "Assigned To", "Text", "");
            //AddDataRow(0, 0, "Account", "", "LeadStatus", "", "", "Lead Status", "Text", "");
            //AddDataRow(0, 0, "Account", "", "LastUpdated", "", "", "Last Updated", "Text", ""); //Company Status last Changed
            #endregion

            #region Contact Fields
            AddDataRow(0, 0, "Contact", "", "Firstname", "", "", "Firstname", "Text", "");
            AddDataRow(0, 0, "Contact", "", "Middlename", "", "", "Middlename", "Text", "");
            AddDataRow(0, 0, "Contact", "", "Lastname", "", "", "Lastname", "Text", "");
            AddDataRow(0, 0, "Contact", "", "DirectPhone", "", "", "Direct Phone", "Text", "");
            AddDataRow(0, 0, "Contact", "", "Mobile", "", "", "Mobile", "Text", "");
            AddDataRow(0, 0, "Contact", "", "Email", "", "", "Email", "Text", "");
            AddDataRow(0, 0, "Contact", "", "TitleNotConfirmed", "", "", "Title Not Confirmed", "Text", "");
            AddDataRow(0, 0, "Contact", "", "Title", "", "", "Title", "Text", "");
            AddDataRow(0, 0, "Contact", "", "Roles", "", "", "Roles", "Text", "");
            AddDataRow(0, 0, "Contact", "", "Address 1", "", "", "Address 1", "Text", "");
            AddDataRow(0, 0, "Contact", "", "Address 2", "", "", "Address 2", "Text", "");
            AddDataRow(0, 0, "Contact", "", "City", "", "", "City", "Text", "");
            AddDataRow(0, 0, "Contact", "", "ZipCode", "", "", "Zip Code", "Text", "");
            AddDataRow(0, 0, "Contact", "", "Country", "", "", "Country", "Text", "");
            AddDataRow(0, 0, "Contact", "", "Priority", "", "", "Priority", "Text", "");
            //AddDataRow(0, 0, "Contact", "", "LastContact", "", "", "Last Contact", "Text", ""); //Contact Last Changed
            //AddDataRow(0, 0, "Contact", "", "LastUpdated", "", "", "Last Updated", "Text", ""); //Contact Status Last Changed
            #endregion

            #region General Fields
            AddDataRow(0, 0, "General", "", "ExportViewName", "", "", "Export View Name", "Text", "");
            AddDataRow(0, 0, "General", "", "CustomerName", "", "", "Customer Name", "Text", "");
            AddDataRow(0, 0, "General", "", "CampaignName", "", "", "Campaign Name", "Text", "");
            AddDataRow(0, 0, "General", "", "SubcampaignName", "", "", "SubCampaign Name", "Text", "");
            AddDataRow(0, 0, "General", "", "DialogCreatedBy", "", "", "Dialog Creator", "Text", "");
            AddDataRow(0, 0, "General", "", "DialogCreatedDate", "", "", "Dialog Created Date", "Text", "");

            /**
             * [@jeff 06.08.2012]: https://brightvision.jira.com/browse/PLATFORM-1465
             * changed UI label from 'Dialog Status' to 'Contact Status' to avoid confusion on user end.
             * to be exact, this is the contact's dialog status. so we rename it to contact status instead of dialog status.
             */
            AddDataRow(0, 0, "General", "", "DialogStatus", "", "", "Contact Status", "Text", "");

            AddDataRow(0, 0, "General", "", "ListSourceName", "", "", "List Source Name", "Text", "");                        
            AddDataRow(0, 0, "General", "", "CompanyLeadStatus", "", "", "Company Lead Status", "Text", "");
            AddDataRow(0, 0, "General", "", "CompanyStatus", "", "", "Company Status", "Text", "");
            AddDataRow(0, 0, "General", "", "CompanyLastChanged", "", "", "Company Last Changed", "Text", ""); //Company last changed
            AddDataRow(0, 0, "General", "", "CompanyStatusLastChanged", "", "", "Company Status Last Changed", "Text", ""); //Company Status last Changed
            AddDataRow(0, 0, "General", "", "ContactLastChanged", "", "", "Contact Last Changed", "Text", ""); //Contact Last Changed
            AddDataRow(0, 0, "General", "", "AccountSubCampaignCallAttempts", "", "", "Account Subcampaign Call Attempts", "Text", "");
            AddDataRow(0, 0, "General", "", "ContactSubCampaignCallAttempts", "", "", "Contact Subcampaign Call Attempts", "Text", "");
            #endregion
            
            #region Bind Datasource
            //DataView positionView = dtAvailableColumns.DefaultView;
            //select distinct rows
            //dtAvailableColumns = positionView.ToTable("available_columns", true, "source","component_type", "label_name", "field_type", "default");
            //DataColumn dtIndex = new DataColumn("id", typeof(int));            
            //dtAvailableColumns.Columns.Add(dtIndex);
            //dtIndex.SetOrdinal(0);//position column at 0 index
            
            //int count = 1;
            //foreach (DataRow dr in dtAvailableColumns.Rows) {
            //    dr["id"] = count;
            //    count++;
            //}
            dtAvailableColumns.AcceptChanges();
            gcAvailableColumns.DataSource = dtAvailableColumns;
            gvAvailableColumns.Columns["source"].Group();
            gvAvailableColumns.Columns["component_type"].Group();
            gvAvailableColumns.ExpandAllGroups();                    
            #endregion     
        }
        private void BindGridColumnsInView() {
            PopulateDataTableColumnsInView();
            gcColumnView.DataSource = dtColumnsInView;
            gvColumnView.Columns[OrderFieldName].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            gvColumnView.OptionsCustomization.AllowSort = false;
        }
        private void ReorderColumnViewPositionIndex() {
            int index = 0;
            //foreach (DataRow dr in dtColumnsInView.Rows) {
            //    dr["position"] = index + 1;                
            //    index++;
            //}
            //dtColumnsInView.AcceptChanges();

            int rowcount = gvColumnView.RowCount;
            DataRow dr;
            bool valid = true;
            for (int x = 0; x < rowcount; ++x)
            {
                dr = gvColumnView.GetDataRow(x);
                dr["position"] = index + 1;
                index++;
            }
            
            gcColumnView.RefreshDataSource();            
        }
        private void SetValidationRules() {
            //CustomIsBlankValidationRule isBlankValidationRule = new CustomIsBlankValidationRule();
            //isBlankValidationRule.ErrorText = "View name cannot be empty";
            //isBlankValidationRule.ErrorType = ErrorType.Critical;
            //dxValidationProvider1.SetValidationRule(txtViewName, isBlankValidationRule);
        }
        private bool ValidateColumnsInView() {
            int rowcount = gvColumnView.RowCount;
            string displayName = string.Empty;
            //int rowId = -1;
            DataRow dr;
            DataRow[] matches;
            bool valid = true;
            for (int x = 0; x < rowcount; ++x) {
                dr = gvColumnView.GetDataRow(x);
                //int rowId = (int)dr["id"];
                if (dr["display_name"] != DBNull.Value)
                    displayName = (string)dr["display_name"];

                if (displayName.Contains("\r") || displayName.Contains("\n")) {
                    dr.SetColumnError("display_name", "New line not allowed.");
                    valid = false;
                }
                else if (string.IsNullOrEmpty(displayName)) {
                    dr.SetColumnError("display_name", "Display name is required.");
                    valid = false;
                } 
                else {
                    matches = dtColumnsInView.Select("display_name = '" + displayName.ToString().Replace("'", "''")+ "'");//  AND id <> " + rowId);
                    if (matches != null && matches.Length > 1) {
                        dr.SetColumnError("display_name", "Duplicate display name is not allowed. Please choose another name that is unique.");
                        valid = false;
                    } 
                    else {
                        dr.ClearErrors();
                    }
                }
            }
            return valid;
        }
        private bool AllowedSaveTempalte() {
            return true;
            int rowcount = gvColumnView.RowCount;
            if (rowcount <= 0) return false;
            DataRow[] matches = dtColumnsInView.Select("source = 'Dialog'");
            if (matches != null && matches.Length <= 0) return true;
            return false;
        }
        private bool SaveConfiguration(bool pIsNew)
        {
            System.IO.StringWriter writer = new System.IO.StringWriter();
            DataSet dsResult = null;
            if (dtColumnsInView.DataSet == null) {
                dsResult = new DataSet("view");
                dsResult.Tables.Add(dtColumnsInView);
            }
            else {
                dsResult = dtColumnsInView.DataSet;
            }
            dsResult.WriteXml(writer, XmlWriteMode.IgnoreSchema);
            var xmlConfig = writer.ToString();
            if (string.IsNullOrEmpty(xmlConfig) || xmlConfig.Equals("<view />")) {
                msgPopupRegular.Show(this.ParentForm, "Bright Manager", "No columns defined for the report.");
                return false;
            }

            if (m_efoViewConfig == null) {
                m_efoViewConfig = new view_configuration() {
                    date_created = DateTime.Now,
                    created_by = UserSession.CurrentUser.UserId
                };
            }
            else {
                m_efoViewConfig.modified_by = UserSession.CurrentUser.UserId;
                m_efoViewConfig.date_modified = DateTime.Now;
            }
            m_efoViewConfig.xml_config = xmlConfig;
            m_efoViewConfig.subcampaign_id = m_iSubcampaignID;
            if (pIsNew)
                m_efoViewConfig.name = m_ViewName;

            try {
                if (IsNewConfig)
                    m_efDbContext.view_configuration.AddObject(m_efoViewConfig);

                m_efDbContext.SaveChanges();
                int configID = m_efoViewConfig.id;
                BindViewList();
                lookUpEditViewList.EditValue = configID;
                btnDeleteView.Enabled = true;
                btnShowXmlViewConfig.Enabled = true;
                //MessageBox.Show("View configuration has been saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                IsNewConfig = false;
                IsSaved = true;
                m_ConfigGrid.Complete = true;
                tbpgeGridConfig.Image = ManagerApplication.Properties.Resources.save_ok;
                this.SetPreviewReport();

                return true;

                /**
                 * changing tab color code.
                 */
                //tcViewConfig.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
                //tcViewConfig.LookAndFeel.UseDefaultLookAndFeel = false;
                //tbpgeGridReport.Appearance.Header.BackColor = Color.PaleGreen;
                //tbpgeGridReport.Appearance.Header.Options.UseBackColor = true;
            }
            catch {
                return false;
            }
        }

        private void AddDataRowDialogFields(int dialog_id, int questionlayout_id, bool isAccountLevel, string componentType, string label,
            string questionText, string questionHelp, string bvownershipvalue, string customerownershipvalue, string plotdonestatusvalue,
            string priorityvalue)
        {
            string sourceLabel = "Dialog Contact Level";
            if (isAccountLevel)
                sourceLabel = "Dialog Account Level";
            AddDataRow(dialog_id, questionlayout_id, sourceLabel, componentType, "BVOwnership", "", bvownershipvalue, "BVOwnership", "Label", "BVOwnership");
            AddDataRow(dialog_id, questionlayout_id, sourceLabel, componentType, "CustomerOwnership", "", customerownershipvalue, "CustomerOwnership", "Label", "CustomerOwnership");
            AddDataRow(dialog_id, questionlayout_id, sourceLabel, componentType, "PlotDoneStatus", "", plotdonestatusvalue, "PlotDoneStatus", "Label", "PlotDoneStatus");
            AddDataRow(dialog_id, questionlayout_id, sourceLabel, componentType, "Priority", "", priorityvalue, "Priority", "Label", "Priority");
            AddDataRow(dialog_id, questionlayout_id, sourceLabel, componentType, "QuestionTextLabel", "", label, label, "Label", "QuestionTextLabel");
            AddDataRow(dialog_id, questionlayout_id, sourceLabel, componentType, "QuestionText", "", questionText, questionText, "Label", "QuestionText");
            AddDataRow(dialog_id, questionlayout_id, sourceLabel, componentType, "QuestionHelp", "", questionHelp, questionHelp, "Label", "QuestionHelp");
        }

        private void AddDataRow(int dialog_id, int questionlayout_id, string source, string component_type, string fieldName, string fieldIndex,
            string externalValue, string labelName, string fieldType, string Default)
        {
            DataRow row = dtAvailableColumns.NewRow();
            row["source"] = source;
            row["dialog_id"] = dialog_id;
            row["questionlayout_id"] = questionlayout_id;
            //row["questiontextlanguage_id"] = questiontextlanguage_id;
            row["component_type"] = string.IsNullOrEmpty(component_type) ? "None" : component_type;
            row["field_name"] = fieldName;
            row["field_index"] = fieldIndex;
            row["external_value"] = externalValue;
            row["label_name"] = labelName;
            row["field_type"] = fieldType;
            row["default"] = Default;
            dtAvailableColumns.Rows.Add(row);
            //if (source == "Dialog") {
            m_oDialogFields.Add(new DialogField()
            {
                id = row["id"].ToString(),
                dialog_id = dialog_id.ToString(),
                questionlayout_id = questionlayout_id.ToString(),
                source = source,
                component_type = string.IsNullOrEmpty(component_type) ? "None" : component_type,
                field_name = fieldName,
                field_index = fieldIndex,
                external_value = externalValue,
                label_name = labelName,
                display_name = Default
            });
            //}
        }

        private void AddDataRow(int id, int position, int dialog_id, int questionlayout_id, string source, string component_type,
            string externalValue, string field_name, string fieldIndex, string labelName, string displayName, string columnWidth, string sort1, string sort2)
        {
            DataRow row = dtColumnsInView.NewRow();
            row["id"] = id;
            row["position"] = position;
            row["dialog_id"] = dialog_id == 0 ? "" : dialog_id.ToString();
            row["questionlayout_id"] = questionlayout_id == 0 ? "" : questionlayout_id.ToString();
            //row["questiontextlanguage_id"] = questiontextlanguage_id == 0 ? "" : questiontextlanguage_id.ToString();
            row["source"] = source;
            row["component_type"] = component_type;
            row["field_name"] = field_name;
            row["field_index"] = fieldIndex;
            row["external_value"] = externalValue;
            row["label_name"] = labelName;
            row["display_name"] = displayName;
            row["column_width"] = columnWidth;
            row["sort1"] = sort1;
            row["sort2"] = sort2;
            dtColumnsInView.Rows.Add(row);
        }
        #endregion

        #region Public Methods
        public void ReloadSubCampaignList()
        {
            int? _EditValue = null;
            if (lookUpEditSubcampaign.EditValue != null)
                _EditValue = (int)lookUpEditSubcampaign.EditValue;

            this.lookUpEditSubcampaign.EditValueChanged -= new System.EventHandler(this.lookUpEditSubcampaign_EditValueChanged);
            List<CTCustomerCampaignSubcampaign> listCCS = m_efDbContext.FIGetCustomerCampaignSubcampaign(UserSession.CurrentUser.UserId, 2).ToList();
            if (listCCS != null && listCCS.Count > 0) {
                lookUpEditSubcampaign.Properties.Columns.Clear();
                lookUpEditSubcampaign.Properties.DataSource = listCCS;
                lookUpEditSubcampaign.Properties.DisplayMember = "title";
                lookUpEditSubcampaign.Properties.ValueMember = "subcamapaign_id";
                lookUpEditSubcampaign.Properties.Columns.Add(new LookUpColumnInfo("title"));
                lookUpEditSubcampaign.Properties.ShowHeader = false;
                lookUpEditSubcampaign.Properties.ReadOnly = false;
            }

            if (_EditValue != null && _EditValue > 0)
                lookUpEditSubcampaign.EditValue = _EditValue;

            this.lookUpEditSubcampaign.EditValueChanged += new System.EventHandler(this.lookUpEditSubcampaign_EditValueChanged);
        }
        public void ResetColumnsInView()
        {
            gcColumnView.DataSource = null;
            BindGridColumnsInView();
        }
        public void LoadXML(string xml)
        {
            System.IO.StringReader reader = new System.IO.StringReader(xml);
            dtColumnsInView.ReadXml(reader);

        }
        public void BindSubcampaignLookup()
        {
            lookUpEditViewList.Properties.ReadOnly = true;
            lookUpEditSubcampaign.Properties.ReadOnly = true;
            btnLoadView.Enabled = false;
            btnCreateBlankView.Enabled = false;
            btnRenameView.Enabled = false;
            btnDeleteView.Enabled = false;
            //btnPreviewView.Enabled = false;
            btnCancel.Enabled = false;
            m_HasSelection = false;

            List<CTCustomerCampaignSubcampaign> listCCS = m_efDbContext.FIGetCustomerCampaignSubcampaign(UserSession.CurrentUser.UserId, 2).ToList();
            if (listCCS != null && listCCS.Count > 0) {
                lookUpEditSubcampaign.Properties.Columns.Clear();
                lookUpEditSubcampaign.Properties.DataSource = listCCS;
                lookUpEditSubcampaign.Properties.DisplayMember = "title";
                lookUpEditSubcampaign.Properties.ValueMember = "subcamapaign_id";
                lookUpEditSubcampaign.Properties.Columns.Add(new LookUpColumnInfo("title"));
                lookUpEditSubcampaign.Properties.ShowHeader = false;
                lookUpEditSubcampaign.Properties.ReadOnly = false;
                m_HasSelection = true;
            }
            gcAvailableColumns.DataSource = null;
            gcColumnView.DataSource = null;
            //lcgHeaderControls.Enabled = false;
            lcgLeft.Enabled = false;
            lcgRight.Enabled = false;
            tcViewConfig.Enabled = false;
            m_ViewName = string.Empty;
        }
        #endregion

        #region Controller
        private void btnLoadView_Click(object sender, EventArgs e) 
        {
            if (!IsSaved) {
                if (MessageBox.Show("Do you want to save changes to the current view?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    btnSaveConfig_Click(null, null);
                } else {
                    IsSaved = true;
                }
            }
            
            WaitDialog.Show(ParentForm, "Loading data view ...");
            
            /**
             * set default icons and flags first.
             */
            m_ConfigGrid = new ReportConfig() {
                Complete = false,
                OnEditMode = false
            };
            m_ConfigLayout = new ReportConfig() {
                Complete = false,
                OnEditMode = false
            };
            m_ConfigParameter = new ReportConfig() {
                Complete = false,
                OnEditMode = false
            };

            m_LoadedParameters = false;
            m_LoadedLayout = false;
            m_LoadedConfig = true;
            m_IsNew = false;
            IsNewConfig = false;

            tbpgeLayoutConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;
            btnEditReportTemplate.Image = ManagerApplication.Properties.Resources.view_config_incomplete;
            tbpgeParameterConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;

            gcAvailableColumns.DataSource = null;
            gcColumnView.DataSource = null;
            this.BindGridAvailableColumns();
            this.BindGridColumnsInView();

            btnShowXmlViewConfig.Enabled = true;
            lookUpEditSubcampaign.Properties.ReadOnly = true;
            lcgLeft.Enabled = true;
            lcgRight.Enabled = true;
            tcViewConfig.Enabled = true;
            btnDeleteView.Enabled = true;
            btnCancel.Enabled = true;
            btnLoadLayoutTemplateToReport.Enabled = true;
            //btnPreviewView.Enabled = true;

            var objSender = lookUpEditViewList;
            if (objSender != null) {
                var val = objSender.EditValue;
                if (val != null) {
                    int configId = (int)val;
                    if (configId > 0) {
                        m_efoViewConfig = m_efDbContext.view_configuration.FirstOrDefault(x => x.id == configId);
                        if (m_efoViewConfig != null) {
                            this.LoadXML(m_efoViewConfig.xml_config);
                            m_ViewName = m_efoViewConfig.name;

                            /**
                             * grid config.
                             */
                            m_ConfigGrid.OnEditMode = true;
                            if (!string.IsNullOrEmpty(m_efoViewConfig.xml_config)) {
                                m_ConfigGrid.Complete = true;
                                tbpgeGridConfig.Image = ManagerApplication.Properties.Resources.save_ok;
                            }
                            else
                                tbpgeGridConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;

                            /**
                             * layout config.
                             */
                            if (!string.IsNullOrEmpty(m_efoViewConfig.report_layout_config)) {
                                m_ConfigLayout.Complete = true;
                                tbpgeLayoutConfig.Image = ManagerApplication.Properties.Resources.save_ok;
                                btnEditReportTemplate.Image = ManagerApplication.Properties.Resources.view_config_completed;
                            }
                            else {
                                tbpgeLayoutConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;
                                btnEditReportTemplate.Image = ManagerApplication.Properties.Resources.view_config_incomplete;
                            }

                            /**
                             * parameter config.
                             */
                            if (!string.IsNullOrEmpty(m_efoViewConfig.report_data_config)) {
                                m_ConfigParameter.Complete = true;
                                tbpgeParameterConfig.Image = ManagerApplication.Properties.Resources.save_ok;
                            }
                            else
                                tbpgeParameterConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;
                        }
                    }

                }
            }
            this.SetPreviewReport();
            WaitDialog.Close();
        }
        private void lookUpEditViewList_EditValueChanged(object sender, EventArgs e) 
        {
            var objSender = sender as LookUpEdit;
            if (objSender != null) {
                if (objSender.EditValue != null && Convert.ToInt32(objSender.EditValue) > 0) {
                    btnLoadView.Enabled = true;
                    btnDeleteView.Enabled = true;
                    btnRenameView.Enabled = true;
                    //if (!btnLoadView.Enabled)
                    //    btnDeleteView.Enabled = false;
                } 
                else {
                    btnLoadView.Enabled = false;
                    btnDeleteView.Enabled = false;
                    btnRenameView.Enabled = false;
                }
            }
        }
        private void btnRenameView_Click(object sender, EventArgs e)
        {
            if (!IsSaved) {
                if (MessageBox.Show("Do you want to save changes to the current view?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    btnSaveConfig_Click(null, null);
                else
                    IsSaved = true;
            }

            if (gvAvailableColumns.RowCount < 1) {
                MessageBox.Show("Please load the config file first.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CreateBlankView _control = new CreateBlankView(false, m_efoViewConfig.name);
            _control.AfterSave += new CreateBlankView.AfterSaveEventHandler(_control_AfterSave);

            PopupDialog _dlg = new PopupDialog() {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Rename View"
            };
            _dlg.Controls.Add(_control);
            _dlg.ClientSize = new Size(_control.Width + 2, _control.Height + 2);
            _dlg.ShowDialog(this.ParentForm);
        }
        private void btnCreateBlankView_Click(object sender, EventArgs e) 
        {
            if (!IsSaved) {
                if (MessageBox.Show("Do you want to save changes to the current view?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    btnSaveConfig_Click(null, null);
                else
                    IsSaved = true;
            }

            WaitDialog.Show(ParentForm, "Loading Data Views...");
            m_ConfigGrid = new ReportConfig() {
                Complete = false,
                OnEditMode = true // create new blank view is always on edit mode.
            };
            m_ConfigLayout = new ReportConfig() {
                Complete = false,
                OnEditMode = false
            };
            m_ConfigParameter = new ReportConfig() {
                Complete = false,
                OnEditMode = false
            };

            m_LoadedParameters = false;
            m_LoadedLayout = false;
            m_LoadedConfig = false; // means this is not a loaded existing view config

            tbpgeGridConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;
            tbpgeLayoutConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;
            btnEditReportTemplate.Image = ManagerApplication.Properties.Resources.view_config_incomplete;
            tbpgeParameterConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;

            m_GoDefault = true;
            tcViewConfig.SelectedTabPage = tbpgeGridConfig;

            lookUpEditViewList.EditValue = null;
            lookUpEditViewList.Properties.ReadOnly = true;
            lookUpEditSubcampaign.Properties.ReadOnly = true;

            //lcgHeaderControls.Enabled = true;
            lcgLeft.Enabled = true;
            lcgRight.Enabled = true;
            tcViewConfig.Enabled = true;
            //txtViewName.Text = "";

            BindGridAvailableColumns();
            BindGridColumnsInView();
            //var objSen = sender as SimpleButton;
            //if (objSen != null) objSen.Enabled = false;
            btnCreateBlankView.Enabled = false;
            btnCancel.Enabled = true;
            btnLoadView.Enabled = false;
            btnDeleteView.Enabled = false;
            btnLoadLayoutTemplateToReport.Enabled = false;
            //btnDuplicateView.Enabled = false;
            btnShowXmlViewConfig.Enabled = true;
            IsNewConfig = true;
            m_efoViewConfig = null;
            WaitDialog.Close();

            

            //CreateBlankView _control = new CreateBlankView(true);
            //_control.AfterSave += new CreateBlankView.AfterSaveEventHandler(_control_AfterSave);

            //PopupDialog _dlg = new PopupDialog() {
            //    FormBorderStyle = FormBorderStyle.FixedSingle,
            //    MinimizeBox = false,
            //    MaximizeBox = false,
            //    StartPosition = FormStartPosition.CenterScreen,
            //    Text = "New View"
            //};
            //_dlg.Controls.Add(_control);
            //_dlg.ClientSize = new Size(_control.Width + 2, _control.Height + 2);
            //_dlg.ShowDialog(this.ParentForm);

            //if (!IsSaved) {
            //    if (MessageBox.Show("Do you want to save changes to the current view?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
            //        btnSaveConfig_Click(null, null);
            //    } 
            //    else {
            //        IsSaved = true;
            //    }
            //}

            //lookUpEditViewList.EditValue = null;
            //lookUpEditViewList.Properties.ReadOnly = true;
            //lookUpEditSubcampaign.Properties.ReadOnly = true;
            
            //WaitDialog.Show(ParentForm, "Loading Data Views...");
            ////lcgHeaderControls.Enabled = true;
            //lcgLeft.Enabled = true;
            //lcgRight.Enabled = true;
            ////txtViewName.Text = "";
            //BindGridAvailableColumns();
            //BindGridColumnsInView();
            //var objSen = sender as SimpleButton;
            //if (objSen != null) objSen.Enabled = false;
            //btnLoadView.Enabled = false;
            //btnDeleteView.Enabled = false;
            ////btnDuplicateView.Enabled = false;
            //btnShowXmlViewConfig.Enabled = true;
            //IsNewConfig = true;
            //viewConfig = null;
            //WaitDialog.Close();
        }
        private void btnDeleteView_Click(object sender, EventArgs e) 
        {
            if (MessageBox.Show("Are you sure you want to delete current selected view?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.No) return;

            tcViewConfig.SelectedTabPage = tbpgeGridConfig;
            tbpgeGridConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;
            tbpgeLayoutConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;
            btnEditReportTemplate.Image = ManagerApplication.Properties.Resources.view_config_incomplete;
            tbpgeParameterConfig.Image = ManagerApplication.Properties.Resources.incomplete_on_edit;

            var objSender = lookUpEditViewList;
            if (objSender != null) {
                var val = objSender.EditValue;
                if (val != null) {
                    int configId = (int)val;
                    if (configId > 0) {
                        m_efoViewConfig = m_efDbContext.view_configuration.FirstOrDefault(x => x.id == configId);
                        if (m_efoViewConfig != null) {
                            m_efoViewConfig.MGC = true;
                            m_efDbContext.SaveChanges();
                        }
                    }
                }
                BindViewList();
                gcAvailableColumns.DataSource = null;
                gcColumnView.DataSource = null;
                lcgLeft.Enabled = false;
                lcgRight.Enabled = false;
                tcViewConfig.Enabled = false;
                btnPreviewView.Enabled = false;
                //txtViewName.Text = "";
                m_ViewName = string.Empty;
                MessageBox.Show("View configuration has been deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);                
            }
        }
        private void btnCancel_Click(object sender, EventArgs e) 
        {
            if (!IsSaved) {
                if (MessageBox.Show("Do you want to save changes to the current view?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    btnSaveConfig_Click(null, null);
                else
                    IsSaved = true;
            }
            gcAvailableColumns.DataSource = null;
            gcColumnView.DataSource = null;
            //btnDuplicateView.Enabled = false;
            btnLoadView.Enabled = false;
            btnDeleteView.Enabled = false;
            btnCreateBlankView.Enabled = false;
            btnRenameView.Enabled = false;
            btnPreviewView.Enabled = false;
            btnCancel.Enabled = false;
            btnCancel.Enabled = false;
            lcgLeft.Enabled = false;
            lcgRight.Enabled = false;
            tcViewConfig.Enabled = false;
            tcViewConfig.Enabled = false;
            btnLoadLayoutTemplateToReport.Enabled = false;
            //txtViewName.Text = "";
            m_ViewName = string.Empty;
            lookUpEditViewList.EditValue = null;

            tbpgeGridConfig.Image = null;
            tbpgeLayoutConfig.Image = null;
            tbpgeParameterConfig.Image = null;
            tcViewConfig.SelectedTabPage = tbpgeGridConfig;
            tcViewConfig.SelectedTabPageIndex = 0;

            //lookUpEditSubcampaign.Properties.ReadOnly = false;
            //btnLoadView.Enabled = lookUpEditViewList.EditValue != null ? true : false;
            if (m_HasSelection) {
                btnCreateBlankView.Enabled = true;
                lookUpEditSubcampaign.Properties.ReadOnly = false;
                lookUpEditViewList.Properties.ReadOnly = false;
            }
        }
        private void lookUpEditSubcampaign_EditValueChanged(object sender, EventArgs e) 
        {
            lcgLeft.Enabled = false;
            lcgRight.Enabled = false;
            tcViewConfig.Enabled = false;
            btnShowXmlViewConfig.Enabled = false;
            this.BindViewList();
        }
        private void gvAvailableColumns_DoubleClick(object sender, EventArgs e) {
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                view.FocusedRowHandle = hitInfo.RowHandle;
                btnAdd_Click(null, null);
            }
        }
        private void btnAdd_Click(object sender, EventArgs e) 
        {
            IsSaved = false;
            gvAvailableColumns.GridControl.Focus();
            int[] selectedRows = gvAvailableColumns.GetSelectedRows();
            DataRow[] filter;
            string source = string.Empty, org_source = string.Empty,
                labelName = string.Empty, component_type = string.Empty;
            foreach (int i in selectedRows) {
                //don't include negative numbers in the loop because this is the header of the field.
                //although it is still filtered by id for whose added but it will be a nuisance when ordering the selected fields.
                if (i < 0) continue;
                int curRow = gvAvailableColumns.GetDataSourceRowIndex(i);
                DataRow dr = dtAvailableColumns.Rows[curRow];

                /** 
                 * [@jeff.albano 05-07-2012]: https://brightvision.jira.com/browse/PLATFORM-1384
                 * added filter conditions to correctly evaluate existing columns.
                 */
                string _filterString = "id = " + dr["id"].ToString() + " and " +
                    "source = '" + dr["source"].ToString().Replace("'", "''") + "' and " +
                    "field_name = '" + dr["field_name"].ToString().Replace("'", "''") + "' and " +
                    "label_name = '" + dr["label_name"].ToString().Replace("'", "''") + "'";

                //filter = dtColumnsInView.Select("id=" + dr["id"].ToString());
                filter = dtColumnsInView.Select(_filterString);
                if (filter.Length > 0) 
                    continue;
                
                int maxPos = gvColumnView.RowCount;
                org_source = dr["source"].ToString();
                source = org_source + ">";
                component_type = dr["component_type"].ToString();
                component_type = component_type != "None" ? component_type : "";
                labelName = Regex.Replace(dr["label_name"].ToString(), ",|;|\\.|:", delegate(Match match) {
                    string v = match.ToString();
                    return v.Replace(v, "\"" + v + "\"");
                });
                string dispName = org_source + " " +  component_type + " " + Regex.Replace(dr["label_name"].ToString(),",|;|\\.|:|\"|'","");

                AddDataRow(
                    Convert.ToInt32(dr["id"]),
                    (maxPos + 1),
                    Convert.ToInt32(dr["dialog_id"]),
                    Convert.ToInt32(dr["questionlayout_id"]),
                   // Convert.ToInt32(dr["questiontextlanguage_id"]),
                    org_source,
                    component_type,
                    dr["external_value"].ToString(),
                    dr["field_name"].ToString(),
                    dr["field_index"].ToString(),
                    source + (component_type != "" ? component_type + ">" : "") + labelName,
                    dispName, "", "", "");
            }

            dtColumnsInView.AcceptChanges();
            gcColumnView.RefreshDataSource();
        }
        private void btnAddEmptyColumn_Click(object sender, EventArgs e) {
            IsSaved = false;
            gvAvailableColumns.GridControl.Focus();
            int maxPos = gvColumnView.RowCount;
            DataRow dr = dtColumnsInView.NewRow();
            int resCount = 0;
            DataRow[] results = dtColumnsInView.Select("label_name = 'EMPTY'");
            if (results != null && results.Length > 0)
                resCount = results.Length + 1;
            dr["id"] = 0;
            dr["position"] = maxPos + 1;
            dr["label_name"] = "EMPTY";
            dr["display_name"] = "EMPTY" + resCount.ToString();
            dr["dialog_id"] = DBNull.Value;
            dr["questionlayout_id"] = DBNull.Value;
            dr["source"] = DBNull.Value;
            dr["component_type"] = DBNull.Value;
            dr["field_name"] = DBNull.Value;
            dr["external_value"] = DBNull.Value;
            dr["field_index"] = DBNull.Value;
            dr["merge_data"] = DBNull.Value;
            dtColumnsInView.Rows.Add(dr);  
        }
        private void btnMoveUp_Click(object sender, EventArgs e) {
            gcAvailableColumns.Focus();
            IsSaved = false;
            GridView view = gvColumnView;
            gcColumnView.Focus();
            int index = view.FocusedRowHandle;
            if (index <= 0) return;
             
            DataRow row1 = view.GetDataRow(index);
            DataRow row2 = view.GetDataRow(index - 1);
            object val1 = row1[OrderFieldName];
            object val2 = row2[OrderFieldName];

            //https://brightvision.jira.com/browse/PLATFORM-2467
            //There is a scenarion where it has the same ordinal position
            if (int.Parse(val1.ToString()) == int.Parse(val2.ToString()))
            {
                row1[OrderFieldName] = int.Parse(val1.ToString()) - 1;
            }
            else
            {
                row1[OrderFieldName] = val2;
                row2[OrderFieldName] = val1;
            }
           
            view.FocusedRowHandle = index - 1;
            dtColumnsInView.AcceptChanges();
        }
        private void btnMoveDown_Click(object sender, EventArgs e) {
            gcAvailableColumns.Focus();
            IsSaved = false;
            if (gvColumnView.RowCount < 1)
                return;

            GridView view = gvColumnView;
            gcColumnView.Focus();
            int index = view.FocusedRowHandle;
            if (index >= view.DataRowCount - 1) return;

            DataRow row1 = view.GetDataRow(index);
            DataRow row2 = view.GetDataRow(index + 1);
            object val1 = row1[OrderFieldName];
            object val2 = row2[OrderFieldName];

            //https://brightvision.jira.com/browse/PLATFORM-2467
            //There is a scenarion where it has the same ordinal position
            if ((int.Parse(val1.ToString()) + 2) == int.Parse(val2.ToString()))
            {
                row1[OrderFieldName] = int.Parse(val1.ToString()) + 1;
                row2[OrderFieldName] = int.Parse(val2.ToString()) - 2;
            }
            else
            {
                row1[OrderFieldName] = val2;
                row2[OrderFieldName] = val1;
            }

            view.FocusedRowHandle = index + 1;
            dtColumnsInView.AcceptChanges();
        }
        private void btnRemove_Click(object sender, EventArgs e) {
            IsSaved = false;
            GridView view = gvColumnView;
            if (view == null || view.SelectedRowsCount == 0) return;

            DataRow[] rows = new DataRow[view.SelectedRowsCount];
            for (int i = 0; i < view.SelectedRowsCount; i++)
                rows[i] = view.GetDataRow(view.GetSelectedRows()[i]);
            foreach (DataRow row in rows) {
                row.Delete();
            }
            dtColumnsInView.AcceptChanges();
            ReorderColumnViewPositionIndex();
        }
        private void btnMerge_Click(object sender, EventArgs e) 
        {
            IsSaved = false;
            gvColumnView.GridControl.Focus();
            int[] selectedRows = gvColumnView.GetSelectedRows();
            
            if (selectedRows.Length == 0) return;

            if (selectedRows.Length == 1) {
                MessageBox.Show("Please select more than one row inorder to merge.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string separator = ";";
            string sepEditval = cbSeparator.EditValue.ToString();
            if (sepEditval == ", - Comma")
                separator = ",";
            else if (sepEditval == ". - Dot")
                separator = ".";
            else if (sepEditval == ": - Colon")
                separator = ":";
                        
            StringBuilder sbLabelName = new StringBuilder();

            int counter = 0;
            int curRow = 0;
            DataRow drMerge = null;
            DataRow dr = null;
            //fetch all label name to merge for selected rows
            string labelName = string.Empty, dialog_id = string.Empty, questionlayout_id = string.Empty, questiontextlanguage_id = string.Empty,
                fieldName = string.Empty, source = string.Empty, externalvalue = string.Empty, component_type = string.Empty, fieldIndex = string.Empty;

            /**
             * [jeff 05.14.2012]: https://brightvision.jira.com/browse/PLATFORM-1363
             * must not allow merged columns to be merged again.
             */
            short _MergedColumns = 0;
            foreach (int x in selectedRows)
            {
                curRow = gvColumnView.GetDataSourceRowIndex(x);
                dr = dtColumnsInView.Rows[curRow];
                if (dr["merge_data"] != DBNull.Value || dr["merge_data"].ToString() != string.Empty)
                    _MergedColumns++;

                if (_MergedColumns > 1)
                    break;
            }

            if (_MergedColumns > 1)
            {
                MessageBox.Show("Two merged columns cannot be merged.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            /**
             * [@jeff 05.11.2012]: https://brightvision.jira.com/browse/PLATFORM-1363
             * added checking to verify if there are dialog account level and contact level data
             * that has been merged. let the user know that dialog account level and contact level
             * columns cannot be merged as per ticket description.
             */
            bool _bHasDialogAccountLevelColumn = false;
            bool _bHasDialogContactLevelColumn = false;
            foreach (int x in selectedRows) 
            {
                curRow = gvColumnView.GetDataSourceRowIndex(x);
                dr = dtColumnsInView.Rows[curRow];

                if (dr.ItemArray[5].ToString().Equals("Dialog Account Level"))
                    _bHasDialogAccountLevelColumn = true;
                else if (dr.ItemArray[5].ToString().Equals("Dialog Contact Level"))
                    _bHasDialogContactLevelColumn = true;

                if ((_bHasDialogAccountLevelColumn && _bHasDialogContactLevelColumn))
                    break;

                if (dr["merge_data"] != DBNull.Value || dr["merge_data"].ToString() != string.Empty) {
                    drMerge = dr;
                    break;
                }
            }

            /**
             * checking for dialog account level and contact level columns.
             * must not be allowed.
             */
            if (_bHasDialogAccountLevelColumn && _bHasDialogContactLevelColumn)
            {
                MessageBox.Show("Dialog account level and contact level columns cannot be merged.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            XElement xElem = null;
            if (drMerge == null) {
                xElem = new XElement("merge_view");
                xElem.Add(new XAttribute("separator", separator));
            } else
                xElem = XElement.Parse(drMerge["merge_data"].ToString());

            XElement xItem = null;
            foreach (int i in selectedRows) {
                curRow = gvColumnView.GetDataSourceRowIndex(i);
                dr = dtColumnsInView.Rows[curRow];
                if (!dr.Equals(drMerge)) {
                    xItem = new XElement("item");
                    xItem.Add(new XElement("dialog_id", dr["dialog_id"].ToString()));
                    xItem.Add(new XElement("questionlayout_id", dr["questionlayout_id"].ToString()));
                    xItem.Add(new XElement("source", dr["source"].ToString()));
                    xItem.Add(new XElement("component_type", dr["component_type"].ToString()));
                    xItem.Add(new XElement("field_name", dr["field_name"].ToString()));
                    xItem.Add(new XElement("field_index", dr["field_index"].ToString()));
                    xItem.Add(new XElement("external_value", dr["external_value"].ToString()));
                    xItem.Add(new XElement("label_name", dr["label_name"].ToString()));
                    xItem.Add(new XElement("display_name", dr["display_name"].ToString()));
                    xElem.Add(xItem);                    
                }
                sbLabelName.Append(dr["label_name"].ToString() + (counter == selectedRows.Length -1 ? "" : separator));
                counter++;
            }
            
            //delete other rows except for the top most            
            DataRow[] rows = new DataRow[selectedRows.Length - 1];            
            for (int i = 0; i < selectedRows.Length; i++) {
                dr = gvColumnView.GetDataRow(selectedRows[i]);
                if(drMerge != null && dr.Equals(drMerge)) continue;
                else if (drMerge == null && i == 0) continue;
                rows[i-1] = dr;
            }
            if (drMerge != null)
                dr = drMerge;
            else
                dr = gvColumnView.GetDataRow(selectedRows[0]);

            rows.ForEach(delegate(DataRow y) { y.Delete(); });            
            
            dr["label_name"] = sbLabelName.ToString();
            dr["display_name"] = "Merged Field";
            dr["id"] = 0;
            dr["dialog_id"] = DBNull.Value;
            dr["questionlayout_id"] = DBNull.Value;
            dr["source"] = DBNull.Value;
            dr["component_type"] = DBNull.Value;
            dr["field_name"] = DBNull.Value;
            dr["external_value"] = DBNull.Value;
            dr["field_index"] = DBNull.Value;
            dr["merge_data"] = xElem.ToString();
            
            dtColumnsInView.AcceptChanges();
            gvColumnView.FocusedRowHandle = selectedRows[0];

            ReorderColumnViewPositionIndex();

        }
        private void gvColumnView_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e) {
            if (e.Column.FieldName == "label_name") {
                e.DisplayText = Regex.Replace(e.Value.ToString(), "\"(,|;\\.|:)\"", delegate(Match match) {
                    return match.Groups[1].Value;
                });
            }
        }
        private void btnSaveConfig_Click(object sender, EventArgs e) 
        {
            //if (txtViewName.Text.Length < 1)
            //{
            //    MessageBox.Show("Please enter a view name.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            //if (!dxValidationProvider1.Validate()) 
            //    return;
                      
            if (!ValidateColumnsInView()) 
                return;

            if (!m_LoadedConfig) {
                CreateBlankView _control = new CreateBlankView(true);
                _control.AfterSave += new CreateBlankView.AfterSaveEventHandler(_control_AfterSave);
                PopupDialog _dlg = new PopupDialog() {
                    FormBorderStyle = FormBorderStyle.FixedSingle,
                    MinimizeBox = false,
                    MaximizeBox = false,
                    StartPosition = FormStartPosition.CenterScreen,
                    Text = "New View"
                };
                _dlg.Controls.Add(_control);
                _dlg.ClientSize = new Size(_control.Width + 2, _control.Height + 2);
                _dlg.ShowDialog(this.ParentForm);
            }
            else {
                WaitDialog.Show("Saving Configuration ...");
                this.SaveConfiguration(false);
                WaitDialog.Close();
            }

            //m_ConfigGrid.Complete = true;
            //tbpgeGridConfig.Image = ManagerApplication.Properties.Resources.save_ok;

            #region Saving Logic
            //WaitDialog.Show("Saving configuration...");
            //System.IO.StringWriter writer = new System.IO.StringWriter();
            //DataSet dsResult = null;
            //if (dtColumnsInView.DataSet == null) {
            //    dsResult = new DataSet("view");
            //    dsResult.Tables.Add(dtColumnsInView);
            //} else {
            //    dsResult = dtColumnsInView.DataSet;
            //}
            //dsResult.WriteXml(writer, XmlWriteMode.IgnoreSchema);
            //var xmlConfig = writer.ToString();

            //if (viewConfig == null) {
            //    viewConfig = new view_configuration() {
            //        date_created = DateTime.Now,
            //        created_by = UserSession.CurrentUser.UserId
            //    };
            //} else {
            //    viewConfig.modified_by = UserSession.CurrentUser.UserId;
            //    viewConfig.date_modified = DateTime.Now;
            //}
            //viewConfig.xml_config = xmlConfig;
            //viewConfig.name = m_ViewName;
            //viewConfig.subcampaign_id = m_iSubcampaignID;

            //try {
            //    if (IsNewConfig)
            //        BPContext.view_configuration.AddObject(viewConfig);
            //    BPContext.SaveChanges();

            //    int configID = viewConfig.id;
            //    BindViewList();
            //    lookUpEditViewList.EditValue = configID;
            //    btnDeleteView.Enabled = true;
            //    btnShowXmlViewConfig.Enabled = true;
            //    MessageBox.Show("View configuration has been saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    IsNewConfig = false;
            //    IsSaved = true;

            //    /**
            //     * changing tab color code.
            //     */
            //    //tcViewConfig.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            //    //tcViewConfig.LookAndFeel.UseDefaultLookAndFeel = false;
            //    //tbpgeGridReport.Appearance.Header.BackColor = Color.PaleGreen;
            //    //tbpgeGridReport.Appearance.Header.Options.UseBackColor = true;
                
            //} catch {
            //}
            //WaitDialog.Close();
            #endregion
        }
        private void btnShowXmlViewConfig_Click(object sender, EventArgs e) {
            System.IO.StringWriter writer = new System.IO.StringWriter();
            DataTable dtTemp = dtColumnsInView.Copy();
            DataSet dsResult = new DataSet("view");
            dsResult.Tables.Add(dtTemp);
            dsResult.WriteXml(writer, XmlWriteMode.IgnoreSchema);
            var xmlConfig = writer.ToString();

            if (dtColumnsInView.Rows.Count <= 0) return;

            PopupDialog diag = new PopupDialog();
            MemoEdit memoEdit1 = new MemoEdit();
            memoEdit1.Text = xmlConfig;
            memoEdit1.DeselectAll();
            memoEdit1.Font = new System.Drawing.Font("Courier New", 10, FontStyle.Bold);
            memoEdit1.BackColor = Color.White;
            memoEdit1.Properties.ReadOnly = true;
            diag.Controls.Add(memoEdit1);
            memoEdit1.Dock = DockStyle.Fill;
            diag.Size = new Size(850, 600);
            diag.Text = "XML Configuration";
            diag.StartPosition = FormStartPosition.CenterScreen;
            diag.ShowDialog();
        }
        private void gvColumnView_ValidateRow(object sender, ValidateRowEventArgs e) {
            var rowView = (e.Row as DataRowView);
            if (rowView != null) {
                object displayName = rowView.Row["display_name"];
                int currentRow = (int) rowView.Row["id"];
                ColumnView colView = sender as ColumnView;
                GridColumn column1 = colView.Columns["display_name"];
                if (displayName == DBNull.Value || string.IsNullOrEmpty(displayName.ToString())) {
                    e.Valid = false;
                    colView.SetColumnError(column1, "Display name is required.");
                } else if (!string.IsNullOrEmpty(displayName.ToString())) {
                    DataRow[] matches = dtColumnsInView.Select("display_name = '" 
                        + displayName.ToString().Replace("'", "''")+ "'");//  AND id <> " + currentRow.ToString());
                    if (matches != null && matches.Length > 1) {
                        e.Valid = false;
                        colView.SetColumnError(column1, "Duplicate display name is not allowed. Please choose another name that is unique.");                        
                    } else {
                        colView.ClearColumnErrors();
                        e.Valid = true;
                    }
                } else {
                    colView.ClearColumnErrors();
                    e.Valid = true;
                }
            }            
        }
        private void gvColumnView_InvalidRowException(object sender, InvalidRowExceptionEventArgs e) {
            e.ExceptionMode = ExceptionMode.NoAction;
        }
        private void btnSaveTemplate_Click(object sender, EventArgs e) {
            if (gvColumnView.RowCount <= 0) {
                MessageBox.Show("Please add one or more fields in the Columns In View grid first.", "Save Template", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateColumnsInView()) return;
            if (!AllowedSaveTempalte()) {
                MessageBox.Show("Dialog field components is not allowed to save as a template. Please remove any dialog related fields first before saving as a template.", 
                    "Save Template", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            System.IO.StringWriter writer = new System.IO.StringWriter();
            DataSet dsResult = null;
            if (dtColumnsInView.DataSet == null) {
                dsResult = new DataSet("view");
                dsResult.Tables.Add(dtColumnsInView);
            } else {
                dsResult = dtColumnsInView.DataSet;
            }
            dsResult.WriteXml(writer, XmlWriteMode.IgnoreSchema);
            var xmlConfig = writer.ToString();
            
            PopupDialog diag = new PopupDialog();            
            diag.MaximizeBox = false;
            diag.MinimizeBox = false;            
            ViewTemplate template = new ViewTemplate(false);
            template.XMLConfig = xmlConfig;
            diag.Controls.Add(template);
            template.Dock = DockStyle.Fill;
            Size mSize = new Size(template.Width + 10, template.Height + 50);
            diag.Size = mSize;
            diag.MinimumSize = mSize; 
            diag.Text = "Save Grid Report Template";
            diag.StartPosition = FormStartPosition.CenterScreen;
            diag.ShowDialog();
        }
        private void btnLoadTemplate_Click(object sender, EventArgs e) {
            PopupDialog diag = new PopupDialog();
            diag.MaximizeBox = false;
            diag.MinimizeBox = false;  
            ViewTemplate template = new ViewTemplate(true);
            template.ViewConfigurationModule = this;
            diag.Controls.Add(template);
            template.Dock = DockStyle.Fill;
            Size mSize = new Size(template.Width + 10, template.Height + 50);
            diag.Size = mSize;
            diag.MinimumSize = mSize;
            diag.Text = "Load Grid Report Template";
            diag.StartPosition = FormStartPosition.CenterScreen;
            diag.ShowDialog();
        }
        private void gvAvailableColumns_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void gvColumnView_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void tcViewConfig_SelectedPageChanging(object sender, DevExpress.XtraTab.TabPageChangingEventArgs e)
        {
            if (m_GoDefault) {
                m_GoDefault = false;
                if (tcViewConfig.SelectedTabPage.Equals(tbpgeGridConfig))
                    e.Cancel = true;

                return;
            }

            if (e.PrevPage.Equals(tbpgeGridConfig) && !m_ConfigGrid.Complete) {
                msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Please complete grid configuration first.");
                e.Cancel = true;
                return;
            }

            else if (e.PrevPage.Equals(tbpgeLayoutConfig) && !m_ConfigLayout.Complete) {
                msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Please complete layout configuration first.");
                e.Cancel = true;
                return;
            }

            else if (e.PrevPage.Equals(tbpgeParameterConfig) && !m_ConfigParameter.Complete) {
                msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Please complete parameter configuration first.");
                e.Cancel = true;
                return;
            }
        }
        private void tcViewConfig_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (e.Page.Equals(tbpgeLayoutConfig) && !m_LoadedLayout) {
                WaitDialog.Show("Loading data ...");
                m_LoadedLayout = true;
                this.LoadLayoutConfigTemplates();
                WaitDialog.Close();
            }
            else if (e.Page.Equals(tbpgeParameterConfig) && !m_LoadedParameters) {
                WaitDialog.Show("Loading data ...");
                m_LoadedParameters = true;
                this.LoadParameterConfigTemplates();
                this.LoadParameterSettings(false);
                WaitDialog.Close();
            }
        }
        private void btnLoadLayoutTemplateToReport_Click(object sender, EventArgs e)
        {
            if (gvReportTemplate.RowCount < 1)
                return;
            WaitDialog.Show("Loading designer ...");
            CTAdditionalDataReportTemplate _template = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            string layout = GetLayoutConfig(_template.id);
            if (_template == null || string.IsNullOrEmpty(layout))
            {
                WaitDialog.Close();
                msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Selected report template does not contain a layout.");
                return;
            }

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                view_configuration _item = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_efoViewConfig.id);
                if (_item != null) {
                    _item.report_layout_config = layout;
                    _efDbContext.view_configuration.ApplyCurrentValues(_item);
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_item);
                    m_ConfigLayout.Complete = true;
                    tbpgeLayoutConfig.Image = ManagerApplication.Properties.Resources.save_ok;
                    btnEditReportTemplate.Image = ManagerApplication.Properties.Resources.view_config_completed;
                    this.SetPreviewReport();
                    msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Selected report template layout saved to active report layout.");
                }
            }

            m_efoViewConfig.report_layout_config = layout;
            m_efDbContext.SaveChanges();

            //view_configuration _eftToUpdate = m_efDbContext.view_configuration.FirstOrDefault(i => i.id == m_efoViewConfig.id);
            //_eftToUpdate.report_layout_config = layout;
            //m_efDbContext.SaveChanges();

            //m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            //m_efDbContext.Refresh(RefreshMode.;
            WaitDialog.Close();

            //if (_template == null || string.IsNullOrEmpty(_template.layout_config)) {
            //    MessageBox.Show("Please kindly set a layout first by pressing Edit Report Designer.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}
            //else {
            //    m_oReport = new ReportUserDesigner(_template.layout_config) {
            //        ModuleType = ReportUserDesigner.eModuleType.ViewConfiguration,
            //        ViewConfigId = m_efoViewConfig.id,
            //        Text = "Load Layout Template to Report"
            //    };
            //    m_oReport.AfterSave += new ReportUserDesigner.AfterSaveEventHandler(m_oReport_AfterSave);
            //    WaitDialog.Close();
            //    m_oReport.ShowReportDesigner();
            //}

            //_oReport.AfterSave += new ReportUserDesigner.AfterSaveEventHandler(_oReport_AfterSave);
            //pnlReportLayout.Controls.Clear();
            //pnlReportLayout.Controls. Add(_oReport);
            //_oReport.Show();
        }

        private string GetLayoutConfig(long id)
        {
            string layout = string.Empty;
            using (BrightPlatformEntities entities = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var res = entities.additional_data_report_templates.FirstOrDefault(e => e.id == id);
                if (res != null)
                {
                    layout = res.layout_config;
                    entities.Detach(res);
                }
            }
            return layout;
        }
        private void cbxDefaultTemplate_CheckedChanged(object sender, EventArgs e)
        {
            WaitDialog.Show("Saving data.");
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                CheckEdit _cbx = sender as CheckEdit;
                CTAdditionalDataReportTemplate _ReportTemplate = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
                if (_ReportTemplate != null) {
                    // set the previous default to false.
                    if (_cbx.Checked) {
                        additional_data_report_templates _efeDefaultTemplate = _efDbContext.additional_data_report_templates.FirstOrDefault(i => i.is_default);
                        if (_efeDefaultTemplate != null) {
                            _efeDefaultTemplate.is_default = false;
                            _efeDefaultTemplate.modified_on = DateTime.Now;
                            _efeDefaultTemplate.modified_by = UserSession.CurrentUser.UserId;
                            _efDbContext.additional_data_report_templates.ApplyCurrentValues(_efeDefaultTemplate);
                            _efDbContext.SaveChanges();
                            _efDbContext.Detach(_efeDefaultTemplate);
                        }
                    }

                    additional_data_report_templates _efeReportTemplate = _efDbContext.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplate.id);
                    _efeReportTemplate.is_default = _cbx.Checked;
                    _efeReportTemplate.modified_on = DateTime.Now;
                    _efeReportTemplate.modified_by = UserSession.CurrentUser.UserId;
                    _efDbContext.additional_data_report_templates.ApplyCurrentValues(_efeReportTemplate);
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_efeReportTemplate);
                    this.LoadLayoutConfigTemplates();
                }
            }
            WaitDialog.Close();
        }
        private void cbxDefaultTemplate_EditValueChanging(object sender, ChangingEventArgs e)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                CheckEdit _cbx = sender as CheckEdit;
                if (!_cbx.Checked && _efDbContext.additional_data_report_templates.FirstOrDefault(i => i.is_default == true) != null) {
                    DialogResult _dlg = MessageBox.Show(
                        string.Format("A default report template has already been set.{0}Would you like to set this as a default tempalte instead?", Environment.NewLine), 
                        "Bright Manager", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Question
                    );
                    if (_dlg == DialogResult.No)
                        e.Cancel = true;
                }
            }
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
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                CTAdditionalDataReportTemplate _ReportTemplate = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
                if (_efDbContext.export_view_report_templates.FirstOrDefault(i => i.additional_data_report_template_id == _ReportTemplate.id) != null) {
                    MessageBox.Show("Delete not allowed. Some display view reports are referenced to this template.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (_ReportTemplate.is_default) {
                    string _msg = string.Format(
                        "You are deleting a default report template. You will need to set a default template after deleting for the reports to work well.{0}Are you sure to do this?",
                        Environment.NewLine
                     );
                    _dlg = MessageBox.Show(_msg, "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }
                else {
                    _dlg = MessageBox.Show("Are you sure to delete this template?", "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }

                if (_dlg == DialogResult.No)
                    return;

                _efDbContext.additional_data_report_templates.DeleteObject(
                    _efDbContext.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplate.id)
                );
                _efDbContext.SaveChanges();

                gvReportTemplate.DeleteRow(gvReportTemplate.FocusedRowHandle);
                gvReportTemplate.FocusedRowHandle = 0;
                gvReportTemplate.SelectRow(0);
            }            
            WaitDialog.Close();
        }
        private void btnNewTemplate_Click(object sender, EventArgs e)
        {
            AddReportTemplate _frm = new AddReportTemplate();
            PopupDialog _dlg = new PopupDialog();
            _frm.AfterSave += new AddReportTemplate.AfterSaveEventHandler(_frm_AfterSave);
            _dlg.FormBorderStyle = FormBorderStyle.FixedSingle;
            _dlg.MinimizeBox = false;
            _dlg.MaximizeBox = false;
            _dlg.StartPosition = FormStartPosition.CenterScreen;
            _dlg.Text = "New Report Template";
            _dlg.Controls.Add(_frm);
            _dlg.ClientSize = new Size(_frm.Width + 2, _frm.Height + 2);
            _dlg.ShowDialog(this.ParentForm);
        }
        private void btnEditReportTemplate_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading designer ...");
            view_configuration _item;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _item = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_efoViewConfig.id);
                _efDbContext.Detach(_item);
            }

            if (_item == null || string.IsNullOrEmpty(_item.report_layout_config)) {
                WaitDialog.Close();
                MessageBox.Show("This report has no layout template yet. Please kindly set a layout first.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else {
                m_oReport = new ReportUserDesigner(_item.report_layout_config) {
                    ModuleType = ReportUserDesigner.eModuleType.ViewConfiguration,
                    ViewConfigId = m_efoViewConfig.id,
                    Text = "Edit Report Template"
                };
                m_oReport.AfterSave += new ReportUserDesigner.AfterSaveEventHandler(m_oReport_AfterSave);
                WaitDialog.Close();
                m_oReport.ShowReportDesigner();
            }
        }
        private void btnRefreshTemplate_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading data ...");
            this.LoadLayoutConfigTemplates();
            WaitDialog.Close();
        }
        private void btnRefreshTemplateParamsTab_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading data ...");
            this.LoadParameterConfigTemplates();
            this.LoadParameterSettings(false);
            WaitDialog.Close();
        }
        private void msgPopupRegular_BeforeFormShow(object sender, DevExpress.XtraBars.Alerter.AlertFormEventArgs e)
        {
            DevExpress.Skins.Skin currentSkin;
            currentSkin = DevExpress.Skins.BarSkins.GetSkin(e.AlertForm.LookAndFeel);
            DevExpress.Skins.SkinElement element;
            element = currentSkin["AlertWindow"];
            Graphics g = Graphics.FromImage(element.Image.Image);
            g.FillRectangle(Brushes.PaleGreen, new Rectangle(0, 0, element.Image.Image.Width, element.Image.Image.Height));
        }
        private void btnSetActiveLayoutToTemplate_Click(object sender, EventArgs e)
        {
            if (gvReportTemplate.RowCount < 1)
                return;

            CTAdditionalDataReportTemplate _ReportTemplate = gvReportTemplate.GetFocusedRow() as CTAdditionalDataReportTemplate;
            if (_ReportTemplate == null)
                return;

            DialogResult _dlg = MessageBox.Show("Are you sure to overwrite selected template with the active layout?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlg == DialogResult.No)
                return;

            WaitDialog.Show("Saving data ...");
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                view_configuration _eftViewConfig = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_efoViewConfig.id);
                if (_eftViewConfig != null && !string.IsNullOrEmpty(_eftViewConfig.report_layout_config)) {
                    additional_data_report_templates _item = _efDbContext.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplate.id);
                    if (_item != null) {
                        _item.layout_config = m_efoViewConfig.report_layout_config;
                        _efDbContext.additional_data_report_templates.ApplyCurrentValues(_item);
                        _efDbContext.SaveChanges();
                        _efDbContext.Detach(_item);
                        gvReportTemplate.SetRowCellValue(gvReportTemplate.FocusedRowHandle, "layout_config", m_efoViewConfig.report_layout_config);
                        gvReportTemplate.SetRowCellValue(gvReportTemplate.FocusedRowHandle, "has_layout", true);
                        msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Active report layout saved for the selected report template.");
                    }
                }
                else
                    msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Active report does not contain any layout yet.");

                _efDbContext.Detach(_eftViewConfig);
            }
            WaitDialog.Close();
        }
        private void btnSaveSettingToReport_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Saving data.");
            string _xml = SerializeUtility.Serialize(templateData);
            m_efoViewConfig.report_data_config = _xml;
            m_efDbContext.view_configuration.ApplyCurrentValues(m_efoViewConfig);
            m_efDbContext.SaveChanges();
            m_ConfigParameter.Complete = true;
            tbpgeParameterConfig.Image = ManagerApplication.Properties.Resources.save_ok;
            this.SetPreviewReport();
            msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Layout saved to active report.");
            WaitDialog.Close();

            /** /
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                string _xml = SerializeUtility.Serialize(templateData);
                view_configuration _eftViewConfig = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_efoViewConfig.id);
                if (_eftViewConfig != null) {
                    _eftViewConfig.report_data_config = _xml;
                    _eftViewConfig.modified_by = UserSession.CurrentUser.UserId;
                    _efDbContext.view_configuration.ApplyCurrentValues(_eftViewConfig);
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftViewConfig);
                    m_ConfigParameter.Complete = true;
                    tbpgeParameterConfig.Image = ManagerApplication.Properties.Resources.save_ok;
                    msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Layout saved to active report.");
                }
            }
            /**/
        }
        private void btnNewReportTemplateParamsTab_Click(object sender, EventArgs e)
        {
            AddReportTemplate _frmParamsTab = new AddReportTemplate();
            PopupDialog _dlg = new PopupDialog();
            _frmParamsTab.AfterSave += new AddReportTemplate.AfterSaveEventHandler(_frmParamsTab_AfterSave);
            _dlg.FormBorderStyle = FormBorderStyle.FixedSingle;
            _dlg.MinimizeBox = false;
            _dlg.MaximizeBox = false;
            _dlg.StartPosition = FormStartPosition.CenterScreen;
            _dlg.Text = "New Report Template";
            _dlg.Controls.Add(_frmParamsTab);
            _dlg.ClientSize = new Size(_frmParamsTab.Width + 2, _frmParamsTab.Height + 2);
            _dlg.ShowDialog(this.ParentForm);
        }
        private void btnDeleteReportTemplateParamsTab_Click(object sender, EventArgs e)
        {
            /**
             * TODO: 
             * please kindly confirm to dave/johan on bahavoir on deleting templates.
             * what will happen if we delete a template with reports already using it as a template?
             * 
             * TEMPORARY SOLUTION:
             * will not allow to delete templates when its being referenced by reports.
             */

            if (gvReportTemplateParameterTab.RowCount < 1)
                return;

            /**
             * check if there are existing reports being referenced to the selected report template.
             */
            DialogResult _dlg;
            WaitDialog.Show("Deleting data.");
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                CTAdditionalDataReportTemplate _ReportTemplate = gvReportTemplateParameterTab.GetFocusedRow() as CTAdditionalDataReportTemplate;
                if (_efDbContext.export_view_report_templates.FirstOrDefault(i => i.additional_data_report_template_id == _ReportTemplate.id) != null) {
                    MessageBox.Show("Delete not allowed. Some display view reports are referenced to this template.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (_ReportTemplate.is_default) {
                    string _msg = string.Format(
                        "You are deleting a default report template. You will need to set a default template after deleting for the reports to work well.{0}Are you sure to do this?",
                        Environment.NewLine
                     );
                    _dlg = MessageBox.Show(_msg, "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }
                else {
                    _dlg = MessageBox.Show("Are you sure to delete this template?", "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }

                if (_dlg == DialogResult.No)
                    return;

                _efDbContext.additional_data_report_templates.DeleteObject(
                    _efDbContext.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplate.id)
                );
                _efDbContext.SaveChanges();

                gvReportTemplateParameterTab.DeleteRow(gvReportTemplateParameterTab.FocusedRowHandle);
                gvReportTemplateParameterTab.FocusedRowHandle = 0;
                gvReportTemplateParameterTab.SelectRow(0);
            }
            WaitDialog.Close();
        }
        private void btnSetActiveLayoutToSelectedTemplate2_Click(object sender, EventArgs e)
        {
            if (gvReportTemplateParameterTab.RowCount < 1)
                return;

            CTAdditionalDataReportTemplate _ReportTemplate = gvReportTemplateParameterTab.GetFocusedRow() as CTAdditionalDataReportTemplate;
            if (_ReportTemplate == null)
                return;

            DialogResult _dlg = MessageBox.Show("Are you sure to overwrite selected template with the active layout?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlg == DialogResult.No)
                return;

            WaitDialog.Show("Saving data ...");
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                view_configuration _eftViewConfig = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_efoViewConfig.id);
                if (_eftViewConfig != null && !string.IsNullOrEmpty(_eftViewConfig.report_data_config)) {
                    additional_data_report_templates _item = _efDbContext.additional_data_report_templates.FirstOrDefault(i => i.id == _ReportTemplate.id);
                    if (_item != null) {
                        _item.data_config = m_efoViewConfig.report_data_config;
                        _efDbContext.additional_data_report_templates.ApplyCurrentValues(_item);
                        _efDbContext.SaveChanges();
                        _efDbContext.Detach(_item);
                        gvReportTemplateParameterTab.SetRowCellValue(gvReportTemplateParameterTab.FocusedRowHandle, "data_config", m_efoViewConfig.report_data_config);
                        gvReportTemplateParameterTab.SetRowCellValue(gvReportTemplateParameterTab.FocusedRowHandle, "has_params", true);
                        msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Active report layout saved for the selected report template.");
                    }
                }
                else
                    msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Active report does not contain any layout yet.");

                _efDbContext.Detach(_eftViewConfig);
            }
            WaitDialog.Close();
        }
        private void btnLoadSelectedTemplateToActiveLayout2_Click(object sender, EventArgs e)
        {
            if (gvReportTemplateParameterTab.RowCount < 1)
                return;
            WaitDialog.Show("Loading designer ...");
            CTAdditionalDataReportTemplate _template = gvReportTemplateParameterTab.GetFocusedRow() as CTAdditionalDataReportTemplate;
            string layout = GetLayoutConfig(_template.id);
            if (_template == null || string.IsNullOrEmpty(layout))
            {
                WaitDialog.Close();
                msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Selected report template does not contain a layout.");
                return;
            }

            this.LoadParameterSettings();
            //using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
            //    view_configuration _item = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_efoViewConfig.id);
            //    if (_item != null) {
            //        _item.report_data_config = _template.data_config;
            //        _efDbContext.view_configuration.ApplyCurrentValues(_item);
            //        _efDbContext.SaveChanges();
            //        _efDbContext.Detach(_item);
            //        this.LoadParameterSettings();
            //    }
            //}
            //msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Selected report template layout saved to active report layout.");
            WaitDialog.Close();
        }
        private void simpleButtonClear_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Clearing grid ...");
            templateData = new TemplateProperty {
                DynamicProperty = new List<TemplateDynamicData>(),
                IsFooterVisible = true,
                IsPageNumberVisible = true,
                IsEmptyDynamicValueVisible = false
            };
            vgridReportParameter.CreateDefaultLayoutSetting(templateData);
            WaitDialog.Close();
        }
        private void btnAddLayoutSetting_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Adding element ...");
            TemplateDynamicData templateDynamic = null;
            if (templateData.DynamicProperty == null) {
                templateData.DynamicProperty = new List<TemplateDynamicData>();
            }

            switch (cboLayoutElement.EditValue.ToString()) {

                case "Page Break":
                    templateDynamic = new TemplateDynamicData() { Type = TemplateDynamicType.PageBreak, IsVisible = true };
                    vgridReportParameter.AddPageBreak(templateDynamic);
                    break;

                case "Text Field":
                    templateDynamic = new TemplateDynamicData {
                        Type = TemplateDynamicType.TextField,
                        IsVisible = true,
                        Text = new TextField { Size = TextFieldFontSize.Normal, Style = TextFieldFontStyle.None }
                    };
                    vgridReportParameter.AddTextField(templateDynamic);
                    break;

                case "Statistical Component":
                    templateDynamic = new TemplateDynamicData {
                        Type = TemplateDynamicType.Statistics,
                        IsVisible = true,
                        Statistics = new StatisticsTemplate {
                            IsNullCategoryVisible = true,
                            IsTotalCountVisible = true,
                            GraphSize = GraphSize.Normal,
                            PercentageValueSortBy = PercentageValueSortingOption.PercentageAscending
                        }
                    };
                    this.SetStatisticsAvailableColumns();
                    vgridReportParameter.AddStatistics(templateDynamic);
                    break;
            }
            WaitDialog.Close();
        }
        private void simpleButtonReOrder_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Sorting items.");
            this.LoadParameterSettings();
            WaitDialog.Close();
        }
        private void btnPreviewView_Click(object sender, EventArgs e)
        {
            if (!m_ConfigGrid.Complete || !m_ConfigLayout.Complete || !m_ConfigParameter.Complete) {
                msgPopupRegular.Show(this.ParentForm, "Bright Manager", "Please kindly complete all configuration first.");
                return;
            }

            if (m_efoViewConfig == null)
                return;

            if (btnPreviewViewOnClick != null)
                btnPreviewViewOnClick(m_efoViewConfig.id);
        }
        #endregion

        #region DialogFields
        public class DialogField {
            public string id { get; set; }
            public string dialog_id { get; set; }
            public string questionlayout_id { get; set; }
            public string source { get; set; }
            public string component_type { get; set; }
            public string field_name { get; set; }
            public string field_index { get; set; }
            public string external_value { get; set; }
            public string label_name { get; set; }
            public string display_name { get; set; }
        }
        #endregion
    }
}
