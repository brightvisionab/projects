
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraLayout;
using DevExpress.XtraTab;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraReports.UI;

using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
using BrightVision.Reporting;
using BrightVision.Reporting.UI;

using SalesConsultant.Business;
using BrightVision.Reporting.Template;
using BrightVision.DQControl.Utilities;
using SalesConsultant.Facade;
using SalesConsultant.PublicProperties;
using BrightVision.Reporting.Utility;
using BrightVision.Common.Events.Core;
using SalesConsultant.Events;

namespace SalesConsultant.Modules
{
    public partial class ViewDisplay : XtraUserControl
    {
        #region Constructors
        public ViewDisplay()
        {
            this.Visible = false;
            this.InitializeComponent();
            this.Initialize();
        }
        public ViewDisplay(int pReportConfigId)
        {
            m_ReportConfigurationId = pReportConfigId;
            this.Visible = false;
            this.InitializeComponent();
            this.Initialize();
            this.AutoLoadReport();
        }
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private BrightPlatformEntities BPContext = null;
        private List<ReportsUtility.SubcampaignData> listSubcampaignData = null;
        private BackgroundWorker worker = null;

        private int m_CustomerId = 0;
        private int m_CampaignId = 0;
        private int m_SubCampaignId = 0;
        private string m_CustomerName = string.Empty;
        private string m_CampaignName = string.Empty;
        private string m_SubCampaignName = string.Empty;

        private ReportsUtility.ReportPage m_ReportPage = new ReportsUtility.ReportPage();

        private int m_ReportConfigurationId = 0;
        private view_configuration m_eftReportConfig = null;
        private List<CTScSubCampaignList> m_lstSubCampaigns = new List<CTScSubCampaignList>();
        #endregion

        #region Public Methods
        public void GenerateReportPages(int[] pSubCampaignIds, int pViewConfigId = 0)
        {
            ReportsUtility _Reports = new ReportsUtility() {
                CampaignInfo = string.Format("{0}", lookUpEditCustomerCampaign.Text),
                LSubCampaignData = new List<ReportsUtility.SubcampaignData>(),
                CallingEnvironment = ReportsUtility.eCallingEnvironment.BrightSales_ViewDisplay,
                CallingApplication = ReportsUtility.eCallingApplication.BrightSales
            };

            if (cboDisplayMode.Text.Equals("Accounts & contacts having dialog data"))
                _Reports.DisplayMode = ReportsUtility.eDisplayMode.AccountsContacts_WithDialogData;
            else if (cboDisplayMode.Text.Equals("Accounts & contacts that have made call attempts"))
                _Reports.DisplayMode = ReportsUtility.eDisplayMode.AccountsContacts_WithCallAttempts;
            
            _Reports.GenerateReportPages(ref tcgView, pSubCampaignIds, pViewConfigId);
        }
        //public void ReportPagePreview(int viewconfigId)
        //{
        //    m_ReportPage.PrintPreview(viewconfigId, true);
        //}
        //public string CreateReportsPdf()
        //{
        //    return m_ReportPage.CreatePDF();
        //}
        #endregion

        #region Private Methods
        private void AutoLoadReport()
        {
            m_eftReportConfig = new view_configuration();
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                m_eftReportConfig = _efDbContext.view_configuration.FirstOrDefault(i => i.id == m_ReportConfigurationId);
                _efDbContext.Detach(m_eftReportConfig);
            }

            cboDisplayMode.SelectedIndex = 0;
            for (int i = 0; i < m_lstSubCampaigns.Count; i++) {
                string[] _ids = m_lstSubCampaigns[i].id.ToString().Split(';');
                string[] _sub_campaign = _ids[2].Split('|');
                if (m_eftReportConfig.subcampaign_id == Convert.ToInt32(_sub_campaign[0].ToString())) {
                    lookUpEditCustomerCampaign.ItemIndex = i;
                    this.btnLoad.PerformClick();
                    break;
                }
            }

            m_ReportConfigurationId = 0;
            m_eftReportConfig = null;
        }
        private void Initialize()
        {
            DevExpress.Utils.ImageCollection stateImages = new DevExpress.Utils.ImageCollection();
            stateImages.ImageSize = new System.Drawing.Size(16, 16);
            stateImages.AddImage(Properties.Resources.loader);
            tcgView.Images = stateImages;

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            this.GetCampaigns();
            this.Visible = true;
        }
        private void GetCampaigns()
        {
            WaitDialog.Show("Loading campaign list ...");
            List<string> _lstStatuses = new List<string>();
            
            if (checkEditArchived.Checked)
                _lstStatuses.Add("Archived");
            if (checkEditActive.Checked)
                _lstStatuses.Add("Active");
            if (checkEditOnHold.Checked)
                _lstStatuses.Add("On Hold");

            this.ClearPages();
            btnLoad.Enabled = false;
            lookUpEditCustomerCampaign.Properties.DataSource = null;
            lookUpEditCustomerCampaign.Properties.Columns.Clear();

            if (_lstStatuses.Count > 0) {
                m_lstSubCampaigns = ObjectSubCampaign.GetActiveSubCampaigns(UserSession.CurrentUser.UserId, _lstStatuses);
                if (m_lstSubCampaigns.Count > 0) {
                    lookUpEditCustomerCampaign.Properties.DataSource = m_lstSubCampaigns;
                    lookUpEditCustomerCampaign.Properties.DisplayMember = "item";
                    lookUpEditCustomerCampaign.Properties.ValueMember = "id";
                    lookUpEditCustomerCampaign.Properties.Columns.Add(new LookUpColumnInfo("item"));
                    btnLoad.Enabled = true;
                }
            }
            WaitDialog.Close();
        }
        private void ClearPages()
        {
            tcgView.TabPages.Clear();
        }
        #endregion

        #region Control Events
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bwAsync = sender as BackgroundWorker;
            if (bwAsync.CancellationPending)
                e.Cancel = true;
        }
        private void lookUpEditCustomerCampaign_EditValueChanged(object sender, EventArgs e)
        {
            btnLoad.Enabled = false;
            if (lookUpEditCustomerCampaign.Text.Length < 1 || lookUpEditCustomerCampaign.EditValue == null)
                return;

            string[] _ids = lookUpEditCustomerCampaign.EditValue.ToString().Split(';');
            string[] _customer = _ids[0].Split('|');
            string[] _campaign = _ids[1].Split('|');
            string[] _sub_campaign = _ids[2].Split('|');

            m_CustomerId = Convert.ToInt32(_customer[0]);
            m_CustomerName = _customer[1].ToString();
            m_CampaignId = Convert.ToInt32(_campaign[0]);
            m_CampaignName = _campaign[1].ToString();
            m_SubCampaignId = Convert.ToInt32(_sub_campaign[0]);
            m_SubCampaignName = _sub_campaign[1].ToString();
            btnLoad.Enabled = true;
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            #region Debugging Use Only
            /**  /
            int _SubCampaignId = 287;
            int _ViewConfigId = 351;
            subcampaign _eftSubCampaign;
            view_configuration _eftViewConfig;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == _SubCampaignId);
                _eftViewConfig = _efDbContext.view_configuration.FirstOrDefault(i => i.id == _ViewConfigId);
                _efDbContext.Detach(_eftSubCampaign);
                _efDbContext.Detach(_eftViewConfig);
            }

            this.ClearAllTabs();
            List<XtraTabPage> listGroup = new List<XtraTabPage>();
            ViewTab group = null;
            group = new ViewTab("Debugging Mode");
            group.ConfigData = new ViewCofigData() {
                id = _eftViewConfig.id,
                name = _eftViewConfig.name,
                subcampaign_id = _eftViewConfig.subcampaign_id
            };
            group.BindControls();
            listGroup.Add(group);
            tcgView.BeginUpdate();
            tcgView.TabPages.AddRange(listGroup.ToArray());
            tcgView.EndUpdate();
            worker.RunWorkerAsync();
            return;
            /**/
            #endregion

            if (lookUpEditCustomerCampaign.Text.Length < 1 || lookUpEditCustomerCampaign.EditValue == null)
                return;

            if (worker.IsBusy) {
                worker.CancelAsync();
                return;
            }

            WaitDialog.Show("Generating report pages ...");
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                _efDbContext.FIUpdateContactTitles();
            }

            if (m_ReportConfigurationId > 0) {
                List<int> subcampaign_ids = new List<int>();
                subcampaign_ids.Add(m_eftReportConfig.subcampaign_id);
                this.ClearPages();
                this.GenerateReportPages(subcampaign_ids.ToArray(), m_eftReportConfig.id);
                worker.RunWorkerAsync();
            }
            else {
                List<int> subcampaign_ids = new List<int>();
                subcampaign_ids.Add(m_SubCampaignId);
                this.ClearPages();
                this.GenerateReportPages(subcampaign_ids.ToArray());
                worker.RunWorkerAsync();
                WaitDialog.Close();
            }
            WaitDialog.Close();

            //List<int> subcampaign_ids = new List<int>();
            //subcampaign_ids.Add(m_SubCampaignId);
            //this.ClearPages();
            //this.GenerateReportPages(subcampaign_ids.ToArray());
            //worker.RunWorkerAsync();
            //WaitDialog.Close();

            //if (m_ViewConfigId < 1)
            //{
            //    string val = (string)ccbeSubcampaign.EditValue;
            //    if (!string.IsNullOrEmpty(val))
            //    {
            //        List<int> subcampaign_ids = new List<int>();
            //        var strVals = val.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            //        strVals.ForEach(delegate(string x)
            //        {
            //            subcampaign_ids.Add(int.Parse(x));
            //        });
            //        this.ClearPages();
            //        this.GenerateReportPages(subcampaign_ids.ToArray());
            //        worker.RunWorkerAsync();
            //    }
            //}
            //else
            //{
            //    List<int> subcampaign_ids = new List<int>();
            //    subcampaign_ids.Add(m_efoViewConfig.subcampaign_id);
            //    this.ClearPages();
            //    this.GenerateReportPages(subcampaign_ids.ToArray(), m_efoViewConfig.id);
            //    worker.RunWorkerAsync();
            //}
        }
        private void checkEditActive_CheckedChanged(object sender, EventArgs e)
        {
            this.GetCampaigns();
        }
        private void checkEditArchived_CheckedChanged(object sender, EventArgs e)
        {
            this.GetCampaigns();
        }
        private void checkEditOnHold_CheckedChanged(object sender, EventArgs e)
        {
            this.GetCampaigns();
        }
        private void bbiExportCSV_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (tcgView.SelectedTabPage != null) {
                var oView = tcgView.SelectedTabPage.Tag as ReportsUtility.ReportPage;
                if (oView != null)
                    oView.Export(ViewExportType.CSV);
            }

        }
        private void bbiExportXLS_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (tcgView.SelectedTabPage != null) {
                var oView = tcgView.SelectedTabPage.Tag as ReportsUtility.ReportPage;
                if (oView != null)
                    oView.Export(ViewExportType.Excel2003);
            }
        }
        private void bbiExportXLSX_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (tcgView.SelectedTabPage != null) {
                var oView = tcgView.SelectedTabPage.Tag as ReportsUtility.ReportPage;
                if (oView != null)
                    oView.Export(ViewExportType.Excel2007);
            }
        }
        #endregion
/**
 * ================================================================
 * This is where we define the implementation of the view tab class
 * ================================================================
 */
        //#region View Tab Class Implementation
        //private class ViewTab : DevExpress.XtraTab.XtraTabPage
        //{
        //    #region Member Variables
        //    private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        //    private LayoutControl layoutControl;
        //    private LayoutControlGroup layoutControlGroup1;
        //    private LayoutControlItem layoutControlItem1;
        //    private LayoutControlItem layoutControlItem2;
        //    private LayoutControlItem layoutControlItem3;
        //    private LayoutControlItem layoutControlItem4;
        //    private LayoutControlItem layoutControlItem5;
        //    private EmptySpaceItem emptySpaceItem1;
        //    private EmptySpaceItem emptySpaceItemLoding;
        //    private SimpleButton simpleButton1;
        //    private SimpleButton simpleButton2;
        //    private SimpleButton simpleButton3;
        //    private LabelControl labelControl1;
        //    private GridControl gridControl1;
        //    private GridView gridView1;
        //    private DataTable dtSource = null;
        //    private string m_viewConfigName;
        //    private int m_viewConfigId;
        //    private int m_subCampaignId;
        //    private int m_customerId;
        //    private Form m_parentForm;
        //    private eViewType m_viewType;
        //    private static DataTable static_dt = null;
        //    #endregion

        //    #region Constructor
        //    public ViewTab(string groupName)
        //        : base()
        //    {
        //        InitializeComponent();
        //        this.Text = groupName;
        //        this.ImageIndex = 0;
        //    }
        //    #endregion

        //    #region Public Properties
        //    public ViewCofigData ConfigData { get; set; }
        //    public string XML_Config { get; set; }
        //    public string XML_ConfigWithData { get; set; }

        //    public DataTable DataSource
        //    {
        //        get { return dtSource; }
        //        set {
        //            dtSource = value;
        //            this.FillDisplayView();
        //        }
        //    }
        //    public string ViewConfigName
        //    {
        //        set { m_viewConfigName = value; }
        //    }
        //    public int ViewConfigId
        //    {
        //        set { m_viewConfigId = value; }
        //    }
        //    public int SubCampaignId
        //    {
        //        set { m_subCampaignId = value; }
        //    }
        //    public int CustomerId
        //    {
        //        set { m_customerId = value; }
        //    }
        //    public Form ParentForm
        //    {
        //        set { m_parentForm = value; }
        //    }
        //    public eViewType ViewType
        //    {
        //        set { m_viewType = value; }
        //    }

        //    public enum eViewType
        //    {
        //        AccountsContactsWithDialogData,
        //        AccountsContactsWithCallAttempts
        //    }
        //    #endregion

        //    #region Private Properties
        //    private class DialogComponentColumn
        //    {
        //        public int position_index { get; set; }
        //        public bool account_level { get; set; }
        //        public string component_type { get; set; }
        //        public string label_name { get; set; }
        //        public bool merged_column { get; set; }
        //    }
        //    #endregion

        //    #region Object Events
        //    private void gridView1_ColumnFilterChanged(object sender, EventArgs e)
        //    {
        //        if (gridView1.RowCount < 1)
        //            return;

        //        labelControl1.Text = "Records: " + gridView1.RowCount.ToString();
        //    }
        //    private void gridView1_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        //    {
        //        GridView view = sender as GridView;
        //        GridUtility.CreateGridContextMenu(view, e);
        //    }
        //    private void simpleButton1_Click(object sender, EventArgs e)
        //    {
        //        //WaitDialog.Show(m_parentForm, "Loading report...");
        //        //DataSet gridData = GetReportDataSet();

        //        //ReportUserDesigner _oReport = new ReportUserDesigner(this.GetReportDataSet(m_viewConfigId, gridData));
        //        //_oReport.SubCampaignId = m_subCampaignId;
        //        //_oReport.ViewConfigId = m_viewConfigId;
        //        //WaitDialog.Close();
        //        //_oReport.ShowReportDesigner();
        //    }
        //    private void simpleButton2_Click(object sender, EventArgs e)
        //    {
        //        //bool _hasLayOut = true;
        //        //BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        //        //export_view_report_templates _item = _efDbModel.export_view_report_templates.FirstOrDefault(i => i.view_config_id == m_viewConfigId);
        //        //if (_item == null)
        //        //{
        //        //    MessageBox.Show("No layout available for this view. Will load using the default template.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        //    _hasLayOut = false;
        //        //}

        //        //WaitDialog.Show("Loading report...");
        //        //ReportUserDesigner _oReport = null;
        //        //DataSet gridData = GetReportDataSet();
        //        //if (_hasLayOut)
        //        //    _oReport = new ReportUserDesigner(this.GetReportDataSet(m_viewConfigId, gridData), _item.layout_config.ToString());
        //        //else
        //        //    _oReport = new ReportUserDesigner(this.GetReportDataSet(m_viewConfigId, gridData));
        //        //_oReport.SubCampaignId = m_subCampaignId;
        //        //_oReport.ViewConfigId = m_viewConfigId;
        //        //_oReport.ShowReportDesigner();
        //        //WaitDialog.Close();
        //    }
        //    private void simpleButtonShowPrintPreview_Click(object sender, EventArgs e)
        //    {
        //        if (gridView1.DataRowCount == null || gridView1.DataRowCount < 1)
        //            return;

        //        WaitDialog.Show("Loading report ...");
        //        try {
        //            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {

        //                // update contact titles first
        //                _efDbModel.FIUpdateContactTitles();

        //                view_configuration _eftViewConfig = _efDbModel.view_configuration.FirstOrDefault(i => i.id == m_viewConfigId);
        //                _efDbModel.Detach(_eftViewConfig);

        //                if (_eftViewConfig == null || _eftViewConfig.report_layout_config == null) {
        //                    WaitDialog.Close();
        //                    NotificationDialog.Information("Bright Sales", "No layout available for this view.");
        //                    return;
        //                }

        //                if (string.IsNullOrEmpty(_eftViewConfig.report_data_config)) {
        //                    WaitDialog.Close();
        //                    NotificationDialog.Information("Bright Sales", "No parameter layout has been set for this report.");
        //                    return;
        //                }

        //                var templateData = SerializeUtility.DeserializeFromXml<TemplateProperty>(_eftViewConfig.report_data_config);
        //                ReportDataSet getReportDataSet = this.GetReportDataSet(m_viewConfigId, templateData, null, null);

        //                //DAN: https://brightvision.jira.com/browse/PLATFORM-2457
        //                //- Implement to sort report when grid has been sorted out
        //                //---------------------------------------------------------------------------------------
        //                var sortExpression = GetSortExpression(gridView1);
        //                if (sortExpression != "")
        //                {
        //                    string[] arrSortEx = sortExpression.Split('|');
        //                    string tableFieldName = GetTableFieldName(arrSortEx[0]);

        //                    if (tableFieldName != "")
        //                    {
        //                        string tableName = tableFieldName.Split('|')[0];
        //                        string fieldName = tableFieldName.Split('|')[1];
        //                        ReportDataSet reportDatasetTemp = new ReportDataSet();
        //                        reportDatasetTemp.EnforceConstraints = false;
        //                        /*var sortedRows = (from myRow in getReportDataSet.Tables["account"].AsEnumerable()
        //                                          orderby myRow["company_name"] descending
        //                                          select myRow).ToArray();*/
        //                        //DAN: Copy to DataTable first as it causes error during linq when original table has been filter/deleted rows.
        //                        DataTable dt = getReportDataSet.Tables[tableName].DefaultView.ToTable();
        //                        var sortedRows = (from myRow in dt.AsEnumerable()
        //                                          orderby myRow[fieldName] ascending
        //                                          select myRow).ToArray();

        //                        if (arrSortEx[1] == "DESC")
        //                        {
        //                            sortedRows = (from myRow in dt.AsEnumerable()
        //                                          orderby myRow[fieldName] descending
        //                                          select myRow).ToArray();
        //                        }

        //                        DataTable dtSorted = sortedRows.CopyToDataTable();

        //                        foreach (DataRow dr in dtSorted.Rows)
        //                        {
        //                            reportDatasetTemp.Tables[tableName].Rows.Add(dr.ItemArray);
        //                        }

        //                        for (int i = 0; i < getReportDataSet.Tables.Count; i++)
        //                        {
        //                            if (getReportDataSet.Tables[i].TableName != tableName)
        //                            {
        //                                dt.Clear();
        //                                dt.Dispose();
        //                                dt = null;
        //                                dt = new DataTable();
        //                                dt = getReportDataSet.Tables[i].DefaultView.ToTable();

        //                                sortedRows = (from myRow in dt.AsEnumerable()
        //                                              select myRow).ToArray();
        //                                if (sortedRows.Count() == 0) continue;
        //                                dtSorted = sortedRows.CopyToDataTable();

        //                                foreach (DataRow dr in dtSorted.Rows)
        //                                {
        //                                    reportDatasetTemp.Tables[i].Rows.Add(dr.ItemArray);
        //                                }
        //                            }
        //                        }
        //                        reportDatasetTemp.EnforceConstraints = true;

        //                        templateData.StatisticsDataSource = GetFilteredData();
        //                        string template = _eftViewConfig.report_layout_config;
        //                        if (string.IsNullOrEmpty(template))
        //                            template = ToString(new XtraReportDefaultTemplate());

        //                        XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate(template, templateData) { DataSource = reportDatasetTemp };
        //                        _report.ShowPreview();
        //                    }
        //                    else
        //                    {
        //                        templateData.StatisticsDataSource = GetFilteredData();
        //                        string template = _eftViewConfig.report_layout_config;
        //                        if (string.IsNullOrEmpty(template))
        //                            template = ToString(new XtraReportDefaultTemplate());

        //                        XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate(template, templateData) { DataSource = getReportDataSet };
        //                        _report.ShowPreview();
        //                    }
        //                    //----------------------------------------------------------------------------------------------
        //                }
        //                else
        //                {
        //                    templateData.StatisticsDataSource = GetFilteredData();
        //                    string template = _eftViewConfig.report_layout_config;
        //                    if (string.IsNullOrEmpty(template))
        //                        template = ToString(new XtraReportDefaultTemplate());

        //                    XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate(template, templateData) { DataSource = getReportDataSet };
        //                    _report.ShowPreview();
        //                }


        //            }
        //        }
        //        catch (Exception ex) {
        //            BrightVision.Common.UI.NotificationDialog.Error("Bright Manager", ex.Message);
        //        }


        //        WaitDialog.Close();
        //    }
        //    #endregion

        //    #region Public Methods
        //    public void InitRecordCount()
        //    {
        //        this.labelControl1.Text = "Records: 0";
        //    }
        //    public void SetRecordCount(int pRecordCount)
        //    {
        //        this.labelControl1.Text = "Records: " + pRecordCount.ToString();
        //    }
        //    public void InitializeComponent()
        //    {
        //        layoutControl = new LayoutControl();
        //        layoutControl.Name = "layoutControl" + Guid.NewGuid().ToString();
        //        layoutControl.Dock = DockStyle.Fill;
        //        this.Controls.Add(layoutControl);

        //        this.layoutControlGroup1 = new LayoutControlGroup();
        //        this.layoutControlGroup1.Name = "layoutControlGroup" + Guid.NewGuid().ToString();
        //        this.layoutControlGroup1.Text = this.Text;
        //        this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
        //        this.layoutControlGroup1.ShowInCustomizationForm = false;

        //        //Loading datasource
        //        this.emptySpaceItemLoding = new EmptySpaceItem();
        //        this.emptySpaceItemLoding.Text = "Loading view display. Please wait...";
        //        this.emptySpaceItemLoding.AppearanceItemCaption.Font = new Font("Arial", 10f, FontStyle.Bold);
        //        this.emptySpaceItemLoding.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        //        this.emptySpaceItemLoding.TextVisible = true;
        //        this.emptySpaceItemLoding.Size = new Size(100, 20);
        //        this.emptySpaceItemLoding.MaxSize = new Size(100, 20);
        //        this.emptySpaceItemLoding.MinSize = new Size(100, 20);
        //        this.emptySpaceItemLoding.SizeConstraintsType = SizeConstraintsType.Custom;
        //        this.layoutControlGroup1.AddItem(emptySpaceItemLoding);

        //        this.gridView1 = new GridView();
        //        this.gridView1.Name = "gridView" + Guid.NewGuid().ToString();
        //        this.gridView1.GridControl = this.gridControl1;
        //        this.gridView1.OptionsFind.AlwaysVisible = false;
        //        //this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
        //        this.gridView1.OptionsBehavior.AutoPopulateColumns = true;
        //        this.gridView1.OptionsSelection.MultiSelect = false;
        //        this.gridView1.OptionsView.ShowGroupPanel = false;
        //        this.gridView1.OptionsView.ColumnAutoWidth = false;
        //        this.gridView1.OptionsBehavior.Editable = false;
        //        this.gridView1.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(gridView1_PopupMenuShowing);
        //        this.gridView1.ColumnFilterChanged += new EventHandler(gridView1_ColumnFilterChanged);

        //        this.gridControl1 = new GridControl();
        //        this.gridControl1.Name = "gridControl" + Guid.NewGuid().ToString();
        //        this.gridControl1.MainView = this.gridView1;
        //        this.gridControl1.ViewCollection.AddRange(new BaseView[] { this.gridView1 });

        //        //this.simpleGenerateReport = new SimpleButton() ;
        //        //this.gridControl1.Name = "simpleGenerateReport" + Guid.NewGuid().ToString();
        //        //this.Text = "Generate Report";
        //        //this.Width = 100;

        //        //this.simpleButton2 = new SimpleButton();
        //        //this.simpleButton2.Name = "simpleButton" + Guid.NewGuid().ToString();
        //        //this.simpleButton2.Text = "Export to Excel";
        //        //this.simpleButton2.Size = new System.Drawing.Size(150, 22);
        //        //this.simpleButton2.Enabled = false;
        //        //this.simpleButton2.Click += new EventHandler(simpleButton2_Click);

        //        //this.simpleButton3 = new SimpleButton();
        //        //this.simpleButton3.Name = "simpleButton" + Guid.NewGuid().ToString();
        //        //this.simpleButton3.Text = "Export to PDF";
        //        //this.simpleButton3.Size = new System.Drawing.Size(150, 22);
        //        //this.simpleButton3.Click += new EventHandler(simpleButton1_Click);

        //        //this.simpleButton4 = new SimpleButton();
        //        //this.simpleButton4.Name = "simpleButton" + Guid.NewGuid().ToString();
        //        //this.simpleButton4.Text = "Show in Dialog";
        //        //this.simpleButton4.Size = new System.Drawing.Size(150, 22);
        //        //this.simpleButton4.Click += new EventHandler(simpleButton1_Click);

        //    }
        //    public void Export(ViewExportType exportType)
        //    {
        //        SaveFileDialog dialog1 = new SaveFileDialog();
        //        if (exportType == ViewExportType.Excel2003)
        //        {
        //            dialog1.Filter = "Excel Workbook (*.xls)|*.xls";
        //        }
        //        else if (exportType == ViewExportType.Excel2007)
        //        {
        //            dialog1.Filter = "Excel Workbook (*.xslx)|*.xlsx";
        //        }
        //        else if (exportType == ViewExportType.CSV)
        //        {
        //            dialog1.Filter = "CSV (Comma Delimited) (*.csv)|*.csv";
        //        }

        //        dialog1.Title = "Save As";
        //        dialog1.CheckPathExists = true;
        //        dialog1.CheckFileExists = false;
        //        if (dialog1.ShowDialog() == DialogResult.OK)
        //        {
        //            if (dialog1.FileName != "")
        //            {
        //                if (dialog1.FilterIndex == 1)
        //                {
        //                    gridView1.OptionsPrint.AutoWidth = false;
        //                    gridView1.BestFitColumns();

        //                    FileStream fs = (FileStream)dialog1.OpenFile();
        //                    if (exportType == ViewExportType.CSV)
        //                    {
        //                        DevExpress.XtraPrinting.CsvExportOptions opts = new DevExpress.XtraPrinting.CsvExportOptions();
        //                        gridView1.Export(DevExpress.XtraPrinting.ExportTarget.Csv, fs, opts);
        //                    }
        //                    else if (exportType == ViewExportType.Excel2007)
        //                    {
        //                        DevExpress.XtraPrinting.XlsxExportOptions opts = new DevExpress.XtraPrinting.XlsxExportOptions();
        //                        opts.ExportMode = DevExpress.XtraPrinting.XlsxExportMode.SingleFile;
        //                        opts.SheetName = "Sheet1";
        //                        gridControl1.ExportToXlsx(fs, opts);
        //                    }
        //                    else if (exportType == ViewExportType.Excel2003)
        //                    {
        //                        DevExpress.XtraPrinting.XlsExportOptions opts = new DevExpress.XtraPrinting.XlsExportOptions();
        //                        opts.ExportMode = DevExpress.XtraPrinting.XlsExportMode.SingleFile;
        //                        opts.SheetName = "Sheet1";
        //                        gridControl1.ExportToXls(fs, opts);
        //                    }
        //                    fs.Close();
        //                }

        //            }
        //        }
        //    }
        //    public void BindControls()
        //    {
        //        this.layoutControl.BeginUpdate();
        //        this.layoutControlItem1 = new LayoutControlItem();
        //        this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();
        //        this.layoutControlItem1.Control = gridControl1;
        //        this.layoutControlItem1.TextVisible = false;
        //        this.layoutControlGroup1.AddItem(this.layoutControlItem1);

        //        this.emptySpaceItem1 = new EmptySpaceItem();
        //        //this.emptySpaceItem1.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 3, 3, 5);
        //        this.layoutControlGroup1.AddItem(emptySpaceItem1);

        //        /**
        //         * [@jeff 06.18.2012]: https://brightvision.jira.com/browse/PLATFORM-1462
        //         * added label to display the current grid record count.
        //         */
        //        this.labelControl1 = new LabelControl();
        //        this.labelControl1.Name = "labelControl" + Guid.NewGuid().ToString();
        //        this.labelControl1.Text = "Records: 0";
        //        this.labelControl1.Size = new System.Drawing.Size(120, 22);

        //        this.layoutControlItem5 = new LayoutControlItem();
        //        this.layoutControlItem5.Name = "layoutControlItem" + Guid.NewGuid().ToString();
        //        this.layoutControlItem5.Control = labelControl1;
        //        this.layoutControlItem5.SizeConstraintsType = SizeConstraintsType.Custom;
        //        this.layoutControlItem5.MaxSize = new Size(130, 30);
        //        this.layoutControlItem5.MinSize = new Size(80, 24);
        //        this.layoutControlItem5.Size = new Size(this.labelControl1.Width + 8, this.labelControl1.Height + 8);
        //        this.layoutControlItem5.TextVisible = false;
        //        this.layoutControlItem5.ShowInCustomizationForm = false;
        //        this.layoutControlItem5.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 3, 3, 5);
        //        this.layoutControlGroup1.AddItem(layoutControlItem5, emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Left);

        //        /**
        //         * create new template.
        //         */
        //        /** /
        //        this.simpleButton1 = new SimpleButton();
        //        this.simpleButton1.Name = "simpleButton" + Guid.NewGuid().ToString();
        //        this.simpleButton1.Text = "Create Template";
        //        this.simpleButton1.Size = new System.Drawing.Size(100, 22);
        //        this.simpleButton1.Click += new EventHandler(simpleButton1_Click);

        //        this.layoutControlItem2 = new LayoutControlItem();
        //        this.layoutControlItem2.Name = "layoutControlItem" + Guid.NewGuid().ToString();
        //        this.layoutControlItem2.Control = simpleButton1;
        //        this.layoutControlItem2.SizeConstraintsType = SizeConstraintsType.Custom;
        //        this.layoutControlItem2.MaxSize = new Size(110, 30);
        //        this.layoutControlItem2.MinSize = new Size(80, 24);
        //        this.layoutControlItem2.Size = new Size(this.simpleButton1.Width + 8, this.simpleButton1.Height + 8);
        //        this.layoutControlItem2.TextVisible = false;
        //        this.layoutControlItem2.ShowInCustomizationForm = false;
        //        this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 3, 3, 5);
        //        this.layoutControlGroup1.AddItem(layoutControlItem2, emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Left);
        //        /**/

        //        /**
        //         * edit template.
        //         */
        //        /** /
        //        this.simpleButton2 = new SimpleButton();
        //        this.simpleButton2.Name = "simpleButton" + Guid.NewGuid().ToString();
        //        this.simpleButton2.Text = "Edit Template";
        //        this.simpleButton2.Size = new System.Drawing.Size(90, 22);
        //        this.simpleButton2.Click += new EventHandler(simpleButton2_Click);

        //        this.layoutControlItem3 = new LayoutControlItem();
        //        this.layoutControlItem3.Name = "layoutControlItem" + Guid.NewGuid().ToString();
        //        this.layoutControlItem3.Control = simpleButton2;
        //        this.layoutControlItem3.SizeConstraintsType = SizeConstraintsType.Custom;
        //        this.layoutControlItem3.MaxSize = new Size(100, 30);
        //        this.layoutControlItem3.MinSize = new Size(80, 24);
        //        this.layoutControlItem3.Size = new Size(this.simpleButton2.Width + 8, this.simpleButton2.Height + 8);
        //        this.layoutControlItem3.TextVisible = false;
        //        this.layoutControlItem3.ShowInCustomizationForm = false;
        //        this.layoutControlItem3.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 3, 3, 5);
        //        this.layoutControlGroup1.AddItem(layoutControlItem3, emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Left);
        //        /**/

        //        /**
        //         * show report.
        //         */
        //        this.simpleButton3 = new SimpleButton();
        //        this.simpleButton3.Name = "simpleButton" + Guid.NewGuid().ToString();
        //        this.simpleButton3.Text = "Show Report";
        //        this.simpleButton3.Size = new System.Drawing.Size(90, 22);
        //        this.simpleButton3.Click += new EventHandler(simpleButtonShowPrintPreview_Click);

        //        this.layoutControlItem4 = new LayoutControlItem();
        //        this.layoutControlItem4.Name = "layoutControlItem" + Guid.NewGuid().ToString();
        //        this.layoutControlItem4.Control = simpleButton3;
        //        this.layoutControlItem4.SizeConstraintsType = SizeConstraintsType.Custom;
        //        this.layoutControlItem4.MaxSize = new Size(100, 30);
        //        this.layoutControlItem4.MinSize = new Size(80, 24);
        //        this.layoutControlItem4.Size = new Size(this.simpleButton3.Width + 8, this.simpleButton3.Height + 8);
        //        this.layoutControlItem4.TextVisible = false;
        //        this.layoutControlItem4.ShowInCustomizationForm = false;
        //        this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 3, 3, 5);
        //        this.layoutControlGroup1.AddItem(layoutControlItem4, emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Left);

        //        //this.layoutControlItem2 = new LayoutControlItem();
        //        //this.layoutControlItem2.Name = "layoutControlItem" + Guid.NewGuid().ToString();
        //        //this.layoutControlItem2.Control = simpleButton2;
        //        //this.layoutControlItem2.SizeConstraintsType = SizeConstraintsType.Custom;
        //        //this.layoutControlItem2.MaxSize = new Size(100, 24);
        //        //this.layoutControlItem2.MinSize = new Size(80, 24);
        //        //this.layoutControlItem2.Size = new Size(80, 24);
        //        //this.layoutControlItem2.TextVisible = false;
        //        //this.layoutControlItem2.ShowInCustomizationForm = false;                
        //        //this.layoutControlGroup1.AddItem(layoutControlItem2, emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Right);

        //        layoutControl.Root = layoutControlGroup1;
        //        layoutControl.Root.GroupBordersVisible = false;
        //        layoutControl.EndUpdate();
        //        this.Tag = this;
        //    }
        //    public void SetButtonState(bool state)
        //    {

        //        /** /
        //        this.simpleButton1.Enabled = state;
        //        this.simpleButton2.Enabled = state;
        //        this.simpleButton3.Enabled = state;
        //        /**/

        //        this.simpleButton3.Enabled = state;
        //    }
        //    public void PrintPreview(int viewconfigId, bool IsFromRelease = false)
        //    {
        //        int pAccountId = 0;
        //        DataTable _dt = null;
        //        if (!IsFromRelease)
        //        {
        //            if (gridView1.DataRowCount == null || gridView1.DataRowCount < 1)
        //                return;
        //        }
        //        else
        //        {
        //            pAccountId = (BrightSalesFacade.Property).CommonProperty.AccountId;
        //            //m_viewConfigId = viewconfigId;
        //            _dt = static_dt;
        //        }

        //        //WaitDialog.Show("Loading report ...");
        //        using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 })
        //        {
        //            view_configuration _eftViewConfig = _efDbModel.view_configuration.FirstOrDefault(i => i.id == m_viewConfigId);
        //            _efDbModel.Detach(_eftViewConfig);

        //            if (_eftViewConfig == null || _eftViewConfig.report_layout_config == null)
        //            {
        //                WaitDialog.Close();
        //                NotificationDialog.Information("Bright Sales", "No layout available for this view.");
        //                return;
        //            }

        //            if (string.IsNullOrEmpty(_eftViewConfig.report_data_config))
        //            {
        //                WaitDialog.Close();
        //                NotificationDialog.Information("Bright Sales", "No parameter layout has been set for this report.");
        //                return;
        //            }

        //            var templateData = SerializeUtility.DeserializeFromXml<TemplateProperty>(_eftViewConfig.report_data_config);
        //            ReportDataSet getReportDataSet = this.GetReportDataSet(m_viewConfigId, templateData, _eftViewConfig.name, _eftViewConfig.xml_config, pAccountId, _dt);

        //            //DAN: https://brightvision.jira.com/browse/PLATFORM-2457
        //            //- Implement to sort report when grid has been sorted out
        //            //---------------------------------------------------------------------------------------
        //            var sortExpression = GetSortExpression(gridView1);
        //            if (sortExpression != "")
        //            {
        //                string[] arrSortEx = sortExpression.Split('|');
        //                string tableFieldName = GetTableFieldName(arrSortEx[0]);

        //                if (tableFieldName != "")
        //                {
        //                    string tableName = tableFieldName.Split('|')[0];
        //                    string fieldName = tableFieldName.Split('|')[1];
        //                    ReportDataSet reportDatasetTemp = new ReportDataSet();

        //                    /*var sortedRows = (from myRow in getReportDataSet.Tables["account"].AsEnumerable()
        //                                      orderby myRow["company_name"] descending
        //                                      select myRow).ToArray();*/
        //                    //DAN: Copy to DataTable first as it causes error during linq when original table has been filter/deleted rows.
        //                    DataTable dt = getReportDataSet.Tables[tableName].DefaultView.ToTable();
        //                    var sortedRows = (from myRow in dt.AsEnumerable()
        //                                      orderby myRow[fieldName] ascending
        //                                      select myRow).ToArray();

        //                    if (arrSortEx[1] == "DESC")
        //                    {
        //                        sortedRows = (from myRow in dt.AsEnumerable()
        //                                      orderby myRow[fieldName] descending
        //                                      select myRow).ToArray();
        //                    }

        //                    DataTable sortedCompany = sortedRows.CopyToDataTable();

        //                    foreach (DataRow dr in sortedCompany.Rows)
        //                    {
        //                        reportDatasetTemp.Tables["account"].Rows.Add(dr.ItemArray);
        //                    }

        //                    for (int i = 1; i < getReportDataSet.Tables.Count; i++)
        //                    {
        //                        dt.Clear();
        //                        dt.Dispose();
        //                        dt = null;
        //                        dt = new DataTable();
        //                        dt = getReportDataSet.Tables[i].DefaultView.ToTable();

        //                        sortedRows = (from myRow in dt.AsEnumerable()
        //                                      select myRow).ToArray();
        //                        if (sortedRows.Count() == 0) continue;
        //                        sortedCompany = sortedRows.CopyToDataTable();

        //                        foreach (DataRow dr in sortedCompany.Rows)
        //                        {
        //                            reportDatasetTemp.Tables[i].Rows.Add(dr.ItemArray);
        //                        }
        //                    }

        //                    templateData.StatisticsDataSource = GetFilteredData();
        //                    string template = _eftViewConfig.report_layout_config;
        //                    if (string.IsNullOrEmpty(template))
        //                        template = ToString(new XtraReportDefaultTemplate());

        //                    XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate(template, templateData) { DataSource = reportDatasetTemp };
        //                    _report.ShowPreview();
        //                }
        //                //----------------------------------------------------------------------------------------------
        //            }
        //            else
        //            {
        //                templateData.StatisticsDataSource = GetFilteredData();
        //                string template = _eftViewConfig.report_layout_config;
        //                if (string.IsNullOrEmpty(template))
        //                    template = ToString(new XtraReportDefaultTemplate());

        //                XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate(template, templateData) { DataSource = getReportDataSet };
        //                _report.ShowPreview();
        //            }


        //        }
        //        //WaitDialog.Close();
        //    }
        //    public string CreatePDF()
        //    {
        //        string filename = "";
        //        int pAccountId = (BrightSalesFacade.Property).CommonProperty.AccountId;

        //        WaitDialog.Show("Loading report ...");
        //        using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 })
        //        {
        //            view_configuration _eftViewConfig = _efDbModel.view_configuration.FirstOrDefault(i => i.id == m_viewConfigId);
        //            _efDbModel.Detach(_eftViewConfig);

        //            if (_eftViewConfig == null || _eftViewConfig.report_layout_config == null)
        //            {
        //                WaitDialog.Close();
        //                NotificationDialog.Information("Bright Sales", "No layout available for this view.");
        //                return "";
        //            }

        //            if (string.IsNullOrEmpty(_eftViewConfig.report_data_config))
        //            {
        //                WaitDialog.Close();
        //                NotificationDialog.Information("Bright Sales", "No parameter layout has been set for this report.");
        //                return "";
        //            }

        //            var templateData = SerializeUtility.DeserializeFromXml<TemplateProperty>(_eftViewConfig.report_data_config);
        //            ReportDataSet getReportDataSet = this.GetReportDataSet(m_viewConfigId, templateData, _eftViewConfig.name, _eftViewConfig.xml_config, pAccountId, static_dt);

        //            //templateData.StatisticsDataSource = GetFilteredData();
        //            //string template = _eftViewConfig.report_layout_config;
        //            //if (string.IsNullOrEmpty(template))
        //            //    template = ToString(new XtraReportDefaultTemplate());

        //            //XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate(template, templateData) { DataSource = getReportDataSet };
        //            ////_report.ShowPreview();
        //            //filename = XtraReportDefaultTemplate.SavePDF(_report, getReportDataSet);

        //            XtraReportDefaultTemplate _report = new XtraReportDefaultTemplate();
        //            var result = templateData.DynamicProperty.Where(q => q.Type == TemplateDynamicType.Statistics);
        //            if (templateData.DynamicProperty != null && templateData.DynamicProperty.Count > 0)
        //                templateData.DynamicProperty.RemoveAll(param => param.Type == TemplateDynamicType.Statistics);
        //            string template = _eftViewConfig.report_layout_config;
        //            if (template == null)
        //                template = BrightVision.Reporting.Business.FacadeReportTemplate.GetDefaultReportLayout();

        //            _report = new XtraReportDefaultTemplate(template);
        //            filename = XtraReportDefaultTemplate.SavePDF(_report, getReportDataSet);

        //        }
        //        WaitDialog.Close();

        //        return filename;
        //    }
        //    #endregion

        //    #region Private Methods
        //    public string ToString(XtraReportDefaultTemplate template)
        //    {
        //        var ms = new MemoryStream();
        //        //xreport.SaveLayoutToXml(ms);
        //        template.SaveLayout(ms);
        //        ms.Position = 0;

        //        var sr = new StreamReader(ms, Encoding.Default);
        //        string _reportTemplate = sr.ReadToEnd();
        //        return _reportTemplate;
        //    }
        //    public void HideLoadingInfo()
        //    {
        //        this.layoutControlGroup1.Remove(emptySpaceItemLoding);
        //    }
        //    private DataTable GetFilteredData()
        //    {
        //        DataTable table = new DataTable();
        //        foreach (GridColumn item in this.gridView1.Columns)
        //        {
        //            if (item.Visible || (item.FieldName == "accountid" || item.FieldName == "contactid"))
        //            {
        //                table.Columns.Add(item.FieldName);
        //            }
        //        }

        //        //int rowCount = this.gridView1.DataRowCount;
        //        //for (int cnt = 0; cnt < rowCount; cnt++) {
        //        //    table.Rows.Add((this.gridView1.GetRow(cnt) as DataRowView).Row.ItemArray);
        //        //}

        //        int rowCount = this.gridView1.DataRowCount;
        //        for (int cnt = 0; cnt < rowCount; cnt++)
        //        {
        //            DataRow newrow = table.NewRow();
        //            var datarow = (this.gridView1.GetRow(cnt) as DataRowView).Row;
        //            foreach (GridColumn item in this.gridView1.Columns)
        //            {
        //                if (item.Visible || (item.FieldName == "accountid" || item.FieldName == "contactid"))
        //                {
        //                    newrow[item.FieldName] = datarow[item.FieldName];
        //                }
        //            }
        //            table.Rows.Add(newrow);
        //        }
        //        return table;
        //    }
        //    private ReportDataSet GetReportDataSet(int viewid, TemplateProperty templateData, string pConfigName, string pXmlConfig, int pAccountId = 0, DataTable dt = null)
        //    {
        //        ReportDataSet reportDataset = new ReportDataSet();
        //        DataTable filterdDataSource = GetFilteredData();
        //        if (dt != null) filterdDataSource = dt;

        //        if (m_viewType == eViewType.AccountsContactsWithDialogData)
        //            reportDataset = ReportDataSet.GetReportDataset(viewid, m_customerId, ReportDataSet.eViewType.AccountsContactsWithDialogData, pAccountId);
        //        else if (m_viewType == eViewType.AccountsContactsWithCallAttempts)
        //            reportDataset = ReportDataSet.GetReportDataset(viewid, m_customerId, ReportDataSet.eViewType.AccountsContactsWithCallAttempts, pAccountId);

        //        reportDataset = FilterByGridData(reportDataset);
        //        PopulateClientInfo(ref reportDataset, pConfigName, pAccountId > 0? true: false);
        //        PopulateAccountDynamic(ref reportDataset, filterdDataSource, templateData, pXmlConfig);
        //        PopulateContactDynamic(ref reportDataset, filterdDataSource, templateData, pXmlConfig);
        //        PopulateAccountStatic(ref reportDataset, filterdDataSource, pXmlConfig);
        //        PopulateContactStatic(ref reportDataset, filterdDataSource, pXmlConfig);

        //        //int accountNumRows = dset.Tables["Account"].Rows.Count;
        //        //int contactNumRows = dset.Tables["Contact"].Rows.Count;
        //        //for (int cnt1 = 0; cnt1 < accountNumRows; cnt1++)
        //        //{
        //        //    int id = int.Parse(dset.Tables["Account"].Rows[cnt1]["Id"].ToString());
        //        //    for (int cnt2 = 0; cnt2 < dset.Tables["Account"].Columns.Count; cnt2++)
        //        //    {
        //        //        string columnName = dset.Tables["Account"].Columns[cnt2].ColumnName;
        //        //        object colRowValue = dset.Tables["Account"].Rows[cnt1][columnName];
        //        //        if (columnName != "Id")
        //        //        {
        //        //            reportDataset.accountdynamic.Rows.Add(id, columnName, colRowValue);
        //        //        }
        //        //    }
        //        //}
        //        //for (int cnt1 = 0; cnt1 < contactNumRows; cnt1++)
        //        //{
        //        //    int id = int.Parse(dset.Tables["Contact"].Rows[cnt1]["Id"].ToString());
        //        //    for (int cnt2 = 0; cnt2 < dset.Tables["Contact"].Columns.Count; cnt2++)
        //        //    {
        //        //        string columnName = dset.Tables["Contact"].Columns[cnt2].ColumnName;
        //        //        object colRowValue = dset.Tables["Contact"].Rows[cnt1][columnName];
        //        //        if (columnName != "Id" && columnName != "AccountId")
        //        //        {
        //        //            reportDataset.contactdynamic.Rows.Add(id, columnName, colRowValue);
        //        //        }
        //        //    }
        //        //}

        //        ///**
        //        // * fill-out client information.
        //        // */
        //        //BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        //        //subcampaign _efeSubCampaign = _efDbModel.subcampaigns.FirstOrDefault(i => i.id == m_subCampaignId);
        //        //string _campaignName = _efDbModel.campaigns.FirstOrDefault(i => i.id == _efeSubCampaign.campaign_id).campaign_name;
        //        //string _customerName = _efDbModel.customers.FirstOrDefault(i => i.id == m_customerId).customer_name;
        //        //reportDataset.clientinfo.Rows.Add(
        //        //    _customerName,
        //        //    _campaignName,
        //        //    _efeSubCampaign.title,
        //        //    UserSession.CurrentUser.UserFullName,
        //        //    DateTime.Now.ToShortDateString(),
        //        //    m_viewConfigName
        //        //);

        //        return reportDataset;

        //    }
        //    private ReportDataSet FilterByGridData(ReportDataSet reportDataset)
        //    {
        //        int rowCount = this.gridView1.DataRowCount;
        //        string accountFilter = string.Empty;
        //        string contactFilter = string.Empty;
        //        List<int> listAccount = new List<int>();
        //        List<int> listContact = new List<int>();

        //        #region get the filter of account and contact
        //        for (int cnt = 0; cnt < rowCount; cnt++)
        //        {
        //            var rowview = this.gridView1.GetRow(cnt) as DataRowView;
        //            int accountId = int.Parse(rowview.Row["accountid"].ToString());
        //            int contactid = int.Parse(rowview.Row["contactid"].ToString());
        //            if (listAccount.Where(param => param == accountId).Count() == 0)
        //            {
        //                listAccount.Add(accountId);
        //            }
        //            if (listContact.Where(param => param == contactid).Count() == 0)
        //            {
        //                listContact.Add(contactid);
        //            }
        //        }
        //        foreach (var ac in listAccount)
        //        {
        //            accountFilter += ac + ",";
        //        }
        //        foreach (var c in listContact)
        //        {
        //            contactFilter += c + ",";
        //        }


        //        #endregion

        //        if (accountFilter != string.Empty)
        //        {
        //            accountFilter = accountFilter.Substring(0, accountFilter.Length - 1);
        //            var toDeleteAccount = reportDataset.account.Select(String.Format("account_id NOT IN ({0})", accountFilter));
        //            foreach (var tmp in toDeleteAccount)
        //            {
        //                tmp.Delete();
        //            }
        //        }
        //        if (contactFilter != string.Empty)
        //        {
        //            contactFilter = contactFilter.Substring(0, contactFilter.Length - 1);
        //            var toDeleteContact = reportDataset.contact.Select(String.Format("contact_id NOT IN ({0})", contactFilter));
        //            foreach (var tmp in toDeleteContact)
        //            {
        //                tmp.Delete();
        //            }
        //        }


        //        return reportDataset;
        //    }
        //    private void PopulateClientInfo(ref ReportDataSet reportDataset, string pConfigName, bool pIsSpecificAccount = false)
        //    {
        //        string customerName = string.Empty;
        //        string campaignName = string.Empty;
        //        string efeSubCampaignName = string.Empty;
        //        string _efeSubCampaignListname = string.Empty;

        //        if (!pIsSpecificAccount) {
        //            XElement xmlConfigWithData = XElement.Parse(XML_ConfigWithData);

        //            /**
        //             * [@jeff 06.26.2012]: https://brightvision.jira.com/browse/PLATFORM-1527
        //             * added validation for null objects.
        //             */

        //            if (xmlConfigWithData.XPathSelectElement("relation/customer") != null &&
        //                xmlConfigWithData.XPathSelectElement("relation/customer").Attribute("name").Value != null)
        //                customerName = xmlConfigWithData.XPathSelectElement("relation/customer").Attribute("name").Value;

        //            if (xmlConfigWithData.XPathSelectElement("relation/campaign") != null &&
        //                xmlConfigWithData.XPathSelectElement("relation/campaign").Attribute("name").Value != null)
        //                campaignName = xmlConfigWithData.XPathSelectElement("relation/campaign").Attribute("name").Value;

        //            if (xmlConfigWithData.XPathSelectElement("relation/subcampaign") != null &&
        //                xmlConfigWithData.XPathSelectElement("relation/subcampaign").Attribute("name").Value != null)
        //                efeSubCampaignName = xmlConfigWithData.XPathSelectElement("relation/subcampaign").Attribute("name").Value;

        //            if (xmlConfigWithData.XPathSelectElement("relation/dialog") != null &&
        //                xmlConfigWithData.XPathSelectElement("relation/dialog").Attribute("list_source_name").Value != null)
        //                _efeSubCampaignListname = xmlConfigWithData.XPathSelectElement("relation/dialog").Attribute("list_source_name").Value;
        //        }
        //        else {
        //            customerName = m_BrightSalesProperty.CommonProperty.SubCampaignName;
        //            campaignName = m_BrightSalesProperty.CommonProperty.CampaignName;
        //            efeSubCampaignName = m_BrightSalesProperty.CommonProperty.SubCampaignName;

        //            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
        //                sub_campaign_account_lists _eftData = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
        //                    i.account_id == m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId &&
        //                    i.final_list_id == m_BrightSalesProperty.CommonProperty.FinalListId
        //                );
        //                if (_eftData != null && string.IsNullOrEmpty(_eftData.list_source))
        //                    _efeSubCampaignListname = _eftData.list_source;

        //                m_viewConfigName = pConfigName;
        //                _efDbContext.Detach(_eftData);
        //            }
        //        }

        //        reportDataset.clientinfo.Rows.Add(
        //            customerName,
        //            campaignName,
        //            efeSubCampaignName,
        //            UserSession.CurrentUser.UserFullName,
        //            DateTime.Now.ToShortDateString(),
        //            m_viewConfigName,
        //            _efeSubCampaignListname
        //        );
        //    }
        //    private void PopulateContactStatic(ref ReportDataSet dataset, DataTable datasource, string pXmlConfig = "")
        //    {
        //        var config = string.IsNullOrEmpty(pXmlConfig) ? XElement.Parse(XML_Config) : XElement.Parse(pXmlConfig);
        //        for (int rowCount = 0; rowCount < datasource.Rows.Count; rowCount++)
        //        {
        //            for (int colCount = 0; colCount < datasource.Columns.Count; colCount++)
        //            {
        //                string colName = datasource.Columns[colCount].ColumnName;
        //                if (colName == "accountid" || colName == "contactid")
        //                    continue;
        //                // get the source base on the column name
        //                // the columnname is the display name in the xml_config
        //                var filter = string.Format("item[display_name='{0}']", colName);
        //                var item = config.XPathSelectElement(filter);
        //                var xsource = item.XPathSelectElements("source").FirstOrDefault();
        //                var xfieldName = item.XPathSelectElements("field_name").FirstOrDefault();
        //                string source = string.Empty;
        //                string fieldName = string.Empty;
        //                if (xsource != null && xfieldName != null)
        //                {
        //                    source = xsource.Value;
        //                    fieldName = xfieldName.Value;
        //                }
        //                else
        //                {
        //                    continue;
        //                }

        //                //AccountMerge and Dialog Account Level source is added to accountdynamic table
        //                if (source == "General" && (fieldName == "DialogCreatedBy" ||
        //                    fieldName == "DialogCreatedDate" || fieldName == "DialogStatus" ||
        //                    fieldName == "ContactLastChanged" || fieldName == "ContactStatusLastChanged"))
        //                {
        //                      int contactid = int.Parse(datasource.Rows[rowCount]["contactid"].ToString());
        //                    DataRow[] existColName = dataset.contactstatic.Select(string.Format("contactid={0} and name='{1}'", contactid, colName));

        //                    if (existColName.Count() < 1)
        //                    {
        //                        var newContactStatic = dataset.contactstatic.NewcontactstaticRow();
        //                        newContactStatic.contactid = int.Parse(datasource.Rows[rowCount]["contactid"].ToString());
        //                        newContactStatic.name = colName;
        //                        newContactStatic.value = datasource.Rows[rowCount][colCount].ToString();
        //                        dataset.contactstatic.AddcontactstaticRow(newContactStatic);
        //                    }
        //                }

        //            }
        //        }
        //    }
        //    private void PopulateAccountStatic(ref ReportDataSet dataset, DataTable datasource, string pXmlConfig = "")
        //    {
        //        var config = string.IsNullOrEmpty(pXmlConfig) ? XElement.Parse(XML_Config) : XElement.Parse(pXmlConfig);
        //        for (int rowCount = 0; rowCount < datasource.Rows.Count; rowCount++)
        //        {
        //            for (int colCount = 0; colCount < datasource.Columns.Count; colCount++)
        //            {
        //                string colName = datasource.Columns[colCount].ColumnName;
        //                if (colName == "accountid" || colName == "contactid")
        //                    continue;
        //                // get the source base on the column name
        //                // the columnname is the display name in the xml_config
        //                var filter = string.Format("item[display_name='{0}']", colName);
        //                var item = config.XPathSelectElement(filter);
        //                var xsource = item.XPathSelectElements("source").FirstOrDefault();
        //                var xfieldName = item.XPathSelectElements("field_name").FirstOrDefault();
        //                string source = string.Empty;
        //                string fieldName = string.Empty;
        //                if (xsource != null && xfieldName != null)
        //                {
        //                    source = xsource.Value;
        //                    fieldName = xfieldName.Value;
        //                }
        //                else
        //                {
        //                    continue;
        //                }

        //                //AccountMerge and Dialog Account Level source is added to accountdynamic table
        //                if (source == "General" && (fieldName == "CompanyLeadStatus" ||
        //                    fieldName == "CompanyStatus" || fieldName == "CompanyLastChanged" ||
        //                    fieldName == "CompanyStatusLastChanged" || fieldName == "AccountSubCampaignCallAttempts"))
        //                {
        //                     int accountid = int.Parse(datasource.Rows[rowCount]["accountid"].ToString());
        //                    DataRow[] existColName = dataset.accountstatic.Select(string.Format("accountid={0} and name='{1}'", accountid, colName));

        //                    if (existColName.Count() < 1)
        //                    {
        //                        try
        //                        {
        //                            var newAccountStatic = dataset.accountstatic.NewaccountstaticRow();
        //                            newAccountStatic.accountid = int.Parse(datasource.Rows[rowCount]["accountid"].ToString());
        //                            newAccountStatic.name = colName;
        //                            newAccountStatic.value = datasource.Rows[rowCount][colCount].ToString();
        //                            dataset.accountstatic.AddaccountstaticRow(newAccountStatic);
        //                        }
        //                        catch { }
        //                    }
        //                }

        //            }
        //        }
        //    }
        //    private void PopulateContactDynamic(ref ReportDataSet dataset, DataTable datasource, TemplateProperty templateData, string pXmlConfig = "")
        //    {
        //        var config = string.IsNullOrEmpty(pXmlConfig) ? XElement.Parse(XML_Config) : XElement.Parse(pXmlConfig);
        //        for (int rowCount = 0; rowCount < datasource.Rows.Count; rowCount++)
        //        {
        //            for (int colCount = 0; colCount < datasource.Columns.Count; colCount++)
        //            {
        //                string colName = datasource.Columns[colCount].ColumnName;
        //                if (colName == "accountid" || colName == "contactid")
        //                    continue;
        //                // get the source base on the column name
        //                // the columnname is the display name in the xml_config
        //                var filter = string.Format("item[display_name='{0}']", colName);
        //                var item = config.XPathSelectElement(filter);
        //                var xsource = item.XPathSelectElements("source").FirstOrDefault();
        //                string source = string.Empty;
        //                if (xsource != null)
        //                {
        //                    source = xsource.Value;
        //                }
        //                else
        //                {
        //                    var lbl = item.XPathSelectElement("label_name");
        //                    if (lbl.Value == "EMPTY") continue;
        //                    var mergeitem = item.XPathSelectElements("merge_data").First();
        //                    var innerItem = XElement.Parse(mergeitem.Value);
        //                    var mergeContactItem = innerItem.XPathSelectElements("//item[source='Contact' or source='Dialog Contact Level']");
        //                    if (mergeContactItem.Count() > 0)
        //                        source = "ContactMerge";
        //                }
        //                int contactid = int.Parse(datasource.Rows[rowCount]["contactid"].ToString()); ;
        //                var datarowDynamic = dataset.contactdynamic.Select(string.Format("contactid={0} and name='{1}'", contactid, colName));
        //                //AccountMerge and Dialog Account Level source is added to accountdynamic table
        //                if (datarowDynamic.Count() == 0 && (source == "ContactMerge" || source == "Dialog Contact Level"))
        //                {
        //                    var newContactDynamic = dataset.contactdynamic.NewcontactdynamicRow();
        //                    newContactDynamic.contactid = int.Parse(datasource.Rows[rowCount]["contactid"].ToString());
        //                    newContactDynamic.name = colName;
        //                    newContactDynamic.value = datasource.Rows[rowCount][colCount].ToString();
        //                    if (templateData.IsEmptyDynamicValueVisible)
        //                    {
        //                        dataset.contactdynamic.AddcontactdynamicRow(newContactDynamic);
        //                    }
        //                    else if (!templateData.IsEmptyDynamicValueVisible && !string.IsNullOrWhiteSpace(newContactDynamic.value))
        //                    {
        //                        dataset.contactdynamic.AddcontactdynamicRow(newContactDynamic);
        //                    }
        //                }

        //            }
        //        }
        //    }
        //    private void PopulateAccountDynamic(ref ReportDataSet dataset, DataTable datasource,TemplateProperty templateData, string pXmlConfig = "")
        //    {

        //        var config = string.IsNullOrEmpty(pXmlConfig) ? XElement.Parse(XML_Config) : XElement.Parse(pXmlConfig);
        //            for (int rowCount = 0; rowCount < datasource.Rows.Count; rowCount++)
        //            {
        //                for (int colCount = 0; colCount < datasource.Columns.Count; colCount++)
        //                {
        //                    try
        //                    {
        //                        string colName = datasource.Columns[colCount].ColumnName;
        //                        if (colName == "accountid" || colName == "contactid")
        //                            continue;
        //                        // get the source base on the column name
        //                        // the columnname is the display name in the xml_config
        //                        var filter = string.Format("item[display_name='{0}']", colName);
        //                        var item = config.XPathSelectElement(filter);
        //                        var xsource = item.XPathSelectElements("source").FirstOrDefault();
        //                        string source = string.Empty;
        //                        if (xsource != null)
        //                        {
        //                            source = xsource.Value;
        //                        }
        //                        else
        //                        {
        //                            var lbl = item.XPathSelectElement("label_name");
        //                            if (lbl.Value == "EMPTY") continue;
        //                            var mergeitem = item.XPathSelectElements("merge_data").First();
        //                            var innerItem = XElement.Parse(mergeitem.Value);
        //                            var mergeContactItem = innerItem.XPathSelectElements("//item[source='Contact' or source='Dialog Contact Level']");
        //                            if (mergeContactItem.Count() == 0)
        //                                source = "AccountMerge";
        //                        }
        //                        int accountid = int.Parse(datasource.Rows[rowCount]["accountid"].ToString()); ;
        //                        var datarowDynamic = dataset.Tables["accountdynamic"].Select(string.Format("accountid={0} and name='{1}'", accountid, colName));
        //                        //AccountMerge and Dialog Account Level source is added to accountdynamic table
        //                        if (datarowDynamic.Count() == 0 && (source == "AccountMerge" || source == "Dialog Account Level"))
        //                        {
        //                            var newAccountDynamic = dataset.accountdynamic.NewaccountdynamicRow();
        //                            newAccountDynamic.accountid = accountid;
        //                            newAccountDynamic.name = colName;
        //                            newAccountDynamic.value = datasource.Rows[rowCount][colCount].ToString();
        //                            if (templateData.IsEmptyDynamicValueVisible)
        //                            {
        //                                dataset.accountdynamic.AddaccountdynamicRow(newAccountDynamic);
        //                            }
        //                            else if (!templateData.IsEmptyDynamicValueVisible && !string.IsNullOrWhiteSpace(newAccountDynamic.value))
        //                            {
        //                                dataset.accountdynamic.AddaccountdynamicRow(newAccountDynamic);
        //                            }
        //                        }
        //                    }
        //                    catch { }
        //                }
                   
        //        }
              
        //    }
        //    private void FillDisplayView()
        //    {
        //        Action _action = delegate {
        //            gridControl1.DataSource = null;
        //            gridControl1.DataSource = dtSource.Clone();

        //            GridColumn _column = null;
        //            emptySpaceItemLoding.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
        //            RepositoryItemMemoEdit _tbxColumn = new RepositoryItemMemoEdit() {
        //                WordWrap = true
        //            };

        //            gridView1.GridControl.RepositoryItems.Add(_tbxColumn);

        //            if (gridView1.Columns.Count > 0) {
        //                gridView1.OptionsView.RowAutoHeight = true;
        //                gridView1.OptionsView.ColumnAutoWidth = false;
        //                gridView1.OptionsSelection.MultiSelect = true;
        //                gridView1.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;

        //                for (int i = 0; i < gridView1.Columns.Count; i++) {
        //                    _column = gridView1.Columns[i];
        //                    _column.ColumnEdit = _tbxColumn;
        //                    _column.Width = 300;
        //                    _column.MaxWidth = 500;
        //                    _column.MinWidth = 100;
        //                    _column.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
        //                    _column.OptionsColumn.AllowEdit = false;
        //                    _column.OptionsColumn.AllowFocus = true;
        //                    _column.OptionsColumn.FixedWidth = false;
        //                    if (_column.FieldName == "accountid" || _column.FieldName == "contactid")
        //                        _column.Visible = false;
        //                    //gc.BestFit();        
        //                }

        //                //gridView1.BestFitColumns();
        //                //gridView1.LeftCoord = 0;
        //            }

        //            gridControl1.DataSource = dtSource;
        //            gridView1.LeftCoord = 0;
        //            static_dt = dtSource;
        //        };

        //        if (gridControl1.InvokeRequired)
        //            gridControl1.BeginInvoke(_action);
        //        else
        //            _action();
        //    }
        //    #endregion

        //    #region Commented Codes
        //    //private class ViewTab : DevExpress.XtraTab.XtraTabPage {

        //    //    #region Member Variables
        //    //    private LayoutControl layoutControl;
        //    //    private LayoutControlGroup layoutControlGroup1;            
        //    //    private LayoutControlItem layoutControlItem1;
        //    //    private LayoutControlItem layoutControlItem2;
        //    //    private EmptySpaceItem emptySpaceItem1;
        //    //    private EmptySpaceItem emptySpaceItemLoding;
        //    //    private SimpleLabelItem simpleLabelItem;
        //    //    //private SimpleButton simpleButton1;
        //    //    //private SimpleButton simpleButton2;
        //    //    //private SimpleButton simpleButton3;
        //    //    //private SimpleButton simpleButton4;
        //    //    private GridControl gridControl1;
        //    //    private GridView gridView1;
        //    //    private DataTable dtSource = null;
        //    //    #endregion

        //    //    #region Constructor
        //    //    public ViewTab(string groupName)
        //    //        : base() {                    
        //    //            InitializeComponent();
        //    //            this.Text = groupName;
        //    //            this.ImageIndex = 0;
        //    //    }
        //    //    public void InitializeComponent() {
        //    //        layoutControl = new LayoutControl();
        //    //        layoutControl.Name = "layoutControl" + Guid.NewGuid().ToString();
        //    //        layoutControl.Dock = DockStyle.Fill;
        //    //        this.Controls.Add(layoutControl);   

        //    //        this.layoutControlGroup1 = new LayoutControlGroup();                
        //    //        this.layoutControlGroup1.Name = "layoutControlGroup" + Guid.NewGuid().ToString();
        //    //        this.layoutControlGroup1.Text = this.Text;
        //    //        this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
        //    //        this.layoutControlGroup1.ShowInCustomizationForm = false;

        //    //        //Loading datasource
        //    //        this.emptySpaceItemLoding = new EmptySpaceItem();
        //    //        this.emptySpaceItemLoding.Text = "Loading view display. Please wait...";
        //    //        this.emptySpaceItemLoding.AppearanceItemCaption.Font = new Font("Arial", 10f, FontStyle.Bold);
        //    //        this.emptySpaceItemLoding.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        //    //        this.emptySpaceItemLoding.TextVisible = true;
        //    //        this.emptySpaceItemLoding.Size = new Size(100, 20);
        //    //        this.emptySpaceItemLoding.MaxSize = new Size(100, 20);
        //    //        this.emptySpaceItemLoding.MinSize = new Size(100, 20);
        //    //        this.emptySpaceItemLoding.SizeConstraintsType = SizeConstraintsType.Custom;
        //    //        this.layoutControlGroup1.AddItem(emptySpaceItemLoding);                


        //    //        this.gridView1 = new GridView();
        //    //        this.gridView1.Name = "gridView" + Guid.NewGuid().ToString();
        //    //        this.gridView1.GridControl = this.gridControl1;                
        //    //        this.gridView1.OptionsFind.AlwaysVisible = false;
        //    //        this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
        //    //        this.gridView1.OptionsBehavior.AutoPopulateColumns = true;
        //    //        this.gridView1.OptionsSelection.MultiSelect = false;
        //    //        this.gridView1.OptionsView.ShowGroupPanel = false;
        //    //        this.gridView1.OptionsView.ColumnAutoWidth = false;
        //    //        this.gridView1.OptionsBehavior.Editable = false;
        //    //        this.gridView1.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(gridView1_PopupMenuShowing);

        //    //        this.gridControl1 = new GridControl();
        //    //        this.gridControl1.Name = "gridControl" + Guid.NewGuid().ToString();
        //    //        this.gridControl1.MainView = this.gridView1;                
        //    //        this.gridControl1.ViewCollection.AddRange(new BaseView[] { this.gridView1 });                

        //    //        //this.simpleButton1 = new SimpleButton();
        //    //        //this.simpleButton1.Name = "simpleButton" + Guid.NewGuid().ToString();
        //    //        //this.simpleButton1.Text = "Show in Dialog";
        //    //        //this.simpleButton1.Size = new System.Drawing.Size(150, 22);
        //    //        //this.simpleButton1.Click += new EventHandler(simpleButton1_Click);

        //    //        //this.simpleButton2 = new SimpleButton();
        //    //        //this.simpleButton2.Name = "simpleButton" + Guid.NewGuid().ToString();
        //    //        //this.simpleButton2.Text = "Export to Excel";
        //    //        //this.simpleButton2.Size = new System.Drawing.Size(150, 22);
        //    //        //this.simpleButton2.Enabled = false;
        //    //        //this.simpleButton2.Click += new EventHandler(simpleButton2_Click);

        //    //        //this.simpleButton3 = new SimpleButton();
        //    //        //this.simpleButton3.Name = "simpleButton" + Guid.NewGuid().ToString();
        //    //        //this.simpleButton3.Text = "Export to PDF";
        //    //        //this.simpleButton3.Size = new System.Drawing.Size(150, 22);
        //    //        //this.simpleButton3.Click += new EventHandler(simpleButton1_Click);

        //    //        //this.simpleButton4 = new SimpleButton();
        //    //        //this.simpleButton4.Name = "simpleButton" + Guid.NewGuid().ToString();
        //    //        //this.simpleButton4.Text = "Show in Dialog";
        //    //        //this.simpleButton4.Size = new System.Drawing.Size(150, 22);
        //    //        //this.simpleButton4.Click += new EventHandler(simpleButton1_Click);

        //    //    }


        //    //    #endregion

        //    //    #region Properties

        //    //    public ViewCofigData ConfigData { get; set; }

        //    //    public DataTable DataSource { 
        //    //        get { return dtSource; } 
        //    //        set {                                         
        //    //            dtSource = value;
        //    //            Action action = delegate {
        //    //                gridControl1.DataSource = null;
        //    //                gridControl1.DataSource = dtSource;
        //    //                //simpleButton2.Enabled = true;
        //    //                GridColumn gc = null;
        //    //                emptySpaceItemLoding.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
        //    //                var memoedit = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
        //    //                memoedit.WordWrap = true;
        //    //                gridView1.GridControl.RepositoryItems.Add(memoedit);
        //    //                if (gridView1.Columns.Count > 0) {
        //    //                    for (int x = 0; x < gridView1.Columns.Count; ++x) {
        //    //                        gc = gridView1.Columns[x];
        //    //                        if (gc.FieldName.ToLower().Contains("smarttext") || 
        //    //                            gc.FieldName.ToLower().Contains("smart text") || 
        //    //                            gc.FieldName.ToLower().Contains("comment")){
        //    //                                gc.ColumnEdit = memoedit;
        //    //                                gridView1.OptionsView.RowAutoHeight = true;
        //    //                        }
        //    //                        gc = gridView1.Columns[x];
        //    //                        gc.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
        //    //                        gc.OptionsColumn.AllowEdit = false;
        //    //                        gc.OptionsColumn.AllowFocus = false;
        //    //                        gc.OptionsColumn.FixedWidth = false;
        //    //                        gc.BestFit();        
        //    //                    }
        //    //                    gridView1.OptionsView.ColumnAutoWidth = false;
        //    //                    gridView1.OptionsSelection.MultiSelect = true;
        //    //                    gridView1.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
        //    //                    gridView1.BestFitColumns();
        //    //                    gridView1.LeftCoord = 0;
        //    //                }
        //    //            };
        //    //            if (gridControl1.InvokeRequired) {
        //    //                gridControl1.BeginInvoke(action);
        //    //            } else {
        //    //                action();
        //    //            }            
        //    //        } 
        //    //    }

        //    //    #endregion

        //    //    #region Controllers
        //    //    public void Export(ViewExportType exportType) {
        //    //        SaveFileDialog dialog1 = new SaveFileDialog();
        //    //        if (exportType == ViewExportType.Excel2003) {
        //    //            dialog1.Filter = "Excel Workbook (*.xls)|*.xls";
        //    //        } else if (exportType == ViewExportType.Excel2007) {
        //    //            dialog1.Filter = "Excel Workbook (*.xslx)|*.xlsx";
        //    //        } else if (exportType == ViewExportType.CSV) {
        //    //            dialog1.Filter = "CSV (Comma Delimited) (*.csv)|*.csv";
        //    //        }

        //    //        dialog1.Title = "Save As";
        //    //        dialog1.CheckPathExists = true;
        //    //        dialog1.CheckFileExists = false;
        //    //        if (dialog1.ShowDialog() == DialogResult.OK) {
        //    //            if (dialog1.FileName != "") {
        //    //                if (dialog1.FilterIndex == 1) {
        //    //                    gridView1.OptionsPrint.AutoWidth = false;
        //    //                    gridView1.BestFitColumns();

        //    //                    FileStream fs = (FileStream)dialog1.OpenFile();
        //    //                    if (exportType == ViewExportType.CSV) {
        //    //                        DevExpress.XtraPrinting.CsvExportOptions opts = new DevExpress.XtraPrinting.CsvExportOptions();
        //    //                        gridView1.Export(DevExpress.XtraPrinting.ExportTarget.Csv, fs, opts);
        //    //                    } else if (exportType == ViewExportType.Excel2007) {
        //    //                        DevExpress.XtraPrinting.XlsxExportOptions opts = new DevExpress.XtraPrinting.XlsxExportOptions();
        //    //                        opts.ExportMode = DevExpress.XtraPrinting.XlsxExportMode.SingleFile;
        //    //                        opts.SheetName = "Sheet1";
        //    //                        gridControl1.ExportToXlsx(fs, opts);
        //    //                    } else if (exportType == ViewExportType.Excel2003) {
        //    //                        DevExpress.XtraPrinting.XlsExportOptions opts = new DevExpress.XtraPrinting.XlsExportOptions();
        //    //                        opts.ExportMode = DevExpress.XtraPrinting.XlsExportMode.SingleFile;
        //    //                        opts.SheetName = "Sheet1";
        //    //                        gridControl1.ExportToXls(fs, opts);
        //    //                    }
        //    //                    fs.Close();
        //    //                }

        //    //            }
        //    //        }
        //    //    }

        //    //    private void gridView1_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
        //    //        GridView view = sender as GridView;
        //    //        GridUtility.CreateGridContextMenu(view, e);
        //    //    }

        //    //    //private void simpleButton1_Click(object sender, EventArgs e) {
        //    //    //    MessageBox.Show("Coming soon...");
        //    //    //}
        //    //    //private void simpleButton2_Click(object sender, EventArgs e) {
        //    //    //    SaveFileDialog dialog1 = new SaveFileDialog();
        //    //    //    dialog1.Filter = "Excel Workbook|*.xlsx";
        //    //    //    dialog1.Title = "Save As";                
        //    //    //    dialog1.CheckPathExists = true;
        //    //    //    dialog1.CheckFileExists = false;                
        //    //    //    if (dialog1.ShowDialog() == DialogResult.OK) {
        //    //    //        if (dialog1.FileName != "") {                        
        //    //    //            if (dialog1.FilterIndex == 1) {
        //    //    //                FileStream fs = (FileStream) dialog1.OpenFile();

        //    //    //                DevExpress.XtraPrinting.XlsxExportOptions opts = new DevExpress.XtraPrinting.XlsxExportOptions();
        //    //    //                opts.ExportMode = DevExpress.XtraPrinting.XlsxExportMode.SingleFile;
        //    //    //                opts.SheetName = "Sheet1";
        //    //    //                gridView1.OptionsPrint.AutoWidth = false;
        //    //    //                gridView1.BestFitColumns();
        //    //    //                gridControl1.ExportToXlsx(fs, opts);
        //    //    //                fs.Close();
        //    //    //            }

        //    //    //        }
        //    //    //    }
        //    //    //    //MessageBox.Show("Coming soon...");
        //    //    //}
        //    //    #endregion

        //    //    #region Methods

        //    //    public void BindControls() {              
        //    //        this.layoutControl.BeginUpdate();   
        //    //        this.layoutControlItem1 = new LayoutControlItem();
        //    //        this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();
        //    //        this.layoutControlItem1.Control = gridControl1;
        //    //        this.layoutControlItem1.TextVisible = false;                
        //    //        this.layoutControlGroup1.AddItem(this.layoutControlItem1);

        //    //        //this.emptySpaceItem1 = new EmptySpaceItem();
        //    //        //this.layoutControlGroup1.AddItem(emptySpaceItem1);

        //    //        //this.layoutControlItem1 = new LayoutControlItem();
        //    //        //this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();
        //    //        //this.layoutControlItem1.Control = simpleButton1;
        //    //        //this.layoutControlItem1.SizeConstraintsType = SizeConstraintsType.Custom;
        //    //        //this.layoutControlItem1.MaxSize = new Size(100, 24);
        //    //        //this.layoutControlItem1.MinSize = new Size(80, 24);
        //    //        //this.layoutControlItem1.Size = new Size(80, 24);
        //    //        //this.layoutControlItem1.TextVisible = false;
        //    //        //this.layoutControlItem1.ShowInCustomizationForm = false;
        //    //        //this.layoutControlGroup1.AddItem(layoutControlItem1, emptySpaceItem1,DevExpress.XtraLayout.Utils.InsertType.Right);

        //    //        //this.layoutControlItem2 = new LayoutControlItem();
        //    //        //this.layoutControlItem2.Name = "layoutControlItem" + Guid.NewGuid().ToString();
        //    //        //this.layoutControlItem2.Control = simpleButton2;
        //    //        //this.layoutControlItem2.SizeConstraintsType = SizeConstraintsType.Custom;
        //    //        //this.layoutControlItem2.MaxSize = new Size(100, 24);
        //    //        //this.layoutControlItem2.MinSize = new Size(80, 24);
        //    //        //this.layoutControlItem2.Size = new Size(80, 24);
        //    //        //this.layoutControlItem2.TextVisible = false;
        //    //        //this.layoutControlItem2.ShowInCustomizationForm = false;                
        //    //        //this.layoutControlGroup1.AddItem(layoutControlItem2, emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Right);

        //    //        layoutControl.Root = layoutControlGroup1;
        //    //        layoutControl.Root.GroupBordersVisible = false;
        //    //        layoutControl.EndUpdate();
        //    //        this.Tag = this;
        //    //    }
        //    //    #endregion

        //    //}
        //    #endregion

        //    #region Table Field Name Values
        //    private class TableField
        //    {
        //        private string _table_field_name = null;

        //        public string table_field_name
        //        {
        //            set { _table_field_name = value; }
        //            get { return _table_field_name; }
        //        }
        //    }
        //    private Dictionary<string, TableField> tableFields = new Dictionary<string, TableField>();
        //    private void SetTableFields()
        //    {
        //        tableFields["a"] = new TableField();
        //        tableFields["a"].table_field_name = "table_field_name";
        //    }

        //    private string GetTableFieldName(string labelName)
        //    {
        //        ColumnView view = gridView1.SortedColumns.View;



        //        var config = XElement.Parse(XML_Config);
        //        var filter = string.Format("item[display_name='{0}']", labelName);
        //        var item = config.XPathSelectElement(filter);
        //        var xsource = item.XPathSelectElements("source").FirstOrDefault();
        //        var xfieldName = item.XPathSelectElements("field_name").FirstOrDefault();
        //        string source = string.Empty;
        //        string fieldName = string.Empty;
        //        if (xsource != null && xfieldName != null)
        //        {
        //            source = xsource.Value;
        //            fieldName = xfieldName.Value;

        //            if (xsource.Value == "Account" && xfieldName.Value == "CompanyName") return "account|company_name";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "OrgNo") return "account|org_no";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "YearEstablished") return "account|year_established";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "ParentCompany") return "account|parent_company";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Website") return "account|www";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Telephone") return "account|telephone";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Telefax") return "account|telefax";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Box") return "account|box_address";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Street") return "account|street_address";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "ZipCode") return "account|zip_code";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "City") return "account|city";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Country") return "account|country";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "County") return "account|county";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Municipality") return "account|municipality";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Region") return "account|regions";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "ActivityCode") return "account|activity_code_1";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "ActivityCode2") return "account|activity_code_2";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Currency") return "account|currency";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "FiscalYear1") return "account|fiscal_1";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Turnover1") return "account|turnover_1";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Export1") return "account|export_1";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Result1") return "account|result_1";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "SalesAbroad1") return "account|sales_abroad_1";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesAboad1") return "account|employees_abroad_1";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesTotal1") return "account|employees_total_1";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "FiscalYear2") return "account|fiscal_2";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Turnover2") return "account|turnover_2";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Export2") return "account|export_2";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Result2") return "account|result_2";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "SalesAbroad2") return "account|sales_abroad_2";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesAboad2") return "account|employees_abroad_2";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesTotal2") return "account|employees_total_2";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "FiscalYear3") return "account|fiscal_3";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Turnover3") return "account|turnover_3";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Export3") return "account|export_3";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Result3") return "account|result_3";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "SalesAbroad3") return "account|sales_abroad_3";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesAboad3") return "account|employees_abroad_3";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "EmployeesTotal3") return "account|employees_total_3";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Category") return "account|category";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "BVSource") return "account|bv_source";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "Priority") return "account|priority";
        //            else if (xsource.Value == "Account" && xfieldName.Value == "AssignedTo") return "account|assigned_to";

        //            else if (xsource.Value == "Contact" && xfieldName.Value == "Firstname") return "contact|first_name";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "Middlename") return "contact|middle_name";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "Lastname") return "contact|last_name";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "DirectPhone") return "contact|direct_phone";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "Mobile") return "contact|mobile";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "Email") return "contact|email";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "TitleNotConfirmed") return "contact|title_not_confirmed";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "Title") return "contact|title";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "Address 1") return "contact|address1";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "City") return "contact|city";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "ZipCode") return "contact|zip_code";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "Country") return "contact|country";
        //            else if (xsource.Value == "Contact" && xfieldName.Value == "Priority") return "contact|priority";

        //            else if (xsource.Value == "General" && xfieldName.Value == "DialogCreatedDate") return "contact|created_date";
        //            //else if (xsource.Value == "General" && xfieldName.Value == "DialogCreatedDate") return "contact|dialog_status";


        //        }

        //        return "";
        //    }

        //    public string GetSortExpression(ColumnView view)
        //    {
        //        string fields = "";
        //        foreach (GridColumnSortInfo info in view.SortInfo)
        //        {
        //            if (fields != "") fields += ";";
        //            fields += string.Format("{0}", info.Column.FieldName);

        //            if (info.SortOrder == DevExpress.Data.ColumnSortOrder.Descending)
        //                fields += "|DESC";
        //            else
        //                fields += "|ASC";
        //        }
        //        return fields;
        //    }


        //    //public string GetSortExpression(ColumnView view)
        //    //{
        //    //    var expression = String.Empty;
        //    //    foreach (GridColumnSortInfo info in view.SortInfo)
        //    //    {
        //    //        expression += string.Format("[{0}]", info.Column.FieldName);

        //    //        if (info.SortOrder == DevExpress.Data.ColumnSortOrder.Descending)
        //    //            expression += " DESC";
        //    //        else
        //    //            expression += " ASC";
        //    //        expression += ", ";
        //    //    }
        //    //    return expression.TrimEnd(',', ' ');
        //    //}
        //    #endregion
        //}
        //#endregion
    }
}
