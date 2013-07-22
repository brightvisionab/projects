using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.FileManagement;

namespace ManagerApplication.Business
{
    public static class ManagerApplicationFacade
    {
        private static WebDavFileManager _webDavFile;
        public static WebDavFileManager WebDavFile
        {

            get
            {
                if (_webDavFile == null)
                {
                    _webDavFile = new WebDavFileManager();
                }
                return _webDavFile;
            }
        }
    }
}
