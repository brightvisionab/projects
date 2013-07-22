
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;

using BrightVision.Model;
using BrightVision.Reporting.Utility;
using BrightVision.Common.Utilities;
using BrightVision.Common.Classes;
using BrightVision.WebPortal.Facade;
using System.Net;

namespace BrightVision.WebPortal.Controllers
{
    public class ServicesController : Controller
    {
        public ActionResult Index()
        {
            return PartialView();
        }
        public ActionResult SendEmail(string pData)
        {
            /**
             * connection info.
             */
            #region Code Logic
            string _Connection = ConfigurationManager.ConnectionStrings["DefaultEntityConnection"].ToString();
            if (string.IsNullOrEmpty(_Connection))
                Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "No available connection. Please kindly contact your administrator." }));
            #endregion

            /**
             * user and request infos.
             */
            #region Code Logic
            user _eftUser = null;
            serverside_report_requests _eftRequest = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(_Connection)) {
                try {
                    Guid _id = new Guid(pData);
                    _eftRequest = _efDbContext.serverside_report_requests.FirstOrDefault(i => i.id == _id);
                    if (_eftRequest == null)
                        Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "Invalid web service request. Please kindly contact your administrator." }));

                    _eftRequest.processed = true;
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftRequest);

                    _eftUser = _efDbContext.users.FirstOrDefault(i => i.id == _eftRequest.requested_by);
                    if (_eftUser != null)
                        _efDbContext.Detach(_eftUser);
                }
                catch {
                    Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "Invalid web service request. Please kindly contact your administrator." }));
                }
            }
            #endregion

            string _EmailStatus = "";
            string _SmsStatus = "";
            try {
                /**
                 * if has selected a report config, then generate a file name based on the
                 * selected view config id.
                 */
                #region Code Logic
                string _FileName = string.Empty;
                if (_eftRequest.view_config_id > 0) {
                    ReportsUtility _Reports = new ReportsUtility(_Connection, true) {
                        CampaignInfo = _eftRequest.campaign_info,
                        LSubCampaignData = new List<ReportsUtility.SubcampaignData>(),
                        CallingEnvironment = (ReportsUtility.eCallingEnvironment)_eftRequest.calling_environment,
                        DisplayMode = (ReportsUtility.eDisplayMode)_eftRequest.display_mode,
                        AccountId = (int)_eftRequest.account_id,
                        WebPortalRequester = _eftUser.fullname,
                        ReportsPath = ConfigurationManager.AppSettings["PdfReportsPath"].ToString(),
                        GridFilterString = _eftRequest.active_filter_string,
                        GridSortInfo = _eftRequest.sort_info,
                        GridColumnsInfo = _eftRequest.columns_info
                    };

                    string _ReturnMessage = string.Empty;
                    bool _GoodConfig = _Reports.VerifyReportTemplate((int)_eftRequest.view_config_id, ref _ReturnMessage);
                    if (!_GoodConfig && !string.IsNullOrEmpty(_ReturnMessage))
                        Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = _ReturnMessage }));

                    string[] _ids = _eftRequest.sub_campaign_ids.Split(',');
                    List<int> _SubCampaignIds = new List<int>();
                    _SubCampaignIds.Add(Convert.ToInt32(_ids[0]));
                    _Reports.GenerateReportPages(_SubCampaignIds.ToArray(), (int)_eftRequest.view_config_id);
                    if (Convert.ToInt32(_eftRequest.account_id) > 0 && !_Reports.AccountDataExists())
                        Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "No account dialog data found." }));

                    _FileName = _Reports.GenerateReports();
                }
                #endregion

                /**
                 * send email goes here.
                 */
                ReportsProperty.SendEmailInfoCollection _SendMailCollection = SerializeUtility.DeserializeFromXml<ReportsProperty.SendEmailInfoCollection>(_eftRequest.send_email_info);
                _EmailStatus = FacadeCommon.SendEmail(_FileName, _SendMailCollection, _eftRequest);

                /**
                 * send sms goes here.
                 */
                ReportsProperty.SendSmsInfoCollection _SendSmsCollection = SerializeUtility.DeserializeFromXml<ReportsProperty.SendSmsInfoCollection>(_eftRequest.send_sms_info);
                _SmsStatus = FacadeCommon.SendSMS(_SendSmsCollection, _eftRequest);

                /**
                 * delete file after use.
                 */
                if (System.IO.File.Exists(_FileName))
                    System.IO.File.Delete(_FileName);

            }
            catch (Exception e) {
                Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = e.ToString() }));
            }

            return Content(string.Format("<h2>Email Status:</h2><br />{0}<br /><br /><h2>SMS send to the following:</h2><br />{1}", _EmailStatus, _SmsStatus));
        }
        public ActionResult DisplayError(string pExceptionMessage)
        {
            ViewBag.ExceptionMessage = pExceptionMessage;
            return PartialView();
        }
        public ActionResult PlayAudio(string pData)
        {
            /**
             * connection info.
             */
            #region Code Logic
            string _Connection = ConfigurationManager.ConnectionStrings["DefaultEntityConnection"].ToString();
            if (string.IsNullOrEmpty(_Connection))
                Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "No available connection. Please kindly contact your administrator." }));
            #endregion

            /**
             * user and request infos.
             */
            #region Code Logic
            serverside_report_requests _eftRequest = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(_Connection)) {
                try {
                    Guid _id = new Guid(pData);
                    _eftRequest = _efDbContext.serverside_report_requests.FirstOrDefault(i => i.id == _id);
                    if (_eftRequest == null)
                        Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "Invalid web service request. Please kindly contact your administrator." }));

                    _eftRequest.processed = true;
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftRequest);
                }
                catch {
                    Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "Invalid web service request. Please kindly contact your administrator." }));
                }
            }
            #endregion

            Stream stream = WebRequest.Create("https://lii.blob.core.windows.net/old/0a4b23f7-eab8-4043-ad84-36b4b5d23f1e_.mp3").GetResponse().GetResponseStream();
            return new FileStreamResult(stream, "audio/mpeg");
        }
    }
}
