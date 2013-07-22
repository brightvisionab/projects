using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Mvp.Xml.Common.Xsl;
using BrightVision.Common.Business;
/// <summary>
/// Summary description for XsltUtility
/// </summary>
/// 
namespace BrightVision.Common.Utilities {
    public class XsltUtility {
        public static string GetXmlString(string stylesheet, string xmlstring, List<XsltExtensionParam> listExtensionParam,
            bool enableDocumentFunction, bool enableScript) {

            MvpXslTransform xslt = new MvpXslTransform();
            string text = string.Empty;
            try {
                using (XmlReader strm = XmlReader.Create(new MemoryStream(ASCIIEncoding.Default.GetBytes(stylesheet)))) {
                    if (enableDocumentFunction && enableScript) {
                        XsltSettings xslSettings = new XsltSettings(enableDocumentFunction, enableScript);
                        XmlResolver xmlResolver = new XmlUrlResolver();
                        xslt.Load(strm, xslSettings, xmlResolver);
                    } else if (enableDocumentFunction && !enableScript) {
                        XsltSettings xslSettings = new XsltSettings(enableDocumentFunction, false);
                        XmlResolver xmlResolver = new XmlUrlResolver();
                        xslt.Load(strm, xslSettings, xmlResolver);
                    } else {
                        xslt.Load(strm);
                    }
                }
                using (TextReader stringReader = new StringReader(xmlstring)) {
                    XPathDocument mydata = new XPathDocument(stringReader);
                    XmlInput a = new XmlInput(mydata);
                    using (TextWriter txtWriter = new StringWriter()) {
                        XsltArgumentList b = new XsltArgumentList();
                        foreach (XsltExtensionParam oep in listExtensionParam) {
                            b.AddExtensionObject(oep.Namespace, oep.Object);
                        }
                        xslt.Transform(a, b, new XmlOutput(txtWriter));
                        text = txtWriter.ToString();
                    }
                }
            } catch (Exception ex) {

            }
            return text;
        }
    }

}