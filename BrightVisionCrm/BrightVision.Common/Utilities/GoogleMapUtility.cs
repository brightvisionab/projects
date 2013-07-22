using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BrightVision.Common.Utilities 
{
    public partial class GoogleMapUtility
    {
        #region Public Properties
        public Dictionary<string, string> GeoDataReturnCode
        {
            get { return m_GeoDataReturnCode; }
        }
        #endregion

        #region Private Members
        /**
         * unique key assigned for maps.brightvision@gmail.com
         * wiki page: http://brightvision.jira.com/wiki/display/PLATFORM/Geo+Application+Account+Credentials
         */
        //private string m_ApiKey = "ABQIAAAAAHbNQpAh-KhHuXOwegGnixRn5qTP7atA0ZHAq92E968qg4_wtxTjBlL5vjaXaWw67ndtPFHGDf35Y"; // old obsolete api key
        private string m_ApiKey = "AIzaSyBwBIhfetiYp8UqqXz2cSjckOFxBaPLyFs";

        private Dictionary<string, string> m_GeoDataReturnCode = null;
        #endregion

        #region Constuctors
        public GoogleMapUtility() 
        {
        }

        public GoogleMapUtility(string Key) 
        {
            if (string.IsNullOrEmpty(Key))
                throw new ArgumentException("Google Maps API Key is invalid");

            this.m_ApiKey = Key;
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Gets map geo data, where index 2 is Latitude and index 3 is Longitude
        /// http://code.google.com/apis/maps/documentation/geocoding/v2/#GeocodingResponses
        ///200	G_GEO_SUCCESS	            No errors occurred; the address was successfully parsed and its geocode was returned.
        ///500	G_GEO_SERVER_ERROR	        A geocoding or directions request could not be successfully processed, yet the exact reason for the failure is unknown.
        ///601	G_GEO_MISSING_QUERY	        An empty address was specified in the HTTP q parameter.
        ///602	G_GEO_UNKNOWN_ADDRESS	    No corresponding geographic location could be found for the specified address, possibly because the address is relatively new, or because it may be incorrect.
        ///603	G_GEO_UNAVAILABLE_ADDRESS	The geocode for the given address or the route for the given directions query cannot be returned due to legal or contractual reasons.
        ///610	G_GEO_BAD_KEY	            The given key is either invalid or does not match the domain for which it was given.
        ///620	G_GEO_TOO_MANY_QUERIES
        /// </summary>
        public string GetGeographicalData(string pAddress)
        {
            #region Old Implementation v2 (depreciated by google)
            //// using google maps api, returns comma separated values
            ////var url = "http://maps.google.co.uk/maps/geo?output=csv&key=" + this.m_ApiKey + "&q=" + Uri.EscapeUriString(Address);
            //var url = "http://maps.google.com/maps/geo?output=csv&key=" + this.m_ApiKey + "&q=" + Uri.EscapeUriString(Address);

            ////http://maps.googleapis.com/maps/api/geocode/json?address=1600+Amphitheatre+Parkway,+Mountain+View,+CA&sensor=true_or_false

            //#region Other Fetch Methods
            //// using web api, returns json data
            ////var url = "http://api.maps.yahoo.com/ajax/geocode?appid=batchGeocode&qt=3&id=0&qs=" + Uri.EscapeUriString(Address);
            //#endregion

            //var objRequest = WebRequest.Create(url);
            //var objResponse = (HttpWebResponse)objRequest.GetResponse();

            //if (objResponse.StatusCode == HttpStatusCode.OK) {
            //    var objStream = new MemoryStream();
            //    var objResponseStream = objResponse.GetResponseStream();
            //    var objBuffer = new Byte[2048];

            //    int count = objResponseStream.Read(objBuffer, 0, objBuffer.Length);
            //    while (count > 0) {
            //        objStream.Write(objBuffer, 0, count);
            //        count = objResponseStream.Read(objBuffer, 0, objBuffer.Length);
            //    }

            //    objResponseStream.Close();
            //    objStream.Close();

            //    var objResponseBytes = objStream.ToArray();
            //    var objEncoding = new System.Text.ASCIIEncoding();
            //    var objGeoData = objEncoding.GetString(objResponseBytes);

            //    return objGeoData.ToString();
            //}
            #endregion

            string _Url = string.Format("http://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=true", pAddress);
            try {
                HttpWebRequest _Request = WebRequest.Create(_Url) as HttpWebRequest;
                _Request.Method = "GET";
                WebResponse _Response = _Request.GetResponse();
                if (_Response != null) {
                    XPathDocument _Document = new XPathDocument(_Response.GetResponseStream());
                    XPathNavigator _Navigator = _Document.CreateNavigator();
                    string _Status = XmlUtility.GetXmlNodeInnerText(_Navigator.OuterXml, "/GeocodeResponse/status");
                    string _Latitude = XmlUtility.GetXmlNodeInnerText(_Navigator.OuterXml, "/GeocodeResponse/result/geometry/location/lat");
                    string _Longitude = XmlUtility.GetXmlNodeInnerText(_Navigator.OuterXml, "/GeocodeResponse/result/geometry/location/lng");
                    string _Code = _Status.Equals("OK") ? "200" : "0";
                    return string.Format("{0},{1},{2},{3}", _Code, _Status, _Latitude, _Longitude);
                }
            }
            catch {
            }

            return string.Empty;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Check if the geo address is valid or not
        /// </summary>
        /// <param name="GeoAddress"></param>
        /// <returns></returns>
        public static bool IsValidGeoAddress(string GeoAddress) {
            GeoAddress = GeoAddress.Replace(" ", "");
            GeoAddress = GeoAddress.Replace(",", "");
            if (GeoAddress.Length < 1)
                return false;
            else
                return true;
        }
        #endregion
    }
}
