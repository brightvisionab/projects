using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.Events
{
    public class FrmManualMatchAccountEvents
    {
        public class OnClose {
            public List<ClassesProperty.ManualMatchAccount> lstMatchAccounts { get; set; }
        }
    }
}
