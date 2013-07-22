using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightVision.Common.Classes
{
    public class SendMail
    {
        public enum eMailType : int
        {
            Send_Report_To_Customer = 1,
            Send_SMS_To_Customer,
            Send_Mail_To_Prospect,
            Send_SMS_To_Prospect
        }
    }
}