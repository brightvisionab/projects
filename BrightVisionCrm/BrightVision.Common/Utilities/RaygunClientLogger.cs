using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Globalization;
using Mindscape.Raygun4Net;


namespace BrightVision.Common.Utilities
{
    public class RaygunClientLogger
    {
        public void Send(Exception e)
        {
            RaygunClient _client = new RaygunClient("EBvSU9kriUrVGUyppe8v3Q==");
            _client.Send(e);
        }


    }
}
