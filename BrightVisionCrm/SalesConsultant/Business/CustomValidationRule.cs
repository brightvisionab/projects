
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors.DXErrorProvider;

namespace SalesConsultant.Business 
{
    public class CustomIsBlankValidationRule : ValidationRule 
    {
        public override bool Validate(Control control, object value) 
        {
            if (value == null) 
                return false;

            if (value.GetType().Equals(typeof(string))) {
                if (string.IsNullOrEmpty(value.ToString()))
                    return false;
                else
                    return value.ToString().Length > 0;
            }
            else if (value.GetType().Equals(typeof(int))) {
                if (Convert.ToInt32(value) > 0)
                    return true;
                else
                    return false;
            }
            return false;            
        }
    }    
}
