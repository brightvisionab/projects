using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.DQControl.Utilities;
using System.Xml.Linq;

namespace SalesConsultant.Utils
{
    public static class TooltipUtility
    {
        public static string GetEventTooltip(CTMyFollowUps followUp) {
            string followUpXml = SerializeUtility.Serialize(followUp);
            XElement followUpXElement = XElement.Parse(followUpXml);
            int userID = UserSession.CurrentUser.UserId;
            followUpXElement.Add(new XElement("current_user_id", userID));
            followUpXml = followUpXElement.ToString();
            var xsltExt = new DQXsltFunction();
            var xsltExtParam = new XsltExtensionParam
            {
                Namespace = "util:xsltextension",
                Object = xsltExt
            };
            string xsl = Properties.Resources.tooltip;
            string htmlToolTipContent = XsltUtility.GetXmlString(
                xsl,
                followUpXml,
                new List<XsltExtensionParam> { xsltExtParam },
                true,
                true);
            return System.Net.WebUtility.HtmlDecode( htmlToolTipContent);
        }
        public static string GetEventFollowUpTooltip(CTScEventAndFollowUpLog followUp)
        {
            string followUpXml = SerializeUtility.Serialize(followUp);

            XElement followUpXElement = XElement.Parse(followUpXml);
            int userID = UserSession.CurrentUser.UserId;
            followUpXElement.Add(new XElement("current_user_id", userID));
            followUpXml = followUpXElement.ToString();
            var xsltExt = new DQXsltFunction();
            var xsltExtParam = new XsltExtensionParam
            {
                Namespace = "util:xsltextension",
                Object = xsltExt
            };
            string xsl = Properties.Resources.event_followup_tooltip;
            string htmlToolTipContent = XsltUtility.GetXmlString(
                xsl,
                followUpXml,
                new List<XsltExtensionParam> { xsltExtParam },
                true,
                true);
            return System.Net.WebUtility.HtmlDecode(htmlToolTipContent);
            return "";
        }
    }
}
