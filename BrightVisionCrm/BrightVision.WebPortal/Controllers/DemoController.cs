
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip;
using System.Net;

namespace BrightVision.WebPortal.Controllers
{
    public class DemoController : Controller
    {
        public ActionResult Index()
        {
            return Content("Demo page ...");
        }
        public FileStreamResult Show(int pData = 1)
        {
            byte[] _Data = null;
            string _FilePath = string.Empty;
            
            if (pData == 1) {
                _FilePath = @"C:\BrightVision\DemoFiles\DemoFile.pdf";
                _Data = System.IO.File.ReadAllBytes(_FilePath);
            }

            else if (pData == 2) {
                _FilePath = @"C:\BrightVision\DemoFiles\DemoJpg.jpg";
                _Data = System.IO.File.ReadAllBytes(_FilePath);
            }

            else if (pData == 3) {
                _FilePath = @"C:\BrightVision\DemoFiles\DemoPng.png";
                _Data = System.IO.File.ReadAllBytes(_FilePath);
            }

            else if (pData == 4) {
                _FilePath = @"C:\BrightVision\DemoFiles\DemoMp3.mp3";
                _Data = System.IO.File.ReadAllBytes(_FilePath);
            }

            else if (pData == 5) {
                _FilePath = @"C:\BrightVision\DemoFiles\DemoMp4.mp4";
                _Data = System.IO.File.ReadAllBytes(_FilePath);
            }

            string _MimeType = BrightVision.Common.Utilities.FileManagerUtility.GetMimeType(_FilePath);
            MemoryStream _ms = new MemoryStream(_Data);
            return new FileStreamResult(_ms, _MimeType);
        }
        public ActionResult Download()
        {
            List<string> _lstFilePaths = new List<string>();
            _lstFilePaths.Add(@"C:\BrightVision\PdfReports\File1.pdf");
            _lstFilePaths.Add(@"C:\BrightVision\PdfReports\File2.pdf");
            _lstFilePaths.Add(@"C:\BrightVision\PdfReports\File3.pdf");
            _lstFilePaths.Add(@"C:\BrightVision\PdfReports\File4.pdf");
            _lstFilePaths.Add(@"C:\BrightVision\PdfReports\File5.pdf");
            _lstFilePaths.Add(@"C:\BrightVision\PdfReports\File6.pdf");
            _lstFilePaths.Add(@"C:\BrightVision\PdfReports\File7.pdf");
            _lstFilePaths.Add(@"C:\BrightVision\PdfReports\File8.pdf");

            Response.AddHeader("Content-Disposition", "attachment; filename=DemoZipFile.zip");
            Response.ContentType = "application/zip";

            using (ZipOutputStream _ZipStream = new ZipOutputStream(Response.OutputStream)) {
                foreach (string _filePath in _lstFilePaths) {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(_filePath);
                    ZipEntry _fileEntry = new ZipEntry(Path.GetFileName(_filePath)) {
                        Size = fileBytes.Length
                    };

                    _ZipStream.PutNextEntry(_fileEntry);
                    _ZipStream.Write(fileBytes, 0, fileBytes.Length);
                    System.IO.File.Delete(_filePath);
                }

                _ZipStream.Flush();
                _ZipStream.Close();
            }

            return Content("<h2>Downloading File<h2>");

            //string _MimeType = BrightVision.Common.Utilities.FileManagerUtility.GetMimeType(_FilePath);
            //MemoryStream _ms = new MemoryStream(_Data);
            //return new FileStreamResult(_ms, _MimeType);
        }
        public ActionResult PlayBlob()
        {
            Stream stream = WebRequest.Create("https://lii.blob.core.windows.net/old/0a4b23f7-eab8-4043-ad84-36b4b5d23f1e_.mp3").GetResponse().GetResponseStream();
            return new FileStreamResult(stream, "audio/mpeg");
        }
    }
}
