
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Data;
using System.Configuration;

using DevExpress.XtraTab;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using DevExpress.XtraGrid.Columns;

using BrightVision.Reporting;
using BrightVision.Reporting.Template;
using BrightVision.Reporting.UI;
using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
using BrightVision.DQControl;
using BrightVision.DQControl.Utilities;
using System.IO;
using System.ComponentModel;
using DevExpress.XtraEditors.Repository;
using BrightVision.Common.Events.Core;
using System.Diagnostics;
using DevExpress.XtraPrinting;

namespace BrightVision.Reporting.Utility
{
    public partial class ReportsUtility: Form
    {
        #region Constructors
        public ReportsUtility()
        {
            m_DatabaseConnection = UserSession.EntityConnection.ConnectionString;
            m_ReportsPath = ConfigManager.AppSettings["PdfReportsPath"].ToString();
            this.InitializeComponent();
        }
        public ReportsUtility(string pDatabaseConnection, bool pIsWebPortalcall)
        {
            m_DatabaseConnection = pDatabaseConnection;
            m_IsWebPortalCall = pIsWebPortalcall;
            this.InitializeComponent();
        }
        #endregion

        #region Public Events & Args
        public event EventHandler OnReportPageCompleted;
        #endregion

        #region Public Properties
        public enum eCallingEnvironment {
            BrightManager_ViewDisplay = 1,
            BrightSales_ViewDisplay = 2,
            BrightSales_SendEmail = 3,
            BrightManager_SaveAccountPerPdf = 4,
            BrightSales_SaveAccountPerPdf = 5,
            None = 0
        }
        public enum eDisplayMode {
            AccountsContacts_WithDialogData = 1,
            AccountsContacts_WithCallAttempts = 2,
            None = 0
        }
        public enum eCallingApplication {
            BrightManager = 1,
            BrightSales = 2,
            None = 0
        }
        public string CampaignInfo { get; set; }
        public List<SubcampaignData> LSubCampaignData { get; set; }
        public eCallingEnvironment CallingEnvironment { get; set; }
        public eCallingApplication CallingApplication { get; set; }
        public eDisplayMode DisplayMode { get; set; }
        public int AccountId { get; set; }
        public string WebPortalRequester
        {
            set { m_WebPortalRequester = value; }
        }
        public string ReportsPath
        {
            set { m_ReportsPath = value; }
        }
        public string GridFilterString
        {
            set { m_GridFilterString = value; }
        }
        public string GridSortInfo
        {
            set { m_GridSortInfo = value; }
        }
        public string GridColumnsInfo
        {
            set { m_GridColumnsInfo = value; }
        }
        #endregion

        #region Private Properties
        public class ViewCofigData {
            public int id { get; set; }
            public int subcampaign_id { get; set; }
            public string name { get; set; }
        }
        public class SubcampaignData {
            public int id { get; set; }
            public string title { get; set; }
        }

        private List<ViewCofigData> m_lstViewConfigData = null;
        private delegate void ReportPagePoolDelegate(XtraTabPage pGroup);
        private ReportPage m_TabPage = null;
        private GridControl gcReport;
        private GridView gvReport;
        private BackgroundWorker bw = null;
        private string m_DatabaseConnection = string.Empty;
        private bool m_IsWebPortalCall = false;
        private string m_WebPortalRequester = string.Empty;
        private string m_ReportsPath = string.Empty;
        private string m_GridFilterString = string.Empty;
        private string m_GridSortInfo = string.Empty;
        private string m_GridColumnsInfo = string.Empty;
        #endregion

        #region Public Methods
        public static void CreatePdfFilesPerAccount(string pWebRequestId)
        {
            string _WebPortalUrl = ConfigManager.AppSettings["WebPortalUrl"].ToString(); // ConfigurationManager.AppSettings["WebPortalUrl"].ToString();
            string _WebPortalRequest = string.Format("{0}/reports/download?pdata={1}", _WebPortalUrl, pWebRequestId);
            Process.Start(_WebPortalRequest);
        }
        public static void SendReportRequest(string pWebRequestId)
        {
            string _WebPortalUrl = ConfigManager.AppSettings["WebPortalUrl"].ToString(); // ConfigurationManager.AppSettings["WebPortalUrl"].ToString();
            string _WebPortalRequest = string.Format("{0}/reports/show?pdata={1}", _WebPortalUrl, pWebRequestId);
            Process.Start(_WebPortalRequest);
        }
        public static void SendMailRequest(string pWebRequestId)
        {
            string _WebPortalUrl = ConfigManager.AppSettings["WebPortalUrl"].ToString();
            string _WebPortalRequest = string.Format("{0}/services/sendemail?pdata={1}", _WebPortalUrl, pWebRequestId);
            Process.Start(_WebPortalRequest);
        }
        public void GenerateReportPages(int[] pSubcampaignIds, int pViewConfigId = 0)
        {
            XtraTabControl _DummyTab = new XtraTabControl();
            this.GenerateReportPages(ref _DummyTab, pSubcampaignIds, pViewConfigId);
        }
        public void GenerateReportPages(ref XtraTabControl pTabControl, int[] pSubcampaignIds, int pViewConfigId = 0)
        {
            List<XtraTabPage> _lstGroup = new List<XtraTabPage>();
            ReportPage _ReportPage = null;
            int _DefaultTab = 0;
            int _SubcampaignId = 0;
            string _SubCampaignTitle = string.Empty;
            SubcampaignData _SubCampaignData = null;

            m_lstViewConfigData = this.GetConfigurationInfos(pSubcampaignIds);
            for (int x = 0; x < m_lstViewConfigData.Count; ++x) {
                /**
                 * if specific view is specified (pViewConfigId > 0), we will only need to use that view.
                 * so we can save process.
                 */
                if (pViewConfigId > 0 && m_lstViewConfigData[x].id != pViewConfigId)
                    continue;

                if (m_lstViewConfigData[x].id == pViewConfigId)
                    _DefaultTab = x;

                _SubcampaignId = m_lstViewConfigData[x].subcampaign_id;
                if (pSubcampaignIds.Length > 1) {
                    _SubCampaignData = LSubCampaignData.FirstOrDefault(f => f.id == _SubcampaignId);
                    if (_SubCampaignData != null)
                        _SubCampaignTitle = _SubCampaignData.title + ">";
                }

                _ReportPage = new ReportPage(ref this.gcReport, ref this.gvReport, string.Format("{0}{1}", _SubCampaignTitle, m_lstViewConfigData[x].name), CallingEnvironment, m_IsWebPortalCall) {
                    ConfigData = m_lstViewConfigData[x],
                    DatabaseConnection = m_DatabaseConnection,
                    WebPortalRequester = m_WebPortalRequester,
                    ReportsPath = m_ReportsPath,
                    CallingApplication = CallingApplication
                };

                _lstGroup.Add(_ReportPage);
            }

            /**
             * if its a web portal call, no need to thread tab loading.
             * since we only get one tab to process when called from web portal.
             */
            if (CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail || m_IsWebPortalCall) {
                this.GenerateReportPage(_lstGroup[0]);
                return;
            }

            pTabControl.BeginUpdate();
            pTabControl.TabPages.AddRange(_lstGroup.ToArray());
            pTabControl.EndUpdate();
            pTabControl.SelectedTabPageIndex = _DefaultTab;
            this.StartPoolingReportPages(ref pTabControl);
        }
        public void ReportPagePreview()
        {
            m_TabPage.ReportPagePreview(AccountId);
        }
        public string GenerateReports()
        {
            return m_TabPage.GenerateReports(AccountId);
        }
        public bool AccountDataExists()
        {
            return m_TabPage.AccountDataExists(AccountId);
        }
        public bool VerifyReportTemplate(int pViewConfigId, ref string pReturnMessage)
        {
            view_configuration _eftConfigData;
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(m_DatabaseConnection) { CommandTimeout = 0 }) {
                _eftConfigData = _efDbModel.view_configuration.FirstOrDefault(i => i.id == pViewConfigId);
                _efDbModel.Detach(_eftConfigData);
            }

            if (_eftConfigData == null || _eftConfigData.report_layout_config == null) {
                pReturnMessage = "No layout available for the selected view.";
                return false;
            }

            if (string.IsNullOrEmpty(_eftConfigData.report_data_config)) {
                pReturnMessage = "No parameter layout has been set for this report.";
                return false;
            }

            return true;
        }
        public List<string> CreatePdfPerAccount()
        {
            return m_TabPage.CreatePdfPerAccount();
        }
        #endregion

        #region Private Methods
        private void InitializeComponent()
        {
            this.gcReport = new DevExpress.XtraGrid.GridControl();
            this.gvReport = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.gcReport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvReport)).BeginInit();
            this.SuspendLayout();
            // 
            // gcReport
            // 
            this.gcReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcReport.Location = new System.Drawing.Point(0, 0);
            this.gcReport.MainView = this.gvReport;
            this.gcReport.Name = "gcReport";
            this.gcReport.Size = new System.Drawing.Size(616, 353);
            this.gcReport.TabIndex = 0;
            this.gcReport.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvReport});
            // 
            // gvReport
            // 
            this.gvReport.GridControl = this.gcReport;
            this.gvReport.Name = "gvReport";
            this.gvReport.OptionsFind.AlwaysVisible = false;
            this.gvReport.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gvReport.OptionsBehavior.AutoPopulateColumns = true;
            this.gvReport.OptionsSelection.MultiSelect = false;
            this.gvReport.OptionsView.ShowGroupPanel = false;
            this.gvReport.OptionsView.ColumnAutoWidth = false;
            this.gvReport.OptionsBehavior.Editable = false;
            // 
            // ReportsUtility
            // 
            this.ClientSize = new System.Drawing.Size(616, 353);
            this.Controls.Add(this.gcReport);
            this.Name = "ReportsUtility";
            ((System.ComponentModel.ISupportInitialize)(this.gcReport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvReport)).EndInit();
            this.ResumeLayout(false);

        }
        private List<ViewCofigData> GetConfigurationInfos(int[] pSubcampaignIds)
        {
            List<ViewCofigData> _lstData = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(m_DatabaseConnection)) {
                if (pSubcampaignIds.Length <= 0) 
                    return null;

                _lstData = _efDbContext.view_configuration
                    .Where(x => pSubcampaignIds.Contains(x.subcampaign_id) && x.MGC == false)
                    .Select(x => new ViewCofigData { 
                        id = x.id, 
                        name = x.name,
                        subcampaign_id = x.subcampaign_id
                    }).ToList();
            }
            return _lstData;
        }
        //private DataTable GetXmlData(string pXmlConfig, IDataReader pXmlData, ref string pXmlConfigData)
        private DataTable GetXmlData(string pXmlConfig, Dictionary<string, string> pXmlData, ref string pXmlConfigData)
        {
            if (string.IsNullOrEmpty(pXmlConfig))
                return null;

            try {
                /**
                 * combine the extracted xml data into one element holder.
                 */
                XElement _xelem = XElement.Parse(pXmlConfig);
                if (!string.IsNullOrEmpty(pXmlData["dialogdata"]))
                    _xelem.Add(XElement.Parse(pXmlData["dialogdata"]));
                if (!string.IsNullOrEmpty(pXmlData["scheduledata"]))
                    _xelem.Add(XElement.Parse(pXmlData["scheduledata"]));
                if (!string.IsNullOrEmpty(pXmlData["relationdata"]))
                    _xelem.Add(XElement.Parse(pXmlData["relationdata"]));
                if (!string.IsNullOrEmpty(pXmlData["accountdata"]))
                    _xelem.Add(XElement.Parse(pXmlData["accountdata"]));
                if (!string.IsNullOrEmpty(pXmlData["contactdata"]))
                    _xelem.Add(XElement.Parse(pXmlData["contactdata"]));

                /**
                 * load the xslt function from BrightVision.DQControl 
                 */
                string _XmlSource = _xelem.ToString();
                pXmlConfigData = _XmlSource;

                DQXsltFunction _XsltFunction = new DQXsltFunction();
                XsltExtensionParam xsltExtParam = new XsltExtensionParam {
                    Namespace = "util:xsltextension",
                    Object = _XsltFunction
                };

                /**
                 * here, we process the output by using the resource xslt file,
                 * and processing it using the xslt function.
                 */
                string _XslFileResource = BrightVision.Common.Resources.show_view;
                string _XmlData = XsltUtility.GetXmlString (
                    _XslFileResource,
                    _XmlSource,
                    new List<XsltExtensionParam> { 
                        xsltExtParam 
                    },
                    true,
                    true
                );

                /**
                 * we return the dataset processed out of the xml data.
                 */
                DataSet _dsData = new DataSet();
                _dsData.ReadXml(new System.IO.StringReader(_XmlData));
                return _dsData.Tables[0];
            }
            catch (Exception e) {
                if (m_IsWebPortalCall)
                    throw e;                    

                NotificationDialog.Error(e.Source, e.Message);
                return null;
            }
        }
        private void GenerateReportPage(XtraTabPage pTabPage)
        {
            /**
             * if from send email, we will need to use the private var -> m_TabPage.
             * so we can be able to call the public methods preview report and create pdf via the send email.
             * we create the pool handle on the tab page itself.
             * 
             * Important Note: we dont need threading when send email since we only load one tab.
             * 
             * will be utilized also when called from web portal.
             */
            #region Code Logic
            if (CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail || m_IsWebPortalCall) {
                //m_TabPage = pTabPage.Tag as ReportPage;
                m_TabPage = pTabPage as ReportPage;
                if (m_TabPage != null) {
                    try {
                        view_configuration _eftConfig = null;
                        using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(m_DatabaseConnection)) {
                            _eftConfig = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_TabPage.ConfigData.id);
                            _efDbContext.Detach(_eftConfig);

                            int _CampaignId = (int)_efDbContext.subcampaigns.FirstOrDefault(i => i.id == _eftConfig.subcampaign_id).campaign_id;
                            int _CustomerId = (int)_efDbContext.campaigns.FirstOrDefault(i => i.id == _CampaignId).customers.id;
                            m_TabPage.CustomerId = _CustomerId;
                        }

                        m_TabPage.VerifyPoolHandler();
                        if (string.IsNullOrEmpty(_eftConfig.xml_config) || _eftConfig.xml_config.Equals("<view />"))
                            return;

                        /**
                         * [jeff 05.14.2012]: https://brightvision.jira.com/browse/PLATFORM-1381
                         * add second parameter to determine the selected display mode of the export view.
                         * if no 2nd parameter defined, default will be loaded.
                         */
                        m_TabPage.ViewType = DisplayMode;
                        Dictionary<string, string> _drConfigData = new Dictionary<string,string>();
                        if (DisplayMode == eDisplayMode.AccountsContacts_WithDialogData)
                            _drConfigData = DatabaseUtility.GetViews(m_TabPage.ConfigData.id, ExportView.eExportViewDisplayMode.Default, m_DatabaseConnection, AccountId);

                        else if (DisplayMode == eDisplayMode.AccountsContacts_WithCallAttempts)
                            _drConfigData = DatabaseUtility.GetViews(m_TabPage.ConfigData.id, ExportView.eExportViewDisplayMode.AccountsAndContactsHavingSubCampaignCallAttemps, m_DatabaseConnection, AccountId);

                        if (_drConfigData == null)
                            throw new Exception("No data was found.");

                        string _XmlData = string.Empty;
                        var datasource = this.GetXmlData(_eftConfig.xml_config, _drConfigData, ref _XmlData);
                        System.Threading.Thread.Sleep(1500);

                        if (m_TabPage != null) {
                            m_TabPage.ViewType = DisplayMode; 
                            m_TabPage.DataSource = datasource;
                            m_TabPage.XmlConfig = _eftConfig.xml_config;
                            m_TabPage.XmlConfigData = _XmlData;

                            if (m_IsWebPortalCall) {
                                if (!string.IsNullOrEmpty(m_GridFilterString))
                                    m_TabPage.GridFilterString = m_GridFilterString;
                                if (!string.IsNullOrEmpty(m_GridSortInfo))
                                    m_TabPage.GridSortInfo = m_GridSortInfo;
                                if (!string.IsNullOrEmpty(m_GridColumnsInfo))
                                    m_TabPage.GridColumnsInfo = m_GridColumnsInfo;
                            }
                        }

                        if (datasource != null && datasource.Rows.Count > 0) {
                            m_TabPage.ViewConfigName = _eftConfig.name;
                            m_TabPage.ViewConfigId = _eftConfig.id;
                            m_TabPage.SubCampaignId = _eftConfig.subcampaign_id;
                            m_TabPage.CampaignInfo = CampaignInfo;
                        }
                    }
                    catch (Exception e) {
                        if (m_IsWebPortalCall)
                            throw e;

                        NotificationDialog.Error("Reports", string.Format("{0}{1}{2}", e.Message, Environment.NewLine, e.InnerException.Message));
                    }

                    if (OnReportPageCompleted != null)
                        OnReportPageCompleted(this, new EventArgs());
                }
            }
            #endregion

            /**
             * if not send email, just use method level report page var, so we can be able to display
             * multiple tab pages on a tab control calling this module.
             * we create the pool handle right on this method.
             */
            #region Code Logic
            else {
                ReportPage _TabPage = pTabPage.Tag as ReportPage;
                if (_TabPage != null) {
                    Action _action = delegate() {
                        try {
                            view_configuration _eftConfig = null;
                            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(m_DatabaseConnection)) {
                                _eftConfig = _efDbContext.view_configuration.FirstOrDefault(i => i.id == _TabPage.ConfigData.id);
                                _efDbContext.Detach(_eftConfig);

                                int _CampaignId = (int)_efDbContext.subcampaigns.FirstOrDefault(i => i.id == _eftConfig.subcampaign_id).campaign_id;
                                int _CustomerId = (int)_efDbContext.campaigns.FirstOrDefault(i => i.id == _CampaignId).customers.id;
                                _TabPage.CustomerId = _CustomerId;
                            }

                            if (!IsHandleCreated)
                                this.CreateHandle();

                            _TabPage.Invoke(new MethodInvoker(delegate {
                                _TabPage.SetButtonState(false);
                                _TabPage.InitRecordCount();
                            }));

                            if (string.IsNullOrEmpty(_eftConfig.xml_config) || _eftConfig.xml_config.Equals("<view />")) {
                                _TabPage.Invoke(new MethodInvoker(delegate {
                                    _TabPage.HideLoadingInfo();
                                }));
                                return;
                            }

                            /**
                                * [jeff 05.14.2012]: https://brightvision.jira.com/browse/PLATFORM-1381
                                * add second parameter to determine the selected display mode of the export view.
                                * if no 2nd parameter defined, default will be loaded.
                                */
                            _TabPage.ViewType = DisplayMode;
                            Dictionary<string, string> _drConfigData = new Dictionary<string, string>();
                            if (DisplayMode == eDisplayMode.AccountsContacts_WithDialogData)
                                _drConfigData = DatabaseUtility.GetViews(_TabPage.ConfigData.id, ExportView.eExportViewDisplayMode.Default);

                            else if (DisplayMode == eDisplayMode.AccountsContacts_WithCallAttempts)
                                _drConfigData = DatabaseUtility.GetViews(_TabPage.ConfigData.id, ExportView.eExportViewDisplayMode.AccountsAndContactsHavingSubCampaignCallAttemps);

                            if (_drConfigData == null)
                                throw new Exception("No data was found.");

                            string _XmlData = string.Empty;
                            var datasource = this.GetXmlData(_eftConfig.xml_config, _drConfigData, ref _XmlData);
                            System.Threading.Thread.Sleep(1500);

                            if (_TabPage != null) {
                                _TabPage.ViewType = DisplayMode;
                                _TabPage.DataSource = datasource;
                                _TabPage.XmlConfig = _eftConfig.xml_config;
                                _TabPage.XmlConfigData = _XmlData;

                                if (m_IsWebPortalCall) {
                                    if (!string.IsNullOrEmpty(m_GridFilterString))
                                        _TabPage.GridFilterString = m_GridFilterString;
                                    if (!string.IsNullOrEmpty(m_GridSortInfo))
                                        _TabPage.GridSortInfo = m_GridSortInfo;
                                    if (!string.IsNullOrEmpty(m_GridColumnsInfo))
                                        _TabPage.GridColumnsInfo = m_GridColumnsInfo;
                                }
                            }

                            if (datasource != null && datasource.Rows.Count > 0) {
                                _TabPage.ViewConfigName = _eftConfig.name;
                                _TabPage.ViewConfigId = _eftConfig.id;
                                _TabPage.SubCampaignId = _eftConfig.subcampaign_id;

                                _TabPage.Invoke(new MethodInvoker(delegate {
                                    _TabPage.SetButtonState(true);
                                    _TabPage.SetRecordCount(datasource.Rows.Count);
                                }));
                            }
                        }
                        catch (Exception e) {
                            _TabPage.Invoke(new MethodInvoker(delegate {
                                NotificationDialog.Error("Reports", string.Format("{0}{1}", e.Message, Environment.NewLine));
                            }));
                        }
                    };

                    bw = new BackgroundWorker();
                    bw.DoWork += (sender, e) => { _action(); };
                    bw.RunWorkerCompleted += (sender, e) => { _TabPage.ImageIndex = -1; };
                    bw.RunWorkerAsync();
                }
            }
            #endregion
        }
        private void StartPoolingReportPages(ref XtraTabControl pTabControl)
        {
            XtraTabPage _TabPage = null;
            ReportPagePoolDelegate _dlg = new ReportPagePoolDelegate(CreateReportReportPagePoolThread);
            for (int x = 0; x < pTabControl.TabPages.Count; ++x) {
                _TabPage = pTabControl.TabPages[x];
                _dlg.BeginInvoke(_TabPage, null, null);
                System.Threading.Thread.Sleep(750);
            }
        }
        private void CreateReportReportPagePoolThread(XtraTabPage pTabPage)
        {
            if (pTabPage.InvokeRequired)
                pTabPage.BeginInvoke(new ReportPagePoolDelegate(GenerateReportPage), new object[] { pTabPage });
            else
                this.GenerateReportPage(pTabPage);
        }
        #endregion

        #region Control Events
        #endregion

        /**
         * view tab implementation.
         */
        public class ReportPage : XtraTabPage
        {
            #region Constructors
            public ReportPage(): base()
            {
            }
            public ReportPage(ref GridControl pGridControl, ref GridView pGridView, string pGroupName, eCallingEnvironment pCallingEnvironment, bool pIsWebPortalCall = false): base() 
            {
                m_CallingEnvironment = pCallingEnvironment;
                m_IsWebPortalCall = pIsWebPortalCall;

                if (m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail || m_IsWebPortalCall) {
                    this.gridControl1 = pGridControl as GridControl;
                    this.gridView1 = pGridView as GridView;
                    this.gridControl1.MainView = this.gridView1;
                    this.gridControl1.ViewCollection.AddRange(new BaseView[] { this.gridView1 });
                    this.gridView1.GridControl = this.gridControl1;
                    this.gridView1.OptionsFind.AlwaysVisible = false;
                    this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
                    this.gridView1.OptionsBehavior.AutoPopulateColumns = true;
                    this.gridView1.OptionsSelection.MultiSelect = false;
                    this.gridView1.OptionsView.ShowGroupPanel = false;
                    this.gridView1.OptionsView.ColumnAutoWidth = false;
                    this.gridView1.OptionsBehavior.Editable = false;
                }
                else 
                    this.Initialize();

                this.Text = pGroupName;
                this.ImageIndex = 0;
            }
            #endregion

            #region Public Properties
            public eDisplayMode ViewType {
                set { 
                    m_ViewType = value; 
                }
            }
            public eCallingApplication CallingApplication {
                set { m_CallingApplication = value; }
            }

            public string XmlConfig { get; set; } // the view config xml structure only.
            public string XmlConfigData { get; set; } // xml config with data in it.

            public ViewCofigData ConfigData { get; set; }
            public DataTable DataSource {
                get {
                    return m_dtData;
                }
                set {
                    m_dtData = value;
                    if (m_IsWebPortalCall)
                        this.FillGrid();

                    else {
                        Action _action = delegate {
                            this.FillGrid();
                        };

                        if (gridControl1.InvokeRequired)
                            gridControl1.BeginInvoke(_action);
                        else
                            _action();
                    }
                }
            }
            public string GridFilterString {
                set { 
                    m_GridFilerString = value;
                    this.SetGridFilter();
                }
            }
            public string GridSortInfo {
                set {
                    m_GridSortInfo = value;
                    this.SetGridSortInfo();
                }
            }
            public string GridColumnsInfo {
                set
                {
                    m_GridColumnsInfo = value;
                    this.SetGridColumnsInfo();
                }
            }
            
            public string ViewConfigName
            {
                set { m_ViewConfigName = value; }
            }
            public int ViewConfigId
            {
                set { m_ViewConfigId = value; }
            }
            public int SubCampaignId
            {
                set { m_SubCampaignId = value; }
            }
            public int CustomerId
            {
                set { m_CustomerId = value; }
            }
            public bool IsEmpty {
                get {
                    return m_IsEmpty;
                }
            }
            public string CampaignInfo 
            {
                set { m_CampaignInfo = value; }
            }
            public int[] SubCampaignIds
            {
                set { m_SubcampaignIds = value; }
            }

            public string DatabaseConnection 
            {
                set { m_DatabaseConnection = value; }
            }
            public bool IsWebPortalCall
            {
                set { m_IsWebPortalCall = value; }
            }
            public string WebPortalRequester
            {
                set { m_WebPortalRequester = value; }
            }
            public string ReportsPath
            {
                set { m_ReportsPath = value; }
            }
            #endregion

            #region Private Properties
            private eCallingEnvironment m_CallingEnvironment = eCallingEnvironment.None;
            private eCallingApplication m_CallingApplication = eCallingApplication.None;
            private eDisplayMode m_ViewType;

            private DataTable m_dtData = null;
            private view_configuration m_eftConfigData = null;
            private customer m_eftCustomer = null;
            private campaign m_eftCampaign = null;
            private subcampaign m_eftSubCampaign = null;
            private sub_campaign_account_lists m_eftSubCampaignAccountList = null;
            private final_lists m_eftFinalList = null;

            private ReportDataSet m_ReportPageDataSet = null;
            private TemplateProperty m_ReportPageTemplateProperty = null;

            private LayoutControl layoutControl;
            private LayoutControlGroup layoutControlGroup1;
            private LayoutControlItem layoutControlItem1;
            private LayoutControlItem layoutControlItem4;
            private LayoutControlItem layoutControlItem5;
            private LayoutControlItem layoutControlItem7;
            private EmptySpaceItem emptySpaceItem1;
            private EmptySpaceItem emptySpaceItemLoading;
            private GridView gridView1;
            private GridControl gridControl1;
            private LabelControl labelControl1;
            private SimpleButton btnShowReport;
            private SimpleButton btnSaveReportPerPdf;

            private int m_AccountId = 0;
            private int m_ViewConfigId = 0;
            private int m_SubCampaignId = 0;
            private int m_CustomerId = 0;
            private int[] m_SubcampaignIds;

            private bool m_IsWebPortalCall = false;
            private bool m_IsEmpty = false;

            private string m_ViewConfigName = string.Empty;
            private string m_CampaignInfo = string.Empty;
            private string m_DatabaseConnection = string.Empty;
            private string m_WebPortalRequester = string.Empty;
            private string m_ReportsPath = string.Empty;
            private string m_GridFilerString = string.Empty;
            private string m_GridSortInfo = string.Empty;
            private string m_GridColumnsInfo = string.Empty;

            private List<string> m_lstPdfFiles = null;
            private List<string> m_lstHiddenColumnsInfo = new List<string>();
            #endregion

            #region Public Methods
            public void HideLoadingInfo()
            {
                this.layoutControlGroup1.Remove(emptySpaceItemLoading);
            }
            public void Export(ViewExportType exportType)
            {
                SaveFileDialog dialog1 = new SaveFileDialog();
                if (exportType == ViewExportType.Excel2003)
                    dialog1.Filter = "Excel Workbook (*.xls)|*.xls";
                else if (exportType == ViewExportType.Excel2007)
                    dialog1.Filter = "Excel Workbook (*.xslx)|*.xlsx";
                else if (exportType == ViewExportType.CSV)
                    dialog1.Filter = "CSV (Comma Delimited) (*.csv)|*.csv";

                dialog1.Title = "Save As";
                dialog1.CheckPathExists = true;
                dialog1.CheckFileExists = false;
                if (dialog1.ShowDialog() == DialogResult.OK) {
                    if (dialog1.FileName != "") {
                        if (dialog1.FilterIndex == 1) {
                            gridView1.OptionsPrint.AutoWidth = false;
                            gridView1.BestFitColumns();

                            FileStream fs = (FileStream)dialog1.OpenFile();
                            if (exportType == ViewExportType.CSV) {
                                DevExpress.XtraPrinting.CsvExportOptions opts = new DevExpress.XtraPrinting.CsvExportOptions();
                                gridView1.Export(DevExpress.XtraPrinting.ExportTarget.Csv, fs, opts);
                            }
                            else if (exportType == ViewExportType.Excel2007) {
                                DevExpress.XtraPrinting.XlsxExportOptions opts = new DevExpress.XtraPrinting.XlsxExportOptions();
                                opts.ExportMode = DevExpress.XtraPrinting.XlsxExportMode.SingleFile;
                                opts.SheetName = "Sheet1";
                                gridControl1.ExportToXlsx(fs, opts);
                            }
                            else if (exportType == ViewExportType.Excel2003) {
                                DevExpress.XtraPrinting.XlsExportOptions opts = new DevExpress.XtraPrinting.XlsExportOptions();
                                opts.ExportMode = DevExpress.XtraPrinting.XlsExportMode.SingleFile;
                                opts.SheetName = "Sheet1";
                                gridControl1.ExportToXls(fs, opts);
                            }
                            fs.Close();
                        }
                    }
                }
            }
            public void SetButtonState(bool state)
            {
                this.btnShowReport.Enabled = state;
                this.btnSaveReportPerPdf.Enabled = state;
            }
            public void SetRecordCount(int pRecordCount)
            {
                this.labelControl1.Text = "Records: " + pRecordCount.ToString();
            }
            public void InitRecordCount()
            {
                this.labelControl1.Text = "Records: 0";
            }
            public void ReportPagePreview(int pAccountId)
            {
                m_AccountId = pAccountId;
                this.ReportPagePreview();
            }
            public string GenerateReports(int pAccountId)
            {
                m_AccountId = pAccountId;
                return this.GenerateReports();
            }
            public void VerifyPoolHandler()
            {
                if (!this.IsHandleCreated)
                    this.CreateHandle();
            }
            public bool AccountDataExists(int pAccountId)
            {
                DataRow[] _Rows = m_dtData.Select(string.Format("accountid = '{0}'", pAccountId));
                if (_Rows.Count() < 1)
                    return false;

                return true;
            }
            public List<string> CreatePdfPerAccount()
            {
                this.GenerateReports();
                return m_lstPdfFiles;
            }
            #endregion

            #region Private Methods
            private void Initialize()
            {
                /**
                 * if ui is needed, we will just leave this method.
                 * we will just use the grid control default of this module (gcReport)
                 */
                if (m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail || m_IsWebPortalCall)
                    return;

                this.layoutControl = new LayoutControl();
                this.layoutControl.Name = "layoutControl" + Guid.NewGuid().ToString();
                this.layoutControl.Dock = DockStyle.Fill;
                this.Controls.Add(layoutControl);

                this.layoutControlGroup1 = new LayoutControlGroup();
                this.layoutControlGroup1.Name = "layoutControlGroup" + Guid.NewGuid().ToString();
                this.layoutControlGroup1.Text = this.Text;
                this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
                this.layoutControlGroup1.ShowInCustomizationForm = false;

                this.emptySpaceItemLoading = new EmptySpaceItem();
                this.emptySpaceItemLoading.Text = "Loading view display. Please wait...";
                this.emptySpaceItemLoading.AppearanceItemCaption.Font = new Font("Arial", 10f, FontStyle.Bold);
                this.emptySpaceItemLoading.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                this.emptySpaceItemLoading.TextVisible = true;
                this.emptySpaceItemLoading.Size = new Size(100, 20);
                this.emptySpaceItemLoading.MaxSize = new Size(100, 20);
                this.emptySpaceItemLoading.MinSize = new Size(100, 20);
                this.emptySpaceItemLoading.SizeConstraintsType = SizeConstraintsType.Custom;
                this.layoutControlGroup1.AddItem(emptySpaceItemLoading);

                /**
                 * grid control and grid view.
                 */
                this.gridControl1 = new GridControl();
                this.gridView1 = new GridView();

                this.gridControl1.Name = "gridControl" + Guid.NewGuid().ToString();
                this.gridControl1.MainView = this.gridView1;
                this.gridControl1.ViewCollection.AddRange(new BaseView[] { this.gridView1 });

                this.gridView1.Name = "gridView" + Guid.NewGuid().ToString();
                this.gridView1.GridControl = this.gridControl1;
                this.gridView1.OptionsFind.AlwaysVisible = false;
                this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
                this.gridView1.OptionsBehavior.AutoPopulateColumns = true;
                this.gridView1.OptionsSelection.MultiSelect = false;
                this.gridView1.OptionsView.ShowGroupPanel = false;
                this.gridView1.OptionsView.ColumnAutoWidth = false;
                this.gridView1.OptionsBehavior.Editable = false;
                this.gridView1.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(gridView1_PopupMenuShowing);
                this.gridView1.ColumnFilterChanged += new EventHandler(gridView1_ColumnFilterChanged);
                this.gridView1.DragObjectDrop += gridView1_DragObjectDrop;

                this.labelControl1 = new LabelControl();
                this.labelControl1.Name = "labelControl" + Guid.NewGuid().ToString();
                this.labelControl1.Text = "Records: 0";
                this.labelControl1.Size = new System.Drawing.Size(120, 22);

                this.layoutControl.BeginUpdate();
                this.layoutControlItem1 = new LayoutControlItem();
                this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.layoutControlItem1.Control = gridControl1;
                this.layoutControlItem1.TextVisible = false;
                this.layoutControlGroup1.AddItem(this.layoutControlItem1);

                this.emptySpaceItem1 = new EmptySpaceItem();
                this.layoutControlGroup1.AddItem(emptySpaceItem1);

                /**
                 * [@jeff 06.18.2012]: https://brightvision.jira.com/browse/PLATFORM-1462
                 * added label to display the current grid record count.
                 */
                this.layoutControlItem5 = new LayoutControlItem();
                this.layoutControlItem5.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.layoutControlItem5.Control = labelControl1;
                this.layoutControlItem5.SizeConstraintsType = SizeConstraintsType.Custom;
                this.layoutControlItem5.MaxSize = new Size(130, 30);
                this.layoutControlItem5.MinSize = new Size(80, 24);
                this.layoutControlItem5.Size = new Size(this.labelControl1.Width + 8, this.labelControl1.Height + 8);
                this.layoutControlItem5.TextVisible = false;
                this.layoutControlItem5.ShowInCustomizationForm = false;
                this.layoutControlItem5.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 3, 3, 5);
                this.layoutControlGroup1.AddItem(layoutControlItem5, emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Left);

                /**
                 * show report.
                 */
                this.btnShowReport = new SimpleButton();
                this.btnShowReport.Name = "simpleButton" + Guid.NewGuid().ToString();
                this.btnShowReport.Text = "Show Report";
                this.btnShowReport.Size = new System.Drawing.Size(90, 22);
                this.btnShowReport.Enabled = false;
                this.btnShowReport.Click += new EventHandler(btnShowReport_Click);

                this.layoutControlItem4 = new LayoutControlItem();
                this.layoutControlItem4.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.layoutControlItem4.Control = btnShowReport;
                this.layoutControlItem4.SizeConstraintsType = SizeConstraintsType.Custom;
                this.layoutControlItem4.MaxSize = new Size(100, 30);
                this.layoutControlItem4.MinSize = new Size(80, 24);
                this.layoutControlItem4.Size = new Size(this.btnShowReport.Width + 8, this.btnShowReport.Height + 8);
                this.layoutControlItem4.TextVisible = false;
                this.layoutControlItem4.ShowInCustomizationForm = false;
                this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 3, 3, 5);
                this.layoutControlGroup1.AddItem(layoutControlItem4, emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Left);

                /**
                 * Save As Account Per PDF.
                 */
                this.btnSaveReportPerPdf = new SimpleButton();
                this.btnSaveReportPerPdf.Name = "simpleButton" + Guid.NewGuid().ToString();
                this.btnSaveReportPerPdf.Text = "Save Account Per PDF";
                this.btnSaveReportPerPdf.Size = new System.Drawing.Size(170, 22);
                this.btnSaveReportPerPdf.Enabled = false;
                this.btnSaveReportPerPdf.Click += new EventHandler(btnSaveReportPerPdf_Click);

                this.layoutControlItem7 = new LayoutControlItem();
                this.layoutControlItem7.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.layoutControlItem7.Control = btnSaveReportPerPdf;
                this.layoutControlItem7.SizeConstraintsType = SizeConstraintsType.Custom;
                this.layoutControlItem7.MaxSize = new Size(180, 30);
                this.layoutControlItem7.MinSize = new Size(80, 24);
                this.layoutControlItem7.Size = new Size(this.btnSaveReportPerPdf.Width + 8, this.btnSaveReportPerPdf.Height + 8);
                this.layoutControlItem7.TextVisible = false;
                this.layoutControlItem7.ShowInCustomizationForm = false;
                this.layoutControlItem7.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 3, 3, 5);
                this.layoutControlGroup1.AddItem(layoutControlItem7, emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Left);

                /**
                 * hide (Save As Account Per PDF) button
                 * if not called by bright manager application.
                 */
                //if (m_CallingEnvironment != eCallingEnvironment.BrightManager_ViewDisplay)
                //    layoutControlItem7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;

                this.layoutControl.Root = layoutControlGroup1;
                this.layoutControl.Root.GroupBordersVisible = false;
                this.layoutControl.EndUpdate();
                this.Tag = this;
            }
            private ReportDataSet GetReportDataSet(TemplateProperty pTemplateData)
            {
                ReportDataSet _drData = new ReportDataSet();
                DataTable _dtFilterdData = new DataTable();

                _dtFilterdData = this.GetFilteredData();

                /**
                 * This process will set the account and contact table
                 */
                if (m_ViewType == eDisplayMode.AccountsContacts_WithDialogData)
                    _drData = ReportDataSet.GetReportDataset(m_ViewConfigId, m_CustomerId, ReportDataSet.eViewType.AccountsContactsWithDialogData, m_AccountId, m_DatabaseConnection);
                else if (m_ViewType == eDisplayMode.AccountsContacts_WithCallAttempts)
                    _drData = ReportDataSet.GetReportDataset(m_ViewConfigId, m_CustomerId, ReportDataSet.eViewType.AccountsContactsWithCallAttempts, m_AccountId, m_DatabaseConnection);

                /**
                 * this will remove the account or contact which is not present in the display view grid
                 */
                _drData = this.FilterByGridData(_drData);

                PopulateClientInfo(ref _drData);
                PopulateAccountDynamic(ref _drData, _dtFilterdData, pTemplateData);
                PopulateContactDynamic(ref _drData, _dtFilterdData, pTemplateData);
                PopulateAccountStatic(ref _drData, _dtFilterdData);
                PopulateContactStatic(ref _drData, _dtFilterdData);

                /**
                 * remove derived contacts.
                 * -> temporary contacts added for viewing records.
                 * -> contacts with empty first name and last name.
                 */
                //for (int i = 0; i < _drData.Tables["contact"].Rows.Count; i++) {
                //    if (string.IsNullOrEmpty(_drData.Tables["contact"].Rows[i]["first_name"].ToString()) &&
                //        string.IsNullOrEmpty(_drData.Tables["contact"].Rows[i]["last_name"].ToString()))
                //        _drData.Tables["contact"].Rows[i].Delete();
                //}

                return _drData;
            }
            private DataTable GetFilteredData()
            {
                DataTable table = new DataTable();
                foreach (GridColumn item in this.gridView1.Columns)
                    if (item.Visible || (item.FieldName == "accountid" || item.FieldName == "contactid"))
                        table.Columns.Add(item.FieldName);

                for (int cnt = 0; cnt < this.gridView1.DataRowCount; cnt++) {
                    var datarow = (this.gridView1.GetRow(cnt) as DataRowView).Row;
                    if (Convert.ToInt32(datarow["accountid"]) != m_AccountId && m_AccountId > 0)
                        continue;

                    DataRow newrow = table.NewRow();
                    foreach (GridColumn item in this.gridView1.Columns)
                        if (item.Visible || (item.FieldName == "accountid" || item.FieldName == "contactid"))
                            newrow[item.FieldName] = datarow[item.FieldName];

                    table.Rows.Add(newrow);
                }

                //if (m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail || m_IsWebPortalCall) {
                //    for (int cnt = 0; cnt < m_dtData.Rows.Count; cnt++) {
                //        var datarow = m_dtData.Rows[cnt];
                //        if (Convert.ToInt32(datarow["accountid"]) != m_AccountId && m_AccountId > 0)
                //            continue;

                //        DataRow newrow = table.NewRow();
                //        foreach (GridColumn item in this.gridView1.Columns)
                //            if (item.Visible || (item.FieldName == "accountid" || item.FieldName == "contactid"))
                //                newrow[item.FieldName] = datarow[item.FieldName];

                //        table.Rows.Add(newrow);
                //    }
                //}

                //else {
                //    for (int cnt = 0; cnt < this.gridView1.DataRowCount; cnt++) {
                //        var datarow = (this.gridView1.GetRow(cnt) as DataRowView).Row;
                //        DataRow newrow = table.NewRow();
                //        foreach (GridColumn item in this.gridView1.Columns)
                //            if (item.Visible || (item.FieldName == "accountid" || item.FieldName == "contactid"))
                //                newrow[item.FieldName] = datarow[item.FieldName];

                //        table.Rows.Add(newrow);
                //    }
                //}

                return table;
            }
            private ReportDataSet FilterByGridData(ReportDataSet reportDataset)
            {
                int rowCount = this.gridView1.DataRowCount;
                string accountFilter = string.Empty;
                string contactFilter = string.Empty;
                List<int> listAccount = new List<int>();
                List<int> listContact = new List<int>();

                #region get the filter of account and contact
                for (int cnt = 0; cnt < rowCount; cnt++) {
                    var rowview = this.gridView1.GetRow(cnt) as DataRowView;
                    int accountId = int.Parse(rowview.Row["accountid"].ToString());
                    int contactid = int.Parse(rowview.Row["contactid"].ToString());
                    if (listAccount.Where(param => param == accountId).Count() == 0)
                        listAccount.Add(accountId);
                    if (listContact.Where(param => param == contactid).Count() == 0)
                        listContact.Add(contactid);
                }

                foreach (var ac in listAccount)
                    accountFilter += ac + ",";

                foreach (var c in listContact)
                    contactFilter += c + ",";
                #endregion

                if (accountFilter != string.Empty) {
                    accountFilter = accountFilter.Substring(0, accountFilter.Length - 1);
                    var toDeleteAccount = reportDataset.account.Select(String.Format("account_id NOT IN ({0})", accountFilter));
                    foreach (var tmp in toDeleteAccount)
                        tmp.Delete();
                }
                if (contactFilter != string.Empty) {
                    contactFilter = contactFilter.Substring(0, contactFilter.Length - 1);
                    var toDeleteContact = reportDataset.contact.Select(String.Format("contact_id NOT IN ({0})", contactFilter));
                    foreach (var tmp in toDeleteContact)
                        tmp.Delete();
                }

                return reportDataset;
            }
            private void PopulateClientInfo(ref ReportDataSet reportDataset)
            {
                string customerName = string.Empty;
                string campaignName = string.Empty;
                string efeSubCampaignName = string.Empty;
                string _efeSubCampaignListname = string.Empty;

                if (m_AccountId < 1) {
                    XElement xmlConfigWithData = XElement.Parse(XmlConfigData);

                    /**
                     * [@jeff 06.26.2012]: https://brightvision.jira.com/browse/PLATFORM-1527
                     * added validation for null objects.
                     */

                    if (xmlConfigWithData.XPathSelectElement("relation/customer") != null &&
                        xmlConfigWithData.XPathSelectElement("relation/customer").Attribute("name").Value != null)
                        customerName = xmlConfigWithData.XPathSelectElement("relation/customer").Attribute("name").Value;

                    if (xmlConfigWithData.XPathSelectElement("relation/campaign") != null &&
                        xmlConfigWithData.XPathSelectElement("relation/campaign").Attribute("name").Value != null)
                        campaignName = xmlConfigWithData.XPathSelectElement("relation/campaign").Attribute("name").Value;

                    if (xmlConfigWithData.XPathSelectElement("relation/subcampaign") != null &&
                        xmlConfigWithData.XPathSelectElement("relation/subcampaign").Attribute("name").Value != null)
                        efeSubCampaignName = xmlConfigWithData.XPathSelectElement("relation/subcampaign").Attribute("name").Value;

                    if (xmlConfigWithData.XPathSelectElement("relation/dialog") != null &&
                        xmlConfigWithData.XPathSelectElement("relation/dialog").Attribute("list_source_name").Value != null)
                        _efeSubCampaignListname = xmlConfigWithData.XPathSelectElement("relation/dialog").Attribute("list_source_name").Value;
                }
                else {
                    customerName = m_eftCustomer.customer_name;
                    campaignName = m_eftCampaign.campaign_name;
                    efeSubCampaignName = m_eftSubCampaign.title;
                    if (m_eftSubCampaignAccountList != null)
                        _efeSubCampaignListname = m_eftSubCampaignAccountList.list_source;
                }

                string _CurrentUser = string.Empty;
                if (m_IsWebPortalCall)
                    _CurrentUser = m_WebPortalRequester;
                else
                    _CurrentUser = UserSession.CurrentUser.UserFullName;

                reportDataset.clientinfo.Rows.Add(
                    customerName,
                    campaignName,
                    efeSubCampaignName,
                    _CurrentUser,
                    DateTime.Now.ToShortDateString(),
                    m_eftConfigData.name,
                    _efeSubCampaignListname
                );
            }
            private void PopulateContactStatic(ref ReportDataSet dataset, DataTable datasource)
            {
                //var config = XElement.Parse(XmlConfig);

                var config = XElement.Parse(m_eftConfigData.xml_config);
                for (int rowCount = 0; rowCount < datasource.Rows.Count; rowCount++) {
                    int accountid = int.Parse(datasource.Rows[rowCount]["accountid"].ToString());
                    if (m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail && (accountid != m_AccountId && m_AccountId > 0))
                        continue;

                    //string _ContactName = string.Empty;
                    //if (datasource.Columns.Contains("Contact Firstname") && datasource.Columns.Contains("Contact Lastname"))
                    //    _ContactName = string.Format("{0}{1}", datasource.Rows[rowCount]["Contact Firstname"].ToString(), datasource.Rows[rowCount]["Contact Lastname"].ToString());
                    //else if (datasource.Columns.Contains("Contact  Firstname") && datasource.Columns.Contains("Contact  Lastname"))
                    //    _ContactName = string.Format("{0}{1}", datasource.Rows[rowCount]["Contact  Firstname"].ToString(), datasource.Rows[rowCount]["Contact  Lastname"].ToString());

                    //if (string.IsNullOrEmpty(_ContactName))
                    //    continue;

                    for (int colCount = 0; colCount < datasource.Columns.Count; colCount++) {
                        string colName = datasource.Columns[colCount].ColumnName;
                        if (colName == "accountid" || colName == "contactid")
                            continue;

                        // get the source base on the column name
                        // the columnname is the display name in the xml_config
                        var filter = string.Format("item[display_name='{0}']", colName);
                        var item = config.XPathSelectElement(filter);
                        var xsource = item.XPathSelectElements("source").FirstOrDefault();
                        var xfieldName = item.XPathSelectElements("field_name").FirstOrDefault();
                        string source = string.Empty;
                        string fieldName = string.Empty;
                        
                        if (xsource != null && xfieldName != null) {
                            source = xsource.Value;
                            fieldName = xfieldName.Value;
                        }
                        else
                            continue;

                        // AccountMerge and Dialog Account Level source is added to accountstatic table
                        if (source == "General" && (fieldName == "DialogCreatedBy" ||
                            fieldName == "DialogCreatedDate" || fieldName == "DialogStatus" ||
                            fieldName == "ContactLastChanged" || fieldName == "ContactStatusLastChanged" ||
                            fieldName == "ContactSubCampaignCallAttempts")) {
                                int contactid = int.Parse(datasource.Rows[rowCount]["contactid"].ToString());
                                DataRow[] existColName = dataset.contactstatic.Select(string.Format("contactid={0} and name='{1}'", contactid, colName));
                                if (existColName.Count() < 1) {
                                    var newContactStatic = dataset.contactstatic.NewcontactstaticRow();
                                    newContactStatic.contactid = contactid;
                                    newContactStatic.name = colName;
                                    newContactStatic.value = datasource.Rows[rowCount][colCount].ToString();
                                    dataset.contactstatic.AddcontactstaticRow(newContactStatic);
                                }
                        }
                    }
                }
            }
            private void PopulateAccountStatic(ref ReportDataSet dataset, DataTable datasource)
            {
                var config = XElement.Parse(m_eftConfigData.xml_config);
                for (int rowCount = 0; rowCount < datasource.Rows.Count; rowCount++) {
                    int accountid = int.Parse(datasource.Rows[rowCount]["accountid"].ToString());
                    if (m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail && (accountid != m_AccountId && m_AccountId > 0))
                        continue;

                    for (int colCount = 0; colCount < datasource.Columns.Count; colCount++) {
                        string colName = datasource.Columns[colCount].ColumnName;
                        if (colName == "accountid" || colName == "contactid")
                            continue;

                        // get the source base on the column name
                        // the columnname is the display name in the xml_config
                        var filter = string.Format("item[display_name='{0}']", colName);
                        var item = config.XPathSelectElement(filter);
                        var xsource = item.XPathSelectElements("source").FirstOrDefault();
                        var xfieldName = item.XPathSelectElements("field_name").FirstOrDefault();

                        string source = string.Empty;
                        string fieldName = string.Empty;

                        if (xsource != null && xfieldName != null) {
                            source = xsource.Value;
                            fieldName = xfieldName.Value;
                        }
                        else
                            continue;

                        //AccountMerge and Dialog Account Level source is added to accountstatic table
                        if (source == "General" && (fieldName == "CompanyLeadStatus" ||
                            fieldName == "CompanyStatus" || fieldName == "CompanyLastChanged" ||
                            fieldName == "CompanyStatusLastChanged" || fieldName == "AccountSubCampaignCallAttempts")) {
                                DataRow[] existColName = dataset.accountstatic.Select(string.Format("accountid={0} and name='{1}'", accountid, colName));
                                if (existColName.Count() < 1) {
                                    var newAccountStatic = dataset.accountstatic.NewaccountstaticRow();
                                    newAccountStatic.accountid = accountid;
                                    newAccountStatic.name = colName;
                                    newAccountStatic.value = datasource.Rows[rowCount][colCount].ToString();
                                    dataset.accountstatic.AddaccountstaticRow(newAccountStatic);
                                }
                        }
                    }
                }
            }
            private void PopulateContactDynamic(ref ReportDataSet dataset, DataTable datasource, TemplateProperty templateData)
            {
                var config = XElement.Parse(m_eftConfigData.xml_config);
                for (int rowCount = 0; rowCount < datasource.Rows.Count; rowCount++) {
                    int accountid = int.Parse(datasource.Rows[rowCount]["accountid"].ToString());
                    if (m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail && (accountid != m_AccountId && m_AccountId > 0))
                        continue;

                    //string _ContactName = string.Empty;
                    //if (datasource.Columns.Contains("Contact Firstname") && datasource.Columns.Contains("Contact Lastname"))
                    //    _ContactName = string.Format("{0}{1}", datasource.Rows[rowCount]["Contact Firstname"].ToString(), datasource.Rows[rowCount]["Contact Lastname"].ToString());
                    //else if (datasource.Columns.Contains("Contact  Firstname") && datasource.Columns.Contains("Contact  Lastname"))
                    //    _ContactName = string.Format("{0}{1}", datasource.Rows[rowCount]["Contact  Firstname"].ToString(), datasource.Rows[rowCount]["Contact  Lastname"].ToString());
                    
                    //if (string.IsNullOrEmpty(_ContactName))
                    //    continue;

                    for (int colCount = 0; colCount < datasource.Columns.Count; colCount++) {
                        string colName = datasource.Columns[colCount].ColumnName;
                        if (colName == "accountid" || colName == "contactid")
                            continue;

                        if (colName.Contains("\r") || colName.Contains("\n"))
                            throw new Exception(string.Format("Column{0}contains a next line character.{1}Please kindly remove this on configuration.", colName, Environment.NewLine));

                        // get the source base on the column name
                        // the columnname is the display name in the xml_config
                        var filter = string.Format("item[display_name='{0}']", colName);
                        var item = config.XPathSelectElement(filter);
                        var xsource = item.XPathSelectElements("source").FirstOrDefault();

                        string source = string.Empty;
                        if (xsource != null)
                            source = xsource.Value;
                        else {
                            var lbl = item.XPathSelectElement("label_name");
                            if (lbl.Value == "EMPTY") continue;
                            var mergeitem = item.XPathSelectElements("merge_data").First();
                            var innerItem = XElement.Parse(mergeitem.Value);
                            var mergeContactItem = innerItem.XPathSelectElements("item[source='Contact' or source='Dialog Contact Level']");
                            if (mergeContactItem.Count() > 0)
                                source = "ContactMerge";
                        }

                        int contactid = int.Parse(datasource.Rows[rowCount]["contactid"].ToString()); ;
                        var datarowDynamic = dataset.contactdynamic.Select(string.Format("contactid={0} and name='{1}'", contactid, colName));

                        //AccountMerge and Dialog Account Level source is added to accountdynamic table
                        if (datarowDynamic.Count() == 0 && (source == "ContactMerge" || source == "Dialog Contact Level")) {
                            var newContactDynamic = dataset.contactdynamic.NewcontactdynamicRow();
                            newContactDynamic.contactid = int.Parse(datasource.Rows[rowCount]["contactid"].ToString());
                            newContactDynamic.name = colName;
                            newContactDynamic.value = datasource.Rows[rowCount][colCount].ToString();

                            if (templateData.IsEmptyDynamicValueVisible)
                                dataset.contactdynamic.AddcontactdynamicRow(newContactDynamic);
                            else if (!templateData.IsEmptyDynamicValueVisible && !string.IsNullOrWhiteSpace(newContactDynamic.value))
                                dataset.contactdynamic.AddcontactdynamicRow(newContactDynamic);
                        }
                    }
                }
            }
            private void PopulateAccountDynamic(ref ReportDataSet dataset, DataTable datasource, TemplateProperty templateData)
            {
                var config = XElement.Parse(m_eftConfigData.xml_config);
                for (int rowCount = 0; rowCount < datasource.Rows.Count; rowCount++) {
                    int accountid = int.Parse(datasource.Rows[rowCount]["accountid"].ToString());
                    if (m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail && (accountid != m_AccountId && m_AccountId > 0))
                        continue;

                    for (int colCount = 0; colCount < datasource.Columns.Count; colCount++) {
                        string colName = datasource.Columns[colCount].ColumnName;
                        if (colName == "accountid" || colName == "contactid")
                            continue;

                        // get the source base on the column name
                        // the columnname is the display name in the xml_config
                        var filter = string.Format("item[display_name='{0}']", colName);
                        var item = config.XPathSelectElement(filter);
                        var xsource = item.XPathSelectElements("source").FirstOrDefault();
                        
                        string source = string.Empty;
                        if (xsource != null)
                            source = xsource.Value;
                        else {
                            var lbl = item.XPathSelectElement("label_name");
                            if (lbl.Value == "EMPTY") continue;
                            var mergeitem = item.XPathSelectElements("merge_data").First();
                            var innerItem = XElement.Parse(mergeitem.Value);
                            var mergeContactItem = innerItem.XPathSelectElements("//item[source='Contact' or source='Dialog Contact Level']");
                            if (mergeContactItem.Count() == 0)
                                source = "AccountMerge";
                        }

                        var datarowDynamic = dataset.accountdynamic.Select(string.Format("accountid={0} and name='{1}'", accountid, colName));
                        
                        //AccountMerge and Dialog Account Level source is added to accountdynamic table
                        if (datarowDynamic.Count() == 0 && (source == "AccountMerge" || source == "Dialog Account Level")) {
                            var newAccountDynamic = dataset.accountdynamic.NewaccountdynamicRow();
                            newAccountDynamic.accountid = accountid;
                            newAccountDynamic.name = colName;
                            newAccountDynamic.value = datasource.Rows[rowCount][colCount].ToString();
                            if (templateData.IsEmptyDynamicValueVisible)
                                dataset.accountdynamic.AddaccountdynamicRow(newAccountDynamic);
                            else if (!templateData.IsEmptyDynamicValueVisible && !string.IsNullOrWhiteSpace(newAccountDynamic.value))
                                dataset.accountdynamic.AddaccountdynamicRow(newAccountDynamic);
                        }
                    }
                }
            }
            private string GetSortExpression(ColumnView view)
            {
                string fields = "";
                foreach (GridColumnSortInfo info in view.SortInfo) {
                    if (fields != "") fields += ";";
                        fields += string.Format("{0}", info.Column.FieldName);

                    if (info.SortOrder == DevExpress.Data.ColumnSortOrder.Descending)
                        fields += "|DESC";
                    else
                        fields += "|ASC";
                }

                return fields;
            }
            private string GetTableFieldName(string labelName)
            {
                //var config = XElement.Parse(XML_Config);

                ColumnView view = gridView1.SortedColumns.View;
                var config = XElement.Parse(XmlConfigData);
                var filter = string.Format("item[display_name='{0}']", labelName);
                var item = config.XPathSelectElement(filter);
                var xsource = item.XPathSelectElements("source").FirstOrDefault();
                var xfieldName = item.XPathSelectElements("field_name").FirstOrDefault();
                
                string source = string.Empty;
                string fieldName = string.Empty;

                if (xsource != null && xfieldName != null) {
                    source = xsource.Value;
                    fieldName = xfieldName.Value;

                    if (xsource.Value == "Account" && xfieldName.Value == "CompanyName") return "account|company_name";
                    else if (xsource.Value == "Account" && xfieldName.Value == "OrgNo") return "account|org_no";
                    else if (xsource.Value == "Account" && xfieldName.Value == "YearEstablished") return "account|year_established";
                    else if (xsource.Value == "Account" && xfieldName.Value == "ParentCompany") return "account|parent_company";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Website") return "account|www";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Telephone") return "account|telephone";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Telefax") return "account|telefax";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Box") return "account|box_address";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Street") return "account|street_address";
                    else if (xsource.Value == "Account" && xfieldName.Value == "ZipCode") return "account|zip_code";
                    else if (xsource.Value == "Account" && xfieldName.Value == "City") return "account|city";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Country") return "account|country";
                    else if (xsource.Value == "Account" && xfieldName.Value == "County") return "account|county";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Municipality") return "account|municipality";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Region") return "account|regions";
                    else if (xsource.Value == "Account" && xfieldName.Value == "ActivityCode") return "account|activity_code_1";
                    else if (xsource.Value == "Account" && xfieldName.Value == "ActivityCode2") return "account|activity_code_2";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Currency") return "account|currency";
                    else if (xsource.Value == "Account" && xfieldName.Value == "FiscalYear1") return "account|fiscal_1";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Turnover1") return "account|turnover_1";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Export1") return "account|export_1";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Result1") return "account|result_1";
                    else if (xsource.Value == "Account" && xfieldName.Value == "SalesAbroad1") return "account|sales_abroad_1";
                    else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesAboad1") return "account|employees_abroad_1";
                    else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesTotal1") return "account|employees_total_1";
                    else if (xsource.Value == "Account" && xfieldName.Value == "FiscalYear2") return "account|fiscal_2";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Turnover2") return "account|turnover_2";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Export2") return "account|export_2";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Result2") return "account|result_2";
                    else if (xsource.Value == "Account" && xfieldName.Value == "SalesAbroad2") return "account|sales_abroad_2";
                    else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesAboad2") return "account|employees_abroad_2";
                    else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesTotal2") return "account|employees_total_2";
                    else if (xsource.Value == "Account" && xfieldName.Value == "FiscalYear3") return "account|fiscal_3";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Turnover3") return "account|turnover_3";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Export3") return "account|export_3";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Result3") return "account|result_3";
                    else if (xsource.Value == "Account" && xfieldName.Value == "SalesAbroad3") return "account|sales_abroad_3";
                    else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesAboad3") return "account|employees_abroad_3";
                    else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesTotal3") return "account|employees_total_3";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Category") return "account|category";
                    else if (xsource.Value == "Account" && xfieldName.Value == "BVSource") return "account|bv_source";
                    else if (xsource.Value == "Account" && xfieldName.Value == "Priority") return "account|priority";
                    else if (xsource.Value == "Account" && xfieldName.Value == "AssignedTo") return "account|assigned_to";

                    else if (xsource.Value == "Contact" && xfieldName.Value == "Firstname") return "contact|first_name";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "Middlename") return "contact|middle_name";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "Lastname") return "contact|last_name";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "DirectPhone") return "contact|direct_phone";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "Mobile") return "contact|mobile";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "Email") return "contact|email";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "TitleNotConfirmed") return "contact|title_not_confirmed";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "Title") return "contact|title";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "Address 1") return "contact|address1";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "City") return "contact|city";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "ZipCode") return "contact|zip_code";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "Country") return "contact|country";
                    else if (xsource.Value == "Contact" && xfieldName.Value == "Priority") return "contact|priority";
                }
                
                return "";
            }
            private void SetGridFilter()
            {
                gridView1.ActiveFilterString = m_GridFilerString;
            }
            private void FillGrid()
            {
                gridControl1.DataSource = null;
                if (m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail || m_IsWebPortalCall) {
                    gridControl1.DataSource = m_dtData;
                    gridView1.Columns["accountid"].Visible = false;
                    gridView1.Columns["contactid"].Visible = false;
                    return;
                }

                GridColumn _column = null;
                gridControl1.DataSource = m_dtData.Clone();
                emptySpaceItemLoading.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                RepositoryItemMemoEdit _tbxColumn = new RepositoryItemMemoEdit() {
                    WordWrap = true
                };

                gridView1.GridControl.RepositoryItems.Add(_tbxColumn);
                if (gridView1.Columns.Count > 0) {
                    gridView1.OptionsView.RowAutoHeight = true;
                    gridView1.OptionsView.ColumnAutoWidth = false;
                    gridView1.OptionsSelection.MultiSelect = true;
                    gridView1.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;

                    for (int i = 0; i < gridView1.Columns.Count; i++) {
                        _column = gridView1.Columns[i];
                        _column.ColumnEdit = _tbxColumn;
                        _column.Width = 300;
                        _column.MaxWidth = 500;
                        _column.MinWidth = 100;
                        _column.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
                        _column.OptionsColumn.AllowEdit = false;
                        _column.OptionsColumn.AllowFocus = true;
                        _column.OptionsColumn.FixedWidth = false;

                        if (_column.FieldName == "accountid" || _column.FieldName == "contactid")
                            _column.Visible = false;
                    }
                }

                gridControl1.DataSource = m_dtData;
                gridView1.LeftCoord = 0;
            }
            private void ReportPagePreview()
            {
                try {
                    this.GetReportPageBaseData();

                    /**
                     * DAN: https://brightvision.jira.com/browse/PLATFORM-2457
                     * Implement to sort report when grid has been sorted out
                     */
                    string sortExpression = GetSortExpression(gridView1);
                    if (!string.IsNullOrEmpty(sortExpression)) {
                        string[] arrSortEx = sortExpression.Split('|');
                        string tableFieldName = GetTableFieldName(arrSortEx[0]);

                        if (!string.IsNullOrEmpty(tableFieldName)) {
                            string tableName = tableFieldName.Split('|')[0];
                            string fieldName = tableFieldName.Split('|')[1];
                            ReportDataSet reportDatasetTemp = new ReportDataSet() {
                                EnforceConstraints = false
                            };
                            /*var sortedRows = (from myRow in getReportDataSet.Tables["account"].AsEnumerable()
                                                orderby myRow["company_name"] descending
                                                select myRow).ToArray();*/
                            //DAN: Copy to DataTable first as it causes error during linq when original table has been filter/deleted rows.
                            DataTable dt = m_ReportPageDataSet.Tables[tableName].DefaultView.ToTable();
                            var sortedRows = (from myRow in dt.AsEnumerable()
                                                orderby myRow[fieldName] ascending
                                                select myRow).ToArray();

                            if (arrSortEx[1] == "DESC") {
                                sortedRows = (from myRow in dt.AsEnumerable()
                                                orderby myRow[fieldName] descending
                                                select myRow).ToArray();
                            }

                            DataTable sortedCompany = sortedRows.CopyToDataTable();
                            foreach (DataRow dr in sortedCompany.Rows)
                                reportDatasetTemp.Tables[tableName].Rows.Add(dr.ItemArray);

                            //reportDatasetTemp.Tables["account"].Rows.Add(dr.ItemArray);

                            for (int i = 1; i < m_ReportPageDataSet.Tables.Count; i++) {
                                dt.Clear();
                                dt.Dispose();
                                dt = null;
                                dt = new DataTable();
                                dt = m_ReportPageDataSet.Tables[i].DefaultView.ToTable();

                                sortedRows = (from myRow in dt.AsEnumerable()
                                                select myRow).ToArray();

                                if (sortedRows.Count() == 0)
                                    continue;

                                sortedCompany = sortedRows.CopyToDataTable();
                                foreach (DataRow dr in sortedCompany.Rows)
                                    reportDatasetTemp.Tables[i].Rows.Add(dr.ItemArray);
                            }
                            reportDatasetTemp.EnforceConstraints = true;

                            this.ShowReportPagePreview(reportDatasetTemp);
                            //m_ReportPageTemplateProperty.StatisticsDataSource = this.GetFilteredData();
                            //string template = m_eftConfigData.report_layout_config;
                            //if (string.IsNullOrEmpty(template))
                            //    template = BrightVision.Reporting.Business.FacadeReportTemplate.GetDefaultReportLayout();

                            //XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate(template, m_ReportPageTemplateProperty) { DataSource = reportDatasetTemp };
                            //_report.ShowPreview();
                        }
                        else {
                            this.ShowReportPagePreview(m_ReportPageDataSet);
                            //m_ReportPageTemplateProperty.StatisticsDataSource = this.GetFilteredData();
                            //string template = m_eftConfigData.report_layout_config;
                            //if (string.IsNullOrEmpty(template))
                            //    template = new XtraReportDefaultTemplate().ToString();

                            //XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate(template, m_ReportPageTemplateProperty) { DataSource = m_ReportPageDataSet };
                            //_report.ShowPreview();
                        }
                    }

                    else {
                        this.ShowReportPagePreview(m_ReportPageDataSet);
                        //m_ReportPageTemplateProperty.StatisticsDataSource = this.GetFilteredData(); 
                        //string template = m_eftConfigData.report_layout_config;
                        //if (string.IsNullOrEmpty(template))
                        //    template = BrightVision.Reporting.Business.FacadeReportTemplate.GetDefaultReportLayout();

                        //XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate(template, m_ReportPageTemplateProperty) { 
                        //    DataSource = m_ReportPageDataSet 
                        //};

                        //_report.ShowPreview();
                    }
                }
                catch (Exception ex) {
                    if (m_IsWebPortalCall)
                        throw ex;

                    BrightVision.Common.UI.NotificationDialog.Error("Reports", ex.Message);
                }
            }
            private string ShowReportPagePreview(ReportDataSet pReportDataSet, bool pCreateFile = false)
            {
                m_ReportPageTemplateProperty.StatisticsDataSource = new DataTable();
                if (m_CallingEnvironment != eCallingEnvironment.BrightSales_SendEmail)
                    m_ReportPageTemplateProperty.StatisticsDataSource = this.GetFilteredData();
                
                string template = m_eftConfigData.report_layout_config;
                if (string.IsNullOrEmpty(template))
                    template = BrightVision.Reporting.Business.FacadeReportTemplate.GetDefaultReportLayout();

                XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate(template, m_ReportPageTemplateProperty) {
                    DataSource = pReportDataSet
                };

                /**
                 * return the file name created if has any,
                 * else just show report and return empty string.
                 */
                if (pCreateFile) {
                    string _FileName = String.Format(@"{0}\Report_{1}_{2}.pdf",
                        m_ReportsPath,
                        DateTime.Now.ToString("yyyy-MM-dd"),
                        Guid.NewGuid().ToString()
                    );

                    XtraReportDefaultTemplate _TemplatePdf = _report;
                    PdfExportOptions pdfOptions = _TemplatePdf.ExportOptions.Pdf;
                    pdfOptions.Compressed = true;
                    pdfOptions.ImageQuality = PdfJpegImageQuality.Low;
                    _TemplatePdf.DataSource = pReportDataSet;
                    _TemplatePdf.ExportToPdf(_FileName);

                    return _FileName;
                }
                else
                    _report.ShowPreview();

                return string.Empty;
            }
            private string GenerateReports()
            {
                string _PdfFileName = string.Empty;

                try {
                    this.GetReportPageBaseData();
                    XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate();
                    var result = m_ReportPageTemplateProperty.DynamicProperty.Where(q => q.Type == TemplateDynamicType.Statistics);
                    if (m_ReportPageTemplateProperty.DynamicProperty != null && m_ReportPageTemplateProperty.DynamicProperty.Count > 0)
                        m_ReportPageTemplateProperty.DynamicProperty.RemoveAll(param => param.Type == TemplateDynamicType.Statistics);

                    string template = m_eftConfigData.report_layout_config;
                    if (template == null)
                        template = BrightVision.Reporting.Business.FacadeReportTemplate.GetDefaultReportLayout();

                    _report = new XtraReportDefaultTemplate(template);
                    if (m_IsWebPortalCall) {
                        if (m_CallingEnvironment == eCallingEnvironment.BrightManager_ViewDisplay ||
                            m_CallingEnvironment == eCallingEnvironment.BrightSales_ViewDisplay ||
                            m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail) {

                            /**
                             * create report for a specific account only 
                             * with no report headers.
                             */
                            if (m_AccountId > 0)
                                _PdfFileName = XtraReportDefaultTemplate.SavePDF(_report, m_ReportPageDataSet, m_ReportsPath, m_IsWebPortalCall);

                            /**
                             * create a report containing all accounts and contacts 
                             * with report headers.
                             */
                            else
                                _PdfFileName = this.ShowReportPagePreview(m_ReportPageDataSet, true);
                        }
                        else if (m_CallingEnvironment == eCallingEnvironment.BrightManager_SaveAccountPerPdf ||
                                 m_CallingEnvironment == eCallingEnvironment.BrightSales_SaveAccountPerPdf) {

                            m_lstPdfFiles = new List<string>();
                            m_lstPdfFiles = XtraReportDefaultTemplate.CreatePdfPerAccount(_report, m_ReportPageDataSet, m_ReportsPath);
                        }
                    }

                    /**
                     * this means that the call was made by the application. 
                     * Bright Manager or Bright Sales. (for sending emails)
                     */
                    else {
                        if (m_AccountId > 0 && m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail)
                            _PdfFileName = XtraReportDefaultTemplate.SavePDF(_report, m_ReportPageDataSet, m_ReportsPath, m_IsWebPortalCall);
                    }

                    //if (m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail || m_IsWebPortalCall) {
                    //    /**
                    //     * create report for a specific account only 
                    //     * with no report headers.
                    //     */
                    //    if (m_AccountId > 0)
                    //        _PdfFileName = XtraReportDefaultTemplate.SavePDF(_report, m_ReportPageDataSet, m_ReportsPath, m_IsWebPortalCall);

                    //    /**
                    //     * create a report containing all accounts and contacts 
                    //     * with report headers.
                    //     */
                    //    else
                    //        _PdfFileName = this.ShowReportPagePreview(m_ReportPageDataSet, true);
                    //}
                    //else
                    //    XtraReportDefaultTemplate.SavePDFPerAccount(_report, m_ReportPageDataSet);
                }
                catch (Exception e) {
                    if (m_IsWebPortalCall)
                        throw e;

                    BrightVision.Common.UI.NotificationDialog.Error("Reports", e.Message);
                }

                return _PdfFileName;
            }
            private void GetReportPageBaseData()
            {
                m_IsEmpty = false;
                if (this.gridView1.DataRowCount == null || this.gridView1.DataRowCount < 1) {
                    BrightVision.Common.UI.NotificationDialog.Information("Reports", "No data to preview.");
                    m_IsEmpty = true;
                    return;
                }

                using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(m_DatabaseConnection) { CommandTimeout = 0 }) {
                    m_eftConfigData = _efDbModel.view_configuration.FirstOrDefault(i => i.id == m_ViewConfigId);
                    m_eftSubCampaign = _efDbModel.subcampaigns.FirstOrDefault(i => i.id == m_eftConfigData.subcampaign_id);
                    m_eftCampaign = _efDbModel.campaigns.FirstOrDefault(i => i.id == m_eftSubCampaign.campaign_id);
                    m_eftCustomer = _efDbModel.customers.FirstOrDefault(i => i.id == m_eftCampaign.customer_id);
                    _efDbModel.Detach(m_eftConfigData);
                    _efDbModel.Detach(m_eftSubCampaign);
                    _efDbModel.Detach(m_eftCampaign);
                    _efDbModel.Detach(m_eftCustomer);
                    _efDbModel.FIUpdateContactTitles();

                    /**
                     * if send email, get data for sub_campaign_account_list and final_list.
                     */
                    if (m_CallingEnvironment == eCallingEnvironment.BrightSales_SendEmail && m_AccountId > 0) {
                        m_eftFinalList = _efDbModel.final_lists.FirstOrDefault(i => i.sub_campaign_id == m_eftSubCampaign.id);
                        if (m_eftFinalList != null) {
                            _efDbModel.Detach(m_eftFinalList);
                            m_eftSubCampaignAccountList = _efDbModel.sub_campaign_account_lists.FirstOrDefault(i =>
                                i.final_list_id == m_eftFinalList.id &&
                                i.account_id == m_AccountId
                            );
                                _efDbModel.Detach(m_eftSubCampaignAccountList);
                        }
                    }
                }

                if (m_eftConfigData == null || m_eftConfigData.report_layout_config == null) {
                    WaitDialog.Close();
                    if (m_IsWebPortalCall)
                        throw new Exception("No layout available for the selected view.");

                    BrightVision.Common.UI.NotificationDialog.Information("Reports", "No layout available for this view.");
                    return;
                }

                if (string.IsNullOrEmpty(m_eftConfigData.report_data_config)) {
                    WaitDialog.Close();
                    if (m_IsWebPortalCall)
                        throw new Exception("No parameter layout has been set for this report.");

                    BrightVision.Common.UI.NotificationDialog.Information("Reports", "No parameter layout has been set for this report.");
                    return;
                }

                m_ReportPageTemplateProperty = SerializeUtility.DeserializeFromXml<TemplateProperty>(m_eftConfigData.report_data_config);
                m_ReportPageDataSet = this.GetReportDataSet(m_ReportPageTemplateProperty);

                /**
                 * if has sort info, then apply.
                 */
                #region Sorting Logic
                if (!string.IsNullOrEmpty(m_GridSortInfo)) {
                    string sortExpression = this.GetSortExpression(gridView1);
                    if (!string.IsNullOrEmpty(sortExpression)) {
                        string[] _SortInfoCollection = sortExpression.Split(';');

                        ReportDataSet _rdsTemporary = (ReportDataSet)m_ReportPageDataSet.Clone();
                        DataSet _dsSortedData = new DataSet();
                        Dictionary<string, List<string>> _TableSortRules = new Dictionary<string, List<string>>();

                        /**
                         * group all sort rules by table.
                         */
                        foreach (string _SortInfo in _SortInfoCollection) {
                            string[] _item = _SortInfo.Split('|');
                            string _FieldNameInfo = this.GetTableFieldName(_item[0].ToString());
                            if (!string.IsNullOrEmpty(_FieldNameInfo)) {
                                string[] _val = _FieldNameInfo.Split('|');
                                string _TableName = _val[0];
                                string _FieldName = _val[1];

                                /**
                                 * create new table sort rule.
                                 * else, update existing table sort rule.
                                 * 
                                 * format:
                                 * <column_name1>|<sort_rule1>;<column_name2>|<sort_rule2>; and so on ...
                                 * 
                                 * this would later be processed by splitting the sort rules by semicolon(;), 
                                 * then split by bar(|).
                                 */
                                string _ColumnName = m_ReportPageDataSet.Tables[_TableName].Columns[_FieldName].ColumnName;
                                string _SortOrder = _item[1].ToString();
                                if (!_TableSortRules.ContainsKey(_TableName))
                                    _TableSortRules.Add(_TableName, new List<string>());

                                _TableSortRules[_TableName].Add(string.Format("{0} {1}", _ColumnName, _SortOrder));
                            }
                        }

                        /**
                         * set the sorting rules from KeyValuePair<string, List<string>> from _TableSortRules
                         * string = table name
                         * List<string> = sort rules
                         */
                        foreach (KeyValuePair<string, List<string>> _pair in _TableSortRules) {
                            DataTable _dtToSort = m_ReportPageDataSet.Tables[_pair.Key];
                            _dtToSort.DefaultView.Sort = string.Join(",", _TableSortRules[_pair.Key].ToArray());
                            _dsSortedData.Tables.Add(_dtToSort.DefaultView.ToTable());
                        }

                        /**
                         * copy all tables to the temporary report data set.
                         * then overwrite the original report data set with the
                         * temporary report dataset, since it contains the sorted
                         * tables that the report needs.
                         * 
                         * order of the tables, according to relationship:
                         * 1. account
                         * 2. accountdynamic
                         * 3. accountstatic
                         * 4. contact
                         * 5. contactdynamic
                         * 6. contactstatic
                         * 7. clientinfo
                         * 8. customers
                         */
                        if (_dsSortedData.Tables["account"] != null)
                            this.CopyTableRows(ref _rdsTemporary, _dsSortedData.Tables["account"].Rows, "account");
                        else
                            this.CopyTableRows(ref _rdsTemporary, m_ReportPageDataSet.Tables["account"].Rows, "account");

                        if (_dsSortedData.Tables["accountdynamic"] != null)
                            this.CopyTableRows(ref _rdsTemporary, _dsSortedData.Tables["accountdynamic"].Rows, "accountdynamic");
                        else
                            this.CopyTableRows(ref _rdsTemporary, m_ReportPageDataSet.Tables["accountdynamic"].Rows, "accountdynamic");

                        if (_dsSortedData.Tables["accountstatic"] != null)
                            this.CopyTableRows(ref _rdsTemporary, _dsSortedData.Tables["accountstatic"].Rows, "accountstatic");
                        else
                            this.CopyTableRows(ref _rdsTemporary, m_ReportPageDataSet.Tables["accountstatic"].Rows, "accountstatic");

                        if (_dsSortedData.Tables["contact"] != null)
                            this.CopyTableRows(ref _rdsTemporary, _dsSortedData.Tables["contact"].Rows, "contact");
                        else
                            this.CopyTableRows(ref _rdsTemporary, m_ReportPageDataSet.Tables["contact"].Rows, "contact");

                        if (_dsSortedData.Tables["contactdynamic"] != null)
                            this.CopyTableRows(ref _rdsTemporary, _dsSortedData.Tables["contactdynamic"].Rows, "contactdynamic");
                        else
                            this.CopyTableRows(ref _rdsTemporary, m_ReportPageDataSet.Tables["contactdynamic"].Rows, "contactdynamic");

                        if (_dsSortedData.Tables["contactstatic"] != null)
                            this.CopyTableRows(ref _rdsTemporary, _dsSortedData.Tables["contactstatic"].Rows, "contactstatic");
                        else
                            this.CopyTableRows(ref _rdsTemporary, m_ReportPageDataSet.Tables["contactstatic"].Rows, "contactstatic");

                        if (_dsSortedData.Tables["clientinfo"] != null)
                            this.CopyTableRows(ref _rdsTemporary, _dsSortedData.Tables["clientinfo"].Rows, "clientinfo");
                        else
                            this.CopyTableRows(ref _rdsTemporary, m_ReportPageDataSet.Tables["clientinfo"].Rows, "clientinfo");

                        if (_dsSortedData.Tables["customers"] != null)
                            this.CopyTableRows(ref _rdsTemporary, _dsSortedData.Tables["customers"].Rows, "customers");
                        else
                            this.CopyTableRows(ref _rdsTemporary, m_ReportPageDataSet.Tables["customers"].Rows, "customers");

                        m_ReportPageDataSet = null;
                        m_ReportPageDataSet = _rdsTemporary;
                    }
                }
                #endregion
            }
            private void CopyTableRows(ref ReportDataSet pDestination, DataRowCollection pSource, string pTableName)
            {
                DataRow _row = null;
                foreach (DataRow _drow in pSource) {
                    _row = pDestination.Tables[pTableName].NewRow();
                    _row = _drow;
                    pDestination.Tables[pTableName].ImportRow(_drow);
                }
            }
            private void SendReportRequest(string pWebRequestId)
            {
                string _WebPortalUrl = ConfigurationManager.AppSettings["WebPortalUrl"].ToString();
                string _WebPortalRequest = string.Format("{0}/pData/{1}", _WebPortalUrl, pWebRequestId);
                Process.Start(_WebPortalRequest);
            }
            private string GetSerializedSortInfo(GridColumnSortInfoCollection pSortinfoCollection)
            {
                ReportsProperty.SortInfoCollection _SortInfoCollection = new ReportsProperty.SortInfoCollection();
                foreach (GridColumnSortInfo _item in pSortinfoCollection) {
                    _SortInfoCollection.SortInfo.Add(new ReportsProperty.SortInfoProperty() {
                        Column = _item.Column.FieldName,
                        SortOrder = _item.SortOrder.ToString()
                    });
                }

                return SerializeUtility.Serialize(_SortInfoCollection);
            }
            private string GetSerializedColumnsInfo()
            {
                ReportsProperty.ColumnsInfoCollection _ColumnsInfoCollection = new ReportsProperty.ColumnsInfoCollection() {
                    HiddenColumnsInfo = m_lstHiddenColumnsInfo
                };

                return SerializeUtility.Serialize(_ColumnsInfoCollection);
            }
            private void SetGridSortInfo()
            {
                ReportsProperty.SortInfoCollection _Collection = SerializeUtility.DeserializeFromXml<ReportsProperty.SortInfoCollection>(m_GridSortInfo);
                gridView1.BeginSort();
                foreach (ReportsProperty.SortInfoProperty _item in _Collection.SortInfo) {
                    DevExpress.Data.ColumnSortOrder _SortOrder = _item.SortOrder.Equals("Ascending") ? DevExpress.Data.ColumnSortOrder.Ascending : DevExpress.Data.ColumnSortOrder.Descending;
                    gridView1.SortInfo.Add(new GridColumnSortInfo(
                        gridView1.Columns[_item.Column],
                        _SortOrder
                    ));
                }
                gridView1.EndSort();
            }
            private void SetGridColumnsInfo()
            {
                ReportsProperty.ColumnsInfoCollection _Collection = SerializeUtility.DeserializeFromXml<ReportsProperty.ColumnsInfoCollection>(m_GridColumnsInfo);
                gridView1.BeginUpdate();
                foreach (string _ColumnFieldName in _Collection.HiddenColumnsInfo)
                    gridView1.Columns[_ColumnFieldName].Visible = false;
                gridView1.EndUpdate();
            }
            #endregion

            #region Control Events
            private void gridView1_DragObjectDrop(object sender, DragObjectDropEventArgs e)
            {
                GridColumn _column = (e.DragObject as GridColumn);
                if (_column == null)
                    return;

                if (!_column.Visible && !m_lstHiddenColumnsInfo.Contains(_column.FieldName))
                    m_lstHiddenColumnsInfo.Add(_column.FieldName);

                else if (_column.Visible && m_lstHiddenColumnsInfo.Contains(_column.FieldName))
                    m_lstHiddenColumnsInfo.Remove(_column.FieldName);
            }
            private void gridView1_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
            {
                GridView view = sender as GridView;
                GridUtility.CreateGridContextMenu(view, e);
            }
            private void gridView1_ColumnFilterChanged(object sender, EventArgs e)
            {
                if (gridView1.RowCount < 1 || m_IsWebPortalCall)
                    return;

                labelControl1.Text = "Records: " + gridView1.RowCount.ToString();
            }
            private void btnShowReport_Click(object sender, EventArgs e)
            {
                WaitDialog.Show("Sending web service request ...");
                List<int> _SubCampaignIds = new List<int>();
                _SubCampaignIds.Add(m_SubCampaignId);

                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    _efDbContext.FIClearUserReuests(UserSession.CurrentUser.UserId);
                    m_eftConfigData = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_ViewConfigId);
                    m_eftSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == m_eftConfigData.subcampaign_id);
                    m_eftCampaign = _efDbContext.campaigns.FirstOrDefault(i => i.id == m_eftSubCampaign.campaign_id);
                    m_eftCustomer = _efDbContext.customers.FirstOrDefault(i => i.id == m_eftCampaign.customer_id);

                    _efDbContext.Detach(m_eftConfigData);
                    _efDbContext.Detach(m_eftSubCampaign);
                    _efDbContext.Detach(m_eftCampaign);
                    _efDbContext.Detach(m_eftCustomer);

                    Guid _RequestId = Guid.NewGuid();
                    string _GridSortInfo = this.GetSerializedSortInfo(gridView1.SortInfo);
                    string _ColumnsInfo = this.GetSerializedColumnsInfo();

                    serverside_report_requests _eftRequest = new serverside_report_requests() {
                        id = _RequestId,
                        calling_environment = (short)m_CallingEnvironment,
                        display_mode = (short)m_ViewType,
                        //calling_environment = (short)ReportsUtility.eCallingEnvironment.BrightSales_ViewDisplay,
                        //display_mode = (short)ReportsUtility.eDisplayMode.AccountsContacts_WithDialogData,
                        campaign_info = string.Format("{0}>{1}>{2}",
                            m_eftCustomer.customer_name,
                            m_eftCampaign.campaign_name,
                            m_eftSubCampaign.title
                        ),
                        sub_campaign_ids = string.Join(",", _SubCampaignIds),
                        view_config_id = m_ViewConfigId,
                        account_id = 0,
                        active_filter_string = gridView1.ActiveFilterString,
                        sort_info = _GridSortInfo,
                        columns_info = _ColumnsInfo,
                        requested_by = UserSession.CurrentUser.UserId,
                        requested_on = DateTime.Now
                    };
                    _efDbContext.serverside_report_requests.AddObject(_eftRequest);
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftRequest);
                    ReportsUtility.SendReportRequest(_RequestId.ToString());
                }
                WaitDialog.Close();

                //WaitDialog.Show("Sending web service request ...");
                //using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                //    serverside_report_requests _eftRequest = new serverside_report_requests() {
                //        calling_environment = (short)ReportsUtility.eCallingEnvironment.BrightSales_ViewDisplay,
                //        display_mode = (short)ReportsUtility.eDisplayMode.AccountsContacts_WithDialogData,
                //        campaign_info = m_CampaignInfo,
                //        //sub_campaign_ids = string.Join(",", pSubCampaignIds),
                //        view_config_id = m_ViewConfigId,
                //        account_id = 0,
                //        requested_by = UserSession.CurrentUser.UserId,
                //        requested_on = DateTime.Now
                //    };
                //    _efDbContext.serverside_report_requests.AddObject(_eftRequest);
                //    string _RequestId = _eftRequest.id.ToString();
                //    _efDbContext.Detach(_eftRequest);
                //    this.SendReportRequest(_RequestId);
                //}
                //WaitDialog.Close();

                //WaitDialog.Show("Loading report preview ...");
                //this.ReportPagePreview();
                //WaitDialog.Close();
            }
            private void btnSaveReportPerPdf_Click(object sender, EventArgs e)
            {
                WaitDialog.Show("Sending web service request ...");
                List<int> _SubCampaignIds = new List<int>();
                _SubCampaignIds.Add(m_SubCampaignId);
                
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    m_eftConfigData = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_ViewConfigId);
                    m_eftSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == m_eftConfigData.subcampaign_id);
                    m_eftCampaign = _efDbContext.campaigns.FirstOrDefault(i => i.id == m_eftSubCampaign.campaign_id);
                    m_eftCustomer = _efDbContext.customers.FirstOrDefault(i => i.id == m_eftCampaign.customer_id);

                    _efDbContext.Detach(m_eftConfigData);
                    _efDbContext.Detach(m_eftSubCampaign);
                    _efDbContext.Detach(m_eftCampaign);
                    _efDbContext.Detach(m_eftCustomer);

                    string _GridSortInfo = this.GetSerializedSortInfo(gridView1.SortInfo);
                    string _ColumnsInfo = this.GetSerializedColumnsInfo();
                    short _CallingEnvironment = (short)(m_CallingApplication == eCallingApplication.BrightManager ? eCallingEnvironment.BrightManager_SaveAccountPerPdf : eCallingEnvironment.BrightSales_SaveAccountPerPdf);

                    Guid _RequestId = Guid.NewGuid();
                    serverside_report_requests _eftRequest = new serverside_report_requests() {
                        id = _RequestId,
                        calling_environment = _CallingEnvironment,
                        display_mode = (short)m_ViewType,
                        campaign_info = string.Format("{0}>{1}>{2}",
                            m_eftCustomer.customer_name,
                            m_eftCampaign.campaign_name,
                            m_eftSubCampaign.title
                        ),
                        sub_campaign_ids = string.Join(",", _SubCampaignIds),
                        view_config_id = m_ViewConfigId,
                        account_id = 0,
                        active_filter_string = gridView1.ActiveFilterString,
                        sort_info = _GridSortInfo,
                        columns_info = _ColumnsInfo,
                        requested_by = UserSession.CurrentUser.UserId,
                        requested_on = DateTime.Now
                    };
                    _efDbContext.serverside_report_requests.AddObject(_eftRequest);
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftRequest);
                    ReportsUtility.CreatePdfFilesPerAccount(_RequestId.ToString());
                }
                WaitDialog.Close();

                //WaitDialog.Show("Generating PDF reports ...");
                //this.GenerateReports();
                //WaitDialog.Close();
            }
            #endregion
        }
    }
}
