using System;
using System.Drawing;
using System.Collections;
using DevExpress.XtraReports.UI;
using BrightVision.Reporting.Template;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
using DevExpress.XtraPrinting;
using BrightVision.Common.Business;
using System.Configuration;

namespace BrightVision.Reporting.UI
{
    public partial class XtraReportDefaultTemplate : DevExpress.XtraReports.UI.XtraReport
    {
        #region Variable
        string _layout;
        #endregion

        #region Constructor
        public XtraReportDefaultTemplate()
        {
            InitializeComponent();
        }
        public XtraReportDefaultTemplate(string layout)
        {
            _layout = layout;
            LoadLayoutFromString(layout);
        }
        public XtraReportDefaultTemplate(string layout, TemplateProperty property)
        {
            LoadLayoutFromString(layout);
            LoadTemplateProperty(property);
        }
        #endregion

        #region Public Methods
        public void LoadLayoutFromString(string str)
        {
            MemoryStream _stream = new MemoryStream();
            StreamWriter _writer = new StreamWriter(_stream, Encoding.Default);
            _writer.Write(str);
            _writer.Flush();
            _stream.Position = 0;
            this.LoadLayout(_stream);
        }
        public void LoadTemplateProperty(TemplateProperty property)
        {
            foreach (Band band in this.Bands)
            {
                //set PageNumber's visibility
                SetPageNumberVisibility(band, property.IsPageNumberVisible);

            }
            foreach (Band band in this.Bands)
            {
                //Add pagebreak, textfield, or statistics;
                if (band is ReportHeaderBand)
                {
                    float ylocation = 0F;
                    float xlocation = 0F;
                    foreach (XRControl control in band.Controls)
                    {
                        var ylocationTmp = control.HeightF + control.LocationF.Y;
                        if (ylocationTmp > ylocation)
                        {
                            ylocation = ylocationTmp;
                            xlocation = control.LocationF.X;
                        }
                    }
                    List<XRControl> list = CreateXRControlList(property, (band as ReportHeaderBand), ylocation + 30, xlocation);
                    band.Controls.AddRange(list.ToArray());
                    break;
                }
            }

        }
        public void SetPageNumberVisibility(XRControl control, bool visibility)
        {
            if (control.Controls.Count > 0)
            {
                foreach (XRControl subControl in control.Controls)
                {
                    SetPageNumberVisibility(subControl, visibility);
                }
            }
            else if (control is XRPageInfo)
            {
                control.Visible = visibility;
            }
        }
        #endregion

        #region Private Methods
        private List<XRControl> CreateXRControlList(TemplateProperty template, ReportHeaderBand band, float initialYPosition, float xPosition)
        {
            var controlList = new List<XRControl>();
            if (template.DynamicProperty == null)
                return controlList;
            var _TemplateProperty = from property in template.DynamicProperty orderby property.Order select property;
            foreach (var property in _TemplateProperty)
            {
                if (!property.IsVisible)
                    continue;
                float yPosition = GetNextPosition(controlList, initialYPosition);
                if (property.Type == TemplateDynamicType.TextField)
                {
                    controlList.Add(AddTextfield(property, yPosition, xPosition));
                }
                else if (property.Type == TemplateDynamicType.PageBreak)
                {
                    controlList.Add(AddPageBreak(property, yPosition));
                }
                else if (property.Type == TemplateDynamicType.Statistics)
                {
                    if (string.IsNullOrEmpty(property.Statistics.ColumnName) ||
                        template.StatisticsDataSource.Columns[property.Statistics.ColumnName] == null)
                        continue;
                    controlList.Add(AddStatistics(template, property, yPosition, xPosition));
                }
            }
            return controlList;
        }
        private XRControl AddStatistics(TemplateProperty template, TemplateDynamicData property, float yPosition, float xPosition)
        {

            var chart1 = new XRChart();
            var pieSeriesView1 = new DevExpress.XtraCharts.PieSeriesView();
            pieSeriesView1.RuntimeExploding = false;
            chart1.BorderColor = Color.Black;
            chart1.Borders = DevExpress.XtraPrinting.BorderSide.None;
            chart1.LocationFloat = new DevExpress.Utils.PointFloat(xPosition, yPosition);
            chart1.Name = "xrChart1" + Guid.NewGuid();

            //chart1.Titles.Add(new DevExpress.XtraCharts.ChartTitle { Text= });
            var series1 = new DevExpress.XtraCharts.Series();
            chart1.Series.Add(series1);

            series1.View = pieSeriesView1;

            var pieSeriesLabel1 = series1.Label as DevExpress.XtraCharts.PieSeriesLabel;
            pieSeriesLabel1.LineVisible = true;
            pieSeriesLabel1.Position = DevExpress.XtraCharts.PieSeriesLabelPosition.TwoColumns;
            chart1.Legend.AlignmentHorizontal = DevExpress.XtraCharts.LegendAlignmentHorizontal.LeftOutside;
            chart1.Legend.AlignmentVertical = DevExpress.XtraCharts.LegendAlignmentVertical.Top;

            //this is the label with lines connected to the pie chart
            var piePointOptions1 = pieSeriesLabel1.PointOptions as DevExpress.XtraCharts.PiePointOptions;
            piePointOptions1.PointView = DevExpress.XtraCharts.PointView.Values;
            piePointOptions1.PercentOptions.ValueAsPercent = false;
            piePointOptions1.ValueNumericOptions.Format = DevExpress.XtraCharts.NumericFormat.Number;
            piePointOptions1.ValueNumericOptions.Precision = 0;

            // this is the label with the name and value
            var piePointOptions2 = series1.LegendPointOptions as DevExpress.XtraCharts.PiePointOptions;
            piePointOptions2.PointView = DevExpress.XtraCharts.PointView.ArgumentAndValues;
            piePointOptions2.ValueNumericOptions.Format = DevExpress.XtraCharts.NumericFormat.Percent;
            piePointOptions2.ValueNumericOptions.Precision = 0;
            piePointOptions2.PercentOptions.ValueAsPercent = true;
            piePointOptions2.Pattern = "{V} : {A} ";

            //sorting by argument or by value
            PercentageValueSortingOption sortby = property.Statistics.PercentageValueSortBy;
            if (sortby == null ||
                sortby == PercentageValueSortingOption.PercentageDescending)
            {
                series1.SeriesPointsSorting = DevExpress.XtraCharts.SortingMode.Descending;
                series1.SeriesPointsSortingKey = DevExpress.XtraCharts.SeriesPointKey.Value_1;
            }
            else if (sortby == PercentageValueSortingOption.PercentageAscending)
            {
                series1.SeriesPointsSorting = DevExpress.XtraCharts.SortingMode.Ascending;
                series1.SeriesPointsSortingKey = DevExpress.XtraCharts.SeriesPointKey.Value_1;
            }
            else if (sortby == PercentageValueSortingOption.ValueDescending)
            {
                series1.SeriesPointsSorting = DevExpress.XtraCharts.SortingMode.Descending;
                series1.SeriesPointsSortingKey = DevExpress.XtraCharts.SeriesPointKey.Argument;
            }
            else if (sortby == PercentageValueSortingOption.ValueAscending)
            {
                series1.SeriesPointsSorting = DevExpress.XtraCharts.SortingMode.Ascending;
                series1.SeriesPointsSortingKey = DevExpress.XtraCharts.SeriesPointKey.Argument;
            }

            //set the data base on the statistics.data 
            property.Statistics.Data = GetetStatisticsData(template, property.Statistics);

            //add data to the charts
            int cnt = 0;

            var sorted = property.Statistics.Data.OrderBy(e => e.Value);
            foreach (var item in sorted)
            {

                var seriesPoint1 = new DevExpress.XtraCharts.SeriesPoint(item.Key, new object[] { ((object)(item.Value)) }, cnt);
                cnt++;
                series1.Points.Add(seriesPoint1);
            }

            if (!string.IsNullOrEmpty(property.Statistics.ChartTitle))
            {
                DevExpress.XtraCharts.ChartTitle newChartTitle = new DevExpress.XtraCharts.ChartTitle { Text = property.Statistics.ChartTitle };
                newChartTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold);
                newChartTitle.Alignment = StringAlignment.Near;
                newChartTitle.Dock = DevExpress.XtraCharts.ChartTitleDockStyle.Top;
                chart1.Titles.Add(newChartTitle);
            }

            //Add total items
            int totalCategory = property.Statistics.Data.Count;

            if (property.Statistics.IsTotalCountVisible)
            {
                int totalCount = GetTotalItems(property.Statistics.Data);
                DevExpress.XtraCharts.ChartTitle totalitems = new DevExpress.XtraCharts.ChartTitle { Text = "Total items: " + totalCount };
                totalitems.Font = new System.Drawing.Font("Tahoma", 8F, FontStyle.Bold);
                totalitems.Alignment = StringAlignment.Near;
                totalitems.Dock = DevExpress.XtraCharts.ChartTitleDockStyle.Bottom;
                chart1.Titles.Add(totalitems);
            }
            //chart1.SizeF = new System.Drawing.SizeF(505.2083F, 100 + (totalCategory * 60));
            if (property.Statistics.GraphSize == GraphSize.Small)
                chart1.SizeF = new System.Drawing.SizeF(505.2083F, 300);
            else if (property.Statistics.GraphSize == GraphSize.Normal)
                chart1.SizeF = new System.Drawing.SizeF(505.2083F, 500);
            else if (property.Statistics.GraphSize == GraphSize.Large)
                chart1.SizeF = new System.Drawing.SizeF(505.2083F, 700);
            return chart1;
        }
        private int GetTotalItems(List<KeyValuePair<string, int>> list)
        {
            int result = 0;
            foreach (var item in list)
            {
                result = result + item.Value;
            }
            return result;
        }
        private List<KeyValuePair<string, int>> GetetStatisticsData(TemplateProperty template, StatisticsTemplate statisticsTemplate)
        {
            string name = statisticsTemplate.ColumnName;
            Dictionary<string, int> tempList = new Dictionary<string, int>();
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();

            foreach (DataRow row in template.StatisticsDataSource.Rows)
            {
                string val = row[name].ToString();

                if (statisticsTemplate.IsNullCategoryVisible && string.IsNullOrEmpty(val))
                    val = "Null value";
                else if (!statisticsTemplate.IsNullCategoryVisible && string.IsNullOrEmpty(val))
                    continue;

                if (string.IsNullOrEmpty(val))
                    continue;

                if (tempList.ContainsKey(val))
                {
                    tempList[val] = tempList[val] + 1;
                }
                else
                {
                    tempList.Add(val, 1);
                }
            }
            foreach (var val in tempList)
            {
                list.Add(new KeyValuePair<string, int>(val.Key, val.Value));
            }

            return list;
        }
        private XRControl AddPageBreak(TemplateDynamicData property, float yPosition)
        {
            var xrPageBreak1 = new DevExpress.XtraReports.UI.XRPageBreak();
            xrPageBreak1.LocationFloat = new DevExpress.Utils.PointFloat(0F, yPosition);
            xrPageBreak1.Name = "xrPageBreak1" + Guid.NewGuid();
            return xrPageBreak1;
        }
        private XRControl AddTextfield(TemplateDynamicData property, float yPosition, float xPosition)
        {
            float fontSize = 0.1f;
            FontStyle fontStyle = FontStyle.Regular;
            var xrLabelTextField = new DevExpress.XtraReports.UI.XRLabel();
            xrLabelTextField.LocationFloat = new DevExpress.Utils.PointFloat(xPosition, yPosition);
            xrLabelTextField.Name = "xrLabel1" + Guid.NewGuid();

            xrLabelTextField.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 2, 0, 0, 96F);
            xrLabelTextField.SizeF = new System.Drawing.SizeF(633, 20F);
            xrLabelTextField.Text = string.Empty;

            if (property.Text != null)
                xrLabelTextField.Text = property.Text.Text;
            else
                property.Text = new TextField { Text = "", Size = TextFieldFontSize.Normal };

            xrLabelTextField.AutoWidth = true;
            xrLabelTextField.CanGrow = true;
            xrLabelTextField.CanShrink = true;
            xrLabelTextField.Multiline = true;

            if (property.Text != null)
            {
                if (property.Text.Text != null)
                {
                    xrLabelTextField.Text = property.Text.Text;

                    if (property.Text.Size == TextFieldFontSize.Small)
                        fontSize = 8F;
                    else if (property.Text.Size == TextFieldFontSize.Normal)
                        fontSize = 10F;
                    else if (property.Text.Size == TextFieldFontSize.Large)
                        fontSize = 14F;

                    if (property.Text.Style == TextFieldFontStyle.Bold)
                        fontStyle = FontStyle.Bold;
                    else if (property.Text.Style == TextFieldFontStyle.Italic)
                        fontStyle = FontStyle.Italic;
                    else if (property.Text.Style == TextFieldFontStyle.BoldItalic)
                        fontStyle = FontStyle.Bold | FontStyle.Italic;
                }
            }
            xrLabelTextField.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, fontStyle);
            //xrLabelTextField.Font = new System.Drawing.Font("Tahoma", fontSize, fontStyle);
            //xrLabelTextField.Font = new System.Drawing.Font("Tahoma", 8F, FontStyle.Bold);
            xrLabelTextField.StylePriority.UseFont = false;
            return xrLabelTextField;
        }
        private float GetNextPosition(List<XRControl> list, float initialYPosition)
        {
            float yPosition = 0;
            if (list.Count > 0)
            {
                var position = list[list.Count - 1].LocationF;
                var size = list[list.Count - 1].SizeF;
                yPosition = position.Y + size.Height + 30f;
            }
            else
                return initialYPosition;
            return yPosition;
        }
        #endregion

        #region Static
        public static List<string> CreatePdfPerAccount(XtraReportDefaultTemplate xreport, ReportDataSet dSet, string pReportsPath)
        {
            List<string> _PdfFiles = new List<string>();
            foreach (DataRow row in dSet.account.Rows) {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                var dSetClone = (ReportDataSet)dSet.Copy();
                var dset = dSetClone.account.Select(String.Format("NOT(account_id={0})", row["account_id"]));
                foreach (var rowD in dset)
                    rowD.Delete();

                dSetClone.AcceptChanges();
                string fileName = String.Format(@"{0}\{1}_{2}_{3}.pdf",
                    pReportsPath,
                    row["company_name"].ToString().Replace(" ", "_").Replace(":", "_").Replace("/", "").Replace(@"\", ""),
                    DateTime.Now.ToString("yyyy-MM-dd"),
                    Guid.NewGuid().ToString()
                );

                var report = xreport.CloneReport() as XtraReportDefaultTemplate;
                report.ReportHeader.Controls.Clear();
                PdfExportOptions pdfOptions = report.ExportOptions.Pdf;
                pdfOptions.Compressed = true;
                pdfOptions.ImageQuality = PdfJpegImageQuality.Low;
                report.DataSource = dSetClone;
                report.ExportToPdf(fileName);
                _PdfFiles.Add(fileName);
            }

            return _PdfFiles;
        }
        public static void SavePDFPerAccount(XtraReportDefaultTemplate xreport, ReportDataSet dSet)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            string reportPath = string.Empty;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK) {
                foreach (DataRow row in dSet.account.Rows) {
                    if (row.RowState == DataRowState.Deleted)
                        continue;

                    var dSetClone = (ReportDataSet)dSet.Copy();
                    var dset = dSetClone.account.Select(String.Format("NOT(account_id={0})", row["account_id"]));
                    foreach (var rowD in dset)
                        rowD.Delete();

                    dSetClone.AcceptChanges();
                    reportPath = folderBrowserDialog.SelectedPath;
                    string fileName = String.Format(@"{0}\{1}_{2}_{3}.pdf",
                        reportPath,
                        row["company_name"].ToString().Replace(" ", "_").Replace(":", "_").Replace("/", "").Replace(@"\", ""),
                        DateTime.Now.ToString("yyyy-MM-dd"),
                        Guid.NewGuid().ToString()
                    );
                    
                    //string fileName = String.Format("{0}\\{1}_{2}.pdf", reportPath, row["company_name"].ToString().Replace(" ", "_").Replace(":", "_"), DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss"));
                    
                    var report = xreport.CloneReport() as XtraReportDefaultTemplate;
                    report.ReportHeader.Controls.Clear();
                    PdfExportOptions pdfOptions = report.ExportOptions.Pdf;
                    pdfOptions.Compressed = true;
                    pdfOptions.ImageQuality = PdfJpegImageQuality.Low;
                    report.DataSource = dSetClone;
                    report.ExportToPdf(fileName);
                }
                DialogResult _dlg = MessageBox.Show(
                    string.Format("Successfully generated reports.{0}Would you like to open the folder where the reports are saved?", Environment.NewLine), 
                    "Brightvision Reports", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question
                );
                if (_dlg == DialogResult.Yes)
                    System.Diagnostics.Process.Start(folderBrowserDialog.SelectedPath);
            }
        }
        public static string SavePDF(XtraReportDefaultTemplate xreport, ReportDataSet dSet, string pReportsPath, bool pIsWebPortalCall = false)
        {
            // Set PDF-specific export options.
            //string reportPath = Directory.GetCurrentDirectory();

            //string reportPath = ConfigurationManager.AppSettings["PdfReportsPath"].ToString(); // for debugging purposes only
            //if (pIsWebPortalCall)
            //string reportPath = ConfigManager.AppSettings["PdfReportsPath"].ToString();

            if (!pIsWebPortalCall) {
                if (!Directory.Exists(@"C:\BrightVision"))
                    Directory.CreateDirectory(@"C:\BrightVision");

                if (!Directory.Exists(@"C:\BrightVision\PdfReports"))
                    Directory.CreateDirectory(@"C:\BrightVision\PdfReports");
            }
            
            string fileName = "";
            ReportDataSet dSetClone = new ReportDataSet();
            foreach (DataRow row in dSet.account.Rows) {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                dSetClone = (ReportDataSet)dSet.Copy();
                var dset = dSetClone.account.Select(String.Format("NOT(account_id={0})", row["account_id"]));
                foreach (var rowD in dset)
                    rowD.Delete();

                dSetClone.AcceptChanges();
                fileName = String.Format(@"{0}\{1}_{2}_{3}.pdf",
                    pReportsPath,
                    row["company_name"].ToString().Replace(" ", "_").Replace(":", "_").Replace("/", "").Replace(@"\", ""),
                    DateTime.Now.ToString("yyyy-MM-dd"),
                    Guid.NewGuid().ToString()
                );
            }
            
            var report = xreport.CloneReport() as XtraReportDefaultTemplate;
            report.ReportHeader.Controls.Clear();
            PdfExportOptions pdfOptions = report.ExportOptions.Pdf;
            pdfOptions.Compressed = true;
            pdfOptions.ImageQuality = PdfJpegImageQuality.Low;
            report.DataSource = dSetClone;
            report.ExportToPdf(fileName);

            return fileName;
        }
        #endregion
    }
}
