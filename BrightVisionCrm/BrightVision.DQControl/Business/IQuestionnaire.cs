using System;
using System.Collections.Generic;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.Utils;
using BrightVision.DQControl.Business;
using BrightVision.DQControl.UI;
using Newtonsoft.Json;
using BrightVision.Model;

namespace BrightVision.DQControl.Business {
    /// <summary>
    /// Represents a dropbox interface
    /// </summary>
    public interface IQuestionnaire {
        /// <summary>
        /// Gets or sets the JSON String
        /// </summary>
        string JSONString { get; set; }
        /// <summary>
        /// Indicates if the Questionnaire component has missing field values
        /// </summary>
        bool HasMissingFields { get; set; }
        /// <summary>
        /// Component is readonly
        /// </summary>
        bool ReadOnly { get; set; }
        /// <summary>
        /// Indicates the if the Questionnaire got focused
        /// </summary>
        bool Focused { get; set; }
        /// <summary>
        /// Indicates the if layout selection is disabled
        /// </summary>
        bool DisableSelection { get; set; }
        /// <summary>
        /// Indicates if the 
        /// </summary>
        bool HasChanged { get; set; }
        /// <summary>
        /// Bind dynamic controls inside layout control
        /// </summary>
        void BindControls();
        /// <summary>
        /// Bind Questionnaire from JSON
        /// </summary>
        void BindQuestionnaire();
        /// <summary>
        /// Validate component
        /// </summary>
        /// <returns></returns>
        bool Validate();
        /// <summary>
        /// Gets or sets the tool tip controller of the control
        /// </summary>
        DefaultToolTipController ToolTipController { get; set; }
        /// <summary>
        /// Gets or sets the style controller of the control
        /// </summary>
        IStyleController StyleController { get; set; }
        /// <summary>
        /// Gets or sets the questionnaire object
        /// </summary>
        CampaignQuestionnaire Questionnaire { get; set; }
        /// <summary>
        /// Gets or sets the control group of the questionnaire
        /// </summary>
        LayoutControlGroup ControlGroup { get; set; }
        /// <summary>
        /// Gets the footer control of the component
        /// </summary>
        Footer ControlFooter { get;}
        /// <summary>
        /// Raises an event to notify subscribed object if the Questionnaire values has been changed.
        /// </summary>
        event ComponentDialogNotifyChangedEventHandler OnComponentNotifyChanged;

        void Save();
        void Render();

        bool HasMustSaveDefaultValues { get; set; }
        bool IsInitializing { get; set; }
        /// <summary>
        /// gets or sets whether the particular component is included to display on questionnaire.
        /// for use in saving and displaying.
        /// if true, display on questionnaire and save answers, else, hide and bypass during saving.
        /// </summary>
        bool InQuestionnaire { get; set; }

        int AccountId { get; set; }
        int QuestionId { get; set; }
        int QuestionLayoutId { get; set; }
        int ContactId { get; set; } //remove...

        int UserId { get; set; }
        string UserName { get; set; }

        CampaignQuestionnaire DefaultQuestionnaire { get; set; }
        CTScSubCampaignContactList ContactPerson { get; set; }
        List<CTScSubCampaignContactList> ContactList { get; set; }
    }
}
