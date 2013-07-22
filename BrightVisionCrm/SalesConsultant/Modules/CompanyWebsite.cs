
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
#endregion

namespace SalesConsultant.Modules
{
    public partial class CompanyWebsite : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Properties
        public bool ReloadPage { get; set; }
        #endregion

        #region Constructor
        public CompanyWebsite()
        {
            InitializeComponent();
            ReloadPage = false;
            //SuppressScriptErrorsOnly(wbCompanyWebsite);
        }
        #endregion

        #region Object Events
        private void cmdOpenBrowser_Click(object sender, EventArgs e)
        {
            if (theUri != null)
                System.Diagnostics.Process.Start(theUri.ToString());
        }
        #endregion

        #region Private Properties
        Uri theUri = null;
        private int m_AccountId = 0;
        private int m_PreviousAccountId = 0;
        #endregion

        #region Public Methods
        public void Show(int pAccountId)
        {
            m_PreviousAccountId = m_AccountId;
            m_AccountId = pAccountId;
            if (m_AccountId != m_PreviousAccountId || ReloadPage) {
                this.LoadCompanyWebsite(null);
                ReloadPage = false;
            }
        }
        public void Clear()
        {
            wbCompanyWebsite.Url = null;
        }
        public void LoadCompanyWebsite(string pCompanyWebsite = null)
        {
            string _Url = string.Empty;
            if (string.IsNullOrEmpty(pCompanyWebsite)) {
                using (BrightPlatformEntities _efDbCOntext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    account _eftCompany = _efDbCOntext.accounts.FirstOrDefault(i => i.id == m_AccountId);
                    if (_eftCompany != null) {
                        _efDbCOntext.Detach(_eftCompany);
                        _Url = _eftCompany.www;
                    }
                }
            }
            else
                _Url = pCompanyWebsite;

            theUri = null;
            if (_Url != null)
            {
                try
                {
                    if (!_Url.Contains("http://"))
                        _Url = "http://" + _Url; //"aa";                
                    if (Uri.IsWellFormedUriString(_Url, UriKind.Absolute))
                    {
                        if (Uri.TryCreate(_Url, UriKind.Absolute, out theUri))
                        {
                            wbCompanyWebsite.Url = theUri;
                        }
                        else
                        {
                            theUri = null;
                        }
                    }
                }
                catch
                {
                }
            }

            if (theUri == null) {
                wbCompanyWebsite.DocumentText =
                    "<div style=\"padding:40px 40px 40px 40px;\">" + 
                    "<div style=\"background-color:#d9e2b3;border:1px solid #ccc;padding:40px 40px 40px 40px;font-family:arial;" + 
                    "font-size:12px;font-weight:bold;\"><center>No website is currently registered for the selected company.</center>" + 
                    "<br/><center><span style=\"font-weight:normal;font-style:italic;margin-top:20px;font-size:9px;\">" + 
                    "Brightvision - We accelerate your sales.</span></center></div></div></div>";
            }
        }

        #region Old Code
        // Hides script errors without hiding other dialog boxes.
        //private void SuppressScriptErrorsOnly(WebBrowser browser) {
        //    // Ensure that ScriptErrorsSuppressed is set to false.
        //    browser.ScriptErrorsSuppressed = false;

        //    // Handle DocumentCompleted to gain access to the Document object.
        //    browser.DocumentCompleted +=
        //        new WebBrowserDocumentCompletedEventHandler(
        //            browser_DocumentCompleted);
        //}

        //private void browser_DocumentCompleted(object sender,
        //    WebBrowserDocumentCompletedEventArgs e) {
        //    ((WebBrowser)sender).Document.Window.Error +=
        //        new HtmlElementErrorEventHandler(Window_Error);
        //}

        //private void Window_Error(object sender,
        //    HtmlElementErrorEventArgs e) {
        //    // Ignore the error and suppress the error dialog box. 
        //    e.Handled = true;
        //}
        #endregion
        #endregion
    }
}
