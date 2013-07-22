using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightVision.Common.Classes
{
    public class EventLog
    {
        public enum EventTypes
        {
            /**
             * since it is being fired thru table triggers on accounts and contacts tables.
             * google docs: https://docs.google.com/a/myndconsulting.com/spreadsheet/ccc?key=0Av2stuK1YKUAdGEwTmhsZ0NlVDRHVXl6elYwZzh6SXc#gid=20
             * 
             * events not included here (executed thru triggers):
             * 10 = contact modified
             * 11 = accountt modified
             */
            PRESS_CALL_DIRECT = 1,
            PRESS_CALL_MOBILE = 2,
            PRESS_CALL_SWITCH = 3,
            PRESS_CALL_DIRECT_AND_WAIT_60_SECONDS = 4,
            PRESS_CALL_MOBILE_AND_WAIT_60_SECONDS = 5,
            PRESS_CALL_ANONYMOUS = 17,
            PRESS_HANG_UP_CALL = 6,

            CHANGE_DIALOG_SAVE_EVENT = 7,
            CHANGE_LEAD_STATUS_SAVE_EVENT = 12,
            CHANGE_COMPANY_STATUS_SAVE_EVENT = 13,

            DIALOG_EVENT = 16,

            EMAIL_NEW_PASSWORD = 19,
            RESET_PASSWORD = 20,
            SEND_SMS = 21
        }
    }
}
