using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.Business
{
    public class CallLogArgs
    {
        public int? ContactId { get; set; }
        public string ContactNo { get; set; }
        public SelectionProperty.CallMethod CallMethod { get; set; }
    }
}
