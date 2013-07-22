using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesConsultant.Business
{
    public class CampaignProperty
    {
        public int  CustomerId{get;set;}
        public string  CustomerName  {get;set;}
        public int  CampaignId  {get;set;}
        public string  CampaignName   {get;set;}
        public int  SubCampaignId {get;set;}
        public string  SubCampaignName   {get;set;}
        public AccountDetails Account { get; set; }
    }
}
