
#region Namespace
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Model;
using BrightVision.Common.Business;
using System.Linq;
using System.Collections;
using BrightVision.Common.Events.Core;
using SalesConsultant.Facade;
using SalesConsultant.Events;
using SalesConsultant.PublicProperties;
using BrightVision.Common.UI;
#endregion

namespace SalesConsultant.Modules
{
    public partial class LinkedIn : DevExpress.XtraEditors.XtraUserControl
    {
        //private ArrayList history = new ArrayList();
        private Dictionary<int, string> history = new Dictionary<int, string>();
        private int iCurrentHistoryIndex = 0;
        private string m_linkedin = null;
        private int m_contactId = 0;

        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        #region Constructor
        public LinkedIn()
        {
            InitializeComponent();
            //SuppressScriptErrorsOnly(wbGoogleSearch);
        }
        #endregion

        #region Public Methods
        public void LoadLinkedIn(int accountid, int contactId)
        {
            ResetValues();
            m_contactId = contactId;

            this.Cursor = Cursors.WaitCursor;
            this.Enabled = false;
            try
            {
                if (accountid > 0 && contactId > 0)
                {
                    using (BrightPlatformEntities _efDbCOntext = new BrightPlatformEntities(UserSession.EntityConnection))
                    {
                        string companyname = "";
                        string contactname = "";

                        account _eftCompany = _efDbCOntext.accounts.FirstOrDefault(i => i.id == accountid);
                        if (_eftCompany != null)
                        {
                            companyname = _eftCompany.company_name;
                            _efDbCOntext.Detach(_eftCompany);
                            //_Url = _eftCompany.www;
                        }

                        contact _eftContacts = _efDbCOntext.contacts.FirstOrDefault(i => i.id == contactId);
                        if (_eftContacts != null)
                        {
                            contactname = _eftContacts.first_name + " " + _eftContacts.last_name;
                            m_linkedin = _eftContacts.linkedin_url;
                            _efDbCOntext.Detach(_eftContacts);
                            //_Url = _eftCompany.www;
                        }

                        string searchString = contactname + " " + companyname;

                        if (BrightVision.Common.Utilities.ValidationUtility.IFNullString(m_linkedin,"").Trim() != "")
                            wbLinkedIn.Url = new Uri(m_linkedin);
                        else
                            wbLinkedIn.Url = new Uri("http://www.linkedin.com/search/fpsearch?type=people&keywords=" + searchString + "&pplSearchOrigin=GLHD&pageKey=fps_results");                        
                    }
                }
                else
                {

                    wbLinkedIn.DocumentText =
                        "<div style=\"padding:40px 40px 40px 40px;\">" +
                        "<div style=\"background-color:#d9e2b3;border:1px solid #ccc;padding:40px 40px 40px 40px;font-family:arial;" +
                        "font-size:12px;font-weight:bold;\"><center>No LinkedIn to be search for.</center>" +
                        "<br/><center><span style=\"font-weight:normal;font-style:italic;margin-top:20px;font-size:9px;\">" +
                        "Brightvision - We accelerate your sales.</span></center></div></div></div>";

                }
            }
            catch { }

            this.Enabled = true;
            this.Cursor = Cursors.Default;

        }
        #endregion

        #region Events
        private void wbLinkedIn_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            txtAddressBar.Text = wbLinkedIn.Url.ToString();

            if (history.Count == 0 && txtAddressBar.Text == "about:blank") return;

            if (!history.ContainsValue(txtAddressBar.Text))
            {
                iCurrentHistoryIndex = history.Count;
                history.Add(iCurrentHistoryIndex, txtAddressBar.Text);

                SetBackForwardButtons();
            }

            SaveLinkedInProfile();

            if (m_linkedin != null && m_linkedin != "") btnLoadContact.Enabled = true;
            else btnLoadContact.Enabled = false;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            iCurrentHistoryIndex -= 1;
            string url = "";
            history.TryGetValue(iCurrentHistoryIndex, out url);
            wbLinkedIn.Url = new Uri(url);
            SetBackForwardButtons();
            this.Cursor = Cursors.Default;
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            iCurrentHistoryIndex += 1;
            string url = "";
            history.TryGetValue(iCurrentHistoryIndex, out url);
            wbLinkedIn.Url = new Uri(url);

            SetBackForwardButtons();
            this.Cursor = Cursors.Default;
        }

        private void btnSaveToContact_Click(object sender, EventArgs e)
        {
            try
            {
                bool _editable = true;
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
                {
                    sub_campaign_account_lists _eftCurrentCompany = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i => i.final_list_id == m_BrightSalesProperty.CommonProperty.FinalListId && i.account_id == m_BrightSalesProperty.CommonProperty.AccountId);
                    if (_eftCurrentCompany != null)
                    {
                        if (_eftCurrentCompany.locked_by != null && _eftCurrentCompany.locked_by != UserSession.CurrentUser.UserId)
                            _editable = false;

                        _efDbContext.Detach(_eftCurrentCompany);
                    }
                }

                if (!_editable)
                {
                    NotificationDialog.Error("Bright Sales", "Currently worked by another user.");
                    return;
                }


                if (txtAddressBar.Text.Trim() == "" && BrightVision.Common.Utilities.ValidationUtility.IFNullString(wbLinkedIn.Url, "").Contains("/profile/view?id="))
                    txtAddressBar.Text = wbLinkedIn.Url.ToString();

                if (!CheckValidURL(txtAddressBar.Text))
                {
                    NotificationDialog.Error("Error", "Invalid URL");
                    return;
                }


                using (BrightPlatformEntities _efDbCOntext = new BrightPlatformEntities(UserSession.EntityConnection))
                {
                    contact _eftContacts = _efDbCOntext.contacts.FirstOrDefault(i => i.id == m_contactId);
                    if (_eftContacts != null)
                    {
                        _eftContacts.linkedin_url = txtAddressBar.Text;
                    }
                    _efDbCOntext.SaveChanges();

                    _efDbCOntext.Detach(_eftContacts);

                    m_linkedin = txtAddressBar.Text;
                    btnLoadContact.Enabled = true;

                    if (m_BrightSalesProperty.CommonProperty.CurrentTab == SelectionProperty.CurrentTab.CampaignBooking)
                    {
                        m_EventBus.Notify(new LinkedInEvents.OnSave.ManageCampaignBooking()
                        {
                            OnSaveArgs = new LinkedInEvents.LinkedInArgs()
                            {
                                LinkedInUrl = txtAddressBar.Text
                            }
                        });
                    }
                }

                NotificationDialog.Information("Bright Sales", "Successfully updated LinkedIn url.");
            }
            catch(Exception ex)
            {
                NotificationDialog.Error("Bright Sales", "An error has been encountered while trying to save LinkedIn url.\nPlease consult system administrator.");
            }
        }

        private void btnLoadContact_Click(object sender, EventArgs e)
        {
            wbLinkedIn.Url = new Uri(m_linkedin);
        }

        private void txtAddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (txtAddressBar.Text.Trim().Length > 0)
                    {
                        if (txtAddressBar.Text.IndexOf("http://") == -1)
                            txtAddressBar.Text = "http://" + txtAddressBar.Text;
                    }

                    wbLinkedIn.Url = new Uri(txtAddressBar.Text);
                    
                }
                catch
                {
                    wbLinkedIn.DocumentText =
                         "<div style=\"padding:40px 40px 40px 40px;\">" +
                         "<div style=\"background-color:#d9e2b3;border:1px solid #ccc;padding:40px 40px 40px 40px;font-family:arial;" +
                         "font-size:12px;font-weight:bold;\"><center>No LinkedIn to be search for.</center>" +
                         "<br/><center><span style=\"font-weight:normal;font-style:italic;margin-top:20px;font-size:9px;\">" +
                         "Brightvision - We accelerate your sales.</span></center></div></div></div>";
                }
            }
        }
        #endregion

        #region Private Method
        private void ResetValues()
        {
            btnSaveToContact.Enabled = false;
            btnLoadContact.Enabled = false;
            history.Clear();
            iCurrentHistoryIndex = 0;
            m_linkedin = null;
            m_contactId = 0;

            btnBack.Enabled = false;
            btnForward.Enabled = false;
            this.btnBack.Image = global::SalesConsultant.Properties.Resources.back_disable;
            this.btnForward.Image = global::SalesConsultant.Properties.Resources.forward_disable;

            wbLinkedIn.Url = null;
            txtAddressBar.Text = "";

            Application.DoEvents();
        }

        private void SetBackForwardButtons()
        {
            if (iCurrentHistoryIndex > 0)
            {
                btnBack.Enabled = true;
                btnBack.Image = global::SalesConsultant.Properties.Resources.back;
            }
            else
            {
                btnBack.Enabled = false;
                btnBack.Image = global::SalesConsultant.Properties.Resources.back_disable;
            }

            if (iCurrentHistoryIndex < history.Count - 1)
            {
                btnForward.Enabled = true;
                btnForward.Image = global::SalesConsultant.Properties.Resources.forward;
            }
            else
            {
                btnForward.Enabled = false;
                btnForward.Image = global::SalesConsultant.Properties.Resources.forward_disable;
            }
        }

        private void SaveLinkedInProfile()
        {
            if (BrightVision.Common.Utilities.ValidationUtility.IFNullString(wbLinkedIn.Url, "").Contains("/profile/view?id=") && int.Parse(BrightVision.Common.Utilities.ValidationUtility.IFNullString(m_contactId, "0")) > 0)
            {
                txtAddressBar.Text = wbLinkedIn.Url.ToString();
                btnSaveToContact.Enabled = true;
            }
            else
            {
                btnSaveToContact.Enabled = false;
            }
        }

        private bool CheckValidURL(string url)
        {
            try
            {
                Uri _uri = new Uri(url);

                return true;
                     
            }
            catch { }
            return false;

        }
        #endregion



    }
}
