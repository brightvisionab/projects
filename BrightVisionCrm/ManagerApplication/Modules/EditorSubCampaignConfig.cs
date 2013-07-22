
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

namespace ManagerApplication.Modules
{
    public partial class EditorSubCampaignConfig : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public EditorSubCampaignConfig()
        {
            InitializeComponent();
        }
        public EditorSubCampaignConfig(int pSubCampaignId)
        {
            m_DoneInitializing = false;
            InitializeComponent();
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
        private BrightPlatformEntities m_efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        private subcampaign m_efSubCampaign = null;
        private int m_SubCampaignId = 0;
        private bool m_DoneInitializing = true;
        #endregion

        #region Public Methods
        public void SetLabel(string pLabel)
        {
            lblSubCampaign.Text = pLabel;
        }
        #endregion

        #region Private Methods
        private void LoadXmlData()
        {
            m_efSubCampaign = m_efDbModel.subcampaigns.FirstOrDefault(i => i.id == m_SubCampaignId);
            if (m_efSubCampaign == null)
                return;

            try {
                tbxContactStatus.Text = XmlUtility.GetXmlNodeOuterData(m_efSubCampaign.xml_config_data, "/sub_campaign_config/contact/contact_status_dropdown");
                tbxLeadStatus.Text = XmlUtility.GetXmlNodeOuterData(m_efSubCampaign.xml_config_data, "/sub_campaign_config/account/account_lead_status_dropdown");
                tbxCompanyStatus.Text = XmlUtility.GetXmlNodeOuterData(m_efSubCampaign.xml_config_data, "/sub_campaign_config/account/account_status_dropdown");
                tbxGeneralSettings.Text = XmlUtility.GetXmlNodeOuterData(m_efSubCampaign.xml_config_data, "/sub_campaign_config/general_settings");
                tbxLeadStatus.FormatOnLoad();
                tbxContactStatus.FormatOnLoad();
                tbxCompanyStatus.FormatOnLoad();
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
        private string BuildXmlData()
        {
            return string.Format("<sub_campaign_config><contact>{0}</contact><account>{1}{2}</account><general_settings>{3}</general_settings></sub_campaign_config>",
                tbxContactStatus.Text,
                tbxLeadStatus.Text,
                tbxCompanyStatus.Text,
                tbxGeneralSettings.Text
            );
        }
        #endregion

        #region Control Events
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (m_efSubCampaign == null)
                return;

            WaitDialog.Show("Saving config data...");
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
            if (gvTemplate.GetRowCellValue(gvTemplate.FocusedRowHandle, "type").Equals("Company Status"))
                tbxCompanyStatus.Text = tbxTemplateEditor.Text;
            else if (gvTemplate.GetRowCellValue(gvTemplate.FocusedRowHandle, "type").Equals("Lead Status"))
                tbxLeadStatus.Text = tbxTemplateEditor.Text;
            else if (gvTemplate.GetRowCellValue(gvTemplate.FocusedRowHandle, "type").Equals("Contact Status"))
                tbxContactStatus.Text = tbxTemplateEditor.Text;
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
                WaitDialog.Show("Loading config data...");

            CTSubCampaignConfigTemplate _item = gvTemplate.GetFocusedRow() as CTSubCampaignConfigTemplate;
            sub_campaign_configuration_templates _efeItem = m_efDbModel.sub_campaign_configuration_templates.FirstOrDefault(i => i.id == _item.id);
            tbxTemplateEditor.Text = "";
            if (!string.IsNullOrEmpty(_efeItem.template))
            {
                tbxTemplateEditor.Text = XmlUtility.ConvertToXml(_efeItem.template);
                tbxTemplateEditor.FormatOnLoad();
            }

            if (m_DoneInitializing)
                WaitDialog.Close();
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
        #endregion
    }
}
