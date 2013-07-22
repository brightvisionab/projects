using System;
using System.Collections.Generic;

namespace BrightVision.DQControl.Business {
    /// <summary>
    /// Represents a multiple choice interface
    /// </summary>
    public interface IMultipleChoice {        
        /// <summary>
        /// Gets or sets the number of Multiple choice columns
        /// </summary>
        int MultipleChoiceColumns { get; set; }
        /// <summary>
        /// Represents the collection of multiple choice value
        /// </summary>
        List<MultipleChoiceValue> MultipleChoiceValues { get; set; }
        /// <summary>
        /// Gets or sets the other choice object
        /// </summary>
        List<OtherChoice> OtherChoices { get; set; }
        /// <summary>
        /// Indicates if input field is required
        /// </summary>
        bool MultipleChoiceRequired { get; set; }
        
    }
}
