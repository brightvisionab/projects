
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightVision.EventLog.Business
{
    public class FacadeEventLog
    {
        /**
         * sources of event logs for contact modified event (10)
         */
        public const string Source_Bright_Manager_Add_Contact = "Bright Manager Add Contact";
        public const string Source_Bright_Manager_Company_Contact = "Bright Manager Company Contact";
        public const string Source_Bright_Manager_Title_Setting = "Bright Manager Title Setting";
        public const string Source_Bright_Manager_Master_Data_Import = "Bright Manager Master Data Import";
        public const string Source_Bright_Manager_Import_List = "Bright Manager Import List";
        public const string Source_Bright_Sales_Contact_Information = "Bright Sales Contact Information";
        public const string Source_Bright_Sales_Contact_View = "Bright Sales Contact View";

        /**
         * sources of event logs for account modified event (11)
         */
        public const string Source_Bright_Manager_Add_Company = "Bright Manager Add Company";
        public const string Source_Bright_Sales_Company_Information = "Bright Sales Company Information";
    }
}
