
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common;

namespace BrightVision.Common.Business
{
    public class Email
    {
        #region Constructors
        #endregion

        #region Public Properties
        public class ContactEmail
        {
            public int contact_id { get; set; }
            public bool selected { get; set; }
            public string name { get; set; }
            public string company_name { get; set; }
            public string email { get; set; }
            public DateTime? last_verified_date { get; set; }
            public int last_verified_by { get; set; }
            public short verify_attempt_1 { get; set; }
            public short verify_attempt_2 { get; set; }
            public short verify_attempt_3 { get; set; }
            public string s_verify_attempt_1 { get; set; }
            public string s_verify_attempt_2 { get; set; }
            public string s_verify_attempt_3 { get; set; }
            public string verify_no { get; set; }
        }
        #endregion

        #region Private Properties
        
        #endregion

        #region Business Methods
        public static string GetVerifyCodeValue(short pVerificationCode)
        {
            /**
             * Verify code values:
             * 0 = no test have been made (no test)
             * 1 = was invalid (failed)
             * 2 = was valid (valid)
             */
            if (pVerificationCode.Equals(0))
                return "no test";

            else if (pVerificationCode.Equals(1))
                return "failed";

            else if (pVerificationCode.Equals(2))
                return "valid";

            else
                return "no test";
        }
        #endregion
    }
}
