using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Globalization;

namespace BrightVision.Common.Utilities
{
    public class ValidationUtility
    {
        public static bool HasLetters(string pInput)
        {
            if (string.IsNullOrEmpty(pInput))
                return true;

            return Regex.IsMatch(pInput, @"[a-zA-Z]+");
        }

        public static string StripJsonInvalidChars(string pJsonData)
        {
            pJsonData = pJsonData.Replace("\\\"", ""); // remove \" chars
            return pJsonData;
        }

        public static bool IsEmail(string strEmail)
        {
            strEmail = strEmail == null? "": strEmail;
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

            Regex objEmailRegEx = new Regex(strRegex);
            
            if (objEmailRegEx.IsMatch(strEmail))
                return true;
            else
                return false;
        }
        
        public static bool HasSpecialChars(string Entry)
        {
            Regex objRegEx = new Regex("^[a-zA-Z]*$");
            if (!objRegEx.IsMatch(Entry))
                return true;
            else
                return false;
        }

        public static bool HasNumericEntries(string Entry)
        {
            Regex objRegEx = new Regex(@"\d+");
            if (objRegEx.IsMatch(Entry))
                return true;
            else
                return false;
        }

        public static bool IsCurrency(string AmountFigure)
        {
            if (Information.IsNumeric(AmountFigure))
                return true;
            else
                return false;
        }

        public static int TryParseInt(string sInput)
        {
            if (IsCurrency(sInput))
                return Convert.ToInt32(sInput, CultureInfo.InvariantCulture);
            else
                return 0;
        }

        public static int TryParseInt(object oInput)
        {
            if(oInput != null)
                return TryParseInt(oInput.ToString()); 
            else
                return 0;
        }             

        public static decimal TryParseDecimal(string sInput)
        {
            if (IsCurrency(sInput))
                return Convert.ToDecimal(sInput, CultureInfo.InvariantCulture);
            else
                return 0;
        }

        public static byte TryParseByte(string sInput)
        {
            if (IsCurrency(sInput))
                return Convert.ToByte(sInput, CultureInfo.InvariantCulture);
            else
                return 0;
        }

        public static string IFNullString(object oInput, string sDefaultValue)
        {
            if (oInput != null) return oInput.ToString();

            return sDefaultValue;
        }

        public static bool NumberInBetween(int pNumber, int pFrom, int pTo)
        {
            if (pNumber >= pFrom && pNumber <= pTo)
                return true;

            return false;
        }
    }
}
