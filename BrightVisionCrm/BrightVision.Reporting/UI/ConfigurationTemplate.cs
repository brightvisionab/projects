using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BrightVision.Reporting.Template;
namespace BrightVision.Reporting.UI
{
    public partial class ConfigurationTemplate : DevExpress.XtraReports.UI.XtraReport
    {
        TemplateProperty _template;
        public ConfigurationTemplate()
        {
            InitializeComponent();
        }

        public void Add(TemplateProperty template) {
            _template = template;
            foreach (var property in template.DynamicProperty)
            {
                if (property.Type == TemplateDynamicType.TextField)
                {
                    AddTextfield(property);
                }
                else if (property.Type == TemplateDynamicType.PageBreak)
                {
                    AddPageBreak(property);
                }
                else if (property.Type == TemplateDynamicType.Statistics)
                {
                    AddStatistics(property);
                }
            }
        }

        private void AddStatistics(TemplateDynamicData property)
        {
            var chart1 = new DevExpress.XtraReports.UI.XRChart();
            this.xrChart1.BorderColor = System.Drawing.Color.Black;
            this.xrChart1.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.xrChart1.LocationFloat = new DevExpress.Utils.PointFloat(184.375F, 0F);
            this.xrChart1.Name = "xrChart1";
            this.Detail.Controls.Add(chart1);

        }

        private void AddPageBreak(TemplateDynamicData property)
        {
            
        }

        private void AddTextfield(TemplateDynamicData property)
        {
            
        }
    }
}
