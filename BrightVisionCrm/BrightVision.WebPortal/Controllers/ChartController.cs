using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Configuration;

using BrightVision.Model;
using BrightVision.Reporting.Utility;
using BrightVision.Common.Utilities;
using BrightVision.Common.Classes;
using BrightVision.WebPortal.Facade;

namespace BrightVision.WebPortal.Controllers
{
    public class ChartController : Controller
    {
        //
        // GET: /Chart/
        public class Datas
        {
            public string name { get; set; }
            public int value { get; set; }
        }

        public ActionResult Index()
        {            
            /**
             * connection info.
             */
            #region Code Logic
            string _Connection = ConfigurationManager.ConnectionStrings["DefaultEntityConnection"].ToString();
            if (string.IsNullOrEmpty(_Connection))
                Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "No available connection. Please kindly contact your administrator." }));
            #endregion

            string sdata = null;
            string _title = "";
            try
            {
                int _subcampaignid = int.Parse(ValidationUtility.IFNullString(Request.QueryString["subcampaignid"], "0"));
                int _userid = int.Parse(ValidationUtility.IFNullString(Request.QueryString["userid"], "0"));
                //_title = ValidationUtility.IFNullString(Request.QueryString["title"], "");

                if (_subcampaignid <= 0 || _userid <= 0)
                {
                    Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = "Invalid SubcampaignId or UserId." }));

                }

                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(_Connection))
                {
                    subcampaign sc = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == _subcampaignid);
                    if (sc != null)
                    {
                        if (ValidationUtility.IFNullString(sc.title, "") != "")
                        {
                            _title = sc.title;
                        }

                        campaign c = _efDbContext.campaigns.FirstOrDefault(i => i.id == sc.campaign_id);
                        _efDbContext.Detach(sc);

                        if (c != null)
                        {
                            if (ValidationUtility.IFNullString(c.campaign_name, "") != "")
                            {
                                _title = c.campaign_name + " > " + _title;
                            }

                            customer cus = _efDbContext.customers.FirstOrDefault(i => i.id == c.customer_id);
                            _efDbContext.Detach(c);

                            if (cus != null)
                            {
                                if (ValidationUtility.IFNullString(cus.customer_name, "") != "")
                                {
                                    _title = cus.customer_name + " > " + _title;
                                }
                                _efDbContext.Detach(cus);
                            }
                        }
                    }

                    _efDbContext.FIPopulateCampaignListCompanies(_subcampaignid, _userid);
                    int _final_list_id = _efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == _subcampaignid).id;

                    List<CXbvGetVwCampaignListCompaniesStatuses> data = _efDbContext.FXbvGetVwCampaignListCompaniesStatuses(_final_list_id, _userid).ToList();

                    for (int i = 0; i < data.Count; i++)
                    {
                        if (sdata != null) sdata += ", ";
                        sdata += "[\"" + data[i].Company_Status + "\", " + data[i].total + "]";
                    }
                    if (sdata != null) sdata = "[" + sdata + "]";


                        //System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                        //ViewBag.Data = jss.Serialize(data);
                }
            }
            catch (Exception ex)
            {
                Response.Redirect(Url.RouteUrl(new { action = "DisplayError", pExceptionMessage = ex.ToString() }));
            }


            //ViewBag.Data = "[[\"Open\", 321], [\"In Progress\", 370], [\"Not Qualified\", 225], [\"Follow-Up\", 35], [\"relreased\", 19], [\"Approved\", 8]]";
            ViewBag.Title = _title;
            ViewBag.Data = sdata;
            
            return View();
        }

        public ActionResult DisplayError(string pExceptionMessage)
        {
            ViewBag.ExceptionMessage = pExceptionMessage;
            return PartialView();
        }

    }
}
