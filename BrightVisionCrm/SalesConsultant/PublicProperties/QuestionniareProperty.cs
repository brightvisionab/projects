
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using SalesConsultant.PublicProperties;
using BrightVision.Common.Business;
using BrightVision.DQControl.Business;

namespace SalesConsultant.PublicProperties
{
    public class QuestionniareProperty
    {
        private SelectionProperty.DialogSaveMode _Mode = SelectionProperty.DialogSaveMode.Unspecified;
        public SelectionProperty.DialogSaveMode Mode {
            get { return _Mode; }
            set { _Mode = value; }
        }

        private SelectionProperty.DialogEditorState _State = SelectionProperty.DialogEditorState.Empty;
        public SelectionProperty.DialogEditorState State
        {
            get { return _State; }
            set { _State = value; }
        }

        private DevExpress.XtraLayout.LayoutControlGroup _QuestionnaireLayout = new DevExpress.XtraLayout.LayoutControlGroup();
        public DevExpress.XtraLayout.LayoutControlGroup QuestionnaireLayout {
            get { return _QuestionnaireLayout; }
            set { _QuestionnaireLayout = value; }
        }

        private List<CampaignQuestionnaire> _lstQuestionnaireDialog = null;
        public List<CampaignQuestionnaire> lstQuestionnaireDialog {
            get { return _lstQuestionnaireDialog; }
            set { _lstQuestionnaireDialog = value; }
        }

        private List<string> _lstQuestionLayoutIds = null;
        public List<string> lstQuestionLayoutIds {
            get { return _lstQuestionLayoutIds; }
            set { _lstQuestionLayoutIds = value; }
        }
    }
}
