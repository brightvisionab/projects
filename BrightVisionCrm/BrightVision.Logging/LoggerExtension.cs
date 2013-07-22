using System.Web;
using System;

namespace BrightVision.Logging
{
    public static class LoggerExtension
    {
        public static string LoggerString(this string str)
        {
           string ret = String.Format("\"{0}\"", HttpUtility.HtmlEncode(str));
           return ret;
        }
    }
}
