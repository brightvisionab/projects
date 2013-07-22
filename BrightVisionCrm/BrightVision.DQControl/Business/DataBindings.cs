using System;

namespace BrightVision.DQControl.Business {
    /// <summary>
    /// Represents the data bindings object
    /// </summary>
    public class DataBindings {
        public bool same_contact_binding { get; set; }
        /// <summary>
        /// Indicates if an account level or contact level
        /// </summary>
        public bool account_level { get; set; }
        /// <summary>
        /// Gets or sets the account id of the object
        /// </summary>
        public string language_code { get; set; }
        /// <summary>
        /// Gets or sets the account id of the object
        /// </summary>
        public string account_id { get; set; }
        /// <summary>
        /// Gets or sets the answer id of the object
        /// </summary>
        public string answer_id { get; set; }
        /// <summary>
        /// Gets or sets the contact id of the object
        /// </summary>
        public string contact_id { get; set; }
        /// <summary>
        /// Gets or sets the dialog id of the object
        /// </summary>
        public string dialog_id { get; set; }
        /// <summary>
        /// Gets or sets the campaign id of the object
        /// </summary>        
        public string campaign_id { get; set; }
        /// <summary>
        /// Gets or sets the question id of the object
        /// </summary>
        public string question_id { get; set; }
        /// <summary>
        /// Gets or sets the question text language id of the object
        /// </summary>
        public string questions_text_language_id { get; set; }
        /// <summary>
        /// Gets or sets the question layout  id of the object
        /// </summary>
        public string questionlayout_id { get; set; }
        /// <summary>
        /// Gets or sets the question layout language id of the object
        /// </summary>
        public string questionlayout_language_id { get; set; }
        /// <summary>
        /// Gets or sets the schedule id of the object
        /// </summary>
        public string schedule_id { get; set; }
        /// <summary>
        /// Gets or sets the created by user id
        /// </summary>
        public string created_by { get; set; }
    }
}		