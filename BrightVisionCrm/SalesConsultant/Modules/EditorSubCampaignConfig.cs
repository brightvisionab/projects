
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Linq;

using DevExpress.XtraEditors;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraRichEdit.Services.Implementation;
using DevExpress.XtraRichEdit.Services;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Controls;
using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using SalesConsultant.Forms;
using SalesConsultant.Business;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using BrightVision.Common.Events.Core;
using SalesConsultant.Events;
using DevExpress.Utils.Menu;

namespace SalesConsultant.Modules
{
    public partial class EditorSubCampaignConfig : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public EditorSubCampaignConfig()
        {
            InitializeComponent();
            this.RegisterEvents();
        }
        public EditorSubCampaignConfig(int pSubCampaignId)
        {
            m_DoneInitializing = false;
            InitializeComponent();
            this.RegisterEvents();
            m_SubCampaignId = pSubCampaignId;
            this.LoadConfigTemplates();
            this.LoadXmlData();
            m_DoneInitializing = true;
        }
        #endregion

        #region Public Events & Args

        #endregion

        #region Subscribed Events
        private void objForm_AfterSave(int pConfigTemplateId)
        {
            this.LoadConfigTemplates(pConfigTemplateId);
        }
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;

        private BrightPlatformEntities m_efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        private subcampaign m_efSubCampaign = null;
        private int m_SubCampaignId = 0;
        private bool m_DoneInitializing = true;

        private List<XmlUtility.SubCampaignConfig> m_ContactStatusConfig = new List<XmlUtility.SubCampaignConfig>();
        private List<XmlUtility.SubCampaignConfig> m_LeadStatusConfig = new List<XmlUtility.SubCampaignConfig>();
        private List<XmlUtility.SubCampaignConfig> m_CompanyStatusConfig = new List<XmlUtility.SubCampaignConfig>();
        #endregion

        #region Public Methods
        public void SetLabel(string pLabel)
        {
            lblSubCampaign.Text = pLabel;
        }
        #endregion

        #region Private Methods
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<FrmContactStatusCheckEvents.OnClose>().Subscribe(ContactStatusChecks_OnClose);
        }
        private void ContactStatusChecks_OnClose(FrmContactStatusCheckEvents.OnClose e)
        {
            //tbxContactStatus.Text = e.XmlData;
            this.LoadContactStatuses(e.ContactStatuses);
        }

        private void LoadContactStatuses(List<XmlUtility.SubCampaignConfig> pDataSource)
        {
            gcContactStatus.BeginUpdate();
            gcContactStatus.DataSource = null;
            gcContactStatus.DataSource = pDataSource;
            gcContactStatus.EndUpdate();
        }
        private void LoadLeadStatuses(List<XmlUtility.SubCampaignConfig> pDataSource)
        {
            gcLeadStatus.BeginUpdate();
            gcLeadStatus.DataSource = null;
            gcLeadStatus.DataSource = pDataSource;
            gcLeadStatus.EndUpdate();
        }
        private void LoadCompanyStatuses(List<XmlUtility.SubCampaignConfig> pDataSource)
        {
            gcCompanyStatus.BeginUpdate();
            gcCompanyStatus.DataSource = null;
            gcCompanyStatus.DataSource = pDataSource;
            gcCompanyStatus.EndUpdate();
        }
        private void LoadXmlData()
        {
            m_efSubCampaign = m_efDbModel.subcampaigns.FirstOrDefault(i => i.id == m_SubCampaignId);
            if (m_efSubCampaign == null)
                return;

            try {
                m_ContactStatusConfig = XmlUtility.GetXmlNodeDataAsList(m_efSubCampaign.xml_config_data, "/sub_campaign_config/contact/contact_status_dropdown", true);
                m_LeadStatusConfig = XmlUtility.GetXmlNodeDataAsList(m_efSubCampaign.xml_config_data, "/sub_campaign_config/account/account_lead_status_dropdown", true);
                m_CompanyStatusConfig = XmlUtility.GetXmlNodeDataAsList(m_efSubCampaign.xml_config_data, "/sub_campaign_config/account/account_status_dropdown", true);

                this.LoadContactStatuses(m_ContactStatusConfig);
                this.LoadLeadStatuses(m_LeadStatusConfig);
                this.LoadCompanyStatuses(m_CompanyStatusConfig);

                //tbxContactStatus.Text = XmlUtility.GetXmlNodeOuterData(m_efSubCampaign.xml_config_data, "/sub_campaign_config/contact/contact_status_dropdown");
                //tbxLeadStatus.Text = XmlUtility.GetXmlNodeOuterData(m_efSubCampaign.xml_config_data, "/sub_campaign_config/account/account_lead_status_dropdown");
                //tbxCompanyStatus.Text = XmlUtility.GetXmlNodeOuterData(m_efSubCampaign.xml_config_data, "/sub_campaign_config/account/account_status_dropdown");
                tbxGeneralSettings.Text = XmlUtility.GetXmlNodeInnerData(m_efSubCampaign.xml_config_data, "/sub_campaign_config/general_settings");
                //tbxLeadStatus.FormatOnLoad();
                //tbxContactStatus.FormatOnLoad();
                //tbxCompanyStatus.FormatOnLoad();
                tbxGeneralSettings.FormatOnLoad();
            }
            catch { 
            }
        }
        private void LoadConfigTemplates(int pNewConfigTemplateId = 0)
        {
            try
            {
                DevExpress.XtraEditors.Repository.RepositoryItemComboBox cboType = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
                cboType.TextEditStyle = TextEditStyles.DisableTextEditor;
                cboType.Items.Add("Company Status");
                cboType.Items.Add("Lead Status");
                cboType.Items.Add("Contact Status");
                cboType.Items.Add("General Settings");

                gcTemplate.DataSource = m_efDbModel.FIGetSubCampaignConfigTemplates().ToList();
                gvTemplate.Columns["type"].ColumnEdit = cboType;
                gvTemplate.BestFitColumns();

                /**
                 * set focus on the newly added template config.
                 */
                if (pNewConfigTemplateId > 0) {
                    for (int i = 0; i < gvTemplate.RowCount; i++)
                        if (Convert.ToInt32(gvTemplate.GetRowCellValue(i, "id")) == pNewConfigTemplateId)
                            gvTemplate.FocusedRowHandle = i;
                }
            }
            catch { }
        }
        private string ExtractXmlData(List<XmlUtility.SubCampaignConfig> pDataSource)
        {
            StringBuilder _Xml = new StringBuilder();
            for (int i = 0; i < pDataSource.Count; i++) {
                List<string> _Attributes = new List<string>();
                if (pDataSource[i].selected)
                    _Attributes.Add("selected=\"true\"");
                if (pDataSource[i].not_qualified_default)
                    _Attributes.Add("not_qualified_default=\"true\"");
                if (pDataSource[i].send_email)
                    _Attributes.Add("send_email=\"true\"");
                if (!string.IsNullOrEmpty(pDataSource[i].description))
                    _Attributes.Add(string.Format("description=\"{0}\"", pDataSource[i].description));
                if (!string.IsNullOrEmpty(pDataSource[i].field_checks))
                    _Attributes.Add(string.Format("field_checks=\"{0}\"", pDataSource[i].field_checks.Replace(Environment.NewLine, ",")));

                string _CombinedAttributes = string.Join(" ", _Attributes.ToArray());
                _Xml.Append(string.Format("<item {1}>{0}</item>", pDataSource[i].status, _CombinedAttributes));
            }

            return _Xml.ToString();
        }
        private string BuildXmlData()
        {
            string _XmlContactStatuses = string.Format("<contact_status_dropdown>{0}</contact_status_dropdown>", this.ExtractXmlData(gcContactStatus.DataSource as List<XmlUtility.SubCampaignConfig>));
            string _XmlLeadStatuses = string.Format("<account_lead_status_dropdown>{0}</account_lead_status_dropdown>", this.ExtractXmlData(gcLeadStatus.DataSource as List<XmlUtility.SubCampaignConfig>));
            string _XmlCompanyStatuses = string.Format("<account_status_dropdown>{0}</account_status_dropdown>", this.ExtractXmlData(gcCompanyStatus.DataSource as List<XmlUtility.SubCampaignConfig>));
            return string.Format("<sub_campaign_config><contact>{0}</contact><account>{1}{2}</account><general_settings>{3}</general_settings></sub_campaign_config>",
                _XmlContactStatuses,
                _XmlLeadStatuses,
                _XmlCompanyStatuses,
                tbxGeneralSettings.Text
            );

            //return string.Format("<contact_status_dropdown>{0}</contact_status_dropdown>", _Xml.ToString());

            //return string.Format("<sub_campaign_config><contact>{0}</contact><account>{1}{2}</account><general_settings>{3}</general_settings></sub_campaign_config>",
            //    tbxContactStatus.Text,
            //    tbxLeadStatus.Text,
            //    tbxCompanyStatus.Text,
            //    tbxGeneralSettings.Text
            //);
        }

        /**
         * grid context menu.
         */
        private void CreateContextMenu(GridView pView, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs pArgs)
        {
            if (pView.Columns.Count < 1 && pView.RowCount < 1)
                return;

            if (pArgs.MenuType == GridMenuType.Row || pArgs.MenuType == GridMenuType.User) {
                if (pArgs.MenuType == GridMenuType.User)
                    if (pArgs.Menu == null)
                        pArgs.Menu = new DevExpress.XtraGrid.Menu.GridViewMenu(pView);

                pArgs.Menu.Items.Clear();
                int _row = pArgs.HitInfo.RowHandle;

                if (pView.Name.Equals("gvContactStatus"))
                    pArgs.Menu.Items.Add(this.MenuConfigureFieldChecks(pView, _row));
                
                pArgs.Menu.Items.Add(this.MenuAddNewStatus(pView, _row));
                pArgs.Menu.Items.Add(this.MenuRemoveStatus(pView, _row));
                pArgs.Menu.Items.Add(this.MenuLoadToTemplateDesigner(pView, _row));
                pArgs.Menu.CloseUp += Menu_CloseUp;
            }
        }
        private void Menu_CloseUp(object sender, EventArgs e)
        {
            // add here menus to nullify if necessary.
        }
        private DXMenuItem MenuConfigureFieldChecks(GridView pView, int pRow)
        {
            DXMenuItem _menu = new DXMenuItem { 
                Caption = "Configure Field Checks", 
                //Shortcut = Shortcut.CtrlC,
                Tag = new RowInfo(pView, pRow),
                Image = Properties.Resources.booking
            };

            if (pView.Name.Equals("gvContactStatus"))
                _menu.Click += new EventHandler(gvContactStatus_MenuConfigureFieldChecks_Click);
            //else if (pView.Name.Equals("gvLeadStatus"))
            //    _menu.Click += new EventHandler(gvLeadStatus_MenuConfigureFieldChecks_Click);
            //else if (pView.Name.Equals("gvCompanyStatus"))
            //    _menu.Click += new EventHandler(gvCompanyStatus_MenuConfigureFieldChecks_Click);
            
            return _menu;
        }
        private void gvContactStatus_MenuConfigureFieldChecks_Click(object sender, EventArgs e)
        {
            DXMenuItem _menu = sender as DXMenuItem;
            RowInfo _info = _menu.Tag as RowInfo;
            if (_info == null || _info.View == null)
                return;

            WaitDialog.Show("Loading field checks editor ...");
            if (gvContactStatus.RowCount < 1) {
                BrightVision.Common.UI.NotificationDialog.Warning("Bright Sales", "No contact status defined. Please set contact statuses.");
                return;
            }

            FrmContactStatusChecks _form = new FrmContactStatusChecks(gcContactStatus.DataSource as List<XmlUtility.SubCampaignConfig>) {
                StartPosition = FormStartPosition.CenterParent
            };
            WaitDialog.Close();
            _form.ShowDialog(this.ParentForm);
        }
        //private void gvLeadStatus_MenuConfigureFieldChecks_Click(object sender, EventArgs e)
        //{
        //    DXMenuItem _menu = sender as DXMenuItem;
        //    RowInfo _info = _menu.Tag as RowInfo;
        //    if (_info == null || _info.View == null)
        //        return;

        //    //if (gvContactStatus.RowCount < 1) {
        //    //    BrightVision.Common.UI.NotificationDialog.Warning("Bright Sales", "No contact status defined. Please set contact statuses.");
        //    //    return;
        //    //}

        //    //FrmContactStatusChecks _form = new FrmContactStatusChecks(gcContactStatus.DataSource as List<XmlUtility.SubCampaignConfig>) {
        //    //    StartPosition = FormStartPosition.CenterParent
        //    //};
        //    //_form.ShowDialog(this.ParentForm);
        //}
        //private void gvCompanyStatus_MenuConfigureFieldChecks_Click(object sender, EventArgs e)
        //{
        //    DXMenuItem _menu = sender as DXMenuItem;
        //    RowInfo _info = _menu.Tag as RowInfo;
        //    if (_info == null || _info.View == null)
        //        return;

            //if (gvContactStatus.RowCount < 1) {
            //    BrightVision.Common.UI.NotificationDialog.Warning("Bright Sales", "No contact status defined. Please set contact statuses.");
            //    return;
            //}

            //FrmContactStatusChecks _form = new FrmContactStatusChecks(gcContactStatus.DataSource as List<XmlUtility.SubCampaignConfig>) {
            //    StartPosition = FormStartPosition.CenterParent
            //};
            //_form.ShowDialog(this.ParentForm);
        //}
        private DXMenuItem MenuAddNewStatus(GridView pView, int pRow)
        {
            DXMenuItem _menu = new DXMenuItem {
                Caption = "Add New Status",
                //Shortcut = Shortcut.CtrlC,
                Tag = new RowInfo(pView, pRow),
                Image = Properties.Resources.add
            };

            if (pView.Name.Equals("gvContactStatus"))
                _menu.Click += new EventHandler(gvContactStatus_MenuAddNewStatus_Click);
            else if (pView.Name.Equals("gvLeadStatus"))
                _menu.Click += new EventHandler(gvLeadStatus_MenuAddNewStatus_Click);
            else if (pView.Name.Equals("gvCompanyStatus"))
                _menu.Click += new EventHandler(gvCompanyStatus_MenuAddNewStatus_Click);

            return _menu;
        }
        private void gvContactStatus_MenuAddNewStatus_Click(object sender, EventArgs e)
        {
            DXMenuItem _menu = sender as DXMenuItem;
            RowInfo _info = _menu.Tag as RowInfo;
            if (_info == null || _info.View == null)
                return;

            WaitDialog.Show("Adding status item ...");
            m_ContactStatusConfig.Add(new XmlUtility.SubCampaignConfig() {
                status = string.Format("Status Item {0}", m_ContactStatusConfig.Count + 1),
                description = string.Format("Status Item {0}", m_ContactStatusConfig.Count + 1),
                field_checks = string.Empty,
                not_qualified_default = false,
                selected = false,
                send_email = false
            });

            this.LoadContactStatuses(m_ContactStatusConfig);
            WaitDialog.Close();
        }
        private void gvLeadStatus_MenuAddNewStatus_Click(object sender, EventArgs e)
        {
            DXMenuItem _menu = sender as DXMenuItem;
            RowInfo _info = _menu.Tag as RowInfo;
            if (_info == null || _info.View == null)
                return;

            WaitDialog.Show("Adding status item ...");
            m_LeadStatusConfig.Add(new XmlUtility.SubCampaignConfig() {
                status = string.Format("Status Item {0}", m_LeadStatusConfig.Count + 1),
                description = string.Format("Status Item {0}", m_LeadStatusConfig.Count + 1),
                field_checks = string.Empty,
                not_qualified_default = false,
                selected = false,
                send_email = false
            });

            this.LoadLeadStatuses(m_LeadStatusConfig);
            WaitDialog.Close();
        }
        private void gvCompanyStatus_MenuAddNewStatus_Click(object sender, EventArgs e)
        {
            DXMenuItem _menu = sender as DXMenuItem;
            RowInfo _info = _menu.Tag as RowInfo;
            if (_info == null || _info.View == null)
                return;

            WaitDialog.Show("Adding status item ...");
            m_CompanyStatusConfig.Add(new XmlUtility.SubCampaignConfig() {
                status = string.Format("Status Item {0}", m_CompanyStatusConfig.Count + 1),
                description = string.Format("Status Item {0}", m_CompanyStatusConfig.Count + 1),
                field_checks = string.Empty,
                not_qualified_default = false,
                selected = false,
                send_email = false
            });

            this.LoadCompanyStatuses(m_CompanyStatusConfig);
            WaitDialog.Close();
        }
        private DXMenuItem MenuRemoveStatus(GridView pView, int pRow)
        {
            DXMenuItem _menu = new DXMenuItem {
                Caption = "Remove Selected Status",
                //Shortcut = Shortcut.CtrlC,
                Tag = new RowInfo(pView, pRow),
                Image = Properties.Resources.cancel_delete
            };

            if (pView.Name.Equals("gvContactStatus"))
                _menu.Click += new EventHandler(gvContactStatus_MenuRemoveStatus_Click);
            else if (pView.Name.Equals("gvLeadStatus"))
                _menu.Click += new EventHandler(gvLeadStatus_MenuRemoveStatus_Click);
            else if (pView.Name.Equals("gvCompanyStatus"))
                _menu.Click += new EventHandler(gvCompanyStatus_MenuRemoveStatus_Click);
            
            return _menu;
        }
        private void gvContactStatus_MenuRemoveStatus_Click(object sender, EventArgs e)
        {
            DXMenuItem _menu = sender as DXMenuItem;
            RowInfo _info = _menu.Tag as RowInfo;
            if (_info == null || _info.View == null)
                return;

            WaitDialog.Show("Removing status item ...");
            XmlUtility.SubCampaignConfig _Config = gvContactStatus.GetFocusedRow() as  XmlUtility.SubCampaignConfig;
            m_ContactStatusConfig.Remove(_Config);
            this.LoadContactStatuses(m_ContactStatusConfig);
            WaitDialog.Close();
        }
        private void gvLeadStatus_MenuRemoveStatus_Click(object sender, EventArgs e)
        {
            DXMenuItem _menu = sender as DXMenuItem;
            RowInfo _info = _menu.Tag as RowInfo;
            if (_info == null || _info.View == null)
                return;

            WaitDialog.Show("Removing status item ...");
            XmlUtility.SubCampaignConfig _Config = gvLeadStatus.GetFocusedRow() as XmlUtility.SubCampaignConfig;
            m_LeadStatusConfig.Remove(_Config);
            this.LoadLeadStatuses(m_LeadStatusConfig);
            WaitDialog.Close();
        }
        private void gvCompanyStatus_MenuRemoveStatus_Click(object sender, EventArgs e)
        {
            DXMenuItem _menu = sender as DXMenuItem;
            RowInfo _info = _menu.Tag as RowInfo;
            if (_info == null || _info.View == null)
                return;

            WaitDialog.Show("Removing status item ...");
            XmlUtility.SubCampaignConfig _Config = gvCompanyStatus.GetFocusedRow() as XmlUtility.SubCampaignConfig;
            m_CompanyStatusConfig.Remove(_Config);
            this.LoadCompanyStatuses(m_CompanyStatusConfig);
            WaitDialog.Close();
        }
        private DXMenuItem MenuLoadToTemplateDesigner(GridView pView, int pRow)
        {
            DXMenuItem _menu = new DXMenuItem {
                Caption = "Load to Template Editor",
                //Shortcut = Shortcut.CtrlC,
                Tag = new RowInfo(pView, pRow),
                Image = Properties.Resources.upload
            };

            if (pView.Name.Equals("gvContactStatus"))
                _menu.Click += new EventHandler(gvContactStatus_MenuLoadToTemplateDesigner_Click);
            else if (pView.Name.Equals("gvLeadStatus"))
                _menu.Click += new EventHandler(gvLeadStatus_MenuLoadToTemplateDesigner_Click);
            else if (pView.Name.Equals("gvCompanyStatus"))
                _menu.Click += new EventHandler(gvCompanyStatus_MenuLoadToTemplateDesigner_Click);

            return _menu;
        }
        private void gvContactStatus_MenuLoadToTemplateDesigner_Click(object sender, EventArgs e)
        {
            DXMenuItem _menu = sender as DXMenuItem;
            RowInfo _info = _menu.Tag as RowInfo;
            if (_info == null || _info.View == null)
                return;

            tbxTemplateEditor.Text = string.Empty;
            WaitDialog.Show("Loading to template editor ...");
            tbxTemplateEditor.Text = XmlUtility.ConvertToXml(string.Format("<contact_status_dropdown>{0}</contact_status_dropdown>", this.ExtractXmlData(gcContactStatus.DataSource as List<XmlUtility.SubCampaignConfig>)));
            tbxTemplateEditor.FormatOnLoad();
            WaitDialog.Close();
        }
        private void gvLeadStatus_MenuLoadToTemplateDesigner_Click(object sender, EventArgs e)
        {
            DXMenuItem _menu = sender as DXMenuItem;
            RowInfo _info = _menu.Tag as RowInfo;
            if (_info == null || _info.View == null)
                return;

            tbxTemplateEditor.Text = string.Empty;
            WaitDialog.Show("Loading to template editor ...");
            tbxTemplateEditor.Text = XmlUtility.ConvertToXml(string.Format("<account_lead_status_dropdown>{0}</account_lead_status_dropdown>", this.ExtractXmlData(gcLeadStatus.DataSource as List<XmlUtility.SubCampaignConfig>)));
            tbxTemplateEditor.FormatOnLoad();
            WaitDialog.Close();
        }
        private void gvCompanyStatus_MenuLoadToTemplateDesigner_Click(object sender, EventArgs e)
        {
            DXMenuItem _menu = sender as DXMenuItem;
            RowInfo _info = _menu.Tag as RowInfo;
            if (_info == null || _info.View == null)
                return;

            tbxTemplateEditor.Text = string.Empty;
            WaitDialog.Show("Loading to template editor ...");
            tbxTemplateEditor.Text = XmlUtility.ConvertToXml(string.Format("<account_status_dropdown>{0}</account_status_dropdown>", this.ExtractXmlData(gcCompanyStatus.DataSource as List<XmlUtility.SubCampaignConfig>)));
            tbxTemplateEditor.FormatOnLoad();
            WaitDialog.Close();
        }
        #endregion

        #region Control Events
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (m_efSubCampaign == null)
                return;

            WaitDialog.Show("Saving config data ...");
            m_efSubCampaign.xml_config_data = this.BuildXmlData();
            subcampaign _handle = m_efDbModel.subcampaigns.FirstOrDefault(i => i.id == m_efSubCampaign.id);
            m_efDbModel.subcampaigns.ApplyCurrentValues(m_efSubCampaign);
            m_efDbModel.SaveChanges();
            WaitDialog.Close();
            this.ParentForm.Close();
        }
        private void btnSaveTemplate_Click(object sender, EventArgs e)
        {
            if (gcTemplate == null || gvTemplate.RowCount < 1)
                return;

            WaitDialog.Show("Saving config template...");
            CTSubCampaignConfigTemplate _item = gvTemplate.GetFocusedRow() as CTSubCampaignConfigTemplate;
            sub_campaign_configuration_templates _efeItem = m_efDbModel.sub_campaign_configuration_templates.FirstOrDefault(i => i.id == _item.id);
            _efeItem.template = tbxTemplateEditor.Text;
            _efeItem.name = _item.name;
            _efeItem.type = _item.type;
            _efeItem.is_default = _item.is_default;
            _efeItem.modified_by = UserSession.CurrentUser.UserId;
            _efeItem.modified_on = DateTime.Now;
            m_efDbModel.SaveChanges();
            gvTemplate.SetRowCellValue(gvTemplate.FocusedRowHandle, "modified_on", DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString().PadLeft(2, Convert.ToChar("0")) + "-" + DateTime.Now.Day.ToString().PadLeft(2, Convert.ToChar("0")));
            System.Threading.Thread.Sleep(500);
            WaitDialog.Close();
        }
        private void btnLoadToSubCampaign_Click(object sender, EventArgs e)
        {
            if (gcTemplate == null || gvTemplate.RowCount < 1)
                return;

            WaitDialog.Show("Loading config...");
            if (gvTemplate.GetRowCellValue(gvTemplate.FocusedRowHandle, "type").Equals("Company Status")) {
                m_CompanyStatusConfig = XmlUtility.GetXmlNodeDataAsList(tbxTemplateEditor.Text, "account_status_dropdown", true);
                this.LoadCompanyStatuses(m_CompanyStatusConfig);
                //tbxCompanyStatus.Text = tbxTemplateEditor.Text;
            }
            else if (gvTemplate.GetRowCellValue(gvTemplate.FocusedRowHandle, "type").Equals("Lead Status")) {
                m_LeadStatusConfig = XmlUtility.GetXmlNodeDataAsList(tbxTemplateEditor.Text, "account_lead_status_dropdown", true);
                this.LoadLeadStatuses(m_LeadStatusConfig);
                //tbxLeadStatus.Text = tbxTemplateEditor.Text;
            }
            else if (gvTemplate.GetRowCellValue(gvTemplate.FocusedRowHandle, "type").Equals("Contact Status")) {
                m_ContactStatusConfig = XmlUtility.GetXmlNodeDataAsList(tbxTemplateEditor.Text, "contact_status_dropdown", true);
                this.LoadContactStatuses(m_ContactStatusConfig);
                //tbxContactStatus.Text = tbxTemplateEditor.Text;
            }
            else if (gvTemplate.GetRowCellValue(gvTemplate.FocusedRowHandle, "type").Equals("General Settings"))
                tbxGeneralSettings.Text = tbxTemplateEditor.Text;
            WaitDialog.Close();
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (gcTemplate == null || gvTemplate.RowCount < 1)
                return;

            DialogResult _dlg = MessageBox.Show("Are you sure to delete this config template?", "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlg == DialogResult.No)
                return;

            WaitDialog.Show("Deleting config template...");
            CTSubCampaignConfigTemplate _item = gvTemplate.GetFocusedRow() as CTSubCampaignConfigTemplate;
            sub_campaign_configuration_templates _efeItem = m_efDbModel.sub_campaign_configuration_templates.FirstOrDefault(i => i.id == _item.id);
            m_efDbModel.sub_campaign_configuration_templates.DeleteObject(_efeItem);
            m_efDbModel.SaveChanges();
            gvTemplate.DeleteRow(gvTemplate.FocusedRowHandle);
            WaitDialog.Close();
        }
        private void btnNew_Click(object sender, EventArgs e)
        {
            AddSubCampaignConfigTemplate objForm = new AddSubCampaignConfigTemplate();
            objForm.AfterSave += new AddSubCampaignConfigTemplate.AfterSaveEventHandler(objForm_AfterSave);
            objForm.Dock = DockStyle.Fill;
            PopupDialog m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = "Add Sub-campaign Xml Config";
            m_objPopupDialog.ClientSize = new Size(objForm.Width + 2, objForm.Height + 2);
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.Controls.Add(objForm);
            m_objPopupDialog.ShowDialog(this.ParentForm);
        }
        private void cbxDefault_CheckedChanged(object sender, EventArgs e)
        {
            WaitDialog.Show("Activating / De-activating template...");
            CheckEdit _cbx = sender as CheckEdit;
            CTSubCampaignConfigTemplate _item = gvTemplate.GetFocusedRow() as CTSubCampaignConfigTemplate;
            sub_campaign_configuration_templates _efeItem = m_efDbModel.sub_campaign_configuration_templates.FirstOrDefault(i => i.id == _item.id);
            _efeItem.is_default = _cbx.Checked;
            _efeItem.modified_by = UserSession.CurrentUser.UserId;
            _efeItem.modified_on = DateTime.Now;
            m_efDbModel.SaveChanges();
            gvTemplate.SetRowCellValue(gvTemplate.FocusedRowHandle, "modified_on", DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString().PadLeft(2, Convert.ToChar("0")) + "-" + DateTime.Now.Day.ToString().PadLeft(2, Convert.ToChar("0")));
            WaitDialog.Close();
        }
        private void cbxDefault_EditValueChanging(object sender, ChangingEventArgs e)
        {
            CheckEdit _cbx = sender as CheckEdit;
            if (!_cbx.Checked)
            {
                int _ExistingDefaultItem = 0;
                CTSubCampaignConfigTemplate _item = gvTemplate.GetFocusedRow() as CTSubCampaignConfigTemplate;
                CTSubCampaignConfigTemplate _loop_item = null;
                for (int i = 0; i < gvTemplate.RowCount; i++)
                {
                    _loop_item = gvTemplate.GetRow(i) as CTSubCampaignConfigTemplate;
                    if (_loop_item.is_default && _loop_item.type == _item.type)
                    {
                        _ExistingDefaultItem = i;
                        break;
                    }
                }
                if (_ExistingDefaultItem > 0)
                {
                    DialogResult _dlg = MessageBox.Show(
                        "A default item has already been set for this template type. Would you like to set this item as the default template instead?",
                        "Bright Manager",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (_dlg == DialogResult.No)
                        e.Cancel = true;
                    else
                    {
                        gvTemplate.SetRowCellValue(_ExistingDefaultItem, "is_default", false);
                        sub_campaign_configuration_templates _efeConfigTemplate = m_efDbModel.sub_campaign_configuration_templates.FirstOrDefault(i => i.id == _loop_item.id);
                        _efeConfigTemplate.is_default = false;
                        m_efDbModel.SaveChanges();
                    }
                }
            }
        }
        private void gvTemplate_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void gvTemplate_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (gcTemplate == null || gvTemplate.RowCount < 1)
                return;

            if (gvTemplate.IsNewItemRow(gvTemplate.RowCount))
                return;

            if (m_DoneInitializing)
                WaitDialog.Show("Loading config data ...");

            CTSubCampaignConfigTemplate _item = gvTemplate.GetFocusedRow() as CTSubCampaignConfigTemplate;
            sub_campaign_configuration_templates _efeItem = m_efDbModel.sub_campaign_configuration_templates.FirstOrDefault(i => i.id == _item.id);
            tbxTemplateEditor.Text = "";
            if (!string.IsNullOrEmpty(_efeItem.template)) {
                tbxTemplateEditor.Text = XmlUtility.ConvertToXml(_efeItem.template);
                tbxTemplateEditor.FormatOnLoad();
            }

            if (m_DoneInitializing)
                WaitDialog.Close();
        }
        private void gvContactStatus_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            GridView _GridView = sender as GridView;
            this.CreateContextMenu(_GridView, e);
        }
        private void gvLeadStatus_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            GridView _GridView = sender as GridView;
            this.CreateContextMenu(_GridView, e);
        }
        private void gvCompanyStatus_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            GridView _GridView = sender as GridView;
            this.CreateContextMenu(_GridView, e);
        }
        private void btnHelp_Click(object sender, EventArgs e)
        {
            SubCampaignConfigurationHelp _control = new SubCampaignConfigurationHelp() {
                Dock = DockStyle.Fill
            };
            PopupDialog _dlg = new PopupDialog() {
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Sub Campaign Configuration Settings Keywords",
                FormBorderStyle = FormBorderStyle.FixedSingle
            };
            _dlg.ClientSize = new Size(_control.Width + 2, _control.Height + 2);
            _dlg.Controls.Add(_control);
            _dlg.ShowDialog(this.ParentForm);
        }
        private void cbxContactStateDefault_EditValueChanging(object sender, ChangingEventArgs e)
        {
            for (int i = 0; i < gvContactStatus.RowCount; i++)
                gvContactStatus.SetRowCellValue(i, "selected", false);
        }
        private void cbxContactStateNotQualified_EditValueChanging(object sender, ChangingEventArgs e)
        {
            for (int i = 0; i < gvContactStatus.RowCount; i++)
                gvContactStatus.SetRowCellValue(i, "not_qualified_default", false);
        }
        private void cbxContactStateSendEmail_EditValueChanging(object sender, ChangingEventArgs e)
        {
            for (int i = 0; i < gvContactStatus.RowCount; i++)
                gvContactStatus.SetRowCellValue(i, "send_email", false);
        }
        private void cbxLeadStateDefault_EditValueChanging(object sender, ChangingEventArgs e)
        {
            for (int i = 0; i < gvLeadStatus.RowCount; i++)
                gvLeadStatus.SetRowCellValue(i, "selected", false);
        }
        private void cbxLeadStateNotQualified_EditValueChanging(object sender, ChangingEventArgs e)
        {
            for (int i = 0; i < gvLeadStatus.RowCount; i++)
                gvLeadStatus.SetRowCellValue(i, "not_qualified_default", false);
        }
        private void cbxLeadStateSendEmail_EditValueChanging(object sender, ChangingEventArgs e)
        {
            for (int i = 0; i < gvLeadStatus.RowCount; i++)
                gvLeadStatus.SetRowCellValue(i, "send_email", false);
        }
        private void cbxCompanyStateDefault_EditValueChanging(object sender, ChangingEventArgs e)
        {
            for (int i = 0; i < gvCompanyStatus.RowCount; i++)
                gvCompanyStatus.SetRowCellValue(i, "selected", false);
        }
        private void cbxCompanyStateNotQualified_EditValueChanging(object sender, ChangingEventArgs e)
        {
            for (int i = 0; i < gvCompanyStatus.RowCount; i++)
                gvCompanyStatus.SetRowCellValue(i, "not_qualified_default", false);
        }
        private void cbxCompanyStateSendEmail_EditValueChanging(object sender, ChangingEventArgs e)
        {
            for (int i = 0; i < gvCompanyStatus.RowCount; i++)
                gvCompanyStatus.SetRowCellValue(i, "send_email", false);
        }
        #endregion
    }
}
