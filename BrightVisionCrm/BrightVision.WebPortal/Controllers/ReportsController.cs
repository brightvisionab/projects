
using BrightVision.Model;
using BrightVision.Reporting.Utility;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;
using ICSharpCode.SharpZipLib.Zip;

namespace BrightVision.WebPortal.Controllers
{
    public class ReportsController : Controller
    {
        public ActionResult Index() 
        {
            return Content("Welcome to web service.");
        }
        public ActionResult Download(string pData)
        {
            string _Connection = ConfigurationManager.ConnectionStrings["DefaultEntityConnection"].ToString();
            if (string.IsNullOrEmpty(_Connection))
                Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "No available connection. Please kindly contact your administrator." }));

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

            try {
                /**
                 * lets check first if the config has
                 * a valid layout and params configuration.
                 */
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

                List<string> _PdfFiles = _Reports.CreatePdfPerAccount();
                string _ZipFileName = Guid.NewGuid().ToString().Replace("-", "");
                Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}.zip", _ZipFileName));
                Response.ContentType = "application/zip";
                using (ZipOutputStream _ZipStream = new ZipOutputStream(Response.OutputStream)) {
                    foreach (string _filePath in _PdfFiles) {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(_filePath);
                        ZipEntry _fileEntry = new ZipEntry(Path.GetFileName(_filePath)) {
                            Size = fileBytes.Length
                        };

                        _ZipStream.PutNextEntry(_fileEntry);
                        _ZipStream.Write(fileBytes, 0, fileBytes.Length);
                        System.IO.File.Delete(_filePath);
                    }

                    _ZipStream.Flush();
                    _ZipStream.Close();
                }
                //string _FileName = _Reports.GenerateReports();
                //byte[] _PdfData = System.IO.File.ReadAllBytes(_FileName);
                //if (System.IO.File.Exists(_FileName))
                //    System.IO.File.Delete(_FileName);

                //MemoryStream _ms = new MemoryStream(_PdfData);
                //return new FileStreamResult(_ms, "application/pdf");
            }
            catch (Exception e) {
                Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = e.ToString() }));
            }

            return Content("<h2>Downloading pdf files ...</h2>");
        }
        public FileStreamResult Show(string pData)
        {
            string _Connection = ConfigurationManager.ConnectionStrings["DefaultEntityConnection"].ToString();
            if (string.IsNullOrEmpty(_Connection))
                Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "No available connection. Please kindly contact your administrator." }));

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

            /** /
            string _FileName = "";
            _Reports.OnReportPageCompleted += (sender, e) => {
                _FileName = _Reports.GenerateReports();
            };
            /**/

            try {
                /**
                 * lets check first if the config has
                 * a valid layout and params configuration.
                 */
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

                string _FileName = _Reports.GenerateReports();
                byte[] _PdfData = System.IO.File.ReadAllBytes(_FileName);
                if (System.IO.File.Exists(_FileName))
                    System.IO.File.Delete(_FileName);

                MemoryStream _ms = new MemoryStream(_PdfData);
                return new FileStreamResult(_ms, "application/pdf");
            }
            catch (Exception e) {
                Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = e.ToString() }));
            }

            throw new Exception();
        }
        public ActionResult DisplayError(string pExceptionMessage)
        {
            ViewBag.ExceptionMessage = pExceptionMessage;
            return PartialView();
        }
    }
}
