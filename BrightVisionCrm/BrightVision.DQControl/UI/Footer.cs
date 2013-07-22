using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BrightVision.DQControl.UI {
    public partial class Footer : UserControl {
        #region Constructor
        public Footer() {
            InitializeComponent();
        }
        public Footer(bool isAccountLevel, bool isCustomerOwned, bool isBrightvisionOwned, bool isRequired,
            string languageCode, string source, string helpText, string questionText) {
            InitializeComponent();
            IsAccountLevel = isAccountLevel;
            //IsCustomerOwnershipOnly = isCustomerOwnedOnly;
            IsCustomerOwned = isCustomerOwned;
            IsBrightvisionOwned = isBrightvisionOwned;
            IsRequired = isRequired;
            LanguageCode = languageCode;
            Source = source;
            HelpText = helpText;
            QuestionText = questionText;
            InitializeFooter();
        } 
        #endregion

        #region Properties
        public bool IsAccountLevel { get; set; }
        //public bool IsCustomerOwnershipOnly { get; set; }        
        public bool IsCustomerOwned { get; set; }
        public bool IsBrightvisionOwned { get; set; }
        public bool IsRequired { get; set; }        
        public string LanguageCode { get; set; }
        public string Source { get; set; }
        public string HelpText { get; set; }
        public string QuestionText { get; set; }
        #endregion

        #region Methods
       
        public void InitializeFooter() {
            if (IsAccountLevel) {                
                bsiContactLevel.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                this.bar1.BarAppearance.Normal.BackColor = System.Drawing.Color.FromArgb(220, 201, 241);//purple
            } else {
                bsiAccountLevel.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                this.bar1.BarAppearance.Normal.BackColor = System.Drawing.SystemColors.Info;//yellow
            }

            //if (IsCustomerOwnershipOnly) {
            //    bsiOwnership.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            //}

            /**
             * https://brightvision.jira.com/browse/PLATFORM-1374
             * extend truth table validation for showing green public icon.
             */

            if (!string.IsNullOrEmpty(QuestionText)) {
                bsiQuestionText.Caption = QuestionText;
                if(bsiQuestionText.SuperTip == null)
                    bsiQuestionText.SuperTip = new DevExpress.Utils.SuperToolTip();
                var tipArgs = new DevExpress.Utils.SuperToolTipSetupArgs();
                tipArgs.AllowHtmlText = DevExpress.Utils.DefaultBoolean.True;
                tipArgs.Title.Text = QuestionText;
                bsiQuestionText.SuperTip.Setup(tipArgs);
            }
            if ((IsCustomerOwned && IsBrightvisionOwned) ||
                (!IsCustomerOwned && IsBrightvisionOwned))
                bsiOwnership.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            else if ((IsCustomerOwned && !IsBrightvisionOwned) ||
                (!IsCustomerOwned && !IsBrightvisionOwned))
                bsiOwnership.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;

            SetLanguageIcon(LanguageCode);

            SetHelpText(HelpText);
        }

        public void SetBackgroundColor(Color color) {
            if (color == null) return;
            bar1.BarAppearance.Normal.BackColor = color;
        }        
        public void SetQuestionRequired(bool isvalid, bool isrequired) {
            if (bsiRequired.SuperTip == null)
                bsiRequired.SuperTip = new DevExpress.Utils.SuperToolTip();

            var tipArgs = new DevExpress.Utils.SuperToolTipSetupArgs();
            tipArgs.AllowHtmlText = DevExpress.Utils.DefaultBoolean.True;
            if (!isvalid && isrequired) {
                bsiRequired.Glyph = global::BrightVision.DQControl.Properties.Resources.required;                
                tipArgs.Title.Text = "Required Question";
                tipArgs.Contents.Text = "The question is required to be answered.";
                bsiRequired.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                bsiRequired.SuperTip.Setup(tipArgs);
            } else if(isvalid && isrequired) {
                bsiRequired.Glyph = global::BrightVision.DQControl.Properties.Resources.completed;
                tipArgs.Title.Text = "Done";
                tipArgs.Contents.Text = "The question has done answered.";
                bsiRequired.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                bsiRequired.SuperTip.Setup(tipArgs);
            } else if (!isrequired) {
                bsiRequired.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }
            
        }
        public void SetHelpText(string value) { 
            if (!string.IsNullOrEmpty(value)) {
                if (bsiHelp.SuperTip == null)
                    bsiHelp.SuperTip = new DevExpress.Utils.SuperToolTip();
                var tipArgs = new DevExpress.Utils.SuperToolTipSetupArgs();
                tipArgs.AllowHtmlText = DevExpress.Utils.DefaultBoolean.True;
                tipArgs.Title.Text = "Question Help";
                tipArgs.Contents.Text = value.Trim();
                bsiHelp.SuperTip.Setup(tipArgs);
            } else {
                bsiHelp.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }
        }
        public void SetSourceTooltip(string contactPerson, string salesUser, string lastModifiedDate) {
            if (string.IsNullOrEmpty(contactPerson)) return;
            if (string.IsNullOrEmpty(salesUser)) return;
            if (string.IsNullOrEmpty(lastModifiedDate)) return;

            if (bsiSource.SuperTip == null)
                bsiSource.SuperTip = new DevExpress.Utils.SuperToolTip();
            var tipArgs = new DevExpress.Utils.SuperToolTipSetupArgs();            
            tipArgs.AllowHtmlText = DevExpress.Utils.DefaultBoolean.True;
            tipArgs.Title.Text = "Source Information";
            tipArgs.Contents.Text = string.Format("Source: {0}\nLast modified: {1}\nLast modified date: {2}",contactPerson,salesUser,lastModifiedDate);
            bsiSource.SuperTip.Setup(tipArgs);
        }
        public void SetLanguageIcon(string languageCode) {
            if (string.IsNullOrEmpty(languageCode)) return;
            try {
                switch (languageCode.ToUpper()) {
                    case "BE": bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.BE; break;
                    case "DE": bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.DE; break;
                    case "DK": bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.DK; break;
                    case "FI": bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.FI; break;
                    case "FR": bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.FR; break;
                    case "IT": bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.IT; break;
                    case "NL": bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.NL; break;
                    case "NO": bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.NO; break;
                    case "PH": bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.PH; break;
                    case "SE": bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.SE; break;
                    case "UK":
                    case "EN":
                        bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.UK; break;
                }
            }
            catch {
                bsiLanguage.Glyph = global::BrightVision.DQControl.Properties.Resources.UK;
            }
        }
        
        #endregion

    }
}
