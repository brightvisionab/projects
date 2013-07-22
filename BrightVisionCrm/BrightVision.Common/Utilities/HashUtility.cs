using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Data;

using System.Security.Cryptography;

namespace BrightVision.Common.Utilities
{
    public partial class HashUtility
    {
        public static string GetHashPassword(string strRawPassword)
        {
            MD5CryptoServiceProvider objMD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] encodedString = System.Text.Encoding.UTF8.GetBytes(strRawPassword);
            encodedString = objMD5.ComputeHash(encodedString);
            System.Text.StringBuilder strHashedPassword = new System.Text.StringBuilder();

            foreach (byte bChar in encodedString)
                strHashedPassword.Append(bChar.ToString("x2").ToLower());

            return strHashedPassword.ToString();
        }
    }
}
