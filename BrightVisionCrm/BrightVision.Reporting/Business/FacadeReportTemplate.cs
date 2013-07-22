
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Reporting.UI;
using System.IO;

namespace BrightVision.Reporting.Business
{
    public class FacadeReportTemplate
    {
        public static string GetDefaultReportLayout()
        { 
            XtraReportDefaultTemplate _item = new XtraReportDefaultTemplate();
            var ms = new MemoryStream();
            //xreport.SaveLayoutToXml(ms);
            _item.SaveLayout(ms);
            ms.Position = 0;

            var sr = new StreamReader(ms, Encoding.Default);
            string _reportTemplate = sr.ReadToEnd();
            return _reportTemplate;
        }

        public static string GetReportLayout(XtraReportDefaultTemplate pTemplate)
        {
            var ms = new MemoryStream();
            //xreport.SaveLayoutToXml(ms);
            pTemplate.SaveLayout(ms);
            ms.Position = 0;

            var sr = new StreamReader(ms, Encoding.Default);
            string _reportTemplate = sr.ReadToEnd();
            return _reportTemplate;
        }
    }
}
