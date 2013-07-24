
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesConsultant.PublicProperties
{
    public class ClassesProperty
    {
        public class ManualMatchAccount {
            public long fuzzy_lookup_account_id { get; set; }
            public int match_account_id { get; set; }
            public string import_data_company_name { get; set; }
            public string master_data_company_name { get; set; }
            public bool is_match { get; set; }

            public string org_no { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public string zip_code { get; set; }
            public string country { get; set; }
            public string telephone { get; set; }
            public bool validated { get; set; }
        }
    }
}
