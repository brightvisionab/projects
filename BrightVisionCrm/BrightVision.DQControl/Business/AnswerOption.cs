using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace BrightVision.DQControl.Business {
    /// <summary>
    /// Represents the sub question of the question form object
    /// </summary>
    public class AnswerOption : IDropbox, IMultipleChoice, ITextbox, ISchedule, ISmartText {
        
        #region Implementation of IDropbox
        private List<string> dropboxValues;
        /// <summary>
        /// Represents a collection of string values for the dropbox
        /// </summary>
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", 
            typeof(System.Drawing.Design.UITypeEditor))]        
        public List<string> DropBoxValues {
            get {
                if (dropboxValues == null) {
                    dropboxValues = new List<string>();
                }
                return dropboxValues;
            }
            set {
                dropboxValues = value;
            }
        }

        /// <summary>
        /// Gets or sets the text prefix of the sub question
        /// </summary>             
        public string TextPrefix {  get; set; }
        /// <summary>
        /// Gets or sets the selection value of the sub question
        /// </summary>        
        public string SelectionValue { get; set; }
        /// <summary>
        /// Indicates if selection value field is required
        /// </summary>
        public bool SelectionValueRequired { get; set; }
        /// <summary>
        /// Gets or sets the default selection value of the sub question
        /// </summary>
        public string DefaultSelectionValue { get; set; }
        /// <summary>
        /// Gets or sets the selection value of the sub question if other will be used
        /// </summary>
        public string SelectionValueIfOther { get; set; }
        /// <summary>
        /// Indicates if selection value if other field is required
        /// </summary>
        public bool SelectionValueIfOtherRequired { get; set; }
        /// <summary>
        /// Gets or sets the selection value of the sub question
        /// </summary>
        public string DefaultSelectionValueIfOther { get; set; }
        /// <summary>
        /// Gets or sets the max width of the dropbox control
        /// </summary>        
        public string DropboxMaxWidth { get; set; }
        /// <summary>
        /// Indicates if the Freetextbox or other value is visible
        /// </summary>
        public bool SelectionValueIfOtherVisible { get; set; }
        #endregion

        #region Implementation of ITextbox
        /// <summary>
        /// Gets or sets the input value of the textbox
        /// </summary>
        public string InputValue { get; set; }
        /// <summary>
        /// Indicates if input field is required
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Gets or sets the default input value of the textbox
        /// </summary>
        public string DefaultInputValue { get; set; }
        /// <summary>
        /// Gets or sets the max width of the textbox
        /// </summary>
        public string TextboxMaxWidth { get; set; }
        /// <summary>
        /// Gets or sets the max height of the textbox
        /// </summary>
        public string TextboxMaxHeight { get; set; } 
        #endregion

        #region Implementation of IMultipleChoice
        /// <summary>
        /// Gets or sets the number of Multiple choice columns
        /// </summary>
        public int MultipleChoiceColumns { get; set; }

        private List<MultipleChoiceValue> multipleChoiceValues;
        /// <summary>
        /// Represents the collection of multiple choice value
        /// </summary>        
        public List<MultipleChoiceValue> MultipleChoiceValues {
            get {
                if (multipleChoiceValues == null) {
                    multipleChoiceValues = new List<MultipleChoiceValue>();
                }
                return multipleChoiceValues;
            }
            set {
                multipleChoiceValues = value;
            }
        }
        /// <summary>
        /// Indicates if input field multiple choice is required
        /// </summary>
        public bool MultipleChoiceRequired { get; set; }
        #endregion

        #region Implementation of ISchedule
        /// <summary>
        /// Gets or sets the label of view detail summary button
        /// </summary>
        public string ViewDetailSummaryButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the create meeting button
        /// </summary>
        public string CreateMeetingButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the Add caller
        /// </summary>
        public string AddCallerButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the Add additional attendi button
        /// </summary>
        public string AddAdditionalAttendieButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the delete attendie button
        /// </summary>
        public string DeleteAttendieButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the list of available bookings
        /// </summary>
        public string ListOfBookingsAvailableLabel { get; set; }
        /// <summary>
        /// Indicates if input field list of bookings available is required
        /// </summary>
        public bool ListOfBookingsAvailableRequired { get; set; }
        /// <summary>
        /// Gets or sets the booking type
        /// </summary>               
        public ScheduleType ScheduleType { get; set; }
        /// <summary>
        /// Gets or sets the booking sales person
        /// </summary>
        public ScheduleSalesPerson ScheduleSalesPerson { get; set; }
        /// <summary>
        /// Gets or sets the booking schedule value
        /// </summary>
        public ScheduleValue ScheduleValue { get; set; }
        #endregion
        
        #region Shared Implementation
        private List<OtherChoice> otherChoices;
        /// <summary>
        /// Gets or sets the other choice object
        /// </summary>      
        public List<OtherChoice> OtherChoices {
            get {
                if (otherChoices == null) {
                    otherChoices = new List<OtherChoice>();
                }
                return otherChoices;
            }
            set {
                otherChoices = value;
            }
        }

        private List<Attendie> oAttendies;
        /// <summary>
        /// Gets or sets the list of attendies
        /// </summary>      
        public List<Attendie> Attendies {
            get {
                if (oAttendies == null) {
                    oAttendies = new List<Attendie>();
                }
                return oAttendies;
            }
            set {
                oAttendies = value;
            }
        }
        /// <summary>
        /// Gets or sets the label of the attendies list
        /// </summary>
        public string AttendiesLabel { get; set; }
        /// <summary>
        /// Indicates if input field attendies is required
        /// </summary>
        public bool AttendiesRequired { get; set; }
        #endregion

        #region ISmartText Members

        /// <summary>
        /// Gets or sets the header comment text
        /// </summary>
        public string HeaderCommentText{get;set;}
        /// <summary>
        /// Gets or sets the header creation date text
        /// </summary>
        public string HeaderCreationDateText{get;set;}
        /// <summary>
        /// Gets or sets the header user text
        /// </summary>
        public string HeaderUserText{get;set;}
        /// <summary>
        /// Gets or sets the header customer contact text
        /// </summary>
        public string HeaderCustomerContactText{get;set;}
        /// <summary>
        /// Gets or sets the order by of the history
        /// </summary>
        public string OrderBy{get;set;}
        /// <summary>
        /// Gets or sets the order by direction
        /// </summary>
        public string OrderDirection{get;set;}
        /// <summary>
        /// represent the values of smarttext
        /// </summary>
        public List<SmartTextValue> SmartTextValues { get; set; }
        #endregion
    }
}
       