using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using System.ComponentModel;

using BrightVision.DQControl.Business;

namespace ManagerApplication.Business {
    public enum EditingOptions {
        UnSpecified = 0,
        MultipleChoiceValueEdit = 1,
        OtherChoiceEdit = 2,
        AnswerOptionsEdit = 3
    }
    public enum SaveMode {
        Unspecified = 0,
        Edit = 1,
        Add = 2,
        Delete = 3
        
    }
    public enum OtherChoiceOptions {
        UnKnown = 0,
        OtherChoice = 1,
        MeetingPlaceDetails = 2
    }
}
