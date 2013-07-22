
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.Events
{
    public class AddTitleSettingEvents
    {
        public class OnSave
        {
            public title Title { get; set; }
            public SelectionProperty.eWriteMode WriteMode { get; set; }
        }
    }
}
