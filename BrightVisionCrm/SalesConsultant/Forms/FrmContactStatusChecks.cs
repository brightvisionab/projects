
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Common.Utilities;
using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Events.Core;
using SalesConsultant.Facade;
using SalesConsultant.Events;

namespace SalesConsultant.Forms
{
    public partial class FrmContactStatusChecks : XtraForm
    {
        #region Constructors
        public FrmContactStatusChecks()
        {
            InitializeComponent();
        }
        public FrmContactStatusChecks(List<XmlUtility.SubCampaignConfig> pContactStatuses)
        {
            //m_SubCampaignId = pSubCampaignId;
            m_ContactStatuses = pContactStatuses;
            InitializeComponent();
            this.GetContactFields();
            this.GetContactInterviewStatuses();
        }
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        //private class ContactStatus {
        //    public string contact_status { get; set; }
        //    public string field_list { get; set; }
        //}
        private class ContactField {
            public bool for_checking { get; set; }
            public string field_name { get; set; }
        }

        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        //private List<ContactStatus> m_ContactStatuses = null;
        private List<ContactField> m_ContactFields = null;
        //private int m_SubCampaignId = 0;
        private int m_ItemSelectedIndex = 0;
        private int m_NotQualifiedIndex = -1;
        private int m_SendEmailIndex = -1;
        private List<XmlUtility.SubCampaignConfig> m_ContactStatuses = new List<XmlUtility.SubCampaignConfig>();
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void GetContactInterviewStatuses()
        {
            //subcampaign _eftSubCampaign = null;
            //using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
            //    _eftSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == m_SubCampaignId);
            //    if (_eftSubCampaign != null)
            //        _efDbContext.Detach(_eftSubCampaign);
            //}

            //string _XmlData = _eftSubCampaign.xml_config_data;
            //List<string> _lstFieldChecks = new List<string>();
            //List<string> _lstContactStatuses = XmlUtility.GetXmlNodeDataAsList(_XmlData, "/sub_campaign_config/contact/contact_status_dropdown", ref m_ItemSelectedIndex, ref m_NotQualifiedIndex, ref m_SendEmailIndex, ref _lstFieldChecks);
            //if (_lstContactStatuses.Count < 1)
            //    return;

            //m_ContactStatuses = new List<ContactStatus>();
            //for (int i = 0; i < _lstContactStatuses.Count; i++)
            //    m_ContactStatuses.Add(new ContactStatus() {
            //        contact_status = _lstContactStatuses[i],
            //        field_list = _lstFieldChecks[i]
            //    });

            for (int i = 0; i < m_ContactStatuses.Count; i++)
                m_ContactStatuses[i].field_checks = m_ContactStatuses[i].field_checks.Replace(Environment.NewLine, ",");

            gcContactStatus.BeginUpdate();
            gcContactStatus.DataSource = null;
            gcContactStatus.DataSource = m_ContactStatuses;
            gcContactStatus.EndUpdate();
        }
        private void GetContactFields()
        {
            m_ContactFields = new List<ContactField>();
            m_ContactFields.Add(new ContactField() { for_checking = false, field_name = "first_name" });
            m_ContactFields.Add(new ContactField() { for_checking = false, field_name = "last_name" });
            m_ContactFields.Add(new ContactField() { for_checking = false, field_name = "email" });
            m_ContactFields.Add(new ContactField() { for_checking = false, field_name = "title" });

            gcDatabaseFieldChecks.BeginUpdate();
            gcDatabaseFieldChecks.DataSource = null;
            gcDatabaseFieldChecks.DataSource = m_ContactFields;
            gcDatabaseFieldChecks.EndUpdate();
        }
        //private string GetXml()
        //{
        //    StringBuilder _Xml = new StringBuilder();
        //    List<ContactStatus> _lstContactStatuses = gcContactStatus.DataSource as List<ContactStatus>;
        //    for (int i = 0; i < _lstContactStatuses.Count; i++) {
        //        List<string> _Attributes = new List<string>();
        //        if (m_ItemSelectedIndex == i)
        //            _Attributes.Add("selected=\"true\"");
        //        if (m_NotQualifiedIndex == i)
        //            _Attributes.Add("not_qualified_default=\"true\"");
        //        if (m_SendEmailIndex == i)
        //            _Attributes.Add("send_email=\"true\"");
        //        if (!string.IsNullOrEmpty(_lstContactStatuses[i].field_list))
        //            _Attributes.Add(string.Format("field_checks=\"{0}\"", _lstContactStatuses[i].field_list));

        //        string _CombinedAttributes = string.Join(" ", _Attributes.ToArray());
        //        _Xml.Append(string.Format("<item {1}>{0}</item>", _lstContactStatuses[i].contact_status, _CombinedAttributes));
        //    }

        //    return string.Format("<contact_status_dropdown>{0}</contact_status_dropdown>", _Xml.ToString());
        //}
        #endregion

        #region Control Events
        private void cbxForChecking_CheckedChanged(object sender, EventArgs e)
        {
            XmlUtility.SubCampaignConfig _ContactStateConfig = gvContactStatus.GetFocusedRow() as XmlUtility.SubCampaignConfig;
            List<string> _Fields = _ContactStateConfig.field_checks.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            CheckEdit _control = sender as CheckEdit;
            ContactField _clsContactField = gvDatabaseFieldChecks.GetFocusedRow() as ContactField;
            _Fields.Remove(_clsContactField.field_name);
            if (_control.Checked)
                _Fields.Add(_clsContactField.field_name);

            gvContactStatus.SetRowCellValue(gvContactStatus.FocusedRowHandle, "field_checks", string.Join(",", _Fields.ToArray().Distinct()));
        }
        private void gvContactStatus_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            XmlUtility.SubCampaignConfig _ContactStateConfig = gvContactStatus.GetFocusedRow() as XmlUtility.SubCampaignConfig;
            string[] _CheckedItems = _ContactStateConfig.field_checks.Split(',');
            this.cbxForChecking.CheckedChanged -= new EventHandler(this.cbxForChecking_CheckedChanged);

            for (int i = 0; i < gvDatabaseFieldChecks.RowCount; i++) {
                ContactField _clsContactField = gvDatabaseFieldChecks.GetRow(i) as ContactField;
                if (_CheckedItems.Contains(_clsContactField.field_name)) {
                    gvDatabaseFieldChecks.SetRowCellValue(i, "for_checking", true);
                    continue;
                }
                gvDatabaseFieldChecks.SetRowCellValue(i, "for_checking", false);
            }

            this.cbxForChecking.CheckedChanged += new EventHandler(this.cbxForChecking_CheckedChanged);
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            List<XmlUtility.SubCampaignConfig> _ContactStatuses = gcContactStatus.DataSource as List<XmlUtility.SubCampaignConfig>;
            for (int i = 0; i < _ContactStatuses.Count; i++)
                _ContactStatuses[i].field_checks = _ContactStatuses[i].field_checks.Replace(",", Environment.NewLine);

            m_EventBus.Notify(new FrmContactStatusCheckEvents.OnClose() {
                ContactStatuses = gcContactStatus.DataSource as List<XmlUtility.SubCampaignConfig>
            });

            this.Close();
        }
        #endregion
    }
}