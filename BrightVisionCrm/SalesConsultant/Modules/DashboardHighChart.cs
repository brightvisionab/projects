
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
using System.Net;
using System.IO;
#endregion

namespace SalesConsultant.Modules
{
    public partial class DashboardHighChart : DevExpress.XtraEditors.XtraUserControl
    {        
        #region Constructor
        public DashboardHighChart()
        {
            InitializeComponent();
            //Skybound.Gecko.Xpcom.Initialize(Application.ExecutablePath + @"extensions\xulrunner"); // for example "c:\\xulrunner\\"
        }
        #endregion

        #region Public Methods
        public void LoadDashboard(int subcampaignid, int userid)
        {

            this.Cursor = Cursors.WaitCursor;
            this.Enabled = false;
            try
            {
                if (subcampaignid > 0 && userid > 0)
                {
                    string _WebPortalUrl = ConfigManager.AppSettings["WebPortalUrl"].ToString(); 
                    string _WebPortalRequest = string.Format("{0}/chart?subcampaignid={1}&userid={2}", _WebPortalUrl, subcampaignid, userid);

                    wbDashboard.Url = new Uri(_WebPortalRequest);                                           
                    //wbDashboard.DocumentStream = receiveStream;
                }
                else
                {

                    /*wbDashboard.DocumentText =
                        "<div style=\"padding:40px 40px 40px 40px;\">" +
                        "<div style=\"background-color:#d9e2b3;border:1px solid #ccc;padding:40px 40px 40px 40px;font-family:arial;" +
                        "font-size:12px;font-weight:bold;\"><center>No data to display.</center>" +
                        "<br/><center><span style=\"font-weight:normal;font-style:italic;margin-top:20px;font-size:9px;\">" +
                        "Brightvision - We accelerate your sales.</span></center></div></div></div>";*/

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
            
        }


        
        #endregion

        #region Private Method
        
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
