
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesConsultant.Events
{
    public class ReportConfigurationEvents
    {
        public class OnReportPreview
        {
            public int ReportConfigId { get; set; }
        }
    }
}
