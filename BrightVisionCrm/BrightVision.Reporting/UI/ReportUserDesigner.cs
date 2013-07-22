
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Data.Objects;

using DevExpress.XtraReports.UserDesigner;
using DevExpress.XtraReports.Extensions;
using DevExpress.XtraReports.UI;
using System.Data.OleDb;
using BrightVision.Model;
using BrightVision.Common.Business;

namespace BrightVision.Reporting.UI
{
    public partial class ReportUserDesigner : Form
    {
        #region Constructor
        public ReportUserDesigner()
        {
            InitializeComponent();
            //string _LayoutConfig = m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.is_default == true).layout_config;
            //if (!string.IsNullOrEmpty(_LayoutConfig)) {
            //    MemoryStream _stream = new MemoryStream();
            //    StreamWriter _writer = new StreamWriter(_stream, Encoding.Default);
            //    _writer.Write(_LayoutConfig);
            //    _writer.Flush();
            //    _stream.Position = 0;
            //    xreport.LoadLayout(_stream);
            //}
            //this.LoadDefault();
        }
        /**
         * created this constructor to support loading of default templates coming from the UI,
         * not on the report templates.
         */
        public ReportUserDesigner(bool pLoadApplicationDefaultTemplate)
        {
            pLoadApplicationDefaultTemplate = true;
            InitializeComponent();
            this.LoadDefault();
        }
        public ReportUserDesigner(XtraReportDefault xreport, bool NoChangeNamePopup)
        {
           // this.xreport = xreport;
            //ReportDesignExtension.AssociateReportWithExtension(xreport, ExtensionName);
            nochangenamepop = NoChangeNamePopup;
            InitializeComponent();
            LoadDefault();
        }
        public ReportUserDesigner(XtraReportDefault xreport, DataSet dataset)
        {
           // this.xreport = xreport;
            this.xreport.DataSource = dataset;
            //ReportDesignExtension.AssociateReportWithExtension(xreport, ExtensionName);
            nochangenamepop = true;
            InitializeComponent();
            LoadDefault();
        }
        public ReportUserDesigner(DataSet dataset)
        {
         //   xreport = XtraReportDefault.CreateXtraReport(dataset);
            InitializeComponent();
        }
        public ReportUserDesigner(ReportDataSet dataset)
        {
            InitializeComponent();
            xreport = XtraReportDefault.CreateXtraReportDefaultTemplate(dataset);
            additional_data_report_templates _item = m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.is_default == true);
            if (_item != null) {
                if (!string.IsNullOrEmpty(_item.layout_config)) {
                    MemoryStream _stream = new MemoryStream();
                    StreamWriter _writer = new StreamWriter(_stream, Encoding.Default);
                    _writer.Write(_item.layout_config);
                    _writer.Flush();
                    _stream.Position = 0;
                    xreport.LoadLayout(_stream);
                }
            }
            this.LoadDefault();
        }
        public ReportUserDesigner(ReportDataSet dataset, string layoutConfig)
        {
            //xreport = XtraReportDefault.CreateXtraReport(dataset);
            MemoryStream _stream = new MemoryStream();
            StreamWriter _writer = new StreamWriter(_stream, Encoding.Default);
            _writer.Write(layoutConfig);
            _writer.Flush();
            _stream.Position = 0;
            //xreport.LoadLayoutFromXml(_stream);
            xreport.LoadLayout(_stream);
            xreport.DataSource = dataset;
            InitializeComponent();
        }
        public ReportUserDesigner(string layoutConfig)
        {
            MemoryStream _stream = new MemoryStream();
            StreamWriter _writer = new StreamWriter(_stream, Encoding.Default);
            _writer.Write(layoutConfig);
            _writer.Flush();
            _stream.Position = 0;
            xreport.LoadLayout(_stream);
            InitializeComponent();

            this.Load += ReportUserDesigner_Load;
        }

        void ReportUserDesigner_Load(object sender, EventArgs e)
        {
            this.MdiChildren[0].FormClosing += MDIChild_FormClosing;
            
        }

        //DAN: Prevent MDIChild from closing like being pressed CTRL + F4.
        //Proper way to close is during save or close the parent/report holder of the child
        private void MDIChild_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
        #endregion

        #region Public Properties
        public int ViewConfigId;
        public int SubCampaignId;
        public long ReportTemplateId;


        public eModuleType ModuleType = eModuleType.Default;
        public enum eModuleType
        {
            Default, // View Display
            ReportTemplate,
            ViewConfiguration
        }
        public XtraReportDefaultTemplate Report { get { return xreport; } set { xreport = value; } }
        #endregion

        #region Private Properties
        private XtraReportDefaultTemplate xreport = new XtraReportDefaultTemplate();
        private bool nochangenamepop = false;
        private const string ExtensionName = "Custom";
        //private TemplateName m_frmTemplateName;
        private BrightPlatformEntities m_efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        private string m_TemplateName = string.Empty;
        #endregion

        #region Public Subscribed Events & Args
        public delegate void AfterSaveEventHandler(object sender, AfterSaveArgs e);
        public event AfterSaveEventHandler AfterSave;
        public class AfterSaveArgs : EventArgs
        {
            public export_view_report_templates efExportViewReportTemplate;
            public additional_data_report_templates efAdditionalDataReportTemplate;
            public view_configuration efViewConfiguration;
        }

        public event SaveReportEventHandler SaveReport;
        public event ChangeNameEventHandler ChangeReportName;
        #endregion

        #region Public Methods
        public void ShowReportDesigner()
        {
            this.xrDesignMdiController1.OpenReport(xreport);
            this.ShowDialog();
        }
        public void Show()
        {
            this.ShowDialog();
        }
        #endregion

        #region Private Methods
        private void LoadDefault()
        {
            // DataSet dataset = new DataSet("ReportData");
            //  xreport.DataSource = ReportDataSet.GetSampleData();
            this.xrDesignMdiController1.OpenReport(xreport);
        }
        private SaveReportEventArgs GetEventArgs()
        {
            var ms = new MemoryStream();
            //xreport.SaveLayoutToXml(ms);
            xreport.SaveLayout(ms);
            ms.Position = 0;

            var sr = new StreamReader(ms, Encoding.Default);
            string _reportTemplate = sr.ReadToEnd();
            var args = new SaveReportEventArgs(_reportTemplate);
            return args;
        }
        #endregion

        #region Object Events
        private void barButtonItemSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //m_frmTemplateName = new TemplateName();
            //m_frmTemplateName.AfterSave += new TemplateName.AfterSaveEventHandler(m_frmTemplateName_AfterSave);
            //m_frmTemplateName.ShowDialog(this);
            WaitDialog.Show("Saving template......");
            export_view_report_templates _efeExportViewTemplate = new export_view_report_templates(); // used in EditorDisplayViewReportTemplate
            additional_data_report_templates _efeReportTemplate = new additional_data_report_templates(); // used in EditorReportTemplate
            view_configuration _efoViewConfiguration = new view_configuration(); // used in report configuration
            SaveReportEventArgs _args = GetEventArgs();

            /**
             * if view display
             */
            #region Logic
            if (ModuleType == eModuleType.Default) {
                _efeExportViewTemplate = m_efDbModel.export_view_report_templates.FirstOrDefault(i => i.view_config_id == ViewConfigId);
                if (_efeExportViewTemplate != null)  {
                    _efeExportViewTemplate.layout_config = _args.ReportTemplate;
                    _efeExportViewTemplate.modified_on = DateTime.Now;
                    _efeExportViewTemplate.modified_by = UserSession.CurrentUser.UserId;
                    m_efDbModel.export_view_report_templates.ApplyCurrentValues(_efeExportViewTemplate);
                }
                else {
                    _efeExportViewTemplate = new export_view_report_templates() {
                        sub_campaign_id = SubCampaignId,
                        view_config_id = ViewConfigId,
                        layout_config = _args.ReportTemplate,
                        created_on = DateTime.Now,
                        created_by = UserSession.CurrentUser.UserId,
                        modified_on = DateTime.Now,
                        modified_by = UserSession.CurrentUser.UserId
                    };
                    m_efDbModel.export_view_report_templates.AddObject(_efeExportViewTemplate);
                }
                m_efDbModel.SaveChanges();
            }
            #endregion

            /**
             * if report template.
             * only update of item, no creating of new record.
             * creation of report template is done at the gui part of the EditorReportTemplate.cs
             */
            #region Logic
            else if (ModuleType == eModuleType.ReportTemplate) {
                _efeReportTemplate = m_efDbModel.additional_data_report_templates.FirstOrDefault(i => i.id == ReportTemplateId);
                if (_efeExportViewTemplate != null) {
                    _efeReportTemplate.layout_config = _args.ReportTemplate;
                    _efeReportTemplate.modified_on = DateTime.Now;
                    _efeReportTemplate.modified_by = UserSession.CurrentUser.UserId;
                    m_efDbModel.additional_data_report_templates.ApplyCurrentValues(_efeReportTemplate);
                }
                m_efDbModel.SaveChanges();
            }
            #endregion

            /**
             * if view configuration.
             * the layout is being saved to the view_configuration table.
             */
            #region Logic
            else if (ModuleType == eModuleType.ViewConfiguration) {
                _efoViewConfiguration = m_efDbModel.view_configuration.FirstOrDefault(i => i.id == ViewConfigId);
                if (_efoViewConfiguration != null) {
                    _efoViewConfiguration.report_layout_config = _args.ReportTemplate;
                    _efeReportTemplate.modified_on = DateTime.Now;
                    _efeReportTemplate.modified_by = UserSession.CurrentUser.UserId;
                    m_efDbModel.view_configuration.ApplyCurrentValues(_efoViewConfiguration);
                }
                m_efDbModel.SaveChanges();
            }
            #endregion

            //if (_efeExportViewTemplate != null)
            //{
            //    _efeExportViewTemplate.layout_config = _args.ReportTemplate;
            //    _efeExportViewTemplate.modified_on = DateTime.Now;
            //    _efeExportViewTemplate.modified_by = UserSession.CurrentUser.UserId;
            //    m_efDbModel.export_view_report_templates.ApplyCurrentValues(_efeExportViewTemplate);
            //}
            //else
            //{
            //    _efeExportViewTemplate = new export_view_report_templates() 
            //    {
            //        sub_campaign_id = SubCampaignId,
            //        view_config_id = ViewConfigId,
            //        layout_config = _args.ReportTemplate,
            //        created_on = DateTime.Now,
            //        created_by = UserSession.CurrentUser.UserId,
            //        modified_on = DateTime.Now,
            //        modified_by = UserSession.CurrentUser.UserId
            //    };
            //    m_efDbModel.export_view_report_templates.AddObject(_efeExportViewTemplate);
            //}
           
            if (ModuleType == eModuleType.Default && AfterSave != null)
                AfterSave(this, new AfterSaveArgs() { efExportViewReportTemplate = _efeExportViewTemplate });

            else if (ModuleType == eModuleType.ReportTemplate && AfterSave != null)
                AfterSave(this, new AfterSaveArgs() { efAdditionalDataReportTemplate = _efeReportTemplate });

            else if (ModuleType == eModuleType.ViewConfiguration && AfterSave != null)
                AfterSave(this, new AfterSaveArgs() { efViewConfiguration = _efoViewConfiguration });

            WaitDialog.Close();
            MessageBox.Show("Active layout for visual report saved.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
            
            //if(!nochangenamepop){
            //    string name = string.Empty;
            //    name = this.GetTemplateName();
            //    if (name == string.Empty)
            //        return;
            //    args.Name = name;
            //}

            //if (SaveReport != null)
            //    SaveReport(this, args);
        }
        private void ReportUserDesigner_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.xrDesignMdiController1.ActiveDesignPanel.ReportState = ReportState.None;
            }
            catch (Exception ex) { }

            //if (this.xrDesignMdiController1.ActiveDesignPanel != null)
            //    this.xrDesignMdiController1.ActiveDesignPanel.ReportState = ReportState.Saved;
        }

        void ActiveDesignPanel_Deactivated(object sender, EventArgs e)
        {
            barButtonItemSave.Enabled = false;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            
            if(keyData == (Keys.Control | Keys.S)){
                barButtonItemSave.PerformClick();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Static Method
        public static XtraReportDefault StringToXtraReportDefault(string xml)
        {
            var ms2 = new MemoryStream();
            var sr2 = new StreamWriter(ms2, Encoding.UTF8);
            sr2.Write(xml);
            sr2.Flush();
            ms2.Position = 0;
            XtraReportDefault  xreport = new XtraReportDefault();
            xreport.LoadLayoutFromXml(ms2);
            sr2.Close();
            sr2.Dispose();
            return xreport;
        }
        #endregion
    }

    #region Public Subscribed Events & Args

    public delegate void SaveReportEventHandler(object sender, SaveReportEventArgs e);
    public delegate void ChangeNameEventHandler(object sender, ref ReportNameEventArgs e);


    public class SaveReportEventArgs : EventArgs
    {
        public SaveReportEventArgs(string xmltemplate)
        {
            this.ReportTemplate = xmltemplate;
        }
        public string ReportTemplate;
        public string Name;
    }   
    public class ReportNameEventArgs:EventArgs{
        public bool Exist{get;set;}
        public string Name{get;set;}
    }

    #endregion

    class ReportExtension : ReportStorageExtension
    {
        public override void SetData(XtraReport report, Stream stream)
        {
            report.SaveLayoutToXml(stream);
        }
    }
    class DesignExtension : ReportDesignExtension
    {
        protected override bool CanSerialize(object data)
        {
            return data is DataSet || data is OleDbDataAdapter;
        }
        protected override string SerializeData(object data, XtraReport report)
        {
            if (data is DataSet)
                return (data as DataSet).GetXmlSchema();
            if (data is OleDbDataAdapter)
            {
                OleDbDataAdapter adapter = data as OleDbDataAdapter;
                return adapter.SelectCommand.Connection.ConnectionString +
                    "\r\n" + adapter.SelectCommand.CommandText;
            }

            return base.SerializeData(data, report);
        }

        protected override bool CanDeserialize(string value, string typeName)
        {
            return typeof(DataSet).FullName ==
                typeName || typeof(OleDbDataAdapter).FullName == typeName;
        }
        protected override object DeserializeData(string value, string typeName, XtraReport report)
        {
            if (typeof(DataSet).FullName == typeName)
            {
                DataSet dataSet = new DataSet();
                dataSet.ReadXmlSchema(new StringReader(value));
                return dataSet;
            }
            if (typeof(OleDbDataAdapter).FullName == typeName)
            {
                OleDbDataAdapter adapter = new OleDbDataAdapter();
                string[] values = value.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                adapter.SelectCommand = new OleDbCommand(values[1], new OleDbConnection(values[0]));
                return adapter;
            }
            return base.DeserializeData(value, typeName, report);
        }
    }
}
