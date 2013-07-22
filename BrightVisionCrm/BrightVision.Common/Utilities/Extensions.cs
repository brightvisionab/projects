using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization.Formatters.Binary;   //binary formatter
using System.IO;      //memory stream
using Newtonsoft.Json.Linq;
using System.Management;

namespace BrightVision.Common.Utilities {
    public static class Extensions {
        /// <summary>
        /// Deep clone a generic list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listToClone"></param>
        /// <returns></returns>
        public static IList<T> Clone<T>(this IList<T> listToClone) {
            if (listToClone == null) return null;

            IList<T> clone = default(IList<T>);
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream memStream = new MemoryStream();
            bf.Serialize(memStream, listToClone);
            memStream.Flush();
            memStream.Position = 0;
            clone = ((IList<T>)bf.Deserialize(memStream));
            return clone;
        }
        /// <summary>
        /// Remove double quotes in a JToken string
        /// </summary>
        /// <param name="obj">JToken object</param>
        /// <returns></returns>
        public static string RemoveQuotes(this JToken obj) {
            return obj.ToString().Replace("\"", "");
        }
        /// <summary>
        /// Unescape string or removes extra (",\,\\) etc.
        /// </summary>
        /// <param name="str">string to unescape</param>
        /// <returns></returns>
        public static string Unescape(this string str) {
            string oldStr = Regex.Unescape(str);
            if (oldStr.Substring(0, 1) == '"'.ToString())
                oldStr = oldStr.Substring(1);
            if (oldStr.Substring(oldStr.Length - 1, 1) == '"'.ToString())
                oldStr = oldStr.Substring(0, oldStr.Length - 1);
            return oldStr;
        }
        public static bool CanOpen(this SqlConnection connection) {
            try {
                if (connection == null) { return false; }

                var ds = connection.DataSource ?? "";
                var ipstring = ds.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                var address = ipstring.Trim().Split(".".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                if (address.Length == 1 && address[0].StartsWith("\\"))
                    return true;
                byte[] ipaddress = new byte[address.Length];
                for (int x = 0; x < address.Length; ++x) {
                    ipaddress[x] = byte.Parse(address[x]);
                }
                var ping = new Ping();
                var reply = ping.Send(new IPAddress(ipaddress), 3000);
                if (reply.Status == IPStatus.Success) {
                    return true;
                } else {
                    return false;
                }

                //connection.Open();
                //var canOpen = connection.State == ConnectionState.Open;
                //connection.Close();
                //return canOpen;
            } catch {
                return false;
            }
        }
        public static string ToSwedishPhoneNumber(this string phoneNumber) {
            string theNumber = phoneNumber.Trim();

            
            if (theNumber.StartsWith("+"))
                theNumber = theNumber.Replace("+", "00");
            if (theNumber.StartsWith("00"))
                theNumber = theNumber.Replace("(0)", "");
            if (theNumber.Contains("-"))
                theNumber = theNumber.Replace("-", "");
             
            
            return Regex.Replace(theNumber, @"[^\d]", "");
            


            ///**
            // * https://brightvision.jira.com/browse/PLATFORM-2758
            // */

            //if (theNumber.StartsWith("00"))
            //    theNumber = "+" + theNumber.Substring(2, theNumber.Length - 2);
            //if (theNumber.Contains("-"))
            //    theNumber = theNumber.Replace("-", "");

            //return theNumber;
        }
        /// <summary>
        /// Replaces invalid XML characters in a string with their valid XML equivalent.
        /// </summary>                
        public static string EscapeInvalidXmlChars(this string text) {
            return System.Security.SecurityElement.Escape(text);
        }

        public static bool IsA<T>(this object obj) {
            return obj is T;
        }

        public static string GetCPUId(string path)
        {
            string cpuInfo = "";
            /*// Build an options object for the remote connection
            //   if you plan to connect to the remote
            //   computer with a different user name
            //   and password than the one you are currently using

                 ConnectionOptions options = 
                     new ConnectionOptions();

                 // and then set the options.Username and 
                 // options.Password properties to the correct values
                 // and also set 
                 // options.Authority = "ntlmdomain:DOMAIN";
                 // and replace DOMAIN with the remote computer's
                 // domain.  You can also use Kerberos instead
                 // of ntlmdomain.
            */

            // Make a connection to a remote computer. 
            // Replace the "FullComputerName" section of the
            // string "\\\\FullComputerName\\root\\cimv2" with
            // the full computer name or IP address of the 
            // remote computer.

            ConnectionOptions co = new ConnectionOptions();
            co.Impersonation = ImpersonationLevel.Impersonate;
            co.Authentication = AuthenticationLevel.Packet;
            co.Timeout = new TimeSpan(0, 0, 30);
            co.EnablePrivileges = true;
            //co.Username = "admin";
            //co.Password = "pgibizdavao";

            ManagementScope scope =
                new ManagementScope(
                "\\\\" + path + "\\root\\cimv2", co);
            scope.Connect();

            // Use this code if you are connecting with a  
            // different user name and password: 
            // 
            // ManagementScope scope =  
            //    new ManagementScope( 
            //        "\\\\FullComputerName\\root\\cimv2", options);
            // scope.Connect();

            cpuInfo += GetCPUInfo(scope, "Win32_BIOS", "SerialNumber");
            cpuInfo += GetCPUInfo(scope, "Win32_Processor", "ProcessorId");
            cpuInfo += GetCPUInfo(scope, "Win32_ComputerSystem", "Name");
            return cpuInfo;
        }

        static string GetCPUInfo(ManagementScope scope, string managementClass, string fieldInfo)
        {
            string info = "";
            try
            {
                //Query system for Operating System information
                ObjectQuery query = new ObjectQuery(
                    "SELECT * FROM " + managementClass);
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher(scope, query);

                ManagementObjectCollection queryCollection = searcher.Get();
                foreach (ManagementObject m in queryCollection)
                {
                    info = m[fieldInfo].ToString().Trim();
                    break;
                }
            }
            catch (Exception e) { }

            return info;
        }

    }
}
