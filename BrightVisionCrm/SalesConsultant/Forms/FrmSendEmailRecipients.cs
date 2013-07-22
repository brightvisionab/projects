
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using BrightVision.Common.Events.Core;
using SalesConsultant.Modules;
using BrightVision.Model;
using BrightVision.Common.Business;
using DevExpress.XtraEditors.Controls;
using System.Linq;
using BrightVision.Common.UI;
using DevExpress.XtraGrid.Views.Grid;
using System.IO;
using BrightVision.Reporting.Utility;
using BrightVision.Common.Utilities;

namespace SalesConsultant.Forms
{
    public partial class FrmSendEmailRecipients : DevExpress.XtraEditors.XtraForm
    {
        #region Constructors
        public FrmSendEmailRecipients()
        {
            WaitDialog.Show("Loading send email recipients ...");
            InitializeComponent();
            this.GetDefaultSubjectAndMessage();
            InitEvents();
        }

        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private List<int> m_lstSubCampaignIds = null;
        private ReportsUtility m_Reports = null;
        private bool m_SendEmailButtonPressed = false;

        private class ViewCofigData {
            public int id { get; set; }
            public int subcampaign_id { get; set; }
            public string name { get; set; }
        }
        //private enum eMailType : int {
        //    Send_Report_To_Customer = 1,
        //    Send_SMS_To_Customer,
        //    Send_Mail_To_Prospect,
        //    Send_SMS_To_Prospect
        //}
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void GetRecipients()
        {
            gcRecipients.DataSource = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                gcRecipients.DataSource = _efDbContext.FIGetSubCampaignEmailRecipients(m_BrightSalesProperty.CommonProperty.SubCampaignId);
                gvRecipients.BestFitColumns();
            }

            gvRecipients.Columns["role_name"].Width = 100;
            gvRecipients.Columns["mail_cc"].Visible = false;
        }
        private void GetReports()
        {
            List<ViewCofigData> _lstData = new List<ViewCofigData>();
            m_lstSubCampaignIds = new List<int>();
            m_lstSubCampaignIds.Add(m_BrightSalesProperty.CommonProperty.SubCampaignId);
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _lstData = this.GetViewConfigInfo(m_lstSubCampaignIds.ToArray());
            }
            if (_lstData.Count > 0) {
                cboReports.Properties.DataSource = _lstData;
                cboReports.Properties.DisplayMember = "name";
                cboReports.Properties.ValueMember = "id";
                cboReports.Properties.Columns.Add(new LookUpColumnInfo("name"));
                cboReports.ItemIndex = 0;
            }
        }
        private List<ViewCofigData> GetViewConfigInfo(int[] pSubCampaignIds)
        {
            List<ViewCofigData> listViewConfig = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                if (pSubCampaignIds.Length <= 0) return null;
                listViewConfig = _efDbContext.view_configuration.Where(i =>
                    pSubCampaignIds.Contains(i.subcampaign_id) &&
                    i.MGC == false &&
                    i.report_layout_config != null
                ).Select(x =>
                    new ViewCofigData { id = x.id, name = x.name }
                ).ToList();
            }
            return listViewConfig;
        }
        private int CountRecipients()
        {
            int _CheckedEmails = 0;
            for (int i = 0; i < gvRecipients.RowCount; i++) {
                CTSubCampaignEmailRecipient _item = gvRecipients.GetRow(i) as CTSubCampaignEmailRecipient;
                if ((Convert.ToBoolean(_item.mail_to) && !string.IsNullOrEmpty(_item.email)))
                    _CheckedEmails++;
            }

            return _CheckedEmails;
        }
        private int CountSMSRecipients()
        {
            int _CheckedSMS = 0;
            for (int i = 0; i < gvRecipients.RowCount; i++)
            {
                CTSubCampaignEmailRecipient _item = gvRecipients.GetRow(i) as CTSubCampaignEmailRecipient;
                if ((Convert.ToBoolean(_item.sms) && !string.IsNullOrEmpty(_item.mobile_no)))
                    _CheckedSMS++;
            }

            return _CheckedSMS;
        }
        private int CountBCCRecipients()
        {
            int _CheckedBCC = 0;
            for (int i = 0; i < gvRecipients.RowCount; i++)
            {
                CTSubCampaignEmailRecipient _item = gvRecipients.GetRow(i) as CTSubCampaignEmailRecipient;
                if ( Convert.ToBoolean(_item.mail_bcc) )
                    _CheckedBCC++;
            }

            return _CheckedBCC;
        }
        private string SendEmail(string pFile = "", string pMimeType = "application/pdf")
        {
            /**
             * DAN . 4.29.30: Send Email is aleady implemented on server side. Remove any trace of sending email on app as per advice.
             */


            /*
            WaitDialog.Show("Sending mails to recipients ...");
            BrightVision.Mandrill.MandrillEx mandrillEx = new BrightVision.Mandrill.MandrillEx();

            string mailTo = "";
            string mailCC = "";
            string mailBCC = "";
            string mailAttachmentUrl = (pFile != "") ? "1" : "";
            string SMS = "";
            bool bolProceed = false;

            string xml = @"<message>
                                <sender_email>@@sender_mail</sender_email>
                                <email_to>@@emailTo</email_to>
                                <email_cc>@@emailCC</email_cc>
                                <email_bcc>@@emailBCC</email_bcc>
                                <subject>@@subject</subject>
                                <body>@@body</body>
                                <attachment_url>@@attachment_url</attachment_url>
                        </message>";

            for (int i = 0; i < gvRecipients.RowCount; i++) {
                CTSubCampaignEmailRecipient _item = gvRecipients.GetRow(i) as CTSubCampaignEmailRecipient;
                if (this.IsNull(_item.email, "") != "") {
                    //Get mailBCC
                    if (Convert.ToBoolean(this.IsNull(_item.mail_bcc, "0")))
                    {
                        mandrillEx.BCC.Add(this.IsNull(_item.email, ""), this.IsNull(_item.fullname, ""));

                        if (mailBCC != "")
                            mailBCC += "\n";

                        mailBCC += "<email>" + _item.email + "</email>";
                    }


                    //Get mailTo
                    if (Convert.ToBoolean(this.IsNull(_item.mail_to, "0"))) {
                        if (mandrillEx.TO.ContainsKey(_item.email))
                            NotificationDialog.Information("Bright Sales", string.Format("Duplicate email found for {0}. Will skip adding this to recipients list.", _item.email));

                        else {
                            mandrillEx.TO.Add(this.IsNull(_item.email, ""), this.IsNull(_item.fullname, ""));

                            if (mailTo != "") 
                                mailTo += "\n";

                            mailTo += "<email>" + _item.email + "</email>";
                            bolProceed = true;
                        }
                    }
                }
            }

            xml = xml.Replace("@@sender_mail", mandrillEx.From)
                    .Replace("@@emailTo", mailTo)
                    .Replace("@@emailCC", mailCC)
                    .Replace("@@emailBCC", mailBCC)
                    .Replace("@@subject", tbxMailSubject.Text)
                    .Replace("@@body", tbxMailContent.Text)
                    .Replace("@@attachment_url", mailAttachmentUrl);

            long? message_log_id = null;
            message_log_id = this.SaveMail(xml);

            if (bolProceed) {
                mandrillEx.Subject = tbxMailSubject.Text;
                mandrillEx.MessageTEXT = tbxMailContent.Text.Replace("{send_to}", m_BrightSalesProperty.CommonProperty.CustomerName).Replace("\r\n","\\n");
                if (pFile != "") 
                    mandrillEx.Attachment.Add(pMimeType, pFile);
                //WaitDialog.Close();
                //WaitDialog.Show("Sending email");
                mandrillEx.Send(message_log_id);
            }
            
            this.DeleteCreatedPDF(mandrillEx);
            mandrillEx = null;
            

            WaitDialog.Close();
            return xml;
            */
            return "";
        }
        private void SendSMS(string pFile = "", string pMimeType = "application/pdf")
        {
            /*Sample XML
             * <SMS>
	                <contact>
		                <name>Dan</name>
		                <number>+6394992380</number>
	                </contact>
	                <contact>
		                <name>Venus</name>
		                <number>+639069163504</number>
	                </contact>
                </SMS>;
             */

            WaitDialog.Show("Sending sms to recipients ...");

            bool bolProceed = false;

            string smsRecipients = "";
            string xml = "";
            for (int i = 0; i < gvRecipients.RowCount; i++)
            {
                CTSubCampaignEmailRecipient _item = gvRecipients.GetRow(i) as CTSubCampaignEmailRecipient;

                if (this.IsNull(_item.mobile_no, "") != "")
                {
                    if (Convert.ToBoolean(this.IsNull(_item.sms, "0")))
                    {

                        if (xml != "") xml += "\n";
                        xml += "<contact><name>" + _item.fullname + "</name><number>" + _item.mobile_no + "</number></contact>";

                        if (smsRecipients != "") smsRecipients += "\n";
                        smsRecipients += _item.fullname;

                        bolProceed = true;
                    }
                }
            }

            if (bolProceed)
            {
                xml = "<SMS>" + xml + "</SMS>";
                LogEventSMS(xml, tbxMailSubject.Text);
                NotificationDialog.Information("Bright Sales", "SMS send to the following:\n" + smsRecipients);
            }

            WaitDialog.Close();
        }
        private string IsNull(object obj, string defaultValue)
        {
            if (obj == null)
                return defaultValue;
            else
                return obj.ToString();
        }
        /*
        private void SaveMail(string xml)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_emails _eftSubCampaignEmails = new sub_campaign_emails() {
                    account_id = m_BrightSalesProperty.CommonProperty.AccountId,
                    created_by = UserSession.CurrentUser.UserId,
                    created_on = DateTime.Now,
                    mail_type = (short)eMailType.Send_SMS_To_Customer,
                    sub_campaign_id = m_BrightSalesProperty.CommonProperty.SubCampaignId,
                    sent_by = UserSession.CurrentUser.UserId,
                    sent_on = DateTime.Now,
                    mail_thread_id = Guid.NewGuid(),
                    mail_content = xml
                };
                _efDbContext.sub_campaign_emails.AddObject(_eftSubCampaignEmails);
                _efDbContext.SaveChanges();
                _efDbContext.Detach(_eftSubCampaignEmails);
            }
        }
        */

        private long? SaveMail(string pXml)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                message_log _eftMessageLog = new message_log() {
                    message_type = (int)BrightVision.Common.Classes.SendMail.eMailType.Send_Mail_To_Prospect,
                    sub_campaign_id = m_BrightSalesProperty.CommonProperty.SubCampaignId,
                    company_id = m_BrightSalesProperty.CommonProperty.AccountId,
                    user_id = UserSession.CurrentUser.UserId,
                    XML = pXml,
                    created_date = DateTime.Now
                };
                _efDbContext.message_log.AddObject(_eftMessageLog);
                _efDbContext.SaveChanges();
                _efDbContext.Detach(_eftMessageLog);
                return _eftMessageLog.id;
            }
        }

        private void LogEventSMS(string pXML, string pMessage)
        {
            string Source = "BrightSales";
            //if (Application.ProductName == "ManagerApplication") Source = "BrightManager";

            BrightPlatformEntities m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);

            m_efDbContext.event_log.AddObject(
               new event_log()
               {
                   event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.SEND_SMS,
                   user_id = UserSession.CurrentUser.UserId,
                   subcampaign_id = m_BrightSalesProperty.CommonProperty.SubCampaignId,
                   account_id = m_BrightSalesProperty.CommonProperty.AccountId,
                   contact_id = null,
                   local_datetime = DateTime.Now,
                   computer_name = UserSession.CurrentUser.ComputerName,
                   param1 = pXML,
                   param2 = pMessage,
                   param3 = Source,
                   param4 = UserSession.CurrentUser.UserId.ToString(),
                   param5 = UserSession.CurrentUser.ComputerIP,
                   param6 = null
               }
            );

            m_efDbContext.SaveChanges();
        }
        private void DeleteCreatedPDF(BrightVision.Mandrill.MandrillEx mandrillEx)
        {
            try
            {
                foreach (KeyValuePair<string, string> pair in mandrillEx.Attachment) {
                    if (File.Exists(pair.Value))
                        File.Delete(pair.Value);
                }
            }
            catch {
                BrightVision.Common.UI.NotificationDialog.Error("Error", "An error has encountered when trying to delete temporarily created attachment.\nPlease contact system administrator.");
            }
        }
        private void GetDefaultSubjectAndMessage()
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                subcampaign _eftSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == m_BrightSalesProperty.CommonProperty.SubCampaignId);
                if (_eftSubCampaign != null) {
                    if (!string.IsNullOrEmpty(_eftSubCampaign.xml_config_data)) {
                        tbxMailSubject.Text = XmlUtility.GetXmlNodeInnerText(_eftSubCampaign.xml_config_data, "/sub_campaign_config/general_settings/send_mail_settings/mail_subject");
                        tbxMailContent.Text = XmlUtility.GetXmlNodeInnerText(_eftSubCampaign.xml_config_data, "/sub_campaign_config/general_settings/send_mail_settings/mail_body", true);
                    }
                    _efDbContext.Detach(_eftSubCampaign);
                }
            }
        }
        private string GetSerializedSendEmailInfo()
        {
            ReportsProperty.SendEmailInfoCollection _Collection = new ReportsProperty.SendEmailInfoCollection() {
                SenderEmail = UserSession.CurrentUser.UserEmail,
                SenderName = UserSession.CurrentUser.UserFullName,
                MailSubject = tbxMailSubject.Text,
                MailContent = tbxMailContent.Text.Replace("{send_to}", m_BrightSalesProperty.CommonProperty.CustomerName).Replace("\r\n", "\\n")
            };

            for (int i = 0; i < gvRecipients.RowCount; i++) {
                CTSubCampaignEmailRecipient _item = gvRecipients.GetRow(i) as CTSubCampaignEmailRecipient;
                if (!string.IsNullOrEmpty(_item.email)) {
                    bool _AsTo = _item.mail_to == null ? false : (bool)_item.mail_to;
                    bool _AsCC = _item.mail_cc == null ? false : (bool)_item.mail_cc;
                    bool _AsBCC = _item.mail_bcc == null ? false : (bool)_item.mail_bcc;

                    if (_AsTo)
                        _Collection.SendToContacts.Add(new ReportsProperty.SendEmailRecipientInfoProperty() { Name = _item.fullname, EmailAddress = _item.email });
                    if (_AsCC)
                        _Collection.CarbonCopyContacts.Add(new ReportsProperty.SendEmailRecipientInfoProperty() { Name = _item.fullname, EmailAddress = _item.email });
                    if (_AsBCC)
                        _Collection.BlindCarbonCopyContacts.Add(new ReportsProperty.SendEmailRecipientInfoProperty() { Name = _item.fullname, EmailAddress = _item.email });
                }
            }

            return SerializeUtility.Serialize(_Collection);
        }
        private string GetSerializedSendSmsInfo()
        {
            ReportsProperty.SendSmsInfoCollection _Collection = new ReportsProperty.SendSmsInfoCollection() {
                SmsMessage = tbxMailSubject.Text,
                ComputerName = UserSession.CurrentUser.ComputerName,
                ComputerIP = UserSession.CurrentUser.ComputerIP
            };

            for (int i = 0; i < gvRecipients.RowCount; i++) {
                CTSubCampaignEmailRecipient _item = gvRecipients.GetRow(i) as CTSubCampaignEmailRecipient;
                if (!string.IsNullOrEmpty(_item.mobile_no)) {
                    bool _SendSms = _item.sms == null ? false : (bool)_item.sms;
                    if (_SendSms)
                        _Collection.SmsContacts.Add(new ReportsProperty.SendSmsRecipientInfoProperty() { Name = _item.fullname, MobileNo = _item.mobile_no });
                }
            }

            return SerializeUtility.Serialize(_Collection);
        }
        #endregion

        #region Control Events
        private void FrmSendEmailRecipients_Load(object sender, EventArgs e)
        {
            this.GetRecipients();
            this.GetReports();
            WaitDialog.Close();
        }
        private void btnPreview_Click(object sender, EventArgs e)
        {
            btnPreview.Enabled = false;
            if (Convert.ToInt32(cboReports.EditValue) < 1) {
                NotificationDialog.Information("Bright Sales", "Please select a report to preview.");
                return;
            }

            WaitDialog.Show("Sending web service request ...");
            List<int> _SubCampaignIds = new List<int>();
            _SubCampaignIds.Add(m_BrightSalesProperty.CommonProperty.SubCampaignId);

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efDbContext.FIClearUserReuests(UserSession.CurrentUser.UserId);
                Guid _RequestId = Guid.NewGuid();
                serverside_report_requests _eftRequest = new serverside_report_requests() {
                    id = _RequestId,
                    calling_environment = (short)ReportsUtility.eCallingEnvironment.BrightSales_SendEmail,
                    display_mode = (short)ReportsUtility.eDisplayMode.AccountsContacts_WithDialogData,
                    campaign_info = string.Format("{0}>{1}>{2}",
                        m_BrightSalesProperty.CommonProperty.CustomerName,
                        m_BrightSalesProperty.CommonProperty.CampaignName,
                        m_BrightSalesProperty.CommonProperty.SubCampaignName
                    ),
                    sub_campaign_ids = string.Join(",", _SubCampaignIds),
                    view_config_id = Convert.ToInt32(cboReports.EditValue),
                    account_id = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                    requested_by = UserSession.CurrentUser.UserId,
                    requested_on = DateTime.Now
                };
                _efDbContext.serverside_report_requests.AddObject(_eftRequest);
                _efDbContext.SaveChanges();
                _efDbContext.Detach(_eftRequest);
                ReportsUtility.SendReportRequest(_RequestId.ToString());
            }
            WaitDialog.Close();
            System.Threading.Thread.Sleep(2000);
            btnPreview.Enabled = true;

            //if (Convert.ToInt32(cboReports.EditValue) < 1) {
            //    NotificationDialog.Information("Bright Sales", "Please select a report to preview.");
            //    return;
            //}

            //m_SendEmailButtonPressed = false;
            //this.Cursor = Cursors.WaitCursor;
            //WaitDialog.Show("Loading report preview ...");
            //m_Reports = new ReportsUtility() {
            //    CampaignInfo = string.Format("{0}>{1}>{2}",
            //        m_BrightSalesProperty.CommonProperty.CustomerName,
            //        m_BrightSalesProperty.CommonProperty.CampaignName,
            //        m_BrightSalesProperty.CommonProperty.SubCampaignName
            //    ),
            //    LSubCampaignData = new List<ReportsUtility.SubcampaignData>(),
            //    CallingEnvironment = ReportsUtility.eCallingEnvironment.BrightSales_SendEmail,
            //    DisplayMode = ReportsUtility.eDisplayMode.AccountsContacts_WithDialogData,
            //    AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId
            //};
            //List<int> _SubCampaignIds = new List<int>();
            //_SubCampaignIds.Add(m_BrightSalesProperty.CommonProperty.SubCampaignId);
            //m_Reports.OnReportPageCompleted += m_Reports_OnReportPageCompleted;
            //DevExpress.XtraTab.XtraTabControl _DummyTab = new DevExpress.XtraTab.XtraTabControl();
            //m_Reports.GenerateReportPages(ref _DummyTab, _SubCampaignIds.ToArray(), Convert.ToInt32(cboReports.EditValue));
        }
        private void m_Reports_OnReportPageCompleted(object sender, EventArgs e)
        {
            m_Reports.OnReportPageCompleted -= m_Reports_OnReportPageCompleted;

            /**
             * report preview.
             */
            if (!m_SendEmailButtonPressed)
                m_Reports.ReportPagePreview();

            /**
             * send email.
             */
            else {
                if (this.CountRecipients() > 0) {
                    string _FileName = m_Reports.GenerateReports();
                    this.SendEmail(_FileName);
                }
                if (this.CountSMSRecipients() > 0)
                    this.SendSMS();
            }

            this.Cursor = Cursors.Default;
            WaitDialog.Close();

            if (m_SendEmailButtonPressed)
                this.Close();
        }
        private void btnSendEmail_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            btnSendEmail.Enabled = false;

            int iRecipients = this.CountRecipients();
            int iSMSRecipients = this.CountSMSRecipients();

            if (string.IsNullOrEmpty(tbxMailSubject.Text)) {
                NotificationDialog.Error("Bright Sales", "Please specify your mail subject.");
                tbxMailSubject.Focus();
                return;
            }
            else if (string.IsNullOrEmpty(tbxMailContent.Text)) {
                NotificationDialog.Error("Bright Sales", "Please specify your mail message.");
                tbxMailContent.Focus();
                return;
            }
            else if (iRecipients < 1 && iSMSRecipients < 1) {
                NotificationDialog.Error("Bright Sales", string.Format("No emails/sms selected.{0}No emails/sms will be sent.", Environment.NewLine));
                gvRecipients.Focus();
                return;
            }

            if (iRecipients > 0 || iSMSRecipients > 0) {
                WaitDialog.Show("Sending web service request.");
                Guid _RequestId = Guid.NewGuid();
                string _SendEmailInfo = this.GetSerializedSendEmailInfo();
                string _SendSmsInfo = this.GetSerializedSendSmsInfo();

                List<int> _SubCampaignIds = new List<int>();
                _SubCampaignIds.Add(m_BrightSalesProperty.CommonProperty.SubCampaignId);
                
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    serverside_report_requests _eftRequest = new serverside_report_requests() {
                        id = _RequestId,
                        calling_environment = (short)ReportsUtility.eCallingEnvironment.BrightSales_SendEmail,
                        display_mode = (short)ReportsUtility.eDisplayMode.AccountsContacts_WithDialogData,
                        campaign_info = string.Format("{0}>{1}>{2}",
                            m_BrightSalesProperty.CommonProperty.CustomerName,
                            m_BrightSalesProperty.CommonProperty.CampaignName,
                            m_BrightSalesProperty.CommonProperty.SubCampaignName
                        ),
                        sub_campaign_ids = string.Join(",", _SubCampaignIds),
                        view_config_id = Convert.ToInt32(cboReports.EditValue) > 0? Convert.ToInt32(cboReports.EditValue): 0,
                        account_id = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                        send_email_info = _SendEmailInfo,
                        send_sms_info = _SendSmsInfo,
                        requested_by = UserSession.CurrentUser.UserId,
                        requested_on = DateTime.Now
                    };
                    _efDbContext.serverside_report_requests.AddObject(_eftRequest);
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftRequest);
                }

                if (iRecipients > 0)
                    ReportsUtility.SendMailRequest(_RequestId.ToString());

                WaitDialog.Close();
                this.Close();
            }

            /**
             * when we need to send pdf attachments, we go here.
             * else, send email without attachments by calling the SendEmail() method directly.
             */
            //if (iRecipients > 0 && Convert.ToInt32(cboReports.EditValue) > 0) {
            //m_SendEmailButtonPressed = true;
            //this.Cursor = Cursors.WaitCursor;
            //WaitDialog.Show("Sending mails to recipients ...");
            //m_Reports = new ReportsUtility() {
            //    CampaignInfo = string.Format("{0}>{1}>{2}",
            //        m_BrightSalesProperty.CommonProperty.CustomerName,
            //        m_BrightSalesProperty.CommonProperty.CampaignName,
            //        m_BrightSalesProperty.CommonProperty.SubCampaignName
            //    ),
            //    LSubCampaignData = new List<ReportsUtility.SubcampaignData>(),
            //    CallingEnvironment = ReportsUtility.eCallingEnvironment.BrightSales_SendEmail,
            //    DisplayMode = ReportsUtility.eDisplayMode.AccountsContacts_WithDialogData,
            //    AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
            //    CallingApplication = ReportsUtility.eCallingApplication.BrightSales
            //};
            //List<int> _SubCampaignIds = new List<int>();
            //_SubCampaignIds.Add(m_BrightSalesProperty.CommonProperty.SubCampaignId);
            //m_Reports.OnReportPageCompleted += m_Reports_OnReportPageCompleted;
            //DevExpress.XtraTab.XtraTabControl _DummyTab = new DevExpress.XtraTab.XtraTabControl();
            //m_Reports.GenerateReportPages(ref _DummyTab, _SubCampaignIds.ToArray(), Convert.ToInt32(cboReports.EditValue));
            //}

            //else
            //{
            //    m_SendEmailButtonPressed = true;

            //    if (iRecipients > 0)
            //        this.SendEmail();

            //    if (iSMSRecipients > 0)
            //        this.SendSMS();

            //    if (m_SendEmailButtonPressed)
            //        this.Close();
            //}

            this.Cursor = Cursors.Default;
            btnSendEmail.Enabled = true;
        }
        #endregion

        #region Initialized Events
        private void InitEvents()
        {
            repositoryItemCheckEditBCC.EditValueChanged += repositoryItemCheckEditBCC_EditValueChanged;
        }

        void repositoryItemCheckEditBCC_EditValueChanged(object sender, EventArgs e)
        {
            int iCountBCC = CountBCCRecipients();
            CheckEdit chk = (CheckEdit)sender;

            if (chk.Checked && iCountBCC > 0)
            {
                repositoryItemCheckEditBCC.EditValueChanged -= repositoryItemCheckEditBCC_EditValueChanged;

                for (int i = 0; i < gvRecipients.RowCount; i++)
                {
                    CTSubCampaignEmailRecipient _item = gvRecipients.GetRow(i) as CTSubCampaignEmailRecipient;
                    if (Convert.ToBoolean(_item.mail_bcc))
                        _item.mail_bcc = false;
                }

                chk.Checked = true;

                repositoryItemCheckEditBCC.EditValueChanged += repositoryItemCheckEditBCC_EditValueChanged;
            }
        }
        #endregion
    }
}