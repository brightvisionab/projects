using System;
using System.Collections.Generic;
using System.ComponentModel;
namespace BrightVision.DQControl.Business {
    /// <summary>
    /// Represents a textbox interface
    /// </summary>
    public interface ITextbox {
        /// <summary>
        /// Gets or sets the text prefix of the textbox
        /// </summary>               
        string TextPrefix { get; set; }
        /// <summary>
        /// Gets or sets the input value of the textbox
        /// </summary>
        string InputValue { get; set; }
        /// <summary>
        /// Indicates if input field is required
        /// </summary>
        bool Required { get; set; }
        /// <summary>
        /// Gets or sets the default input value of the textbox
        /// </summary>
        string DefaultInputValue { get; set; }        
        /// <summary>
        /// Gets or sets the max width of the textbox
        /// </summary>
        string TextboxMaxWidth { get; set; }
        /// <summary>
        /// Gets or sets the max height of the textbox
        /// </summary>
        string TextboxMaxHeight { get; set; }
    }
}
