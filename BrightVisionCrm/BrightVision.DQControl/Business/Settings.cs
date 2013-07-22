using System;
using System.Collections.Generic;
using System.ComponentModel;
namespace BrightVision.DQControl.Business {
    /// <summary>
    /// Represents the settings of the question form object
    /// </summary>
    public class Settings {
        /// <summary>
        /// Gets or sets the label of the question form
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the question text of the question form
        /// </summary>
        public string QuestionText { get; set; }
        /// <summary>
        /// Gets or sets the question help of the question form
        /// </summary>
        public string QuestionHelp { get; set; }
        /// <summary>
        /// Gets or sets the priority of the question form
        /// </summary>
        public string Priority { get; set; }
        /// <summary>
        /// Gets or sets the plot done status of the question form
        /// </summary>
        public string PlotDoneStatus { get; set; }
        /// <summary>
        /// Gets or sets the Data bindings of the question form
        /// </summary>
        public DataBindings DataBindings { get; set; }
        /// <summary>
        /// Indicates if it is a Brightvision plot ownership is true or false
        /// </summary>
        public bool BVOwnership { get; set; }
        /// <summary>
        /// Indicates if it is a customer plot ownership is true or false
        /// </summary>
        public bool CustomerOwnership { get; set; }
        /// <summary>
        /// Gets or sets the background color of the question form
        /// </summary>
        public string BackgroundColor { get; set; }
        /// <summary>
        /// Represents a collection of sub questions of the question form
        /// </summary>       
        [BrowsableAttribute(true), ReadOnly(false), TypeConverter(typeof(CollectionConverter))]
        public IList<AnswerOption> AnswerOptions { get; set; }        
    }
}
