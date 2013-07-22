
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.DQControl.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SalesConsultant.Business
{
    public class BrightSalesCommon
    {
        public static DataTable GetData(string pXmlConfig, IDataReader pXmlData, ref string pConfigWithData)
        {
            if (string.IsNullOrEmpty(pXmlConfig))
                return null;

            try {
                string dialogData = string.Empty,
                        scheduleData = string.Empty,
                        accountData = string.Empty,
                        contactData = string.Empty,
                        relationData = string.Empty;

                if (pXmlData != null) {
                    while (pXmlData.Read()) {
                        string _data = pXmlData["dialogdata"].ToString();
                        if (!string.IsNullOrEmpty(_data))
                            dialogData = _data;
                    }
                    if (pXmlData.NextResult()) {
                        while (pXmlData.Read()) {
                            string _data = pXmlData["scheduledata"].ToString();
                            if (!string.IsNullOrEmpty(_data))
                                scheduleData = _data;
                        }
                    }
                    if (pXmlData.NextResult()) {
                        while (pXmlData.Read()) {
                            string _data = pXmlData["relationdata"].ToString();
                            if (!string.IsNullOrEmpty(_data))
                                relationData = _data;
                        }
                    }
                    if (pXmlData.NextResult()) {
                        while (pXmlData.Read()) {
                            string _data = pXmlData["accountdata"].ToString();
                            if (!string.IsNullOrEmpty(_data))
                                accountData = _data;
                        }
                    }
                    if (pXmlData.NextResult()) {
                        while (pXmlData.Read()) {
                            string _data = pXmlData["contactdata"].ToString();
                            if (!string.IsNullOrEmpty(_data))
                                contactData = _data;
                        }
                    }
                }

                var xelem = XElement.Parse(pXmlConfig);
                if (!string.IsNullOrEmpty(dialogData))
                    xelem.Add(XElement.Parse(dialogData));
                if (!string.IsNullOrEmpty(scheduleData))
                    xelem.Add(XElement.Parse(scheduleData));
                if (!string.IsNullOrEmpty(relationData))
                    xelem.Add(XElement.Parse(relationData));
                if (!string.IsNullOrEmpty(accountData))
                    xelem.Add(XElement.Parse(accountData));
                if (!string.IsNullOrEmpty(contactData))
                    xelem.Add(XElement.Parse(contactData));

                var xmlSource = xelem.ToString();
                pConfigWithData = xmlSource;
                var xsltExt = new DQXsltFunction();
                var xsltExtParam = new XsltExtensionParam {
                    Namespace = "util:xsltextension",
                    Object = xsltExt
                };
                string xsl = BrightVision.Common.Resources.show_view; //Properties.Resources.showview;
                string xmloutput = XsltUtility.GetXmlString(
                    xsl,
                    xmlSource,
                    new List<XsltExtensionParam> { xsltExtParam },
                    true,
                    true);
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(new System.IO.StringReader(xmloutput));
                return dataSet.Tables[0];
            }
            catch (Exception e) {
                BrightVision.Common.UI.NotificationDialog.Information("Bright Sales", string.Format("{0}{1}{2}", e.Message, Environment.NewLine, e.Source));
                return null;
            }
        }
        public static DataTable GetFilteredData(DataTable pRawDataSource)
        {
            DataTable _dtFilteredData = new DataTable();
            foreach (DataRow _row in pRawDataSource.Rows) {
                foreach (DataColumn _col in pRawDataSource.Columns) {
                    if (_col.ColumnName.Equals("accountid") || _col.ColumnName.Equals("contactid"))
                        continue;
                    _dtFilteredData.Columns.Add(_col.ColumnName);
                }

                foreach (DataColumn _col in pRawDataSource.Columns) {

                }
            }

            int rowCount = pRawDataSource.Rows.Count;
            for (int cnt = 0; cnt < rowCount; cnt++)
            {
                DataRow newrow = _dtFilteredData.NewRow();
                var datarow = (this.gridView1.GetRow(cnt) as DataRowView).Row;
                foreach (GridColumn item in this.gridView1.Columns)
                {
                    if (item.Visible || (item.FieldName == "accountid" || item.FieldName == "contactid"))
                    {
                        newrow[item.FieldName] = datarow[item.FieldName];
                    }
                }
                _dtFilteredData.Rows.Add(newrow);
            }
            
            return _dtFilteredData;
        }
    }
}
