using System;
using System.Reflection;
using System.ComponentModel;


namespace BrightVision.Logging.Enums
{
    public enum ConnectionType {
        [Description("Gothernburg")]
        Gothernburg=0,
        [Description("Hamachi")]
        Hamachi= 1,
        [Description("DemoEnv")]
        DemoEnv = 2
    }
    public enum BrightVisionApplication {
        [Description("BrightSales")]
        BrightSales = 0,
        [Description("BrightManager")]
        BrightManager = 1 
    }
    public enum BrightVisionEnvironment {
        [Description("Production")]
        Production = 0,
        [Description("Staging")]
        Staging = 1 
    }
    public enum LoggingField
    {
        [Description("customer_id")]
        customer_id,
        [Description("customer_name")]
        customer_name,
        [Description("campaign_id")]
        campaign_id,
        [Description("campaign_name")]
        campaign_name,
        [Description("sub_campaign_id")]
        sub_campaign_id,
        [Description("sub_campaign_name")]
        sub_campaign_name,
        [Description("dialog_id")]
        dialog_id,
        [Description("account_id")]
        account_id,
        [Description("account_name")]
        account_name,
        [Description("contact_id")]
        contact_id,
        [Description("contact_name")]
        contact_name,
        [Description("complete_loading_time")]
        complete_loading_time,
        [Description("nr_companies_campagin_list")]
        nr_companies_campagin_list,
        [Description("sub_campagin_list_type")]
        sub_campagin_list_type,
        [Description("called_number")]
        called_number,
        [Description("called_number_type")]
        called_number_type,
        [Description("call_engine")]
        call_engine,
        [Description("time_from_start")]
        time_from_start
    }

    public static class EnumExtension {
        public static string GetEnumDescription(this Enum val)
        {
            FieldInfo fi = val.GetType().GetField(val.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return val.ToString();
        }
    }
}
