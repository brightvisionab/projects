
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.Events
{
    public class LoginEvents
    {
        public class OnSuccess
        {
            public SelectionProperty.WorkingEnvironment WorkingEnvironment { get; set; }
        }
    }
}
