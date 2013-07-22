using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO;

using BrightVision.Common.Utilities;
using System.Data.SqlClient;
using DevExpress.XtraGrid.Views.Grid;
using BrightVision.Model;
using BrightVision.Common.Business;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;


using BrightVision.Reporting.UI;
using BrightVision.Reporting;
using BrightVision.Reporting.Template;
using BrightVision.DQControl.Utilities;

namespace SalesConsultant.Modules
{
    public partial class ReleaseSendMail : DevExpress.XtraEditors.XtraUserControl
    {
        #region Instances
        public static ReleaseSendMail Instance()
        {
            //if (instance == null || p_SubCampaignId != m_BrightSalesProperty.CommonProperty.SubCampaignId) {
            //    p_SubCampaignId = m_BrightSalesProperty.CommonProperty.SubCampaignId;
            //    instance = new ReleaseSendMail();                
            //}

            //if (instance == null)
            //    instance = new ReleaseSendMail();

            instance = new ReleaseSendMail();
            instance.txtSubject.Text = "";
            instance.richTextBoxMessage.Text = "";
            return instance;
        }
        #endregion

        #region Constructors
        public ReleaseSendMail()
        {
            InitializeComponent();
        }
        #endregion

        #region Private Properties
        private enum ennMailType : short {
            Send_Report_To_Customer = 0,
            Send_SMS_To_Customer,
            Send_Mail_To_Prospect,
            Send_SMS_To_Prospect
        }
        //private static int p_SubCampaignId;
        private static ReleaseSendMail instance = null;
        private static BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        #endregion

        #region Public Methods
        /**
         * add checking later for cc and bcc
         */
        public int CountRecipients()
        {
            int _CheckedEmails = 0;
            GridView view = gcRecipients.FocusedView as GridView;
            for (int i = 0; i < gvRecipients.RowCount; i++) {
                CTSubCampaignEmailRecipient _item = gvRecipients.GetRow(i) as CTSubCampaignEmailRecipient;
                if (Convert.ToBoolean(_item.mail_to) && !string.IsNullOrEmpty(_item.email))
                    _CheckedEmails++;
            }

            return _CheckedEmails;
        }
        public string ProcessSending(string file = "", string MIMEType = "application/pdf")
        {

            BrightVision.Mandrill.MandrillEx mandrillEx = new BrightVision.Mandrill.MandrillEx();

            string mailTo = "";
            string mailCC = "";
            string mailBCC = "";
            string SMS = "";
            bool bolProceed = false;

            string xml = @"<email>
	                            <type>@@type</type>
	                            <sender_mail>@@sender_mail</sender_email>
	                            <receiver_mail>@@receiver_mail</receiver_mail>
	                            <cc_mail>@@cc_mail</cc_mail>
	                            <bcc_mail>@@bcc_mail<bcc_mail>
	                            <subject>@@subject</subject>
	                            <message>@@message</message>	
                            </email>";

            GridView view = gcRecipients.FocusedView as GridView;
            for (int i = 0; i < gvRecipients.RowCount; i++)
            {
                CTSubCampaignEmailRecipient _item = gvRecipients.GetRow(i) as CTSubCampaignEmailRecipient;

                if (this.IsNull(_item.email, "") != "")
                {
                    //Get mailTo
                    if (Convert.ToBoolean(this.IsNull(_item.mail_to, "0")))
                    {
                        mandrillEx.TO.Add(this.IsNull(_item.email, ""), this.IsNull(_item.fullname, ""));

                        if (mailTo != "") mailTo += "\n";
                        mailTo += "<email>" + _item.email + "</email>";

                        bolProceed = true;
                    }
                }
            }

            //GridView view = gcRecipients.FocusedView as GridView;
            //for (int i = 0; i < view.DataRowCount; i++)
            //{
            //    DataRow row = view.GetDataRow(i);

            //    if (this.IsNull(row["email"], "") != "")
            //    {
            //        //Get mailTo
            //        if (Convert.ToBoolean(this.IsNull(row["mail_to"], "0")))
            //        {
            //            mandrillEx.TO.Add(this.IsNull(row["email"], ""), this.IsNull(row["fullname"], ""));

            //            if (mailTo != "") mailTo += "\n";
            //            mailTo += "<email>" + row["email"] + "</email>";

            //            bolProceed = true;
            //        }
            //    }
            //}

            if (bolProceed)
            {
                mandrillEx.Subject = txtSubject.Text;
                mandrillEx.MessageTEXT = richTextBoxMessage.Text;
                //mandrillEx.Attachment.Add("application/pdf", @"D:\sample.pdf");
                if (file != "") mandrillEx.Attachment.Add(MIMEType, file);
                WaitDialog.Close();
                WaitDialog.Show("Sending email");
                mandrillEx.Send();
            }
            else {
                //BrightVision.Common.UI.NotificationDialog.Information("Information", "Please kindly check at least one email address.");
                //this.DeleteCreatedPDF(mandrillEx);
                //return null;
            }

            this.DeleteCreatedPDF(mandrillEx);

            xml = xml.Replace("@@type", "mail")
                    .Replace("@@sender_mail", mandrillEx.From)
                    .Replace("@@receiver_mail", mailTo)
                    .Replace("@@mailCC", mailTo)
                    .Replace("@@mailBCC", mailTo)
                    .Replace("@@subject", txtSubject.Text)
                    .Replace("@@message", richTextBoxMessage.Text);
            mandrillEx = null;

            //SaveMail(xml);

            return xml;
        }
        #endregion

        #region Private Methods
        private void PopulateGrid()
        {
            gcRecipients.DataSource = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                gcRecipients.DataSource = _efDbContext.FIGetSubCampaignEmailRecipients(m_BrightSalesProperty.CommonProperty.SubCampaignId);
                gvRecipients.BestFitColumns();
            }

            //SqlParameter sqlParam1 = new SqlParameter("p_sub_campaign_id", p_SubCampaignId);
            //DataTable dt = DatabaseUtility.ExecuteStoredProcedure("bvGetSubCampaignEmailRecipients_sp", "SubCampaignEmailRecipients", sqlParam1);
            //gridControl.DataSource = dt;

            //gridView.Columns["mail_cc"].AppearanceHeader.ForeColor = Color.Gray;
            //gridView.Columns["mail_cc"].OptionsColumn.AllowEdit = false;
            //gridView.Columns["mail_cc"].OptionsColumn.ReadOnly = true;

            //gridView.Columns["mail_bcc"].AppearanceHeader.ForeColor = Color.Gray;
            //gridView.Columns["mail_bcc"].OptionsColumn.AllowEdit = false;
            //gridView.Columns["mail_bcc"].OptionsColumn.ReadOnly = true;

            //gridView.Columns["sms"].AppearanceHeader.ForeColor = Color.Gray;
            //gridView.Columns["sms"].OptionsColumn.AllowEdit = false;
            //gridView.Columns["sms"].OptionsColumn.ReadOnly = true;
        }
        private string IsNull(object obj, string defaultValue)
        {
            if (obj == null) 
                return defaultValue;
            else 
                return obj.ToString();
        }
        private void SaveMail(string xml)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                sub_campaign_emails _eftSubCampaignEmails = new sub_campaign_emails()
                {
                    account_id = m_BrightSalesProperty.CommonProperty.AccountId,
                    created_by = UserSession.CurrentUser.UserId,
                    created_on = DateTime.Now,
                    mail_type = (short)ennMailType.Send_SMS_To_Customer,
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
        private void DeleteCreatedPDF(BrightVision.Mandrill.MandrillEx mandrillEx)
        {
            try
            {
                //if (mandrillEx.Attachment.Count > 0)
                //{
                    foreach (KeyValuePair<string, string> pair in mandrillEx.Attachment)
                    {
                        if (File.Exists(pair.Value))
                        {
                            File.Delete(pair.Value);
                        }
                    }
                //}
            }
            catch
            {
                BrightVision.Common.UI.NotificationDialog.Error("Error", "An error has encountered when trying to delete temporarily created attachment.\nPlease contact system administrator.");
            }
        }
        #endregion

        #region Control Events
        private void ReleaseSendMail_Load(object sender, EventArgs e)
        {
            this.PopulateGrid();
        }
        #endregion
    }
}
