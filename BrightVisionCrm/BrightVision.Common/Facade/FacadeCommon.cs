
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Common.Events.Core;

namespace BrightVision.FileManagement
{
    public static class FacadeCommon
    {
        private static WebDavFileManager _webDavFile;
        public static WebDavFileManager WebDavFile
        {
            get
            {
                if (_webDavFile == null)
                    _webDavFile = new WebDavFileManager();

                return _webDavFile;
            }
        }
    }
}
