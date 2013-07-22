
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports;
using DevExpress.XtraReports.UI;

namespace BrightVision.Reporting.UI
{
    /**
     * @jeff 05.28.2012
     * the DefaultBindableProperty attribute is intended to make the position
     * properly bindable whan an item is dropped from the field list
     * 
     * source: http://documentation.devexpress.com/#XtraReports/CustomDocument1304
     */
    [ToolboxItem(true), DefaultBindableProperty("Position"), ToolboxBitmap(typeof(SaveTemplateButton))]
    
    /**
     * save to template custom control to be binded on the report designer
     * to provide save to template functionality.
     */
    public class SaveTemplateButton: XRControl
    {
        public SaveTemplateButton() { }
        //public delegate void OnClickEventHandler(object sender, EventArgs e);
        //public event OnClickEventHandler OnClick;        
    }
}
